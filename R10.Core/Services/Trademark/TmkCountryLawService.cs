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
// using R10.Core.Entities.RMS; // Removed during deep clean

namespace R10.Core.Services
{
    public class TmkCountryLawService : ITmkCountryLawService
    {
        private readonly IApplicationDbContext _repository;
        private readonly ITmkCountryDueRepository _countryDueRepository;

        public TmkCountryLawService(IApplicationDbContext repository,
            ITmkCountryDueRepository countryDueRepository)

        {
            _repository = repository;
            _countryDueRepository = countryDueRepository;
        }

        public async Task AddCountryLaw(TmkCountryLaw countryLaw)
        {
            _repository.TmkCountryLaws.Add(countryLaw);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateCountryLaw(TmkCountryLaw countryLaw)
        {
            _repository.TmkCountryLaws.Update(countryLaw);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateCountryLawRemarks(TmkCountryLaw countryLaw)
        {
            var entity = _repository.TmkCountryLaws.Attach(countryLaw);
            entity.Property(c => c.UserRemarks).IsModified = true;
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteCountryLaw(TmkCountryLaw countryLaw)
        {
            _repository.TmkCountryLaws.Remove(countryLaw);
            await _repository.SaveChangesAsync();
        }

        public async Task<bool> HasDesignatedCountries(string country, string caseType)
        {
            return await TmkDesCaseTypes.AnyAsync(d => d.IntlCode == country && d.CaseType == caseType);
        }

        public async Task UpdateChild<T>(int parentId, string country, string caseType, string userName, byte[] tStamp, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            if (updated.Any())
                _repository.Set<T>().UpdateRange(updated);

            if (added.Any())
                _repository.Set<T>().AddRange(added);

            if (deleted.Any())
                _repository.Set<T>().RemoveRange(deleted);

            UpdateParentStamps(parentId, country, caseType, userName, tStamp);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteCountryDue(int parentId, string country, string caseType, string userName, byte[] tStamp, IEnumerable<TmkCountryDue> deleted)
        {
            if (deleted.Any())
            {
                var followUpAction = await _repository.TmkActionTypes.FirstOrDefaultAsync(a => a.CDueId == deleted.FirstOrDefault().CDueId);
                if (followUpAction != null)
                    _repository.TmkActionTypes.Remove(followUpAction);

                await UpdateChild(parentId, country, caseType, userName, tStamp, new List<TmkCountryDue>(), new List<TmkCountryDue>(), deleted);
            }
        }

        public List<LookupDTO> GetBasedOnList()
        {
            var list = new List<LookupDTO>
            {
                new LookupDTO() {Text = "Allowance"},
                new LookupDTO() {Text = "Filing"},
                new LookupDTO() {Text = "Priority"},
                new LookupDTO() {Text = "Publication"},
                new LookupDTO() {Text = "Registration"},
                new LookupDTO() {Text = "Renewal"},
                new LookupDTO() {Text = "Parent Filing"},
                new LookupDTO() {Text = ""}
            };
            return list;
        }

        #region CountryDue

        public async Task<List<TmkCountryDue>> GetCountryDues(int countryLawId)
        {
            return await _repository.TmkCountryDues.Where(c => c.CountryLawID == countryLawId).AsNoTracking().ToListAsync();
        }

        public async Task<TmkCountryDue> GetCountryDue(int cDueId)
        {
            var countryDue = await _repository.TmkCountryDues.Where(c => c.CDueId == cDueId)
                                              .AsNoTracking().Include(c => c.TmkCountryLaw).FirstOrDefaultAsync();
            var followUpAction = await _repository.TmkActionTypes.FirstOrDefaultAsync(a => a.CDueId == cDueId);
            if (followUpAction != null)
            {
                countryDue.FollowupAction = followUpAction.ActionType;
                countryDue.OldFollowupAction = followUpAction.ActionType;
            }

            if (countryDue.CPIAction)
            {
                countryDue.RecurringDesc = GetRecurringDesc(countryDue.Recurring);
            }
            countryDue.ParentTStamp = countryDue.TmkCountryLaw.tStamp;
            return countryDue;
        }

        public async Task<List<LookupDTO>> GetActionDues()
        {
            return await _repository.TmkCountryDues.Select(c => new LookupDTO() { Text = c.ActionDue }).Distinct().OrderBy(c => c.Text).AsNoTracking().ToListAsync();
        }

        public async Task<List<LookupDTO>> GetActionTypes()
        {
            return await _repository.TmkCountryDues.Select(c => new LookupDTO() { Text = c.ActionType }).Distinct().OrderBy(c => c.Text).AsNoTracking().ToListAsync();
        }

        public async Task CountryDueUpdate(TmkCountryDue countryDue)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {

                if (countryDue.CDueId > 0)
                {
                    _repository.TmkCountryDues.Update(countryDue);
                }
                else
                {
                    _repository.TmkCountryDues.Add(countryDue);
                    countryDue.OldFollowupAction = ""; //maybe from copy

                    // Removed during deep clean - RMS module removed
                    // if (!string.IsNullOrEmpty(countryDue.ActionType) &&
                    //        await _repository.RMSInstrxTypeAction.AsNoTracking().AnyAsync(a => a.ActionType == countryDue.ActionType) &&
                    //     (!(await _repository.RMSReminderSetup.AsNoTracking().AnyAsync(r => r.Country == countryDue.Country && r.CaseType == countryDue.CaseType && r.ActionType == countryDue.ActionType && string.IsNullOrEmpty(r.ActionDue)))))
                    // {
                    //     _repository.RMSReminderSetup.Add(new RMSReminderSetup()
                    //     {
                    //         Country = countryDue.Country,
                    //         CaseType = countryDue.CaseType,
                    //         ActionType = countryDue.ActionType,
                    //         ActionDue = ""
                    //     });
                    // }
                }

                UpdateParentStamps(countryDue.CountryLawID, countryDue.Country, countryDue.CaseType, countryDue.UpdatedBy, countryDue.ParentTStamp);
                await _repository.SaveChangesAsync(); //we need to get the CDueId

                if (countryDue.FollowupAction != countryDue.OldFollowupAction)
                {
                    //delete old
                    if (!string.IsNullOrEmpty(countryDue.OldFollowupAction))
                    {
                        var oldFollowUpAction = await _repository.TmkActionTypes.FirstOrDefaultAsync(a => a.ActionType == countryDue.OldFollowupAction && (a.CDueId == countryDue.CDueId));
                        if (oldFollowUpAction != null)
                            _repository.TmkActionTypes.Remove(oldFollowUpAction);
                    }

                    if (!string.IsNullOrEmpty(countryDue.FollowupAction))
                    {
                        //set new
                        var newFollowUpAction = await _repository.TmkActionTypes.AsNoTracking().FirstOrDefaultAsync(a => a.ActionType == countryDue.FollowupAction && (a.CDueId == null || a.CDueId == 0));
                        if (newFollowUpAction != null)
                        {
                            var addFollowUp = new TmkActionType();
                            addFollowUp.ActionType = newFollowUpAction.ActionType;
                            addFollowUp.Country = countryDue.Country;
                            addFollowUp.CDueId = countryDue.CDueId;
                            addFollowUp.FollowUpGen = -1;
                            addFollowUp.CreatedBy = countryDue.UpdatedBy;
                            addFollowUp.UpdatedBy = countryDue.UpdatedBy;
                            addFollowUp.DateCreated = countryDue.LastUpdate;
                            addFollowUp.LastUpdate = countryDue.LastUpdate;
                            _repository.TmkActionTypes.Add(addFollowUp);
                        }
                    }
                    await _repository.SaveChangesAsync();
                }
                scope.Complete();
            }
        }

        //public async Task CountryDuesUpdate(int parentId, string userName, byte[] tStamp,
        //    IList<TmkCountryDue> updatedCountryDues, IList<TmkCountryDue> newCountryDues,
        //    IList<TmkCountryDue> deletedCountryDues)
        //{
        //    if (updatedCountryDues.Any())
        //    {
        //        _repository.TmkCountryDues.UpdateRange(updatedCountryDues);
        //    }

        //    if (newCountryDues.Any())
        //    {
        //        _repository.TmkCountryDues.AddRange(newCountryDues);
        //    }

        //    UpdateParentStamps(parentId, userName, tStamp);
        //    await _repository.SaveChangesAsync();
        //}

        //public async Task CountryDueDelete(TmkCountryDue deletedCountryDue)
        //{
        //    UpdateParentStamps(deletedCountryDue.CountryLawID, deletedCountryDue.UpdatedBy, deletedCountryDue.ParentTStamp);
        //    _repository.TmkCountryDues.Remove(deletedCountryDue);
        //    await _repository.SaveChangesAsync();
        //}

        public List<LookupDTO> GetRecurringOptions()
        {
            var list = new List<LookupDTO>
            {
                new LookupDTO() {Value="0", Text = "Non Recurring"},
                new LookupDTO() {Value="1", Text = "Based on Taken Date"},
                new LookupDTO() {Value="-1", Text = "Based on Due Date"}
            };
            return list;
        }

        public string GetRecurringDesc(short? value)
        {
            var option = GetRecurringOptions().FirstOrDefault(o => o.Value == value.ToString());
            return option?.Text;
        }

        public async Task<List<LookupDTO>> GetFollowupList(string country)
        {
            return await _repository.TmkActionTypes
                .Where(a => a.CDueId == 0 && (a.Country == "" || a.Country == country)).OrderBy(a => a.ActionType)
                .Select(a => new LookupDTO() { Text = a.ActionType })
                .ToListAsync();
        }

        public async Task GenerateCountryLawActions(CountryLawRetroParam criteria)
        {
            await _countryDueRepository.GenerateCountryLawActions(criteria);
        }


        #endregion



        #region TmkDesCaseType
        //public async Task DesCaseTypesUpdate(int parentId, string userName, byte[] tStamp, IList<TmkDesCaseType> updatedDesCaseTypes,
        //    IList<TmkDesCaseType> newDesCaseTypes, IList<TmkDesCaseType> deletedDesCaseTypes)
        //{
        //    if (updatedDesCaseTypes.Any())
        //    {
        //        _repository.TmkDesCaseTypes.UpdateRange(updatedDesCaseTypes);
        //    }
        //    if (newDesCaseTypes.Any())
        //    {
        //        _repository.TmkDesCaseTypes.AddRange(updatedDesCaseTypes);
        //    }

        //    UpdateParentStamps(parentId, userName, tStamp);
        //    await _repository.SaveChangesAsync();
        //}

        //public async Task DesCaseTypeDelete(TmkDesCaseType deletedDesCaseType)
        //{
        //    UpdateParentStamps(deletedDesCaseType.CountryLawID, deletedDesCaseType.UpdatedBy, deletedDesCaseType.ParentTStamp);
        //    _repository.TmkDesCaseTypes.Remove(deletedDesCaseType);
        //    await _repository.SaveChangesAsync();
        //}
        #endregion

        public IQueryable<TmkCountryLaw> TmkCountryLaws => _repository.TmkCountryLaws.AsNoTracking();
        public IQueryable<TmkCaseType> TmkCaseTypes => _repository.TmkCaseTypes.AsNoTracking();
        public IQueryable<TmkCountryDue> TmkCountryDues => _repository.TmkCountryDues;
        public IQueryable<TmkDesCaseType> TmkDesCaseTypes => _repository.TmkDesCaseTypes.AsNoTracking();

        protected void UpdateParentStamps(int countryLawId, string country, string caseType, string userName, byte[] tStamp)
        {
            var countryLaw = new TmkCountryLaw() { CountryLawID = countryLawId, Country = country, CaseType = caseType, UpdatedBy = userName, LastUpdate = DateTime.Now, tStamp = tStamp };
            var entity = _repository.TmkCountryLaws.Attach(countryLaw);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
        }
    }
}
