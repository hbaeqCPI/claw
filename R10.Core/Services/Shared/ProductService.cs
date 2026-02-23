using System;
using System.Collections.Generic;
using System.Text;
using R10.Core.Entities.Shared;
using R10.Core.Interfaces.Shared;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using R10.Core.Queries.Shared;

namespace R10.Core.Services
{
    public class ProductService : EntityService<Product>, IProductService
    {
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly IProductRepository _productRepository;
        private readonly IApplicationDbContext _repository;


        public ProductService(
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user,
            ISystemSettings<DefaultSetting> settings,
            IProductRepository productRepository,
            IApplicationDbContext repository
            ) : base(cpiDbContext, user)
        {
            _settings = settings;
            _productRepository = productRepository;
            _repository = repository;
        }

        public async Task<List<SharedCountryLookupDTO>> GetSharedCountryList()
        {
            return await _productRepository.GetSharedCountryList();
        }

        public IQueryable<SharedCountryLookupDTO> SharedCountries => _repository.SharedCountryLookupDTO
                            .FromSqlRaw($"SELECT CAST([Country] AS varchar) AS [Country], ISNULL([CountryName], '') AS [CountryName] FROM vwWebSysSharedCountry").AsNoTracking();


        public async Task<List<RelatedProductDTO>> GetRelatedProducts(int productId)
        {
            return await _productRepository.GetRelatedProducts(productId);
        }

        public async Task UpdateChild<T>(int productId, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            var product = await GetByIdAsync(productId);
            await _productRepository.UpdateChild(product, userName, updated, added, deleted);
        }

    }


}
