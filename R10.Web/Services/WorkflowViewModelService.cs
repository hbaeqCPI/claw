using Kendo.Mvc.Extensions;
using Microsoft.EntityFrameworkCore;
// using R10.Core.Entities.Clearance; // Removed during deep clean
// using R10.Core.Entities.DMS; // Removed during deep clean
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
// using R10.Core.Entities.PatClearance; // Removed during deep clean
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
// using R10.Core.Interfaces.DMS; // Removed during deep clean
using R10.Core.Interfaces.Patent;
using R10.Web.Interfaces;

namespace R10.Web.Services
{
    public class WorkflowViewModelService : IWorkflowViewModelService
    {
        private readonly ICountryApplicationService _applicationService;
        private readonly ITmkTrademarkService _trademarkService;
//         private readonly IGMMatterService _gmMatterService; // Removed during deep clean
        // Removed during deep clean - GM module removed
        // private readonly IGMMatterAttorneyService _matterAttorneyService;
//         private readonly ITmcClearanceService _tmcClearanceService; // Removed during deep clean
//         private readonly IPacClearanceService _pacClearanceService; // Removed during deep clean
//         private readonly IDisclosureService _disclosureService; // Removed during deep clean

        public WorkflowViewModelService(ICountryApplicationService applicationService,
                                        ITmkTrademarkService trademarkService
//                                         IGMMatterService gmMatterService, // Removed during deep clean
                                        // Removed during deep clean - GM module removed
                                        // IGMMatterAttorneyService matterAttorneyService,
//                                         ITmcClearanceService tmcClearanceService, // Removed during deep clean
//                                         IPacClearanceService pacClearanceService, // Removed during deep clean
//                                         IDisclosureService disclosureService) // Removed during deep clean
        )
        {
            _applicationService = applicationService;
            _trademarkService = trademarkService;
            // Removed during deep clean
            // _gmMatterService = gmMatterService;
            // _matterAttorneyService = matterAttorneyService;
            // _tmcClearanceService = tmcClearanceService;
            // _pacClearanceService = pacClearanceService;
            // _disclosureService = disclosureService;
        }

        public async Task<List<PatWorkflowAction>> GetCountryApplicationWorkflowActions(CountryApplication application, PatWorkflowTriggerType triggerType, bool clearBase = true)
        {
            var workflowActions = await _applicationService.CheckWorkflowAction(triggerType);
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || (w.Workflow.ClientFilter != null && w.Workflow.ClientFilter.Contains("|" + application.Invention.ClientID.ToString() + "|"))).ToList();

            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.CountryFilter) || (w.Workflow.CountryFilter != null && w.Workflow.CountryFilter.Contains("|" + application.Country + "|"))).ToList();
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.CaseTypeFilter) || (w.Workflow.CaseTypeFilter != null && w.Workflow.CaseTypeFilter.Contains("|" + application.CaseType + "|"))).ToList();
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.RespOfficeFilter) || (w.Workflow.RespOfficeFilter != null && w.Workflow.RespOfficeFilter.Contains("|" + application.RespOffice + "|"))).ToList();

            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.AttorneyFilter) || (w.Workflow.AttorneyFilter != null &&
                               (w.Workflow.AttorneyFilter.Contains("|" + application.Invention.Attorney1ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + application.Invention.Attorney2ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + application.Invention.Attorney3ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + application.Invention.Attorney4ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + application.Invention.Attorney5ID.ToString() + "|")
                               ))).ToList();

            if (clearBase)
            {
                workflowActions = ClearPatBaseWorkflowActions(workflowActions);
            }
            return workflowActions;
        }

        public async Task<List<PatWorkflowAction>> GetInventionWorkflowActions(Invention invention, PatWorkflowTriggerType triggerType, bool clearBase = true)
        {
            var workflowActions = await _applicationService.CheckWorkflowAction(triggerType);
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || (w.Workflow.ClientFilter != null && w.Workflow.ClientFilter.Contains("|" + invention.ClientID.ToString() + "|"))).ToList();
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.RespOfficeFilter) || (w.Workflow.RespOfficeFilter != null && w.Workflow.RespOfficeFilter.Contains("|" + invention.RespOffice + "|"))).ToList();

            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.AttorneyFilter) || (w.Workflow.AttorneyFilter != null &&
                               (w.Workflow.AttorneyFilter.Contains("|" + invention.Attorney1ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + invention.Attorney2ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + invention.Attorney3ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + invention.Attorney4ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + invention.Attorney5ID.ToString() + "|")
                               ))).ToList();

            if (clearBase)
            {
                workflowActions = ClearPatBaseWorkflowActions(workflowActions);
            }
            return workflowActions;
        }

        public async Task<List<PatWorkflowAction>> GetPatActionDueWorkflowActions(PatActionDue actionDue, PatWorkflowTriggerType triggerType, bool clearBase = true)
        {
            var workflowActions = await _applicationService.CheckWorkflowAction(triggerType);
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || (w.Workflow.ClientFilter != null && w.Workflow.ClientFilter.Contains("|" + actionDue.CountryApplication.Invention.ClientID.ToString() + "|"))).ToList();

            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.CountryFilter) || (w.Workflow.CountryFilter != null && w.Workflow.CountryFilter.Contains("|" + actionDue.CountryApplication.Country + "|"))).ToList();
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.CaseTypeFilter) || (w.Workflow.CaseTypeFilter != null && w.Workflow.CaseTypeFilter.Contains("|" + actionDue.CountryApplication.CaseType + "|"))).ToList();
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.RespOfficeFilter) || (w.Workflow.RespOfficeFilter != null && w.Workflow.RespOfficeFilter.Contains("|" + actionDue.CountryApplication.RespOffice + "|"))).ToList();

            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.AttorneyFilter) || (w.Workflow.AttorneyFilter != null &&
                               (w.Workflow.AttorneyFilter.Contains("|" + actionDue.CountryApplication.Invention.Attorney1ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + actionDue.CountryApplication.Invention.Attorney2ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + actionDue.CountryApplication.Invention.Attorney3ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + actionDue.CountryApplication.Invention.Attorney4ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + actionDue.CountryApplication.Invention.Attorney5ID.ToString() + "|")
                               ))).ToList();

            if (clearBase)
            {
                workflowActions = ClearPatBaseWorkflowActions(workflowActions);
            }
            return workflowActions;
        }

        public async Task<List<PatWorkflowAction>> GetPatActionDueInvWorkflowActions(PatActionDueInv actionDue, PatWorkflowTriggerType triggerType, bool clearBase = true)
        {
            var workflowActions = await _applicationService.CheckWorkflowAction(triggerType);
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || (w.Workflow.ClientFilter != null && w.Workflow.ClientFilter.Contains("|" + actionDue.Invention.ClientID.ToString() + "|"))).ToList();

            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.RespOfficeFilter) || (w.Workflow.RespOfficeFilter != null && w.Workflow.RespOfficeFilter.Contains("|" + actionDue.Invention.RespOffice + "|"))).ToList();

            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.AttorneyFilter) || (w.Workflow.AttorneyFilter != null &&
                               (w.Workflow.AttorneyFilter.Contains("|" + actionDue.Invention.Attorney1ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + actionDue.Invention.Attorney2ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + actionDue.Invention.Attorney3ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + actionDue.Invention.Attorney4ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + actionDue.Invention.Attorney5ID.ToString() + "|")
                               ))).ToList();

            if (clearBase)
            {
                workflowActions = ClearPatBaseWorkflowActions(workflowActions);
            }
            return workflowActions;
        }

        public async Task<List<PatWorkflowAction>> GetPatCostTrackingWorkflowActions(PatCostTrack costTrack, PatWorkflowTriggerType triggerType, bool clearBase = true)
        {
            var workflowActions = await _applicationService.CheckWorkflowAction(triggerType);
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || (w.Workflow.ClientFilter != null && w.Workflow.ClientFilter.Contains("|" + costTrack.CountryApplication.Invention.ClientID.ToString() + "|"))).ToList();

            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.CountryFilter) || (w.Workflow.CountryFilter != null && w.Workflow.CountryFilter.Contains("|" + costTrack.CountryApplication.Country + "|"))).ToList();
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.CaseTypeFilter) || (w.Workflow.CaseTypeFilter != null && w.Workflow.CaseTypeFilter.Contains("|" + costTrack.CountryApplication.CaseType + "|"))).ToList();
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.RespOfficeFilter) || (w.Workflow.RespOfficeFilter != null && w.Workflow.RespOfficeFilter.Contains("|" + costTrack.CountryApplication.RespOffice + "|"))).ToList();

            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.AttorneyFilter) || (w.Workflow.AttorneyFilter != null &&
                               (w.Workflow.AttorneyFilter.Contains("|" + costTrack.CountryApplication.Invention.Attorney1ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + costTrack.CountryApplication.Invention.Attorney2ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + costTrack.CountryApplication.Invention.Attorney3ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + costTrack.CountryApplication.Invention.Attorney4ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + costTrack.CountryApplication.Invention.Attorney5ID.ToString() + "|")
                               ))).ToList();

            if (clearBase)
            {
                workflowActions = ClearPatBaseWorkflowActions(workflowActions);
            }
            return workflowActions;
        }

        public async Task<List<TmkWorkflowAction>> GetTrademarkWorkflowActions(TmkTrademark trademark, TmkWorkflowTriggerType triggerType, bool clearBase = true)
        {
            var workflowActions = await _trademarkService.CheckWorkflowAction(triggerType);
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || (w.Workflow.ClientFilter != null && w.Workflow.ClientFilter.Contains("|" + trademark.ClientID.ToString() + "|"))).ToList();

            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.CountryFilter) || (w.Workflow.CountryFilter != null && w.Workflow.CountryFilter.Contains("|" + trademark.Country + "|"))).ToList();
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.CaseTypeFilter) || (w.Workflow.CaseTypeFilter != null && w.Workflow.CaseTypeFilter.Contains("|" + trademark.CaseType + "|"))).ToList();
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.RespOfficeFilter) || (w.Workflow.RespOfficeFilter != null && w.Workflow.RespOfficeFilter.Contains("|" + trademark.RespOffice + "|"))).ToList();

            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.AttorneyFilter) || (w.Workflow.AttorneyFilter != null &&
                               (w.Workflow.AttorneyFilter.Contains("|" + trademark.Attorney1ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + trademark.Attorney2ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + trademark.Attorney3ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + trademark.Attorney4ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + trademark.Attorney5ID.ToString() + "|")
                               ))).ToList();

            if (clearBase)
            {
                workflowActions = ClearTmkBaseWorkflowActions(workflowActions);
            }
            return workflowActions;
        }

        public async Task<List<TmkWorkflowAction>> GetTmkActionDueWorkflowActions(TmkActionDue actionDue, TmkWorkflowTriggerType triggerType, bool clearBase = true)
        {
            var workflowActions = await _trademarkService.CheckWorkflowAction(triggerType);
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || (w.Workflow.ClientFilter != null && w.Workflow.ClientFilter.Contains("|" + actionDue.TmkTrademark.ClientID.ToString() + "|"))).ToList();

            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.CountryFilter) || (w.Workflow.CountryFilter != null && w.Workflow.CountryFilter.Contains("|" + actionDue.TmkTrademark.Country + "|"))).ToList();
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.CaseTypeFilter) || (w.Workflow.CaseTypeFilter != null && w.Workflow.CaseTypeFilter.Contains("|" + actionDue.TmkTrademark.CaseType + "|"))).ToList();
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.RespOfficeFilter) || (w.Workflow.RespOfficeFilter != null && w.Workflow.RespOfficeFilter.Contains("|" + actionDue.TmkTrademark.RespOffice + "|"))).ToList();

            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.AttorneyFilter) || (w.Workflow.AttorneyFilter != null &&
                               (w.Workflow.AttorneyFilter.Contains("|" + actionDue.TmkTrademark.Attorney1ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + actionDue.TmkTrademark.Attorney2ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + actionDue.TmkTrademark.Attorney3ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + actionDue.TmkTrademark.Attorney4ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + actionDue.TmkTrademark.Attorney5ID.ToString() + "|")
                               ))).ToList();

            if (clearBase)
            {
                workflowActions = ClearTmkBaseWorkflowActions(workflowActions);
            }
            return workflowActions;
        }

        public async Task<List<TmkWorkflowAction>> GetTmkCostTrackingWorkflowActions(TmkCostTrack costTrack, TmkWorkflowTriggerType triggerType, bool clearBase = true)
        {
            var workflowActions = await _trademarkService.CheckWorkflowAction(triggerType);
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || (w.Workflow.ClientFilter != null && w.Workflow.ClientFilter.Contains("|" + costTrack.TmkTrademark.ClientID.ToString() + "|"))).ToList();

            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.CountryFilter) || (w.Workflow.CountryFilter != null && w.Workflow.CountryFilter.Contains("|" + costTrack.TmkTrademark.Country + "|"))).ToList();
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.CaseTypeFilter) || (w.Workflow.CaseTypeFilter != null && w.Workflow.CaseTypeFilter.Contains("|" + costTrack.TmkTrademark.CaseType + "|"))).ToList();
            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.RespOfficeFilter) || (w.Workflow.RespOfficeFilter != null && w.Workflow.RespOfficeFilter.Contains("|" + costTrack.TmkTrademark.RespOffice + "|"))).ToList();

            workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.AttorneyFilter) || (w.Workflow.AttorneyFilter != null &&
                               (w.Workflow.AttorneyFilter.Contains("|" + costTrack.TmkTrademark.Attorney1ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + costTrack.TmkTrademark.Attorney2ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + costTrack.TmkTrademark.Attorney3ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + costTrack.TmkTrademark.Attorney4ID.ToString() + "|") ||
                                w.Workflow.AttorneyFilter.Contains("|" + costTrack.TmkTrademark.Attorney5ID.ToString() + "|")
                               ))).ToList();

            if (clearBase)
            {
                workflowActions = ClearTmkBaseWorkflowActions(workflowActions);
            }
            return workflowActions;
        }

        // Removed during deep clean - GM module removed
        // public async Task<List<GMWorkflowAction>> GetGeneralMatterWorkflowActions(GMMatter gm, GMWorkflowTriggerType triggerType, bool clearBase = true)
        // {
        //     var workflowActions = await _gmMatterService.CheckWorkflowAction(triggerType);
        //     workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || (w.Workflow.ClientFilter != null && w.Workflow.ClientFilter.Contains("|" + gm.ClientID.ToString() + "|"))).ToList();
        //     workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.MatterTypeFilter) || (w.Workflow.MatterTypeFilter != null && w.Workflow.MatterTypeFilter.Contains("|" + gm.MatterType + "|"))).ToList();
        //     workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.RespOfficeFilter) || (w.Workflow.RespOfficeFilter != null && w.Workflow.RespOfficeFilter.Contains("|" + gm.RespOffice + "|"))).ToList();
        //
        //     if (workflowActions.Any(w => !string.IsNullOrEmpty(w.Workflow.AttorneyFilter)))
        //     {
        //         var matterAttorneys = await _matterAttorneyService.QueryableList.Where(a => a.MatId == gm.MatId).ToListAsync();
        //         workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.AttorneyFilter) || (w.Workflow.AttorneyFilter != null && matterAttorneys.Any(a => w.Workflow.AttorneyFilter.Contains("|" + a.AttorneyID.ToString() + "|")))).ToList();
        //     }
        //
        //     if (clearBase)
        //     {
        //         workflowActions = ClearGMBaseWorkflowActions(workflowActions);
        //     }
        //     return workflowActions;
        // }

        // Removed during deep clean - GM module removed
        // public async Task<List<GMWorkflowAction>> GetGMActionDueWorkflowActions(GMActionDue actionDue, GMWorkflowTriggerType triggerType, bool clearBase = true)
        // {
        //     var workflowActions = await _gmMatterService.CheckWorkflowAction(triggerType);
        //     workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || (w.Workflow.ClientFilter != null && w.Workflow.ClientFilter.Contains("|" + actionDue.GMMatter.ClientID.ToString() + "|"))).ToList();
        //     workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.MatterTypeFilter) || (w.Workflow.MatterTypeFilter != null && w.Workflow.MatterTypeFilter.Contains("|" + actionDue.GMMatter.MatterType + "|"))).ToList();
        //     workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.RespOfficeFilter) || (w.Workflow.RespOfficeFilter != null && w.Workflow.RespOfficeFilter.Contains("|" + actionDue.GMMatter.RespOffice + "|"))).ToList();
        //
        //     if (workflowActions.Any(w => !string.IsNullOrEmpty(w.Workflow.AttorneyFilter)))
        //     {
        //         var matterAttorneys = await _matterAttorneyService.QueryableList.Where(a => a.MatId == actionDue.GMMatter.MatId).ToListAsync();
        //         workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.AttorneyFilter) || (w.Workflow.AttorneyFilter != null && matterAttorneys.Any(a => w.Workflow.AttorneyFilter.Contains("|" + a.AttorneyID.ToString() + "|")))).ToList();
        //     }
        //
        //     if (clearBase)
        //     {
        //         workflowActions = ClearGMBaseWorkflowActions(workflowActions);
        //     }
        //     return workflowActions;
        // }

        // Removed during deep clean - GM module removed
        // public async Task<List<GMWorkflowAction>> GetGMCostTrackingWorkflowActions(GMCostTrack costTrack, GMWorkflowTriggerType triggerType, bool clearBase = true)
        // {
        //     var workflowActions = await _gmMatterService.CheckWorkflowAction(triggerType);
        //     workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || (w.Workflow.ClientFilter != null && w.Workflow.ClientFilter.Contains("|" + costTrack.GMMatter.ClientID.ToString() + "|"))).ToList();
        //     workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.MatterTypeFilter) || (w.Workflow.MatterTypeFilter != null && w.Workflow.MatterTypeFilter.Contains("|" + costTrack.GMMatter.MatterType + "|"))).ToList();
        //     workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.RespOfficeFilter) || (w.Workflow.RespOfficeFilter != null && w.Workflow.RespOfficeFilter.Contains("|" + costTrack.GMMatter.RespOffice + "|"))).ToList();
        //
        //     if (workflowActions.Any(w => !string.IsNullOrEmpty(w.Workflow.AttorneyFilter)))
        //     {
        //         var matterAttorneys = await _matterAttorneyService.QueryableList.Where(a => a.MatId == costTrack.GMMatter.MatId).ToListAsync();
        //         workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.AttorneyFilter) || (w.Workflow.AttorneyFilter != null && matterAttorneys.Any(a => w.Workflow.AttorneyFilter.Contains("|" + a.AttorneyID.ToString() + "|")))).ToList();
        //     }
        //
        //     if (clearBase)
        //     {
        //         workflowActions = ClearGMBaseWorkflowActions(workflowActions);
        //     }
        //     return workflowActions;
        // }

        public List<PatWorkflowAction> ClearPatBaseWorkflowActions(List<PatWorkflowAction> workflowActions)
        {

            //with filter will override the record with no filter at all
            foreach (var item in workflowActions.Where(wf => !(string.IsNullOrEmpty(wf.Workflow.ClientFilter) && string.IsNullOrEmpty(wf.Workflow.CountryFilter) && string.IsNullOrEmpty(wf.Workflow.CaseTypeFilter)
                                                              && string.IsNullOrEmpty(wf.Workflow.RespOfficeFilter) && string.IsNullOrEmpty(wf.Workflow.AttorneyFilter))).ToList())
            {
                workflowActions.RemoveAll(bf => string.IsNullOrEmpty(bf.Workflow.ClientFilter) && string.IsNullOrEmpty(bf.Workflow.CountryFilter) && string.IsNullOrEmpty(bf.Workflow.CaseTypeFilter)
                                                              && string.IsNullOrEmpty(bf.Workflow.RespOfficeFilter) && string.IsNullOrEmpty(bf.Workflow.AttorneyFilter) && bf.ActionTypeId == item.ActionTypeId && bf.Workflow.TriggerValueId == item.Workflow.TriggerValueId && (bf.Workflow.TriggerValueName ?? "") == (item.Workflow.TriggerValueName ?? ""));
            }
            return workflowActions;
        }

        public List<TmkWorkflowAction> ClearTmkBaseWorkflowActions(List<TmkWorkflowAction> workflowActions)
        {

            //with filter will override the record with no filter at all
            foreach (var item in workflowActions.Where(wf => !(string.IsNullOrEmpty(wf.Workflow.ClientFilter) && string.IsNullOrEmpty(wf.Workflow.CountryFilter) && string.IsNullOrEmpty(wf.Workflow.CaseTypeFilter)
                                                              && string.IsNullOrEmpty(wf.Workflow.RespOfficeFilter) && string.IsNullOrEmpty(wf.Workflow.AttorneyFilter))).ToList())
            {
                workflowActions.RemoveAll(bf => string.IsNullOrEmpty(bf.Workflow.ClientFilter) && string.IsNullOrEmpty(bf.Workflow.CountryFilter) && string.IsNullOrEmpty(bf.Workflow.CaseTypeFilter)
                                                              && string.IsNullOrEmpty(bf.Workflow.RespOfficeFilter) && string.IsNullOrEmpty(bf.Workflow.AttorneyFilter) && bf.ActionTypeId == item.ActionTypeId && bf.Workflow.TriggerValueId == item.Workflow.TriggerValueId && (bf.Workflow.TriggerValueName ?? "") == (item.Workflow.TriggerValueName ?? ""));
            }
            return workflowActions;
        }

        // Removed during deep clean - GM module removed
        // public List<GMWorkflowAction> ClearGMBaseWorkflowActions(List<GMWorkflowAction> workflowActions)
        // {
        //     //with filter will override the record with no filter at all
        //     foreach (var item in workflowActions.Where(wf => !(string.IsNullOrEmpty(wf.Workflow.ClientFilter) && string.IsNullOrEmpty(wf.Workflow.MatterTypeFilter)
        //                                                       && string.IsNullOrEmpty(wf.Workflow.RespOfficeFilter) && string.IsNullOrEmpty(wf.Workflow.AttorneyFilter))).ToList())
        //     {
        //         workflowActions.RemoveAll(bf => string.IsNullOrEmpty(bf.Workflow.ClientFilter) && string.IsNullOrEmpty(bf.Workflow.MatterTypeFilter)
        //                                                       && string.IsNullOrEmpty(bf.Workflow.RespOfficeFilter) && string.IsNullOrEmpty(bf.Workflow.AttorneyFilter) && bf.ActionTypeId == item.ActionTypeId && bf.Workflow.TriggerValueId == item.Workflow.TriggerValueId && (bf.Workflow.TriggerValueName ?? "") == (item.Workflow.TriggerValueName ?? ""));
        //     }
        //     return workflowActions;
        // }

        // Removed during deep clean - Clearance module removed
        // public async Task<List<TmcWorkflowAction>> GetSearchRequestWorkflowActions(TmcClearance clearance, TmcWorkflowTriggerType triggerType, bool clearBase = true)
        // {
        //     var workflowActions = await _tmcClearanceService.CheckWorkflowAction(triggerType);
        //     return workflowActions;
        // }

        // Removed during deep clean - PatClearance module removed
        // public async Task<List<PacWorkflowAction>> GetPatClearanceWorkflowActions(PacClearance pacClearance, PacWorkflowTriggerType triggerType, bool clearBase = true)
        // {
        //     var workflowActions = await _pacClearanceService.CheckWorkflowAction(triggerType);
        //     return workflowActions;
        // }

        // Removed during deep clean - DMS/Disclosure module removed
        // public async Task<List<DMSWorkflowAction>> GetDisclosureWorkflowActions(Disclosure disclosure, DMSWorkflowTriggerType triggerType, bool clearBase = true)
        // {
        //     var workflowActions = await _disclosureService.CheckWorkflowAction(triggerType);
        //     workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.DMSWorkflow!.ClientFilter) || (w.DMSWorkflow.ClientFilter != null && w.DMSWorkflow.ClientFilter.Contains("|" + disclosure.ClientID.ToString() + "|"))).ToList();
        //
        //     if (clearBase)
        //     {
        //         workflowActions = ClearDMSBaseWorkflowActions(workflowActions);
        //     }
        //     return workflowActions;
        // }

        // Removed during deep clean - DMS/Disclosure module removed
        // public List<DMSWorkflowAction> ClearDMSBaseWorkflowActions(List<DMSWorkflowAction> workflowActions)
        // {
        //     //with filter will override the record with no filter at all
        //     foreach (var item in workflowActions.Where(wf => wf.DMSWorkflow != null && !(string.IsNullOrEmpty(wf.DMSWorkflow.ClientFilter) && string.IsNullOrEmpty(wf.DMSWorkflow.ReviewerEntityFilter))).ToList())
        //     {
        //         workflowActions.RemoveAll(bf => bf.DMSWorkflow != null && string.IsNullOrEmpty(bf.DMSWorkflow.ClientFilter) && string.IsNullOrEmpty(bf.DMSWorkflow.ReviewerEntityFilter) && bf.ActionTypeId == item.ActionTypeId && bf.DMSWorkflow.TriggerValueId == item.DMSWorkflow!.TriggerValueId);
        //     }
        //     return workflowActions;
        // }
    }
}
