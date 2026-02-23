using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class AMSAnnuitiesDueThisYearViewModel
    {
        public string Group { get; set; }
        public decimal Value { get; set; }

        [Display(Name = "Case Number")]
        public string CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Sub Case")]
        public string SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string CPICaseType { get; set; }

        [Display(Name = "Title")]
        public string CPITitle { get; set; }

        [Display(Name = "Application No.")]
        public string CPIAppNo { get; set; }

        [Display(Name = "Patent No.")]
        public string CPIPatNo { get; set; }

        [Display(Name = "Expiration Date")]
        public DateTime? CPIExpireDate { get; set; }

        [Display(Name = "Annuity Due Date")]
        public DateTime? AnnuityDueDate { get; set; }

        [Display(Name = "Annuity Cost")]
        public decimal AnnuityCost { get; set; }
    }
}
