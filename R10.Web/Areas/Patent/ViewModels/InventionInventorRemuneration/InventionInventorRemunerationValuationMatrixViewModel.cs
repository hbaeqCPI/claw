using R10.Core.Entities.Patent;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventorRemunerationValuationMatrixViewModel
    {
        public List<FormulaFactorViewModel> FormulaFactors { get; set; }
        public List<ValuationMatrixViewModel> ValuationMatrixes { get; set; }

    }

    public class FormulaFactorViewModel : PatIRRemunerationFormulaFactor
    {
        public double? ActualValue { get; set; }
        public bool UseManualEntry { get; set; }
    }

    public class ValuationMatrixViewModel : PatIRRemunerationValuationMatrix
    {
        public double? ActualValue { get; set; }
        public bool UseManualEntry { get; set; }
        public List<ValuationMatrixCriteriaViewModel> ValuationMatrixCriteria { get; set; }
    }

    public class ValuationMatrixCriteriaViewModel : PatIRRemunerationValuationMatrixCriteria
    {
        public double? ActualValue { get; set; }
        public bool UseManualEntry { get; set; }
    }

    public class ManualEntryOptions
    {
        public int FactorManualId { get; set; }
        public int MatrixManualId { get; set; }
        public double? ActualValue { get; set; }
    }
}
