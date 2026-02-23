using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace R10.Core.Services
{

    public class RTSActionSearchService : IRTSActionSearchService
    {
        private readonly ICountryApplicationService _countryApplicationService;
        private readonly IApplicationDbContext _repository;
        private readonly ClaimsPrincipal _user;
        private readonly ISystemSettings<PatSetting> _settings;

        public RTSActionSearchService(
            ICountryApplicationService countryApplicationService,
            IApplicationDbContext repository,
            ClaimsPrincipal user,
            ISystemSettings<PatSetting> settings
            ) 
        {
            _countryApplicationService = countryApplicationService;
            _repository = repository;
            _user = user;
            _settings = settings;
        }

        public IQueryable<CountryApplication> CountryApplications
        {
            get
            {
                var applications = _countryApplicationService.CountryApplications;
                applications = applications.Where(ca=> _repository.RTSSearchRecords.Any(pl=>pl.PMSAppId==ca.AppId && pl.RTSSearchActions.Any()));
                return applications;
            }
        }

        public IQueryable<RTSSearchAction> SearchActions
        {
            get
            {
                var searchActions = _repository.RTSSearchActions.Where(a => CountryApplications.Any(ca => ca.RTSSearch.PLAppId == a.PLAppId));
                return searchActions;
            }
        }

        


    }
}

