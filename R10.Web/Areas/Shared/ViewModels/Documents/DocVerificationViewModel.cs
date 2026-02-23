using R10.Core.Entities.Documents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocVerificationViewModel : DocVerification
    {
        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }       
                
        [Display(Name = "Verified By")]
        public string? VerifiedBy { get; set; }
        [Display(Name = "Verified Date")]
        public DateTime? DateVerified { get; set; }

        //[NotMapped]
        //public bool CanDelete { get; set; } = true;

        [NotMapped]
        public string? DriveItemId { get; set; }
    }
}
