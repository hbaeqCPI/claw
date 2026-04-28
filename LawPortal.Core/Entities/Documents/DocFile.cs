using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Documents
{
    public class DocFile : BaseEntity
    {
        [Key]
        public int FileId { get; set; }

        public string? FileExt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? DocFileName { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? ThumbFileName { get; set; }
        public string? UserFileName { get; set; }

        public int FileSize{ get; set; }

        public bool IsImage { get; set; }
        public string? DriveItemId { get; set; }

        public DocIcon? DocIcon { get; set; }
        public DocDocument? DocDocument{ get; set; }
        public bool? ForSignature { get; set; }
        public bool? SignedDoc { get; set; }
        public DocFileSignature? DocFileSignature { get; set; }

    }
}
