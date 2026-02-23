using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using R10.Core;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.DashboardViewModels;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Services;
using R10.Web.Services.DocumentStorage;
using R10.Web.Services.iManage;
using R10.Web.Services.NetDocuments;
using R10.Web.Services.SharePoint;
using System.Globalization;
using static R10.Web.Helpers.ImageHelper;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessDocumentVerification)]
    public class DocumentVerificationController : BaseController
    {
        private readonly UserManager<CPiUser> _userManager;
        private readonly ICPiUserGroupManager _groupManager;
        private readonly ICPiSystemSettingManager _systemSettingManager;

        private readonly IDocumentVerificationViewModelService _docVerificationDTOService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        private readonly IReportService _reportService;
        private readonly ExportHelper _exportHelper;

        private readonly IActionDueService<PatActionDue, PatDueDate> _patActionDueService;
        private readonly IDueDateService<PatActionDue, PatDueDate> _patDueDateService;
        private readonly IParentEntityService<PatActionType, PatActionParameter> _patActionTypeService;
        private readonly IPatActionDueViewModelService _patActionDueViewModelService;
        private readonly ICountryApplicationService _countryApplicationService;

        private readonly IActionDueService<TmkActionDue, TmkDueDate> _tmkActionDueService;
        private readonly IDueDateService<TmkActionDue, TmkDueDate> _tmkDueDateService;
        private readonly IParentEntityService<TmkActionType, TmkActionParameter> _tmkActionTypeService;
        private readonly ITmkActionDueViewModelService _tmkActionDueViewModelService;
        private readonly ITmkTrademarkService _tmkTrademarkService;

        private readonly IDocketRequestService _docketRequestService;

        private readonly IGlobalSearchService _globalSearchService;
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;

        private readonly ISharePointService _sharePointService;
        private readonly ISharePointViewModelService _sharePointViewModelService;
        private readonly GraphSettings _graphSettings;

        private readonly IiManageClientFactory _iManageClientFactory;
        private readonly INetDocumentsClientFactory _netDocsClientFactory;

        private readonly IDocumentStorage _documentStorage;
        private readonly IDocumentService _docService;
        private readonly IDocumentsViewModelService _docViewModelService;
        private readonly IWorkflowViewModelService _workflowViewModelService;

        private readonly IQuickEmailService _quickEmailService;

        public DocumentVerificationController(IAuthorizationService authService,
            UserManager<CPiUser> userManager,
            ICPiUserGroupManager groupManager,
            ICPiSystemSettingManager systemSettingManager,
            IDocumentVerificationViewModelService docVerificationDTOService,
            IMapper mapper,
            IStringLocalizer<SharedResource> localizer,
            ISystemSettings<PatSetting> patSettings,
            ISystemSettings<TmkSetting> tmkSettings,
            ISystemSettings<DefaultSetting> defaultSettings,
            IReportService reportService,
            ExportHelper exportHelper,
            IActionDueService<PatActionDue, PatDueDate> patActionDueService,
            IDueDateService<PatActionDue, PatDueDate> patDueDateService,
            IParentEntityService<PatActionType, PatActionParameter> patActionTypeService,
            IPatActionDueViewModelService patActionDueViewModelService,
            ICountryApplicationService countryApplicationService,
            IActionDueService<TmkActionDue, TmkDueDate> tmkActionDueService,
            IDueDateService<TmkActionDue, TmkDueDate> tmkDueDateService,
            IParentEntityService<TmkActionType, TmkActionParameter> tmkActionTypeService,
            ITmkActionDueViewModelService tmkActionDueViewModelService,
            ITmkTrademarkService tmkTrademarkService,
            IEntityService<TmkIndicator> tmkIndicatorService,
            IDocketRequestService docketRequestService,
            IGlobalSearchService globalSearchService,
            IApplicationDbContext repository,
            ISharePointService sharePointService,
            ISharePointViewModelService sharePointViewModelService,
            IOptions<GraphSettings> graphSettings,
            IiManageClientFactory iManageClientFactory,
            INetDocumentsClientFactory netDocsClientFactory,
            IDocumentStorage documentStorage,
            IDocumentService docService,
            IDocumentsViewModelService docViewModelService,
            IWorkflowViewModelService workflowViewModelService,
            IQuickEmailService quickEmailService)
        {
            _userManager = userManager;
            _groupManager = groupManager;
            _systemSettingManager = systemSettingManager;

            _docVerificationDTOService = docVerificationDTOService;
            _mapper = mapper;
            _localizer = localizer;
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            _defaultSettings = defaultSettings;
            _reportService = reportService;
            _exportHelper = exportHelper;

            _patActionDueService = patActionDueService;
            _patDueDateService = patDueDateService;
            _patActionTypeService = patActionTypeService;
            _patActionDueViewModelService = patActionDueViewModelService;
            _countryApplicationService = countryApplicationService;

            _tmkActionDueService = tmkActionDueService;
            _tmkDueDateService = tmkDueDateService;
            _tmkActionTypeService = tmkActionTypeService;
            _tmkActionDueViewModelService = tmkActionDueViewModelService;
            _tmkTrademarkService = tmkTrademarkService;

            _docketRequestService = docketRequestService;

            _globalSearchService = globalSearchService;
            _authService = authService;
            _repository = repository;

            _sharePointService = sharePointService;
            _sharePointViewModelService = sharePointViewModelService;
            _graphSettings = graphSettings.Value;

            _iManageClientFactory = iManageClientFactory;
            _netDocsClientFactory = netDocsClientFactory;

            _documentStorage = documentStorage;
            _docService = docService;
            _docViewModelService = docViewModelService;
            _workflowViewModelService = workflowViewModelService;

            _quickEmailService = quickEmailService;
        }

        #region Main
        //only to filter records by system type, still uses the Shared permission 
        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessDocumentVerification)]
        public async Task<IActionResult> Patent()
        {
            return await Startup(SystemTypeCode.Patent);
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessDocumentVerification)]
        public async Task<IActionResult> Trademark()
        {
            return await Startup(SystemTypeCode.Trademark);
        }

        [Authorize(Policy = GeneralMatterAuthorizationPolicy.CanAccessDocumentVerification)]
        public async Task<IActionResult> GeneralMatter()
        {
            return await Startup(SystemTypeCode.GeneralMatter);
        }

        public async Task<IActionResult> Index()
        {
            var systems = User.GetSystems();
            if (systems.Any(s => s == SystemType.Patent))
                return await Startup(SystemTypeCode.Patent);
            else if (systems.Any(s => s == SystemType.Trademark))
                return await Startup(SystemTypeCode.Trademark);
            else if (systems.Any(s => s == SystemType.GeneralMatter))
                return await Startup(SystemTypeCode.GeneralMatter);
            else
                return await Startup(SystemTypeCode.Patent);
        }

        private async Task<IActionResult> Startup(string systemType)
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "documentVerificationSearch",
                Title = "",
                SystemType = systemType,
                GridPageSize = 8
            };

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View("Index", model);
        }

        public async Task<IActionResult> GetDocVerificationCount()
        {
            var toDoItemCount = 0;
            var defaultSettings = await _defaultSettings.GetSetting();
            var canAccessPatDocVer = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;
            var canAccessTmkDocVer = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;
            var canAccessGmDocVer = (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;

            if (!canAccessPatDocVer && !canAccessTmkDocVer && !canAccessGmDocVer)
                return Json(new { ToDoItemCount = toDoItemCount });

            var userId = User.GetUserIdentifier();
            var userGroups = await _groupManager.CPiUserGroups.Where(d => d.UserId == userId).Select(d => new { GroupName = d.CPiGroup != null ? d.CPiGroup.Name : "", d.GroupId }).Distinct().ToListAsync();
            var userGroupIds = userGroups.Select(d => d.GroupId).Distinct().ToList();

            var docDocuments = _docService.DocDocuments.AsNoTracking()
                                .Where(d => (canAccessPatDocVer && d.DocFolder != null
                                            && (d.DocFolder.SystemType ?? "").ToUpper() == SystemTypeCode.Patent.ToUpper()
                                            && (d.DocFolder.DataKey ?? "").ToLower() == "appid")
                                        || (canAccessTmkDocVer && d.DocFolder != null
                                            && (d.DocFolder.SystemType ?? "").ToUpper() == SystemTypeCode.Trademark.ToUpper()
                                            && (d.DocFolder.DataKey ?? "").ToLower() == "tmkid")
                                        || (canAccessGmDocVer && d.DocFolder != null
                                            && (d.DocFolder.SystemType ?? "").ToUpper() == SystemTypeCode.GeneralMatter.ToUpper()
                                            && (d.DocFolder.DataKey ?? "").ToLower() == "matid"));

            //****************************************************************************
            //2nd tab

            //DocVerification
            //Count ActionTypes need to be created from documents
            //Verified documents without actions/verifications and "Docket Required?" (IsActRequired) checked
            //AND user is listed as Responsible (Docketing) on the document
            //Empty DocVerifications
            toDoItemCount += await docDocuments
                            .Where(v => v.IsVerified && v.IsActRequired
                                && (v.DocResponsibleDocketings != null && v.DocResponsibleDocketings.Any(dp => dp.UserId == userId || userGroupIds.Contains(dp.GroupId ?? 0)))
                                && (v.DocVerifications == null || !v.DocVerifications.Any())
                            )
                            .CountAsync();

            //ActionType from DocVerifications
            toDoItemCount += await docDocuments
                            .Where(v => v.IsVerified && v.IsActRequired
                                && (v.DocResponsibleDocketings != null && v.DocResponsibleDocketings.Any(dp => dp.UserId == userId || userGroupIds.Contains(dp.GroupId ?? 0)))
                                && (v.DocVerifications != null && v.DocVerifications.Any(dv => dv.ActionTypeID > 0))
                            )
                            .SelectMany(d => d.DocVerifications!.Select(v => new { v.DocId, v.ActionTypeID }))
                            .Where(d => d.ActionTypeID > 0)
                            .CountAsync();

            //DocketRequest - DeDocket
            if (canAccessPatDocVer)
            {
                toDoItemCount += _docketRequestService.PatDocketRequests.AsNoTracking().Include(d => d.PatDocketRequestResps).AsEnumerable()
                    .Where(d => d.CompletedDate == null && d.PatDocketRequestResps != null && d.PatDocketRequestResps.Any(r => r.UserId == userId || userGroupIds.Contains(r.GroupId ?? 0)))
                    .DistinctBy(d => d.ReqId).Count();

                if (defaultSettings.IncludeDeDocketInVerification == true)
                    toDoItemCount += _repository.PatDueDateDeDockets.AsNoTracking().Include(d => d.PatDueDateDeDocketResps).AsEnumerable()
                        .Where(d => d.CompletedDate == null && d.InstructionCompleted == false && d.PatDueDateDeDocketResps != null
                            && d.PatDueDateDeDocketResps.Any(r => r.UserId == userId || userGroupIds.Contains(r.GroupId ?? 0)))
                        .DistinctBy(d => d.DeDocketId).Count();
            }
            if (canAccessTmkDocVer)
            {
                toDoItemCount += _docketRequestService.TmkDocketRequests.AsNoTracking().Include(d => d.TmkDocketRequestResps).AsEnumerable()
                    .Where(d => d.CompletedDate == null && d.TmkDocketRequestResps != null && d.TmkDocketRequestResps.Any(r => r.UserId == userId || userGroupIds.Contains(r.GroupId ?? 0)))
                    .DistinctBy(d => d.ReqId).Count();

                if (defaultSettings.IncludeDeDocketInVerification == true)
                    toDoItemCount += _repository.TmkDueDateDeDockets.AsNoTracking().Include(d => d.TmkDueDateDeDocketResps).AsEnumerable()
                        .Where(d => d.CompletedDate == null && d.InstructionCompleted == false && d.TmkDueDateDeDocketResps != null
                            && d.TmkDueDateDeDocketResps.Any(r => r.UserId == userId || userGroupIds.Contains(r.GroupId ?? 0)))
                        .DistinctBy(d => d.DeDocketId).Count();
            }
            if (canAccessGmDocVer)
            {
                toDoItemCount += _docketRequestService.GMDocketRequests.AsNoTracking().Include(d => d.GMDocketRequestResps).AsEnumerable()
                    .Where(d => d.CompletedDate == null && d.GMDocketRequestResps != null && d.GMDocketRequestResps.Any(r => r.UserId == userId || userGroupIds.Contains(r.GroupId ?? 0)))
                    .DistinctBy(d => d.ReqId).Count();

                if (defaultSettings.IncludeDeDocketInVerification == true)
                    toDoItemCount += _repository.GMDueDateDeDockets.AsNoTracking().Include(d => d.GMDueDateDeDocketResps).AsEnumerable()
                        .Where(d => d.CompletedDate == null && d.InstructionCompleted == false && d.GMDueDateDeDocketResps != null
                            && d.GMDueDateDeDocketResps.Any(r => r.UserId == userId || userGroupIds.Contains(r.GroupId ?? 0)))
                        .DistinctBy(d => d.DeDocketId).Count();
            }

            //****************************************************************************
            //4th tab - Count documents need to be sent to client
            //Verified documents with "Forward document to client?" (SendToClient) checked
            //AND not yet sent out (tblDocQELog is empty)
            //AND user is listed as Responsible (Reporting) on the document
            toDoItemCount += await docDocuments
                                .Where(d => d.IsVerified && d.SendToClient
                                    && (d.DocQuickEmailLogs == null || !d.DocQuickEmailLogs.Any())
                                    && (d.DocResponsibleReportings != null && d.DocResponsibleReportings.Any(dp => dp.UserId == userId || userGroupIds.Contains(dp.GroupId ?? 0)))
                                    && (d.DocVerifications == null || !d.DocVerifications.Any() || d.DocVerifications.Any(dv => dv.ActionTypeID > 0))
                                ).CountAsync();

            return Json(new { ToDoItemCount = toDoItemCount });
        }

        public async Task<IActionResult> DocVerification()
        {
            var userId = User.GetUserIdentifier();
            var userGroups = await _groupManager.CPiUserGroups.Where(d => d.UserId == userId).Select(d => new { GroupName = d.CPiGroup != null ? d.CPiGroup.Name : "", d.GroupId }).Distinct().ToListAsync();
            var inDocketingGroup = userGroups.Any(d => d.GroupName.ToLower() == "docketing");
            var inRerpotingGroup = userGroups.Any(d => d.GroupName.ToLower() == "communication");

            ViewData["DefaultResponsibleId"] = userId;
            ViewData["InDocketingGroup"] = inDocketingGroup;
            ViewData["InReportingGroup"] = inRerpotingGroup;

            var systemTypes = "";
            if ((await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded)
                systemTypes += "|" + SystemTypeCode.Patent;
            if ((await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded)
                systemTypes += "|" + SystemTypeCode.Trademark;
            if ((await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded)
                systemTypes += "|" + SystemTypeCode.GeneralMatter;

            return await Startup(systemTypes);
        }
        #endregion

        #region Update Responsible Docketing-Reporting
        [HttpGet()]
        public async Task<IActionResult> UpdateResponsible(string ids, DocRespLogType? respType, string? docNames = "")
        {
            //ids: SystemType|DataKey|DataKeyValue
            //Ex: P|DocId|123;P|ReqId|321;P|DeDocketId|123
            var idList = ParseSystemDateKey(ids);

            if (idList == null || idList.Count <= 0)
                return BadRequest();

            if (respType != null)
            {
                //Get existing responsible (Docketing/Reporting)
                var respIds = new List<(string UserId, int GroupId)>();

                //tblDocRespDocketing - tblDocRespReporting
                var docIdList = idList.Where(d => !string.IsNullOrEmpty(d.DataKey) && d.DataKey.ToLower() == "docid").Select(d => d.DataKeyValue).ToList();
                if (respType == DocRespLogType.Docketing)
                {
                    respIds = (await _docService.DocRespDocketings.AsNoTracking()
                                        .Where(d => d.DocDocument != null && d.DocDocument.DocId > 0 && ((d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0))
                                            && docIdList.Contains(d.DocDocument.DocId)
                                        )
                                        .Select(d => new { UserId = d.UserId ?? "", GroupId = d.GroupId ?? 0 }).ToListAsync()).Select(d => (d.UserId, d.GroupId)).ToList();
                }
                else if (respType == DocRespLogType.Reporting)
                {
                    respIds = (await _docService.DocRespReportings.AsNoTracking()
                                        .Where(d => d.DocDocument != null && d.DocDocument.DocId > 0 && ((d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0))
                                            && docIdList.Contains(d.DocDocument.DocId)
                                        )
                                        .Select(d => new { UserId = d.UserId ?? "", GroupId = d.GroupId ?? 0 }).ToListAsync()).Select(d => (d.UserId, d.GroupId)).ToList();
                }

                //tbl_RequestDocketResp
                var reqIdList = idList.Where(d => !string.IsNullOrEmpty(d.DataKey) && d.DataKey.ToLower() == "reqid").Select(d => new { d.SystemType, d.DataKeyValue }).ToList();
                var patReqIds = reqIdList.Where(d => d.SystemType.ToLower() == SystemTypeCode.Patent.ToLower()).Select(d => d.DataKeyValue).Distinct().ToList();
                if (patReqIds != null && patReqIds.Count > 0)
                    respIds.AddRange((await _docketRequestService.PatDocketRequestResps.AsNoTracking()
                                            .Where(d => patReqIds.Contains(d.ReqId) && ((d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0)))
                                            .Select(d => new { UserId = d.UserId ?? "", GroupId = d.GroupId ?? 0 }).ToListAsync())
                                    .Select(d => (d.UserId, d.GroupId)).ToList());

                var tmkReqIds = reqIdList.Where(d => d.SystemType.ToLower() == SystemTypeCode.Trademark.ToLower()).Select(d => d.DataKeyValue).Distinct().ToList();
                if (tmkReqIds != null && tmkReqIds.Count > 0)
                    respIds.AddRange((await _docketRequestService.TmkDocketRequestResps.AsNoTracking()
                                            .Where(d => tmkReqIds.Contains(d.ReqId) && ((d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0)))
                                            .Select(d => new { UserId = d.UserId ?? "", GroupId = d.GroupId ?? 0 }).ToListAsync())
                                    .Select(d => (d.UserId, d.GroupId)).ToList());

                var gmReqIds = reqIdList.Where(d => d.SystemType.ToLower() == SystemTypeCode.GeneralMatter.ToLower()).Select(d => d.DataKeyValue).Distinct().ToList();
                if (gmReqIds != null && gmReqIds.Count > 0)
                    respIds.AddRange((await _docketRequestService.GMDocketRequestResps.AsNoTracking()
                                            .Where(d => gmReqIds.Contains(d.ReqId) && ((d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0)))
                                            .Select(d => new { UserId = d.UserId ?? "", GroupId = d.GroupId ?? 0 }).ToListAsync())
                                    .Select(d => (d.UserId, d.GroupId)).ToList());

                //tbl_DueDateDeDocketResp
                var dedocketIdList = idList.Where(d => !string.IsNullOrEmpty(d.DataKey) && d.DataKey.ToLower() == "dedocketid").Select(d => new { d.SystemType, d.DataKeyValue }).ToList();
                var patDeDocketIds = dedocketIdList.Where(d => d.SystemType.ToLower() == SystemTypeCode.Patent.ToLower()).Select(d => d.DataKeyValue).Distinct().ToList();
                if (patDeDocketIds != null && patDeDocketIds.Count > 0)
                    respIds.AddRange((await _patDueDateService.DueDateDeDocketResps.AsNoTracking()
                                            .Where(d => patDeDocketIds.Contains(d.DeDocketId) && ((d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0)))
                                            .Select(d => new { UserId = d.UserId ?? "", GroupId = d.GroupId ?? 0 }).ToListAsync())
                                    .Select(d => (d.UserId, d.GroupId)).ToList());

                var tmkDeDocketIds = reqIdList.Where(d => d.SystemType.ToLower() == SystemTypeCode.Trademark.ToLower()).Select(d => d.DataKeyValue).Distinct().ToList();
                if (tmkDeDocketIds != null && tmkDeDocketIds.Count > 0)
                    respIds.AddRange((await _tmkDueDateService.DueDateDeDocketResps.AsNoTracking()
                                            .Where(d => tmkDeDocketIds.Contains(d.DeDocketId) && ((d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0)))
                                            .Select(d => new { UserId = d.UserId ?? "", GroupId = d.GroupId ?? 0 }).ToListAsync())
                                    .Select(d => (d.UserId, d.GroupId)).ToList());

                // GM module removed - GeneralMatter docket lookup disabled
                // var gmDeDocketIds = reqIdList.Where(d => d.SystemType.ToLower() == SystemTypeCode.GeneralMatter.ToLower()).Select(d => d.DataKeyValue).Distinct().ToList();
                // if (gmDeDocketIds != null && gmDeDocketIds.Count > 0)
                //     respIds.AddRange((await _gmDueDateService.DueDateDeDocketResps.AsNoTracking()
                //                             .Where(d => gmDeDocketIds.Contains(d.DeDocketId) && ((d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0)))
                //                             .Select(d => new { UserId = d.UserId ?? "", GroupId = d.GroupId ?? 0 }).ToListAsync())
                //                     .Select(d => (d.UserId, d.GroupId)).ToList());

                //Get responsible names with id
                var userIds = new List<string>();
                var groupIds = new List<int>();
                if (respIds != null && respIds.Count > 0)
                {
                    userIds.AddRange(respIds.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).Distinct().ToList());
                    groupIds.AddRange(respIds.Where(d => d.GroupId > 0).Select(d => d.GroupId).Distinct().ToList());
                }

                var respList = _userManager.Users
                                            .Select(d => new { d.Id, d.FirstName, d.LastName })
                                            .AsEnumerable()
                                            .Where(d => userIds.Contains(d.Id))
                                            .Select(d => new SelectListItem { Text = d.FirstName + " " + d.LastName, Value = d.Id.ToString() }).ToList();

                respList.AddRange(_groupManager.QueryableList
                                            .Select(d => new { d.Id, d.Name })
                                            .AsEnumerable()
                                            .Where(d => groupIds.Contains(d.Id))
                                            .Select(d => new SelectListItem { Text = d.Name, Value = d.Id.ToString() }).ToList());

                ViewData["ResponsibleList"] = respList;
            }

            ViewData["DocumentNames"] = docNames;
            ViewData["ResponsibleType"] = respType;

            return PartialView("_UpdateResponsible", ids);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateResponsible(DocRespLogType respType, string ids, List<string> oldResponsible, List<string> newResponsible)
        {
            if (string.IsNullOrEmpty(ids))
                return BadRequest("Missing Ids");

            if (!newResponsible.Any())
                return BadRequest("Missing Responsible");

            var keyIds = ParseSystemDateKey(ids);

            if (keyIds == null || keyIds.Count <= 0)
                return BadRequest("Cannot parse ids");

            //Get old and new UserIds and GroupIds for responsible
            var newResponsibleParsed = ParseResponsibleData(newResponsible);
            var newUserList = newResponsibleParsed.userList;
            var newGroupList = newResponsibleParsed.groupList;

            var oldResponsibleParsed = ParseResponsibleData(oldResponsible);
            var oldUserList = oldResponsibleParsed.userList;
            var oldGroupList = oldResponsibleParsed.groupList;

            //Return if nothing in new UserIds and GroupIds in both Resp Docketing and Reporting
            if ((newUserList == null || newUserList.Count <= 0) && (newGroupList == null || newGroupList.Count <= 0))
                return Ok();

            var settings = await _defaultSettings.GetSetting();
            var emailWorkflows = new List<WorkflowEmailViewModel>();
            var userName = User.GetUserName();
            var userEmail = User.GetEmail();

            emailWorkflows.AddRange(await UpdateDocumentResponsible(respType, keyIds, newUserList, newGroupList, oldUserList, oldGroupList));
            emailWorkflows.AddRange(await UpdateRequestDocketResponsible(keyIds, newUserList, newGroupList, oldUserList, oldGroupList));
            emailWorkflows.AddRange(await UpdateDeDocketResponsible(keyIds, newUserList, newGroupList, oldUserList, oldGroupList));

            if (emailWorkflows != null && emailWorkflows.Any())
            {
                var emailUrl = emailWorkflows.First().emailUrl;
                return Json(new { id = 0, sendEmail = true, folderId = 0, emailUrl, emailWorkflows });
            }

            return Json(new { docIds = 0 });
        }

        [HttpPost]
        public async Task<IActionResult> AssignDocumentResponsible(string ids, List<string> responsibleDocketing, List<string> responsibleReporting)
        {
            if (string.IsNullOrEmpty(ids))
                return BadRequest("Missing Ids");

            if (!responsibleDocketing.Any() && !responsibleReporting.Any())
                return BadRequest("Missing Responsible");

            var keyIds = ParseSystemDateKey(ids);

            if (keyIds == null || keyIds.Count <= 0)
                return BadRequest("Cannot parse ids");

            //Get old and new UserIds and GroupIds for responsible
            var responsibleDocketingParsed = ParseResponsibleData(responsibleDocketing);
            var newDocketing_UserList = responsibleDocketingParsed.userList;
            var newDocketing_GroupList = responsibleDocketingParsed.groupList;

            var responsibleReportingParsed = ParseResponsibleData(responsibleReporting);
            var newReporting_UserList = responsibleReportingParsed.userList;
            var newReporting_GroupList = responsibleReportingParsed.groupList;

            //Return if nothing in new UserIds and GroupIds in both Resp Docketing and Reportings
            if (!newDocketing_UserList.Any() && !newDocketing_GroupList.Any() && !newReporting_UserList.Any() && !newReporting_GroupList.Any())
                return Ok();

            var settings = await _defaultSettings.GetSetting();
            var emailWorkflows = new List<WorkflowEmailViewModel>();
            var userName = User.GetUserName();
            var userEmail = User.GetEmail();

            if (newDocketing_UserList.Any() || newDocketing_GroupList.Any())
                emailWorkflows.AddRange(await UpdateDocumentResponsible(DocRespLogType.Docketing, keyIds, newDocketing_UserList, newDocketing_GroupList, null, null, true));

            if (newReporting_UserList.Any() || newReporting_GroupList.Any())
                emailWorkflows.AddRange(await UpdateDocumentResponsible(DocRespLogType.Reporting, keyIds, newReporting_UserList, newReporting_GroupList, null, null, true));

            if (emailWorkflows != null && emailWorkflows.Any())
            {
                var emailUrl = emailWorkflows.First().emailUrl;
                return Json(new { id = 0, sendEmail = true, folderId = 0, emailUrl, emailWorkflows });
            }

            return Json(new { docIds = 0 });
        }

        private async Task<List<WorkflowEmailViewModel>> UpdateDocumentResponsible(DocRespLogType respType, List<SystemDataKey> keyIds, List<string>? newUserList, List<int>? newGroupList, List<string>? oldUserList, List<int>? oldGroupList, bool replaceAll = false)
        {
            var settings = await _defaultSettings.GetSetting();
            var emailWorkflows = new List<WorkflowEmailViewModel>();
            var userName = User.GetUserName();

            var docIds = keyIds.Where(d => !string.IsNullOrEmpty(d.DataKey) && d.DataKey.ToLower() == "docid").Select(d => d.DataKeyValue).ToList();
            foreach (var docId in docIds)
            {
                var currentRespDocketings = new List<DocResponsibleDocketing>();
                var currentRespReportings = new List<DocResponsibleReporting>();

                var hasNewRespDocketing = false;
                var hasRespDocketingReassigned = false;

                var hasNewRespReporting = false;
                var hasRespReportingReassigned = false;

                if (respType == DocRespLogType.Docketing)
                {
                    currentRespDocketings = await _docService.DocRespDocketings.AsNoTracking()
                        .Where(d => d.DocId == docId).ToListAsync();
                }
                else if (respType == DocRespLogType.Reporting)
                {
                    currentRespReportings = await _docService.DocRespReportings.AsNoTracking()
                        .Where(d => d.DocId == docId).ToListAsync();
                }

                //Compare old and new Resp Docketing, and update
                //Only process if there is something in new UserIds or GroupIds for Resp Docketing
                if (respType == DocRespLogType.Docketing && ((newGroupList != null && newGroupList.Count > 0) || (newUserList != null && newUserList.Count > 0)))
                {

                    var currentDocketingUserList = currentRespDocketings.Where(d => !string.IsNullOrEmpty(d.UserId) && (d.GroupId == null || d.GroupId == 0)).Select(d => d.UserId ?? "").ToList();
                    var currentDocketingGroupList = currentRespDocketings.Where(d => d.GroupId > 0 && string.IsNullOrEmpty(d.UserId)).Select(d => d.GroupId ?? 0).ToList();

                    // Check Users
                    var userUpdateResult = UpdateResponsible(currentDocketingUserList, oldUserList, newUserList, replaceAll);
                    currentDocketingUserList = userUpdateResult.UpdatedList;
                    hasRespDocketingReassigned = userUpdateResult.HasReassigned ? userUpdateResult.HasReassigned : hasRespDocketingReassigned;
                    hasNewRespDocketing = userUpdateResult.HasNew ? userUpdateResult.HasNew : hasNewRespDocketing;

                    // Check Groups
                    var groupUpdateResult = UpdateResponsible(currentDocketingGroupList, oldGroupList, newGroupList, replaceAll);
                    currentDocketingGroupList = groupUpdateResult.UpdatedList;
                    hasRespDocketingReassigned = groupUpdateResult.HasReassigned ? groupUpdateResult.HasReassigned : hasRespDocketingReassigned;
                    hasNewRespDocketing = groupUpdateResult.HasNew ? groupUpdateResult.HasNew : hasNewRespDocketing;

                    //Update
                    if (hasNewRespDocketing || hasRespDocketingReassigned)
                    {
                        var updatedRespDocketingList = currentDocketingUserList.Union(currentDocketingGroupList.Select(d => d.ToString())).Distinct().ToList();
                        await _docService.UpdateDocRespDocketing(updatedRespDocketingList, userName, docId);
                    }

                }

                //Compare old and new Resp Reporting, and update
                //Only process if there is something in new UserIds or GroupIds for Resp Reporting
                if (respType == DocRespLogType.Reporting && ((newGroupList != null && newGroupList.Count > 0) || (newUserList != null && newUserList.Count > 0)))
                {
                    var currentReportingUserList = currentRespReportings.Where(d => !string.IsNullOrEmpty(d.UserId) && (d.GroupId == null || d.GroupId == 0)).Select(d => d.UserId ?? "").ToList();
                    var currentReportingGroupList = currentRespReportings.Where(d => d.GroupId > 0 && string.IsNullOrEmpty(d.UserId)).Select(d => d.GroupId ?? 0).ToList();

                    // Check Users
                    var userUpdateResult = UpdateResponsible(currentReportingUserList, oldUserList, newUserList, replaceAll);
                    currentReportingUserList = userUpdateResult.UpdatedList;
                    hasRespReportingReassigned = userUpdateResult.HasReassigned ? userUpdateResult.HasReassigned : hasRespReportingReassigned;
                    hasNewRespReporting = userUpdateResult.HasNew ? userUpdateResult.HasNew : hasNewRespReporting;

                    // Check Groups
                    var groupUpdateResult = UpdateResponsible(currentReportingGroupList, oldGroupList, newGroupList, replaceAll);
                    currentReportingGroupList = groupUpdateResult.UpdatedList;
                    hasRespReportingReassigned = groupUpdateResult.HasReassigned ? groupUpdateResult.HasReassigned : hasRespReportingReassigned;
                    hasNewRespReporting = groupUpdateResult.HasNew ? groupUpdateResult.HasNew : hasNewRespReporting;

                    if (hasNewRespReporting || hasRespReportingReassigned)
                    {
                        var updatedRespReportingList = currentReportingUserList.Union(currentReportingGroupList.Select(d => d.ToString())).Distinct().ToList();
                        await _docService.UpdateDocRespReporting(updatedRespReportingList, userName, docId);
                    }
                }

                //Only process workflow is updating 1 document               
                //Updating multiple documents could result in a lot of workflow emails to send out
                if (keyIds.Count == 1 && (hasNewRespDocketing || hasRespDocketingReassigned || hasNewRespReporting || hasRespReportingReassigned))
                {
                    //Prepare workflows
                    var document = await _docService.DocDocuments.AsNoTracking().Where(d => d.DocId == docId)
                                            .Select(d => new
                                            {
                                                d.DocId,
                                                d.FileId,
                                                SystemType = d.DocFolder != null ? d.DocFolder.SystemType : "",
                                                ScreenCode = d.DocFolder != null ? d.DocFolder.ScreenCode : "",
                                                DataKey = d.DocFolder != null ? d.DocFolder.DataKey : "",
                                                DataKeyValue = d.DocFolder != null ? d.DocFolder.DataKeyValue : 0
                                            }).FirstOrDefaultAsync();

                    if (document == null) continue;

                    var docFile = await _docService.DocFiles.AsNoTracking().Where(d => d.FileId == document.FileId).Select(d => new { d.UserFileName, d.DriveItemId }).FirstOrDefaultAsync();
                    if (docFile != null && !string.IsNullOrEmpty(docFile.UserFileName))
                    {
                        var attachments = new List<WorkflowEmailAttachmentViewModel>() {
                            //Use actual filename for locating the file (Use FileId, DocFileName could be Null)
                            //new WorkflowEmailAttachmentViewModel { DocId = doc.DocId, FileId = doc.FileId, FileName = userFileName, DocParent = docFolder.DataKeyValue }
                            new WorkflowEmailAttachmentViewModel {
                                DocId = document.DocId,
                                FileId = document.FileId,
                                FileName = $"{document.FileId}{Path.GetExtension(docFile.UserFileName)}",
                                DocParent = document.DataKeyValue,
                                Id = settings.IsSharePointIntegrationOn ? docFile.DriveItemId : null
                            }
                        };

                        var documentLink = $"{document.SystemType}|{document.ScreenCode}|{document.DataKey}|{document.DataKeyValue.ToString()}";
                        var workflowHeader = await GenerateWorkflow(documentLink, attachments, false, hasNewRespDocketing, hasRespDocketingReassigned, hasNewRespReporting, hasRespReportingReassigned);
                        var emailWorkflowList = GenerateEmailWorkflow(workflowHeader, attachments, document.DataKeyValue);
                        if (emailWorkflowList.Any())
                            emailWorkflows.AddRange(emailWorkflowList);
                    }
                }
            }

            return emailWorkflows;
        }

        private (List<T> UpdatedList, bool HasReassigned, bool HasNew) UpdateResponsible<T>(List<T> currentList, List<T>? oldList, List<T>? newList, bool replaceAll)
        {
            bool hasReassigned = false;
            bool hasNew = false;

            // Create a new list to avoid modifying the original list directly
            List<T> updatedList = new List<T>(currentList);

            // Check items to be removed
            if (oldList != null && oldList.Any(d => updatedList.Contains(d)))
            {
                hasReassigned = true;
                updatedList.RemoveAll(d => oldList.Contains(d));
            }

            // Check if replace all items that are not in newList
            if (replaceAll && newList != null)
            {
                hasReassigned = updatedList.Any(d => !newList.Contains(d));
                updatedList.RemoveAll(d => !newList.Contains(d));
            }

            // Check items to be added
            if (newList != null && newList.Any(d => !updatedList.Contains(d)))
            {
                hasNew = true;
                updatedList.AddRange(newList.Where(d => !updatedList.Contains(d)).ToList());
            }

            return (updatedList, hasReassigned, hasNew);
        }

        private async Task<List<WorkflowEmailViewModel>> UpdateRequestDocketResponsible(List<SystemDataKey> keyIds, List<string>? newUserList, List<int>? newGroupList, List<string>? oldUserList, List<int>? oldGroupList)
        {
            var settings = await _defaultSettings.GetSetting();
            var emailWorkflows = new List<WorkflowEmailViewModel>();
            var userName = User.GetUserName();

            var reqIds = keyIds.Where(d => !string.IsNullOrEmpty(d.DataKey) && d.DataKey.ToLower() == "reqid").ToList();
            foreach (var keyItem in reqIds)
            {
                var hasNewDocketRequestResp = false;
                var hasDocketRequestReassigned = false;
                int reqId = keyItem.DataKeyValue;
                var currentDocketRequestResps = new List<DocketRequestResp>();

                switch (keyItem.SystemType!.ToUpper())
                {
                    case SystemTypeCode.Patent:
                        currentDocketRequestResps = await _docketRequestService.PatDocketRequestResps.AsNoTracking()
                        .Where(d => d.ReqId == reqId).Select(d => new DocketRequestResp() { RespId = d.RespId, ReqId = d.ReqId, UserId = d.UserId, GroupId = d.GroupId }).ToListAsync();
                        break;
                    case SystemTypeCode.Trademark:
                        currentDocketRequestResps = await _docketRequestService.TmkDocketRequestResps.AsNoTracking()
                        .Where(d => d.ReqId == reqId).Select(d => new DocketRequestResp() { RespId = d.RespId, ReqId = d.ReqId, UserId = d.UserId, GroupId = d.GroupId }).ToListAsync();
                        break;
                    case SystemTypeCode.GeneralMatter:
                        currentDocketRequestResps = await _docketRequestService.GMDocketRequestResps.AsNoTracking()
                        .Where(d => d.ReqId == reqId).Select(d => new DocketRequestResp() { RespId = d.RespId, ReqId = d.ReqId, UserId = d.UserId, GroupId = d.GroupId }).ToListAsync();
                        break;
                    default:
                        break;
                }

                //Compare old and new Resp Docketing, and update
                //Only process if there is something in new UserIds or GroupIds for Resp Docketing
                if ((newGroupList != null && newGroupList.Count > 0) || (newUserList != null && newUserList.Count > 0))
                {

                    var currentDocketingUserList = currentDocketRequestResps.Where(d => !string.IsNullOrEmpty(d.UserId) && (d.GroupId == null || d.GroupId == 0)).Select(d => d.UserId ?? "").ToList();
                    var currentDocketingGroupList = currentDocketRequestResps.Where(d => d.GroupId > 0 && string.IsNullOrEmpty(d.UserId)).Select(d => d.GroupId ?? 0).ToList();

                    //Check Users
                    if (oldUserList != null && oldUserList.Any(d => currentDocketingUserList.Contains(d)))
                    {
                        hasDocketRequestReassigned = true;
                        currentDocketingUserList.RemoveAll(d => oldUserList.Contains(d));
                    }

                    if (newUserList != null && newUserList.Any(d => !currentDocketingUserList.Contains(d)))
                    {
                        hasNewDocketRequestResp = true;
                        currentDocketingUserList.AddRange(newUserList.Where(d => !currentDocketingUserList.Contains(d)).ToList());
                    }

                    //Check Groups
                    if (oldGroupList != null && oldGroupList.Any(d => currentDocketingGroupList.Contains(d)))
                    {
                        hasDocketRequestReassigned = true;
                        currentDocketingGroupList.RemoveAll(d => oldGroupList.Contains(d));
                    }

                    if (newGroupList != null && newGroupList.Any(d => !currentDocketingGroupList.Contains(d)))
                    {
                        hasNewDocketRequestResp = true;
                        currentDocketingGroupList.AddRange(newGroupList.Where(d => !currentDocketingGroupList.Contains(d)).ToList());
                    }

                    //Update
                    if (hasNewDocketRequestResp || hasDocketRequestReassigned)
                    {
                        var updatedRespDocketingList = currentDocketingUserList.Union(currentDocketingGroupList.Select(d => d.ToString())).Distinct().ToList();

                        switch (keyItem.SystemType.ToUpper())
                        {
                            case SystemTypeCode.Patent:
                                await _docketRequestService.UpdatePatDocketRequestResp(updatedRespDocketingList, userName, reqId);
                                break;
                            case SystemTypeCode.Trademark:
                                await _docketRequestService.UpdateTmkDocketRequestResp(updatedRespDocketingList, userName, reqId);
                                break;
                            case SystemTypeCode.GeneralMatter:
                                await _docketRequestService.UpdateGMDocketRequestResp(updatedRespDocketingList, userName, reqId);
                                break;
                            default:
                                break;
                        }
                    }

                }

                ////Only process workflow is updating 1 document               
                ////Updating multiple documents could result in a lot of workflow emails to send out
                //if (keyIds.Count == 1 && (hasNewRespDocketing || hasRespDocketingReassigned || hasNewRespReporting || hasRespReportingReassigned))
                //{
                //    //Prepare workflows
                //    var document = await _docService.DocDocuments.AsNoTracking().Where(d => d.DocId == docId)
                //                            .Select(d => new {
                //                                d.DocId,
                //                                d.FileId,
                //                                SystemType = d.DocFolder != null ? d.DocFolder.SystemType : "",
                //                                ScreenCode = d.DocFolder != null ? d.DocFolder.ScreenCode : "",
                //                                DataKey = d.DocFolder != null ? d.DocFolder.DataKey : "",
                //                                DataKeyValue = d.DocFolder != null ? d.DocFolder.DataKeyValue : 0
                //                            }).FirstOrDefaultAsync();

                //    if (document == null) continue;

                //    var docFile = await _docService.DocFiles.AsNoTracking().Where(d => d.FileId == document.FileId).Select(d => new { d.UserFileName, d.DriveItemId }).FirstOrDefaultAsync();
                //    if (docFile != null && !string.IsNullOrEmpty(docFile.UserFileName))
                //    {
                //        var attachments = new List<WorkflowEmailAttachmentViewModel>() {
                //            //Use actual filename for locating the file (Use FileId, DocFileName could be Null)
                //            //new WorkflowEmailAttachmentViewModel { DocId = doc.DocId, FileId = doc.FileId, FileName = userFileName, DocParent = docFolder.DataKeyValue }
                //            new WorkflowEmailAttachmentViewModel { 
                //                DocId = document.DocId, 
                //                FileId = document.FileId, 
                //                FileName = $"{document.FileId}{Path.GetExtension(docFile.UserFileName)}", 
                //                DocParent = document.DataKeyValue,
                //                Id = settings.IsSharePointIntegrationOn ? docFile.DriveItemId : null
                //            }
                //        };

                //        var documentLink = $"{document.SystemType}|{document.ScreenCode}|{document.DataKey}|{document.DataKeyValue.ToString()}";
                //        var workflowHeader = await GenerateWorkflow(documentLink, attachments, false, hasNewRespDocketing, hasRespDocketingReassigned, hasNewRespReporting, hasRespReportingReassigned);
                //        var emailWorkflowList = GenerateEmailWorkflow(workflowHeader, attachments, document.DataKeyValue);
                //        if (emailWorkflowList.Any())
                //            emailWorkflows.AddRange(emailWorkflowList);
                //    }                    
                //}
            }

            return emailWorkflows;
        }

        private async Task<List<WorkflowEmailViewModel>> UpdateDeDocketResponsible(List<SystemDataKey> keyIds, List<string>? newUserList, List<int>? newGroupList, List<string>? oldUserList, List<int>? oldGroupList)
        {
            var settings = await _defaultSettings.GetSetting();
            var emailWorkflows = new List<WorkflowEmailViewModel>();
            var userName = User.GetUserName();

            var deDocketIds = keyIds.Where(d => !string.IsNullOrEmpty(d.DataKey) && d.DataKey.ToLower() == "dedocketid").ToList();
            foreach (var keyItem in deDocketIds)
            {
                var hasNewDeDocketResp = false;
                var hasDeDocketRespReassigned = false;
                int deDocketId = keyItem.DataKeyValue;
                var currentDeDocketResps = new List<DueDateDeDocketResp>();

                switch (keyItem.SystemType!.ToUpper())
                {
                    case SystemTypeCode.Patent:
                        currentDeDocketResps = await _patDueDateService.DueDateDeDocketResps.AsNoTracking()
                        .Where(d => d.DeDocketId == deDocketId).Select(d => new DueDateDeDocketResp() { RespId = d.RespId, DeDocketId = d.DeDocketId, UserId = d.UserId, GroupId = d.GroupId }).ToListAsync();
                        break;
                    case SystemTypeCode.Trademark:
                        currentDeDocketResps = await _patDueDateService.DueDateDeDocketResps.AsNoTracking()
                        .Where(d => d.DeDocketId == deDocketId).Select(d => new DueDateDeDocketResp() { RespId = d.RespId, DeDocketId = d.DeDocketId, UserId = d.UserId, GroupId = d.GroupId }).ToListAsync();
                        break;
                    case SystemTypeCode.GeneralMatter:
                        currentDeDocketResps = await _patDueDateService.DueDateDeDocketResps.AsNoTracking()
                        .Where(d => d.DeDocketId == deDocketId).Select(d => new DueDateDeDocketResp() { RespId = d.RespId, DeDocketId = d.DeDocketId, UserId = d.UserId, GroupId = d.GroupId }).ToListAsync();
                        break;
                    default:
                        break;
                }

                //Compare old and new Resp Docketing, and update
                //Only process if there is something in new UserIds or GroupIds for Resp Docketing
                if ((newGroupList != null && newGroupList.Count > 0) || (newUserList != null && newUserList.Count > 0))
                {

                    var currentDocketingUserList = currentDeDocketResps.Where(d => !string.IsNullOrEmpty(d.UserId) && (d.GroupId == null || d.GroupId == 0)).Select(d => d.UserId ?? "").ToList();
                    var currentDocketingGroupList = currentDeDocketResps.Where(d => d.GroupId > 0 && string.IsNullOrEmpty(d.UserId)).Select(d => d.GroupId ?? 0).ToList();

                    //Check Users
                    if (oldUserList != null && oldUserList.Any(d => currentDocketingUserList.Contains(d)))
                    {
                        hasDeDocketRespReassigned = true;
                        currentDocketingUserList.RemoveAll(d => oldUserList.Contains(d));
                    }

                    if (newUserList != null && newUserList.Any(d => !currentDocketingUserList.Contains(d)))
                    {
                        hasNewDeDocketResp = true;
                        currentDocketingUserList.AddRange(newUserList.Where(d => !currentDocketingUserList.Contains(d)).ToList());
                    }

                    //Check Groups
                    if (oldGroupList != null && oldGroupList.Any(d => currentDocketingGroupList.Contains(d)))
                    {
                        hasDeDocketRespReassigned = true;
                        currentDocketingGroupList.RemoveAll(d => oldGroupList.Contains(d));
                    }

                    if (newGroupList != null && newGroupList.Any(d => !currentDocketingGroupList.Contains(d)))
                    {
                        hasNewDeDocketResp = true;
                        currentDocketingGroupList.AddRange(newGroupList.Where(d => !currentDocketingGroupList.Contains(d)).ToList());
                    }

                    //Update
                    if (hasNewDeDocketResp || hasDeDocketRespReassigned)
                    {
                        var updatedRespDocketingList = currentDocketingUserList.Union(currentDocketingGroupList.Select(d => d.ToString())).Distinct().ToList();

                        switch (keyItem.SystemType.ToUpper())
                        {
                            case SystemTypeCode.Patent:
                                await _patDueDateService.UpdateDeDocketResp(updatedRespDocketingList, userName, deDocketId);
                                break;
                            case SystemTypeCode.Trademark:
                                await _tmkDueDateService.UpdateDeDocketResp(updatedRespDocketingList, userName, deDocketId);
                                break;
                            // GM module removed
                            // case SystemTypeCode.GeneralMatter:
                            //     await _gmDueDateService.UpdateDeDocketResp(updatedRespDocketingList, userName, deDocketId);
                            //     break;
                            default:
                                break;
                        }
                    }

                }

                ////Only process workflow is updating 1 document               
                ////Updating multiple documents could result in a lot of workflow emails to send out
                //if (keyIds.Count == 1 && (hasNewRespDocketing || hasRespDocketingReassigned || hasNewRespReporting || hasRespReportingReassigned))
                //{
                //    //Prepare workflows
                //    var document = await _docService.DocDocuments.AsNoTracking().Where(d => d.DocId == docId)
                //                            .Select(d => new {
                //                                d.DocId,
                //                                d.FileId,
                //                                SystemType = d.DocFolder != null ? d.DocFolder.SystemType : "",
                //                                ScreenCode = d.DocFolder != null ? d.DocFolder.ScreenCode : "",
                //                                DataKey = d.DocFolder != null ? d.DocFolder.DataKey : "",
                //                                DataKeyValue = d.DocFolder != null ? d.DocFolder.DataKeyValue : 0
                //                            }).FirstOrDefaultAsync();

                //    if (document == null) continue;

                //    var docFile = await _docService.DocFiles.AsNoTracking().Where(d => d.FileId == document.FileId).Select(d => new { d.UserFileName, d.DriveItemId }).FirstOrDefaultAsync();
                //    if (docFile != null && !string.IsNullOrEmpty(docFile.UserFileName))
                //    {
                //        var attachments = new List<WorkflowEmailAttachmentViewModel>() {
                //            //Use actual filename for locating the file (Use FileId, DocFileName could be Null)
                //            //new WorkflowEmailAttachmentViewModel { DocId = doc.DocId, FileId = doc.FileId, FileName = userFileName, DocParent = docFolder.DataKeyValue }
                //            new WorkflowEmailAttachmentViewModel { 
                //                DocId = document.DocId, 
                //                FileId = document.FileId, 
                //                FileName = $"{document.FileId}{Path.GetExtension(docFile.UserFileName)}", 
                //                DocParent = document.DataKeyValue,
                //                Id = settings.IsSharePointIntegrationOn ? docFile.DriveItemId : null
                //            }
                //        };

                //        var documentLink = $"{document.SystemType}|{document.ScreenCode}|{document.DataKey}|{document.DataKeyValue.ToString()}";
                //        var workflowHeader = await GenerateWorkflow(documentLink, attachments, false, hasNewRespDocketing, hasRespDocketingReassigned, hasNewRespReporting, hasRespReportingReassigned);
                //        var emailWorkflowList = GenerateEmailWorkflow(workflowHeader, attachments, document.DataKeyValue);
                //        if (emailWorkflowList.Any())
                //            emailWorkflows.AddRange(emailWorkflowList);
                //    }                    
                //}
            }

            return emailWorkflows;
        }

        #region Workflow
        private async Task<WorkflowHeaderViewModel> GenerateWorkflow(string documentLink, List<WorkflowEmailAttachmentViewModel> attachments, bool isNewFileUpload = false, bool hasNewRespDocketing = false, bool hasRespDocketingReassigned = false, bool hasNewRespReporting = false, bool hasRespReportingReassigned = false)
        {
            var documentLinkArray = documentLink.Split("|");
            var systemTypeCode = documentLinkArray[0];
            var screenCode = documentLinkArray[1];
            var parentId = Convert.ToInt32(documentLinkArray[3]);

            switch (screenCode)
            {
                case ScreenCode.Application:
                    return await GenerateCountryAppWorkflow(attachments, isNewFileUpload, hasNewRespDocketing, hasRespDocketingReassigned, hasNewRespReporting, hasRespReportingReassigned);

                case ScreenCode.Trademark:
                    return await GenerateTrademarkWorkflow(attachments, isNewFileUpload, hasNewRespDocketing, hasRespDocketingReassigned, hasNewRespReporting, hasRespReportingReassigned);

                case ScreenCode.GeneralMatter:
                    return await GenerateGMWorkflow(attachments, isNewFileUpload, hasNewRespDocketing, hasRespDocketingReassigned, hasNewRespReporting, hasRespReportingReassigned);

                    //case ScreenCode.Action:
                    //    return await GenerateActionWorkflow(systemTypeCode, attachments);
            }
            return new WorkflowHeaderViewModel();
        }

        private List<WorkflowEmailViewModel> GenerateEmailWorkflow(WorkflowHeaderViewModel workflowHeader, List<WorkflowEmailAttachmentViewModel> attachments, int parentId)
        {
            if (workflowHeader != null && workflowHeader.Workflows != null)
            {
                //All System workflow settings have SendMail at the top
                var emailWorkflows = workflowHeader.Workflows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
                if (emailWorkflows.Any())
                {
                    var wfs = new List<WorkflowEmailViewModel>();
                    foreach (var wf in emailWorkflows)
                    {
                        if (wf.Attachments != null && wf.Attachments.Any())
                        {
                            foreach (var attachment in wf.Attachments)
                            {
                                wfs.Add(new WorkflowEmailViewModel
                                {
                                    isAutoEmail = !wf.Preview,
                                    qeSetupId = wf.ActionValueId,
                                    autoAttachImages = wf.AutoAttachImages,
                                    id = attachment.DocId,
                                    fileNames = new string[] { (attachment.FileName ?? "") }, //actual filename when using blob storage
                                    parentId = parentId,
                                    emailUrl = string.IsNullOrEmpty(wf.EmailUrl) ? workflowHeader.EmailUrl : wf.EmailUrl,
                                    strId = attachment.Id, //SharePoint
                                    emailTo = string.IsNullOrEmpty(wf.EmailTo) ? null : wf.EmailTo,
                                    attachmentFilter = wf.AttachmentFilter
                                });

                            }
                            ;
                        }
                    }
                    if (wfs.Any())
                        return wfs;
                }
            }
            return new List<WorkflowEmailViewModel>();
        }

        private async Task<WorkflowHeaderViewModel> GenerateCountryAppWorkflow(List<WorkflowEmailAttachmentViewModel> attachments, bool isNewFileUpload = false, bool hasNewRespDocketing = false, bool hasRespDocketingReassigned = false, bool hasNewRespReporting = false, bool hasRespReportingReassigned = false)
        {
            var settings = await _patSettings.GetSetting();
            if (settings.IsWorkflowOn)
            {
                var emailUrl = Url.Action("Email", "PatImageApp", new { area = "Patent" });
                var parentId = attachments.First().DocParent;

                var newDocRespDocketingUrl = Url.Action("EmailDocRespDocketing", "PatImageApp", new { area = "Patent" });
                var reassignedDocRespDocketingUrl = Url.Action("EmailReassignedDocRespDocketing", "PatImageApp", new { area = "Patent" });

                var newDocRespReportingUrl = Url.Action("EmailDocRespReporting", "PatImageApp", new { area = "Patent" });
                var reassignedDocRespReportingUrl = Url.Action("EmailReassignedDocRespReporting", "PatImageApp", new { area = "Patent" });

                var workflows = await _docViewModelService.GenerateCountryAppWorkflow(attachments, parentId, isNewFileUpload, hasNewRespDocketing, hasRespDocketingReassigned, newDocRespDocketingUrl ?? "", reassignedDocRespDocketingUrl ?? "", false, hasNewRespReporting, hasRespReportingReassigned, newDocRespReportingUrl ?? "", reassignedDocRespReportingUrl ?? "");
                return new WorkflowHeaderViewModel { Id = parentId, EmailUrl = emailUrl, Workflows = workflows };
            }
            return new WorkflowHeaderViewModel();
        }

        private async Task<WorkflowHeaderViewModel> GenerateTrademarkWorkflow(List<WorkflowEmailAttachmentViewModel> attachments, bool isNewFileUpload = false, bool hasNewRespDocketing = false, bool hasRespDocketingReassigned = false, bool hasNewRespReporting = false, bool hasRespReportingReassigned = false)
        {
            var settings = await _tmkSettings.GetSetting();
            if (settings.IsWorkflowOn)
            {
                var emailUrl = Url.Action("Email", "TmkImage", new { area = "Trademark" });
                var parentId = attachments.First().DocParent;

                var newDocRespDocketingUrl = Url.Action("EmailDocRespDocketing", "TmkImage", new { area = "Trademark" });
                var reassignedDocRespDocketingUrl = Url.Action("EmailReassignedDocRespDocketing", "TmkImage", new { area = "Trademark" });

                var newDocRespReportingUrl = Url.Action("EmailDocRespReporting", "TmkImage", new { area = "Trademark" });
                var reassignedDocRespReportingUrl = Url.Action("EmailReassignedDocRespReporting", "TmkImage", new { area = "Trademark" });

                var workflows = await _docViewModelService.GenerateTrademarkWorkflow(attachments, parentId, isNewFileUpload, hasNewRespDocketing, hasRespDocketingReassigned, newDocRespDocketingUrl ?? "", reassignedDocRespDocketingUrl ?? "", hasNewRespReporting, hasRespReportingReassigned, newDocRespReportingUrl ?? "", reassignedDocRespReportingUrl ?? "");
                return new WorkflowHeaderViewModel { Id = parentId, EmailUrl = emailUrl, Workflows = workflows };
            }
            return new WorkflowHeaderViewModel();
        }

        private Task<WorkflowHeaderViewModel> GenerateGMWorkflow(List<WorkflowEmailAttachmentViewModel> attachments, bool isNewFileUpload = false, bool hasNewRespDocketing = false, bool hasRespDocketingReassigned = false, bool hasNewRespReporting = false, bool hasRespReportingReassigned = false)
        {
            // GeneralMatter module removed
            return Task.FromResult(new WorkflowHeaderViewModel());
        }
        #endregion

        #endregion

        //1st tab - unverified (IsVerified=0) documents (link or not link to a case); the user has to "verify" the document to drop-off from the tab or delete the record from the tab
        #region New Documents tab        

        public async Task<IActionResult> NewDocGrid_Read([DataSourceRequest] DataSourceRequest request, DocumentVerificationSearchCriteriaViewModel searchCriteria)
        {
            if (ModelState.IsValid)
            {
                var result = await _docVerificationDTOService.GetDocVerificationNewDocs(searchCriteria);
                if (result != null && result.Count > 0)
                {                    
                    foreach (var item in result)
                    {
                        item.DocLibrary = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(item.SystemType ?? "");
                        item.CanUploadDocument = item.ParentId == 0 || string.IsNullOrEmpty(item.SystemType) ? true : await CanUploadDocument(item.SystemType ?? "", item.RespOffice ?? "");
                    }
                }
                var list = result.ToDataSourceResult(request);
                return Json(list);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpGet()]
        public async Task<IActionResult> SearchLink(string ids, string docNames)
        {
            var systemTypes = new List<QuickDocketSystemTypeViewModel>();
            if ((await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded)
                systemTypes.Add(new QuickDocketSystemTypeViewModel { SystemName = SystemType.Patent, SystemId = SystemType.Patent, TypeId = SystemTypeCode.Patent });

            if ((await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded)
                systemTypes.Add(new QuickDocketSystemTypeViewModel { SystemName = SystemType.Trademark, SystemId = SystemType.Trademark, TypeId = SystemTypeCode.Trademark });

            if ((await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded)
                systemTypes.Add(new QuickDocketSystemTypeViewModel { SystemName = "General Matter", SystemId = SystemType.GeneralMatter, TypeId = SystemTypeCode.GeneralMatter });

            ViewData["SystemTypes"] = systemTypes;
            ViewData["DocumentNames"] = docNames;

            return PartialView("_SearchLinkRecord", ids);
        }

        private async Task<List<QuickDocketSystemTypeViewModel>> GetAvailableSystemTypes()
        {
            var systems = User.GetSystemTypes();
            var qdSystems = new List<QuickDocketSystemTypeViewModel>();

            if (systems.Any(s => s.SystemId == SystemType.Patent) && (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanUploadDocuments)).Succeeded)
                qdSystems.Add(new QuickDocketSystemTypeViewModel { SystemName = SystemType.Patent, SystemId = SystemType.Patent, TypeId = SystemTypeCode.Patent });

            if (systems.Any(s => s.SystemId == SystemType.Trademark) && (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanUploadDocuments)).Succeeded)
                qdSystems.Add(new QuickDocketSystemTypeViewModel { SystemName = SystemType.Trademark, SystemId = SystemType.Trademark, TypeId = SystemTypeCode.Trademark });

            if (systems.Any(s => s.SystemId == SystemType.GeneralMatter) && (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanUploadDocuments)).Succeeded)
                qdSystems.Add(new QuickDocketSystemTypeViewModel { SystemName = "General Matter", SystemId = SystemType.GeneralMatter, TypeId = SystemTypeCode.GeneralMatter });

            return qdSystems;
        }

        public async Task<IActionResult> SearchLinkRead([DataSourceRequest] DataSourceRequest request, string searchTerm, string systemIds)
        {
            //using global search for searching records to link to documents
            if (string.IsNullOrEmpty(searchTerm) || string.IsNullOrEmpty(systemIds)) return Ok();

            var systemList = systemIds.Split("|").ToList();

            var objParam = new GSParamDTO()
            {
                SearchMode = "c",
                SystemScreens = "",
                BasicSearchTerm = "",
                MoreFilters = new List<GSMoreFilter>(),
                DataFilters = new List<GSDataFilterBase>() { new GSDataFilterBase() { FieldId = 0, OrderEntry = 0, Criteria = "", LogicalOperator = "", LeftParen = "", RightParen = "" } },
                DocFilters = new List<GSDocFilterBase>()
            };

            var searchFieldSettings = await GetSearchSettings("DocVerificationSearch");
            var savedFieldIds = new List<int>();
            if (!string.IsNullOrEmpty(searchFieldSettings))
                savedFieldIds = JsonConvert.DeserializeObject<List<int>>(searchFieldSettings);

            var searchFields = await _repository.DocVerificationSearchFields.AsNoTracking()
                                        .Where(d => systemList.Contains(d.GSField.GSScreen.SystemType)
                                            && (savedFieldIds == null || savedFieldIds.Count == 0 || savedFieldIds.Contains(d.KeyId))
                                        )
                                        .Select(d => new GSDataFilterBase()
                                        {
                                            FieldId = d.FieldId,
                                            OrderEntry = d.EntryOrder,
                                            Criteria = searchTerm,
                                            LogicalOperator = "",
                                            LeftParen = "",
                                            RightParen = ""
                                        }).ToListAsync();

            if (!searchFields.Any())
                throw new Exception(_localizer["Please select field(s) to search."]);

            objParam.DataFilters = searchFields;

            var result = (await _globalSearchService.RunGlobalSearchDB(User.GetEmail(), User.HasRespOfficeFilter(), User.HasEntityFilter(), objParam))
                            .Select(d => new DocVerificationSearchLinkViewModel()
                            {
                                Link = d.Link,
                                SystemName = d.SystemName,
                                ScreenName = d.ScreenName,
                                FieldValues = d.FieldValues,
                                IsActRequired = false,
                                RespDocketing = null,
                                RespReporting = null
                            });
            var list = result.ToDataSourceResult(request);
            return Json(list);
        }

        [HttpGet()]
        public async Task<IActionResult> SearchSettings(string systemTypes)
        {
            var customOrder = new List<string>() { "P", "T", "G " };
            var searchFields = await _repository.DocVerificationSearchFields.AsNoTracking()
                                        .Where(d => string.IsNullOrEmpty(systemTypes) || systemTypes.Contains(d.GSField.GSScreen.SystemType))
                                        .Select(d => new DocVerificationSearchFieldViewModel()
                                        {
                                            KeyId = d.KeyId,
                                            EntryOrder = d.EntryOrder,
                                            System = d.GSField.GSScreen.SystemType,
                                            FieldLabel = d.FieldLabel,
                                            IsEnabled = d.IsEnabled
                                        })
                                        .ToListAsync();

            var viewModel = searchFields.OrderBy(o => customOrder.IndexOf(o.System ?? "")).ToList();

            var searchSettings = await GetSearchSettings("DocVerificationSearch");

            if (!string.IsNullOrEmpty(searchSettings))
            {
                var savedSettings = JsonConvert.DeserializeObject<List<int>>(searchSettings);

                if (savedSettings != null && savedSettings.Count > 0)
                {
                    var unselectedFields = viewModel.Where(d => !savedSettings.Contains(d.KeyId)).ToList();
                    unselectedFields.ForEach(d => { d.IsEnabled = false; });
                }
            }

            return PartialView("_SearchSettings", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditSearchSettings(List<int> keyIds)
        {
            var settings = JsonConvert.SerializeObject(keyIds);
            if (!string.IsNullOrEmpty(settings))
            {
                var user = await _userManager.GetUserAsync(User);
                var setting = await GetSettingByNameAsync("DocVerificationSearch");

                if (setting != null && user != null)
                {
                    var userSetting = await GetUserSettingsAsync(user.Id, setting.Id);
                    if (userSetting != null)
                    {
                        userSetting.Settings = settings;
                        await UpdateUserSettingsAsync(userSetting);
                    }
                    else
                    {
                        userSetting = new CPiUserSetting();
                        userSetting.UserId = user.Id;
                        userSetting.SettingId = setting.Id;
                        userSetting.Settings = settings;
                        await AddUserSettingsAsync(userSetting);
                    }
                }
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult PrintNewDoc([FromBody] PrintViewModel newDocPrintViewModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(newDocPrintViewModel, ReportType.SharedDocVerificationNewDocPrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        [HttpPost]
        public async Task<IActionResult> ExportToExcelNewDoc([FromBody] string ids)
        {
            if (ModelState.IsValid)
            {
                var result = await _docVerificationDTOService.GetDocVerificationNewDocById(ids);
                var reportFields = result.Select(d => _mapper.Map<DocVerificationNewPrintViewModel>(d)).ToList();
                var fileStream = await _exportHelper.ListToExcelMemoryStream(reportFields, "List", _localizer, true, "", null, 50, false, null);
                return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), "DocumentsForReviewList.xlsx");
            }

            return BadRequest("Unhandled error.");
        }

        [HttpGet()]
        public async Task<IActionResult> ReviewFilters()
        {
            if (!User.IsAdmin())
                return BadRequest(_localizer["Access denied."].ToString());

            var reviewFilters = await _systemSettingManager.GetSystemSetting<DocVerificationReviewFilters>();

            var viewModel = new DocVerificationReviewFilterViewModel();

            if (reviewFilters != null)
            {
                viewModel = _mapper.Map<DocVerificationReviewFilterViewModel>(reviewFilters);

                if (!string.IsNullOrEmpty(reviewFilters.CountryFilter))
                    viewModel.Countries = reviewFilters.CountryFilter.Split("|").ToArray();

                if (!string.IsNullOrEmpty(reviewFilters.CaseTypeFilter))
                    viewModel.CaseTypes = reviewFilters.CaseTypeFilter.Split("|").ToArray();

                if (!string.IsNullOrEmpty(reviewFilters.ClientFilter))
                    viewModel.Clients = reviewFilters.ClientFilter.Split("|").ToArray();
            }

            return PartialView("_ReviewFilters", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveReviewFilters([FromBody] DocVerificationReviewFilterViewModel viewModel)
        {
            try
            {
                var cpiSetting = await _systemSettingManager.GetCPiSetting("DocVerificationReviewFilters");
                Guard.Against.NoRecordPermission(cpiSetting != null);

                if (!User.IsAdmin())
                    return BadRequest(_localizer["Access denied."].ToString());

                var reviewSettings = await _systemSettingManager.QueryableList.Where(d => d.SettingId == cpiSetting!.Id).FirstOrDefaultAsync();

                if (reviewSettings == null)
                {
                    reviewSettings = new CPiSystemSetting()
                    {
                        Id = 0,
                        SystemId = "",
                        SettingId = cpiSetting!.Id
                    };
                }

                var settings = _mapper.Map<DocVerificationReviewFilters>(viewModel);
                reviewSettings.Settings = JsonConvert.SerializeObject(settings);

                if (reviewSettings.Id > 0)
                    await _systemSettingManager.Update(reviewSettings);
                else
                    await _systemSettingManager.Add(reviewSettings);

                return Json(new { success = _localizer["Criteria have been saved successfully."].ToString() });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        //2nd tab - all documents linked to a case, verified (IsVerified=1), and action required checked (IsActRequired=1). The record doesn't have an action due record, only linked with action type
        //optional: tbl_DocketRequest
        #region Documents tab        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel()
            {
                Page = PageType.SearchResults,
                PageId = "documentVerificationSearchResults",
                Title = _localizer["Document Verification"].ToString(),
                GridPageSize = 8
            };
            return PartialView("Index", model);
        }

        public async Task<IActionResult> DocVerification_Read([DataSourceRequest] DataSourceRequest request, DocumentVerificationSearchCriteriaViewModel searchCriteria)
        {
            if (ModelState.IsValid)
            {
                var settings = await _defaultSettings.GetSetting();
                var list = new DataSourceResult();

                var data = await _docVerificationDTOService.GetDocVerificationDocuments(searchCriteria);
                if (data != null && data.Count > 0)
                {
                    var ids = new List<string>();

                    foreach (var item in data)
                    {
                        item.DocLibrary = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(item.System ?? "");
                        
                        if (string.IsNullOrEmpty(item.KeyId)) continue;

                        // verifyid - DocVerification, reqid - DocketRequest, dedocketid - DeDocket
                        if (item.KeyId.ToLower().StartsWith("verifyid"))                         
                        {
                            ids.Add(string.Format("{0}:{1}:DocId|{2}", item.System, item.ParentId, item.DocId));
                            item.CanUploadDocument = await CanUploadDocument(item.System ?? "", item.RespOffice ?? "");
                        }
                        else
                        {
                            ids.Add(string.Format("{0}:{1}:{2}", item.System, item.ParentId, item.KeyId));
                        }

                        if (item.KeyId.ToLower().StartsWith("reqid"))
                        {
                            item.CanViewRemarks = await CanViewRemarks(item.System ?? "", item.RespOffice ?? "");
                            item.CanViewDocketRequest = settings.IsDocketRequestOn && await CanAccessDocketRequest(item.System ?? "");
                        }

                        if (item.KeyId.ToLower().StartsWith("dedocketid"))
                        {
                            item.CanViewRemarks = await CanViewRemarks(item.System ?? "", item.RespOffice ?? "");
                            item.CanViewInstruction = settings.IsDeDocketOn && item.CanViewRemarks;
                            item.CanCompleteInstruction = settings.IsDeDocketOn && await CanCompleteInstruction(item.System ?? "", item.RespOffice ?? "");
                        }
                    }

                    var dataModel = data.ToDataSourceResult(request);
                    return Json(new
                    {
                        Data = dataModel.Data,
                        Total = dataModel.Total,
                        AggregateResult = dataModel.AggregateResults,
                        Errors = dataModel.Errors,
                        Ids = ids.Distinct().ToArray()
                    });

                }
                list = data.ToDataSourceResult(request);

                return Json(list);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<IActionResult> DeleteLinkedAction(List<string> keyIds)
        {
            if (keyIds == null || keyIds.Count <= 0)
                return BadRequest(_localizer["Missing Id."]);

            var keyList = keyIds.Select(d =>
            {
                var keyArr = d.Split("|");
                string systemType = "";
                string dataKey = "";
                int dataKeyValue = 0;
                bool hasKey = false;
                if (keyArr.Length > 0)
                {
                    systemType = keyArr[0];
                    dataKey = keyArr[1];
                    if (int.TryParse(keyArr[2], out dataKeyValue))
                    {
                        hasKey = true;
                    }
                }
                return new { hasKey, systemType, dataKey, dataKeyValue };
            }).Where(d => d.hasKey).ToList();

            //tblDocVerification
            var verifyIds = keyList.Where(d => d.dataKey.ToLower() == "verifyid").Select(d => d.dataKeyValue).ToList();
            if (verifyIds != null && verifyIds.Count > 0)
            {
                var deleteList = await _docService.DocVerifications.Where(d => verifyIds.Contains(d.VerifyId)).ToListAsync();
                if (deleteList.Any())
                {
                    var userName = User.GetUserName();
                    await _docService.UpdateDocVerifications(0, userName, new List<DocVerification>(), new List<DocVerification>(), deleteList);

                    var docIds = deleteList.Where(d => d.DocId > 0).Select(d => d.DocId).Distinct().ToList();
                    if (docIds != null && docIds.Count > 0)
                    {
                        var docDocuments = await _docService.DocDocuments.Where(d => docIds.Contains(d.DocId) && d.IsActRequired == true && (d.DocVerifications == null || !d.DocVerifications.Any())).ToListAsync();
                        if (docDocuments.Any())
                        {
                            foreach (var document in docDocuments)
                            {
                                document.IsActRequired = false;
                                document.LastUpdate = DateTime.Now;
                                document.UpdatedBy = userName;
                            }
                            await _docService.UpdateDocuments(userName, docDocuments, new List<DocDocument>(), new List<DocDocument>());
                        }
                    }
                }
            }

            //tbl_DocketRequest
            var patDocketRequests = keyList.Where(d => d.systemType.ToLower() == SystemTypeCode.Patent.ToLower() && d.dataKey.ToLower() == "reqid").ToList();
            if (patDocketRequests.Any())
            {
                await _docketRequestService.DeletePatDocketRequests(patDocketRequests.Select(d => d.dataKeyValue).Distinct().ToList());
            }
            var tmkDocketRequests = keyList.Where(d => d.systemType.ToLower() == SystemTypeCode.Trademark.ToLower() && d.dataKey.ToLower() == "reqid").ToList();
            if (tmkDocketRequests.Any())
            {
                await _docketRequestService.DeleteTmkDocketRequests(tmkDocketRequests.Select(d => d.dataKeyValue).Distinct().ToList());
            }
            var gmDocketRequests = keyList.Where(d => d.systemType.ToLower() == SystemTypeCode.GeneralMatter.ToLower() && d.dataKey.ToLower() == "reqid").ToList();
            if (gmDocketRequests.Any())
            {
                await _docketRequestService.DeleteGMDocketRequests(gmDocketRequests.Select(d => d.dataKeyValue).Distinct().ToList());
            }

            return Ok(new { success = _localizer["Record(s) has been deleted successfully."].ToString() });
        }

        [HttpPost]
        public IActionResult PrintDoc([FromBody] PrintViewModel docPrintViewModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(docPrintViewModel, ReportType.SharedDocVerificationDocPrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        [HttpPost]
        public async Task<IActionResult> ExportToExcelDoc([FromBody] string ids)
        {
            if (ModelState.IsValid)
            {
                var result = await _docVerificationDTOService.GetDocVerificationDocById(ids);
                var reportFields = result.Select(d => _mapper.Map<DocVerificationDocPrintViewModel>(d)).ToList();
                var fileStream = await _exportHelper.ListToExcelMemoryStream(reportFields, "List", _localizer, true, "", null, 50, false, null);
                return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), "DocketingRequests.xlsx");
            }

            return BadRequest("Unhandled error.");
        }

        [HttpGet()]
        public IActionResult ProductivityReports()
        {
            return PartialView("_ProductivityReports");
        }

        public async Task<IActionResult> ViewDocketRequestDocument(string systemTypeCode, string docFileName, int parentId, string keyId)
        {
            //View document from DocketRequest or DeDocket
            //DocVerification will use regular process

            if (ImageHelper.IsUrl(docFileName))
                return Redirect(docFileName);

            var userName = User.GetUserName();
            var docLibrary = string.Empty;
            var systemType = string.Empty;
            switch (systemTypeCode)
            {
                case SystemTypeCode.Patent:
                    docLibrary = SharePointDocLibrary.Patent;
                    systemType = SystemType.Patent;
                    break;
                case SystemTypeCode.Trademark:
                    docLibrary = SharePointDocLibrary.Trademark;
                    systemType = SystemType.Trademark;
                    break;
                case SystemTypeCode.GeneralMatter:
                    docLibrary = SharePointDocLibrary.GeneralMatter;
                    systemType = SystemType.GeneralMatter;
                    break;
                default:
                    break;
            }

            Guard.Against.NoRecordPermission(!string.IsNullOrEmpty(docFileName));

            var settings = await _defaultSettings.GetSetting();
            var tempFilePath = string.Empty;

            if (settings.DocumentStorage == DocumentStorageOptions.SharePoint || settings.DocumentStorage == DocumentStorageOptions.iManage || settings.DocumentStorage == DocumentStorageOptions.NetDocuments)
            {
                tempFilePath = await PrepareTemporaryFile(docFileName, docLibrary);
            }
            else
            {
                var tempFileName = (docFileName ?? "").ReplaceInvalidFilenameChars();
                tempFilePath = GetTemporaryFilePath(tempFileName);
                if (!System.IO.File.Exists(tempFilePath))
                {
                    var file = await _documentStorage.GetFileStream(systemType, docFileName ?? "", CPiSavedFileType.DocMgt);
                    if (file != null)
                    {
                        using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                        {
                            await file.Stream.CopyToAsync(fileStream);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(tempFilePath))
            {
                return RedirectToAction("ZoomTempFile", "DocViewer", new { fileName = tempFilePath });
            }

            return BadRequest();
        }

        public async Task<IActionResult> RequestZoom(string systemTypeCode, int parentId, string keyId)
        {
            if (string.IsNullOrEmpty(keyId))
                return BadRequest();

            var dataKey = "";
            var dataKeyValue = 0;
            var keyArr = keyId.Split("|");
            if (keyArr.Length > 0 && int.TryParse(keyArr[1], out dataKeyValue))
                dataKey = keyArr[0];

            if (string.IsNullOrEmpty(dataKey) || dataKeyValue <= 0 || (dataKey.ToLower() != "reqid" && dataKey.ToLower() != "dedocketid" && dataKey.ToLower() != "docid"))
                return BadRequest();

            var userName = User.GetUserName();
            DocVerificationRequestDocZoomViewModel? requestDocZoomVM = null;
            var docLibrary = string.Empty;
            var systemType = string.Empty;
            switch (systemTypeCode)
            {
                case SystemTypeCode.Patent:
                    if (dataKey.ToLower() == "reqid")
                    {
                        requestDocZoomVM = await _docketRequestService.PatDocketRequests.AsNoTracking().Where(d => d.ReqId == dataKeyValue && d.AppId == parentId && d.CountryApplication != null)
                                            .Select(d => new DocVerificationRequestDocZoomViewModel()
                                            {
                                                DataKey = "ReqId",
                                                DataKeyValue = d.ReqId,
                                                ParentId = d.AppId,
                                                InvId = d.CountryApplication!.InvId,
                                                CaseNumber = d.CountryApplication.CaseNumber,
                                                Country = d.CountryApplication.Country,
                                                SubCase = d.CountryApplication.SubCase,
                                                CaseType = d.CountryApplication.CaseType,
                                                Status = d.CountryApplication.ApplicationStatus,
                                                AppNumber = d.CountryApplication.AppNumber,
                                                FilDate = d.CountryApplication.FilDate,
                                                RespOffice = d.CountryApplication.RespOffice,

                                                DateCreated = d.DateCreated,
                                                DueDate = d.DueDate,
                                                RequestType = d.RequestType,
                                                CompletedBy = d.CompletedBy,
                                                CompletedDate = d.CompletedDate,
                                                CreatedBy = d.CreatedBy,

                                                System = SystemType.Patent,
                                                ScreenCode = ScreenCode.Application,
                                                DocFileName = "",
                                                FileType = CPiSavedFileType.DocMgt,
                                                Remarks = d.Remarks,
                                                FileId = d.FileId,
                                                DocFile = d.DocFile,
                                                DriveItemId = d.DriveItemId
                                            }).FirstOrDefaultAsync();
                        if (requestDocZoomVM != null)
                            requestDocZoomVM.CanSave = await CanAccessDocketRequest(SystemTypeCode.Patent);
                    }
                    else if (dataKey.ToLower() == "dedocketid")
                    {
                        requestDocZoomVM = await _repository.PatDueDateDeDockets.AsNoTracking()
                                            .Where(d => d.DeDocketId == dataKeyValue
                                                && d.PatDueDate != null && d.PatDueDate.PatActionDue != null && d.PatDueDate.PatActionDue.CountryApplication != null
                                                && d.PatDueDate.PatActionDue.CountryApplication.AppId == parentId
                                            )
                                            .Select(d => new DocVerificationRequestDocZoomViewModel()
                                            {
                                                DataKey = "DeDocketId",
                                                DataKeyValue = d.DeDocketId,
                                                ParentId = d.PatDueDate!.PatActionDue!.CountryApplication!.AppId,
                                                InvId = d.PatDueDate.PatActionDue.CountryApplication.InvId,
                                                CaseNumber = d.PatDueDate.PatActionDue.CountryApplication.CaseNumber,
                                                Country = d.PatDueDate.PatActionDue.CountryApplication.Country,
                                                SubCase = d.PatDueDate.PatActionDue.CountryApplication.SubCase,
                                                CaseType = d.PatDueDate.PatActionDue.CountryApplication.CaseType,
                                                Status = d.PatDueDate.PatActionDue.CountryApplication.ApplicationStatus,
                                                AppNumber = d.PatDueDate.PatActionDue.CountryApplication.AppNumber,
                                                FilDate = d.PatDueDate.PatActionDue.CountryApplication.FilDate,
                                                RespOffice = d.PatDueDate.PatActionDue.CountryApplication.RespOffice,

                                                ActId = d.PatDueDate.PatActionDue.ActId,
                                                ActionType = d.PatDueDate.PatActionDue.ActionType,
                                                BaseDate = d.PatDueDate.PatActionDue.BaseDate,
                                                ActionDue = d.PatDueDate.ActionDue,
                                                DueDate = d.PatDueDate.DueDate,
                                                Indicator = d.PatDueDate.Indicator,
                                                Instruction = d.Instruction,
                                                InstructedBy = d.InstructedBy,
                                                InstructionDate = d.InstructionDate,
                                                InstructionCompleted = d.InstructionCompleted,
                                                CompletedBy = d.CompletedBy,
                                                CompletedDate = d.CompletedDate,

                                                System = SystemType.Patent,
                                                ScreenCode = ScreenCode.Application,
                                                DocFileName = "",
                                                FileType = CPiSavedFileType.DocMgt,
                                                Remarks = d.Remarks,
                                                FileId = d.FileId,
                                                DocFile = d.DocFile,
                                                DriveItemId = d.DriveItemId
                                            }).FirstOrDefaultAsync();
                        if (requestDocZoomVM != null)
                            requestDocZoomVM.CanSave = requestDocZoomVM.InstructionCompleted == false && await CanCompleteInstruction(SystemTypeCode.Patent, requestDocZoomVM.RespOffice ?? "");
                    }
                    else if (dataKey.ToLower() == "docid")
                    {
                        var docDocument = await _docService.DocDocuments.AsNoTracking()
                            .Where(d => d.DocId == dataKeyValue
                                && d.DocFolder != null && !string.IsNullOrEmpty(d.DocFolder.SystemType) && d.DocFolder.SystemType.ToLower() == SystemTypeCode.Patent.ToLower()
                                && !string.IsNullOrEmpty(d.DocFolder.ScreenCode) && d.DocFolder.ScreenCode.ToLower() == ScreenCode.Application.ToLower()
                                && !string.IsNullOrEmpty(d.DocFolder.DataKey) && d.DocFolder.DataKey.ToLower() == "appid"
                            ).Include(d => d.DocFolder).Include(d => d.DocFile).FirstOrDefaultAsync();

                        if (docDocument != null && docDocument.DocFolder != null && docDocument.DocFile != null)
                        {
                            requestDocZoomVM = await _countryApplicationService.CountryApplications.AsNoTracking().Where(d => d.AppId == docDocument.DocFolder.DataKeyValue)
                                .Select(d => new DocVerificationRequestDocZoomViewModel()
                                {
                                    DataKey = "DocId",
                                    DataKeyValue = dataKeyValue,
                                    ParentId = d.AppId,
                                    InvId = d.InvId,
                                    CaseNumber = d.CaseNumber,
                                    Country = d.Country,
                                    SubCase = d.SubCase,
                                    CaseType = d.CaseType,
                                    Status = d.ApplicationStatus,
                                    AppNumber = d.AppNumber,
                                    FilDate = d.FilDate,
                                    RespOffice = d.RespOffice,
                                    DocName = docDocument.DocName,
                                    System = SystemType.Patent,
                                    ScreenCode = ScreenCode.Application,
                                    DocFileName = "",
                                    FileType = CPiSavedFileType.DocMgt,
                                    Remarks = d.Remarks,
                                    FileId = docDocument.DocFile.FileId,
                                    DocFile = docDocument.DocFile.DocFileName,
                                    DriveItemId = docDocument.DocFile.DriveItemId
                                }).FirstOrDefaultAsync();
                        }

                        if (requestDocZoomVM != null)
                            requestDocZoomVM.CanSave = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.DocumentVerificationModify)).Succeeded;
                    }

                    docLibrary = SharePointDocLibrary.Patent;
                    systemType = SystemType.Patent;

                    break;
                case SystemTypeCode.Trademark:
                    if (dataKey.ToLower() == "reqid")
                    {
                        requestDocZoomVM = await _docketRequestService.TmkDocketRequests.AsNoTracking().Where(d => d.ReqId == dataKeyValue && d.TmkId == parentId && d.TmkTrademark != null)
                                            .Select(d => new DocVerificationRequestDocZoomViewModel()
                                            {
                                                DataKey = "ReqId",
                                                DataKeyValue = d.ReqId,
                                                ParentId = d.TmkId,
                                                CaseNumber = d.TmkTrademark!.CaseNumber,
                                                Country = d.TmkTrademark.Country,
                                                SubCase = d.TmkTrademark.SubCase,
                                                CaseType = d.TmkTrademark.CaseType,
                                                Status = d.TmkTrademark.TrademarkStatus,
                                                AppNumber = d.TmkTrademark.AppNumber,
                                                FilDate = d.TmkTrademark.FilDate,
                                                DateCreated = d.DateCreated,
                                                DueDate = d.DueDate,
                                                RequestType = d.RequestType,
                                                CompletedBy = d.CompletedBy,
                                                CompletedDate = d.CompletedDate,
                                                CreatedBy = d.CreatedBy,
                                                System = SystemType.Trademark,
                                                ScreenCode = ScreenCode.Trademark,
                                                DocFileName = "",
                                                FileType = CPiSavedFileType.DocMgt,
                                                Remarks = d.Remarks,
                                                FileId = d.FileId,
                                                DocFile = d.DocFile,
                                                DriveItemId = d.DriveItemId
                                            }).FirstOrDefaultAsync();
                        if (requestDocZoomVM != null)
                            requestDocZoomVM.CanSave = await CanAccessDocketRequest(SystemTypeCode.Trademark);
                    }
                    else if (dataKey.ToLower() == "dedocketid")
                    {
                        requestDocZoomVM = await _repository.TmkDueDateDeDockets.AsNoTracking()
                                            .Where(d => d.DeDocketId == dataKeyValue
                                                && d.TmkDueDate != null && d.TmkDueDate.TmkActionDue != null && d.TmkDueDate.TmkActionDue.TmkTrademark != null
                                                && d.TmkDueDate.TmkActionDue.TmkTrademark.TmkId == parentId
                                            )
                                            .Select(d => new DocVerificationRequestDocZoomViewModel()
                                            {
                                                DataKey = "DeDocketId",
                                                DataKeyValue = d.DeDocketId,
                                                ParentId = d.TmkDueDate!.TmkActionDue!.TmkTrademark!.TmkId,
                                                CaseNumber = d.TmkDueDate.TmkActionDue.TmkTrademark.CaseNumber,
                                                Country = d.TmkDueDate.TmkActionDue.TmkTrademark.Country,
                                                SubCase = d.TmkDueDate.TmkActionDue.TmkTrademark.SubCase,
                                                CaseType = d.TmkDueDate.TmkActionDue.TmkTrademark.CaseType,
                                                Status = d.TmkDueDate.TmkActionDue.TmkTrademark.TrademarkStatus,
                                                AppNumber = d.TmkDueDate.TmkActionDue.TmkTrademark.AppNumber,
                                                FilDate = d.TmkDueDate.TmkActionDue.TmkTrademark.FilDate,
                                                RespOffice = d.TmkDueDate.TmkActionDue.TmkTrademark.RespOffice,

                                                ActId = d.TmkDueDate.TmkActionDue.ActId,
                                                ActionType = d.TmkDueDate.TmkActionDue.ActionType,
                                                BaseDate = d.TmkDueDate.TmkActionDue.BaseDate,
                                                ActionDue = d.TmkDueDate.ActionDue,
                                                DueDate = d.TmkDueDate.DueDate,
                                                Indicator = d.TmkDueDate.Indicator,
                                                Instruction = d.Instruction,
                                                InstructedBy = d.InstructedBy,
                                                InstructionDate = d.InstructionDate,
                                                InstructionCompleted = d.InstructionCompleted,
                                                CompletedBy = d.CompletedBy,
                                                CompletedDate = d.CompletedDate,

                                                System = SystemType.Trademark,
                                                ScreenCode = ScreenCode.Trademark,
                                                DocFileName = "",
                                                FileType = CPiSavedFileType.DocMgt,
                                                Remarks = d.Remarks,
                                                FileId = d.FileId,
                                                DocFile = d.DocFile,
                                                DriveItemId = d.DriveItemId
                                            }).FirstOrDefaultAsync();
                        if (requestDocZoomVM != null)
                            requestDocZoomVM.CanSave = requestDocZoomVM.InstructionCompleted == false && await CanCompleteInstruction(SystemTypeCode.Trademark, requestDocZoomVM.RespOffice ?? "");
                    }
                    else if (dataKey.ToLower() == "docid")
                    {
                        var docDocument = await _docService.DocDocuments.AsNoTracking()
                            .Where(d => d.DocId == dataKeyValue
                                && d.DocFolder != null && !string.IsNullOrEmpty(d.DocFolder.SystemType) && d.DocFolder.SystemType.ToLower() == SystemTypeCode.Trademark.ToLower()
                                && !string.IsNullOrEmpty(d.DocFolder.ScreenCode) && d.DocFolder.ScreenCode.ToLower() == ScreenCode.Trademark.ToLower()
                                && !string.IsNullOrEmpty(d.DocFolder.DataKey) && d.DocFolder.DataKey.ToLower() == "tmkid"
                            ).Include(d => d.DocFolder).Include(d => d.DocFile).FirstOrDefaultAsync();

                        if (docDocument != null && docDocument.DocFolder != null && docDocument.DocFile != null)
                        {
                            requestDocZoomVM = await _tmkTrademarkService.TmkTrademarks.AsNoTracking().Where(d => d.TmkId == docDocument.DocFolder.DataKeyValue)
                                .Select(d => new DocVerificationRequestDocZoomViewModel()
                                {
                                    DataKey = "DocId",
                                    DataKeyValue = dataKeyValue,
                                    ParentId = d.TmkId,
                                    CaseNumber = d.CaseNumber,
                                    Country = d.Country,
                                    SubCase = d.SubCase,
                                    CaseType = d.CaseType,
                                    Status = d.TrademarkStatus,
                                    AppNumber = d.AppNumber,
                                    FilDate = d.FilDate,
                                    RespOffice = d.RespOffice,
                                    DocName = docDocument.DocName,
                                    System = SystemType.Trademark,
                                    ScreenCode = ScreenCode.Trademark,
                                    DocFileName = "",
                                    FileType = CPiSavedFileType.DocMgt,
                                    Remarks = d.Remarks,
                                    FileId = docDocument.DocFile.FileId,
                                    DocFile = docDocument.DocFile.DocFileName,
                                    DriveItemId = docDocument.DocFile.DriveItemId
                                }).FirstOrDefaultAsync();
                        }

                        if (requestDocZoomVM != null)
                            requestDocZoomVM.CanSave = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.DocumentVerificationModify)).Succeeded;
                    }

                    docLibrary = SharePointDocLibrary.Trademark;
                    systemType = SystemType.Trademark;

                    break;
                // GM module removed - GeneralMatter case disabled
                // case SystemTypeCode.GeneralMatter:
                //     (entire GeneralMatter document verification block removed)
                //     break;
                default:
                    break;
            }

            Guard.Against.NoRecordPermission(requestDocZoomVM != null);

            var settings = await _defaultSettings.GetSetting();

            //Get file to display
            if (requestDocZoomVM != null)
            {
                var docFile = await _docService.DocFiles.AsNoTracking().Where(d => d.FileId == requestDocZoomVM.FileId).FirstOrDefaultAsync();
                var docFileName = string.Empty;

                if (docFile != null)
                {
                    docFileName = docFile.UserFileName;

                    if (settings.DocumentStorage == DocumentStorageOptions.SharePoint || settings.DocumentStorage == DocumentStorageOptions.iManage || settings.DocumentStorage == DocumentStorageOptions.NetDocuments)
                    {
                        var tempFilePath = await PrepareTemporaryFile(requestDocZoomVM.DriveItemId!, docLibrary);
                        if (!string.IsNullOrEmpty(tempFilePath))
                        {
                            ViewBag.FromTemp = true;
                            requestDocZoomVM.DocFileName = tempFilePath;
                            requestDocZoomVM.DocName = docFile != null ? docFile.UserFileName : "";
                        }
                    }
                    else
                    {
                        if (dataKey.ToLower() == "docid")
                        {
                            requestDocZoomVM.DocFileName = _documentStorage.GetFilePath(systemType, (requestDocZoomVM.DocFile ?? "") ?? (docFile.UserFileName ?? ""), CPiSavedFileType.DocMgt);
                        }
                        else
                        {
                            var tempFileName = docFile.FileId.ToString() + "_" + (docFile.UserFileName ?? "").ReplaceInvalidFilenameChars();
                            var tempFilePath = GetTemporaryFilePath(tempFileName);
                            if (!System.IO.File.Exists(tempFilePath))
                            {
                                var file = await _documentStorage.GetFileStream(systemType, requestDocZoomVM.DocFile ?? (docFile.DocFileName ?? ""), CPiSavedFileType.DocMgt);
                                if (file != null)
                                {
                                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                                    {
                                        await file.Stream.CopyToAsync(fileStream);
                                    }
                                }
                            }
                            requestDocZoomVM.DocName = docFile != null ? docFile.UserFileName : "";
                            ViewBag.FromTemp = true;
                            requestDocZoomVM.DocFileName = tempFilePath;
                        }
                    }
                }
            }

            return PartialView("_RequestZoom", requestDocZoomVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRequestsAsCompleted(DateTime? completedDate, string keyIds)
        {
            var defaultSettings = await _defaultSettings.GetSetting();
            var userName = User.GetUserName();
            var temp = keyIds.Split(";").Where(d => !string.IsNullOrEmpty(d)).ToList();

            var keyList = temp.Select(d =>
            {
                var keyArr = d.Split("|");
                string systemType = string.Empty;
                string dataKey = string.Empty;
                int dataKeyValue = 0;
                if (keyArr != null && keyArr.Length > 0 && int.TryParse(keyArr[2], out dataKeyValue))
                {
                    systemType = keyArr[0];
                    dataKey = keyArr[1];
                }
                return new { systemType, dataKey, dataKeyValue };
            }).Where(d => d.dataKeyValue > 0).ToList();

            if (string.IsNullOrEmpty(keyIds) || keyList == null || keyList.Count <= 0)
                return BadRequest();

            //DocketRequest
            var patReqIds = keyList.Where(d => d.systemType.ToLower() == SystemTypeCode.Patent.ToLower() && d.dataKey.ToLower() == "reqid").Select(d => d.dataKeyValue).ToList();
            if (patReqIds.Any()) await _docketRequestService.MarkPatDocketRequestsAsCompleted(patReqIds, completedDate);

            var tmkReqIds = keyList.Where(d => d.systemType.ToLower() == SystemTypeCode.Trademark.ToLower() && d.dataKey.ToLower() == "reqid").Select(d => d.dataKeyValue).ToList();
            if (tmkReqIds.Any()) await _docketRequestService.MarkTmkDocketRequestsAsCompleted(tmkReqIds, completedDate);

            var gmReqIds = keyList.Where(d => d.systemType.ToLower() == SystemTypeCode.GeneralMatter.ToLower() && d.dataKey.ToLower() == "reqid").Select(d => d.dataKeyValue).ToList();
            if (gmReqIds.Any()) await _docketRequestService.MarkGMDocketRequestsAsCompleted(gmReqIds, completedDate);

            //DeDocket
            if (defaultSettings.IsDeDocketOn && defaultSettings.IncludeDeDocketInVerification)
            {
                var patDeDocketIds = keyList.Where(d => d.systemType.ToLower() == SystemTypeCode.Patent.ToLower() && d.dataKey.ToLower() == "dedocketid").Select(d => d.dataKeyValue).ToList();
                if (patDeDocketIds.Any()) await _patDueDateService.MarkDeDocketInstructionsAsCompleted(patDeDocketIds, completedDate);

                var tmkDeDocketIds = keyList.Where(d => d.systemType.ToLower() == SystemTypeCode.Trademark.ToLower() && d.dataKey.ToLower() == "dedocketid").Select(d => d.dataKeyValue).ToList();
                if (tmkDeDocketIds.Any()) await _tmkDueDateService.MarkDeDocketInstructionsAsCompleted(tmkDeDocketIds, completedDate);

                // GM module removed
                // var gmDeDocketIds = keyList.Where(d => d.systemType.ToLower() == SystemTypeCode.GeneralMatter.ToLower() && d.dataKey.ToLower() == "dedocketid").Select(d => d.dataKeyValue).ToList();
                // if (gmDeDocketIds.Any()) await _gmDueDateService.MarkDeDocketInstructionsAsCompleted(gmDeDocketIds, completedDate);
            }

            return Ok(new { message = _localizer["Completed Date has been saved successfully."].ToString(), userName });
        }

        #region Widgets       

        [HttpPost]
        public async Task<PartialViewResult> GetWidget(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            var widgetSettings = await GetSettingByNameAsync("DocVerificationWidgetSettings");
            var settings = string.Empty;

            if (widgetSettings != null && user != null)
            {
                var userSetting = await GetUserSettingsAsync(user.Id, widgetSettings.Id);
                if (userSetting != null)
                {
                    var existingSettings = JsonConvert.DeserializeObject<List<DocVerificationWidgetInfo>>(userSetting.Settings);
                    if (existingSettings != null)
                        settings = existingSettings.FirstOrDefault(d => !string.IsNullOrEmpty(d.WidgetId) && d.WidgetId.ToLower() == id.ToLower())?.Settings ?? string.Empty;
                }
            }

            return PartialView("_" + id, settings);
        }

        [HttpPost]
        public async Task<JsonResult> SaveWidgetSetting(string id, string settings)
        {
            var user = await _userManager.GetUserAsync(User);
            var userWidgetSettings = await GetSettingByNameAsync("DocVerificationWidgetSettings");

            try
            {
                if (userWidgetSettings != null && user != null)
                {
                    var userSetting = await GetUserSettingsAsync(user.Id, userWidgetSettings.Id);
                    if (userSetting != null)
                    {
                        //Parse setting
                        var existingSettings = JsonConvert.DeserializeObject<List<DocVerificationWidgetInfo>>(userSetting.Settings);
                        if (existingSettings != null)
                        {
                            var widget = existingSettings.FirstOrDefault(d => !string.IsNullOrEmpty(d.WidgetId) && d.WidgetId.ToLower() == id.ToLower());
                            if (widget != null)
                            {
                                JObject widgetSettings = new JObject();
                                if (!string.IsNullOrEmpty(widget.Settings))
                                {
                                    widgetSettings = JObject.Parse(widget.Settings);
                                }
                                JObject newSettings = JObject.Parse(settings);

                                foreach (var token in newSettings)
                                {
                                    widgetSettings[token.Key] = token.Value;
                                }

                                widget.Settings = JsonConvert.SerializeObject(widgetSettings);
                            }
                            else
                            {
                                existingSettings.Add(new DocVerificationWidgetInfo() { WidgetId = id.ToLower(), Settings = settings });
                            }

                            userSetting.Settings = JsonConvert.SerializeObject(existingSettings);
                        }
                        else
                        {
                            userSetting.Settings = JsonConvert.SerializeObject(new List<DocVerificationWidgetInfo>() { new DocVerificationWidgetInfo()
                            {
                                WidgetId = id.ToLower(), Settings = settings
                            } });
                        }

                        await UpdateUserSettingsAsync(userSetting);
                    }
                    else
                    {
                        userSetting = new CPiUserSetting();
                        userSetting.UserId = user.Id;
                        userSetting.SettingId = userWidgetSettings.Id;
                        userSetting.Settings = JsonConvert.SerializeObject(new List<DocVerificationWidgetInfo>()
                        { new DocVerificationWidgetInfo()
                            {
                                WidgetId = id.ToLower(), Settings = settings
                            }
                        });
                        await AddUserSettingsAsync(userSetting);
                    }
                }
                return Json(new { success = true, message = "Widget successfully updated." });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Unable to update widget." });
            }
        }

        /// <summary>
        /// To show the distribution of the total tasks among users or groups
        /// DocVerification: verified documents either has "Docket Required? (IsActRequired) checked or have record(s) in tblDocVerification and must have at least 1 docketing responsible
        /// DocketRequest: tbl_DocketRequest and tbl_DocketRequestResp
        /// DeDocket: tbl_DueDateDeDocket and tbl_DueDateDeDocketResp
        /// Docketing responsible is used as the category on the widget - for grouping/counting
        /// Show assigned users, users under assigned groups, and assigned groups
        /// Not showing groups linked to assigned users
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetTaskDistributionChart([DataSourceRequest] DataSourceRequest request)
        {
            var defaultSettings = await _defaultSettings.GetSetting();
            var canAccessPatDocVer = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;
            var canAccessTmkDocVer = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;
            var canAccessGmDocVer = (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;

            var data = new List<ChartDTO>();

            bool showPatent = true;
            bool showTrademark = true;
            bool showGenMatter = true;
            //****************************************************************************
            //Raw data sources
            //DocVerification
            //DataSource: document is verified (IsVerified), required dockets (Docket Required? - IsActRequired) or have records in tblDocVerification
            var docResponsibleDocketings = await _docService.DocRespDocketings.AsNoTracking()
                .Where(d => (d.DocId > 0 && d.DocDocument != null && d.DocDocument.IsVerified && d.DocDocument.IsActRequired
                            && (d.DocDocument.DocVerifications == null || !d.DocDocument.DocVerifications.Any() || d.DocDocument.DocVerifications.Any()))).ToListAsync();

            //DocketRequest
            var docketRequestResps = await _docketRequestService.PatDocketRequestResps.AsNoTracking()
                                        .Select(d => new { SystemType = SystemTypeCode.Patent, d.ReqId, d.UserId, d.GroupId, d.LastUpdate })
                                        .Union(_docketRequestService.TmkDocketRequestResps.AsNoTracking()
                                        .Select(d => new { SystemType = SystemTypeCode.Trademark, d.ReqId, d.UserId, d.GroupId, d.LastUpdate }))
                                        .Union(_docketRequestService.GMDocketRequestResps.AsNoTracking()
                                        .Select(d => new { SystemType = SystemTypeCode.GeneralMatter, d.ReqId, d.UserId, d.GroupId, d.LastUpdate }))
                                        .ToListAsync();

            //DeDocket
            var deDocketResps = await _patDueDateService.DueDateDeDocketResps.AsNoTracking()
                                        .Select(d => new { SystemType = SystemTypeCode.Patent, d.DeDocketId, d.UserId, d.GroupId, d.LastUpdate })
                                        .Union(_tmkDueDateService.DueDateDeDocketResps.AsNoTracking()
                                        .Select(d => new { SystemType = SystemTypeCode.Trademark, d.DeDocketId, d.UserId, d.GroupId, d.LastUpdate }))
                                        // GM module removed
                                        // .Union(_gmDueDateService.DueDateDeDocketResps.AsNoTracking()
                                        // .Select(d => new { SystemType = SystemTypeCode.GeneralMatter, d.DeDocketId, d.UserId, d.GroupId, d.LastUpdate }))
                                        .ToListAsync();
            if (defaultSettings.IncludeDeDocketInVerification == false)
                deDocketResps.Clear();
            //****************************************************************************

            //****************************************************************************
            //Apply filters
            //DocVerification are from CountryApplication/Trademark/GeneralMatter
            var settings = await GetWidgetSettings("taskdistribution");
            if (!string.IsNullOrEmpty(settings))
            {
                //Prepare filters - start
                showPatent = settings.Contains("ShowPatent") ? GetWidgetSetting<bool>("ShowPatent", settings) : true;
                showTrademark = settings.Contains("ShowTrademark") ? GetWidgetSetting<bool>("ShowTrademark", settings) : true;
                showGenMatter = settings.Contains("ShowGenMatter") ? GetWidgetSetting<bool>("ShowGenMatter", settings) : true;

                if (!canAccessPatDocVer) showPatent = false;
                if (!canAccessTmkDocVer) showTrademark = false;
                if (!canAccessGmDocVer) showGenMatter = false;

                string assignedDateFromStr = GetWidgetSetting<string>("CreatedDateFrom", settings);
                DateTime? assignedDateFrom = !string.IsNullOrEmpty(assignedDateFromStr) ? DateTime.ParseExact(assignedDateFromStr, "MM/dd/yyyy", CultureInfo.InvariantCulture) : null;
                string assignedDateToStr = GetWidgetSetting<string>("AssignedDateTo", settings);
                DateTime? assignedDateTo = !string.IsNullOrEmpty(assignedDateToStr) ? DateTime.ParseExact(assignedDateToStr, "MM/dd/yyyy", CultureInfo.InvariantCulture) : null;

                var respDocketings = GetWidgetSetting<JArray>("Responsibles", settings);
                var respDocketingFilter = respDocketings == null ? new List<string>() : respDocketings.Select(a => (string)a.ToString() ?? "").ToList();

                var userIdFilter = new List<string>();
                var groupIdFilter = new List<int>();

                foreach (var respDocketing in respDocketingFilter.Where(d => !string.IsNullOrEmpty(d)).ToList())
                {
                    var itemArr = respDocketing.Split("|");
                    var intVal = 0;
                    if (int.TryParse(itemArr[1], out intVal))
                        groupIdFilter.Add(intVal);
                    else
                        userIdFilter.Add(itemArr[1]);
                }
                //Prepare filters - end                

                //DocVerification
                //Filter on Documents
                var docIds = docResponsibleDocketings.Where(d => d.DocId > 0).Select(d => d.DocId).Distinct().ToList();
                var filteredDocIds = await _docService.DocDocuments.AsNoTracking().Where(d => docIds.Contains(d.DocId) && d.DocFolder != null)
                    .Select(d => new
                    {
                        d.DocId,
                        Include = d.DocFolder!.SystemType == SystemTypeCode.Patent && d.DocFolder.ScreenCode == ScreenCode.Application && d.DocFolder.DataKey == "AppId"
                                    ? _countryApplicationService.CountryApplications.Any(c => c.AppId == d.DocFolder.DataKeyValue) && showPatent
                                    : d.DocFolder!.SystemType == SystemTypeCode.Trademark && d.DocFolder.ScreenCode == ScreenCode.Trademark && d.DocFolder.DataKey == "TmkId"
                                    ? _tmkTrademarkService.TmkTrademarks.Any(t => t.TmkId == d.DocFolder.DataKeyValue) && showTrademark
                                    : d.DocFolder!.SystemType == SystemTypeCode.GeneralMatter && d.DocFolder.ScreenCode == ScreenCode.GeneralMatter && d.DocFolder.DataKey == "MatId"
                                    ? false // GM module removed: _gmMatterService.QueryableList.Any(g => g.MatId == d.DocFolder.DataKeyValue) && showGenMatter
                                    : false
                    }).Distinct().ToListAsync();
                //Filter on RespDocketings
                docResponsibleDocketings = docResponsibleDocketings
                    .Where(d => (filteredDocIds == null || !filteredDocIds.Any() || filteredDocIds.Any(f => f.Include && f.DocId == d.DocId))
                        && (!userIdFilter.Any() || (!string.IsNullOrEmpty(d.UserId) && userIdFilter.Contains(d.UserId)))
                        && (!groupIdFilter.Any() || groupIdFilter.Contains(d.GroupId ?? 0))
                        && (assignedDateFrom == null || (d.LastUpdate != null && d.LastUpdate.Value.Date >= assignedDateFrom.Value.Date))
                        && (assignedDateTo == null || (d.LastUpdate != null && d.LastUpdate.Value.Date <= assignedDateTo.Value.Date))
                    ).ToList();

                //DocketRequest
                if (!showPatent)
                    docketRequestResps.RemoveAll(d => d.SystemType == SystemTypeCode.Patent);
                if (!showTrademark)
                    docketRequestResps.RemoveAll(d => d.SystemType == SystemTypeCode.Trademark);
                if (!showGenMatter)
                    docketRequestResps.RemoveAll(d => d.SystemType == SystemTypeCode.GeneralMatter);

                docketRequestResps = docketRequestResps
                    .Where(d => (!userIdFilter.Any() || (!string.IsNullOrEmpty(d.UserId) && userIdFilter.Contains(d.UserId)))
                        && (!groupIdFilter.Any() || groupIdFilter.Contains(d.GroupId ?? 0))
                        && (assignedDateFrom == null || (d.LastUpdate != null && d.LastUpdate.Value.Date >= assignedDateFrom.Value.Date))
                        && (assignedDateTo == null || (d.LastUpdate != null && d.LastUpdate.Value.Date <= assignedDateTo.Value.Date))
                    ).ToList();

                //DeDocket
                if (!showPatent)
                    deDocketResps.RemoveAll(d => d.SystemType == SystemTypeCode.Patent);
                if (!showTrademark)
                    deDocketResps.RemoveAll(d => d.SystemType == SystemTypeCode.Trademark);
                if (!showGenMatter)
                    deDocketResps.RemoveAll(d => d.SystemType == SystemTypeCode.GeneralMatter);

                deDocketResps = deDocketResps
                    .Where(d => (!userIdFilter.Any() || (!string.IsNullOrEmpty(d.UserId) && userIdFilter.Contains(d.UserId)))
                        && (!groupIdFilter.Any() || groupIdFilter.Contains(d.GroupId ?? 0))
                        && (assignedDateFrom == null || (d.LastUpdate != null && d.LastUpdate.Value.Date >= assignedDateFrom.Value.Date))
                        && (assignedDateTo == null || (d.LastUpdate != null && d.LastUpdate.Value.Date <= assignedDateTo.Value.Date))
                    )
                    .ToList();
            }
            else
            {
                //Check if module is enabled for each system when widget setting is default (empty)
                if (!canAccessPatDocVer) showPatent = false;
                if (!canAccessTmkDocVer) showTrademark = false;
                if (!canAccessGmDocVer) showGenMatter = false;

                var docIds = docResponsibleDocketings.Where(d => d.DocId > 0).Select(d => d.DocId).Distinct().ToList();
                var filteredDocIds = await _docService.DocDocuments.AsNoTracking().Where(d => docIds.Contains(d.DocId) && d.DocFolder != null)
                    .Select(d => new
                    {
                        d.DocId,
                        Include = d.DocFolder!.SystemType == SystemTypeCode.Patent && d.DocFolder.ScreenCode == ScreenCode.Application && d.DocFolder.DataKey == "AppId"
                                    ? _countryApplicationService.CountryApplications.Any(c => c.AppId == d.DocFolder.DataKeyValue) && showPatent
                                    : d.DocFolder!.SystemType == SystemTypeCode.Trademark && d.DocFolder.ScreenCode == ScreenCode.Trademark && d.DocFolder.DataKey == "TmkId"
                                    ? _tmkTrademarkService.TmkTrademarks.Any(t => t.TmkId == d.DocFolder.DataKeyValue) && showTrademark
                                    : d.DocFolder!.SystemType == SystemTypeCode.GeneralMatter && d.DocFolder.ScreenCode == ScreenCode.GeneralMatter && d.DocFolder.DataKey == "MatId"
                                    ? false // GM module removed: _gmMatterService.QueryableList.Any(g => g.MatId == d.DocFolder.DataKeyValue) && showGenMatter
                                    : false
                    }).Distinct().ToListAsync();
                //Filter on RespDocketings
                docResponsibleDocketings = docResponsibleDocketings.Where(d => filteredDocIds == null || !filteredDocIds.Any() || filteredDocIds.Any(f => f.Include && f.DocId == d.DocId)).ToList();

                //DocketRequest
                if (!showPatent)
                    docketRequestResps.RemoveAll(d => d.SystemType == SystemTypeCode.Patent);
                if (!showTrademark)
                    docketRequestResps.RemoveAll(d => d.SystemType == SystemTypeCode.Trademark);
                if (!showGenMatter)
                    docketRequestResps.RemoveAll(d => d.SystemType == SystemTypeCode.GeneralMatter);

                //DeDocket
                if (!showPatent)
                    deDocketResps.RemoveAll(d => d.SystemType == SystemTypeCode.Patent);
                if (!showTrademark)
                    deDocketResps.RemoveAll(d => d.SystemType == SystemTypeCode.Trademark);
                if (!showGenMatter)
                    deDocketResps.RemoveAll(d => d.SystemType == SystemTypeCode.GeneralMatter);
            }
            //****************************************************************************

            if ((docResponsibleDocketings == null || docResponsibleDocketings.Count <= 0)
                && (docketRequestResps == null || docketRequestResps.Count <= 0)
                && (deDocketResps == null || deDocketResps.Count <= 0))
                return Json(data);

            //Get list of users and groups for each data type
            var docUserList = new List<(string UserId, int DocId)>();
            var docGroupList = new List<(int GroupId, int DocId)>();
            var docketRequestUserList = new List<(string SystemType, string UserId, int ReqId)>();
            var docketRequestGroupList = new List<(string SystemType, int GroupId, int ReqId)>();
            var deDocketUserList = new List<(string SystemType, string UserId, int DeDocketId)>();
            var deDocketGroupList = new List<(string SystemType, int GroupId, int DeDocketId)>();

            if (docResponsibleDocketings != null && docResponsibleDocketings.Count > 0)
            {
                docUserList = docResponsibleDocketings.Where(d => !string.IsNullOrEmpty(d.UserId) && d.DocId > 0).Select(d => (d.UserId ?? "", d.DocId)).Distinct().ToList();
                docGroupList = docResponsibleDocketings.Where(d => d.GroupId > 0 && d.DocId > 0).Select(d => (d.GroupId ?? 0, d.DocId)).Distinct().ToList();
            }

            if (docketRequestResps != null && docketRequestResps.Count > 0)
            {
                docketRequestUserList = docketRequestResps.Where(d => !string.IsNullOrEmpty(d.UserId) && d.ReqId > 0).Select(d => (d.SystemType, d.UserId ?? "", d.ReqId)).Distinct().ToList();
                docketRequestGroupList = docketRequestResps.Where(d => d.GroupId > 0 && d.ReqId > 0).Select(d => (d.SystemType, d.GroupId ?? 0, d.ReqId)).Distinct().ToList();
            }

            if (deDocketResps != null && deDocketResps.Count > 0)
            {
                deDocketUserList = deDocketResps.Where(d => !string.IsNullOrEmpty(d.UserId) && d.DeDocketId > 0).Select(d => (d.SystemType, d.UserId ?? "", d.DeDocketId)).Distinct().ToList();
                deDocketGroupList = deDocketResps.Where(d => d.GroupId > 0 && d.DeDocketId > 0).Select(d => (d.SystemType, d.GroupId ?? 0, d.DeDocketId)).Distinct().ToList();
            }

            //Get distinct groupId to get group names
            var groupIdList = docGroupList.Select(d => d.GroupId)
                .Union(docketRequestGroupList.Select(d => d.GroupId))
                .Union(deDocketGroupList.Select(d => d.GroupId))
                .Distinct().ToList();

            //Get all userIds under all distinct groupIds for all data type
            var groupUserList = await _groupManager.CPiUserGroups.AsNoTracking().Where(d => groupIdList.Contains(d.GroupId)).Select(d => new { d.GroupId, d.UserId }).ToListAsync();
            //Get group names for all data type
            var groupList = await _groupManager.QueryableList.AsNoTracking().Where(d => groupIdList.Contains(d.Id)).Select(d => new { GroupId = d.Id, Name = d.Name }).ToListAsync();

            //Link userIds under groupIds back to each data type
            var docGroupUserList = docGroupList.Join(groupUserList,
                                                        grp => grp.GroupId,
                                                        grpUsr => grpUsr.GroupId,
                                                        (grp, grpUsr) => new { grp.DocId, grp.GroupId, grpUsr.UserId })
                                                .DistinctBy(d => new { d.DocId, d.GroupId, d.UserId }).ToList();

            var docketRequestGroupUserList = docketRequestGroupList.Join(groupUserList,
                                                        grp => grp.GroupId,
                                                        grpUsr => grpUsr.GroupId,
                                                        (grp, grpUsr) => new { grp.SystemType, grp.ReqId, grp.GroupId, grpUsr.UserId })
                                                .DistinctBy(d => new { d.SystemType, d.ReqId, d.GroupId, d.UserId }).ToList();

            var deDocketGroupUserList = deDocketGroupList.Join(groupUserList,
                                                        grp => grp.GroupId,
                                                        grpUsr => grpUsr.GroupId,
                                                        (grp, grpUsr) => new { grp.SystemType, grp.DeDocketId, grp.GroupId, grpUsr.UserId })
                                                .DistinctBy(d => new { d.SystemType, d.DeDocketId, d.GroupId, d.UserId }).ToList();

            //Get distinct userId to get user names for all data type
            var userIdList = docUserList.Select(d => d.UserId)
                .Union(docGroupUserList.Select(dg => dg.UserId))
                .Union(docketRequestUserList.Select(dg => dg.UserId))
                .Union(docketRequestGroupUserList.Select(dg => dg.UserId))
                .Union(deDocketUserList.Select(dg => dg.UserId))
                .Union(deDocketGroupUserList.Select(dg => dg.UserId))
                .Distinct().ToList();
            //Get user names for all data type
            var userList = await _userManager.Users.AsNoTracking().Where(d => userIdList.Contains(d.Id)).Select(d => new { UserId = d.Id, Name = d.FirstName + " " + d.LastName }).ToListAsync();

            //Get distinct userId and key id from individual and from groups for each data type
            var userDocIdList = docUserList.Select(d => new { UserId = d.UserId ?? "", DocId = d.DocId })
                .Union(docGroupUserList.Select(d => new { UserId = d.UserId ?? "", DocId = d.DocId }))
                .DistinctBy(d => new { d.UserId, d.DocId }).ToList();

            var userReqIdList = docketRequestUserList.Select(d => new { d.SystemType, d.UserId, d.ReqId })
                .Union(docketRequestGroupUserList.Select(d => new { d.SystemType, d.UserId, d.ReqId }))
                .DistinctBy(d => new { d.SystemType, d.UserId, d.ReqId }).ToList();

            var userDeDocketIdList = deDocketUserList.Select(d => new { d.SystemType, d.UserId, d.DeDocketId })
                .Union(deDocketGroupUserList.Select(d => new { d.SystemType, d.UserId, d.DeDocketId }))
                .DistinctBy(d => new { d.SystemType, d.UserId, d.DeDocketId }).ToList();

            //****************************************************************************
            //Get task count for DocVerification - START
            //Get unique DocIds to get task count for each DocId
            var uniqueDocIdList = userDocIdList.Select(d => d.DocId).Union(docGroupList.Select(d => d.DocId)).Distinct().ToList();
            //Get task count for DocIds
            var docTaskList = await _docService.DocDocuments.AsNoTracking()
                .Where(d => uniqueDocIdList.Contains(d.DocId) && d.IsVerified && d.IsActRequired && (d.DocVerifications == null || !d.DocVerifications.Any() || d.DocVerifications.Any()))
                .Select(d => new
                {
                    d.DocId,
                    TaskCount = d.DocVerifications == null || !d.DocVerifications.Any() ? 1 : d.DocVerifications.Count()
                })
                .ToListAsync();
            //Get count for user (individually and under groups) for Docs            
            var docUserCount = userDocIdList.Join(docTaskList,
                                                    u => u.DocId,
                                                    d => d.DocId,
                                                    (u, d) => new { u.UserId, d.TaskCount })
                .GroupBy(grp => grp.UserId).Select(d => new { UserId = d.Key, TaskCount = d.Sum(s => s.TaskCount) }).ToList();
            //Get group counts for Docs            
            var docGroupCount = docGroupList.Join(docTaskList,
                                                g => g.DocId,
                                                d => d.DocId,
                                                (g, d) => new { g.GroupId, d.TaskCount })
                .GroupBy(grp => grp.GroupId).Select(d => new { GroupId = d.Key, TaskCount = d.Sum(s => s.TaskCount) }).ToList();
            //Get total assigned tasks count
            var totalAssignedDocTask = docTaskList.Sum(d => d.TaskCount);
            //Get task count for DocVerification - END            

            //Get task count for Docket Request
            var docketRequestUserCount = userReqIdList.GroupBy(grp => grp.UserId).Select(d => new { UserId = d.Key, TaskCount = d.Count() }).ToList();
            var docketRequestGroupCount = docketRequestGroupList.GroupBy(grp => grp.GroupId).Select(d => new { GroupId = d.Key, TaskCount = d.Count() }).ToList();
            var totalAssignedDocketRequestTask = userReqIdList.Select(d => new { d.SystemType, d.ReqId }).Union(docketRequestGroupList.Select(d => new { d.SystemType, d.ReqId })).DistinctBy(d => new { d.SystemType, d.ReqId }).Count();

            //Get task count for DeDocket
            var deDocketUserCount = userDeDocketIdList.GroupBy(grp => grp.UserId).Select(d => new { UserId = d.Key, TaskCount = d.Count() }).ToList();
            var deDocketGroupCount = deDocketGroupList.GroupBy(grp => grp.GroupId).Select(d => new { GroupId = d.Key, TaskCount = d.Count() }).ToList();
            var totalAssignedDeDocketTask = userDeDocketIdList.Select(d => new { d.SystemType, d.DeDocketId }).Union(deDocketGroupList.Select(d => new { d.SystemType, d.DeDocketId })).DistinctBy(d => new { d.SystemType, d.DeDocketId }).Count();

            //****************************************************************************            
            //Prepare data to return
            var userCount = docUserCount.Concat(docketRequestUserCount).Concat(deDocketUserCount).GroupBy(grp => grp.UserId)
                .Select(d => new { UserId = d.Key, TaskCount = d.Sum(s => s.TaskCount) }).ToList();
            var groupCount = docGroupCount.Concat(docketRequestGroupCount).Concat(deDocketGroupCount).GroupBy(grp => grp.GroupId)
                .Select(d => new { GroupId = d.Key, TaskCount = d.Sum(s => s.TaskCount) }).ToList();

            var totalAssignedTask = totalAssignedDocTask + totalAssignedDocketRequestTask + totalAssignedDeDocketTask;

            data.AddRange(userCount.Join(userList,
                                                c => c.UserId,
                                                l => l.UserId,
                                                (c, l) => new ChartDTO
                                                {
                                                    Category = l.Name,
                                                    Color = "UserId|" + l.UserId,
                                                    Value = Decimal.Round(Convert.ToDecimal(c.TaskCount * 100.00) / Convert.ToDecimal(totalAssignedTask), 2)
                                                }).ToList());
            data.AddRange(groupCount.Join(groupList,
                                                    c => c.GroupId,
                                                    l => l.GroupId,
                                                    (c, l) => new ChartDTO
                                                    {
                                                        Category = l.Name,
                                                        Color = "GroupId|" + l.GroupId.ToString(),
                                                        Value = Decimal.Round(Convert.ToDecimal(c.TaskCount * 100.00) / Convert.ToDecimal(totalAssignedTask), 2)
                                                    }).ToList());

            return Json(data);
        }

        /// <summary>
        /// To show detail of the distribution of the total tasks among users or groups
        /// </summary>
        /// <param name="request"></param>
        /// <param name="mainSearchFilters"></param>
        /// <param name="selectedId"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetTaskDistributionGrid([DataSourceRequest] DataSourceRequest request, string? selectedTaskDistributionEntity = null)
        {
            var defaultSettings = await _defaultSettings.GetSetting();
            var selectUserIdFilter = string.Empty;
            var selectGroupIdFilter = 0;

            if (!string.IsNullOrEmpty(selectedTaskDistributionEntity))
            {
                var selectedArr = selectedTaskDistributionEntity.Split("|");
                if (selectedArr.Length > 0)
                {
                    if (selectedArr[0].ToLower() == "userid")
                        selectUserIdFilter = selectedArr[1];
                    else if (selectedArr[0].ToLower() == "groupid")
                        selectGroupIdFilter = int.Parse(selectedArr[1]);
                }
            }

            var canAccessPatDocVer = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;
            var canAccessTmkDocVer = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;
            var canAccessGmDocVer = (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;

            bool showPatent = true;
            bool showTrademark = true;
            bool showGenMatter = true;
            //****************************************************************************
            //Raw data sources
            //DocVerification
            var rawDocRespDocketings = await _docService.DocRespDocketings.AsNoTracking()
                .Where(d => d.DocId > 0 && d.DocDocument != null && d.DocDocument.IsVerified && d.DocDocument.IsActRequired
                    && (string.IsNullOrEmpty(selectedTaskDistributionEntity)
                        || ((!string.IsNullOrEmpty(selectUserIdFilter) && d.UserId == selectUserIdFilter))
                        || (selectGroupIdFilter > 0 && d.GroupId == selectGroupIdFilter)
                        )
                )
                .Select(d => new
                {
                    d.DocId,
                    d.UserId,
                    d.GroupId,
                    d.LastUpdate,
                    Verifications = d.DocDocument != null && d.DocDocument.DocVerifications != null ? d.DocDocument.DocVerifications.ToList() : null
                })
                .ToListAsync();

            //DocketRequest
            var docketRequestResps = await _docketRequestService.PatDocketRequestResps.AsNoTracking()
                .Where(d => d.PatDocketRequest != null && (string.IsNullOrEmpty(selectedTaskDistributionEntity)
                    || ((!string.IsNullOrEmpty(selectUserIdFilter) && d.UserId == selectUserIdFilter))
                    || (selectGroupIdFilter > 0 && d.GroupId == selectGroupIdFilter))
                )
                .Select(d => new { SystemType = SystemTypeCode.Patent, d.ReqId, d.UserId, d.GroupId, d.LastUpdate, d.PatDocketRequest!.CompletedDate })
                .Union(_docketRequestService.TmkDocketRequestResps.AsNoTracking()
                .Where(d => d.TmkDocketRequest != null && (string.IsNullOrEmpty(selectedTaskDistributionEntity)
                    || ((!string.IsNullOrEmpty(selectUserIdFilter) && d.UserId == selectUserIdFilter))
                    || (selectGroupIdFilter > 0 && d.GroupId == selectGroupIdFilter))
                )
                .Select(d => new { SystemType = SystemTypeCode.Trademark, d.ReqId, d.UserId, d.GroupId, d.LastUpdate, d.TmkDocketRequest!.CompletedDate }))
                .Union(_docketRequestService.GMDocketRequestResps.AsNoTracking()
                .Where(d => d.GMDocketRequest != null && (string.IsNullOrEmpty(selectedTaskDistributionEntity)
                    || ((!string.IsNullOrEmpty(selectUserIdFilter) && d.UserId == selectUserIdFilter))
                    || (selectGroupIdFilter > 0 && d.GroupId == selectGroupIdFilter))
                )
                .Select(d => new { SystemType = SystemTypeCode.GeneralMatter, d.ReqId, d.UserId, d.GroupId, d.LastUpdate, d.GMDocketRequest!.CompletedDate }))
                .ToListAsync();

            //DeDocket
            var deDocketResps = await _repository.PatDueDateDeDocketResps.AsNoTracking()
                .Where(d => d.PatDueDateDeDocket != null && (string.IsNullOrEmpty(selectedTaskDistributionEntity)
                    || ((!string.IsNullOrEmpty(selectUserIdFilter) && d.UserId == selectUserIdFilter))
                    || (selectGroupIdFilter > 0 && d.GroupId == selectGroupIdFilter))
                )
                .Select(d => new { SystemType = SystemTypeCode.Patent, d.DeDocketId, d.UserId, d.GroupId, d.LastUpdate, d.PatDueDateDeDocket!.InstructionCompleted })
                .Union(_repository.TmkDueDateDeDocketResps.AsNoTracking()
                .Where(d => d.TmkDueDateDeDocket != null && (string.IsNullOrEmpty(selectedTaskDistributionEntity)
                    || ((!string.IsNullOrEmpty(selectUserIdFilter) && d.UserId == selectUserIdFilter))
                    || (selectGroupIdFilter > 0 && d.GroupId == selectGroupIdFilter))
                )
                .Select(d => new { SystemType = SystemTypeCode.Trademark, d.DeDocketId, d.UserId, d.GroupId, d.LastUpdate, d.TmkDueDateDeDocket!.InstructionCompleted }))
                .Union(_repository.GMDueDateDeDocketResps.AsNoTracking()
                .Where(d => d.GMDueDateDeDocket != null && (string.IsNullOrEmpty(selectedTaskDistributionEntity)
                    || ((!string.IsNullOrEmpty(selectUserIdFilter) && d.UserId == selectUserIdFilter))
                    || (selectGroupIdFilter > 0 && d.GroupId == selectGroupIdFilter))
                )
                .Select(d => new { SystemType = SystemTypeCode.GeneralMatter, d.DeDocketId, d.UserId, d.GroupId, d.LastUpdate, d.GMDueDateDeDocket!.InstructionCompleted }))
                .ToListAsync();

            if (defaultSettings.IncludeDeDocketInVerification == false)
                deDocketResps.Clear();
            //****************************************************************************

            //****************************************************************************
            //Apply filters
            //DocVerification are from CountryApplication/Trademark/GeneralMatter
            var settings = await GetWidgetSettings("taskdistribution");
            if (!string.IsNullOrEmpty(settings))
            {
                //Prepare filters - start
                showPatent = settings.Contains("ShowPatent") ? GetWidgetSetting<bool>("ShowPatent", settings) : true;
                showTrademark = settings.Contains("ShowTrademark") ? GetWidgetSetting<bool>("ShowTrademark", settings) : true;
                showGenMatter = settings.Contains("ShowGenMatter") ? GetWidgetSetting<bool>("ShowGenMatter", settings) : true;

                if (!canAccessPatDocVer) showPatent = false;
                if (!canAccessTmkDocVer) showTrademark = false;
                if (!canAccessGmDocVer) showGenMatter = false;

                string assignedDateFromStr = GetWidgetSetting<string>("CreatedDateFrom", settings);
                DateTime? assignedDateFrom = !string.IsNullOrEmpty(assignedDateFromStr) ? DateTime.ParseExact(assignedDateFromStr, "MM/dd/yyyy", CultureInfo.InvariantCulture) : null;
                string assignedDateToStr = GetWidgetSetting<string>("AssignedDateTo", settings);
                DateTime? assignedDateTo = !string.IsNullOrEmpty(assignedDateToStr) ? DateTime.ParseExact(assignedDateToStr, "MM/dd/yyyy", CultureInfo.InvariantCulture) : null;

                var respDocketings = GetWidgetSetting<JArray>("Responsibles", settings);
                var respDocketingFilter = respDocketings == null ? new List<string>() : respDocketings.Select(a => (string)a.ToString() ?? "").ToList();

                var userIdFilter = new List<string>();
                var groupIdFilter = new List<int>();

                foreach (var respDocketing in respDocketingFilter.Where(d => !string.IsNullOrEmpty(d)).ToList())
                {
                    var itemArr = respDocketing.Split("|");
                    var intVal = 0;
                    if (int.TryParse(itemArr[1], out intVal))
                        groupIdFilter.Add(intVal);
                    else
                        userIdFilter.Add(itemArr[1]);
                }
                //Prepare filters - end
                //****************************************************************************

                //DocVerification
                //Filter on Documents
                var docIds = rawDocRespDocketings.Where(d => d.DocId > 0).Select(d => d.DocId).Distinct().ToList();
                var filteredDocIds = await _docService.DocDocuments.AsNoTracking().Where(d => docIds.Contains(d.DocId) && d.DocFolder != null)
                    .Select(d => new
                    {
                        d.DocId,
                        Include = d.DocFolder!.SystemType == SystemTypeCode.Patent && d.DocFolder.ScreenCode == ScreenCode.Application && d.DocFolder.DataKey == "AppId"
                                    ? _countryApplicationService.CountryApplications.Any(c => c.AppId == d.DocFolder.DataKeyValue) && showPatent
                                    : d.DocFolder!.SystemType == SystemTypeCode.Trademark && d.DocFolder.ScreenCode == ScreenCode.Trademark && d.DocFolder.DataKey == "TmkId"
                                    ? _tmkTrademarkService.TmkTrademarks.Any(t => t.TmkId == d.DocFolder.DataKeyValue) && showTrademark
                                    : d.DocFolder!.SystemType == SystemTypeCode.GeneralMatter && d.DocFolder.ScreenCode == ScreenCode.GeneralMatter && d.DocFolder.DataKey == "MatId"
                                    ? false // GM module removed: _gmMatterService.QueryableList.Any(g => g.MatId == d.DocFolder.DataKeyValue) && showGenMatter
                                    : false
                    }).Distinct().ToListAsync();
                //Filter on RespDocketings
                rawDocRespDocketings = rawDocRespDocketings
                    .Where(d => (filteredDocIds == null || !filteredDocIds.Any() || filteredDocIds.Any(f => f.Include && f.DocId == d.DocId))
                        && (!userIdFilter.Any() || (!string.IsNullOrEmpty(d.UserId) && userIdFilter.Contains(d.UserId)))
                        && (!groupIdFilter.Any() || groupIdFilter.Contains(d.GroupId ?? 0))
                        && (assignedDateFrom == null || (d.LastUpdate != null && d.LastUpdate.Value.Date >= assignedDateFrom.Value.Date))
                        && (assignedDateTo == null || (d.LastUpdate != null && d.LastUpdate.Value.Date <= assignedDateTo.Value.Date))
                    )
                    .ToList();

                //DocketRequest
                if (!showPatent)
                    docketRequestResps.RemoveAll(d => d.SystemType == SystemTypeCode.Patent);
                if (!showTrademark)
                    docketRequestResps.RemoveAll(d => d.SystemType == SystemTypeCode.Trademark);
                if (!showGenMatter)
                    docketRequestResps.RemoveAll(d => d.SystemType == SystemTypeCode.GeneralMatter);

                docketRequestResps = docketRequestResps
                    .Where(d => (!userIdFilter.Any() || (!string.IsNullOrEmpty(d.UserId) && userIdFilter.Contains(d.UserId)))
                        && (!groupIdFilter.Any() || groupIdFilter.Contains(d.GroupId ?? 0))
                        && (assignedDateFrom == null || (d.LastUpdate != null && d.LastUpdate.Value.Date >= assignedDateFrom.Value.Date))
                        && (assignedDateTo == null || (d.LastUpdate != null && d.LastUpdate.Value.Date <= assignedDateTo.Value.Date))
                    ).ToList();

                //DeDocket
                if (!showPatent)
                    deDocketResps.RemoveAll(d => d.SystemType == SystemTypeCode.Patent);
                if (!showTrademark)
                    deDocketResps.RemoveAll(d => d.SystemType == SystemTypeCode.Trademark);
                if (!showGenMatter)
                    deDocketResps.RemoveAll(d => d.SystemType == SystemTypeCode.GeneralMatter);

                deDocketResps = deDocketResps
                    .Where(d => (!userIdFilter.Any() || (!string.IsNullOrEmpty(d.UserId) && userIdFilter.Contains(d.UserId)))
                        && (!groupIdFilter.Any() || groupIdFilter.Contains(d.GroupId ?? 0))
                        && (assignedDateFrom == null || (d.LastUpdate != null && d.LastUpdate.Value.Date >= assignedDateFrom.Value.Date))
                        && (assignedDateTo == null || (d.LastUpdate != null && d.LastUpdate.Value.Date <= assignedDateTo.Value.Date))
                    )
                    .ToList();
            }
            else
            {
                //Check if module is enabled for each system when widget setting is default (empty)
                if (!canAccessPatDocVer) showPatent = false;
                if (!canAccessTmkDocVer) showTrademark = false;
                if (!canAccessGmDocVer) showGenMatter = false;

                var docIds = rawDocRespDocketings.Where(d => d.DocId > 0).Select(d => d.DocId).Distinct().ToList();
                var filteredDocIds = await _docService.DocDocuments.AsNoTracking().Where(d => docIds.Contains(d.DocId) && d.DocFolder != null)
                    .Select(d => new
                    {
                        d.DocId,
                        Include = d.DocFolder!.SystemType == SystemTypeCode.Patent && d.DocFolder.ScreenCode == ScreenCode.Application && d.DocFolder.DataKey == "AppId"
                                    ? _countryApplicationService.CountryApplications.Any(c => c.AppId == d.DocFolder.DataKeyValue) && showPatent
                                    : d.DocFolder!.SystemType == SystemTypeCode.Trademark && d.DocFolder.ScreenCode == ScreenCode.Trademark && d.DocFolder.DataKey == "TmkId"
                                    ? _tmkTrademarkService.TmkTrademarks.Any(t => t.TmkId == d.DocFolder.DataKeyValue) && showTrademark
                                    : d.DocFolder!.SystemType == SystemTypeCode.GeneralMatter && d.DocFolder.ScreenCode == ScreenCode.GeneralMatter && d.DocFolder.DataKey == "MatId"
                                    ? false // GM module removed: _gmMatterService.QueryableList.Any(g => g.MatId == d.DocFolder.DataKeyValue) && showGenMatter
                                    : false
                    }).Distinct().ToListAsync();
                //Filter on RespDocketings
                rawDocRespDocketings = rawDocRespDocketings.Where(d => filteredDocIds == null || !filteredDocIds.Any() || filteredDocIds.Any(f => f.Include && f.DocId == d.DocId)).ToList();

                //DocketRequest
                if (!showPatent)
                    docketRequestResps.RemoveAll(d => d.SystemType == SystemTypeCode.Patent);
                if (!showTrademark)
                    docketRequestResps.RemoveAll(d => d.SystemType == SystemTypeCode.Trademark);
                if (!showGenMatter)
                    docketRequestResps.RemoveAll(d => d.SystemType == SystemTypeCode.GeneralMatter);

                //DeDocket
                if (!showPatent)
                    deDocketResps.RemoveAll(d => d.SystemType == SystemTypeCode.Patent);
                if (!showTrademark)
                    deDocketResps.RemoveAll(d => d.SystemType == SystemTypeCode.Trademark);
                if (!showGenMatter)
                    deDocketResps.RemoveAll(d => d.SystemType == SystemTypeCode.GeneralMatter);
            }

            //Get UserId/GroupId where DocVerification is both empty and not empty
            var docRespDocketings = rawDocRespDocketings.Where(d => d.Verifications != null && d.Verifications.Count > 0)
                .SelectMany(d => d.Verifications!.Select(v => new { DocId = d.DocId, UserId = d.UserId, GroupId = d.GroupId, ActId = v.ActId ?? 0, ActionTypeID = v.ActionTypeID ?? 0 }))
                .Union(rawDocRespDocketings.Where(d => d.Verifications == null || d.Verifications.Count <= 0).Select(d => new { DocId = d.DocId, UserId = d.UserId, GroupId = d.GroupId, ActId = 0, ActionTypeID = 0 }))
                .ToList();
            //****************************************************************************

            //****************************************************************************
            //Get users' names and groups' names
            //Get distinct groupId to get group names for all data type
            var groupIdList = docRespDocketings.Where(d => d.GroupId > 0).Select(d => d.GroupId)
                .Union(docketRequestResps.Where(d => d.GroupId > 0).Select(d => d.GroupId))
                .Union(deDocketResps.Where(d => d.GroupId > 0).Select(d => d.GroupId))
                .Distinct().ToList();
            //Get group names for all data type
            var groupList = await _groupManager.QueryableList.AsNoTracking().Where(d => groupIdList.Contains(d.Id)).Select(d => new { GroupId = d.Id, Name = d.Name }).ToListAsync();

            //Get users under groups for all data type
            var userGroupList = await _groupManager.CPiUserGroups.AsNoTracking()
                .Where(d => groupIdList.Contains(d.GroupId) && !string.IsNullOrEmpty(d.UserId))
                .Select(d => new { d.GroupId, d.UserId })
                .Distinct().ToListAsync();

            //Get distinct userId to get user names for all data type
            var userIdList = docRespDocketings.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId)
                .Union(userGroupList.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId))
                .Union(docketRequestResps.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId))
                .Union(deDocketResps.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId))
                .Distinct().ToList();
            //Get user names for all data type
            var userList = await _userManager.Users.AsNoTracking().Where(d => userIdList.Contains(d.Id)).Select(d => new { UserId = d.Id, Name = d.FirstName + " " + d.LastName }).ToListAsync();
            //****************************************************************************

            //****************************************************************************
            //Add users under groups to data source
            //DocVerification
            var user_docRespDocketings = docRespDocketings.Where(d => d.GroupId > 0).Join(userGroupList,
                d => d.GroupId,
                u => u.GroupId,
                (d, u) => new
                {
                    d.DocId,
                    d.ActId,
                    d.ActionTypeID,
                    u.UserId
                }).ToList();
            user_docRespDocketings.AddRange(docRespDocketings.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => new { d.DocId, d.ActId, d.ActionTypeID, UserId = d.UserId ?? "" }).ToList());
            user_docRespDocketings = user_docRespDocketings.DistinctBy(d => new { d.DocId, d.ActId, d.ActionTypeID, d.UserId }).ToList();

            //DocketRequest
            var user_docketRequestResps = docketRequestResps.Where(d => d.GroupId > 0).Join(userGroupList,
                d => d.GroupId,
                grpUsr => grpUsr.GroupId,
                (d, grpUsr) => new
                {
                    d.SystemType,
                    d.ReqId,
                    grpUsr.UserId,
                    d.LastUpdate,
                    d.CompletedDate
                })
            .Union(docketRequestResps.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => new { d.SystemType, d.ReqId, UserId = d.UserId ?? "", d.LastUpdate, d.CompletedDate }))
            .DistinctBy(d => new { d.SystemType, d.ReqId, d.UserId, d.LastUpdate, d.CompletedDate }).ToList();

            //DeDocket
            var user_deDocketResps = deDocketResps.Where(d => d.GroupId > 0).Join(userGroupList,
                d => d.GroupId,
                grpUsr => grpUsr.GroupId,
                (d, grpUsr) => new
                {
                    d.SystemType,
                    d.DeDocketId,
                    grpUsr.UserId,
                    d.LastUpdate,
                    d.InstructionCompleted
                })
            .Union(deDocketResps.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => new { d.SystemType, d.DeDocketId, UserId = d.UserId ?? "", d.LastUpdate, d.InstructionCompleted }))
            .DistinctBy(d => new { d.SystemType, d.DeDocketId, d.UserId, d.LastUpdate, d.InstructionCompleted }).ToList();
            //****************************************************************************

            //****************************************************************************
            //Prepare data
            //Number of assigned task
            //DocVerification
            var doc_userAssignedCountList = user_docRespDocketings.Where(d => !string.IsNullOrEmpty(d.UserId)).GroupBy(grp => grp.UserId)
                .Select(d => new
                {
                    UserId = d.Key,
                    AssignedCount = d.Count(),
                    CompletedCount = 0,
                    OutstandingCount = 0,
                    PercentCount = (decimal)0
                }).ToList();
            var doc_groupAssignedCountList = docRespDocketings.Where(d => d.GroupId > 0).GroupBy(grp => grp.GroupId)
                .Select(d => new
                {
                    GroupId = d.Key,
                    AssignedCount = d.Count(),
                    CompletedCount = 0,
                    OutstandingCount = 0,
                    PercentCount = (decimal)0
                }).ToList();

            //DocketRequest
            var docketRequest_userAssignedCountList = user_docketRequestResps.Where(d => !string.IsNullOrEmpty(d.UserId)).GroupBy(grp => grp.UserId)
                .Select(d => new
                {
                    UserId = d.Key,
                    AssignedCount = d.Count(),
                    CompletedCount = 0,
                    OutstandingCount = 0,
                    PercentCount = (decimal)0
                }).ToList();
            var docketRequest_groupAssignedCountList = docketRequestResps.Where(d => d.GroupId > 0).GroupBy(grp => grp.GroupId)
                .Select(d => new
                {
                    GroupId = d.Key,
                    AssignedCount = d.Count(),
                    CompletedCount = 0,
                    OutstandingCount = 0,
                    PercentCount = (decimal)0
                }).ToList();

            //DeDocket
            var deDocket_userAssignedCountList = user_deDocketResps.Where(d => !string.IsNullOrEmpty(d.UserId)).GroupBy(grp => grp.UserId)
                .Select(d => new
                {
                    UserId = d.Key,
                    AssignedCount = d.Count(),
                    CompletedCount = 0,
                    OutstandingCount = 0,
                    PercentCount = (decimal)0
                }).ToList();
            var deDocket_groupAssignedCountList = deDocketResps.Where(d => d.GroupId > 0).GroupBy(grp => grp.GroupId)
                .Select(d => new
                {
                    GroupId = d.Key,
                    AssignedCount = d.Count(),
                    CompletedCount = 0,
                    OutstandingCount = 0,
                    PercentCount = (decimal)0
                }).ToList();

            //****************************************************************************
            //Number of completed task
            //DocVerification
            var doc_userCompletedCountList = user_docRespDocketings.Where(d => !string.IsNullOrEmpty(d.UserId) && d.ActId > 0).GroupBy(grp => grp.UserId)
                .Select(d => new
                {
                    UserId = d.Key,
                    AssignedCount = 0,
                    CompletedCount = d.Count(),
                    OutstandingCount = 0,
                    PercentCount = (decimal)0
                }).ToList();
            var doc_groupCompletedCountList = docRespDocketings.Where(d => d.GroupId > 0 && d.ActId > 0).GroupBy(grp => grp.GroupId)
                .Select(d => new
                {
                    GroupId = d.Key,
                    AssignedCount = 0,
                    CompletedCount = d.Count(),
                    OutstandingCount = 0,
                    PercentCount = (decimal)0
                }).ToList();

            //DocketRequest
            var docketRequest_userCompletedCountList = user_docketRequestResps.Where(d => !string.IsNullOrEmpty(d.UserId) && d.CompletedDate != null).GroupBy(grp => grp.UserId)
                .Select(d => new
                {
                    UserId = d.Key,
                    AssignedCount = 0,
                    CompletedCount = d.Count(),
                    OutstandingCount = 0,
                    PercentCount = (decimal)0
                }).ToList();
            var docketRequest_groupCompletedCountList = docketRequestResps.Where(d => d.GroupId > 0 && d.CompletedDate != null).GroupBy(grp => grp.GroupId)
                .Select(d => new
                {
                    GroupId = d.Key,
                    AssignedCount = 0,
                    CompletedCount = d.Count(),
                    OutstandingCount = 0,
                    PercentCount = (decimal)0
                }).ToList();

            //DeDocket
            var deDocket_userCompletedCountList = user_deDocketResps.Where(d => !string.IsNullOrEmpty(d.UserId) && d.InstructionCompleted == true).GroupBy(grp => grp.UserId)
                .Select(d => new
                {
                    UserId = d.Key,
                    AssignedCount = 0,
                    CompletedCount = d.Count(),
                    OutstandingCount = 0,
                    PercentCount = (decimal)0
                }).ToList();
            var deDocket_groupCompletedCountList = deDocketResps.Where(d => d.GroupId > 0 && d.InstructionCompleted == true).GroupBy(grp => grp.GroupId)
                .Select(d => new
                {
                    GroupId = d.Key,
                    AssignedCount = 0,
                    CompletedCount = d.Count(),
                    OutstandingCount = 0,
                    PercentCount = (decimal)0
                }).ToList();

            //****************************************************************************
            //Number of outstanding task
            //DocVerification
            var doc_userOutstandingCountList = user_docRespDocketings.Where(d => !string.IsNullOrEmpty(d.UserId) && (d.ActionTypeID > 0 || (d.ActId <= 0 && d.ActionTypeID <= 0)))
                .GroupBy(grp => grp.UserId)
                .Select(d => new
                {
                    UserId = d.Key,
                    AssignedCount = 0,
                    CompletedCount = 0,
                    OutstandingCount = d.Count(),
                    PercentCount = (decimal)0
                }).ToList();
            var doc_groupOutstandingCountList = docRespDocketings.Where(d => d.GroupId > 0 && (d.ActionTypeID > 0 || (d.ActId <= 0 && d.ActionTypeID <= 0)))
                .GroupBy(grp => grp.GroupId)
                .Select(d => new
                {
                    GroupId = d.Key,
                    AssignedCount = 0,
                    CompletedCount = 0,
                    OutstandingCount = d.Count(),
                    PercentCount = (decimal)0
                }).ToList();

            //DocketRequest
            var docketRequest_userOutstandingCountList = user_docketRequestResps.Where(d => !string.IsNullOrEmpty(d.UserId) && d.CompletedDate == null)
                .GroupBy(grp => grp.UserId)
                .Select(d => new
                {
                    UserId = d.Key,
                    AssignedCount = 0,
                    CompletedCount = 0,
                    OutstandingCount = d.Count(),
                    PercentCount = (decimal)0
                }).ToList();
            var docketRequest_groupOutstandingCountList = docketRequestResps.Where(d => d.GroupId > 0 && d.CompletedDate == null)
                .GroupBy(grp => grp.GroupId)
                .Select(d => new
                {
                    GroupId = d.Key,
                    AssignedCount = 0,
                    CompletedCount = 0,
                    OutstandingCount = d.Count(),
                    PercentCount = (decimal)0
                }).ToList();

            //DeDocket
            var deDocket_userOutstandingCountList = user_deDocketResps.Where(d => !string.IsNullOrEmpty(d.UserId) && d.InstructionCompleted == false)
                .GroupBy(grp => grp.UserId)
                .Select(d => new
                {
                    UserId = d.Key,
                    AssignedCount = 0,
                    CompletedCount = 0,
                    OutstandingCount = d.Count(),
                    PercentCount = (decimal)0
                }).ToList();
            var deDocket_groupOutstandingCountList = deDocketResps.Where(d => d.GroupId > 0 && d.InstructionCompleted == false)
                .GroupBy(grp => grp.GroupId)
                .Select(d => new
                {
                    GroupId = d.Key,
                    AssignedCount = 0,
                    CompletedCount = 0,
                    OutstandingCount = d.Count(),
                    PercentCount = (decimal)0
                }).ToList();

            //****************************************************************************
            //Combine counts before calculating percentage of completion
            var userData = doc_userAssignedCountList
                .Concat(doc_userCompletedCountList)
                .Concat(doc_userOutstandingCountList)
                //.Concat(doc_userPercentCountList)
                .Concat(docketRequest_userAssignedCountList)
                .Concat(docketRequest_userCompletedCountList)
                .Concat(docketRequest_userOutstandingCountList)
                //.Concat(docketRequest_userPercentCountList)
                .Concat(deDocket_userAssignedCountList)
                .Concat(deDocket_userCompletedCountList)
                .Concat(deDocket_userOutstandingCountList)
                //.Concat(deDocket_userPercentCountList)
                .GroupBy(grp => grp.UserId)
                .Select(d => new DocketingTaskDistributionViewModel()
                {
                    Name = d.Key,
                    NoOfAssigned = d.Sum(s => s.AssignedCount),
                    NoOfCompleted = d.Sum(s => s.CompletedCount),
                    NoOfOutstanding = d.Sum(s => s.OutstandingCount),
                    //CompletePercent = d.Sum(s => s.PercentCount)
                    CompletePercent = 0
                }).ToList();

            var groupData = doc_groupAssignedCountList
                .Concat(doc_groupCompletedCountList)
                .Concat(doc_groupOutstandingCountList)
                //.Concat(doc_groupPercentCountList)
                .Concat(docketRequest_groupAssignedCountList)
                .Concat(docketRequest_groupCompletedCountList)
                .Concat(docketRequest_groupOutstandingCountList)
                //.Concat(docketRequest_groupPercentCountList)
                .Concat(deDocket_groupAssignedCountList)
                .Concat(deDocket_groupCompletedCountList)
                .Concat(deDocket_groupOutstandingCountList)
                //.Concat(deDocket_groupPercentCountList)
                .GroupBy(grp => grp.GroupId)
                .Select(d => new DocketingTaskDistributionViewModel()
                {
                    Name = d.Key.ToString(),
                    NoOfAssigned = d.Sum(s => s.AssignedCount),
                    NoOfCompleted = d.Sum(s => s.CompletedCount),
                    NoOfOutstanding = d.Sum(s => s.OutstandingCount),
                    //CompletePercent = d.Sum(s => s.PercentCount)
                    CompletePercent = 0
                }).ToList();

            //Get names for users and groupd, and also calculate percentage of completion
            userData.ForEach(d =>
            {
                d.Name = userList.FirstOrDefault(u => u.UserId == d.Name)?.Name ?? d.Name;
                d.CompletePercent = Decimal.Round(Convert.ToDecimal(d.NoOfCompleted * 100.00) / Convert.ToDecimal(d.NoOfAssigned), 2);
            });
            groupData.ForEach(d =>
            {
                d.Name = groupList.FirstOrDefault(g => g.GroupId.ToString() == d.Name)?.Name ?? d.Name;
                d.CompletePercent = Decimal.Round(Convert.ToDecimal(d.NoOfCompleted * 100.00) / Convert.ToDecimal(d.NoOfAssigned), 2);
            });

            //****************************************************************************
            var model = userData.Union(groupData).AsQueryable();
            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(app => app.Name);

            var recCount = model.Select(d => d.Name).Count();

            var result = new CPiDataSourceResult()
            {
                Data = model.ApplyPaging(request.Page, request.PageSize).ToList(),
                Total = recCount,
                //Ids = ids
            };

            return Json(result);
        }

        /// <summary>
        /// To show the workload assigned and completed for the given year (calendar or fiscal) by month
        /// 3 stacked/layer: 
        /// total number of tasks
        /// total number of assigned
        /// total number of completed
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetWorkloadHistogramChart([DataSourceRequest] DataSourceRequest request)
        {
            var settings = await _defaultSettings.GetSetting();
            var today = DateTime.Today;
            var startDate = today.FirstDayOfYear();
            var endDate = today.LastDayOfYear();

            if (!string.IsNullOrEmpty(settings.FiscalCalendarYearStart) && !string.IsNullOrEmpty(settings.FiscalCalendarYearEnd))
            {
                startDate = DateTime.ParseExact(settings.FiscalCalendarYearStart + "/" + DateTime.Today.Year.ToString(), "MM/dd/yyyy", CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(settings.FiscalCalendarYearEnd + "/" + DateTime.Today.Year.ToString(), "MM/dd/yyyy", CultureInfo.InvariantCulture);

                //Calculate FY
                if (startDate > endDate && today <= endDate)
                {
                    startDate = startDate.AddYears(-1);
                }
                else if (startDate > endDate && today > endDate)
                {
                    endDate = endDate.AddYears(1);
                }
            }

            //Apply filters
            var canAccessPatDocVer = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;
            var canAccessTmkDocVer = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;
            var canAccessGmDocVer = (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;

            var widgetSettings = await GetWidgetSettings("workloadhistogram");
            bool showPatent = true;
            bool showTrademark = true;
            bool showGenMatter = true;
            var clientFilter = new List<int>();

            if (!string.IsNullOrEmpty(widgetSettings))
            {
                showPatent = widgetSettings.Contains("ShowPatent") ? GetWidgetSetting<bool>("ShowPatent", widgetSettings) : true;
                showTrademark = widgetSettings.Contains("ShowTrademark") ? GetWidgetSetting<bool>("ShowTrademark", widgetSettings) : true;
                showGenMatter = widgetSettings.Contains("ShowGenMatter") ? GetWidgetSetting<bool>("ShowGenMatter", widgetSettings) : true;

                var clients = GetWidgetSetting<JArray>("Clients", widgetSettings);
                clientFilter = clients == null ? new List<int>() : clients.Select(a => (int)a).ToList();
            }

            if (!canAccessPatDocVer) showPatent = false;
            if (!canAccessTmkDocVer) showTrademark = false;
            if (!canAccessGmDocVer) showGenMatter = false;

            var data = new List<StackedChartViewModel>();

            //****************************************************************************
            //Total number of tasks - START
            //DocVerification
            //1. IsActRequired checked and DocVerification is empty: count as 1 task
            var emptyVerTaskCount = await _docService.DocDocuments.AsNoTracking()
                .Where(d => d.IsVerified && d.IsActRequired && d.ActRequiredLastUpdate != null && d.ActRequiredLastUpdate >= startDate && d.ActRequiredLastUpdate <= endDate
                    && (d.DocVerifications == null || !d.DocVerifications.Any())
                    //Filters
                    && d.DocFolder != null
                    && ((d.DocFolder.SystemType == SystemTypeCode.Patent && d.DocFolder.ScreenCode == ScreenCode.Application && d.DocFolder.DataKey == "AppId"
                            && _countryApplicationService.CountryApplications.Any(c => c.AppId == d.DocFolder.DataKeyValue && (!clientFilter.Any() || clientFilter.Contains(c.Invention!.ClientID ?? 0)))
                            && showPatent)
                        || (d.DocFolder.SystemType == SystemTypeCode.Trademark && d.DocFolder.ScreenCode == ScreenCode.Trademark && d.DocFolder.DataKey == "TmkId"
                            && _tmkTrademarkService.TmkTrademarks.Any(t => t.TmkId == d.DocFolder.DataKeyValue && (!clientFilter.Any() || clientFilter.Contains(t.ClientID ?? 0)))
                            && showTrademark)
                        // GM module removed
                        // || (d.DocFolder.SystemType == SystemTypeCode.GeneralMatter && d.DocFolder.ScreenCode == ScreenCode.GeneralMatter && d.DocFolder.DataKey == "MatId"
                        //     && _gmMatterService.QueryableList.Any(g => g.MatId == d.DocFolder.DataKeyValue && (!clientFilter.Any() || clientFilter.Contains(g.ClientID ?? 0)))
                        //     && showGenMatter))
                        )
                )
                .GroupBy(grp => grp.ActRequiredLastUpdate!.Value.Month)
                .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer1 = d.Count() })
                .ToListAsync();
            //2. IsActRequired check and DocVerification is not empty: count each record in DocVerification with filter on lastupdate
            var verTaskCount = await _docService.DocVerifications.AsNoTracking()
                .Where(d => d.DocDocument != null && d.DocDocument.IsVerified && d.DocDocument.IsActRequired && d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate
                    //Filters
                    && d.DocDocument.DocFolder != null
                    && ((d.DocDocument.DocFolder.SystemType == SystemTypeCode.Patent && d.DocDocument.DocFolder.ScreenCode == ScreenCode.Application && d.DocDocument.DocFolder.DataKey == "AppId"
                            && _countryApplicationService.CountryApplications.Any(c => c.AppId == d.DocDocument.DocFolder.DataKeyValue && (!clientFilter.Any() || clientFilter.Contains(c.Invention!.ClientID ?? 0)))
                            && showPatent)
                        || (d.DocDocument.DocFolder.SystemType == SystemTypeCode.Trademark && d.DocDocument.DocFolder.ScreenCode == ScreenCode.Trademark && d.DocDocument.DocFolder.DataKey == "TmkId"
                            && _tmkTrademarkService.TmkTrademarks.Any(t => t.TmkId == d.DocDocument.DocFolder.DataKeyValue && (!clientFilter.Any() || clientFilter.Contains(t.ClientID ?? 0)))
                            && showTrademark)
                        // GM module removed
                        // || (d.DocDocument.DocFolder.SystemType == SystemTypeCode.GeneralMatter && d.DocDocument.DocFolder.ScreenCode == ScreenCode.GeneralMatter && d.DocDocument.DocFolder.DataKey == "MatId"
                        //     && _gmMatterService.QueryableList.Any(g => g.MatId == d.DocDocument.DocFolder.DataKeyValue && (!clientFilter.Any() || clientFilter.Contains(g.ClientID ?? 0)))
                        //     && showGenMatter))
                        )
                )
                .GroupBy(grp => grp.LastUpdate!.Value.Month)
                .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer1 = d.Count() })
                .ToListAsync();

            //DocketRequest
            var docketRequestTaskCount = new List<StackedChartViewModel>();
            if (showPatent)
                docketRequestTaskCount.AddRange(await _docketRequestService.PatDocketRequests.AsNoTracking()
                    .Where(d => d.DateCreated != null && d.DateCreated >= startDate && d.DateCreated <= endDate)
                    .GroupBy(grp => grp.DateCreated!.Value.Month).Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer1 = d.Count() }).ToListAsync());
            if (showTrademark)
                docketRequestTaskCount.AddRange(await _docketRequestService.TmkDocketRequests.AsNoTracking()
                    .Where(d => d.DateCreated != null && d.DateCreated >= startDate && d.DateCreated <= endDate)
                    .GroupBy(grp => grp.DateCreated!.Value.Month).Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer1 = d.Count() }).ToListAsync());
            if (showGenMatter)
                docketRequestTaskCount.AddRange(await _docketRequestService.GMDocketRequests.AsNoTracking()
                    .Where(d => d.DateCreated != null && d.DateCreated >= startDate && d.DateCreated <= endDate)
                    .GroupBy(grp => grp.DateCreated!.Value.Month).Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer1 = d.Count() }).ToListAsync());

            //DeDocket
            var deDocketTaskCount = new List<StackedChartViewModel>();
            if (settings.IncludeDeDocketInVerification == true)
            {
                if (showPatent)
                    deDocketTaskCount.AddRange(await _patDueDateService.DueDateDeDockets.AsNoTracking()
                        .Where(d => d.InstructionDate != null && d.InstructionDate >= startDate && d.InstructionDate <= endDate)
                        .GroupBy(grp => grp.InstructionDate!.Value.Month).Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer1 = d.Count() }).ToListAsync());
                if (showTrademark)
                    deDocketTaskCount.AddRange(await _tmkDueDateService.DueDateDeDockets.AsNoTracking()
                        .Where(d => d.InstructionDate != null && d.InstructionDate >= startDate && d.InstructionDate <= endDate)
                        .GroupBy(grp => grp.InstructionDate!.Value.Month).Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer1 = d.Count() }).ToListAsync());
                // GM module removed
                // if (showGenMatter)
                //     deDocketTaskCount.AddRange(await _gmDueDateService.DueDateDeDockets.AsNoTracking()
                //         .Where(d => d.InstructionDate != null && d.InstructionDate >= startDate && d.InstructionDate <= endDate)
                //         .GroupBy(grp => grp.InstructionDate!.Value.Month).Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer1 = d.Count() }).ToListAsync());
            }
            //Total number of tasks for Docs - END

            //****************************************************************************
            //Total number of assigned - START
            //DocVerification
            var docAssignedCount = _docService.DocRespDocketings.AsNoTracking()
                .Where(d => d.DocId > 0 && d.DocDocument != null && d.DocDocument.IsVerified && d.DocDocument.IsActRequired && (!string.IsNullOrEmpty(d.UserId) || d.GroupId > 0)
                    && d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate
                    //Filters
                    && (d.DocDocument != null && d.DocDocument.DocFolder != null &&
                        ((d.DocDocument.DocFolder.SystemType == SystemTypeCode.Patent && d.DocDocument.DocFolder.ScreenCode == ScreenCode.Application && d.DocDocument.DocFolder.DataKey == "AppId"
                            && _countryApplicationService.CountryApplications.Any(c => c.AppId == d.DocDocument.DocFolder.DataKeyValue && (!clientFilter.Any() || clientFilter.Contains(c.Invention!.ClientID ?? 0)))
                            && showPatent)
                        || (d.DocDocument.DocFolder.SystemType == SystemTypeCode.Trademark && d.DocDocument.DocFolder.ScreenCode == ScreenCode.Trademark && d.DocDocument.DocFolder.DataKey == "TmkId"
                            && _tmkTrademarkService.TmkTrademarks.Any(t => t.TmkId == d.DocDocument!.DocFolder.DataKeyValue && (!clientFilter.Any() || clientFilter.Contains(t.ClientID ?? 0)))
                            && showTrademark)
                        // GM module removed
                        // || (d.DocDocument.DocFolder.SystemType == SystemTypeCode.GeneralMatter && d.DocDocument.DocFolder.ScreenCode == ScreenCode.GeneralMatter && d.DocDocument.DocFolder.DataKey == "MatId"
                        //     && _gmMatterService.QueryableList.Any(g => g.MatId == d.DocDocument.DocFolder.DataKeyValue && (!clientFilter.Any() || clientFilter.Contains(g.ClientID ?? 0)))
                        //     && showGenMatter))
                        )
                        )
                )
                .AsEnumerable()
                .Select(d => new
                {
                    DocId = d.DocId,
                    Month = d.LastUpdate!.Value.Month,
                    TaskAssignedCount = d.DocDocument != null && d.DocDocument.DocVerifications != null ? d.DocDocument.DocVerifications.Count() : 1
                })
                .DistinctBy(d => new { d.DocId, d.Month, d.TaskAssignedCount })
                .GroupBy(grp => grp.Month)
                .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer2 = d.Sum(s => s.TaskAssignedCount) })
                .ToList();

            //DocketRequset
            var docketRequestAssignedCount = new List<StackedChartViewModel>();
            if (showPatent)
                docketRequestAssignedCount.AddRange(await _docketRequestService.PatDocketRequestResps.AsNoTracking()
                    .Where(d => d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate)
                    .GroupBy(grp => grp.LastUpdate!.Value.Month)
                    .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer2 = d.Count() }).ToListAsync());
            if (showTrademark)
                docketRequestAssignedCount.AddRange(await _docketRequestService.TmkDocketRequestResps.AsNoTracking()
                    .Where(d => d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate)
                    .GroupBy(grp => grp.LastUpdate!.Value.Month)
                    .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer2 = d.Count() }).ToListAsync());
            if (showGenMatter)
                docketRequestAssignedCount.AddRange(await _docketRequestService.GMDocketRequestResps.AsNoTracking()
                    .Where(d => d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate)
                    .GroupBy(grp => grp.LastUpdate!.Value.Month)
                    .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer2 = d.Count() }).ToListAsync());

            //DeDocket
            var deDocketAssignedCount = new List<StackedChartViewModel>();
            if (settings.IncludeDeDocketInVerification == true)
            {
                if (showPatent)
                    deDocketAssignedCount.AddRange(await _patDueDateService.DueDateDeDocketResps.AsNoTracking()
                        .Where(d => d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate)
                        .GroupBy(grp => grp.LastUpdate!.Value.Month)
                        .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer2 = d.Count() }).ToListAsync());
                if (showTrademark)
                    deDocketAssignedCount.AddRange(await _tmkDueDateService.DueDateDeDocketResps.AsNoTracking()
                        .Where(d => d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate)
                        .GroupBy(grp => grp.LastUpdate!.Value.Month)
                        .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer2 = d.Count() }).ToListAsync());
                // GM module removed
                // if (showGenMatter)
                //     deDocketAssignedCount.AddRange(await _gmDueDateService.DueDateDeDocketResps.AsNoTracking()
                //         .Where(d => d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate)
                //         .GroupBy(grp => grp.LastUpdate!.Value.Month)
                //         .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer2 = d.Count() }).ToListAsync());
            }
            //Total number of assigned - END

            //****************************************************************************
            //Total number of completed - START
            //DocVerification
            var docCompletedCount = await _docService.DocVerifications.AsNoTracking()
                .Where(d => d.DocId > 0 && d.DocDocument != null && d.DocDocument.IsVerified && d.DocDocument.IsActRequired && d.ActId > 0
                    && d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate
                    //Filters
                    && (d.DocDocument != null && d.DocDocument.DocFolder != null &&
                        ((d.DocDocument.DocFolder.SystemType == SystemTypeCode.Patent && d.DocDocument.DocFolder.ScreenCode == ScreenCode.Application && d.DocDocument.DocFolder.DataKey == "AppId"
                            && _countryApplicationService.CountryApplications.Any(c => c.AppId == d.DocDocument.DocFolder.DataKeyValue && (!clientFilter.Any() || clientFilter.Contains(c.Invention!.ClientID ?? 0)))
                            && showPatent)
                        || (d.DocDocument.DocFolder.SystemType == SystemTypeCode.Trademark && d.DocDocument.DocFolder.ScreenCode == ScreenCode.Trademark && d.DocDocument.DocFolder.DataKey == "TmkId"
                            && _tmkTrademarkService.TmkTrademarks.Any(t => t.TmkId == d.DocDocument!.DocFolder.DataKeyValue && (!clientFilter.Any() || clientFilter.Contains(t.ClientID ?? 0)))
                            && showTrademark)
                        // GM module removed
                        // || (d.DocDocument.DocFolder.SystemType == SystemTypeCode.GeneralMatter && d.DocDocument.DocFolder.ScreenCode == ScreenCode.GeneralMatter && d.DocDocument.DocFolder.DataKey == "MatId"
                        //     && _gmMatterService.QueryableList.Any(g => g.MatId == d.DocDocument.DocFolder.DataKeyValue && (!clientFilter.Any() || clientFilter.Contains(g.ClientID ?? 0)))
                        //     && showGenMatter))
                        )
                        )
                )
                .GroupBy(grp => grp.LastUpdate!.Value.Month)
                .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer3 = d.Count() })
                .ToListAsync();

            //DocketRequest
            var docketRequestCompletedCount = new List<StackedChartViewModel>();
            if (showPatent)
                docketRequestCompletedCount.AddRange(await _docketRequestService.PatDocketRequests.AsNoTracking()
                    .Where(d => d.CompletedDate != null && d.CompletedDate >= startDate && d.CompletedDate <= endDate)
                    .GroupBy(grp => grp.CompletedDate!.Value.Month)
                    .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer3 = d.Count() }).ToListAsync());
            if (showTrademark)
                docketRequestCompletedCount.AddRange(await _docketRequestService.TmkDocketRequests.AsNoTracking()
                    .Where(d => d.CompletedDate != null && d.CompletedDate >= startDate && d.CompletedDate <= endDate)
                    .GroupBy(grp => grp.CompletedDate!.Value.Month)
                    .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer3 = d.Count() }).ToListAsync());
            if (showGenMatter)
                docketRequestCompletedCount.AddRange(await _docketRequestService.GMDocketRequests.AsNoTracking()
                    .Where(d => d.CompletedDate != null && d.CompletedDate >= startDate && d.CompletedDate <= endDate)
                    .GroupBy(grp => grp.CompletedDate!.Value.Month)
                    .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer3 = d.Count() }).ToListAsync());

            //DeDocket
            var deDocketCompletedCount = new List<StackedChartViewModel>();
            if (settings.IncludeDeDocketInVerification == true)
            {
                if (showPatent)
                    deDocketCompletedCount.AddRange(await _patDueDateService.DueDateDeDockets.AsNoTracking()
                        .Where(d => d.InstructionCompleted == true && d.CompletedDate != null && d.CompletedDate >= startDate && d.CompletedDate <= endDate)
                        .GroupBy(grp => grp.CompletedDate!.Value.Month)
                        .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer3 = d.Count() }).ToListAsync());
                if (showTrademark)
                    deDocketCompletedCount.AddRange(await _tmkDueDateService.DueDateDeDockets.AsNoTracking()
                        .Where(d => d.InstructionCompleted == true && d.CompletedDate != null && d.CompletedDate >= startDate && d.CompletedDate <= endDate)
                        .GroupBy(grp => grp.CompletedDate!.Value.Month)
                        .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer3 = d.Count() }).ToListAsync());
                // GM module removed
                // if (showGenMatter)
                //     deDocketCompletedCount.AddRange(await _gmDueDateService.DueDateDeDockets.AsNoTracking()
                //         .Where(d => d.InstructionCompleted == true && d.CompletedDate != null && d.CompletedDate >= startDate && d.CompletedDate <= endDate)
                //         .GroupBy(grp => grp.CompletedDate!.Value.Month)
                //         .Select(d => new StackedChartViewModel() { Id = d.Key, StackLayer3 = d.Count() }).ToListAsync());
            }
            //Total number of completed - END

            //****************************************************************************
            //Order month list - fiscal year
            var monthList = new List<(int monthInt, int SortOrder)>();
            var sortedMonth = new List<(int monthInt, int SortOrder)>();
            monthList.Add((1, 0));
            monthList.Add((2, 0));
            monthList.Add((3, 0));
            monthList.Add((4, 0));
            monthList.Add((5, 0));
            monthList.Add((6, 0));
            monthList.Add((7, 0));
            monthList.Add((8, 0));
            monthList.Add((9, 0));
            monthList.Add((10, 0));
            monthList.Add((11, 0));
            monthList.Add((12, 0));

            var indx = startDate.Month;
            for (int x = 1; x <= 12; x++)
            {
                if (indx <= 12)
                {
                    var tempMonth = monthList[indx - 1];
                    tempMonth.SortOrder = x;
                    sortedMonth.Add(tempMonth);
                    indx++;
                }

                if (indx == 13) indx = 1;
            }

            //Parse data for return
            var totalTask = emptyVerTaskCount
                .Concat(verTaskCount)
                .Concat(docketRequestTaskCount)
                .Concat(deDocketTaskCount)
                .GroupBy(grp => grp.Id)
                .Select(d => new { Id = d.Key, TotalCount = d.Sum(s => s.StackLayer1) }).ToList(); ;
            var totalAssigned = docAssignedCount
                .Concat(docketRequestAssignedCount)
                .Concat(deDocketAssignedCount)
                .GroupBy(grp => grp.Id)
                .Select(d => new { Id = d.Key, TotalCount = d.Sum(s => s.StackLayer2) }).ToList();
            var totalCompleted = docCompletedCount
                .Concat(docketRequestCompletedCount)
                .Concat(deDocketCompletedCount)
                .GroupBy(grp => grp.Id)
                .Select(d => new { Id = d.Key, TotalCount = d.Sum(s => s.StackLayer3) }).ToList();
            foreach (var month in sortedMonth)
            {
                var monthData = new StackedChartViewModel()
                {
                    Id = month.monthInt,
                    Category = _localizer[System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(month.monthInt)]
                };
                monthData.StackLayer1 = totalTask.FirstOrDefault(d => d.Id == month.monthInt)?.TotalCount ?? 0;
                monthData.StackLayer2 = totalAssigned.FirstOrDefault(d => d.Id == month.monthInt)?.TotalCount ?? 0;
                monthData.StackLayer3 = totalCompleted.FirstOrDefault(d => d.Id == month.monthInt)?.TotalCount ?? 0;

                data.Add(monthData);
            }

            return Json(data);
        }

        /// <summary>
        /// Workload Management donut chart: (year to date) to show counts for number of task assigned, number of task completed, and number of outstanding tasks
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetWorkloadManagementChart([DataSourceRequest] DataSourceRequest request)
        {
            var settings = await _defaultSettings.GetSetting();
            var today = DateTime.Today;
            var startDate = today.FirstDayOfYear();
            var endDate = today.LastDayOfYear();

            if (!string.IsNullOrEmpty(settings.FiscalCalendarYearStart) && !string.IsNullOrEmpty(settings.FiscalCalendarYearEnd))
            {
                startDate = DateTime.ParseExact(settings.FiscalCalendarYearStart + "/" + DateTime.Today.Year.ToString(), "MM/dd/yyyy", CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(settings.FiscalCalendarYearEnd + "/" + DateTime.Today.Year.ToString(), "MM/dd/yyyy", CultureInfo.InvariantCulture);

                //Calculate FY
                if (startDate > endDate && today <= endDate)
                {
                    startDate = startDate.AddYears(-1);
                }
                else if (startDate > endDate && today > endDate)
                {
                    endDate = endDate.AddYears(1);
                }
            }

            var canAccessPatDocVer = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;
            var canAccessTmkDocVer = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;
            var canAccessGmDocVer = (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;

            var data = new List<ChartDTO>();

            //****************************************************************************
            //Total number of tasks - START
            //DocVerification
            var emptyVerDocTaskCount = await _docService.DocDocuments.AsNoTracking()
                .Where(d => d.IsVerified && d.IsActRequired && d.ActRequiredLastUpdate != null
                    && d.ActRequiredLastUpdate >= startDate && d.ActRequiredLastUpdate <= endDate
                    && (d.DocVerifications == null || !d.DocVerifications.Any())
                    && d.DocFolder != null
                    && ((d.DocFolder.SystemType == SystemTypeCode.Patent && d.DocFolder.ScreenCode == ScreenCode.Application
                            && !string.IsNullOrEmpty(d.DocFolder.DataKey) && d.DocFolder.DataKey.ToLower() == "appid"
                            && _countryApplicationService.CountryApplications.Any(c => c.AppId == d.DocFolder.DataKeyValue) && canAccessPatDocVer)
                        || (d.DocFolder.SystemType == SystemTypeCode.Trademark && d.DocFolder.ScreenCode == ScreenCode.Trademark
                            && !string.IsNullOrEmpty(d.DocFolder.DataKey) && d.DocFolder.DataKey.ToLower() == "tmkid"
                            && _tmkTrademarkService.TmkTrademarks.Any(t => t.TmkId == d.DocFolder.DataKeyValue) && canAccessTmkDocVer)
                        // GM module removed
                        // || (d.DocFolder.SystemType == SystemTypeCode.GeneralMatter && d.DocFolder.ScreenCode == ScreenCode.GeneralMatter
                        //     && !string.IsNullOrEmpty(d.DocFolder.DataKey) && d.DocFolder.DataKey.ToLower() == "matid"
                        //     && _gmMatterService.QueryableList.Any(g => g.MatId == d.DocFolder.DataKeyValue) && canAccessGmDocVer))
                        )
                ).CountAsync();

            var verDocTaskCount = await _docService.DocVerifications.AsNoTracking()
                .Where(d => d.DocDocument != null && d.DocDocument.IsVerified && d.DocDocument.IsActRequired && d.LastUpdate != null
                    && d.LastUpdate >= startDate && d.LastUpdate <= endDate
                    && (d.ActionTypeID > 0 || d.ActId > 0)
                    && d.DocDocument.DocFolder != null
                    && ((d.DocDocument.DocFolder.SystemType == SystemTypeCode.Patent && d.DocDocument.DocFolder.ScreenCode == ScreenCode.Application
                            && !string.IsNullOrEmpty(d.DocDocument.DocFolder.DataKey) && d.DocDocument.DocFolder.DataKey.ToLower() == "appid"
                            && _countryApplicationService.CountryApplications.Any(c => c.AppId == d.DocDocument.DocFolder.DataKeyValue) && canAccessPatDocVer)
                        || (d.DocDocument.DocFolder.SystemType == SystemTypeCode.Trademark && d.DocDocument.DocFolder.ScreenCode == ScreenCode.Trademark
                            && !string.IsNullOrEmpty(d.DocDocument.DocFolder.DataKey) && d.DocDocument.DocFolder.DataKey.ToLower() == "tmkid"
                            && _tmkTrademarkService.TmkTrademarks.Any(t => t.TmkId == d.DocDocument.DocFolder.DataKeyValue) && canAccessTmkDocVer)
                        // GM module removed
                        // || (d.DocDocument.DocFolder.SystemType == SystemTypeCode.GeneralMatter && d.DocDocument.DocFolder.ScreenCode == ScreenCode.GeneralMatter
                        //     && !string.IsNullOrEmpty(d.DocDocument.DocFolder.DataKey) && d.DocDocument.DocFolder.DataKey.ToLower() == "matid"
                        //     && _gmMatterService.QueryableList.Any(g => g.MatId == d.DocDocument.DocFolder.DataKeyValue) && canAccessGmDocVer))
                        )
                ).CountAsync();

            //DocketRequest
            int docketRequestTaskCount = 0;
            if (canAccessPatDocVer)
                docketRequestTaskCount += await _docketRequestService.PatDocketRequests.AsNoTracking()
                    .CountAsync(d => d.DateCreated != null && d.DateCreated >= startDate && d.DateCreated <= endDate);
            if (canAccessTmkDocVer)
                docketRequestTaskCount += await _docketRequestService.TmkDocketRequests.AsNoTracking()
                    .CountAsync(d => d.DateCreated != null && d.DateCreated >= startDate && d.DateCreated <= endDate);
            if (canAccessGmDocVer)
                docketRequestTaskCount += await _docketRequestService.GMDocketRequests.AsNoTracking()
                    .CountAsync(d => d.DateCreated != null && d.DateCreated >= startDate && d.DateCreated <= endDate);

            //DeDocket
            int deDocketTaskCount = 0;
            if (settings.IncludeDeDocketInVerification == true)
            {
                if (canAccessPatDocVer)
                    deDocketTaskCount += await _patDueDateService.DueDateDeDockets.AsNoTracking()
                        .CountAsync(d => d.InstructionDate != null && d.InstructionDate >= startDate && d.InstructionDate <= endDate);
                if (canAccessTmkDocVer)
                    deDocketTaskCount += await _tmkDueDateService.DueDateDeDockets.AsNoTracking()
                        .CountAsync(d => d.InstructionDate != null && d.InstructionDate >= startDate && d.InstructionDate <= endDate);
                // GM module removed
                // if (canAccessGmDocVer)
                //     deDocketTaskCount += await _gmDueDateService.DueDateDeDockets.AsNoTracking()
                //         .CountAsync(d => d.InstructionDate != null && d.InstructionDate >= startDate && d.InstructionDate <= endDate);
            }

            var totalTask = emptyVerDocTaskCount + verDocTaskCount + docketRequestTaskCount + deDocketTaskCount;
            //Total number of tasks for Docs - END

            //****************************************************************************
            //Total number of assigned - START
            //DocVerification
            var emptyVerDocAssignedCount = await _docService.DocRespDocketings.AsNoTracking()
                .Where(d => d.DocDocument != null && d.DocDocument.IsVerified && d.DocDocument.IsActRequired && d.LastUpdate != null
                    && d.LastUpdate >= startDate && d.LastUpdate <= endDate
                    && (d.DocDocument.DocVerifications == null || !d.DocDocument.DocVerifications.Any())
                    && d.DocDocument.DocFolder != null
                    && ((d.DocDocument.DocFolder.SystemType == SystemTypeCode.Patent && d.DocDocument.DocFolder.ScreenCode == ScreenCode.Application
                            && !string.IsNullOrEmpty(d.DocDocument.DocFolder.DataKey) && d.DocDocument.DocFolder.DataKey.ToLower() == "appid"
                            && _countryApplicationService.CountryApplications.Any(c => c.AppId == d.DocDocument.DocFolder.DataKeyValue) && canAccessPatDocVer)
                        || (d.DocDocument.DocFolder.SystemType == SystemTypeCode.Trademark && d.DocDocument.DocFolder.ScreenCode == ScreenCode.Trademark
                            && !string.IsNullOrEmpty(d.DocDocument.DocFolder.DataKey) && d.DocDocument.DocFolder.DataKey.ToLower() == "tmkid"
                            && _tmkTrademarkService.TmkTrademarks.Any(t => t.TmkId == d.DocDocument.DocFolder.DataKeyValue) && canAccessTmkDocVer)
                        // GM module removed
                        // || (d.DocDocument.DocFolder.SystemType == SystemTypeCode.GeneralMatter && d.DocDocument.DocFolder.ScreenCode == ScreenCode.GeneralMatter
                        //     && !string.IsNullOrEmpty(d.DocDocument.DocFolder.DataKey) && d.DocDocument.DocFolder.DataKey.ToLower() == "matid"
                        //     && _gmMatterService.QueryableList.Any(g => g.MatId == d.DocDocument.DocFolder.DataKeyValue) && canAccessGmDocVer))
                        )
                ).CountAsync();

            var verDocAssignedCount = _docService.DocRespDocketings.AsNoTracking()
                .Where(d => d.DocDocument != null && d.DocDocument.IsVerified && d.DocDocument.IsActRequired && d.LastUpdate != null
                    && d.LastUpdate >= startDate && d.LastUpdate <= endDate
                    && d.DocDocument.DocVerifications != null && d.DocDocument.DocVerifications.Count() > 0
                    && d.DocDocument.DocFolder != null
                    && ((d.DocDocument.DocFolder.SystemType == SystemTypeCode.Patent && d.DocDocument.DocFolder.ScreenCode == ScreenCode.Application
                            && !string.IsNullOrEmpty(d.DocDocument.DocFolder.DataKey) && d.DocDocument.DocFolder.DataKey.ToLower() == "appid"
                            && _countryApplicationService.CountryApplications.Any(c => c.AppId == d.DocDocument.DocFolder.DataKeyValue) && canAccessPatDocVer)
                        || (d.DocDocument.DocFolder.SystemType == SystemTypeCode.Trademark && d.DocDocument.DocFolder.ScreenCode == ScreenCode.Trademark
                            && !string.IsNullOrEmpty(d.DocDocument.DocFolder.DataKey) && d.DocDocument.DocFolder.DataKey.ToLower() == "tmkid"
                            && _tmkTrademarkService.TmkTrademarks.Any(t => t.TmkId == d.DocDocument.DocFolder.DataKeyValue) && canAccessTmkDocVer)
                        // GM module removed
                        // || (d.DocDocument.DocFolder.SystemType == SystemTypeCode.GeneralMatter && d.DocDocument.DocFolder.ScreenCode == ScreenCode.GeneralMatter
                        //     && !string.IsNullOrEmpty(d.DocDocument.DocFolder.DataKey) && d.DocDocument.DocFolder.DataKey.ToLower() == "matid"
                        //     && _gmMatterService.QueryableList.Any(g => g.MatId == d.DocDocument.DocFolder.DataKeyValue) && canAccessGmDocVer))
                        )
                )
                .Include(d => d.DocDocument).Include(d => d.DocDocument!.DocVerifications)
                .AsEnumerable()
                .Select(d => new { DocId = d.DocId, LastUpdate = d.LastUpdate, AssignedCount = d.DocDocument!.DocVerifications!.Count() })
                .GroupBy(grp => grp.DocId)
                .Select(d => new
                {
                    DocId = d.Key,
                    LastUpdate = d.Max(s => s.LastUpdate),
                    AssignedCount = d.Select(s => s.AssignedCount).FirstOrDefault()
                })
                .Sum(d => d.AssignedCount);

            //DocketRequest
            int docketRequestAssignedCount = 0;
            if (canAccessPatDocVer)
                docketRequestAssignedCount += await _docketRequestService.PatDocketRequestResps.AsNoTracking()
                    .CountAsync(d => d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate);
            if (canAccessTmkDocVer)
                docketRequestAssignedCount += await _docketRequestService.TmkDocketRequestResps.AsNoTracking()
                    .CountAsync(d => d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate);
            if (canAccessGmDocVer)
                docketRequestAssignedCount += await _docketRequestService.GMDocketRequestResps.AsNoTracking()
                    .CountAsync(d => d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate);

            //DeDocket
            int deDocketAssignedCount = 0;
            if (settings.IncludeDeDocketInVerification == true)
            {
                if (canAccessPatDocVer)
                    deDocketAssignedCount += await _patDueDateService.DueDateDeDocketResps.AsNoTracking()
                        .CountAsync(d => d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate);
                if (canAccessTmkDocVer)
                    deDocketAssignedCount += await _tmkDueDateService.DueDateDeDocketResps.AsNoTracking()
                        .CountAsync(d => d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate);
                // GM module removed
                // if (canAccessGmDocVer)
                //     deDocketAssignedCount += await _gmDueDateService.DueDateDeDocketResps.AsNoTracking()
                //         .CountAsync(d => d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate);
            }

            var totalAssigned = emptyVerDocAssignedCount + verDocAssignedCount + docketRequestAssignedCount + deDocketAssignedCount;
            //Total number of assigned - END

            //****************************************************************************
            //Total number of completed - START
            //DocVerification
            var docCompletedCount = await _docService.DocVerifications.AsNoTracking()
                .Where(d => d.DocId > 0 && d.DocDocument != null && d.DocDocument.IsVerified && d.DocDocument.IsActRequired && d.ActId > 0
                    && d.LastUpdate != null && d.LastUpdate >= startDate && d.LastUpdate <= endDate
                    && d.DocDocument.DocFolder != null
                    && ((d.DocDocument.DocFolder.SystemType == SystemTypeCode.Patent && d.DocDocument.DocFolder.ScreenCode == ScreenCode.Application
                            && !string.IsNullOrEmpty(d.DocDocument.DocFolder.DataKey) && d.DocDocument.DocFolder.DataKey.ToLower() == "appid"
                            && _countryApplicationService.CountryApplications.Any(c => c.AppId == d.DocDocument.DocFolder.DataKeyValue) && canAccessPatDocVer)
                        || (d.DocDocument.DocFolder.SystemType == SystemTypeCode.Trademark && d.DocDocument.DocFolder.ScreenCode == ScreenCode.Trademark
                            && !string.IsNullOrEmpty(d.DocDocument.DocFolder.DataKey) && d.DocDocument.DocFolder.DataKey.ToLower() == "tmkid"
                            && _tmkTrademarkService.TmkTrademarks.Any(t => t.TmkId == d.DocDocument.DocFolder.DataKeyValue) && canAccessTmkDocVer)
                        // GM module removed
                        // || (d.DocDocument.DocFolder.SystemType == SystemTypeCode.GeneralMatter && d.DocDocument.DocFolder.ScreenCode == ScreenCode.GeneralMatter
                        //     && !string.IsNullOrEmpty(d.DocDocument.DocFolder.DataKey) && d.DocDocument.DocFolder.DataKey.ToLower() == "matid"
                        //     && _gmMatterService.QueryableList.Any(g => g.MatId == d.DocDocument.DocFolder.DataKeyValue) && canAccessGmDocVer))
                        )
                ).CountAsync();

            //DocketRequest
            int docketRequestCompletedCount = 0;
            if (canAccessPatDocVer)
                docketRequestCompletedCount += await _docketRequestService.PatDocketRequests.AsNoTracking()
                    .CountAsync(d => d.CompletedDate != null && d.CompletedDate >= startDate && d.CompletedDate <= endDate);
            if (canAccessTmkDocVer)
                docketRequestCompletedCount += await _docketRequestService.TmkDocketRequests.AsNoTracking()
                    .CountAsync(d => d.CompletedDate != null && d.CompletedDate >= startDate && d.CompletedDate <= endDate);
            if (canAccessGmDocVer)
                docketRequestCompletedCount += await _docketRequestService.GMDocketRequests.AsNoTracking()
                    .CountAsync(d => d.CompletedDate != null && d.CompletedDate >= startDate && d.CompletedDate <= endDate);

            //DeDocket
            int deDocketCompletedCount = 0;
            if (settings.IncludeDeDocketInVerification == true)
            {
                if (canAccessPatDocVer)
                    deDocketCompletedCount += await _patDueDateService.DueDateDeDockets.AsNoTracking()
                        .CountAsync(d => d.InstructionCompleted == true && d.CompletedDate != null && d.CompletedDate >= startDate && d.CompletedDate <= endDate);
                if (canAccessTmkDocVer)
                    deDocketCompletedCount += await _tmkDueDateService.DueDateDeDockets.AsNoTracking()
                        .CountAsync(d => d.InstructionCompleted == true && d.CompletedDate != null && d.CompletedDate >= startDate && d.CompletedDate <= endDate);
                // GM module removed
                // if (canAccessGmDocVer)
                //     deDocketCompletedCount += await _gmDueDateService.DueDateDeDockets.AsNoTracking()
                //         .CountAsync(d => d.InstructionCompleted == true && d.CompletedDate != null && d.CompletedDate >= startDate && d.CompletedDate <= endDate);
            }

            var totalCompleted = docCompletedCount + docketRequestCompletedCount + deDocketCompletedCount;
            //Total number of completed = END

            //****************************************************************************
            //Total number of outstanding
            var totalOutstanding = totalTask - totalCompleted;

            //****************************************************************************
            //data.Add(new ChartDTO() { Category = _localizer["Number of Tasks Assigned"], Value = Decimal.Round(Convert.ToDecimal(totalAssigned * 100.00) / Convert.ToDecimal(totalTask), 2) });
            //data.Add(new ChartDTO() { Category = _localizer["Number of Tasks Completed"], Value = Decimal.Round(Convert.ToDecimal(totalCompleted * 100.00) / Convert.ToDecimal(totalTask), 2) });
            //data.Add(new ChartDTO() { Category = _localizer["Number of Tasks Outstanding"], Value = Decimal.Round(Convert.ToDecimal(totalOutstanding * 100.00) / Convert.ToDecimal(totalTask), 2) });
            data.Add(new ChartDTO() { Category = _localizer["Number of Tasks Assigned"], Value = totalAssigned });
            data.Add(new ChartDTO() { Category = _localizer["Number of Tasks Completed"], Value = totalCompleted });
            data.Add(new ChartDTO() { Category = _localizer["Number of Tasks Outstanding"], Value = totalOutstanding });

            return Json(data);
        }
        #endregion

        #endregion

        //3rd tab - all documents linked to a case, verified (IsVerified=1), action required checked (IsActRequired=1), and check actions checked and action due record exist
        #region Actions tab

        public async Task<IActionResult> ActionVerification_Read([DataSourceRequest] DataSourceRequest request, DocumentVerificationSearchCriteriaViewModel searchCriteria)
        {
            if (ModelState.IsValid)
            {
                var list = new DataSourceResult();
                //Keep sorting from stored proc
                request.Sorts.Clear();
                var data = await _docVerificationDTOService.GetDocVerificationActions(searchCriteria);
                if (data != null && data.Count > 0)
                {
                    data.ForEach(d =>
                    {
                        d.DocLibrary = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(d.System ?? "");
                    });

                    var dataModel = data.ToDataSourceResult(request);
                    var ids = data.Where(d => !string.IsNullOrEmpty(d.DocNames))
                                            .SelectMany(d => (d.DocNames ?? "").Split(";").Select(s => $"{d.System}|{s.Split("|")[0]}|{d.ParentId}|{d.ActId}|{s.Split("|")[2]}"))
                                            .Distinct().ToArray();

                    return Json(new
                    {
                        Data = dataModel.Data,
                        Total = dataModel.Total,
                        AggregateResult = dataModel.AggregateResults,
                        Errors = dataModel.Errors,
                        Ids = ids
                    });
                }

                list = data.ToDataSourceResult(request);
                return Json(list);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAllActionVerify(DateTime? verifiedDate, string selectedIds)
        {
            var userName = User.GetUserName();
            var temp = selectedIds.Split("|").Where(d => !string.IsNullOrEmpty(d)).ToList();

            var keyList = temp.Select(d =>
            {
                int keyId; string strVal = d;
                bool isInt = int.TryParse(d.Substring(1), out keyId);
                string systemType = d.Substring(0, 1);
                return new { systemType, keyId, strVal, isInt };
            }).Where(d => d.isInt).ToList();

            if (string.IsNullOrEmpty(selectedIds) || keyList == null || keyList.Count <= 0)
                return BadRequest();

            var actIds = keyList.Where(d => d.keyId > 0).Select(d => d.systemType + "|" + d.keyId.ToString()).ToList();

            if (actIds != null && actIds.Count > 0)
            {
                _docService.DetachAllEntities();
                await _docService.UpdateVerificationActionVerify(actIds, verifiedDate, userName);
                await UpdateDocVerified(actIds);
            }

            return Ok(new { message = _localizer["Action Verified Date has been saved successfully."].ToString(), userName });
        }

        private async Task UpdateDocVerified(List<string> keyIds)
        {
            var docList = new List<DocDocument>();
            foreach (var item in keyIds)
            {
                var keyArr = item.Split("|");
                var systemType = keyArr[0];
                var actId = keyArr[1];
                var keyId = 0;
                var temp = int.TryParse(actId, out keyId);

                if (keyId > 0)
                {
                    if (systemType.ToLower() == "p")
                    {
                        docList.AddRange(await _docService.DocDocuments.Where(d => d.DocVerifications != null && d.DocVerifications.Any(v => v.ActId == keyId)
                                                    && d.DocFolder != null
                                                    && (d.DocFolder.SystemType ?? "").ToLower() == SystemTypeCode.Patent.ToLower()
                                                    && (d.DocFolder.DataKey ?? "").ToLower() == "appid"
                                                    && (d.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Application.ToLower()
                                                ).Distinct().ToListAsync());
                    }
                    if (systemType.ToLower() == "t")
                    {
                        docList.AddRange(await _docService.DocDocuments.Where(d => d.DocVerifications != null && d.DocVerifications.Any(v => v.ActId == keyId)
                                                    && d.DocFolder != null
                                                    && (d.DocFolder.SystemType ?? "").ToLower() == SystemTypeCode.Trademark.ToLower()
                                                    && (d.DocFolder.DataKey ?? "").ToLower() == "tmkid"
                                                    && (d.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Trademark.ToLower()
                                                ).Distinct().ToListAsync());
                    }
                    if (systemType.ToLower() == "g")
                    {
                        docList.AddRange(await _docService.DocDocuments.Where(d => d.DocVerifications != null && d.DocVerifications.Any(v => v.ActId == keyId)
                                                    && d.DocFolder != null
                                                    && (d.DocFolder.SystemType ?? "").ToLower() == SystemTypeCode.GeneralMatter.ToLower()
                                                    && (d.DocFolder.DataKey ?? "").ToLower() == "matid"
                                                    && (d.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.GeneralMatter.ToLower()
                                                ).Distinct().ToListAsync());
                    }
                }
            }

            if (docList.Count > 0)
            {
                var userName = User.GetUserName();
                var settings = await _defaultSettings.GetSetting();
                var patSettings = await _patSettings.GetSetting();
                var tmkSettings = await _tmkSettings.GetSetting();
                foreach (var doc in docList)
                {
                    if (!doc.IsVerified)
                    {
                        doc.IsVerified = true;
                        doc.CheckAct = false;
                        //Move document to main default folder if currently on "Dockets for Verification" folder
                        var docFolder = await _docService.DocFolders.AsNoTracking().FirstOrDefaultAsync(d => d.FolderId == doc.FolderId);
                        if (docFolder != null)
                        {
                            var docVerificationFolderName = string.Empty;
                            switch (docFolder.SystemType)
                            {
                                case SystemTypeCode.Patent:
                                    docVerificationFolderName = patSettings.DocVerificationDefaultFolderName;
                                    break;
                                case SystemTypeCode.Trademark:
                                    docVerificationFolderName = tmkSettings.DocVerificationDefaultFolderName;
                                    break;
                                // GeneralMatter module removed
                            }

                            if (string.IsNullOrEmpty(docVerificationFolderName)) docVerificationFolderName = "Dockets for Verification";

                            if (docFolder.FolderName == docVerificationFolderName)
                            {
                                var documentLink = $"{docFolder.SystemType}|{docFolder.ScreenCode}|{docFolder.DataKey}|{docFolder.DataKeyValue.ToString()}";
                                var mainDefaultFolder = await _docViewModelService.GetOrAddDefaultFolder(documentLink);
                                doc.FolderId = mainDefaultFolder.FolderId;
                            }
                        }

                        await _docService.UpdateDocuments(userName, new List<DocDocument>() { doc }, new List<DocDocument>(), new List<DocDocument>());

                        if (settings.DocumentStorage == DocumentStorageOptions.SharePoint)
                        {
                            var graphClient = _sharePointService.GetGraphClient();
                            var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;

                            var driveItemId = await _docService.DocFiles.AsNoTracking().Where(d => d.FileId == doc.FileId).Select(d => d.DriveItemId).FirstOrDefaultAsync();
                            var docLibrary = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(docFolder != null ? docFolder.SystemType ?? "" : "");
                            var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();

                            if (list != null && !string.IsNullOrEmpty(driveItemId))
                            {
                                await graphClient.MarkVerified(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId, site.Id, list.Id);
                            }
                        }
                    }

                }
            }
        }

        public async Task<IActionResult> ActionDocumentZoom(string systemTypeCode, string docFileName, int parentId, int actId, int docId)
        {
            if (ImageHelper.IsUrl(docFileName))
                return Redirect(docFileName);

            var userName = User.GetUserName();
            DocVerificationActionDocZoomViewModel? actDocZoomVM = null;
            var docLibrary = string.Empty;
            var systemType = string.Empty;
            switch (systemTypeCode)
            {
                case SystemTypeCode.Patent:
                    actDocZoomVM = await _patActionDueService.QueryableList.AsNoTracking().Where(d => d.ActId == actId && d.AppId == parentId)
                                            .Select(d => new DocVerificationActionDocZoomViewModel()
                                            {
                                                ActId = d.ActId,
                                                ParentId = d.AppId,
                                                InvId = d.CountryApplication != null ? d.CountryApplication.InvId : 0,
                                                CaseNumber = d.CountryApplication != null ? d.CountryApplication.CaseNumber : "",
                                                Country = d.CountryApplication != null && d.CountryApplication.PatCountry != null ? d.CountryApplication.PatCountry.CountryName : "",
                                                SubCase = d.CountryApplication != null ? d.CountryApplication.SubCase : "",
                                                CaseType = d.CountryApplication != null ? d.CountryApplication.CaseType : "",
                                                Status = d.CountryApplication != null ? d.CountryApplication.ApplicationStatus : "",
                                                AppNumber = d.CountryApplication != null ? d.CountryApplication.AppNumber : "",
                                                FilDate = d.CountryApplication != null ? d.CountryApplication.FilDate : null,
                                                BaseDate = d.BaseDate,
                                                ActionType = d.ActionType,
                                                VerifiedBy = d.VerifiedBy,
                                                VerifierId = d.VerifierId,
                                                DateVerified = d.DateVerified,
                                                CreatedBy = d.CreatedBy,
                                                System = SystemType.Patent,
                                                ScreenCode = ScreenCode.Application,
                                                DocFileName = "",
                                                FileType = CPiSavedFileType.DocMgt
                                            }).FirstOrDefaultAsync();
                    docLibrary = SharePointDocLibrary.Patent;
                    systemType = SystemType.Patent;
                    if (actDocZoomVM != null)
                        actDocZoomVM.CanVerifyAction = actDocZoomVM.CreatedBy != userName && (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.DocumentVerificationModify)).Succeeded;

                    break;
                case SystemTypeCode.Trademark:
                    actDocZoomVM = await _tmkActionDueService.QueryableList.AsNoTracking().Where(d => d.ActId == actId && d.TmkId == parentId)
                                            .Select(d => new DocVerificationActionDocZoomViewModel()
                                            {
                                                ActId = d.ActId,
                                                ParentId = d.TmkId,
                                                CaseNumber = d.TmkTrademark != null ? d.TmkTrademark.CaseNumber : "",
                                                Country = d.TmkTrademark != null && d.TmkTrademark.TmkCountry != null ? d.TmkTrademark.TmkCountry.CountryName : "",
                                                SubCase = d.TmkTrademark != null ? d.TmkTrademark.SubCase : "",
                                                CaseType = d.TmkTrademark != null ? d.TmkTrademark.CaseType : "",
                                                Status = d.TmkTrademark != null ? d.TmkTrademark.TrademarkStatus : "",
                                                AppNumber = d.TmkTrademark != null ? d.TmkTrademark.AppNumber : "",
                                                FilDate = d.TmkTrademark != null ? d.TmkTrademark.FilDate : null,
                                                BaseDate = d.BaseDate,
                                                ActionType = d.ActionType,
                                                VerifiedBy = d.VerifiedBy,
                                                VerifierId = d.VerifierId,
                                                DateVerified = d.DateVerified,
                                                CreatedBy = d.CreatedBy,
                                                System = SystemType.Trademark,
                                                ScreenCode = ScreenCode.Trademark,
                                                DocFileName = "",
                                                FileType = CPiSavedFileType.DocMgt
                                            }).FirstOrDefaultAsync();
                    docLibrary = SharePointDocLibrary.Trademark;
                    systemType = SystemType.Trademark;
                    if (actDocZoomVM != null)
                        actDocZoomVM.CanVerifyAction = actDocZoomVM.CreatedBy != userName && (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.DocumentVerificationModify)).Succeeded;

                    break;
                // GM module removed - GeneralMatter case disabled
                // case SystemTypeCode.GeneralMatter:
                //     (entire GeneralMatter action document zoom block removed)
                //     break;
                default:
                    break;
            }

            Guard.Against.NoRecordPermission(actDocZoomVM != null);

            var settings = await _defaultSettings.GetSetting();
            if (actDocZoomVM != null && !string.IsNullOrEmpty(docFileName))
            {
                actDocZoomVM.DocName = await _docService.DocDocuments.AsNoTracking().Where(d => d.DocFile != null && (d.DocFile.DriveItemId == docFileName || d.DocFile.DocFileName == docFileName))
                    .Select(d => d.DocName).FirstOrDefaultAsync();

                if (settings.DocumentStorage == DocumentStorageOptions.SharePoint || settings.DocumentStorage == DocumentStorageOptions.iManage || settings.DocumentStorage == DocumentStorageOptions.NetDocuments)
                {
                    var tempFilePath = await PrepareTemporaryFile(docFileName, docLibrary);
                    if (!string.IsNullOrEmpty(tempFilePath))
                    {
                        ViewBag.FromTemp = true;
                        actDocZoomVM.DocFileName = tempFilePath;
                    }
                }
                else
                {
                    actDocZoomVM.DocFileName = _documentStorage.GetFilePath(systemType, docFileName, CPiSavedFileType.DocMgt);
                }
            }

            return PartialView("_ActDocumentZoom", actDocZoomVM);
        }

        [HttpPost]
        public IActionResult PrintActionDoc([FromBody] PrintViewModel docPrintViewModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(docPrintViewModel, ReportType.SharedDocVerificationActionDocPrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        [HttpPost]
        public async Task<IActionResult> ExportToExcelActionDoc([FromBody] string ids)
        {
            if (ModelState.IsValid)
            {
                var result = await _docVerificationDTOService.GetDocVerificationActionDocById(ids);
                var reportFields = result.GroupBy(d => d.ActId).Select(group =>
                {
                    var firstRecord = group.First();
                    var viewModel = _mapper.Map<DocVerificationActionDocPrintViewModel>(firstRecord);
                    var cultureInfo = Thread.CurrentThread.CurrentCulture;
                    viewModel.DueDates = string.Join("; ", group.OrderBy(dd => dd.DueDate).ThenBy(dd => dd.ActionDue).Select(dd => $"{dd.ActionDue}, {dd.DueDate?.ToString("dd-MMM-yyyy", cultureInfo)}, {dd.Indicator}"));
                    return viewModel;
                }).ToList();

                var fileStream = await _exportHelper.ListToExcelMemoryStream(reportFields, "List", _localizer, true, "", null, 50, false, null);
                return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), "DocketsForVerification.xlsx");
            }

            return BadRequest("Unhandled error.");
        }

        #endregion

        //4th tab - all verified (IsVerified=1) documents with "Forward document to client?" checked (SendToClient=1)
        #region Client Communications tab

        public async Task<IActionResult> Communication_Read([DataSourceRequest] DataSourceRequest request, DocumentVerificationSearchCriteriaViewModel searchCriteria)
        {
            if (ModelState.IsValid)
            {
                var list = new DataSourceResult();
                //Keep sorting from stored proc
                request.Sorts.Clear();
                var data = await _docVerificationDTOService.GetDocVerificationCommunications(searchCriteria);
                if (data != null && data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        item.DocLibrary = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(item.System ?? "");
                        item.CanUploadDocument = await CanUploadDocument(item.System ?? "", item.RespOffice ?? "");
                    }

                    var dataModel = data.OrderByDescending(o => o.UploadedDate).ToDataSourceResult(request);
                    var ids = data.Where(d => d.DocId > 0 || !string.IsNullOrEmpty(d.DriveItemId)).OrderByDescending(o => o.UploadedDate)
                                            .Select(d => $"{d.System}|{d.DocId}|{d.DriveItemId}|{d.ParentId}")
                                            .Distinct().ToArray();
                    return Json(new
                    {
                        Data = dataModel.Data,
                        Total = dataModel.Total,
                        AggregateResult = dataModel.AggregateResults,
                        Errors = dataModel.Errors,
                        Ids = ids
                    });
                }
                list = data.ToDataSourceResult(request);
                return Json(list);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<IActionResult> CommDocumentZoom(string systemTypeCode, int docId, string driveItemId, int parentId, bool isQELogLookup = false)
        {
            if (docId <= 0 && string.IsNullOrEmpty(driveItemId))
                return new NoRecordFoundResult();

            var commDocZoomVM = new DocVerificationCommDocZoomViewModel();
            QuickEmailScreenParameterViewModel? qeScreenParameterVM = new QuickEmailScreenParameterViewModel();
            var docLibrary = string.Empty;
            var systemType = string.Empty;
            switch (systemTypeCode)
            {
                case SystemTypeCode.Patent:
                    var application = await _countryApplicationService.CountryApplications.AsNoTracking()
                                    .Where(d => d.AppId == parentId)
                                    .Select(d => new { d.AppId, d.InvId, d.CaseNumber, d.Country, d.SubCase, d.RespOffice, d.AppTitle })
                                    .FirstOrDefaultAsync();
                    if (application != null)
                    {
                        docLibrary = SharePointDocLibrary.Patent;
                        systemType = SystemType.Patent;

                        commDocZoomVM.ParentId = parentId;
                        commDocZoomVM.InvId = application.InvId;
                        commDocZoomVM.System = SystemType.Patent;
                        commDocZoomVM.ScreenCode = ScreenCode.Application;
                        commDocZoomVM.FileType = CPiSavedFileType.DocMgt;

                        commDocZoomVM.CaseNumber = application.CaseNumber;
                        commDocZoomVM.Country = application.Country;
                        commDocZoomVM.SubCase = application.SubCase;
                        commDocZoomVM.Title = application.AppTitle;

                        var hasRespOfficeFilter = User.HasRespOfficeFilter(SystemType.Patent);
                        var canEmail = User.IsAdmin() || !(hasRespOfficeFilter ? (await _authService.AuthorizeAsync(User, application.RespOffice, PatentAuthorizationPolicy.LimitedReadByRespOffice)).Succeeded : (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.LimitedRead)).Succeeded);

                        if (canEmail)
                        {
                            qeScreenParameterVM.SystemType = SystemTypeCode.Patent;
                            qeScreenParameterVM.ScreenCode = "CA-QEmail";
                            qeScreenParameterVM.ParentScreenName = ScreenName.PatCountryApplication;
                            qeScreenParameterVM.ParentKey = "AppId";
                            qeScreenParameterVM.ParentId = parentId;
                            qeScreenParameterVM.ParentTable = "PC";
                            qeScreenParameterVM.IncludeImages = true;
                            qeScreenParameterVM.SendImmediately = false;
                            qeScreenParameterVM.AutoAttachImages = false;
                            qeScreenParameterVM.RoleLink = $"PI,{application.InvId}|PC,{parentId}";
                            qeScreenParameterVM.SharePointDocLibrary = SharePointDocLibrary.Patent;
                            qeScreenParameterVM.SharePointDocLibraryFolder = SharePointDocLibraryFolder.Application;
                            qeScreenParameterVM.SharePointRecKey = application.CaseNumber + "`" + application.Country + (string.IsNullOrEmpty(application.SubCase) ? "" : "~" + application.SubCase);
                            qeScreenParameterVM.IsPopup = true;
                        }
                    }
                    break;
                case SystemTypeCode.Trademark:
                    var trademark = await _tmkTrademarkService.TmkTrademarks.AsNoTracking()
                                    .Where(d => d.TmkId == parentId)
                                    .Select(d => new { d.TmkId, d.CaseNumber, d.Country, d.SubCase, d.RespOffice, d.TrademarkName })
                                    .FirstOrDefaultAsync();
                    if (trademark != null)
                    {
                        docLibrary = SharePointDocLibrary.Trademark;
                        systemType = SystemType.Trademark;

                        commDocZoomVM.ParentId = parentId;
                        commDocZoomVM.System = SystemType.Trademark;
                        commDocZoomVM.ScreenCode = ScreenCode.Trademark;
                        commDocZoomVM.FileType = CPiSavedFileType.DocMgt;

                        commDocZoomVM.CaseNumber = trademark.CaseNumber;
                        commDocZoomVM.Country = trademark.Country;
                        commDocZoomVM.SubCase = trademark.SubCase;
                        commDocZoomVM.Title = trademark.TrademarkName;

                        var hasRespOfficeFilter = User.HasRespOfficeFilter(SystemType.Trademark);
                        var canEmail = User.IsAdmin() || !(hasRespOfficeFilter ? (await _authService.AuthorizeAsync(User, trademark.RespOffice, TrademarkAuthorizationPolicy.LimitedReadByRespOffice)).Succeeded : (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.LimitedRead)).Succeeded);

                        if (canEmail)
                        {
                            qeScreenParameterVM.SystemType = SystemTypeCode.Trademark;
                            qeScreenParameterVM.ScreenCode = "Tmk-QEmail";
                            qeScreenParameterVM.ParentScreenName = ScreenName.TmkTrademark;
                            qeScreenParameterVM.ParentKey = "TmkId";
                            qeScreenParameterVM.ParentId = parentId;
                            qeScreenParameterVM.ParentTable = "TM";
                            qeScreenParameterVM.IncludeImages = true;
                            qeScreenParameterVM.SendImmediately = false;
                            qeScreenParameterVM.AutoAttachImages = false;
                            qeScreenParameterVM.RoleLink = $"TM,{parentId}";
                            qeScreenParameterVM.SharePointDocLibrary = SharePointDocLibrary.Trademark;
                            qeScreenParameterVM.SharePointDocLibraryFolder = SharePointDocLibraryFolder.Trademark;
                            qeScreenParameterVM.SharePointRecKey = trademark.CaseNumber + "`" + trademark.Country + (string.IsNullOrEmpty(trademark.SubCase) ? "" : "~" + trademark.SubCase);
                            qeScreenParameterVM.IsPopup = true;
                        }
                    }
                    break;
                // GM module removed - GeneralMatter case disabled
                // case SystemTypeCode.GeneralMatter:
                //     (entire GeneralMatter block removed)
                //     break;
                default:
                    break;
            }

            Guard.Against.NoRecordPermission(commDocZoomVM != null);

            var settings = await _defaultSettings.GetSetting();
            if (commDocZoomVM != null)
            {
                commDocZoomVM.SystemTypeCode = systemTypeCode;

                var docFileName = string.Empty;
                var docName = string.Empty;

                var docDocument = await _docService.DocDocuments.AsNoTracking()
                                .Where(d => d.DocFile != null && ((docId > 0 && d.DocId == docId) || (!string.IsNullOrEmpty(driveItemId) && d.DocFile.DriveItemId == driveItemId)))
                                .Select(d => new { d.DocId, d.DocName, DocFileName = d.DocFile != null ? d.DocFile.DocFileName : "" }).FirstOrDefaultAsync();

                if (docDocument != null)
                {
                    docFileName = docDocument.DocFileName;
                    docName = docDocument.DocName;
                    commDocZoomVM.DocId = docDocument.DocId;
                    qeScreenParameterVM.FileNames = docDocument.DocFileName;
                    qeScreenParameterVM.AutoAttachImages = true;
                }

                if (settings.DocumentStorage == DocumentStorageOptions.SharePoint)
                {
                    var graphClient = _sharePointService.GetGraphClient();
                    var existing = await graphClient.GetSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId);
                    if (existing != null) qeScreenParameterVM.FileNames = existing.Name;

                    var tempSPFilePath = await PrepareTemporaryFile(driveItemId, docLibrary);
                    if (!string.IsNullOrEmpty(tempSPFilePath))
                    {
                        ViewBag.FromTemp = true;
                        commDocZoomVM.DocFileName = tempSPFilePath;
                    }
                }
                else if (settings.DocumentStorage == DocumentStorageOptions.iManage || settings.DocumentStorage == DocumentStorageOptions.NetDocuments)
                {
                    qeScreenParameterVM.FileNames = driveItemId;
                    var tempIManageFilePath = await PrepareTemporaryFile(driveItemId, docLibrary);
                    if (!string.IsNullOrEmpty(tempIManageFilePath))
                    {
                        ViewBag.FromTemp = true;
                        commDocZoomVM.DocFileName = tempIManageFilePath;
                    }
                }
                else
                {
                    commDocZoomVM.DocFileName = _documentStorage.GetFilePath(systemType, docFileName ?? "", CPiSavedFileType.DocMgt);
                }

                commDocZoomVM.DocName = docName;

                if (qeScreenParameterVM != null) commDocZoomVM.jsonQEParam = JsonConvert.SerializeObject(qeScreenParameterVM);
            }

            if (!isQELogLookup)
                return PartialView("_CommDocumentZoom", commDocZoomVM);
            else
                return PartialView("_CommDocumentQELog", commDocZoomVM);
        }

        public async Task<IActionResult> CommDocumentQELogRead([DataSourceRequest] DataSourceRequest request, string systemTypeCode, int docId)
        {
            var screenCode = string.Empty;
            var systemType = string.Empty;
            var qeLogIds = new List<int>();

            if (docId > 0)
            {
                var docDocument = await _docService.DocDocuments.AsNoTracking().Where(d => d.DocId == docId).Select(d => new
                {
                    DocId = d.DocId,
                    SystemType = d.DocFolder != null ? d.DocFolder.SystemType : "",
                    ScreenCode = d.DocFolder != null ? d.DocFolder.ScreenCode : "",
                    DataKey = d.DocFolder != null ? d.DocFolder.DataKey : "",
                    DataKeyValue = d.DocFolder != null ? d.DocFolder.DataKeyValue : 0
                }).FirstOrDefaultAsync();

                if (docDocument != null)
                {
                    qeLogIds = await _docService.DocQuickEmailLogs.AsNoTracking().Where(d => d.DocDocument != null && d.DocDocument.DocId == docDocument.DocId).Select(d => d.LogID ?? 0).ToListAsync();
                    screenCode = docDocument.ScreenCode;
                    systemType = docDocument.SystemType;
                }
            }

            var docsOut = await _quickEmailService.QELogs.AsNoTracking().Where(d => qeLogIds.Contains(d.LogID) && d.SystemType == systemType)
                .Select(d => new DocsOutDTO()
                {
                    DocLogId = d.LogID,
                    Document = d.Subject,
                    DocumentCode = "QE",
                    DocumentType = "Quick Email",
                    GeneratedBy = d.GenBy,
                    GeneratedOn = d.GenDate,
                    RecKey = d.DataKeyValue,
                    ScreenCode = screenCode,
                    SystemType = systemType,
                    LogFile = @"Searchable\Logs\QuickEmails\" + d.QEFile,
                    ItemId = d.ItemId
                }).OrderByDescending(d => d.GeneratedOn).ToListAsync();

            var result = docsOut.ToDataSourceResult(request);

            return Json(result);
        }

        public async Task<IActionResult> GetQELogSharePointViewer(string system, string docFileName, string screenCode, int key, CPiSavedFileType fileType, string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return new NoRecordFoundResult();

            var tempFilePath = await PrepareTemporaryFile(itemId, SharePointDocLibrary.QELog, false);

            return RedirectToAction("GetDocumentViewer", "DocViewer", new { area = "Shared", system = system, docFileName = tempFilePath, screenCode = screenCode, key = key, fileType = fileType, isPartialView = true });
        }

        [HttpPost]
        public IActionResult PrintCommDoc([FromBody] PrintViewModel commDocPrintViewModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(commDocPrintViewModel, ReportType.SharedDocVerificationCommDocPrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        [HttpPost]
        public async Task<IActionResult> ExportToExcelCommDoc([FromBody] string ids)
        {
            if (ModelState.IsValid)
            {
                var result = await _docVerificationDTOService.GetDocVerificationCommunicationsDocById(ids);
                var reportFields = result.Select(d => _mapper.Map<DocVerificationCommDocPrintViewModel>(d)).ToList();
                var fileStream = await _exportHelper.ListToExcelMemoryStream(reportFields, "List", _localizer, true, "", null, 50, false, null);
                return File(fileStream.ToArray(), ImageHelper.GetContentType(".xlsx"), "DocumentsForSendingList.xlsx");
            }

            return BadRequest("Unhandled error.");
        }
        #endregion                

        #region Lookup        

        public async Task<IActionResult> GetDocVerificationLinkedDoc(string property, string text, FilterType filterType, string systemType)
        {
            var settings = await _defaultSettings.GetSetting();
            var docNames = new List<string>();
            docNames = await _docService.DocDocuments
                                .Where(d => d.DocName != null && d.IsActRequired
                                    && (d.DocVerifications != null && d.DocVerifications.Any(dv => dv.ActId != null || dv.ActionTypeID != null))
                                    && (systemType == null
                                           || (systemType.Contains(SystemTypeCode.Patent) && d.DocFolder != null && (d.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Application.ToLower() && (d.DocFolder.DataKey ?? "").ToLower() == "appid")
                                           || (systemType.Contains(SystemTypeCode.Trademark) && d.DocFolder != null && (d.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Trademark.ToLower() && (d.DocFolder.DataKey ?? "").ToLower() == "tmkid")
                                           || (systemType.Contains(SystemTypeCode.GeneralMatter) && d.DocFolder != null && (d.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.GeneralMatter.ToLower() && (d.DocFolder.DataKey ?? "").ToLower() == "matid"))
                                )
                                .Select(d => d.DocName ?? "").Distinct().ToListAsync();

            return Json(docNames.Where(d => !string.IsNullOrEmpty(d)).Select(d => new { DocName = d }).Distinct().OrderBy(o => o.DocName).ToList());
        }

        public async Task<IActionResult> GetDocVerificationVerifiedBy(string property, string text, FilterType filterType, string systemType)
        {
            var data = new List<string>();

            if (systemType.Contains(SystemTypeCode.Patent))
            {
                data.AddRange(await _patActionDueService.QueryableList.Where(d => !string.IsNullOrEmpty(d.VerifiedBy)).Select(d => d.VerifiedBy ?? "").ToListAsync());
            }
            if (systemType.Contains(SystemTypeCode.Trademark))
            {
                data.AddRange(await _tmkActionDueService.QueryableList.Where(d => !string.IsNullOrEmpty(d.VerifiedBy)).Select(d => d.VerifiedBy ?? "").ToListAsync());
            }
            // GM module removed
            // if (systemType.Contains(SystemTypeCode.GeneralMatter))
            // {
            //     data.AddRange(await _gmActionDueService.QueryableList.Where(d => !string.IsNullOrEmpty(d.VerifiedBy)).Select(d => d.VerifiedBy ?? "").ToListAsync());
            // }

            return Json(data.Where(d => !string.IsNullOrEmpty(d)).Select(d => new { VerifiedBy = d }).OrderBy(o => o.VerifiedBy).Distinct().ToList());
        }

        public async Task<IActionResult> GetDocVerificationActionType(string property, string text, FilterType filterType, string systemType, bool getDistinctOnly = false)
        {
            var defaultSettings = await _defaultSettings.GetSetting();

            var actions = new List<DocumentVerificationLookupViewModel>();
            var idList = new List<DocVerificationActionTypeListViewModel>();

            idList.AddRange(await _docService.DocVerifications
                                        .Where(d => (systemType.Contains(SystemTypeCode.Patent) && d.DocDocument != null && d.DocDocument.DocFolder != null
                                                        && (d.DocDocument.DocFolder.DataKey ?? "").ToLower() == "appid")
                                                || (systemType.Contains(SystemTypeCode.Trademark) && d.DocDocument != null && d.DocDocument.DocFolder != null
                                                        && (d.DocDocument.DocFolder.DataKey ?? "").ToLower() == "tmkid")
                                                || (systemType.Contains(SystemTypeCode.GeneralMatter) && d.DocDocument != null && d.DocDocument.DocFolder != null
                                                        && (d.DocDocument.DocFolder.DataKey ?? "").ToLower() == "matid")
                                            )
                                        .Select(d => new DocVerificationActionTypeListViewModel
                                        {
                                            ActId = d.ActId ?? 0,
                                            ActionTypeId = d.ActionTypeID ?? 0,
                                            SystemType = d.DocDocument != null && d.DocDocument.DocFolder != null ? d.DocDocument.DocFolder.SystemType : ""
                                        })
                                        .ToListAsync());

            if (systemType.Contains(SystemTypeCode.Patent))
            {
                var actIds = idList.Where(d => d.ActId > 0 && (d.SystemType ?? "").ToUpper() == SystemTypeCode.Patent).Select(d => d.ActId).ToList();
                var actionTypeIds = idList.Where(d => d.ActionTypeId > 0 && (d.SystemType ?? "").ToUpper() == SystemTypeCode.Patent).Select(d => d.ActionTypeId).ToList();

                actions.AddRange(await _patActionDueService.QueryableList.Where(d => actIds.Contains(d.ActId))
                                        .Select(d => new DocumentVerificationLookupViewModel
                                        {
                                            Id = "ActId|" + d.ActId.ToString(),
                                            ActionType = d.ActionType,
                                            BaseDate = d.BaseDate.ToString("dd-MMM-yyyy")
                                        })
                                        .ToListAsync());

                actions.AddRange(await _patActionTypeService.QueryableList.Where(d => actionTypeIds.Contains(d.ActionTypeID))
                                        .Select(d => new DocumentVerificationLookupViewModel
                                        {
                                            Id = "ActionTypeId|" + d.ActionTypeID.ToString(),
                                            ActionType = d.ActionType,
                                            BaseDate = ""
                                        })
                                        .ToListAsync());
            }

            if (systemType.Contains(SystemTypeCode.Trademark))
            {
                var actIds = idList.Where(d => d.ActId > 0 && (d.SystemType ?? "").ToUpper() == SystemTypeCode.Trademark).Select(d => d.ActId).ToList();
                var actionTypeIds = idList.Where(d => d.ActionTypeId > 0 && (d.SystemType ?? "").ToUpper() == SystemTypeCode.Trademark).Select(d => d.ActionTypeId).ToList();

                actions.AddRange(await _patActionDueService.QueryableList.Where(d => actIds.Contains(d.ActId))
                                        .Select(d => new DocumentVerificationLookupViewModel
                                        {
                                            Id = "ActId|" + d.ActId.ToString(),
                                            ActionType = d.ActionType,
                                            BaseDate = d.BaseDate.ToString("dd-MMM-yyyy")
                                        })
                                        .ToListAsync());

                actions.AddRange(await _patActionTypeService.QueryableList.Where(d => actionTypeIds.Contains(d.ActionTypeID))
                                        .Select(d => new DocumentVerificationLookupViewModel
                                        {
                                            Id = "ActionTypeId|" + d.ActionTypeID.ToString(),
                                            ActionType = d.ActionType,
                                            BaseDate = ""
                                        })
                                        .ToListAsync());
            }

            if (systemType.Contains(SystemTypeCode.GeneralMatter))
            {
                var actIds = idList.Where(d => d.ActId > 0 && (d.SystemType ?? "").ToUpper() == SystemTypeCode.GeneralMatter).Select(d => d.ActId).ToList();
                var actionTypeIds = idList.Where(d => d.ActionTypeId > 0 && (d.SystemType ?? "").ToUpper() == SystemTypeCode.GeneralMatter).Select(d => d.ActionTypeId).ToList();

                actions.AddRange(await _patActionDueService.QueryableList.Where(d => actIds.Contains(d.ActId))
                                        .Select(d => new DocumentVerificationLookupViewModel
                                        {
                                            Id = "ActId|" + d.ActId.ToString(),
                                            ActionType = d.ActionType,
                                            BaseDate = d.BaseDate.ToString("dd-MMM-yyyy")
                                        })
                                        .ToListAsync());

                actions.AddRange(await _patActionTypeService.QueryableList.Where(d => actionTypeIds.Contains(d.ActionTypeID))
                                        .Select(d => new DocumentVerificationLookupViewModel
                                        {
                                            Id = "ActionTypeId|" + d.ActionTypeID.ToString(),
                                            ActionType = d.ActionType,
                                            BaseDate = ""
                                        })
                                        .ToListAsync());
            }

            if (getDistinctOnly)
            {
                var filtered = actions.Select(d => d.ActionType).Distinct().Select(d => new DocumentVerificationLookupViewModel() { Id = "", ActionType = d, BaseDate = "" }).ToList();

                //include request docket types for filter on DocVerification screen only
                if (defaultSettings.IsDocketRequestOn)
                {
                    var docketRequestTypes = new List<string>();

                    if (systemType.Contains(SystemTypeCode.Patent))
                    {
                        docketRequestTypes.AddRange(await _docketRequestService.PatDocketRequests.AsNoTracking().Select(d => d.RequestType ?? "").Distinct().ToListAsync());
                    }
                    if (systemType.Contains(SystemTypeCode.Trademark))
                    {
                        docketRequestTypes.AddRange(await _docketRequestService.TmkDocketRequests.AsNoTracking().Select(d => d.RequestType ?? "").Distinct().ToListAsync());
                    }
                    if (systemType.Contains(SystemTypeCode.GeneralMatter))
                    {
                        docketRequestTypes.AddRange(await _docketRequestService.GMDocketRequests.AsNoTracking().Select(d => d.RequestType ?? "").Distinct().ToListAsync());
                    }

                    filtered.AddRange(docketRequestTypes.Select(d => new DocumentVerificationLookupViewModel() { Id = "", ActionType = d, BaseDate = "" }).Distinct().ToList());
                }

                //include action types from dedockets for filter on DocVerification screen only
                if (defaultSettings.IsDeDocketOn && defaultSettings.IncludeDeDocketInVerification)
                {
                    var deDocketActionTypes = new List<String>();

                    if (systemType.Contains(SystemTypeCode.Patent))
                    {
                        deDocketActionTypes.AddRange(await _patActionDueService.QueryableList.AsNoTracking()
                            .Where(d => d.DueDates != null && d.DueDates.Any(a => a.DueDateDeDockets != null && a.DueDateDeDockets.Any()))
                            .Select(d => d.ActionType ?? "").Distinct().ToListAsync());
                    }
                    if (systemType.Contains(SystemTypeCode.Trademark))
                    {
                        deDocketActionTypes.AddRange(await _tmkActionDueService.QueryableList.AsNoTracking()
                            .Where(d => d.DueDates != null && d.DueDates.Any(a => a.DueDateDeDockets != null && a.DueDateDeDockets.Any()))
                            .Select(d => d.ActionType ?? "").Distinct().ToListAsync());
                    }
                    // GM module removed
                    // if (systemType.Contains(SystemTypeCode.GeneralMatter))
                    // {
                    //     deDocketActionTypes.AddRange(await _gmActionDueService.QueryableList.AsNoTracking()
                    //         .Where(d => d.DueDates != null && d.DueDates.Any(a => a.DueDateDeDockets != null && a.DueDateDeDockets.Any()))
                    //         .Select(d => d.ActionType ?? "").Distinct().ToListAsync());
                    // }

                    filtered.AddRange(deDocketActionTypes.Select(d => new DocumentVerificationLookupViewModel() { Id = "", ActionType = d, BaseDate = "" }).Distinct().ToList());
                }

                return Json(filtered.Distinct().OrderBy(o => o.ActionType).ToList());
            }

            return Json(actions.Distinct().OrderBy(o => o.ActionType).ToList());
        }

        public async Task<IActionResult> GetDocumentNames([DataSourceRequest] DataSourceRequest request, string systemType)
        {
            var docNames = new List<string>();
            docNames.AddRange(await _docService.DocDocuments.Where(d => d.DocName != null
                                        && (
                                            //1st tab
                                            d.FolderId == 0
                                            //2nd, 3rd, and 4th tab
                                            || (((!d.IsVerified && !d.IsActRequired)
                                                    || (!d.IsVerified && d.IsActRequired)
                                                    || (d.IsVerified && d.IsActRequired)
                                                    || (d.IsVerified && d.SendToClient))
                                                && (systemType == null
                                                || (systemType.Contains(SystemTypeCode.Patent) && d.DocFolder != null && (d.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Application.ToLower()
                                                        && (d.DocFolder.DataKey ?? "").ToLower() == "appid")
                                                || (systemType.Contains(SystemTypeCode.Trademark) && d.DocFolder != null && (d.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Trademark.ToLower()
                                                        && (d.DocFolder.DataKey ?? "").ToLower() == "tmkid")
                                                || (systemType.Contains(SystemTypeCode.GeneralMatter) && d.DocFolder != null && (d.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.GeneralMatter.ToLower()
                                                        && (d.DocFolder.DataKey ?? "").ToLower() == "matid")))
                                            )
                                    )
                                    .Select(d => d.DocName ?? "").Distinct().ToListAsync());

            var settings = await _defaultSettings.GetSetting();
            if (settings.IsDocketRequestOn)
            {
                if (systemType.Contains(SystemTypeCode.Patent))
                    docNames.AddRange(await _docService.DocFiles.AsNoTracking().Where(d => _docketRequestService.PatDocketRequests.Any(r => r.FileId == d.FileId)).Select(d => d.UserFileName ?? "").ToListAsync());

                if (systemType.Contains(SystemTypeCode.Trademark))
                    docNames.AddRange(await _docService.DocFiles.AsNoTracking().Where(d => _docketRequestService.TmkDocketRequests.Any(r => r.FileId == d.FileId)).Select(d => d.UserFileName ?? "").ToListAsync());

                if (systemType.Contains(SystemTypeCode.GeneralMatter))
                    docNames.AddRange(await _docService.DocFiles.AsNoTracking().Where(d => _docketRequestService.GMDocketRequests.Any(r => r.FileId == d.FileId)).Select(d => d.UserFileName ?? "").ToListAsync());
            }

            if (settings.IsDeDocketOn && settings.IncludeDeDocketInVerification)
            {
                if (systemType.Contains(SystemTypeCode.Patent))
                    docNames.AddRange(await _docService.DocFiles.AsNoTracking().Where(d => _patDueDateService.DueDateDeDockets.Any(r => r.FileId == d.FileId)).Select(d => d.UserFileName ?? "").ToListAsync());

                if (systemType.Contains(SystemTypeCode.Trademark))
                    docNames.AddRange(await _docService.DocFiles.AsNoTracking().Where(d => _tmkDueDateService.DueDateDeDockets.Any(r => r.FileId == d.FileId)).Select(d => d.UserFileName ?? "").ToListAsync());

                // GM module removed
                // if (systemType.Contains(SystemTypeCode.GeneralMatter))
                //     docNames.AddRange(await _docService.DocFiles.AsNoTracking().Where(d => _gmDueDateService.DueDateDeDockets.Any(r => r.FileId == d.FileId)).Select(d => d.UserFileName ?? "").ToListAsync());
            }

            return Json(docNames.Select(d => new { DocName = d }).Distinct().OrderBy(o => o.DocName));
        }

        public async Task<IActionResult> GetDocumentUploadedBy(string property, string text, FilterType filterType, string systemType)
        {
            var uploadedBys = new List<string>();
            uploadedBys.AddRange(await _docService.DocDocuments.Where(d => d.DocName != null
                                        && ((systemType == null
                                                    || (systemType.Contains(SystemTypeCode.Patent) && d.DocFolder != null && (d.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Application.ToLower()
                                                            && (d.DocFolder.DataKey ?? "").ToLower() == "appid")
                                                    || (systemType.Contains(SystemTypeCode.Trademark) && d.DocFolder != null && (d.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Trademark.ToLower()
                                                            && (d.DocFolder.DataKey ?? "").ToLower() == "tmkid")
                                                    || (systemType.Contains(SystemTypeCode.GeneralMatter) && d.DocFolder != null && (d.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.GeneralMatter.ToLower()
                                                            && (d.DocFolder.DataKey ?? "").ToLower() == "matid"))
                                            || d.FolderId == 0)
                                        )
                                        .Select(d => d.DocFile != null ? d.DocFile.UpdatedBy ?? "" : "").Distinct().ToListAsync());

            var settings = await _defaultSettings.GetSetting();
            if (settings.IsDocketRequestOn)
            {
                if (systemType.Contains(SystemTypeCode.Patent))
                    uploadedBys.AddRange(await _docService.DocFiles.AsNoTracking().Where(d => _docketRequestService.PatDocketRequests.Any(r => r.FileId == d.FileId)).Select(d => d.UpdatedBy ?? "").ToListAsync());

                if (systemType.Contains(SystemTypeCode.Trademark))
                    uploadedBys.AddRange(await _docService.DocFiles.AsNoTracking().Where(d => _docketRequestService.TmkDocketRequests.Any(r => r.FileId == d.FileId)).Select(d => d.UpdatedBy ?? "").ToListAsync());

                if (systemType.Contains(SystemTypeCode.GeneralMatter))
                    uploadedBys.AddRange(await _docService.DocFiles.AsNoTracking().Where(d => _docketRequestService.GMDocketRequests.Any(r => r.FileId == d.FileId)).Select(d => d.UpdatedBy ?? "").ToListAsync());
            }

            if (settings.IsDeDocketOn && settings.IncludeDeDocketInVerification)
            {
                if (systemType.Contains(SystemTypeCode.Patent))
                    uploadedBys.AddRange(await _docService.DocFiles.AsNoTracking().Where(d => _patDueDateService.DueDateDeDockets.Any(r => r.FileId == d.FileId)).Select(d => d.UpdatedBy ?? "").ToListAsync());

                if (systemType.Contains(SystemTypeCode.Trademark))
                    uploadedBys.AddRange(await _docService.DocFiles.AsNoTracking().Where(d => _tmkDueDateService.DueDateDeDockets.Any(r => r.FileId == d.FileId)).Select(d => d.UpdatedBy ?? "").ToListAsync());

                // GM module removed
                // if (systemType.Contains(SystemTypeCode.GeneralMatter))
                //     uploadedBys.AddRange(await _docService.DocFiles.AsNoTracking().Where(d => _gmDueDateService.DueDateDeDockets.Any(r => r.FileId == d.FileId)).Select(d => d.UpdatedBy ?? "").ToListAsync());
            }

            return Json(uploadedBys.Where(d => !string.IsNullOrEmpty(d)).Select(d => new { UpdatedBy = d }).Distinct().OrderBy(o => o.UpdatedBy).ToList());
        }

        public async Task<IActionResult> GetActionCreatedBy(string property, string text, FilterType filterType, string systemType)
        {
            var actCreatedBys = new List<string>();
            var actIdList = new List<DocVerificationActionTypeListViewModel>();

            actIdList.AddRange(await _docService.DocVerifications.Where(d => d.ActId > 0
                                    && d.DocDocument != null && d.DocDocument.IsActRequired
                                    && (systemType == null
                                            || (systemType.Contains(SystemTypeCode.Patent) && d.DocDocument != null && d.DocDocument.DocFolder != null
                                                    && (d.DocDocument.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Application.ToLower() && (d.DocDocument.DocFolder.DataKey ?? "").ToLower() == "appid")
                                            || (systemType.Contains(SystemTypeCode.Trademark) && d.DocDocument != null && d.DocDocument.DocFolder != null
                                                    && (d.DocDocument.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Trademark.ToLower() && (d.DocDocument.DocFolder.DataKey ?? "").ToLower() == "tmkid")
                                            || (systemType.Contains(SystemTypeCode.GeneralMatter) && d.DocDocument != null && d.DocDocument.DocFolder != null
                                                    && (d.DocDocument.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.GeneralMatter.ToLower() && (d.DocDocument.DocFolder.DataKey ?? "").ToLower() == "matid")
                                        )
                                    ).Select(d => new DocVerificationActionTypeListViewModel
                                    {
                                        ActId = d.ActId ?? 0,
                                        SystemType = d.DocDocument != null && d.DocDocument.DocFolder != null ? d.DocDocument.DocFolder.SystemType : ""
                                    })
                                    .Distinct().ToListAsync());


            if (systemType.Contains(SystemTypeCode.Patent))
            {
                var patActIds = actIdList.Where(d => (d.SystemType ?? "").ToUpper() == SystemTypeCode.Patent).Select(d => d.ActId).ToList();
                actCreatedBys.AddRange(await _patActionDueService.QueryableList.Where(d => patActIds.Contains(d.ActId)).Select(d => d.CreatedBy ?? "").ToListAsync());
            }
            if (systemType.Contains(SystemTypeCode.Trademark))
            {
                var tmkActIds = actIdList.Where(d => (d.SystemType ?? "").ToUpper() == SystemTypeCode.Trademark).Select(d => d.ActId).ToList();
                actCreatedBys.AddRange(await _tmkActionDueService.QueryableList.Where(d => tmkActIds.Contains(d.ActId)).Select(d => d.CreatedBy ?? "").ToListAsync());
            }
            // GM module removed
            // if (systemType.Contains(SystemTypeCode.GeneralMatter))
            // {
            //     var gmActIds = actIdList.Where(d => (d.SystemType ?? "").ToUpper() == SystemTypeCode.GeneralMatter).Select(d => d.ActId).ToList();
            //     actCreatedBys.AddRange(await _gmActionDueService.QueryableList.Where(d => gmActIds.Contains(d.ActId)).Select(d => d.CreatedBy ?? "").ToListAsync());
            // }

            return Json(actCreatedBys.Where(d => !string.IsNullOrEmpty(d)).Select(d => new { CreatedBy = d }).Distinct().OrderBy(o => o.CreatedBy).ToList());
        }

        public async Task<IActionResult> GetRespDocketingList(string property, string text, FilterType filterType, string systemType)
        {
            var defaultSettings = await _defaultSettings.GetSetting();
            var userIds = new List<string>();
            var groupIds = new List<int>();

            //////////////////////////////////////////////////
            //tblDocRespDocketing
            var responsibleIds = await _docService.DocRespDocketings
                                .Where(d => d.DocDocument != null && d.DocDocument.DocId > 0
                                    && ((d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0))
                                    && (string.IsNullOrEmpty(systemType)
                                        || (systemType.Contains(SystemTypeCode.Patent) && d.DocDocument != null && d.DocDocument.DocFolder != null
                                                && (d.DocDocument.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Application.ToLower() && (d.DocDocument.DocFolder.DataKey ?? "").ToLower() == "appid")
                                        || (systemType.Contains(SystemTypeCode.Trademark) && d.DocDocument != null && d.DocDocument.DocFolder != null
                                                && (d.DocDocument.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Trademark.ToLower() && (d.DocDocument.DocFolder.DataKey ?? "").ToLower() == "tmkid")
                                        || (systemType.Contains(SystemTypeCode.GeneralMatter) && d.DocDocument != null && d.DocDocument.DocFolder != null
                                                && (d.DocDocument.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.GeneralMatter.ToLower() && (d.DocDocument.DocFolder.DataKey ?? "").ToLower() == "matid")
                                        )
                                ).Select(d => new { UserId = d.UserId, GroupId = d.GroupId }).ToListAsync();
            //////////////////////////////////////////////////
            //tbl_DocketRequestResp
            if (defaultSettings.IsDocketRequestOn)
            {
                if (string.IsNullOrEmpty(systemType) || systemType.Contains(SystemTypeCode.Patent))
                    responsibleIds.AddRange(await _docketRequestService.PatDocketRequestResps.AsNoTracking()
                                                .Where(d => (d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0))
                                                .Select(d => new { UserId = d.UserId, GroupId = d.GroupId }).ToListAsync());
                if (string.IsNullOrEmpty(systemType) || systemType.Contains(SystemTypeCode.Trademark))
                    responsibleIds.AddRange(await _docketRequestService.TmkDocketRequestResps.AsNoTracking()
                                                .Where(d => (d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0))
                                               .Select(d => new { UserId = d.UserId, GroupId = d.GroupId }).ToListAsync());

                if (string.IsNullOrEmpty(systemType) || systemType.Contains(SystemTypeCode.GeneralMatter))
                    responsibleIds.AddRange(await _docketRequestService.GMDocketRequestResps.AsNoTracking()
                                                .Where(d => (d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0))
                                                .Select(d => new { UserId = d.UserId, GroupId = d.GroupId }).ToListAsync());
            }

            //////////////////////////////////////////////////
            //tbl_DueDateDeDocketResp
            if (defaultSettings.IsDeDocketOn && defaultSettings.IncludeDeDocketInVerification)
            {
                if (string.IsNullOrEmpty(systemType) || systemType.Contains(SystemTypeCode.Patent))
                    responsibleIds.AddRange(await _patDueDateService.DueDateDeDocketResps.AsNoTracking()
                                                .Where(d => (d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0))
                                                .Select(d => new { UserId = d.UserId, GroupId = d.GroupId }).ToListAsync());
                if (string.IsNullOrEmpty(systemType) || systemType.Contains(SystemTypeCode.Trademark))
                    responsibleIds.AddRange(await _tmkDueDateService.DueDateDeDocketResps.AsNoTracking()
                                                .Where(d => (d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0))
                                                .Select(d => new { UserId = d.UserId, GroupId = d.GroupId }).ToListAsync());

                // GM module removed
                // if (string.IsNullOrEmpty(systemType) || systemType.Contains(SystemTypeCode.GeneralMatter))
                //     responsibleIds.AddRange(await _gmDueDateService.DueDateDeDocketResps.AsNoTracking()
                //                                 .Where(d => (d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0))
                //                                 .Select(d => new { UserId = d.UserId, GroupId = d.GroupId }).ToListAsync());
            }

            //////////////////////////////////////////////////

            if (responsibleIds != null && responsibleIds.Count > 0)
            {
                userIds.AddRange(responsibleIds.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId ?? "").Distinct().ToList());
                groupIds.AddRange(responsibleIds.Where(d => d.GroupId > 0).Select(d => d.GroupId ?? 0).Distinct().ToList());
            }

            var responsibleList = _userManager.Users
                                        .Select(d => new { d.Id, d.FirstName, d.LastName })
                                        .AsEnumerable()
                                        .Where(d => userIds.Contains(d.Id))
                                        .Select(d => new { Responsible = d.FirstName + " " + d.LastName, ResponsibleId = d.Id.ToString() }).ToList();

            responsibleList.AddRange(_groupManager.QueryableList
                                        .Select(d => new { d.Id, d.Name })
                                        .AsEnumerable()
                                        .Where(d => groupIds.Contains(d.Id))
                                        .Select(d => new { Responsible = d.Name, ResponsibleId = d.Id.ToString() }).ToList());

            //Add current user
            var userId = User.GetUserIdentifier();
            var userName = User.GetFullName();
            if (!responsibleList.Any(d => d.ResponsibleId == userId.ToString()))
                responsibleList.Add(new { Responsible = userName, ResponsibleId = userId });

            return Json(responsibleList.OrderBy(o => o.Responsible).ToList());
        }

        public async Task<IActionResult> GetRespReportingList(string property, string text, FilterType filterType, string systemType)
        {
            var userIds = new List<string>();
            var groupIds = new List<int>();

            var responsibleIds = await _docService.DocRespReportings
                                        .Where(d => d.DocDocument != null && d.DocDocument.DocId > 0
                                            && ((d.UserId != "" && d.UserId != null) || (d.GroupId != null && d.GroupId > 0))
                                            && (systemType == null
                                                || (systemType.Contains(SystemTypeCode.Patent) && d.DocDocument != null && d.DocDocument.DocFolder != null
                                                        && (d.DocDocument.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Application.ToLower() && (d.DocDocument.DocFolder.DataKey ?? "").ToLower() == "appid")
                                                || (systemType.Contains(SystemTypeCode.Trademark) && d.DocDocument != null && d.DocDocument.DocFolder != null
                                                        && (d.DocDocument.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Trademark.ToLower() && (d.DocDocument.DocFolder.DataKey ?? "").ToLower() == "tmkid")
                                                || (systemType.Contains(SystemTypeCode.GeneralMatter) && d.DocDocument != null && d.DocDocument.DocFolder != null
                                                        && (d.DocDocument.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.GeneralMatter.ToLower() && (d.DocDocument.DocFolder.DataKey ?? "").ToLower() == "matid")
                                                )
                                        )
                                        .Select(d => new { UserId = d.UserId, GroupId = d.GroupId }).ToListAsync();

            if (responsibleIds != null && responsibleIds.Count > 0)
            {
                userIds.AddRange(responsibleIds.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId ?? "").Distinct().ToList());
                groupIds.AddRange(responsibleIds.Where(d => d.GroupId > 0).Select(d => d.GroupId ?? 0).Distinct().ToList());
            }

            var responsibleList = _userManager.Users
                                        .Select(d => new { d.Id, d.FirstName, d.LastName })
                                        .AsEnumerable()
                                        .Where(d => userIds.Contains(d.Id))
                                        .Select(d => new { Responsible = d.FirstName + " " + d.LastName, ResponsibleId = d.Id.ToString() }).ToList();

            responsibleList.AddRange(_groupManager.QueryableList
                                        .Select(d => new { d.Id, d.Name })
                                        .AsEnumerable()
                                        .Where(d => groupIds.Contains(d.Id))
                                        .Select(d => new { Responsible = d.Name, ResponsibleId = d.Id.ToString() }).ToList());

            //Add current user
            var userId = User.GetUserIdentifier();
            var userName = User.GetFullName();
            if (!responsibleList.Any(d => d.ResponsibleId == userId.ToString()))
                responsibleList.Add(new { Responsible = userName, ResponsibleId = userId });

            return Json(responsibleList.OrderBy(o => o.Responsible).ToList());
        }

        public async Task<IActionResult> GetCountryList(string property, string text, FilterType filterType, string systemType)
        {
            var countryList = new List<SelectListItem>();

            var parentIds = await _docService.DocFolders.AsNoTracking().Where(d => !string.IsNullOrEmpty(d.DataKey)
                && (string.IsNullOrEmpty(systemType)
                || (systemType.Contains(SystemTypeCode.Patent) && d.SystemType == SystemTypeCode.Patent && d.ScreenCode == ScreenCode.Application && d.DataKey.ToLower() == "appid")
                || (systemType.Contains(SystemTypeCode.Trademark) && d.SystemType == SystemTypeCode.Trademark && d.ScreenCode == ScreenCode.Trademark && d.DataKey.ToLower() == "tmkid")
                || (systemType.Contains(SystemTypeCode.GeneralMatter) && d.SystemType == SystemTypeCode.GeneralMatter && d.ScreenCode == ScreenCode.GeneralMatter && d.DataKey.ToLower() == "matid")))
                .Select(d => new { d.DataKey, d.DataKeyValue }).Distinct().ToListAsync();

            var appIds = parentIds.Where(d => !string.IsNullOrEmpty(d.DataKey) && d.DataKey.ToLower() == "appid").Select(d => d.DataKeyValue).Distinct().ToList();

            var tmkIds = parentIds.Where(d => !string.IsNullOrEmpty(d.DataKey) && d.DataKey.ToLower() == "tmkid").Select(d => d.DataKeyValue).Distinct().ToList();

            var matIds = parentIds.Where(d => !string.IsNullOrEmpty(d.DataKey) && d.DataKey.ToLower() == "matid").Select(d => d.DataKeyValue).Distinct().ToList();

            countryList.AddRange(await _countryApplicationService.CountryApplications.AsNoTracking().Where(d => appIds.Contains(d.AppId))
                .Select(d => new SelectListItem() { Text = d.Country, Value = d.PatCountry != null ? d.PatCountry.CountryName : "" }).ToListAsync());
            countryList.AddRange(await _tmkTrademarkService.TmkTrademarks.AsNoTracking().Where(d => tmkIds.Contains(d.TmkId))
                .Select(d => new SelectListItem() { Text = d.Country, Value = d.TmkCountry != null ? d.TmkCountry.CountryName : "" }).ToListAsync());
            // GM module removed
            // countryList.AddRange(await _gmMatterCountryService.QueryableList.AsNoTracking().Where(d => matIds.Contains(d.MatId))
            //     .Select(d => new SelectListItem() { Text = d.Country, Value = d.GMCountry != null ? d.GMCountry.CountryName : "" }).ToListAsync());

            var settings = await _defaultSettings.GetSetting();
            if (settings.IsDocketRequestOn)
            {
                if (systemType.Contains(SystemTypeCode.Patent))
                    countryList.AddRange(await _docketRequestService.PatDocketRequests.AsNoTracking()
                        .Where(d => d.CountryApplication != null && d.CountryApplication.PatCountry != null)
                        .Select(d => new SelectListItem()
                        {
                            Text = d.CountryApplication!.PatCountry!.Country,
                            Value = d.CountryApplication.PatCountry.CountryName
                        }).Distinct().ToListAsync());

                if (systemType.Contains(SystemTypeCode.Trademark))
                    countryList.AddRange(await _docketRequestService.TmkDocketRequests.AsNoTracking()
                        .Where(d => d.TmkTrademark != null && d.TmkTrademark.TmkCountry != null)
                        .Select(d => new SelectListItem()
                        {
                            Text = d.TmkTrademark!.TmkCountry!.Country,
                            Value = d.TmkTrademark.TmkCountry.CountryName
                        }).Distinct().ToListAsync());

                // GM module removed
                // if (systemType.Contains(SystemTypeCode.GeneralMatter))
                //     countryList.AddRange(await _gmMatterCountryService.QueryableList.AsNoTracking()
                //         .Where(d => d.GMMatter != null && d.GMMatter.GMDocketRequests != null && d.GMMatter.GMDocketRequests.Any() && d.GMCountry != null)
                //         .Select(d => new SelectListItem()
                //         {
                //             Text = d.GMCountry!.Country,
                //             Value = d.GMCountry.CountryName
                //         }).Distinct().ToListAsync());
            }

            if (settings.IsDeDocketOn && settings.IncludeDeDocketInVerification)
            {
                if (systemType.Contains(SystemTypeCode.Patent))
                    countryList.AddRange(await _patActionDueService.QueryableList.AsNoTracking()
                        .Where(d => d.CountryApplication != null && d.CountryApplication.PatCountry != null && d.DueDates != null && d.DueDates.Any(a => a.DueDateDeDockets != null && a.DueDateDeDockets.Any()))
                        .Select(d => new SelectListItem()
                        {
                            Text = d.CountryApplication!.PatCountry!.Country,
                            Value = d.CountryApplication.PatCountry.CountryName
                        }).Distinct().ToListAsync());

                if (systemType.Contains(SystemTypeCode.Trademark))
                    countryList.AddRange(await _tmkActionDueService.QueryableList.AsNoTracking()
                        .Where(d => d.TmkTrademark != null && d.TmkTrademark.TmkCountry != null && d.DueDates != null && d.DueDates.Any(a => a.DueDateDeDockets != null && a.DueDateDeDockets.Any()))
                        .Select(d => new SelectListItem()
                        {
                            Text = d.TmkTrademark!.TmkCountry!.Country,
                            Value = d.TmkTrademark.TmkCountry.CountryName
                        }).Distinct().ToListAsync());

                // GM module removed
                // if (systemType.Contains(SystemTypeCode.GeneralMatter))
                //     countryList.AddRange(await _gmMatterCountryService.QueryableList.AsNoTracking()
                //         .Where(d => d.GMMatter != null && d.GMCountry != null && d.GMMatter.ActionsDue != null && d.GMMatter.ActionsDue.Any(a => a.DueDates != null && a.DueDates.Any(s => s.DueDateDeDockets != null && s.DueDateDeDockets.Any())))
                //         .Select(d => new SelectListItem()
                //         {
                //             Text = d.GMCountry!.Country,
                //             Value = d.GMCountry.CountryName
                //         }).Distinct().ToListAsync());
            }

            return Json(countryList.Select(c => new { Country = c.Text, CountryName = c.Value }).DistinctBy(c => new { c.Country, c.CountryName })
                .Where(c => !String.IsNullOrEmpty(c.Country)).OrderBy(c => c.Country).ToList());
        }

        public async Task<IActionResult> GetSourceList(string property, string text, FilterType filterType, string systemType)
        {
            var sourceList = await _docService.DocDocuments.AsNoTracking().Where(d => d.DocFolder != null && !string.IsNullOrEmpty(d.Source)
                && !string.IsNullOrEmpty(d.DocFolder.DataKey)
                && (string.IsNullOrEmpty(systemType)
                || (systemType.Contains(SystemTypeCode.Patent) && d.DocFolder.SystemType == SystemTypeCode.Patent
                    && d.DocFolder.ScreenCode == ScreenCode.Application && d.DocFolder.DataKey.ToLower() == "appid")
                || (systemType.Contains(SystemTypeCode.Trademark) && d.DocFolder.SystemType == SystemTypeCode.Trademark
                    && d.DocFolder.ScreenCode == ScreenCode.Trademark && d.DocFolder.DataKey.ToLower() == "tmkid")
                || (systemType.Contains(SystemTypeCode.GeneralMatter) && d.DocFolder.SystemType == SystemTypeCode.GeneralMatter
                    && d.DocFolder.ScreenCode == ScreenCode.GeneralMatter && d.DocFolder.DataKey.ToLower() == "matid"))
                ).Select(d => d.Source).Distinct().ToListAsync();

            return Json(sourceList.Where(c => !String.IsNullOrEmpty(c)).Select(d => new { Source = d }).OrderBy(c => c.Source).ToList());
        }

        public async Task<IActionResult> GetClientList(string property, string text, FilterType filterType, string systemType)
        {
            var clientList = new List<SelectListItem>();

            var parentIds = await _docService.DocFolders.AsNoTracking().Where(d => !string.IsNullOrEmpty(d.DataKey)
                && (string.IsNullOrEmpty(systemType)
                || (systemType.Contains(SystemTypeCode.Patent) && d.SystemType == SystemTypeCode.Patent && d.ScreenCode == ScreenCode.Application && d.DataKey.ToLower() == "appid")
                || (systemType.Contains(SystemTypeCode.Trademark) && d.SystemType == SystemTypeCode.Trademark && d.ScreenCode == ScreenCode.Trademark && d.DataKey.ToLower() == "tmkid")
                || (systemType.Contains(SystemTypeCode.GeneralMatter) && d.SystemType == SystemTypeCode.GeneralMatter && d.ScreenCode == ScreenCode.GeneralMatter && d.DataKey.ToLower() == "matid")))
                .Select(d => new { d.DataKey, d.DataKeyValue }).Distinct().ToListAsync();

            var appIds = parentIds.Where(d => !string.IsNullOrEmpty(d.DataKey) && d.DataKey.ToLower() == "appid").Select(d => d.DataKeyValue).Distinct().ToList();

            var tmkIds = parentIds.Where(d => !string.IsNullOrEmpty(d.DataKey) && d.DataKey.ToLower() == "tmkid").Select(d => d.DataKeyValue).Distinct().ToList();

            var matIds = parentIds.Where(d => !string.IsNullOrEmpty(d.DataKey) && d.DataKey.ToLower() == "matid").Select(d => d.DataKeyValue).Distinct().ToList();


            clientList.AddRange(await _countryApplicationService.CountryApplications.AsNoTracking().Where(d => appIds.Contains(d.AppId) && d.Invention != null && d.Invention.Client != null)
                .Select(d => new SelectListItem()
                {
                    Text = d.Invention != null && d.Invention.Client != null ? d.Invention.Client.ClientCode : "",
                    Value = d.Invention != null && d.Invention.Client != null ? d.Invention.Client.ClientName : ""
                }).ToListAsync());
            clientList.AddRange(await _tmkTrademarkService.TmkTrademarks.AsNoTracking().Where(d => tmkIds.Contains(d.TmkId) && d.Client != null)
                .Select(d => new SelectListItem()
                {
                    Text = d.Client != null ? d.Client.ClientCode : "",
                    Value = d.Client != null ? d.Client.ClientName : ""
                }).ToListAsync());
            // GM module removed
            // clientList.AddRange(await _gmMatterService.QueryableList.AsNoTracking().Where(d => matIds.Contains(d.MatId) && d.Client != null)
            //     .Select(d => new SelectListItem()
            //     {
            //         Text = d.Client != null ? d.Client.ClientCode : "",
            //         Value = d.Client != null ? d.Client.ClientName : ""
            //     }).ToListAsync());

            var settings = await _defaultSettings.GetSetting();
            if (settings.IsDocketRequestOn)
            {
                if (systemType.Contains(SystemTypeCode.Patent))
                    clientList.AddRange(await _docketRequestService.PatDocketRequests.AsNoTracking()
                        .Where(d => d.CountryApplication != null && d.CountryApplication.Invention != null && d.CountryApplication.Invention.Client != null)
                        .Select(d => new SelectListItem()
                        {
                            Text = d.CountryApplication!.Invention!.Client!.ClientCode,
                            Value = d.CountryApplication.Invention.Client.ClientName
                        }).Distinct().ToListAsync());

                if (systemType.Contains(SystemTypeCode.Trademark))
                    clientList.AddRange(await _docketRequestService.TmkDocketRequests.AsNoTracking()
                        .Where(d => d.TmkTrademark != null && d.TmkTrademark.Client != null)
                        .Select(d => new SelectListItem()
                        {
                            Text = d.TmkTrademark!.Client!.ClientCode,
                            Value = d.TmkTrademark.Client.ClientName
                        }).Distinct().ToListAsync());

                if (systemType.Contains(SystemTypeCode.GeneralMatter))
                    clientList.AddRange(await _docketRequestService.GMDocketRequests.AsNoTracking()
                        .Where(d => d.GMMatter != null && d.GMMatter.Client != null)
                        .Select(d => new SelectListItem()
                        {
                            Text = d.GMMatter!.Client!.ClientCode,
                            Value = d.GMMatter.Client.ClientName
                        }).Distinct().ToListAsync());
            }

            if (settings.IsDeDocketOn && settings.IncludeDeDocketInVerification)
            {
                if (systemType.Contains(SystemTypeCode.Patent))
                    clientList.AddRange(await _patActionDueService.QueryableList.AsNoTracking()
                        .Where(d => d.CountryApplication != null && d.CountryApplication.Invention != null && d.CountryApplication.Invention.Client != null && d.DueDates != null && d.DueDates.Any(a => a.DueDateDeDockets != null && a.DueDateDeDockets.Any()))
                        .Select(d => new SelectListItem()
                        {
                            Text = d.CountryApplication!.Invention!.Client!.ClientCode,
                            Value = d.CountryApplication.Invention.Client.ClientName
                        }).Distinct().ToListAsync());

                if (systemType.Contains(SystemTypeCode.Trademark))
                    clientList.AddRange(await _tmkActionDueService.QueryableList.AsNoTracking()
                        .Where(d => d.TmkTrademark != null && d.TmkTrademark.Client != null && d.DueDates != null && d.DueDates.Any(a => a.DueDateDeDockets != null && a.DueDateDeDockets.Any()))
                        .Select(d => new SelectListItem()
                        {
                            Text = d.TmkTrademark!.Client!.ClientCode,
                            Value = d.TmkTrademark.Client.ClientName
                        }).Distinct().ToListAsync());

                // GM module removed
                // if (systemType.Contains(SystemTypeCode.GeneralMatter))
                //     clientList.AddRange(await _gmActionDueService.QueryableList.AsNoTracking()
                //         .Where(d => d.GMMatter != null && d.GMMatter.Client != null && d.DueDates != null && d.DueDates.Any(s => s.DueDateDeDockets != null && s.DueDateDeDockets.Any()))
                //         .Select(d => new SelectListItem()
                //         {
                //             Text = d.GMMatter!.Client!.ClientCode,
                //             Value = d.GMMatter.Client.ClientName
                //         }).Distinct().ToListAsync());
            }

            return Json(clientList.Select(c => new { Client = c.Text, ClientName = c.Value }).DistinctBy(c => new { c.Client, c.ClientName })
                .Where(c => !String.IsNullOrEmpty(c.Client)).OrderBy(c => c.Client).ToList());
        }

        public async Task<IActionResult> GetCaseTypeList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var caseTypes = await _countryApplicationService.CountryApplications.AsNoTracking()
                .Select(d => new { CaseType = d.CaseType, Description = d.PatCaseType != null ? d.PatCaseType.Description : "" })
                .Union(_tmkTrademarkService.TmkTrademarks.AsNoTracking().Select(d => new { CaseType = d.CaseType, Description = d.TmkCaseType != null ? d.TmkCaseType.Description : "" }))
                // GM module removed
                // .Union(_gmMatterService.QueryableList.AsNoTracking().Select(d => new { CaseType = d.MatterType, Description = d.GMMatterType != null ? d.GMMatterType.Description : "" }))
                .ToListAsync();

            var result = caseTypes.Distinct().Where(c => c.CaseType != null).OrderBy(o => o.CaseType).ToList();
            return Json(result);
        }
        #endregion

        #region Document Editor Popup  

        /// <summary>
        /// Data source for assigning new responsible (docketing and reporting) to document
        /// </summary>
        /// <param name="property"></param>
        /// <param name="text"></param>
        /// <param name="filterType"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDocResponsibleCombinedList(string property, string text, FilterType filterType)
        {
            var data = await _groupManager.GetGroups().Where(d => string.IsNullOrEmpty(text) || d.Name.Contains(text))
                .Select(g => new { Name = g.Name, Id = g.Id.ToString() })
                .Distinct().OrderBy(g => g.Name).ToListAsync();

            data.AddRange(await _userManager.Users.Select(g => new { Name = g.FirstName + " " + g.LastName, Id = g.Id })
                .Where(d => string.IsNullOrEmpty(text) || d.Name.Contains(text))
                .Distinct().OrderBy(g => g.Name).ToListAsync());

            return Json(data);
        }

        /// <summary>
        /// Data source for ActionType ComboBox on document editor popup Action grid
        /// </summary>
        /// <param name="property"></param>
        /// <param name="text"></param>
        /// <param name="filterType"></param>
        /// <param name="parentId"></param>
        /// <param name="screenCode"></param>
        /// <param name="docLibraryFolder"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetCorrespondingActList(string property, string text, FilterType filterType, int parentId, string screenCode = "", string docLibraryFolder = "")
        {
            var systemType = "";
            if (screenCode.ToUpper() == ScreenCode.Application.ToUpper() || docLibraryFolder.ToUpper() == SharePointDocLibraryFolder.Application.ToUpper())
                systemType = SystemTypeCode.Patent;
            else if (screenCode.ToUpper() == ScreenCode.Trademark.ToUpper() || docLibraryFolder.ToUpper() == SharePointDocLibraryFolder.Trademark.ToUpper())
                systemType = SystemTypeCode.Trademark;
            else if (screenCode.ToUpper() == ScreenCode.GeneralMatter.ToUpper() || docLibraryFolder.ToUpper() == SharePointDocLibraryFolder.GeneralMatter.ToUpper())
                systemType = SystemTypeCode.GeneralMatter;

            if (string.IsNullOrEmpty(systemType) || parentId <= 0)
                return Ok();

            if (systemType == SystemTypeCode.Patent)
            {
                var app = await _countryApplicationService.GetById(parentId);

                var actions = await _patActionDueService.QueryableList.Where(d => d.AppId == parentId)
                                    .Select(d => new
                                    {
                                        KeyId = "ActId|" + d.ActId.ToString(),
                                        ActionType = d.ActionType ?? "",
                                        BaseDate = d.BaseDate.ToString("dd-MMM-yyyy"),
                                        Source = d.ComputerGenerated == true ? "CL" : "OA"
                                    })
                                    .ToListAsync();

                actions.AddRange((await _patActionTypeService.QueryableList
                                    .Where(d => d.Country == app.Country || string.IsNullOrEmpty(d.Country))
                                    .Select(d => new
                                    {
                                        KeyId = "ActionTypeID|" + d.ActionTypeID.ToString(),
                                        ActionType = d.ActionType,
                                        BaseDate = "",
                                        Source = "OA"
                                    })
                                    .Distinct()
                                    .ToListAsync())
                                    .Where(d => !actions.Any() || !actions.Any(ad => ad.ActionType == d.ActionType))
                                    .ToList());

                return Json(actions.OrderBy(o => o.ActionType).ToList());
            }
            else if (systemType == SystemTypeCode.Trademark)
            {
                var tmk = await _tmkTrademarkService.GetByIdAsync(parentId);

                var actions = await _tmkActionDueService.QueryableList.Where(d => d.TmkId == parentId)
                                    .Select(d => new
                                    {
                                        KeyId = "ActId|" + d.ActId.ToString(),
                                        ActionType = d.ActionType,
                                        BaseDate = d.BaseDate.ToString("dd-MMM-yyyy"),
                                        Source = d.ComputerGenerated == true ? "CL" : "OA"
                                    })
                                    .ToListAsync();

                actions.AddRange((await _tmkActionTypeService.QueryableList
                                    .Where(d => d.Country == tmk.Country || string.IsNullOrEmpty(d.Country))
                                    .Select(d => new
                                    {
                                        KeyId = "ActionTypeID|" + d.ActionTypeID.ToString(),
                                        ActionType = d.ActionType,
                                        BaseDate = "",
                                        Source = "OA"
                                    })
                                    .Distinct().ToListAsync()).Where(d => !actions.Any() || !actions.Any(ad => ad.ActionType == d.ActionType)).ToList());

                return Json(actions.OrderBy(o => o.ActionType).ToList());
            }
            // GM module removed
            // else if (systemType == SystemTypeCode.GeneralMatter)
            // {
            //     (entire GeneralMatter action type block removed)
            // }

            return Ok();
        }

        /// <summary>
        /// Data source for Action grid (tblDocVerification) on document editor popup
        /// </summary>
        /// <param name="request"></param>
        /// <param name="docId"></param>
        /// <param name="documentLink"></param>
        /// <param name="driveItemId"></param>
        /// <param name="docLibraryFolder"></param>
        /// <param name="randomGuid"></param>
        /// <returns></returns>
        public async Task<IActionResult> DocVerificationRead([DataSourceRequest] DataSourceRequest request, int docId = 0, string documentLink = "", string driveItemId = "", string docLibraryFolder = "", string randomGuid = "")
        {
            var settings = await _defaultSettings.GetSetting();
            var verifications = new List<DocVerificationViewModel>();

            if (settings.DocumentStorage == DocumentStorageOptions.SharePoint)
            {
                verifications = await _docService.DocVerifications.ProjectTo<DocVerificationViewModel>()
                                .Where(t => (!string.IsNullOrEmpty(driveItemId) && t.DocDocument != null && t.DocDocument.DocFile != null && t.DocDocument.DocFile.DriveItemId == driveItemId) || (!string.IsNullOrEmpty(randomGuid) && t.RandomGuid == randomGuid))
                                .ToListAsync();

                var screenCode = "";
                if (docLibraryFolder.ToUpper() == SharePointDocLibraryFolder.Application.ToUpper())
                    screenCode = ScreenCode.Application;
                else if (docLibraryFolder.ToUpper() == SharePointDocLibraryFolder.Trademark.ToUpper())
                    screenCode = ScreenCode.Trademark;
                else if (docLibraryFolder.ToUpper() == SharePointDocLibraryFolder.GeneralMatter.ToUpper())
                    screenCode = ScreenCode.GeneralMatter;

                verifications = await CreateViewModelForDocVerification(verifications, screenCode);

                return Json(verifications.ToDataSourceResult(request));
            }
            else
            {
                verifications = await _docService.DocVerifications.ProjectTo<DocVerificationViewModel>()
                                        .Where(t => (docId > 0 && t.DocId == docId) || (!string.IsNullOrEmpty(randomGuid) && t.RandomGuid == randomGuid))
                                        .ToListAsync();

                var docLinkArr = documentLink.Split("|");
                var screenCode = docLinkArr[1];
                verifications = await CreateViewModelForDocVerification(verifications, screenCode);

                return Json(verifications.ToDataSourceResult(request));
            }
        }

        /// <summary>
        /// Method to handle updating and adding new ActionType/ActionDue record to Action grid (tblDocVerification) on document editor popup
        /// </summary>
        /// <param name="request"></param>
        /// <param name="tStamp"></param>
        /// <param name="updated"></param>
        /// <param name="added"></param>
        /// <param name="deleted"></param>
        /// <param name="docId"></param>
        /// <param name="driveItemId"></param>
        /// <param name="documentLink"></param>
        /// <returns></returns>
        public async Task<IActionResult> DocVerificationUpdate([DataSourceRequest] DataSourceRequest request, byte[]? tStamp,
            [Bind(Prefix = "updated")] IList<DocVerification> updated,
            [Bind(Prefix = "new")] IList<DocVerification> added,
            [Bind(Prefix = "deleted")] IList<DocVerification> deleted,
            int docId = 0, string driveItemId = "", string documentLink = "")
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                var updatedDups = updated.Where(d => d.ActionTypeID > 0).GroupBy(grp => new { grp.ActionTypeID, grp.BaseDate }).Where(d => d.Count() > 1).Any();
                var addedDups = added.Where(d => d.ActionTypeID > 0).GroupBy(grp => new { grp.ActionTypeID, grp.BaseDate }).Where(d => d.Count() > 1).Any();

                if (updatedDups || addedDups)
                {
                    ModelState.AddModelError("Actions", _localizer["Unable to save record as this will create duplicate entries in the database."]);
                    return new JsonBadRequest(new { errors = ModelState.Errors() });
                }

                var docLinkArr = documentLink.Split("|");
                var systemType = string.Empty;

                if (docLinkArr != null && docLinkArr.Length > 0) systemType = docLinkArr[0];

                var settings = await _defaultSettings.GetSetting();
                if (settings.DocumentStorage == DocumentStorageOptions.SharePoint)
                {
                    var docDocument = await _docService.DocDocuments.Where(d => d.DocFile != null && d.DocFile.DriveItemId == driveItemId).FirstOrDefaultAsync();
                    if (docDocument == null) docId = 0;
                    else docId = docDocument.DocId;
                }

                foreach (var item in updated)
                {
                    //Set DocId to null if update DocVerification records on new Document
                    //New document uses RandomGuid to link DocVerification with Document after saving
                    if (item.DocId == 0 && !string.IsNullOrEmpty(item.RandomGuid)) item.DocId = null;
                    UpdateEntityStamps(item, item.VerifyId);
                }
                foreach (var item in added)
                {
                    //Set DocId to null if update DocVerification records on new Document
                    //New document uses RandomGuid to link DocVerification with Document after saving
                    item.DocId = docId;
                    if (item.DocId == 0 && !string.IsNullOrEmpty(item.RandomGuid)) item.DocId = null;
                    UpdateEntityStamps(item, item.VerifyId);
                }

                await _docService.UpdateDocVerifications(docId, User.GetUserName(), updated, added, deleted, documentLink);

                var success = updated.Count() + added.Count() == 1 ?
                    _localizer["Action has been saved successfully."].ToString() :
                    _localizer["Actions have been saved successfully"].ToString();
                return Ok(new { message = success });
            }
            return Ok();
        }

        /// <summary>
        /// Method to delete ActionType/ActionDue from Action grid (tblDocVerification) on document editor popup
        /// </summary>
        /// <param name="request"></param>
        /// <param name="deleted"></param>
        /// <returns></returns>
        public async Task<IActionResult> DocVerificationDelete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] DocVerificationDetail deleted)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.VerifyId > 0)
            {
                UpdateEntityStamps(deleted, deleted.VerifyId);
                await _docService.UpdateDocVerifications(deleted.DocId ?? 0, User.GetUserName(), new List<DocVerification>(), new List<DocVerification>(), new List<DocVerification>() { _mapper.Map<DocVerification>(deleted) });
                return Ok(new { message = _localizer["Action has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        private async Task<List<DocVerificationViewModel>> CreateViewModelForDocVerification(List<DocVerificationViewModel> viewModels, string screenCode)
        {
            foreach (var item in viewModels)
            {
                if (screenCode.ToUpper() == ScreenCode.Application.ToUpper())
                {
                    if (item.ActId > 0)
                    {
                        var action = await _patActionDueService.QueryableList.Where(d => d.ActId == item.ActId).FirstOrDefaultAsync();
                        if (action != null)
                        {
                            item.ActionType = action.ActionType;
                            item.BaseDate = action.BaseDate;
                            item.VerifiedBy = action.VerifiedBy;
                            item.DateVerified = action.DateVerified;
                        }
                    }
                    else if (item.ActionTypeID > 0 && item.ActId == 0)
                    {
                        item.ActionType = await _patActionTypeService.QueryableList.Where(d => d.ActionTypeID == item.ActionTypeID).Select(d => d.ActionType).FirstOrDefaultAsync();
                    }
                }
                else if (screenCode.ToUpper() == ScreenCode.Trademark.ToUpper())
                {
                    if (item.ActId > 0)
                    {
                        var action = await _tmkActionDueService.QueryableList.Where(d => d.ActId == item.ActId).FirstOrDefaultAsync();
                        if (action != null)
                        {
                            item.ActionType = action.ActionType;
                            item.BaseDate = action.BaseDate;
                            item.VerifiedBy = action.VerifiedBy;
                            item.DateVerified = action.DateVerified;
                        }
                    }
                    else if (item.ActionTypeID > 0 && item.ActId == 0)
                    {
                        item.ActionType = await _tmkActionTypeService.QueryableList.Where(d => d.ActionTypeID == item.ActionTypeID).Select(d => d.ActionType).FirstOrDefaultAsync();
                    }
                }
                // GM module removed
                // else if (screenCode.ToUpper() == ScreenCode.GeneralMatter.ToUpper())
                // {
                //     (entire GeneralMatter action lookup block removed)
                // }
            }

            //if (viewModels.Count == 1) viewModels.ForEach(d => { d.CanDelete = false; });

            return viewModels;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Prepares a temporary file for viewing by either returning the path to an existing temporary file
        /// or downloading the document from SharePoint or iManage and saving it to a temporary location. 
        /// </summary>
        /// <param name="driveItemId"></param>
        /// <param name="tempFileName"></param>
        /// <param name="docLibrary"></param>
        /// <returns></returns>
        private async Task<string> PrepareTemporaryFile(string driveItemId, string? docLibrary = "", bool useUniqueName = true)
        {
            var settings = await _defaultSettings.GetSetting();
            var tempFilePath = string.Empty;

            if (string.IsNullOrEmpty(driveItemId)) return tempFilePath;

            if (settings.DocumentStorage == DocumentStorageOptions.SharePoint)
            {
                try
                {
                    var graphClient = _sharePointService.GetGraphClient();
                    var driveItem = await graphClient.GetSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId);

                    if (driveItem == null) return string.Empty;

                    var tempFileName = (driveItem.CTag ?? "").ReplaceInvalidFilenameChars() + "_" + driveItem.Name.ReplaceInvalidFilenameChars();

                    if (!useUniqueName) tempFileName = driveItem.Name.ReplaceInvalidFilenameChars();

                    tempFilePath = GetTemporaryFilePath(tempFileName);

                    if (System.IO.File.Exists(tempFilePath)) return tempFilePath;

                    using (var stream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId))
                    {
                        if (stream == null) return string.Empty;

                        using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return string.Empty;
                    }
                    throw;
                }
            }
            else if (settings.DocumentStorage == DocumentStorageOptions.iManage)
            {
                try
                {
                    var iManageClient = await _iManageClientFactory.GetClient();
                    var iManageDocument = await iManageClient.GetDocumentProfile(driveItemId);

                    if (iManageDocument == null) return string.Empty;

                    var tempFileName = (driveItemId ?? "").ReplaceInvalidFilenameChars() + "_" + (iManageDocument.GetFileName()).ReplaceInvalidFilenameChars();

                    if (!useUniqueName) tempFileName = iManageDocument.GetFileName().ReplaceInvalidFilenameChars();

                    tempFilePath = GetTemporaryFilePath(tempFileName);

                    if (System.IO.File.Exists(tempFilePath)) return tempFilePath;

                    var iManageResponse = await iManageClient.GetDocument(driveItemId);
                    iManageResponse.EnsureSuccessStatusCode();
                    using (var stream = await iManageResponse.Content.ReadAsStreamAsync())
                    {
                        if (stream == null) return string.Empty;

                        using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                }
                catch (iManageServiceException ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return string.Empty;
                    }
                    throw;
                }
            }
            else if (settings.DocumentStorage == DocumentStorageOptions.NetDocuments)
            {
                try
                {
                    var netDocsClient = await _netDocsClientFactory.GetClient();
                    var netDocsDocument = await netDocsClient.GetDocumentProfile(driveItemId);

                    if (netDocsDocument == null) return string.Empty;

                    var tempFileName = (driveItemId ?? "").ReplaceInvalidFilenameChars() + "_" + (netDocsDocument.GetFileName()).ReplaceInvalidFilenameChars();

                    if (!useUniqueName) tempFileName = netDocsDocument.GetFileName().ReplaceInvalidFilenameChars();

                    tempFilePath = GetTemporaryFilePath(tempFileName);

                    if (System.IO.File.Exists(tempFilePath)) return tempFilePath;

                    var response = await netDocsClient.GetDocument(driveItemId);
                    response.EnsureSuccessStatusCode();
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        if (stream == null) return string.Empty;

                        using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                }
                catch (NetDocumentsServiceException ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return string.Empty;
                    }
                    throw;
                }
            }

            return tempFilePath;
        }

        private string GetTemporaryFilePath(string tempFileName)
        {
            var tempFolder = FileHelper.GetTemporaryFolder(User.GetUserName());
            var tempFilePath = Path.Combine(tempFolder, tempFileName);
            return tempFilePath;
        }

        [HttpPost]
        public async Task<IActionResult> ExportToPowerPoint(string contentType, string base64, string fileName)
        {
            try
            {
                var ppMS = await _exportHelper.DataToPowerPointMemoryStream(contentType, base64, fileName);
                return File(ppMS.ToArray(), ImageHelper.GetContentType(".pptx"), fileName.Split("|")[0] + ".pptx");
            }
            catch (Exception e)
            {
                var error = e.Message;
            }
            return Ok();
        }

        private async Task<string> GetSearchSettings(string settingName)
        {
            var user = await _userManager.GetUserAsync(User);
            var setting = await GetSettingByNameAsync(settingName);

            if (setting != null && user != null)
            {
                var userSetting = await GetUserSettingsAsync(user.Id, setting.Id);
                if (userSetting != null)
                    return userSetting.Settings;

                return string.Empty;
            }
            return string.Empty;
        }

        private async Task<CPiSetting?> GetSettingByNameAsync(string name)
        {
            return await _repository.CPiSettings.Where(d => d.Name == name).AsNoTracking().FirstOrDefaultAsync() ?? null;
        }

        private async Task<CPiUserSetting?> GetUserSettingsAsync(string userId, int settingId)
        {
            return await _repository.CPiUserSettings.Where(u => u.UserId == userId && u.SettingId == settingId).AsNoTracking().FirstOrDefaultAsync() ?? null;
        }

        private async Task UpdateUserSettingsAsync(CPiUserSetting userSetting)
        {
            _repository.CPiUserSettings.Update(userSetting);
            await _repository.SaveChangesAsync();
        }

        private async Task AddUserSettingsAsync(CPiUserSetting userSetting)
        {
            _repository.CPiUserSettings.Add(userSetting);
            await _repository.SaveChangesAsync();
        }

        private T GetWidgetSetting<T>(string settingName, string widgetSettings)
        {
            T setting = default(T);

            JObject settings = GetWidgetSetting(widgetSettings);

            if (settings.Property(settingName) != null)
            {
                setting = settings[settingName].Value<T>();
            }
            return setting;
        }

        private JObject GetWidgetSetting(string widgetSettings)
        {
            JObject settings = new JObject();

            if (!string.IsNullOrEmpty(widgetSettings))
            {
                settings = JObject.Parse(widgetSettings);
            }
            return settings;
        }

        private async Task<string> GetWidgetSettings(string settingName)
        {
            var user = await _userManager.GetUserAsync(User);
            var widgetSettings = await GetSettingByNameAsync("DocVerificationWidgetSettings");
            var settings = string.Empty;

            if (widgetSettings != null && user != null)
            {
                var userSetting = await GetUserSettingsAsync(user.Id, widgetSettings.Id);
                if (userSetting != null)
                {
                    var existingSettings = JsonConvert.DeserializeObject<List<DocVerificationWidgetInfo>>(userSetting.Settings);
                    if (existingSettings != null)
                        settings = existingSettings.FirstOrDefault(d => !string.IsNullOrEmpty(d.WidgetId) && d.WidgetId.ToLower() == settingName.ToLower())?.Settings ?? string.Empty;
                }
            }

            return settings;
        }

        private async Task<bool> CanViewRemarks(string systemType, string respOffice)
        {

            if (User.IsAdmin())
                return true;

            if (systemType == SystemTypeCode.Patent || systemType == SystemTypeCode.PTOActions)
            {
                var hasRespOfficeFilter = User.HasRespOfficeFilter(SystemType.Patent);

                if (hasRespOfficeFilter)
                {
                    return (!(await _authService.AuthorizeAsync(User, respOffice, PatentAuthorizationPolicy.LimitedReadByRespOffice)).Succeeded);

                }
                return (!(await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.LimitedRead)).Succeeded);
            }
            else if (systemType == SystemTypeCode.Trademark || systemType == SystemTypeCode.TrademarkLinks)
            {
                var hasRespOfficeFilter = User.HasRespOfficeFilter(SystemType.Trademark);

                if (hasRespOfficeFilter)
                {
                    return (!(await _authService.AuthorizeAsync(User, respOffice, TrademarkAuthorizationPolicy.LimitedReadByRespOffice)).Succeeded);

                }
                return (!(await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.LimitedRead)).Succeeded);
            }
            else if (systemType == SystemTypeCode.GeneralMatter)
            {
                var hasRespOfficeFilter = User.HasRespOfficeFilter(SystemType.GeneralMatter);

                if (hasRespOfficeFilter)
                {
                    return (!(await _authService.AuthorizeAsync(User, respOffice, GeneralMatterAuthorizationPolicy.LimitedReadByRespOffice)).Succeeded);

                }
                return (!(await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.LimitedRead)).Succeeded);
            }
            else if (systemType == SystemTypeCode.DMS)
            {
                //return (!(await _authService.AuthorizeAsync(User, DMSAuthorizationPolicy.LimitedRead)).Succeeded); //no limitedread in DMS
                return (await _authService.AuthorizeAsync(User, DMSAuthorizationPolicy.CanAccessSystem)).Succeeded;
            }
            return false;

        }

        private async Task<bool> CanAccessDocketRequest(string systemType)
        {
            if (User.IsAdmin())
                return true;
            
            switch (systemType)
            {
                case SystemTypeCode.Patent:
                    return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanRequestDocket)).Succeeded;
                case SystemTypeCode.Trademark:
                    return (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanRequestDocket)).Succeeded;
                case SystemTypeCode.GeneralMatter:
                    return (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanRequestDocket)).Succeeded;
                default:
                    return false;
            }
        }

        private async Task<bool> CanCompleteInstruction(string systemType, string respOffice)
        {

            if (!"PTG".Contains(systemType))
                return false;

            if (User.IsAdmin())
                return true;

            if (systemType == SystemTypeCode.Patent)
            {
                if (User.IsDeDocketer(SystemType.Patent, respOffice))
                    return false;

                var hasRespOfficeFilter = User.HasRespOfficeFilter(SystemType.Patent);
                if (hasRespOfficeFilter)
                {
                    return (await _authService.AuthorizeAsync(User, respOffice, PatentAuthorizationPolicy.FullModifyByRespOffice)).Succeeded;
                }
                return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.FullModify)).Succeeded;
            }
            else if (systemType == SystemTypeCode.Trademark)
            {
                if (User.IsDeDocketer(SystemType.Trademark, respOffice))
                    return false;

                var hasRespOfficeFilter = User.HasRespOfficeFilter(SystemType.Trademark);
                if (hasRespOfficeFilter)
                {
                    return (await _authService.AuthorizeAsync(User, respOffice, TrademarkAuthorizationPolicy.FullModifyByRespOffice)).Succeeded;
                }
                return (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.FullModify)).Succeeded;
            }
            else if (systemType == SystemTypeCode.GeneralMatter)
            {
                if (User.IsDeDocketer(SystemType.GeneralMatter, respOffice))
                    return false;

                var hasRespOfficeFilter = User.HasRespOfficeFilter(SystemType.GeneralMatter);
                if (hasRespOfficeFilter)
                {
                    return (await _authService.AuthorizeAsync(User, respOffice, GeneralMatterAuthorizationPolicy.FullModifyByRespOffice)).Succeeded;
                }
                return (await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.FullModify)).Succeeded;
            }
            return false;
        }

        private async Task<bool> CanUploadDocument(string systemType, string respOffice)
        {
            if (!"PTG".Contains(systemType))
                return false;

            if (User.IsAdmin())
                return true;

            if (systemType == SystemTypeCode.Patent)
            {
                var hasRespOfficeFilter = User.HasRespOfficeFilter(SystemType.Patent);
                if (hasRespOfficeFilter)
                {
                    return ((await _authService.AuthorizeAsync(User, respOffice, PatentAuthorizationPolicy.FullModifyByRespOffice)).Succeeded);
                }
                return ((await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanUploadDocuments)).Succeeded);
            }
            else if (systemType == SystemTypeCode.Trademark)
            {
                var hasRespOfficeFilter = User.HasRespOfficeFilter(SystemType.Trademark);
                if (hasRespOfficeFilter)
                {
                    return ((await _authService.AuthorizeAsync(User, respOffice, TrademarkAuthorizationPolicy.FullModifyByRespOffice)).Succeeded);
                }
                return ((await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanUploadDocuments)).Succeeded);
            }
            else if (systemType == SystemTypeCode.GeneralMatter)
            {
                var hasRespOfficeFilter = User.HasRespOfficeFilter(SystemType.GeneralMatter);

                if (hasRespOfficeFilter)
                {
                    return ((await _authService.AuthorizeAsync(User, respOffice, GeneralMatterAuthorizationPolicy.FullModifyByRespOffice)).Succeeded);
                }
                return ((await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanUploadDocuments)).Succeeded);
            }
            return false;
        }

        private (List<string> userList, List<int> groupList) ParseResponsibleData(List<string> data)
        {
            var userIds = new List<string>();
            var groupIds = new List<int>();

            foreach (var item in data)
            {
                if (int.TryParse(item, out int intVal))
                {
                    groupIds.Add(intVal);
                }
                else
                {
                    userIds.Add(item);
                }
            }

            return (userIds, groupIds);
        }

        private List<SystemDataKey> ParseSystemDateKey(string ids)
        {
            //ids: SystemType|DataKey|DataKeyValue
            //Ex: P|DocId|123;P|ReqId|321;P|DeDocketId|123
            var result = ids.Split(";").Where(d => !string.IsNullOrEmpty(d)).Select(d =>
            {
                int dataKeyValue = 0;
                string dataKey = string.Empty;
                string systemType = string.Empty;
                var itemArr = d.Split("|");
                if (itemArr != null && itemArr.Length > 0)
                {
                    if (int.TryParse(itemArr[2], out dataKeyValue))
                    {
                        systemType = itemArr[0];
                        dataKey = itemArr[1];
                    }
                }
                return new SystemDataKey { SystemType = systemType, DataKey = dataKey, DataKeyValue = dataKeyValue };
            }).Where(d => d.DataKeyValue > 0 && !string.IsNullOrEmpty(d.SystemType) && !string.IsNullOrEmpty(d.DataKey)).Distinct().ToList();

            return result;
        }

        public class SystemDataKey
        {
            public string? SystemType { get; set; }
            public string? DataKey { get; set; }
            public int DataKeyValue { get; set; }
        }
        #endregion
    }
}