using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Patent
{
    public class PatEPODocumentMapTag : BaseEntity
    {
        [Key]
        public int MapTagId { get; set; }

        [Display(Name = "Communication Code")]
        [StringLength(25)]
        public string? DocumentCode { get; set; }

        [Required]
        [StringLength(255)]        
        public string? Tag { get; set; }
    }
}
