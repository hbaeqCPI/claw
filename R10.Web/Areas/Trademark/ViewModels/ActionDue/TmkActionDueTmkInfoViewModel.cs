using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkActionDueTmkInfoViewModel
    {
        public int TmkId { get; set; }
        public string?  CaseNumber { get; set; }
        public string?  Country { get; set; }
        public string?  CountryName { get; set; }
        public string?  SubCase { get; set; }
        public string?  CaseType { get; set; }
        public string?  TrademarkStatus { get; set; }
        public string?  AppNumber { get; set; }
        public DateTime? FilDate { get; set; }
        public string? TrademarkName { get; set; }
    }
}
