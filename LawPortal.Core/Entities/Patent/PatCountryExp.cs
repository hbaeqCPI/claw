using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Patent
{
    public class PatCountryExp
    {
        [Key]
        public int CExpId { get; set; }

        [Required]
        [StringLength(5)]
        public string? Country { get; set; }

        [Required]
        [StringLength(3)]
        public string? CaseType { get; set; }

        [Required(ErrorMessage = "The Type field is required.")]
        [StringLength(30)]
        [Display(Name = "Type")]
        public string? Type { get; set; }

        [Required(ErrorMessage = "The Based On field is required.")]
        [StringLength(12)]
        [Display(Name = "Based On")]
        public string? BasedOn { get; set; }

        [Display(Name = "Yr")]
        public int Yr { get; set; }

        [Display(Name = "Mo")]
        public int Mo { get; set; }

        [Display(Name = "Dy")]
        public int Dy { get; set; }

        [Required(ErrorMessage = "The Eff Based On field is required.")]
        [StringLength(15)]
        [Display(Name = "Eff Based On")]
        public string? EffBasedOn { get; set; }

        [Display(Name = "Eff Start Date")]
        public DateTime? EffStartDate { get; set; }

        [Display(Name = "Eff End Date")]
        public DateTime? EffEndDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        [NotMapped]
        public bool IsNewRecord { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }

        [NotMapped]
        public byte[]? ParentTStamp { get; set; }
    }
}
