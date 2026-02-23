namespace R10.Web.Services
{
    public static class TaskSchedulerSettings
    {
        public static bool Enabled { get; private set; }
        public static string? BaseUrl { get; private set; }
        public static double CancellationTokenTimespanInMinutes { get; private set; } = 60;
        public static double HttpClientTimespanInMinutes { get; private set; } = 5;
        public static string? TaskSchedulerNotification { get; private set; } = "Task Scheduler Notification";

        public static void SetTaskScheduler(this IConfiguration configuration)
        {
            var enabled = configuration["TaskScheduler:Enabled"];
            if (!string.IsNullOrEmpty(enabled))
                Enabled = bool.Parse(enabled);

            var baseUrl = configuration["TaskScheduler:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
                BaseUrl = baseUrl;
            else
            {
                var reportApiUrl = configuration["Report:ClientApiUrl"];
                if (!string.IsNullOrEmpty(reportApiUrl))
                {
                    var uri = new Uri(reportApiUrl);
                    BaseUrl = uri.GetLeftPart(System.UriPartial.Authority) + "/";
                }
            }

            var cancellationToken = configuration["TaskScheduler:CancellationTokenTimespanInMinutes"];
            if (!string.IsNullOrEmpty(cancellationToken))
                CancellationTokenTimespanInMinutes = Double.Parse(cancellationToken);

            var requestTimeout = configuration["TaskScheduler:HttpClientTimespanInMinutes"];
            if (!string.IsNullOrEmpty(requestTimeout))
                HttpClientTimespanInMinutes = Double.Parse(requestTimeout);

            var notification = configuration["TaskScheduler:TaskSchedulerNotification"];
            if (!string.IsNullOrEmpty(notification))
                TaskSchedulerNotification = notification;
        }
    }
}
