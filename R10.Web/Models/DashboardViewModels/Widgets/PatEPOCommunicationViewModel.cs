using System.ComponentModel.DataAnnotations;

namespace R10.Web.Models.DashboardViewModels
{
    public class PatEPOCommunicationExportViewModel
    {
        [Display(Name = "Communication Code")]
        public string? DocumentCode { get; set; }

        [Display(Name = "Communication Name")]
        public string? Title { get; set; }

        [Display(Name = "Dispatch Date")]
        public DateTime? DispatchDate { get; set; }

        [Display(Name = "Applicant Name")]
        public string? ApplicantName { get; set; }

        [Display(Name = "Application Number")]
        public string? ApplicationNumber { get; set; }

        [Display(Name = "User Reference")]
        public string? UserReference { get; set; }

        [Display(Name = "Recipient Name")]
        public string? RecipientName { get; set; }

        [Display(Name = "Download Date")]
        public DateTime? DateCreated { get; set; }

    }

    public class PatEPOCommunicationViewModel : PatEPOCommunicationExportViewModel
    {
        public string? CommunicationId { get; set; }

        public bool Handled { get; set; }

        public bool Read { get; set; }

        public int AppId { get; set; }

        public bool CustomCheck { get; set; }
    }

    public class PatEPOCommunicationWithApplicationExportViewModel : PatPortfolioByStatusExportViewModel
    {
        [Display(Name = "Communication Code")]
        public string? DocumentCode { get; set; }

        [Display(Name = "Communication Name")]
        public string? Title { get; set; }

        [Display(Name = "Dispatch Date")]
        public DateTime? DispatchDate { get; set; }

        [Display(Name = "Applicant Name")]
        public string? ApplicantName { get; set; }

        [Display(Name = "Application Number")]
        public string? ApplicationNumber { get; set; }

        [Display(Name = "User Reference")]
        public string? UserReference { get; set; }

        [Display(Name = "Recipient Name")]
        public string? RecipientName { get; set; }

        [Display(Name = "Download Date")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Handled")]
        public bool Handled { get; set; }
    }

    public class PatEPOCommunicationAttorneyExportViewModel : PatEPOCommunicationExportViewModel
    {
        [Display(Name = "Case Number")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Application Title")]
        public string? AppTitle { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatNumber { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

        [Display(Name = "Attorney 1 Code")]
        public string? Attorney1Code { get; set; }

        [Display(Name = "Attorney 1 Name")]
        public string? Attorney1Name { get; set; }

        [Display(Name = "Attorney 2 Code")]
        public string? Attorney2Code { get; set; }

        [Display(Name = "Attorney 2 Name")]
        public string? Attorney2Name { get; set; }

        [Display(Name = "Attorney 3 Code")]
        public string? Attorney3Code { get; set; }

        [Display(Name = "Attorney 3 Name")]
        public string? Attorney3Name { get; set; }

        [Display(Name = "Attorney 4 Code")]
        public string? Attorney4Code { get; set; }

        [Display(Name = "Attorney 4 Name")]
        public string? Attorney4Name { get; set; }

        [Display(Name = "Attorney 5 Code")]
        public string? Attorney5Code { get; set; }

        [Display(Name = "Attorney 5 Name")]
        public string? Attorney5Name { get; set; }

        [Display(Name = "Handled")]
        public bool Handled { get; set; }
    }
}
