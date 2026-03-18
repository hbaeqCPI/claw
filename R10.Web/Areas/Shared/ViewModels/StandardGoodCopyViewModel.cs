using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class StandardGoodCopyViewModel
    {
        public int ClassId { get; set; }

        [Display(Name = "Class")]
        [StringLength(3)]
        [Required]
        public string Class { get; set; }

        [Display(Name = "Class Type")]
        [StringLength(40)]
        [Required]
        public string ClassType { get; set; }
    }
}
