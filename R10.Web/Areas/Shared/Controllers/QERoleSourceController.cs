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
using R10.Core.Entities.Shared;
using System.Net.Mail;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)] 
    public class QERoleSourceController : BaseController
    {
        private readonly IAsyncRepository<QERoleSource> _repository;
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<QERoleSource> _viewModelService;

        private readonly string _dataContainer = "QERoleSourceDetailsView";

        public QERoleSourceController(IAsyncRepository<QERoleSource> repository, IAuthorizationService authService,
            IViewModelService<QERoleSource> viewModelService, ISystemSettings<DefaultSetting> settings)
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
            var roleSources = _viewModelService.AddCriteria(mainSearchFilters);
            int[] ids = roleSources.Select(c => c.RoleSourceID).ToArray();

            if (ids.Length == 0)
                return new NoRecordFoundResult();
            else if (ids.Length == 1)
            {
                return RedirectToAction(nameof(Detail), new { id = ids[0], singleRecord = true });
            }
            ViewBag.SearchUrl = $"{Request.PathBase}/shared/quickemail/QERoleSource/search".ToLower();
            ViewBag.PageSize = GetSearchPageSize();
            return PartialView("_SearchResult");
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var roleSource = _viewModelService.AddCriteria(mainSearchFilters);
                var result = await _viewModelService.CreateViewModelForGrid(request, roleSource, "roleSourceName", "RoleSourceID");
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
                var entity = await _viewModelService.GetEntityByCode("RoleSourceName", value);
                if (entity?.RoleSourceID == null)
                    return new NoRecordFoundResult();
                else
                {
                    return RedirectToAction(nameof(Detail), new { id = entity.RoleSourceID, singleRecord = true, fromSearch = true });
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

            var roleSource = viewModel.Detail;
            var initialize = singleRecord || !Request.IsAjax() || fromSearch;
            if (initialize)
            {
                ViewBag.FromSearch = fromSearch;
                ViewBag.SingleRecord = singleRecord || !Request.IsAjax();
                ViewBag.Url = $"{Request.PathBase}{Request.Path}".ToLower();
                return View(roleSource);
            }
            else
            {
                ViewBag.Url = $"{Request.PathBase}{Request.Path}{Request.QueryString}".ToLower();
            }
            return PartialView("_DetailContent", roleSource);

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

            ViewBag.DeleteHandler = "QERoleSourcePage.deleteMainRecord";
            return PartialView("_SimpleDeletePrompt");
        }


        [HttpPost, ActionName("Delete")]
        [Authorize(Policy = SharedAuthorizationPolicy.CanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int RoleSourceID, string roleSourceName, string rowVersion)
        {
            if (ModelState.IsValid)
            {
                var roleSource = new QERoleSource { RoleSourceID = RoleSourceID, RoleName = roleSourceName, tStamp = System.Convert.FromBase64String(rowVersion) };

                await _repository.DeleteAsync(roleSource);
                return Json(new { id = roleSource.RoleSourceID });
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] QERoleSource roleSource)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(roleSource, roleSource.RoleSourceID);

                if (roleSource.RoleSourceID > 0)
                    await _repository.UpdateAsync(roleSource);
                else
                    await _repository.AddAsync(roleSource);

                return Json(roleSource.RoleSourceID);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var roleSource = await _repository.GetByIdAsync(id);
            if (roleSource == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = roleSource.CreatedBy, dateCreated = roleSource.DateCreated, updatedBy = roleSource.UpdatedBy, lastUpdate = roleSource.LastUpdate, tStamp = roleSource.tStamp });
        }

        public async Task<IActionResult> GetPicklistData(string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var roleSource = _repository.QueryableList;
            var result = await QueryHelper.GetPicklistDataAsync(roleSource, property, text, filterType, requiredRelation);
            return Json(result);
        }

        public async Task<IActionResult> GetRoleSourceList(string textProperty, string systemType, string text, FilterType filterType, string requiredRelation = "",string screenName="")
        {
            var roleSources = _repository.QueryableList.Where( r => r.SystemType == systemType || r.SystemType=="");
            roleSources = QueryHelper.BuildCriteria(roleSources, textProperty, text, filterType, requiredRelation);
            var list = roleSources.Select(l => new { RoleSourceID = l.RoleSourceID, RoleName = l.RoleName, RoleType = l.RoleType, SystemType = l.SystemType }).OrderBy(l => l.RoleName).ToList();

            //Attorney Modified

            //Add custom email address to list
            if (textProperty == "RoleName" && !string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text.Trim()))
            {
                try
                {
                    var emailAddr = new MailAddress(text.Trim());
                    list.Add(new { RoleSourceID = 9999, RoleName = emailAddr.Address.ToString(), RoleType = "Z", SystemType = "" });
                }
                catch (Exception ex) { }
            }

            if (screenName.ToLower() !="attorney modified") {
                var newAtty = list.Where(l => l.RoleName == "NewAtty").FirstOrDefault();
                var modifiedAtty = list.Where(l => l.RoleName == "ModifiedAtty").FirstOrDefault();
                if (newAtty !=null) list.Remove(newAtty);
                if (modifiedAtty !=null) list.Remove(modifiedAtty);
            }
            return Json(list);
        }

        private async Task<DetailPageViewModel<QERoleSource>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<QERoleSource>();
            viewModel.Detail = await _repository.QueryableList.SingleOrDefaultAsync(c => c.RoleSourceID == id);

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

        private async Task<DetailPageViewModel<QERoleSource>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<QERoleSource>();
            viewModel.Detail = new QERoleSource();

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }
    }



}