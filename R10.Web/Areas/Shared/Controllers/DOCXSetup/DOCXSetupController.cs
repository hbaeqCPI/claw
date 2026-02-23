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
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessLetters)] //TO DO: DOCX permission
    public class DOCXSetupController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        private readonly IDOCXService _docxService;
        private readonly IDOCXViewModelService _docxViewModelService;

        private readonly IMapper _mapper;

        private readonly string _searchContainer = "docxSearch";
        private readonly string _detailContainer = "docxDetail";

        public DOCXSetupController(
                    IAuthorizationService authService, 
                    IStringLocalizer<SharedResource> localizer,
                    IDOCXService docxService,
                    IDOCXViewModelService docxViewModelService,
                    IMapper mapper
                    )
        {
            _authService = authService;
            _localizer = localizer;
            _docxService = docxService;
            _docxViewModelService = docxViewModelService;
            _mapper = mapper;
        }

        //TO DO: DOCX permission
        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessLettersSetup)]
        public IActionResult Patent()
        {
            return Index("P");
        }

        //[Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessDOCXters)]
        //public IActionResult Trademark()
        //{
        //    return Index("T");
        //}

        //[Authorize(Policy = GeneralMatterAuthorizationPolicy.CanAccessDOCXters)]
        //public IActionResult GenMatter()
        //{
        //    return Index("G");
        //}


        private IActionResult Index(string sys)
        {
            var viewModel = new DOCXPageViewModel()
            {
                PageId = _searchContainer,                  // container name
                DetailPageId = _detailContainer,            // for view data passed to _SearchIndex partial inside _SearchIndex
                Title = _localizer["DOCX"].ToString(),
                SystemType = sys
            };

            return View("Index", viewModel);
        }


        #region CRUD main

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters, string sys)
        {
            var canAccess = await DOCXHelper.CanAccessDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            // search grid result
            if (ModelState.IsValid)
            {
                var docxes = _docxViewModelService.AddCriteria(_docxService.DOCXesMain, mainSearchFilters );
                var result = await _docxViewModelService.CreateViewModelForSearchGrid(request, docxes);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<IActionResult> Detail(int id, string sys)
        {
            var canAccess = await DOCXHelper.CanAccessDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var page = await PrepareEditScreen(id, sys);
            if (page.Detail == null)
            {
                Guard.Against.NoRecordPermission(!Request.IsAjax());
                return RedirectToAction(GetEntryAction(sys));
            }

            SetDetailViewData(page);

            return PartialView("_DOCXDetailContent", page.Detail);
        }

        public async Task<IActionResult> EmptyDetail(string sys)
        {
            var page = await PrepareEmptyScreen(sys);
            SetDetailViewData(page);
            return PartialView("_DOCXDetailContent", page.Detail);
        }

        public async Task<IActionResult> Add(string sys)
        {
            if (!Request.IsAjax())
                return RedirectToAction(GetEntryAction(sys));

            var canUpdate = await DOCXHelper.CanUpdateDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            var page = await PrepareAddScreen(sys);
            if (page.Detail == null)
                return RedirectToAction(GetEntryAction(sys));

            SetDetailViewData(page);

            return PartialView("_DOCXDetailContent", page.Detail);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Copy(int id, string sys)
        {
            var canUpdate = await DOCXHelper.CanUpdateDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            var page = await PrepareCopyScreen(id, sys);
            if (page.Detail == null)
                return RedirectToAction(GetEntryAction(sys));

            SetDetailViewData(page);

            return PartialView("_DOCXDetailContent", page.Detail);
        }

        private void SetDetailViewData(DetailPageViewModel<DOCXMainDetailViewModel> page)
        {
            ViewData["PagePermission"] = page;      // (DetailPagePermission)page;
            ViewData["PageId"] = page.Container;
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string sys, string tStamp)
        {
            var canUpdate = await DOCXHelper.CanUpdateDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            await _docxService.Delete(new DOCXMain { DOCXId = id, tStamp = System.Convert.FromBase64String(tStamp) });
            return Ok();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] DOCXMainDetailViewModel docxMainVM)
        {
            var canUpdate = await DOCXHelper.CanUpdateDOCX(docxMainVM.SystemType, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (ModelState.IsValid)
            {
                UpdateEntityStamps(docxMainVM, docxMainVM.DOCXId);

                var docxMain = _mapper.Map<DOCXMain>(docxMainVM);
                if (docxMainVM.DOCXId > 0) 
                {
                    //var categoryInUse = _docxService.DOCXesMain.Where(d => d.DOCXCatId != 1 && d.SystemScreen.ScreenName == "Country Application" && d.SystemScreen.ScreenId == docxMainVM.ScreenId && d.DOCXCatId == docxMainVM.DOCXCatId && d.DOCXId != docxMainVM.DOCXId).Select(d => d.DOCXId).Any();
                    //if (categoryInUse) throw new Exception("Category has already been used on Country Application screen."); // check if Category is in use on Country Application
                    await _docxService.Update(docxMain); 
                }
                else
                {
                    await _docxService.Add(docxMain);
                }

                return Json(new { DOCXId = docxMain.DOCXId, DOCXName = docxMain.DOCXName });
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
        private async Task<DetailPageViewModel<DOCXMainDetailViewModel>> PrepareAddScreen(string sys)
        {
            var viewModel = new DetailPageViewModel<DOCXMainDetailViewModel>();

            var detail = await _docxViewModelService.CreateViewModelForDetailScreen(0);
            detail.SystemType = sys;
            viewModel.Detail = detail;

            viewModel.AddDOCXSecurityPolicies(sys);
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            if (viewModel.CanAddRecord)
                viewModel.PageActions = GetMorePageActions(sys);

            viewModel.Container = _detailContainer;
            return viewModel;
        }

        private async Task<DetailPageViewModel<DOCXMainDetailViewModel>> PrepareEmptyScreen(string sys)
        {
            var viewModel = new DetailPageViewModel<DOCXMainDetailViewModel>();
            var detail = await _docxViewModelService.CreateViewModelForDetailScreen(0);
            detail.DOCXId = -1;
            detail.SystemType = sys;
            viewModel.Detail = detail;

            viewModel.AddDOCXSecurityPolicies(sys);
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

        private async Task<DetailPageViewModel<DOCXMainDetailViewModel>> PrepareCopyScreen(int id, string sys)
        {
            var viewModel = new DetailPageViewModel<DOCXMainDetailViewModel>();

            var detail = await _docxViewModelService.CreateViewModelForDetailScreen(id);
            detail.DOCXId = 0;
            detail.DOCXName = _localizer["Copy"] + " " + detail.DOCXName;
            detail.CreatedBy = null;
            detail.DateCreated = null;
            detail.UpdatedBy = null;
            detail.LastUpdate = null;
            detail.SystemType = sys;

            viewModel.Detail = detail;

            viewModel.AddDOCXSecurityPolicies(sys);
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            if (viewModel.CanAddRecord)
                viewModel.PageActions = GetMorePageActions(sys);

            viewModel.Container = _detailContainer;
            return viewModel;
        }

        private async Task<DetailPageViewModel<DOCXMainDetailViewModel>> PrepareEditScreen(int id, string sys)
        {
            var viewModel = new DetailPageViewModel<DOCXMainDetailViewModel>();
            viewModel.Detail = await _docxViewModelService.CreateViewModelForDetailScreen(id);

            if (viewModel.Detail != null)
            {
                viewModel.AddDOCXSecurityPolicies(sys);
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

        //-------------------------------------------------- DOCX Preview --------------------------------------------------
        #region DOCX Preview
        public IActionResult DOCXPreviewInit(int docxId, bool includeGenerated)
        {
            DataTable gridTable = _docxService.PreviewDOCXData(docxId, includeGenerated, User.GetEmail(),
                                        User.HasRespOfficeFilter(), User.HasEntityFilter(), "", 0, 0);
            string jsonTable = JsonConvert.SerializeObject(gridTable, Formatting.Indented, new JsonSerializerSettings { Converters = new[] { new Newtonsoft.Json.Converters.DataSetConverter() } });
            return Json(jsonTable);
        }

        public IActionResult DOCXPreview([DataSourceRequest] DataSourceRequest request, int docxId, bool includeGenerated, string sortField, string sortDir)
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
            DataTable gridTable = _docxService.PreviewDOCXData(docxId, includeGenerated, User.GetEmail(),
                            User.HasRespOfficeFilter(), User.HasEntityFilter(), sortExpr, request.Page, request.PageSize);

            int recordCount = GetRecordCount();
            if (recordCount == 0)
            {
                gridTable.Rows[0][0] = _localizer[gridTable.Rows[0][0].ToString()];
            }

            request.Page = 1;               // work-around to page-skip/jump issue that causes empty grid on 2nd and succeeding pages
            var result = gridTable.ToDataSourceResult(request);
            result.Total = recordCount;

            return Json(result);
        }

        private int GetRecordCount()
        {
            return _docxService.PreviewDOCXCount();
        }
        #endregion

        //-------------------------------------------------- Template Manager --------------------------------------------------
        #region Template Manager
        private List<DetailPageAction> GetMorePageActions(string sys)
        {
            var pageActions = new List<DetailPageAction>();
            pageActions.Add(new DetailPageAction
            {
                Url = Url.Action("TemplateManager", "DOCXSetup", new { area = "Shared", sys = sys }),
                Label = _localizer[$"Template Manager"],
                IconClass = "fa-folder",
                ControlId = "openTemplateManager"
            });

            return pageActions;
        }

        public async Task<IActionResult> TemplateManager(string sys)
        {
            var canUpdate = await DOCXHelper.CanUpdateDOCX(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            return PartialView("_TemplateManager", sys);
        }

        #endregion

        //-------------------------------------------------- Miscellaneous --------------------------------------------------
        #region Miscellaneous
        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_docxService.DOCXesMain, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var docx = await _docxService.GetDOCXMainById(id);
            if (docx == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = docx.CreatedBy, dateCreated = docx.DateCreated, updatedBy = docx.UpdatedBy, lastUpdate = docx.LastUpdate, tStamp = docx.tStamp });
        }

        
        #endregion

    }
}