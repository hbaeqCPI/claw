using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Security;
using AutoMapper;
using System.Reflection;
using System.Text;
using R10.Web.Filters;
using System.Linq.Expressions;
using R10.Core.Services.Shared;
using R10.Core.Identity;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class ModuleController : BaseController
    {
        private readonly IAsyncRepository<ModuleMain> _repository;
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<ModuleMain> _viewModelService;
        private readonly string _dataContainer = "QEModuleDetailsView";

        public ModuleController(IAsyncRepository<ModuleMain> repository, IAuthorizationService authService,
            IViewModelService<ModuleMain> viewModelService)
        {
            _repository = repository;
            _authService = authService;
            _viewModelService = viewModelService;
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
            var modules = _viewModelService.AddCriteria(mainSearchFilters);
            int[] ids = modules.Select(c => c.ModuleId).ToArray();

            if (ids.Length == 0)
                return new NoRecordFoundResult();
            else if (ids.Length == 1)
            {
                return RedirectToAction(nameof(Detail), new { id = ids[0], singleRecord = true });
            }
            ViewBag.SearchUrl = $"{Request.PathBase}/shared/quickemail/ModuleMain/search".ToLower();
            ViewBag.PageSize = GetSearchPageSize();
            return PartialView("_SearchResult");
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var module = _viewModelService.AddCriteria(mainSearchFilters);
                var result = await _viewModelService.CreateViewModelForGrid(request, module, "moduleName", "ModuleId");
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
        public async Task<IActionResult> DetailLink(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                var entity = await _viewModelService.GetEntityByCode("ModuleName", value);
                if (entity?.ModuleId == null)
                    return new NoRecordFoundResult();
                else
                {
                    return RedirectToAction(nameof(Detail), new { id = entity.ModuleId, singleRecord = true, fromSearch = true });
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

            var module = viewModel.Detail;
            var initialize = singleRecord || !Request.IsAjax() || fromSearch;
            if (initialize)
            {
                ViewBag.FromSearch = fromSearch;
                ViewBag.SingleRecord = singleRecord || !Request.IsAjax();
                ViewBag.Url = $"{Request.PathBase}{Request.Path}".ToLower();
                return View(module);
            }
            else
            {
                ViewBag.Url = $"{Request.PathBase}{Request.Path}{Request.QueryString}".ToLower();
            }
            return PartialView("_DetailContent", module);

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

            ViewBag.DeleteHandler = "QEModulePage.deleteMainRecord";
            return PartialView("_SimpleDeletePrompt");
        }


        [HttpPost, ActionName("Delete")]
        [Authorize(Policy = SharedAuthorizationPolicy.CanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int ModuleId, string moduleName, string rowVersion)
        {
            if (ModelState.IsValid)
            {
                var module = new ModuleMain { ModuleId = ModuleId, ModuleName = moduleName, tStamp = System.Convert.FromBase64String(rowVersion) };

                await _repository.DeleteAsync(module);
                return Json(new { id = module.ModuleId });
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] ModuleMain module)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(module, module.ModuleId);

                if (module.ModuleId > 0)
                    await _repository.UpdateAsync(module);
                else
                    await _repository.AddAsync(module);

                return Json(module.ModuleId);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var module = await _repository.GetByIdAsync(id);
            if (module == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = module.CreatedBy, dateCreated = module.DateCreated, updatedBy = module.UpdatedBy, lastUpdate = module.LastUpdate, tStamp = module.tStamp });
        }

        public async Task<IActionResult> GetPicklistData(string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var module = _repository.QueryableList;
            var result = await QueryHelper.GetPicklistDataAsync(module, property, text, filterType, requiredRelation);
            return Json(result);
        }

        public IActionResult GetModulesList(string textProperty, string text, FilterType filterType, string requiredRelation = "")
        {
            var modules = _repository.QueryableList;
            modules = QueryHelper.BuildCriteria(modules, textProperty, text, filterType, requiredRelation);
            var list = modules.Select(l => new { ModuleId = l.ModuleId, ModuleName = l.ModuleName }).OrderBy(l => l.ModuleId).ToList();
            return Json(list);
        }

        private async Task<DetailPageViewModel<ModuleMain>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<ModuleMain>();
            viewModel.Detail = await _repository.QueryableList.SingleOrDefaultAsync(c => c.ModuleId == id);

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

        private async Task<DetailPageViewModel<ModuleMain>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<ModuleMain>();
            viewModel.Detail = new ModuleMain();

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }
    }



}