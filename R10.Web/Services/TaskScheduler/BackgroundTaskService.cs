namespace R10.Web.Services
{
    /// <summary>
    /// Asynchronous timed background task
    /// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-9.0&tabs=visual-studio#asynchronous-timed-background-task
    /// </summary>
    public class BackgroundTaskService : BackgroundService
    {
        public static int TimerPeriodInMinutes = 5;

        private readonly TimeSpan _period = TimeSpan.FromMinutes(TimerPeriodInMinutes);
        private readonly ILogger<BackgroundTaskService> _logger;
        private readonly IServiceScopeFactory _factory;

        private bool _isRunning = false;

        public BackgroundTaskService(ILogger<BackgroundTaskService> logger, IServiceScopeFactory factory)
        {
            _logger = logger;
            _factory = factory;
        }

        public bool IsRunning => _isRunning;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _isRunning = true;
            _logger.LogInformation("Background service is running.");

            // Use a Periodic Timer which does not block resources
            using PeriodicTimer timer = new PeriodicTimer(_period);

            // Check the cancellation token to avoid blocking the application shutdown
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                if (TaskSchedulerSettings.Enabled)
                {
                    try
                    {
                        // Create scope
                        await using AsyncServiceScope asyncScope = _factory.CreateAsyncScope();

                        // Get ScheduledTaskService from scope
                        var taskScheduler = asyncScope.ServiceProvider.GetRequiredService<ScheduledTaskService>();
                        await taskScheduler.RunTasks();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to run scheduled task with exception message: {ex.Message}.");
                    }
                }
            }

            _isRunning = false;
            _logger.LogInformation("Background service is stopping.");
        }
    }
}
