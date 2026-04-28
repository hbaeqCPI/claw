using System.ComponentModel.DataAnnotations;


namespace LawPortal.Core.Entities.Documents
{
    public class DocFolder : BaseEntity
    {
        [Key]
        public int FolderId { get; set;}

        public string? SystemType { get; set; }
        public string? ScreenCode { get; set; }

        public string? DataKey { get; set; }
        public int DataKeyValue { get; set; }

        [Display(Name = "Author")]
        public string? Author { get; set; }

        [Required]
        [Display(Name = "Folder Name")]
        public string? FolderName { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public int ParentFolderId { get; set; }

        [Display(Name = "Private?")]
        public bool IsPrivate { get; set; } = false;
        public bool IsFixed { get; set; } = false;

        /// <summary>
        /// Root container
        /// For grouping folders within same family 
        /// (country apps under the same invention could have the same root container)
        /// iManage root container is a Workspace which can only contain folders, not documents
        /// </summary>
        public string? StorageRootContainerId { get; set; }

        /// <summary>
        /// Folder inside root container
        /// </summary>
        public string? StorageDefaultFolderId { get; set; }

        public List<DocDocument>? DocDocuments { get; set; }
    }

    public class DocFolderHeader 
    {
        public string? SystemType { get; set; }
        public string? ScreenCode { get; set; }

        public int ParentId { get; set; }

    }
}
