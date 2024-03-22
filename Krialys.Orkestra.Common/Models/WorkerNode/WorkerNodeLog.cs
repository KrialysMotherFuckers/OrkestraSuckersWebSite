using Krialys.Common.Extensions;

namespace Krialys.Orkestra.Common.Models.WorkerNode;

public class WorkerNodeLog
{
    #region Properties

    /// <summary>
    /// Gets or sets the worker node identifier.
    /// </summary>
    /// <value>
    /// The worker node identifier.
    /// </value>
    public string WorkerNodeId { get; set; }

    /// <summary>
    /// Gets or sets the name of the worker node.
    /// </summary>
    /// <value>
    /// The name of the worker node.
    /// </value>
    public string WorkerNodeName { get; set; }

    /// <summary>
    /// Gets or sets the worker node machine.
    /// </summary>
    /// <value>
    /// The worker node machine.
    /// </value>
    public string WorkerNodeMachine { get; set; }

    /// <summary>
    /// Gets or sets the name of the invoked function.
    /// </summary>
    /// <value>
    /// The name of the invoked function.
    /// </value>
    public string InvokedFunctionName { get; set; }

    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    /// <value>
    /// The request identifier.
    /// </value>
    public int? RequestId { get; set; }

    /// <summary>
    /// Gets or sets the workflow step.
    /// </summary>
    /// <value>
    /// The workflow step.
    /// </value>
    public string WorkflowStep { get; set; }

    /// <summary>
    /// Gets or sets the error. If not null, else will contain ex.Message
    /// </summary>
    /// <value>
    /// The error.
    /// </value>
    public string Error { get; set; }

    /// <summary>
    /// Gets or sets the log date.
    /// </summary>
    /// <value>
    /// The date.
    /// </value>
    public DateTime? Date { get; set; }

    #endregion

    public WorkerNodeLog() { }

    public WorkerNodeLog(WorkerNode workerNodeLog, string invokedFunctionName, string error)
    {
        WorkerNodeId = workerNodeLog.Id;
        WorkerNodeName = workerNodeLog.Name;
        WorkerNodeMachine = workerNodeLog.Machine;
        RequestId = workerNodeLog.Demande.DemandeId;
        WorkflowStep = workerNodeLog.StepWorkflow;

        InvokedFunctionName = invokedFunctionName;
        Error = error;
        Date = DateTime.UtcNow.Truncate(TimeSpan.FromSeconds(1));
    }
}
