using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class GlobalUpdateFields : BaseEntity
    {
        [Key]
        public int FieldId { get; set; }
        public string?  SystemType { get; set; }
        public string?  UpdateField { get; set; }
        public string?  FieldDescription { get; set; }
        public int EntryOrder { get; set; }
    }
}
