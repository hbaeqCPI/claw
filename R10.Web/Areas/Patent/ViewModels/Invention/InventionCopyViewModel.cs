using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionCopyViewModel
    {
        public int CopyInvId { get; set; }

        [Required]
        public string? CaseNumber { get; set; }        


        [Display(Name="Case Info")]
        public bool CopyCaseInfo { get; set; }

        [Display(Name = "Owners")]
        public bool CopyOwners { get; set; }

        [Display(Name = "Inventors")]
        public bool CopyInventors { get; set; }

        [Display(Name = "Priorities")]
        public bool CopyPriorities { get; set; }

        [Display(Name = "Abstract")]
        public bool CopyAbstract { get; set; }

        [Display(Name = "Keywords")]
        public bool CopyKeywords { get; set; }

        [Display(Name = "Documents")]
        public bool CopyImages { get; set; }

        [Display(Name = "Related Inventions")]
        public bool CopyRelatedInventions { get; set; }

        [Display(Name = "Products")]
        public bool CopyProducts { get; set; }

        [Display(Name = "Costs")]
        public bool CopyCosts { get; set; }
    }
}
