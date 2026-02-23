using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSAction : BaseEntity
    {
        [Key]
        public int ActionId { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        [Display(Name = "Action")]
        public int ActionTypeId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "File Format")]
        public string? OutputFormat { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Sort Order")]
        public string? SortOrder { get; set; }

        [Display(Name = "Status")]
        public bool IsEnabled { get; set; }

        [StringLength(500)]
        [Display(Name = "To")]
        public string? EmailTo { get; set; }

        [StringLength(500)]
        [Display(Name = "CC")]
        public string? EmailCopyTo { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Subject")]
        public string? EmailSubject { get; set; }

        [Required]
        [Display(Name = "Body")]
        public string? EmailBody { get; set; }

        [Display(Name = "Header")]
        public string? EmailHeader { get; set; }

        [Display(Name = "Footer")]
        public string? EmailFooter { get; set; }

        [StringLength(100)]
        [Display(Name = "Host")]
        public string? FTPAddress { get; set; }

        [StringLength(50)]
        [Display(Name = "User")]
        public string? FTPUserID { get; set; }

        [StringLength(50)]
        [Display(Name = "Password")]
        public string? FTPPassword { get; set; }

        public RSMain? RSMain { get; set; }
    }
}
