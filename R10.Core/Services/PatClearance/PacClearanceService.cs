using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Entities.PatClearance;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.DMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;

namespace R10.Core.Services
{
    public class PacClearanceService : EntityService<PacClearance>, IPacClearanceService
    {
        private readonly ISystemSettings<PacSetting> _settings;
        private readonly IApplicationDbContext _repository;

        public PacClearanceService(
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user,
            ISystemSettings<PacSetting> settings,
            IApplicationDbContext repository
            ) : base(cpiDbContext, user)

        {
            _settings = settings;
            _repository = repository;
        }

        public override IQueryable<PacClearance> QueryableList
        {
            get
            {
                IQueryable<PacClearance> clearances = base.QueryableList;

                if (_user.HasEntityFilter())
                {
                    clearances = clearances.Where(EntityFilter());
                }

                return clearances;
            }
        }

        public IQueryable<PacClearanceCopySetting> PacClearanceCopySettings => _cpiDbContext.GetRepository<PacClearanceCopySetting>().QueryableList;
        public IQueryable<PacClearanceCopyDisclosureSetting> PacClearanceCopyDisclosureSettings => _cpiDbContext.GetRepository<PacClearanceCopyDisclosureSetting>().QueryableList;

        public IQueryable<PacClearance> ForReviewList
        {
            get
            {
                var forReviewList = QueryableList.Where(d => d.ClearanceStatus.ToLower() != "draft");

                if (_user.GetEntityFilterType() == CPiEntityType.ContactPerson)
                    forReviewList = forReviewList.Where(c => UserEntityFilters.Any(f => f.UserId == UserId && c.Client.ClientContacts.Any(r => r.ContactID == f.EntityId)));

                return forReviewList;
            }
        }

        public Expression<Func<PacClearance, bool>> EntityFilter()
        {
            var userEntityType = _user.GetEntityFilterType();

            switch (userEntityType)
            {
                case CPiEntityType.Client:
                    return c => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == c.ClientID && (c.ClearanceStatus.ToLower() != "draft" || c.UserId == UserId));
                case CPiEntityType.Attorney:
                    return c => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == c.AttorneyID && (c.ClearanceStatus.ToLower() != "draft" || c.UserId == UserId));
                case CPiEntityType.Inventor:
                    return c => UserEntityFilters.Any(f => f.UserId == UserId && (c.Inventors.Any(ci => ci.InventorID == f.EntityId) || c.UserId == UserId));
                //REVIEWERS
                case CPiEntityType.ContactPerson:
                    //Only requestor within the division
                    return c => UserEntityFilters.Any(f => f.UserId == UserId
                                    && (c.Client.ClientContacts.Any(r => r.ContactID == f.EntityId) && c.ClearanceStatus.ToLower() != "draft")
                                    )
                                ||
                                //Only requestor creating the record 
                                UserEntityFilters.Any(f => f.UserId == UserId
                                    && (c.UserId == UserId)
                                    );
            }

            return a => true; ;
        }

        public IQueryable<PacQuestionGroup> QuestionGroups => _cpiDbContext.GetReadOnlyRepositoryAsync<PacQuestionGroup>().QueryableList;
        public IQueryable<PacQuestion> PacQuestions => _cpiDbContext.GetReadOnlyRepositoryAsync<PacQuestion>().QueryableList;

        public override async Task<PacClearance> GetByIdAsync(int pacId)
        {
            return await QueryableList.SingleOrDefaultAsync(d => d.PacId == pacId);
        }

        public override async Task Add(PacClearance clearance)
        {
            var isModifyUser = await ValidateRole(CPiPermissions.FullModify); 
            
            if (clearance.CaseNumber == "{auto}") clearance.CaseNumber = await BuildCaseNumber(clearance.CaseNumber);

            if (clearance.ClearanceStatusDate == null) clearance.ClearanceStatusDate = DateTime.Now.Date;

            _cpiDbContext.GetRepository<PacClearance>().Add(clearance);

            await _cpiDbContext.SaveChangesAsync();

            //IGNORE DEFAULT INVENTOR IF MODIFY USERS
            if (!isModifyUser) await AddDefaultInventor(clearance);

            await AddClearanceQuestions(clearance);

            //LOG NEW STATUS
            await LogStatusHistory(clearance);

            await _cpiDbContext.SaveChangesAsync();
        }

        private async Task AddDefaultInventor(PacClearance clearance)
        {
            //DO NOT ALLOW SAVE IF USER ACCOUNT HAS NO LINKED INVENTOR
            if (_user.HasEntityFilter() && _user.GetEntityFilterType() == CPiEntityType.Inventor)
            {
                var inventorId = await GetUserInventorId();
                if (inventorId == 0)
                    throw new ValueNotAllowedException("Your user account is not linked to an Inventor. Unable to add clearance search.");

                _cpiDbContext.GetRepository<PacInventor>().Add(new PacInventor
                {
                    PacId = clearance.PacId,
                    InventorID = inventorId,
                    OrderOfEntry = 1,
                    CreatedBy = clearance.CreatedBy,
                    UpdatedBy = clearance.UpdatedBy,
                    DateCreated = clearance.DateCreated,
                    LastUpdate = clearance.LastUpdate
                });
            }
            
        }

        public async Task<int> GetUserInventorId()
        {
            //Re-use Reviewer permission so Inventor account has Reviewer permission in Patent Clearance
            if (await ValidateRole(CPiPermissions.Reviewer))
                return await UserEntityFilters.Where(e => e.UserId == UserId && e.CPiUser.EntityFilterType == CPiEntityType.Inventor).Select(e => e.EntityId).FirstOrDefaultAsync();

            return 0;
        }

        private async Task AddClearanceQuestions(PacClearance clearance)
        {
            var questionGuides = await _cpiDbContext.GetReadOnlyRepositoryAsync<PacQuestionGuide>().QueryableList
                .Where(q => q.AddToFuture)
                .Select(q => new PacQuestion
            {
                PacId = clearance.PacId,
                QuestionId = q.QuestionId,
                CreatedBy = clearance.CreatedBy,
                UpdatedBy = clearance.UpdatedBy,
                DateCreated = clearance.DateCreated,
                LastUpdate = clearance.LastUpdate
            }).ToListAsync();

            _cpiDbContext.GetRepository<PacQuestion>().Add(questionGuides);
            await _cpiDbContext.SaveChangesAsync();
            _cpiDbContext.Detach(questionGuides);
        }

        public override async Task Update(PacClearance clearance)
        {
            await ValidatePermission(clearance.PacId);

            //Update Status Date
            var existing = await QueryableList.FirstOrDefaultAsync(c => c.PacId == clearance.PacId);
            var logStatusHistory = false;
            if (existing != null && existing.ClearanceStatus != clearance.ClearanceStatus)
            {
                clearance.ClearanceStatusDate = DateTime.Now.Date;
                logStatusHistory = true;
            }

            _cpiDbContext.GetRepository<PacClearance>().Update(clearance);

            if (logStatusHistory)
                await LogStatusHistory(clearance);

            //GET STATUS
            var clearanceStatus = await _cpiDbContext.GetReadOnlyRepositoryAsync<PacClearanceStatus>().QueryableList.Where(s => s.ClearanceStatus == clearance.ClearanceStatus).FirstOrDefaultAsync();
            Guard.Against.ValueNotAllowed(clearanceStatus != null, "ClearanceStatus");

            await _cpiDbContext.SaveChangesAsync();
        }

        //UPDATING REMARKS FROM REVIEW SCREEN NEEDS NEW tStamp AS RETURN VALUE
        public new async Task<byte[]> UpdateRemarks(PacClearance clearance)
        {
            //Clearance user can no longer edit remarks  when disclosure is for review.
            Guard.Against.NoRecordPermission(await ValidateRole(CPiPermissions.Reviewer.Concat(CPiPermissions.FullModify).ToList()));

            //Validate disclosure record using ForReviewList
            var updated = await ForReviewList.Where(d => d.PacId == clearance.PacId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(updated != null);

            _cpiDbContext.GetRepository<PacClearance>().Attach(updated);
            updated.Remarks = clearance.Remarks;
            updated.UpdatedBy = clearance.UpdatedBy;
            updated.LastUpdate = clearance.LastUpdate;
            updated.tStamp = clearance.tStamp;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }

        public async Task<byte[]> UpdateStatus(PacClearance clearance, string remarks)
        {
            //REVIEWERS AND FULLMDOFIY CAN UPDATE RECOMMENDATION
            Guard.Against.NoRecordPermission(await ValidateRole(CPiPermissions.Reviewer.Concat(CPiPermissions.FullModify).ToList()));

            //Validate disclosure record using ForReviewList
            var updated = await ForReviewList.Where(d => d.PacId == clearance.PacId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(updated != null);

            //GET STATUS
            var clearanceStatus = await _cpiDbContext.GetReadOnlyRepositoryAsync<PacClearanceStatus>().QueryableList.Where(r => r.ClearanceStatus == clearance.ClearanceStatus).FirstOrDefaultAsync();
            Guard.Against.ValueNotAllowed(clearanceStatus != null, "ClearanceStatus");

            string oldClearanceStatus = updated.ClearanceStatus;

            _cpiDbContext.GetRepository<PacClearance>().Attach(updated);
            updated.ClearanceStatus = clearance.ClearanceStatus;
            updated.ClearanceStatusDate = clearance.LastUpdate;
            updated.UpdatedBy = clearance.UpdatedBy;
            updated.LastUpdate = clearance.LastUpdate;
            updated.tStamp = clearance.tStamp;

            await LogStatusHistory(updated, remarks);

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(updated);

            return updated.tStamp;
        }

        public override async Task Delete(PacClearance clearance)
        {
            await ValidatePermission(clearance.PacId);

            _cpiDbContext.GetRepository<PacClearance>().Delete(clearance);
            await _cpiDbContext.SaveChangesAsync();
        }

        private async Task LogStatusHistory(PacClearance clearance, string remarks = "")
        {
            var statusHistory = new PacClearanceStatusHistory()
            {
                PacId = clearance.PacId,
                OldStatus = "",
                NewStatus = clearance.ClearanceStatus,
                Remarks = remarks,
                CreatedBy = clearance.UpdatedBy,
                DateChanged = clearance.LastUpdate,
            };

            var existing = await _cpiDbContext.GetReadOnlyRepositoryAsync<PacClearance>().QueryableList.Where(t => t.PacId == clearance.PacId).FirstOrDefaultAsync();

            if (existing != null && clearance.ClearanceStatus != existing.ClearanceStatus)
                statusHistory.OldStatus = existing.ClearanceStatus;

            _cpiDbContext.GetRepository<PacClearanceStatusHistory>().Add(statusHistory);
        }

        public IQueryable<PacClearanceStatusHistory> GetStatusHistory(int pacId)
        {
            if (_user.HasEntityFilter())
                return _cpiDbContext.GetReadOnlyRepositoryAsync<PacClearanceStatusHistory>().QueryableList.Where(e => e.PacId == pacId && QueryableList.Any(d => d.PacId == e.PacId));
            else
                return _cpiDbContext.GetReadOnlyRepositoryAsync<PacClearanceStatusHistory>().QueryableList.Where(e => e.PacId == pacId);
        }

        public async Task<bool> UserCanEditClearance(int dmsId)
        {
            var isModifyUser = await ValidateRole(CPiPermissions.FullModify);
            return isModifyUser;
        }

        public async Task ValidatePermission(int pacId)
        {
            if (_user.HasEntityFilter())
            {
                Guard.Against.NoRecordPermission(await QueryableList.AnyAsync(d => d.PacId == pacId));
            }
        }

        private async Task<bool> ValidateRole(List<string> roles)
        {
            return await ValidatePermission(SystemType.PatClearance, roles, null);
        }
        
        public async Task<bool> CanSubmitClearance(int pacId)
        {
            string statusCanSubmitClearance = await QueryableList.Where(d => d.PacId == pacId).Select(d => d.ClearanceStatus).FirstOrDefaultAsync();
            return statusCanSubmitClearance.ToLower() == "draft";
        }

        public IQueryable<T> QueryableChildList<T>() where T : BaseEntity
        {
            var queryableList = _repository.Set<T>() as IQueryable<T>;

            if (_user.HasRespOfficeFilter(SystemType.PatClearance) || _user.HasEntityFilter())
                queryableList = queryableList.Where(a => this.QueryableList.Any(ca => ca.PacId == EF.Property<int>(a, "PacId")));

            return queryableList;
        }

        public async Task Submit(PacClearance submitted)
        {
            ////ONLY MODIFY USERS CAN SUBMIT
            await ValidatePermission(submitted.PacId);
            Guard.Against.NoRecordPermission(await CanSubmitClearance(submitted.PacId));

            var clearance = await QueryableList.Where(d => d.PacId == submitted.PacId).FirstOrDefaultAsync();

            var submittedStatus = await _cpiDbContext.GetReadOnlyRepositoryAsync<PacClearanceStatus>().QueryableList
                                            .Where(s => s.ClearanceStatus.ToLower() == "submitted").Select(s => s.ClearanceStatus).FirstOrDefaultAsync();

            _cpiDbContext.GetRepository<PacClearance>().Attach(clearance);
            clearance.ClearanceStatus = submittedStatus;
            clearance.ClearanceStatusDate = submitted.LastUpdate;
            clearance.UpdatedBy = submitted.UpdatedBy;
            clearance.LastUpdate = submitted.LastUpdate;
            clearance.tStamp = submitted.tStamp;

            if (clearance.DateRequested == null) { clearance.DateRequested = submitted.LastUpdate; }

            //LOG NEW STATUS
            await LogStatusHistory(clearance);

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(clearance);
        }        

        public async Task<int> CheckStatusHistoryRemark(int pacId)
        {
            var lastStatus = await _repository.PacClearanceStatusesHistory
                                                .Where(h => h.PacId == pacId)
                                                .OrderByDescending(h => h.LogID)
                                                .FirstOrDefaultAsync();

            if (lastStatus != null && lastStatus.OldStatus.ToLower() == "submitted" && lastStatus.NewStatus.ToLower() != "draft" && string.IsNullOrEmpty(lastStatus.Remarks))
            {
                return lastStatus.LogID;
            }
            else
            {
                return 0;
            }
        }

        public async Task UpdateStatusRemark(int pacId, int logId, string remarks)
        {
            var clearance = await GetByIdAsync(pacId);
            await ValidatePermission(clearance.PacId);

            var statusHistory = await _repository.PacClearanceStatusesHistory.Where(h => h.PacId == pacId && h.LogID == logId).FirstOrDefaultAsync();
            if (statusHistory != null)
            {
                statusHistory.Remarks = remarks;
                _cpiDbContext.GetRepository<PacClearanceStatusHistory>().Update(statusHistory);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task CopyClearance(int oldPacId, int newPacId, string userName, bool copyCaseInfo, bool copyKeywords, bool CopyImages, string copiedQuestions)
        {
            await _cpiDbContext.ExecuteSqlRawAsync($"procPacClearanceCopy @OldPacId={oldPacId},@NewPacId={newPacId},@CreatedBy='{userName}',@CopyCaseInfo={copyCaseInfo},@CopyKeywords={copyKeywords},@CopyImages={CopyImages},@CopiedQuestions='{copiedQuestions}'");
        }

        public async Task RefreshCopySetting(List<PacClearanceCopySetting> added, List<PacClearanceCopySetting> deleted)
        {
            if (added.Count > 0)
                _cpiDbContext.GetRepository<PacClearanceCopySetting>().Add(added);

            if (deleted.Count > 0)
                _cpiDbContext.GetRepository<PacClearanceCopySetting>().Delete(deleted);
           
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task UpdateCopySetting(PacClearanceCopySetting setting)
        {            
            var existing = await _cpiDbContext.GetRepository<PacClearanceCopySetting>().GetByIdAsync(setting.CopySettingId);
            if (existing != null)
            {
                existing.Copy = setting.Copy;                
                _cpiDbContext.GetRepository<PacClearanceCopySetting>().Update(existing);                
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task CopyToDisclosure(int pacId, int dmsId, bool copyKeywords)
        {
            var clearance = await GetByIdAsync(pacId);
            Guard.Against.NoRecordPermission(clearance != null);

            var disclosure = await _cpiDbContext.GetReadOnlyRepositoryAsync<Disclosure>().QueryableList.Where(d => d.DMSId == dmsId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(disclosure != null);

            //Copy Requestors from Patent Clearance to Invention Disclosure Inventors
            var requestors = await _cpiDbContext.GetReadOnlyRepositoryAsync<PacInventor>().QueryableList.Where(d => d.PacId == clearance.PacId).ToListAsync();
            if (requestors.Any())
            {
                var inventorList = new List<DMSInventor>();
                foreach (var requestor in requestors)
                {
                    var newInventor = new DMSInventor()
                    {
                        DMSId = disclosure.DMSId,
                        InventorID = requestor.InventorID,
                        OrderOfEntry = requestor.OrderOfEntry,
                        Remarks = requestor.Remarks,
                        CreatedBy = disclosure.CreatedBy,
                        UpdatedBy = disclosure.UpdatedBy,
                        DateCreated = disclosure.DateCreated,
                        LastUpdate = disclosure.LastUpdate
                    };
                    inventorList.Add(newInventor);
                }

                if (inventorList.Any())
                {
                    _cpiDbContext.GetRepository<DMSInventor>().Add(inventorList);
                    await _cpiDbContext.SaveChangesAsync();
                }
            }

            if (copyKeywords)
            {
                var keywords = await _cpiDbContext.GetReadOnlyRepositoryAsync<PacKeyword>().QueryableList.Where(d => d.PacId == clearance.PacId).ToListAsync();
                if (keywords.Any())
                {
                    var newKeywordList = new List<DMSKeyword>();
                    foreach (var keyword in keywords)
                    {
                        var newKeyword = new DMSKeyword()
                        {
                            DMSId = disclosure.DMSId,
                            Keyword = keyword.Keyword,
                            OrderOfEntry = keyword.OrderOfEntry,
                            CreatedBy = disclosure.CreatedBy,
                            UpdatedBy = disclosure.UpdatedBy,
                            DateCreated = disclosure.DateCreated,
                            LastUpdate = disclosure.LastUpdate
                        };
                        newKeywordList.Add(newKeyword);
                    }
                    if (newKeywordList.Any())
                    {
                        _cpiDbContext.GetRepository<DMSKeyword>().Add(newKeywordList);
                        await _cpiDbContext.SaveChangesAsync();
                    }

                }
            }
        }

        public async Task RefreshCopyDisclosureSetting(List<PacClearanceCopyDisclosureSetting> added, List<PacClearanceCopyDisclosureSetting> deleted)
        {
            if (added.Count > 0)
                _cpiDbContext.GetRepository<PacClearanceCopyDisclosureSetting>().Add(added);

            if (deleted.Count > 0)
                _cpiDbContext.GetRepository<PacClearanceCopyDisclosureSetting>().Delete(deleted);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task UpdateCopyDisclosureSetting(PacClearanceCopyDisclosureSetting setting)
        {
            var existing = await _cpiDbContext.GetRepository<PacClearanceCopyDisclosureSetting>().GetByIdAsync(setting.CopySettingId);
            if (existing != null)
            {
                existing.Copy = setting.Copy;
                _cpiDbContext.GetRepository<PacClearanceCopyDisclosureSetting>().Update(existing);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task<string> BuildCaseNumber(string caseNumber)
        {
            string currentYear = DateTime.Now.Year.ToString();

            var tempCaseNumber = "P-" + currentYear + "-AAAA";

            //Get latest sequential # of current year
            var numList = await _cpiDbContext.GetReadOnlyRepositoryAsync<PacClearance>().QueryableList.Where(cl => EF.Functions.Like(cl.CaseNumber, "P-" + currentYear + "-[0-9][0-9][0-9][0-9]%")).Select(cl => Int32.Parse(cl.CaseNumber.Substring(7, 4))).ToListAsync();

            if (numList.Count > 0)
            {
                var latestNum = numList.Max();

                if (latestNum < 9999)
                {
                    latestNum++;
                    return tempCaseNumber.Replace("AAAA", latestNum.ToString("D4"));
                }
                else
                {
                    //Back to 1 if exceed 9999
                    return tempCaseNumber.Replace("AAAA", "0001");
                }
            }
            else
            {
                return tempCaseNumber.Replace("AAAA", "0001");
            }
        }

        public async Task<List<PacWorkflowAction>> CheckWorkflowAction(PacWorkflowTriggerType triggerType)
        {
            var actions = await _repository.PacWorkflowActions.Where(w => w.Workflow != null && w.Workflow.TriggerTypeId == (int)triggerType && w.Workflow.ActiveSwitch)
                .Include(w => w.Workflow).OrderBy(w => w.OrderOfEntry).ToListAsync();
            return actions;
        }  
       
        public async Task CheckRequiredOnSubmission(int pacId)
        {
            //Check if there are any required questions
            var questionGroups = await _cpiDbContext.GetReadOnlyRepositoryAsync<PacQuestion>().QueryableList
                                            .Where(q => q.PacId == pacId
                                                    && q.PacQuestionGuide.ActiveSwitch && q.PacQuestionGuide.RequiredOnSubmission
                                                    && string.IsNullOrEmpty(q.Answer))
                                            .Select(q => q.PacQuestionGuide.PacQuestionGroup.GroupName)
                                            .Distinct().ToListAsync();
            if (questionGroups.Count > 0)
            {
                throw new ValueNotAllowedException($"Please answer question(s) in {questionGroups.FirstOrDefault()} tab.");
            }
        }
        
    }
}