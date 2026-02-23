using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSReview : BaseEntity
    {
        [Key]
        public int DMSReviewId { get; set; }
        public int DMSId { get; set; }

        public CPiEntityType ReviewerType { get; set; }

        //NULLABLE FOREIGN KEY SO EF WILL USE LEFT JOIN INSTEAD OF INNER JOIN
        public int? ReviewerId { get; set; }

        //NULLABLE FOREIGN KEY SO EF WILL USE LEFT JOIN INSTEAD OF INNER JOIN
        public int? RatingId { get; set; }

        public DateTime? RatingDate { get; set; }

        public string? UserId { get; set; }

        public string? Remarks { get; set; }

        public Disclosure? Disclosure { get; set; }
        public DMSRating? Rating { get; set; }

        //REVIEWER TYPES
        public ContactPerson? Contact { get; set; }
        public PatInventor? Inventor { get; set; }
    }
}
