using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.DTOs
{
    public class PatStatSearchCitationOutput
    {
        public int AppId { get; set; }
        public int Count { get; set; }
        public string? PatNumber { get; set; }
        public string? Country { get; set; }
    }

    public class PatStatSearchCitationDetail
    {
        public string? PatNumber { get; set; }
        public string? Country { get; set; }
        public DateTime? IssDate { get; set; }
        public string? AppTitle { get; set; }
        public DateTime? EarliestIssDate { get; set; }
        public string? Applicants { get; set; }
        public string? LinkUrl { get; set; }
    }
}
