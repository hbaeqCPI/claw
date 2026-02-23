using R10.Core.Entities.Patent;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventorFRRemunerationValuationMatrixViewModel
    {
        public List<IRFRFormulaFactorViewModel> FormulaFactors { get; set; }
        public List<IRFRValuationMatrixViewModel> ValuationMatrixes { get; set; }

    }

    public class IRFRFormulaFactorViewModel : PatIRFRRemunerationFormulaFactor
    {
        public double? ActualValue { get; set; }
        public bool UseManualEntry { get; set; }
    }

    public class IRFRValuationMatrixViewModel : PatIRFRRemunerationValuationMatrix
    {
        public double? ActualValue { get; set; }
        public bool UseManualEntry { get; set; }
        public List<IRFRValuationMatrixCriteriaViewModel> ValuationMatrixCriteria { get; set; }
    }

    public class IRFRValuationMatrixCriteriaViewModel : PatIRFRRemunerationValuationMatrixCriteria
    {
        public double? ActualValue { get; set; }
        public bool UseManualEntry { get; set; }
    }

    public class IRFRManualEntryOptions
    {
        public int FactorManualId { get; set; }
        public int MatrixManualId { get; set; }
        public double? ActualValue { get; set; }
    }
}
