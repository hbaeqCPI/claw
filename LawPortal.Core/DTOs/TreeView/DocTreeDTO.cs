using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;

namespace LawPortal.Core.DTOs
{
    public class DocTreeDTO : TreeBaseDTO
    {
        public bool isReadOnly { get; set; }

        public string? detailAction { get; set; }

        public string? iconClass { get; set; }

        //[NotMapped]
        //public string? imageUrl { get; set; }
        
    }
}
