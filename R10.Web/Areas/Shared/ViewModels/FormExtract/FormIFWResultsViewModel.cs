using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class FormIFWResultsViewModel
    {
        public int IFWId { get; set; }
        
        public int PLAppId { get; set; }

        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Status")]
        public string? ApplicationStatus { get; set; }

        [Display(Name = "Mail Date")]
        public DateTime MailRoomDate { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNo { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime FilDate { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatNo { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime IssDate { get; set; }


        [Display(Name = "Description")]
        public string? Description { get; set; }

        public string? FormType { get; set; }

        [Display(Name = "Form Type")]
        public string? FormName { get; set; }

        public DateTime? AIParseDate { get; set; }
        public DateTime? AIActionGenDate { get; set; }

        [Display(Name = "Include?")]
        public bool AIInclude { get; set; }

        [NotMapped]
        [Display(Name = "Extracted?")]
        public bool WasParsed { get; set; }

        [NotMapped]
        [Display(Name = "Act Gen?")]
        public bool WasActGen { get; set; }

    }

    public class FormIFWDetailViewModel : FormIFWResultsViewModel
    {

        [Display(Name = "Title")]
        public string? AppTitle { get; set; }
        public string? ClientName { get; set; }
        public string? Attorney1 { get; set; }
        public string? Attorney2 { get; set; }
        public string? Attorney3 { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNo { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime PubDate { get; set; }


        public int DocTypeId { get; set; }
        public string? ScanPages { get; set; }

        public int NoPages { get; set; }
        public int PageStart { get; set; }
        public string? FileName { get; set; }   
    }
}
