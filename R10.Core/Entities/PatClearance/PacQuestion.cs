using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.PatClearance
{
    public class PacQuestion : PacQuestionDetail
    {
        public PacClearance? Clearance { get; set; }        
        public PacQuestionGuide? PacQuestionGuide { get; set; }
    }

    public class PacQuestionDetail : BaseEntity
    {
        [Key]
        public int PacQuestionId { get; set; }
        public int PacId { get; set; }
        public int QuestionId { get; set; }
        public string? Answer { get; set; }
    }
}