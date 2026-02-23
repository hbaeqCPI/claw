using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Interfaces;

namespace R10.Core.Services.Trademark
{
    public class TmkCECountrySetupService : ITmkCECountrySetupService
    {
        private readonly IApplicationDbContext _repository;
        private readonly ISystemSettings<TmkSetting> _settings;

        public TmkCECountrySetupService(IApplicationDbContext repository,
           ISystemSettings<TmkSetting> settings)
        {
            _repository = repository;
            _settings = settings;
        }

        public IQueryable<TmkCECountrySetup> TmkCECountrySetups => _repository.TmkCECountrySetups.AsNoTracking();
        public IQueryable<TmkCECountryCost> TmkCECountryCosts => _repository.TmkCECountryCosts;
        public IQueryable<TmkCECountryCostChild> TmkCECountryCostChildren => _repository.TmkCECountryCostChilds;
        public IQueryable<TmkCECountryCostSub> TmkCECountryCostSubs => _repository.TmkCECountryCostSubs;
        public IQueryable<TmkCaseType> TmkCaseTypes => _repository.TmkCaseTypes.AsNoTracking();

        public async Task AddCECountrySetup(TmkCECountrySetup countrySetup)
        {
            //Only 1 default setup per country
            if (countrySetup.IsDefault)
            {
                var countryGroups = await _repository.TmkCECountrySetups.Where(d => d.Country == countrySetup.Country).ToListAsync();
                countryGroups.ForEach(d => d.IsDefault = false);
                _repository.TmkCECountrySetups.UpdateRange(countryGroups);
                await _repository.SaveChangesAsync();
            }

            _repository.TmkCECountrySetups.Add(countrySetup);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateCECountrySetup(TmkCECountrySetup countrySetup)
        {
            //Only 1 default setup per country
            if (countrySetup.IsDefault)
            {
                var countryGroups = await _repository.TmkCECountrySetups.Where(d => d.Country == countrySetup.Country && d.CECountryId != countrySetup.CECountryId).ToListAsync();
                countryGroups.ForEach(d => d.IsDefault = false);
                _repository.TmkCECountrySetups.UpdateRange(countryGroups);
                await _repository.SaveChangesAsync();
            }

            _repository.TmkCECountrySetups.Update(countrySetup);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteCECountrySetup(TmkCECountrySetup countrySetup)
        {
            _repository.TmkCECountrySetups.Remove(countrySetup);
            await _repository.SaveChangesAsync();
        }

        public async Task CopyCECountrySetup(int oldCECountryId, int newCECountryId, string userName, bool copyCosts)
        {
            var updatedDate = DateTime.Now;

            //Copy costs
            if (copyCosts)
            {
                var oldCosts = await _repository.TmkCECountryCosts.AsNoTracking()
                                        .Where(d => d.CECountryId == oldCECountryId)
                                        .ToListAsync();

                foreach (var cost in oldCosts)
                {
                    var newCost = new TmkCECountryCost()
                    {
                        CECountryId = newCECountryId,
                        Description = cost.Description,
                        DataType = cost.DataType,
                        DefaultValue = cost.DefaultValue,
                        Opts = cost.Opts,
                        Cost = cost.Cost,
                        AltCost = cost.AltCost,
                        MultCost = cost.MultCost,
                        OrderOfEntry = cost.OrderOfEntry,
                        CostType = cost.CostType,
                        Stage = cost.Stage,
                        MarkType = cost.MarkType,
                        UseCostFactor = cost.UseCostFactor,
                        CostFormula = cost.CostFormula,
                        CostFactor1 = cost.CostFactor1,
                        CostFactor2 = cost.CostFactor2,
                        CostFactor3 = cost.CostFactor3,
                        TranslationType = cost.TranslationType,
                        ActiveSwitch = cost.ActiveSwitch,
                        CPICost = false,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = updatedDate,
                        LastUpdate = updatedDate
                    };
                    _repository.TmkCECountryCosts.Add(newCost);
                    await _repository.SaveChangesAsync();

                    var newChildCosts = await _repository.TmkCECountryCostChilds.AsNoTracking()
                                            .Where(d => d.CostId == cost.CostId)
                                            .Select(d => new TmkCECountryCostChild()
                                            {
                                                OldCCId = d.CCId,
                                                CostId = newCost.CostId,
                                                CDescription = d.CDescription,
                                                CDataType = d.CDataType,
                                                CDefaultValue = d.CDefaultValue,
                                                CAltValue = d.CAltValue,
                                                COpts = d.COpts,
                                                CAltOpts = d.CAltOpts,
                                                CCost = d.CCost,
                                                CAltCost = d.CAltCost,
                                                CMultCost = d.CMultCost,
                                                COrderOfEntry = d.COrderOfEntry,
                                                CurrencyType = d.CurrencyType,                                                
                                                CActiveSwitch = d.CActiveSwitch,
                                                CCPICost = false,
                                                CreatedBy = userName,
                                                UpdatedBy = userName,
                                                DateCreated = updatedDate,
                                                LastUpdate = updatedDate                                                
                                            })
                                            .ToListAsync();
                    if (newChildCosts.Any())
                    {
                        _repository.TmkCECountryCostChilds.AddRange(newChildCosts);
                        await _repository.SaveChangesAsync();

                        var newSubCosts = new List<TmkCECountryCostSub>();
                        foreach (var newChild in newChildCosts)
                        {
                            newSubCosts.AddRange(await _repository.TmkCECountryCostSubs.AsNoTracking()
                                .Where(d => d.CCId == newChild.OldCCId)
                                .Select(d => new TmkCECountryCostSub()
                                {
                                    CCId = newChild.CCId,
                                    SDescription = d.SDescription,
                                    SDataType = d.SDataType,
                                    SDefaultValue = d.SDefaultValue,
                                    SAltValue = d.SAltValue,
                                    SOpts = d.SOpts,
                                    SAltOpts = d.SAltOpts,
                                    SCost = d.SCost,
                                    SAltCost = d.SAltCost,
                                    SMultCost = d.SMultCost,
                                    SOrderOfEntry = d.SOrderOfEntry,
                                    SUseCostFactor = d.SUseCostFactor,
                                    SCostFormula = d.SCostFormula,
                                    SCostFactor1 = d.SCostFactor1,
                                    SCostFactor2 = d.SCostFactor2,
                                    SCostFactor3 = d.SCostFactor3,
                                    STranslationType = d.STranslationType,
                                    SActiveSwitch = d.SActiveSwitch,
                                    SCPICost = false,
                                    CreatedBy = userName,
                                    UpdatedBy = userName,
                                    DateCreated = updatedDate,
                                    LastUpdate = updatedDate,
                                })
                                .ToListAsync());
                        }

                        if (newSubCosts != null && newSubCosts.Count > 0)
                        {
                            _repository.TmkCECountryCostSubs.AddRange(newSubCosts);
                            await _repository.SaveChangesAsync();
                        }
                    }
                }

            }
        }

        #region Question Guide
        public async Task UpdateChild(int parentId, string userName, IEnumerable<TmkCECountryCost> updated, IEnumerable<TmkCECountryCost> added, IEnumerable<TmkCECountryCost> deleted)
        {
            if (updated.Any())
            {
                foreach (var item in updated)
                {
                    var oldDataType = await TmkCECountryCosts.AsNoTracking().Where(d => d.CostId == item.CostId).FirstOrDefaultAsync();
                    var childCosts = await TmkCECountryCostChildren.Where(d => d.CostId == item.CostId).ToListAsync();

                    if (oldDataType != null && oldDataType.DataType != item.DataType && childCosts.Any())
                    {
                        _repository.TmkCECountryCostChilds.RemoveRange(childCosts);
                    }

                    // remove sub costs with CostFactors if parent cost CostType changed from Translation
                    if (!string.IsNullOrEmpty(item.CostType) && item.CostType.ToLower() != "translation")
                    {
                        var subCostsWithCostFactors = await TmkCECountryCostSubs.Where(d => d.TmkCECountryCostChild != null && d.TmkCECountryCostChild.CostId == item.CostId 
                            && d.SDataType == TmkCECostDataType.Numeric && d.SUseCostFactor).ToListAsync();
                        if (subCostsWithCostFactors != null && subCostsWithCostFactors.Count > 0)
                        {
                            //subCostsWithCostFactors.ForEach(d => { 
                            //    d.SDefaultValue = "0"; 
                            //    d.SCost = 0;
                            //    d.SAltCost = 0;
                            //    d.SMultCost = 0;
                            //    d.SOpts = "=";
                            //    d.SUseCostFactor = false;
                            //    d.SCostFormula = null;
                            //    d.SCostFactor1 = 0;
                            //    d.SCostFactor2 = 0;
                            //    d.SCostFactor3 = 0;
                            //});
                            _repository.Set<TmkCECountryCostSub>().RemoveRange(subCostsWithCostFactors);
                        }                        
                    }
                }
                _repository.Set<TmkCECountryCost>().UpdateRange(updated);
            }

            if (added.Any())
            {
                var startIndex = await GetCECountryCostNextOrderOfEntry(parentId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
                _repository.Set<TmkCECountryCost>().AddRange(added);
            }

            if (deleted.Any())
                _repository.Set<TmkCECountryCost>().RemoveRange(deleted);

            await UpdateParentStampsAsync(parentId, userName);
            await _repository.SaveChangesAsync();
        }

        public async Task ReorderCECountryCost(int id, string userName, int newIndex)
        {
            var countryCost = await TmkCECountryCosts.SingleOrDefaultAsync(a => a.CostId == id);
            Guard.Against.NoRecordPermission(countryCost != null);

            if (countryCost == null) return;

            countryCost.UpdatedBy = userName;
            countryCost.LastUpdate = DateTime.Now;

            int cECountryId = countryCost.CECountryId;
            int oldIndex = countryCost.OrderOfEntry;

            var countrySetup = await TmkCECountrySetups.Where(w => w.CECountryId == cECountryId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(countrySetup != null);

            if (countrySetup == null) return;

            countrySetup.UpdatedBy = countryCost.UpdatedBy;
            countrySetup.LastUpdate = countryCost.LastUpdate;

            List<TmkCECountryCost> countryCosts = new List<TmkCECountryCost>();
            if (oldIndex > newIndex)
            {
                countryCosts = await TmkCECountryCosts.Where(w => w.CECountryId == cECountryId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                countryCosts.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                countryCosts = await TmkCECountryCosts.Where(w => w.CECountryId == cECountryId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                countryCosts.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            countryCost.OrderOfEntry = newIndex;
            countryCosts.Add(countryCost);

            _repository.Set<TmkCECountryCost>().UpdateRange(countryCosts);
            _repository.TmkCECountrySetups.Update(countrySetup);
            await _repository.SaveChangesAsync();
        }

        private async Task<int> GetCECountryCostNextOrderOfEntry(int cECountryId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                if (await TmkCECountryCosts.AnyAsync(d => d.CECountryId == cECountryId))
                {
                    lastOrderOfEntry = await TmkCECountryCosts.Where(ma => ma.CECountryId == cECountryId).MaxAsync(ma => ma.OrderOfEntry);
                    return lastOrderOfEntry + 1;
                }
                return lastOrderOfEntry;
            }
            catch (Exception e)
            {
                var err = e.Message;
                return lastOrderOfEntry;
            }
            //return lastOrderOfEntry + 1;
        }


        public async Task UpdateCostChild(int parentId, string userName, IEnumerable<TmkCECountryCostChild> updated, IEnumerable<TmkCECountryCostChild> added, IEnumerable<TmkCECountryCostChild> deleted)
        {
            if (updated.Any())
            {
                _repository.Set<TmkCECountryCostChild>().UpdateRange(updated);
            }

            if (added.Any())
            {
                var startIndex = await GetCECostChildNextOrderOfEntry(parentId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.COrderOfEntry = startIndex++;
                }
                _repository.Set<TmkCECountryCostChild>().AddRange(added);
            }

            if (deleted.Any())
                _repository.Set<TmkCECountryCostChild>().RemoveRange(deleted);

            await UpdateCostParentStampsAsync(parentId, userName);
            await _repository.SaveChangesAsync();
        }
        private async Task<int> GetCECostChildNextOrderOfEntry(int costId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                if (await TmkCECountryCostChildren.AnyAsync(d => d.CostId == costId))
                {
                    lastOrderOfEntry = await TmkCECountryCostChildren.Where(ma => ma.CostId == costId).MaxAsync(ma => ma.COrderOfEntry);
                    return lastOrderOfEntry + 1;
                }
                return lastOrderOfEntry;
            }
            catch (Exception e)
            {
                var err = e.Message;
                return lastOrderOfEntry;
            }
            //return lastOrderOfEntry + 1;
        }
        public async Task ReorderCECostChild(int id, string userName, int newIndex)
        {
            var costChild = await TmkCECountryCostChildren.SingleOrDefaultAsync(a => a.CCId == id);
            Guard.Against.NoRecordPermission(costChild != null);

            if (costChild == null) return;

            costChild.UpdatedBy = userName;
            costChild.LastUpdate = DateTime.Now;

            int costId = costChild.CostId;
            int oldIndex = costChild.COrderOfEntry;

            var costSetup = await TmkCECountryCosts.Where(w => w.CostId == costId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(costSetup != null);

            if (costSetup == null) return;

            costSetup.UpdatedBy = costChild.UpdatedBy;
            costSetup.LastUpdate = costChild.LastUpdate;

            List<TmkCECountryCostChild> costChildren = new List<TmkCECountryCostChild>();
            if (oldIndex > newIndex)
            {
                costChildren = await TmkCECountryCostChildren.Where(w => w.CostId == costId && w.COrderOfEntry >= newIndex && w.COrderOfEntry < oldIndex).ToListAsync();
                costChildren.ForEach(m => m.COrderOfEntry = m.COrderOfEntry + 1);
            }
            else
            {
                costChildren = await TmkCECountryCostChildren.Where(w => w.CostId == costId && w.COrderOfEntry <= newIndex && w.COrderOfEntry > oldIndex).ToListAsync();
                costChildren.ForEach(m => m.COrderOfEntry = m.COrderOfEntry - 1);
            }
            costChild.COrderOfEntry = newIndex;
            costChildren.Add(costChild);

            _repository.Set<TmkCECountryCostChild>().UpdateRange(costChildren);
            _repository.TmkCECountryCosts.Update(costSetup);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateCostSub(int parentId, string userName, IEnumerable<TmkCECountryCostSub> updated, IEnumerable<TmkCECountryCostSub> added, IEnumerable<TmkCECountryCostSub> deleted)
        {
            if (updated.Any())
            {
                _repository.Set<TmkCECountryCostSub>().UpdateRange(updated);
            }

            if (added.Any())
            {
                var startIndex = await GetCECostSubNextOrderOfEntry(parentId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.SOrderOfEntry = startIndex++;
                }
                _repository.Set<TmkCECountryCostSub>().AddRange(added);
            }

            if (deleted.Any())
                _repository.Set<TmkCECountryCostSub>().RemoveRange(deleted);

            await UpdateCostChildStampsAsync(parentId, userName);
            await _repository.SaveChangesAsync();
        }
        private async Task<int> GetCECostSubNextOrderOfEntry(int ccId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                if (await TmkCECountryCostSubs.AnyAsync(d => d.CCId == ccId))
                {
                    lastOrderOfEntry = await TmkCECountryCostSubs.Where(ma => ma.CCId == ccId).MaxAsync(ma => ma.SOrderOfEntry);
                    return lastOrderOfEntry + 1;
                }
                return lastOrderOfEntry;
            }
            catch (Exception e)
            {
                var err = e.Message;
                return lastOrderOfEntry;
            }
            //return lastOrderOfEntry + 1;
        }
        public async Task ReorderCECostSub(int id, string userName, int newIndex)
        {
            var costSub = await TmkCECountryCostSubs.SingleOrDefaultAsync(a => a.SubId == id);
            Guard.Against.NoRecordPermission(costSub != null);

            if (costSub == null) return;

            costSub.UpdatedBy = userName;
            costSub.LastUpdate = DateTime.Now;

            int ccId = costSub.CCId;
            int oldIndex = costSub.SOrderOfEntry;

            var costChild = await TmkCECountryCostChildren.Where(w => w.CCId == ccId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(costChild != null);

            if (costChild == null) return;

            costChild.UpdatedBy = costSub.UpdatedBy;
            costChild.LastUpdate = costSub.LastUpdate;

            List<TmkCECountryCostSub> costSubs = new List<TmkCECountryCostSub>();
            if (oldIndex > newIndex)
            {
                costSubs = await TmkCECountryCostSubs.Where(w => w.CCId == ccId && w.SOrderOfEntry >= newIndex && w.SOrderOfEntry < oldIndex).ToListAsync();
                costSubs.ForEach(m => m.SOrderOfEntry = m.SOrderOfEntry + 1);
            }
            else
            {
                costSubs = await TmkCECountryCostSubs.Where(w => w.CCId == ccId && w.SOrderOfEntry <= newIndex && w.SOrderOfEntry > oldIndex).ToListAsync();
                costSubs.ForEach(m => m.SOrderOfEntry = m.SOrderOfEntry - 1);
            }
            costSub.SOrderOfEntry = newIndex;
            costSubs.Add(costSub);

            _repository.Set<TmkCECountryCostSub>().UpdateRange(costSubs);
            _repository.TmkCECountryCostChilds.Update(costChild);
            await _repository.SaveChangesAsync();
        }
        #endregion        

        

        private async Task UpdateParentStampsAsync(int ceCountryId, string userName)
        {
            var countrySetup = await _repository.TmkCECountrySetups.Where(w => w.CECountryId == ceCountryId).FirstOrDefaultAsync();

            if (countrySetup != null)
            {
                countrySetup.UpdatedBy = userName;
                countrySetup.LastUpdate = DateTime.Now;
                countrySetup.FeesEffDate = DateTime.Now;

                var entity = _repository.TmkCECountrySetups.Attach(countrySetup);
                entity.Property(c => c.UpdatedBy).IsModified = true;
                entity.Property(c => c.LastUpdate).IsModified = true;
            }
        }

        private async Task UpdateCostParentStampsAsync(int costId, string userName)
        {
            var parentCost = await TmkCECountryCosts.Where(w => w.CostId == costId).FirstOrDefaultAsync();

            if (parentCost != null)
            {
                parentCost.UpdatedBy = userName;
                parentCost.LastUpdate = DateTime.Now;

                var entity = _repository.TmkCECountryCosts.Attach(parentCost);
                entity.Property(c => c.UpdatedBy).IsModified = true;
                entity.Property(c => c.LastUpdate).IsModified = true;

                if (parentCost.CECountryId > 0)
                    await UpdateParentStampsAsync(parentCost.CECountryId, userName);
            }
        }

        private async Task UpdateCostChildStampsAsync(int ccId, string userName)
        {
            var costChild = await TmkCECountryCostChildren.Where(w => w.CCId == ccId).FirstOrDefaultAsync();

            if (costChild != null)
            {
                costChild.UpdatedBy = userName;
                costChild.LastUpdate = DateTime.Now;

                var entity = _repository.TmkCECountryCostChilds.Attach(costChild);
                entity.Property(c => c.UpdatedBy).IsModified = true;
                entity.Property(c => c.LastUpdate).IsModified = true;

                if (costChild.CostId > 0)
                {
                    await UpdateCostParentStampsAsync(costChild.CostId, userName);
                }
            }
        }
    }
}
