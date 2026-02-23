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
    public class QEDataSourceController : BaseController
    {
        private readonly IAsyncRepository<QEDataSource> _repository;
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<QEDataSource> _viewModelService;
        private readonly string _dataContainer = "qeDataSourceDetailsView";

        public QEDataSourceController(IAsyncRepository<QEDataSource> repository, IAuthorizationService authService,
            IViewModelService<QEDataSource> viewModelService)
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
            var dataSources = _viewModelService.AddCriteria(mainSearchFilters);
            int[] ids = dataSources.Select(c => c.DataSourceID).ToArray();

            if (ids.Length == 0)
                return new NoRecordFoundResult();
            else if (ids.Length == 1)
            {
                return RedirectToAction(nameof(Detail), new { id = ids[0], singleRecord = true });
            }
            ViewBag.SearchUrl = $"{Request.PathBase}/shared/quickemail/qedatasource/search".ToLower();
            ViewBag.PageSize = GetSearchPageSize();
            return PartialView("_SearchResult");
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var dataSource = _viewModelService.AddCriteria(mainSearchFilters);
                var result = await _viewModelService.CreateViewModelForGrid(request, dataSource, "dataSourceName", "dataSourceID");
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
        public IActionResult DetailLink(int? id)
        {
            if (id > 0)
            {
                return RedirectToAction(nameof(Detail), new { id = id, singleRecord = true, fromSearch = true });
            }
            else
            {
                return RedirectToAction(nameof(Add), new { fromSearch = true });
            }
        }

        //[HttpGet()]
        //public async Task<IActionResult> DetailLink(string value)
        //{
        //    if (!String.IsNullOrEmpty(value))
        //    {
        //        var entity = await _viewModelService.GetEntityByCode("DataSourceName", value);
        //        if (entity?.DataSourceID == null)
        //            return new NoRecordFoundResult();
        //        else
        //        {
        //            return RedirectToAction(nameof(Detail), new { id = entity.DataSourceID, singleRecord = true, fromSearch = true });
        //        }
        //    }
        //    else
        //    {
        //        return RedirectToAction(nameof(Add), new { fromSearch = true });
        //    }
        //}

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false)
        {
            var viewModel = await PrepareEditScreen(id);

            if (viewModel.Detail == null)
                return new NoRecordFoundResult();

            ViewBag.PermissionViewModel = viewModel;

            var dataSource = viewModel.Detail;
            var initialize = singleRecord || !Request.IsAjax() || fromSearch;
            if (initialize)
            {
                ViewBag.FromSearch = fromSearch;
                ViewBag.SingleRecord = singleRecord || !Request.IsAjax();
                ViewBag.Url = $"{Request.PathBase}{Request.Path}".ToLower();
                return View(dataSource);
            }
            else
            {
                ViewBag.Url = $"{Request.PathBase}{Request.Path}{Request.QueryString}".ToLower();
            }
            return PartialView("_DetailContent", dataSource);

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

            ViewBag.DeleteHandler = "qeDataSourcePage.deleteMainRecord";
            return PartialView("_SimpleDeletePrompt");
        }


        [HttpPost, ActionName("Delete")]
        [Authorize(Policy = SharedAuthorizationPolicy.CanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int dataSourceID, string dataSourceName, string rowVersion)
        {
            if (ModelState.IsValid)
            {
                var dataSource = new QEDataSource { DataSourceID = dataSourceID, DataSourceName = dataSourceName, tStamp = System.Convert.FromBase64String(rowVersion) };

                await _repository.DeleteAsync(dataSource);
                return Json(new { id = dataSource.DataSourceID });
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] QEDataSource dataSource)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(dataSource, dataSource.DataSourceID);

                if (dataSource.DataSourceID > 0)
                    await _repository.UpdateAsync(dataSource);
                else
                    await _repository.AddAsync(dataSource);

                return Json(dataSource.DataSourceID);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var dataSource = await _repository.GetByIdAsync(id);
            if (dataSource == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = dataSource.CreatedBy, dateCreated = dataSource.DateCreated, updatedBy = dataSource.UpdatedBy, lastUpdate = dataSource.LastUpdate, tStamp = dataSource.tStamp });
        }

        public async Task<IActionResult> GetPicklistData(string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var dataSource = _repository.QueryableList;
            var result = await QueryHelper.GetPicklistDataAsync(dataSource, property, text, filterType, requiredRelation);
            return Json(result);
        }

        public IActionResult GetDataSourceList(string textProperty, string text, FilterType filterType, string requiredRelation = "")
        {
            var dataSource = _repository.QueryableList;
            dataSource = QueryHelper.BuildCriteria(dataSource, textProperty, text, filterType, requiredRelation);
            var list = dataSource.Select(l => new { DataSourceID = l.DataSourceID, DataSourceName = l.DataSourceName }).OrderBy(l => l.DataSourceID).ToList();
            return Json(list);
        }

        private async Task<DetailPageViewModel<QEDataSource>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<QEDataSource>();
            viewModel.Detail = await _repository.QueryableList.SingleOrDefaultAsync(c => c.DataSourceID == id);

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

        private async Task<DetailPageViewModel<QEDataSource>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<QEDataSource>();
            viewModel.Detail = new QEDataSource();

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }
    }



}