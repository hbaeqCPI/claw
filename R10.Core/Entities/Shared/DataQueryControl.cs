using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class DataQueryControl : BaseEntity
    {
        [Key]
        [StringLength(100)]
        public string?  ObjectName { get; set; }

        [Required]
        [StringLength(100)]
        public string?  SqlObjectName { get; set; }

        [StringLength(100)]
        public string?  ObjectDesc { get; set; }

        public bool IsInclude { get; set; }

        [StringLength(1)]
        public string?  SystemType { get; set; }

        [StringLength(20)]
        public string?  EntityTrigger { get; set; }

        [StringLength(500)]
        public string?  FilterExpr { get; set; }

        public short FilterOrder { get; set; }
    }
}
