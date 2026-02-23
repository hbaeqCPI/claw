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
    public class DOCXViewModelService : IDOCXViewModelService
    {
        private readonly IDOCXService _docxService;
        private readonly IMapper _mapper;

        public DOCXViewModelService(
                IDOCXService docxService,
                IMapper mapper
                )
        {
            _docxService = docxService;
            _mapper = mapper;

        }

        #region DOCX Setup Screen
        public IQueryable<DOCXMain> AddCriteria(IQueryable<DOCXMain> docxes, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (mainSearchFilters.Count > 0)
            {
                var systemType = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemType");
                if (systemType != null)
                {
                    docxes = docxes.Where(l => l.SystemScreen.SystemType == systemType.Value);
                    mainSearchFilters.Remove(systemType);
                }
            }
            if (mainSearchFilters.Any())
                docxes = QueryHelper.BuildCriteria(docxes, mainSearchFilters);

            return docxes;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForSearchGrid(DataSourceRequest request, IQueryable<DOCXMain> docxesMain)
        {
            var model = docxesMain.ProjectTo<DOCXSearchResultViewModel>();
            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(l => l.DOCXName);

            //var ids = await model.Select(l => l.DOCXId).ToArrayAsync();
            var recCount = await model.Select(l => l.DOCXId).CountAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = recCount
                //Ids = ids
            };

        }

        public async Task<DOCXMainDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var viewModel = new DOCXMainDetailViewModel();

            if (id > 0)
            {
                viewModel = await _docxService.DOCXesMain.ProjectTo<DOCXMainDetailViewModel>()
                                .SingleOrDefaultAsync(l => l.DOCXId == id);
            }
            return viewModel;
        }
        #endregion

        #region DOCX Popup Screen
        public async Task<CPiDataSourceResult> CreateViewModelForDOCXGrid(DataSourceRequest request, string systemType, string screenCode,string? docxName)
        {
            var model = _docxService.DOCXesMain.Where(l => l.SystemScreen.SystemType == systemType && l.SystemScreen.ScreenCode == screenCode && (string.IsNullOrEmpty(docxName) || l.DOCXName.Contains(docxName)) && l.DOCXRecordSources.Any())
                                .ProjectTo<DOCXListViewModel>();
            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(l => l.DOCXName);

            var ids = await model.Select(l => l.DOCXId).ToArrayAsync();

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
