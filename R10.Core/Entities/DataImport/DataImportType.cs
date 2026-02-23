using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    
    public class DataImportType:BaseEntity
    {
        [Key]
        public int DataTypeId { get; set; }

        [Display(Name = "Type")]
        public string? DataType { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "System Type")]
        public string? SystemType { get; set; }

        [Display(Name = "Group")]
        public string? DataGroup { get; set; }

        [Display(Name = "Display Order")]
        public byte DisplayOrder { get; set; }

        [Display(Name = "Key")]
        public string? KeyColumns { get; set; }

        public string? TableType { get; set; }
        public string? TableLoader { get; set; }
        public string? OptionsView { get; set; }

        public bool InUse { get; set; }

        public bool IsUpdate { get; set; }

        public List<DataImportHistory>? Imports { get; set; }
    }
}
