using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using R10.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Core.Interfaces.Patent;
using R10.Core.Helpers;
using System.Security.Claims;
using R10.Core.DTOs;
using R10.Core.Services.Shared;
using Newtonsoft.Json;
using R10.Web.Areas.Patent.ViewModels;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessRecentViewed)]
    public class RecentViewedController : BaseController
    {
        private readonly IAuthorizationService _authService;        
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ILoggerService<ActivityLog> _activityLogger;
        protected readonly IUrlHelper _url;

        private readonly ICountryApplicationService _countryApplicationService;
        private readonly ITmkTrademarkService _trademarkService;
//         private readonly IGMMatterService _matterService; // Removed during deep clean
        private readonly ExportHelper _exportHelper;
        private readonly IStringLocalizer<RecentViewedViewModel> _searchResultLocalizer;
        protected readonly ClaimsPrincipal _user;

        private readonly string _dataContainer = "recentViewedDetail";

        public RecentViewedController(
            IAuthorizationService authService,
            IStringLocalizer<SharedResource> localizer,
            ILoggerService<ActivityLog> activityLogger,
            IUrlHelper url,
            ICountryApplicationService countryApplicationService,
            ITmkTrademarkService trademarkService,
//             IGMMatterService matterService, // Removed during deep clean
            ClaimsPrincipal user,
            ExportHelper exportHelper,
            IStringLocalizer<RecentViewedViewModel> searchResultLocalizer
            )
        {
            _authService = authService;            
            _localizer = localizer;
            _activityLogger = activityLogger;
            _url = url;

            _countryApplicationService = countryApplicationService;
            _trademarkService = trademarkService;
            _exportHelper = exportHelper;
            _searchResultLocalizer = searchResultLocalizer;
            _user = user;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "recentViewedSearch",
                Title = _localizer["Recently Viewed Records Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded
            };

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel()
            {
                Page = PageType.SearchResults,
                PageId = "recentViewedSearchResults",
                Title = _localizer["Recently Viewed Records Search Results"].ToString(),
                CanAddRecord = false
            };

            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search()
        {
            return RedirectToAction("Index");
        }

        [HttpPost()]
        public async Task<IActionResult> ExportToExcel(string mainSearchFiltersJSON, string sortField, string sortDirection)
        {
            var mainSearchFilters = JsonConvert.DeserializeObject<List<QueryFilterViewModel>>(mainSearchFiltersJSON);
            var data = await GetRecentlyViewedItems(mainSearchFilters);


            var fileStream = await _exportHelper.ListToExcelMemoryStream(data, "List", _searchResultLocalizer, true, "", "", 0, true);
            return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), "RecentlyViewedList.xlsx");

        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var data = await GetRecentlyViewedItems(mainSearchFilters);
                var ids = data.Select(d => d.Id).ToArray();
                var dataQuery = data.AsQueryable();
                if (request.Sorts != null && request.Sorts.Any())
                    dataQuery = dataQuery.ApplySorting(request.Sorts);
                else
                    dataQuery = dataQuery.OrderBy(d => d.ActivityDate);
                var list = dataQuery.ApplyPaging(request.Page, request.PageSize).ToList();

                var result = new CPiDataSourceResult() { Data = list, Total = ids.Length, Ids = ids };

                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost()]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        public async Task<IActionResult> GetActivityLogUserList(string property, string text, FilterType filterType)
        {
            var data = await _activityLogger.QueryableList.AsNoTracking().Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => new { d.UserId }).Distinct().OrderBy(o => o.UserId).ToListAsync();
            return Json(data);
        }


        private async Task<List<RecentViewedViewModel>> GetRecentlyViewedItems(List<QueryFilterViewModel> mainSearchFilters)
        {
            var data = new List<RecentViewedViewModel>();
            var activityLog = _activityLogger.QueryableList.Where(d => !string.IsNullOrEmpty(d.UserId));

            var patent = mainSearchFilters.FirstOrDefault(f => f.Property == "Patent");
            var trademark = mainSearchFilters.FirstOrDefault(f => f.Property == "Trademark");
            var clientCode = mainSearchFilters.FirstOrDefault(f => f.Property == "Client.ClientCode");
            var viewedBy = mainSearchFilters.FirstOrDefault(f => f.Property == "ViewedBy");
            var activityDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "ActivityDateFrom");
            var activityDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "ActivityDateTo");
            var activityDateTimeFrame = mainSearchFilters.FirstOrDefault(f => f.Property == "ActivityDateTimeFrame");

            if (activityDateFrom == null && activityDateTo == null)
            {
                activityDateFrom = new QueryFilterViewModel() { Operator = null, Property = "ActivityDateFrom", Value = "" };
                activityDateTo = new QueryFilterViewModel() { Operator = null, Property = "ActivityDateTo", Value = "" };
                switch (activityDateTimeFrame.Value)
                {
                    case "D":
                        activityDateFrom.Value = DateTime.Today.ToString();
                        activityDateTo.Value = DateTime.Today.AddDays(1).ToString();
                        break;
                    case "W":
                        activityDateFrom.Value = DateTime.Today.FirstDayOfWeek().ToString();
                        activityDateTo.Value = DateTime.Today.LastDayOfWeek().ToString();
                        break;
                    case "M":
                        activityDateFrom.Value = DateTime.Today.FirstDayOfMonth().ToString();
                        activityDateTo.Value = DateTime.Today.LastDayOfMonth().ToString();
                        break;
                }
            }

            var isAdmin = User.IsAdmin();

            if (!isAdmin)
            {
                activityLog = activityLog.Where(d => (EF.Functions.Like(d.UserId, _user.GetEmail()))
                                                && (activityDateFrom == null || d.ActivityDate.Date >= Convert.ToDateTime(activityDateFrom.Value))
                                                && (activityDateTo == null || d.ActivityDate.Date <= Convert.ToDateTime(activityDateTo.Value))
                                            );
            }
            else
            {
                activityLog = activityLog.Where(d => (viewedBy == null || EF.Functions.Like(d.UserId, viewedBy.Value))
                                                && (activityDateFrom == null || d.ActivityDate.Date >= Convert.ToDateTime(activityDateFrom.Value))
                                                && (activityDateTo == null || d.ActivityDate.Date <= Convert.ToDateTime(activityDateTo.Value))
                                            );
            }



            if (patent != null)
            {
                var patUrl = _url.ActionLink("Detail", "CountryApplication", new { area = "Patent" }) + "/";
                var appIds = activityLog.Where(d => EF.Functions.Like(d.RequestUrl, $"%{patUrl}%"))
                                        .Select(d => new
                                        {
                                            ViewedId = Int32.Parse(d.RequestUrl.Substring(patUrl.Length, (d.RequestUrl.IndexOf("?") < 0 ? d.RequestUrl.Length : d.RequestUrl.IndexOf("?")) - d.RequestUrl.IndexOf(patUrl) - patUrl.Length)),
                                            ActivityDate = d.ActivityDate,
                                            UserId = d.UserId.Substring(0, d.UserId.IndexOf("@"))
                                        })
                                        .Distinct()
                                        .ToList();

                data.AddRange(_countryApplicationService.CountryApplications
                                        .Where(d => (clientCode == null || EF.Functions.Like(d.Invention.Client.ClientCode, clientCode.Value)))
                                        .Select(d => new
                                        {
                                            d.AppId,
                                            d.CaseNumber,
                                            d.AppTitle,
                                            d.Country,
                                            d.SubCase,
                                            d.ApplicationStatus,
                                            d.AppNumber,
                                            d.FilDate,
                                            d.PatNumber,
                                            d.IssDate
                                        })
                                        .AsEnumerable()
                                        .Join(appIds, app => app.AppId, filtered => filtered.ViewedId, (app, filtered) => new RecentViewedViewModel()
                                        {
                                            ActivityDate = filtered.ActivityDate,
                                            UserId = filtered.UserId,
                                            Id = app.AppId,
                                            IdType = "CountryApplication",
                                            System = "Patent",
                                            Title = app.AppTitle,
                                            CaseNumber = app.CaseNumber,
                                            Country = app.Country,
                                            SubCase = app.SubCase,
                                            Status = app.ApplicationStatus,
                                            AppNumber = app.AppNumber,
                                            FilDate = app.FilDate,
                                            PatRegNumber = app.PatNumber,
                                            IssRegDate = app.IssDate
                                        })
                                        .ToList());
            }
            if (trademark != null)
            {
                var tmkUrl = _url.ActionLink("Detail", "TmkTrademark", new { area = "Trademark" }) + "/";
                var tmkIds = activityLog.Where(d => EF.Functions.Like(d.RequestUrl, $"%{tmkUrl}%"))
                                        .Select(d => new
                                        {
                                            ViewedId = Int32.Parse(d.RequestUrl.Substring(tmkUrl.Length, (d.RequestUrl.IndexOf("?") < 0 ? d.RequestUrl.Length : d.RequestUrl.IndexOf("?")) - d.RequestUrl.IndexOf(tmkUrl) - tmkUrl.Length)),
                                            ActivityDate = d.ActivityDate,
                                            UserId = d.UserId.Substring(0, d.UserId.IndexOf("@"))
                                        })
                                        .Distinct()
                                        .ToList();

                data.AddRange(_trademarkService.TmkTrademarks
                                        .Where(d => (clientCode == null || EF.Functions.Like(d.Client.ClientCode, clientCode.Value)))
                                        .Select(d => new
                                        {
                                            d.TmkId,
                                            d.CaseNumber,
                                            d.TrademarkName,
                                            d.Country,
                                            d.SubCase,
                                            d.TrademarkStatus,
                                            d.AppNumber,
                                            d.FilDate,
                                            d.RegNumber,
                                            d.RegDate
                                        })
                                        .AsEnumerable()
                                        .Join(tmkIds, tmk => tmk.TmkId, filtered => filtered.ViewedId, (tmk, filtered) => new RecentViewedViewModel()
                                        {
                                            ActivityDate = filtered.ActivityDate,
                                            UserId = filtered.UserId,
                                            Id = tmk.TmkId,
                                            IdType = "TmkTrademark",
                                            System = "Trademark",
                                            Title = tmk.TrademarkName,
                                            CaseNumber = tmk.CaseNumber,
                                            Country = tmk.Country,
                                            SubCase = tmk.SubCase,
                                            Status = tmk.TrademarkStatus,
                                            AppNumber = tmk.AppNumber,
                                            FilDate = tmk.FilDate,
                                            PatRegNumber = tmk.RegNumber,
                                            IssRegDate = tmk.RegDate
                                        })
                                        .ToList());

            }
            return data;
        }
    }
}
