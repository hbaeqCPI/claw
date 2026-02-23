using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{

    public class DueDateExtension : BaseEntity
    {
        [Key]
        public int ExtensionId { get; set; }

        public int DDId { get; set; }

        public int? ExtendDay { get; set; } = 0;
        public int? ExtendWeek { get; set; } = 0;
        public int? ExtendMonth { get; set; } = 0;

        [Display(Name = "Repeat every")]
        public int? RepeatInterval { get; set; } = 0;

        public string? RepeatRecurrence { get; set; } = "D";

        [Display(Name = "Repeat on")]
        public int? RepeatOnDay { get; set; } = 1;

        [Display(Name = "Ends")]
        public string? StopIndicator { get; set; } = "N";

        [Display(Name = "occurences")]
        public int? StopAfterCount { get; set; } = 1;

        [Display(Name = "Stop Date")]
        public DateTime? StopDate { get; set; }

        [Display(Name = "Next Run Date")]
        public DateTime? NextRunDate { get; set; }

        [Display(Name = "Last Run Date")]
        public DateTime? LastRunDate { get; set; }

        public DateTime? NewDueDate { get; set; }
        public DateTime? LastDueDate { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; } = DateTime.Now.Date.AddDays(1);

        [Display(Name = "Active?")]
        public bool IsEnabled  { get; set; } = false;

        [Display(Name = "Occurence Count")]
        public int OccurenceCount { get; set; }
    }


    public class DueDateExtensionLog
    {
        [Key]
        public int LogId { get; set; }
        public int ExtensionId { get; set; }
        public int DDId { get; set; }
        public string? SystemType { get; set; }
        public int? ExtendDay { get; set; }
        public int? ExtendWeek { get; set; }
        public int? ExtendMonth { get; set; }
        public int? RepeatInterval { get; set; } 
        public string? RepeatRecurrence { get; set; }
        public int? RepeatOnDay { get; set; }
        public string? StopIndicator { get; set; }
        public DateTime? StopDate { get; set; }
        public int? StopAfterCount { get; set; }
        public int OccurenceCount { get; set; }
        public DateTime? NewDueDate { get; set; }
        public DateTime? LastDueDate { get; set; }
        public DateTime? NextRunDate { get; set; }
        public DateTime? ExecutedOn { get; set; }
        
    }

    public static class DueDateExtensionRecurrence
    {
        public const string Day = "D";
        public const string Week = "W";
        public const string Month = "M";
    }

    public static class DueDateExtensionStopIndicator
    {
        public const string Never = "N";
        public const string On = "O";
        public const string After = "A";
    }
}
