using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LawPortal.Core.Entities
{
    public class DeleteLog
    {
        [Key]
        public int DeleteLogId { get; set; }
                
        public string? SystemType { get; set; }

        public string? DataKey { get; set; }

        public int? DataKeyValue { get; set; }

        public string? DeletedBy { get; set; }
        public DateTime? DateDeleted { get; set; }

        public string? Record { get; set; }
    }
}
