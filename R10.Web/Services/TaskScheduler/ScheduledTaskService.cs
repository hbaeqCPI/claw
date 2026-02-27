using Azure;
using DocumentFormat.OpenXml.Bibliography;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using R10.Core.Entities;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Services;
using R10.Core.Services.Shared;
using R10.Web.Areas.Shared.ViewModels;

using R10.Web.Extensions;
using R10.Web.Interfaces;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;

namespace R10.Web.Services
{
    public class ScheduledTaskService
    {
        private readonly ICPiDbContext _cpiDbContext;
        private readonly ILogger<ScheduledTaskService> _logger;
        private readonly INotificationService _notificationService;
        private readonly INotificationSettingManager _settingsManager;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ServiceAccount _serviceAccount;

        public ScheduledTaskService(ICPiDbContext cpiDbContext,
            ILogger<ScheduledTaskService> logger,
            INotificationService notificationService,
            INotificationSettingManager settingsManager,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService,
            IOptions<ServiceAccount> serviceAccount)
        {
            _cpiDbContext = cpiDbContext;
            _logger = logger;
            _notificationService = notificationService;
            _settingsManager = settingsManager;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _serviceAccount = serviceAccount.Value;

            BaseUrl = TaskSchedulerSettings.BaseUrl;
        }

        public string? BaseUrl { get; }

        public IQueryable<ScheduledTask> Tasks => _cpiDbContext.GetRepository<ScheduledTask>().QueryableList;

        public async Task RunTasks()
        {
            try
            {
                var taskIds = await GetTaskIds();
                foreach (var taskId in taskIds)
                {
                    var task = await Tasks.FirstOrDefaultAsync(t => t.TaskId == taskId);
                    if (task == null)
                        continue;

                    // Recheck status
                    if (task.Status == ScheduledTaskStatus.Ready || task.LastRunTime == null || ((DateTime)task.LastRunTime).AddMinutes(task.CancelTimeInMinutes ?? TaskSchedulerSettings.CancellationTokenTimespanInMinutes) <= DateTime.Now)
                    {
                        var result = await RunTask(task);

                        // Task was processed by another host
                        if (result.Status == ScheduledTaskStatus.Skipped)
                        {
                            _logger.LogWarning(result.Message);
                            continue;
                        }

                        // Log failed task
                        if (result.Status == ScheduledTaskStatus.Failed)
                            _logger.LogError($"Failed to run scheduled task \"{task.Name}\" with exception message: {result.Message}.");

                        // Send alert if task did not complete
                        if (result.Status != ScheduledTaskStatus.Completed)
                            await SendNotification(task, result);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to run scheduled task with exception message: {ex.Message}.");
            }
        }

        public async Task<ScheduledTask> SaveTask(ScheduledTask task)
        {
            var repository = _cpiDbContext.GetRepository<ScheduledTask>();

            if (task.TaskId == 0)
                repository.Add(task);
            else
                repository.Update(task);

            await _cpiDbContext.SaveChangesAsync();
            return task;
        }

        public async Task RemoveTask(ScheduledTask task)
        {
            _cpiDbContext.GetRepository<ScheduledTask>().Delete(task);
            await _cpiDbContext.SaveChangesAsync();
        }

        private async Task<List<int>> GetTaskIds()
        {
            return await Tasks.Where(t =>
                                (t.ExpirationTime == null || t.ExpirationTime >= DateTime.Now) &&
                                (t.NextRunTime == null || t.NextRunTime <= DateTime.Now) &&
                                (t.IsEnabled ?? false) &&
                                (t.Status == ScheduledTaskStatus.Ready || t.LastRunTime == null || ((DateTime)t.LastRunTime).AddMinutes(t.CancelTimeInMinutes ?? TaskSchedulerSettings.CancellationTokenTimespanInMinutes) <= DateTime.Now)
                                ).Select(t => t.TaskId).ToListAsync();
        }

        public async Task<BackgroundTaskResult> RunTask(ScheduledTask task)
        {
            var result = BackgroundTaskResult.Running;


            if (!(await UpdateTaskStatus(task, result)))
                return BackgroundTaskResult.Skipped(task);

            try
            {
                if (!string.IsNullOrEmpty(task.RequestUri))
                {
                    using (var httpClient = new HttpClient())
                    {
                        // Default credentials
                        var isServiceAccount = false;
                        if (task.GrantType?.ToLower() == "password" && string.IsNullOrEmpty(task.UserName) && string.IsNullOrEmpty(task.Password))
                        {
                            task.UserName = _serviceAccount.UserName;
                            task.Password = _serviceAccount.Password;

                            isServiceAccount = true;
                        }

                        var requestUri = task.RequestUri.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? task.RequestUri : $"{BaseUrl}{task.RequestUri}";
                        var requestIsSecured = !string.IsNullOrEmpty(task.UserName) && !string.IsNullOrEmpty(task.Password);
                        var authToken = "";

                        if (requestIsSecured)
                            authToken = await GetAuthToken(task);

                        if (requestIsSecured && string.IsNullOrEmpty(authToken))
                            result = BackgroundTaskResult.Failed("Scheduled task API failed when retrieving authentication token.");
                        else
                        {
                            // CPI APIs are using Bearer token
                            if (!string.IsNullOrEmpty(authToken))
                                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                            var request = new HttpRequestMessage
                            {
                                Method = GetHttpMethod(task.RequestMethod),
                                RequestUri = new Uri(requestUri),
                                Content = new StringContent(task.RequestContent ?? "", Encoding.UTF8, "application/json")
                            };

                            // Task scheduler request header
                            request.Headers.Add("X-Client-Type", this.GetType().Name);

                            // Cancellation token
                            var tokenSource = new CancellationTokenSource();
                            tokenSource.CancelAfter(TimeSpan.FromMinutes(task.CancelTimeInMinutes ?? TaskSchedulerSettings.CancellationTokenTimespanInMinutes));

                            // Request timeout
                            httpClient.Timeout = TimeSpan.FromMinutes(TaskSchedulerSettings.HttpClientTimespanInMinutes);

                            using (var response = await httpClient.SendAsync(request, tokenSource.Token))
                            {
                                if (response.IsSuccessStatusCode)
                                    result = BackgroundTaskResult.Completed;
                                else
                                    result = BackgroundTaskResult.Failed(await response.GetErrorMessage());
                            }

                            tokenSource.Dispose();
                        }

                        // Do not save default credentials
                        if (isServiceAccount)
                        {
                            task.UserName = "";
                            task.Password = "";
                        }
                    }
                }
                else
                    result = BackgroundTaskResult.Failed("Scheduled task API is not defined.");
            }
            catch (Exception ex)
            {
                result = BackgroundTaskResult.Failed(ex.Message);
            }

            if (!(await UpdateTaskStatus(task, result)))
            {
                if (result.Status == ScheduledTaskStatus.Completed)
                    result = BackgroundTaskResult.Failed($"Task \"{task.Name}\" completed successfully but error occured while updating the status to {result.Status}.");
            }

            return result;
        }

        private async Task<bool> UpdateTaskStatus(ScheduledTask task, BackgroundTaskResult result)
        {
            _cpiDbContext.GetRepository<ScheduledTask>().Attach(task);

            // Set status
            task.Status = result.Status;
            task.LastRunResult = result.Message;

            // Set last run time
            if (result.Status == ScheduledTaskStatus.Running)
                task.LastRunTime = DateTime.Now;

            else if (result.Status == ScheduledTaskStatus.Completed || result.Status == ScheduledTaskStatus.Failed)
            {
                // Disable one-time task
                if (task.Frequency == ScheduledTaskFrequency.OneTime)
                    task.IsEnabled = false;
                else
                {
                    // Set status of recurring task to Ready
                    task.Status = ScheduledTaskStatus.Ready;

                    // Set next run time
                    task.NextRunTime = GetNextRunTime(task);
                }
            }

            try
            {
                await _cpiDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateConcurrencyException)
                    return false;
                else
                    _logger.LogError(ex, $"Error updating task \"{task.Name}\" status to {result.Status}.");
                return false;
            }

            return true;
        }

        private DateTime GetNextRunTime(ScheduledTask task)
        {
            var runTime = task.LastRunTime ?? DateTime.Now;
            var recurFactor = task.RecurFactor ?? 1;

            if (task.Frequency == ScheduledTaskFrequency.EveryMinute)
                return runTime.AddMinutes(recurFactor);

            else if (task.Frequency == ScheduledTaskFrequency.Daily)
                return runTime.AddDays(recurFactor);

            else if (task.Frequency == ScheduledTaskFrequency.Monthly)
            {
                var nextMonth = runTime.AddMonths(recurFactor);
                var startOfNextMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1);

                if (task.DayOfMonth == null)
                    return nextMonth;

                switch (task.DayOfMonth)
                {
                    case ScheduledTaskDayOfMonth.First:
                        return startOfNextMonth;
                    case ScheduledTaskDayOfMonth.Fifteenth:
                        return new DateTime(nextMonth.Year, nextMonth.Month, 15);
                    case ScheduledTaskDayOfMonth.Last:
                        return startOfNextMonth.AddMonths(1).AddDays(-1);
                }
            }

            else if (task.Frequency == ScheduledTaskFrequency.Weekly)
            {
                var weekDays = GetWeekDays(task);
                if (!weekDays.Any())
                    return runTime.AddDays(7 * recurFactor);

                var increments = Enumerable.Range(1, 6);
                var matchedIncrement = increments.FirstOrDefault(i => weekDays.Contains(runTime.AddDays(i).DayOfWeek));
                return runTime.AddDays((matchedIncrement > 0 ? matchedIncrement : 7) * recurFactor);
            }

            return runTime;
        }

        private List<DayOfWeek> GetWeekDays(ScheduledTask task)        {
            var weekDays = new List<DayOfWeek>();
            if (task.Sun ?? false) weekDays.Add(DayOfWeek.Sunday);
            if (task.Mon ?? false) weekDays.Add(DayOfWeek.Monday);
            if (task.Tue ?? false) weekDays.Add(DayOfWeek.Tuesday);
            if (task.Wed ?? false) weekDays.Add(DayOfWeek.Wednesday);
            if (task.Thu ?? false) weekDays.Add(DayOfWeek.Thursday);
            if (task.Fri ?? false) weekDays.Add(DayOfWeek.Friday);
            if (task.Sat ?? false) weekDays.Add(DayOfWeek.Saturday);
            return weekDays;
        }

        private HttpMethod GetHttpMethod(string? method)
        {
            switch ((method ?? "").ToUpper())
            {
                case "POST": return HttpMethod.Post;
                case "PUT": return HttpMethod.Put;
                case "PATCH": return HttpMethod.Patch;
                case "DELETE": return HttpMethod.Delete;
                default: return HttpMethod.Get;
            }
        }

        private async Task<string> GetAuthToken(ScheduledTask task)
        {
            var tokenEndpoint = string.IsNullOrEmpty(task.TokenEndpoint) ? $"{BaseUrl}connect/token" : task.TokenEndpoint;
            var grantType = string.IsNullOrEmpty(task.GrantType) ? "password" : (task.GrantType ?? "").ToLower();
            var username = task.UserName ?? "";
            var password = task.Password ?? "";

            using (var httpClient = new HttpClient())
            {
                var formData = new List<KeyValuePair<string, string>>();
                formData.Add(new KeyValuePair<string, string>("grant_type", grantType));
                formData.Add(new KeyValuePair<string, string>(grantType == "password" ? "username" : "client_id", username));
                formData.Add(new KeyValuePair<string, string>(grantType == "password" ? "password" : "client_secret", password));

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(tokenEndpoint),
                    Content = new FormUrlEncodedContent(formData)
                };

                // Request timeout
                httpClient.Timeout = TimeSpan.FromMinutes(TaskSchedulerSettings.HttpClientTimespanInMinutes);

                using (var response = await httpClient.SendAsync(request))
                {
                    if (!response.IsSuccessStatusCode) 
                        return "";

                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(content)) 
                        return "";

                    var token = JObject.Parse(content)["access_token"]?.ToString();
                    return token ?? "";
                }
            }
        }

        private async Task SendNotification(ScheduledTask task, BackgroundTaskResult result)
        {
            try
            {
                var recipients = await _settingsManager.GetTaskSchedulerNotificationRecipients();
                await SendAlert(recipients, task, result);


                var emailResult = await SendEmail(recipients, task, result);
                if (!emailResult.Success)
                    _logger.LogError($"Error sending task scheduler notification email. {emailResult.ErrorMessage}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending task scheduler error notification.");
            }
        }

        private async Task SendAlert(List<LocalizedMailAddress> recipients, ScheduledTask task, BackgroundTaskResult result)
        {
            var title = "Task Scheduler Error";
            var callToActionUrl = $"{BaseUrl}Admin/TaskScheduler/Detail/{task.TaskId}";
            var message = $"Failed to run scheduled task \"{task.Name}\" with exception message: {result.Message}.";

            if (!recipients.Any())
                return;

            try
            {
                // Send alert
                await _notificationService.SendAlert("Task Scheduler",
                    recipients.Select(r => new MailAddress(r.Address, r.DisplayName)).ToList(),
                    title, message, callToActionUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending task scheduler error alert.");
            }
        }

        public async Task<EmailSenderResult> SendEmail(List<LocalizedMailAddress> recipients, ScheduledTask task, BackgroundTaskResult result)
        {
            if (recipients.Count > 0)
            {
                var callToActionUrl = $"{BaseUrl}Admin/TaskScheduler/Detail/{task.TaskId}";
                var logoUrl = $"{BaseUrl}images/site_banner.png";
                var emailType = TaskSchedulerSettings.TaskSchedulerNotification;

                //group email by locale
                foreach (var locale in recipients.Select(m => m.Locale).Distinct().ToList())
                {
                    var emailMessage = await _emailTemplateService.GetEmailMessage(emailType, locale,
                        new TaskSchedulerNotification()
                        {
                            TaskName = task.Name,
                            Message = result.Message,
                            CallToAction = "View Scheduled Task",
                            CallToActionUrl = callToActionUrl,
                            LogoUrl = logoUrl
                        });

                    if (emailMessage != null)
                        await _emailSender.SendEmailAsync(recipients.Where(m => m.Locale == locale).Select(m => m.MailAddress).ToList(), emailMessage.Subject, emailMessage.Body);
                    else
                        return new EmailSenderResult() { ErrorMessage = $"Email template \"{emailType} ({locale})\" not found." };
                }
                return new EmailSenderResult() { Success = true };
            }
            else
                return new EmailSenderResult() { ErrorMessage = "No recipients." };
        }
    }
}
