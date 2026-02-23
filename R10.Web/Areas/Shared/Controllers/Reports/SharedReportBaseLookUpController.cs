using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Interfaces.Patent;
using R10.Core.Entities.Patent;
using Kendo.Mvc.Extensions;
using R10.Web.Helpers;
using Microsoft.EntityFrameworkCore;
using R10.Web.Security;
using Microsoft.AspNetCore.Authorization;
using R10.Core.Interfaces;
using R10.Web.Interfaces.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.DMS;
using R10.Core.Entities.AMS;
using R10.Core.Interfaces.DMS;
using R10.Core.Interfaces.AMS;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers.Reports
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class SharedReportBaseLookUpController : BaseController
    {
        protected readonly ICountryApplicationService _applicationService;
        protected readonly IInventionService _inventionService;
        protected readonly ISharedReportViewModelService _sharedReportViewModelService;
        protected readonly ITmkTrademarkService _trademarkService;
        protected readonly IGMMatterService _gmMatterService;
        protected readonly IDisclosureService _disclosureService;
        protected readonly IAMSDueService _amsDueService;
        private readonly ISystemSettings<PatSetting> _patSettings;
        protected readonly IMultipleEntityService<Invention, PatOwnerInv> _patOwnerInvService;
        protected readonly IMultipleEntityService<PatOwnerApp> _patOwnerAppService;
        protected readonly IEntityService<TmkOwner> _tmkOwnerService;
        protected readonly IMultipleEntityService<GMMatter, GMMatterAttorney> _matterAttorneyService;
        protected readonly IGMMatterCountryService _matterCountryService;

        protected readonly IDueDateService<PatActionDue, PatDueDate> _patDueDateService;
        protected readonly IDueDateService<TmkActionDue, TmkDueDate> _tmkDueDateService;
        protected readonly IDueDateService<GMActionDue, GMDueDate> _gmDueDateService;
        protected readonly IDueDateService<DMSActionDue, DMSDueDate> _dmsDueDateService;

        protected IQueryable<Invention> Inventions;
        protected IQueryable<CountryApplication> CountryApplications;
        protected IQueryable<TmkTrademark> TmkTrademarks;
        protected IQueryable<GMMatter> GMMatters;
        protected IQueryable<Disclosure> Disclosures;
        protected IQueryable<AMSProjection> AMSProjections;

        public SharedReportBaseLookUpController(
            IInventionService inventionService,
            ICountryApplicationService applicationService,
            ISharedReportViewModelService sharedReportViewModelService,
            ITmkTrademarkService trademarkService,
            IGMMatterService gmMatterService,
            IDisclosureService disclosureService,
            IAMSDueService amsDueService,
            ISystemSettings<PatSetting> patSettings,
            IMultipleEntityService<Invention, PatOwnerInv> patOwnerInvService,
            IMultipleEntityService<PatOwnerApp> patOwnerAppService,
            IEntityService<TmkOwner> tmkOwnerService,
            IMultipleEntityService<GMMatter, GMMatterAttorney> matterAttorneyService,
            IGMMatterCountryService matterCountryService

            , IDueDateService<PatActionDue, PatDueDate> patDueDateService
            , IDueDateService<TmkActionDue, TmkDueDate> tmkDueDateService
            , IDueDateService<GMActionDue, GMDueDate> gmDueDateService
            , IDueDateService<DMSActionDue, DMSDueDate> dmsDueDateService)
        {
            _inventionService = inventionService;
            _applicationService = applicationService;
            _sharedReportViewModelService = sharedReportViewModelService;
            _trademarkService = trademarkService;
            _gmMatterService = gmMatterService;
            _disclosureService = disclosureService;
            _amsDueService = amsDueService;
            _patSettings = patSettings;
            _patOwnerInvService = patOwnerInvService;
            _inventionService = inventionService;
            _applicationService = applicationService; _patOwnerAppService = patOwnerAppService;
            _tmkOwnerService = tmkOwnerService;
            _matterAttorneyService = matterAttorneyService;
            _matterCountryService = matterCountryService;

            Inventions = inventionService.QueryableList;
            CountryApplications = applicationService.CountryApplications;
            TmkTrademarks = trademarkService.TmkTrademarks;
            GMMatters = gmMatterService.QueryableList;
            Disclosures = _disclosureService.QueryableList;
            AMSProjections = _amsDueService.ProjectionList;
            _patDueDateService = patDueDateService;
            _tmkDueDateService = tmkDueDateService;
            _gmDueDateService = gmDueDateService;
            _dmsDueDateService = dmsDueDateService;
        }

        public async Task<IActionResult> GetCountryList(string property, string text, FilterType filterType)
        {
            IList<SharedEntity> countries = new List<SharedEntity>();
            countries.AddRange(CountryApplications.Select(c => new SharedEntity { Id = null, Code = c.PatCountry.Country, Name = c.PatCountry.CountryName }).Distinct().ToList());
            countries.AddRange(TmkTrademarks.Select(c => new SharedEntity { Id = null, Code = c.TmkCountry.Country, Name = c.TmkCountry.CountryName }).Distinct().ToList());
            countries.AddRange(_matterCountryService.QueryableList.Where(c => GMMatters.Any(gm => gm.MatId == c.MatId)).Select(c => new SharedEntity { Id = null, Code = c.GMCountry.Country, Name = c.GMCountry.CountryName }).Distinct().ToList());
            countries.AddRange(AMSProjections.Select(c => new SharedEntity { Id = null, Code = c.AMSMain.CountryApplication == null ? c.AMSMain.Country : c.AMSMain.CountryApplication.Country, Name = null }).Distinct().ToList());

            //var result = _sharedReportViewModelService.GetCombinedCountries;
            //result = result.Where(c => countries.Any(e => e.Code == c.Country));

            if (property.Equals("Country"))
                return Json(countries.Select(c => new { Country = c.Code, CountryName = c.Name }).DistinctBy(c=>c.Country).Where(c => !String.IsNullOrEmpty(c.Country)).OrderBy(c => c.Country).ToList());
            else
                return Json(countries.Select(c => new { Country = c.Code, CountryName = c.Name }).DistinctBy(c => c.CountryName).Where(c => !String.IsNullOrEmpty(c.CountryName)).OrderBy(c => c.CountryName).ToList());
        }

        public async Task<IActionResult> GetClientList(string property, string text, FilterType filterType)
        {
            IList<SharedEntity> clientEntities = new List<SharedEntity>();
            clientEntities.AddRange(Inventions.Where(c => c.ClientID != null).Select(c => new SharedEntity { Id = c.Client.ClientID, Code = c.Client.ClientCode, Name = c.Client.ClientName }).Distinct().ToList());
            clientEntities.AddRange(TmkTrademarks.Where(c => c.ClientID != null).Select(c => new SharedEntity { Id = c.Client.ClientID, Code = c.Client.ClientCode, Name = c.Client.ClientName }).Distinct().ToList());
            clientEntities.AddRange(GMMatters.Where(c => c.ClientID != null).Select(c => new SharedEntity { Id = c.Client.ClientID, Code = c.Client.ClientCode, Name = c.Client.ClientName }).Distinct().ToList());
            clientEntities.AddRange(Disclosures.Where(c => c.ClientID != null).Select(c => new SharedEntity { Id = c.Client.ClientID, Code = c.Client.ClientCode, Name = c.Client.ClientName }).Distinct().ToList());
            clientEntities.AddRange(AMSProjections.Select(c => new SharedEntity { Id = c.AMSMain.CountryApplication == null ? 0 : c.AMSMain.CountryApplication.Invention.Client.ClientID, Code = c.AMSMain.CountryApplication == null ? c.AMSMain.CPIClient : c.AMSMain.CountryApplication.Invention.Client.ClientCode, Name = c.AMSMain.CountryApplication == null ? "" : c.AMSMain.CountryApplication.Invention.Client.ClientName }).Distinct().ToList());

            //var clients = _sharedReportViewModelService.GetCombinedClients;
            //var clients2 = clients.Where(c => clientEntities.Any(e=> e.Id==c.ClientID || e.Code==c.ClientCode));

            //if (property.Equals("ClientCode"))
            //    return Json(await clientEntities.Select(c => new { ClientID = c.ClientID, ClientCode = c.ClientCode, ClientName = c.ClientName }).OrderBy(c => c.ClientCode).ToListAsync());
            //else
            //    return Json(await clientEntities.Select(c => new { ClientID = c.ClientID, ClientCode = c.ClientCode, ClientName = c.ClientName }).Where(c=>!String.IsNullOrEmpty(c.ClientName)).OrderBy(c => c.ClientName).ToListAsync());
            if (property.Equals("ClientCode"))
                return Json(clientEntities.Select(c => new { ClientID = c.Id, ClientCode = c.Code, ClientName = c.Name }).DistinctBy(c => c.ClientCode).Where(c => !String.IsNullOrEmpty(c.ClientCode)).OrderBy(c => c.ClientCode).ToList());
            else
                return Json(clientEntities.Select(c => new { ClientID = c.Id, ClientCode = c.Code, ClientName = c.Name }).DistinctBy(c => c.ClientName).Where(c => !String.IsNullOrEmpty(c.ClientName)).OrderBy(c => c.ClientName).ToList());
        }

        public async Task<IActionResult> GetOwnerList(string property, string text, FilterType filterType)
        {
            IList<SharedEntity> ownerEntities = new List<SharedEntity>();
            if (!(await _patSettings.GetSetting()).IsPatCtryAppOwnerOn)
            {
                ownerEntities.AddRange(_patOwnerInvService.QueryableList.Where(c => Inventions.Any(i => i.InvId == c.InvId)).Select(c => new SharedEntity { Id = c.Owner.OwnerID, Code = c.Owner.OwnerCode, Name = c.Owner.OwnerName }).Distinct().ToList());
            }
            else
            {
                ownerEntities.AddRange(_patOwnerAppService.QueryableListWithEntityFilter.Where(c => CountryApplications.Any(ca => ca.AppId == c.AppId)).Select(c => new SharedEntity { Id = c.Owner.OwnerID, Code = c.Owner.OwnerCode, Name = c.Owner.OwnerName }).Distinct().ToList());
            }

            ownerEntities.AddRange(_tmkOwnerService.QueryableList.Where(c => TmkTrademarks.Any(tmk => tmk.TmkId == c.TmkID)).Select(c => new SharedEntity { Id = c.Owner.OwnerID, Code = c.Owner.OwnerCode, Name = c.Owner.OwnerName }).Distinct().ToList());
            ownerEntities.AddRange(AMSProjections.Select(c => new SharedEntity { Id = null, Code = null, Name = c.AMSMain.CountryApplication == null ? c.AMSMain.CPIOwner : null }).Distinct().ToList());

            //var owners = _sharedReportViewModelService.GetCombinedOwners;
            //owners = owners.Where(c => ownerEntities.Any(e => e.Id == c.OwnerID || e.Name == c.OwnerName));
            //if (property.Equals("OwnerCode"))
            //    return Json(await owners.Select(c => new { OwnerID = c.OwnerID, OwnerCode = c.OwnerCode, OwnerName = c.OwnerName }).OrderBy(c => c.OwnerCode).ToListAsync());
            //else
            //    return Json(await owners.Select(c => new { OwnerID = c.OwnerID, OwnerCode = c.OwnerCode, OwnerName = c.OwnerName }).Where(c => !String.IsNullOrEmpty(c.OwnerName)).OrderBy(c => c.OwnerName).ToListAsync());
            if (property.Equals("OwnerCode"))
                return Json(ownerEntities.Select(c => new { OwnerID = c.Id, OwnerCode = c.Code, OwnerName = c.Name }).DistinctBy(c => c.OwnerCode).Where(c => !String.IsNullOrEmpty(c.OwnerCode)).OrderBy(c => c.OwnerCode).ToList());
            else
                return Json(ownerEntities.Select(c => new { OwnerID = c.Id, OwnerCode = c.Code, OwnerName = c.Name }).DistinctBy(c => c.OwnerName).Where(c => !String.IsNullOrEmpty(c.OwnerName)).OrderBy(c => c.OwnerName).ToList());
        }

        public async Task<IActionResult> GetAttorneyList(string property, string text, FilterType filterType)
        {
            IList<SharedEntity> attorneyEntities = new List<SharedEntity>();
            attorneyEntities.AddRange(Inventions.Where(c => c.Attorney1ID != null).Select(c => new SharedEntity { Id = c.Attorney1.AttorneyID, Code = c.Attorney1.AttorneyCode, Name = c.Attorney1.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(Inventions.Where(c => c.Attorney2ID != null).Select(c => new SharedEntity { Id = c.Attorney2.AttorneyID, Code = c.Attorney2.AttorneyCode, Name = c.Attorney2.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(Inventions.Where(c => c.Attorney3ID != null).Select(c => new SharedEntity { Id = c.Attorney3.AttorneyID, Code = c.Attorney3.AttorneyCode, Name = c.Attorney3.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(Inventions.Where(c => c.Attorney4ID != null).Select(c => new SharedEntity { Id = c.Attorney4.AttorneyID, Code = c.Attorney4.AttorneyCode, Name = c.Attorney4.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(Inventions.Where(c => c.Attorney5ID != null).Select(c => new SharedEntity { Id = c.Attorney5.AttorneyID, Code = c.Attorney5.AttorneyCode, Name = c.Attorney5.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(TmkTrademarks.Where(c => c.Attorney1ID != null).Select(c => new SharedEntity { Id = c.Attorney1.AttorneyID, Code = c.Attorney1.AttorneyCode, Name = c.Attorney1.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(TmkTrademarks.Where(c => c.Attorney2ID != null).Select(c => new SharedEntity { Id = c.Attorney2.AttorneyID, Code = c.Attorney2.AttorneyCode, Name = c.Attorney2.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(TmkTrademarks.Where(c => c.Attorney3ID != null).Select(c => new SharedEntity { Id = c.Attorney3.AttorneyID, Code = c.Attorney3.AttorneyCode, Name = c.Attorney3.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(TmkTrademarks.Where(c => c.Attorney4ID != null).Select(c => new SharedEntity { Id = c.Attorney4.AttorneyID, Code = c.Attorney4.AttorneyCode, Name = c.Attorney4.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(TmkTrademarks.Where(c => c.Attorney5ID != null).Select(c => new SharedEntity { Id = c.Attorney5.AttorneyID, Code = c.Attorney5.AttorneyCode, Name = c.Attorney5.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(_matterAttorneyService.QueryableList.Where(c => GMMatters.Any(gm => gm.MatId == c.MatId)).Select(c => new SharedEntity { Id = c.Attorney.AttorneyID, Code = c.Attorney.AttorneyCode, Name = c.Attorney.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(Disclosures.Where(c => c.AttorneyID != null).Select(c => new SharedEntity { Id = c.Attorney.AttorneyID, Code = c.Attorney.AttorneyCode, Name = c.Attorney.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(AMSProjections.Select(c => new SharedEntity { Id = null, Code = c.AMSMain.CountryApplication == null ? c.AMSMain.CPIAttorney : null, Name = null }).Distinct().ToList());

            attorneyEntities.AddRange(_patDueDateService.QueryableList.Select(c => new SharedEntity { Id = c.PatActionDue.Responsible.AttorneyID, Code = c.PatActionDue.Responsible.AttorneyCode, Name = c.PatActionDue.Responsible.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(_patDueDateService.QueryableList.Select(c => new SharedEntity { Id = c.DueDateAttorney.AttorneyID, Code = c.DueDateAttorney.AttorneyCode, Name = c.DueDateAttorney.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(_tmkDueDateService.QueryableList.Select(c => new SharedEntity { Id = c.TmkActionDue.Responsible.AttorneyID, Code = c.TmkActionDue.Responsible.AttorneyCode, Name = c.TmkActionDue.Responsible.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(_tmkDueDateService.QueryableList.Select(c => new SharedEntity { Id = c.DueDateAttorney.AttorneyID, Code = c.DueDateAttorney.AttorneyCode, Name = c.DueDateAttorney.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(_gmDueDateService.QueryableList.Select(c => new SharedEntity { Id = c.GMActionDue.Responsible.AttorneyID, Code = c.GMActionDue.Responsible.AttorneyCode, Name = c.GMActionDue.Responsible.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(_gmDueDateService.QueryableList.Select(c => new SharedEntity { Id = c.DueDateAttorney.AttorneyID, Code = c.DueDateAttorney.AttorneyCode, Name = c.DueDateAttorney.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(_dmsDueDateService.QueryableList.Select(c => new SharedEntity { Id = c.DMSActionDue.Responsible.AttorneyID, Code = c.DMSActionDue.Responsible.AttorneyCode, Name = c.DMSActionDue.Responsible.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(_dmsDueDateService.QueryableList.Select(c => new SharedEntity { Id = c.DueDateAttorney.AttorneyID, Code = c.DueDateAttorney.AttorneyCode, Name = c.DueDateAttorney.AttorneyName }).Distinct().ToList());

            //var attorneys = _sharedReportViewModelService.GetCombinedAttorneys;
            //attorneys = attorneys.Where(c => attorneyEntities.Any(e => e.Id == c.AttorneyID || e.Code == c.AttorneyCode));

            if (property.Equals("AttorneyCode"))
                return Json(attorneyEntities.Select(c => new { AttorneyID = c.Id, AttorneyCode = c.Code, AttorneyName = c.Name }).DistinctBy(c => c.AttorneyCode).Where(c => !String.IsNullOrEmpty(c.AttorneyCode)).OrderBy(c => c.AttorneyCode).ToList());
            else
                return Json(attorneyEntities.Select(c => new { AttorneyID = c.Id, AttorneyCode = c.Code, AttorneyName = c.Name }).DistinctBy(c => c.AttorneyName).Where(c => !String.IsNullOrEmpty(c.AttorneyName)).OrderBy(c => c.AttorneyName).ToList());
        }

        public async Task<IActionResult> GetAgentList(string property, string text, FilterType filterType)
        {
            IList<SharedEntity> agentEntities = new List<SharedEntity>();
            agentEntities.AddRange(CountryApplications.Where(c => c.AgentID != null).Select(c => new SharedEntity { Id = c.Agent.AgentID, Code = c.Agent.AgentCode, Name = c.Agent.AgentName }).Distinct().ToList());
            agentEntities.AddRange(TmkTrademarks.Where(c => c.AgentID != null).Select(c => new SharedEntity { Id = c.Agent.AgentID, Code = c.Agent.AgentCode, Name = c.Agent.AgentName }).Distinct().ToList());
            agentEntities.AddRange(GMMatters.Where(c => c.AgentID != null).Select(c => new SharedEntity { Id = c.Agent.AgentID, Code = c.Agent.AgentCode, Name = c.Agent.AgentName }).Distinct().ToList());
            agentEntities.AddRange(AMSProjections.Select(c => new SharedEntity { Id = null, Code = c.AMSMain.CountryApplication == null ? c.AMSMain.CPIAgent : null, Name = null }).Distinct().ToList());

            //var Agents = _sharedReportViewModelService.GetCombinedAgents;
            //Agents = Agents.Where(c => agentEntities.Any(e => e.Id == c.AgentID || e.Code == c.AgentCode));

            if (property.Equals("AgentCode"))
                return Json( agentEntities.Select(c => new { AgentID = c.Id, AgentCode = c.Code, AgentName = c.Name }).DistinctBy(c => c.AgentCode).Where(c => !String.IsNullOrEmpty(c.AgentCode)).OrderBy(c => c.AgentCode).ToList());
            else
                return Json( agentEntities.Select(c => new { AgentID = c.Id, AgentCode = c.Code, AgentName = c.Name }).DistinctBy(c => c.AgentName).Where(c => !String.IsNullOrEmpty(c.AgentName)).OrderBy(c => c.AgentName).ToList());
        }

        public async Task<IActionResult> GetAreaList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var areas = _sharedReportViewModelService.GetCombinedAreas;
            return Json(await QueryHelper.GetPicklistDataAsync(areas, property, text, filterType, requiredRelation));
        }

        public IActionResult GetCaseTypeList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            IList<SharedEntity> caseTypes = new List<SharedEntity>();
            caseTypes.AddRange(CountryApplications.Select(c => new SharedEntity { Id = null, Code = c.CaseType, Name = null }).Distinct().ToList());
            caseTypes.AddRange(TmkTrademarks.Select(c => new SharedEntity { Id = null, Code = c.CaseType, Name = null }).Distinct().ToList());
            caseTypes.AddRange(GMMatters.Select(c => new SharedEntity { Id = null, Code = c.MatterType, Name = null }).Distinct().ToList());
            caseTypes.AddRange(AMSProjections.Select(c => new SharedEntity { Id = null, Code = c.AMSMain.CountryApplication == null ? c.AMSMain.CPICaseType : c.AMSMain.CountryApplication.CaseType, Name = null }).Distinct().ToList());

            var result = caseTypes.Select(c => new { CaseType = c.Code }).Distinct().Where(c => c.CaseType != null).ToList();
            return Json(result);
        }

        public IActionResult GetRespOfficeList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            IList<SharedEntity> responsibleOffices = new List<SharedEntity>();
            responsibleOffices.AddRange(CountryApplications.Select(c => new SharedEntity { Id = null, Code = c.RespOffice, Name = null }).Distinct().ToList());
            responsibleOffices.AddRange(TmkTrademarks.Select(c => new SharedEntity { Id = null, Code = c.RespOffice, Name = null }).Distinct().ToList());
            responsibleOffices.AddRange(GMMatters.Select(c => new SharedEntity { Id = null, Code = c.RespOffice, Name = null }).Distinct().ToList());
            responsibleOffices.AddRange(AMSProjections.Select(c => new SharedEntity { Id = null, Code = c.AMSMain.CPIClientCode }).Distinct().ToList());

            var result = responsibleOffices.Select(c => new { RespOffice = c.Code }).Distinct().Where(c => c.RespOffice != null).ToList();
            return Json(result);
        }

        public async Task<IActionResult> GetCaseNumberList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            IList<SharedEntity> caseNumbers = new List<SharedEntity>();
            caseNumbers.AddRange(Inventions.Select(c => new SharedEntity { Id = null, Code = c.CaseNumber, Name = null }).Distinct().ToList());
            caseNumbers.AddRange(TmkTrademarks.Select(c => new SharedEntity { Id = null, Code = c.CaseNumber, Name = null }).Distinct().ToList());
            caseNumbers.AddRange(GMMatters.Select(c => new SharedEntity { Id = null, Code = c.CaseNumber, Name = null }).Distinct().ToList());
            caseNumbers.AddRange(Disclosures.Select(c => new SharedEntity { Id = null, Code = c.DisclosureNumber, Name = null }).Distinct().ToList());
            caseNumbers.AddRange(AMSProjections.Select(c => new SharedEntity { Id = null, Code = c.AMSMain.CaseNumber, Name = null }).Distinct().ToList());

            var result = caseNumbers.Where(c => c.Code.ToUpper().StartsWith(text == null ? "" : text.ToUpper())).Select(c => new { CaseNumber = c.Code }).Distinct().ToList();

            if (request.PageSize > 0)
            {
                request.Filters.Clear();
                return Json(await result.ToDataSourceResultAsync(request));
            }

            var list = result;
            return Json(list);
        }

        private class CaseNumberHolder
        {
            public string CaseNumber { get; set; }
        }

        public class SharedEntity
        {
            public int? Id { get; set; }
            public string? Code { get; set; }
            public string? Name { get; set; }
        }
    }
}