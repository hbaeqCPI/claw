using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ProductViewModel : BaseEntity
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public int ProductId { get; set; }

        [Display(Name = "Product")]
        public string? ProductName { get; set; }

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
    }


}
