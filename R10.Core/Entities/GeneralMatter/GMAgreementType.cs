using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMAgreementType : BaseEntity
    {
        public int AgreementTypeID { get; set; }

        [Key]
        [Required]
        [StringLength(20)]
        [Display(Name = "Agreement Type")]
        public string AgreementType { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
