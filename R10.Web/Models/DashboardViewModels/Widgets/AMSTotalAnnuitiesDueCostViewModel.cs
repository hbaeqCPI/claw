using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class AMSTotalAnnuitiesDueCostViewModel
    {
        public string Country { get; set; }
        public string CountryName { get; set; }
        public string Area { get; set; }
        public int TotalAnnuities { get; set; }
        public decimal TotalCost { get; set; }
    }
}
