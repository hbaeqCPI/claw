using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkConflictSearchResultViewModel
    {
        public int ConflictId { get; set; }
        public int TmkId { get; set; }

        [Display(Name = "Case Number")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Other Party")]
        public string? OtherParty { get; set; }

        [Display(Name = "Other Party Mark")]
        public string? OtherPartyMark { get; set; }
    }
}
