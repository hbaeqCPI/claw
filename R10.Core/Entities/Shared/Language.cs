using System.ComponentModel.DataAnnotations;


namespace R10.Core.Entities
{

    public partial class Language : BaseEntity
    {
        public int KeyID { get; set; }

        [Key]
        [StringLength(10)]
        [Required]
        [Display(Name = "Language")]
        public string LanguageName { get; set; }

        [StringLength(10)]
        [Required]
        [Display(Name = "Language Culture")]
        public string? LanguageCulture { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
