using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PatCostEstimator: PatCostEstimatorDetail
    {     
        public List<PatCostEstimatorCountry>? PatCostEstimatorCountries { get; set; }
        public CountryApplication? BaseCountryApplication { get; set; }
        public Invention? BaseInvention { get; set; }
        public PatDueDate? PatDueDate { get; set; }
        public List<PatCostEstimatorCountryCost>? PatCostEstimatorCountryCosts { get; set; }
        public List<PatCEQuestionGeneral>? PatCEQuestionGenerals { get; set; }

        [NotMapped]
        public string? CopyOptions { get; set; }
    }

    public class PatCostEstimatorDetail : BaseEntity
    {
        [Key]
        public int KeyId { get; set; }

        [StringLength(60)]
        [Display(Name = "Cost Estimator Name")]
        [Required]
        public string Name { get; set; }

        //0 = Large Entity; 1 = Small Entity; 2 = Micro Entity
        public int? Applicant { get; set; }

        //0 = Patent; 1 = PCT National Phase; 2 = EPO Validation
        public int? ApplicationType { get; set; }

        public int? AppId { get; set; }
        public int? InvId { get; set; }
        public int? DDId { get; set; }

        [Display(Name = "Projected Filing Date")]
        public DateTime? ProjectedFilDate { get; set; }

        [Display(Name = "File through EPO")]
        public bool FileEPO { get; set; }

        [Display(Name = "File through PCT")]
        public bool FilePCT { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        public double? ExchangeRate { get; set; }
    }
}
