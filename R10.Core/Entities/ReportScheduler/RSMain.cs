using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSMain : BaseEntity
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        [Display(Name = "Report")]
        public int ReportId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Schedule Name")]
        public string? Name { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Enabled")]
        public bool IsEnabled { get; set; }

        [Display(Name = "Next Run Time")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy hh:mm tt}")]
        public DateTime? NextRunTime { get; set; }

        [Display(Name = "Last Run Time")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy hh:mm tt}")]
        public DateTime? LastRunTime { get; set; }

        [StringLength(255)]
        [Display(Name = "Last Run Result")]
        public string? LastRunResult { get; set; }

        [Required]
        public int FreqTypeId { get; set; }

        [Required]
        [Display(Name = "Task Start")]
        public DateTime TaskStartDateTime { get; set; }

        [Display(Name = "Sunday")]
        public bool Sun { get; set; }

        [Display(Name = "Monday")]
        public bool Mon { get; set; }

        [Display(Name = "Tuesday")]
        public bool Tue { get; set; }

        [Display(Name = "Wednesday")]
        public bool Wed { get; set; }

        [Display(Name = "Thursday")]
        public bool Thu { get; set; }

        [Display(Name = "Friday")]
        public bool Fri { get; set; }

        [Display(Name = "Saturday")]
        public bool Sat { get; set; }

        [StringLength(10)]
        [Display(Name = "day of the Month")]
        public string? DayOfMonth { get; set; }

        [StringLength(50)]
        [Display(Name = "Date Type")]
        public string? DateType { get; set; }

        [Display(Name = "Is Fixed Range")]
        public string? IsFixedRange { get; set; }

        [StringLength(1)]
        [Display(Name = "Start Date Operator")]
        public string? StartDateOperator { get; set; }

        [Display(Name = "Number Of")]
        public int StartDateOffSet { get; set; }

        [StringLength(1)]
        [Display(Name = "Start Date Unit")]
        public string? StartDateUnit { get; set; }

        [Display(Name = "Number Of")]
        public int EndDateOffSet { get; set; }

        [StringLength(10)]
        [Display(Name = "End Date Unit")]
        public string? EndDateUnit { get; set; }

        [StringLength(1)]
        [Display(Name = "Fixed Range")]
        public string? FixedRange { get; set; }

        [StringLength(200)]
        //[Display(Name = "Sched Complete Recipient")]
        public string? SchedCompleteRecipient { get; set; }

        [StringLength(200)]
        //[Display(Name = "Sched Error Recipient")]
        public string? SchedErrorRecipient { get; set; }

        [Display(Name = "Task Expire")]
        public DateTime? TaskExpireDateTime { get; set; }

        [StringLength(450)]
        [Display(Name = "TaskCreatorIdt")]
        public string? TaskCreatorId { get; set; }

        public bool IsShared { get; set; }
        public bool IsEditable { get; set; }

        public List<RSAction>? RSActions { get; set; }
        public List<RSCriteria>? RSCriterias { get; set; }
        public List<RSPrintOption>? RSPrintOptions { get; set; }
        public RSReportType? RSReportType { get; set; }

    }
}
