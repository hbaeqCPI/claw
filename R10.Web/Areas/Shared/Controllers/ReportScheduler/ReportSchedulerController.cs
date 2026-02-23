using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using R10.Core;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Shared.ViewModels.ReportScheduler;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Interfaces.Shared;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers.ReportScheduler
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class ReportSchedulerController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IRSActionService _rSActionService;
        private readonly IRSCriteriaService _rSCriteriaService;
        private readonly IRSPrintOptionService _rSPrintOptionService;
        private readonly IRSMainService _rSMainService;
        private readonly IRSHistoryService _rSHistoryService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IRSMainViewModelService _rSMainViewModelService;
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly ICPiUserSettingManager _userSettingManager;
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        private readonly IRSCTMService _rSCTMService;
        ClaimsPrincipal _user;

        private readonly RSCTMScheduleBuilder rSCTMScheduleBuilder;

        private IQueryable<RSMain> RSMains => _rSMainService.RSMains;
        private readonly string _dataContainer = "reportSchedulerDetail"; //detail and add page container
        private readonly string _scheduleLimitErrorMessage = "You have reached the maximum number of scheduled reports. To add a new one, please disable an existing scheduled report you no longer need.";
        public ReportSchedulerController(
            IAuthorizationService authService
            , IRSActionService rSActionService
            , IRSCriteriaService rSCriteriaService
            , IRSPrintOptionService rSPrintOptionService
            , IRSMainService rSMainService
            , IRSHistoryService rSHistoryService
            , IStringLocalizer<SharedResource> localizer
            , IRSMainViewModelService rSMainViewModelService
            , ISystemSettings<PatSetting> settings
            , ICPiUserSettingManager userSettingManager
            , IRSActionService rsActionService
            , IHostingEnvironment hostingEnvironment
            , IReportService reportService
            , ISystemSettings<DefaultSetting> defaultSettings
            , ClaimsPrincipal user
            , IEmailSender emailSender
            , IConfiguration configuration
            , IRSCTMService rSCTMService
            )
        {
            _authService = authService;
            _rSActionService = rSActionService;
            _rSCriteriaService = rSCriteriaService;
            _rSPrintOptionService = rSPrintOptionService;
            _rSMainService = rSMainService;
            _rSHistoryService = rSHistoryService;
            _localizer = localizer;
            _rSMainViewModelService = rSMainViewModelService;
            _settings = settings;
            _userSettingManager = userSettingManager;
            _defaultSettings = defaultSettings;
            _user = user;
            _rSCTMService = rSCTMService;
            rSCTMScheduleBuilder = new RSCTMScheduleBuilder(configuration);
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "ReportSchedulerSearch",  //container name
                Title = _localizer["Report Scheduler Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded,
                HasSavedCriteria = true
            };

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View(model);
        }

        [HttpGet]
        public IActionResult Search()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel()
            {
                Page = PageType.SearchResults,
                PageId = "reportSchedulerSearchResults",   //container name
                Title = _localizer["Report Scheduler Search Results"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded,
                HasSavedCriteria = true
            };

            return PartialView("Index", model);
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> ScheduleNameSearchValueMapper(string value)
        {
            var result = await _rSMainViewModelService.ScheduleNameSearchValueMapper(RSMains, value);
            return Json(result);
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(RSMains.Where(c=>c.IsShared||c.TaskCreatorId==User.GetUserIdentifier()), request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var rSMains = _rSMainViewModelService.AddCriteria(mainSearchFilters, RSMains.Where(c => c.IsShared || c.TaskCreatorId == User.GetUserIdentifier()));
                var result = await _rSMainViewModelService.CreateViewModelForGrid(request, rSMains);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false, string tab = "", int reportId = 0)
        {
            if (id== 0  && await IsScheduleLimitReached())
            {
                throw new InvalidOperationException(_scheduleLimitErrorMessage);
            }

            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                Guard.Against.NoRecordPermission(!Request.IsAjax());
                return RedirectToAction("Index");
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,    //container name
                Title = _localizer["Report Scheduler Detail"].ToString(),
                RecordId = detail.TaskId,
                SingleRecord = singleRecord || !Request.IsAjax(),
                ActiveTab = tab,
                PagePermission = page,
                Data = detail
            };
            var main = _rSMainService.QueryableList.FirstOrDefault(c => c.TaskId == id);
            if (main != null)
                reportId = main.ReportId;

            ViewBag.ReportId = reportId;

            if (Request.IsAjax())
            {
                if (!singleRecord && !fromSearch)
                    model.Page = PageType.DetailContent;

                return PartialView("Index", model);
            }

            return View("Index", model);
        }

        //zoom from reports
        [HttpGet()]
        public async Task<IActionResult> DetailLink(int id, bool singleRecord = false, bool fromSearch = false, string tab = "", int reportId = 0)
        {
            if (id > 0)
            {
                var schedule = await _rSMainService.GetByIdAsync(id);
                if (schedule == null)
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction(nameof(Detail), new { id = id, singleRecord = true, fromSearch = true, reportId = reportId });
            }

            if ((await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.FullModify)).Succeeded)
                return RedirectToAction(nameof(Add), new { fromSearch = true });
            else
                return new RecordDoesNotExistResult();
        }

        private async Task<DetailPageViewModel<RSMainDetailViewModel>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<RSMainDetailViewModel>();
            viewModel.Detail = await _rSMainViewModelService.CreateViewModelForDetailScreen(id);

            if (viewModel.Detail != null)
            {
                this.AddDefaultNavigationUrls(viewModel);

                viewModel.CanCopyRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded;
                viewModel.IsCopyScreenPopup = true;
                viewModel.CopyScreenUrl = Url.Action("Copy", new { id = id });

                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.Container = _dataContainer;

                //search screen and delete confirmation urls
                viewModel.SearchScreenUrl = Url.Action("Index");
                viewModel.DeleteConfirmationUrl = Url.DeleteConfirmWithCodeLink();
                viewModel.DeleteScreenUrl = Url.Action("Delete", new { id = id });

                viewModel.CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded;
                viewModel.AddScreenUrl = Url.Action("Detail", new { id = 0 });

                viewModel.CanDeleteRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.CanDelete)).Succeeded;
                viewModel.CanPrintRecord = false;
                viewModel.CanEditRecord = viewModel.Detail.IsEditable || viewModel.Detail.TaskCreatorId ==User.GetUserIdentifier();

                var setting = await _settings.GetSetting();

            }
            return viewModel;
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] RSMainDetailViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                //viewModel.CaseNumber = await BuildCaseNumber(viewModel);
                var rSMain = _rSMainViewModelService.ConvertViewModelToRSMain(viewModel);
                UpdateEntityStamps(rSMain, rSMain.TaskId);

                rSMain.StartDateOperator = rSMain.StartDateOperator == null ? "-" : rSMain.StartDateOperator;

                if (rSMain.TaskId > 0)
                {
                    await _rSMainService.Update(rSMain);
                    _rSCTMService.UpdateCTMSchedule(await CreateCTMEntity(rSMain));
                }

                else
                {
                    if (await IsScheduleLimitReached())
                    {
                        throw new InvalidOperationException(_scheduleLimitErrorMessage);
                    }

                    //update Date Type
                    rSMain.DateType = _rSMainService.RSDateTypeControls.FirstOrDefault(c => c.ReportId == rSMain.ReportId && c.DateType==1).DateTypeName;

                    await _rSMainService.Add(rSMain);
                    _rSCTMService.InsertCTMSchedule(await CreateCTMEntity(rSMain));
                }
                    

                return Json(rSMain.TaskId);
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string scheduleName = "", int reportId = 0)
        {
            //if (!Request.IsAjax())
            //    return RedirectToAction("Index");
            
            if (await IsScheduleLimitReached())
            {
                throw new InvalidOperationException(_scheduleLimitErrorMessage);
            }

            var page = await PrepareAddScreen(scheduleName);
                if (page.Detail == null)
                    return RedirectToAction("Index");

                var detail = page.Detail;

                PageViewModel model = new PageViewModel()
                {
                    Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                    PageId = page.Container,    //container name
                    Title = _localizer["New Schedule"].ToString(),
                    RecordId = detail.TaskId,
                    //SingleRecord = singleRecord || !Request.IsAjax(),
                    //ActiveTab = tab,
                    CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded,
                    PagePermission = page,
                    Data = detail,
                    FromSearch = fromSearch,
                    BeforeSubmit = ""
                };

                ViewBag.ReportId = reportId;

                return View("Index", model);
        }

        private async Task<DetailPageViewModel<RSMainDetailViewModel>> PrepareAddScreen(string scheduleName)
        {
            var viewModel = new DetailPageViewModel<RSMainDetailViewModel>();

            viewModel.Detail = await _rSMainViewModelService.CreateViewModelForDetailScreen(0);
            viewModel.Detail.Name = scheduleName;

            //viewModel.AddPatentSecurityPolicies(false);
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }


        [Authorize(Policy = SharedAuthorizationPolicy.CanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            tblCTMMain cTMMain = await CreateCTMEntity(_rSMainService.GetRSMainById(id));
            await _rSMainService.Delete(_rSMainService.GetRSMainById(id));
            _rSCTMService.DeleteCTMSchedule(cTMMain);
            return Ok();
        }

        [HttpGet()]
        public IActionResult Copy(int id)
        {
            var viewModel = new RSMainCopyViewModel
            {
                CopyTaskId = id,
                CopyScheduleName = "",
                CopyActions = true,
                CopyCriteria = true,
                CopyPrintOptions = true,
                CopySettings = true,
            };

            return PartialView("_CopyReportScheduler", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CopySchedule(RSMainCopyViewModel copy)
        {
            if (await IsScheduleLimitReached())
            {
                throw new InvalidOperationException(_scheduleLimitErrorMessage);
            }

            var result = await _rSMainService.CopySchedule(copy.CopyTaskId, copy.CopyScheduleName, copy.CopySettings, 
                                            copy.CopyActions, copy.CopyCriteria, copy.CopyPrintOptions, User.GetUserName());
            // add CTM Tasks
            if (result.Item2 != "0" && result.Item2 != "")
            {
                var rSMain = _rSMainService.GetRSMainById(int.Parse(result.Item2));
                _rSCTMService.InsertCTMSchedule(await CreateCTMEntity(rSMain));
            }
                
            string message = "";
            if (result.Item1 != "") message += _localizer.GetString($"\nAdded Scheuless: {result.Item1}");

            if (message != "") message = _localizer.GetString("Copy results:\n") + message;

            if (result.Item2 != "0" && result.Item2 != "")
                return Json(new { Message = message, NewID = result.Item2 });
            return Json(new { Message = message , NewID = ""});
        }

        public IActionResult GetRecordStamps(int id)
        {
            var rSAction = _rSActionService.GetRSActionById(id);
            if (rSAction == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = rSAction.CreatedBy, dateCreated = rSAction.DateCreated, updatedBy = rSAction.UpdatedBy, lastUpdate = rSAction.LastUpdate, tStamp = rSAction.tStamp });
        }

        public IActionResult Help()
        {
            return PartialView("_CriteriaHelp");
        }
        
        public async Task<IActionResult> CheckScheduleLimit()
        {
            var limitReached = await IsScheduleLimitReached();
            if (limitReached)
                return new JsonBadRequest(new { errors = _scheduleLimitErrorMessage });
            else
                return Ok();
        }

        public async Task<bool> IsScheduleLimitReached()
        {
            var defaultSettings = await _defaultSettings.GetSetting();
            var isLimitReached = RSMains.Where(r => r.IsEnabled == true).Count() >= defaultSettings.RSMaxScheduleCount;
            return (isLimitReached);
        }

        #region Preview
        public IActionResult GetDueDatesList(
            [DataSourceRequest] DataSourceRequest request, int parentId
            )
        {
            var rSDueDates = _rSMainService.GetDueDates(parentId);
            var result = rSDueDates.Select(c => new
            {
                CaseNumber = c.CaseNumber,
                Country = c.Country,
                CountryName = c.CountryName,
                SubCase = c.SubCase,
                ActionType = c.ActionType,
                BaseDate = c.BaseDate,
                ActionDue = c.ActionDue,
                DueDate = c.DueDate,
                Indicator = c.Indicator,
                Responsible = c.Responsible,
                Status = c.Status,
                RespOffice = c.RespOffice,
                CaseType = c.CaseType,
                Type = c.SysSrc.IsCaseInsensitiveEqual("P")?"Patent":(c.SysSrc.IsCaseInsensitiveEqual("T") ? "Trademark" : "Genral Matter")

            }).ToList();

            return Json(result.ToDataSourceResult(request));
        }

        public IActionResult GetPatentListPreviewList(
            [DataSourceRequest] DataSourceRequest request, int parentId
            )
        {
            var result = _rSMainService.GetPatentListPreviewList(parentId).ToList();

            return Json(result.ToDataSourceResult(request));
        }

        public IActionResult GetTrademarkListPreviewList(
            [DataSourceRequest] DataSourceRequest request, int parentId
            )
        {
            var result = _rSMainService.GetTrademarkListPreviewList(parentId).ToList();

            return Json(result.ToDataSourceResult(request));
        }

        public IActionResult GetMatterListPreviewList(
            [DataSourceRequest] DataSourceRequest request, int parentId
            )
        {
            var result = _rSMainService.GetMatterListPreviewList(parentId).ToList();

            return Json(result.ToDataSourceResult(request));
        }

        #endregion Preview

        #region Main
        public async Task<IActionResult> GetReportList(string property, string text, FilterType filterType)
        {
            var list = _rSMainService.RSReportTypes.Where(c => c.IsEnabled).Select(a => new { ReportId = a.ReportId, Report = a.ReportName }).OrderBy(a => a.Report).ToList();
            var includedSystems = (await _defaultSettings.GetSetting()).RSIncludedSystems;
            if (!User.IsInSystem(SystemType.Patent) || !includedSystems.Contains("P"))
                list.Remove(list.FirstOrDefault(c=>c.ReportId==1));//Patent List Report
            if (!User.IsInSystem(SystemType.Trademark) || !includedSystems.Contains("T"))
                list.Remove(list.FirstOrDefault(c => c.ReportId == 5));//Trademark List Report
            if (!User.IsInSystem(SystemType.GeneralMatter) || !includedSystems.Contains("G"))
                list.Remove(list.FirstOrDefault(c => c.ReportId == 6));//Matter List Report
            if (!User.IsInUserType(CPiUserType.SuperAdministrator))
                list.Remove(list.FirstOrDefault(c => c.ReportId == 8));//AMS Reminder
            return Json(list);
        }

        public IActionResult GetFrequencyList(string property, string text, FilterType filterType)
        {
            var list = _rSMainService.RSFrequencyTypes.Select(a => new { FreqTypeId = a.FreqTypeId, Frequency = a.Frequency }).OrderBy(a => a.FreqTypeId).ToList();
            return Json(list);
        }

        public IActionResult GetDateTypeList(string property, string text, FilterType filterType, int taskId)
        {
            int reportId;
            if (taskId != 0)
                reportId = _rSMainService.RSMains.FirstOrDefault(c => c.TaskId == taskId).ReportId;
            else
                reportId = 2;
            var list = _rSMainService.RSDateTypeControls.Where(c => c.ReportId == reportId).Select(a => new { DateType = a.DateTypeName }).OrderBy(a => a.DateType).ToList();
            return Json(list);
        }

        public IActionResult GetStartDateOperatorList(string property, string text, FilterType filterType, int reportId)
        {
            var list = new List<StartDateOperatorClass>();
            StartDateOperatorClass startDateOperatorClassM = new StartDateOperatorClass() { StartDateOperator = "-" };
            StartDateOperatorClass startDateOperatorClassP = new StartDateOperatorClass() { StartDateOperator = "+" };
            list.Add(startDateOperatorClassM);
            list.Add(startDateOperatorClassP);
            return Json(list);
        }

        private class StartDateOperatorClass
        {
            public string StartDateOperator { get; set; }
        }

        public IActionResult GetDateUnitList(string property, string text, FilterType filterType, int reportId)
        {
            var list = Enum.GetValues(typeof(DateUnit)).Cast<DateUnit>().Select(a => new { DateUnitValue = a, DateUnit = a.ToString() }).ToList();
            return Json(list);
        }

        private enum DateUnit
        {
            Days,
            Weeks,
            Months,
            Years
        }

        public IActionResult GetFixedRangeList(string property, string text, FilterType filterType)
        {
            var list = new List<FixedRangeClass>();
            FixedRangeClass week = new FixedRangeClass() { FixedRange = "0", FixedRangeText = "This Week" };
            FixedRangeClass month = new FixedRangeClass() { FixedRange = "1", FixedRangeText = "This Month" };
            list.Add(week);
            list.Add(month);
            return Json(list);
        }

        private class FixedRangeClass
        {
            public string FixedRange { get; set; }
            public string FixedRangeText { get; set; }
        }

        public IActionResult GetDayOfMonthList(string property, string text, FilterType filterType, int reportId)
        {
            var list = new List<DayOfMonthClass>();
            DayOfMonthClass first = new DayOfMonthClass() { DayOfMonth = "First" };
            DayOfMonthClass fifteenth = new DayOfMonthClass() { DayOfMonth = "15th" };
            DayOfMonthClass last = new DayOfMonthClass() { DayOfMonth = "Last" };
            list.Add(first);
            list.Add(fifteenth);
            list.Add(last);
            return Json(list);
        }

        private class DayOfMonthClass
        {
            public string DayOfMonth { get; set; }
        }

        #endregion Main

        #region DueDateScheduleGrid

        public IActionResult GetSchedulesList(
            [DataSourceRequest] DataSourceRequest request, int parentId, int reportId, bool userSchedulesOnly
            )
        {
            var rSMains = _rSMainService.RSMains.Where(c => c.IsShared || c.TaskCreatorId == User.GetUserIdentifier()).Where(c => c.ReportId == reportId && (!userSchedulesOnly || c.TaskCreatorId == User.GetUserIdentifier()));
            var rSFrequencyTypes = _rSMainService.RSFrequencyTypes.ToList();
            //var result = await rSMains.Where(c=> c.ReportId == reportId && (!userSchedulesOnly || c.TaskCreatorId == User.Claims.FirstOrDefault(a => a.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value)).Select(c => new
            //var result = await rSMains.Where(c=> c.ReportId == reportId && (!userSchedulesOnly || c.TaskCreatorId == "aa0cd3bd-c488-45d4-b036-3efc21a0a3d8")).Select(c => new
            //{
            //    TaskId = c.TaskId,
            //    Schedule = c.Name,
            //    Status = c.IsEnabled ? "Enabled" : "Disabled",
            //    Trigger = DateTimeToTrigger(c, rSFrequencyTypes),
            //    NextRunTime = DateTimeToString(c.NextRunTime),
            //    CreatedBy = c.CreatedBy,
            //    DateCreated = DateTimeToString(c.DateCreated),
            //}).ToListAsync();
            List<ScheduleViewModel> result = new List<ScheduleViewModel>();

            foreach(RSMain c in rSMains)
            {
                ScheduleViewModel model = new ScheduleViewModel()
                {
                    TaskId = c.TaskId,
                    Schedule = c.Name,
                    Status = c.IsEnabled ? "Enabled" : "Disabled",
                    Trigger = DateTimeToTrigger(c, rSFrequencyTypes),
                    NextRunTime = DateTimeToString(c.NextRunTime),
                    CreatedBy = c.CreatedBy,
                    DateCreated = DateTimeToString(c.DateCreated)
                };
                result.Add(model);
            }

            return Json(result.ToDataSourceResult(request));
        }

        public string DateTimeToString(DateTime? t)
        {
            if (t == null) return "";
            DateTime temp = (DateTime)t;

            //return temp.ToShortDateString() + " " + temp.ToShortTimeString();
            return temp.ToString("dd-MMM-yyyy hh:mm tt");
        }

        public string DateTimeToTrigger(RSMain rSMain, List<RSFrequencyType> rSFrequencyTypes)
        {
            string frequency = rSFrequencyTypes.FirstOrDefault(c => c.FreqTypeId == rSMain.FreqTypeId).Frequency;
            //if (rSMain.TaskStartDateTime == null) return "";
            DateTime taskStartDate = rSMain.TaskStartDateTime;
            string taskStartDateString = taskStartDate.ToString("dd-MMM-yyyy");
            string taskStartTimeString = taskStartDate.ToString("hh:mm tt");

            if (frequency.IsCaseInsensitiveEqual("DAILY"))
            {
                return String.Format("At {0} every day, starting {1}", taskStartTimeString, taskStartDateString);
            }
            else if (frequency.IsCaseInsensitiveEqual("MONTHLY"))
            {
                return "At " + taskStartTimeString + " every " + rSMain.DayOfMonth + " day of the Month, starting " + taskStartDateString;
            }
            else if (frequency.IsCaseInsensitiveEqual("WEEKLY"))
            {
                return "At " + taskStartTimeString + " every "
                    + (rSMain.Sun ? "Sunday, " : "")
                    + (rSMain.Mon ? "Monday, " : "")
                    + (rSMain.Tue ? "Tuesday, " : "")
                    + (rSMain.Wed ? "Wednesday, " : "")
                    + (rSMain.Thu ? "Thursday, " : "")
                    + (rSMain.Fri ? "Friday, " : "")
                    + (rSMain.Sat ? "Saturday, " : "")
                    + "starting " + taskStartDateString;
            }

            return "";
        }

        #endregion DueDateScheduleGrid

        #region CTM
        public async Task<tblCTMMain> CreateCTMEntity(RSMain rSMain)
        {
            var defaultSettings = await _defaultSettings.GetSetting();
            var rSFrequencyTypes = _rSMainService.RSFrequencyTypes.ToList();

            tblCTMMain schedule = new tblCTMMain();

            schedule.SchedID = rSMain.TaskId;
            schedule.TaskCode = defaultSettings.RSCTMTaskCode;
            schedule.TaskName = defaultSettings.RSCTMClientName+ " "+ rSMain.Name;
            schedule.Active = rSMain.IsEnabled;
            schedule.NextProcessDate = rSMain.NextRunTime;
            schedule.WorkStationID = _user.GetUserName();
            schedule.Notes = DateTimeToTrigger(rSMain, rSFrequencyTypes);
            schedule.URL = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value.Replace("-staging", "") + HttpContext.Request.PathBase;
            schedule.TaskSubType = "B";
            return rSCTMScheduleBuilder.CreateSchedule(schedule);
        }


        #endregion
    }
}