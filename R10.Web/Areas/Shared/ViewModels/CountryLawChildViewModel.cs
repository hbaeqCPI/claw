using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class CountryLawChildViewModel : ChildViewModel
    {
        public string? Country { get; set; }
        public string? CaseType { get; set; }
        public bool CanDeleteChild { get; set; }
        
    }
}
