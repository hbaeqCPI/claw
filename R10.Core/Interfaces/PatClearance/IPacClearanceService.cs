using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Entities.PatClearance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IPacClearanceService : IEntityService<PacClearance>
    {
        IQueryable<PacClearanceCopySetting> PacClearanceCopySettings { get; }
        IQueryable<PacClearanceCopyDisclosureSetting> PacClearanceCopyDisclosureSettings { get; }

        IQueryable<PacClearance> ForReviewList { get; }

        Task<bool> UserCanEditClearance(int pacId);

        Task ValidatePermission(int PacId);

        IQueryable<PacQuestionGroup> QuestionGroups { get; }
        IQueryable<PacQuestion> PacQuestions { get; }

        IQueryable<PacClearanceStatusHistory> GetStatusHistory(int pacId);        

        IQueryable<T> QueryableChildList<T>() where T : BaseEntity;

        Task<bool> CanSubmitClearance(int pacId);

        Task Submit(PacClearance disclosure);

        Task<int> CheckStatusHistoryRemark(int pacId);

        Task UpdateStatusRemark(int pacId, int logId, string remark);

        new Task<byte[]> UpdateRemarks(PacClearance clearance);

        Task<byte[]> UpdateStatus(PacClearance clearance, string remarks);

        Task CopyClearance(int oldPacId, int newPacId, string userName, bool copyCaseInfo, bool copyKeywords, bool CopyImages, string copiedQuestions);
        Task RefreshCopySetting(List<PacClearanceCopySetting> added, List<PacClearanceCopySetting> deleted);
        Task UpdateCopySetting(PacClearanceCopySetting setting);

        Task CopyToDisclosure(int pacId, int dmsId, bool copyKeywords);
        Task RefreshCopyDisclosureSetting(List<PacClearanceCopyDisclosureSetting> added, List<PacClearanceCopyDisclosureSetting> deleted);
        Task UpdateCopyDisclosureSetting(PacClearanceCopyDisclosureSetting setting);


        Task<string> BuildCaseNumber(string caseNumber);
               
        Task<List<PacWorkflowAction>> CheckWorkflowAction(PacWorkflowTriggerType triggerType);

        Task CheckRequiredOnSubmission(int pacId);
        Task<int> GetUserInventorId();
    }
}