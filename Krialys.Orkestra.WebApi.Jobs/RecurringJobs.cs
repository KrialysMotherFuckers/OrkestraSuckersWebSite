using Hangfire;
using Krialys.Common.Interfaces;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.WebApi.Jobs.Commun;
using Krialys.Orkestra.WebApi.Services.RefManager;
using Krialys.Orkestra.WebApi.Services.System;

namespace Krialys.Orkestra.WebApi.Jobs;

public interface IRecurringJobs: IScopedService
{
    public void HangfireJobCreation();
}

[HangFirePerformContext]
public class RecurringJobs : IRecurringJobs
{
    private readonly Serilog.ILogger _logger;
    private readonly IRefManagerServices _iRefManagerServices;
    private readonly ICpuServices _iCpuServices;
    private readonly KrialysDbContext _dbContext;

    public RecurringJobs(Serilog.ILogger logger, KrialysDbContext dbContext, IRefManagerServices iRefManagerServices, ICpuServices iCpuServices)
    {
        _iRefManagerServices = iRefManagerServices ?? throw new ArgumentNullException(nameof(iRefManagerServices));
        _iCpuServices = iCpuServices ?? throw new ArgumentNullException(nameof(iCpuServices));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Cron scheduler.
    /// <br/>Ref.: https://www.atatus.com/tools/cron
    /// </summary>
    public void HangfireJobCreation()
    {
        // At 05:00 AM, Monday through Sunday
        RecurringJob.AddOrUpdate("CpuServices-LogsCheckForCleanupInformation", () => _iCpuServices.LogsCheckForCleanupInformation(), "0 5 * * 1-7");

        // At 06:00 AM, Monday through Friday
        RecurringJob.AddOrUpdate("CpuServices-CmdCheckLifePhaseEveryDay", () => _iCpuServices.CmdCheckLifePhaseEveryDay(), "0 6 * * 1-5");

        // At 08:00 AM, Monday through Friday
        //RecurringJob.AddOrUpdate("RefManager-RefreshData", () => _iRefManagerServices.RefreshData(), "0 8 * * 1-5");
    }
}
