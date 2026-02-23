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
    public class PatCEAnnuitySetupService : IPatCEAnnuitySetupService
    {
        private readonly IApplicationDbContext _repository;        
        private readonly ISystemSettings<PatSetting> _settings;
        
        public PatCEAnnuitySetupService(IApplicationDbContext repository,            
           ISystemSettings<PatSetting> settings) 
        {
            _repository = repository;           
           _settings = settings;
        }

        public IQueryable<PatCEAnnuitySetup> PatCEAnnuitySetups => _repository.PatCEAnnuitySetups.AsNoTracking();
        public IQueryable<PatCaseType> PatCaseTypes => _repository.PatCaseTypes.AsNoTracking();
        public IQueryable<PatCEAnnuityCost> PatCEAnnuityCosts => _repository.PatCEAnnuityCosts.AsNoTracking();

        public async Task AddAnnuitySetup(PatCEAnnuitySetup annuitySetup)
        {
            annuitySetup.EntityStatus = annuitySetup.EntityStatus ?? "";
            _repository.PatCEAnnuitySetups.Add(annuitySetup);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateAnnuitySetup(PatCEAnnuitySetup annuitySetup)
        {
            annuitySetup.EntityStatus = annuitySetup.EntityStatus ?? "";
            _repository.PatCEAnnuitySetups.Update(annuitySetup);
            await _repository.SaveChangesAsync();
        }        

        public async Task DeleteAnnuitySetup(PatCEAnnuitySetup annuitySetup)
        {
            _repository.PatCEAnnuitySetups.Remove(annuitySetup);
            await _repository.SaveChangesAsync();
        }

        public async Task<string?> CheckExistingAnnuitySetup(PatCEAnnuitySetup annuitySetup)
        {
            var existingSetup = await PatCEAnnuitySetups.Where(d => d.Country == annuitySetup.Country 
                                                            && (d.EntityStatus ?? "") == (annuitySetup.EntityStatus ?? "")
                                                            && d.CEAnnuityId > 0 
                                                            && d.CEAnnuityId != annuitySetup.CEAnnuityId)
                                                        .FirstOrDefaultAsync();          

            if (existingSetup != null)
            {
                if (existingSetup.CaseType == null && annuitySetup.CaseType == null) return "";

                var existingCaseType = (existingSetup.CaseType ?? "").Split("|").Where(d => !string.IsNullOrEmpty(d)).ToList();
                var currentCaseType = (annuitySetup.CaseType ?? "").Split("|").Where(d => !string.IsNullOrEmpty(d)).ToList();
                if (!existingCaseType.Any() && !currentCaseType.Any()) { 
                    return ""; 
                }
                else if (currentCaseType.Any(d => existingCaseType.Contains(d))) {
                    var temp = currentCaseType.Where(d => existingCaseType.Contains(d)).ToList();
                    return string.Join(", ", temp);
                }
            }
            return null;
        }

        public async Task CopyAnnuitySetup(int oldCEAnnuityId, int newCEAnnuityId, string userName, bool copyCosts)
        {
            var updatedDate = DateTime.Now;

            //Copy costs
            if (copyCosts)
            {
                var oldCosts = await _repository.PatCEAnnuityCosts.AsNoTracking()
                                        .Where(d => d.CEAnnuityId == oldCEAnnuityId)
                                        .ToListAsync();
                var newCosts = new List<PatCEAnnuityCost>();
                foreach (var cost in oldCosts)
                {
                    var newCost = new PatCEAnnuityCost()
                    {
                        CEAnnuityId = newCEAnnuityId,
                        CostType = cost.CostType,                       
                        Y1 = cost.Y1,
                        Y2 = cost.Y2,
                        Y3 = cost.Y3,
                        Y4 = cost.Y4,
                        Y5 = cost.Y5,
                        Y6 = cost.Y6,
                        Y7 = cost.Y7,
                        Y8 = cost.Y8,
                        Y9 = cost.Y9,
                        Y10 = cost.Y10,
                        Y11 = cost.Y11,
                        Y12 = cost.Y12,
                        Y13 = cost.Y13,
                        Y14 = cost.Y14,
                        Y15 = cost.Y15,
                        Y16 = cost.Y16,
                        Y17 = cost.Y17,
                        Y18 = cost.Y18,
                        Y19 = cost.Y19,
                        Y20 = cost.Y20,
                        ActiveSwitch = cost.ActiveSwitch,
                        CPICost = false,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = updatedDate,
                        LastUpdate = updatedDate
                    };
                    newCosts.Add(newCost);                   
                }
                if (newCosts.Any())
                {
                    _repository.PatCEAnnuityCosts.AddRange(newCosts);
                    await _repository.SaveChangesAsync();
                }
            }
        }
    }
}
