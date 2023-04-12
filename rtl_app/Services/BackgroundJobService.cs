using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

public record BackgroundJob(IServiceScopeFactory scopeFactory,  
                            ILogger<BackgroundJob> logger,
                            IOptions<ApplicationInfo> applicationInfo, IOptions<ApplicationLogging> applicationLogging) : IHostedService, IDisposable
{
    private Timer timer;

    private void Log(string message, System.ConsoleColor color = ConsoleColor.Yellow)
    {
        logger.Log(message, () => applicationLogging != null && applicationLogging.Value.logBackgroundJobService, color);

    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (applicationInfo == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Cannot start BackgroundJob. Cannot determine if this application is the main one");
            Console.ResetColor(); // reset console color to default
        }
        if (!applicationInfo.Value.IsMainApplication)
        {
            return Task.CompletedTask;
        }
        timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(11));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async void DoWork(object state)
    {
        Log("Starting background job");
        try
        {
            // Do something every 10 seconds
            using (var scope = scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IConsumer>();
                // Use context to access database or other scoped services
                await context.ProcessDataIncremental();
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions thrown by the async method
            Log("Something went wrong in the background job. " + ex.Message, ConsoleColor.Red);
        }
        Log("Done with background job");
    }

    public void Dispose()
    {
        timer?.Dispose();
    }
}