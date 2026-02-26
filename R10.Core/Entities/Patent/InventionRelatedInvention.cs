// using R10.Core.Entities.DMS; // Removed during deep clean
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class InventionRelatedInvention: BaseEntity
    {
        [Key]
        public int RelatedId { get; set; }

        public int InvId { get; set; }

        public int RelatedInvId { get; set; }

        public Invention? Invention { get; set; }

        public Invention? RelatedInvention { get; set; }
    }
}
