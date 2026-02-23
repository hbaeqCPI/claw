using System;
using R10.Core.Interfaces;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces.Patent;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using R10.Core.Entities;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using R10.Core.Helpers;
using R10.Core.Exceptions;
using R10.Core.DTOs;

namespace R10.Core.Services
{
    public interface ICountryApplicationApiService : IWebApiBaseService<CountryApplicationWebSvc, CountryApplication>
    {
        IQueryable<PatCountry> Countries { get; }
        IQueryable<PatCaseType> CaseTypes { get; }
        IQueryable<PatActionDue> Actions {  get; }
        IQueryable<PatCostTrack> Costs { get; }
        IQueryable<PatApplicationStatus> Statuses { get; }
        IQueryable<RTSSearchUSIFW> RTSSearchUSIFWs { get; }
        Task<List<PatRelatedCaseDTO>> GetRelatedCases(int appId);
    }

    public class CountryApplicationApiService : WebApiBaseService<CountryApplicationWebSvc>, ICountryApplicationApiService
    {
        private readonly ICountryApplicationService _countryAppService;
        private readonly IInventionService _inventionService;
        private readonly IPatTaxStartExpirationService _taxStartExpirationService;
        private readonly ISystemSettings<PatSetting> _settings;

        public CountryApplicationApiService(
            ICountryApplicationService countryAppService,
            IInventionService inventionService,
            IPatTaxStartExpirationService taxStartExpirationService,
            ISystemSettings<PatSetting> settings,
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user)  : base(cpiDbContext, user)
        {
            _countryAppService = countryAppService;
            _inventionService = inventionService;
            _taxStartExpirationService = taxStartExpirationService;
            _settings = settings;
        }

        IQueryable<CountryApplication> IWebApiBaseService<CountryApplicationWebSvc, CountryApplication>.QueryableList => _countryAppService.CountryApplications;

        public IQueryable<PatCountry> Countries => _cpiDbContext.GetRepository<PatCountry>().QueryableList;

        public IQueryable<PatCaseType> CaseTypes => _cpiDbContext.GetRepository<PatCaseType>().QueryableList;

        public IQueryable<PatActionDue> Actions => _cpiDbContext.GetRepository<PatActionDue>().QueryableList;

        public IQueryable<PatCostTrack> Costs => _cpiDbContext.GetRepository<PatCostTrack>().QueryableList;

        public IQueryable<PatApplicationStatus> Statuses => _cpiDbContext.GetRepository<PatApplicationStatus>().QueryableList;

        public IQueryable<RTSSearchUSIFW> RTSSearchUSIFWs => _cpiDbContext.GetRepository<RTSSearchUSIFW>().QueryableList;

        public async Task<int> Add(CountryApplicationWebSvc webApiCountryApplication, DateTime runDate)
        {
            await ValidateCountryApplication(0, webApiCountryApplication, true);
            return await SaveCountryApplication(webApiCountryApplication, new CountryApplication(), runDate);
        }

        public async Task<List<int>> Import(List<CountryApplicationWebSvc> webApiCountryApplications, DateTime runDate)
        {
            await ValidateCountryApplications(webApiCountryApplications, true);

            var appIds = new List<int>();
            foreach (var webApiCountryApplication in webApiCountryApplications)
            {
                appIds.Add(await SaveCountryApplication(webApiCountryApplication, new CountryApplication(), runDate));
            }

            return appIds;
        }

        public async Task Update(int id, CountryApplicationWebSvc webApiCountryApplication, DateTime runDate)
        {
            await ValidateCountryApplication(id, webApiCountryApplication, false);

            var countryApplication = await _countryAppService.CountryApplications.FirstOrDefaultAsync(ca => ca.AppId == id);
            if (countryApplication != null)
                await SaveCountryApplication(webApiCountryApplication, countryApplication, runDate);
        }

        public async Task Update(List<CountryApplicationWebSvc> webApiCountryApplications, DateTime runDate)
        {
            await ValidateCountryApplications(webApiCountryApplications, false);
            foreach (var webApiCountryApplication in webApiCountryApplications)
            {
                var countryApplication = await _countryAppService.CountryApplications.FirstOrDefaultAsync(ca => ca.CaseNumber == webApiCountryApplication.CaseNumber && ca.Country == webApiCountryApplication.Country && ca.SubCase == webApiCountryApplication.SubCase);
                if (countryApplication != null)
                    await SaveCountryApplication(webApiCountryApplication, countryApplication, runDate);
            }
        }

        private async Task<int> SaveCountryApplication(CountryApplicationWebSvc webApiCountryApplication, CountryApplication countryApplication, DateTime runDate)
        {
            await SetData(webApiCountryApplication, countryApplication, runDate);

            if (countryApplication.AppId == 0)
                await _countryAppService.AddCountryApplication(countryApplication, null, runDate, false,"");
            else
                await _countryAppService.UpdateCountryApplication(countryApplication, null, runDate);

            return countryApplication.AppId;
        }

        private async Task ValidateCountryApplication(int id, CountryApplicationWebSvc webApiCountryApplication, bool forInsert)
        {
            try
            {
                await ValidateData(id, webApiCountryApplication, forInsert);
            }
            catch (Exception ex)
            {
                throw new WebApiValidationException(FormatErrorMessage(id, ex.Message, webApiCountryApplication.CaseNumber, webApiCountryApplication.Country, webApiCountryApplication.SubCase));
            }
        }

        private async Task ValidateCountryApplications(List<CountryApplicationWebSvc> webApiCountryApplications, bool forInsert)
        {
            var errors = new List<string>();
            var duplicates = webApiCountryApplications.GroupBy(c => new { c.CaseNumber, c.Country, c.SubCase }).Where(g => g.Count() > 1).ToList();

            if (duplicates.Count > 0)
                throw new WebApiValidationException("Duplicate records found.", duplicates.Select(d => $"{d.Key}").ToList());

            for (int i = 0; i < webApiCountryApplications.Count; i++)
            {
                var webApiCountryApplication = webApiCountryApplications[i];

                try
                {
                    await ValidateData(0, webApiCountryApplication, forInsert);
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, webApiCountryApplication.CaseNumber, webApiCountryApplication.Country, webApiCountryApplication.SubCase));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);
        }

        private async Task ValidateData(int id, CountryApplicationWebSvc webApiCountryApplication, bool forInsert)
        {
            //check key fields
            Guard.Against.NullOrEmpty(webApiCountryApplication.CaseNumber, "CaseNumber");
            Guard.Against.NullOrEmpty(webApiCountryApplication.Country, "Country");
            webApiCountryApplication.SubCase = webApiCountryApplication.SubCase ?? "";

            //use queryable list without filters when adding new records
            //use queryable list from ctry app service when updating records to enforce resp office and entity filters
            var ctryApps = forInsert ? 
                _cpiDbContext.GetReadOnlyRepositoryAsync<CountryApplication>().QueryableList : 
                _countryAppService.CountryApplications;
            var isFound = await ctryApps.AnyAsync(ca => ca.Invention != null && !(ca.Invention.IsTradeSecret ?? false) && ((id != 0 && ca.AppId == id) || (id == 0 && ca.CaseNumber == webApiCountryApplication.CaseNumber && ca.Country == webApiCountryApplication.Country && ca.SubCase == webApiCountryApplication.SubCase)));

            if (forInsert)
            {
                //check if country app already exists
                Guard.Against.RecordExists(isFound);

                //check if invention exists or if user has permissions
                Guard.Against.ValueNotAllowed(await _inventionService.QueryableList.AnyAsync(i => (i.CaseNumber == webApiCountryApplication.CaseNumber)), "CaseNumber");

                //check required fields when adding new record
                if (_user.HasRespOfficeFilter(SystemType.Patent))
                    Guard.Against.NullOrEmpty(webApiCountryApplication.RespOffice, "RespOffice");

                Guard.Against.NullOrEmpty(webApiCountryApplication.CaseType, "CaseType");
                Guard.Against.NullOrEmpty(webApiCountryApplication.ApplicationStatus, "ApplicationStatus");
            }
            else
            {
                //check if record exists or if user has permissions
                Guard.Against.RecordNotFound(isFound);
            }

            //check lookup fields
            if (!string.IsNullOrEmpty(webApiCountryApplication.CaseType))
                Guard.Against.ValueNotAllowed(await _cpiDbContext.GetReadOnlyRepositoryAsync<PatCountryLaw>().QueryableList
                    .AnyAsync(cl => cl.Country == webApiCountryApplication.Country && cl.CaseType == webApiCountryApplication.CaseType), "CaseType");

            if (!string.IsNullOrEmpty(webApiCountryApplication.ApplicationStatus))
                Guard.Against.ValueNotAllowed(await _cpiDbContext.GetReadOnlyRepositoryAsync<PatApplicationStatus>().QueryableList
                    .AnyAsync(s => s.ApplicationStatus == webApiCountryApplication.ApplicationStatus), "ApplicationStatus");

            //check respoffice
            if (!string.IsNullOrEmpty(webApiCountryApplication.RespOffice))
                Guard.Against.ValueNotAllowed(await ValidateRespOffice(SystemType.Patent, webApiCountryApplication.RespOffice ?? "", CPiPermissions.FullModify), "RespOffice");

            //check shared aux permission if agent does not exist
            if (!string.IsNullOrEmpty(webApiCountryApplication.Agent) && !HasSharedAuxModify)
                Guard.Against.ValueNotAllowed(await HasAgent(webApiCountryApplication.Agent), "Agent");
        }

        private async Task SetData(CountryApplicationWebSvc webApiCountryApplication, CountryApplication countryApplication, DateTime runDate)
        {
            //set key fields
            countryApplication.CaseNumber = webApiCountryApplication.CaseNumber ?? "";
            countryApplication.Country = webApiCountryApplication.Country ?? "";
            countryApplication.SubCase = webApiCountryApplication.SubCase ?? "";

            //set required fields if values are not null or empty
            if (!string.IsNullOrEmpty(webApiCountryApplication.CaseType)) countryApplication.CaseType = webApiCountryApplication.CaseType;
            if (!string.IsNullOrEmpty(webApiCountryApplication.ApplicationStatus)) countryApplication.ApplicationStatus = webApiCountryApplication.ApplicationStatus;
            if (!string.IsNullOrEmpty(webApiCountryApplication.RespOffice)) countryApplication.RespOffice = webApiCountryApplication.RespOffice;

            //set text fields if values are not null or empty
            //set text fields to null if values are empty string
            if (!string.IsNullOrEmpty(webApiCountryApplication.PubNumber)) countryApplication.PubNumber = webApiCountryApplication.PubNumber;
            else if (webApiCountryApplication.PubNumber == "") countryApplication.PubNumber = null;

            if (!string.IsNullOrEmpty(webApiCountryApplication.PCTNumber)) countryApplication.PCTNumber = webApiCountryApplication.PCTNumber;
            else if (webApiCountryApplication.PCTNumber == "") countryApplication.PCTNumber = null;

            if (!string.IsNullOrEmpty(webApiCountryApplication.AppTitle)) countryApplication.AppTitle = webApiCountryApplication.AppTitle;
            else if (webApiCountryApplication.AppTitle == "") countryApplication.AppTitle = null;

            //set date fields if values are not null or EmptyDate
            //set date fields to null if value is EmptyDate
            if (webApiCountryApplication.PubDate != null && webApiCountryApplication.PubDate != EmptyDate) countryApplication.PubDate = webApiCountryApplication.PubDate;
            else if (webApiCountryApplication.PubDate == EmptyDate) countryApplication.PubDate = null;

            if (webApiCountryApplication.PCTDate != null && webApiCountryApplication.PCTDate != EmptyDate) countryApplication.PCTDate = webApiCountryApplication.PCTDate;
            else if (webApiCountryApplication.PCTDate == EmptyDate) countryApplication.PCTDate = null;

            //set entity id fields if entity code values are not null or empty
            //set entity id fields to null if entity code values are empty string
            if (!string.IsNullOrEmpty(webApiCountryApplication.Agent))
                countryApplication.AgentID = await GetAgentID(webApiCountryApplication.Agent, webApiCountryApplication.AgentName, runDate);
            else if (webApiCountryApplication.Agent == "")
                countryApplication.AgentID = null;

            //generate tax start date
            var settings = await _settings.GetSetting();
            if (settings.IsTaxStartCalcOn && countryApplication.TaxStartDate == null)
            {
                var taxStartInfo = await _taxStartExpirationService.ComputeTaxStart(countryApplication.AppId);
                if (taxStartInfo.ExpTaxDate.HasValue)
                    countryApplication.TaxStartDate = taxStartInfo.ExpTaxDate;
            }

            //generate expiration date
            if (countryApplication.ExpDate == null)
            {
                var expireInfo = await _taxStartExpirationService.ComputeExpiration(countryApplication.AppId);
                if (expireInfo.ExpTaxDate.HasValue)
                    countryApplication.ExpDate = expireInfo.ExpTaxDate;
            }

            //update user and date stamp
            countryApplication.UpdatedBy = _user.GetUserName();
            countryApplication.LastUpdate = runDate;
            if (countryApplication.AppId == 0)
            {
                countryApplication.CreatedBy = _user.GetUserName();
                countryApplication.DateCreated = runDate;
            }
        }

        public async Task<List<PatRelatedCaseDTO>> GetRelatedCases(int appId)
        {
            return await _countryAppService.GetRelatedCases(appId);
        }
    }
}