using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatAreaCountry: BaseEntity
    {
        [Key]
        public int AreaCtryId { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public int AreaID { get; set; }

        public PatCountry? AreaCountry { get; set; }
        public PatArea? Area { get; set; }

    }
}
