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
using R10.Core.Helpers;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class CurrencyTypeController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IEntityService<CurrencyType> _auxService;
        private readonly IViewModelService<CurrencyType> _viewModelService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IReportService _reportService;

        private readonly string _dataContainer = "currencyTypeDetail";

        public CurrencyTypeController(
            IAuthorizationService authService,
            IEntityService<CurrencyType> auxService,
            IViewModelService<CurrencyType> viewModelService,
            IReportService reportService,
            IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _auxService = auxService;
            _viewModelService = viewModelService;
            _reportService = reportService;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "currencyTypeSearch",
                Title = _localizer["Currency Search"].ToString(),
                CanAddRecord = await CanModify()
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
                PageId = "currencyTypeSearchResults",
                Title = _localizer["Currency Search Results"].ToString(),
                CanAddRecord = await CanModify()
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
                var currencyTypes = _viewModelService.AddCriteria(mainSearchFilters);
                var result = await _viewModelService.CreateViewModelForGrid(request, currencyTypes, "currencyTypeCode", "keyID");
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

        [HttpGet()]
        public async Task<IActionResult> DetailLink(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                var entity = await _viewModelService.GetEntityByCode("CurrencyTypeCode", id);
                if (entity == null)
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction(nameof(Detail), new { id = entity.KeyID, singleRecord = true, fromSearch = true });
            }
            else if (await CanModify())
                return RedirectToAction(nameof(Add), new { fromSearch = true });
            else
                return new RecordDoesNotExistResult();
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
                Title = _localizer["Currency Detail"].ToString(),
                RecordId = detail.KeyID,
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
            ViewBag.DownloadName = "Currency Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintViewModel sharedCurrencyTypePrintModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(sharedCurrencyTypePrintModel, ReportType.SharedCurrencyTypePrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        //allow pat/tmk/gm cost tracking user
        //[Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(bool fromSearch = false)
        {
            if (!(await CanModify()))
                return Forbid();

            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Currency"].ToString(),
                RecordId = detail.KeyID,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };

            return PartialView("Index", model);
        }

        //allow pat/tmk/gm cost tracking user
        //[HttpPost, Authorize(Policy = SharedAuthorizationPolicy.CanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string tStamp)
        {
            if (!(await CanDelete()))
                return Forbid();

            var entity = await _auxService.GetByIdAsync(id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            entity.tStamp = Convert.FromBase64String(tStamp);
            await _auxService.Delete(entity);

            return Ok();
        }

        //allow pat/tmk/gm cost tracking user
        //[HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] CurrencyType currencyType)
        {
            if (!(await CanModify()))
                return Forbid();

            if (ModelState.IsValid)
            {
                UpdateEntityStamps(currencyType, currencyType.KeyID);

                if (currencyType.KeyID > 0)
                {
                    var oldExchangeRate = await _auxService.QueryableList.AnyAsync(d => d.KeyID == currencyType.KeyID && d.ExchangeRate != currencyType.ExchangeRate);
                    if (oldExchangeRate)
                    {
                        currencyType.ExRateUpdatedBy = currencyType.UpdatedBy;
                        currencyType.ExRateLastUpdate = currencyType.LastUpdate;
                    }

                    await _auxService.Update(currencyType);
                }                    
                else
                {
                    currencyType.ExRateUpdatedBy = currencyType.UpdatedBy;
                    currencyType.ExRateLastUpdate = currencyType.LastUpdate;

                    await _auxService.Add(currencyType);
                }                    

                return Json(currencyType.KeyID);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var currencyType = await _auxService.GetByIdAsync(id);
            if (currencyType == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = currencyType.CreatedBy, dateCreated = currencyType.DateCreated, updatedBy = currencyType.UpdatedBy, lastUpdate = currencyType.LastUpdate, tStamp = currencyType.tStamp });
        }

        public async Task<IActionResult> GetPicklistData(string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var currencyTypes = _auxService.QueryableList;
            var result = await QueryHelper.GetPicklistDataAsync(currencyTypes, property, text, filterType, requiredRelation);
            return Json(result);
        }

        public async Task<IActionResult> GetCurrencyTypeList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_auxService.QueryableList.Select(s => new { CurrencyType = s.CurrencyTypeCode, Description = s.Description, ExchangeRate = s.ExchangeRate, AllowanceRate = s.AllowanceRate }), request, property, text, filterType, requiredRelation, false);
        }

        private async Task<DetailPageViewModel<CurrencyType>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<CurrencyType>();
            viewModel.Detail = await _auxService.QueryableList.SingleOrDefaultAsync(c => c.KeyID == id);

            if (viewModel.Detail != null)
            {
                viewModel.AddSharedSecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                //override to allow cost tracking user
                var costTrackingAuxModify = CostTrackingAuxiliaryModify();
                viewModel.CanAddRecord = viewModel.CanAddRecord || costTrackingAuxModify;
                viewModel.CanEditRecord = viewModel.CanEditRecord || costTrackingAuxModify;
                viewModel.CanDeleteRecord = viewModel.CanDeleteRecord || costTrackingAuxModify;

                //disable delete and edit if CPI currency type
                viewModel.CanEditRecord = viewModel.CanEditRecord && !viewModel.Detail.CPICurrencyType;
                viewModel.CanDeleteRecord = viewModel.CanDeleteRecord && !viewModel.Detail.CPICurrencyType;

                //hide copy and email buttons
                viewModel.CanCopyRecord = false;
                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.SearchScreenUrl = this.Url.Action("Index");
                viewModel.Container = _dataContainer;
            }
            return viewModel;
        }

        private async Task<DetailPageViewModel<CurrencyType>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<CurrencyType>();
            viewModel.Detail = new CurrencyType();

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            //override to allow cost tracking user
            var costTrackingAuxModify = CostTrackingAuxiliaryModify();
            viewModel.CanAddRecord = viewModel.CanAddRecord || costTrackingAuxModify;
            viewModel.CanEditRecord = viewModel.CanEditRecord || costTrackingAuxModify;
            viewModel.CanDeleteRecord = viewModel.CanDeleteRecord || costTrackingAuxModify;

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        private async Task<bool> CanModify()
        {
            return (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded || CostTrackingAuxiliaryModify();
        }

        private async Task<bool> CanDelete()
        {
            return (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.CanDelete)).Succeeded || CostTrackingAuxiliaryModify();
        }

        private bool CostTrackingAuxiliaryModify()
        {
            return User.IsInRoles(CPiPermissions.CostTrackingAuxiliaryModify);
        }
    }
}