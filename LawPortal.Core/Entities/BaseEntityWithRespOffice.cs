using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LawPortal.Core.Entities
{
    public class BaseEntityWithRespOffice:BaseEntity
    {
        [StringLength(10)]
        [Display(Name = "Responsible Office")]
        public string? RespOffice { get; set; }

    }
}
