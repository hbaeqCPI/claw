using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.RMS
{
    public class RMSDueCountry : BaseEntity
    {
        [Key]
        public int DueCountryId { get; set; }

        [Required]
        public int DueId { get; set; }

        public string? Country { get; set; }

        public RMSDue? RMSDue { get; set; }

        public TmkCountry? TmkCountry { get; set; }
    }
}
