using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class PatAttyCaseLoadExportViewModel
    {
        [Display(Name = "Case Number")]
        public string CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Sub Case")]
        public string SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string CaseType { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Application Title")]
        public string AppTitle { get; set; }

        [Display(Name = "Application No.")]
        public string AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Publication No.")]
        public string PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [Display(Name = "Patent No.")]
        public string PatNumber { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

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

        [Display(Name = "Attorney 1 Code")]
        public string Attorney1Code { get; set; }

        [Display(Name = "Attorney 1 Name")]
        public string Attorney1Name { get; set; }

        [Display(Name = "Attorney 2 Code")]
        public string Attorney2Code { get; set; }

        [Display(Name = "Attorney 2 Name")]
        public string Attorney2Name { get; set; }

        [Display(Name = "Attorney 3 Code")]
        public string Attorney3Code { get; set; }

        [Display(Name = "Attorney 3 Name")]
        public string Attorney3Name { get; set; }

        [Display(Name = "Attorney 4 Code")]
        public string Attorney4Code { get; set; }

        [Display(Name = "Attorney 4 Name")]
        public string Attorney4Name { get; set; }

        [Display(Name = "Attorney 5 Code")]
        public string Attorney5Code { get; set; }

        [Display(Name = "Attorney 5 Name")]
        public string Attorney5Name { get; set; }
    }

    public class PatAttyCaseLoadDrillDownViewModel : PatAttyCaseLoadExportViewModel
    {
        public int AppId { get; set; }
        public int ActId { get; set; }
        public int DdId { get; set; }
    }
}
