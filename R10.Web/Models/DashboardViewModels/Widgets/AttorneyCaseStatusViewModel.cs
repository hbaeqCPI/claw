using R10.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class AttorneyCaseStatusViewModel : ChartDTO
    {
        public int LegendOrder { get; set; }
        public int Id { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
