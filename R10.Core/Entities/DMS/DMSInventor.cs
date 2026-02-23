using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSInventor: DMSInventorDetail
    {
       
        public PatInventor? PatInventor { get; set; }

        public Disclosure? InventorDMSDisclosure { get; set; }

        [NotMapped]
        public int OldInventorID { get; set; }
    }

    public class DMSInventorDetail : BaseEntity
    {
        [Key]
        public int DMSInventorID { get; set; }
        public int DMSId { get; set; }
        public int InventorID { get; set; }
        [Display(Name = "Default Inventor")]
        public bool IsDefaultInventor { get; set; }

        [StringLength(15)]
        public string? Initial { get; set; }

        [Display(Name = "Initial Date")]
        public DateTime? InitialDate { get; set; }

        public int OrderOfEntry { get; set; }

        public string? Remarks { get; set; }

        [Display(Name = "Non-Employee")]
        public bool IsNonEmployee { get; set; }

        [Display(Name = "% of Invention")]
        [Range(0, 100, ErrorMessage = "Enter number between 0 to 100")]
        public double? Percentage { get; set; }
        
        [Display(Name = "Reviewed?")]
        public bool IsReviewed { get; set; }
    }
}
