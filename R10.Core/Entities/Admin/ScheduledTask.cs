using R10.Core.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace R10.Core.Entities
{
    public class ScheduledTask : BaseEntity
    {

        [Key]
        public int TaskId { get; set; }

        [Required]
        [StringLength(50)]
        public string? Name { get; set; }

        [Required]
        [StringLength(255)]
        public string? Description { get; set; }

        [Display(Name = "Enabled")]
        public bool? IsEnabled { get; set; }

        /// <summary>
        /// Indicates that the task was created from a system feature (e.g. System Status)
        /// It can only be modified or deleted from the source system feature
        /// </summary>
        [Display(Name = "CPI Task")]
        public bool? IsCpiTask { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ScheduledTaskStatus Status { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ScheduledTaskFrequency Frequency { get; set; }

        [Display(Name = "Recur Every")]
        public int? RecurFactor { get; set; }

        [Display(Name = "Day of the Month")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ScheduledTaskDayOfMonth? DayOfMonth { get; set; }

        public bool? Sun { get; set; }
        public bool? Mon { get; set; }
        public bool? Tue { get; set; }
        public bool? Wed { get; set; }
        public bool? Thu { get; set; }
        public bool? Fri { get; set; }
        public bool? Sat { get; set; }

        [Display(Name = "Next Run Time")]
        public DateTime? NextRunTime { get; set; }

        [Display(Name = "Last Run Time")]
        public DateTime? LastRunTime { get; set; }

        [Display(Name = "Last Run Result")]
        public string? LastRunResult { get; set; }

        [Display(Name = "Cancel Task In")]
        public int? CancelTimeInMinutes { get; set; }

        [Display(Name = "Expiration Time")]
        public DateTime? ExpirationTime { get; set; }

        [Required]
        [Display(Name = "Request Uri")]
        [StringLength(2083)]
        public string? RequestUri { get; set; }

        [Display(Name = "Request Method")]
        [StringLength(10)]
        public string? RequestMethod { get; set; }

        [Display(Name = "Request Content")]
        public string? RequestContent { get; set; }

        [Display(Name = "Request Content Type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ScheduledTaskRequestContentType? RequestContentType { get; set; }

        [Display(Name = "Token Endpoint")]
        [StringLength(2083)]
        public string? TokenEndpoint { get; set; }

        [Display(Name = "Grant Type")]
        [StringLength(20)]
        public string? GrantType { get; set; }

        [Encrypted]
        [Display(Name = "User Name")]
        [StringLength(256)]
        public string? UserName { get; set; }

        [Encrypted]
        [Display(Name = "Password")]
        [StringLength(256)]
        public string? Password { get; set; }
    }

    public enum ScheduledTaskFrequency
    {
        [Display(Name = "One time")]
        OneTime,
        [Display(Name = "Every minute")]
        EveryMinute,
        Daily,
        Weekly,
        Monthly
    }

    public enum ScheduledTaskStatus
    {
        Ready,
        Running,
        Completed,
        Failed,
        Skipped
    }

    public enum ScheduledTaskDayOfMonth
    {
        First,
        Fifteenth,
        Last
    }

    public enum ScheduledTaskRequestContentType
    {
        [Display(Name = "raw (JSON)")]
        StringContent
        //,
        //[Display(Name = "x-www-form-urlencoded")]
        //FormUrlEncodedContent
    }

    public class BackgroundTaskResult
    {
        public ScheduledTaskStatus Status { get; set; }
        public string? Message { get; set; }

        public static BackgroundTaskResult Running => new BackgroundTaskResult { Status = ScheduledTaskStatus.Running, Message = "The task is currently running." };

        public static BackgroundTaskResult Completed => new BackgroundTaskResult { Status = ScheduledTaskStatus.Completed, Message = "The operation completed successfully." };

        public static BackgroundTaskResult Failed(string message) => new BackgroundTaskResult { Status = ScheduledTaskStatus.Failed, Message = message };

        public static BackgroundTaskResult Skipped(ScheduledTask task) => new BackgroundTaskResult { Status = ScheduledTaskStatus.Skipped, Message = $"The task \"{task.Name}\" was processed by another host." };
    }
}
