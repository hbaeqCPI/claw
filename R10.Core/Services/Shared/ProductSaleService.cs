using System;
using System.Collections.Generic;
using System.Text;
using R10.Core.Entities;
using R10.Core.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace R10.Core.Services.Shared
{
    public class ProductSaleService : ChildEntityService<Product, ProductSale>, IProductSaleService
    {
        public ProductSaleService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        public override async Task<bool> Update(object key, string userName, IEnumerable<ProductSale> updated, IEnumerable<ProductSale> added, IEnumerable<ProductSale> deleted)
        {
            foreach (var item in updated)
            {
                item.CurrencyType = item.CurrencyType ?? string.Empty;
            }
            foreach (var item in added)
            {
                item.CurrencyType = item.CurrencyType ?? string.Empty;
            }

            return await base.Update(key, userName, updated, added, deleted);
        }

        //public async Task<bool> Update(object key, int parentId, string systemType, string userName, IEnumerable<ProductSale> updated, IEnumerable<ProductSale> added, IEnumerable<ProductSale> deleted)
        //{
        //    var productId = (int)key;

        //    var parent = await _cpiDbContext.GetRepository<Product>().GetByIdAsync(key);
        //    _cpiDbContext.GetRepository<Product>().Attach(parent);
        //    parent.UpdatedBy = userName;
        //    parent.LastUpdate = DateTime.Now;

        //    foreach (var item in updated)
        //    {
        //        item.UpdatedBy = parent.UpdatedBy;
        //        item.LastUpdate = parent.LastUpdate;
        //        item.CurrencyType = item.CurrencyType ?? string.Empty;
        //    }

        //    foreach (var item in added)
        //    {
        //        item.ProductId = productId;
        //        item.CreatedBy = parent.UpdatedBy;
        //        item.DateCreated = parent.LastUpdate;
        //        item.UpdatedBy = parent.UpdatedBy;
        //        item.LastUpdate = parent.LastUpdate;
        //        item.CurrencyType = item.CurrencyType ?? string.Empty;
        //    }

        //    var repository = _cpiDbContext.GetRepository<ProductSale>();
        //    repository.Delete(deleted);
        //    repository.Update(updated);
        //    repository.Add(added);

        //    await _cpiDbContext.SaveChangesAsync();
        //    return true;
        //}
    }
}
