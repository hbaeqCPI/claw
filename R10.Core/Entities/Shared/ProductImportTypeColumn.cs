using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    
    public class ProductImportTypeColumn
    {
        public string?  ColumnName { get; set; }
        public bool IsNullable { get; set; }
        public string?  DataType { get; set; }
        public Int16 CharMaxLength { get; set; }
        public string?  Description { get; set; }
    }
}
