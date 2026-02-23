using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.DMS
{
    public class DMSQuestionGuideSub : BaseEntity
    {
        [Key]
        public int SubId { get; set; }

        public int ChildId { get; set; }

        [Required]
        [StringLength(510)]
        public string? Description { get; set; }
        public int OrderOfEntry { get; set; }

        [Required]
        public string? SAnswerType { get; set; }

        [Display(Name = "Active?")]
        public bool SActiveSwitch { get; set; }

        [Display(Name = "Required On Submission?")]
        public bool SRequiredOnSubmission { get; set; }

        [Display(Name = "Add to future records?")]
        public bool SAddToFuture { get; set; }

        public string? SPlaceholder { get; set; }

        public DMSQuestionGuideChild? DMSQuestionGuideChild { get; set; }
        public List<DMSQuestionGuideSubDtl>? DMSQuestionGuideSubDtls { get; set; }
        public List<DMSQuestion>? DMSQuestions { get; set; }

        [NotMapped]
        public bool CanEdit { get; set; }
    }
}
