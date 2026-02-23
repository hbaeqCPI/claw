using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSRecommendation : BaseEntity
    {
        [Key]
        public int RecommendationId { get; set; }
        
        [Required, StringLength(50)]
        public string? Recommendation { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Generate Invention?")]
        public bool IsGenInvention { get; set; }

        [Display(Name = "Reset Status?")]
        public bool IsResetStatus { get; set; }

        [Display(Name = "Disclosure Status")]
        public string? DisclosureStatus { get; set; }

        public string? ReviewerEntityFilter { get; set; }

        [Display(Name = "Confidential Data")]
        public bool IsTradeSecret { get; set; }

        [NotMapped]
        public int[]? ReviewerEntityFilterList { get; set; }
        
        [NotMapped]
        public string? ReviewerEntityFilterStr { get; set; }
    }

    public static class DMSCombineRecommendationOption
    {
        public const string With = "With";
        public const string Into = "Into";
    }
}
