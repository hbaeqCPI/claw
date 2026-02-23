using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.DMS
{
    public class DMSQuestionGuideChild : BaseEntity
    {
        [Key]
        public int ChildId { get; set; }

        public int QuestionId { get; set; }

        [Required]
        [StringLength(510)]
        public string? Description { get; set; }
        public int OrderOfEntry { get; set; }

        //VALID OPTIONS: (using C# value type names)
        //string | DateTime | bool | double
        [Required]
        public string? CAnswerType { get; set; }

        [Display(Name = "Active?")]
        public bool CActiveSwitch { get; set; }

        [Display(Name = "Required On Submission?")]
        public bool CRequiredOnSubmission { get; set; }

        [Display(Name = "Add to future records?")]
        public bool CAddToFuture { get; set; }

        public string? CPlaceholder { get; set; }

        [Display(Name = "Follow-up Questions?")]
        public bool CFollowUp { get; set; }

        public DMSQuestionGuide? DMSQuestionGuide { get; set; }
        public List<DMSQuestion>? DMSQuestions { get; set; }
        public List<DMSQuestionGuideSub>? DMSQuestionGuideSubs { get; set; }

        [NotMapped]
        public bool CanEdit { get; set; }
    }
}
