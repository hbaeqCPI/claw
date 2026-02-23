using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.DMS
{
    public class DMSValuationMatrix : BaseEntity
    {
        [Key]
        public int ValId { get; set; }

        [Required, StringLength(255)]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "In Use?")]
        public bool ActiveSwitch { get; set; }

        [Display(Name = "Rating System")]
        [StringLength(50)]
        public string? RatingSystem { get; set; }

        public string? ReviewerEntityFilter { get; set; }


        public List<DMSValuationMatrixRate>? DMSValuationMatrixRates { get; set; }
        public List<DMSValuation>? Valuations { get; set; }

        [NotMapped]
        public bool CanEditRatingSystem { get; set; }

        [NotMapped]
        public string? CopyOptions { get; set; }        

        [NotMapped]
        public int[]? ReviewerEntityFilterList { get; set; }
        
        [NotMapped]
        public string? ReviewerEntityFilterStr { get; set; }
    }
}
