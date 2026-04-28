using LawPortal.Core.Entities.Patent;
using LawPortal.Core.Entities.Trademark;
using LawPortal.Core.Interfaces;
using LawPortal.Infrastructure.Identity;
using LawPortal.Web.Areas.Shared.ViewModels;
using LawPortal.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LawPortal.Web.Extensions;
using LawPortal.Core.Entities;
using LawPortal.Core.Helpers;

namespace LawPortal.Web.Services
{
    public class CountryLookupViewModelService : ICountryLookupViewModelService
    {
        private readonly ClaimsPrincipal _claimsPrincipal;
        private readonly IEntityService<PatCountry> _patCountryService;
        private readonly IEntityService<TmkCountry> _tmkCountryService;

        public CountryLookupViewModelService(
            ClaimsPrincipal claimsPrincipal,
            IEntityService<PatCountry> patCountryService,
            IEntityService<TmkCountry> tmkCountryService)
        {
            _claimsPrincipal = claimsPrincipal;
            _patCountryService = patCountryService;
            _tmkCountryService = tmkCountryService;
        }

        public string CountrySource
        {
            get
            {
                var countrySource = new Dictionary<string, string>()
                {
                    { SystemType.Patent, SystemType.Patent },
                    { SystemType.Trademark, SystemType.Trademark },
                    { SystemType.IDS, SystemType.Patent }
                };

                foreach (var source in countrySource)
                {
                    if (_claimsPrincipal.IsInSystem(source.Key))
                        return source.Value;
                }

                return SystemType.Patent;
            }
        }

        public IQueryable<CountryLookupViewModel> Countries
        {
            get
            {
                switch (this.CountrySource)
                {
                    case SystemType.Trademark:
                        return _tmkCountryService.QueryableList.Select(c => new CountryLookupViewModel
                        {
                            Country = c.Country,
                            CountryName = c.CountryName
                        });

                    default:
                        return _patCountryService.QueryableList.Select(c => new CountryLookupViewModel
                        {
                            Country = c.Country,
                            CountryName = c.CountryName
                        });
                }
            }
        }
    }
}
