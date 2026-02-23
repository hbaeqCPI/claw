using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;

namespace R10.Core.Services
{
    public class TLUpdateLookupService : ITLUpdateLookupService
    {
     
        private readonly IApplicationDbContext _repository;
        private readonly ITLUpdateService _updateService;

        public TLUpdateLookupService(IApplicationDbContext repository, ITLUpdateService updateService)
        {
            _repository = repository;
            _updateService = updateService;
        }

        public  IQueryable<LookupDTO> GetClientList<T>() where T : TMSEntityFilter
        {
            var result = _repository.Clients.Where(c => _updateService.TLUpdates<T>().Any(t => t.ClientId == c.ClientID))
                                          .Select(c => new LookupDTO { Value = c.ClientCode, Text = c.ClientName });
            return result;
        }

        public IQueryable<LookupDTO> GetCountryList<T>() where T : TMSEntityFilter
        {
            var result = _repository.TmkCountries.Where(c => _updateService.TLUpdates<T>().Any(t => t.TMSCountry == c.Country))
                                          .Select(c => new LookupDTO { Value = c.Country, Text = c.CountryName });
            return result;
        }


        public IQueryable<T> TLUpdates<T>() where T : TMSEntityFilter {
            return _updateService.TLUpdates<T>();
        }

        public async Task<IQueryable<TLActionComparePTO>> TLActionUpdates() {
            return await _updateService.TLActionUpdates();
        }



    }
}
