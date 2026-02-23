using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Trademark;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkCEQuestionGeneralViewModel : TmkCEQuestionGeneralDetail
    {
        public string QuestionGuide { get; set; }
        public string AnswerType { get; set; }

        [NotMapped]
        public string? QuestionIds { get; set; }
    }
}
