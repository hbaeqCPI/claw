using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Core.Helpers;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;

using Newtonsoft.Json;
using R10.Web.Areas;

namespace R10.Web.Areas.Patent.Controllers
{
    [Area("Patent"), Authorize(Policy = PatentAuthorizationPolicy.CanAccessAuxiliary)]
    public class CountryDueController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<PatCountryDue> _viewModelService;
        private readonly IEntityService<PatCountryDue> _auxService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        private readonly string _dataContainer = "patCountryDueDetail";

        public CountryDueController(
            IAuthorizationService authService,
            IViewModelService<PatCountryDue> viewModelService,
            IEntityService<PatCountryDue> auxService,
            IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _auxService = auxService;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "patCountryDueSearch",
                Title = _localizer["Country Due Search"].ToString(),
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
                PageId = "patCountryDueSearchResults",
                Title = _localizer["Country Due Search Results"].ToString(),
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
                var entities = _viewModelService.AddCriteria(mainSearchFilters);
                var result = await _viewModelService.CreateViewModelForGrid(request, entities, "ActionType", "CDueId");
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<DetailPageViewModel<PatCountryDue>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<PatCountryDue>
            {
                Detail = await GetById(id)
            };

            if (viewModel.Detail != null)
            {
                viewModel.AddPatentAuxiliarySecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = _dataContainer;

                viewModel.EditScreenUrl = this.Url.Action("Detail", new { id = id });
                viewModel.CopyScreenUrl = $"{viewModel.CopyScreenUrl}/{id}";
                viewModel.SearchScreenUrl = this.Url.Action("Index");
            }
            return viewModel;
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false)
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            PatCountryDue detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Country Due Detail"].ToString(),
                RecordId = detail.CDueId,
                SingleRecord = singleRecord || !Request.IsAjax(),
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

        [HttpPost()]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        private async Task<DetailPageViewModel<PatCountryDue>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<PatCountryDue>
            {
                Detail = new PatCountryDue()
            };

            viewModel.AddPatentAuxiliarySecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            if (TempData["CopyOptions"] != null)
                await ExtractCopyParams(page);

            PatCountryDue detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Country Due"].ToString(),
                RecordId = detail.CDueId,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };
            ModelState.Clear();

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatCountryDue entity)
        {
            if (ModelState.IsValid)
            {
                var userName = User.GetUserName();
                var now = DateTime.Now;
                entity.UserID = userName;
                entity.LastUpdate = now;
                if (entity.CDueId <= 0)
                    entity.DateCreated = now;

                if (entity.CDueId > 0)
                    await _auxService.Update(entity);
                else
                    await _auxService.Add(entity);

                return Json(entity.CDueId);
            }
            else
                return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await GetById(id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            await _auxService.Delete(entity);

            return Ok();
        }

        private async Task<PatCountryDue> GetById(int id)
        {
            return await _auxService.QueryableList.SingleOrDefaultAsync((c => c.CDueId == id));
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_auxService.QueryableList, request, property, text, filterType, requiredRelation);
        }

        [HttpGet()]
        public async Task<IActionResult> Copy(int id)
        {
            var entity = await GetById(id);
            if (entity == null) return new RecordDoesNotExistResult();
            var viewModel = new CountryDueCopyViewModel
            {
                CDueId = entity.CDueId,
                Country = entity.Country,
                CaseType = entity.CaseType,
                ActionType = entity.ActionType,
                ActionDue = entity.ActionDue,
                BasedOn = entity.BasedOn,
                Yr = entity.Yr,
                Mo = entity.Mo,
                Dy = entity.Dy,
                Indicator = entity.Indicator,
                Recurring = entity.Recurring,
                EffBasedOn = entity.EffBasedOn,
                EffStartDate = entity.EffStartDate,
                EffEndDate = entity.EffEndDate
            };
            return PartialView("_Copy", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCopied([FromBody] CountryDueCopyViewModel copy)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });
            TempData["CopyOptions"] = JsonConvert.SerializeObject(copy);
            return RedirectToAction("Add");
        }

        private async Task ExtractCopyParams(DetailPageViewModel<PatCountryDue> page)
        {
            var copyOptionsString = TempData["CopyOptions"].ToString();
            ViewBag.CopyOptions = copyOptionsString;
            var copyOptions = JsonConvert.DeserializeObject<CountryDueCopyViewModel>(copyOptionsString);
            if (copyOptions != null)
            {
                var source = await _auxService.QueryableList.AsNoTracking().FirstOrDefaultAsync(c => c.CDueId == copyOptions.CDueId);
                if (source != null)
                {
                    page.Detail = source;
                    page.Detail.CDueId = 0;
                    page.Detail.Country = copyOptions.Country;
                    page.Detail.CaseType = copyOptions.CaseType;
                    page.Detail.ActionType = copyOptions.ActionType;
                    page.Detail.ActionDue = copyOptions.ActionDue;
                    page.Detail.BasedOn = copyOptions.BasedOn;
                    page.Detail.Yr = copyOptions.Yr;
                    page.Detail.Mo = copyOptions.Mo;
                    page.Detail.Dy = copyOptions.Dy;
                    page.Detail.Indicator = copyOptions.Indicator;
                    page.Detail.Recurring = copyOptions.Recurring;
                    page.Detail.EffBasedOn = copyOptions.EffBasedOn;
                    page.Detail.EffStartDate = copyOptions.EffStartDate;
                    page.Detail.EffEndDate = copyOptions.EffEndDate;
                    page.Detail.CPIAction = false;
                    page.Detail.CPIPermanentID = 0;
                }
            }
        }

        [HttpGet()]
        public async Task<IActionResult> DetailLink(string id)
        {
            if (!string.IsNullOrEmpty(id) && id != "0")
            {
                var entity = await _viewModelService.GetEntityByCode("CDueId", id);
                if (entity == null)
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction(nameof(Detail), new { id = entity.CDueId, singleRecord = true, fromSearch = true });
            }
            else
                return RedirectToAction(nameof(Add), new { fromSearch = true });
        }
    }
}