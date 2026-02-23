using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkCEQuestionGeneral: TmkCEQuestionGeneralDetail
    {
        public TmkCostEstimator CostEstimator { get; set; }
        public TmkCEGeneralCost TmkCEGeneralCost { get; set; }
    }

    public class TmkCEQuestionGeneralDetail : BaseEntity
    {
        [Key]
        public int EntityId { get; set; }
        public int KeyId { get; set; }
        public int CostId { get; set; }
        public string? Answer { get; set; }
    }
}
