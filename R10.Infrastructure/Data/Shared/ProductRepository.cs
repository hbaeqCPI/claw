 using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Interfaces;
using R10.Core.Entities.Shared;
using R10.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Transactions;


namespace R10.Infrastructure.Data
{
    public class ProductRepository : IProductRepository
    {
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly ApplicationDbContext _dbContext;
        public ProductRepository(ApplicationDbContext dbContext, ISystemSettings<DefaultSetting> settings)
        {
            _settings = settings;
            _dbContext = dbContext;
        }

        public async Task<List<SharedCountryLookupDTO>> GetSharedCountryList()
        {
            var list = await _dbContext.SharedCountryLookupDTO
                .FromSqlRaw($"SELECT CAST([Country] AS varchar) AS [Country], ISNULL([CountryName], '') AS [CountryName] FROM vwWebSysSharedCountry ORDER BY [Country]").AsNoTracking().ToListAsync();
            return list;
        }


        public async Task<List<RelatedProductDTO>> GetRelatedProducts(int productId)
        {
            var list = await _dbContext.RelatedProductDTO
                .FromSqlRaw($"procPrdRelatedProducts @ProductId={productId}").AsNoTracking().ToListAsync();
            return list;
        }



        public async Task UpdateChild<T>(Product product, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                product.UpdatedBy = userName;
                product.LastUpdate = DateTime.Now;
                var parent = _dbContext.Products.Attach(product);
                parent.Property(c => c.UpdatedBy).IsModified = true;
                parent.Property(c => c.LastUpdate).IsModified = true;

                foreach (var item in updated)
                {
                    item.UpdatedBy = product.UpdatedBy;
                    item.LastUpdate = product.LastUpdate;
                }

                foreach (var item in added)
                {
                    item.CreatedBy = product.UpdatedBy;
                    item.DateCreated = product.LastUpdate;
                    item.UpdatedBy = product.UpdatedBy;
                    item.LastUpdate = product.LastUpdate;
                }
                var dbSet = _dbContext.Set<T>();
                if (updated.Any())
                    dbSet.UpdateRange(updated);

                if (added.Any())
                    dbSet.AddRange(added);

                if (deleted.Any())
                    dbSet.RemoveRange(deleted);
                await _dbContext.SaveChangesAsync();

                scope.Complete();
            }
        }
    }
}
