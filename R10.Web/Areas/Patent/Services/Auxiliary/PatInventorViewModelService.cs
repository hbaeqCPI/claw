using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R9.Core.Entities.Patent;
using R9.Core.Interfaces;
using R9.Core.Interfaces.Patent;
using R9.Web.Areas.Patent.ViewModels;
using R9.Web.Areas.Shared.ViewModels;
using R9.Web.Extensions;
using R9.Web.Helpers;
using R9.Web.Interfaces;
using R9.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace R9.Web.Areas.Patent.Services
{
    public class PatInventorViewModelService : IPatInventorViewModelService
    {
        private readonly IEntityService<PatInventor> _inventorService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public PatInventorViewModelService(
            IEntityService<PatInventor> inventorService,
            IMapper mapper, 
            IStringLocalizer<SharedResource> sharedLocalizer
            )
        {
            _inventorService = inventorService;
            _mapper = mapper;
            _sharedLocalizer = sharedLocalizer;
        }


        //todo:remove
        public IQueryable<PatInventor> AddCriteria(List<QueryFilterViewModel> mainSearchFilters)
        {
            var patInventors = _inventorService.QueryableList;
            if (mainSearchFilters != null)
            {
                patInventors = AddCriteria(patInventors, mainSearchFilters);
            }
            return patInventors;
        }

        public IQueryable<PatInventor> AddCriteria(IQueryable<PatInventor> patInventors, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (mainSearchFilters.Count > 0)
            {
                patInventors = QueryHelper.BuildCriteria<PatInventor>(patInventors, mainSearchFilters);
            }
            return patInventors;
        }

        public async Task<PatInventorDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            //var patInventor = await _inventorServce.Inventors.Where(i => i.InventorID == id).Include(i => i.AddressCountry).Include(i => i.POAddressCountry).FirstOrDefaultAsync();
            //return _mapper.Map<PatInventorDetailViewModel>(patInventor);

            var patInventor = await _inventorService.QueryableList.ProjectTo<PatInventorDetailViewModel>().FirstOrDefaultAsync(a => a.InventorID == id);
            return patInventor;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<PatInventor> patInventors)
        {
            var model = patInventors.ProjectTo<PatInventorSearchResultViewModel>();

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(inventor => inventor.Inventor);

            var ids = await model.Select(c => c.InventorID).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = model.ApplyPaging(request.Page, request.PageSize),
                Total = ids.Length,
                Ids = ids
            };
        }

        public async Task<int?> GetInventorId(string inventorName)
        {
            var patInventor = await _inventorService.QueryableList.Where(i => i.Inventor == inventorName).FirstOrDefaultAsync();
            return patInventor?.InventorID;
        }

        public List<LetterOptionViewModel> GetLetterOptions()
        {
            return LetterOptionViewModel.BuildList(_sharedLocalizer);
        }

        public List<SendAsOptionViewModel> GetSendAsOptions()
        {
            return SendAsOptionViewModel.BuildList(_sharedLocalizer);
        }
    }
}
