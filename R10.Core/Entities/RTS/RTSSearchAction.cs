using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class RTSSearchAction
    {
        [Key]
        public int PLActId { get; set; }
        public int PLAppId { get; set; }
        public int OrderOfEntry { get; set; }
        public string? SearchAction { get; set; }
        public DateTime? BaseDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? DateCompleted { get; set; }
        public DateTime? LastWebUpdate { get; set; }

        public RTSSearch? RTSSearch { get; set; }
    }

}
