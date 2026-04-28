using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web
{
    public class CaseNumberLookupViewModel
    {
        public int Id { get; set; }
        public string CaseNumber { get; set; }
    }
}
