using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using R10.Core.Entities.Trademark;
using System;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using System.Transactions;
using R10.Core.Exceptions;

namespace R10.Core.Services
{
    public class TmkCEGeneralSetupService : ITmkCEGeneralSetupService
    {
        private readonly IApplicationDbContext _repository;        
        private readonly ISystemSettings<TmkSetting> _settings;
        
        public TmkCEGeneralSetupService(IApplicationDbContext repository,            
           ISystemSettings<TmkSetting> settings) 
        {
            _repository = repository;           
           _settings = settings;
        }

        public async Task AddCEGeneralSetup(TmkCEGeneralSetup generalSetup)
        {   
            _repository.TmkCEGeneralSetups.Add(generalSetup);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateCEGeneralSetup(TmkCEGeneralSetup generalSetup)
        {
            _repository.TmkCEGeneralSetups.Update(generalSetup);
            await _repository.SaveChangesAsync();
        }        

        public async Task DeleteCEGeneralSetup(TmkCEGeneralSetup generalSetup)
        {
            _repository.TmkCEGeneralSetups.Remove(generalSetup);
            await _repository.SaveChangesAsync();
        }

        public async Task CopyCEGeneralSetup(int oldCEGeneralId, int newCEGeneralId, string userName, bool copyCosts)
        {
            //Copy costs
            if (copyCosts)
            {
                var newCosts = await _repository.TmkCEGeneralCosts.AsNoTracking()
                                    .Where(d => d.CEGeneralId == oldCEGeneralId && d.CPICost == false)
                                    .Select(d => new TmkCEGeneralCost()
                                    {
                                        CEGeneralId = newCEGeneralId,
                                        Description = d.Description,
                                        DataType = d.DataType,
                                        DefaultValue = d.DefaultValue,
                                        Opts = d.Opts,
                                        Cost = d.Cost,
                                        AltCost = d.AltCost,
                                        MultCost = d.MultCost,
                                        OrderOfEntry = d.OrderOfEntry,
                                        ActiveSwitch = d.ActiveSwitch,                                        
                                        CPICost = false,
                                        CreatedBy = userName,
                                        UpdatedBy = userName,
                                        DateCreated = DateTime.Now,
                                        LastUpdate = DateTime.Now
                                    }).ToListAsync();
                if (newCosts.Any())
                {
                    _repository.TmkCEGeneralCosts.AddRange(newCosts);
                    await _repository.SaveChangesAsync();
                }
            }
        }

        #region Question Guide
        public async Task UpdateChild<T>(int parentId, string userName, IEnumerable<TmkCEGeneralCost> updated, IEnumerable<TmkCEGeneralCost> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            if (updated.Any())
            {
                _repository.Set<TmkCEGeneralCost>().UpdateRange(updated);
            }

            if (added.Any())
            {
                var startIndex = await GetCEGeneralCostNextOrderOfEntry(parentId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
                _repository.Set<TmkCEGeneralCost>().AddRange(added);
            }

            if (deleted.Any())
                _repository.Set<T>().RemoveRange(deleted);

            await UpdateParentStampsAsync(parentId, userName);
            await _repository.SaveChangesAsync();
        }

        public async Task ReorderCEGeneralCost(int id, string userName, int newIndex)
        {
            var generalCost = await TmkCEGeneralCosts.SingleOrDefaultAsync(a => a.CostId == id);
            Guard.Against.NoRecordPermission(generalCost != null);
            generalCost.UpdatedBy = userName;
            generalCost.LastUpdate = DateTime.Now;

            int cEGeneralId = generalCost.CEGeneralId;
            int oldIndex = generalCost.OrderOfEntry;

            var generalSetup = await TmkCEGeneralSetups.Where(w => w.CEGeneralId == cEGeneralId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(generalSetup != null);
            generalSetup.UpdatedBy = generalCost.UpdatedBy;
            generalSetup.LastUpdate = generalCost.LastUpdate;

            List<TmkCEGeneralCost> generalCosts = new List<TmkCEGeneralCost>();
            if (oldIndex > newIndex)
            {
                generalCosts = await TmkCEGeneralCosts.Where(w => w.CEGeneralId == cEGeneralId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                generalCosts.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                generalCosts = await TmkCEGeneralCosts.Where(w => w.CEGeneralId == cEGeneralId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                generalCosts.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            generalCost.OrderOfEntry = newIndex;
            generalCosts.Add(generalCost);

            _repository.Set<TmkCEGeneralCost>().UpdateRange(generalCosts);
            _repository.TmkCEGeneralSetups.Update(generalSetup);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetCEGeneralCostNextOrderOfEntry(int cEGeneralId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                if (await TmkCEGeneralCosts.AnyAsync(d => d.CEGeneralId == cEGeneralId))
                {
                    lastOrderOfEntry = await TmkCEGeneralCosts.Where(ma => ma.CEGeneralId == cEGeneralId).MaxAsync(ma => ma.OrderOfEntry);
                    return lastOrderOfEntry + 1;
                }
                return lastOrderOfEntry;
            }
            catch (Exception e){
                var err = e.Message;
                return lastOrderOfEntry;
            }
            //return lastOrderOfEntry + 1;
        }
        #endregion        

        public IQueryable<TmkCEGeneralSetup> TmkCEGeneralSetups => _repository.TmkCEGeneralSetups.AsNoTracking();
        public IQueryable<TmkCEGeneralCost> TmkCEGeneralCosts => _repository.TmkCEGeneralCosts;

        protected async Task UpdateParentStampsAsync(int ceGeneralId, string userName)
        {
            var generalSetup = await _repository.TmkCEGeneralSetups.Where(w => w.CEGeneralId == ceGeneralId).FirstOrDefaultAsync();

            generalSetup.UpdatedBy = userName;
            generalSetup.LastUpdate = DateTime.Now;

            var entity = _repository.TmkCEGeneralSetups.Attach(generalSetup);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
        }
    }
}
