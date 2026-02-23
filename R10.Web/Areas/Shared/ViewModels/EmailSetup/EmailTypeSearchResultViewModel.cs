using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class EmailTypeSearchResultViewModel
    {
        public int EmailTypeId { get; set; }

        [Display(Name = "Name")]
        public string? Name { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Notification Type")]
        public string? ContentTypeDescription { get; set; }

        [Display(Name = "Template")]
        public string? Template { get; set; }
    }
}
