using R10.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PortfolioAnalysisStackedChart : ChartDTO
    {
        public int Id { get; set; }
        public decimal StackLayer1 { get; set; }
        public decimal StackLayer2 { get; set; }
        public decimal StackLayer3 { get; set; }
    }

    public class PortfolioAnalysisCoverageViewModel : MapDTO
    {
        public int Value { get; set; }
        public decimal[] Location { get; set; }
    }
}
