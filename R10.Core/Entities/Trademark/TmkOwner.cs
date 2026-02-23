using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Trademark
{
    public class TmkOwner : BaseEntity
    {
        [Key]
        public int TmkOwnerID { get; set; }

        [Required]
        public int TmkID { get; set; }

        [Required]
        public int OwnerID { get; set; }

        public int? OrderOfEntry { get; set; }

        public string? Remarks { get; set; }

        public Owner? Owner { get; set; }
        public TmkTrademark? TmkTrademark { get; set; }

        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        public double? Percentage { get; set; }
    }
}
