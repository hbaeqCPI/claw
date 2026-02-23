using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCEQuestionGeneralViewModel : PatCEQuestionGeneralDetail
    {
        public string QuestionGuide { get; set; }
        public string AnswerType { get; set; }

        [NotMapped]
        public string? QuestionIds { get; set; }
    }
}
