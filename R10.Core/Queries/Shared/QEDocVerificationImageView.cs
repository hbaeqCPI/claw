using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEDocVerificationImageView
    {        
        public int DocId { get; set; }        
        public string? DocName { get; set; }
        public string? DocFileName { get; set; }
        public string? ThumbFileName { get; set; }
        public DateTime? ImageDate { get; set; }        
        public string? Remarks { get; set; }
        public string? UserFileName { get; set; }
        public string? DateToday { get; set; }
        public DateTime? DateTimeToday { get; set; }
        public string? DocVerificationUrl { get; set; }
    }
}
