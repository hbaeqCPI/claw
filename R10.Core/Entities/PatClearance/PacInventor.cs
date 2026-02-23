using R10.Core.Entities.PatClearance;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.PatClearance
{
    public class PacInventor: PacInventorDetail
    {
       
        public PatInventor PatInventor { get; set; }

        public PacClearance InventorPacClearance { get; set; }

    }

    public class PacInventorDetail : BaseEntity
    {
        [Key]
        public int PacInventorID { get; set; }
        public int PacId { get; set; }
        public int InventorID { get; set; }
        
        [StringLength(15)]
        public string? Initial { get; set; }

        [Display(Name = "Initial Date")]
        public DateTime? InitialDate { get; set; }

        public int OrderOfEntry { get; set; }

        public string? Remarks { get; set; }
    }
}
