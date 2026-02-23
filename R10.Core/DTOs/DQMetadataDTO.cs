using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.DTOs
{
    [Keyless]
    public class DQMetadataDTO
    {
        public string? TableName { get; set; }
        public string? TableAlias { get; set; }
        public string? ObjectType { get; set; }
        public string? ColumnName { get; set; }
        public string? DataType { get; set; }
        public short DataSize { get; set; }
        public bool Visible { get; set; }
    }
}
