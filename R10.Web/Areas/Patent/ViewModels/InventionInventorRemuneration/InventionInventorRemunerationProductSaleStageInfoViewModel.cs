namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionInventorRemunerationProductSaleStageInfoViewModel
    {
        public int CurrentProductSaleId { get; set; }
        public double PrevRevenuesAccumulatedEuro { get; set; }
        public double CurrentRevenuesAccumulatedEuro { get; set; }
        public double ExchangeRate { get; set; }
        //public double PrevRevenuesAccumulatedDM { get; set; }
        //public double CurrentRevenuesAccumulatedDM { get; set; }
        public IQueryable<InventionInventorRemunerationProductSaleStageInfoStageViewModel>? PrevStages { get; set; }
        public IQueryable<InventionInventorRemunerationProductSaleStageInfoStageViewModel>? CurrentStages { get; set; }
        //public double PrevRevenuesStaggeredDM { get; set; }
        public double PrevRevenuesStaggeredEuro { get; set; }
        //public double CurrentRevenuesStaggeredDM { get; set; }
        public double CurrentRevenuesStaggeredEuro { get; set; }
        //public double RevenueForRemunerationDM { get; set; }
        public double RevenueForRemunerationEuro { get; set; }
    }
}
