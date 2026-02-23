using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSCriteria : BaseEntity
    {
        public RSCriteria()
        {

        }

        public RSCriteria(RSCriteriaControl rSCriteriaControl, int taskId, string creator, DateTime createTime)
        {
            this.TaskId = taskId;
            this.FieldName = rSCriteriaControl.FieldName;
            this.FieldValue = rSCriteriaControl.DefaultValue;
            this.CreatedBy = creator;
            this.UpdatedBy = creator;
            this.DateCreated = createTime;
            this.LastUpdate = createTime;
        }

        [Key]
        public int SchedCritId { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Field")]
        public string? FieldName { get; set; }

        [StringLength(20)]
        [Display(Name = "Condition")]
        public string? Condition { get; set; }

        [StringLength(255)]
        [Display(Name = "Criteria")]
        public string? FieldValue { get; set; }

        [StringLength(50)]
        [Display(Name = "Special")]
        public string? Special { get; set; }

        public int? ParamOrder { get; set; }

        public RSMain? RSMain { get; set; }
    }
}
