using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Identity;

namespace R10.Core.Interfaces
{

    public interface IProductRepository
    {
        Task<List<SharedCountryLookupDTO>> GetSharedCountryList();

        Task<List<RelatedProductDTO>> GetRelatedProducts(int productId);

        Task UpdateChild<T>(Product product, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity;
    }
}
