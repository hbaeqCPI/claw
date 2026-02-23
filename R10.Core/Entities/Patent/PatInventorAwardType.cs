using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatInventorAwardType: PatInventorAwardTypeDetail
    {
        public List<PatInventorAwardCriteria>? PatInventorAwardCriterias { get; set; }
    }

    public class PatInventorAwardTypeDetail : BaseEntity
    {
        [Key]
        public int AwardTypeId { get; set; }

        [StringLength(20)]
        [Display(Name = "Award Type")]
        public string? AwardType { get; set; }

        [StringLength(20)]
        [Display(Name = "Based On")]
        public string? BasedOn { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Max Amount")]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "Clients")]
        public string? ClientIds { get; set; }
        [NotMapped]
        public int[]? Clients { get; set; }
    }
}
