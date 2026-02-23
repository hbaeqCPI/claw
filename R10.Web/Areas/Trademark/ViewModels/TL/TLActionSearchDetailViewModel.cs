using R10.Core.Entities.Trademark;
using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels
{ 
    public class TLActionSearchDetailViewModel 
    {
        public int TmkId { get; set; }
        public int TLTmkId { get; set; }

        public string CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Country Name")]
        public string CountryName { get; set; }

        [Display(Name = "Sub Case")]
        public string SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string CaseType { get; set; }

        [Display(Name = "Mark Type")]
        public string MarkType { get; set; }

        [Display(Name = "Trademark Name")]
        public string TrademarkName { get; set; }

        [Display(Name = "Client")]
        public int? ClientID { get; set; }

        [Display(Name = "Status")]
        public string TrademarkStatus { get; set; }

        [Display(Name = "Application No.")]
        public string AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }
        
        [Display(Name = "Registration No.")]
        public string RegNumber { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime? RegDate { get; set; }

        [Display(Name = "T.O. Application No.")]
        public string TLAppNo { get; set; }

        [Display(Name = "T.O. Filing Date")]
        public DateTime? TLFilDate { get; set; }

    }

}
