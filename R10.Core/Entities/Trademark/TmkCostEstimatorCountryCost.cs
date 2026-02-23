using R10.Core.Entities.Patent;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkCostEstimatorCountryCost : TmkCostEstimatorCountryCostDetail
    {
        public TmkCostEstimator? CostEstimator { get; set; }
        public TmkCostEstimatorCost? TmkCostEstimatorCost { get; set; }
        public TmkCostEstimatorCostChild? TmkCostEstimatorCostChild { get; set; }
        public TmkCostEstimatorCostSub? TmkCostEstimatorCostSub { get; set; }
    }

    public class TmkCostEstimatorCountryCostDetail : BaseEntity
    {
        [Key]
        public int EntityId { get; set; }
        public int KeyId { get; set; }
        public int CECostId { get; set; }
        public int? CECCId { get; set; }
        public int? CESubId { get; set; }
        public string? Answer { get; set; }
        public CEAnswerStatus AnswerStatus { get; set; }
    }
}
