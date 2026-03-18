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

namespace R10.Core.Services
{
    public class PatCountryLawService : IPatCountryLawService
    {
        private readonly IApplicationDbContext _repository;
        private readonly IPatCountryDueRepository _countryDueRepository;
        private readonly ISystemSettings<PatSetting> _settings;
        
        public PatCountryLawService(IApplicationDbContext repository,
            IPatCountryDueRepository countryDueRepository,
           ISystemSettings<PatSetting> settings) 
        {
            _repository = repository;
            _countryDueRepository = countryDueRepository;
           _settings = settings;
        }

        public async Task AddCountryLaw(PatCountryLaw countryLaw)
        {
            _repository.PatCountryLaws.Add(countryLaw);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateCountryLaw(PatCountryLaw countryLaw)
        {
            _repository.PatCountryLaws.Update(countryLaw);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateCountryLawRemarks(PatCountryLaw countryLaw)
        {
            var entity = _repository.PatCountryLaws.Attach(countryLaw);
            entity.Property(c => c.UserRemarks).IsModified = true;
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteCountryLaw(PatCountryLaw countryLaw)
        {
            _repository.PatCountryLaws.Remove(countryLaw);
            await _repository.SaveChangesAsync();
        }

        public async Task<bool> HasDesignatedCountries(string country,string caseType)
        {
            return await PatDesCaseTypes.AnyAsync(d => d.IntlCode == country && d.CaseType == caseType);
        }


        //not needed
        //use BasedOnOption and GetPublicConstantValues type extension
        //public List<LookupDTO> GetBasedOnList()
        //{
        //    var list = new List<LookupDTO>
        //    {
        //        new LookupDTO() {Text = "Filing"},
        //        new LookupDTO() {Text = "Issue"},
        //        new LookupDTO() {Text = "Parent Filing"},
        //        new LookupDTO() {Text = "Parent Issue"},
        //        new LookupDTO() {Text = "PCT"},
        //        new LookupDTO() {Text = "Priority"},
        //        new LookupDTO() {Text = "Publication"},
        //        new LookupDTO() {Text = ""}
        //    };
        //    return list;
        //}

        public async Task<List<LookupDTO>> GetExpirationTypeList()
        {
            var list = new List<LookupDTO>
            {
                new LookupDTO() {Text = "Expiration"},
            };
            var settings = await _settings.GetSetting();
            if (settings.IsTaxStartCalcOn)
            {
                list.Add(new LookupDTO() { Text = "Tax start" });
            }
            return list;
        }

        public async Task UpdateChild<T>(int parentId, string country, string caseType, string userName, byte[] tStamp, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            if (updated.Any())
                _repository.Set<T>().UpdateRange(updated);

            if (added.Any())
                _repository.Set<T>().AddRange(added);

            if (deleted.Any())
                _repository.Set<T>().RemoveRange(deleted);

            UpdateParentStamps(parentId, country, caseType,userName, tStamp);
            await _repository.SaveChangesAsync();
        }

        public async Task AddChildren<T>(IEnumerable<T> entities) where T : BaseEntity
        {
            if (entities.Any())
            {
                _repository.Set<T>().AddRange(entities);
                await _repository.SaveChangesAsync();
            }
        }

        public async Task DeleteCountryDue(int parentId, string country, string caseType, string userName, byte[] tStamp, IEnumerable<PatCountryDue> deleted) 
        {
            if (deleted.Any())
            {
                var followUpAction = await _repository.PatActionTypes.FirstOrDefaultAsync(a => a.CDueId == deleted.FirstOrDefault().CDueId);
                if (followUpAction != null)
                    _repository.PatActionTypes.Remove(followUpAction);

                await UpdateChild(parentId, country, caseType, userName, tStamp, new List<PatCountryDue>(), new List<PatCountryDue>(), deleted);
            }
        }

        #region CountryDue

        public async Task<List<PatCountryDue>> GetCountryDues(int countryLawId)
        {
            return await _repository.PatCountryDues.Where(c => c.CountryLawID == countryLawId).AsNoTracking().ToListAsync();
        }

        public async Task<PatCountryDue> GetCountryDue(int cDueId)
        {
            var countryDue = await _repository.PatCountryDues.Where(c => c.CDueId == cDueId)
                                              .AsNoTracking().Include(c=>c.PatCountryLaw).FirstOrDefaultAsync();
            var followUpAction = await _repository.PatActionTypes.FirstOrDefaultAsync(a => a.CDueId == cDueId);
            if (followUpAction != null)
            {
                countryDue.FollowupAction = followUpAction.ActionType;
                countryDue.OldFollowupAction = followUpAction.ActionType;
            }

            //not needed
            //use RecurringOption enum for Recurring values
            //use GetDisplayName enum extension to display Recurring description
            //if (countryDue.CPIAction)
            //{
            //    countryDue.RecurringDesc = GetRecurringDesc(countryDue.Recurring);
            //}
            countryDue.ParentTStamp = countryDue.PatCountryLaw.tStamp;
            return countryDue;
        }

        public async Task<List<LookupDTO>> GetActionDues()
        {
            return await _repository.PatCountryDues.Select(c => new LookupDTO() { Text = c.ActionDue }).Distinct().OrderBy(c => c.Text).AsNoTracking().ToListAsync();
        }

        public async Task<List<LookupDTO>> GetActionTypes()
        {
            return await _repository.PatCountryDues.Select(c => new LookupDTO() { Text = c.ActionType }).Distinct().OrderBy(c => c.Text).AsNoTracking().ToListAsync();
        }

        public async Task CountryDueUpdate(PatCountryDue countryDue)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                if (countryDue.CDueId > 0)
                {
                    _repository.PatCountryDues.Update(countryDue);
                }
                else
                {
                    _repository.PatCountryDues.Add(countryDue);
                    countryDue.OldFollowupAction = ""; //maybe from copy
                }

                // if (!string.IsNullOrEmpty(countryDue.ActionType) &&
                //        await _repository.FFInstrxTypeAction.AsNoTracking().AnyAsync(a => a.ActionType == countryDue.ActionType && (string.IsNullOrEmpty(a.Country) || a.Country == countryDue.Country)) &&
                //     (!(await _repository.FFReminderSetup.AsNoTracking().AnyAsync(r => r.Country == countryDue.Country && r.CaseType == countryDue.CaseType && r.ActionType == countryDue.ActionType && string.IsNullOrEmpty(r.ActionDue)))))
                // {
                //     _repository.FFReminderSetup.Add(new FFReminderSetup()
                //     {
                //         Country = countryDue.Country,
                //         CaseType = countryDue.CaseType,
                //         ActionType = countryDue.ActionType,
                //         ActionDue = "",
                //         CreatedBy = countryDue.CreatedBy,
                //         UpdatedBy = countryDue.UpdatedBy,
                //         DateCreated = countryDue.DateCreated,
                //         LastUpdate = countryDue.LastUpdate,
                //     });
                // }


                UpdateParentStamps(countryDue.CountryLawID, countryDue.Country, countryDue.CaseType, countryDue.UpdatedBy, countryDue.ParentTStamp);
                await _repository.SaveChangesAsync(); //we need to get the CDueId

                if (countryDue.FollowupAction != countryDue.OldFollowupAction)
                {
                    //delete old
                    if (!string.IsNullOrEmpty(countryDue.OldFollowupAction))
                    {
                        var oldFollowUpAction = await _repository.PatActionTypes.FirstOrDefaultAsync(a => a.ActionType == countryDue.OldFollowupAction && (a.CDueId == countryDue.CDueId));
                        if (oldFollowUpAction != null)
                            _repository.PatActionTypes.Remove(oldFollowUpAction);
                    }

                    if (!string.IsNullOrEmpty(countryDue.FollowupAction))
                    {
                        //set new
                        var newFollowUpAction = await _repository.PatActionTypes.AsNoTracking().FirstOrDefaultAsync(a => a.ActionType == countryDue.FollowupAction && (a.CDueId == null || a.CDueId == 0));
                        if (newFollowUpAction != null)
                        {
                            var addFollowUp = new PatActionType();
                            addFollowUp.ActionType = newFollowUpAction.ActionType;
                            addFollowUp.Country = countryDue.Country;
                            addFollowUp.CDueId = countryDue.CDueId;
                            addFollowUp.FollowUpGen = -1;
                            addFollowUp.CreatedBy = countryDue.UpdatedBy;
                            addFollowUp.UpdatedBy = countryDue.UpdatedBy;
                            addFollowUp.DateCreated = countryDue.LastUpdate;
                            addFollowUp.LastUpdate = countryDue.LastUpdate;
                            _repository.PatActionTypes.Add(addFollowUp);
                        }
                    }
                    await _repository.SaveChangesAsync(); 
                }
                scope.Complete();
            }


        }

        //public async Task CountryDuesUpdate(int parentId, string userName, byte[] tStamp,
        //    IList<PatCountryDue> updatedCountryDues, IList<PatCountryDue> newCountryDues,
        //    IList<PatCountryDue> deletedCountryDues)
        //{
        //    if (updatedCountryDues.Any())
        //    {
        //        _repository.PatCountryDues.UpdateRange(updatedCountryDues);
        //    }

        //    if (newCountryDues.Any())
        //    {
        //        _repository.PatCountryDues.AddRange(newCountryDues);
        //    }

        //    UpdateParentStamps(parentId, userName, tStamp);
        //    await _repository.SaveChangesAsync();
        //}

        //public async Task CountryDueDelete(PatCountryDue deletedCountryDue)
        //{
        //    UpdateParentStamps(deletedCountryDue.CountryLawID, deletedCountryDue.UpdatedBy, deletedCountryDue.ParentTStamp);
        //    _repository.PatCountryDues.Remove(deletedCountryDue);
        //    await _repository.SaveChangesAsync();
        //}

        //not needed
        //use RecurringOption enum for Recurring values
        //public List<LookupDTO> GetRecurringOptions()
        //{
        //    var list = new List<LookupDTO>
        //    {
        //        new LookupDTO() {Value="0", Text = "Non Recurring"},
        //        new LookupDTO() {Value="1", Text = "Based on Taken Date"},
        //        new LookupDTO() {Value="-1", Text = "Based on Due Date"}
        //    };
        //    return list;
        //}

        //not needed
        //use RecurringOption enum for Recurring values
        //use GetDisplayName enum extension to display Recurring description
        //public string GetRecurringDesc(short? value)
        //{
        //    var option = GetRecurringOptions().FirstOrDefault(o => o.Value == value.ToString());
        //    return option?.Text;
        //}

        public async Task<List<LookupDTO>> GetFollowupList(string country)
        {
            return await _repository.PatActionTypes
                .Where(a => a.CDueId == 0 && (a.Country == "" || a.Country == country)).OrderBy(a => a.ActionType)
                .Select(a => new LookupDTO() { Text = a.ActionType })
                .ToListAsync();
        }

        public async Task GenerateCountryLawActions(CountryLawRetroParam criteria)
        {
            await _countryDueRepository.GenerateCountryLawActions(criteria);
        }
        #endregion

        #region CountryExp
        public async Task<List<PatCountryExp>> GetCountryExps(int countryLawId)
        {
            return await _repository.PatCountryExpirations.Where(c => c.CountryLawID == countryLawId).AsNoTracking().ToListAsync();
        }

        //public async Task CountryExpsUpdate(int parentId, string userName, byte[] tStamp,
        //    IList<PatCountryExp> updatedCountryExps, IList<PatCountryExp> newCountryExps,
        //    IList<PatCountryExp> deletedCountryExps)
        //{
        //    if (updatedCountryExps.Any())
        //    {
        //        _repository.PatCountryExpirations.UpdateRange(updatedCountryExps);
        //    }

        //    if (newCountryExps.Any())
        //    {
        //        _repository.PatCountryExpirations.AddRange(newCountryExps);
        //    }

        //    UpdateParentStamps(parentId, userName, tStamp);
        //    await _repository.SaveChangesAsync();
        //}

        //public async Task CountryExpDelete(PatCountryExp deletedCountryExp)
        //{
        //    UpdateParentStamps(deletedCountryExp.CountryLawID, deletedCountryExp.UpdatedBy, deletedCountryExp.ParentTStamp);
        //    _repository.PatCountryExpirations.Remove(deletedCountryExp);
        //    await _repository.SaveChangesAsync();
        //}
        #endregion

        #region PatDesCaseType
        //public async Task DesCaseTypesUpdate(int parentId, string userName, byte[] tStamp, IList<PatDesCaseType> updatedDesCaseTypes,
        //    IList<PatDesCaseType> newDesCaseTypes, IList<PatDesCaseType> deletedDesCaseTypes)
        //{
        //    if (updatedDesCaseTypes.Any())
        //    {
        //        _repository.PatDesCaseTypes.UpdateRange(updatedDesCaseTypes);
        //    }
        //    if (newDesCaseTypes.Any())
        //    {
        //        _repository.PatDesCaseTypes.AddRange(updatedDesCaseTypes);
        //    }

        //    UpdateParentStamps(parentId, userName,tStamp);
        //    await _repository.SaveChangesAsync();
        //}

        //public async Task DesCaseTypeDelete(PatDesCaseType deletedDesCaseType)
        //{
        //    UpdateParentStamps(deletedDesCaseType.CountryLawID, deletedDesCaseType.UpdatedBy, deletedDesCaseType.ParentTStamp);
        //    _repository.PatDesCaseTypes.Remove(deletedDesCaseType);
        //    await _repository.SaveChangesAsync();
        //}
        #endregion

        public IQueryable<PatCountryLaw> PatCountryLaws => _repository.PatCountryLaws.AsNoTracking();
        public IQueryable<PatCaseType> PatCaseTypes => _repository.PatCaseTypes.AsNoTracking();
        public IQueryable<PatCountryDue> PatCountryDues => _repository.PatCountryDues.AsNoTracking();
        public IQueryable<PatDesCaseType> PatDesCaseTypes => _repository.PatDesCaseTypes.AsNoTracking();

        protected void UpdateParentStamps(int countryLawId, string country, string caseType, string userName, byte[] tStamp)
        {
            var countryLaw = new PatCountryLaw() { CountryLawID = countryLawId, Country = country, CaseType = caseType, UpdatedBy = userName, LastUpdate = DateTime.Now, tStamp = tStamp };
            var entity = _repository.PatCountryLaws.Attach(countryLaw);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
        }
    }
}
