using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Entities.FormExtract;
using R10.Core.Entities.Patent;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers.FormExtract
{
    [Area("Shared")]
    public class FormIFWActionMapController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IFormIFWService _ifwService;
        private readonly IParentEntityService<FormIFWActMap, FormIFWActMapPat> _actMapService;
        private readonly IParentEntityService<FormIFWActMap, FormIFWActMapTmk> _actTmkMapService;
        private readonly IReportService _reportService;
        private readonly IDistributedCache _distributedCache;

        private readonly string _dataContainer = "formIFWActionMapDetail";

        public FormIFWActionMapController(IAuthorizationService authService,
                                          IStringLocalizer<SharedResource> localizer,
                                          IFormIFWService ifwService,
                                          IParentEntityService<FormIFWActMap, FormIFWActMapPat> actMapService,
                                          IParentEntityService<FormIFWActMap, FormIFWActMapTmk> actTmkMapService,
                                          IReportService reportService, IDistributedCache distributedCache)
        {
            _authService = authService;
            _localizer = localizer;
            _ifwService = ifwService;
            _actMapService = actMapService;
            _actTmkMapService = actTmkMapService;
            _reportService = reportService;
            _distributedCache = distributedCache;
        }

        [Authorize(Policy = PatentAuthorizationPolicy.InternalFullModify)]
        public async Task<IActionResult> Patent()
        {
            return  await Index(SystemTypeCode.Patent);
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.InternalFullModify)]
        public async Task<IActionResult> Trademark()
        {
            return await Index(SystemTypeCode.Trademark);
        }

        private async Task<IActionResult> Index(string systemType)
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "formIFWActionMapSearch",
                Title = _localizer["IFW Action Mapping Search"].ToString(),
                CanAddRecord = false,
                SystemType = systemType
            };

            var cacheKey = User.GetUserName() + ":AIM-SystemType";
            await _distributedCache.SetStringAsync(cacheKey, systemType);

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View("Index", model);
        }

        public async Task<IActionResult> Search()
        {
            var cacheKey = User.GetUserName() + ":AIM-SystemType";
            var systemType = await _distributedCache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(systemType))
            {
                return await Index(systemType);
            }
            return BadRequest();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel()
            {
                Page = PageType.SearchResults,
                PageId = "formIFWActionMapSearchResults",
                Title = _localizer["IFW Action Mapping Search Results"].ToString(),
                CanAddRecord = false,
                GridPageSize = 10
            };
            var permission = new DetailPagePermission();

            var systemType = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemType");
            if (systemType.Value == SystemTypeCode.Patent)
            {
                permission.AddPatentActionTypeSecurityPolicies();
            }
            else {
                permission.AddTrademarkActionTypeSecurityPolicies();
            }
            
            await permission.ApplyDetailPagePermission(User, _authService);
            model.PagePermission = permission;

            return PartialView("Index", model);
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                if (!mainSearchFilters.Any(f => f.Property == "SystemType"))
                    return BadRequest();

                var ifwMaps = AddCriteria(_ifwService.FormIFWActMaps, mainSearchFilters).ProjectTo<FormIFWActionMapViewModel>();
                var result = await CreateViewModelForGrid(request, ifwMaps, "DocDesc", "MapHdrId");
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

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["IFW Action Mapping Detail"].ToString(),
                RecordId = detail.MapHdrId,
                SingleRecord = singleRecord || !Request.IsAjax(),
                ActiveTab = tab,
                PagePermission = page,
                Data = detail
            };

            if (Request.IsAjax())
            {
                if (!singleRecord && !fromSearch)
                    model.Page = PageType.DetailContent;

                return PartialView("Index", model);
            }

            return View("Index", model);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] FormIFWActionMapViewModel actMap)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(actMap, actMap.MapHdrId);

                await _ifwService.UpdateActMap(actMap.MapHdrId, actMap.IsGenActionDE, actMap.IsCompareDE, User.GetUserName());

                return Json(actMap.MapHdrId);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var actMap = await _ifwService.GetByIdAsync(id);
            if (actMap == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = actMap.CreatedBy, dateCreated = actMap.DateCreated, updatedBy = actMap.UpdatedBy, lastUpdate = actMap.LastUpdate, tStamp = actMap.tStamp });
        }

        [HttpGet]
        public IActionResult Print()
        {
            ViewBag.Url = Url.Action("Print");
            ViewBag.DownloadName = "IFW Action Mapping Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintViewModel patIFWActionMappingPrintModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(patIFWActionMappingPrintModel, ReportType.PatIFWActionMappingPrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        #region Grid
        public async Task<IActionResult> MapPatRead([DataSourceRequest] DataSourceRequest request, int mapHdrId)
        {
            var result = (await _ifwService.FormIFWActMapsPat.Where(m => m.MapHdrId == mapHdrId).ToListAsync()).ToDataSourceResult(request);
            return Json(result);
        }

        public async Task<IActionResult> MapTmkRead([DataSourceRequest] DataSourceRequest request, int mapHdrId)
        {
            var result = (await _ifwService.FormIFWActMapsTmk.Where(m => m.MapHdrId == mapHdrId).ToListAsync()).ToDataSourceResult(request);
            return Json(result);
        }

        [Authorize(Policy = PatentAuthorizationPolicy.ActionTypeModify)]
        public async Task<IActionResult> MapPatUpdate(int mapHdrId,
            [Bind(Prefix = "updated")] IEnumerable<FormIFWActMapPat> updated,
            [Bind(Prefix = "new")] IEnumerable<FormIFWActMapPat> added,
            [Bind(Prefix = "deleted")] IEnumerable<FormIFWActMapPat> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _actMapService.ChildService.Update(mapHdrId, User.GetUserName(), updated, added, deleted);
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                _localizer["Term Action Setting has been saved successfully."].ToString() :
                _localizer["Term Action Setting have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.ActionTypeModify)]
        public async Task<IActionResult> MapTmkUpdate(int mapHdrId,
            [Bind(Prefix = "updated")] IEnumerable<FormIFWActMapTmk> updated,
            [Bind(Prefix = "new")] IEnumerable<FormIFWActMapTmk> added,
            [Bind(Prefix = "deleted")] IEnumerable<FormIFWActMapTmk> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _actTmkMapService.ChildService.Update(mapHdrId, User.GetUserName(), updated, added, deleted);
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                _localizer["Term Action Setting has been saved successfully."].ToString() :
                _localizer["Term Action Setting have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.ActionTypeModify)]
        public async Task<IActionResult> MapPatDelete([Bind(Prefix = "deleted")] FormIFWActMapPat deleted)
        {
            if (deleted.MapId > 0)
            {
                await _actMapService.ChildService.Update(deleted.MapId, User.GetUserName(), new List<FormIFWActMapPat>(), new List<FormIFWActMapPat>(), new List<FormIFWActMapPat>() { deleted });
                return Ok(new { success = _localizer["Action Parameter has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.ActionTypeModify)]
        public async Task<IActionResult> MapTmkDelete([Bind(Prefix = "deleted")] FormIFWActMapTmk deleted)
        {
            if (deleted.MapId > 0)
            {
                await _actTmkMapService.ChildService.Update(deleted.MapId, User.GetUserName(), new List<FormIFWActMapTmk>(), new List<FormIFWActMapTmk>(), new List<FormIFWActMapTmk>() { deleted });
                return Ok(new { success = _localizer["Action Parameter has been deleted successfully."].ToString() });
            }
            return Ok();
        }
        #endregion

        #region Prepare Screen
        private async Task<DetailPageViewModel<FormIFWActionMapViewModel>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<FormIFWActionMapViewModel>();
            var mapHeader = await _ifwService.FormIFWActMaps.Where(f => f.MapHdrId == id).ProjectTo<FormIFWActionMapViewModel>().FirstOrDefaultAsync();

            viewModel.Detail = mapHeader;
            if (viewModel.Detail != null)
            {
                if (viewModel.Detail.SystemType == SystemTypeCode.Patent)
                {
                    viewModel.AddPatentActionTypeSecurityPolicies();
                }
                else {
                    viewModel.AddTrademarkActionTypeSecurityPolicies();
                }
                    
                await viewModel.ApplyDetailPagePermission(User, _authService);
                this.AddDefaultNavigationUrls(viewModel);

                viewModel.CanEmail = false;
                viewModel.CanAddRecord = false;
                viewModel.CanCopyRecord = false;
                viewModel.CanEditRemarksOnly = false;
                viewModel.CanDeleteRecord = false;

                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";

                var systemTypeName = viewModel.Detail.SystemType == SystemTypeCode.Patent ? "Patent" : "Trademark";
                viewModel.SearchScreenUrl = this.Url.Action(systemTypeName);
                viewModel.Container = _dataContainer;
            }
            return viewModel;
        }
        #endregion

        #region View Model Service
        protected IQueryable<FormIFWActMap> AddCriteria(IQueryable<FormIFWActMap> sources, List<QueryFilterViewModel> mainSearchFilters)
        {
            var systemType = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemType");
            sources = sources.Where(a => a.FormIFWDocType.SystemType==systemType.Value);
            mainSearchFilters.Remove(systemType);

            var docDesc = mainSearchFilters.FirstOrDefault(f => f.Property == "DocDesc");
            if (docDesc != null)
            {
                sources = sources.Where(a => a.FormIFWDocType.DocDesc == docDesc.Value);
                mainSearchFilters.Remove(docDesc);
            }

            if (systemType.Value == SystemTypeCode.Patent)
            {
                var pmsActionType = mainSearchFilters.FirstOrDefault(f => f.Property == "PMSActionType");
                if (pmsActionType != null)
                {
                    sources = sources.Where(a => a.FormIFWActMapPats.Any(p => p.PMSActionType == pmsActionType.Value));
                    mainSearchFilters.Remove(pmsActionType);
                }
            }
            else {
                var tmsActionType = mainSearchFilters.FirstOrDefault(f => f.Property == "TMSActionType");
                if (tmsActionType != null)
                {
                    sources = sources.Where(a => a.FormIFWActMapTmks.Any(p => p.TMSActionType == tmsActionType.Value));
                    mainSearchFilters.Remove(tmsActionType);
                }
            }

            if (mainSearchFilters.Count > 0)
            {
                sources = QueryHelper.BuildCriteria<FormIFWActMap>(sources, mainSearchFilters);
            }
            return sources;
        }

        public virtual async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<FormIFWActionMapViewModel> list, string defaultSortOrder, string idProperty)
        {
            if (request.Sorts != null && request.Sorts.Any())
                list = list.ApplySorting(request.Sorts);
            else
                list = list.OrderBy(m => m.DocDesc);

            var ids = await list.Select(m => m.MapHdrId).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await list.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }

        #endregion

        #region Pick List
        public async Task<IActionResult> GetDocDesc(string systemType)
        {
            var sources = await _ifwService.FormIFWDocTypes.Where(d => d.FormIFWFormType.IsEnabled && d.FormIFWFormType.FormType == "IFW-Act" && d.SystemType==systemType)
                                        .Select(d => new { DocDesc = d.DocDesc }).ToListAsync();
            return Json(sources);
        }

        public async Task<IActionResult> GetActionPickList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var sources = _ifwService.FormIFWActMapsPat;
            return Json(await QueryHelper.GetPicklistDataAsync(sources, property, text, filterType, requiredRelation));
        }

        public async Task<IActionResult> GetTmkActionPickList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var sources = _ifwService.FormIFWActMapsTmk;
            return Json(await QueryHelper.GetPicklistDataAsync(sources, property, text, filterType, requiredRelation));
        }

        #endregion

    }
}