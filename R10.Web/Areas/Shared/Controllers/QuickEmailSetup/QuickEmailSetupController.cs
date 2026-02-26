using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Security;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Core.Helpers;
using Newtonsoft.Json;
using R10.Web.Helpers;
using R10.Core.Exceptions;
using Microsoft.Extensions.Caching.Distributed;
using R10.Core.Entities.Patent;
using DocuSign.eSign.Model;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Services.Shared;
using R10.Web.Services;
using Microsoft.Extensions.Options;
using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class QuickEmailSetupController : BaseController
    {
        private readonly IAuthorizationService _authService;
        protected readonly IOuickEmailSetupViewModelService _viewModelService;
        protected readonly IQuickEmailSetupService _service;
        protected readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IDistributedCache _distributedCache;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;
        // private readonly ISystemSettings<GMSetting> _gmSettings; // Removed during deep clean
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly IChildEntityService<QEMain, QETag> _qeTagService;
        private readonly EPOMailboxSettings _epoMailboxSettings;


        private readonly string _dataContainer = "qeSetupDetail";

        public QuickEmailSetupController(IAuthorizationService authService, IOuickEmailSetupViewModelService viewModelService,
            IQuickEmailSetupService service, IMapper mapper, IStringLocalizer<SharedResource> localizer, IDistributedCache distributedCache,
            ISystemSettings<PatSetting> patSettings, ISystemSettings<DefaultSetting> settings,
            ISystemSettings<TmkSetting> tmkSettings, /* ISystemSettings<GMSetting> gmSettings, // Removed during deep clean */ IChildEntityService<QEMain, QETag> qeTagService,
            IOptions<EPOMailboxSettings> epoMailboxSettings)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _service = service;
            _mapper = mapper;
            _localizer = localizer;
            _distributedCache = distributedCache;
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            // _gmSettings = gmSettings; // Removed during deep clean
            _settings = settings;
            _qeTagService = qeTagService;
            _epoMailboxSettings = epoMailboxSettings.Value;
        }

        [Authorize(Policy = PatentAuthorizationPolicy.FullRead)]
        public async Task<IActionResult> Patent()
        {
            return await Index(SystemTypeCode.Patent);
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.FullRead)]
        public async Task<IActionResult> Trademark()
        {
            return await Index(SystemTypeCode.Trademark);
        }


        private async Task<IActionResult> Index(string systemType)
        {
            var model = await BuildSearchPageViewModel();
            model.SystemType = systemType;

            var cacheKey = User.GetUserName() + ":QES-SystemType";
            await _distributedCache.SetStringAsync(cacheKey,systemType);

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View("Index", model);
        }

        public async Task<IActionResult> Search() {
            var cacheKey = User.GetUserName() + ":QES-SystemType";
            var systemType = await _distributedCache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(systemType)) {
                return await Index(systemType);
            }
            return BadRequest();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel()
            {
                Page = PageType.SearchResults,
                PageId = "qeSetupSearchResults",
                Title = _localizer["Quick Email Setup Search Results"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded,
                SystemType = GetSystemTypeFromQueryFilters(mainSearchFilters)
            };
            if (!(await HasSystemPermission(model.SystemType)))
                throw new NoRecordPermissionException();

            return PartialView("Index", model);

        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var systemType = GetSystemTypeFromQueryFilters(mainSearchFilters);

                if (!(await HasSystemPermission(systemType)))
                    throw new NoRecordPermissionException();

                var templates = _viewModelService.AddCriteria(_service.GetQEMainBySystemType(systemType), mainSearchFilters);


                if (systemType == "P")
                {
                    if (!_patSettings.GetSetting().Result.IsInventorRemunerationOn)
                    {
                        var dataSource = _service.GetQEDataSourcesBySystemTypeAsync(systemType);
                        var RemunerationDSIds = dataSource.Where(c => c.DataSourceName.StartsWith("DE Remuneration")).Select(c=>c.DataSourceID).ToArray();
                        templates = templates.Where(c => !RemunerationDSIds.Contains(c.DataSourceID));
                    }
                    if (!_patSettings.GetSetting().Result.IsInventorFRRemunerationOn)
                    {
                        var dataSource = _service.GetQEDataSourcesBySystemTypeAsync(systemType);
                        var RemunerationDSIds = dataSource.Where(c => c.DataSourceName.StartsWith("FR Remuneration")).Select(c => c.DataSourceID).ToArray();
                        templates = templates.Where(c => !RemunerationDSIds.Contains(c.DataSourceID));
                    }
                }

                if ((systemType != SystemTypeCode.Patent && systemType != SystemTypeCode.Trademark) // Removed SystemTypeCode.GeneralMatter during deep clean
                || (!_patSettings.GetSetting().Result.IsDocumentVerificationOn && !_tmkSettings.GetSetting().Result.IsDocumentVerificationOn)) // Removed _gmSettings.GetSetting().Result.IsDocumentVerificationOn during deep clean
                    templates = templates.Where(c => c.DataSource != null && c.DataSource.SystemType != SystemTypeCode.Shared);

                if (systemType != SystemTypeCode.Patent || !_patSettings.GetSetting().Result.IsDocumentVerificationOn || !_epoMailboxSettings.IsAPIOn)
                    templates = templates.Where(c => c.DataSource != null && !string.IsNullOrEmpty(c.DataSource.DataSourceName) && c.DataSource.DataSourceName.ToLower() != "orphan epo documents");

                var result = await _viewModelService.CreateViewModelForGrid(request, templates);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    throw new NoRecordPermissionException();
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Quick Email Setup Detail"].ToString(),
                RecordId = detail.QESetupID,
                SingleRecord = singleRecord || !Request.IsAjax(),
                ActiveTab = tab,
                PagePermission = page,
                Data = detail,
                SystemType = page.Detail.SystemType
            };

            if (Request.IsAjax())
            {
                if (!singleRecord && !fromSearch)
                    model.Page = PageType.DetailContent;

                return PartialView("Index", model);
            }

            return View("Index", model);
        }

        //zoom from Workflow screen
        [HttpGet()]
        public async Task<IActionResult> DetailLink(int id)
        {
            if (id > 0)
            {
                var quickEmail = await _service.GetQEMains().ProjectTo<QuickEmailSetupDetailViewModel>().FirstOrDefaultAsync(q => q.QESetupID == id);
                if (quickEmail != null)
                    return RedirectToAction(nameof(Detail), new { id = quickEmail.QESetupID, singleRecord = true, fromSearch = true });
            }

            if ((await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded)
                return RedirectToAction(nameof(Add), new { fromSearch = true });
            else
                return new RecordDoesNotExistResult();
        }

        //[Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string systemType = "P")
        {
            if (!(await HasSystemPermission(systemType)))
                throw new NoRecordPermissionException();

            var indexUrl = GetIndexUrlBySystemType(systemType);

            if (!Request.IsAjax())
                return Redirect(indexUrl);

            var page = await PrepareAddScreen(systemType);
            if (page.Detail == null)
                return Redirect(indexUrl);

            if (TempData["CopyOptions"] != null)
            {
                await ExtractCopyParams(page);
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Quick Email Setup"].ToString(),
                RecordId = detail.QESetupID,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch,
                SystemType = systemType
            };

            return PartialView("Index", model);
        }

        [HttpGet()]
        public IActionResult Copy(int id)
        {
            var viewModel = new QuickEmailCopyViewModel
            {
                QESetupID = id,
                CopyMainInfo = true,
                CopyLayout = true,
                CopyRecipients = true,
                CopyRemarks = true
            };
            return PartialView("_QESetupCopy", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCopied([FromBody] QuickEmailCopyViewModel copy)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });
            TempData["CopyOptions"] = JsonConvert.SerializeObject(copy);
            //return RedirectToAction("Add");

            //check system type and pass to Add page, for users who don't have permission to the Patent system or clients that don't have patent system.
            var qE = await _service.GetQEMains().AsNoTracking().Include(qe => qe.SystemScreen).FirstOrDefaultAsync(q => q.QESetupID == copy.QESetupID);
            if (qE == null) return RedirectToAction("Add");

            var systemType = qE.SystemScreen.SystemType;
            return RedirectToAction("Add", new { systemType = systemType });

        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string systemType, string requiredRelation = "")
        {
            return await GetPicklistData(_service.GetQEMainBySystemType(systemType), request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetQEForSignatureTemplates(string property, string text, FilterType filterType, int screenId, string requiredRelation = "")
        {
            var sourceScreen = _service.SystemScreens.FirstOrDefault(s=> s.ScreenId==screenId);
            if (sourceScreen != null) {
                var sourceScreenCode = sourceScreen.ScreenCode.Split("-")[0];
                var screenCode = $"{sourceScreenCode}-eSignature-QE";
                var signatureScreen = _service.SystemScreens.FirstOrDefault(s => s.ScreenCode == screenCode);
                if (signatureScreen != null) {
                    var templates = _service.GetQEMainByScreenId(signatureScreen.ScreenId);
                    var result = templates.Select(t => new { t.QESetupID, t.TemplateName }).OrderBy(t => t.TemplateName);
                    return Json(await result.ToListAsync());
                }
            }
            return BadRequest();
        }

        public async Task<IActionResult> GetScreensListBySystem([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string systemType, string requiredRelation = "")
        {    
            var systemScreens = _service.GetSystemScreensBySystemTypeAsync(systemType);

            if (systemType == "P")
            {
                if (!_patSettings.GetSetting().Result.IsInventorRemunerationOn)
                {
                    systemScreens = systemScreens.Where(c=>!(c.ScreenName == "Distribution Award" || c.ScreenName == "Lump Sum Award" || c.ScreenName == "Yearly Award"));
                }
                if (!_patSettings.GetSetting().Result.IsInventorFRRemunerationOn)
                {
                    systemScreens = systemScreens.Where(c => !(c.ScreenName == "French Remuneration"));
                }
                if (!_patSettings.GetSetting().Result.IsInventionActionOn)
                {
                    systemScreens = systemScreens.Where(c => !(c.ScreenName.Contains("Invention Action") || c.ScreenName.Contains("Invention Due") || c.ScreenName.Contains("Invention DeDocket Instruction")));
                }
                if (!_patSettings.GetSetting().Result.IsInventionCostTrackingOn)
                {
                    systemScreens = systemScreens.Where(c => !(c.ScreenName == "Invention Cost Tracking"));
                }

                if (!_patSettings.GetSetting().Result.IsInventorAwardOn)
                {
                    systemScreens = systemScreens.Where(c => !(c.ScreenName == "Inventor App Award"));
                }               
            }

            if (!_settings.GetSetting().Result.IsESignatureOn)
            {
                systemScreens = systemScreens.Where(c => !(c.ScreenName.Contains("eSignature")));
            }
            

            if (!_settings.GetSetting().Result.IsDeDocketOn)
            {
                systemScreens = systemScreens.Where(c => !(c.ScreenName.Contains("DeDocket")));
            }

            if (!_settings.GetSetting().Result.IsDelegationOn)
            {
                systemScreens = systemScreens.Where(c => !(c.ScreenName.Contains("Delegation") || c.ScreenName.Contains("Delegated Action")));
            }
                        
            if ((systemType != SystemTypeCode.Patent && systemType != SystemTypeCode.Trademark)
                || (!_patSettings.GetSetting().Result.IsDocumentVerificationOn && !_tmkSettings.GetSetting().Result.IsDocumentVerificationOn))
                systemScreens = systemScreens.Where(c => c.SystemType != SystemTypeCode.Shared);

            return await GetPicklistData(systemScreens, request, property, text, filterType, new string[] { "ScreenId", "ScreenName" }, requiredRelation);
        }


        public async Task<IActionResult> GetDataSourceListBySystem([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string systemType, string requiredRelation = "", int screenId=0)
        {
            if (screenId > 0) {
                var screen = await _service.SystemScreens.Where(s => s.ScreenId==screenId).FirstOrDefaultAsync();
                if (screen != null) {
                    var screenDataSource = await _service.GetQEDataSourceByScreenCodeAndSystemTypeAsync(screen.ScreenCode, systemType);
                    if (screenDataSource.Any())
                    {
                        return Json(screenDataSource);
                    }
                }
                
            }
            
            var dataSource = _service.GetQEDataSourcesBySystemTypeAsync(systemType);

            if (systemType == "P")
            {
                if (!_patSettings.GetSetting().Result.IsInventorRemunerationOn)
                {
                    dataSource = dataSource.Where(c=>!c.DataSourceName.StartsWith("DE Remuneration"));
                }

                if (!_patSettings.GetSetting().Result.IsInventorFRRemunerationOn)
                {
                    dataSource = dataSource.Where(c => !c.DataSourceName.StartsWith("FR Remuneration"));
                }

                if (!_patSettings.GetSetting().Result.IsInventionActionOn)
                {
                    dataSource = dataSource.Where(c => !(c.DataSourceName.Contains("Invention Action") || c.DataSourceName.Contains("Invention Due") || c.DataSourceName.Contains("Invention DeDocket Instruction")));
                }

                if (!_patSettings.GetSetting().Result.IsInventionCostTrackingOn)
                {
                    dataSource = dataSource.Where(c => !(c.DataSourceName.StartsWith("Invention Cost Tracking")));
                }

                if (!_patSettings.GetSetting().Result.IsInventorAwardOn)
                {
                    dataSource = dataSource.Where(c => !(c.DataSourceName.Contains("Inventor App Award")));
                }
            }

            if (!_settings.GetSetting().Result.IsDeDocketOn)
            {
                dataSource = dataSource.Where(c => !(c.DataSourceName.Contains("DeDocket")));
            }

            if (!_settings.GetSetting().Result.IsDelegationOn)
            {
                dataSource = dataSource.Where(c => !(c.DataSourceName.Contains("Delegation")));
            }

            if ((systemType != SystemTypeCode.Patent && systemType != SystemTypeCode.Trademark)
                || (!_patSettings.GetSetting().Result.IsDocumentVerificationOn && !_tmkSettings.GetSetting().Result.IsDocumentVerificationOn))
                dataSource = dataSource.Where(c => c.SystemType != SystemTypeCode.Shared);

            if (systemType != SystemTypeCode.Patent || !_patSettings.GetSetting().Result.IsDocumentVerificationOn || !_epoMailboxSettings.IsAPIOn)
                dataSource = dataSource.Where(c => !string.IsNullOrEmpty(c.DataSourceName) && c.DataSourceName.ToLower() != "orphan epo documents");

            return await GetPicklistData(dataSource, request, property, text, filterType, new string[] { "DataSourceID", "DataSourceName" }, requiredRelation);
        }

        public async Task<IActionResult> GetLanguageList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var languages = _service.GetLanguages();
            return await GetPicklistData(languages, request, property, text, filterType, new string[] { "LanguageCulture", "LanguageName" }, requiredRelation);

        }

        //[Authorize(Policy = SharedAuthorizationPolicy.CanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string tStamp)
        {
            if (ModelState.IsValid)
            {
                var qeMainOnFile = await _service.GetQEMains().AsNoTracking().Include(qe=> qe.SystemScreen).FirstOrDefaultAsync(q => q.QESetupID == id);
                if (!(await HasDeletePermission(qeMainOnFile.SystemScreen.SystemType)))
                    throw new NoRecordPermissionException();

                var qeMain = await _service.GetQEMainByIdAsync(id);

                if (qeMain == null)
                    return new RecordDoesNotExistResult();

                //only cpi admins can delete cpi templates
                if (!User.IsSuper() && qeMain.CPITemplate)
                    return BadRequest(_localizer["CPI Template cannot be deleted."].ToString());

                qeMain.tStamp = Convert.FromBase64String(tStamp);
                await _service.DeleteQEMainAsync(qeMain);
                return Ok();
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        //[HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] QuickEmailSetupDetailViewModel viewModel)
        {

            if (ModelState.IsValid && ValidateRequiredFields(viewModel))
            {
                //only cpi admins can edit cpi templates
                if (viewModel.QESetupID > 0 && !User.IsSuper() && await _service.GetQEMains().AnyAsync(qe => qe.QESetupID == viewModel.QESetupID && qe.CPITemplate))
                    return BadRequest(_localizer["CPI Template cannot be edited."].ToString());

                if (!(await HasFullModifyPermission(viewModel.SystemType)))
                    throw new NoRecordPermissionException();

                var quickEmail = _mapper.Map<QEMain>(viewModel);
                UpdateEntityStamps(quickEmail, quickEmail.QESetupID);

                if (quickEmail.QESetupID > 0)
                    await UpdateQuickEmail(quickEmail);
                else
                    await AddQuickEmail(quickEmail);

                if (viewModel.CopyRecipients && viewModel.OldQESetupID > 0)
                {
                    await CopyRecipients(viewModel.OldQESetupID, quickEmail.QESetupID);
                }

                //todo: add handling of default. Only one default template per module
                return Json(quickEmail.QESetupID);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRemarks([FromBody] QuickEmailSetupDetailViewModel viewModel)
        {
            if (!(await HasRemarksOnlyPermission(viewModel.SystemType)))
                throw new NoRecordPermissionException();

            var qeMain = await _service.GetQEMains().FirstOrDefaultAsync(q => q.QESetupID == viewModel.QESetupID);
            qeMain.Remarks = viewModel.Remarks;
            qeMain.tStamp = viewModel.tStamp;
            UpdateEntityStamps(qeMain, qeMain.QESetupID);
            await UpdateQuickEmail(qeMain);
            return Json(qeMain.QESetupID);
        }

        public async Task<IActionResult> RecipientRead([DataSourceRequest] DataSourceRequest request, int qeSetupId)
        {
            var recipients = await _viewModelService.GetRecipients(qeSetupId)
                .ProjectTo<QuickEmailSetupRecipientViewModel>().ToListAsync();

            var result = recipients.ToDataSourceResult(request);
            return Json(result);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> RecipientDelete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] QuickEmailSetupRecipientViewModel deletedQuickEmailSetupRecipient)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            var deletedRecipient = _mapper.Map<QERecipient>(deletedQuickEmailSetupRecipient);
            if (deletedRecipient.RecipientID >= 0)
            {
                var roleSourceId = 0;
                var roleSource = await _service.GetQERoleSourceByIdAsync(deletedRecipient.RoleSourceID);
                if (roleSource != null && roleSource.RoleType.ToLower() == "z")
                    roleSourceId = roleSource.RoleSourceID;

                UpdateEntityStamps(deletedRecipient, deletedRecipient.RecipientID); //for stamping of the header record
                await _service.DeleteRecipientAsync(deletedRecipient);

                if (roleSourceId > 0)
                    await _service.DeleteUnuseRoleSourcesAsync(new List<int> { roleSourceId });
            }

            return Ok();
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> RecipientsUpdate([DataSourceRequest] DataSourceRequest request, int qeSetupID, byte[] tStamp, [Bind(Prefix = "updated")]IEnumerable<QuickEmailSetupRecipientViewModel> updatedRecipient,
            [Bind(Prefix = "new")]IEnumerable<QuickEmailSetupRecipientViewModel> newRecipient, [Bind(Prefix = "deleted")]IEnumerable<QuickEmailSetupRecipientViewModel> deletedRecipient)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            var unUsedRoleSourceIds = new List<int>();

            var deletedRecipients = new List<QERecipient>();
            foreach (var item in deletedRecipient)
            {
                var recipient = _mapper.Map<QERecipient>(item);
                deletedRecipients.Add(recipient);

                if (item.RoleType.ToLower() == "z")
                    unUsedRoleSourceIds.Add(item.RoleSourceID);
            }

            var updatedRecipients = new List<QERecipient>();
            foreach (var item in updatedRecipient)
            {
                var recipient = _mapper.Map<QERecipient>(item);
                if (item.QERoleSource.RoleSourceID > 0 && item.QERoleSource.RoleSourceID != 9999)
                {
                    recipient.RoleSourceID = item.QERoleSource.RoleSourceID;
                }
                else if (item.QERoleSource.RoleSourceID == 9999 && item.QERoleSource.RoleType.ToLower() == "z" && !string.IsNullOrEmpty(item.QERoleSource.RoleName))
                {
                    var newRS = new QERoleSource()
                    {
                        RoleSourceID = 0,
                        SystemType = "",
                        RoleType = "Z",
                        RoleName = item.QERoleSource.RoleName,
                        SourceSQL = "Custom",
                        Description = "Custom Email",
                        OrderOfEntry = 0,
                    };
                    UpdateEntityStamps(newRS, newRS.RoleSourceID);

                    await _service.AddRoleSourceAsync(newRS);

                    if (newRS.RoleSourceID > 0)
                        recipient.RoleSourceID = newRS.RoleSourceID;                    
                }                    

                UpdateEntityStamps(recipient, recipient.RecipientID);
                updatedRecipients.Add(recipient);

                if (item.RoleType.ToLower() == "z")
                    unUsedRoleSourceIds.Add(item.RoleSourceID);
            }

            var newRecipients = new List<QERecipient>();
            foreach (var item in newRecipient)
            {
                //Add custom email to tblQERoleSource before appending to tblQERecipient
                if (item.QERoleSource != null && item.QERoleSource.RoleSourceID == 9999 && item.QERoleSource.RoleType.ToLower() == "z" && !string.IsNullOrEmpty(item.QERoleSource.RoleName))
                {
                    var newRS = new QERoleSource()
                    {
                        RoleSourceID = 0,
                        SystemType = "",
                        RoleType = "Z",
                        RoleName = item.QERoleSource.RoleName,
                        SourceSQL = "Custom",
                        Description = "Custom Email",
                        OrderOfEntry = 0,                        
                    };
                    UpdateEntityStamps(newRS, newRS.RoleSourceID);

                    await _service.AddRoleSourceAsync(newRS);

                    if (newRS.RoleSourceID > 0) 
                        item.QERoleSource.RoleSourceID = newRS.RoleSourceID;
                }

                var recipient = _mapper.Map<QERecipient>(item);
                recipient.QESetupID = qeSetupID;
                recipient.RoleSourceID = item.QERoleSource.RoleSourceID;
                recipient.OrderOfEntry = await GetOrderOfEntry(qeSetupID);

                UpdateEntityStamps(recipient, recipient.RecipientID);
                newRecipients.Add(recipient);
            }

            if (deletedRecipients.Any() || updatedRecipients.Any() || newRecipients.Any())
                await _service.RecipientsUpdateAsync(qeSetupID, User.GetUserName(), tStamp, updatedRecipients,
                    newRecipients, deletedRecipients);

            if (unUsedRoleSourceIds.Count > 0)
                await _service.DeleteUnuseRoleSourcesAsync(unUsedRoleSourceIds);

            return Ok();
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var quickEmail = await _viewModelService.GetQuickEmailSetupById(id);
            return ViewComponent("RecordStamps", new { createdBy = quickEmail.CreatedBy, dateCreated = quickEmail.DateCreated, updatedBy = quickEmail.UpdatedBy, lastUpdate = quickEmail.LastUpdate, quickEmail.tStamp });
        }

        public async Task<IActionResult> GetDataFieldsList([DataSourceRequest] DataSourceRequest request, int dataSourceId)
        {
            if (dataSourceId == 0)
                return new NoRecordFoundResult();

            var list = await _service.GetDataFields(dataSourceId);
            return Json(list);
        }



        public IActionResult GetSendAsData()
        {
            var sendAs = new List<string>
            {
                "To",
                "Copy to"
            };
            return Json(sendAs);
        }

        private async Task<PageViewModel> BuildSearchPageViewModel()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "quickEmailSetupSearch",
                Title = _localizer["Quick Email Setup Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded
            };
            return model;
        }

        private async Task<DetailPageViewModel<QuickEmailSetupDetailViewModel>> PrepareAddScreen(string systemType)
        {
            var viewModel = new DetailPageViewModel<QuickEmailSetupDetailViewModel>
            {
                Detail = new QuickEmailSetupDetailViewModel
                {
                    SystemType = systemType,
                    ScreenName = "",
                    DataSourceName = "",
                    Language="English",
                    InUse = true
                }
            };

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        private async Task<DetailPageViewModel<QuickEmailSetupDetailViewModel>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<QuickEmailSetupDetailViewModel>();
            viewModel.Detail = await _service.GetQEMains().ProjectTo<QuickEmailSetupDetailViewModel>().FirstOrDefaultAsync(q => q.QESetupID == id);

            if (viewModel.Detail != null)
            {
                if (!(await HasSystemPermission(viewModel.Detail.SystemType)))
                    throw new NoRecordPermissionException();

                await AddSecurityPolicies(viewModel.Detail.SystemType, viewModel);
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanEmail = false;
                viewModel.CanPrintRecord = false;

                //only cpi admins can edit and delete cpi indicators
                if (!User.IsSuper() && viewModel.Detail.CPITemplate)
                {
                    viewModel.CanEditRecord = false;
                    viewModel.CanDeleteRecord = false;
                }

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.AddScreenUrl = viewModel.CanAddRecord ? Url.Action("Add", new { fromSearch = false, systemType = viewModel.Detail.SystemType }) : "";
                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.CopyScreenUrl = $"{viewModel.CopyScreenUrl}/{id}";

                viewModel.SearchScreenUrl = GetIndexUrlBySystemType(viewModel.Detail.SystemType);
                viewModel.Container = _dataContainer;
            }
            return viewModel;
        }

        private string GetIndexUrlBySystemType(string systemType)
        {
            var searchUrl = "";
            switch (systemType)
            {
                case "P":
                    searchUrl = this.Url.Action("Patent");
                    break;

                case "T":
                    searchUrl = this.Url.Action("Trademark");
                    break;

            }

            return searchUrl;
        }

        private bool ValidateRequiredFields(QuickEmailSetupDetailViewModel quickEmail)
        {
            if (quickEmail.ScreenId == 0)
            {
                ModelState.AddModelError("ScreenName", "Screen Name is required.");
                return false;
            }

            if (quickEmail.DataSourceID == 0)
            {
                ModelState.AddModelError("DataSource", "Data Source is required.");
                return false;
            }

            if (string.IsNullOrEmpty(quickEmail.SystemType))
            {
                ModelState.AddModelError("SystemType", "SystemType is required.");
                return false;
            }

            return true;
        }

        private async Task UpdateQuickEmail(QEMain quickEmail)
        {
            await _service.UpdateQEMainAsync(quickEmail);
        }

        private async Task AddQuickEmail(QEMain quickEmail)
        {
            await _service.AddQEMainAsync(quickEmail);
        }

        private async Task CopyRecipients(int oldQeSetupId, int qeSetupId)
        {
            var recipients = await _service.GetRecipientsByParentIdAsync(oldQeSetupId);
            foreach (var recipient in recipients)
            {
                recipient.RecipientID = 0;
                recipient.QESetupID = qeSetupId;
                UpdateEntityStamps(recipient, recipient.RecipientID);
                await _service.AddRecipientAsync(recipient);
            }
        }

        private async Task<int> GetOrderOfEntry(int qeSetupID)
        {
            var count = await _service.GetRecipientCount(qeSetupID);
            return count + 1;
        }

        private string GetSystemTypeFromQueryFilters(List<QueryFilterViewModel> mainSearchFilters)
        {
            var systemTypeCriteria = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemType");
            if (systemTypeCriteria != null)
            {
                mainSearchFilters.Remove(systemTypeCriteria);
                return systemTypeCriteria.Value;
            }

            return null;
        }

        private async Task ExtractCopyParams(DetailPageViewModel<QuickEmailSetupDetailViewModel> page)
        {
            var copyOptionsString = TempData["CopyOptions"].ToString();
            var copyOptions = JsonConvert.DeserializeObject<QuickEmailCopyViewModel>(copyOptionsString);

            var source = await _service.GetQEMains().ProjectTo<QuickEmailSetupDetailViewModel>().FirstOrDefaultAsync(q => q.QESetupID == copyOptions.QESetupID);
            if (source != null)
            {
                if (copyOptions.CopyMainInfo)
                {
                    page.Detail = source;
                    page.Detail.CPITemplate = false;
                }

                page.Detail.TemplateName = copyOptions.TemplateName;
                page.Detail.Header = copyOptions.CopyLayout ? source.Header : "";
                page.Detail.Detail = copyOptions.CopyLayout ? source.Detail : "";
                page.Detail.Footer = copyOptions.CopyLayout ? source.Footer : "";
                page.Detail.Remarks = copyOptions.CopyRemarks ? source.Remarks : "";

                page.Detail.QESetupID = 0;
                page.Detail.OldQESetupID = copyOptions.QESetupID;
                page.Detail.CopyRecipients = copyOptions.CopyRecipients;
            }
        }

        private async Task<bool> HasSystemPermission(string systemType)
        {
            switch (systemType)
            {
                case SystemTypeCode.Patent:
                    return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.FullRead)).Succeeded;

                case SystemTypeCode.Trademark:
                    return (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.FullRead)).Succeeded;

                case SystemTypeCode.Shared:
                        return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded
                        || (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanAccessDocumentVerification)).Succeeded;                   

                default:
                    return false;
            }
        }

        private async Task<bool> HasDeletePermission(string systemType)
        {
            switch (systemType)
            {
                case SystemTypeCode.Patent:
                    return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanDelete)).Succeeded;

                case SystemTypeCode.Trademark:
                    return (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanDelete)).Succeeded;

                case SystemTypeCode.Shared:
                    return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.DocumentVerificationModify)).Succeeded
                    || (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.DocumentVerificationModify)).Succeeded;

                default:
                    return false;
            }
        }

        private async Task<bool> HasFullModifyPermission(string systemType)
        {
            switch (systemType)
            {
                case SystemTypeCode.Patent:
                    return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.FullModify)).Succeeded;

                case SystemTypeCode.Trademark:
                    return (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.FullModify)).Succeeded;

                case SystemTypeCode.Shared:
                    return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.DocumentVerificationModify)).Succeeded
                    || (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.DocumentVerificationModify)).Succeeded;

                default:
                    return false;
            }
        }

        private async Task<bool> HasRemarksOnlyPermission(string systemType)
        {
            switch (systemType)
            {
                case SystemTypeCode.Patent:
                    return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.RemarksOnlyModify)).Succeeded;

                case SystemTypeCode.Trademark:
                    return (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.RemarksOnlyModify)).Succeeded;

                case SystemTypeCode.Shared:
                    return (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.DocumentVerificationModify)).Succeeded
                    || (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.DocumentVerificationModify)).Succeeded;

                default:
                    return false;
            }
        }

        private async Task AddSecurityPolicies(string systemType, DetailPageViewModel<QuickEmailSetupDetailViewModel> viewModel)
        {
            switch (systemType)
            {
                case SystemTypeCode.Patent:
                    viewModel.AddPatentSecurityPolicies();
                    break;

                case SystemTypeCode.Trademark:
                    viewModel.AddTrademarkSecurityPolicies();
                    break;

                case SystemTypeCode.Shared:
                    var cacheKey = User.GetUserName() + ":QES-SystemType";
                    var cacheSystemType = await _distributedCache.GetStringAsync(cacheKey);
                    if (cacheSystemType == SystemTypeCode.Patent)
                        viewModel.AddPatentSecurityPolicies();
                    else if (cacheSystemType == SystemTypeCode.Trademark)
                        viewModel.AddTrademarkSecurityPolicies();
                    break;

                default:
                    viewModel.AddSharedSecurityPolicies();
                    break;
                
            }
        }

        #region Tags
        public async Task<IActionResult> GetQETags()
        {
            var tags = await _service.QETags.Select(t => t.Tag).Distinct().ToArrayAsync();
            return Json(tags);

        }

        public async Task<IActionResult> GetTagPickListData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_service.QETags.Select(s => new { Tag = s.Tag }), request, property, text, filterType, requiredRelation, false);
        }

        public async Task<IActionResult> QETagsRead([DataSourceRequest] DataSourceRequest request, int id)
        {
            var tags = await _service.QETags.Where(t => t.QESetupId == id).OrderBy(t => t.Tag).ToListAsync();
            return Json(tags.ToDataSourceResult(request));
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> QETagsUpdate([DataSourceRequest] DataSourceRequest request, int qeSetupID, byte[] tStamp,
            [Bind(Prefix = "updated")] IEnumerable<QETag> updated,
            [Bind(Prefix = "new")] IEnumerable<QETag> added,
            [Bind(Prefix = "deleted")] IEnumerable<QETag> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                foreach(var item in added)
                {
                    item.QETagId = 0;
                    item.QESetupId = qeSetupID;
                    UpdateEntityStamps(item, item.QETagId);
                }

                foreach (var item in updated)
                    UpdateEntityStamps(item, item.QETagId);

                await _service.TagsUpdateAsync(qeSetupID, User.GetUserName(), tStamp, updated,added, deleted);
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer["Quick Email Tag has been saved successfully."].ToString() :
                    _localizer["Quick Email Tags have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> QETagDelete([DataSourceRequest] DataSourceRequest request, int qeSetupID, byte[] tStamp, [Bind(Prefix = "deleted")] QETag deleted)
        {
            if (deleted.QETagId >= 0)
            {
                await _service.DeleteTagAsync(qeSetupID, User.GetUserName(), tStamp, deleted);
                return Ok(new { success = _localizer["Quick Email Tag has been deleted successfully."].ToString() });
            }
            return Ok();
        }
        #endregion

    }
}
