using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;

namespace Krialys.Orkestra.WebApi.Jobs.Commun;

[AttributeUsage(AttributeTargets.Class)]
public class HangFirePerformContextAttribute : JobFilterAttribute, IServerFilter
{
    private static PerformContext _context;

    public static PerformContext PerformContext
    {
        get
        {
            return new PerformContext(_context);
        }
    }

    public static void SetContextManually(IStorageConnection connection, BackgroundJob backgroundJob, IJobCancellationToken cancellationToken)
    {
        _context ??= new PerformContext(null, connection, backgroundJob, cancellationToken);
    }

    public void OnPerformed(PerformedContext filterContext)
    {
        var context = (PerformContext)filterContext;
        _context = context;
    }
    public void OnPerforming(PerformingContext filterContext)
    {
        var context = (PerformContext)filterContext;
        _context = context;
    }
}
