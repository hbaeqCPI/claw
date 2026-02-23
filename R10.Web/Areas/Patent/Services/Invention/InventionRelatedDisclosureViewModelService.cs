using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R9.Core.Entities.Patent;
using R9.Core.Interfaces.Patent;
using R9.Web.Areas.Patent.ViewModels;
using R9.Web.Areas.Shared.ViewModels;
using R9.Web.Extensions;
using R9.Web.Helpers;
using R9.Web.Interfaces;
using R9.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R9.Web.Areas.Patent.Services
{
    public class InventionRelatedDisclosureViewModelService : IInventionRelatedDisclosureViewModelService
    {
        private readonly IInventionRelatedDisclosureRepository _repository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public InventionRelatedDisclosureViewModelService(IInventionRelatedDisclosureRepository repository, IMapper mapper, IStringLocalizer<SharedResource> sharedLocalizer)

        {
            _repository = repository;
            _mapper = mapper;
            _sharedLocalizer = sharedLocalizer;
        }

        public IQueryable<InventionRelatedDisclosure> AddCriteria(List<QueryFilterViewModel> mainSearchFilters)
        {
            var relatedDisclosures = _repository.InventionRelatedDisclosures;
            if (mainSearchFilters != null)
            {
                var moreFilter = mainSearchFilters.ToList();
                relatedDisclosures = AddCriteria(relatedDisclosures, moreFilter);
            }
            return relatedDisclosures;
        }

        public CPiDataSourceResult CreateViewModelForGrid(DataSourceRequest request, IQueryable<InventionRelatedDisclosure> relatedDisclosures)
        {
            if (request.Sorts != null && request.Sorts.Any())
            {
                relatedDisclosures = relatedDisclosures.ApplySorting(request.Sorts);
            }
            else
            {
                relatedDisclosures = relatedDisclosures.OrderBy(k => k.KeyId);
            }
            var ids = relatedDisclosures.Select(i => i.KeyId).ToArray();
            var total = ids.Length;

            var RelatedDisclosureVM = relatedDisclosures.ProjectTo<InventionRelatedDisclosureViewModel>();
            RelatedDisclosureVM = RelatedDisclosureVM.ApplyPaging(request.Page, request.PageSize);

            var result = new CPiDataSourceResult()
            {
                Data = RelatedDisclosureVM,
                Total = total,
                Ids = ids
            };
            return result;
        }   

        public async Task<List<InventionRelatedDisclosureViewModel>> GetInventionRelatedDisclosures(int invId)
        {
            var vm = await _repository.InventionRelatedDisclosures.Where(r => r.InvId == invId).ProjectTo<InventionRelatedDisclosureViewModel>().ToListAsync();

            return vm;
        }

        private IQueryable<InventionRelatedDisclosure> AddCriteria(IQueryable<InventionRelatedDisclosure> RelatedDisclosures, List<QueryFilterViewModel> mainSearchFilters)
        {
            var filteredRelatedDisclosures = QueryHelper.BuildCriteria<InventionRelatedDisclosure>(RelatedDisclosures, mainSearchFilters);
            return filteredRelatedDisclosures;
        }
    }
}
