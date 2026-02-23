using R10.Core.Entities.DMS;
using R10.Core.Entities.ForeignFiling;
using R10.Core.Entities.RMS;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Documents
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

        public List<RMSDueDocUploadLog>? RMSDueDocsUploadLogs { get; set; }
        public List<FFDueDocUploadLog>? FFDueDocsUploadLogs { get; set; }

        public DMSFaqDoc? DMSFaqDoc { get; set; }
    }
}
