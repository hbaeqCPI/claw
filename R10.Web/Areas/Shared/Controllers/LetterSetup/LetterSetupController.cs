using R10.Web.Helpers;
using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using R10.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using R10.Core;
using R10.Core.Entities;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Patent;
using R10.Core.Services.Shared;
using R10.Core.Entities.Documents;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessLetters)]
    public class LetterSetupController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        private readonly ILetterService _letterService;
        private readonly ILetterViewModelService _letterViewModelService;
        private readonly IChildEntityService<LetterMain, LetterTag> _letterTagService;

        private readonly IMapper _mapper;
        private readonly ISystemSettings<PatSetting> _patSettings;

        private readonly string _searchContainer = "letterSearch";
        private readonly string _detailContainer = "letterDetail";

        public LetterSetupController(
                    IAuthorizationService authService, 
                    IStringLocalizer<SharedResource> localizer,
                    ILetterService letterService,
                    ILetterViewModelService letterViewModelService,
                    IChildEntityService<LetterMain, LetterTag> letterTagService,
                    IMapper mapper,
                    ISystemSettings<PatSetting> patSettings
                    )
        {
            _authService = authService;
            _localizer = localizer;
            _letterService = letterService;
            _letterViewModelService = letterViewModelService;
            _letterTagService = letterTagService;
            _mapper = mapper;
            _patSettings = patSettings;
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessLettersSetup)]
        public IActionResult Patent()
        {
            return Index("P");
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessLettersSetup)]
        public IActionResult Trademark()
        {
            return Index("T");
        }

        [Authorize(Policy = GeneralMatterAuthorizationPolicy.CanAccessLettersSetup)]
        public IActionResult GenMatter()
        {
            return Index("G");
        }


        private IActionResult Index(string sys)
        {
            var viewModel = new LetterPageViewModel()
            {
                PageId = _searchContainer,                  // container name
                DetailPageId = _detailContainer,            // for view data passed to _SearchIndex partial inside _SearchIndex
                Title = _localizer["Letters"].ToString(),
                SystemType = sys
            };
            ViewBag.SystemType = sys;
            return View("Index", viewModel);
        }


        #region CRUD main

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters, string sys)
        {
            var canAccess = await LetterHelper.CanAccessLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            // search grid result
            if (ModelState.IsValid)
            {
                var letters = _letterViewModelService.AddCriteria(_letterService.FilteredLettersMain, mainSearchFilters );

                var result = await _letterViewModelService.CreateViewModelForSearchGrid(request, letters);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<IActionResult> Detail(int id, string sys)
        {
            var canAccess = await LetterHelper.CanAccessLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var page = await PrepareEditScreen(id, sys);
            if (page.Detail == null)
            {
                Guard.Against.NoRecordPermission(!Request.IsAjax());
                return RedirectToAction(GetEntryAction(sys));
            }

            SetDetailViewData(page);

            return PartialView("_LetterDetailContent", page.Detail);
        }

        public async Task<IActionResult> EmptyDetail(string sys)
        {
            var page = await PrepareEmptyScreen(sys);
            SetDetailViewData(page);
            return PartialView("_LetterDetailContent", page.Detail);
        }

        public async Task<IActionResult> Add(string sys)
        {
            if (!Request.IsAjax())
                return RedirectToAction(GetEntryAction(sys));

            var canUpdate = await LetterHelper.CanUpdateLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            var page = await PrepareAddScreen(sys);
            if (page.Detail == null)
                return RedirectToAction(GetEntryAction(sys));

            SetDetailViewData(page);

            return PartialView("_LetterDetailContent", page.Detail);
        }

        public async Task<IActionResult> Copy(int id, string sys)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            var page = await PrepareCopyScreen(id, sys);
            if (page.Detail == null)
                return RedirectToAction(GetEntryAction(sys));

            SetDetailViewData(page);

            return PartialView("_LetterDetailContent", page.Detail);
        }

        private void SetDetailViewData(DetailPageViewModel<LetterMainDetailViewModel> page)
        {
            ViewData["PagePermission"] = page;      // (DetailPagePermission)page;
            ViewData["PageId"] = page.Container;
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string sys, string tStamp)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            await _letterService.Delete(new LetterMain { LetId = id, tStamp = System.Convert.FromBase64String(tStamp) });
            return Ok();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] LetterMainDetailViewModel letterMainVM)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(letterMainVM.SystemType, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (ModelState.IsValid)
            {
                UpdateEntityStamps(letterMainVM, letterMainVM.LetId);

                var letterMain = _mapper.Map<LetterMain>(letterMainVM);
                if (letterMainVM.LetId > 0)
                    await _letterService.Update(letterMain);
                else
                {
                    //from copy
                    if (letterMainVM.CopySourceLetId > 0)
                    {
                        var letterRecordSources = await _letterService.LetterRecordSources.Where(s => s.LetId == letterMainVM.CopySourceLetId).Include(s=> s.LetterRecordSourceFilters).ToListAsync();
                        if (letterRecordSources.Any()) {
                            letterRecordSources.Each(s => { 
                                s.LetId = 0; 
                                s.RecSourceId = 0;
                                s.LetterRecordSourceFilters.Each(f=> f.LetFilterId=0);
                            });
                            letterMain.LetterRecordSources = new List<LetterRecordSource>();
                            letterMain.LetterRecordSources.AddRange(letterRecordSources);
                        }
                        var letterUserData = await _letterService.LetterUserData.Where(s => s.LetId == letterMainVM.CopySourceLetId).ToListAsync();
                        if (letterUserData.Any())
                        {
                            letterUserData.Each(s => {
                                s.LetId = 0;
                                s.LetDataId = 0;
                            });
                            letterMain.LetterUserData = new List<LetterUserData>();
                            letterMain.LetterUserData.AddRange(letterUserData);
                        }
                    }
                    await _letterService.Add(letterMain);
                }

                return Json(new { LetId = letterMain.LetId, LetName = letterMain.LetName });
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        private string GetEntryAction(string sys)
        {
            switch (sys)
            {
                case "P": return "Patent";
                case "T": return "Trademark";
                case "G": return "GenMatter";
                default: return "";
            }
        }

        #endregion

        //-------------------------------------------------- Prepare Screens --------------------------------------------------
        #region Prepare screens
        private async Task<DetailPageViewModel<LetterMainDetailViewModel>> PrepareAddScreen(string sys)
        {
            var viewModel = new DetailPageViewModel<LetterMainDetailViewModel>();

            var detail = await _letterViewModelService.CreateViewModelForDetailScreen(0);
            detail.SystemType = sys;
            viewModel.Detail = detail;

            viewModel.AddLettersSecurityPolicies(sys);
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            if (viewModel.CanAddRecord)
                viewModel.PageActions = GetMorePageActions(sys);

            viewModel.Container = _detailContainer;
            return viewModel;
        }

        private async Task<DetailPageViewModel<LetterMainDetailViewModel>> PrepareEmptyScreen(string sys)
        {
            var viewModel = new DetailPageViewModel<LetterMainDetailViewModel>();
            var detail = await _letterViewModelService.CreateViewModelForDetailScreen(0);
            detail.LetId = -1;
            detail.SystemType = sys;
            viewModel.Detail = detail;

            viewModel.AddLettersSecurityPolicies(sys);
            await viewModel.ApplyDetailPagePermission(User, _authService);

            viewModel.CanSearch = false;
            viewModel.CanPrintRecord = false;
            viewModel.CanEmail = false;
            viewModel.CanRefreshRecord = false;
            viewModel.CanCopyRecord = false;
            viewModel.CanDeleteRecord = false;

            this.AddDefaultNavigationUrls(viewModel);
            if (viewModel.CanAddRecord)
                viewModel.PageActions = GetMorePageActions(sys);

            viewModel.AddScreenUrl += $"?sys={sys}";
            viewModel.Container = _detailContainer;

            return viewModel;
        }

        private async Task<DetailPageViewModel<LetterMainDetailViewModel>> PrepareCopyScreen(int id, string sys)
        {
            var viewModel = new DetailPageViewModel<LetterMainDetailViewModel>();

            var detail = await _letterViewModelService.CreateViewModelForDetailScreen(id);
            detail.LetId = 0;
            detail.LetName = _localizer["Copy"] + " " + detail.LetName;
            detail.CreatedBy = null;
            detail.DateCreated = null;
            detail.UpdatedBy = null;
            detail.LastUpdate = null;
            detail.SystemType = sys;
            detail.CopySourceLetId = id;
            viewModel.Detail = detail;

            viewModel.AddLettersSecurityPolicies(sys);
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            if (viewModel.CanAddRecord)
                viewModel.PageActions = GetMorePageActions(sys);

            viewModel.Container = _detailContainer;
            return viewModel;
        }

        private async Task<DetailPageViewModel<LetterMainDetailViewModel>> PrepareEditScreen(int id, string sys)
        {
            var viewModel = new DetailPageViewModel<LetterMainDetailViewModel>();
            viewModel.Detail = await _letterViewModelService.CreateViewModelForDetailScreen(id);

            if (viewModel.Detail != null)
            {
                viewModel.AddLettersSecurityPolicies(sys);
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanSearch = false;
                viewModel.CanPrintRecord = false;
                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);
                if (viewModel.CanAddRecord)
                    viewModel.PageActions = GetMorePageActions(sys);

                viewModel.DeleteConfirmationUrl = Url.DeleteConfirmWithCodeLink();

                viewModel.RefreshRecordUrl = Url.Action("detail") + "?sys=" + sys;
                if (viewModel.CanCopyRecord)
                    viewModel.CopyScreenUrl += "/" + id.ToString() + "?sys=" + sys;
                if (viewModel.CanAddRecord)
                    viewModel.AddScreenUrl += "?sys=" + sys;
                if (viewModel.CanDeleteRecord)
                    viewModel.DeleteScreenUrl += "?sys=" + sys;

                viewModel.Container = _detailContainer;
                viewModel.Detail.SystemType = sys;
            }
            return viewModel;
        }

        #endregion

        //-------------------------------------------------- Letter Preview --------------------------------------------------
        #region Letter Preview
        public IActionResult LetterPreviewInit(int letId, bool includeGenerated)
        {
            DataTable gridTable = _letterService.PreviewLetterData(letId, includeGenerated, User.GetEmail(),
                                        User.HasRespOfficeFilter(), User.HasEntityFilter(), "", 0, 0);
            string jsonTable = JsonConvert.SerializeObject(gridTable, Formatting.Indented, new JsonSerializerSettings { Converters = new[] { new Newtonsoft.Json.Converters.DataSetConverter() } });
            return Json(jsonTable);
        }

        public IActionResult LetterPreview([DataSourceRequest] DataSourceRequest request, int letId, bool includeGenerated, string sortField, string sortDir)
        {
            // (fsn 21-apr-2020) for some reason, the sort info in DataSourceRequest is always null; the network trace shows that it had been passed by the client
            //string sortField = "";
            //string sortDir = "";
            //if (request.Sorts != null && request.Sorts.Any())
            //{
            //    sortColumn = request.Sorts[0].Member;
            //    sortDir = request.Sorts[0].SortDirection.ToString();
            //}

            var sortExpr = (sortField + " " + sortDir).Trim();
            DataTable gridTable = _letterService.PreviewLetterData(letId, includeGenerated, User.GetEmail(),
                            User.HasRespOfficeFilter(), User.HasEntityFilter(), sortExpr, request.Page, request.PageSize);

            int recordCount = GetRecordCount();
            if (recordCount == 0)
            {
                gridTable.Rows[0][0] = _localizer[gridTable.Rows[0][0].ToString()];
            }

            request.Page = 1;               // work-around to page-skip/jump issue that causes empty grid on 2nd and succeeding pages
            var result = gridTable.ToDataSourceResult(request);
            result.Total = recordCount;
            //return Json(result);
            return Content(JsonConvert.SerializeObject(result), "application/json"); //to avoid treating DBNull.Value as {} during conversion
        }

        private int GetRecordCount()
        {
            return _letterService.PreviewLetterCount();
        }
        #endregion

        //-------------------------------------------------- Template Manager --------------------------------------------------
        #region Template Manager
        private List<DetailPageAction> GetMorePageActions(string sys)
        {
            var pageActions = new List<DetailPageAction>();
            pageActions.Add(new DetailPageAction
            {
                Url = Url.Action("TemplateManager", "LetterSetup", new { area = "Shared", sys = sys }),
                Label = _localizer[$"Template Manager"],
                IconClass = "fa-folder",
                ControlId = "openTemplateManager"
            });

            return pageActions;
        }

        public async Task<IActionResult> TemplateManager(string sys)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            return PartialView("_TemplateManager", sys);
        }

        #endregion

        //-------------------------------------------------- Miscellaneous --------------------------------------------------
        #region Miscellaneous
        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_letterService.LettersMain, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetLetterNames([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType,string? systemType)
        {
            var letterMain = _letterService.LettersMain;

            if (!string.IsNullOrEmpty(systemType)) {
                letterMain = letterMain.Where(l => l.SystemScreen.SystemType == systemType);
            }
            return await GetPicklistData(letterMain, request, property, text, filterType);
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var letter = await _letterService.GetLetterMainById(id);
            if (letter == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = letter.CreatedBy, dateCreated = letter.DateCreated, updatedBy = letter.UpdatedBy, lastUpdate = letter.LastUpdate, tStamp = letter.tStamp });
        }


        #endregion

        #region Tags
        public async Task<IActionResult> GetLetterTags()
        {
            var tags = await _letterService.LetterTags.Select(t => t.Tag).Distinct().ToArrayAsync();
            return Json(tags);

        }

        public async Task<IActionResult> LetterTagsRead([DataSourceRequest] DataSourceRequest request, int parentId)
        {
            var tags = await _letterService.LetterTags.Where(t => t.LetId == parentId).OrderBy(t => t.Tag).ToListAsync();
            return Json(tags.ToDataSourceResult(request));
        }

        public async Task<IActionResult> LetterTagsUpdate(int parentId,
            [Bind(Prefix = "updated")] IEnumerable<LetterTag> updated,
            [Bind(Prefix = "new")] IEnumerable<LetterTag> added,
            [Bind(Prefix = "deleted")] IEnumerable<LetterTag> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });



                await _letterTagService.Update(parentId, User.GetUserName(),
                    _mapper.Map<List<LetterTag>>(updated),
                    _mapper.Map<List<LetterTag>>(added),
                    _mapper.Map<List<LetterTag>>(deleted)
                    );
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer["Letter Tag has been saved successfully."].ToString() :
                    _localizer["Letter Tags have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        public async Task<IActionResult> LetterTagDelete([Bind(Prefix = "deleted")] LetterTag deleted)
        {
            if (deleted.LetTagId >= 0)
            {
                await _letterTagService.Update(deleted.LetId, User.GetUserName(), new List<LetterTag>(), new List<LetterTag>(), new List<LetterTag>() { deleted });
                return Ok(new { success = _localizer["Letter Tag has been deleted successfully."].ToString() });
            }
            return Ok();
        }
        #endregion

    }
}