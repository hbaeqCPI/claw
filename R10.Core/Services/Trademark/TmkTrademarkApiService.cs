using System;
using R10.Core.Interfaces;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces.Trademark;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using R10.Core.Entities;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using R10.Core.Helpers;
using R10.Core.Exceptions;

namespace R10.Core.Services
{
    public interface ITmkTrademarkApiService : IWebApiBaseService<TmkTrademarkWebSvc, TmkTrademark>
    {
        IQueryable<TmkCaseType> CaseTypes { get; }
        IQueryable<TmkCountry> Countries { get; }
        IQueryable<TmkActionDue> Actions { get; }
        IQueryable<TmkCostTrack> Costs { get; }
        IQueryable<TmkTrademarkStatus> Statuses { get; }
        IQueryable<TmkMarkType> MarkTypes { get; }
        // Removed during deep clean - TLSearchDocument, TLSearchImage no longer exist
        // IQueryable<TLSearchDocument> TLSearchDocuments { get; }
        // IQueryable<TLSearchImage> TLSearchImages { get; }
    }

    public class TmkTrademarkApiService : WebApiBaseService<TmkTrademarkWebSvc>, ITmkTrademarkApiService
    {
        private readonly ITmkTrademarkService _trademarkService;

        public TmkTrademarkApiService(
            ITmkTrademarkService trademarkService,
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _trademarkService = trademarkService;
        }

        IQueryable<TmkTrademark> IWebApiBaseService<TmkTrademarkWebSvc, TmkTrademark>.QueryableList => _trademarkService.TmkTrademarks;

        public IQueryable<TmkCountry> Countries => _cpiDbContext.GetRepository<TmkCountry>().QueryableList;

        public IQueryable<TmkCaseType> CaseTypes => _cpiDbContext.GetRepository<TmkCaseType>().QueryableList;

        public IQueryable<TmkActionDue> Actions => _cpiDbContext.GetRepository<TmkActionDue>().QueryableList;

        public IQueryable<TmkCostTrack> Costs => _cpiDbContext.GetRepository<TmkCostTrack>().QueryableList;

        public IQueryable<TmkTrademarkStatus> Statuses => _cpiDbContext.GetRepository<TmkTrademarkStatus>().QueryableList;

        public IQueryable<TmkMarkType> MarkTypes => _cpiDbContext.GetRepository<TmkMarkType>().QueryableList;

        // Removed during deep clean - TLSearchDocument, TLSearchImage no longer exist
        // public IQueryable<TLSearchDocument> TLSearchDocuments => _cpiDbContext.GetRepository<TLSearchDocument>().QueryableList;

        // public IQueryable<TLSearchImage> TLSearchImages => _cpiDbContext.GetRepository<TLSearchImage>().QueryableList;

        public async Task<int> Add(TmkTrademarkWebSvc webApiTrademark, DateTime runDate)
        {
            await ValidateTrademark(0, webApiTrademark, true);
            return await SaveTrademark(webApiTrademark, new TmkTrademark(), runDate);
        }

        public async Task<List<int>> Import(List<TmkTrademarkWebSvc> webApiTrademarks, DateTime runDate)
        {
            await ValidateTrademarks(webApiTrademarks, true);

            var tmkIds = new List<int>();
            foreach (var webApiTrademark in webApiTrademarks)
            {
                tmkIds.Add(await SaveTrademark(webApiTrademark, new TmkTrademark(), runDate));
            }

            return tmkIds;
        }

        public async Task Update(int id, TmkTrademarkWebSvc webApiTrademark, DateTime runDate)
        {
            await ValidateTrademark(id, webApiTrademark, false);

            var trademark = await _trademarkService.TmkTrademarks.FirstOrDefaultAsync(tmk => tmk.TmkId == id);
            if (trademark != null)
                await SaveTrademark(webApiTrademark, trademark, runDate);
        }

        public async Task Update(List<TmkTrademarkWebSvc> webApiTrademarks, DateTime runDate)
        {
            await ValidateTrademarks(webApiTrademarks, false);
            foreach (var webApiTrademark in webApiTrademarks)
            {
                var trademark = await _trademarkService.TmkTrademarks.FirstOrDefaultAsync(tmk => tmk.CaseNumber == webApiTrademark.CaseNumber && tmk.Country == webApiTrademark.Country && tmk.SubCase == webApiTrademark.SubCase);
                if (trademark != null)
                    await SaveTrademark(webApiTrademark, trademark, runDate);
            }
        }

        private async Task<int> SaveTrademark(TmkTrademarkWebSvc webApiTrademark, TmkTrademark trademark, DateTime runDate)
        {
            await SetData(webApiTrademark, trademark, runDate);

            if (trademark.TmkId == 0)
                await _trademarkService.AddTrademark(trademark, runDate);
            else
                await _trademarkService.UpdateTrademark(trademark, runDate);

            return trademark.TmkId;
        }

        private async Task ValidateTrademark(int id, TmkTrademarkWebSvc webApiTrademark, bool forInsert)
        {
            try
            {
                await ValidateData(id, webApiTrademark, forInsert);
            }
            catch (Exception ex)
            {
                throw new WebApiValidationException(FormatErrorMessage(id, ex.Message, webApiTrademark.CaseNumber, webApiTrademark.Country, webApiTrademark.SubCase));
            }
        }

        private async Task ValidateTrademarks(List<TmkTrademarkWebSvc> webApiTrademarks, bool forInsert)
        {
            var errors = new List<string>();
            var duplicates = webApiTrademarks.GroupBy(c => new { c.CaseNumber, c.Country, c.SubCase }).Where(g => g.Count() > 1).ToList();

            if (duplicates.Count > 0)
                throw new WebApiValidationException("Duplicate records found.", duplicates.Select(d => $"{d.Key}").ToList());

            for (int i = 0; i < webApiTrademarks.Count; i++)
            {
                var webApiTrademark = webApiTrademarks[i];

                try
                {
                    await ValidateData(0, webApiTrademark, forInsert);
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, webApiTrademark.CaseNumber, webApiTrademark.Country, webApiTrademark.SubCase));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);
        }

        private async Task ValidateData(int id, TmkTrademarkWebSvc webApiTrademark, bool forInsert)
        {
            //check key fields
            Guard.Against.NullOrEmpty(webApiTrademark.CaseNumber, "CaseNumber");
            Guard.Against.NullOrEmpty(webApiTrademark.Country, "Country");
            webApiTrademark.SubCase = webApiTrademark.SubCase ?? "";

            //use queryable list without filters when adding new records
            //use queryable list from tmk service when updating records to enforce resp office and entity filters
            var tmks = forInsert ? 
                _cpiDbContext.GetReadOnlyRepositoryAsync<TmkTrademark>().QueryableList : 
                _trademarkService.TmkTrademarks;
            var isFound = await tmks.AnyAsync(tmk => (id != 0 && tmk.TmkId == id) || (id == 0 && tmk.CaseNumber == webApiTrademark.CaseNumber && tmk.Country == webApiTrademark.Country && tmk.SubCase == webApiTrademark.SubCase));

            if (forInsert)
            {
                //check if tmk already exists
                Guard.Against.RecordExists(isFound);

                //check required fields when adding new record
                if (_user.HasRespOfficeFilter(SystemType.Trademark))
                    Guard.Against.NullOrEmpty(webApiTrademark.RespOffice, "RespOffice");

                Guard.Against.NullOrEmpty(webApiTrademark.CaseType, "CaseType");
                Guard.Against.NullOrEmpty(webApiTrademark.TrademarkStatus, "TrademarkStatus");
                Guard.Against.NullOrEmpty(webApiTrademark.TrademarkName, "TrademarkName");
            }
            else
            {
                //check if record exists or if user has permissions
                Guard.Against.RecordNotFound(isFound);
            }

            //check lookup fields
            if (!string.IsNullOrEmpty(webApiTrademark.CaseType))
                Guard.Against.ValueNotAllowed(await _cpiDbContext.GetReadOnlyRepositoryAsync<TmkCountryLaw>().QueryableList
                    .AnyAsync(cl => cl.Country == webApiTrademark.Country && cl.CaseType == webApiTrademark.CaseType), "CaseType");

            if (!string.IsNullOrEmpty(webApiTrademark.TrademarkStatus))
                Guard.Against.ValueNotAllowed(await _cpiDbContext.GetReadOnlyRepositoryAsync<TmkTrademarkStatus>().QueryableList
                    .AnyAsync(s => s.TrademarkStatus == webApiTrademark.TrademarkStatus), "TrademarkStatus");

            //check respoffice
            if (!string.IsNullOrEmpty(webApiTrademark.RespOffice))
                Guard.Against.ValueNotAllowed(await ValidateRespOffice(SystemType.Trademark, webApiTrademark.RespOffice ?? "", CPiPermissions.FullModify), "RespOffice");

            //check shared aux permission if agent does not exist
            if (!string.IsNullOrEmpty(webApiTrademark.Agent) && !HasSharedAuxModify)
                Guard.Against.ValueNotAllowed(await HasAgent(webApiTrademark.Agent), "Agent");
        }

        private async Task SetData(TmkTrademarkWebSvc webApiTrademark, TmkTrademark trademark, DateTime runDate)
        {
            //set key fields
            trademark.CaseNumber = webApiTrademark.CaseNumber ?? "";
            trademark.Country = webApiTrademark.Country ?? "";
            trademark.SubCase = webApiTrademark.SubCase ?? "";

            //set required fields if values are not null or empty
            if (!string.IsNullOrEmpty(webApiTrademark.CaseType)) trademark.CaseType = webApiTrademark.CaseType;
            if (!string.IsNullOrEmpty(webApiTrademark.TrademarkStatus)) trademark.TrademarkStatus = webApiTrademark.TrademarkStatus;
            if (!string.IsNullOrEmpty(webApiTrademark.TrademarkName)) trademark.TrademarkName = webApiTrademark.TrademarkName;
            if (!string.IsNullOrEmpty(webApiTrademark.RespOffice)) trademark.RespOffice = webApiTrademark.RespOffice;

            //set text fields if values are not null or empty
            //set text fields to null if values are empty string
            if (!string.IsNullOrEmpty(webApiTrademark.PubNumber)) trademark.PubNumber = webApiTrademark.PubNumber;
            else if (webApiTrademark.PubNumber == "") trademark.PubNumber = null;

            //set date fields if values are not null or EmptyDate
            //set date fields to null if value is EmptyDate
            if (webApiTrademark.PubDate != null && webApiTrademark.PubDate != EmptyDate) trademark.PubDate = webApiTrademark.PubDate;
            else if (webApiTrademark.PubDate == EmptyDate) trademark.PubDate = null;

            //set entity id fields if entity code values are not null or empty
            //set entity id fields to null if entity code values are empty string
            if (!string.IsNullOrEmpty(webApiTrademark.Agent)) trademark.AgentID = await GetAgentID(webApiTrademark.Agent, runDate);
            else if (webApiTrademark.Agent == "") trademark.AgentID = null;

            //update user and date stamp
            trademark.UpdatedBy = _user.GetUserName();
            trademark.LastUpdate = runDate;
            if (trademark.TmkId == 0)
            {
                trademark.CreatedBy = _user.GetUserName();
                trademark.DateCreated = runDate;
            }
        }
    }
}