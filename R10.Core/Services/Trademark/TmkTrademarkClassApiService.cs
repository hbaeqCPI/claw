using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using System.Security.Claims;

namespace R10.Core.Services
{
    public interface ITmkTrademarkClassApiService : IWebApiBaseService<TmkTrademarkClassWebSvc, TmkTrademarkClass>
    {
        IQueryable<TmkStandardGood> StandardGoods { get; }
        Task<TmkTrademarkClassWebSvc?> Delete(int tmkClassId);
    }

    public class TmkTrademarkClassApiService : WebApiBaseService<TmkTrademarkClassWebSvc>, IWebApiBaseService<TmkTrademarkClassWebSvc, TmkTrademarkClass>, ITmkTrademarkClassApiService
    {
        private readonly ITmkTrademarkService _trademarkService;

        public TmkTrademarkClassApiService(
            ITmkTrademarkService trademarkService,
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _trademarkService = trademarkService;
        }
        
        private IQueryable<TmkTrademarkClass> TrademarkClasses => _trademarkService.QueryableChildList<TmkTrademarkClass>();

        public IQueryable<TmkStandardGood> StandardGoods => _cpiDbContext.GetRepository<TmkStandardGood>().QueryableList;

        IQueryable<TmkTrademarkClass> IWebApiBaseService<TmkTrademarkClassWebSvc, TmkTrademarkClass>.QueryableList => TrademarkClasses;

        public Task<int> Add(TmkTrademarkClassWebSvc webApiEntity, DateTime runDate)
        {
            throw new NotImplementedException();
        }

        public async Task<List<int>> Import(List<TmkTrademarkClassWebSvc> webApiTrademarkClasses, DateTime runDate)
        {
            await ValidateTrademarkClasses(webApiTrademarkClasses, true);

            var added = new List<TmkTrademarkClass>();

            foreach (var webApiTrademarkClass in webApiTrademarkClasses)
            {
                var trademarkClass = await SetData(0, webApiTrademarkClass, runDate);
                if (trademarkClass != null)
                    added.Add(trademarkClass);
            }

            var tmkId = webApiTrademarkClasses.Select(c => c.TmkId).FirstOrDefault();
            await _trademarkService.UpdateChild(tmkId, _user.GetUserName(), new List<TmkTrademarkClass>(), added, new List<TmkTrademarkClass>());
            return added.Select(a => a.TmkClassId).ToList();
        }

        public async Task Update(int id, TmkTrademarkClassWebSvc webApiTrademarkClass, DateTime runDate)
        {
            await ValidateTrademarkClass(id, webApiTrademarkClass);

            var trademarkClass = await SetData(id, webApiTrademarkClass, runDate);
            if (trademarkClass != null)
            {
                var updated = new List<TmkTrademarkClass>() { trademarkClass };
                await _trademarkService.UpdateChild(webApiTrademarkClass.TmkId, _user.GetUserName(), updated, new List<TmkTrademarkClass>(), new List<TmkTrademarkClass>());
            }
        }

        public Task Update(List<TmkTrademarkClassWebSvc> webApiTrademarkClasses, DateTime runDate)
        {
            throw new NotImplementedException();
        }

        private async Task<TmkTrademarkClass?> SetData(int tmkClassId, TmkTrademarkClassWebSvc webApiTrademarkClass, DateTime runDate)
        {
            var trademarkClass = new TmkTrademarkClass();

            if (tmkClassId > 0)
            {
                trademarkClass = await TrademarkClasses.FirstOrDefaultAsync(c => c.TmkClassId == tmkClassId);
                if (trademarkClass == null)
                    return trademarkClass;
            }

            //set key fields
            trademarkClass.TmkId = webApiTrademarkClass.TmkId;
            trademarkClass.ClassId = webApiTrademarkClass.ClassId;

            //set required fields if values are not null or empty
            //--

            //set text fields if values are not null or empty
            //set text fields to null if values are empty string
            if (!string.IsNullOrEmpty(webApiTrademarkClass.Goods)) trademarkClass.Goods = webApiTrademarkClass.Goods;
            else if (webApiTrademarkClass.Goods == "") trademarkClass.Goods = null;

            //set date fields if values are not null or EmptyDate
            //set date fields to null if value is EmptyDate
            if (webApiTrademarkClass.FirstUseDate != null && webApiTrademarkClass.FirstUseDate != EmptyDate) trademarkClass.FirstUseDate = webApiTrademarkClass.FirstUseDate;
            else if (webApiTrademarkClass.FirstUseDate == EmptyDate) trademarkClass.FirstUseDate = null;

            if (webApiTrademarkClass.FirstUseInCommerce != null && webApiTrademarkClass.FirstUseDate != EmptyDate) trademarkClass.FirstUseDate = webApiTrademarkClass.FirstUseDate;
            else if (webApiTrademarkClass.FirstUseInCommerce == EmptyDate) trademarkClass.FirstUseDate = null;

            //set boolean if values are not null
            if (webApiTrademarkClass.IsStandardGoods != null) trademarkClass.IsStandardGoods = webApiTrademarkClass.IsStandardGoods ?? false;

            //set entity id fields if entity code values are not null or empty
            //set entity id fields to null if entity code values are empty string
            //--

            //update user and date stamp
            trademarkClass.UpdatedBy = _user.GetUserName();
            trademarkClass.LastUpdate = runDate;
            if (trademarkClass.TmkClassId == 0)
            {
                trademarkClass.CreatedBy = _user.GetUserName();
                trademarkClass.DateCreated = runDate;
            }

            return trademarkClass;
        }

        private async Task ValidateTrademarkClass(int id, TmkTrademarkClassWebSvc webApiTrademarkClass)
        {
            try
            {
                await ValidateData(id, webApiTrademarkClass, id == 0);
            }
            catch (Exception ex)
            {
                throw new WebApiValidationException(FormatErrorMessage(id, ex.Message, webApiTrademarkClass.TmkId.ToString(), webApiTrademarkClass.ClassId.ToString()));
            }
        }

        private async Task ValidateTrademarkClasses(List<TmkTrademarkClassWebSvc> webApiTrademarkClasses, bool forInsert)
        {
            var errors = new List<string>();
            var duplicates = webApiTrademarkClasses.GroupBy(c => new { c.ClassId }).Where(g => g.Count() > 1).ToList();

            if (duplicates.Count > 0)
                throw new WebApiValidationException("Duplicate records found.", duplicates.Select(d => $"{d.Key}").ToList());

            for (int i = 0; i < webApiTrademarkClasses.Count; i++)
            {
                var webApiTrademarkClass = webApiTrademarkClasses[i];

                try
                {
                    //import (and batch update) have no TmkClassId
                    await ValidateData(0, webApiTrademarkClass, forInsert);
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, webApiTrademarkClass.TmkId.ToString(), webApiTrademarkClass.ClassId.ToString()));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);
        }

        private async Task ValidateData(int tmkClassId, TmkTrademarkClassWebSvc webApiTrademarkClass, bool forInsert)
        {
            //check key fields
            Guard.Against.NullOrZero(webApiTrademarkClass.TmkId, "TmkId");
            Guard.Against.NullOrZero(webApiTrademarkClass.ClassId, "ClassId");

            //check if trademark exists or if user has permissions
            Guard.Against.ValueNotAllowed(await _trademarkService.TmkTrademarks.AnyAsync(t => t.TmkId == webApiTrademarkClass.TmkId), "Trademark");

            //use queryable list without filters when adding new records
            //use queryable list from trademark service when updating records to enforce resp office and entity filters
            var trademarkClasses = tmkClassId == 0 ?
                _cpiDbContext.GetReadOnlyRepositoryAsync<TmkTrademarkClass>().QueryableList :
                TrademarkClasses;
            var isFound = await trademarkClasses.AnyAsync(c => (tmkClassId != 0 && c.TmkClassId == tmkClassId) || (tmkClassId == 0 && c.TmkId == webApiTrademarkClass.TmkId && c.ClassId == webApiTrademarkClass.ClassId));

            if (forInsert)
            {
                //check if record already exists
                Guard.Against.RecordExists(isFound);
            }
            else
            {
                //check if record exists or if user has permissions
                Guard.Against.RecordNotFound(isFound);
            }

            //check lookup fields
            if (webApiTrademarkClass.ClassId > 0)
                Guard.Against.ValueNotAllowed(await StandardGoods.AnyAsync(g => g.ClassId == webApiTrademarkClass.ClassId), "Standard Goods");
        }

        public async Task<TmkTrademarkClassWebSvc?> Delete(int tmkClassId)
        {
            var trademarkClass = await TrademarkClasses.Where(c => c.TmkClassId == tmkClassId).FirstOrDefaultAsync();
            Guard.Against.RecordNotFound(trademarkClass != null);

            if (trademarkClass == null)
                return null;

            var deleted = new List<TmkTrademarkClass>() { trademarkClass };
            await _trademarkService.UpdateChild(trademarkClass.TmkId, _user.GetUserName(), new List<TmkTrademarkClass>(), new List<TmkTrademarkClass>(), deleted);

            return new TmkTrademarkClassWebSvc()
            {
                TmkId = trademarkClass.TmkId,
                ClassId = trademarkClass.ClassId,
                Goods = trademarkClass.Goods,
                IsStandardGoods = trademarkClass.IsStandardGoods,
                FirstUseDate = trademarkClass.FirstUseDate,
                FirstUseInCommerce = trademarkClass.FirstUseInCommerce
            };
        }
    }
}
