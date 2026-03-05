using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatScore: BaseEntity
    {
        [Key]
        public int ScoreId { get; set; }

        [Required]
        public int AppId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Display(Name = "Score")]
        public double Score { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public PatScoreCategory? ScoreCategory { get; set; }
    }
}
