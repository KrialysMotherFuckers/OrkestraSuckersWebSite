using Krialys.Orkestra.Common.Models.WorkerNode;
using System.Collections.Concurrent;

namespace Krialys.Orkestra.WebApi.Services.System.HUB;

/// <summary>
/// ParallelU Client side - (Manage CPU: Add, Remove, Update, Get, Enumerate)
/// </summary>
public interface ICpuConnectionManager : ISingletonService
{
    WorkerNode AddConnection(WorkerNode newNodeService);

    void RemoveConnection(string connectionId);               // Remove the connection, dealing with to internal cpuMap

    void RemoveLostConnection(string connectionId);           // Remove the lost connection, dealing with to internal cpuMapLost

    WorkerNode UpdateCpu(WorkerNode cpu);                     // Update the connection, the internal key ConnectionId is compared with the cpu parameter, if not found then returns 'cpu'

    WorkerNode GetCpuInstance(string connectionId);           // Get CPU instance thanks to its connectionId

    IEnumerable<string> GetOnlineInstances { get; }           // Get all online CPU instances names

    IEnumerable<string> GetLostInstances { get; }             // Get all LOST CPU instances names

    HashSet<WorkerNode> GetOnlineConnections();               // Get all ACTIVE CPU instances names

    HashSet<WorkerNode> GetOnlineConnections(string cpuName); // Get all Online connections that 1 CPU has (1 instance, 1 to N connections)

    HashSet<WorkerNode> GetLostConnections(string cpuName);   // Get all Lost connections that 1 CPU has (1 instance, 1 to N connections)

    bool Started { get; set; }                                // Get CPU status
}

/// <summary>
/// ParallelU Client side - (compares 2 CPU based on their Id)
/// </summary>
internal class CpuComparer : IEqualityComparer<WorkerNode>
{
    public bool Equals(WorkerNode x, WorkerNode y) => y != null && x != null && x.Id.Equals(y.Id, StringComparison.InvariantCultureIgnoreCase);

    public int GetHashCode(WorkerNode obj) => obj.Id?.GetHashCode() ?? 0;
}

/// <summary>
/// ParallelU Client side
/// </summary>
public class CpuConnectionManager : ICpuConnectionManager
{
    /// <summary>
    /// Contains all ParallelU clients in a static way (allows running N times same cpuName, thread safe using ConcurrentDictionary)
    /// </summary>
    private static readonly ConcurrentDictionary<string, HashSet<WorkerNode>> CpuMap = new();
    private static readonly ConcurrentDictionary<string, HashSet<WorkerNode>> CpuMapLost = new();
    private readonly Serilog.ILogger _logger;

    public CpuConnectionManager(Serilog.ILogger logger)
        => _logger = logger;

    public bool Started { get; set; }

    /// <summary>
    /// Get all ParallelU Clients that are actually connected
    /// </summary>
    public IEnumerable<string> GetOnlineInstances
    {
        get
        {
            return CpuMap.Keys.Any() ? CpuMap.Keys : Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Get all ParallelU Clients that are actually lost
    /// </summary>
    public IEnumerable<string> GetLostInstances
    {
        get
        {
            return CpuMapLost.Keys.Any() ? CpuMapLost.Keys : Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// AddConnection add cpuName if not existing then adds it connectionId, else append another connectionId linked with cpuName
    /// 1 cpuName can hold from 1 to N connectionId
    /// </summary>
    /// <param name="newNodeService"></param>
    /// <returns>CPU update sent to the server side</returns>
    public WorkerNode AddConnection(WorkerNode newNodeService)
    {
        if (!CpuMap.ContainsKey(newNodeService.Name))
            CpuMap[newNodeService.Name] = new HashSet<WorkerNode>(new CpuComparer());

        //// 1st time connected => add it to the cpuMap
        //var newCpu = new CPU(connectionId, cpuName, machine);

        // Add the new CPU
        if (CpuMap[newNodeService.Name].Add(newNodeService))
        {
            _logger.Information($"> Machine: {newNodeService.Machine}, Client: {newNodeService.Name}, Id: {newNodeService.Id}, Action: Add");

            // Update CPU workflow (by setting parameters, declaring this CPU is immediately available)
            return UpdateCpu(newNodeService);
        }

        return new WorkerNode();
    }

    /// <summary>
    /// RemoveConnection via its connectionId
    /// </summary>
    /// <param name="connectionId"></param>
    public void RemoveConnection(string connectionId)
    {
        foreach (var cpuName in CpuMap.Keys.Where(cpuName => CpuMap.ContainsKey(cpuName))
            .Where(cpuName => CpuMap[cpuName].Contains(new WorkerNode { Id = connectionId })))
        {
            if (!CpuMapLost.ContainsKey(cpuName))
                CpuMapLost[cpuName] = new HashSet<WorkerNode>(new CpuComparer());

            CpuMapLost[cpuName].Add(CpuMap[cpuName].FirstOrDefault());
            _logger.Warning($"< Machine: {CpuMap[cpuName].FirstOrDefault()?.Machine}, Client: {cpuName}, Id: {connectionId}, Action: Remove");
            CpuMap[cpuName].Remove(new WorkerNode { Id = connectionId });
        }
    }

    /// <summary>
    /// Remove lost connection
    /// </summary>
    /// <param name="connectionId"></param>
    public void RemoveLostConnection(string connectionId)
    {
        foreach (var cpuName in CpuMapLost.Keys.Where(cpuName => CpuMapLost.ContainsKey(cpuName))
            .Where(cpuName => CpuMapLost[cpuName].Contains(new WorkerNode { Id = connectionId })))
        {
            CpuMapLost[cpuName].Remove(new WorkerNode { Id = connectionId });
        }
    }

    /// <summary>
    /// Update CPU with another CPU structure, checking based on its Connection Id
    /// </summary>
    /// <param name="cpu"></param>
    /// <returns></returns>
    public WorkerNode UpdateCpu(WorkerNode cpu)
    {
        // Get current CPU
        var currentCpu = GetCpuInstance(cpu?.Id) ?? cpu; // Pour éviter que ça plante en cas de décrochage réseau

        // Replace from cpuMap
        if (currentCpu?.Name != null && !CpuMap.ContainsKey(currentCpu.Name))
            return cpu;

        // Remove current CPU from cpuMap
        if (!string.IsNullOrEmpty(currentCpu?.Name) && CpuMap[currentCpu.Name].Remove(currentCpu))
            CpuMap[currentCpu.Name].Add(cpu);

        return cpu;
    }

    /// <summary>
    /// Get CPU instance from its ConnectionId
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    public WorkerNode GetCpuInstance(string connectionId)
    {
        var found = from cpuName in CpuMap.Keys
                    where CpuMap.ContainsKey(cpuName)
                    from cpu in CpuMap[cpuName]
                    where cpu.Id.Equals(connectionId)
                    select cpu;

        var enumerable = found.ToList();

        return enumerable.Any()
            ? enumerable.FirstOrDefault()
            : null;
    }

    /// <summary>
    /// Get all Online connections
    /// </summary>
    /// <param name="cpuName"></param>
    /// <returns></returns>
    public HashSet<WorkerNode> GetOnlineConnections()
    {
        HashSet<WorkerNode> cpuConn;

        try
        {
            cpuConn = CpuMap.Values.SelectMany(x => x).ToHashSet();
        }
        catch
        {
            cpuConn = new HashSet<WorkerNode>();
        }

        return cpuConn;
    }

    /// <summary>
    /// Get all Online connections for 1 cpuName
    /// </summary>
    /// <param name="cpuName"></param>
    /// <returns></returns>
    public HashSet<WorkerNode> GetOnlineConnections(string cpuName)
    {
        HashSet<WorkerNode> cpuConn;

        try
        {
            cpuConn = CpuMap[cpuName];
        }
        catch
        {
            cpuConn = new HashSet<WorkerNode>();
        }

        return cpuConn;
    }

    /// <summary>
    /// Get all Lost connections for 1 cpuName
    /// </summary>
    /// <param name="cpuName"></param>
    /// <returns></returns>
    public HashSet<WorkerNode> GetLostConnections(string cpuName)
    {
        HashSet<WorkerNode> cpuConn;

        try
        {
            cpuConn = CpuMapLost[cpuName];
        }
        catch
        {
            cpuConn = new HashSet<WorkerNode>();
        }

        return cpuConn;
    }
}
