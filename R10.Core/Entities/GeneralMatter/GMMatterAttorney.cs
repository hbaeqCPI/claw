using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMMatterAttorney : BaseEntity
    {
        [Key]
        public int AttID { get; set; }

        [Required]
        public int MatId { get; set; }

        [Required]
        public int AttorneyID { get; set; }

        public int OrderOfEntry { get; set; }

        public GMMatter? GMMatter { get; set; }
        public Attorney? Attorney { get; set; }
    }
}
