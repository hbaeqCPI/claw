using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSQuestionGuide : BaseEntity
    {
        [Key]
        public int QuestionId { get; set; }
        public int GroupId { get; set; }

        [Required]
        public string? Question { get; set; }

        public int OrderOfEntry { get; set; }

        //VALID OPTIONS: (using C# value type names)
        //string | DateTime | bool | double
        [Required]
        public string? AnswerType { get; set; }

        [Display(Name = "In Use?")]
        public bool ActiveSwitch { get; set; }

        [Display(Name = "Required On Submission?")]
        public bool RequiredOnSubmission { get; set; }
        
        [Display(Name = "Add to future records?")]
        public bool AddToFuture { get; set; }

        public string? Placeholder { get; set; }

        [Display(Name = "Follow-up Questions?")]
        public bool FollowUp { get; set; }



        public DMSQuestionGroup? DMSQuestionGroup { get; set; }
        public List<DMSQuestion>? DMSQuestions { get; set; }
        public List<DMSQuestionGuideChild>? DMSQuestionGuideChildren { get; set; }



        [NotMapped]
        public byte[]? ParentTStamp { get; set; }        

        [NotMapped]
        public bool CanEdit { get; set; }
        
        [NotMapped]
        public bool CanAddCurrent { get; set; }
    }
}
