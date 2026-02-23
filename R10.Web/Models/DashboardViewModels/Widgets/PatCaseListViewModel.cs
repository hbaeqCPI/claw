using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class PatCaseListViewModel
    {
        public int? AppId { get; set; }

        public string? CaseNumber { get; set; }

        public string? Country { get; set; }

        public string? SubCase { get; set; }
        public string? CaseType { get; set; }

        public string? AppTitle { get; set; }

        public string? PatNumber { get; set; }
        public string? AppNumber { get; set; }

        public Nullable<DateTime> IssDate { get; set; }

        public Nullable<DateTime> ExpDate { get; set; }

        public Nullable<DateTime> FilDate { get; set; }

        public int? WLMainId { get; set; }

        public bool CustomCheck { get; set; } = false;
    }

    public class PatInventionExportViewModel
    {
        public string? CaseNumber { get; set; }

        [Display(Name = "Family Number")]
        public string? FamilyNumber { get; set; }

        [Display(Name = "Disclosure Status")]
        public string? DisclosureStatus { get; set; }

        [Display(Name = "Title")]
        public string? InvTitle { get; set; }

        public string? ClientCode { get; set; }
        public string? ClientName { get; set; }

    }
}
