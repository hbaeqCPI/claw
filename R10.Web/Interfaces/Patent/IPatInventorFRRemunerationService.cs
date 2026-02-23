using R10.Core;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Web.Areas.Patent.ViewModels;
using R10.Core.Entities;

namespace R10.Web.Interfaces
{
    public interface IPatInventorFRRemunerationService
    {
        Task UpdateDistribution(int remunerationId, List<PatIRFRProductSale> added, List<PatIRFRProductSale> updated);
        Task DeleteDistribution(PatIRFRProductSale deleted);
        Task UpdateDistribution(int remunerationId, List<PatInventorInv> updated);
        InventionInventorRemunerationTotalCostViewModel GetInventorRemunerationTotalCost(int invId);
        InventionInventorRemunerationTotalCostViewModel GetInventorRemunerationTotalCost(Invention invention);

        double GetTotalCost(Invention invention);
        List<InventionInventorRemunerationYearlyCostViewModel> GetYearlyCost(Invention invention);
        int CalculateInventorPosition(int? sum);

        Task InitRemuneration(Invention invention);
        Task DeleteRemuneration(int invId, bool fromInvention = false);
        InventorFRRemunerationValuationMatrixViewModel GetInventorRemunerationValuationMatrixViewModels(int remunerationId);
        Task UpdateMatrixes(InventionInventorFRRemunerationViewModel matrixData, string userName);
        double? GetLumpSumAmount(int remunerationId, InventionInventorFRRemunerationInventorInfoViewModel invt);
        double? GetPaymentAmount(int remunerationId, InventionInventorFRRemunerationInventorInfoViewModel invt, string RemunerationType);
        InventionInventorFRRemunerationProductSaleStageInfoViewModel GetStaggeredInfo(int productSaleId);
        Task ProcessImportedProductSales(string userName);
        //Task ProcessImportedProductSales(ProductSale imported, string userName);
        Task SharedProductSalesDelete(ProductSale deleted);
        Task SharedProductSalesUpdate(List<ProductSale> updated, string userName);
        Task SharedProductSalesAdd(List<ProductSale> added, string userName);
        Task<double> CalculateDistributionValue(PatIRFRDistribution distribution);
    }
}
