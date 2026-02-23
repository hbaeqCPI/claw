using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using R9.Core.Entities;
using R9.Core.Interfaces;
using R9.Web.Areas.Shared.ViewModels;
using R9.Web.Extensions;
using R9.Web.Helpers;
using R9.Web.Interfaces;

namespace R9.Web.Services
{
    public class LetterViewModelServiceOld : ILetterViewModelServiceOld
    {
        private readonly ILetterOldService _letterService;
        private readonly IMapper _mapper;

        public LetterViewModelServiceOld(ILetterOldService letterService, IMapper mapper)
        {
            _letterService = letterService;
            _mapper = mapper;
        }

        #region Letter Main/List & Criteria

        public IQueryable<LetterMain> AddCriteria(List<QueryFilterViewModel> mainSearchFilters)
        {
            var letters = _letterService.Letters;
            if (mainSearchFilters != null)
            {
                var moreFilter = mainSearchFilters.ToList();
                letters = AddCriteria(letters, moreFilter);
            }
            return letters;
        }

        private IQueryable<LetterMain> AddCriteria(IQueryable<LetterMain> letters, List<QueryFilterViewModel> mainSearchFilters)
        {
            var filteredInventions = QueryHelper.BuildCriteria<LetterMain>(letters, mainSearchFilters);
            return filteredInventions;
        }

        public DataSourceResult CreateViewModelForLetterGrid(DataSourceRequest request, IQueryable<LetterMain> letters)
        {
            //filters = filters.Include(s => s.SystemScreen).Include(c => c.LetterCategory);
            //filters = filters.Include(s => s.SystemScreen);
            if (request.Sorts != null && request.Sorts.Any())
            {
                letters = letters.ApplySorting(request.Sorts);
            }
            else
            {
                letters = letters.OrderBy(letmain => letmain.LetName);
            }

            var total = letters.Count();

            var lettersVM = letters.ProjectTo<LetterMainDetailViewModel>();
            lettersVM = lettersVM.ApplyPaging(request.Page, request.PageSize);

            var result = new DataSourceResult()
            {
                Data = lettersVM,
                Total = total
            };
            return result;
        }

        //public LetterMain MapViewModelToDomain(LetterMainViewModel letterMainVM)
        //{
        //    var letterMain = _mapper.Map<LetterMain>(letterMainVM);
        //    return letterMain;
        //}

        //public async Task<LetterMainViewModel> CreateViewModelForLetterDetail(int id)
        //{
        //    var letter = await _letterService.Letters.Where(l => l.LetId == id).Include(s => s.SystemScreen)
        //        .Include(c => c.LetterCategory).FirstOrDefaultAsync();
        //    return _mapper.Map<LetterMainViewModel>(letter);
        //}

        #endregion

        //public async Task<List<LetterRecordSourceFilterViewModel>> CreateViewModelForLetterFilter(int id)
        //{
        //    var filter = await _letterService.LetterFilters.Include(rs => rs.LetterRecordSource)
        //                        .Where(rs => rs.LetterRecordSource.LetId == id).ToListAsync();
        //    return _mapper.Map<List<LetterRecordSourceFilterViewModel>>(filter);
        //}

        //public DataSourceResult CreateViewModelForLetterFilter(DataSourceRequest request, IQueryable<LetterRecordSourceFilter> filters)
        //{
        //    if (request.Sorts != null && request.Sorts.Any())
        //    {
        //        filters = filters.ApplySorting(request.Sorts);
        //    }
        //    else
        //    {
        //        filters = filters.OrderBy(filter => filter.LetFilterId);
        //    }

        //    var total = filters.Count();

        //    var filterVM = filters.ProjectTo<LetterRecordSourceFilterViewModel>();
        //    filterVM = filterVM.ApplyPaging(request.Page, request.PageSize);

        //    var result = new DataSourceResult()
        //    {
        //        Data = filterVM,
        //        Total = total
        //    };
        //    return result;
        //}
    }
}
