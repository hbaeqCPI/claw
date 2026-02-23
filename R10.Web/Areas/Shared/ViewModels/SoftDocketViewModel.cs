using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class SoftDocketViewModel
    {
        
        public string? Area { get; set; }
        public string? Controller { get; set; } = "ActionDue";

        public string? SystemType { get; set; }
        public int ParentId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }
        public string? CaseType { get; set; }
        public string? Status { get; set; }
        public string? Title { get; set; }
        
        [Required]
        [StringLength(60)]
        public string? ActionType { get; set; }
        public int? ResponsibleId { get; set; }
        public string? Responsible { get; set; }

        [Required]
        [Display(Name ="Due Date")]
        public DateTime DueDate { get; set; }
        public string? Remarks { get; set; }

        public bool HasCountry { get; set; } = true;
        public bool HasSubCase { get; set; } = true;
        public bool HasCaseType { get; set; } = true;
        public bool HasStatus { get; set; } = true;
    }
}
