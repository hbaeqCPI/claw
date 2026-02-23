using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Clearance
{
    public class TmcRelatedTrademark : BaseEntity
    {
        [Key]
        public int KeyId { get; set; }

        public int? TmcId { get; set; }

        public int? TmkId { get; set; }

        public int OrderOfEntry { get; set; }

        public TmkTrademark? ClearanceTrademark { get; set; }

        public TmcClearance? Clearance { get; set; }
    }
}
