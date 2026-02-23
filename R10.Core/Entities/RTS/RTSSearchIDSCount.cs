using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class RTSSearchIDSCount
    {
        [Key]
        public int CountLogId { get; set; }
        public int PLAppId { get; set; }
        public DateTime? MailRoomDate { get; set; }
        public int ReferenceCount { get; set; }
        public int NPLCount { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public bool? ProcessOK { get; set; }

    }

}
