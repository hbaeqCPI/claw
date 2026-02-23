using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using R10.Core.Entities.Patent;
using System;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using System.Transactions;
using R10.Core.Exceptions;

namespace R10.Core.Services
{
    public class PatCEGeneralSetupService : IPatCEGeneralSetupService
    {
        private readonly IApplicationDbContext _repository;        
        private readonly ISystemSettings<PatSetting> _settings;
        
        public PatCEGeneralSetupService(IApplicationDbContext repository,            
           ISystemSettings<PatSetting> settings) 
        {
            _repository = repository;           
           _settings = settings;
        }

        public async Task AddCEGeneralSetup(PatCEGeneralSetup generalSetup)
        {   
            _repository.PatCEGeneralSetups.Add(generalSetup);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateCEGeneralSetup(PatCEGeneralSetup generalSetup)
        {
            _repository.PatCEGeneralSetups.Update(generalSetup);
            await _repository.SaveChangesAsync();
        }        

        public async Task DeleteCEGeneralSetup(PatCEGeneralSetup generalSetup)
        {
            _repository.PatCEGeneralSetups.Remove(generalSetup);
            await _repository.SaveChangesAsync();
        }

        public async Task CopyCEGeneralSetup(int oldCEGeneralId, int newCEGeneralId, string userName, bool copyCosts)
        {
            //Copy costs
            if (copyCosts)
            {
                var newCosts = await _repository.PatCEGeneralCosts.AsNoTracking()
                                    .Where(d => d.CEGeneralId == oldCEGeneralId && d.CPICost == false)
                                    .Select(d => new PatCEGeneralCost()
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
                    _repository.PatCEGeneralCosts.AddRange(newCosts);
                    await _repository.SaveChangesAsync();
                }
            }
        }

        #region Question Guide
        public async Task UpdateChild<T>(int parentId, string userName, IEnumerable<PatCEGeneralCost> updated, IEnumerable<PatCEGeneralCost> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            if (updated.Any())
            {
                _repository.Set<PatCEGeneralCost>().UpdateRange(updated);
            }

            if (added.Any())
            {
                var startIndex = await GetCEGeneralCostNextOrderOfEntry(parentId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
                _repository.Set<PatCEGeneralCost>().AddRange(added);
            }

            if (deleted.Any())
                _repository.Set<T>().RemoveRange(deleted);

            await UpdateParentStampsAsync(parentId, userName);
            await _repository.SaveChangesAsync();
        }

        public async Task ReorderCEGeneralCost(int id, string userName, int newIndex)
        {
            var generalCost = await PatCEGeneralCosts.SingleOrDefaultAsync(a => a.CostId == id);
            Guard.Against.NoRecordPermission(generalCost != null);
            generalCost.UpdatedBy = userName;
            generalCost.LastUpdate = DateTime.Now;

            int cEGeneralId = generalCost.CEGeneralId;
            int oldIndex = generalCost.OrderOfEntry;

            var generalSetup = await PatCEGeneralSetups.Where(w => w.CEGeneralId == cEGeneralId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(generalSetup != null);
            generalSetup.UpdatedBy = generalCost.UpdatedBy;
            generalSetup.LastUpdate = generalCost.LastUpdate;

            List<PatCEGeneralCost> generalCosts = new List<PatCEGeneralCost>();
            if (oldIndex > newIndex)
            {
                generalCosts = await PatCEGeneralCosts.Where(w => w.CEGeneralId == cEGeneralId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                generalCosts.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                generalCosts = await PatCEGeneralCosts.Where(w => w.CEGeneralId == cEGeneralId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                generalCosts.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            generalCost.OrderOfEntry = newIndex;
            generalCosts.Add(generalCost);

            _repository.Set<PatCEGeneralCost>().UpdateRange(generalCosts);
            _repository.PatCEGeneralSetups.Update(generalSetup);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetCEGeneralCostNextOrderOfEntry(int cEGeneralId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                if (await PatCEGeneralCosts.AnyAsync(d => d.CEGeneralId == cEGeneralId))
                {
                    lastOrderOfEntry = await PatCEGeneralCosts.Where(ma => ma.CEGeneralId == cEGeneralId).MaxAsync(ma => ma.OrderOfEntry);
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

        public IQueryable<PatCEGeneralSetup> PatCEGeneralSetups => _repository.PatCEGeneralSetups.AsNoTracking();
        public IQueryable<PatCEGeneralCost> PatCEGeneralCosts => _repository.PatCEGeneralCosts;

        protected async Task UpdateParentStampsAsync(int ceGeneralId, string userName)
        {
            var generalSetup = await _repository.PatCEGeneralSetups.Where(w => w.CEGeneralId == ceGeneralId).FirstOrDefaultAsync();

            generalSetup.UpdatedBy = userName;
            generalSetup.LastUpdate = DateTime.Now;

            var entity = _repository.PatCEGeneralSetups.Attach(generalSetup);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
        }
    }
}
