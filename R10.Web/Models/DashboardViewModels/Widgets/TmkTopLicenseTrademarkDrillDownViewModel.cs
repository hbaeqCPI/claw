using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{   
    public class TmkTopLicenseTrademarkDrillDownViewModel : TmkPortfolioByStatusExportViewModel
    {
        public int TmkId { get; set; }

        [Display(Name = "Licensee")]
        public string? Licensee { get; set; }

        [Display(Name = "Licensor")]
        public string? Licensor { get; set; }

        [Display(Name = "License No.")]
        public string? LicenseNo { get; set; }
        [Display(Name = "License Start")]
        public DateTime? LicenseStart { get; set; }

        [Display(Name = "License Expiration")]
        public DateTime? LicenseExpire { get; set; }

        public string? LicenseType { get; set; }
    }
}
