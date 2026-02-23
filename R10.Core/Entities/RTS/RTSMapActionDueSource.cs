using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class RTSMapActionDueSource
    {
        [Key]
        public int MapSourceId { get; set; }

        [Display(Name ="Country")]
        public string? MapCountry { get; set; }
        [Display(Name = "Patent Office Action")]
        public string? MapSearchAction { get; set; }

        public bool? MapDisplay { get; set; }
        public bool? MapCompare { get; set; }
        public bool? MapUpdate { get; set; }

        [NotMapped]
        public string? FormExtractLink { get; set; } = string.Empty;

        public List<RTSMapActionClose>? ActionsClose { get; set; }
    }

}
