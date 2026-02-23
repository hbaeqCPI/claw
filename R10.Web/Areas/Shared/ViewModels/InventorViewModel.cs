using R10.Core.Entities;
using R10.Web.Areas.Patent.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class InventorViewModel : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int ParentId { get; set; }

        public int InventorID { get; set; }

        public int OrderOfEntry { get; set; }

        public string? Remarks { get; set; }

        [UIHint("PatInventor")]
        [Display(Name = "Inventor")]
        public PatInventorListViewModel? InventorDetail { get; set; }
        public bool ReadOnly { get; set; }

        [Display(Name = "Applicant?")]
        public bool IsApplicant { get; set; }
        [Display(Name = "Award")]
        public bool EligibleForBasicAward { get; set; }

        [Display(Name = "% of Invention")]
        [Range(0, 100, ErrorMessage = "Enter number between 0 to 100")]
        public double? Percentage { get; set; }
    }
}
