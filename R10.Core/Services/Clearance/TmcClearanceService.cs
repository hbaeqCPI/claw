using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Clearance;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class TmcClearanceService : EntityService<TmcClearance>, ITmcClearanceService
    {
        private readonly ISystemSettings<TmcSetting> _settings;
        private readonly IApplicationDbContext _repository;

        public TmcClearanceService(
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user,
            ISystemSettings<TmcSetting> settings,
            IApplicationDbContext repository
            ) : base(cpiDbContext, user)

        {
            _settings = settings;
            _repository = repository;
        }

        public override IQueryable<TmcClearance> QueryableList
        {
            get
            {
                IQueryable<TmcClearance> clearances = base.QueryableList;

                if (_user.HasEntityFilter())
                {
                    clearances = clearances.Where(EntityFilter());
                }

                return clearances;
            }
        }

        public IQueryable<TmcClearanceCopySetting> TmcClearanceCopySettings => _cpiDbContext.GetRepository<TmcClearanceCopySetting>().QueryableList;

        public IQueryable<TmcClearance> ForReviewList
        {
            get
            {
                var forReviewList = QueryableList.Where(d => d.ClearanceStatus.ToLower() != "draft");

                if (_user.GetEntityFilterType() == CPiEntityType.ContactPerson)
                    forReviewList = forReviewList.Where(c => UserEntityFilters.Any(f => f.UserId == UserId && c.Client.ClientContacts.Any(r => r.ContactID == f.EntityId)));

                return forReviewList;
            }
        }

        public Expression<Func<TmcClearance, bool>> EntityFilter()
        {
            var userEntityType = _user.GetEntityFilterType();

            switch (userEntityType)
            {
                case CPiEntityType.Client:
                    return c => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == c.ClientID && (c.ClearanceStatus.ToLower() != "draft" || c.UserId == UserId));
                case CPiEntityType.Attorney:
                    return c => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == c.AttorneyID && (c.ClearanceStatus.ToLower() != "draft" || c.UserId == UserId));
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

        public IQueryable<TmcQuestionGroup> QuestionGroups => _cpiDbContext.GetReadOnlyRepositoryAsync<TmcQuestionGroup>().QueryableList;
        public IQueryable<TmcQuestion> TmcQuestions => _cpiDbContext.GetReadOnlyRepositoryAsync<TmcQuestion>().QueryableList;

        public override async Task<TmcClearance> GetByIdAsync(int tmcId)
        {
            return await QueryableList.SingleOrDefaultAsync(d => d.TmcId == tmcId);
        }

        public override async Task Add(TmcClearance clearance)
        {
            //LET SQL GENERATE DEFAULT CASE NUMBER            
            if (clearance.CaseNumber == "{auto}") clearance.CaseNumber = null;

            if (clearance.ClearanceStatusDate == null) clearance.ClearanceStatusDate = DateTime.Now.Date;

            _cpiDbContext.GetRepository<TmcClearance>().Add(clearance);

            await _cpiDbContext.SaveChangesAsync();

            await AddClearanceQuestions(clearance);

            //LOG NEW STATUS
            await LogStatusHistory(clearance);

            await _cpiDbContext.SaveChangesAsync();
        }

        private async Task AddClearanceQuestions(TmcClearance clearance)
        {
            var questionGuides = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmcQuestionGuide>().QueryableList
                .Where(q => q.AddToFuture)
                .Select(q => new TmcQuestion
            {
                TmcId = clearance.TmcId,
                QuestionId = q.QuestionId,
                CreatedBy = clearance.CreatedBy,
                UpdatedBy = clearance.UpdatedBy,
                DateCreated = clearance.DateCreated,
                LastUpdate = clearance.LastUpdate
            }).ToListAsync();

            _cpiDbContext.GetRepository<TmcQuestion>().Add(questionGuides);
        }

        public override async Task Update(TmcClearance clearance)
        {
            await ValidatePermission(clearance.TmcId);

            //Update Status Date
            var existing = await QueryableList.FirstOrDefaultAsync(c => c.TmcId == clearance.TmcId);
            var logStatusHistory = false;
            if (existing != null && existing.ClearanceStatus != clearance.ClearanceStatus)
            {
                clearance.ClearanceStatusDate = DateTime.Now.Date;
                logStatusHistory = true;
            }

            _cpiDbContext.GetRepository<TmcClearance>().Update(clearance);

            if (logStatusHistory)
                await LogStatusHistory(clearance);

            //GET STATUS
            var clearanceStatus = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmcClearanceStatus>().QueryableList.Where(s => s.ClearanceStatus == clearance.ClearanceStatus).FirstOrDefaultAsync();
            Guard.Against.ValueNotAllowed(clearanceStatus != null, "SearchRequestStatus");

            if (clearanceStatus != null && clearanceStatus.GenerateTrademark && existing != null && existing.ClearanceStatus != clearanceStatus.ClearanceStatus)
            {
                await AddTrademarkRecord(clearance);
            }

            await _cpiDbContext.SaveChangesAsync();
        }
                
        public new async Task<byte[]> UpdateRemarks(TmcClearance clearance)
        {
            Guard.Against.NoRecordPermission(await ValidateRole(CPiPermissions.Reviewer.Concat(CPiPermissions.FullModify).ToList()));

            //Validate disclosure record using ForReviewList
            var updated = await ForReviewList.Where(d => d.TmcId == clearance.TmcId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(updated != null);

            _cpiDbContext.GetRepository<TmcClearance>().Attach(updated);
            updated.Remarks = clearance.Remarks;
            updated.UpdatedBy = clearance.UpdatedBy;
            updated.LastUpdate = clearance.LastUpdate;
            updated.tStamp = clearance.tStamp;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }

        public async Task<byte[]> UpdateStatus(TmcClearance clearance, string remarks)
        {
            //REVIEWERS AND FULLMDOFIY CAN UPDATE RECOMMENDATION
            Guard.Against.NoRecordPermission(await ValidateRole(CPiPermissions.Reviewer.Concat(CPiPermissions.FullModify).ToList()));

            //Validate disclosure record using ForReviewList
            var updated = await ForReviewList.Where(d => d.TmcId == clearance.TmcId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(updated != null);

            //GET STATUS
            var clearanceStatus = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmcClearanceStatus>().QueryableList.Where(r => r.ClearanceStatus == clearance.ClearanceStatus).FirstOrDefaultAsync();
            Guard.Against.ValueNotAllowed(clearanceStatus != null, "SearchRequestStatus");

            string oldClearanceStatus = updated.ClearanceStatus;

            _cpiDbContext.GetRepository<TmcClearance>().Attach(updated);
            updated.ClearanceStatus = clearance.ClearanceStatus;
            updated.ClearanceStatusDate = clearance.LastUpdate;
            updated.UpdatedBy = clearance.UpdatedBy;
            updated.LastUpdate = clearance.LastUpdate;
            updated.tStamp = clearance.tStamp;

            await LogStatusHistory(updated, remarks);

            if (clearanceStatus.GenerateTrademark)
            {
                await AddTrademarkRecord(clearance);
            }

            await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(updated);

            return updated.tStamp;
        }

        public override async Task Delete(TmcClearance clearance)
        {
            await ValidatePermission(clearance.TmcId);

            _cpiDbContext.GetRepository<TmcClearance>().Delete(clearance);
            await _cpiDbContext.SaveChangesAsync();
        }

        private async Task LogStatusHistory(TmcClearance clearance, string remarks = "")
        {
            var statusHistory = new TmcClearanceStatusHistory()
            {
                TmcId = clearance.TmcId,
                OldStatus = "",
                NewStatus = clearance.ClearanceStatus,
                Remarks = remarks,
                CreatedBy = clearance.UpdatedBy,
                DateChanged = clearance.LastUpdate,
            };

            var existing = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmcClearance>().QueryableList.Where(t => t.TmcId == clearance.TmcId).FirstOrDefaultAsync();

            if (existing != null && clearance.ClearanceStatus != existing.ClearanceStatus)
                statusHistory.OldStatus = existing.ClearanceStatus;

            _cpiDbContext.GetRepository<TmcClearanceStatusHistory>().Add(statusHistory);
        }

        public IQueryable<TmcClearanceStatusHistory> GetStatusHistory(int tmcId)
        {
            if (_user.HasEntityFilter())
                return _cpiDbContext.GetReadOnlyRepositoryAsync<TmcClearanceStatusHistory>().QueryableList.Where(e => e.TmcId == tmcId && QueryableList.Any(d => d.TmcId == e.TmcId));
            else
                return _cpiDbContext.GetReadOnlyRepositoryAsync<TmcClearanceStatusHistory>().QueryableList.Where(e => e.TmcId == tmcId);
        }

        public async Task<bool> UserCanEditClearance(int tmcId)
        {
            var isModifyUser = await ValidateRole(CPiPermissions.FullModify);
            return isModifyUser;
        }

        public async Task<bool> IsUserReviewerInTMC(int tmcId)
        {
            var tmcClearance = await GetByIdAsync(tmcId);
            var reviewerType = _user.GetEntityFilterType();
            var reviewerId = await GetUserReviewerId(reviewerType);

            if (reviewerId > 0)
                return await _cpiDbContext.GetRepository<ClientContact>().QueryableList
                                                .AnyAsync(cc => cc.ClientID == tmcClearance.ClientID && cc.ContactID == reviewerId);

            return false;
        }

        public async Task<int> GetUserReviewerId(CPiEntityType reviewerType)
        {
            if (await ValidateRole(CPiPermissions.Reviewer))
                return await UserEntityFilters.Where(e => e.UserId == UserId && e.CPiUser.EntityFilterType == reviewerType).Select(e => e.EntityId).FirstOrDefaultAsync();

            return 0;
        }

        public async Task ValidatePermission(int tmcId)
        {
            if (_user.HasEntityFilter())
            {
                Guard.Against.NoRecordPermission(await QueryableList.AnyAsync(d => d.TmcId == tmcId));
            }
        }

        private async Task<bool> ValidateRole(List<string> roles)
        {
            return await ValidatePermission(SystemType.SearchRequest, roles, null);
        }

        public async Task UpdateCountryInfo(int tmcId, string areaFieldName, string countries)
        {
            var clearance = await GetByIdAsync(tmcId);
            await ValidatePermission(clearance.TmcId);

            var properties = clearance.GetType().GetProperties();
            var property = properties.Where(t => t.Name.ToLower() == areaFieldName.ToLower()).FirstOrDefault();
            if (property != null)
            {
                property.SetValue(clearance, countries);
                _cpiDbContext.GetRepository<TmcClearance>().Update(clearance);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> CanSubmitClearance(int tmcId)
        {
            string statusCanSubmitClearance = await QueryableList.Where(d => d.TmcId == tmcId).Select(d => d.ClearanceStatus).FirstOrDefaultAsync();
            return statusCanSubmitClearance.ToLower() == "draft";
        }

        public IQueryable<T> QueryableChildList<T>() where T : BaseEntity
        {
            var queryableList = _repository.Set<T>() as IQueryable<T>;

            if (_user.HasRespOfficeFilter(SystemType.SearchRequest) || _user.HasEntityFilter())
                queryableList = queryableList.Where(a => this.QueryableList.Any(ca => ca.TmcId == EF.Property<int>(a, "TmcId")));

            return queryableList;
        }

        public async Task Submit(TmcClearance submitted)
        {            
            await ValidatePermission(submitted.TmcId);
            Guard.Against.NoRecordPermission(await CanSubmitClearance(submitted.TmcId));

            var clearance = await QueryableList.Where(d => d.TmcId == submitted.TmcId).FirstOrDefaultAsync();

            var submittedStatus = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmcClearanceStatus>().QueryableList
                                            .Where(s => s.ClearanceStatus.ToLower() == "submitted").Select(s => s.ClearanceStatus).FirstOrDefaultAsync();

            _cpiDbContext.GetRepository<TmcClearance>().Attach(clearance);
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

        public async Task<int> CheckStatusHistoryRemark(int tmcId)
        {
            var lastStatus = await _repository.TmcClearanceStatusesHistory
                                                .Where(h => h.TmcId == tmcId)
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

        public async Task UpdateStatusRemark(int tmcId, int logId, string remarks)
        {
            var clearance = await GetByIdAsync(tmcId);
            await ValidatePermission(clearance.TmcId);

            var statusHistory = await _repository.TmcClearanceStatusesHistory.Where(h => h.TmcId == tmcId && h.LogID == logId).FirstOrDefaultAsync();
            if (statusHistory != null)
            {
                statusHistory.Remarks = remarks;
                _cpiDbContext.GetRepository<TmcClearanceStatusHistory>().Update(statusHistory);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task CopyClearance(int oldTmcId, int newTmcId, string userName, bool copyCaseInfo,
            bool copyCountries, bool copyRequestedTerm, bool copyKeywords, bool CopyImages, string copiedQuestions)
        {
            await _cpiDbContext.ExecuteSqlRawAsync($"procTmcClearanceCopy @OldTmcId={oldTmcId},@NewTmcId={newTmcId},@CreatedBy='{userName}',@CopyCaseInfo={copyCaseInfo},@CopyCountries={copyCountries},@CopyRequestedTerm={copyRequestedTerm},@CopyKeywords={copyKeywords},@CopyImages={CopyImages},@CopiedQuestions='{copiedQuestions}'");
        }

        public async Task RefreshCopySetting(List<TmcClearanceCopySetting> added, List<TmcClearanceCopySetting> deleted)
        {
            if (added.Count > 0)
                _cpiDbContext.GetRepository<TmcClearanceCopySetting>().Add(added);

            if (deleted.Count > 0)
                _cpiDbContext.GetRepository<TmcClearanceCopySetting>().Delete(deleted);

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task UpdateCopySetting(TmcClearanceCopySetting setting)
        {
            var existing = await _cpiDbContext.GetRepository<TmcClearanceCopySetting>().GetByIdAsync(setting.CopySettingId);
            if (existing != null)
            {
                existing.Copy = setting.Copy;
                _cpiDbContext.GetRepository<TmcClearanceCopySetting>().Update(existing);

                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task<string> BuildCaseNumber(string caseNumber)
        {
            string currentYear = DateTime.Now.Year.ToString();

            var tempCaseNumber = "T-" + currentYear + "-AAAA";

            //Get latest sequential # of current year
            var numList = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmcClearance>().QueryableList.Where(cl => EF.Functions.Like(cl.CaseNumber, "T-" + currentYear + "-[0-9][0-9][0-9][0-9]%")).Select(cl => Int32.Parse(cl.CaseNumber.Substring(7, 4))).ToListAsync();

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

        public async Task<List<TmcWorkflowAction>> CheckWorkflowAction(TmcWorkflowTriggerType triggerType)
        {
            var actions = await _repository.TmcWorkflowActions.Where(w => w.Workflow != null && w.Workflow.TriggerTypeId == (int)triggerType && w.Workflow.ActiveSwitch)
                .Include(w => w.Workflow).OrderBy(w => w.OrderOfEntry).ToListAsync();
            return actions;
        }       

        public async Task CheckRequiredOnSubmission(int tmcId)
        {
            //Check if there are any required questions
            var questionGroups = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmcQuestion>().QueryableList
                                            .Where(q => q.TmcId == tmcId
                                                    && q.TmcQuestionGuide.ActiveSwitch && q.TmcQuestionGuide.RequiredOnSubmission
                                                    && string.IsNullOrEmpty(q.Answer))
                                            .Select(q => q.TmcQuestionGuide.TmcQuestionGroup.GroupName)
                                            .Distinct().ToListAsync();
            if (questionGroups.Count > 0)
            {
                throw new ValueNotAllowedException($"Please answer question(s) in {questionGroups.FirstOrDefault()} tab.");
            }
        }

        private async Task AddTrademarkRecord(TmcClearance clearance)
        {
            // check first if previously added
            var addTrademarkRecord = !(await _cpiDbContext.GetRepository<TmkTrademark>().QueryableList.AnyAsync(tmk => tmk.TmcId == clearance.TmcId));
            if (!addTrademarkRecord) return;

            var settings = await _settings.GetSetting();
            var tmcClearance = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmcClearance>().QueryableList.AsNoTracking()
                                    .Where(d => d.TmcId == clearance.TmcId)
                                    .Select(d => new { 
                                        d.TmcId, d.CaseNumber, d.ClearanceStatus, d.ClearanceStatusDate, d.ClientID, d.AttorneyID, 
                                        d.Remarks, d.DateCreated, d.CreatedBy
                                    })
                                    .FirstOrDefaultAsync();
            if (tmcClearance == null) return;

            var trademarkName = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmcQuestion>().QueryableList.AsNoTracking()
                                        .Where(d => d.TmcId == clearance.TmcId 
                                            && d.TmcQuestionGuide != null && d.TmcQuestionGuide.TmcQuestionGroup != null 
                                        )
                                        .Select(q => new
                                        {
                                            Answer = q.Answer ?? "",
                                            GroupOrderOfEntry = q.TmcQuestionGuide.TmcQuestionGroup!.OrderOfEntry,
                                            GuideOrderOfEntry = q.TmcQuestionGuide.OrderOfEntry
                                        })
                                        .OrderBy(o => o.GroupOrderOfEntry).ThenBy(t => t.GuideOrderOfEntry)
                                        .Select(d => d.Answer ?? "").FirstOrDefaultAsync();

            Guard.Against.NullOrEmpty(trademarkName, "TrademarkName(s)/Tagline");            

            var trademark = new TmkTrademark()
            {
                TmcId = clearance.TmcId,
                CaseNumber = tmcClearance.CaseNumber,
                Country = settings.DefaultClearanceCountry ?? "US",
                SubCase = "",
                CaseType = "ORD",
                TrademarkName = trademarkName,
                TrademarkStatusDate = tmcClearance.ClearanceStatusDate,
                ClientID = tmcClearance.ClientID,
                Attorney1ID = tmcClearance.AttorneyID,
                Remarks = tmcClearance.Remarks,
                CreatedBy = clearance.UpdatedBy,
                UpdatedBy = clearance.UpdatedBy,
                DateCreated = tmcClearance.DateCreated,
                LastUpdate = clearance.LastUpdate
            };
            _cpiDbContext.GetRepository<TmkTrademark>().Add(trademark);

            await _cpiDbContext.SaveChangesAsync();

            // add keywords
            var tmkKeywords = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmcKeyword>().QueryableList.Where(k => k.TmcId == clearance.TmcId)
                                        .Select(k => new TmkKeyword
                                        {
                                            TmkId = trademark.TmkId,
                                            Keyword = k.Keyword,
                                            CreatedBy = k.CreatedBy,
                                            UpdatedBy = k.UpdatedBy,
                                            DateCreated = k.DateCreated,
                                            LastUpdate = k.LastUpdate
                                        }).ToListAsync();

            _cpiDbContext.GetRepository<TmkKeyword>().Add(tmkKeywords);

            //add documents
            var docFolders = await _cpiDbContext.GetReadOnlyRepositoryAsync<DocFolder>().QueryableList.Where(f => f.SystemType == "C" && f.DataKey == "TmcId" && f.DataKeyValue == clearance.TmcId).Include(f => f.DocDocuments).ToListAsync();
            docFolders.ForEach(f => {
                f.DataKey = "TmkId";
                f.DataKeyValue = trademark.TmkId;
            });
            _cpiDbContext.GetRepository<DocFolder>().Add(docFolders);

            // add references
            var orderList = await _cpiDbContext.GetRepository<TmcRelatedTrademark>().QueryableList.Where(r => r.TmcId == clearance.TmcId).Select(d => d.OrderOfEntry).ToListAsync();
            int orderOfEntry = 0;

            if (orderList != null && orderList.Count > 0) orderOfEntry = orderList.Max() + 1;
            else orderOfEntry += 1;

            var tmcRelatedTrademark = new TmcRelatedTrademark
            {
                TmcId = clearance.TmcId,
                TmkId = trademark.TmkId,
                OrderOfEntry = orderOfEntry,
                CreatedBy = tmcClearance.CreatedBy,
                UpdatedBy = clearance.UpdatedBy,
                DateCreated = tmcClearance.DateCreated,
                LastUpdate = clearance.LastUpdate
            };
            _cpiDbContext.GetRepository<TmcRelatedTrademark>().Add(tmcRelatedTrademark);
        }
    }
}