using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.Patent
{    
    public class PatCostTrackingApiService : WebApiBaseService<PatCostTrackWebSvc>, IWebApiBaseService<PatCostTrackWebSvc, PatCostTrack>
    {
        private readonly ICostTrackingService<PatCostTrack> _costTrackingService;
        private readonly ICountryApplicationService _countryAppService;

        public PatCostTrackingApiService(
            ICostTrackingService<PatCostTrack> costTrackingService,
            ICountryApplicationService countryAppService,
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _costTrackingService = costTrackingService;
            _countryAppService = countryAppService;
        }

        IQueryable<PatCostTrack> IWebApiBaseService<PatCostTrackWebSvc, PatCostTrack>.QueryableList => _costTrackingService.QueryableList;

        public async Task<int> Add(PatCostTrackWebSvc webApiCostTracking, DateTime runDate)
        {
            await ValidateCostTracking(0, webApiCostTracking, true);
            return await SaveCostTracking(webApiCostTracking, new PatCostTrack(), runDate);
        }

        public async Task<List<int>> Import(List<PatCostTrackWebSvc> webApiCostTrackingList, DateTime runDate)
        {
            await ValidateCostTracking(webApiCostTrackingList, true);

            var costTrackIds = new List<int>();
            foreach (var webApiCostTracking in webApiCostTrackingList)
            {
                costTrackIds.Add(await SaveCostTracking(webApiCostTracking, new PatCostTrack(), runDate));
            }

            return costTrackIds;
        }

        public async Task Update(int id, PatCostTrackWebSvc webApiCostTracking, DateTime runDate)
        {
            await ValidateCostTracking(id, webApiCostTracking, false);

            var costTracking = await _costTrackingService.QueryableList.FirstOrDefaultAsync(c => c.CostTrackId == id);
            if (costTracking != null)
                await SaveCostTracking(webApiCostTracking, costTracking, runDate);
        }

        public async Task Update(List<PatCostTrackWebSvc> webApiCostTrackingList, DateTime runDate)
        {
            await ValidateCostTracking(webApiCostTrackingList, false);

            foreach (var webApiCostTracking in webApiCostTrackingList)
            {
                var costTracking = await _costTrackingService.QueryableList
                    .FirstOrDefaultAsync(c => c.CaseNumber == webApiCostTracking.CaseNumber && c.Country == webApiCostTracking.Country && c.SubCase == webApiCostTracking.SubCase &&
                                              c.CostType == webApiCostTracking.CostType && c.InvoiceDate == webApiCostTracking.InvoiceDate && c.InvoiceNumber == webApiCostTracking.InvoiceNumber);
                if (costTracking != null)
                    await SaveCostTracking(webApiCostTracking, costTracking, runDate);
            }
        }

        private async Task<int> SaveCostTracking(PatCostTrackWebSvc webApiCostTracking, PatCostTrack costTracking, DateTime runDate)
        {
            await SetData(webApiCostTracking, costTracking, runDate);

            if (costTracking.CostTrackId == 0)
                await _costTrackingService.Add(costTracking);
            else
                await _costTrackingService.Update(costTracking);

            return costTracking.CostTrackId;
        }

        private async Task ValidateCostTracking(int id, PatCostTrackWebSvc webApiCostTracking, bool forInsert)
        {
            try
            {
                await ValidateData(id, webApiCostTracking, forInsert);
            }
            catch (Exception ex)
            {
                throw new WebApiValidationException(FormatErrorMessage(id, ex.Message, webApiCostTracking.CaseNumber, webApiCostTracking.Country, webApiCostTracking.SubCase, webApiCostTracking.CostType));
            }
        }

        private async Task ValidateCostTracking(List<PatCostTrackWebSvc> webApiCostTrackingList, bool forInsert)
        {
            var errors = new List<string>();
            var duplicates = webApiCostTrackingList.GroupBy(c => new { c.CaseNumber, c.Country, c.SubCase, c.CostType, c.InvoiceNumber, c.InvoiceDate }).Where(g => g.Count() > 1).ToList();

            if (duplicates.Count > 0)
                throw new WebApiValidationException("Duplicate records found.", duplicates.Select(d => $"{d.Key}").ToList());

            for (int i = 0; i < webApiCostTrackingList.Count; i++)
            {
                var webApiCostTracking = webApiCostTrackingList[i];

                try
                {
                    await ValidateData(0, webApiCostTracking, forInsert);
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, webApiCostTracking.CaseNumber, webApiCostTracking.Country, webApiCostTracking.SubCase, webApiCostTracking.CostType));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);
        }

        private async Task ValidateData(int id, PatCostTrackWebSvc webApiCostTracking, bool forInsert)
        {
            //check key fields
            Guard.Against.NullOrEmpty(webApiCostTracking.CaseNumber, "CaseNumber");
            Guard.Against.NullOrEmpty(webApiCostTracking.Country, "Country");
            webApiCostTracking.SubCase = webApiCostTracking.SubCase ?? "";

            Guard.Against.NullOrEmpty(webApiCostTracking.CostType, "CostType");
            Guard.Against.Null(webApiCostTracking.InvoiceDate, "InvoiceDate");
            Guard.Against.NullOrEmpty(webApiCostTracking.InvoiceNumber, "InvoiceNumber");

            //use queryable list without filters when adding new records
            //use queryable list from cost service when updating records to enforce resp office and entity filters
            var costs = forInsert ? 
                _cpiDbContext.GetReadOnlyRepositoryAsync<PatCostTrack>().QueryableList : 
                _costTrackingService.QueryableList;
            var isFound = await costs.AnyAsync(c => (id != 0 && c.CostTrackId == id) || (id == 0 &&
                                    c.CaseNumber == webApiCostTracking.CaseNumber && c.Country == webApiCostTracking.Country && c.SubCase == webApiCostTracking.SubCase &&
                                    c.CostType == webApiCostTracking.CostType && c.InvoiceDate == webApiCostTracking.InvoiceDate && c.InvoiceNumber == webApiCostTracking.InvoiceNumber));

            if (forInsert)
            {
                //check if cost already exists
                Guard.Against.RecordExists(isFound);

                //check if ctry app exists or if user has permissions
                Guard.Against.ValueNotAllowed(await _countryAppService.CountryApplications.AnyAsync(ca => ca.CaseNumber == webApiCostTracking.CaseNumber && ca.Country == webApiCostTracking.Country && ca.SubCase == webApiCostTracking.SubCase), "Country Application");
            }
            else
            {
                //check if record exists or if user has permissions
                Guard.Against.RecordNotFound(isFound);
            }

            //check lookup fields
            if (!string.IsNullOrEmpty(webApiCostTracking.CostType))
                Guard.Against.ValueNotAllowed(await _cpiDbContext.GetReadOnlyRepositoryAsync<PatCostType>().QueryableList
                    .AnyAsync(t => t.CostType == webApiCostTracking.CostType), "CostType");

            //check shared aux permission if agent does not exist
            if (!string.IsNullOrEmpty(webApiCostTracking.Agent) && !HasSharedAuxModify)
                Guard.Against.ValueNotAllowed(await HasAgent(webApiCostTracking.Agent), "Agent");
        }

        private async Task SetData(PatCostTrackWebSvc webApiCostTracking, PatCostTrack costTracking, DateTime runDate)
        {
            //set key fields
            costTracking.CaseNumber = webApiCostTracking.CaseNumber ?? "";
            costTracking.Country = webApiCostTracking.Country ?? "";
            costTracking.SubCase = webApiCostTracking.SubCase ?? "";
            costTracking.CostType = webApiCostTracking.CostType ?? "";
            costTracking.InvoiceDate = webApiCostTracking.InvoiceDate;
            costTracking.InvoiceNumber = webApiCostTracking.InvoiceNumber ?? "";

            //set numeric fields if values are not null
            if (webApiCostTracking.InvoiceAmount != null) costTracking.InvoiceAmount = webApiCostTracking.InvoiceAmount ?? 0;

            //set date fields if values are not null or EmptyDate
            //set date fields to null if value is EmptyDate
            if (webApiCostTracking.PayDate != null && webApiCostTracking.PayDate != EmptyDate) costTracking.PayDate = webApiCostTracking.PayDate;
            else if (webApiCostTracking.PayDate == EmptyDate) costTracking.PayDate = null;

            //set entity id fields if entity code values are not null or empty
            //set entity id fields to null if entity code values are empty string
            if (!string.IsNullOrEmpty(webApiCostTracking.Agent)) costTracking.AgentID = await GetAgentID(webApiCostTracking.Agent, runDate);
            else if (webApiCostTracking.Agent == "") costTracking.AgentID = null;

            //update user and date stamp
            costTracking.UpdatedBy = _user.GetUserName();
            costTracking.LastUpdate = runDate;
            if (costTracking.CostTrackId == 0)
            {
                costTracking.CreatedBy = _user.GetUserName();
                costTracking.DateCreated = runDate;
            }
        }
    }
}
