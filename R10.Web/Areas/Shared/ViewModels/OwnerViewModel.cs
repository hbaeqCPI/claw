using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class OwnerViewModel : BaseEntity
    {

        [Key]
        public int Id { get; set; }

        public int ParentId { get; set; }

        [Required]
        public int OwnerID { get; set; }

        public int? OrderOfEntry { get; set; }

        public string? Remarks { get; set; }

        [Display(Name = "Percentage")]
        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        public double? Percentage { get; set; }

        [Display(Name ="Owner")]
        public string? OwnerCode { get; set; }

        [Display(Name = "Owner Name")]
        public string? OwnerName { get; set; }

        public bool ReadOnly { get; set; }

        [Display(Name = "Applicant?")]
        public bool IsApplicant { get; set; }
    }
}
