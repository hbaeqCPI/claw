using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Documents
{
    public class DocFixedFolder : BaseEntity
    {
        [Key]
        public int FolderId { get; set; }

        public int ParentFolderId { get; set; }

        public string? SystemType { get; set; }
        public string? ScreenCode { get; set; }
        public string? DataKey { get; set; }

        [Required]
        public string? FolderName { get; set; }

        public string? FolderDesc { get; set; }

        public string? PhysicalFolder { get; set; }
        public string? SqlView { get; set; }
        public string? SqlViewDetail { get; set; }

        public string? DetailFolderAction { get; set; }
        public string? DetailDocumentAction { get; set; }

        public int EntryOrder { get; set; }
        public bool InUse { get; set; }
    }
}
