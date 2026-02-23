using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class PatEPODocumentExportViewModel
    {
        [Display(Name = "Dispatch Date")]
        public DateTime? DispatchDate { get; set; }

        [Display(Name = "Document Code")]
        public string? DocumentCode { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "Application Number")]
        public string? ApplicationNumber { get; set; }

        [Display(Name = "User Reference")]
        public string? UserReference { get; set; }

        [Display(Name = "Recipient Name")]
        public string? RecipientName { get; set; }

        [Display(Name = "Document Name")]
        public string? DocumentName { get; set; }

        [Display(Name = "Download Date")]
        public DateTime? DateCreated { get; set; }

    }

    public class PatEPODocumentViewModel : PatEPODocumentExportViewModel
    {
        public string? DriveItemId { get; set; }
        public int DocId { get; set; }
        
        public string? CommunicationId { get; set; }
        
        public bool Handled { get; set; }

        public bool Read { get; set; }

        public string? SystemType { get; set; }

        public string? ScreenCode { get; set; }

        public int AppId { get; set; }

        public string? FileName { get; set; }
        public string? UserFileName { get; set; }

        public bool CustomCheck { get; set; }
    }
}
