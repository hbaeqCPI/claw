using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Clearance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface ITmcClearanceService : IEntityService<TmcClearance>
    {
        IQueryable<TmcClearanceCopySetting> TmcClearanceCopySettings { get; }

        IQueryable<TmcClearance> ForReviewList { get; }

        Task<bool> UserCanEditClearance(int tmcId);
        Task<bool> IsUserReviewerInTMC(int tmcId);

        Task ValidatePermission(int TmcId);

        IQueryable<TmcQuestionGroup> QuestionGroups { get; }
        IQueryable<TmcQuestion> TmcQuestions { get; }

        IQueryable<TmcClearanceStatusHistory> GetStatusHistory(int tmcId);

        Task UpdateCountryInfo(int tmcId, string areaFieldName, string countries);

        IQueryable<T> QueryableChildList<T>() where T : BaseEntity;

        Task<bool> CanSubmitClearance(int tmcId);

        Task Submit(TmcClearance disclosure);

        Task<int> CheckStatusHistoryRemark(int tmcId);

        Task UpdateStatusRemark(int tmcId, int logId, string remark);

        new Task<byte[]> UpdateRemarks(TmcClearance clearance);

        Task<byte[]> UpdateStatus(TmcClearance clearance, string remarks);

        Task CopyClearance(int oldTmcId, int newTmcId, string userName, bool copyCaseInfo, bool copyCountries, 
            bool copyRequestedTerm, bool copyKeywords, bool CopyImages, string copiedQuestions);

        Task RefreshCopySetting(List<TmcClearanceCopySetting> added, List<TmcClearanceCopySetting> deleted);
        Task UpdateCopySetting(TmcClearanceCopySetting setting);

        Task<string> BuildCaseNumber(string caseNumber);

        Task<List<TmcWorkflowAction>> CheckWorkflowAction(TmcWorkflowTriggerType triggerType);

        Task CheckRequiredOnSubmission(int tmcId);

    }
}