using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocFolderViewModel : BaseEntity
    {
        public int FolderId { get; set; }

        [Display(Name = "Author")]
        public string? Author { get; set; }

        [Required]
        [Display(Name = "Folder Name")]
        public string? FolderName { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Private?")]
        public bool IsPrivate { get; set; } = false;
    }
}
