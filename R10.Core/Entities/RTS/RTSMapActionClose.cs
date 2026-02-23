using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class RTSMapActionClose : BaseEntity
    {
        [Key]
        public int MapCloseId { get; set; }
        public int CloseSourceId { get; set; }
        public string? MapGroup { get; set; }
        public int MapSourceId { get; set; }
        
        [NotMapped]
        [Display(Name ="PTO Action")]
        public string? MapSearchAction { get; set; }

        public RTSMapActionDueSource? ActionSource { get; set; }
    }
}
