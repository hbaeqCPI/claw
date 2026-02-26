using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Interfaces;
using R10.Web.Helpers;
using R10.Web.Extensions;
using R10.Web.Security;
using Kendo.Mvc.UI;
using R10.Web.Areas.Shared.ViewModels;
using R10.Core.Services.Shared;
using R10.Web.Extensions.ActionResults;
using R10.Core.Entities;
using R10.Web.Interfaces;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using R10.Core.Identity;
using System.Linq.Expressions;
using R10.Web.Models;
using Microsoft.Extensions.Localization;
using R10.Web.Models.PageViewModels;
using R10.Core.Helpers;
using R10.Core;
using R10.Core.Exceptions;
using R10.Web.Services;
using AutoMapper;
using R10.Core.Interfaces.Patent;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Entities.Shared;
using R10.Web.Interfaces.Shared;
using Newtonsoft.Json;
using Kendo.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.SharePoint.Client.Publishing;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    //[Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    [Area("Shared"), Authorize]
    public class TimeTrackerController : BaseController
    {
        private readonly UserManager<CPiUser> _userManager;
        private readonly IApplicationDbContext _repository;
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<Attorney> _attorneyViewModelService;
        private readonly IAttorneyService _attorneyService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ICountryLookupViewModelService _countryLookupService;
        private readonly IReportService _reportService;
        private readonly IParentEntityService<Attorney, TimeTracker> _attorneyTimeTrackerService;
        private readonly IMapper _mapper;
        protected readonly IInventionService _inventionService;
        protected readonly ICountryApplicationService _applicationService;
        protected readonly ITmkTrademarkService _trademarkService;
        private readonly IConfiguration _configuration;
        private readonly IEntityService<TimeTrack> _timeTrackEntityService;
        private readonly ITimeTrackerService _timeTrackerService;
        protected readonly ICostTrackingService<PatCostTrack> _PatCostTrackingService;
        protected readonly ICostTrackingService<TmkCostTrack> _TmkCostTrackingService;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly ExportHelper _exportHelper;
        private readonly IStringLocalizer<TimeTrackerExportToExcelViewModel> _searchResultLocalizer;

        public TimeTrackerController(
            UserManager<CPiUser> userManager,
            IApplicationDbContext repository,
            IAuthorizationService authService,
            IViewModelService<Attorney> attorneyViewModelService,
            IAttorneyService attorneyService,
            IStringLocalizer<SharedResource> localizer,
            ICountryLookupViewModelService countryLookupService,
            IReportService reportService,
            IParentEntityService<Attorney, TimeTracker> attorneyTimeTrackerService,
            IMapper mapper,
            IInventionService inventionService,
            ICountryApplicationService applicationService,
            ITmkTrademarkService trademarkService,
            IConfiguration configuration,
            IEntityService<TimeTrack> timeTrackEntityService,
            ITimeTrackerService timeTrackerService,
            ICostTrackingService<PatCostTrack> PatCostTrackingService,
            ICostTrackingService<TmkCostTrack> TmkCostTrackingService,
            ISystemSettings<DefaultSetting> settings,
            ExportHelper exportHelper,
            IStringLocalizer<TimeTrackerExportToExcelViewModel> searchResultLocalizer)
        {
            _userManager = userManager;
            _repository = repository;
            _authService = authService;
            _attorneyViewModelService = attorneyViewModelService;
            _attorneyService = attorneyService;
            _localizer = localizer;
            _countryLookupService = countryLookupService;
            _reportService = reportService;
            _attorneyTimeTrackerService = attorneyTimeTrackerService;
            _mapper = mapper;
            _inventionService = inventionService;
            _applicationService = applicationService;
            _trademarkService = trademarkService;
            _timeTrackEntityService = timeTrackEntityService;
            _timeTrackerService = timeTrackerService;
            _configuration = configuration;
            _PatCostTrackingService = PatCostTrackingService;
            _TmkCostTrackingService = TmkCostTrackingService;
            _settings = settings;
            _exportHelper = exportHelper;
            _searchResultLocalizer = searchResultLocalizer;
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> TimeTrackersDelete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] TimeTrackerViewModel deleted)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.TimeTrackerId >= 0)
            {
                var deletedTimeTracker = _attorneyTimeTrackerService.ChildService.QueryableList.FirstOrDefault(c=>c.TimeTrackerId==deleted.TimeTrackerId);
                if (deletedTimeTracker.AppId != null && deletedTimeTracker.Exported)
                {
                    await _PatCostTrackingService.Delete(_PatCostTrackingService.QueryableList.FirstOrDefault(c => c.CostTrackId == deletedTimeTracker.CostTrackId));
                }
                else if (deletedTimeTracker.TmkId != null && deletedTimeTracker.Exported)
                {
                    await _TmkCostTrackingService.Delete(_TmkCostTrackingService.QueryableList.FirstOrDefault(c => c.CostTrackId == deletedTimeTracker.CostTrackId));
                }
                if (string.IsNullOrEmpty(deletedTimeTracker.TrackUserId))
                    await _attorneyTimeTrackerService.ChildService.Update((int)deletedTimeTracker.AttorneyID, User.GetUserName(), new List<TimeTracker>(), new List<TimeTracker>(), new List<TimeTracker>() { deletedTimeTracker });
                else
                    await _attorneyTimeTrackerService.ChildService.Delete(deletedTimeTracker);
                return Ok(new { success = _localizer["Time Tracker has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var attorney = await _attorneyService.GetByIdAsync(id);
            if (attorney == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = attorney.CreatedBy, dateCreated = attorney.DateCreated, updatedBy = attorney.UpdatedBy, lastUpdate = attorney.LastUpdate, tStamp = attorney.tStamp });
        }

        protected IQueryable<Attorney> Attorneys => _attorneyService.QueryableList;

        private async Task<bool> CanAddRecord()
        {
            //do not allow add if user entity filter type is attorney
            //user won't be able to access new record
            return (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded &&
                User.GetEntityFilterType() != CPiEntityType.Attorney;
        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
        public IActionResult TimeTrackersRead([DataSourceRequest] DataSourceRequest request, TimeTrackerSearchViewModel search)
        {
            if (!string.IsNullOrEmpty(search.SearchUserId))
            {
                var user = _userManager.Users.FirstOrDefault(c=>c.Id == search.SearchUserId);
                if(user.UserType == CPiUserType.Attorney)
                {
                    var entityFilter = _repository.CPiUserEntityFilters.Where(c=>c.UserId == user.Id).AsNoTracking().FirstOrDefault();
                    if(entityFilter != null)
                        search.SearchAttorneyId = entityFilter.EntityId;
                }
            }
            var result = _attorneyTimeTrackerService.ChildService.QueryableList.ProjectTo<TimeTrackerViewModel>()
                    .Where(c =>
                    (search.SearchAttorneyId != 0 ? c.AttorneyID == search.SearchAttorneyId : c.TrackUserId == search.SearchUserId) && 
                    (search.SearchOutstandingOnly.Equals("2") || (search.SearchOutstandingOnly.Equals("1") && c.Exported) || (search.SearchOutstandingOnly.Equals("0") && !c.Exported)) &&
                    (search.EntryDateFrom == null || c.EntryDate.Date >= search.EntryDateFrom) &&
                    (search.EntryDateTo == null || c.EntryDate.Date <= search.EntryDateTo) && 
                    (string.IsNullOrEmpty(search.SearchSystemType) || search.SearchSystemType.Equals(c.SystemType)) &&
                    (string.IsNullOrEmpty(search.SearchCaseNumber) || search.SearchCaseNumber.Equals(c.CaseNumber)) &&
                    (string.IsNullOrEmpty(search.SearchCountry) || search.SearchCountry.Equals(c.Country)) &&
                    (string.IsNullOrEmpty(search.SearchSubCase) || search.SearchSubCase.Equals(c.SubCase)) &&
                    (string.IsNullOrEmpty(search.SearchClientCode) || search.SearchClientCode.Equals(c.TimeTrackerClientCode))
                    ).OrderByDescending(c=>c.EntryDate).ToDataSourceResult(request);
            return Json(result);
        }


        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> TimeTrackersUpdate(int attorneyId,
            [Bind(Prefix = "updated")] IEnumerable<TimeTrackerViewModel> updated,
            [Bind(Prefix = "new")] IEnumerable<TimeTrackerViewModel> added,
            [Bind(Prefix = "deleted")] IEnumerable<TimeTrackerViewModel> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });
                List<TimeTracker> updatedTimeTrackers = new List<TimeTracker>();
                List<TimeTracker> addedTimeTrackers = new List<TimeTracker>();
                List<TimeTracker> deletedTimeTrackers = new List<TimeTracker>();

                if (updated.Count() > 0)
                {
                    foreach (TimeTrackerViewModel viewModel in updated)
                    {
                        updatedTimeTrackers.Add(TimeTrackerViewModelToTimeTracker(viewModel, false));
                    }
                }

                if (added.Count() > 0)
                {
                    foreach (TimeTrackerViewModel viewModel in added)
                    {
                        try
                        {
                            addedTimeTrackers.Add(TimeTrackerViewModelToTimeTracker(viewModel, true));
                        }
                        catch (Exception e)
                        {
                            return new RecordDoesNotExistResult();
                        }
                    }
                }

                if (deleted.Count() > 0)
                {
                    deletedTimeTrackers = _attorneyTimeTrackerService.ChildService.QueryableList.Where(c => deleted.Any(d => d.TimeTrackerId == c.TimeTrackerId)).ToList();
                }

                await _attorneyTimeTrackerService.ChildService.Update(attorneyId, User.GetUserName(),
                    updatedTimeTrackers,
                    addedTimeTrackers,
                    deletedTimeTrackers
                    );
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer["Time Tracker has been saved successfully."].ToString() :
                    _localizer["Time Trackers have been saved successfully"].ToString();

                if (updated.Count() > 0)
                {
                    UpdateCostTracking(updated);
                }

                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> TimeTrackersUpdateByUserId(string id,
            [Bind(Prefix = "updated")] IEnumerable<TimeTrackerViewModel> updated,
            [Bind(Prefix = "new")] IEnumerable<TimeTrackerViewModel> added,
            [Bind(Prefix = "deleted")] IEnumerable<TimeTrackerViewModel> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });
                List<TimeTracker> updatedTimeTrackers = new List<TimeTracker>();
                List<TimeTracker> addedTimeTrackers = new List<TimeTracker>();
                List<TimeTracker> deletedTimeTrackers = new List<TimeTracker>();

                if (updated.Count() > 0)
                {
                    foreach (TimeTrackerViewModel viewModel in updated)
                    {
                        updatedTimeTrackers.Add(TimeTrackerViewModelToTimeTracker(viewModel, false));
                    }
                }

                if (added.Count() > 0)
                {
                    foreach (TimeTrackerViewModel viewModel in added)
                    {
                        try
                        {
                            var user = _userManager.Users.FirstOrDefault(c => c.Id == id);
                            if (user.UserType == CPiUserType.Attorney)
                            {
                                var entityFilter = _repository.CPiUserEntityFilters.Where(c => c.UserId == user.Id).AsNoTracking().FirstOrDefault();
                                if (entityFilter != null)
                                    viewModel.AttorneyID = entityFilter.EntityId;
                                else
                                    viewModel.TrackUserId = id;
                            }
                            else
                            {
                                viewModel.TrackUserId = id;
                            }
                            addedTimeTrackers.Add(TimeTrackerViewModelToTimeTracker(viewModel, true));
                        }
                        catch (Exception e)
                        {
                            return new RecordDoesNotExistResult();
                        }
                    }
                }

                if (deleted.Count() > 0)
                {
                    deletedTimeTrackers = _attorneyTimeTrackerService.ChildService.QueryableList.Where(c => deleted.Any(d => d.TimeTrackerId == c.TimeTrackerId)).ToList();
                }
                await _attorneyTimeTrackerService.ChildService.Update(updatedTimeTrackers);
                await _attorneyTimeTrackerService.ChildService.Add(addedTimeTrackers);
                await _attorneyTimeTrackerService.ChildService.Update(deletedTimeTrackers);
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer["Time Tracker has been saved successfully."].ToString() :
                    _localizer["Time Trackers have been saved successfully"].ToString();

                if (updated.Count() > 0)
                {
                    UpdateCostTracking(updated);
                }

                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> TimeTrackersUpdateDelete([Bind(Prefix = "deleted")] TimeTrackerViewModel deleted)
        {
            if (deleted.TimeTrackerId >= 0)
            {
                var deletedTimeTracker = _attorneyTimeTrackerService.ChildService.QueryableList.FirstOrDefault(c => c.TimeTrackerId == deleted.TimeTrackerId);
                if (deletedTimeTracker.AppId != null && deletedTimeTracker.Exported)
                {
                    await _PatCostTrackingService.Delete(_PatCostTrackingService.QueryableList.FirstOrDefault(c => c.CostTrackId == deletedTimeTracker.CostTrackId));
                }
                else if (deletedTimeTracker.TmkId != null && deletedTimeTracker.Exported)
                {
                    await _TmkCostTrackingService.Delete(_TmkCostTrackingService.QueryableList.FirstOrDefault(c => c.CostTrackId == deletedTimeTracker.CostTrackId));
                }
                await _attorneyTimeTrackerService.ChildService.Delete(_mapper.Map<TimeTracker>(deleted));
                return Ok(new { success = _localizer["Time Tracker has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        private TimeTracker TimeTrackerViewModelToTimeTracker(TimeTrackerViewModel viewModel, bool added)
        {
            TimeTracker timeTracker;
            if (added)
            {
                if(string.IsNullOrEmpty(viewModel.TrackUserId))
                    timeTracker = new TimeTracker()
                    {
                        AttorneyID = viewModel.AttorneyID,
                        SystemType = viewModel.SystemType.Equals("Patent") ? "P" : (viewModel.SystemType.Equals("Trademark") ? "T" : "G"),
                        Duration = viewModel.Duration,
                        EntryDate = viewModel.EntryDate,
                        Description = viewModel.Description,
                        Exported = false,
                        ExportedDate = null,
                        CreatedBy = User.GetUserName(),        
                        UpdatedBy = User.GetUserName(),
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now
            };
                else
                    timeTracker = new TimeTracker()
                    {
                        TrackUserId = viewModel.TrackUserId,
                        SystemType = viewModel.SystemType.Equals("Patent") ? "P" : (viewModel.SystemType.Equals("Trademark") ? "T" : "G"),
                        Duration = viewModel.Duration,
                        EntryDate = viewModel.EntryDate,
                        Description = viewModel.Description,
                        Exported = false,
                        ExportedDate = null,
                        CreatedBy = User.GetUserName(),
                        UpdatedBy = User.GetUserName(),
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now
                    };
                if (viewModel.SystemType.Equals("Patent"))
                {
                    timeTracker.AppId = _applicationService.CountryApplications.FirstOrDefault(c => c.CaseNumber.Equals(viewModel.CaseNumber) && c.Country.Equals(viewModel.Country) && (viewModel.SubCase == null || c.SubCase.Equals(viewModel.SubCase))).AppId;
                }
                else if (viewModel.SystemType.Equals("Trademark"))
                {
                    timeTracker.TmkId = _trademarkService.TmkTrademarks.FirstOrDefault(c => c.CaseNumber.Equals(viewModel.CaseNumber) && c.Country.Equals(viewModel.Country) && (viewModel.SubCase == null || c.SubCase.Equals(viewModel.SubCase))).TmkId;
                }

            }
            else
            {
                timeTracker = _attorneyTimeTrackerService.ChildService.QueryableList.FirstOrDefault(c => c.TimeTrackerId == viewModel.TimeTrackerId);
                if (timeTracker != null)
                {
                    timeTracker.Duration = viewModel.Duration;
                    timeTracker.EntryDate = viewModel.EntryDate;
                    timeTracker.Description = viewModel.Description;
                    timeTracker.UpdatedBy = User.GetUserName();
                    timeTracker.LastUpdate = DateTime.Now;
                }
            }
            return timeTracker;
        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
        public IActionResult GetSystemTypeList()
        {
            var list = new List<SystemTypeForTimeTracker>();
            if (User.IsInSystem(SystemType.Patent))
            {
                var sysType = new SystemTypeForTimeTracker() { SystemType = "Patent" };
                list.Add(sysType);
            }
            if (User.IsInSystem(SystemType.Trademark))
            {
                var sysType = new SystemTypeForTimeTracker() { SystemType = "Trademark" };
                list.Add(sysType);
            }
            return Json(list);
        }

        private class SystemTypeForTimeTracker
        {
            public string SystemType { get; set; }
        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetTimeTrackerCaseNumberList([DataSourceRequest] DataSourceRequest request, string systemType, string filter)
        {
            if (string.IsNullOrEmpty(systemType))
            {
                var result = new List<object>();
                return Json(result);
            }

            string text = filter;
            if (!string.IsNullOrEmpty(text))
                text = text.Substring(text.IndexOf("'")+1, text.LastIndexOf("'") - text.IndexOf("'")-1);

            if (systemType.Equals("Trademark"))
            {
                var result = _trademarkService.TmkTrademarks.Where(c => text == null || c.CaseNumber.StartsWith(text)).Select(c => new { CaseNumber = c.CaseNumber }).Distinct().ToList();

                if (request.PageSize > 0)
                {
                    request.Filters.Clear();
                    return Json(await result.ToDataSourceResultAsync(request));
                }

                var list = result;
                return Json(list);
            }
            else
            {
                var result = _applicationService.CountryApplications.Where(c => text == null || c.CaseNumber.StartsWith(text)).Select(c => new { CaseNumber = c.CaseNumber }).Distinct().ToList();

                if (request.PageSize > 0)
                {
                    request.Filters.Clear();
                    return Json(await result.ToDataSourceResultAsync(request));
                }

                var list = result;
                return Json(list);
            }
        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetSearchCaseNumberList([DataSourceRequest] DataSourceRequest request, TimeTrackerSearchViewModel viewModel, string text)
        {
            var result = await _attorneyTimeTrackerService.ChildService.QueryableList.ProjectTo<TimeTrackerViewModel>().Where(c=>text==null||c.CaseNumber.StartsWith(text)).Select(c=> new { CaseNumber = c.CaseNumber }).Distinct().OrderBy(c => c.CaseNumber).ToListAsync();
            
            if (request.PageSize > 0)
            {
                request.Filters.Clear();
                return Json(await result.ToDataSourceResultAsync(request));
            }

            var list = result;
            return Json(list);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetSearchCountryList([DataSourceRequest] DataSourceRequest request, TimeTrackerSearchViewModel viewModel, string text)
        {
            var result = await _attorneyTimeTrackerService.ChildService.QueryableList.ProjectTo<TimeTrackerViewModel>().Where(c => (text == null || c.Country.StartsWith(text)) && !string.IsNullOrEmpty(c.Country)).Select(c => new { Country = c.Country }).Distinct().OrderBy(c => c.Country).ToListAsync();
            return Json(result);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetSearchSubCaseList([DataSourceRequest] DataSourceRequest request, TimeTrackerSearchViewModel viewModel, string text)
        {
            var result = await _attorneyTimeTrackerService.ChildService.QueryableList.ProjectTo<TimeTrackerViewModel>().Where(c => (text == null || c.SubCase.StartsWith(text)) && !string.IsNullOrEmpty(c.SubCase)).Select(c => new { SubCase = c.SubCase }).Distinct().OrderBy(c => c.SubCase).ToListAsync();
            return Json(result);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetSearchClientCodeList([DataSourceRequest] DataSourceRequest request, TimeTrackerSearchViewModel viewModel, string text)
        {
            var result = await _attorneyTimeTrackerService.ChildService.QueryableList.ProjectTo<TimeTrackerViewModel>().Where(c => (text == null || c.TimeTrackerClientCode.StartsWith(text)) && !string.IsNullOrEmpty(c.TimeTrackerClientCode)).Select(c => new { ClientCode = c.TimeTrackerClientCode }).Distinct().OrderBy(c => c.ClientCode).ToListAsync();
            return Json(result);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetTimeTrackerCountryList([DataSourceRequest] DataSourceRequest request, string systemType, string caseNumber, string filter)
        {
            if (string.IsNullOrEmpty(systemType))
            {
                var result = new List<object>();
                return Json(result);
            }

            string text = filter;
            if (!string.IsNullOrEmpty(text))
                text = text.Substring(text.IndexOf("'") + 1, text.LastIndexOf("'") - text.IndexOf("'") - 1);

            if (systemType.Equals("Trademark"))
            {
                var result = _trademarkService.TmkTrademarks.Where(c => c.CaseNumber.Equals(caseNumber) && (text == null || c.Country.StartsWith(text))).Select(c => new { Country = c.Country }).Distinct().ToList();

                if (request.PageSize > 0)
                {
                    request.Filters.Clear();
                    return Json(await result.ToDataSourceResultAsync(request));
                }

                var list = result;
                return Json(list);
            }
            else
            {
                var result = _applicationService.CountryApplications.Where(c => c.CaseNumber.Equals(caseNumber) && (text == null || c.Country.StartsWith(text))).Select(c => new { Country = c.Country }).Distinct().ToList();

                if (request.PageSize > 0)
                {
                    request.Filters.Clear();
                    return Json(await result.ToDataSourceResultAsync(request));
                }

                var list = result;
                return Json(list);
            }
        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> GetTimeTrackerSubCaseList([DataSourceRequest] DataSourceRequest request, string systemType, string caseNumber, string country, string filter)
        {
            if (string.IsNullOrEmpty(systemType))
            {
                var result = new List<object>();
                return Json(result);
            }

            string text = filter;
            if (!string.IsNullOrEmpty(text))
                text = text.Substring(text.IndexOf("'") + 1, text.LastIndexOf("'") - text.IndexOf("'") - 1);

            if (systemType.Equals("Trademark"))
            {
                var result = _trademarkService.TmkTrademarks.Where(c => c.CaseNumber.Equals(caseNumber) && c.Country.Equals(country) && (text == null || c.SubCase.StartsWith(text))).Select(c => new { SubCase = c.SubCase }).Distinct().ToList();

                if (request.PageSize > 0)
                {
                    request.Filters.Clear();
                    return Json(await result.ToDataSourceResultAsync(request));
                }

                var list = result;
                return Json(list);
            }
            else
            {
                var result = _applicationService.CountryApplications.Where(c => c.CaseNumber.Equals(caseNumber) && c.Country.Equals(country) && (text == null || c.SubCase.StartsWith(text))).Select(c => new { SubCase = c.SubCase }).Distinct().ToList();

                if (request.PageSize > 0)
                {
                    request.Filters.Clear();
                    return Json(await result.ToDataSourceResultAsync(request));
                }

                var list = result;
                return Json(list);
            }
        }

        [HttpPost(), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        public async Task<IActionResult> StartTimeTrack(int id, string systemType)
        {
            var attorneys = await _timeTrackerService.GetStartTimeTrackAttorneys(id, systemType);
            var viewModel = new TimeTrackerPageViewModel()
            {
                Id = id,
                SystemType = systemType,
                Attorneys = attorneys
            };
            return PartialView(viewModel);
        }

        [HttpPost(), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
        public async Task<IActionResult> StartTimeTrackPost([FromBody] TimeTrackerPageViewModel viewModel)
        {
            if(viewModel.AttorneyIds == null)
                return BadRequest(_localizer["Please select attorney(s) to track."].ToString());
            var attorneyIds = viewModel.AttorneyIds.Split("|");
            if(attorneyIds.Length == 0)
                return BadRequest(_localizer["Please select attorney(s) to track."].ToString());
            await _timeTrackerService.StartTimeTrack(viewModel.Id, viewModel.SystemType, attorneyIds);
            return Ok(new { success = _localizer["The time tracker has been started for this record"].ToString() });
        }

        public async Task<IActionResult> StopTimeTrack()
        {
            var timeTrackInfo = await _timeTrackerService.StopTimeTrack();
            if (timeTrackInfo == null)
            {
                return BadRequest(_localizer["time track can not be found."].ToString());
            }
            return Ok(new { success = _localizer["The time tracker has been stopped for"].ToString() + " " + timeTrackInfo });
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
        public IActionResult ExportToCost(TimeTrackerSearchViewModel data)
        {
            if (!string.IsNullOrEmpty(data.SearchUserId))
            {
                var user = _userManager.Users.FirstOrDefault(c => c.Id == data.SearchUserId);
                if (user.UserType == CPiUserType.Attorney)
                {
                    var entityFilter = _repository.CPiUserEntityFilters.Where(c => c.UserId == user.Id).AsNoTracking().FirstOrDefault();
                    if (entityFilter != null)
                        data.SearchAttorneyId = entityFilter.EntityId;
                }
            }
            var timeTrackers = _attorneyTimeTrackerService.ChildService.QueryableList.ProjectTo<TimeTrackerViewModel>()
                    .Where(c =>
                    (data.SearchAttorneyId != 0 ? c.AttorneyID == data.SearchAttorneyId : c.TrackUserId == data.SearchUserId) && 
                    (data.SearchOutstandingOnly.Equals("2")||(data.SearchOutstandingOnly.Equals("1") && c.Exported)||(data.SearchOutstandingOnly.Equals("0") && !c.Exported)) &&
                    (data.EntryDateFrom == null || c.EntryDate.Date >= data.EntryDateFrom) &&
                    (data.EntryDateTo == null || c.EntryDate.Date <= data.EntryDateTo) &&
                    (string.IsNullOrEmpty(data.SearchSystemType) || data.SearchSystemType.Equals(c.SystemType)) &&
                    (string.IsNullOrEmpty(data.SearchCaseNumber) || data.SearchCaseNumber.Equals(c.CaseNumber)) &&
                    (string.IsNullOrEmpty(data.SearchCountry) || data.SearchCountry.Equals(c.Country)) &&
                    (string.IsNullOrEmpty(data.SearchSubCase) || data.SearchSubCase.Equals(c.SubCase)) &&
                    (string.IsNullOrEmpty(data.SearchClientCode) || data.SearchClientCode.Equals(c.TimeTrackerClientCode))
                    );
            var ids = "";
            foreach(TimeTrackerViewModel model in timeTrackers)
            {
                ids += model.TimeTrackerId + "|";
            }
            if (!ids.Equals(""))
            {
                ids = ids.Substring(0, ids.Length - 1);
            }
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "procWebSysTimeTrackerExportToCost";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;

                    SqlParameter param = new SqlParameter();
                    param.ParameterName = "@TimeTrackerIds";
                    param.Value = ids;
                    cmd.Parameters.Add(param);

                    SqlParameter param2 = new SqlParameter();
                    param2.ParameterName = "@UserID";
                    param2.Value = User.GetUserName();
                    cmd.Parameters.Add(param2);

                    sqlConnection.Open();

                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { success = _localizer["Data had been exported to Cost Tracking."].ToString() });
        }

        private IActionResult UpdateCostTracking(IEnumerable<TimeTrackerViewModel> data)
        {
            var ids = "";
            foreach (TimeTrackerViewModel model in data)
            {
                ids += model.TimeTrackerId + "|";
            }
            if (!ids.Equals(""))
            {
                ids = ids.Substring(0, ids.Length - 1);
            }
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "procWebSysTimeTrackerUpdateCost";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;

                    SqlParameter param = new SqlParameter();
                    param.ParameterName = "@TimeTrackerIds";
                    param.Value = ids;
                    cmd.Parameters.Add(param);

                    SqlParameter param2 = new SqlParameter();
                    param2.ParameterName = "@UserID";
                    param2.Value = User.GetUserName();
                    cmd.Parameters.Add(param2);

                    sqlConnection.Open();

                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { success = _localizer["Data had been exported to Cost Tracking."].ToString() });
        }

        [HttpPost()]
        public async Task<IActionResult> ExportToExcel(string mainSearchFiltersJSON, string sortField, string sortDirection)
        {
            var mainSearchFilters = JsonConvert.DeserializeObject<TimeTrackerSearchViewModel>(mainSearchFiltersJSON);
            if (!string.IsNullOrEmpty(mainSearchFilters.SearchUserId))
            {
                var user = _userManager.Users.FirstOrDefault(c => c.Id == mainSearchFilters.SearchUserId);
                if (user.UserType == CPiUserType.Attorney)
                {
                    var entityFilter = _repository.CPiUserEntityFilters.Where(c => c.UserId == user.Id).AsNoTracking().FirstOrDefault();
                    if (entityFilter != null)
                        mainSearchFilters.SearchAttorneyId = entityFilter.EntityId;
                }
            }
            var timeTrackers = _attorneyTimeTrackerService.ChildService.QueryableList.ProjectTo<TimeTrackerViewModel>()
                                .Where(c =>
                                (mainSearchFilters.SearchAttorneyId != 0 ? c.AttorneyID == mainSearchFilters.SearchAttorneyId : c.TrackUserId == mainSearchFilters.SearchUserId) && 
                                (mainSearchFilters.SearchOutstandingOnly.Equals("2") || (mainSearchFilters.SearchOutstandingOnly.Equals("1") && c.Exported) || (mainSearchFilters.SearchOutstandingOnly.Equals("0") && !c.Exported)) &&
                                (mainSearchFilters.EntryDateFrom == null || c.EntryDate.Date >= mainSearchFilters.EntryDateFrom) &&
                                (mainSearchFilters.EntryDateTo == null || c.EntryDate.Date <= mainSearchFilters.EntryDateTo) &&
                                (string.IsNullOrEmpty(mainSearchFilters.SearchSystemType) || mainSearchFilters.SearchSystemType.Equals(c.SystemType)) &&
                                (string.IsNullOrEmpty(mainSearchFilters.SearchCaseNumber) || mainSearchFilters.SearchCaseNumber.Equals(c.CaseNumber)) &&
                                (string.IsNullOrEmpty(mainSearchFilters.SearchCountry) || mainSearchFilters.SearchCountry.Equals(c.Country)) &&
                                (string.IsNullOrEmpty(mainSearchFilters.SearchSubCase) || mainSearchFilters.SearchSubCase.Equals(c.SubCase)) &&
                                (string.IsNullOrEmpty(mainSearchFilters.SearchClientCode) || mainSearchFilters.SearchClientCode.Equals(c.TimeTrackerClientCode))
                                );
            if (!string.IsNullOrEmpty(sortField))
            {
                var sort = new SortDescriptor { Member = sortField, SortDirection = sortDirection == "asc" ? ListSortDirection.Ascending : ListSortDirection.Descending };
                timeTrackers = timeTrackers.ApplySorting(new List<SortDescriptor> { sort });
            }
            var data = timeTrackers.ProjectTo<TimeTrackerExportToExcelViewModel>().ToList();

            var properties = await _exportHelper.GetExportPropertyNames(typeof(TimeTrackerExportToExcelViewModel), _searchResultLocalizer);
            var excludeColumns = new List<string>();

            var fileStream = await _exportHelper.ListToExcelMemoryStream(data, "List", _searchResultLocalizer, true, "ImageFile", "", 50, false, excludeColumns);
            return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), "TimeTrackerList.xlsx");
        }
    }
}
