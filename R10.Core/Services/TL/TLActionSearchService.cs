using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace R10.Core.Services
{

    public class TLActionSearchService : ITLActionSearchService
    {
        private readonly IApplicationDbContext _repository;
        private readonly ClaimsPrincipal _user;
        private readonly ISystemSettings<TmkSetting> _settings;
        private readonly ITmkTrademarkService _trademarkService;
        

        public TLActionSearchService(
            IApplicationDbContext repository,
            ClaimsPrincipal user,
            ISystemSettings<TmkSetting> settings,
            ITmkTrademarkService trademarkService
            ) 
        {
            _repository = repository;
            _user = user;
            _settings = settings;
            _trademarkService = trademarkService;
        }

        public IQueryable<TmkTrademark> TmkTrademarks
        {
            get
            {
                var trademarks = _trademarkService.TmkTrademarks;
                trademarks = trademarks.Where(tmk=> _repository.TLSearchRecords.Any(tl=>tl.TMSTmkId==tmk.TmkId && tl.TLSearchActions.Any()));
                return trademarks;
            }
        }

        public IQueryable<TLSearchAction> SearchActions
        {
            get
            {
                var searchActions = _repository.TLSearchActions.Where(a => TmkTrademarks.Any(tmk => tmk.TLSearch.TLTmkId == a.TLTmkId));
                return searchActions;
            }
        }


    }
}

