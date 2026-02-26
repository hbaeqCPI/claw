// using R10.Core.Entities.DMS; // Removed during deep clean
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

//         public Disclosure? InventionDisclosure { get; set; } // Removed during deep clean

        public Invention? Invention { get; set; }
    }
}
