using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Documents
{
    public class DocVerification : DocVerificationDetail
    {
        public DocDocument? DocDocument { get; set; }

        [NotMapped]        
        [Display(Name = "Base Date")]
        public DateTime? BaseDate { get; set; }
    }

    public class DocVerificationDetail : BaseEntity
    {
        [Key]
        public int VerifyId { get; set; }

        public int? DocId { get; set; }      

        public int? ActionTypeID { get; set; }

        public int? ActId { get; set; }

        public string? RandomGuid { get; set; }

        public DocVerificationWorkflowStatus WorkflowStatus { get; set; }
    }

    public enum DocVerificationWorkflowStatus
    {
        DoNotProcess,
        ToBeProcess,
        Processed
    }
}
