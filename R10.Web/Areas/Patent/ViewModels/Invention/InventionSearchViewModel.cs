using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R9.Web.ViewModels.Patent
{
    public class InventionSearchCriteriaViewModel: BaseSearchCriteriaViewModel
    {
        public int InvId { get; set; }
        public string CaseNumber { get; set; }
    }
}
