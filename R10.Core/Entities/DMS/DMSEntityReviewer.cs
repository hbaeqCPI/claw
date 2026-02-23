using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSEntityReviewer : BaseEntity
    {
        [Key]
        public int DMSReviewerId { get; set; }               

        public DMSReviewerType EntityType { get; set; }

        //NULLABLE FOREIGN KEY SO EF WILL USE LEFT JOIN INSTEAD OF INNER JOIN
        public int? EntityId { get; set; }

        public CPiEntityType ReviewerType { get; set; }

        //NULLABLE FOREIGN KEY SO EF WILL USE LEFT JOIN INSTEAD OF INNER JOIN
        public int? ReviewerId { get; set; }

        //ENTITY TYPES
        public Client? Client { get; set; }
        public PatArea? Area { get; set; }

        //REVIEWER TYPES
        public ContactPerson? Contact { get; set; }
        public PatInventor? Inventor { get; set; }
    }
}
