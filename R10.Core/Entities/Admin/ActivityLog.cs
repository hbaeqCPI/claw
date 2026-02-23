using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Activity Date")]
        public DateTime ActivityDate { get; set; }

        [StringLength(256)]
        [Display(Name = "User Id")]
        public string? UserId { get; set; }

        [StringLength(128)]
        [Display(Name = "Host Name")]
        public string? HostName { get; set; }

        [StringLength(128)]
        [Display(Name = "Host IP")]
        public string? HostIP { get; set; }

        [Display(Name = "Request Url")]
        public string? RequestUrl { get; set; }

        [Display(Name = "Request Form")]
        public string? RequestForm { get; set; }

        [StringLength(10)]
        [Display(Name = "Request Method")]
        public string? RequestMethod { get; set; }

        [Display(Name = "Status Code")]
        public int? StatusCode { get; set; }

        [StringLength(128)]
        [Display(Name = "Remote Address")]
        public string? RemoteAddress { get; set; }

        [StringLength(256)]
        [Display(Name = "User Agent")]
        public string? UserAgent { get; set; }

        [StringLength(128)]
        [Display(Name = "Accept Language")]
        public string? AcceptLanguage { get; set; }
    }
}
