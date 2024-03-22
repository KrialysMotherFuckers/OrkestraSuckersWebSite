using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Krialys.Orkestra.WebApi.Jobs;
public class HangfireWorker : BackgroundService, IAsyncDisposable
{
    private readonly Serilog.ILogger _logger;
    private readonly IServiceProvider _iServiceProvider;

    private IRecurringJobs _iRecurringJobs;

    public HangfireWorker(Serilog.ILogger logger, IServiceProvider iServiceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _iServiceProvider = iServiceProvider ?? throw new ArgumentNullException(nameof(iServiceProvider));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using (var scope = _iServiceProvider.CreateScope())
            {
                _iRecurringJobs = scope.ServiceProvider.GetService<IRecurringJobs>();
                await Task.Delay(new TimeSpan(0, 0, 1));

                _iRecurringJobs.HangfireJobCreation();
            }

        }
        catch (TaskCanceledException)
        {
            _logger.Error($"> [HangfireWorker: ExecuteAsync Method] Error: Task canceled");
        }
        catch (Exception ex)
        {
            _logger.Error($"> [HangfireWorker: ExecuteAsync Method] Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
