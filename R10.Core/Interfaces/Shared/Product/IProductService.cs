using System;
using System.Collections.Generic;
using System.Text;
using R10.Core.Entities;
using System.Threading.Tasks;
using R10.Core.DTOs;
using System.Linq;
using R10.Core.Queries.Shared;

namespace R10.Core.Interfaces
{

    public interface IProductService : IEntityService<Product>
    {
        Task<List<SharedCountryLookupDTO>> GetSharedCountryList();
        IQueryable<SharedCountryLookupDTO> SharedCountries { get; }

        Task<List<RelatedProductDTO>> GetRelatedProducts(int productId);

        Task UpdateChild<T>(int productId, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity;
    }
}
