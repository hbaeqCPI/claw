using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Core.DTOs
{
    public class PatTerminalDisclaimerChildDTO
    {
        [Key]
        public int AppId { get; set; }

        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Expiration Date")]
        public DateTime? ExpDate { get; set; }

        [Display(Name = "New Expiration Date")]
        public DateTime? NewExpDate { get; set; }

    }


}
