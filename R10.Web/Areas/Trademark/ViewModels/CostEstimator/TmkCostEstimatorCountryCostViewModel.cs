using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Trademark;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkCostEstimatorCountryCostViewModel : TmkCostEstimatorCountryCostDetail
    {
        [Display(Name = "Country Specific Question(s)")]
        public string QuestionGuide { get; set; }

        [Display(Name = "Answer")]
        public string AnswerType { get; set; }

        [Display(Name = "Stage")]
        public string? Stage { get; set; }

        [NotMapped]
        public string? QuestionIds { get; set; }

        [NotMapped]
        public List<string>? SelectionItems { get; set; }

        [NotMapped]
        public bool FollowUp { get; set; }

        [NotMapped]
        public bool IsVisible { get; set; }

        [NotMapped]
        public string? FollowUpSelection { get; set; }
    }
}
