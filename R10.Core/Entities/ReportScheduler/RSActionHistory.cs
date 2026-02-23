using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSActionHistory
    {
        [Key]
        public int ActionHistoryId { get; set; }

        [Required]
        public int LogId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Action")]
        public string? ActionName { get; set; }

        [StringLength(50)]
        [Display(Name = "File Format")]
        public string? OutputFormat { get; set; }

        [StringLength(20)]
        [Display(Name = "Sort Order")]
        public string? SortOrder { get; set; }


        [StringLength(500)]
        [Display(Name = "To")]
        public string? EmailTo { get; set; }

        [StringLength(500)]
        [Display(Name = "CC")]
        public string? EmailCopyTo { get; set; }

        [StringLength(200)]
        [Display(Name = "Subject")]
        public string? EmailSubject { get; set; }

        [StringLength(500)]
        [Display(Name = "Body")]
        public string? EmailBody { get; set; }

        [StringLength(100)]
        [Display(Name = "Host")]
        public string? FTPAddress { get; set; }

        [StringLength(50)]
        [Display(Name = "User")]
        public string? FTPUserID { get; set; }

        [StringLength(50)]
        [Display(Name = "Password")]
        public string? FTPPassword { get; set; }
    }
}
