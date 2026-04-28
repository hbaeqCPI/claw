using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using LawPortal.Core.Services.Shared;

namespace LawPortal.Core.DTOs
{
    public class WebLinksNumberInfoDTO
    {
        public string? SystemType { get; set; }
        public string? Country {get; set; }
        public string? CaseType {get; set; }
        public string? AppNumber {get; set; }
        public string? PubNumber {get; set; }
        public string? PatRegNumber {get; set; }
        public DateTime? FilDate {get; set; }
        public DateTime? PubDate {get; set; }
        public DateTime? IssRegDate {get; set; }
        public string? Number { get; set; }
        public DateTime? NumberDate { get; set; }
        public string? NumberType { get; set; }
    }
}


