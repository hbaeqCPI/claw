using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatIRRemunerationValuationMatrixData : BaseEntity
    {
        [Key]
        public int DataId { get; set; }
        public int RemunerationId { get; set; }
        public int? FactorId { get; set; }
        public int? MatrixId { get; set; }
        public int? CriteriaId { get; set; }
        public bool UseManualEntry { get; set; }
        public double? ActualValue { get; set; }

        public PatIRRemuneration? Remuneration { get; set; }
    }
}
