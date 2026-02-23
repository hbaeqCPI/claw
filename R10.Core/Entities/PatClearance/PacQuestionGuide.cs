using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.PatClearance
{
    public class PacQuestionGuide : BaseEntity
    {
        [Key]
        public int QuestionId { get; set; }
        public int GroupId { get; set; }

        [Required]
        public string Question { get; set; }
        public int OrderOfEntry { get; set; }

        //VALID OPTIONS: (using C# value type names)
        //string | DateTime | bool | double
        [Required]
        public string AnswerType { get; set; }

        [Display(Name = "Active?")]
        public bool ActiveSwitch { get; set; }

        [Display(Name = "Required On Submission?")]
        public bool RequiredOnSubmission { get; set; }

        [NotMapped]
        [Display(Name = "Add to existing records?")]
        public bool AddCurrent { get; set; }

        [Display(Name = "Add to future records?")]
        public bool AddToFuture { get; set; }
        public string? Placeholder { get; set; }

        public PacQuestionGroup? PacQuestionGroup { get; set; }

        public List<PacQuestion>? PacQuestions { get; set; }

        public List<PacQuestionGuideChild>? PacQuestionGuideChildren { get; set; }


        [NotMapped]
        public byte[]? ParentTStamp { get; set; }

        [NotMapped]
        public bool CanEdit { get; set; }
    }
}