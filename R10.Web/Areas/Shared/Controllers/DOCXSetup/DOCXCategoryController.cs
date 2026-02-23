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
    public class DOCXCategoryController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<DOCXCategory> _viewModelService;
        private readonly IAsyncRepository<DOCXCategory> _repository;
        private readonly string _dataContainer = "docxCategoryDetailsView";
        private readonly IDOCXService _docxService;

        public DOCXCategoryController(IAuthorizationService authService, IViewModelService<DOCXCategory> viewModelService, IAsyncRepository<DOCXCategory> repository, IDOCXService docxService)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _repository = repository;
            _docxService = docxService;
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
            int[] ids = categories.Select(c => c.DOCXCatId).ToArray();

            if (ids.Length == 0)
                return new NoRecordFoundResult();
            else if (ids.Length == 1)
            {
                return RedirectToAction(nameof(Detail), new { id = ids[0], singleRecord = true });
            }
            ViewBag.SearchUrl = $"{Request.PathBase}/shared/docxtercategory/search".ToLower();
            ViewBag.PageSize = GetSearchPageSize();
            return PartialView("_SearchResult");
        }

        public IActionResult ReadCategoryGrid([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var categories = _viewModelService.AddCriteria(mainSearchFilters);
                var result = _viewModelService.CreateViewModelForGrid(request, categories, "DOCXCatDesc", "DOCXCatId");
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
                var entity = await _viewModelService.GetEntityByCode("DOCXCatDesc", category);
                if (entity?.DOCXCatId == null)
                    return new NoRecordFoundResult();
                else
                {
                    return RedirectToAction(nameof(Detail), new { id = entity.DOCXCatId, singleRecord = true, fromSearch = true });
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

            ViewBag.DeleteHandler = "docxCategoryPage.deleteMainRecord";
            return PartialView("_SimpleDeletePrompt");
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Policy = SharedAuthorizationPolicy.CanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int docxCatId, string docxCatDesc, string rowVersion)
        {
            if (ModelState.IsValid)
            {
                var category = new DOCXCategory {  DOCXCatId = docxCatId, DOCXCatDesc = docxCatDesc, tStamp = System.Convert.FromBase64String(rowVersion) };

                await _repository.DeleteAsync(category);
                return Json(new { id = category.DOCXCatId });
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] DOCXCategory category)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(category, category.DOCXCatId);

                if (category.DOCXCatId > 0)
                    await _repository.UpdateAsync(category);
                else
                    await _repository.AddAsync(category);

                return Json(category.DOCXCatId);
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

        //public async Task<IActionResult> GetCategoryList(int? screenId)//string property, string text, FilterType filterType, string requiredRelation = "")
        //{
        //    if (screenId == null) return Json(new List<DOCXCategory>());

        //    var categories = await _repository.QueryableList.ToListAsync();

        //    var nonEfs = categories.Where(c => c.EfsDocId == 0 || c.EfsDocId == null).Select(c => (int)c.DOCXCatId); //no need to check non EFS forms
        //    var categoryInUse = await _docxService.DOCXesMain.Where(d => d.SystemScreen.ScreenId == screenId && d.SystemScreen.ScreenName == "Country Application").Select(d => d.DOCXCatId).Distinct().ToListAsync(); 
        //    if (categoryInUse.Any()) categoryInUse = categoryInUse.Except(nonEfs).ToList(); //remove all EFS forms in use on Country Application to avoid duplicate

        //    var result =  categories.Where(c => !categoryInUse.Any() || !categoryInUse.Contains(c.DOCXCatId)).OrderBy(c => c.EfsDocId).ThenBy(c => c.DOCXCatDesc).Select(c => new { DOCXCatId = c.DOCXCatId, DOCXCatDesc = c.DOCXCatDesc });

        //    return Json(result);
        //}

        public async Task<IActionResult> GetCategoryList(string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return Json(await _repository.QueryableList.ToListAsync());
        }

        private async Task<DetailPageViewModel<DOCXCategory>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<DOCXCategory>();
            viewModel.Detail = await _repository.QueryableList.SingleOrDefaultAsync(c => c.DOCXCatId == id);

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

        private async Task<DetailPageViewModel<DOCXCategory>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<DOCXCategory>();
            viewModel.Detail = new DOCXCategory();

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }
    }
}