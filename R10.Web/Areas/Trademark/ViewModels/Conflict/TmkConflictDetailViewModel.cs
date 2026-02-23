using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkConflictDetailViewModel : TmkConflictDetail
    {
        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Trademark Status")]
        public string? TrademarkStatus { get; set; }

        [Display(Name = "Trademark Name")]
        public string? TrademarkName{ get; set; }
        
        public string? RespOffice { get; set; }

        public string? AgentCode { get; set; }
        public string? AgentName { get; set; }
    }
}
