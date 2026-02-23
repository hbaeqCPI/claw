using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Trademark
{
    public class TLMapActionDueSource
    {
        [Key]
        public int MapSourceId { get; set; }

        [Display(Name ="Country")]
        public string? MapCountry { get; set; }
        [Display(Name = "Trademark Office Action")]
        public string? MapSearchAction { get; set; }

        public bool? MapDisplay { get; set; }
        public bool? MapCompare { get; set; }
        public bool? MapUpdate { get; set; }

        public List<TLMapActionClose>? ActionsClose { get; set; }
    }

}
