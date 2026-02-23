using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    public class QELog
    {
        [Key]
        public int LogID { get; set; }
        [StringLength(1)]
        public string?  SystemType { get; set; }
        public int ScreenId { get; set; }
        public int QESetupId { get; set; }
        [StringLength(50)]
        public string?  TemplateName { get; set; }
        [StringLength(25)]
        public string?  DataKey { get; set; }
        public int DataKeyValue { get; set; }
        public string?  Body { get; set; }
        public string?  Subject { get; set; }
        public string?  From { get; set; }
        public string?  To { get; set; }
        public string?  ReplyTo { get; set; }
        public string?  Cc { get; set; }
        public string?  Bcc { get; set; }
        public string?  Attachments { get; set; }
        public string? RoleLink { get; set; }
        public int DataSourceID { get; set; }
        public int ImageParentId { get; set; }

        [StringLength(20)]
        public string?  GenBy { get; set; }
        public DateTime GenDate { get; set; }

        public string?  QEFile { get; set; }      // quick email .msg file name
        public DateTime? MetaUpdate { get; set; } = DateTime.Now;           // Azure blob storage metadata update date

        [NotMapped]
        public string?  SystemTypeName { get; set; }
        [NotMapped]
        public string? ScreenCode { get; set; }

        public string? ItemId { get; set; }
        public string? SharePointDocLibrary { get; set; }
        public string? SharePointDocLibraryFolder { get; set; }
        public string? SharePointRecKey { get; set; }
        
    }
}
