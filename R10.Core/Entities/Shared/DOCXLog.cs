using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities
{
    public class DOCXLog
    {
        [Key]
        public int DOCXLogId { get; set; }
        [StringLength(1)]
        public string?  SystemType { get; set; }
        public int ScreenId { get; set; }
        public int DOCXId { get; set; }
        [StringLength(150)]
        public string?  DOCXFile { get; set; }
        public int LogDOCXId { get; set; }
        public int LogCatId { get; set; }
        [StringLength(12)]
        
        public string?  DataKey { get; set; }
        public int DataKeyValue { get; set; }

        [StringLength(20)]
        public string?  GenBy { get; set; }
        public DateTime GenDate { get; set; }

        public DateTime? MetaUpdate { get; set; } = DateTime.Now;           // Azure blob storage metadata update date
        //public List<DOCXLogDetail>? DOCXLogDetails { get; set; }
    }

    //public class DOCXLogDetail
    //{
    //    [Key]
    //    public int LogDtlId { get; set; }

    //    [ForeignKey("DOCXLogId")]
    //    public int DOCXLogId { get; set; }
    //    public int DataKeyValue { get; set; }
    //    public string? EntityType { get; set; }
    //    public int EntityId { get; set; }
    //    public int ContactId { get; set; }
    //    public int? LogEntityId { get; set; }
    //    public int? LogContactId { get; set; }
    //    public DOCXLog? DOCXLog { get; set; }
    //}
}
