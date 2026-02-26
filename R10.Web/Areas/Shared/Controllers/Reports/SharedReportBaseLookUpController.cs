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
        private readonly ISystemSettings<PatSetting> _patSettings;
        protected readonly IMultipleEntityService<Invention, PatOwnerInv> _patOwnerInvService;
        protected readonly IMultipleEntityService<PatOwnerApp> _patOwnerAppService;
        protected readonly IEntityService<TmkOwner> _tmkOwnerService;

        protected readonly IDueDateService<PatActionDue, PatDueDate> _patDueDateService;
        protected readonly IDueDateService<TmkActionDue, TmkDueDate> _tmkDueDateService;

        protected IQueryable<Invention> Inventions;
        protected IQueryable<CountryApplication> CountryApplications;
        protected IQueryable<TmkTrademark> TmkTrademarks;

        public SharedReportBaseLookUpController(
            IInventionService inventionService,
            ICountryApplicationService applicationService,
            ISharedReportViewModelService sharedReportViewModelService,
            ITmkTrademarkService trademarkService,
            ISystemSettings<PatSetting> patSettings,
            IMultipleEntityService<Invention, PatOwnerInv> patOwnerInvService,
            IMultipleEntityService<PatOwnerApp> patOwnerAppService,
            IEntityService<TmkOwner> tmkOwnerService,
            IDueDateService<PatActionDue, PatDueDate> patDueDateService,
            IDueDateService<TmkActionDue, TmkDueDate> tmkDueDateService)
        {
            _inventionService = inventionService;
            _applicationService = applicationService;
            _sharedReportViewModelService = sharedReportViewModelService;
            _trademarkService = trademarkService;
            _patSettings = patSettings;
            _patOwnerInvService = patOwnerInvService;
            _patOwnerAppService = patOwnerAppService;
            _tmkOwnerService = tmkOwnerService;

            Inventions = inventionService.QueryableList;
            CountryApplications = applicationService.CountryApplications;
            TmkTrademarks = trademarkService.TmkTrademarks;
            _patDueDateService = patDueDateService;
            _tmkDueDateService = tmkDueDateService;
        }

        public async Task<IActionResult> GetCountryList(string property, string text, FilterType filterType)
        {
            IList<SharedEntity> countries = new List<SharedEntity>();
            countries.AddRange(CountryApplications.Select(c => new SharedEntity { Id = null, Code = c.PatCountry.Country, Name = c.PatCountry.CountryName }).Distinct().ToList());
            countries.AddRange(TmkTrademarks.Select(c => new SharedEntity { Id = null, Code = c.TmkCountry.Country, Name = c.TmkCountry.CountryName }).Distinct().ToList());

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

            attorneyEntities.AddRange(_patDueDateService.QueryableList.Select(c => new SharedEntity { Id = c.PatActionDue.Responsible.AttorneyID, Code = c.PatActionDue.Responsible.AttorneyCode, Name = c.PatActionDue.Responsible.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(_patDueDateService.QueryableList.Select(c => new SharedEntity { Id = c.DueDateAttorney.AttorneyID, Code = c.DueDateAttorney.AttorneyCode, Name = c.DueDateAttorney.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(_tmkDueDateService.QueryableList.Select(c => new SharedEntity { Id = c.TmkActionDue.Responsible.AttorneyID, Code = c.TmkActionDue.Responsible.AttorneyCode, Name = c.TmkActionDue.Responsible.AttorneyName }).Distinct().ToList());
            attorneyEntities.AddRange(_tmkDueDateService.QueryableList.Select(c => new SharedEntity { Id = c.DueDateAttorney.AttorneyID, Code = c.DueDateAttorney.AttorneyCode, Name = c.DueDateAttorney.AttorneyName }).Distinct().ToList());

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

            var result = caseTypes.Select(c => new { CaseType = c.Code }).Distinct().Where(c => c.CaseType != null).ToList();
            return Json(result);
        }

        public IActionResult GetRespOfficeList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            IList<SharedEntity> responsibleOffices = new List<SharedEntity>();
            responsibleOffices.AddRange(CountryApplications.Select(c => new SharedEntity { Id = null, Code = c.RespOffice, Name = null }).Distinct().ToList());
            responsibleOffices.AddRange(TmkTrademarks.Select(c => new SharedEntity { Id = null, Code = c.RespOffice, Name = null }).Distinct().ToList());

            var result = responsibleOffices.Select(c => new { RespOffice = c.Code }).Distinct().Where(c => c.RespOffice != null).ToList();
            return Json(result);
        }

        public async Task<IActionResult> GetCaseNumberList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            IList<SharedEntity> caseNumbers = new List<SharedEntity>();
            caseNumbers.AddRange(Inventions.Select(c => new SharedEntity { Id = null, Code = c.CaseNumber, Name = null }).Distinct().ToList());
            caseNumbers.AddRange(TmkTrademarks.Select(c => new SharedEntity { Id = null, Code = c.CaseNumber, Name = null }).Distinct().ToList());

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
