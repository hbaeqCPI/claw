using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class DMSPatentPortfolioByStatusDrillDownViewModel : PatPortfolioByStatusExportViewModel
    {
        public int AppId { get; set; }

        public int InvId { get; set; }

        public int DMSId { get; set; }

        [Display(Name = "Disclosure Number")]
        public string? DisclosureNumber { get; set; }
    }

    public class DMSPatentPortfolioByStatusExportViewModel : PatPortfolioByStatusExportViewModel
    {
        [Display(Name = "Disclosure Number")]
        public string? DisclosureNumber { get; set; }
    }

    public class DMSPortfolioByStatusDrillDownViewModel: DmsPortfolioByStatusExportViewModel
    {
        public int DMSId { get; set; }
    }
}
