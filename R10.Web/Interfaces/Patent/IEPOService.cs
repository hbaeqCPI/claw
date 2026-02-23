using R10.Core;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Web.Areas.Patent.ViewModels;
using R10.Core.Entities;
using R10.Core.DTOs;

namespace R10.Web.Interfaces
{
    public interface IEPOService
    {
        #region MyEPO
        bool IsMyEPOAPIOn();
        Task ResetSandbox();
        string CleanSearchNumber(string inputStr);

        Task<int> GetEPODocumentCodes();
        Task<List<EPOMailboxOutput>> GetUnhandledCommunications(EPOMailboxInput searchInput);
        Task<int> DownloadEPOMailDocuments(List<int> appIds, int logId = 0);
        Task ProcessDownloadedCommunication(List<EPOMailboxOutput> result, int logId, EPOMailboxInput searchInput, DateTime runDate, List<PatEPOMailLog>? epoLogs);        
        Task<List<string>> MarkCommunicationsHandled(List<string> communicationIds);
        Task<List<EPOWorkflowViewModel>> ProcessMailboxWorkflow(int logId);

        Task<int> GetPortfolios(int logId);
        Task<int> GetApplications(int logId);
        Task<int> GetDueDates(int logId);
        Task<int> GetDueDateTerms(int logId);
        Task LinkEPOApplications (int logId);
        Task ProcessDownloadedDueDates (int logId);
        Task<List<EPOWorkflowViewModel>> ProcessDueDateWorkflow(int logId);

        Task<IEnumerable<EPOWorkflowViewModel>> GetEPOWorkflows();
        Task LogEPOWorkflows(IEnumerable<EPOWorkflowViewModel> emailWorkflows);
        #endregion

        #region OPS
        bool IsOPSAPIOn();
        Task<int> GetFirstDrawings(int logId, List<int>? appIds = null);
        #endregion
    }
}
