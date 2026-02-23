using R10.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class WorldwideCoverageViewModel : MapDTO
    {
        public int Value { get; set; }    
        public decimal[] Location { get; set; }
    }
}
