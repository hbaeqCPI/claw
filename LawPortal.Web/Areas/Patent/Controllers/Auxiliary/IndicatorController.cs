using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LawPortal.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using LawPortal.Core.Entities.Patent;
using LawPortal.Core.Helpers;
using LawPortal.Core.Interfaces;
using LawPortal.Web.Extensions;
using LawPortal.Web.Extensions.ActionResults;
using LawPortal.Web.Helpers;
using LawPortal.Web.Interfaces;
using LawPortal.Web.Models.PageViewModels;
using LawPortal.Web.Areas.Shared.ViewModels;
using LawPortal.Web.Security;
using LawPortal.Web.Services;

using LawPortal.Web.Areas;

namespace LawPortal.Web.Areas.Patent.Controllers
{
    [Area("Patent"), Authorize(Policy = PatentAuthorizationPolicy.CanAccessAuxiliary)]
    public class IndicatorController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<PatIndicator> _viewModelService;
        private readonly IEntityService<PatIndicator> _indicatorService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        private readonly string _dataContainer = "patIndicatorDetail";

        public IndicatorController(
            IAuthorizationService authService,
            IViewModelService<PatIndicator> viewModelService,
            IEntityService<PatIndicator> indicatorService,
            IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _indicatorService = indicatorService;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "patIndicatorSearch",
                Title = _localizer["Indicator Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded
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
                PageId = "patIndicatorSearchResults",
                Title = _localizer["Indicator Search Results"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded
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
                var indicators = _viewModelService.AddCriteria(mainSearchFilters);
                var result = await _viewModelService.CreateViewModelForGrid(request, indicators, "Indicator", "IndicatorId");
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
                Title = _localizer["Indicator Detail"].ToString(),
                RecordId = detail.IndicatorId,
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

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(string id, bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            var detail = page.Detail;

            if (!string.IsNullOrEmpty(id))
                detail.Indicator = id;

            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Indicator"].ToString(),
                RecordId = detail.IndicatorId,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _indicatorService.QueryableList.FirstOrDefaultAsync(c => c.IndicatorId == id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            await _indicatorService.Delete(entity);

            return Ok();
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatIndicator patIndicator)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(patIndicator, patIndicator.IndicatorId);

                if (patIndicator.IndicatorId > 0)
                    await _indicatorService.Update(patIndicator);
                else
                    await _indicatorService.Add(patIndicator);

                return Json(patIndicator.IndicatorId);
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var patIndicator = await _indicatorService.QueryableList.FirstOrDefaultAsync(i => i.IndicatorId == id);
            if (patIndicator == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = patIndicator.CreatedBy, dateCreated = patIndicator.DateCreated, updatedBy = patIndicator.UpdatedBy, lastUpdate = patIndicator.LastUpdate });
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_indicatorService.QueryableList, request, property, text, filterType, requiredRelation);
        }

        [HttpGet()]
        public async Task<IActionResult> DetailLink(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var entity = await _viewModelService.GetEntityByCode("Indicator", id);
                if (entity == null)
                    return RedirectToAction(nameof(Add), new { id = id, fromSearch = true });
                else
                    return RedirectToAction(nameof(Detail), new { id = entity.IndicatorId, singleRecord = true, fromSearch = true });
            }
            else
                return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        private async Task<DetailPageViewModel<PatIndicator>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<PatIndicator>();
            viewModel.Detail = await _indicatorService.QueryableList.FirstOrDefaultAsync(c => c.IndicatorId == id);

            if (viewModel.Detail != null)
            {
                viewModel.AddPatentAuxiliarySecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanCopyRecord = false;
                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.Container = _dataContainer;
                viewModel.AddScreenUrl = viewModel.CanAddRecord ? Url.Action("Add", new { fromSearch = true }) : "";
                viewModel.SearchScreenUrl = this.Url.Action("Index");
            }
            return viewModel;
        }

        private async Task<DetailPageViewModel<PatIndicator>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<PatIndicator>();
            viewModel.Detail = new PatIndicator();

            viewModel.AddPatentAuxiliarySecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }
    }
}
