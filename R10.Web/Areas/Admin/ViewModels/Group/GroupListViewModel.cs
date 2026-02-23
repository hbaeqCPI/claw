using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Admin.ViewModels
{
    public class GroupListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Enabled")]
        public bool IsEnabled { get; set; }
    }
}
