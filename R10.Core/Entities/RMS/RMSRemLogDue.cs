using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.RMS
{
    public class RMSRemLogDue : RemLogDue
    {
        [StringLength(10)]
        public string? Client { get; set; }

        [StringLength(10)]
        public string? Agent { get; set; }  //for logging agent confirmation letters

        public RemLog<TmkDueDate, RMSRemLogDue>? RemLog { get; set; }
        public TmkDueDate? DueDetail { get; set; }
    }
}
