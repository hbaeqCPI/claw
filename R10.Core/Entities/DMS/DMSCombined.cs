using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.DMS
{
    public class DMSCombined : BaseEntity
    {
        [Key]
        public int CombinedId { get; set; }

        public int DMSId { get; set; }

        public int CombinedDMSId { get; set; }

        public Disclosure? Disclosure { get; set; }

        public Disclosure? CombinedDisclosure { get; set; }
    }
}
