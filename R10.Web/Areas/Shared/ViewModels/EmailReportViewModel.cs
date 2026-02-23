using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class EmailReportViewModel
    {
        public string? Subject { get; set; }
        [Display(Name = "To")]
        [Required]
        public string? To { get; set; }

        [Display(Name = "Cc")]
        public string? CopyTo { get; set; }
        public string? Bcc { get; set; }
        public string? Body { get; set; }
        public string? FromAddress { get; set; }
        public bool ReplyToUseSender { get; set; }
        [StringLength(500)]
        public string? ReplyToAddress { get; set; }
        public string? GeneratedReportName { get; set; }
        public string? EmailReportName { get; set; }
    }
}
