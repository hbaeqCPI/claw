using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Interfaces.Shared;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessCustomReport)]
    public class CustomReportController : BaseController
    {
        private readonly ISystemSettings<DefaultSetting> _sharedSettings;
        private readonly ICustomReportService _customReportService;
        private readonly IReportDeployService reportDeployService;
        private readonly IEntityService<CustomReport> _customReportentityService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ICustomReportViewModelService _customReportViewModelService;
        private readonly IDataQueryService _dataQueryService;

        private readonly IAuthorizationService _authService;

        private readonly string _searchContainer = "customReportSearch";
        private readonly string _detailContainer = "customReportDetail";

        public CustomReportController(ISystemSettings<DefaultSetting> sharedSettings
            , ICustomReportService customReportService
            , IReportDeployService reportDeployService
            , IEntityService<CustomReport> customReportentityService
            , IStringLocalizer<SharedResource> localizer
            , IAuthorizationService authService
            , ICustomReportViewModelService customReportViewModelService
            , IDataQueryService dataQueryService)
        {
            _sharedSettings = sharedSettings;
            _customReportService = customReportService;
            this.reportDeployService = reportDeployService;
            _customReportentityService = customReportentityService;
            _localizer = localizer;

            _authService = authService;
            _customReportViewModelService = customReportViewModelService;
            _dataQueryService = dataQueryService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new CustomReportPageViewModel()
            {
                PageId = _searchContainer,   //container name
                Title = _localizer["Custom Report"].ToString(),
            };
            var permission = new DetailPagePermission();
            //permission.AddDataQuerySecurityPolicies(await CanUpdatePatentQuery(), await CanUpdateTrademarkQuery(), await CanUpdateGenMatterQuery());
            await permission.ApplyDetailPagePermission(User, _authService);
            model.PagePermission = permission;

            model.DetailPageId = _detailContainer;              // for view data passed to _SearchIndex partial inside _SearchIndex
            //model.AQBQueryInstance = _aqbQueryInstance;

            return View("Index", model);
        }

        public async Task DownloadReportBuilder()
        {
            var reportBuilderDownloadUrl = (await _sharedSettings.GetSetting()).CRReportBuilderDownloadUrl;
            Response.Redirect(reportBuilderDownloadUrl);
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessCustomReport)]
        public async Task<IActionResult> GetCustomReport()
        {
            string queryName = "CPICustomReportTemplate";
            await AddCustomQueryForTemplate(queryName);
            if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            MemoryStream streamResult = await _customReportService.GetCustomReport(queryName);
            return File(streamResult.ToArray(), ImageHelper.GetContentType(".rdl"), _customReportService.PrepareFileName(queryName) + ".rdl");
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessCustomReport)]
        public async Task<IActionResult> GetCustomReportT()
        {
            string queryName = "CPICustomReportTemplateT";
            await AddCustomQueryForTemplate(queryName);
            if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            MemoryStream streamResult = await _customReportService.GetCustomReport(queryName);
            return File(streamResult.ToArray(), ImageHelper.GetContentType(".rdl"), _customReportService.PrepareFileName(queryName) + ".rdl");
        }

        [Authorize(Policy = GeneralMatterAuthorizationPolicy.CanAccessCustomReport)]
        public async Task<IActionResult> GetCustomReportG()
        {
            string queryName = "CPICustomReportTemplateG";
            await AddCustomQueryForTemplate(queryName);
            if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            MemoryStream streamResult = await _customReportService.GetCustomReport(queryName);
            return File(streamResult.ToArray(), ImageHelper.GetContentType(".rdl"), _customReportService.PrepareFileName(queryName) + ".rdl");
        }

        private async Task AddCustomQueryForTemplate(string queryName)
        {
            if ((await _customReportService.GetDataQuery(queryName)) == null)
            {
                CustomReportTemplateHelper templates = new CustomReportTemplateHelper();
                var property = templates.GetType().GetProperties().FirstOrDefault(c=> c.Name == queryName);
                
                DataQueryMain queryMain = new DataQueryMain()
                {
                        CreatedBy = User.GetUserName(),
                        DateCreated = DateTime.Now,
                        IsEditable = false,
                        IsShared = true,
                        LastUpdate = DateTime.Now,
                        OwnedBy = "CPICustomReportTemplate",
                        QueryId = 0,
                        QueryName = property.Name,
                        Remarks = "",
                        SQLExpr = property.GetValue(templates).ToString(),
                        UpdatedBy = User.GetUserName(),
                        tStamp = null
                };
                await _customReportService.AddDataQuery(queryMain);
            }
        }

        private class CustomReportTemplateHelper
        {
            public string CPICustomReportTemplate { get { return "Select Top 100 _dq_qryPatInventionAndCountryApplications.CaseNumber, _dq_qryPatInventionAndCountryApplications.SubCase, _dq_qryPatInventionAndCountryApplications.CountryName, _dq_qryPatInventionAndCountryApplications.CaseType, _dq_qryPatInventionAndCountryApplications.AppNumber, _dq_qryPatInventionAndCountryApplications.FilDate_Fmt, _dq_qryPatInventionAndCountryApplications.PubNumber, _dq_qryPatInventionAndCountryApplications.PubDate_Fmt, _dq_qryPatInventionAndCountryApplications.PatNumber, _dq_qryPatInventionAndCountryApplications.IssDate_Fmt, _dq_qryPatInventionAndCountryApplications.ApplicationStatus, _dq_qryPatInventionAndCountryApplications.ExpDate_Fmt, _dq_qryPatInventionAndCountryApplications.AppTitle From _dq_qryPatInventionAndCountryApplications Order By _dq_qryPatInventionAndCountryApplications.CaseNumber"; } }
            public string CPICustomReportTemplateT { get { return "Select Top 100 _dq_qryTmkTrademark.CaseNumber, _dq_qryTmkTrademark.SubCase, _dq_qryTmkTrademark.CountryName, _dq_qryTmkTrademark.CaseType, _dq_qryTmkTrademark.AppNumber, _dq_qryTmkTrademark.FilDate_Fmt, _dq_qryTmkTrademark.PubNumber, _dq_qryTmkTrademark.PubDate_Fmt, _dq_qryTmkTrademark.RegNumber, _dq_qryTmkTrademark.RegDate_Fmt, _dq_qryTmkTrademark.NextRenewalDate_Fmt, _dq_qryTmkTrademark.MarkType, _dq_qryTmkTrademark.TrademarkName, _dq_qryTmkTrademark.TrademarkStatus, _dq_qryTmkTrademark.TrademarkStatusDate_Fmt From _dq_qryTmkTrademark Order By _dq_qryTmkTrademark.CaseNumber"; } }
            public string CPICustomReportTemplateG { get { return "Select Top 100 _dq_qryGenMatter.CaseNumber, _dq_qryGenMatter.SubCase, _dq_qryGenMatter.MatterType, _dq_qryGenMatter.MatterTitle, _dq_qryGenMatter.MatterStatus, _dq_qryGenMatter.MatterStatusDate_Fmt, _dq_qryGenMatter.EffectiveOpenDate_Fmt, _dq_qryGenMatter.TerminationEndDate_Fmt From _dq_qryGenMatter Order By _dq_qryGenMatter.CaseNumber"; } }
        }

        public async Task<IActionResult> GetCustomReportList(
            [DataSourceRequest] DataSourceRequest request, int parentId, int reportId, bool userSchedulesOnly
            )
        {
            if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            var customReports = _customReportentityService.QueryableList.Where(c=>c.IsShared||c.UserId==User.GetUserIdentifier());
            List<CustomReportViewModel> reportViewModels = new List<CustomReportViewModel>();
            foreach (CustomReport report in customReports)
            {
                CustomReportViewModel reportViewModel = new CustomReportViewModel() {
                    ReportName = report.ReportName,
                    Remarks = report.Remarks,
                    CreatedBy = report.CreatedBy,
                    UpdatedBy = report.UpdatedBy,
                    LastUpdate = report.LastUpdate,
                    IsShared = report.IsShared,
                    IsEditable = report.IsEditable
                };

                reportViewModels.Add(reportViewModel);
            }
            return Json(reportViewModels.ToDataSourceResult(request));
        }

        //public IActionResult AddCustomReport()
        //{
        //    CustomReportUploadViewModel viewModel = new CustomReportUploadViewModel { };
        //    viewModel.Updating = false;
        //    viewModel.IsShared = true;
        //    viewModel.IsEditable = true;

        //    ViewBag.IsOwner = true;
        //    return PartialView("_AddUpdateCustomReport", viewModel);
        //}

        //public IActionResult UpdateCustomReport(CustomReportUploadViewModel viewModel)
        //{
        //    viewModel.Updating = true;
        //    var customReports = _customReportentityService.QueryableList;
        //    CustomReport customReport = customReports.FirstOrDefault(c => c.ReportName == viewModel.ReportName);
        //    viewModel.IsShared = customReport.IsShared;
        //    viewModel.IsEditable = customReport.IsEditable;
        //    viewModel.Remarks = customReport.Remarks;

        //    ViewBag.IsOwner = customReport.UserId == User.GetUserIdentifier();
        //    return PartialView("_AddUpdateCustomReport", viewModel);
        //}

        public async Task<IActionResult> CustomReportSave(CustomReportDetailViewModel viewModel)
        {
            if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            var updating = viewModel.ReportId != 0;
            var customReports = _customReportentityService.QueryableList;

            List<IFormFile> files = new List<IFormFile>();
            if (viewModel.Files != null)
                files = viewModel.Files.ToList();

            var i = files.Count() - 1;

            CustomReport customReport = new CustomReport();
            if (updating)
            {
                customReport = customReports.FirstOrDefault(c => c.ReportId == viewModel.ReportId);
                if (customReport == null)
                    return BadRequest(_localizer["Could not find the report to update."].ToString());
                UpdateEntityStamps(customReport, 1);
            }
            else
            {
                customReport.UserId = User.GetUserIdentifier();
                UpdateEntityStamps(customReport, 0);
            }

            customReport.ReportName = viewModel.ReportName;
            customReport.IsEditable = viewModel.IsEditable;
            customReport.IsShared = viewModel.IsShared;
            customReport.Remarks = viewModel.Remarks;

            if (i >= 0)
            {
                var formFile = files[i];
                if (formFile.Length > 0)
                {
                    var fileStream = formFile.OpenReadStream();
                    fileStream.Position = 0;
                    StreamReader sr = new StreamReader(fileStream);
                    var contents = sr.ReadToEnd();
                    customReport.QueryId = _customReportService.GetQueryId(contents);
                    customReport.ReportFile = formFile.FileName;
                }
            }

            long size = files.Sum(f => f.Length);
            bool success = true;

            if (updating)
                await _customReportentityService.Update(customReport);
            else
                await _customReportentityService.Add(customReport);

            var filePath = "";
            //foreach (var formFile in files)
            //{
            if (i >= 0)
            {
                var formFile = files[i];
                if (formFile.Length > 0)
                {
                    string customReportsDirectory = reportDeployService.GetCustomReportsDirectory();
                    var name = formFile.FileName;
                    filePath = Path.Combine(customReportsDirectory, name);
                    //Todo: add security and db infomation and query information
                    var fileStream = formFile.OpenReadStream();
                    fileStream.Position = 0;
                    StreamReader sr = new StreamReader(fileStream);
                    var contents = sr.ReadToEnd();
                    string contentsResult = await _customReportService.ConvertLocalRDLToServerRDL(contents);
                    byte[] byteArray = Encoding.ASCII.GetBytes(contentsResult);
                    MemoryStream streamResult = new MemoryStream(byteArray);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await streamResult.CopyToAsync(stream);
                    }
                }
                success = await reportDeployService.DeployCustomReport(filePath, customReport.ReportId.ToString());
            }

            //}

            if (success)
            {
                return Json(new { ReportId = customReport.ReportId, ReportName = customReport.ReportName });
            }
            else
            {
                if (!updating)
                {
                    await Delete(customReport.ReportId);
                    await _customReportentityService.Delete(customReport);
                    return BadRequest(_localizer["Failed to upload report."].ToString());
                }
            }

            return BadRequest(_localizer["Failed to update report file."].ToString());
        }

        public async Task<IActionResult> DownloadCustomReport([FromBody] CustomReportDetailViewModel report)
        {
            if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            string permissionText = await _customReportService.GetDataPermissionTextByReportId(report.ReportId);
            if (permissionText != "")
            {
                return BadRequest(permissionText);
            }
            string reportName = report.ReportName;
            var reportDefinition = await reportDeployService.GetReportDefinition(report.ReportId.ToString());
            MemoryStream streamResult = await _customReportService.ConvertServerRDLToLocalRDL(reportDefinition);
            return File(streamResult.ToArray(), ImageHelper.GetContentType(".rdl"), reportName + ".rdl");
        }

        //public async Task<IActionResult> DeleteCustomReport( CustomReportViewModel report)
        //{
        //    if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
        //        return NotFound();
        //    var customReports = _customReportentityService.QueryableList;
        //    var customReport = customReports.FirstOrDefault(c => c.ReportName == report.ReportName);
        //    if(customReport==null)
        //        return BadRequest(_localizer["Failed to delete report"].ToString());
        //    var result = await reportDeployService.DeleteReport(customReport.ReportId.ToString());
        //    if (result)
        //    {
        //        await _customReportentityService.Delete(customReport);
        //        return Ok(_localizer["Report has been deleted successfully."].ToString());
        //    }
        //    else
        //    {
        //        return BadRequest(_localizer["Failed to delete report"].ToString());
        //    }
        //}

        #region lookup
        public async Task<IActionResult> GetCustomReportNameList(string property, string text, FilterType filterType)
        {
            if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            return Json(await _customReportentityService.QueryableList.Where(c => c.IsShared || c.UserId == User.GetUserIdentifier()).Select(c => new { ReportName = c.ReportName }).ToListAsync());
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            //return Json(await _customReportentityService.QueryableList.Where(c => c.IsShared || c.UserId == User.GetUserIdentifier()).Select(c => new { ReportName = c.ReportName }).ToListAsync());
            if (ModelState.IsValid)
            {
                var reports = _customReportentityService.QueryableList.Where(c=> _dataQueryService.DataQueriesMain.Any(d =>d.QueryId == c.QueryId && (User.IsAdmin() || d.OwnedBy == User.GetEmail() || d.IsShared == true)));
                var customReports = _customReportViewModelService.AddCriteria(mainSearchFilters, reports, User.GetUserIdentifier());
                var result = await _customReportViewModelService.CreateViewModelForGrid(request, customReports);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }
        #endregion lookup

        public async Task<IActionResult> Detail(int id)
        {
            if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            var customReports = _customReportentityService.QueryableList;
            var customReport = customReports.FirstOrDefault(c => c.ReportId == id);
            if (customReport == null)
                return BadRequest(_localizer["Failed to delete report"].ToString());

            var page = await PrepareEditScreen(customReport.ReportName);
            if (page.Detail == null)
            {
                Guard.Against.NoRecordPermission(!Request.IsAjax());
                return RedirectToAction("Index");
            }

            SetDetailViewData(page);

            return PartialView("_CustomReportDetailHeader", page.Detail);
        }

        public async Task<IActionResult> EmptyDetail()
        {
            if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            var page = await PrepareEmptyScreen();
            SetDetailViewData(page);
            return PartialView("_CustomReportDetailHeader", page.Detail);
        }

        private async Task<DetailPageViewModel<CustomReportDetailViewModel>> PrepareEmptyScreen()
        {
            var viewModel = new DetailPageViewModel<CustomReportDetailViewModel>();
            var detail = await _customReportViewModelService.CreateViewModelForDetailScreen(null);
            detail.ReportId = 0;
            viewModel.Detail = detail;

            await viewModel.ApplyDetailPagePermission(User, _authService);

            viewModel.CanSearch = false;
            viewModel.CanPrintRecord = false;
            viewModel.CanEmail = false;
            viewModel.CanRefreshRecord = false;
            viewModel.CanCopyRecord = false;
            viewModel.CanDeleteRecord = false;

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _detailContainer;
            return viewModel;
        }

        private async Task<DetailPageViewModel<CustomReportDetailViewModel>> PrepareEditScreen(string reportName)
        {
            var viewModel = new DetailPageViewModel<CustomReportDetailViewModel>();
            viewModel.Detail = await _customReportViewModelService.CreateViewModelForDetailScreen(reportName);

            if (viewModel.Detail != null)
            {
                viewModel.Detail.IsMyReport = viewModel.Detail.UserId == User.GetUserIdentifier();

                //viewModel.AddDataQuerySecurityPolicies(await CanUpdatePatentQuery(), await CanUpdateTrademarkQuery(), await CanUpdateGenMatterQuery());
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanSearch = false;
                viewModel.CanPrintRecord = false;
                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);
                viewModel.RefreshRecordUrl = Url.Action("detail");
                viewModel.CanCopyRecord = false;
                //viewModel.CopyScreenUrl += "/" + id.ToString();
                viewModel.CanDeleteRecord = (viewModel.CanDeleteRecord && viewModel.Detail.IsMyReport) || User.IsAdmin();
                viewModel.CanEditRecord = true;
                viewModel.CanAddRecord = true;
                viewModel.AddScreenUrl = Url.Action("Add");
                viewModel.CanRefreshRecord = false;
                viewModel.CanPrintRecord = true;
                //viewModel.PrintScreenUrl = Url.Action("CustomReport", "Report", new { Area = "Shared"});
                viewModel.CanEmail = true;
                //viewModel.EmailScreenUrl = Url.Action("EmailCustomReport", "Report", new { Area = "Shared", subjectTitle = _localizer["Custom Report"].ToString() });
                viewModel.DeleteScreenUrl = Url.Action("Delete");
                viewModel.DeleteConfirmationUrl = Url.DeleteConfirmWithCodeLink();
                viewModel.PageActions = GetMorePageActions(viewModel);

                viewModel.Container = _detailContainer;
            }
            return viewModel;
        }

        private List<DetailPageAction> GetMorePageActions(DetailPageViewModel<CustomReportDetailViewModel> pagePermission)
        {
            var pageActions = new List<DetailPageAction>();
            pageActions.Add(new DetailPageAction
            {
                Url = Url.Action("DownloadCustomReport", "CustomReport", new { area = "Shared" }),
                Label = _localizer[$"Download"].ToString(),
                IconClass = "fa-download",
                ControlId = "downloadCustomReport"
            });

            return pageActions;
        }

        private async Task<DetailPageViewModel<CustomReportDetailViewModel>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<CustomReportDetailViewModel>();

            var detail = await _customReportViewModelService.CreateViewModelForDetailScreen(null);
            detail.UserId = User.GetUserIdentifier();
            detail.IsMyReport = true;
            viewModel.Detail = detail;

            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _detailContainer;
            return viewModel;
        }

        private void SetDetailViewData(DetailPageViewModel<CustomReportDetailViewModel> page)
        {
            ViewData["PagePermission"] = page;      // (DetailPagePermission)page;
            ViewData["PageId"] = page.Container;
        }

        public async Task<IActionResult> Add()
        {
            if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            Guard.Against.NoRecordPermission(CanUpdateReport());          // check if user has modify permission

            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            SetDetailViewData(page);

            return PartialView("_CustomReportDetailHeader", page.Detail);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            var customReport = _customReportentityService.QueryableList.FirstOrDefault(c => c.ReportId == id);
            if (customReport == null)
                return BadRequest(_localizer["Failed to delete report"].ToString());
            var result = await reportDeployService.DeleteReport(id.ToString());
            if (result)
            {
                await _customReportentityService.Delete(customReport);
                return Ok(_localizer["Report has been deleted successfully."].ToString());
            }
            else
            {
                return BadRequest(_localizer["Failed to delete report"].ToString());
            }
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            if (!(await _sharedSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            var report = _customReportentityService.QueryableList.FirstOrDefault(c=>c.ReportId == id);
            if (report == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = report.CreatedBy, dateCreated = report.DateCreated, updatedBy = report.UpdatedBy, lastUpdate = report.LastUpdate, tStamp = report.tStamp });
        }

        private bool CanUpdateReport()
        {
            // user can update query definition, if user has any of the PTG query modify policy; the access to different PTG tables is controlled by the backend logic
            if (CanUpdatePatentReport().Result)
                return true;

            if (CanUpdateTrademarkReport().Result)
                return true;

            if (CanUpdateGenMatterReport().Result)
                return true;

            if (CanUpdateAMSReport().Result)
                return true;

            return false;
        }

        private async Task<bool> CanUpdatePatentReport()
        {
            return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CustomQueryModify)).Succeeded;
        }

        private async Task<bool> CanUpdateTrademarkReport()
        {
            return (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CustomQueryModify)).Succeeded;
        }

        private async Task<bool> CanUpdateGenMatterReport()
        {
            return (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CustomQueryModify)).Succeeded;
        }

        private async Task<bool> CanUpdateAMSReport()
        {
            return (await _authService.AuthorizeAsync(User, AMSAuthorizationPolicy.CustomQueryModify)).Succeeded;
        }
    }
}