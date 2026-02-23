using R10.Core.Interfaces;
using R10.Core.Entities;
using R10.Core.Interfaces.DMS;
using R10.Core.Entities.DMS;
using R10.Core.Identity;
using System.Linq.Expressions;
using System.Security.Claims;
using R10.Core.Entities.Patent;
using Microsoft.EntityFrameworkCore;
using R10.Core.Helpers;
using R10.Core.Exceptions;
using R10.Core.DTOs;
using R10.Core.Entities.PatClearance;
using DMSWorkflowTriggerType = R10.Core.Entities.DMS.DMSWorkflowTriggerType;
using R10.Core.Entities.Documents;
using Azure.Core;
using R10.Core.Interfaces.Shared;
using R10.Core.Entities.Shared;

namespace R10.Core.Services
{
    public class DisclosureService : EntityService<Disclosure>, IDisclosureService
    {
        private readonly ISystemSettings<DMSSetting> _settings;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ITradeSecretService _tradeSecretService;
        private readonly IApplicationDbContext _repository;

        public DisclosureService(
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user,
            ISystemSettings<DMSSetting> settings,
            ISystemSettings<PatSetting> patSettings,
            ITradeSecretService tradeSecretService,
            IApplicationDbContext repository
            ) : base(cpiDbContext, user)

        {
            _settings = settings;
            _patSettings = patSettings;
            _tradeSecretService = tradeSecretService;
            _repository = repository;
        }

        private DMSReviewerType ReviewerEntityType => _settings.GetSetting().Result.ReviewerEntityType;
        private bool IsDefaultReviewerOn => _settings.GetSetting().Result.IsDefaultReviewerOn;
        private bool IsPreviewOn => _settings.GetSetting().Result.IsPreviewOn;
        private bool IsESignatureOn => _settings.GetSetting().Result.IsESignatureOn;
        private string InitialReviewStatus {
            get
            {
                var defaultStatus = _settings.GetSetting().Result.InitialReviewStatus;
                if (string.IsNullOrEmpty(defaultStatus))
                    defaultStatus = "Initial Review";

                return defaultStatus;
            }            
        }
        private string FinalReviewStatus {
            get
            {
                var defaultStatus = _settings.GetSetting().Result.FinalReviewStatus;
                if (string.IsNullOrEmpty(defaultStatus))
                    defaultStatus = "Final Review";

                return defaultStatus;
            }            
        }
        private string UnderReviewStatus {
            get
            {
                var defaultStatus = _settings.GetSetting().Result.UnderReviewStatus;
                if (string.IsNullOrEmpty(defaultStatus))
                    defaultStatus = "Under Review";

                return defaultStatus;
            }            
        }
        private string SignatureStatus {
            get
            {
                var defaultStatus = _settings.GetSetting().Result.SignatureStatus;
                if (string.IsNullOrEmpty(defaultStatus))
                    defaultStatus = "Pending Signature";

                return defaultStatus;
            }            
        }
        public string SubmitStatus {
            get
            {
                var defaultStatus = _settings.GetSetting().Result.SubmitStatus;
                if (string.IsNullOrEmpty(defaultStatus))
                    defaultStatus = "Submitted";

                return defaultStatus;
            }            
        }

        public override IQueryable<Disclosure> QueryableList
        {
            get
            {
                IQueryable<Disclosure> disclosures = base.QueryableList;

                if (_user.HasEntityFilter())
                    disclosures = disclosures.Where(EntityFilter());

                if (!_user.CanAccessDMSTradeSecret())
                    disclosures = disclosures.Where(i => !(i.IsTradeSecret ?? false));

                return disclosures;
            }
        }

        public IQueryable<Disclosure> ForReviewList
        {
            get
            {
                var forReviewList = QueryableList.Where(d => d.DMSDisclosureStatus != null && d.DMSDisclosureStatus.CanReview);

                if (IsPreviewOn)
                {
                    forReviewList = forReviewList.Where(d => d.Previews != null && d.Previews.Any(p => p.DMSId == d.DMSId));
                }

                if (_user.GetEntityFilterType() == CPiEntityType.Inventor)
                    if (IsDefaultReviewerOn)
                    {
                        forReviewList = forReviewList.Where(d => UserEntityFilters.Any(f => f.UserId == UserId && (
                                                    DMSEntityReviewers.Any(r => r.EntityType == DMSReviewerType.None && r.EntityId == null && r.ReviewerType == CPiEntityType.Inventor && r.ReviewerId == f.EntityId)
                                                    )));
                    }
                    else
                    {
                        switch (ReviewerEntityType)
                        {
                            case DMSReviewerType.Area:
                                forReviewList = forReviewList.Where(d => UserEntityFilters.Any(f => f.UserId == UserId && (d.Area != null && d.Area.Reviewers != null &&
                                                    d.Area.Reviewers.Any(r => r.EntityType == DMSReviewerType.Area && r.ReviewerType == CPiEntityType.Inventor && r.ReviewerId == f.EntityId)
                                                    )));
                                break;

                            default:
                                forReviewList = forReviewList.Where(d => UserEntityFilters.Any(f => f.UserId == UserId && (d.Client != null && d.Client.Reviewers != null &&
                                                    d.Client.Reviewers.Any(r => r.EntityType == DMSReviewerType.Client && r.ReviewerType == CPiEntityType.Inventor && r.ReviewerId == f.EntityId)
                                                    )));
                                break;
                        }
                    }

                return forReviewList;
            }
        }

        public IQueryable<Disclosure> ForPreviewList
        {
            get
            {
                var forReviewList = QueryableList.Where(d => d.DMSDisclosureStatus != null && d.DMSDisclosureStatus.CanPreview && (d.Reviews == null || !d.Reviews.Any(r => r.DMSId == d.DMSId)));

                return forReviewList;
            }
        }

        public IQueryable<DMSDisclosureStatus> DisclosureStatuses
        {
            get
            {
                var disclosureStatuses = _cpiDbContext.GetReadOnlyRepositoryAsync<DMSDisclosureStatus>().QueryableList;

                if (!IsPreviewOn)
                {
                    disclosureStatuses = disclosureStatuses.Where(d => d.DisclosureStatus.ToLower() != InitialReviewStatus.ToLower()
                                                                    && d.DisclosureStatus.ToLower() != FinalReviewStatus.ToLower());
                }
                else
                {
                    disclosureStatuses = disclosureStatuses.Where(d => d.DisclosureStatus.ToLower() != UnderReviewStatus.ToLower());
                }

                if (!IsESignatureOn)
                {
                    disclosureStatuses = disclosureStatuses.Where(d => d.DisclosureStatus.ToLower() != SignatureStatus.ToLower());
                }

                return disclosureStatuses;
            }
        }

        public IQueryable<DMSDisclosureStatusHistory> DisclosureStatusHistories
        {
            get
            {
                var disclosureStatusHistories = _cpiDbContext.GetReadOnlyRepositoryAsync<DMSDisclosureStatusHistory>().QueryableList.Where(d => QueryableList.Any(q => q.DMSId == d.DMSId));

                return disclosureStatusHistories;
            }
        }

        public Expression<Func<Disclosure, bool>> EntityFilter()
        {
            switch (_user.GetEntityFilterType())
            {
                case CPiEntityType.Client:
                    return d => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == d.ClientID);


                case CPiEntityType.Owner:
                    return d => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == d.OwnerID);

                case CPiEntityType.Attorney:
                    return d => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == d.AttorneyID);

                //Inventor can also be a reviewer
                case CPiEntityType.Inventor:
                    if (IsDefaultReviewerOn)
                    {
                        return d => UserEntityFilters.Any(f => f.UserId == UserId &&
                                                    ((d.Inventors != null && d.Inventors.Any(i => i.InventorID == f.EntityId)) ||
                                                    DMSEntityReviewers.Any(r => r.EntityType == DMSReviewerType.None && r.EntityId == null && r.ReviewerType == CPiEntityType.Inventor && r.ReviewerId == f.EntityId)
                                                    ));
                    }
                    else
                    {
                        switch (ReviewerEntityType)
                        {
                            case DMSReviewerType.Area:
                                return d => UserEntityFilters.Any(f => f.UserId == UserId && (
                                                    (d.Inventors != null && d.Inventors.Any(i => i.InventorID == f.EntityId)) ||
                                                    (d.Area != null && d.Area.Reviewers != null && d.Area.Reviewers.Any(r => r.EntityType == DMSReviewerType.Area && r.ReviewerType == CPiEntityType.Inventor && r.ReviewerId == f.EntityId))
                                                    ));

                            default:
                                return d => UserEntityFilters.Any(f => f.UserId == UserId && (
                                                    (d.Inventors != null && d.Inventors.Any(i => i.InventorID == f.EntityId)) ||
                                                    (d.Client != null && d.Client.Reviewers != null && d.Client.Reviewers.Any(r => r.EntityType == DMSReviewerType.Client && r.ReviewerType == CPiEntityType.Inventor && r.ReviewerId == f.EntityId))
                                                    ));
                        }
                    }
                //Reviewer (ContactPerson)
                case CPiEntityType.ContactPerson:
                    if (IsDefaultReviewerOn)
                    {
                        return d => UserEntityFilters.Any(f => f.UserId == UserId
                                                            && DMSEntityReviewers.Any(r => r.EntityType == DMSReviewerType.None && r.EntityId == null && r.ReviewerType == CPiEntityType.ContactPerson && r.ReviewerId == f.EntityId));
                    }
                    else
                    {
                        switch (ReviewerEntityType)
                        {
                            case DMSReviewerType.Area:
                                return d => UserEntityFilters.Any(f => f.UserId == UserId && (d.Area != null && d.Area.Reviewers != null &&
                                                d.Area.Reviewers.Any(r => r.EntityType == DMSReviewerType.Area && r.ReviewerType == CPiEntityType.ContactPerson && r.ReviewerId == f.EntityId)
                                                ));

                            default:
                                return d => UserEntityFilters.Any(f => f.UserId == UserId && (d.Client != null && d.Client.Reviewers != null &&
                                                d.Client.Reviewers.Any(r => r.EntityType == DMSReviewerType.Client && r.ReviewerType == CPiEntityType.ContactPerson && r.ReviewerId == f.EntityId)
                                                ));
                        }
                    }
            }

            return a => true; ;
        }

        public IQueryable<DMSEntityReviewer> DMSEntityReviewers => _cpiDbContext.GetReadOnlyRepositoryAsync<DMSEntityReviewer>().QueryableList;
        public IQueryable<DisclosureCopySetting> DisclosureCopySettings => _cpiDbContext.GetRepository<DisclosureCopySetting>().QueryableList;
        public IQueryable<DMSQuestionGroup> QuestionGroups => _cpiDbContext.GetReadOnlyRepositoryAsync<DMSQuestionGroup>().QueryableList;
        public IQueryable<DMSQuestion> DMSQuestions => _cpiDbContext.GetReadOnlyRepositoryAsync<DMSQuestion>().QueryableList;
        public IQueryable<DisclosureCopyClearanceSetting> DisclosureCopyClearanceSettings => _cpiDbContext.GetRepository<DisclosureCopyClearanceSetting>().QueryableList;

        public override async Task<Disclosure> GetByIdAsync(int dmsId)
        {
            return await QueryableList.SingleOrDefaultAsync(d => d.DMSId == dmsId);
        }

        public override async Task Add(Disclosure disclosure)
        {
            //ONLY INVENTORS OR MODIFY USERS CAN ADD DISCLOSURES
            var isModifyUser = await ValidateRole(CPiPermissions.FullModify);
            Guard.Against.NoRecordPermission(await ValidateRole(CPiPermissions.Inventor) || isModifyUser);

            //REQUIRE CLIENT OR AREA OTHERWISE NO ONE WILL BE ABLE TO REVIEW - only check if not using default reviewers
            var settings = await _settings.GetSetting();
            if (!settings.IsDefaultReviewerOn)
            {
                if (settings.ReviewerEntityType == DMSReviewerType.Area)
                    Guard.Against.NullOrZero(disclosure.AreaID, "Area");
                else
                    Guard.Against.NullOrZero(disclosure.ClientID, settings.LabelClient ?? "Client");
            }

            //Popuplate DisclosureStatusDate if empty
            if (disclosure.DisclosureStatusDate == null) disclosure.DisclosureStatusDate = DateTime.Now.Date;

            //LET SQL GENERATE DEFAULT DISCLOSURE NUMBER
            if (disclosure != null && !string.IsNullOrEmpty(disclosure.DisclosureNumber) && disclosure.DisclosureNumber.Contains("{auto}")) disclosure.DisclosureNumber = null;

            _cpiDbContext.GetRepository<Disclosure>().Add(disclosure);

            var tsActivity = await SetTradeSecret(disclosure);
            await _cpiDbContext.SaveChangesAsync();

            if (!string.IsNullOrEmpty(tsActivity.ActivityCode))
                await CreateTradeSecretActivityLog(disclosure.DMSId, tsActivity.ActivityCode, tsActivity.AuditLogs);

            //IGNORE DEFAULT INVENTOR IF MODIFY USERS
            if (!isModifyUser) await AddDefaultInventor(disclosure);

            await AddDisclosureQuestions(disclosure);

            //LOG NEW STATUS
            LogStatusHistory(disclosure, DMSStatusHistoryChangeType.Status);

            await _cpiDbContext.SaveChangesAsync();
        }

        private async Task AddDefaultInventor(Disclosure disclosure)
        {
            //DO NOT ALLOW SAVE IF USER ACCOUNT HAS NO LINKED INVENTOR
            var inventorId = await GetUserInventorId();
            if (inventorId == 0)
                throw new ValueNotAllowedException("Your user account is not linked to an Inventor. Unable to add disclosure.");

            _cpiDbContext.GetRepository<DMSInventor>().Add(new DMSInventor
            {
                DMSId = disclosure.DMSId,
                InventorID = inventorId,
                IsDefaultInventor = true,
                IsNonEmployee = false,
                OrderOfEntry = 1,
                CreatedBy = disclosure.CreatedBy,
                UpdatedBy = disclosure.UpdatedBy,
                DateCreated = disclosure.DateCreated,
                LastUpdate = disclosure.LastUpdate
            });
        }

        private async Task AddDisclosureQuestions(Disclosure disclosure)
        {
            var newDMSQuestions = new List<DMSQuestion>();

            // 1. Get matching QuestionGroup based on ReviewerEntityFilter
            // Use QuestionGroup with matching ReviewerEntityFilter
            // If no match, use QuestionGroup with empty ReviewerEntityFilter
            var disclosureReviewerEntityId = ReviewerEntityType == DMSReviewerType.Client ? disclosure.ClientID : ReviewerEntityType == DMSReviewerType.Area ? disclosure.AreaID : 0;

            var matchedGroupIdList = new List<int>();

            var questionGroupList = (await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSQuestionGroup>().QueryableList.AsNoTracking()
                                    .Where(d => d.DMSQuestionGuides != null && d.DMSQuestionGuides.Any(g => g.AddToFuture))
                                    .Select(d => new { d.GroupId, d.GroupName, d.ReviewerEntityFilter }).ToListAsync())
                                    .Select(d => new
                                    {
                                        d.GroupId,
                                        d.GroupName,
                                        ReviewerEntityFilterIds = d.ReviewerEntityFilter != null && !string.IsNullOrEmpty(d.ReviewerEntityFilter) 
                                            ? d.ReviewerEntityFilter.Split('|').Where(r => int.TryParse(r, out _)).Select(int.Parse).ToHashSet() : null
                                    });

            var distinctGroupNames = questionGroupList.Select(d => d.GroupName).Distinct().ToList();

            foreach (var groupName in distinctGroupNames)
            {
                //Check questionGroup with ReviewerEntityFilter first
                var groupId = questionGroupList.Where(d => d.GroupName == groupName                                            
                                            && d.ReviewerEntityFilterIds != null && d.ReviewerEntityFilterIds.Contains(disclosureReviewerEntityId ?? 0)
                                        ).Select(d => d.GroupId).FirstOrDefault();

                //Use questionGroup with empty ReviewerEntityFilter, if there is no matching from above
                if (groupId <= 0)
                    groupId = questionGroupList.Where(d => d.GroupName == groupName && (d.ReviewerEntityFilterIds == null || d.ReviewerEntityFilterIds.Count <= 0)).Select(d => d.GroupId).FirstOrDefault();

                if (groupId > 0)
                    matchedGroupIdList.Add(groupId);
            }

            // 2. Get question Guides based on matching QuestionGroup
            var questionGuides = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSQuestionGuide>().QueryableList
                .Where(q => q.AddToFuture && matchedGroupIdList.Contains(q.GroupId))
                .Select(q => new { q.QuestionId, q.AnswerType, q.FollowUp })
                .ToListAsync();

            if (questionGuides != null && questionGuides.Count > 0)
            {
                newDMSQuestions.AddRange(questionGuides.Select(d => new DMSQuestion()
                {
                    DMSId = disclosure.DMSId,
                    QuestionId = d.QuestionId,
                    ChildId = null,
                    SubId = null,
                    CreatedBy = disclosure.CreatedBy,
                    UpdatedBy = disclosure.UpdatedBy,
                    DateCreated = disclosure.DateCreated,
                    LastUpdate = disclosure.LastUpdate
                }).ToList());

                // 3. Get child questions for boolean questions with follow-up
                var boolQuestionIdsWithFU = questionGuides.Where(d => !string.IsNullOrEmpty(d.AnswerType) && d.AnswerType.ToLower() == "bool" && d.FollowUp == true)
                    .Select(d => d.QuestionId).Distinct().ToList();

                if (boolQuestionIdsWithFU != null && boolQuestionIdsWithFU.Count > 0)
                {
                    newDMSQuestions.AddRange(await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSQuestionGuideChild>().QueryableList.AsNoTracking()
                        .Where(d => d.CAddToFuture && boolQuestionIdsWithFU.Contains(d.QuestionId))
                        .Select(d => new DMSQuestion()
                        {
                            DMSId = disclosure.DMSId,
                            QuestionId = d.QuestionId,
                            ChildId = d.ChildId,
                            SubId = null,
                            CreatedBy = disclosure.CreatedBy,
                            UpdatedBy = disclosure.UpdatedBy,
                            DateCreated = disclosure.DateCreated,
                            LastUpdate = disclosure.LastUpdate
                        }).ToListAsync());
                }

                // 4. Get sub questions for selection question with choices that have follow-up questions
                var selectionQuestionIdsWithFU = questionGuides.Where(d => !string.IsNullOrEmpty(d.AnswerType) && d.AnswerType.ToLower() == "selection")
                    .Select(d => d.QuestionId).Distinct().ToList();

                if (selectionQuestionIdsWithFU != null && selectionQuestionIdsWithFU.Count > 0)
                {
                    newDMSQuestions.AddRange(await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSQuestionGuideSub>().QueryableList.AsNoTracking()
                        .Where(d => d.SAddToFuture && d.DMSQuestionGuideChild != null 
                            && !string.IsNullOrEmpty(d.DMSQuestionGuideChild.CAnswerType) && d.DMSQuestionGuideChild.CAnswerType.ToLower() == "string" 
                            && d.DMSQuestionGuideChild.CFollowUp == true
                            && selectionQuestionIdsWithFU.Contains(d.DMSQuestionGuideChild.QuestionId)
                        ).Select(d => new DMSQuestion()
                        {
                            DMSId = disclosure.DMSId,
                            QuestionId = d.DMSQuestionGuideChild!.QuestionId,
                            ChildId = d.ChildId,
                            SubId = d.SubId,
                            CreatedBy = disclosure.CreatedBy,
                            UpdatedBy = disclosure.UpdatedBy,
                            DateCreated = disclosure.DateCreated,
                            LastUpdate = disclosure.LastUpdate
                        }).ToListAsync());
                }
            }

            if (newDMSQuestions != null && newDMSQuestions.Count > 0)
                _cpiDbContext.GetRepository<DMSQuestion>().Add(newDMSQuestions);
        }

        public override async Task Update(Disclosure disclosure)
        {
            //ONLY DEFAULT INVENTOR OR MODIFY USERS CAN UPDATE DISCLOSURE
            var cpiPermissions = CPiPermissions.Inventor;
            cpiPermissions.Add("modify");
            await ValidatePermission(disclosure.DMSId, cpiPermissions, true);
            Guard.Against.NoRecordPermission(await IsUserDefaultInventor(disclosure.DMSId) || await ValidateRole(CPiPermissions.FullModify));

            //REQUIRE CLIENT OR AREA OTHERWISE NO ONE WILL BE ABLE TO REVIEW - only check if not using default reviewers
            var settings = await _settings.GetSetting();
            if (!settings.IsDefaultReviewerOn)
            {
                if (settings.ReviewerEntityType == DMSReviewerType.Area)
                    Guard.Against.NullOrZero(disclosure.AreaID, "Area");
                else
                    Guard.Against.NullOrZero(disclosure.ClientID, settings.LabelClient ?? "");
            }

            var oldDisclosureStat = await QueryableList.Where(d => d.DMSId == disclosure.DMSId).Select(d => d.DisclosureStatus).FirstOrDefaultAsync();
            var oldDisclosureDate = await QueryableList.Where(d => d.DMSId == disclosure.DMSId).Select(d => d.DisclosureDate).FirstOrDefaultAsync();
            var oldDisclosureStatusDate = await QueryableList.Where(d => d.DMSId == disclosure.DMSId).Select(d => d.DisclosureStatusDate).FirstOrDefaultAsync();

            //Log to Status History if status, disclosure date, or status date changes
            if (oldDisclosureStat != disclosure.DisclosureStatus || oldDisclosureDate != disclosure.DisclosureDate || oldDisclosureStatusDate != disclosure.DisclosureStatusDate)
            {
                if (oldDisclosureStat != disclosure.DisclosureStatus && disclosure.DisclosureStatus == SubmitStatus)
                    disclosure.SubmittedDate = DateTime.Now;

                //Update status date if no change to status date
                if (oldDisclosureStat != disclosure.DisclosureStatus && oldDisclosureStatusDate == disclosure.DisclosureStatusDate)
                    disclosure.DisclosureStatusDate = DateTime.Now.Date;

                var historyChangeType = (oldDisclosureStat != disclosure.DisclosureStatus || oldDisclosureStatusDate != disclosure.DisclosureStatusDate) ? DMSStatusHistoryChangeType.Status :
                                        (oldDisclosureDate != disclosure.DisclosureDate) ? DMSStatusHistoryChangeType.DisclosureDate : DMSStatusHistoryChangeType.Status;
                LogStatusHistory(disclosure, historyChangeType);
            }
            _cpiDbContext.GetRepository<Disclosure>().Update(disclosure);

            var tsActivity = await SetTradeSecret(disclosure);
            if (!string.IsNullOrEmpty(tsActivity.ActivityCode))
                await CreateTradeSecretActivityLog(disclosure.DMSId, tsActivity.ActivityCode, tsActivity.AuditLogs);

            await _cpiDbContext.SaveChangesAsync();
        }

        //UPDATING REMARKS FROM REVIEW SCREEN NEEDS NEW tStamp AS RETURN VALUE
        public new async Task<byte[]> UpdateRemarks(Disclosure disclosure)
        {
            //REVIEWERS OR PREVIEWERS OR FULLMDOFIY CAN UPDATE REMARKS
            //Inventors can no longer edit remarks  when disclosure is for review or preview.
            Guard.Against.NoRecordPermission(await ValidateRole(CPiPermissions.Reviewer.Concat(CPiPermissions.FullModify).Concat(CPiPermissions.Previewer).ToList()));

            var updated = await ForReviewList.Where(d => d.DMSId == disclosure.DMSId).FirstOrDefaultAsync();
            if (updated == null)
            {
                updated = await ForPreviewList.Where(d => d.DMSId == disclosure.DMSId).FirstOrDefaultAsync();
            }
            Guard.Against.NoRecordPermission(updated != null);

            _cpiDbContext.GetRepository<Disclosure>().Attach(updated);
            updated.Remarks = disclosure.Remarks;
            updated.UpdatedBy = disclosure.UpdatedBy;
            updated.LastUpdate = disclosure.LastUpdate;
            updated.tStamp = disclosure.tStamp;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }

        //RETURNS IF EMAIL NEEDS TO BE SENT
        public async Task<byte[]> UpdateRecommendation(Disclosure disclosure)
        {
            //REVIEWERS AND FULLMDOFIY CAN UPDATE RECOMMENDATION
            Guard.Against.NoRecordPermission(await ValidateRole(CPiPermissions.Reviewer.Concat(CPiPermissions.FullModify).ToList()));

            //Validate disclosure record using ForReviewList
            var updated = await ForReviewList.Where(d => d.DMSId == disclosure.DMSId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(updated != null);

            //GET RECOMMENDATION based on Reviewer Entity Type
            var dmsReviewerEntityId = await QueryableList.AsNoTracking().Where(d => disclosure.DMSId == d.DMSId).Select(d => ReviewerEntityType == DMSReviewerType.Client ? d.ClientID : ReviewerEntityType == DMSReviewerType.Area ? d.AreaID : 0).FirstOrDefaultAsync();

            var recommendationList = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSRecommendation>().QueryableList.AsNoTracking().Where(r => r.Recommendation == disclosure.Recommendation).ToListAsync();

            var distinctRecommendations = recommendationList.Select(d => d.Recommendation).Distinct().ToList();

            var recommendation = new DMSRecommendation();

            foreach (var rec in distinctRecommendations)
            {
                recommendation = recommendationList.Where(d => d.Recommendation == rec && !string.IsNullOrEmpty(d.ReviewerEntityFilter)
                                                        && d.ReviewerEntityFilter.Split("|").Where(r => !string.IsNullOrEmpty(r) && int.TryParse(r, out _)).Any(r => int.Parse(r) == dmsReviewerEntityId)
                                                    ).FirstOrDefault();

                if (recommendation == null)
                    recommendation = recommendationList.Where(d => d.Recommendation == rec && string.IsNullOrEmpty(d.ReviewerEntityFilter)).FirstOrDefault();
            }

            Guard.Against.ValueNotAllowed(recommendation != null, "Recommendation");

            var dmsSettings = await _settings.GetSetting();
            string oldRecommendation = updated.Recommendation ?? "";
            string oldDisclosureStatus = updated.DisclosureStatus ?? "";

            _cpiDbContext.GetRepository<Disclosure>().Attach(updated);
            updated.Recommendation = disclosure.Recommendation;
            updated.RecommendationDate = DateTime.Now.Date;
            updated.Combined = disclosure.Combined;
            updated.UpdatedBy = disclosure.UpdatedBy;
            updated.LastUpdate = disclosure.LastUpdate;
            updated.tStamp = disclosure.tStamp;
            
            //Only reset status or gen Invention if Recommendation changes, avoid issue with "Combine" recommendation + changing Combined value
            if (oldRecommendation != disclosure.Recommendation)
            {                
                if (recommendation.IsResetStatus)
                {
                    // reset status
                    updated.DisclosureStatus = await GetDraftStatus();
                    updated.SubmittedDate = null;
                    updated.SignatureFileId = 0;

                    //Clear all inventors' initial and date, and reviewed (for e-signature)
                    await _cpiDbContext.GetRepository<DMSInventor>().QueryableList.Where(d => d.DMSId == updated.DMSId)
                        .ExecuteUpdateAsync(d => d.SetProperty(p => p.IsReviewed, p => false).SetProperty(p => p.Initial, p => null).SetProperty(p => p.InitialDate, p => null));
                }

                if (!string.IsNullOrEmpty(recommendation.DisclosureStatus))
                    updated.DisclosureStatus = recommendation.DisclosureStatus;

                if (oldDisclosureStatus != updated.DisclosureStatus)
                {
                    updated.DisclosureStatusDate = DateTime.Now.Date;
                    LogStatusHistory(updated, DMSStatusHistoryChangeType.Status);
                }

                bool isTradeSecretActive = dmsSettings.IsTradeSecretOn && !string.IsNullOrEmpty(updated.Recommendation) && recommendation.IsTradeSecret;
                if (isTradeSecretActive)
                    updated.IsTradeSecret = isTradeSecretActive;

                // add invention to Patent system
                if (recommendation.IsGenInvention)
                    await AddInventionRecord(updated);

                //Trade Secret                
                if (isTradeSecretActive)
                {
                    var tsActivity = await SetTradeSecret(updated);
                    if (!string.IsNullOrEmpty(tsActivity.ActivityCode))
                        await CreateTradeSecretActivityLog(disclosure.DMSId, tsActivity.ActivityCode, tsActivity.AuditLogs);
                }
            }

            //LOG NEW RECOMMENDATION
            LogRecommendationHistory(updated);

            await _cpiDbContext.SaveChangesAsync();
            _cpiDbContext.Detach(updated);

            //Clear combined disclosure recommendation is not "Combine"
            if (!string.IsNullOrEmpty(updated.Recommendation) && updated.Recommendation.ToLower() != "combine")
                await _cpiDbContext.GetRepository<DMSCombined>().QueryableList.Where(d => d.DMSId == updated.DMSId).ExecuteDeleteAsync();

            return updated.tStamp!;
        }

        private void LogRecommendationHistory(Disclosure disclosure)
        {
            var recommendationHistory = new DMSRecommendationHistory()
            {
                DMSId = disclosure.DMSId,
                Recommendation = disclosure.Recommendation,
                Combined = disclosure.Combined,
                CreatedBy = disclosure.UpdatedBy,
                DateChanged = disclosure.LastUpdate,
            };
            _cpiDbContext.GetRepository<DMSRecommendationHistory>().Add(recommendationHistory);
        }

        private void LogStatusHistory(Disclosure disclosure, DMSStatusHistoryChangeType? historyChangeType = DMSStatusHistoryChangeType.None)
        {
            var statusHistory = new DMSDisclosureStatusHistory()
            {
                DMSId = disclosure.DMSId,
                DisclosureStatus = disclosure.DisclosureStatus,
                DisclosureStatusDate = disclosure.DisclosureStatusDate,
                DisclosureDate = disclosure.DisclosureDate,
                Recommendation = disclosure.Recommendation,
                CreatedBy = disclosure.UpdatedBy,
                DateChanged = disclosure.LastUpdate,
                ChangeType = historyChangeType
            };
            _cpiDbContext.GetRepository<DMSDisclosureStatusHistory>().Add(statusHistory);
        }

        private async Task AddInventionRecord(Disclosure disclosure)
        {
            var patSettings = await _patSettings.GetSetting();

            // check first if previously added
            var addInventionRecord = !(await _cpiDbContext.GetRepository<Invention>().QueryableList.AnyAsync(i => i.DMSId == disclosure.DMSId));
            if (!addInventionRecord) return;

            var invention = new Invention()
            {
                DMSId = disclosure.DMSId,
                CaseNumber = disclosure.DisclosureNumber,
                InvTitle = disclosure.DisclosureTitle,
                DisclosureDate = disclosure.DisclosureDate,
                ClientID = disclosure.ClientID,
                Attorney1ID = disclosure.AttorneyID,
                Remarks = disclosure.Remarks,
                CreatedBy = disclosure.UpdatedBy,
                UpdatedBy = disclosure.UpdatedBy,
                DateCreated = disclosure.DateCreated,
                LastUpdate = disclosure.LastUpdate
            };

            if (patSettings.IsTradeSecretOn)
                invention.IsTradeSecret = disclosure.IsTradeSecret ?? false;

            _cpiDbContext.GetRepository<Invention>().Add(invention);

            if (invention.IsTradeSecret ?? false)
            {
                invention.TradeSecret = invention.CreateTradeSecret(new InventionTradeSecret());
                invention.TradeSecretDate = DateTime.Now;
            }

            await _cpiDbContext.SaveChangesAsync();

            if (invention.IsTradeSecret ?? false)
            {
                var tsRequest = await _tradeSecretService.GetUserRequest(_tradeSecretService.CreateLocator(TradeSecretScreen.Invention, invention.InvId));
                var tsActivity = _tradeSecretService.CreateActivity(TradeSecretScreen.Invention, TradeSecretScreen.Invention, invention.InvId, TradeSecretActivityCode.Create, tsRequest?.RequestId ?? 0,
                    new Dictionary<string, string?[]>()
                    {
                        { "IsTradeSecret", [ "", invention?.IsTradeSecret.ToString() ] },
                        { "InvTitle", [ "", invention?.TradeSecret?.InvTitle ] }
                    });
            }

            // add owner
            if (disclosure.OwnerID > 0)
            {
                var owner = new PatOwnerInv
                {
                    InvId = invention.InvId,
                    OwnerID = (int)disclosure.OwnerID,
                    CreatedBy = disclosure.CreatedBy,
                    UpdatedBy = disclosure.UpdatedBy,
                    DateCreated = disclosure.DateCreated,
                    LastUpdate = disclosure.LastUpdate
                };
                _cpiDbContext.GetRepository<PatOwnerInv>().Add(owner);
            }

            // add inventors
            var patInventors = await _cpiDbContext.GetRepository<DMSInventor>().QueryableList.Where(i => i.DMSId == disclosure.DMSId)
                                        .Select(i => new PatInventorInv()
                                        {
                                            InvId = invention.InvId,
                                            InventorID = i.InventorID,
                                            Percentage = i.Percentage,
                                            OrderOfEntry = i.OrderOfEntry,
                                            Remarks = i.Remarks,
                                            CreatedBy = disclosure.CreatedBy,
                                            UpdatedBy = disclosure.UpdatedBy,
                                            DateCreated = disclosure.DateCreated,
                                            LastUpdate = disclosure.LastUpdate

                                        }).ToListAsync();

            _cpiDbContext.GetRepository<PatInventorInv>().Add(patInventors);

            // add keywords
            var patKeywords = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSKeyword>().QueryableList.Where(k => k.DMSId == disclosure.DMSId)
                                        .Select(k => new PatKeyword
                                        {
                                            InvId = invention.InvId,
                                            Keyword = k.Keyword,
                                            CreatedBy = k.CreatedBy,
                                            UpdatedBy = k.UpdatedBy,
                                            DateCreated = k.DateCreated,
                                            LastUpdate = k.LastUpdate
                                        }).ToListAsync();

            _cpiDbContext.GetRepository<PatKeyword>().Add(patKeywords);

            //add documents
            var docFolders = await _cpiDbContext.GetReadOnlyRepositoryAsync<DocFolder>().QueryableList.Where(f => f.SystemType == "D" && f.DataKey == "DMSId" && f.DataKeyValue == disclosure.DMSId).Include(f => f.DocDocuments).ToListAsync();
            docFolders.ForEach(f =>
            {
                f.FolderId = 0;
                f.DocDocuments.ForEach(d =>
                {
                    d.DocId = 0;
                });
                f.SystemType = "P";
                f.ScreenCode = "Inv";
                f.DataKey = "InvId";
                f.DataKeyValue = invention.InvId;
            });
            _cpiDbContext.GetRepository<DocFolder>().Add(docFolders);

            // add references
            int orderOfEntry = (await _cpiDbContext.GetRepository<InventionRelatedDisclosure>().QueryableList.Where(r => r.DMSId == disclosure.DMSId)
                                        .MaxAsync(r => (int?)r.OrderOfEntry) ?? 0) + 1;

            var dmsRelatedInvention = new InventionRelatedDisclosure
            {
                DMSId = disclosure.DMSId,
                InvId = invention.InvId,
                OrderOfEntry = orderOfEntry,
                CreatedBy = disclosure.CreatedBy,
                UpdatedBy = disclosure.UpdatedBy,
                DateCreated = disclosure.DateCreated,
                LastUpdate = disclosure.LastUpdate
            };
            _cpiDbContext.GetRepository<InventionRelatedDisclosure>().Add(dmsRelatedInvention);
        }

        public override async Task Delete(Disclosure disclosure)
        {
            //ONLY DEFAULT INVENTOR OR MODIFY CAN UPDATE DISCLOSURE
            var cpiPermissions = CPiPermissions.Inventor;
            cpiPermissions.Add("modify");
            await ValidatePermission(disclosure.DMSId, cpiPermissions, true);
            var isModifyUser = await ValidateRole(CPiPermissions.FullModify);
            Guard.Against.NoRecordPermission(await IsUserDefaultInventor(disclosure.DMSId) || isModifyUser);

            var canDeleteTradeSecret = await CanDeleteTradeSecret(disclosure.DMSId);
            Guard.Against.UnAuthorizedAccess(canDeleteTradeSecret.Allowed);

            //Check if record is being used in Related Disclosures tab on Invention screen            
            if (await _cpiDbContext.GetRepository<InventionRelatedDisclosure>().QueryableList.AnyAsync(d => d.DMSId == disclosure.DMSId))
            {
                var canDeleteInventionRelatedDisclosure = true;
                //If so, only allow to delete if user has patent delete permission
                if (_user.IsInSystem(SystemType.Patent))
                {
                    var hasRespOfficeFilter = _user.HasRespOfficeFilter(SystemType.Patent);
                    if (!hasRespOfficeFilter && !(await ValidatePermission(SystemType.Patent, CPiPermissions.CanDelete, "")))
                    {
                        canDeleteInventionRelatedDisclosure = false;
                    }
                    else
                    {
                        var relatedInventionRespOfc = await _cpiDbContext.GetRepository<InventionRelatedDisclosure>().QueryableList.Where(c => c.DMSId == disclosure.DMSId)
                                                    .Select(d => d.Invention.RespOffice).ToListAsync();
                        foreach (var respOfc in relatedInventionRespOfc)
                        {
                            if (!await ValidatePermission(SystemType.Patent, CPiPermissions.CanDelete, respOfc))
                            {
                                canDeleteInventionRelatedDisclosure = false;
                            }
                        }
                    }
                }
                else
                {
                    //User doesn't have any permission for Patent
                    canDeleteInventionRelatedDisclosure = false;
                }
                if (!canDeleteInventionRelatedDisclosure)
                    throw new NoRecordPermissionException("Record cannot be deleted. It is already in use under Related Disclosures tab on Invention");
            }

            _cpiDbContext.GetRepository<Disclosure>().Delete(disclosure);

            if (canDeleteTradeSecret.DMSId > 0)
                await CreateTradeSecretActivityLog(canDeleteTradeSecret.DMSId, TradeSecretActivityCode.Delete, CreateAuditLogs(await GetByIdAsync(disclosure.DMSId), null));

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task UpdateStatus(Disclosure updated, bool resetSignatureFile = false)
        {
            //ONLY FULL MODIFY USERS CAN UPDATE STATUS
            //Also allow default inventor - this is for e-signature
            var cpiPermissions = CPiPermissions.Inventor;
            cpiPermissions.Add("modify");
            await ValidatePermission(updated.DMSId, cpiPermissions, false);
            Guard.Against.NoRecordPermission(await IsUserDefaultInventor(updated.DMSId) || await ValidateRole(CPiPermissions.FullModify));

            if (updated.DisclosureStatus == await GetDraftStatus()) resetSignatureFile = true;

            //Use ExecuteUpdateAsync to avoid tracking issue when using Blob with DocuSign;
            await _cpiDbContext.GetRepository<Disclosure>().QueryableList
                                    .Where(d => d.DMSId == updated.DMSId)
                                    .ExecuteUpdateAsync(d => d.SetProperty(p => p.DisclosureStatus, p => updated.DisclosureStatus)
                                                        .SetProperty(p => p.DisclosureStatusDate, p => DateTime.Now.Date)
                                                        .SetProperty(p => p.SubmittedDate, p => updated.DisclosureStatus!.ToLower() == SubmitStatus.ToLower() ? DateTime.Now : p.SubmittedDate)
                                                        .SetProperty(p => p.UpdatedBy, p => updated.UpdatedBy)
                                                        .SetProperty(p => p.LastUpdate, p => updated.LastUpdate)
                                                        .SetProperty(p => p.SignatureFileId, p => resetSignatureFile ? 0 : p.SignatureFileId));

            if (resetSignatureFile)
            {
                await _cpiDbContext.GetRepository<DMSInventor>().QueryableList.Where(d => d.DMSId == updated.DMSId).ExecuteUpdateAsync(d => d.SetProperty(p => p.IsReviewed, p => false));
            }

            //LOG NEW STATUS
            var disclosure = await _cpiDbContext.GetReadOnlyRepositoryAsync<Disclosure>().QueryableList.AsNoTracking().Where(d => d.DMSId == updated.DMSId).FirstOrDefaultAsync();
            if (disclosure != null)
                LogStatusHistory(disclosure, DMSStatusHistoryChangeType.Status);

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task Submit(Disclosure submitted)
        {
            var settings = await _settings.GetSetting();
            //ONLY DEFAULT INVENTOR OR MODIFY USERS CAN SUBMIT
            var cpiPermissions = CPiPermissions.Inventor;
            cpiPermissions.Add("modify");

            var checkStatus = true;
            //Ignore check LockRecord-status if E-Signature is on and current status is Pending Signature status
            if (settings.IsESignatureOn)
            {
                var eSignatureStatus = await GetOrCreateCPIDisclosureStatus(SignatureStatus, canSubmit: true);
                var currentStatus = await QueryableList.Where(d => d.DMSId == submitted.DMSId).Select(d => d.DisclosureStatus).FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(currentStatus) && currentStatus.ToLower() == eSignatureStatus.ToLower())
                    checkStatus = false;
            }

            await ValidatePermission(submitted.DMSId, cpiPermissions, checkStatus);
            Guard.Against.NoRecordPermission(await CanSubmitDisclosure(submitted.DMSId));

            var disclosure = await QueryableList.Where(d => d.DMSId == submitted.DMSId).FirstOrDefaultAsync();

            if (disclosure == null) return;

            //REQUIRE CLIENT (OR OWNER) OTHERWISE NO ONE WILL BE ABLE TO REVIEW - only check if not using default reviewers            
            if (!settings.IsDefaultReviewerOn)
            {
                if (settings.ReviewerEntityType == DMSReviewerType.Area)
                    Guard.Against.NullOrZero(disclosure.AreaID, "Area");
                else
                    Guard.Against.NullOrZero(disclosure.ClientID, settings.LabelClient ?? "");
            }

            _cpiDbContext.GetRepository<Disclosure>().Attach(disclosure);

            var submittedStatus = await GetOrCreateCPIDisclosureStatus(SubmitStatus, canSubmit: true);
            //Log submit status
            if (!string.IsNullOrEmpty(submittedStatus))
            {                
                disclosure.DisclosureStatus = submittedStatus;
                disclosure.DisclosureStatusDate = DateTime.Now.Date;
                disclosure.SubmittedDate = submitted.LastUpdate;
                disclosure.UpdatedBy = submitted.UpdatedBy;
                disclosure.LastUpdate = submitted.LastUpdate;
                disclosure.tStamp = submitted.tStamp;

                LogStatusHistory(disclosure, DMSStatusHistoryChangeType.Status);
            }                

            //Change Submitted to Initial Review if Preview is on
            string reviewStatus;
            if (settings.IsPreviewOn)
            {
                //Go to Final Review status if record has been submitted -> previewed -> reviewed before
                if (await QueryableList.AsNoTracking().AnyAsync(d => d.Previews != null && d.Previews.Any(p => p.DMSId == submitted.DMSId) && d.Reviews != null && d.Reviews.Any(r => r.DMSId == submitted.DMSId)))
                {
                    reviewStatus = await GetOrCreateCPIDisclosureStatus(FinalReviewStatus, canReview: true);
                }
                else
                {
                    reviewStatus = await GetOrCreateCPIDisclosureStatus(InitialReviewStatus, canPreview: true);
                }
            }
            else
            {
                //Else change Submitted to Under Review if Preview is off
                reviewStatus = await GetOrCreateCPIDisclosureStatus(UnderReviewStatus, canReview: true);
            }

            if (!string.IsNullOrEmpty(submittedStatus) && !string.IsNullOrEmpty(reviewStatus) && submittedStatus != reviewStatus)
            {                
                disclosure.DisclosureStatus = reviewStatus;

                LogStatusHistory(new Disclosure()
                {
                    DMSId = disclosure.DMSId,
                    DisclosureStatus = reviewStatus,
                    DisclosureStatusDate = DateTime.Now.Date,
                    DisclosureDate = submitted.DisclosureDate,
                    Recommendation = disclosure.Recommendation,
                    CreatedBy = submitted.UpdatedBy,
                    UpdatedBy = submitted.UpdatedBy,
                    DateCreated = submitted.LastUpdate.Value.AddMilliseconds(2),
                    LastUpdate = submitted.LastUpdate.Value.AddMilliseconds(2)
                }, DMSStatusHistoryChangeType.Status);

            }

            await _cpiDbContext.SaveChangesAsync();
            _cpiDbContext.Detach(disclosure);
        }

        public async Task<bool> CanSubmitDisclosure(int dmsId)
        {
            var settings = await _settings.GetSetting();
            var isRoleInventor = await ValidateRole(CPiPermissions.Inventor);
            bool statusCanSubmitDisclosure = await QueryableList.Where(d => d.DMSId == dmsId).Select(d => d.DMSDisclosureStatus != null ? d.DMSDisclosureStatus.CanSubmit : false).FirstOrDefaultAsync();
            bool isUserDefaultInventor = await IsUserDefaultInventor(dmsId);

            //ONLY DEFAULT INVENTOR OR MODIFY USERS CAN SUBMIT DISCLOSURE
            if (statusCanSubmitDisclosure && (isUserDefaultInventor && isRoleInventor || await ValidateRole(CPiPermissions.FullModify)))
            {
                //IF INITIAL AND DATE IS ON
                if (settings.IsInventorInitialOn && !settings.IsESignatureOn)
                {
                    bool hasMissingInitials = true;
                    if (settings.IsDefaultInventorInitialOn)
                    {
                        hasMissingInitials = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSInventor>().QueryableList.AsNoTracking()
                                            .AnyAsync(i => i.DMSId == dmsId
                                                && (string.IsNullOrEmpty(i.Initial) || i.InitialDate == null) && !i.IsNonEmployee
                                                && i.IsDefaultInventor
                                            );
                    }
                    else
                    {
                        hasMissingInitials = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSInventor>().QueryableList.AsNoTracking()
                                            .AnyAsync(i => i.DMSId == dmsId
                                                && (string.IsNullOrEmpty(i.Initial) || i.InitialDate == null) && !i.IsNonEmployee
                                            );
                    }

                    return !hasMissingInitials;
                }

                //ELSE IF E-SIGNATURE IS ON
                else if (settings.IsESignatureOn)
                {
                    var signatureFileId = await _cpiDbContext.GetReadOnlyRepositoryAsync<Disclosure>().QueryableList.AsNoTracking().Where(d => d.DMSId == dmsId).Select(d => d.SignatureFileId).FirstOrDefaultAsync();
                    if (settings.IsSharePointIntegrationOn)
                    {
                        return await _cpiDbContext.GetReadOnlyRepositoryAsync<SharePointFileSignature>().QueryableList.AsNoTracking()
                                                                    .AnyAsync(d => d.SignatureCompleted == true && (d.SignatureFileId == (signatureFileId ?? 0)) && !string.IsNullOrEmpty(d.SignedDocDriveItemId));
                    }
                    else
                    {
                        return await _cpiDbContext.GetReadOnlyRepositoryAsync<DocFileSignature>().QueryableList.AsNoTracking()
                                                                    .AnyAsync(d => d.SignatureCompleted == true && (d.SignatureFileId == (signatureFileId ?? 0)) && d.SignedDocFileId > 0);
                    }
                }

                return true;
            }
            return false;
        }

        public async Task<string> GetDraftStatus()
        {
            var status = await DisclosureStatuses.AsNoTracking().Where(s => !s.LockRecord && s.IsDefault).Select(s => s.DisclosureStatus).FirstOrDefaultAsync();
            return status;
        }

        public async Task ValidatePermission(int dmsId, List<string> roles, bool checkStatus)
        {
            if (_user.HasEntityFilter())
            {
                Guard.Against.NoRecordPermission(await QueryableList.AnyAsync(d => d.DMSId == dmsId && (!checkStatus || !d.DMSDisclosureStatus.LockRecord)));
            }

            Guard.Against.NoRecordPermission(await ValidateRole(roles));
        }

        private async Task<bool> ValidateRole(List<string> roles)
        {
            return await ValidatePermission(SystemType.DMS, roles, null);
        }

        public async Task<int> GetUserReviewerId(CPiEntityType reviewerType)
        {
            if (await ValidateRole(CPiPermissions.Reviewer))
                return await UserEntityFilters.Where(e => e.UserId == UserId && e.CPiUser.EntityFilterType == reviewerType).Select(e => e.EntityId).FirstOrDefaultAsync();

            return 0;
        }

        public async Task<int> GetUserPreviewerId(CPiEntityType previewerType)
        {
            if (await ValidateRole(CPiPermissions.Previewer))
                return await UserEntityFilters.Where(e => e.UserId == UserId && e.CPiUser.EntityFilterType == previewerType).Select(e => e.EntityId).FirstOrDefaultAsync();

            return 0;
        }

        public async Task<int> GetUserInventorId()
        {
            if (await ValidateRole(CPiPermissions.Inventor))
                return await UserEntityFilters.Where(e => e.UserId == UserId && e.CPiUser.EntityFilterType == CPiEntityType.Inventor).Select(e => e.EntityId).FirstOrDefaultAsync();

            return 0;
        }

        public async Task<bool> IsUserDefaultInventor(int dmsId)
        {
            //USER ACCOUNT IS LINKED TO INVENTOR USING ENTITY FILTER
            var inventorId = await GetUserInventorId();
            if (inventorId > 0)
                return await _cpiDbContext.GetRepository<DMSInventor>().QueryableList.AnyAsync(d => d.DMSId == dmsId && d.InventorID == inventorId && d.IsDefaultInventor);

            return false;
        }

        public async Task<bool> IsUserInventorInDMS(int dmsId)
        {
            //USER ACCOUNT IS LINKED TO INVENTOR USING ENTITY FILTER
            var inventorId = await GetUserInventorId();
            if (inventorId > 0)
                return await _cpiDbContext.GetRepository<DMSInventor>().QueryableList.AnyAsync(d => d.DMSId == dmsId && d.InventorID == inventorId);

            return false;
        }

        public async Task<bool> IsUserReviewerInDMS(int dmsId)
        {
            var disclosure = await GetByIdAsync(dmsId);
            var reviewerType = _user.GetEntityFilterType();
            var reviewerId = await GetUserReviewerId(reviewerType);

            if (reviewerId > 0)
            {
                var settings = await _settings.GetSetting();
                if (settings.IsDefaultReviewerOn)
                {
                    return await _cpiDbContext.GetRepository<DMSEntityReviewer>().QueryableList
                                                .AnyAsync(r => r.EntityType == DMSReviewerType.None && r.EntityId == null
                                                                && r.ReviewerType == reviewerType && r.ReviewerId == reviewerId);
                }
                else
                {
                    return await _cpiDbContext.GetRepository<DMSEntityReviewer>().QueryableList
                                                .AnyAsync(r => ((r.EntityType == DMSReviewerType.Client && r.EntityId == disclosure.ClientID) ||
                                                                    (r.EntityType == DMSReviewerType.Area && r.EntityId == disclosure.AreaID))
                                                                && r.ReviewerType == reviewerType && r.ReviewerId == reviewerId);
                }
            }

            return false;
        }

        public IQueryable<DMSPreview> GetPreviews(int dmsId)
        {
            if (_user.HasEntityFilter())
                return _cpiDbContext.GetReadOnlyRepositoryAsync<DMSPreview>().QueryableList.Where(e => e.DMSId == dmsId && QueryableList.Any(d => d.DMSId == e.DMSId));
            else
                return _cpiDbContext.GetReadOnlyRepositoryAsync<DMSPreview>().QueryableList.Where(e => e.DMSId == dmsId);
        }
        public IQueryable<DMSReview> GetReviews(int dmsId)
        {
            if (_user.HasEntityFilter())
                return _cpiDbContext.GetReadOnlyRepositoryAsync<DMSReview>().QueryableList.Where(e => e.DMSId == dmsId && QueryableList.Any(d => d.DMSId == e.DMSId));
            else
                return _cpiDbContext.GetReadOnlyRepositoryAsync<DMSReview>().QueryableList.Where(e => e.DMSId == dmsId);
        }

        public IQueryable<DMSValuation> GetValuations(int dmsId)
        {
            if (_user.HasEntityFilter())
                return _cpiDbContext.GetReadOnlyRepositoryAsync<DMSValuation>().QueryableList.Where(e => e.DMSId == dmsId && QueryableList.Any(d => d.DMSId == e.DMSId));
            else
                return _cpiDbContext.GetReadOnlyRepositoryAsync<DMSValuation>().QueryableList.Where(e => e.DMSId == dmsId);
        }

        public IQueryable<DMSDisclosureStatusHistory> GetStatusHistory(int dmsId)
        {
            if (_user.HasEntityFilter())
                return _cpiDbContext.GetReadOnlyRepositoryAsync<DMSDisclosureStatusHistory>().QueryableList.AsNoTracking().Where(e => e.DMSId == dmsId && QueryableList.Any(d => d.DMSId == e.DMSId));
            else
                return _cpiDbContext.GetReadOnlyRepositoryAsync<DMSDisclosureStatusHistory>().QueryableList.AsNoTracking().Where(e => e.DMSId == dmsId);
        }

        public IQueryable<DMSRecommendationHistory> GetRecommendationHistory(int dmsId)
        {
            if (_user.HasEntityFilter())
                return _cpiDbContext.GetReadOnlyRepositoryAsync<DMSRecommendationHistory>().QueryableList.Where(e => e.DMSId == dmsId && QueryableList.Any(d => d.DMSId == e.DMSId));
            else
                return _cpiDbContext.GetReadOnlyRepositoryAsync<DMSRecommendationHistory>().QueryableList.Where(e => e.DMSId == dmsId);
        }

        
        public async Task<List<DMSWorkflowAction>> CheckWorkflowAction(DMSWorkflowTriggerType triggerType)
        {
            var actions = await _repository.DMSWorkflowActions.Where(w => w.DMSWorkflow != null && w.DMSWorkflow.TriggerTypeId == (int)triggerType && w.DMSWorkflow.ActiveSwitch)
                .Include(w => w.DMSWorkflow).OrderBy(w => w.OrderOfEntry).ToListAsync();
            return actions;
        }

        public async Task GenerateWorkflowAction(int dmsId, int actionTypeId)
        {
            var disclosure = await GetByIdAsync(dmsId);
            if (disclosure == null) return;

            var actionType = await _cpiDbContext.GetRepository<DMSActionType>().QueryableList.Where(at => at.ActionTypeID == actionTypeId).FirstOrDefaultAsync();
            if (actionType == null) return;

            var dupActionDue = await _cpiDbContext.GetRepository<DMSActionDue>().QueryableList.Where(a => a.DMSId == dmsId && a.BaseDate.Date == DateTime.Now.Date && a.ActionType == actionType.ActionType).FirstOrDefaultAsync();

            if (dupActionDue == null)
            {
                //Generate action
                DMSActionDue actionDue = new DMSActionDue() { DMSId = disclosure.DMSId, DisclosureNumber = disclosure.DisclosureNumber, ActionType = actionType.ActionType, BaseDate = DateTime.Now.Date, ResponsibleID = null, CreatedBy = "Auto-Gen", UpdatedBy = _user.GetUserName(), DateCreated = DateTime.Now, LastUpdate = DateTime.Now };

                var dueDates = new List<DMSDueDate>();
                var actionParams = new List<DMSActionParameter>();
                //actionDue is based on an ActionType
                //get ActionParameters
                if (actionType != null)
                    actionParams = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSActionParameter>().QueryableList
                                        .Where(ap => ap.ActionTypeID == actionType.ActionTypeID)
                                        .ToListAsync();

                if (actionParams.Any())
                    //generate DueDates based on ActionParameters
                    dueDates = actionParams.Select(ap => new DMSDueDate()
                    {
                        ActId = actionDue.ActId,
                        ActionDue = ap.ActionDue,
                        DueDate = actionDue.BaseDate.AddDays((double)ap.Dy).AddMonths(ap.Mo).AddYears(ap.Yr),
                        DateTaken = actionDue.ResponseDate,
                        Indicator = ap.Indicator,
                        CreatedBy = actionDue.UpdatedBy,
                        DateCreated = actionDue.LastUpdate,
                        UpdatedBy = actionDue.UpdatedBy,
                        LastUpdate = actionDue.LastUpdate
                    }).ToList();
                else
                    //generate DueDate based on actionDue
                    dueDates.Add(new DMSDueDate()
                    {
                        ActId = actionDue.ActId,
                        ActionDue = actionDue.ActionType,
                        DueDate = actionDue.BaseDate,
                        DateTaken = actionDue.ResponseDate,
                        Indicator = "Due Date",
                        CreatedBy = actionDue.UpdatedBy,
                        DateCreated = actionDue.LastUpdate,
                        UpdatedBy = actionDue.UpdatedBy,
                        LastUpdate = actionDue.LastUpdate
                    });
                actionDue.DueDates = dueDates;

                _cpiDbContext.GetRepository<DMSActionDue>().Add(actionDue);
                await _cpiDbContext.SaveChangesAsync();
                _cpiDbContext.Detach(actionDue);
            }
        }
        public async Task<List<DMSActionDue>> CloseWorkflowAction(int dmsId, int actionTypeId)
        {
            var disclosure = await GetByIdAsync(dmsId);
            var actionDues = new List<DMSActionDue>();

            if (actionTypeId != 0)
            {
                var actionType = await _repository.DMSActionTypes.Where(at => at.ActionTypeID == actionTypeId).AsNoTracking().FirstOrDefaultAsync();
                if (actionType != null) {                    
                    actionDues = await _repository.DMSActionDues.Where(a => a.DMSId == dmsId && a.ActionType == actionType.ActionType && (a.ResponseDate == null || a.DueDates.Any(dd => dd.DateTaken == null))).Include(a => a.DueDates).AsNoTracking().ToListAsync();
                }
            }
            //all outstanding actions
            else if (actionTypeId == 0)
            {
                actionDues = await _repository.DMSActionDues.Where(a => a.DMSId == dmsId && (a.ResponseDate == null || a.DueDates.Any(dd => dd.DateTaken == null))).Include(a => a.DueDates).AsNoTracking().ToListAsync();
            }
            if (actionDues.Any())
            {
                foreach (var actionDue in actionDues)
                {
                    //for all outstanding actions, we want to close everything and avoid followup
                    if (actionTypeId != 0)
                    {
                        if (actionDue.ResponseDate == null)
                        {
                            actionDue.ResponseDate = DateTime.Now.Date;
                            actionDue.UpdatedBy = _user.GetUserName();
                            actionDue.LastUpdate = DateTime.Now;
                        }
                    }
                    actionDue.CloseDueDates = true;

                }
            }
            return actionDues;

        }        

        public void DetachAllEntities()
        {
            _repository.DetachAllEntities();
        }

        public async Task CheckRequiredOnSubmission(int dmsId)
        {
            //Questionnaire - START
            var requiredQuestionGroups = new List<string>();

            var dmsQuestions = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSQuestion>().QueryableList.AsNoTracking()
                .Where(d => d.DMSId == dmsId && d.DMSQuestionGuide != null)
                .Include(d => d.DMSQuestionGuide).ThenInclude(d => d!.DMSQuestionGroup)
                .Include(d => d.DMSQuestionGuideChild).Include(d => d.DMSQuestionGuideSub)
                .ToListAsync();

            // 1. Check if there are any required questions - guide questions
            requiredQuestionGroups.AddRange(dmsQuestions
                .Where(q => string.IsNullOrEmpty(q.Answer)
                    && q.DMSQuestionGuide != null
                    && q.DMSQuestionGuide.ActiveSwitch
                    && q.DMSQuestionGuide.RequiredOnSubmission
                    && (q.ChildId == 0 || q.ChildId == null) && (q.SubId == 0 || q.SubId == null))
                .Select(q => q.DMSQuestionGuide != null ? q.DMSQuestionGuide.DMSQuestionGroup != null ? q.DMSQuestionGuide.DMSQuestionGroup.GroupName ?? "" : "" : "")
                .Distinct().ToList());


            // 2. Check if there are any required questions in follow up child questions
            // Only applied if Answer to guide (bool) question is Yes
            var childQuestionIds = dmsQuestions
                .Where(q => string.IsNullOrEmpty(q.Answer)
                    && q.ChildId > 0 && (q.SubId == 0 || q.SubId == null) 
                    && q.DMSQuestionGuideChild != null
                    && q.DMSQuestionGuideChild.CActiveSwitch
                    && q.DMSQuestionGuideChild.CRequiredOnSubmission)
                .Select(q => q.QuestionId).Distinct().ToList();

            if (childQuestionIds != null && childQuestionIds.Count > 0)
            {
                requiredQuestionGroups.AddRange(dmsQuestions.Where(q => !string.IsNullOrEmpty(q.Answer) && q.Answer.ToLower() == "yes"
                        && childQuestionIds.Contains(q.QuestionId)
                        && (q.ChildId == 0 || q.ChildId == null) && (q.SubId == 0 || q.SubId == null) 
                        && q.DMSQuestionGuide != null
                        && !string.IsNullOrEmpty(q.DMSQuestionGuide.AnswerType) && q.DMSQuestionGuide.AnswerType.ToLower() == "bool" 
                        && q.DMSQuestionGuide.ActiveSwitch)
                    .Select(q => q.DMSQuestionGuide != null ? q.DMSQuestionGuide.DMSQuestionGroup != null ? q.DMSQuestionGuide.DMSQuestionGroup.GroupName ?? "" : "" : "")
                    .Distinct().ToList());
            }

            // 3. Check if there are any required questions in follow up sub questions
            // Only applied depending selected Answer to guide (selection) question
            var subQuestions = dmsQuestions.Where(q => string.IsNullOrEmpty(q.Answer) 
                    && q.ChildId > 0 && q.SubId > 0
                    && q.DMSQuestionGuideSub != null 
                    && q.DMSQuestionGuideSub.SActiveSwitch
                    && q.DMSQuestionGuideSub.SRequiredOnSubmission)
                .Select(q => new { q.QuestionId, ChildId = q.ChildId ?? 0 }).Distinct().ToList();

            if (subQuestions != null && subQuestions.Count > 0)
            {
                var questionIds = subQuestions.Select(d => d.QuestionId).Distinct().ToList();
                var selectionQuestions = dmsQuestions
                    .Where(q => questionIds.Contains(q.QuestionId) 
                        && !string.IsNullOrEmpty(q.Answer)
                        && (q.ChildId == 0 || q.ChildId == null) && (q.SubId == 0 || q.SubId == null)
                        && q.DMSQuestionGuide != null
                        && q.DMSQuestionGuide.ActiveSwitch
                        && !string.IsNullOrEmpty(q.DMSQuestionGuide.AnswerType) && q.DMSQuestionGuide.AnswerType.ToLower() == "selection")
                    .Select(q => new
                    {
                        q.QuestionId,
                        Answer = q.Answer ?? "",                                               
                        GroupName = q.DMSQuestionGuide != null ? q.DMSQuestionGuide.DMSQuestionGroup != null ? q.DMSQuestionGuide.DMSQuestionGroup.GroupName ?? "" : "" : ""
                    }).ToList();
                
                var distinctAnswers = selectionQuestions.Select(d => d.Answer).Distinct().ToList();                
                var childSelections = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSQuestionGuideChild>().QueryableList.AsNoTracking()
                    .Where(d => questionIds.Contains(d.QuestionId)
                        && !string.IsNullOrEmpty(d.Description) && distinctAnswers.Contains(d.Description)
                        && !string.IsNullOrEmpty(d.CAnswerType) && d.CAnswerType.ToLower() == "string" && d.CFollowUp == true
                    ).Select(d => new { d.QuestionId, d.ChildId, Description = d.Description ?? ""}).Distinct().ToListAsync();
                
                var parsedSelections = selectionQuestions.Join(childSelections,
                    s => new { s.QuestionId, s.Answer },
                    c => new { c.QuestionId, Answer = c.Description },
                    (s, c) => new { s.QuestionId, c.ChildId, s.GroupName }).ToList();
                
                requiredQuestionGroups.AddRange(subQuestions.Join(parsedSelections, 
                    sub => new { sub.QuestionId, sub.ChildId },
                    sel => new { sel.QuestionId, sel.ChildId },
                    (sub, sel) => sel.GroupName));
            }

            var filteredGroup = requiredQuestionGroups.Distinct().ToList();
            if (filteredGroup.Count > 0)
            {
                throw new ValueNotAllowedException($"Please answer question(s) in {filteredGroup.FirstOrDefault()} tab.");
            }
            //Questionnaire - END
        }

        public async Task CopyDisclosure(int oldDMSId, int newDMSId, string userName, bool copyCaseInfo, bool copyInventors, bool copyAbstract, bool copyRemarks, bool copyKeywords, bool copyImages, string copiedQuestions)
        {
            await _cpiDbContext.ExecuteSqlRawAsync($"procDMSDisclosureCopy @OldDMSId={oldDMSId},@NewDMSId={newDMSId},@CreatedBy='{userName}',@CopyCaseInfo={copyCaseInfo},@CopyInventors={copyInventors},@CopyAbstract={copyAbstract},@CopyRemarks={copyRemarks},@CopyKeywords={copyKeywords},@CopyImages={copyImages},@CopiedQuestions='{copiedQuestions}'");
        }

        public async Task RefreshCopySetting(List<DisclosureCopySetting> added, List<DisclosureCopySetting> deleted)
        {
            if (added.Count > 0)
                _cpiDbContext.GetRepository<DisclosureCopySetting>().Add(added);

            if (deleted.Count > 0)
                _cpiDbContext.GetRepository<DisclosureCopySetting>().Delete(deleted);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task UpdateCopySetting(DisclosureCopySetting setting)
        {
            var existing = await _cpiDbContext.GetRepository<DisclosureCopySetting>().GetByIdAsync(setting.CopySettingId);
            if (existing != null)
            {
                existing.Copy = setting.Copy;
                _cpiDbContext.GetRepository<DisclosureCopySetting>().Update(existing);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task AddCopySettings(List<DisclosureCopySetting> settings)
        {
            if (settings.Count > 0)
            {
                _cpiDbContext.GetRepository<DisclosureCopySetting>().Add(settings);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task<CPiUserSetting> GetMainCopySettings(string userId)
        {
            var setting = await _cpiDbContext.GetRepository<CPiSetting>().QueryableList.Where(d => d.Name == "DMSCopySetting").AsNoTracking().FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = new CPiSetting { Name = "DMSCopySetting", Policy = "*" };
                _cpiDbContext.GetRepository<CPiSetting>().Add(setting);
                await _cpiDbContext.SaveChangesAsync();
            }
            return await _cpiDbContext.GetRepository<CPiUserSetting>().QueryableList.Where(u => u.UserId == userId && u.SettingId == setting.Id).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task UpdateMainCopySettings(CPiUserSetting userSetting)
        {
            if (userSetting.Id > 0)
                _cpiDbContext.GetRepository<CPiUserSetting>().Update(userSetting);
            else
                _cpiDbContext.GetRepository<CPiUserSetting>().Add(userSetting);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task<int> GetMainCopySettingId()
        {
            var setting = await _cpiDbContext.GetRepository<CPiSetting>().QueryableList.Where(d => d.Name == "DMSCopySetting").AsNoTracking().FirstOrDefaultAsync();
            if (setting != null)
            {
                return setting.Id;
            }
            else
            {
                setting = new CPiSetting { Name = "DMSCopySetting", Policy = "*" };
                _cpiDbContext.GetRepository<CPiSetting>().Add(setting);
                await _cpiDbContext.SaveChangesAsync();
                return setting.Id;
            }
        }

        public async Task CopyToClearance(int dmsId, int pacId, bool copyKeywords)
        {
            var disclosure = await GetByIdAsync(dmsId);
            Guard.Against.NoRecordPermission(disclosure != null);

            var clearance = await _cpiDbContext.GetReadOnlyRepositoryAsync<PacClearance>().QueryableList.Where(d => d.PacId == pacId).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(clearance != null);

            //Copy Inventors from DMS to Pat Clearance Requestors
            var inventors = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSInventor>().QueryableList.Where(d => d.DMSId == disclosure.DMSId).ToListAsync();
            if (inventors.Any())
            {
                var requestorList = new List<PacInventor>();
                foreach (var inventor in inventors)
                {
                    var newRequestor = new PacInventor()
                    {
                        PacId = clearance.PacId,
                        InventorID = inventor.InventorID,
                        OrderOfEntry = inventor.OrderOfEntry,
                        Remarks = inventor.Remarks,
                        CreatedBy = clearance.CreatedBy,
                        UpdatedBy = clearance.UpdatedBy,
                        DateCreated = clearance.DateCreated,
                        LastUpdate = clearance.LastUpdate
                    };
                    requestorList.Add(newRequestor);
                }

                if (requestorList.Any())
                {
                    _cpiDbContext.GetRepository<PacInventor>().Add(requestorList);
                    await _cpiDbContext.SaveChangesAsync();
                }
            }

            if (copyKeywords)
            {
                var keywords = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSKeyword>().QueryableList.Where(d => d.DMSId == disclosure.DMSId).ToListAsync();
                if (keywords.Any())
                {
                    var newKeywordList = new List<PacKeyword>();
                    foreach (var keyword in keywords)
                    {
                        var newKeyword = new PacKeyword()
                        {
                            PacId = clearance.PacId,
                            Keyword = keyword.Keyword,
                            OrderOfEntry = keyword.OrderOfEntry ?? 0,
                            CreatedBy = clearance.CreatedBy,
                            UpdatedBy = clearance.UpdatedBy,
                            DateCreated = clearance.DateCreated,
                            LastUpdate = clearance.LastUpdate
                        };
                        newKeywordList.Add(newKeyword);
                    }
                    if (newKeywordList.Any())
                    {
                        _cpiDbContext.GetRepository<PacKeyword>().Add(newKeywordList);
                        await _cpiDbContext.SaveChangesAsync();
                    }

                }
            }

            var patentPortfolio1 = await _cpiDbContext.GetRepository<PacQuestion>().QueryableList
                                                    .Where(d => d.PacId == clearance.PacId
                                                        && d.PacQuestionGuide.PacQuestionGroup.GroupName.Contains("Patent Portfolio")
                                                        && d.PacQuestionGuide.Question.Contains("Have any Invention Records been submitted?")
                                                    )
                                                    .FirstOrDefaultAsync();
            if (patentPortfolio1 != null)
            {
                patentPortfolio1.Answer = "Yes";
                _cpiDbContext.GetRepository<PacQuestion>().Update(patentPortfolio1);
                await _cpiDbContext.SaveChangesAsync();
            }

            var patentPortfolio2 = await _cpiDbContext.GetRepository<PacQuestion>().QueryableList
                                                    .Where(d => d.PacId == clearance.PacId
                                                        && d.PacQuestionGuide.PacQuestionGroup.GroupName.Contains("Patent Portfolio")
                                                        && d.PacQuestionGuide.Question.Contains("If yes, please provide the disclosure number(s)")
                                                    )
                                                    .FirstOrDefaultAsync();

            if (patentPortfolio2 != null)
            {
                patentPortfolio2.Answer = disclosure.DisclosureNumber;
                _cpiDbContext.GetRepository<PacQuestion>().Update(patentPortfolio2);
                await _cpiDbContext.SaveChangesAsync();
            }

        }

        public async Task RefreshCopyClearanceSetting(List<DisclosureCopyClearanceSetting> added, List<DisclosureCopyClearanceSetting> deleted)
        {
            if (added.Count > 0)
                _cpiDbContext.GetRepository<DisclosureCopyClearanceSetting>().Add(added);

            if (deleted.Count > 0)
                _cpiDbContext.GetRepository<DisclosureCopyClearanceSetting>().Delete(deleted);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task UpdateCopyClearanceSetting(DisclosureCopyClearanceSetting setting)
        {
            var existing = await _cpiDbContext.GetRepository<DisclosureCopyClearanceSetting>().GetByIdAsync(setting.CopySettingId);
            if (existing != null)
            {
                existing.Copy = setting.Copy;
                _cpiDbContext.GetRepository<DisclosureCopyClearanceSetting>().Update(existing);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task<string> GetCPiUserName(string userId)
        {
            return await _cpiDbContext.GetReadOnlyRepositoryAsync<CPiUser>().QueryableList.Where(u => u.Id == userId).Select(u => u.FullName).FirstOrDefaultAsync();
        }

        public async Task<Disclosure> GetInventorAwardInfo(int DMSId)
        {
            var dmsAwardInfo = await _cpiDbContext.GetRepository<Disclosure>().QueryableList.Where(d => d.DMSId == DMSId)
                                                    .Include(d => d.Inventors)
                                                    .Include(d => d.Awards)
                                                    .FirstOrDefaultAsync();
            return dmsAwardInfo;
        }

        public async Task<bool> IsESignatureCompleted(int dmsId)
        {
            var settings = await _settings.GetSetting();
            var disclosure = await GetByIdAsync(dmsId);
            if (settings.IsSharePointIntegrationOn)
            {
                return await _cpiDbContext.GetRepository<SharePointFileSignature>().QueryableList.AsNoTracking()
                                                            .AnyAsync(d => d.SignatureCompleted == true && (d.SignatureFileId == (disclosure.SignatureFileId ?? 0)) && !string.IsNullOrEmpty(d.SignedDocDriveItemId));
            }
            else
            {
                return await _cpiDbContext.GetRepository<DocFileSignature>().QueryableList.AsNoTracking()
                                                            .AnyAsync(d => d.SignatureCompleted == true && (d.SignatureFileId == (disclosure.SignatureFileId ?? 0)) && d.SignedDocFileId > 0);
            }
        }


        public async Task<IEnumerable<DMSActionReminderEmailDTO>> GetActionReminderEmails()
        {
            var reminderEmails = await _cpiDbContext.Query<DMSActionReminderEmailDTO>().FromSqlInterpolated($"Exec procDMSActionReminderEmail").AsNoTracking().ToListAsync();
            return reminderEmails;
        }

        public async Task LogActionReminders(List<DMSActionReminderLog> reminderLogs)
        {
            if (reminderLogs != null && reminderLogs.Count > 0)
            {
                _cpiDbContext.GetRepository<DMSActionReminderLog>().Add(reminderLogs);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task<List<EntityFilterDTO>> GetReviewerEntityList(List<int> reviewerEntityIds)
        {
            var reviewerEntityList = new List<EntityFilterDTO>();

            var settings = await _settings.GetSetting();

            if (reviewerEntityIds != null && reviewerEntityIds.Count() > 0)
            {
                if (settings.ReviewerEntityType == DMSReviewerType.Client)
                {
                    reviewerEntityList.AddRange(await _cpiDbContext.GetReadOnlyRepositoryAsync<Client>().QueryableList.AsNoTracking()
                                    .Where(d => reviewerEntityIds.Contains(d.ClientID))
                                    .Select(d => new EntityFilterDTO()
                                    {
                                        Id = d.ClientID,
                                        Code = d.ClientCode,
                                        Name = d.ClientName
                                    })
                                    .ToListAsync());
                }
                else if (settings.ReviewerEntityType == DMSReviewerType.Area)
                {
                    reviewerEntityList.AddRange(await _cpiDbContext.GetReadOnlyRepositoryAsync<PatArea>().QueryableList.AsNoTracking()
                                    .Where(d => reviewerEntityIds.Contains(d.AreaID))
                                    .Select(d => new EntityFilterDTO()
                                    {
                                        Id = d.AreaID,
                                        Code = d.Area,
                                        Name = d.Description
                                    })
                                    .ToListAsync());
                }
            }

            return reviewerEntityList;
        }

        public async Task<string> GetOrCreateCPIDisclosureStatus(string disclosureStatus, bool canReview = false, bool canPreview = false, bool canSubmit = false)
        {
            var existingStatus = await _cpiDbContext.GetRepository<DMSDisclosureStatus>().QueryableList.AsNoTracking()
                                        .Where(s => s.DisclosureStatus.ToLower() == disclosureStatus.ToLower())
                                        .Select(s => s.DisclosureStatus)
                                        .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(existingStatus))
                return existingStatus;

            var userName = _user.GetUserName();
            var today = DateTime.Now;
            var newStatus = new DMSDisclosureStatus
            {
                DisclosureStatus = disclosureStatus,
                Description = disclosureStatus,
                CPIDiscStatus = true,
                LockRecord = true,
                CreatedBy = userName,
                UpdatedBy = userName,
                DateCreated = today,
                LastUpdate = today,
                CanReview = canReview,
                CanPreview = canPreview,
                CanSubmit = canSubmit
            };

            _cpiDbContext.GetRepository<DMSDisclosureStatus>().Add(newStatus);
            return newStatus.DisclosureStatus;
        }

        public async  Task<List<DMSDisclosureStatus>> GetDMSDisclosureStatusesForColorSetting()
        {
            return await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSDisclosureStatus>().QueryableList.ToListAsync();
        }

        #region Trade Secret
        private async Task<(string? ActivityCode, Dictionary<string, string?[]>? AuditLogs)> SetTradeSecret(Disclosure disclosure)
        {
            var activityCode = string.Empty;

            if (!_user.CanAccessDMSTradeSecret())
                return (null, null);

            var current = await GetByIdAsync(disclosure.DMSId);
            var currentTradeSecret = current?.TradeSecret ?? new DisclosureTradeSecret();
            var currentIsTradeSecret = current?.IsTradeSecret ?? false;

            if (current != null)
                disclosure.TradeSecretDate = current.TradeSecretDate;

            if (currentIsTradeSecret)
            {
                var isTSCleared = await _tradeSecretService.IsUserCleared(TradeSecretScreen.DMSDisclosure, disclosure.DMSId);

                //only cleared admins can turn off IsTradeSecret flag
                if (!(disclosure.IsTradeSecret ?? false) && !(_user.IsDMSTradeSecretAdmin() && isTSCleared))
                    disclosure.IsTradeSecret = true;

                //only cleared full modify users can edit trade secret fields
                if (!(_user.CanEditDMSTradeSecretFields() && isTSCleared))
                    disclosure.RestoreTradeSecret(currentTradeSecret, true);
            }

            if (disclosure.IsTradeSecret ?? false)
            {
                disclosure.TradeSecret = disclosure.CreateTradeSecret(new DisclosureTradeSecret());

                if (disclosure.DMSId == 0 || !currentIsTradeSecret)
                {
                    disclosure.TradeSecretDate = DateTime.Now;

                    activityCode = TradeSecretActivityCode.Create;
                }
                else if (disclosure.TradeSecret.DisclosureTitle != current?.TradeSecret?.DisclosureTitle || disclosure.TradeSecret.Abstract != current?.TradeSecret?.Abstract)
                {
                    activityCode = TradeSecretActivityCode.Update;
                }
            }
            else if (currentIsTradeSecret)
            {
                disclosure.RestoreTradeSecret(currentTradeSecret);
                disclosure.TradeSecret = new DisclosureTradeSecret();

                activityCode = TradeSecretActivityCode.Update;
            }

            //create activity log
            if (!string.IsNullOrEmpty(activityCode))
                return (activityCode, CreateAuditLogs(current, disclosure));

            return (null, null);
        }

        private async Task<(bool Allowed, int DMSId)> CanDeleteTradeSecret(int dmsId)
        {
            var isTradeSecret = (await QueryableList.Where(i => i.DMSId == dmsId).Select(i => i.IsTradeSecret).SingleOrDefaultAsync()) ?? false;

            if (isTradeSecret && _user.CanAccessDMSTradeSecret())
            {
                //only cleared admins can delete trade secret
                var isTSCleared = await _tradeSecretService.IsUserCleared(TradeSecretScreen.DMSDisclosure, dmsId);
                return (_user.IsDMSTradeSecretAdmin() && isTSCleared, dmsId);
            }

            return (!isTradeSecret, 0);
        }

        private async Task<TradeSecretActivity> CreateTradeSecretActivityLog(int dmsId, string activityCode, Dictionary<string, string?[]>? auditLogs)
        {
            var tsRequest = await _tradeSecretService.GetUserRequest(_tradeSecretService.CreateLocator(TradeSecretScreen.DMSDisclosure, dmsId));
            var tsActivity = _tradeSecretService.CreateActivity(TradeSecretScreen.DMSDisclosure, TradeSecretScreen.DMSDisclosure, dmsId, activityCode, tsRequest?.RequestId ?? 0, auditLogs);

            return tsActivity;
        }

        private Dictionary<string, string?[]> CreateAuditLogs(Disclosure? oldValues, Disclosure? newValues)
        {
            var auditLogs = new Dictionary<string, string?[]>();

            if (newValues?.IsTradeSecret != oldValues?.IsTradeSecret)
                auditLogs.Add("IsTradeSecret", [oldValues?.IsTradeSecret.ToString(), newValues?.IsTradeSecret.ToString()]);

            if (newValues?.TradeSecret?.DisclosureTitle != oldValues?.TradeSecret?.DisclosureTitle)
                auditLogs.Add("DisclosureTitle", [oldValues?.TradeSecret?.DisclosureTitle, newValues?.TradeSecret?.DisclosureTitle]);

            if (newValues?.TradeSecret?.Abstract != oldValues?.TradeSecret?.Abstract)
                auditLogs.Add("Abstract", [oldValues?.TradeSecret?.Abstract, newValues?.TradeSecret?.Abstract]);

            return auditLogs;
        }
        #endregion
    }
}