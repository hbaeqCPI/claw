using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Admin.ViewModels
{
    public class MailDataMapDetailViewModel : BaseEntity
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Map Name")]
        public string? Name { get; set; }

        [Required]
        [Display(Name = "Field Name")]
        public int AttributeId { get; set; }

        [Display(Name = "Field Name")]
        public string? AttributeName { get; set; }
    }
}
