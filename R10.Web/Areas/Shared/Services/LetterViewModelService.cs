using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.Services
{
    public class LetterViewModelService : ILetterViewModelService
    {
        private readonly ILetterService _letterService;
        private readonly IMapper _mapper;

        public LetterViewModelService(
                ILetterService letterService,
                IMapper mapper
                )
        {
            _letterService = letterService;
            _mapper = mapper;

        }

        #region Letter Setup Screen
        public IQueryable<LetterMain> AddCriteria(IQueryable<LetterMain> letters, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (mainSearchFilters.Count > 0)
            {
                var systemType = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemType");
                if (systemType != null)
                {
                    letters = letters.Where(l => l.SystemScreen.SystemType == systemType.Value);
                    mainSearchFilters.Remove(systemType);

                    
                }

                var tag = mainSearchFilters.FirstOrDefault(f => f.Property == "Tag");
                if (tag != null)
                {
                    var tagsList = "";
                    if (tag != null)
                    {
                        var tags = tag.GetValueListForLoop();
                        if (tags.Count > 1)
                        {
                            foreach (var val in tags)
                            {
                                tagsList = tagsList + val + "~";
                            }
                        }
                    }

                    letters = letters.Where(l => tag == null
                                                    || (string.IsNullOrEmpty(tagsList)
                                                        && l.LetterTags.Any(t => EF.Functions.Like(t.Tag, tag.Value)))
                                                    || (!string.IsNullOrEmpty(tagsList)
                                                        && l.LetterTags.Any(t => EF.Functions.Like(tagsList, '%' + t.Tag + '%')))
                                            );

                    mainSearchFilters.Remove(tag);
                }
            }
            if (mainSearchFilters.Any())
                letters = QueryHelper.BuildCriteria(letters, mainSearchFilters);

            return letters;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForSearchGrid(DataSourceRequest request, IQueryable<LetterMain> lettersMain)
        {
            var model = lettersMain.ProjectTo<LetterSearchResultViewModel>();
            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(l => l.LetName);

            //var ids = await model.Select(l => l.LetId).ToArrayAsync();
            var recCount = await model.Select(l => l.LetId).CountAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = recCount
                //Ids = ids
            };

        }

        public async Task<LetterMainDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var viewModel = new LetterMainDetailViewModel();

            if (id > 0)
            {
                viewModel = await _letterService.LettersMain.ProjectTo<LetterMainDetailViewModel>()
                                .SingleOrDefaultAsync(l => l.LetId == id);
            }
            return viewModel;
        }
        #endregion

        #region Letter Popup Screen
        public async Task<CPiDataSourceResult> CreateViewModelForLetterGrid(DataSourceRequest request, string systemType, string screenCode, string? letterName, int? letCatId, int? letSubCatId, List<string>? tags = null)
        {
            var tagsList = "";
            if (tags != null && tags.Count > 1)
            {
                foreach (var val in tags)
                {
                    tagsList = tagsList + val + "~";
                }
            }
            var model = _letterService.LettersMain.Where(l => l.SystemScreen.SystemType == systemType
                                                            && l.SystemScreen.ScreenCode == screenCode
                                                            && (string.IsNullOrEmpty(letterName) || l.LetName.Contains(letterName))
                                                            && (letCatId == null || l.LetCatId == letCatId)
                                                            && (letSubCatId == null || l.LetSubCatId == letSubCatId)
                                                            //&& (string.IsNullOrEmpty(tag) || l.LetterTags.Any(t => EF.Functions.Like(t.Tag, tag)))
                                                            && (tags == null || tags.Count() == 0
                                                                             || (string.IsNullOrEmpty(tagsList) && l.LetterTags.Any(t => EF.Functions.Like(t.Tag, tags.FirstOrDefault())))
                                                                             || (!string.IsNullOrEmpty(tagsList) && l.LetterTags.Any(t => EF.Functions.Like(tagsList, '%' + t.Tag + '%')))
                                                               )
                                                            && l.LetterRecordSources.Any()
                                                            )
                                .ProjectTo<LetterListViewModel>();
            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(l => l.LetName);

            var ids = await model.Select(l => l.LetId).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };

        }
        #endregion


    }
}
