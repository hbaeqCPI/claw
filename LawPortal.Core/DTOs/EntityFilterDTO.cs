using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LawPortal.Core.DTOs
{
    public class EntityFilterDTO
    {
        [Key]
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
