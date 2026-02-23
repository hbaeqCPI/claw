using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkRelatedTrademarkViewModel:BaseEntity
    {
        public int RelatedId { get; set; }
        public int TmkId { get; set; }
        public int RelatedTmkId { get; set; }

        [Required]
        public string RelatedCaseNumber { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Sub Case")]
        public string SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string CaseType { get; set; }

        [Display(Name = "Status")]
        public string TrademarkStatus { get; set; }

        [Display(Name = "Trademark Name")]
        public string TrademarkName { get; set; }

    }
}
