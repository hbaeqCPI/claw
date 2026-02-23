using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatInventorAppAward: PatInventorAppAwardDetail
    {
        public CountryApplication? PatCountryApplication { get; set; }
        public PatInventor? PatInventor { get; set; }
        public PatInventorAwardCriteria? PatInventorAwardCriteria  { get; set; }
}
    public class PatInventorAppAwardDetail : BaseEntity
    {
        [Key]
        public int AwardId { get; set; }

        public int AppId { get; set; }

        public int InventorID { get; set; }

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "ID")]
        public int AwardCriteriaId { get; set; }

        [Display(Name = "Award Date")]
        public DateTime? AwardDate { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [StringLength(20)]
        [Display(Name = "Award Type")]
        public string? AwardType { get; set; }
    }
}
