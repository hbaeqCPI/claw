using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSValuation : BaseEntity
    {
        [Key]
        public int DMSValId { get; set; }
        public int DMSId { get; set; }

        public CPiEntityType ReviewerType { get; set; }
                
        public int? ReviewerId { get; set; }
                
        public int? ValId { get; set; }
                
        public int? RateId { get; set; }
        
        public string? Weight { get; set; }

        public DateTime? RatingDate { get; set; }

        public string? Remarks { get; set; }

        public string? UserId { get; set; }

        public Disclosure? Disclosure { get; set; }
        public DMSValuationMatrix? ValuationMatrices { get; set; }
        public DMSValuationMatrixRate? Rates { get; set; }

        //REVIEWER TYPES
        public ContactPerson? Contact { get; set; }
        public PatInventor? Inventor { get; set; }
    }
}
