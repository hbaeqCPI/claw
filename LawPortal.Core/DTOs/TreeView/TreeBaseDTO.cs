using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Core.DTOs
{
    public class TreeBaseDTO
    {
        public string? id { get; set; }
        public string? text { get; set; }
        public bool hasChildren { get; set; }
        public bool expanded { get; set; }
    }
}
