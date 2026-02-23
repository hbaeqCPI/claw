using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace R10.Core.Entities.Trademark
{
    public class TmkCostEstimator : TmkCostEstimatorDetail
    {
        public List<TmkCostEstimatorCountry>? TmkCostEstimatorCountries { get; set; }
        public TmkTrademark? BaseTmkTrademark { get; set; }
        public TmkDueDate? TmkDueDate { get; set; }
        public List<TmkCostEstimatorCountryCost>? TmkCostEstimatorCountryCosts { get; set; }
        public List<TmkCEQuestionGeneral>? TmkCEQuestionGenerals { get; set; }

        [NotMapped]
        public string? CopyOptions { get; set; }
    }

    public class TmkCostEstimatorDetail : BaseEntity
    {
        [Key]
        public int KeyId { get; set; }

        [StringLength(60)]
        [Display(Name = "Cost Estimator Name")]
        [Required]
        public string Name { get; set; }

        //0 = Patent; 1 = PCT National Phase; 2 = EPO Validation
        public int? ApplicationType { get; set; }

        //0 = Both, 1 = Filing, 2 = Renewal
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TmkCostEstimateType EstimateType { get; set; }

        public int? TmkId { get; set; }
        public int? DDId { get; set; }

        [Display(Name = "Projected Filing Date")]
        public DateTime? ProjectedFilDate { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        public double? ExchangeRate { get; set; }
    }

    public enum TmkCostEstimateType
    {
        Both,
        Filing,
        Renewal
    }
}
