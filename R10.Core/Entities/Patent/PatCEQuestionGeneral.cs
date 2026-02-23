using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatCEQuestionGeneral: PatCEQuestionGeneralDetail
    {
        public PatCostEstimator CostEstimator { get; set; }
        public PatCEGeneralCost PatCEGeneralCost { get; set; }
    }

    public class PatCEQuestionGeneralDetail : BaseEntity
    {
        [Key]
        public int EntityId { get; set; }
        public int KeyId { get; set; }
        public int CostId { get; set; }
        public string? Answer { get; set; }
    }
}
