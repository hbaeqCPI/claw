using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class CountryApplicationCopyViewModel
    {
        public int AppId { get; set; }

        [Required]
        public string? CaseNumber { get; set; }

        [Required]
        [Display(Name="Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Required]
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Relationship")]
        public string? Relationship { get; set; }

        [Display(Name = "Case Info")]
        public bool CopyCaseInfo { get; set; } = true;

        [Display(Name = "Documents")]
        public bool CopyImages { get; set; } = true;

        [Display(Name = "Inventors")]
        public bool CopyInventors { get; set; } = true;

        //[Display(Name = "Remarks")]
        //public bool CopyRemarks { get; set; }

        [Display(Name = "Assignments")]
        public bool CopyAssignments { get; set; } = true;

        [Display(Name = "Licenses")]
        public bool CopyLicenses { get; set; } = true;

        [Display(Name = "Owners")]
        public bool CopyOwners { get; set; } = true;

        [Display(Name = "Costs")]
        public bool CopyCosts { get; set; } = true;

        [Display(Name = "IDS")]
        public bool CopyIDS { get; set; } = true;

        [Display(Name = "Related Cases")]
        public bool CopyRelatedCases { get; set; } = true;

        [Display(Name = "Related Trademarks")]
        public bool CopyRelatedTrademarks { get; set; } = true;

        [Display(Name = "Inventor Award")]
        public bool CopyInventorAward { get; set; } = true;

        [Display(Name = "Products")]
        public bool CopyProducts { get; set; } = true;

        [Display(Name = "Terminal Disclaimer")]
        public bool CopyTerminalDisclaimer { get; set; } = true;
        
        public int? ParentAppId { get; set; }
        public bool CanCopyIDS { get; set; } = true;
    }
}
