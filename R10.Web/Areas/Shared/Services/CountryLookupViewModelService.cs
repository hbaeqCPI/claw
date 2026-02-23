using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Infrastructure.Identity;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using R10.Web.Extensions;
using R10.Core.Entities;
using R10.Core.Helpers;

namespace R10.Web.Services
{
    public class CountryLookupViewModelService : ICountryLookupViewModelService
    {
        private readonly ClaimsPrincipal _claimsPrincipal;
        private readonly IParentEntityService<PatCountry, PatAreaCountry> _patCountryService;
        private readonly IParentEntityService<TmkCountry, TmkAreaCountry> _tmkCountryService;
        private readonly IParentEntityService<GMCountry, GMAreaCountry> _gmCountryService;

        public CountryLookupViewModelService(
            ClaimsPrincipal claimsPrincipal,
            IParentEntityService<PatCountry, PatAreaCountry> patCountryService,
            IParentEntityService<TmkCountry, TmkAreaCountry> tmkCountryService,
            IParentEntityService<GMCountry, GMAreaCountry> gmCountryService)
        {
            _claimsPrincipal = claimsPrincipal;
            _patCountryService = patCountryService;
            _tmkCountryService = tmkCountryService;
            _gmCountryService = gmCountryService;
        }

        public string CountrySource
        {
            get
            {
                var countrySource = new Dictionary<string, string>()
                {
                    { SystemType.Patent, SystemType.Patent },
                    { SystemType.Trademark, SystemType.Trademark },
                    { SystemType.GeneralMatter, SystemType.GeneralMatter },
                    { SystemType.AMS, SystemType.Patent },
                    { SystemType.DMS, SystemType.Patent },
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
                    case SystemType.GeneralMatter:
                        return _gmCountryService.QueryableList.Select(c => new CountryLookupViewModel
                        {
                            CountryID = c.CountryID,
                            Country = c.Country,
                            CountryName = c.CountryName
                        });

                    case SystemType.Trademark:
                        return _tmkCountryService.QueryableList.Select(c => new CountryLookupViewModel
                        {
                            CountryID = c.CountryID,
                            Country = c.Country,
                            CountryName = c.CountryName
                        });

                    default:
                        return _patCountryService.QueryableList.Select(c => new CountryLookupViewModel
                        {
                            CountryID = c.CountryID,
                            Country = c.Country,
                            CountryName = c.CountryName
                        });
                }
            }
        }
    }
}
