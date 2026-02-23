using R10.Core.Entities.Patent;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionInventorRemunerationViewModel : PatIRRemuneration
    {
        public string? CaseNumber { get; set; }
        public string? ClientCode { get; set; }
        public string? ClientName { get; set; }
        public string? Title { get; set; }
        public string? MatrixData { get; set; }
    }
}
