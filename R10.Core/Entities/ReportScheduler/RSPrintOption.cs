using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSPrintOption : BaseEntity
    {
        public RSPrintOption()
        {

        }

        public RSPrintOption(RSPrintOptionControl rSPrintOptionControl, int taskId, string creator, DateTime createTime)
        {
            this.TaskId = taskId;
            this.OptionName = rSPrintOptionControl.OptionName;
            this.OptionValue = rSPrintOptionControl.DefaultValue;
            this.CreatedBy = creator;
            this.UpdatedBy = creator;
            this.DateCreated = createTime;
            this.LastUpdate = createTime;
        }

        [Key]
        public int SchedParamId { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Option")]
        public string? OptionName { get; set; }

        [Required]
        [Display(Name = "Print")]
        public bool OptionValue { get; set; }

        public RSMain? RSMain { get; set; }
    }
}
