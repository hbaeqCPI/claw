using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocFixedFolderViewModel
    {
        public int FolderId { get; set; }

        [Display(Name = "Folder Name")]
        public string? FolderName { get; set; }

        [Display(Name = "Folder Description")]
        public string? FolderDesc { get; set; }

    }
}
