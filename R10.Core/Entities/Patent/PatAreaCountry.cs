using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatAreaCountry
    {
        [StringLength(10)]
        [Required, Display(Name = "Area")]
        public PatArea? Area { get; set; }

        [StringLength(5)]
        [Required, Display(Name = "Country")]
        public string? Country { get; set; }
    }
}
