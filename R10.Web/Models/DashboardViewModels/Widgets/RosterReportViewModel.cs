using R10.Core.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class RosterReportDrillDownViewModel : RosterReportDrillDownExportViewModel
    {        
        public int Id { get; set; }
    }

    public class RosterReportDrillDownExportViewModel : RosterReportExportViewModel
    {
        [Display(Name = "System")]
        public string? System { get; set; }

        [Display(Name = "LabelCaseNumber")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }        
    }

    public class RosterReportExportViewModel
    {
        [Display(Name = "LabelClient")]
        public string? ClientCode { get; set; }

        [Display(Name = "LabelClientName")]
        public string? ClientName { get; set; }

        [Display(Name = "Attorney 1 Code")]
        public string? Attorney1Codes { get; set; }

        [Display(Name = "Attorney 1 Name")]
        public string? Attorney1Names { get; set; }

        [Display(Name = "Attorney 2 Code")]
        public string? Attorney2Codes { get; set; }

        [Display(Name = "Attorney 2 Name")]
        public string? Attorney2Names { get; set; }

        [Display(Name = "Attorney 3 Code")]
        public string? Attorney3Codes { get; set; }

        [Display(Name = "Attorney 3 Name")]
        public string? Attorney3Names { get; set; }

        [Display(Name = "Attorney 4 Code")]
        public string? Attorney4Codes { get; set; }

        [Display(Name = "Attorney 4 Name")]
        public string? Attorney4Names { get; set; }

        [Display(Name = "Attorney 5 Code")]
        public string? Attorney5Codes { get; set; }

        [Display(Name = "Attorney 5 Name")]
        public string? Attorney5Names { get; set; }
    }
}
