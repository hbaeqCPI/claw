using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.Entities
{
    public class DeployPassword : BaseEntity
    {
        [Key]
        public int DeployPasswordId { get; set; }

        [Required]
        [Display(Name = "Year")]
        public int Year { get; set; }

        [Required]
        [StringLength(2)]
        [Display(Name = "Quarter")]
        public string Quarter { get; set; } = "";

        [StringLength(30)]
        [Display(Name = "Patent Password")]
        public string PatentPassword { get; set; } = "";

        [StringLength(30)]
        [Display(Name = "Trademark Password")]
        public string TrademarkPassword { get; set; } = "";
    }
}
