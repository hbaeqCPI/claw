using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Shared.ViewModels.ReportScheduler;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.Services.ReportScheduler
{
    public class RSMainViewModelService : IRSMainViewModelService
    {

        //private readonly IInventionService _inventionService;
        private readonly IRSMainService _rSMainService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly ISystemSettings<PatSetting> _settings;

        public RSMainViewModelService(
            //IInventionService inventionService,
            IMapper mapper,
            IRSMainService rSMainService, IHttpContextAccessor httpContextAccessor
            //ISystemSettings<PatSetting> settings
            )

        {
            //_inventionService = inventionService;
            _mapper = mapper;
            _rSMainService = rSMainService;
            _httpContextAccessor = httpContextAccessor;
            //_settings = settings;
        }

        public IQueryable<RSMain> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<RSMain> rSMains)
        {
            if (mainSearchFilters.Count > 0)
            {

                var name = mainSearchFilters.FirstOrDefault(f => f.Property == "Name");
                if (name != null)
                {
                    rSMains = rSMains.Where(w => EF.Functions.Like(w.Name, name.Value));
                    mainSearchFilters.Remove(name);
                }

                var description = mainSearchFilters.FirstOrDefault(f => f.Property == "Description");
                if (description != null)
                {
                    rSMains = rSMains.Where(w => EF.Functions.Like(w.Description, description.Value));
                    mainSearchFilters.Remove(description);
                }

                var reportId = mainSearchFilters.FirstOrDefault(f => f.Property == "ReportId");
                if (reportId != null)
                {
                    rSMains = rSMains.Where(w => EF.Functions.Like(w.ReportId.ToString(), reportId.Value.ToString()));
                    mainSearchFilters.Remove(reportId);
                }

                var createdBy = mainSearchFilters.FirstOrDefault(f => f.Property == "CreatedBy");
                if (createdBy != null)
                {
                    rSMains = rSMains.Where(w => EF.Functions.Like(w.CreatedBy, createdBy.Value));
                    mainSearchFilters.Remove(createdBy);
                }

                var updatedBy = mainSearchFilters.FirstOrDefault(f => f.Property == "UpdatedBy");
                if (updatedBy != null)
                {
                    rSMains = rSMains.Where(w => EF.Functions.Like(w.UpdatedBy, updatedBy.Value));
                    mainSearchFilters.Remove(updatedBy);
                }

                var dateCreatedFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DateCreatedFrom");
                var dateCreatedTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DateCreatedTo");
                if (dateCreatedFrom != null && dateCreatedTo == null)
                {
                    rSMains = rSMains.Where(a => a.DateCreated >= DateTime.Parse(dateCreatedFrom.Value));
                    mainSearchFilters.Remove(dateCreatedFrom);
                }
                else if (dateCreatedFrom == null && dateCreatedTo != null)
                {
                    rSMains = rSMains.Where(a => a.DateCreated <= DateTime.Parse(dateCreatedTo.Value));
                 mainSearchFilters.Remove(dateCreatedTo);
                }
                else if (dateCreatedFrom != null && dateCreatedTo != null)
                {
                    rSMains = rSMains.Where(a => a.DateCreated >= DateTime.Parse(dateCreatedFrom.Value) && a.DateCreated <= DateTime.Parse(dateCreatedTo.Value));
                    mainSearchFilters.Remove(dateCreatedFrom);
                    mainSearchFilters.Remove(dateCreatedTo);
                }

                var lastUpdateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "LastUpdateFrom");
                var lastUpdateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "LastUpdateTo");
                if (lastUpdateFrom != null && lastUpdateTo == null)
                {
                    rSMains = rSMains.Where(a => a.DateCreated >= DateTime.Parse(lastUpdateFrom.Value));
                    mainSearchFilters.Remove(lastUpdateFrom);
                }
                else if (lastUpdateFrom == null && lastUpdateTo != null)
                {
                    rSMains = rSMains.Where(a => a.DateCreated <= DateTime.Parse(lastUpdateTo.Value));
                    mainSearchFilters.Remove(lastUpdateTo);
                }
                else if (lastUpdateFrom != null && lastUpdateTo != null)
                {
                    rSMains = rSMains.Where(a => a.DateCreated >= DateTime.Parse(lastUpdateFrom.Value) && a.DateCreated <= DateTime.Parse(lastUpdateTo.Value));
                    mainSearchFilters.Remove(lastUpdateFrom);
                    mainSearchFilters.Remove(lastUpdateTo);
                }

                if (mainSearchFilters.Any())
                    rSMains = QueryHelper.BuildCriteria<RSMain>(rSMains, mainSearchFilters);

            }
            return rSMains;
        }

        public RSMain ConvertViewModelToRSMain(RSMainDetailViewModel viewModel)
        {
            var oldRSMain = _rSMainService.GetRSMainById(viewModel.TaskId);
            var rSMain = _mapper.Map<RSMain>(viewModel);
            rSMain.LastRunResult = oldRSMain.LastRunResult;
            rSMain.LastRunTime = oldRSMain.LastRunTime;
            rSMain.NextRunTime = oldRSMain.NextRunTime;
            rSMain.TaskCreatorId = oldRSMain.TaskCreatorId;
            rSMain.tStamp = oldRSMain.tStamp;

            if (rSMain.NextRunTime == null||rSMain.TaskStartDateTime!=oldRSMain.TaskStartDateTime)
                rSMain.NextRunTime = rSMain.TaskStartDateTime < DateTime.Now ? rSMain.TaskStartDateTime.AddDays(Math.Floor((DateTime.Now - rSMain.TaskStartDateTime).TotalDays)) : rSMain.TaskStartDateTime;
            if (rSMain.TaskCreatorId == null)
                rSMain.TaskCreatorId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
            return rSMain;
        }

        public async Task<RSMainDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var viewModel = new RSMainDetailViewModel();

            if (id > 0)
            {
                viewModel = await _rSMainService.RSMains.ProjectTo<RSMainDetailViewModel>()
                    .SingleOrDefaultAsync(i => i.TaskId == id);

                if (viewModel == null)
                    return viewModel;

            }
            else { viewModel.TaskStartDateTime = DateTime.Now;
                viewModel.IsEnabled = true;
            }

            return viewModel;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<RSMain> rSMains)
        {
            var model = rSMains;

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(i => i.Name);

            var ids = await model.Select(i => i.TaskId).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }

        public IQueryable<ScheduleNameLookupViewModel> GetScheduleNamesList(IQueryable<RSMain> rSMains, DataSourceRequest request, string textProperty, string text, FilterType filterType)
        {
            if (request.Filters?.Count > 0)
            {
                text = ((FilterDescriptor)request.Filters[0]).Value as string;
            }

            rSMains = QueryHelper.BuildCriteria(rSMains, textProperty, text, filterType);
            var result = rSMains.Select(i => new ScheduleNameLookupViewModel { Id = i.TaskId, ScheduleName = i.Name }).OrderBy(i => i.ScheduleName);
            return result;
        }

        public async Task<ScheduleNameLookupViewModel> ScheduleNameSearchValueMapper(IQueryable<RSMain> rSMains, string value)
        {
            var result = await rSMains.Where(i => i.Name == value)
                .Select(i => new ScheduleNameLookupViewModel { Id = i.TaskId, ScheduleName = i.Name }).FirstOrDefaultAsync();
            return result;
        }
    }
}
