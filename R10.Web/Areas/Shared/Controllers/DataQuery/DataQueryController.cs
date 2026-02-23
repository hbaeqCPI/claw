using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.DatabaseSchemaView;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using R10.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using R10.Core;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Interfaces.Shared;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using R10.Web.Areas.Shared.Services.AQB;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Math;
using System.Linq.Expressions;
using static Kendo.Mvc.UI.UIPrimitives;
using System.Net.Mail;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using ActiveQueryBuilder.View;
using R10.Core.Services.Shared;
using Microsoft.EntityFrameworkCore;


using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessCustomQuery)]
    public class DataQueryController : BaseController
    {
        private readonly IQueryBuilderService _aqbs;
        private readonly ICustomQueryBuilderProvider _customQueryBuilderProvider;

        private readonly IAuthorizationService _authService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        private readonly ISystemSettings<DefaultSetting> _genSettings;

        private readonly IDataQueryService _dataQueryService;
        private readonly IDataQueryViewModelService _dataQueryViewModelService;
        private readonly ExportHelper _exportHelper;
        private readonly ICustomReportService _customReportService;
        private readonly IEntityService<CustomReport> _customReportentityService;
        private readonly IReportDeployService _reportDeployService;
        protected readonly IDashboardManager _dashboardManager;
        protected readonly IDocumentHelper _documentHelper;
        private readonly IChildEntityService<DataQueryMain, DataQueryTag> _dataQueryTagService;
        private readonly IMapper _mapper;

        private readonly IEmailSender _emailSender;
        private readonly IHostingEnvironment _hostingEnvironment;


        private readonly string _searchContainer = "dataQuerySearch";
        private readonly string _detailContainer = "dataQueryDetail";
        private readonly string _aqbQueryInstance = "CPiQuery";

        public DataQueryController(
                        IQueryBuilderService aqbs,
                        ICustomQueryBuilderProvider customQueryBuilderProvider,
                        IAuthorizationService authService,
                        IStringLocalizer<SharedResource> localizer,
                        ISystemSettings<DefaultSetting> genSettings,
                        IDataQueryService dataQueryService,
                        IDataQueryViewModelService dataQueryViewModelService,
                        ExportHelper exportHelper,
                        ICustomReportService customReportService,
                        IEntityService<CustomReport> customReportentityService,
                        IReportDeployService reportDeployService,
                        IDashboardManager dashboardManager,
                        IDocumentHelper documentHelper,
                        IChildEntityService<DataQueryMain, DataQueryTag> dataQueryTagService,
                        IMapper mapper,
                        IEmailSender emailSender,
                        IHostingEnvironment hostingEnvironment
                        )
        {
            _aqbs = aqbs;
            _customQueryBuilderProvider = customQueryBuilderProvider;
            _authService = authService;
            _localizer = localizer;
            _genSettings = genSettings;
            _dataQueryService = dataQueryService;
            _dataQueryViewModelService = dataQueryViewModelService;
            _exportHelper = exportHelper;
            _customReportService = customReportService;
            _customReportentityService = customReportentityService;
            _reportDeployService = reportDeployService;
            _dashboardManager = dashboardManager;
            _documentHelper = documentHelper;
            _emailSender = emailSender;
            _hostingEnvironment = hostingEnvironment;
            _dataQueryTagService = dataQueryTagService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DataQueryPageViewModel()
            {
                PageId = _searchContainer,   //container name
                Title = _localizer["Custom Query"].ToString(),
            };
            var permission = new DetailPagePermission();
            permission.AddDataQuerySecurityPolicies(await CanUpdatePatentQuery(), await CanUpdateTrademarkQuery(), await CanUpdateGenMatterQuery(), await CanUpdateAMSQuery());
            await permission.ApplyDetailPagePermission(User, _authService);
            model.PagePermission = permission;

            model.DetailPageId = _detailContainer;              // for view data passed to _SearchIndex partial inside _SearchIndex

            var aqbQueryInstance = $"{_aqbQueryInstance}|{User.GetEmail()}";
            model.AQBQueryInstance = aqbQueryInstance;

            //preload aqb metadata to prevent ui issues
            //metadata is dependent on user permissions
            await _customQueryBuilderProvider.InitializeMetadata(User.GetEmail());

            return View("Index", model);
        }

        [HttpGet]
        public IActionResult Search()
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GetCustomReport(string queryName)
        {
            if (!(await _genSettings.GetSetting()).IsCustomReportON)
                return NotFound();
            var queryId = (await _dataQueryService.GetByNameAsync(queryName)).QueryId;
            MemoryStream streamResult = await _customReportService.GetCustomReport(queryId);
            if (streamResult == null)
                return BadRequest(_localizer["Please check if the query is working."].ToString());
            return File(streamResult.ToArray(), ImageHelper.GetContentType(".rdl"), _customReportService.PrepareFileName(queryName) + ".rdl");
        }

        public IActionResult OpenHelp()
        {
            return PartialView("_DataQueryHelp");
        }

        #region CRUD
        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var dataQueries = _dataQueryViewModelService.AddCriteria(mainSearchFilters, _dataQueryService.DataQueriesMain, User.GetEmail(), User.IsAdmin());
                var result = await _dataQueryViewModelService.CreateViewModelForGrid(request, dataQueries);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<IActionResult> Detail(int id)
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                Guard.Against.NoRecordPermission(!Request.IsAjax());
                return RedirectToAction("Index");
            }

            SetDetailViewData(page);
            var cpiWidgets = await _dashboardManager.GetCPiWidgetByQueryId(id);
            if (cpiWidgets == null || cpiWidgets.Count == 0)
            {
                ViewBag.HavingCustomWidget = false;
            }
            else
            {
                ViewBag.HavingCustomWidget = true;
            }

            return PartialView("_DataQueryDetailHeader", page.Detail);
        }

        public async Task<IActionResult> EmptyDetail()
        {
            var page = await PrepareEmptyScreen();
            SetDetailViewData(page);
            return PartialView("_DataQueryDetailHeader", page.Detail);
        }

        public async Task<IActionResult> Add()
        {
            Guard.Against.NoRecordPermission(CanUpdateQuery());          // check if user has modify permission

            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            SetDetailViewData(page);

            return PartialView("_DataQueryDetailHeader", page.Detail);
        }

        public async Task<IActionResult> Copy(int id)
        {
            Guard.Against.NoRecordPermission(CanUpdateQuery());          // check if user has modify permission

            var page = await PrepareCopyScreen(id);
            if (page.Detail == null)
                return RedirectToAction("Index");

            SetDetailViewData(page);

            return PartialView("_DataQueryDetailHeader", page.Detail);
        }

        private void SetDetailViewData(DetailPageViewModel<DataQueryDetailViewModel> page)
        {
            ViewData["PagePermission"] = page;      // (DetailPagePermission)page;
            ViewData["PageId"] = page.Container;
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string tStamp)
        {
            Guard.Against.NoRecordPermission(CanUpdateQuery());          // check if user has modify permission

            //delete custom widget and custom report.
            var widgets = await _dashboardManager.GetCPiWidgetByQueryId(id);
            foreach (var widget in widgets)
            {
                if (widget != null)
                {
                    var userWidgetsForDelete = await _dashboardManager.GetUserCustomWidgets(widget.Id);
                    await _dashboardManager.RemoveUserWidget(userWidgetsForDelete);
                    await _dashboardManager.RemoveCPiWidget(widget);
                }
            }

            var customReports = _customReportentityService.QueryableList.Where(c => c.QueryId == id);
            foreach (var customReport in customReports)
            {
                await _reportDeployService.DeleteReport(customReport.ReportId.ToString());
            }
            await _customReportentityService.Delete(customReports);

            await _dataQueryService.Delete(new DataQueryMain { QueryId = id, tStamp = System.Convert.FromBase64String(tStamp) });
            return Ok();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] DataQueryMain dataQuery)
        {
            Guard.Against.NoRecordPermission(CanUpdateQuery());          // check if user has modify permission

            if (ModelState.IsValid)
            {
                // update SQL expression
                var aqbQueryInstance = $"{_aqbQueryInstance}|{User.GetEmail()}";
                var queryBuilder = _aqbs.Get(aqbQueryInstance);
                if (queryBuilder != null)
                    dataQuery.SQLExpr = queryBuilder.SQL;

                UpdateEntityStamps(dataQuery, dataQuery.QueryId);

                if (dataQuery.QueryId > 0)
                    await _dataQueryService.Update(dataQuery);
                else
                {
                    dataQuery.OwnedBy = User.GetEmail();
                    await _dataQueryService.Add(dataQuery);
                }

                return Json(new { QueryId = dataQuery.QueryId, QueryName = dataQuery.QueryName });
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        //-------------------------------------------------- PREPARE --------------------------------------------------
        #region Prepare actions
        private async Task<DetailPageViewModel<DataQueryDetailViewModel>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<DataQueryDetailViewModel>();

            var detail = await _dataQueryViewModelService.CreateViewModelForDetailScreen(0);
            detail.OwnedBy = User.GetEmail();
            detail.IsMyQuery = true;
            viewModel.Detail = detail;

            viewModel.AddDataQuerySecurityPolicies(await CanUpdatePatentQuery(), await CanUpdateTrademarkQuery(), await CanUpdateGenMatterQuery(), await CanUpdateAMSQuery());
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _detailContainer;
            return viewModel;
        }

        private async Task<DetailPageViewModel<DataQueryDetailViewModel>> PrepareEmptyScreen()
        {
            var viewModel = new DetailPageViewModel<DataQueryDetailViewModel>();
            var detail = await _dataQueryViewModelService.CreateViewModelForDetailScreen(0);
            detail.QueryId = -1;
            viewModel.Detail = detail;

            viewModel.AddDataQuerySecurityPolicies(await CanUpdatePatentQuery(), await CanUpdateTrademarkQuery(), await CanUpdateGenMatterQuery(), await CanUpdateAMSQuery());
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

        private async Task<DetailPageViewModel<DataQueryDetailViewModel>> PrepareCopyScreen(int id)
        {
            var viewModel = new DetailPageViewModel<DataQueryDetailViewModel>();

            var detail = await _dataQueryViewModelService.CreateViewModelForDetailScreen(id);
            detail.OwnedBy = User.GetEmail();
            detail.QueryId = 0;
            detail.QueryName = _localizer["Copy"] + " " + detail.QueryName;
            detail.CreatedBy = null;
            detail.DateCreated = null;
            detail.UpdatedBy = null;
            detail.LastUpdate = null;
            detail.IsMyQuery = true;
            //detail.AQBQueryInstance = _aqbQueryInstance;    // moved to separate container
            viewModel.Detail = detail;

            viewModel.AddDataQuerySecurityPolicies(await CanUpdatePatentQuery(), await CanUpdateTrademarkQuery(), await CanUpdateGenMatterQuery(), await CanUpdateAMSQuery());
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _detailContainer;
            return viewModel;
        }

        private async Task<DetailPageViewModel<DataQueryDetailViewModel>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<DataQueryDetailViewModel>();
            viewModel.Detail = await _dataQueryViewModelService.CreateViewModelForDetailScreen(id);

            if (viewModel.Detail != null)
            {
                viewModel.Detail.IsMyQuery = viewModel.Detail.OwnedBy == User.GetEmail();

                viewModel.AddDataQuerySecurityPolicies(await CanUpdatePatentQuery(), await CanUpdateTrademarkQuery(), await CanUpdateGenMatterQuery(), await CanUpdateAMSQuery());
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanSearch = false;
                viewModel.CanPrintRecord = false;
                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);
                viewModel.RefreshRecordUrl = Url.Action("detail");
                viewModel.CopyScreenUrl += "/" + id.ToString();
                viewModel.CanDeleteRecord = (viewModel.CanDeleteRecord && (viewModel.Detail.IsMyQuery || viewModel.Detail.IsEditable)) || User.IsAdmin();
                viewModel.DeleteConfirmationUrl = Url.DeleteConfirmWithCodeLink();
                viewModel.Container = _detailContainer;
            }
            return viewModel;
        }
        #endregion


        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var query = await _dataQueryService.GetByIdAsync(id);
            if (query == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = query.CreatedBy, dateCreated = query.DateCreated, updatedBy = query.UpdatedBy, lastUpdate = query.LastUpdate, tStamp = query.tStamp });
        }

        public async Task<IActionResult> UpdateRecordStamps(int id)
        {
            var query = await _dataQueryService.GetByIdAsync(id);
            if (query != null)
            {
                query.UpdatedBy = User.GetUserName();
                query.LastUpdate = DateTime.Now;
                await _dataQueryService.Update(query);
            }

            return Ok();
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var dataQueriesMain = _dataQueryService.DataQueriesMain.Where(q => q.IsShared || q.OwnedBy == User.GetEmail());
            return await GetPicklistData(dataQueriesMain, request, property, text, filterType, requiredRelation);
        }

        #endregion

        #region AQB

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> CreateQueryBuilder(string name)         // name is actually the query id; AQB hard-codes the parameter to "name" and nothing else :(
        {
            _aqbs.Get(name);
            return new EmptyResult();
        }

        public IActionResult RunQueryInit()
        {
            var aqbQueryInstance = $"{_aqbQueryInstance}|{User.GetEmail()}";
            var queryBuilder = _aqbs.Get(aqbQueryInstance);
            var sql = queryBuilder.SQL;

            // remove order by clause, this will mess up sql paging
            int pos = sql.LastIndexOf("Order By");
            if (pos > 0) sql = sql.Substring(0, pos);

            var userName = User.Identity.Name;
            var hasEntityFilterOn = User.HasEntityFilter();
            var hasRespOfficeOn = User.HasRespOfficeFilter();

            string sourceTables = "";
            queryBuilder.QueryStatistics.UsedDatabaseObjects.Each(o => sourceTables += "|" + o.ObjectName);
            if (sourceTables.Length > 0) sourceTables = sourceTables.Substring(1);
            string sourceTablesWithAlias = "";
            foreach (UnionSubQuery query in queryBuilder.ActiveSubQuery.Children.OfType<UnionSubQuery>())
            {
                foreach (DataSourceObject item in query.FromClause.Children.OfType<DataSourceObject>())
                {
                    if (!string.IsNullOrEmpty(item.Alias))
                        sourceTablesWithAlias += "|" + item.MetadataObject.Name.Trim() + " " + item.Alias.Trim();
                }
            }

            DataTable gridTable = _dataQueryService.RunQuery(sql, sourceTables, sourceTablesWithAlias, GetSelectList(queryBuilder), "", 0, 0, userName, hasEntityFilterOn, hasRespOfficeOn);

            string jsonTable = JsonConvert.SerializeObject(gridTable, Formatting.Indented, new JsonSerializerSettings { Converters = new[] { new Newtonsoft.Json.Converters.DataSetConverter() } });
            return Json(jsonTable);
        }

        public IActionResult RunQuery([DataSourceRequest] DataSourceRequest request, string sortField, string sortDir)
        {
            // (fsn 21-apr-2020) for some reason, the sort info in DataSourceRequest is always null; the network trace shows that it had been passed by the client
            //string sortField = "";
            //string sortDir = "";
            //if (request.Sorts != null && request.Sorts.Any())
            //{
            //    sortColumn = request.Sorts[0].Member;
            //    sortDir = request.Sorts[0].SortDirection.ToString();
            //}

            DataTable gridTable = GetQueryResult(sortField, sortDir, request.Page, request.PageSize);
            int recordCount = GetRecordCount();
            if (recordCount == 0 && gridTable.TableName != "Exception")
            {
                gridTable.Rows[0][0] = _localizer[gridTable.Rows[0][0].ToString()];
                gridTable.Columns[0].ColumnName = "NoData";             // don't change this, there is a js code that checks this
            }

            request.Page = 1;               // work-around to page-skip/jump issue that causes empty grid on 2nd and succeeding pages
            var result = gridTable.ToDataSourceResult(request);
            result.Total = recordCount;
            //System.Text.Json.JsonSerializerOptions jss = new System.Text.Json.JsonSerializerOptions();
            //jss.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;

            //return Json(result, jss);
            return Json(result);
        }

        private DataTable GetQueryResult(string sortField, string sortDir, int pageNo, int pageSize, bool forExport = false)
        {
            var aqbQueryInstance = $"{_aqbQueryInstance}|{User.GetEmail()}";
            var queryBuilder = _aqbs.Get(aqbQueryInstance);
            var sql = queryBuilder.SQL;
            var allExpressionStr = queryBuilder.ActiveUnionSubQuery.QueryColumnList.Items.Select(d => d.ExpressionString).ToList();

            // remove order by clause, this will mess up paging in SQL backend
            int pos = sql.LastIndexOf("Order By");
            if (pos > 0) sql = sql.Substring(0, pos);

            var sortExpr = (sortField + " " + sortDir).Trim();
            if (sortExpr.Length == 0) sortExpr = queryBuilder.ActiveUnionSubQuery.OrderByClauseString;

            var userName = User.Identity.Name;
            var hasEntityFilterOn = User.HasEntityFilter();
            var hasRespOfficeOn = User.HasRespOfficeFilter();
            string sourceTables = "";
            //queryBuilder.QueryStatistics.UsedDatabaseObjects.Each(o => sourceTables += "|" + o.ObjectName);
            queryBuilder.QueryStatistics.UsedDatabaseObjects.Each(o => sourceTables += "|" + o.MetadataObject.Name);
            string sourceTablesWithAlias = "";
            foreach (UnionSubQuery query in queryBuilder.ActiveSubQuery.Children.OfType<UnionSubQuery>())
            {
                foreach (DataSourceObject item in query.FromClause.Children.OfType<DataSourceObject>())
                {
                    if (!string.IsNullOrEmpty(item.Alias))
                        sourceTablesWithAlias += "|" + item.MetadataObject.Name.Trim() + " " + item.Alias.Trim();
                }
            }

            if (sourceTables.Length > 0) sourceTables = sourceTables.Substring(1);
            //return _dataQueryService.RunQuery(sql, sourceTables, forExport ? GetSelectListForExport(queryBuilder) : GetSelectList(queryBuilder), sortExpr, pageNo, pageSize, userName, hasEntityFilterOn, hasRespOfficeOn);
            try
            {
                return _dataQueryService.RunQuery(WrapFieldsWithFunctions(allExpressionStr, sql), sourceTables, sourceTablesWithAlias, forExport ? GetSelectListForExport(queryBuilder) : GetSelectList(queryBuilder), WrapFieldsWithFunctions(allExpressionStr, sortExpr), pageNo, pageSize, userName, hasEntityFilterOn, hasRespOfficeOn);
            }
            catch (Exception ex)
            {
                DataTable dt = new DataTable("Exception");
                dt.Columns.Add("Err_Message", typeof(string));
                dt.Columns.Add("Err_Number", typeof(int));

                DataRow row = dt.NewRow();
                row["Err_Number"] = 1;
                row["Err_Message"] = ex.Message;

                dt.Rows.Add(row);

                return dt;
            }
        }

        private string WrapFields(List<string> allExpressions, string sql)
        {
            var distinctExpressions = allExpressions.Distinct().ToList();
            var tableName = string.Empty;
            var fieldName = string.Empty;
            var newExp = string.Empty;
            distinctExpressions.ForEach(c =>
            {
                var expArr = c.Split(".");
                if (expArr.Length > 1)
                {
                    tableName = expArr[0];
                    fieldName = expArr[1];
                    if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(fieldName))
                    {
                        newExp = "[" + tableName + "]." + "[" + fieldName + "]";
                        //sql = sql.Replace(c, newExp); //issue when you have a query like this =  Select _dq_tblClient.Client, _dq_tblClient.ClientName From _dq_tblClient
                        sql = sql.Replace(c + ",", newExp + ",");
                        sql = sql.Replace(c + " ", newExp + " ");
                        if (sql.EndsWith(c))
                        {
                            sql = sql.Replace(c, newExp);
                        }
                    }
                }
            });
            return sql;
        }

        private string WrapFieldsWithFunctions(List<string> allExpressions, string sql)
        {
            var distinctExpressions = allExpressions.Distinct().ToList();
            var tableName = string.Empty;
            var fieldName = string.Empty;
            var newExp = string.Empty;
            foreach (var item in distinctExpressions)
            {
                var expression = item.Trim();
                // Regex to find epression contains parentheses or not
                //string pattern = @"\w+\((.*)\)";
                //Match match = Regex.Match(expression, pattern);

                if (expression.Contains("+") || expression.Contains("("))
                {
                    int plusIndex = expression.IndexOf('+');
                    int parenthesisIndex = expression.IndexOf('(');

                    if (plusIndex == -1 || (parenthesisIndex != -1 && parenthesisIndex < plusIndex))
                    {
                        //'(' appears first
                        var functionName = expression.Substring(0, parenthesisIndex);
                        if (!_dataQueryService.DataQueryAllowedFunction.Any(f => f.FunctionName!.ToLower() == functionName.ToLower()))
                            throw new Exception(_localizer[$"The SQL function '{functionName}' is not supproted in data queries."].ToString());

                        List<string> subExpressions = new List<string>();
                        var firstPlus = GetFirstCharOutSideParentheses(expression, '+');
                        if (firstPlus > 0)
                        {
                            subExpressions.Add(expression.Substring(firstPlus + 1).Trim());
                            expression = expression.Substring(0, firstPlus).Trim();
                        }

                        string pattern = @"\w+\((.*)\)";
                        Match match = Regex.Match(expression, pattern);
                        string functionClause = match.Groups[1].Value;
                        var firstComma = GetFirstCharOutSideParentheses(functionClause, ',');
                        while (firstComma > 0)
                        {
                            subExpressions.Add(functionClause.Substring(0, firstComma).Trim());
                            functionClause = functionClause.Substring(firstComma + 1).Trim();
                            firstComma = GetFirstCharOutSideParentheses(functionClause, ',');
                        }
                        subExpressions.Add(functionClause);

                        sql = WrapFieldsWithFunctions(subExpressions, sql);
                    }
                    else //if (parenthesisIndex == -1 || (plusIndex != -1 && plusIndex < parenthesisIndex))
                    {
                        //'+' appears first
                        sql = WrapFieldsWithFunctions(new List<string> {
                                expression.Substring(0, plusIndex).Trim(),
                                expression.Substring(plusIndex + 1).Trim()
                            }, sql);
                    }
                }
                //if (expression.Contains("+"))
                //{
                //    List<string> parts = new List<string>(expression.Split('+'));
                //    sql = WrapFieldsWithFunctions(parts, sql);
                //}
                //else if (match.Success)
                //{
                //    //porcess function expression
                //    var functionName = expression.Substring(0, expression.IndexOf('('));
                //    if (!_dataQueryService.DataQueryAllowedFunction.Any(f => f.FunctionName!.ToLower() == functionName.ToLower()))
                //        throw new Exception(_localizer[$"The SQL function '{functionName}' is not supproted in data queries."].ToString());
                //    string functionClause = match.Groups[1].Value;
                //    if (Regex.Match(functionClause, pattern).Success || functionClause.Contains("+"))
                //        sql = WrapFieldsWithFunctions(new List<string> { functionClause }, sql);
                //    List<string> functionParamaters = new List<string>(functionClause.Split(','));
                //    sql = WrapFieldsWithFunctions(functionParamaters, sql);
                //}
                else
                {
                    var expArr = expression.Split(".");
                    if (expArr.Length > 1)
                    {
                        tableName = expArr[0];
                        fieldName = expArr[1];
                        if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(fieldName))
                        {
                            newExp = "[" + tableName + "]." + (fieldName == "*" ? "*" : "[" + fieldName + "]");
                            //sql = sql.Replace(c, newExp); //issue when you have a query like this =  Select _dq_tblClient.Client, _dq_tblClient.ClientName From _dq_tblClient
                            sql = sql.Replace(expression + ",", newExp + ",");
                            sql = sql.Replace(expression + " ", newExp + " ");
                            sql = sql.Replace(expression + ")", newExp + ")");
                            if (sql.EndsWith(expression))
                            {
                                sql = sql.Replace(expression, newExp);
                            }
                        }
                    }
                }
            }
            return sql;
        }

        private int GetFirstCharOutSideParentheses(string expression, char findChar)
        {
            if (string.IsNullOrEmpty(expression) || expression.IndexOf(findChar) == -1)
                return -1;

            int openParentheses = 0;

            for (int i = 0; i < expression.Length; i++)
            {
                if (expression[i] == '(')
                {
                    openParentheses++;
                }
                else if (expression[i] == ')')
                {
                    openParentheses--;
                }
                else if (expression[i] == findChar && openParentheses == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        private string GetSelectList(QueryBuilder queryBuilder)
        {
            var columns = queryBuilder.ActiveUnionSubQuery.QueryColumnList.Items;
            columns.RemoveAll(c => c.Selected == false);
            string selectList = "";
            columns.ForEach(c =>
            {
                if (c.AliasString != "")
                    selectList += ", ISNULL(CAST([" + c.AliasString + "] AS NVARCHAR(MAX)),'') AS [" + c.AliasString.Replace(" ", "_") + "]";
                else if (c.ExpressionField != null)
                    selectList += ", ISNULL(CAST([" + c.ExpressionField.Name + "] AS NVARCHAR(MAX)),'') AS [" + c.ExpressionField.Name + "]";
                else if (c.ExpressionString != null)
                {
                    string pattern = @"\w+\((.*)\)";
                    Match match = Regex.Match(c.ExpressionString, pattern);
                    if (match.Success)
                        throw new Exception(_localizer[$"SQL functions must have aliases. Please ensure each function is assigned an alias for proper query execution."].ToString());
                    if (c.ExpressionString.Contains("+"))
                        throw new Exception(_localizer[$"The plus operator requires aliases."].ToString());

                }
            });

            if (selectList.Length > 0)
                selectList = selectList.Right(selectList.Length - 2);
            else
                selectList = "*";

            return selectList;
        }

        private string GetSelectListForExport(QueryBuilder queryBuilder)
        {
            var columns = queryBuilder.ActiveUnionSubQuery.QueryColumnList.Items;
            columns.RemoveAll(c => c.Selected == false);
            string selectList = "";
            columns.ForEach(c =>
            {
                if (c.Selected)
                {
                    if (c.AliasString != "")
                        selectList += ", [" + c.AliasString + "]";
                    else if (c.ExpressionField != null)
                        selectList += ", [" + c.ExpressionField.Name + "]";
                    else if (c.ExpressionString != null)
                    {
                        string pattern = @"\w+\((.*)\)";
                        Match match = Regex.Match(c.ExpressionString, pattern);
                        if (match.Success)
                            throw new Exception(_localizer[$"SQL functions must have aliases. Please ensure each function is assigned an alias for proper query execution."].ToString());
                        if (c.ExpressionString.Contains("+"))
                            throw new Exception(_localizer[$"The plus operator requires aliases."].ToString());

                    }
                }
            });

            if (selectList.Length > 0)
                selectList = selectList.Right(selectList.Length - 2);
            else
                selectList = "*";

            return selectList;
        }


        //private string GetSelectList(QueryBuilder queryBuilder)
        //{

        //    var aFields = queryBuilder.ActiveUnionSubQuery.SelectListString.Split(",");
        //    string selectList = "";
        //    aFields.Each(f => {
        //        // remove table names from select list
        //        if (f.IndexOf(".") > -1)
        //            f = f.Split(".")[1];

        //        // if there is alias, use that as field
        //        var parts = f.Split(" ");
        //        if (parts.Length > 0)
        //            f = parts[parts.Length - 1];

        //        selectList += ", " + f;
        //    });

        //    if (selectList.Length > 0)
        //        selectList = selectList.Right(selectList.Length - 2);
        //    return selectList;
        //}

        private int GetRecordCount()
        {
            return _dataQueryService.RunQueryCount();
        }


        #endregion

        #region Export Data

        public bool CheckExport(string queryName, string sortField, string sortDir)
        {
            DataTable dt = GetQueryResult(sortField, sortDir, 1, int.MaxValue, true);

            if (dt.Rows.Count < 2000)
                return false;

            foreach (DataColumn column in dt.Columns)
            {
                if (column.ColumnName.EndsWith("image", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private string CustomQueryExportFolder(string exportName = "")
        {
            string strRet = "";
            string strPathName = Path.Combine(_hostingEnvironment.ContentRootPath, "UserFiles/DQExports/" + User.GetUserName());

            try
            {
                if (!Directory.Exists(strPathName))
                {
                    Directory.CreateDirectory(strPathName);
                }
                DeleteOldExports(strPathName);
                strRet = strPathName + "/" + exportName;
            }
            catch (Exception e)
            {
                var error = e.Message;
            }
            return strRet;
        }

        static void DeleteOldExports(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath);

                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);

                    if (fileInfo.LastWriteTime < DateTime.Now - TimeSpan.FromDays(1))
                    {
                        fileInfo.Delete();
                    }
                }
            }
        }

        public IActionResult GetGeneratedExport(string generatedReportName)
        {
            var filePathName = CustomQueryExportFolder(generatedReportName);
            byte[] buffer;
            using (var fileStream = new FileStream(filePathName, FileMode.Open, FileAccess.Read))
            {
                int length = (int)fileStream.Length;  // get file length
                buffer = new byte[length];            // create buffer
                int count;                            // actual number of bytes read
                int sum = 0;                          // total number of bytes read

                // read until Read method returns 0 (end of the stream has been reached)
                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;  // sum is a buffer offset for next reading
            }
            FileStreamResult fsr = new FileStreamResult(new MemoryStream(buffer), GetFileMimeType(generatedReportName));

            return fsr;
        }

        private string GetFileMimeType(string fileName)
        {
            if (fileName.EndsWith(".pdf"))
            {
                return "application/pdf";
            }
            if (fileName.EndsWith(".doc"))
            {
                return "application/msword";
            }
            if (fileName.EndsWith(".xls"))
            {
                return "application/vnd.ms-excel";
            }
            if (fileName.EndsWith(".docx"))
            {
                return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            }
            if (fileName.EndsWith(".xlsx"))
            {
                return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }
            return "application/`";
        }

        public async Task<EmailSenderResult> EmailExport(MemoryStream export, string exportName)
        {
            if (!User.GetEmail().Contains("@"))
                return new EmailSenderResult();

            var filePathName = CustomQueryExportFolder(exportName);
            using (FileStream outputFileStream = new FileStream(filePathName, FileMode.Create))
            {
                export.WriteTo(outputFileStream);
            }
            FileStreamResult fsr = (FileStreamResult)GetGeneratedExport(exportName);
            Attachment attachment = new Attachment(fsr.FileStream, exportName);
            var result = await _emailSender.SendEmailAsync(User.GetEmail(), "Custom Query Export", "Please find the Custom Query Export attached.", attachment);
            return result;
        }


        public async Task<IActionResult> ExportToExcel(string queryName, string sortField, string sortDir)
        {
            var settings = await _genSettings.GetSetting();

            DataTable dt = GetQueryResult(sortField, sortDir, 1, int.MaxValue, true);

            if (dt.Columns.Contains("tStamp"))
            {
                dt.Columns.Remove("tStamp");
            }

            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    if (column.DataType == typeof(string))
                    {
                        string? data = row[column].ToString();
                        if (!string.IsNullOrEmpty(data) && data.Length > Int16.MaxValue)
                            row[column] = data.Substring(0, Int16.MaxValue);
                    }
                }
            }

            var imageFolder = _documentHelper.GetDocumentBasePath();
            var fileStream = await _exportHelper.DataTableToExcelMemoryStream(dt, queryName, "image", imageFolder, settings.DQExportImageSize);

            return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), "CustomQueryExport.xlsx");
        }

        public async Task<IActionResult> EmailToExcel(string queryName, string sortField, string sortDir)
        {
            var settings = await _genSettings.GetSetting();

            DataTable dt = GetQueryResult(sortField, sortDir, 1, int.MaxValue, true);

            if (dt.Columns.Contains("tStamp"))
            {
                dt.Columns.Remove("tStamp");
            }

            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    if (column.DataType == typeof(string))
                    {
                        string? data = row[column].ToString();
                        if (!string.IsNullOrEmpty(data) && data.Length > Int16.MaxValue)
                            row[column] = data.Substring(0, Int16.MaxValue);
                    }
                }
            }

            var imageFolder = _documentHelper.GetDocumentBasePath();
            var fileStream = await _exportHelper.DataTableToExcelMemoryStream(dt, queryName, "image", imageFolder, settings.DQExportImageSize);

            await EmailExport(fileStream, $"CustomQueryExport-{DateTime.Now:yyyy-MM-dd-hhmmsstt}.xlsx");
            return Ok();
        }

        public async Task<IActionResult> ExportToWord(string queryName, string sortField, string sortDir)
        {
            var settings = await _genSettings.GetSetting();

            DataTable dt = GetQueryResult(sortField, sortDir, 1, int.MaxValue);
            var imageFolder = _documentHelper.GetDocumentBasePath();
            var fileStream = await _exportHelper.DataTableToWordMemoryStream(dt, "image", imageFolder, settings.DQExportImageSize);

            return File(fileStream.ToArray(), ImageHelper.GetContentType(".docx"), "CustomQueryExport.docx");
        }

        public async Task<IActionResult> EmailToWord(string queryName, string sortField, string sortDir)
        {
            var settings = await _genSettings.GetSetting();

            DataTable dt = GetQueryResult(sortField, sortDir, 1, int.MaxValue);
            var imageFolder = _documentHelper.GetDocumentBasePath();
            var fileStream = await _exportHelper.DataTableToWordMemoryStream(dt, "image", imageFolder, settings.DQExportImageSize);

            //return File(fileStream.ToArray(), ImageHelper.GetContentType(".docx"), "CustomQueryExport.docx");
            await EmailExport(fileStream, $"CustomQueryExport-{DateTime.Now:yyyy-MM-dd-hhmmsstt}.docx");
            return Ok();
        }

        public IActionResult ExportToXML(string queryName, string sortField, string sortDir)
        {
            DataTable dt = GetQueryResult(sortField, sortDir, 1, int.MaxValue);

            var fileStream = _exportHelper.DataTableToXMLMemoryStream(dt, queryName);

            return File(fileStream.ToArray(), ImageHelper.GetContentType(".xml"), "CustomQueryExport.xml");
        }

        public IActionResult ExportToJSON(string queryName, string sortField, string sortDir)
        {
            DataTable dt = GetQueryResult(sortField, sortDir, 1, int.MaxValue);

            var fileStream = _exportHelper.DataTableToJSONMemoryStream(dt);

            return File(fileStream.ToArray(), ImageHelper.GetContentType(".json"), "CustomQueryExport.json");
        }

        public async Task<IActionResult> ExportToWidget(int queryId)
        {
            var query = await _dataQueryService.GetByIdAsync(queryId);
            if (query == null)
                return BadRequest(_localizer["Please check if the query is working."].ToString());
            var cpiWidgets = await _dashboardManager.GetCPiWidgetByQueryId(queryId);
            if (cpiWidgets == null || cpiWidgets.Count == 0)
            {
                DataQueryExportToWidgetViewModel viewModel = new DataQueryExportToWidgetViewModel
                {
                    QueryId = queryId,
                    Title = query.QueryName,
                    AvaliableCharts = await GetCustomWidgetTypeListASString(queryId)
                };
                return PartialView("_ExportToWidgetEntry", viewModel);
            }
            else
            {
                var cpiWidget = cpiWidgets.First();
                DataQueryExportToWidgetViewModel viewModel = new DataQueryExportToWidgetViewModel
                {
                    QueryId = queryId,
                    Category = cpiWidget.Category,
                    Group = cpiWidget.Group,
                    CustomWidgetType = cpiWidget.CustomWidgetType,
                    Title = cpiWidget.Title,
                    CanExport = cpiWidget.CanExport,
                    RecordsLimit = cpiWidget.RecordsLimit,
                    CountColumn = cpiWidget.CountColumn,
                    SharedWidget = cpiWidget.SharedWidget ?? false,
                    AvaliableCharts = await GetCustomWidgetTypeListASString(queryId)
                };
                return PartialView("_ExportToWidgetEntry", viewModel);
            }

        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WidgetSave([FromBody] DataQueryExportToWidgetViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var cpiWidgets = await _dashboardManager.GetCPiWidgetByQueryId(viewModel.QueryId);
                var settings = await _genSettings.GetSetting();
                var colorScheme = settings.ColorScheme ?? "";
                if (cpiWidgets == null || cpiWidgets.Count == 0)
                {
                    var userWidgets = await _dashboardManager.GetUserWidgets(User.GetUserIdentifier(), 7);
                    var cpiWidget = new CPiWidget
                    {
                        Title = viewModel.Title,
                        ViewName = viewModel.CustomWidgetType.Equals("Pie") ? "_CustomPie" :
                                        viewModel.CustomWidgetType.Equals("Donut") ? "_CustomDonut" :
                                        viewModel.CustomWidgetType.Equals("Count") ? "_CustomCount" :
                                        viewModel.CustomWidgetType.Equals("Column") ? "_CustomColumn" :
                                        viewModel.CustomWidgetType.Equals("Bar") ? "_CustomBar" :
                                        viewModel.CustomWidgetType.Equals("Line") ? "_CustomLine" :
                                        viewModel.CustomWidgetType.Equals("List") ? "_CustomList" :
                                        "",
                        RepositoryMethodName = "procCW_RunQuery",
                        SeriesColors = viewModel.CustomWidgetType.Equals("Pie") ? colorScheme :
                                            viewModel.CustomWidgetType.Equals("Donut") ? colorScheme :
                                            viewModel.CustomWidgetType.Equals("Column") ? colorScheme :
                                            viewModel.CustomWidgetType.Equals("Bar") ? colorScheme :
                                            viewModel.CustomWidgetType.Equals("Line") ? colorScheme :
                                            viewModel.CustomWidgetType.Equals("Count") ? colorScheme.Split("|")[0] :
                                            viewModel.CustomWidgetType.Equals("List") ? "" :
                                        "",
                        IsEnabled = true,
                        Icon = viewModel.CustomWidgetType.Equals("Pie") ? "far fa-chart-pie" :
                                            viewModel.CustomWidgetType.Equals("Donut") ? "far fa-chart-pie" :
                                            viewModel.CustomWidgetType.Equals("Bar") ? "far fa-chart-bar" :
                                            viewModel.CustomWidgetType.Equals("Column") ? "far fa-chart-bar" :
                                            viewModel.CustomWidgetType.Equals("Line") ? "far fa-chart-line" :
                                            viewModel.CustomWidgetType.Equals("Count") ? "fas fa-edit" :
                                            viewModel.CustomWidgetType.Equals("List") ? "fal fa-list-ul" :
                                        null,
                        CanExpand = viewModel.CustomWidgetType.Equals("Pie") ? false :
                                            viewModel.CustomWidgetType.Equals("Donut") ? false :
                                            viewModel.CustomWidgetType.Equals("Bar") ? true :
                                            viewModel.CustomWidgetType.Equals("Column") ? true :
                                            viewModel.CustomWidgetType.Equals("Line") ? true :
                                            viewModel.CustomWidgetType.Equals("Count") ? false :
                                            viewModel.CustomWidgetType.Equals("List") ? true :
                                        false,
                        RepositoryClassName = "",
                        RepositoryReturnType = viewModel.CustomWidgetType.Equals("Pie") ? "List<ChartDTO>" :
                                            viewModel.CustomWidgetType.Equals("Donut") ? "List<ChartDTO>" :
                                            viewModel.CustomWidgetType.Equals("Bar") ? "List<ChartDTO>" :
                                            viewModel.CustomWidgetType.Equals("Column") ? "List<ChartDTO>" :
                                            viewModel.CustomWidgetType.Equals("Line") ? "List<ChartDTO>" :
                                            viewModel.CustomWidgetType.Equals("Count") ? "ChartDTO" :
                                            viewModel.CustomWidgetType.Equals("List") ? "List<ListDTO>" :
                                        "",
                        Policy = "",
                        ExportViewModel = "CustomWidget",
                        SystemType = "CORPORATION;LAWFIRM",
                        CanExport = viewModel.CustomWidgetType.Equals("Pie") ? true :
                                            viewModel.CustomWidgetType.Equals("Donut") ? true :
                                            viewModel.CustomWidgetType.Equals("Bar") ? true :
                                            viewModel.CustomWidgetType.Equals("Column") ? true :
                                            viewModel.CustomWidgetType.Equals("Line") ? true :
                                            viewModel.CustomWidgetType.Equals("Count") ? false :
                                            viewModel.CustomWidgetType.Equals("List") ? false :
                                        false,
                        SystemCategory = "CustomWidget",
                        CanDrillDown = false,
                        CanExportPpt = viewModel.CustomWidgetType.Equals("Pie") ? true :
                                            viewModel.CustomWidgetType.Equals("Donut") ? true :
                                            viewModel.CustomWidgetType.Equals("Bar") ? true :
                                            viewModel.CustomWidgetType.Equals("Column") ? true :
                                            viewModel.CustomWidgetType.Equals("Line") ? true :
                                            viewModel.CustomWidgetType.Equals("Count") ? false :
                                            viewModel.CustomWidgetType.Equals("List") ? false :
                                        false,
                        QueryId = viewModel.QueryId,
                        Category = viewModel.Category,
                        Group = viewModel.Group,
                        CustomWidgetType = viewModel.CustomWidgetType,
                        CanCustomWidgetExport = viewModel.CanExport,
                        RecordsLimit = viewModel.RecordsLimit,
                        CountColumn = viewModel.CountColumn,
                        CreatorId = User.GetUserIdentifier(),
                        SharedWidget = viewModel.SharedWidget,
                        RowSpan = viewModel.CustomWidgetType.Equals("Count") ? 1 : 2
                    };
                    await _dashboardManager.AddCPiWidget(cpiWidget);
                    int nextOrder = 0;
                    var lastUserWidget = userWidgets.Where(c => c.CPiUserWidget.WidgetCategory == 7).OrderByDescending(c => c.CPiUserWidget.SortOrder).FirstOrDefault();
                    if (lastUserWidget != null)
                    {
                        nextOrder = lastUserWidget.CPiUserWidget.SortOrder + 1;
                    }
                    CPiUserWidget userWidget = new CPiUserWidget
                    {
                        UserId = User.GetUserIdentifier(),
                        WidgetId = cpiWidget.Id,
                        SortOrder = nextOrder,
                        Settings = "{}",
                        WidgetCategory = 7
                    };

                    await _dashboardManager.AddUserWidget(userWidget);

                    var query = await _dataQueryService.GetByIdAsync(viewModel.QueryId);
                    if (query != null && !query.UsedInWidget)
                    {
                        query.UsedInWidget = true;
                        await _dataQueryService.Update(query);
                    }

                    return Ok();
                }
                else
                {
                    var cpiWidget = cpiWidgets.First();
                    cpiWidget.Title = viewModel.Title;
                    cpiWidget.ViewName = viewModel.CustomWidgetType.Equals("Pie") ? "_CustomPie" :
                                        viewModel.CustomWidgetType.Equals("Donut") ? "_CustomDonut" :
                                        viewModel.CustomWidgetType.Equals("Count") ? "_CustomCount" :
                                        viewModel.CustomWidgetType.Equals("Column") ? "_CustomColumn" :
                                        viewModel.CustomWidgetType.Equals("Bar") ? "_CustomBar" :
                                        viewModel.CustomWidgetType.Equals("Line") ? "_CustomLine" :
                                        viewModel.CustomWidgetType.Equals("List") ? "_CustomList" :
                                        "";
                    cpiWidget.SeriesColors = viewModel.CustomWidgetType.Equals("Pie") ? colorScheme :
                                        viewModel.CustomWidgetType.Equals("Donut") ? colorScheme :
                                        viewModel.CustomWidgetType.Equals("Column") ? colorScheme :
                                        viewModel.CustomWidgetType.Equals("Bar") ? colorScheme :
                                        viewModel.CustomWidgetType.Equals("Line") ? colorScheme :
                                        viewModel.CustomWidgetType.Equals("Count") ? colorScheme.Split("|")[0] :
                                        viewModel.CustomWidgetType.Equals("List") ? "" :
                                    "";
                    cpiWidget.Icon = viewModel.CustomWidgetType.Equals("Pie") ? "far fa-chart-pie" :
                                            viewModel.CustomWidgetType.Equals("Donut") ? "far fa-chart-pie" :
                                            viewModel.CustomWidgetType.Equals("Bar") ? "far fa-chart-bar" :
                                            viewModel.CustomWidgetType.Equals("Column") ? "far fa-chart-bar" :
                                            viewModel.CustomWidgetType.Equals("Line") ? "far fa-chart-line" :
                                            viewModel.CustomWidgetType.Equals("Count") ? "fas fa-edit" :
                                            viewModel.CustomWidgetType.Equals("List") ? "fal fa-list-ul" :
                                        null;
                    cpiWidget.CanExpand = viewModel.CustomWidgetType.Equals("Pie") ? false :
                                            viewModel.CustomWidgetType.Equals("Donut") ? false :
                                            viewModel.CustomWidgetType.Equals("Bar") ? true :
                                            viewModel.CustomWidgetType.Equals("Column") ? true :
                                            viewModel.CustomWidgetType.Equals("Line") ? true :
                                            viewModel.CustomWidgetType.Equals("Count") ? false :
                                            viewModel.CustomWidgetType.Equals("List") ? true :
                                        false;
                    cpiWidget.RepositoryReturnType = viewModel.CustomWidgetType.Equals("Pie") ? "List<ChartDTO>" :
                                            viewModel.CustomWidgetType.Equals("Donut") ? "List<ChartDTO>" :
                                            viewModel.CustomWidgetType.Equals("Bar") ? "List<ChartDTO>" :
                                            viewModel.CustomWidgetType.Equals("Column") ? "List<ChartDTO>" :
                                            viewModel.CustomWidgetType.Equals("Line") ? "List<ChartDTO>" :
                                            viewModel.CustomWidgetType.Equals("Count") ? "ChartDTO" :
                                            viewModel.CustomWidgetType.Equals("List") ? "List<ListDTO>" :
                                        "";
                    cpiWidget.CanExport = viewModel.CustomWidgetType.Equals("Pie") ? true :
                                            viewModel.CustomWidgetType.Equals("Donut") ? true :
                                            viewModel.CustomWidgetType.Equals("Bar") ? true :
                                            viewModel.CustomWidgetType.Equals("Column") ? true :
                                            viewModel.CustomWidgetType.Equals("Line") ? true :
                                            viewModel.CustomWidgetType.Equals("Count") ? false :
                                            viewModel.CustomWidgetType.Equals("List") ? false :
                                        false;
                    cpiWidget.CanExportPpt = viewModel.CustomWidgetType.Equals("Pie") ? true :
                                            viewModel.CustomWidgetType.Equals("Donut") ? true :
                                            viewModel.CustomWidgetType.Equals("Bar") ? true :
                                            viewModel.CustomWidgetType.Equals("Column") ? true :
                                            viewModel.CustomWidgetType.Equals("Line") ? true :
                                            viewModel.CustomWidgetType.Equals("Count") ? false :
                                            viewModel.CustomWidgetType.Equals("List") ? false :
                                        false;
                    cpiWidget.Category = viewModel.Category;
                    cpiWidget.Group = viewModel.Group;
                    cpiWidget.CustomWidgetType = viewModel.CustomWidgetType;
                    cpiWidget.CanCustomWidgetExport = viewModel.CanExport;
                    cpiWidget.RecordsLimit = viewModel.RecordsLimit;
                    cpiWidget.CountColumn = viewModel.CountColumn;
                    cpiWidget.CreatorId = User.GetUserIdentifier();
                    cpiWidget.SharedWidget = viewModel.SharedWidget;
                    cpiWidget.RowSpan = viewModel.CustomWidgetType.Equals("Count") ? 1 : 2;

                    await _dashboardManager.UpdateCPiWidget(cpiWidget);
                    return Ok();
                }
            }
            return BadRequest(ModelState);
        }

        public async Task<IActionResult> GetQueryHeaderList(int queryId)
        {
            DataTable dt = await _customReportService.RunQuery(User.GetUserIdentifier(), queryId);
            var temp = 1;

            List<QueryHeader> result = new List<QueryHeader>();

            foreach (DataColumn column in dt.Columns)
            {
                result.Add(new QueryHeader { Header = column.ColumnName });
            }
            return Json(result);
        }

        private class QueryHeader
        {
            public string Header { get; set; }
        }

        public async Task<IActionResult> GetCustomWidgetTypeList(int? queryId)
        {
            List<WidgetType> result = new List<WidgetType>();

            result.Add(new WidgetType { CustomWidgetType = "Bar" });
            //result.Add(new WidgetType { CustomWidgetType = "Column" }); //Changed to Bar
            result.Add(new WidgetType { CustomWidgetType = "Count" });
            result.Add(new WidgetType { CustomWidgetType = "Donut" });
            result.Add(new WidgetType { CustomWidgetType = "Line" });

            if (queryId != null)
            {
                DataTable dt = await _customReportService.RunQuery(User.GetUserIdentifier(), (int)queryId);

                List<QueryHeader> headers = new List<QueryHeader>();

                foreach (DataColumn column in dt.Columns)
                {
                    headers.Add(new QueryHeader { Header = column.ColumnName });
                }

                if (headers.Any(c => c.Header.ToLower().Equals("casenumber")) && ((headers.Any(c => c.Header.ToLower().Equals("matid")) && headers.Any(c => c.Header.ToLower().Equals("mattertitle"))) || (headers.Any(c => c.Header.ToLower().Equals("appid")) && headers.Any(c => c.Header.ToLower().Equals("apptitle"))) || (headers.Any(c => c.Header.ToLower().Equals("tmkid")) && headers.Any(c => c.Header.ToLower().Equals("trademarkname")))))
                {
                    result.Add(new WidgetType { CustomWidgetType = "List" });
                }

            }

            result.Add(new WidgetType { CustomWidgetType = "Pie" });

            return Json(result);
        }

        public async Task<string> GetCustomWidgetTypeListASString(int? queryId)
        {
            string result = "|Bar|Count|Donut|Line|";

            if (queryId != null)
            {
                DataTable dt = await _customReportService.RunQuery(User.GetUserIdentifier(), (int)queryId);

                List<QueryHeader> headers = new List<QueryHeader>();

                foreach (DataColumn column in dt.Columns)
                {
                    headers.Add(new QueryHeader { Header = column.ColumnName });
                }

                if (headers.Any(c => c.Header.ToLower().Equals("casenumber")) && ((headers.Any(c => c.Header.ToLower().Equals("matid")) && headers.Any(c => c.Header.ToLower().Equals("mattertitle"))) || (headers.Any(c => c.Header.ToLower().Equals("appid")) && headers.Any(c => c.Header.ToLower().Equals("apptitle"))) || (headers.Any(c => c.Header.ToLower().Equals("tmkid")) && headers.Any(c => c.Header.ToLower().Equals("trademarkname")))))
                {
                    result += "List|";
                }

            }

            result += "Pie|";
            return result;
        }

        private class WidgetType
        {
            public string CustomWidgetType { get; set; }
        }

        #endregion

        #region Authorization
        private bool CanUpdateQuery()
        {
            // user can update query definition, if user has any of the PTG query modify policy; the access to different PTG tables is controlled by the backend logic
            if (CanUpdatePatentQuery().Result)
                return true;

            if (CanUpdateTrademarkQuery().Result)
                return true;

            if (CanUpdateGenMatterQuery().Result)
                return true;

            if (CanUpdateAMSQuery().Result)
                return true;

            return false;
        }

        private async Task<bool> CanUpdatePatentQuery()
        {
            return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CustomQueryModify)).Succeeded;
        }

        private async Task<bool> CanUpdateTrademarkQuery()
        {
            return (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CustomQueryModify)).Succeeded;
        }

        private async Task<bool> CanUpdateGenMatterQuery()
        {
            return (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CustomQueryModify)).Succeeded;
        }

        private async Task<bool> CanUpdateAMSQuery()
        {
            return (await _authService.AuthorizeAsync(User, AMSAuthorizationPolicy.CustomQueryModify)).Succeeded;
        }

        #endregion

        #region Tags
        public async Task<IActionResult> GetDataQueryTags()
        {
            var tags = await _dataQueryService.DataQueryTags.Select(t => t.Tag).Distinct().ToArrayAsync();
            return Json(tags);

        }

        public async Task<IActionResult> GetTagPickListData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_dataQueryService.DataQueryTags.Select(s => new { Tag = s.Tag }), request, property, text, filterType, requiredRelation, false);
        }

        public async Task<IActionResult> DataQueryTagsRead([DataSourceRequest] DataSourceRequest request, int id)
        {
            var tags = await _dataQueryService.DataQueryTags.Where(t => t.QueryId == id).OrderBy(t => t.Tag).ToListAsync();
            return Json(tags.ToDataSourceResult(request));
        }

        public async Task<IActionResult> DataQueryTagsUpdate(int? id,
            [Bind(Prefix = "updated")] IEnumerable<DataQueryTag> updated,
            [Bind(Prefix = "new")] IEnumerable<DataQueryTag> added,
            [Bind(Prefix = "deleted")] IEnumerable<DataQueryTag> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                if (id == null || id == 0)
                    id = updated.Any() ? updated.First().QueryId : added.First().QueryId;

                await _dataQueryTagService.Update(id, User.GetUserName(),
                    _mapper.Map<List<DataQueryTag>>(updated),
                    _mapper.Map<List<DataQueryTag>>(added),
                    _mapper.Map<List<DataQueryTag>>(deleted)
                    );
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer["Custom Query Tag has been saved successfully."].ToString() :
                    _localizer["Custom Query Tags have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        public async Task<IActionResult> DataQueryTagDelete([Bind(Prefix = "deleted")] DataQueryTag deleted)
        {
            if (deleted.DQTagId >= 0)
            {
                await _dataQueryTagService.Update(deleted.QueryId, User.GetUserName(), new List<DataQueryTag>(), new List<DataQueryTag>(), new List<DataQueryTag>() { deleted });
                return Ok(new { success = _localizer["Custom Query Tag has been deleted successfully."].ToString() });
            }
            return Ok();
        }
        #endregion

        public async Task<IActionResult> FunctionGridRead([DataSourceRequest] DataSourceRequest request, int parentId)
        {
            var result = _dataQueryService.DataQueryAllowedFunction;
            return Json(result.ToDataSourceResult(request));
        }

    }
}