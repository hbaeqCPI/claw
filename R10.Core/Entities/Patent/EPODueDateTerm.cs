using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{   
    public class EPODueDateTerm : BaseEntity
    {
        [Key]
        public int TermId { get; set; }
        public int LogId { get; set; }

        [Required]
        public string? TermKey { get; set; }        
        public string? DescriptionEN { get; set; }
        public string? DescriptionFR { get; set; }
        public string? DescriptionDE { get; set; }

        public List<PatEPOActionMapAct>? PatEPOActionMapActs { get; set; }
    }
}
