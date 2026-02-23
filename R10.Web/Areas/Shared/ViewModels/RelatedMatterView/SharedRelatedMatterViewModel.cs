using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace R10.Web.Areas.Shared.ViewModels
{
    public class SharedRelatedMatterViewModel : BaseEntity
    {
        public int KeyId { get; set; }
        public int ParentId { get; set; }
        public int? MatId { get; set; }

        [Display(Name = "Case Number")]
        public string? RelatedCaseNumber { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Matter Type")]
        public string? MatterType { get; set; }

        [Display(Name = "Matter Title")]
        public string? MatterTitle { get; set; }

        [Display(Name = "Matter Status")]
        public string? MatterStatus { get; set; }

        [Display(Name = "Effective Open Date")]
        public DateTime? EffectiveOpenDate { get; set; }

        [Display(Name = "Termination/End Date")]
        public DateTime? TerminationEndDate { get; set; }

        [Display(Name = "Project Name")]
        public string? ProjectName { get; set; }

        [Display(Name = "Program")]
        public string? Program { get; set; }

    }
}
