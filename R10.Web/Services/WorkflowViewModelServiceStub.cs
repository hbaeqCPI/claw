using R10.Core.Entities.Clearance;
using R10.Core.Entities.DMS;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.PatClearance;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Web.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    /// <summary>
    /// Stub workflow service for debloated app; returns empty workflow actions.
    /// </summary>
    public class WorkflowViewModelServiceStub : IWorkflowViewModelService
    {
        public Task<List<PatWorkflowAction>> GetCountryApplicationWorkflowActions(CountryApplication application, PatWorkflowTriggerType triggerType, bool clearBase = true)
            => Task.FromResult(new List<PatWorkflowAction>());
        public Task<List<PatWorkflowAction>> GetInventionWorkflowActions(Invention invention, PatWorkflowTriggerType triggerType, bool clearBase = true)
            => Task.FromResult(new List<PatWorkflowAction>());
        public Task<List<PatWorkflowAction>> GetPatActionDueWorkflowActions(PatActionDue actionDue, PatWorkflowTriggerType triggerType, bool clearBase = true)
            => Task.FromResult(new List<PatWorkflowAction>());
        public Task<List<PatWorkflowAction>> GetPatActionDueInvWorkflowActions(PatActionDueInv actionDue, PatWorkflowTriggerType triggerType, bool clearBase = true)
            => Task.FromResult(new List<PatWorkflowAction>());
        public Task<List<PatWorkflowAction>> GetPatCostTrackingWorkflowActions(PatCostTrack costTrack, PatWorkflowTriggerType triggerType, bool clearBase = true)
            => Task.FromResult(new List<PatWorkflowAction>());

        public Task<List<TmkWorkflowAction>> GetTrademarkWorkflowActions(TmkTrademark trademark, TmkWorkflowTriggerType triggerType, bool clearBase = true)
            => Task.FromResult(new List<TmkWorkflowAction>());
        public Task<List<TmkWorkflowAction>> GetTmkActionDueWorkflowActions(TmkActionDue actionDue, TmkWorkflowTriggerType triggerType, bool clearBase = true)
            => Task.FromResult(new List<TmkWorkflowAction>());
        public Task<List<TmkWorkflowAction>> GetTmkCostTrackingWorkflowActions(TmkCostTrack costTrack, TmkWorkflowTriggerType triggerType, bool clearBase = true)
            => Task.FromResult(new List<TmkWorkflowAction>());

        public Task<List<GMWorkflowAction>> GetGeneralMatterWorkflowActions(GMMatter gm, GMWorkflowTriggerType triggerType, bool clearBase = true)
            => Task.FromResult(new List<GMWorkflowAction>());
        public Task<List<GMWorkflowAction>> GetGMActionDueWorkflowActions(GMActionDue actionDue, GMWorkflowTriggerType triggerType, bool clearBase = true)
            => Task.FromResult(new List<GMWorkflowAction>());
        public Task<List<GMWorkflowAction>> GetGMCostTrackingWorkflowActions(GMCostTrack costTrack, GMWorkflowTriggerType triggerType, bool clearBase = true)
            => Task.FromResult(new List<GMWorkflowAction>());

        public List<PatWorkflowAction> ClearPatBaseWorkflowActions(List<PatWorkflowAction> workflowActions) => workflowActions ?? new List<PatWorkflowAction>();
        public List<TmkWorkflowAction> ClearTmkBaseWorkflowActions(List<TmkWorkflowAction> workflowActions) => workflowActions ?? new List<TmkWorkflowAction>();
        public List<GMWorkflowAction> ClearGMBaseWorkflowActions(List<GMWorkflowAction> workflowActions) => workflowActions ?? new List<GMWorkflowAction>();

        public Task<List<TmcWorkflowAction>> GetSearchRequestWorkflowActions(TmcClearance clearance, TmcWorkflowTriggerType triggerType, bool clearBase = true)
            => Task.FromResult(new List<TmcWorkflowAction>());
        public Task<List<PacWorkflowAction>> GetPatClearanceWorkflowActions(PacClearance pacClearance, PacWorkflowTriggerType triggerType, bool clearBase = true)
            => Task.FromResult(new List<PacWorkflowAction>());
        public Task<List<DMSWorkflowAction>> GetDisclosureWorkflowActions(Disclosure disclosure, DMSWorkflowTriggerType triggerType, bool clearBase = true)
            => Task.FromResult(new List<DMSWorkflowAction>());
        public List<DMSWorkflowAction> ClearDMSBaseWorkflowActions(List<DMSWorkflowAction> workflowActions) => workflowActions ?? new List<DMSWorkflowAction>();
    }
}
