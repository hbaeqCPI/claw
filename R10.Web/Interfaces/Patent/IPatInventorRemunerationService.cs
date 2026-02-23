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
    public interface IPatInventorRemunerationService
    {
        Task<PatIRRemuneration> GetByInvIdAsync(int invId);
        Task InventorListSave(int remunerationId, List<PatInventorInv> updated, string userName);
        Task UpdateDistribution(int remunerationId, List<PatIRProductSale> added, List<PatIRProductSale> updated, List<PatIRProductSale> deleted);
        Task DeleteDistribution(PatIRProductSale deleted);
        Task UpdateDistribution(int remunerationId, List<PatInventorInv> updated);
        Task<int> GetRemunerationId(int invId);
        Task<List<PatInventorInv>> GetAvailablePatInventorInvs(int invId);
        Task<InventionInventorRemunerationTotalCostViewModel> GetInventorRemunerationTotalCost(int invId);

        Task<double> GetTotalCost(Invention invention);
        Task<List<InventionInventorRemunerationYearlyCostViewModel>> GetYearlyCost(Invention invention);
        Task<int> CalculateInventorPosition(int? sum);

        Task InitRemuneration(Invention invention);
        Task UpdateCompensationEndDate(int invId, DateTime? compensationEndDate, string userName);
        Task DeleteRemuneration(int invId, bool fromInvention = false);
        Task<InventorRemunerationValuationMatrixViewModel> GetInventorRemunerationValuationMatrixViewModels(int remunerationId);
        Task UpdateMatrixes(InventionInventorRemunerationViewModel matrixData, string userName);
        Task<double> GetLumpSumAmount(int remunerationId, InventionInventorRemunerationInventorInfoViewModel invt);
        Task<InventionInventorRemunerationProductSaleStageInfoViewModel> GetStaggeredInfo(int productSaleId);
        Task ProcessImportedProductSales(string userName);
        //Task ProcessImportedProductSales(ProductSale imported, string userName);
        Task SharedProductSalesDelete(ProductSale deleted);
        Task SharedProductSalesUpdate(List<ProductSale> updated, string userName);
        Task SharedProductSalesAdd(List<ProductSale> added, string userName);
        Task<double> CalculateDistributionValue(PatIRDistribution distribution);
        Task LoadInventionProductsByProductIds(List<int> productIds, string userName);
        Task LoadInventionProducts(int invId, int remunerationId, string userName);
        Task<List<RemunerationRevenueViewModel>> GetRemunerationRevenue(int invId);
        Task<RevenueForRemunerationViewModel> GetRevenueForRemuneration(int year, List<PatIRProductSale> productSales, int remunerationId);


    }
}
