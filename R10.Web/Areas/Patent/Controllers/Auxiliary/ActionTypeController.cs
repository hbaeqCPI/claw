using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using R10.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Patent.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Services;
using R10.Core;

using R10.Web.Areas;

namespace R10.Web.Areas.Patent.Controllers
{
    [Area("Patent"), Authorize(Policy = PatentAuthorizationPolicy.CanAccessActionType)]
    public class ActionTypeController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<PatActionType> _viewModelService;
        private readonly IEntityService<PatActionType> _actionTypeService;
        private readonly IChildEntityService<PatActionType, PatActionParameter> _actionParamService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        private readonly string _dataContainer = "patActionTypeDetail";

        public ActionTypeController(
            IAuthorizationService authService,
            IViewModelService<PatActionType> viewModelService,
            IEntityService<PatActionType> actionTypeService,
            IChildEntityService<PatActionType, PatActionParameter> actionParamService,
            IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _actionTypeService = actionTypeService;
            _actionParamService = actionParamService;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "patActionTypeSearch",
                Title = _localizer["Action Type Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.ActionTypeModify)).Succeeded
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
                PageId = "patActionTypeSearchResults",
                Title = _localizer["Action Type Search Results"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.ActionTypeModify)).Succeeded
            };

            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search()
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var actionTypes = _viewModelService.AddCriteria(mainSearchFilters).Where(t=> (t.CDueId ?? 0) == 0);
                var result = await _viewModelService.CreateViewModelForGrid<ActionTypeSearchResultViewModel>(request, actionTypes, "ActionType", "ActionTypeID");
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
                Title = _localizer["Action Type Detail"].ToString(),
                RecordId = detail.ActionTypeID,
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

        [HttpGet]
        public IActionResult Print()
        {
            ViewBag.Url = Url.Action("Print");
            ViewBag.DownloadName = "Action Type Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintViewModel patActionTypePrintModel)
        {
            // ReportService removed during debloat
            return BadRequest("Report service is not available.");
        }

        [Authorize(Policy = PatentAuthorizationPolicy.ActionTypeModify)]
        public async Task<IActionResult> Add(string actionType = "", string country = "", bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen(actionType, country);
            if (page.Detail == null)
                return RedirectToAction("Index");

            if (TempData["CopyOptions"] != null)
            {
                await ExtractCopyParams(page);
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Action Type"].ToString(),
                RecordId = detail.ActionTypeID,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };
            ModelState.Clear();

            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }


        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _actionTypeService.GetByIdAsync(id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            await _actionTypeService.Delete(entity);

            return Ok();
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.ActionTypeModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatActionType patActionType)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(patActionType, patActionType.ActionTypeID);
                patActionType.CDueId = patActionType.CDueId ?? 0;

                if (patActionType.ActionTypeID > 0)
                    await _actionTypeService.Update(patActionType);
                else {
                    await _actionTypeService.Add(patActionType);
                }
                    

                return Json(patActionType.ActionTypeID);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.ActionTypeRemarksOnly)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRemarks([FromBody] PatActionType actionType)
        {
            UpdateEntityStamps(actionType, actionType.ActionTypeID);
            await _actionTypeService.UpdateRemarks(actionType);
            return Json(actionType.ActionTypeID);
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var patActionType = await _actionTypeService.GetByIdAsync(id);
            if (patActionType == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = patActionType.CreatedBy, dateCreated = patActionType.DateCreated, updatedBy = patActionType.UpdatedBy, lastUpdate = patActionType.LastUpdate });
        }

        [HttpGet()]
        public async Task<IActionResult> Copy(int id)
        {
            var viewModel = await _actionTypeService.QueryableList.Where(a => a.ActionTypeID == id)
                            .Select(c => new ActionTypeCopyViewModel
                            {
                                ActionTypeId = id,
                                ActionType=c.ActionType,
                                CopyParameters = true,
                                CopyRemarks = true
                            }).FirstOrDefaultAsync();

            return PartialView("_Copy", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCopied([FromBody] ActionTypeCopyViewModel copy)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });
            TempData["CopyOptions"] = JsonConvert.SerializeObject(copy);
            return RedirectToAction("Add");
        }

        //NOT USED
        //public async Task<IActionResult> GetActionTypesList(string textProperty, string text, FilterType filterType, string requiredRelation = "")
        //{
        //    var actionTypes = _actionTypeService.QueryableList;
        //    actionTypes = QueryHelper.BuildCriteria(actionTypes, textProperty, text, filterType, requiredRelation);
        //    var list = await actionTypes.Select(c => new { ActionType = c.ActionType, ActionTypeName = c.ActionType }).OrderBy(c => c.ActionType).ToListAsync();
        //    return Json(list);
        //}

        private async Task<DetailPageViewModel<ActionTypeViewModel>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<ActionTypeViewModel>();
            var actionType = await _actionTypeService.QueryableList.Where(a => a.ActionTypeID == id).ProjectTo<ActionTypeViewModel>().FirstOrDefaultAsync();
          
            viewModel.Detail =actionType;
            if (viewModel.Detail != null)
            {
                viewModel.AddPatentActionTypeSecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.PageActions = await GetMorePageActions(viewModel);

                this.AddDefaultNavigationUrls(viewModel);
                
                viewModel.CopyScreenUrl = $"{viewModel.CopyScreenUrl}/{id}";
                viewModel.CanEmail = false;
                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.SearchScreenUrl = this.Url.Action("Index");
                viewModel.Container = _dataContainer;
                viewModel.AddScreenUrl = viewModel.CanAddRecord ? Url.Action("Add", new { fromSearch = true }) : "";
            }
            return viewModel;
        }

        private async Task<List<DetailPageAction>> GetMorePageActions(DetailPageViewModel<ActionTypeViewModel> pagePermission)
        {
            var pageActions = new List<DetailPageAction>();

            if (pagePermission.CanAddRecord)
                pageActions.Add(new DetailPageAction { Url = Url.Action("ActionDueCompute", "ActionType", new { area = "Patent", actionTypeId = pagePermission.Detail.ActionTypeID, country = pagePermission.Detail.Country }), Label = _localizer[$"Generate"], IsPopup = true, IconClass = "fa-bolt", ControlId = "actionDueCompute", IsPageNav = true });            
            
            return pageActions;
        }

        private async Task<DetailPageViewModel<ActionTypeViewModel>> PrepareAddScreen(string actionType, string country)
        {
            var viewModel = new DetailPageViewModel<ActionTypeViewModel>();
            viewModel.Detail = new ActionTypeViewModel();

            viewModel.Detail.ActionType = actionType;
            viewModel.Detail.Country = country;

            viewModel.AddPatentActionTypeSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        //NOT USED
        //[HttpGet()]
        //public async Task<IActionResult> GetFollowUpActionsList(string country)
        //{
        //    var list = await _actionTypeService.QueryableList.Where(a => a.Country == country || a.Country == null).Select(a => a.ActionType).ToListAsync();
        //    return Json(list);
        //}

        //picklist for country law followup action in country due screen
        public async Task<IActionResult> GetCountryLawFollowUpList(string country, int cDueId)
        {
            if (cDueId == 0)
                cDueId = -1;

            var list = await _actionTypeService.QueryableList.Where(a => (a.Country == country || a.Country == null) && (a.CDueId == 0 || a.CDueId == null || a.CDueId == cDueId)).Select(a => new {ActionType=a.ActionType}).Distinct().ToListAsync();
            return Json(list);
        }

        //Use Html.GetEnumSelectList<FollowUpOption>()
        //[HttpGet()]
        //public IActionResult GetFollowUpGenOptions()
        //{
        //    var list = Enum.GetValues(typeof(FollowUpOption)).Cast<FollowUpOption>().Select(value => new { Value = (int)value, Text = value.GetDisplayName() }).ToList();
        //    return Json(list);
        //}

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_actionTypeService.QueryableList, request, property, text, filterType, requiredRelation);
        }

        //picklist for FollowUpMsg
        public async Task<IActionResult> GetFollowUpActionList(string country)
        {
            //exlude actiontypes used as country law followup
            var actionTypes = _actionTypeService.QueryableList.Where(a => (a.CDueId ?? 0) == 0);

            //exlude actiontypes without country if same actiontype with country exists to avoid duplicates
            //keep select in each iqueryable for efficiency vs having select after union
            var hasCountry = actionTypes.Where(a => a.Country == country).Select(a => new { ActionType = a.ActionType });
            var noCountry = actionTypes.Where(a => string.IsNullOrEmpty(a.Country) && !hasCountry.Any(c => c.ActionType == a.ActionType)).Select(a => new { ActionType = a.ActionType });
            
            return Json(await hasCountry.Union(noCountry).OrderBy(a => a.ActionType).ToListAsync());
        }


        //picklist from Action Due screen
        //CDueId == 0
        public async Task<IActionResult> GetActionTypeList(string country)
        {
            //exlude actiontypes used as country law followup
            var actionTypes = _actionTypeService.QueryableList.Where(a => (a.CDueId ?? 0) == 0);

            //exlude actiontypes without country if same actiontype with country exists to avoid duplicates
            //keep select in each iqueryable for efficiency vs having select after union
            var hasCountry = actionTypes.Where(a => a.Country == country)
                                .Select(a => new { ActionTypeID = a.ActionTypeID, ActionType = a.ActionType, ResponsibleID = a.ResponsibleID, ResponsibleCode = (string?)null, ResponsibleName = (string?)null, IsOfficeAction = a.IsOfficeAction });
            var noCountry = actionTypes.Where(a => string.IsNullOrEmpty(a.Country) && !hasCountry.Any(c => c.ActionType == a.ActionType))
                                .Select(a => new { ActionTypeID = a.ActionTypeID, ActionType = a.ActionType, ResponsibleID = a.ResponsibleID, ResponsibleCode = (string?)null, ResponsibleName = (string?)null, IsOfficeAction = a.IsOfficeAction });

            return Json(await hasCountry.Union(noCountry).OrderBy(a => a.ActionType).ToListAsync());
        }

        //zoom from Action Due screen
        [HttpGet()]
        public async Task<IActionResult> DetailLink(string actionType, string country)
        {
            if (!string.IsNullOrEmpty(actionType))
            {
                var actionTypes = await _actionTypeService.QueryableList.Where(a => a.ActionType == actionType && (a.Country == country || string.IsNullOrEmpty(a.Country)) && (a.CDueId == 0 || a.CDueId == null)).ToListAsync();
                if (actionTypes.Any()) {
                    var actionTypeId = 0;
                    var matchCountry = actionTypes.FirstOrDefault(a => a.Country == country);
                    if (matchCountry != null)
                        actionTypeId = matchCountry.ActionTypeID;
                    else
                        actionTypeId = actionTypes.FirstOrDefault().ActionTypeID;

                    if (actionTypeId > 0)
                        return RedirectToAction(nameof(Detail), new { id = actionTypeId, singleRecord = true, fromSearch = true });
                }
            }
            if ((await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.FullModify)).Succeeded)
                return RedirectToAction(nameof(Add), new { fromSearch = true, actionType = actionType, country = country });
            else
                return new RecordDoesNotExistResult();
        }

        [HttpGet()]
        public async Task<IActionResult> DetailLinkId(int id)
        {
            if (id > 0)
            {
                var actionType = await _actionTypeService.QueryableList.Where(a => a.ActionTypeID == id).FirstOrDefaultAsync();
                if (actionType != null)
                    return RedirectToAction(nameof(Detail), new { id = actionType.ActionTypeID, singleRecord = true, fromSearch = true });
            }
            return new RecordDoesNotExistResult();
        }

        private async Task ExtractCopyParams(DetailPageViewModel<ActionTypeViewModel> page)
        {
            var copyOptionsString = TempData["CopyOptions"].ToString();
            ViewBag.CopyOptions = copyOptionsString;
            var copyOptions = JsonConvert.DeserializeObject<ActionTypeCopyViewModel>(copyOptionsString);
            if (copyOptions !=null)
            {

                var source = await _actionTypeService.QueryableList.Where(a => a.ActionTypeID == copyOptions.ActionTypeId).ProjectTo<ActionTypeViewModel>().FirstOrDefaultAsync();
                page.Detail = source;

                page.Detail.ActionTypeID = 0;
                page.Detail.ActionType = copyOptions.ActionType;
                page.Detail.Remarks = copyOptions.CopyRemarks ? source.Remarks : "";
            }
        }

        // Retroactive Action Generation removed during debloat

        #region Action Parameters (child grid)

        public async Task<IActionResult> ActionParametersRead([DataSourceRequest] DataSourceRequest request, int actionTypeId)
        {
            var list = await _actionParamService.QueryableList
                .Where(p => p.ActionTypeID == actionTypeId)
                .ToListAsync();
            return Json(list.ToDataSourceResult(request));
        }

        [Authorize(Policy = PatentAuthorizationPolicy.ActionTypeModify)]
        public async Task<IActionResult> ActionParametersUpdate(int actionTypeId,
            [Bind(Prefix = "updated")] IEnumerable<PatActionParameter> updated,
            [Bind(Prefix = "new")] IEnumerable<PatActionParameter> added,
            [Bind(Prefix = "deleted")] IEnumerable<PatActionParameter> deleted)
        {
            var canDelete = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryCanDelete)).Succeeded;
            if (deleted.Any() && !canDelete)
                return Forbid();

            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _actionParamService.Update(actionTypeId, User.GetUserName(), updated, added, deleted);
                var total = deleted.Count() + updated.Count() + added.Count();
                var success = total == 1
                    ? _localizer["Action Parameter has been saved successfully."].ToString()
                    : _localizer["Action Parameters have been saved successfully"].ToString();
                return Ok(new { success });
            }
            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> ActionParametersDelete([Bind(Prefix = "deleted")] PatActionParameter deleted)
        {
            if (deleted.ActParamId > 0)
            {
                await _actionParamService.Update(deleted.ActionTypeID, User.GetUserName(),
                    new List<PatActionParameter>(), new List<PatActionParameter>(), new List<PatActionParameter> { deleted });
                return Ok(new { success = _localizer["Action Parameter has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        #endregion
    }
}
