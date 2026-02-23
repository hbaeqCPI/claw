using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class IDSCopySearchViewModel
    {
        public string? CaseNumber { get; set; }
        [Display(Name="Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Keyword")]
        public string? Keyword { get; set; }

        [Display(Name = "App Inventor")]
        public string? Inventor { get; set; }

        [Display(Name = "Art Unit")]
        public string? ArtUnit { get; set; }

        [Display(Name = "Search Text")]
        public string? SearchText { get; set; }
    }

    public class IDSRelatedCaseExportViewModel 
    {
        [Display(Name = "Relationship")]
        public string? Relationship { get; set; }

        [Display(Name = "LabelCaseNumber")]
        public string? RelatedCaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? RelatedCountry { get; set; }

        [Display(Name = "Sub Case")]
        public string? RelatedSubCase { get; set; }

        [Display(Name = "Publication No.")]
        public string? RelatedPubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? RelatedPubDate { get; set; }

        [Display(Name = "Patent No.")]
        public string? RelatedPatNumber { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? RelatedIssDate { get; set; }

        [Display(Name = "Applicant Name")]
        public string? RelatedFirstNamedInventor { get; set; }
        
        [Display(Name = "Reference Source")]
        public string? ReferenceSrc { get; set; }

        [Display(Name = "Reference Date")]
        public DateTime? ReferenceDate { get; set; }

        [Display(Name = "IDS File Date")]
        public DateTime? RelatedDateFiled { get; set; }

        [Display(Name = "Applicable?")]
        public bool ActiveSwitch { get; set; }

        [Display(Name = "Has Translation?")]
        public bool HasTranslation { get; set; }

        [Display(Name = "Relied Upon")]
        public bool ReliedUpon { get; set; }

        [Display(Name = "Kind Code")]
        public string? KindCode { get; set; }

        [Display(Name = "Reference Pages")]
        public string? RefPages { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

    }

}
