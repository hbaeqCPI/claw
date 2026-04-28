using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LawPortal.Core.DTOs
{
    public class WebLinksParsedInfoDTO
    {
        public string? Template { get;  set; }
        public string? Number { get; set; }
        public string? Year { get; set; }
        public string? CheckDigit { get; set; }
        public string? City { get; set; }
        public string? PriorityCountry { get; set; }
        public bool Success { get; set; }
    }
}


