using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using R10.Core;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Trademark.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.Services
{
    public class TmkConflictViewModelService : ITmkConflictViewModelService
    {

        protected readonly ITmkConflictService _conflictService;
        protected readonly ITmkTrademarkService _trademarkService;
        private readonly IMapper _mapper;


        public TmkConflictViewModelService(ITmkConflictService conflictService, ITmkTrademarkService trademarkService, IMapper mapper)
        {
            _conflictService = conflictService;
            _trademarkService = trademarkService;
            _mapper = mapper;
        }


        public IQueryable<TmkConflict> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<TmkConflict> conflicts)
        {
            if (mainSearchFilters.Count > 0)
            {
                var countryOp = mainSearchFilters.GetFilterOperator("CountryOp");
                var country = mainSearchFilters.FirstOrDefault(f => f.Property == "Country");
                if (country != null)
                {
                    country.Operator = countryOp;
                    var countries = country.GetValueList();

                    if (countries.Count > 0)
                    {
                        if (country.Operator == "eq")
                            conflicts = conflicts.Where(c => countries.Contains(c.Country));
                        else
                            conflicts = conflicts.Where(c => !countries.Contains(c.Country));

                        mainSearchFilters.Remove(country);
                    }
                }

                var caseTypeOp = mainSearchFilters.GetFilterOperator("CaseTypeOp");
                var caseType = mainSearchFilters.FirstOrDefault(f => f.Property == "TmkTrademark.CaseType");
                if (caseType != null)
                {
                    caseType.Operator = caseTypeOp;
                    var caseTypes = caseType.GetValueList();

                    if (caseTypes.Count > 0)
                    {
                        if (caseType.Operator == "eq")
                            conflicts = conflicts.Where(c => caseTypes.Contains(c.TmkTrademark.CaseType));
                        else
                            conflicts = conflicts.Where(c => !caseTypes.Contains(c.TmkTrademark.CaseType));

                        mainSearchFilters.Remove(caseType);
                    }
                }

                var conflictStatus = mainSearchFilters.FirstOrDefault(f => f.Property == "ConflictStatus");
                if (conflictStatus != null)
                {                    
                    var statuses = conflictStatus.GetValueListForLoop();
                    if (statuses.Count > 0)
                    {
                        Expression<Func<TmkConflict, bool>> conflictStatusPredicate = (item) => false;
                        foreach (var val in statuses)
                        {
                            conflictStatusPredicate = conflictStatusPredicate.Or(a => EF.Functions.Like(a.ConflictStatus, val));
                        }
                        conflicts = conflicts.Where(conflictStatusPredicate);
                    }                   
                    
                    mainSearchFilters.Remove(conflictStatus);
                }

                var otherParty = mainSearchFilters.FirstOrDefault(f => f.Property == "OtherParty");
                if (otherParty != null)
                {
                    var otherParties = otherParty.GetValueListForLoop();
                    if (otherParties.Count > 0)
                    {
                        Expression<Func<TmkConflict, bool>> otherPartyPredicate = (item) => false;
                        foreach (var val in otherParties)
                        {
                            otherPartyPredicate = otherPartyPredicate.Or(a => EF.Functions.Like(a.OtherParty, val));
                        }
                        conflicts = conflicts.Where(otherPartyPredicate);
                    }                    

                    mainSearchFilters.Remove(otherParty);
                }

                if (mainSearchFilters.Any())
                    conflicts = QueryHelper.BuildCriteria<TmkConflict>(conflicts, mainSearchFilters);
            }

            return conflicts;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<TmkConflict> conflicts)
        {
            var model = conflicts.ProjectTo<TmkConflictSearchResultViewModel>();

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(c => c.CaseNumber).ThenBy(c => c.Country).ThenBy(c => c.SubCase);

            var ids = await model.Select(c => c.ConflictId).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }

        public async Task<TmkConflictDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var viewModel = new TmkConflictDetailViewModel();

            if (id > 0)
                viewModel = await _conflictService.TmkConflicts.ProjectTo<TmkConflictDetailViewModel>()
                    .SingleOrDefaultAsync(c => c.ConflictId == id);

            return viewModel;
        }

        public TmkConflict ConvertViewModelToConflict(TmkConflictDetailViewModel viewModel)
        {
            return _mapper.Map<TmkConflict>(viewModel);
        }

        public async Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<TmkConflict> conflicts, string value)
        {
            var result = await _conflictService.TmkConflicts.Where(c => c.CaseNumber == value)
               .Select(c => new CaseNumberLookupViewModel { Id = c.ConflictId, CaseNumber = c.CaseNumber }).FirstOrDefaultAsync();
            return result;
        }

        public async Task<List<TmkConflictTmkInfoViewModel>> GetTmkInfoList(string caseNumber, string country, string subCase)
        {
            var tmk = _trademarkService.TmkTrademarks.Where(t => t.CaseNumber == caseNumber);
            //tmk = tmk.Where(TmkInfoFilter(caseNumber, country, subCase));
            //if (subCase == "")
            //    tmk = tmk.Where(t => t.CaseNumber == caseNumber && t.Country == country);
            //else
            //    tmk = tmk.Where(t => t.CaseNumber == caseNumber && t.Country == country && t.SubCase == subCase);

            var tmkInfo = await tmk.ProjectTo<TmkConflictTmkInfoViewModel>().ToListAsync();
            return tmkInfo;
        }

        //private  Expression<Func<TmkTrademark, bool>> TmkInfoFilter(string caseNumber, string country, string subCase)
        //{
        //    if (subCase == "")
        //        return t => t.CaseNumber == caseNumber && t.Country == country;
        //    else
        //        return t => t.CaseNumber == caseNumber && t.Country == country && t.SubCase == subCase;
        //}


        public IQueryable<TmkConflictTmkInfoViewModel> TmkInfo => _trademarkService.TmkTrademarks.ProjectTo<TmkConflictTmkInfoViewModel>();

    }
}
