using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ForeignFiling
{
    public class FFRemLogDue : RemLogDue
    {
        [StringLength(10)]
        public string? Client { get; set; }

        public RemLog<PatDueDate, FFRemLogDue> RemLog { get; set; }
        public PatDueDate DueDetail { get; set; }
    }
}
