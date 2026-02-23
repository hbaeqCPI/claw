using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class RTSPatentWatch : BaseEntity
    {
        [Key]
        public int WatchId { get; set; }
        
        [Display(Name ="Number Type")]
        public string? NumberType { get; set; }

        [Display(Name = "Number")]
        public string? Number { get; set; }

        [Display(Name = "Status")]
        public string? CurrentStatus { get; set; }

        [Display(Name = "Event Date")]
        public DateTime? EventDate { get; set; }

        [Display(Name = "Latest Legal Status Event")]
        public string? EventCode { get; set; }

        [Display(Name = "Event Desc")]
        public string? EventDesc { get; set; }

        public string? AppNo { get; set; }
        public string? LinkUrl { get; set; }
        
        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        public string? Title { get; set; }
        public DateTime? PubDate { get; set; }
        public string? Remarks { get; set; }
        public string? Keywords { get; set; }
    }
    
}
