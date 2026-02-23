using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Admin.ViewModels
{
    public class MailDataMapListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Map Name")]
        public string? Name { get; set; }

        [Display(Name = "Field Name")]
        public int AttributeId { get; set; }

        [Display(Name = "Field Name")]
        public string? AttributeName { get; set; }
    }
}
