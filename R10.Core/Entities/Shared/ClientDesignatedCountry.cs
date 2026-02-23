using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    public class ClientDesignatedCountry : BaseEntity
    {
        [Key]
        public int EntityDesCtryID { get; set; }
        
        [Required]
        public int ClientID { get; set; }

        [StringLength(1)]
        [Required]
        public string?  SystemType { get; set; }

        public int? ParentDesCtryID { get; set; }

        [StringLength(5)]
        [Required]
        public string?  DesCtry { get; set; }

        [StringLength(3)]
        [Required]
        public string?  DesCaseType { get; set; }

        public bool? IsDropOnParentGrant { get; set; }
        public bool GenApp { get; set; }
        public string?  Remarks { get; set; }

        public Client? Client { get; set; }
        public PatCountry? PatCountry { get; set; }
        public TmkCountry? TmkCountry { get; set; }
    }
}
