using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.DMS
{
    public class DisclosureRelatedDisclosure : BaseEntity
    {
        [Key]
        public int RelatedId { get; set; }

        public int DMSId { get; set; }

        public int RelatedDMSId { get; set; }

        public Disclosure? Disclosure { get; set; }

        public Disclosure? RelatedDisclosure { get; set; }
    }
}
