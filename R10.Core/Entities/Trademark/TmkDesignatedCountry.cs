using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Trademark
{
    public class TmkDesignatedCountry : BaseEntity
    {
        [Key]
        public int DesId { get; set; }

        public int TmkId { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "Des Country")]
        public string? DesCountry { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "Des Case Type")]
        public string? DesCaseType { get; set; }

        [StringLength(25)]
        public string? GenCaseNumber { get; set; }

        [StringLength(8)]
        [Display(Name = "Sub Case")]
        public string? GenSubCase { get; set; }

        [Display(Name = "Gen Date")]
        public DateTime? GenDate { get; set; }

        [Display(Name = "Gen App?")]
        public bool GenApp { get; set; }

        [Display(Name = "Single Class?")]
        public bool GenSingleClassApp { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
        public int? GenTmkId { get; set; }

        [NotMapped]
        public string? CountryName { get; set; }

        public TmkTrademark? TmkTrademark { get; set; }
        public TmkCountry? Country { get; set; }

    }
}
