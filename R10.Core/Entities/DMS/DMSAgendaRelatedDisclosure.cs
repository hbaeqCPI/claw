using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.DMS
{
    public class DMSAgendaRelatedDisclosure : BaseEntity
    {
        [Key]
        public int RelatedId { get; set; }

        public int AgendaId { get; set; }

        public int DMSId { get; set; }        

        public DMSAgenda? DMSAgenda { get; set; }
        public Disclosure? Disclosure { get; set; }
        
    }
}
