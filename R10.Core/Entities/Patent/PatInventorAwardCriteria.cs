using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatInventorAwardCriteria : PatInventorAwardCriteriaDetail
    {
        public PatCountry? PatCountry { get; set; }
        public PatInventorAwardType? PatInventorAwardType { get; set; }
        public List<PatInventorAppAward>? PatInventorAppAwards { get; set; }
        public List<PatInventorDMSAward>? PatInventorDMSAwards { get; set; }
    }
    public class PatInventorAwardCriteriaDetail : BaseEntity
    {
        [Key]
        [Display(Name = "ID")]
        public int AwardCriteriaId { get; set; }

        public int AwardTypeId { get; set; }

        [StringLength(3)]
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        //[Required]
        public string? Country { get; set; }

        [Display(Name = "Eff Start")]
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EffStartDate { get; set; }

        [Display(Name = "Eff End")]
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EffEndDate { get; set; }

        [Display(Name = "Individual Min Amount")]
        [Required]
        public decimal? IndividualAmount { get; set; }

        [Display(Name = "Individual Max Amount")]
        public decimal? IndividualMaxAmount { get; set; }

        [Display(Name = "Lead Amount")]
        public decimal? LeadAmount { get; set; }

        [Display(Name = "Total Max Amount")]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "Divide Total Max Amount if the number of Inventors is greater than the total number of inventors rewarded")]
        public bool DivideMaxAmount { get; set; } = true;

        [Display(Name = "First Filed?")]
        public bool FirstFiled { get; set; }

        [Display(Name = "First Issued?")]
        public bool FirstGranted { get; set; }

        [Display(Name = "Max Number of Inventor to Reward")]
        [Required]
        public int? NoOfInventors { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
        public string? Recommendation { get; set; }

        public string? InventorCondOp1 { get; set; }
        public string? InventorCondOp2 { get; set; }
        public int? InventorCondNum1 { get; set; }
        public int? InventorCondNum2 { get; set; }
        [Display(Name = "Used in Product?")]
        public bool UsedInProduct { get; set; }
        public string? ApplicationCondOp1 { get; set; }
        public string? ApplicationCondOp2 { get; set; }
        public int? ApplicationCondNum1 { get; set; }
        public int? ApplicationCondNum2 { get; set; }

    }
}