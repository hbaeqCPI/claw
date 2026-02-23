using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class AmsCaseListViewModel : CaseListViewModel
    {
        public string? Instruction { get; set; }
        public DateTime? InstructionDate { get; set; }
        public string? ClientCode { get; set; }
    }
}
