using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class TmkAttyCaseLoadExportViewModel
    {
        [Display(Name = "Case Number")]
        public string CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Sub Case")]
        public string SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string CaseType { get; set; }

        [Display(Name = "Mark Type")]
        public string MarkType { get; set; }

        [Display(Name = "Trademark Name")]
        public string TrademarkName { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }       

        [Display(Name = "Application No.")]
        public string AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Publication No.")]
        public string PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [Display(Name = "Registration No.")]
        public string RegNumber { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime? RegDate { get; set; }

        [Display(Name = "Action Type")]
        public string ActionType { get; set; }

        [Display(Name = "Base Date")]
        public DateTime? BaseDate { get; set; }

        [Display(Name = "Action Due")]
        public string ActionDue { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Indicator")]
        public string Indicator { get; set; }

        [Display(Name = "Attorney1 Code")]
        public string Attorney1Code { get; set; }

        [Display(Name = "Attorney1 Name")]
        public string Attorney1Name { get; set; }

        [Display(Name = "Attorney2 Code")]
        public string Attorney2Code { get; set; }

        [Display(Name = "Attorney2 Name")]
        public string Attorney2Name { get; set; }

        [Display(Name = "Attorney3 Code")]
        public string Attorney3Code { get; set; }

        [Display(Name = "Attorney3 Name")]
        public string Attorney3Name { get; set; }

        [Display(Name = "Attorney4 Code")]
        public string Attorney4Code { get; set; }

        [Display(Name = "Attorney4 Name")]
        public string Attorney4Name { get; set; }

        [Display(Name = "Attorney5 Code")]
        public string Attorney5Code { get; set; }

        [Display(Name = "Attorney5 Name")]
        public string Attorney5Name { get; set; }
    }

    public class TmkAttyCaseLoadDrillDownViewModel : TmkAttyCaseLoadExportViewModel
    {
        public int TmkId { get; set; }
        public int ActId { get; set; }
    }
}
