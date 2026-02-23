using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class InventionRelatedDisclosure: BaseEntity
    {
        [Key]
        public int KeyId { get; set; }

        public int? InvId { get; set; }

        public int? DMSId { get; set; }

        public int OrderOfEntry { get; set; }

        public Disclosure? InventionDisclosure { get; set; }

        public Invention? Invention { get; set; }
    }
}
