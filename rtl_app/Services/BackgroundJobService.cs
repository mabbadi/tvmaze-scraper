using Microsoft.Extensions.Options;
using StackExchange.Redis;

public record BackgroundJob(IServiceScopeFactory scopeFactory,
                            ILogger<BackgroundJob> logger,
                            IConnectionMultiplexer _redis,
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

                var db = _redis.GetDatabase();
                var acquiredLock = db.StringSet("my-lock-key", "locked", TimeSpan.FromSeconds(20), When.NotExists);
                if (acquiredLock)
                {
                    try
                    {
                        var consumer = scope.ServiceProvider.GetRequiredService<IConsumer>();
                        // Use context to access database or other scoped services
                        await consumer.ProcessDataIncremental();
                    }
                    finally
                    {
                        // Release the lock
                        Log("Release the lock");
                        db.KeyDelete("my-lock-key");
                    }
                }
                else
                {
                    // Another container has the lock
                    Log("Unable to acquire lock");
                }
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