using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Clearance
{
    public class TmcQuestion : TmcQuestionDetail
    {
        public TmcClearance Clearance { get; set; }        
        public TmcQuestionGuide TmcQuestionGuide { get; set; }
    }

    public class TmcQuestionDetail : BaseEntity
    {
        [Key]
        public int TmcQuestionId { get; set; }
        public int TmcId { get; set; }
        public int QuestionId { get; set; }
        public string? Answer { get; set; }
    }
}