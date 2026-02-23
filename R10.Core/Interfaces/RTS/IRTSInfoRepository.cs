using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;
using R10.Core.Services;

namespace R10.Core.Interfaces
{
    public interface IRTSInfoRepository
    {
        Task<List<RTSInfoSettingsMenu>> GetMenu(string country);
        RTSSearchBiblioDTO GetBiblio(int plAppId);

        Task<List<RTSSearchInventorDTO>> GetInventors(int plAppId);
        Task<List<RTSSearchTitleDTO>> GetTitles(int plAppId);
        RTSSearchBiblioUSDTO GetBiblioUS(int plAppId);
        Task<List<RTSSearchBiblioUSDTO>> GetBiblioUSs(int plAppId);
        Task<List<RTSSearchAssignmentDTO>> GetAssignments(int plAppId);
        Task<List<RTSSearchPriorityDTO>> GetPriorities(int plAppId);
        Task<List<RTSSearchApplicantDTO>> GetApplicants(int plAppId);
        Task<List<RTSSearchIPClassDTO>> GetIPClasses(int plAppId);
        RTSSearchAbstractDTO GetAbstractClaims(int plAppId);
        Task<List<RTSSearchDocCitedDTO>> GetDocsCited(int plAppId);
        Task<List<RTSSearchDocRefByDTO>> GetDocsRefBy(int plAppId);
        Task<List<RTSSearchPTADTO>> GetPTAs(int plAppId);

        Task<List<RTSSearchContinuityParentDTO>> GetContinuitiesParent(int plAppId);
        Task<List<RTSSearchContinuityChildDTO>> GetContinuitiesChild(int plAppId);

        Task<List<RTSSearchIFWDTO>> GetIFWs(int plAppId);

        //Use for Dashboard widget criteria
        Task<List<RTSSearchIFWDTO>> GetSearchPTODocuments();

        RTSSearchUSCorrespondenceDTO GetCorrespondence(int plAppId);
        Task<List<RTSSearchUSCorrespondenceDTO>> GetCorrespondences(int plAppId);
        Task<List<RTSSearchAgentDTO>> GetAgents(int plAppId);
        Task<List<RTSSearchActionUpdHistoryDTO>> GetActionUpdHistory(int plAppId, int revertType, int jobId);
        Task<List<UpdateHistoryBatchDTO>> GetActionUpdHistoryBatches(int plAppId, int revertType, int jobId);
        Task<List<RTSSearchActionClosedUpdHistoryDTO>> GetActionClosedUpdHistory(int plAppId, int jobId);
        Task<List<UpdateHistoryBatchDTO>> GetActionClosedUpdHistoryBatches(int plAppId, int jobId);
        Task<List<RTSSearchActionAsDownloadedDTO>> GetActions(int plAppId, bool asDownloaded);
        Task UndoActions(int jobId, int plAppId, string updatedBy);
        Task<List<RTSSearchDesCountryDTO>> GetDesCountries(int plAppId);
        Task<List<RTSSearchPFSDocDTO>> GetPFSDocs(int appId);
        Task<List<RTSSearchLSDDTO>> GetLSDs(int appId);
        Task<List<RTSSearchIPCDTO>> GetIPCs(int appId);
        Task<List<RTSSearchCPCDTO>> GetCPCs(int appId);
        Task<int> GetClaimsCount(int appId);

        Task<List<RTSPFSTitleUpdHistoryDTO>> GetPFSTitleUpdHistory(int appId);
        Task<List<RTSPFSAbstractUpdHistoryDTO>> GetPFSAbstractUpdHistory(int appId);
        RTSPFSCountryAppUpdHistoryDTO GetPFSCtryAppUpdHistory(int appId);

        Task<List<RTSPFSWorkflowBatch>> GetPFSUpdatesForWorkflow();
        Task<List<RTSPFSWorkflowApp>> GetPFSUpdatesToPublishedForWorkflow(string batchId);
        Task<List<RTSPFSWorkflowApp>> GetPFSUpdatesToGrantedForWorkflow(string batchId);
        void MarkPFSUpdatesWorkflowBatchAsGenerated(string batchId);
        void MarkPFSUpdatesWorkflowAsGenerated(string batchId, int appId);
        void MarkRTSAutoDocketActionWorkflowAsGenerated(int actId);

        List<RTSPFSStatisticsSearchOutput> StatisticsSearchInpadoc(RTSPFSStatisticsSearchInput searchInput);

        IQueryable<RTSSearch> RTSSearchRecords { get; }
        IQueryable<RTSSearchAction> RTSSearchActions { get; }
        IQueryable<RTSSearchUSIFW> RTSSearchUSIFWs { get; }         // for form extraction

        void MarkIFWAsTransferred(string fileName);

        #region RTS Update
        Task<int> UpdateBiblioRecord(int appId, string updatedBy);
        Task<List<RTSUpdateWorkflow>> GetUpdateWorkflowRecs(int jobId, int appId);
        Task<int> UpdateBiblioRecords(RTSUpdateCriteria criteria);
        Task MarkBiblioRecords();
        Task<bool> UndoBiblio(int jobId, int appId, int logId, string updatedBy);

        #endregion
    }
}
