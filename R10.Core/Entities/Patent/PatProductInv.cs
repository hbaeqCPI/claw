using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatProductInv : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int InvId { get; set; }

        public int OrderOfEntry { get; set; }

        #region German Remuneration Module

        [Display(Name = "Invention Value")]
        public double? InventionValue { get; set; } = 0;
        [Display(Name = "License Factor")]
        public double? LicenseFactor { get; set; } = 0;
        [Display(Name = "Use Begin")]
        public DateTime? StartDate { get; set; }
        [Display(Name = "Use End")]
        public DateTime? EndDate { get; set; }
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        #endregion

        public Invention? Invention { get; set; }
        public Product? Product { get; set; }
}
}
