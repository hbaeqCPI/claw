using R10.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class StackedChartViewModel : ChartDTO
    {
        public string? GroupName { get; set; }
        public string? CategoryName { get; set; }
        public string? UniqueId { get; set; }
        public int Id { get; set; }
        public decimal StackLayer1 { get; set; }
        public decimal StackLayer2 { get; set; }
        public decimal StackLayer3 { get; set; }        
    }
}
