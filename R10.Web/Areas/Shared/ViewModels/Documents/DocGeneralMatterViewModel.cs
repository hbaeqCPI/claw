using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocGeneralMatterViewModel
    {
        public int MatId { get; set; }

        public string? CaseNumber { get; set; }        

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Matter Type")]
        public string? MatterType { get; set; }

        public string? ClientName { get; set; }       

        public string? AgentName { get; set; }  
        

        [Display(Name = "Title")]
        public string? MatterTitle { get; set; }

        [Display(Name = "Status")]
        public string? MatterStatus { get; set; }

        [Display(Name = "Status Date")]
        public DateTime? MatterStatusDate { get; set; }
        

        [Display(Name = "Effective Open Date")]
        public DateTime? EffectiveOpenDate { get; set; }        

        [Display(Name = "Termination/End Date")]
        public DateTime? TerminationEndDate { get; set; }

        [Display(Name = "Result/Royalty Description")]
        public string? ResultRoyalty { get; set; }


        [Display(Name = "Agreement")]
        public string? AgreementType { get; set; }

        [Display(Name = "Extent")]
        public string? Extent { get; set; }

        [Display(Name = "Court")]
        public string? Court { get; set; }

        [Display(Name = "Court Docket No.")]
        public string? CourtDocketNumber { get; set; }

        [Display(Name = "Judge/Magistrate")]
        public string? CourtJudgeMagistrate { get; set; }


    }
}
