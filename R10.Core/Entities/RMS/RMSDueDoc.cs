using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.RMS
{
    public class RMSDueDoc : RMSDueDocDetail
    {
        public TmkDueDate? TmkDueDate { get; set; }
        public RMSDoc? RMSDoc { get; set; }
        public List<RMSDueDocUploadLog>? RMSDueDocsUploadLogs { get; set; }
    }

    public class RMSDueDocDetail : BaseEntity
    {
        [Key]
        public int DueDocId { get; set; }

        [Required]
        public int DDId { get; set; }

        [Required]
        public int DocId { get; set; }

        [Display(Name = "Required")]
        public bool IsRequired { get; set; }

        [Display(Name = "Uploaded")]
        public bool IsUploaded { get; set; } //requires reason for change in log table if IsUploaded is manually edited

        public DateTime? DateUploaded { get; set; } //null if IsUploaded is manually edited
    }
}
