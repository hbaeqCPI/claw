using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSRecommendationHistory
    {
        [Key]
        public int LogID { get; set; }

        [Required]
        public int DMSId { get; set; }
        
        [Display(Name = "Recommendation")]
        public string? Recommendation { get; set; }

        public string? Combined { get; set; }

        [StringLength(20)]
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }
        
        [Display(Name = "Date Changed")]
        public DateTime? DateChanged { get; set; }
        
        public Disclosure? Disclosure { get; set; }
    }
}
