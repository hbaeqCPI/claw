namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionInventorFRRemunerationTotalCostViewModel
    {
        public int InvId { get; set; }

        public string CaseNumber { get; set; }

        public double TotalCost { get; set; }

        public List<InventionInventorFRRemunerationYearlyCostViewModel> YearlyCost { get; set; }
    }

    public class InventionInventorFRRemunerationYearlyCostViewModel
    {
        public int Year { get; set; }
        public double Cost { get; set; }
    }
}
