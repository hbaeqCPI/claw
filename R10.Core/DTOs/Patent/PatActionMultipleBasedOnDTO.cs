using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Core.DTOs
{
    public class PatActionMultipleBasedOnDTO
    {
        [Key]
        public int LogId { get; set; }
        public int AppId { get; set; }

        [Display(Name ="Action Type")]
        public string? ActionType { get; set; }

        [Display(Name = "Action Due")]
        public string? ActionDue { get; set; }

        [Display(Name = "Based On")]
        public string? BasedOn { get; set; }

        [Display(Name = "Base Date")]
        public DateTime? BaseDate { get; set; }
        public DateTime? DueDate { get; set; }

        [Display(Name = "Accept?")]
        public bool Accept { get; set; }
    }

    public class PatActionMultipleBasedOnSelectionDTO
    {
        
        public int LogId { get; set; }
        public bool Accept { get; set; }
    }

}
