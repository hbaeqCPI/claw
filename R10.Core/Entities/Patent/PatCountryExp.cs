using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PatCountryExp: BaseEntity
    {
        [Key]
        public int CExpId { get; set; }

        public int CountryLawID { get; set; }

        [Required]
        [StringLength(5)]
        public string? Country { get; set; }

        [Required]
        [StringLength(3)]
        public string? CaseType { get; set; }

        [StringLength(30)]
        [Display(Name = "Type")]
        public string? Type { get; set; }

        [StringLength(15)]
        [Display(Name = "Based On")]
        public string? BasedOn { get; set; }

        [Display(Name = "Yr")]
        public int Yr { get; set; }

        [Display(Name = "Mo")]
        public int Mo { get; set; }

        [Display(Name = "Dy")]
        public int Dy { get; set; }

        [StringLength(15)]
        [Display(Name = "Eff Based On")]
        public string? EffBasedOn { get; set; }

        [Display(Name = "Eff Start Date")]
        public DateTime? EffStartDate { get; set; }

        [Display(Name = "Eff End Date")]
        public DateTime? EffEndDate { get; set; }

        [NotMapped]
        public byte[]? ParentTStamp { get; set; }
       
    }
    
}
