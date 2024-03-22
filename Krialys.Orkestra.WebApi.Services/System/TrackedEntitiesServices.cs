using Krialys.Common;
using Krialys.Data.EF.Logs;
using Krialys.Orkestra.Common.Models;
using Krialys.Orkestra.Common.Models.WorkerNode;
using Krialys.Orkestra.WebApi.Services.System.HUB;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Serilog.Events;
using System.Text;
using System.Text.Json;

namespace Krialys.Orkestra.WebApi.Services.System;

public interface ITrackedEntitiesServices : ISingletonService
{
    /// <summary>
    /// Add all tracked entities (used by GenericCrud)
    /// </summary>
    /// <param name="changeTracker">ChangeTracker</param>
    /// <param name="fullNames">Entity fullname</param>
    /// <param name="verb">Action kind (insert, update, patch, delete)</param>
    /// <param name="userId">User id that did the action</param>
    /// <param name="uuid"></param>
    Task<bool> AddTrackedEntitiesAsync(ChangeTracker changeTracker, string[] fullNames, string verb, string userId, string uuid);

    /// <summary>
    /// Get tracked entities relative to TIMEOUT (used by CommonController)
    /// </summary>
    /// <param name="entityTypes">Name of an entity, else * if you want all entities, ordered by last execution time</param>
    /// <param name="lastChecked"></param>
    /// <returns>Enumerable TrackedEntities (ordered by date descending)</returns>
    IEnumerable<TrackedEntity> GetTrackedEntities(string[] entityTypes, DateTime lastChecked);

    /// <summary>
    /// Get tracked Logger (used through 'IHttpProxyCore' RefIt oriented interface)
    /// </summary>
    /// <param name="cpuId"></param>
    /// <param name="lastChecked"></param>
    /// <returns>Enumerable TrackedEntities</returns>
    IEnumerable<WorkerNodeLog> GetTrackedLogger(string cpuId, DateTime lastChecked);
}

/// <summary>
/// Entities tracker
/// </summary>
public class TrackedEntitiesServices : ITrackedEntitiesServices
{
    #region PARAMETERS
    private const int MaxBufferSize = 25_000;
    private static readonly CircularBuffer<TrackedEntity> TrackedEntities = new CircularBuffer<TrackedEntity>(MaxBufferSize, new TrackedEntity[] { });
    private static DateTime? _trackedLoggerMessageDateTime = DateTime.MinValue;
    private readonly IHubContext<ChatHub> _hubContext;
    #endregion

    public TrackedEntitiesServices(IHubContext<ChatHub> hubContext)
        => _hubContext = hubContext;

    /// <summary>
    /// Add all tracked entities (used by 'GenericCrud' controller)
    /// </summary>
    /// <param name="changeTracker">ChangeTracker</param>
    /// <param name="fullNames">Entity/ies fullnames</param>
    /// <param name="verb">Action kind (insert, update, patch, delete)</param>
    /// <param name="userId">User id that did the action</param>
    /// <param name="uuid"></param>
    public async Task<bool> AddTrackedEntitiesAsync(ChangeTracker changeTracker, string[] fullNames, string verb, string userId, string uuid)
    {
        var dt = DateTime.UtcNow;
        var trackedEntities = new HashSet<TrackedEntity>();

        if (changeTracker is not null && changeTracker.Entries().Any())
        {
            int count = 0;
            // Traverse changetracker entries
            foreach (var entry in changeTracker.Entries())
            {
                var item = new TrackedEntity
                {
                    Id = HashCode.Combine(entry.Entity.GetType().FullName, dt, verb, userId, uuid),
                    FullName = entry.Entity.GetType().FullName,
                    Date = dt,
                    Action = verb,
                    UserId = userId,
                    UuidOrAny = uuid,
                    //Entity = entry.Entity,
                };

                trackedEntities.Add(item);

                if (!TrackedEntities.Contains(item))
                    TrackedEntities.PushBack(item);

                count++;
            }

            // Support multiple entities
            if (count > 1)
            {
                for (int i = 1; i < fullNames.Length; i++)
                {
                    var item = new TrackedEntity
                    {
                        Id = HashCode.Combine(fullNames[i], dt, verb, userId, uuid),
                        FullName = fullNames[i],
                        Date = dt,
                        Action = verb,
                        UserId = userId,
                        UuidOrAny = uuid,
                        //Entity = entry.Entity,
                    };

                    trackedEntities.Add(item);

                    if (!TrackedEntities.Contains(item))
                        TrackedEntities.PushBack(item);
                }
            }
        }
        else
        {
            // Support multiple entities
            foreach (var fullName in fullNames)
            {
                var item = new TrackedEntity
                {
                    Id = HashCode.Combine(fullName, dt, verb, userId, uuid),
                    Date = dt,
                    FullName = fullName,
                    Action = verb,
                    UserId = userId,
                    UuidOrAny = uuid,
                    //Entity = Enumerable.Empty<TrackedEntity>(),
                };

                trackedEntities.Add(item);

                if (!TrackedEntities.Contains(item))
                    TrackedEntities.PushBack(item);
            }
        }

        if (trackedEntities.Any())
        {
            var foundLogger = trackedEntities.Any(x
                => x.Action.Equals(LogEventLevel.Error.ToString())
                || x.Action.Equals(LogEventLevel.Warning.ToString()));

            // Then inject the newest entry to the right circuit
            if (foundLogger)
            {
                var cpuLog = JsonSerializer.Deserialize<WorkerNodeLog>(DecodeFrom64(trackedEntities.FirstOrDefault()!.UuidOrAny));

                var dateTime = cpuLog!.Date;
                if (dateTime == null
                    || _trackedLoggerMessageDateTime == null
                    || _trackedLoggerMessageDateTime.Value >= dateTime.Value)
                {
                    return true;
                }

                if (cpuLog.Date != null) _trackedLoggerMessageDateTime = cpuLog.Date.Value;

                // Invoke event dedicated to Logger
                await _hubContext.Clients.All.SendCoreAsync("OnReceiveTrackedLoggerMessage", new object[] { cpuLog });
            }
            else
            {
                // Invoke event dedicated to standard CRUD
                await _hubContext.Clients.All.SendCoreAsync("OnReceiveTrackedMessage", new object[] { trackedEntities });
            }
        }

        return true;
    }

    private static string DecodeFrom64(string encodedData)
        => Encoding.UTF8.GetString(Convert.FromBase64String(encodedData));

    /// <summary>
    /// Get tracked entities (used through 'IHttpProxyCore' RefIt oriented interface)
    /// </summary>
    /// <param name="entityTypes">Entities names (or null), else * if you want all entities, ordered by last execution time</param>
    /// <param name="lastChecked">Date and time filter where to start from</param>
    /// <returns>Enumerable TrackedEntities</returns>
    public IEnumerable<TrackedEntity> GetTrackedEntities(string[] entityTypes, DateTime lastChecked)
    {
        IList<TrackedEntity> listEntities = new List<TrackedEntity>();
        bool isNotNull = entityTypes is not null && entityTypes.Any();
        //var size = TrackedEntities.Size;

        if (isNotNull)
        {
            TrackedEntities.Where(x => entityTypes.Contains(x.FullName))
                           .ToList()
                           .ForEach(x => listEntities.Add(x));
        }

        // Get eligible items
        IList<TrackedEntity> entities = isNotNull
            ? listEntities.Where(x => x.Date > lastChecked).OrderByDescending(o => o.Date).ToList()
            : TrackedEntities.Where(x => x.Date > lastChecked).OrderByDescending(o => o.Date).ToList();

        return entities.Any()
            ? entities
            : Enumerable.Empty<TrackedEntity>();
    }

    /// <summary>
    /// Get tracked Logger (used through 'IHttpProxyCore' RefIt oriented interface)
    /// </summary>
    /// <param name="cpuId">Name of an entity, else * if you want all entities, ordered by last execution time</param>
    /// <param name="lastChecked"></param>
    /// <returns>Enumerable TrackedEntities</returns>
    public IEnumerable<WorkerNodeLog> GetTrackedLogger(string cpuId, DateTime lastChecked)
    {
        HashSet<WorkerNodeLog> cpuLog = new();

        // Get eligible items
        foreach (var entity in GetTrackedEntities(new[] { typeof(TM_LOG_Logs).FullName }, lastChecked))
        {
            var data = DecodeFrom64(entity.UuidOrAny);
            if (string.IsNullOrEmpty(data))
                continue;

            var log = JsonSerializer.Deserialize<WorkerNodeLog>(DecodeFrom64(entity.UuidOrAny));
            if (!log.WorkerNodeId.Equals(cpuId))
                continue;

            cpuLog.Add(log);
        }

        return cpuLog.Any()
            ? cpuLog.OrderByDescending(o => o.Date)
            : Enumerable.Empty<WorkerNodeLog>();
    }
}