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
using R10.Web.Models.PageViewModels;
using R10.Web.Models;
using R10.Core.Entities.Patent;
using R10.Core.Helpers;
using R10.Core;
using R10.Core.Exceptions;
using AutoMapper.QueryableExtensions;
using R10.Core.Entities.Shared;
using R10.Web.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using R10.Core.Entities.Trademark;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class AgentController :BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IAgentViewModelService _agentViewModelService;
        private readonly IAgentService _agentService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ICountryLookupViewModelService _countryLookupService;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly IReportService _reportService;
        private readonly IChildEntityService<Agent, AgentCEFee> _agentCEFeeService;
        private readonly IEntityService<PatCostType> _patCostTypeService;
        private readonly IEntityService<TmkCostType> _tmkCostTypeService;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;

        private readonly string _dataContainer = "agentDetail";

        public AgentController(
            IAuthorizationService authService,
            IAgentViewModelService agentViewModelService,
            IAgentService agentService, 
            IMapper mapper,
            IStringLocalizer<SharedResource> localizer,
            ICountryLookupViewModelService countryLookupService,
            IReportService reportService,
            ISystemSettings<DefaultSetting> settings,
            IChildEntityService<Agent, AgentCEFee> agentCEFeeService,
            IEntityService<PatCostType> patCostTypeService,
            IEntityService<TmkCostType> tmkCostTypeService,
            ISystemSettings<PatSetting> patSettings,
            ISystemSettings<TmkSetting> tmkSettings)
        {
            _authService = authService;
            _agentViewModelService = agentViewModelService;
            _agentService = agentService;
            _mapper = mapper;
            _localizer = localizer;
            _countryLookupService = countryLookupService;
            _settings = settings;
            _reportService = reportService;
            _agentCEFeeService = agentCEFeeService;
            _patCostTypeService = patCostTypeService;
            _tmkCostTypeService = tmkCostTypeService;
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
        }

        public async Task<IActionResult> Index()
        {
            var agentLabel = (await _settings.GetSetting()).LabelAgent;

            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "agentSearch",
                Title = _localizer[$"{agentLabel} Search"].ToString(),
                CanAddRecord = await CanAddRecord()
            };

            var settings = await _settings.GetSetting();
            if (settings.IsShowCustomFieldOn)
            {
                ViewBag.SysCustomFieldSettings = await _agentService.GetCustomFields();
            }

            if (Request.IsAjax())
                return PartialView("Index", model);            

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var agentLabel = (await _settings.GetSetting()).LabelAgent;

            var model = new PageViewModel()
            {
                Page = PageType.SearchResults,
                PageId = "agentSearchResults",
                Title = _localizer[$"{agentLabel} Search Results"].ToString(),
                CanAddRecord = await CanAddRecord()
            };

            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search()
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var agents = _agentViewModelService.AddCriteria(this.Agents, mainSearchFilters);
                var result = await _agentViewModelService.CreateViewModelForGrid(request, agents);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost()]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName) {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        //standardize method signature to simplify ajax call
        [HttpGet()]
        public async  Task<IActionResult> DetailLink(int? id, string code)
        {
            if (id == null && !string.IsNullOrEmpty(code))
                id = await Agents.Where(a => a.AgentCode == code).Select(a => a.AgentID).FirstOrDefaultAsync();

            if (id > 0)
                return RedirectToAction(nameof(Detail), new { id = id, singleRecord = true, fromSearch = true });
            else if ((await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded)
                return RedirectToAction(nameof(Add), new { fromSearch = true, code = code });
            else
                return new RecordDoesNotExistResult();
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            var agentLabel = (await _settings.GetSetting()).LabelAgent;

            var page = await PrepareEditScreen(this.Agents, id);
            if (page.Detail == null)
            {
                Guard.Against.NoRecordPermission(!Request.IsAjax());
                return RedirectToAction("Index");
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer[$"{agentLabel} Detail"].ToString(),
                RecordId = detail.AgentID,
                SingleRecord = singleRecord || !Request.IsAjax(),
                ActiveTab = tab,
                PagePermission = page,
                Data = detail
            };

            if (Request.IsAjax())
            {
                if (!singleRecord && !fromSearch)
                    model.Page = PageType.DetailContent;

                return PartialView("Index", model);
            }

            return View("Index", model);
        }

        [HttpGet]
        public IActionResult Print()
        {
            ViewBag.Url = Url.Action("Print");
            ViewBag.DownloadName = "Agent Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintViewModel agentPrintModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(agentPrintModel, ReportType.SharedAgentPrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string code = "")
        {
            var agentLabel = (await _settings.GetSetting()).LabelAgent;

            if (!Request.IsAjax())
                return RedirectToAction("Index");

            //do not allow add if user entity filter type is client
            //user won't be able to access new record
            Guard.Against.UnAuthorizedAccess(await CanAddRecord());

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");
            else if (!string.IsNullOrEmpty(code))
                page.Detail.AgentCode = code;

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer[$"New {agentLabel}"].ToString(),
                RecordId = detail.AgentID,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };

            return PartialView("Index", model);
        }

        //standardize method signature to simplify ajax call
        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.CanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string tStamp)
        {
            var agent = await _agentService.GetByIdAsync(id); //we need the agentcode as alternate key
            agent.tStamp = Convert.FromBase64String(tStamp);
            await _agentService.Delete(agent);
            return Ok();
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] Agent agent)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(agent, agent.AgentID);

                if (agent.AgentID > 0)
                    await _agentService.Update(agent);
                else
                    await _agentService.Add(agent);

                return Json(agent.AgentID);
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.RemarksOnlyModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRemarks([FromBody] Agent agent)
        {
            UpdateEntityStamps(agent, agent.AgentID);
            await _agentService.UpdateRemarks(agent);
            return Json(agent.AgentID);
        }

        public async Task<IActionResult> GetRecordStamps(int id) {
            var agent = await _agentService.GetByIdAsync(id);
            if (agent==null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new {createdBy=agent.CreatedBy, dateCreated = agent.DateCreated, updatedBy = agent.UpdatedBy, lastUpdate=agent.LastUpdate, tStamp=agent.tStamp});
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> ValueMapper(string value)
        {
            var agent = await this.Agents.FirstOrDefaultAsync(a => a.AgentCode == value);
            return Json(agent);
        }

        //NOT USED ??
        //public async Task<IActionResult> ContactRead([DataSourceRequest] DataSourceRequest request, int agentId)
        //{
        //    var result = (await _agentViewModelService.GetAgentContacts(agentId)).ToDataSourceResult(request);
        //    return Json(result);
        //}

        private IQueryable<Agent> Agents => _agentService.QueryableList;

        private async Task<DetailPageViewModel<AgentDetailViewModel>> PrepareEditScreen(IQueryable<Agent> agents, int id)
        {
            var viewModel = new DetailPageViewModel<AgentDetailViewModel>();
            viewModel.Detail = await _agentViewModelService.CreateViewModelForDetailScreen(id);

            if (viewModel.Detail != null) {
                viewModel.AddSharedSecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                //do not allow add if user entity filter type is client
                //user won't be able to access new record
                viewModel.CanAddRecord = await CanAddRecord();

                //hide copy and email buttons
                viewModel.CanCopyRecord = false;
                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.SearchScreenUrl = this.Url.Action("Index");
                viewModel.Container = _dataContainer;

                var setting = await _settings.GetSetting();
                if (setting.IsShowCustomFieldOn)
                {
                    viewModel.Detail.SysCustomFieldSettings = await _agentService.GetCustomFields();
                }
            }
            return viewModel;
        }

        private async Task<DetailPageViewModel<AgentDetailViewModel>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<AgentDetailViewModel>();
            viewModel.Detail = new AgentDetailViewModel();

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            var setting = await _settings.GetSetting();
            if (setting.IsShowCustomFieldOn)
            {
                viewModel.Detail.SysCustomFieldSettings = await _agentService.GetCustomFields();
            }
            return viewModel;
        }

        private async Task<bool> CanAddRecord()
        {
            //do not allow add if user entity filter type is client
            //user won't be able to access new record
            return (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded &&
                User.GetEntityFilterType() != CPiEntityType.Agent;
        }

        //TODO: USE GetAgentList
        public async Task<IActionResult> GetAgentsList(string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var agents = this.Agents;

            agents = QueryHelper.BuildCriteria(agents, property, text, filterType, requiredRelation).OrderBy(property);
            var list = await agents.Select(a => new { AgentID = a.AgentID, AgentCode = a.AgentCode, AgentName = a.AgentName }).ToListAsync();
            return Json(list);
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(Agents, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetAgentList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var agents = Agents.Where(c => (bool)c.IsActive);
            return await GetPicklistData(agents, request, property, text, filterType, new string[] { "AgentID", "AgentCode", "AgentName","City","Country" }, requiredRelation);
            //requiredRelation won't work if already projected to viewmodel
            //return await GetPicklistData(Agents.ProjectTo<AgentListViewModel>(), request, property, text, filterType, requiredRelation, false);
        }

        public async Task<IActionResult> GetContactList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var agentContacts = _agentService.ChildService.QueryableList.Where(w => Agents.Any(a => a.AgentID == w.AgentID));
            IQueryable<ContactListViewModel> contacts;

            if (property.ToUpper() == "CONTACTNAME")
                contacts = agentContacts.Select(s => new ContactListViewModel { ContactName = s.Contact.ContactName });
            else
                contacts = agentContacts.Select(s => new ContactListViewModel { Contact = s.Contact.Contact, ContactName = s.Contact.ContactName });

            contacts = contacts.Distinct().OrderBy(property).BuildCriteria(property, text, filterType, requiredRelation);
            return await GetPicklistData(contacts, request);
        }

        public async Task<IActionResult> GetCountryList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType)
        {
            var countries = _countryLookupService.Countries
                                    .Where(w => Agents.Any(a => a.Country == w.Country))
                                    .Distinct().OrderBy(property)
                                    .BuildCriteria(property, text, filterType);
            return await GetPicklistData(countries, request);
        }

        public async Task<IActionResult> GetPOCountryList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType)
        {
            var countries = _countryLookupService.Countries
                                    .Where(w => Agents.Any(a => a.POCountry == w.Country))
                                    .Distinct().OrderBy(property)
                                    .BuildCriteria(property, text, filterType);
            return await GetPicklistData(countries, request);
        }

        #region Agent Contacts
        public async Task<IActionResult> ContactsRead([DataSourceRequest] DataSourceRequest request, int agentId)
        {
            var result = (await _agentViewModelService.GetAgentContacts(agentId)).ToDataSourceResult(request);
            return Json(result);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> ContactsUpdate(int agentId,
            [Bind(Prefix = "updated")]IEnumerable<AgentContactViewModel> updated,
            [Bind(Prefix = "new")]IEnumerable<AgentContactViewModel> added,
            [Bind(Prefix = "deleted")]IEnumerable<AgentContactViewModel> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _agentService.ChildService.Update(agentId, User.GetUserName(),
                    _mapper.Map<List<AgentContact>>(updated),
                    _mapper.Map<List<AgentContact>>(added),
                    _mapper.Map<List<AgentContact>>(deleted)
                    );
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer["Contact has been saved successfully."].ToString() :
                    _localizer["Contacts have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> ContactsDelete([Bind(Prefix = "deleted")] AgentContactViewModel deleted)
        {
            if (deleted.AgentContactID >= 0)
            {
                await _agentService.ChildService.Update(deleted.AgentID, User.GetUserName(), new List<AgentContact>(), new List<AgentContact>(), new List<AgentContact>() { _mapper.Map<AgentContact>(deleted) });
                return Ok(new { success = _localizer["Contact has been deleted successfully."].ToString() });
            }
            return Ok();
        }
        #endregion

        #region Agent RMS Fees
        public async Task<IActionResult> CEFeesRead([DataSourceRequest] DataSourceRequest request, int agentId)
        {
            var data = await _agentCEFeeService.QueryableList.Where(d => d.AgentID == agentId).ToListAsync();
            data.ForEach(d => { 
                d.SystemTypeName = d.SystemType == SystemTypeCode.Patent ? SystemType.Patent : d.SystemType == SystemTypeCode.Trademark ? SystemType.Trademark : d.SystemType;
                d.CostFactors = string.Format("{0}, {1}, {2}", d.CostFactor1 ?? 0, (d.CostFactor2 ?? 0).FormatToDisplay(), d.CostFactor3 ?? 0);
            });
            return Json(data.ToDataSourceResult(request));
        }

        //[Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        //public async Task<IActionResult> CEFeesUpdate(int agentId,
        //    [Bind(Prefix = "updated")] IEnumerable<AgentCEFee> updated,
        //    [Bind(Prefix = "new")] IEnumerable<AgentCEFee> added,
        //    [Bind(Prefix = "deleted")] IEnumerable<AgentCEFee> deleted)
        //{
        //    if (updated.Any() || added.Any() || deleted.Any())
        //    {
        //        if (!ModelState.IsValid)
        //            return new JsonBadRequest(new { errors = ModelState.Errors() });                

        //        await _agentCEFeeService.Update(agentId, User.GetUserName(), updated, added, deleted);
        //        var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
        //            _localizer["Fee has been saved successfully."].ToString() :
        //            _localizer["Fees have been saved successfully"].ToString();
        //        return Ok(new { success = success });
        //    }
        //    return Ok();
        //}

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> CEFeesUpdate([DataSourceRequest] DataSourceRequest request, AgentCEFee agentCEFee, bool isUpdate, int agentId)
        {
            if (agentId != 0)
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                IEnumerable<AgentCEFee> added = Enumerable.Empty<AgentCEFee>();
                IEnumerable<AgentCEFee> updated = Enumerable.Empty<AgentCEFee>();
                IEnumerable<AgentCEFee> deleted = Enumerable.Empty<AgentCEFee>();

                if (agentCEFee.CostFactor1 > 0 || agentCEFee.CostFactor2 > 0 || agentCEFee.CostFactor3 > 0)
                    agentCEFee.CostFormula = "[Answer]*[CostFactor1]/[CostFactor3]*[CostFactor2]";
                else
                    agentCEFee.CostFormula = null;

                UpdateEntityStamps(agentCEFee, agentCEFee.FeeID);
                if (isUpdate)
                {
                    updated = new List<AgentCEFee>() { agentCEFee };
                }
                else
                {
                    added = new List<AgentCEFee>() { agentCEFee };
                }

                await _agentCEFeeService.Update(agentId, User.GetUserName(), updated, added, deleted);
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer["Fee has been saved successfully."].ToString() :
                    _localizer["Fees have been saved successfully."].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        //[Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        //public async Task<IActionResult> CEFeesDelete([Bind(Prefix = "deleted")] AgentCEFee deleted)
        //{
        //    if (deleted.FeeID >= 0)
        //    {
        //        await _agentCEFeeService.Update(deleted.AgentID, User.GetUserName(), new List<AgentCEFee>(), new List<AgentCEFee>(), new List<AgentCEFee>() { deleted });
        //        return Ok(new { success = _localizer["Fee has been deleted successfully."].ToString() });
        //    }
        //    return Ok();
        //}

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> CEFeesDelete([DataSourceRequest] DataSourceRequest request, AgentCEFee deleted)
        {
            if (deleted.FeeID >= 0)
            {
                await _agentCEFeeService.Update(deleted.AgentID, User.GetUserName(), new List<AgentCEFee>(), new List<AgentCEFee>(), new List<AgentCEFee>() { deleted });
                return Ok(new { success = _localizer["Fee has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        public async Task<IActionResult> GetSystemTypeList(string property, string text, FilterType filterType)
        {
            var data = new List<SelectListItem>() { new SelectListItem() { Text = " ", Value = " " } };
            var patSettings = await _patSettings.GetSetting();
            var tmkSettings = await _tmkSettings.GetSetting();
            if (patSettings.IsCostEstimatorOn)
                data.Add(new SelectListItem() { Text = SystemType.Patent, Value = SystemTypeCode.Patent });

            if (tmkSettings.IsCostEstimatorOn)
                data.Add(new SelectListItem() { Text = SystemType.Trademark, Value = SystemTypeCode.Trademark });

            return Json(data);
        }

        public async Task<IActionResult> GetCostTypeList(string property, string text, FilterType filterType)
        {
            var data = new List<SelectListItem>();

            var patSettings = await _patSettings.GetSetting();
            var tmkSettings = await _tmkSettings.GetSetting();
            if (patSettings.IsCostEstimatorOn)
                data.AddRange(await _patCostTypeService.QueryableList.AsNoTracking()
                    .Where(d => string.IsNullOrEmpty(text) || (!string.IsNullOrEmpty(d.CostType) && d.CostType.Contains(text)))
                    .Select(d => new SelectListItem() { Text = d.CostType, Value = d.CostType }).Distinct().ToListAsync());

            if (tmkSettings.IsCostEstimatorOn)
                data.AddRange(await _tmkCostTypeService.QueryableList.AsNoTracking()
                    .Where(d => string.IsNullOrEmpty(text) || (!string.IsNullOrEmpty(d.CostType) && d.CostType.Contains(text)))
                    .Select(d => new SelectListItem() { Text = d.CostType, Value = d.CostType }).Distinct().ToListAsync());

            // Only allow "Agent Fee" and "Translation" for now
            data.RemoveAll(d => string.IsNullOrEmpty(d.Text) || (d.Text != "Agent Fee" && d.Text != "Translation"));

            return Json(data.DistinctBy(d => d.Text).OrderBy(o => o.Text).ToList());
        }

        public IActionResult GetTranslationTypeList(string property, string text, FilterType filterType)
        {
            var result = new List<SelectListItem>();
            foreach (int i in Enum.GetValues(typeof(AgentCETranslationType)))
            {
                var dataType = (AgentCETranslationType)i;
                
                DisplayAttribute[] attributes = (DisplayAttribute[])dataType
                   .GetType()
                   .GetField(dataType.ToString())
                   .GetCustomAttributes(typeof(DisplayAttribute), false);
                if (attributes.Length > 0)
                {
                    result.Add(new SelectListItem { Text = attributes[0].Name, Value = i.ToString() });
                }
            }

            return Json(result);
        }
        #endregion
    }
}