using System.ComponentModel.DataAnnotations;


namespace R10.Core.Entities.Patent
{
    public class PatScoreCategory : BaseEntity
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        public List<PatScore>? PatScores { get; set; }
    }
}
