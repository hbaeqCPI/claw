using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Security;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessLetters)]
    public class LetterCategoryController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<LetterCategory> _viewModelService;
        private readonly IAsyncRepository<LetterCategory> _repository;
        private readonly string _dataContainer = "letCategoryDetailsView";

        public LetterCategoryController(IAuthorizationService authService, IViewModelService<LetterCategory> viewModelService, IAsyncRepository<LetterCategory> repository)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _repository = repository;
        }

        [HttpGet()]
        public async Task<IActionResult> Search()
        {
            var canAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded;
            ViewBag.CanAddRecord = canAddRecord;

            if (canAddRecord)
                ViewBag.AddScreenUrl = Url.Action(nameof(Add), new { fromSearch = true });
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var categories = _viewModelService.AddCriteria(mainSearchFilters);
            int[] ids = categories.Select(c => c.LetCatId).ToArray();

            if (ids.Length == 0)
                return new NoRecordFoundResult();
            else if (ids.Length == 1)
            {
                return RedirectToAction(nameof(Detail), new { id = ids[0], singleRecord = true });
            }
            ViewBag.SearchUrl = $"{Request.PathBase}/shared/lettercategory/search".ToLower();
            ViewBag.PageSize = GetSearchPageSize();
            return PartialView("_SearchResult");
        }

        public IActionResult ReadCategoryGrid([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var categories = _viewModelService.AddCriteria(mainSearchFilters);
                var result = _viewModelService.CreateViewModelForGrid(request, categories, "LetCatDesc", "LetCatId");
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
        public async Task<IActionResult> DetailLink(string category)
        {
            if (category != null && category.Length > 0)
            {
                var entity = await _viewModelService.GetEntityByCode("LetCatDesc", category);
                if (entity?.LetCatId == null)
                    return new NoRecordFoundResult();
                else
                {
                    return RedirectToAction(nameof(Detail), new { id = entity.LetCatId, singleRecord = true, fromSearch = true });
                }
            }
            else
            {
                return RedirectToAction(nameof(Add), new { fromSearch = true });
            }
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false)
        {
            var viewModel = await PrepareEditScreen(id);

            if (viewModel.Detail == null)
                return new NoRecordFoundResult();

            ViewBag.PermissionViewModel = viewModel;

            var categoryViewModel = viewModel.Detail;
            var initialize = singleRecord || !Request.IsAjax() || fromSearch;
            if (initialize)
            {
                ViewBag.FromSearch = fromSearch;
                ViewBag.SingleRecord = singleRecord || !Request.IsAjax();
                ViewBag.Url = $"{Request.PathBase}{Request.Path}".ToLower();
                return View(categoryViewModel);
            }
            else
            {
                ViewBag.Url = $"{Request.PathBase}{Request.Path}{Request.QueryString}".ToLower();
            }
            return PartialView("_DetailContent", categoryViewModel);

        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction(nameof(Search));

            var viewModel = await PrepareAddScreen();
            ViewBag.PermissionViewModel = viewModel;
            ViewBag.FromSearch = fromSearch;
            return PartialView(fromSearch ? "Detail" : "_DetailContent", viewModel.Detail);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanDelete)]
        public IActionResult Delete()
        {
            if (!Request.IsAjax())
                return RedirectToAction(nameof(Search));

            ViewBag.DeleteHandler = "letCategoryPage.deleteMainRecord";
            return PartialView("_SimpleDeletePrompt");
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Policy = SharedAuthorizationPolicy.CanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int letCatId, string letCatDesc, string rowVersion)
        {
            if (ModelState.IsValid)
            {
                var category = new LetterCategory {  LetCatId = letCatId, LetCatDesc = letCatDesc, tStamp = System.Convert.FromBase64String(rowVersion) };

                await _repository.DeleteAsync(category);
                return Json(new { id = category.LetCatId });
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] LetterCategory category)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(category, category.LetCatId);

                if (category.LetCatId > 0)
                    await _repository.UpdateAsync(category);
                else
                    await _repository.AddAsync(category);

                return Json(category.LetCatId);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = category.CreatedBy, dateCreated = category.DateCreated, updatedBy = category.UpdatedBy, lastUpdate = category.LastUpdate, tStamp = category.tStamp });
        }

        public async Task<IActionResult> GetCategoryList(string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var categories = _repository.QueryableList;
            var result = categories.Select(c => new {LetCatId = c.LetCatId, LetCatDesc = c.LetCatDesc}).OrderBy(c => c.LetCatDesc);
            return Json(await result.ToListAsync());
        }

        public async Task<IActionResult> GetCategoryListBySystem(string property, string text, FilterType filterType, string system = "", string requiredRelation = "")
        {
            var categories = _repository.QueryableList.Where(c => c.Systems.Contains(system));
            var result = categories.Select(c => new { LetCatId = c.LetCatId, LetCatDesc = c.LetCatDesc }).OrderBy(c => c.LetCatDesc);
            return Json(await result.ToListAsync());
        }

        //public async Task<IActionResult> GetCategoriesList()
        //{
        //    return Json(await _repository.QueryableList.ToListAsync());
        //}

        private async Task<DetailPageViewModel<LetterCategory>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<LetterCategory>();
            viewModel.Detail = await _repository.QueryableList.SingleOrDefaultAsync(c => c.LetCatId == id);

            if (viewModel.Detail != null)
            {
                viewModel.AddSharedSecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                //hide copy and email buttons
                viewModel.CanCopyRecord = false;
                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.Container = _dataContainer;
            }
            return viewModel;
        }

        private async Task<DetailPageViewModel<LetterCategory>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<LetterCategory>();
            viewModel.Detail = new LetterCategory();

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }
    }
}