using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.DTOs
{
    [Keyless]
    public class DQMetaRelationsDTO
    {
        public string? FKeyName { get; set; }
        public string? ParentTable { get; set; }
        public string? ChildTable { get; set; }
        public string? ParentKey { get; set; }
        public string? ChildKey { get; set; }
    }
}
