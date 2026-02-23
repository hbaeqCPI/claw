using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSInventorHistory : BaseEntity
    {
        [Key]
        public int HistId { get; set; }
        public int DMSId { get; set; }
        public int? OldInventorID { get; set; }
        public int? NewInventorID { get; set; }
        public bool OldIsNonEmployee { get; set; }
        public bool NewIsNonEmployee { get; set; }
    }
}
