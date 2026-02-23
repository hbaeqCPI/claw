using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class SubjectMatterViewModel : BaseEntity
    {
        [Key]
        public int KwdId { get; set; }

        [Required]
        public int ParentId { get; set; }

        [Required, StringLength(255)]
        public string? SubjectMatter { get; set; }

    }
}
