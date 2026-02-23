using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMMatterCountry : BaseEntity
    {
        [Key]
        public int CtryID { get; set; }

        [Required]
        public int MatId { get; set; }

        [Required]
        [StringLength(5)]
        [UIHint("Country")]
        public string? Country { get; set; }

        public GMMatter? GMMatter { get; set; }
        public GMCountry? GMCountry { get; set; }
    }
}
