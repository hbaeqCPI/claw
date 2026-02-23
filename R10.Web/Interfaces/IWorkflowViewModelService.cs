using R10.Core.Entities.Clearance;
using R10.Core.Entities.DMS;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.PatClearance;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;

namespace R10.Web.Interfaces
{
    public interface IWorkflowViewModelService
    {
        Task<List<PatWorkflowAction>> GetCountryApplicationWorkflowActions(CountryApplication application, PatWorkflowTriggerType triggerType, bool clearBase = true);
        Task<List<PatWorkflowAction>> GetInventionWorkflowActions(Invention invention, PatWorkflowTriggerType triggerType, bool clearBase = true);
        Task<List<PatWorkflowAction>> GetPatActionDueWorkflowActions(PatActionDue actionDue, PatWorkflowTriggerType triggerType, bool clearBase = true);
        Task<List<PatWorkflowAction>> GetPatActionDueInvWorkflowActions(PatActionDueInv actionDue, PatWorkflowTriggerType triggerType, bool clearBase = true);
        Task<List<PatWorkflowAction>> GetPatCostTrackingWorkflowActions(PatCostTrack costTrack, PatWorkflowTriggerType triggerType, bool clearBase = true);

        Task<List<TmkWorkflowAction>> GetTrademarkWorkflowActions(TmkTrademark trademark, TmkWorkflowTriggerType triggerType, bool clearBase = true);
        Task<List<TmkWorkflowAction>> GetTmkActionDueWorkflowActions(TmkActionDue actionDue, TmkWorkflowTriggerType triggerType, bool clearBase = true);
        Task<List<TmkWorkflowAction>> GetTmkCostTrackingWorkflowActions(TmkCostTrack costTrack, TmkWorkflowTriggerType triggerType, bool clearBase = true);

        Task<List<GMWorkflowAction>> GetGeneralMatterWorkflowActions(GMMatter gm, GMWorkflowTriggerType triggerType, bool clearBase = true);
        Task<List<GMWorkflowAction>> GetGMActionDueWorkflowActions(GMActionDue actionDue, GMWorkflowTriggerType triggerType, bool clearBase = true);
        Task<List<GMWorkflowAction>> GetGMCostTrackingWorkflowActions(GMCostTrack costTrack, GMWorkflowTriggerType triggerType, bool clearBase = true);
        
        List<PatWorkflowAction> ClearPatBaseWorkflowActions(List<PatWorkflowAction> workflowActions);
        List<TmkWorkflowAction> ClearTmkBaseWorkflowActions(List<TmkWorkflowAction> workflowActions);
        List<GMWorkflowAction> ClearGMBaseWorkflowActions(List<GMWorkflowAction> workflowActions);  
        
        Task<List<TmcWorkflowAction>> GetSearchRequestWorkflowActions(TmcClearance clearance, TmcWorkflowTriggerType triggerType, bool clearBase = true);

        Task<List<PacWorkflowAction>> GetPatClearanceWorkflowActions(PacClearance pacClearance, PacWorkflowTriggerType triggerType, bool clearBase = true);

        Task<List<DMSWorkflowAction>> GetDisclosureWorkflowActions(Disclosure disclosure, DMSWorkflowTriggerType triggerType, bool clearBase = true);

        List<DMSWorkflowAction> ClearDMSBaseWorkflowActions(List<DMSWorkflowAction> workflowActions);
    }
}
