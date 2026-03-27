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
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Services;

using Newtonsoft.Json;
using R10.Web.Areas;

namespace R10.Web.Areas.Trademark.Controllers
{
    [Area("Trademark"), Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessAuxiliary)]
    public class CaseTypeController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<TmkCaseType> _viewModelService;
        private readonly IEntityService<TmkCaseType> _auxService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "tmkCaseTypeDetail";
        private readonly IReportService _reportService;

        public CaseTypeController(
            IAuthorizationService authService,
            IViewModelService<TmkCaseType> viewModelService,
            IEntityService<TmkCaseType> auxService,
            IStringLocalizer<SharedResource> localizer,
            IReportService reportService)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _auxService = auxService;
            _localizer = localizer;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "tmkCaseTypeSearch",
                Title = _localizer["Case Type Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded
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
                PageId = "tmkCaseTypeSearchResults",
                Title = _localizer["Case Type Search Results"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded
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
                var tmkCaseTypes = _viewModelService.AddCriteria(mainSearchFilters);
                var result = await _viewModelService.CreateViewModelForGrid(request, tmkCaseTypes, "CaseType", "CaseType");
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<DetailPageViewModel<TmkCaseType>> PrepareEditScreen(string id)
        {
            var viewModel = new DetailPageViewModel<TmkCaseType>
            {
                Detail = await _auxService.QueryableList.FirstOrDefaultAsync(c => c.CaseType == id)
            };

            if (viewModel.Detail != null)
            {
                viewModel.AddTrademarkAuxiliarySecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = _dataContainer;

                viewModel.CopyScreenUrl = $"{viewModel.CopyScreenUrl}/{id}";
                viewModel.EditScreenUrl = this.Url.Action("Detail", new { id = id });
                viewModel.SearchScreenUrl = this.Url.Action("Index");
            }
            return viewModel;
        }

        public async Task<IActionResult> Detail(string id, bool singleRecord = false, bool fromSearch = false)
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            TmkCaseType detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Case Type Detail"].ToString(),
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

        private async Task<DetailPageViewModel<TmkCaseType>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<TmkCaseType>
            {
                Detail = new TmkCaseType()
            };

            viewModel.AddTrademarkAuxiliarySecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            if (TempData["CopyOptions"] != null)
                await ExtractCopyParams(page);

            TmkCaseType detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Case Type"].ToString(),
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };
            ModelState.Clear();

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkCaseType caseType)
        {
            if (ModelState.IsValid)
            {
                var userName = User.GetUserName();
                var now = DateTime.Now;
                caseType.UserID = userName;
                caseType.LastUpdate = now;

                var existing = await _auxService.QueryableList.AsNoTracking().FirstOrDefaultAsync(c => c.CaseType == caseType.CaseType);
                if (existing != null)
                    await _auxService.Update(caseType);
                else
                {
                    caseType.DateCreated = now;
                    await _auxService.Add(caseType);
                }

                return Json(caseType.CaseType);
            }
            else
                return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var entity = await _auxService.QueryableList.FirstOrDefaultAsync(c => c.CaseType == id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            await _auxService.Delete(entity);

            return Ok();
        }

        [HttpGet()]
        public async Task<IActionResult> Copy(string id)
        {
            var entity = await _auxService.QueryableList.FirstOrDefaultAsync(c => c.CaseType == id);
            if (entity == null) return new RecordDoesNotExistResult();

            var viewModel = new CaseTypeCopyViewModel
            {
                OriginalCaseType = entity.CaseType,
                CaseType = entity.CaseType
            };
            return PartialView("_Copy", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCopied([FromBody] CaseTypeCopyViewModel copy)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });
            TempData["CopyOptions"] = JsonConvert.SerializeObject(copy);
            return RedirectToAction("Add");
        }

        private async Task ExtractCopyParams(DetailPageViewModel<TmkCaseType> page)
        {
            var copyOptionsString = TempData["CopyOptions"].ToString();
            ViewBag.CopyOptions = copyOptionsString;
            var copyOptions = JsonConvert.DeserializeObject<CaseTypeCopyViewModel>(copyOptionsString);
            if (copyOptions != null)
            {
                var source = await _auxService.QueryableList.AsNoTracking().FirstOrDefaultAsync(c => c.CaseType == copyOptions.OriginalCaseType);
                if (source != null)
                {
                    page.Detail = source;
                    page.Detail.CaseType = copyOptions.CaseType;
                }
            }
        }

        [HttpGet]
        public IActionResult Print()
        {
            ViewBag.Url = Url.Action("Print");
            ViewBag.DownloadName = "Case Type Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintViewModel tmkCaseTypePrintModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(tmkCaseTypePrintModel, ReportType.TmkCaseTypePrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_auxService.QueryableList, request, property, text, filterType, requiredRelation);
        }

        [Route("Trademark/CaseType/GetCaseTypesList"), Route("Trademark/CaseType/GetCaseTypeList")]
        public async Task<IActionResult> GetCaseTypesList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_auxService.QueryableList, request, property, text, filterType, new string[] { "CaseType", "Description" }, requiredRelation);
        }

        public async Task<IActionResult> GetCaseTypesByCountry(string country)
        {
            var caseTypes = _auxService.QueryableList;
            var list = await caseTypes.Select(c => new { CaseType = c.CaseType, Description = c.Description }).OrderBy(c => c.CaseType).ToListAsync();
            return Json(list);
        }

        [HttpGet()]
        public async Task<IActionResult> DetailLink(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var entity = await _viewModelService.GetEntityByCode("CaseType", id);
                if (entity == null)
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction(nameof(Detail), new { id = entity.CaseType, singleRecord = true, fromSearch = true });
            }
            else
                return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        public async Task<IActionResult> GetActiveCaseTypeList()
        {
            var list = await _auxService.QueryableList.Select(c => new { CaseType = c.CaseType, Description = c.Description }).OrderBy(c => c.CaseType).ToListAsync();

            return Json(list);
        }

    }
}