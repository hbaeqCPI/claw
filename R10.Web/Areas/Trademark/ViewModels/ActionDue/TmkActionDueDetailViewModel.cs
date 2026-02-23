using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkActionDueDetailViewModel : TmkActionDueDetail
    {
        [Display(Name = "Country Name")]
        public string?  CountryName { get; set; }

        [Display(Name = "Case Type")]
        public string?  CaseType { get; set; }

        [Display(Name = "Status")]
        public string?  TrademarkStatus { get; set; }

        [Display(Name = "Application No.")]
        public string?  AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Responsible Attorney")]
        public string?  ResponsibleCode { get; set; }

        [Display(Name = "Attorney Name")]
        public string?  ResponsibleName { get; set; }

        public bool CanModifyAttorney { get; set; } = true;

        public string?  RespOffice { get; set; }

        [Display(Name = "Trademark Name")]
        public string?  TrademarkName { get; set; }

        public string?  CopyOptions { get; set; }

        public bool CanVerifyAction { get; set; } = false;
        public bool ShowVerifyAction { get; set; } = false;
        public int? ActionTypeID { get; set; }
        public bool ShowCheckDocket { get; set; } = false;
    }
}
