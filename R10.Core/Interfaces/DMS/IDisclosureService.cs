using R10.Core.DTOs;
using R10.Core.Entities.DMS;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.DMS
{
    public interface IDisclosureService : IEntityService<Disclosure>
    {
        string SubmitStatus { get; }
        IQueryable<DisclosureCopySetting> DisclosureCopySettings { get; }
        IQueryable<DisclosureCopyClearanceSetting> DisclosureCopyClearanceSettings { get; }

        IQueryable<Disclosure> ForReviewList { get; }
        IQueryable<Disclosure> ForPreviewList { get; }

        IQueryable<DMSDisclosureStatus> DisclosureStatuses { get; }
        IQueryable<DMSDisclosureStatusHistory> DisclosureStatusHistories { get; }

        Task Submit(Disclosure disclosure);
        Task UpdateStatus(Disclosure disclosure, bool resetSignatureFile = false);

        new Task<byte[]> UpdateRemarks(Disclosure disclosure);
        Task<byte[]> UpdateRecommendation(Disclosure disclosure);

        Task<string> GetDraftStatus();
        Task<bool> CanSubmitDisclosure(int dmsId);
        
        Task ValidatePermission(int DMSId, List<string> roles, bool checkStatus);

        IQueryable<DMSQuestionGroup> QuestionGroups { get; }
        IQueryable<DMSQuestion> DMSQuestions { get; }

        Task<int> GetUserReviewerId(CPiEntityType reviewerType);
        Task<int> GetUserPreviewerId(CPiEntityType previewerType);
        Task<int> GetUserInventorId();
        Task<bool> IsUserDefaultInventor(int dmsId);

        Task<bool> IsUserInventorInDMS(int dmsId);
        Task<bool> IsUserReviewerInDMS(int dmsId);

        IQueryable<DMSPreview> GetPreviews(int dmsId);
        IQueryable<DMSReview> GetReviews(int dmsId);
        IQueryable<DMSValuation> GetValuations(int dmsId);
        IQueryable<DMSDisclosureStatusHistory> GetStatusHistory(int dmsId);
        IQueryable<DMSRecommendationHistory> GetRecommendationHistory(int dmsId);
                
        Task<List<DMSWorkflowAction>> CheckWorkflowAction(DMSWorkflowTriggerType triggerType);
        Task GenerateWorkflowAction(int dmsId, int actionTypeId);
        Task<List<DMSActionDue>> CloseWorkflowAction(int dmsId, int actionTypeId);
        void DetachAllEntities();

        Task CheckRequiredOnSubmission(int dmsId);

        Task CopyDisclosure(int oldDMSId, int newDMSId, string userName, bool copyCaseInfo, bool copyInventors, bool copyAbstract, bool copyRemarks, bool copyKeywords, bool copyImages, string copiedQuestions);
        Task RefreshCopySetting(List<DisclosureCopySetting> added, List<DisclosureCopySetting> deleted);
        Task UpdateCopySetting(DisclosureCopySetting setting);
        Task<CPiUserSetting> GetMainCopySettings(string userId);
        Task UpdateMainCopySettings(CPiUserSetting userSetting);
        Task<int> GetMainCopySettingId();
        Task AddCopySettings(List<DisclosureCopySetting> settings);

        Task CopyToClearance(int dmsId, int pacId, bool copyKeywords);
        Task RefreshCopyClearanceSetting(List<DisclosureCopyClearanceSetting> added, List<DisclosureCopyClearanceSetting> deleted);
        Task UpdateCopyClearanceSetting(DisclosureCopyClearanceSetting setting);        

        Task<string> GetCPiUserName(string userId);

        Task<Disclosure> GetInventorAwardInfo(int DMSId);

        Task<bool> IsESignatureCompleted(int DMSId);

        Task<IEnumerable<DMSActionReminderEmailDTO>> GetActionReminderEmails();

        Task LogActionReminders(List<DMSActionReminderLog> reminderLogs);

        Task<List<EntityFilterDTO>> GetReviewerEntityList(List<int> reviewerEntityIds);

        Task<string> GetOrCreateCPIDisclosureStatus(string disclosureStatus, bool canReview = false, bool canPreview = false, bool canSubmit = false);

        Task<List<DMSDisclosureStatus>> GetDMSDisclosureStatusesForColorSetting();
    }
}
