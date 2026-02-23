using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSRating: BaseEntity
    {
        public int RatingId { get; set; }
         
        [Required]
        [Key]
        [StringLength(50)]
        public string Rating { get; set; }

        [Display(Name = "Rating Value")]
        public double RatingValue { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }


        [Display(Name = "Create Invention?")]
        public bool AddInvention { get; set; }

        public List<DMSReview>? Reviews { get; set; }

    }
}
