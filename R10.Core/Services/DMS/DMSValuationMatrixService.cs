using System;
using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using R10.Core.Interfaces.DMS;
using R10.Core.Entities.DMS;
using R10.Core.Identity;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using R10.Core.Entities.Patent;
using Microsoft.EntityFrameworkCore;
using R10.Core.Helpers;
using R10.Core.Exceptions;
using R10.Core.DTOs;
using System.Transactions;
using System.ComponentModel;

namespace R10.Core.Services
{
    public class DMSValuationMatrixService : IDMSValuationMatrixService
    {
        private readonly IApplicationDbContext _repository;

        public DMSValuationMatrixService(IApplicationDbContext repository)
        {
            _repository = repository;
        }

        public IQueryable<DMSValuationMatrix> DMSValuationMatrices => _repository.DMSValuationMatrices.AsNoTracking();
        public IQueryable<DMSValuationMatrixRate> DMSValuationMatrixRates => _repository.DMSValuationMatrixRates;

        public async Task AddValuationMatrix(DMSValuationMatrix valuationMatrix)
        {
            _repository.DMSValuationMatrices.Add(valuationMatrix);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateValuationMatrix(DMSValuationMatrix valuationMatrix)
        {
            //Clear out Rating values if Rating System change
            var oldValuationMatrix = await _repository.DMSValuationMatrices.AsNoTracking().FirstOrDefaultAsync(d => d.ValId == valuationMatrix.ValId);

            if (oldValuationMatrix.RatingSystem != valuationMatrix.RatingSystem)
            {
                var ratings = await _repository.DMSValuationMatrixRates.Where(d => d.ValId == valuationMatrix.ValId).ToListAsync();
                ratings.ForEach(d => { d.WeightMax = null; d.WeightMin = null; });
                _repository.DMSValuationMatrixRates.UpdateRange(ratings);
            }

            _repository.DMSValuationMatrices.Update(valuationMatrix);
            await _repository.SaveChangesAsync();
        }
        
        public async Task DeleteValuationMatrix(DMSValuationMatrix valuationMatrix)
        {
            _repository.DMSValuationMatrices.Remove(valuationMatrix);
            await _repository.SaveChangesAsync();
        }

        public async Task CopyValuationMatrix(int oldValId, int newValId, string userName, bool copyRating)
        {
            //Copy rating
            if (copyRating)
            {
                var oldRatingSystem = await _repository.DMSValuationMatrices.AsNoTracking().Where(d => d.ValId == oldValId).Select(d => d.RatingSystem).FirstOrDefaultAsync();
                var newRatingSystem = await _repository.DMSValuationMatrices.AsNoTracking().Where(d => d.ValId == newValId).Select(d => d.RatingSystem).FirstOrDefaultAsync();

                var newRatings = await _repository.DMSValuationMatrixRates.AsNoTracking()
                                    .Where(d => d.ValId == oldValId)
                                    .Select(d => new DMSValuationMatrixRate()
                                    {
                                        ValId = newValId,
                                        Rating = d.Rating,
                                        OrderOfEntry = d.OrderOfEntry,
                                        InUse = d.InUse,
                                        WeightMin = oldRatingSystem != newRatingSystem ? null : d.WeightMin,
                                        WeightMax = oldRatingSystem != newRatingSystem ? null : d.WeightMax,
                                        CreatedBy = userName,
                                        UpdatedBy = userName,
                                        DateCreated = DateTime.Now,
                                        LastUpdate = DateTime.Now
                                    }).ToListAsync();
                if (newRatings.Any())
                {
                    _repository.DMSValuationMatrixRates.AddRange(newRatings);
                    await _repository.SaveChangesAsync();
                }
            }
        }        

        #region ValuationMatrixRate
        public async Task UpdateChild(int parentId, string userName, IEnumerable<DMSValuationMatrixRate> updated, IEnumerable<DMSValuationMatrixRate> added, IEnumerable<DMSValuationMatrixRate> deleted)
        {
            if (updated.Any())
                _repository.Set<DMSValuationMatrixRate>().UpdateRange(updated);

            if (added.Any())
            {                
                var startIndex = await GetRateNextOrderOfEntry(parentId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
                _repository.Set<DMSValuationMatrixRate>().AddRange(added);
            }                

            if (deleted.Any())
                _repository.Set<DMSValuationMatrixRate>().RemoveRange(deleted);

            await UpdateParentStampsAsync(parentId, userName);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteValuationMatrixRate(int parentId, string userName, IEnumerable<DMSValuationMatrixRate> deleted)
        {
            if (deleted.Any())
            {
                await UpdateChild(parentId, userName, new List<DMSValuationMatrixRate>(), new List<DMSValuationMatrixRate>(), deleted);
            }
        }        

        public async Task ReorderValuationMatrixRate(int id, string userName, int newIndex)
        {
            var valuationMatrixRate = await DMSValuationMatrixRates.SingleOrDefaultAsync(a => a.RateId == id);
            Guard.Against.NoRecordPermission(valuationMatrixRate != null);
            valuationMatrixRate.UpdatedBy = userName;
            valuationMatrixRate.LastUpdate = DateTime.Now;

            int valId = valuationMatrixRate.ValId;
            int oldIndex = valuationMatrixRate.OrderOfEntry;

            var valuationMatrix = await DMSValuationMatrices.Where(w => w.ValId == valId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(valuationMatrix != null);
            valuationMatrix.UpdatedBy = valuationMatrixRate.UpdatedBy;
            valuationMatrix.LastUpdate = valuationMatrixRate.LastUpdate;

            List<DMSValuationMatrixRate> valuationMatrixRates = new List<DMSValuationMatrixRate>();
            if (oldIndex > newIndex)
            {
                valuationMatrixRates = await DMSValuationMatrixRates.Where(w => w.ValId == valId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                valuationMatrixRates.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                valuationMatrixRates = await DMSValuationMatrixRates.Where(w => w.ValId == valId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                valuationMatrixRates.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            valuationMatrixRate.OrderOfEntry = newIndex;
            valuationMatrixRates.Add(valuationMatrixRate);

            //_repository.DMSValuationMatrixRates.AsNoTracking().Update(valuationMatrixRates);
            _repository.Set<DMSValuationMatrixRate>().UpdateRange(valuationMatrixRates);
            _repository.DMSValuationMatrices.Update(valuationMatrix);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetRateNextOrderOfEntry(int valId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await DMSValuationMatrixRates.Where(ma => ma.ValId == valId).MaxAsync(ma => ma.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        #endregion

        private async Task UpdateParentStampsAsync(int valId, string userName)
        {
            var valuationMatrix = await _repository.DMSValuationMatrices.Where(w => w.ValId == valId).FirstOrDefaultAsync();
            //var valuationMatrix = new DMSValuationMatrix() { ValId = valId, UpdatedBy = userName, LastUpdate = DateTime.Now, tStamp = tStamp };
            valuationMatrix.UpdatedBy = userName;
            valuationMatrix.LastUpdate = DateTime.Now;
            
            var entity = _repository.DMSValuationMatrices.Attach(valuationMatrix);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
        }
    }
}