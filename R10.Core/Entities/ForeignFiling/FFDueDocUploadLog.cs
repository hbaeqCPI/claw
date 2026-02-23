using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.Entities.Documents;

namespace R10.Core.Entities.ForeignFiling
{
    public class FFDueDocUploadLog
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        public int DueDocId { get; set; }

        public int DocFileId { get; set; } //uploaded doc file id

        [Display(Name = "Filename")]
        [StringLength(255)]
        public string? DocFileName { get; set; } //uploaded doc user filename

        [Display(Name = "Uploaded")]
        public bool? IsUploaded { get; set; }

        [StringLength(255)]
        [Display(Name = "Reason For Change")]
        public string? ReasonForChange { get; set; } //reason for manually updating RMSDueDocs.IsUploaded

        [Display(Name = "Date")]
        public DateTime DateCreated { get; set; }

        [StringLength(20)]
        [Display(Name = "User")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[] tStamp { get; set; }

        public FFDueDoc? FFDueDoc { get; set; }
        public DocFile? DocFile { get; set; }
    }
}
