using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class GMPortfolioByStatusDrillDownViewModel : GMPortfolioByStatusExportViewModel
    {
        public int MatId { get; set; }
    }
}
