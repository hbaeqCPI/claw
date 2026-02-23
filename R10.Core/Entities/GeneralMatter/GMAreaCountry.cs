using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMAreaCountry : BaseEntity
    {
        [Key]
        public int AreaCtryId { get; set; }

        [Required]
        public int AreaID { get; set; }

        [Required]
        public string? Country { get; set; }
               
        public GMCountry? GMCountry { get; set; }
        public GMArea? GMArea { get; set; }

    }
}
