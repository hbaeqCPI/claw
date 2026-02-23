using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatCostEstimatorCountryCost: PatCostEstimatorCountryCostDetail
    {
        public PatCostEstimator? CostEstimator { get; set; }
        public PatCostEstimatorCost? PatCostEstimatorCost { get; set; }
        public PatCostEstimatorCostChild? PatCostEstimatorCostChild { get; set; }
        public PatCostEstimatorCostSub? PatCostEstimatorCostSub { get; set; }
    }

    public class PatCostEstimatorCountryCostDetail : BaseEntity
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

    public enum CEAnswerStatus
    {
        NotSet,
        Original,
        Cascaded
    }
}
