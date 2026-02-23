using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class KeywordViewModel : BaseEntity
    {
        [Key]
        public int KwdId { get; set; }

        [Required]
        public int ParentId { get; set; }

        [Required, StringLength(50)]
        public string? Keyword { get; set; }

    }
}
