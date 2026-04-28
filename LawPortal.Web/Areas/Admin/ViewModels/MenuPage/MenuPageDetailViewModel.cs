using System.ComponentModel.DataAnnotations;

namespace LawPortal.Web.Areas.Admin.ViewModels
{
    public class MenuPageDetailViewModel
    {
        public int Id { get; set; }

        [StringLength(256)]
        [Required]
        public string Name { get; set; }

        [StringLength(256)]
        [Required]
        public string Controller { get; set; }

        [StringLength(256)]
        [Required]
        public string Action { get; set; }

        [StringLength(256)]
        [Display(Name = "Route Options")]
        public string? RouteOptions { get; set; }

        [StringLength(450)]
        public string? Policy { get; set; }
    }
}
