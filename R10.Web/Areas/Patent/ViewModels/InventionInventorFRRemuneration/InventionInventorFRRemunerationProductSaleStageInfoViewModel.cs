namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionInventorFRRemunerationProductSaleStageInfoViewModel
    {
        public int CurrentProductSaleId { get; set; }
        public double PrevRevenuesAccumulatedEuro { get; set; }
        public double CurrentRevenuesAccumulatedEuro { get; set; }
        public double ExchangeRate { get; set; }
        public double PrevRevenuesAccumulatedDM { get; set; }
        public double CurrentRevenuesAccumulatedDM { get; set; }
        public IQueryable<InventionInventorFRRemunerationProductSaleStageInfoStageViewModel>? PrevStages { get; set; }
        public IQueryable<InventionInventorFRRemunerationProductSaleStageInfoStageViewModel>? CurrentStages { get; set; }
        public double PrevRevenuesStaggeredDM { get; set; }
        public double CurrentRevenuesStaggeredDM { get; set; }
        public double RevenueForRemunerationDM { get; set; }
        public double RevenueForRemunerationEuro { get; set; }
    }
}
