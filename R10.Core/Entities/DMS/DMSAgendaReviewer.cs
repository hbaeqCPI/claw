using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSAgendaReviewer : BaseEntity
    {
        [Key]
        public int AgendaRevId { get; set; }

        public int AgendaId { get; set; }

        public CPiEntityType ReviewerType { get; set; }
                
        public int? ReviewerId { get; set; }
        
        
        public ContactPerson? Contact { get; set; }
        public PatInventor? Inventor { get; set; }
    }
}
