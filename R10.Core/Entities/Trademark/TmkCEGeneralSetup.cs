using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{
    public class TmkCEGeneralSetup: BaseEntity
    {
        [Key]
        public int CEGeneralId { get; set; }

        [StringLength(25)]
        [Display(Name = "Cost Setup")]
        [Required]
        public string CostSetup { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string Description { get; set; }        

       
        public List<TmkCEGeneralCost>? TmkCEGeneralCosts { get; set; }
        public List<Client>? Client { get; set; }

        [NotMapped]
        public string? CopyOptions { get; set; }
    }
    
}
