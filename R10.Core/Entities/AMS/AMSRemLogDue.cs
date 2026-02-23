using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSRemLogDue : RemLogDue
    {
        [StringLength(10)]
        public string? CPIClient { get; set; }

        [StringLength(10)]
        public string? CPIAttorney { get; set; }

        public RemLog<AMSDue, AMSRemLogDue> RemLog { get; set; }
        public AMSDue DueDetail { get; set; }
    }
}
