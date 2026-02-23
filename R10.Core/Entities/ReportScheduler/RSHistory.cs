using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSHistoryView : RSHistoryDetail
    {
        public RSHistoryView()
        {

        }

        public RSHistoryView(RSMain rSMain, int actionId)
        {
            this.TaskId = rSMain.TaskId;
            this.Name = rSMain.Name;
            this.Description = rSMain.Description;
            this.ReportName = "";
            this.Frequency = "";
            this.TaskStartDateTime = rSMain.TaskStartDateTime;
            this.StartDateTime = DateTime.Now;
            //this.EndDateTime = DateTime.;
            //this.ElapsedTime = TimeSpan.Zero;
            this.Status = "Initiate";
            this.IsEnabled = rSMain.IsEnabled;
            this.Message = "History Record Initiate";
            this.Sun = rSMain.Sun;
            this.Mon = rSMain.Mon;
            this.Tue = rSMain.Tue;
            this.Wed = rSMain.Wed;
            this.Thu = rSMain.Thu;
            this.Fri = rSMain.Fri;
            this.Sat = rSMain.Sat;
            this.CreatedBy = "admin";
            this.UpdatedBy = "admin";
            this.DateCreated = this.TaskStartDateTime;
            this.LastUpdate = this.TaskStartDateTime;
            this.FileName = "";
            this.DayOfMonth = rSMain.DayOfMonth;
            this.DateType = rSMain.DateType;
            this.IsFixedRange = rSMain.IsFixedRange;
            this.StartDateOperator = rSMain.StartDateOperator;
            this.StartDateOffSet = rSMain.StartDateOffSet;
            this.StartDateUnit = rSMain.StartDateUnit;
            this.EndDateOffSet = rSMain.EndDateOffSet;
            this.EndDateUnit = rSMain.EndDateUnit;
            this.FixedRange = rSMain.FixedRange;
            this.Action = "";
            this.SchedCompleteRecipient = rSMain.SchedCompleteRecipient;
            this.SchedErrorRecipient = rSMain.SchedErrorRecipient;
            this.TaskExpireDateTime = rSMain.TaskExpireDateTime;
            this.TaskCreatorId = rSMain.TaskCreatorId;
            this.ActionId = actionId;
        }
    }
    public class RSHistory : RSHistoryDetail
    {
        public RSHistory()
        {

        }

        public RSHistory(RSMain rSMain, int actionId)
        {
            this.TaskId = rSMain.TaskId;
            this.Name = rSMain.Name;
            this.Description = rSMain.Description;
            this.ReportName = "";
            this.Frequency = "";
            this.TaskStartDateTime = rSMain.TaskStartDateTime;
            this.StartDateTime = DateTime.Now;
            //this.EndDateTime = DateTime.;
            //this.ElapsedTime = TimeSpan.Zero;
            this.Status = "Initiate";
            this.IsEnabled = rSMain.IsEnabled;
            this.Message = "History Record Initiate";
            this.Sun = rSMain.Sun;
            this.Mon = rSMain.Mon;
            this.Tue = rSMain.Tue;
            this.Wed = rSMain.Wed;
            this.Thu = rSMain.Thu;
            this.Fri = rSMain.Fri;
            this.Sat = rSMain.Sat;
            this.CreatedBy = "admin";
            this.UpdatedBy = "admin";
            this.DateCreated = this.TaskStartDateTime;
            this.LastUpdate = this.TaskStartDateTime;
            this.FileName = "";
            this.DayOfMonth = rSMain.DayOfMonth;
            this.DateType = rSMain.DateType;
            this.IsFixedRange = rSMain.IsFixedRange;
            this.StartDateOperator = rSMain.StartDateOperator;
            this.StartDateOffSet = rSMain.StartDateOffSet;
            this.StartDateUnit = rSMain.StartDateUnit;
            this.EndDateOffSet = rSMain.EndDateOffSet;
            this.EndDateUnit = rSMain.EndDateUnit;
            this.FixedRange = rSMain.FixedRange;
            this.Action = "";
            this.SchedCompleteRecipient = rSMain.SchedCompleteRecipient;
            this.SchedErrorRecipient = rSMain.SchedErrorRecipient;
            this.TaskExpireDateTime = rSMain.TaskExpireDateTime;
            this.TaskCreatorId = rSMain.TaskCreatorId;
            this.ActionId = actionId;
        }

        [NotMapped]
        public List<RSActionHistory> RSActionHistorys { get; set; }
        [NotMapped]
        public List<RSCriteriaHistory> RSCriteriaHistorys { get; set; }
        [NotMapped] 
        public List<RSPrintOptionHistory> RSPrintOptionHistorys { get; set; }
    }

    public class RSHistoryDetail : BaseEntity
    {
        public RSHistoryDetail()
        {

        }

        public RSHistoryDetail(RSMain rSMain)
        {
            this.TaskId = rSMain.TaskId;
            this.Name = rSMain.Name;
            this.Description = rSMain.Description;
            this.ReportName = "";
            this.Frequency = "";
            this.TaskStartDateTime = rSMain.TaskStartDateTime;
            this.StartDateTime = DateTime.Now;
            //this.EndDateTime = DateTime.;
            //this.ElapsedTime = TimeSpan.Zero;
            this.Status = "Initiate";
            this.IsEnabled = rSMain.IsEnabled;
            this.Message = "History Record Initiate";
            this.Sun = rSMain.Sun;
            this.Mon = rSMain.Mon;
            this.Tue = rSMain.Tue;
            this.Wed = rSMain.Wed;
            this.Thu = rSMain.Thu;
            this.Fri = rSMain.Fri;
            this.Sat = rSMain.Sat;
            this.CreatedBy = "admin";
            this.UpdatedBy = "admin";
            this.DateCreated = this.TaskStartDateTime;
            this.LastUpdate = this.TaskStartDateTime;
            this.FileName = "";
            this.DayOfMonth = rSMain.DayOfMonth;
            this.DateType = rSMain.DateType;
            this.IsFixedRange = rSMain.IsFixedRange;
            this.StartDateOperator = rSMain.StartDateOperator;
            this.StartDateOffSet = rSMain.StartDateOffSet;
            this.StartDateUnit = rSMain.StartDateUnit;
            this.EndDateOffSet = rSMain.EndDateOffSet;
            this.EndDateUnit = rSMain.EndDateUnit;
            this.FixedRange = rSMain.FixedRange;
            this.Action = "";
            this.SchedCompleteRecipient = rSMain.SchedCompleteRecipient;
            this.SchedErrorRecipient = rSMain.SchedErrorRecipient;
            this.TaskExpireDateTime = rSMain.TaskExpireDateTime;
            this.TaskCreatorId = rSMain.TaskCreatorId;
        }

        [Key]
        public int LogId { get; set; }

        [Required]
        public int TaskId { get; set; }
        public int ActionId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Schedule Name")]
        public string? Name { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Report Name")]
        public string? ReportName { get; set; }

        [StringLength(20)]
        [Display(Name = "Frequency")]
        public string? Frequency { get; set; }

        [Display(Name = "Task Start")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy hh:mm tt}")]
        public DateTime? TaskStartDateTime { get; set; }

        [Display(Name = "Start Date Time")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy hh:mm tt}")]
        public DateTime? StartDateTime { get; set; }

        [Display(Name = "End Date Time")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy hh:mm tt}")]
        public DateTime? EndDateTime { get; set; }

        [Display(Name = "Elapsed Time")]
        public TimeSpan? ElapsedTime { get; set; }

        [StringLength(50)]
        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Required]
        [Display(Name = "Enabled")]
        public bool IsEnabled { get; set; }

        [Display(Name = "Message")]
        public string? Message { get; set; }

        [Display(Name = "Exception")]
        public string? Exception { get; set; }

        [Required]
        [Display(Name = "Sunday")]
        public bool Sun { get; set; }

        [Required]
        [Display(Name = "Monday")]
        public bool Mon { get; set; }

        [Required]
        [Display(Name = "Tuesday")]
        public bool Tue { get; set; }

        [Required]
        [Display(Name = "Wednesday")]
        public bool Wed { get; set; }

        [Required]
        [Display(Name = "Thursday")]
        public bool Thu { get; set; }

        [Required]
        [Display(Name = "Friday")]
        public bool Fri { get; set; }

        [Required]
        [Display(Name = "Saturday")]
        public bool Sat { get; set; }

        [StringLength(100)]
        [Display(Name = "File Name")]
        public string? FileName { get; set; }

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

        [Display(Name = "Start Date OffSet")]
        public int StartDateOffSet { get; set; }

        [StringLength(1)]
        [Display(Name = "Start Date Unit")]
        public string? StartDateUnit { get; set; }

        [Display(Name = "End Date OffSet")]
        public int EndDateOffSet { get; set; }

        [StringLength(10)]
        [Display(Name = "End Date Unit")]
        public string? EndDateUnit { get; set; }

        [StringLength(1)]
        [Display(Name = "Fixed Range")]
        public string? FixedRange { get; set; }

        [StringLength(50)]
        [Display(Name = "Action")]
        public string? Action { get; set; }

        [StringLength(200)]
        [Display(Name = "Sched Complete Recipient")]
        public string? SchedCompleteRecipient { get; set; }

        [StringLength(200)]
        [Display(Name = "Sched Error Recipient")]
        public string? SchedErrorRecipient { get; set; }

        [Display(Name = "Task Expire")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy hh:mm tt}")]
        public DateTime? TaskExpireDateTime { get; set; }

        [StringLength(450)]
        [Display(Name = "TaskCreatorId")]
        public string? TaskCreatorId { get; set; }
    }
}



