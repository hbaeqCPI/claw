using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSQuestion : DMSQuestionDetail
    {
        public Disclosure? Disclosure { get; set; }

        public DMSQuestionGuide? DMSQuestionGuide { get; set; }

        public DMSQuestionGuideChild? DMSQuestionGuideChild { get; set; }

        public DMSQuestionGuideSub? DMSQuestionGuideSub { get; set; }
    }

    public class DMSQuestionDetail : BaseEntity
    {
        [Key]
        public int DMSQuestionId {get; set;}
        public int DMSId {get; set;}
        public int QuestionId { get; set; }
        public int? ChildId { get; set; }
        public int? SubId { get; set; }
        public string? Answer { get; set; }
    }
}
