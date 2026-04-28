using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Core.Entities
{
    public class ErrorMapping
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? Message { get; set; }
    }
}
