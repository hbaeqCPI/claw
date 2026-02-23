using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.Trademark
{
    public class TmkCostTrackingApiService : WebApiBaseService<TmkCostTrackWebSvc>, IWebApiBaseService<TmkCostTrackWebSvc, TmkCostTrack>
    {
        private readonly ICostTrackingService<TmkCostTrack> _costTrackingService;
        private readonly ITmkTrademarkService _trademarkService;

        public TmkCostTrackingApiService(
            ICostTrackingService<TmkCostTrack> costTrackingService,
            ITmkTrademarkService trademarkService,
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _costTrackingService = costTrackingService;
            _trademarkService = trademarkService;
        }

        IQueryable<TmkCostTrack> IWebApiBaseService<TmkCostTrackWebSvc, TmkCostTrack>.QueryableList => _costTrackingService.QueryableList;

        public async Task<int> Add(TmkCostTrackWebSvc webApiCostTracking, DateTime runDate)
        {
            await ValidateCostTracking(0, webApiCostTracking, true);
            return await SaveCostTracking(webApiCostTracking, new TmkCostTrack(), runDate);
        }

        public async Task<List<int>> Import(List<TmkCostTrackWebSvc> webApiCostTrackingList, DateTime runDate)
        {
            await ValidateCostTracking(webApiCostTrackingList, true);

            var costTrackIds = new List<int>();
            foreach (var webApiCostTracking in webApiCostTrackingList)
            {
                costTrackIds.Add(await SaveCostTracking(webApiCostTracking, new TmkCostTrack(), runDate));
            }

            return costTrackIds;
        }

        public async Task Update(int id, TmkCostTrackWebSvc webApiCostTracking, DateTime runDate)
        {
            await ValidateCostTracking(id, webApiCostTracking, false);

            var costTracking = await _costTrackingService.QueryableList.FirstOrDefaultAsync(c => c.CostTrackId == id);
            if (costTracking != null)
                await SaveCostTracking(webApiCostTracking, costTracking, runDate);
        }

        public async Task Update(List<TmkCostTrackWebSvc> webApiCostTrackingList, DateTime runDate)
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

        private async Task<int> SaveCostTracking(TmkCostTrackWebSvc webApiCostTracking, TmkCostTrack costTracking, DateTime runDate)
        {
            await SetData(webApiCostTracking, costTracking, runDate);

            if (costTracking.CostTrackId == 0)
                await _costTrackingService.Add(costTracking);
            else
                await _costTrackingService.Update(costTracking);

            return costTracking.CostTrackId;
        }

        private async Task ValidateCostTracking(int id, TmkCostTrackWebSvc webApiCostTracking, bool forInsert)
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

        private async Task ValidateCostTracking(List<TmkCostTrackWebSvc> webApiCostTrackingList, bool forInsert)
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

        private async Task ValidateData(int id, TmkCostTrackWebSvc webApiCostTracking, bool forInsert)
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
                _cpiDbContext.GetReadOnlyRepositoryAsync<TmkCostTrack>().QueryableList :
                _costTrackingService.QueryableList;
            var isFound = await costs.AnyAsync(c => (id != 0 && c.CostTrackId == id) || (id == 0 &&
                                    c.CaseNumber == webApiCostTracking.CaseNumber && c.Country == webApiCostTracking.Country && c.SubCase == webApiCostTracking.SubCase &&
                                    c.CostType == webApiCostTracking.CostType && c.InvoiceDate == webApiCostTracking.InvoiceDate && c.InvoiceNumber == webApiCostTracking.InvoiceNumber));

            if (forInsert)
            {
                //check if cost already exists
                Guard.Against.RecordExists(isFound);

                //check if trademark exists or if user has permissions
                Guard.Against.ValueNotAllowed(await _trademarkService.TmkTrademarks.AnyAsync(t => t.CaseNumber == webApiCostTracking.CaseNumber && t.Country == webApiCostTracking.Country && t.SubCase == webApiCostTracking.SubCase), "Trademark");
            }
            else
            {
                //check if record exists or if user has permissions
                Guard.Against.RecordNotFound(isFound);
            }

            //check lookup fields
            if (!string.IsNullOrEmpty(webApiCostTracking.CostType))
                Guard.Against.ValueNotAllowed(await _cpiDbContext.GetReadOnlyRepositoryAsync<TmkCostType>().QueryableList
                    .AnyAsync(t => t.CostType == webApiCostTracking.CostType), "CostType");

            //check shared aux permission if agent does not exist
            if (!string.IsNullOrEmpty(webApiCostTracking.Agent) && !HasSharedAuxModify)
                Guard.Against.ValueNotAllowed(await HasAgent(webApiCostTracking.Agent), "Agent");
        }

        private async Task SetData(TmkCostTrackWebSvc webApiCostTracking, TmkCostTrack costTracking, DateTime runDate)
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
