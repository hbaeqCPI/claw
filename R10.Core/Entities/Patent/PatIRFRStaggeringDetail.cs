using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatIRFRStaggeringDetail : BaseEntity
    {
        [Key]
        public int DetailId { get; set; }
        public int StaggeringId { get; set; }

        [StringLength(20)]
        [Display(Name = "Stage")]
        [Required]
        public string? Stage { get; set; }

        [Display(Name = "Amount From(DM)")]
        [Required]
        public double? AmountFrom { get; set; }
        [Display(Name = "Amount To(DM)")]
        public double? AmountTo { get; set; }
        [Range(0,1,ErrorMessage ="Please enter a correct reduction")]
        [Display(Name = "Reduction")]
        [Required]
        public double Reduction { get; set; }
        public int OrderOfEntry { get; set; }


        public virtual PatIRFRStaggering? PatIRStaggering { get; set; }
    }
}
