using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using R10.Core.Queries.Shared;
using R10.Web.Areas;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Interfaces
{
    public interface IOuickDocketViewModelService
    {
        QuickDocketSearchCriteriaViewModel GetSearchCriteria(QuickDocketDefaultSettingsViewModel defaultSettings);
        Task<List<QuickDocketDTO>> GetQuickDocket(QuickDocketSearchCriteriaViewModel criteria);
        List<QuickDocketSchedulerViewModel> GetQuickDocketScheduler(QuickDocketSearchCriteriaViewModel criteria);
        QuickDocketPrintViewModel GetQuickDocketSearchCriteria(QuickDocketSearchCriteriaViewModel searchCriteriaViewModel);

        Task<List<QDActionTypeLookupDTO>> GetCombinedActionTypes(string systemType, string text);
        Task<List<QDActionDueLookupDTO>> GetCombinedActionDues(string systemType, string text);
        Task<List<QDCaseNumberLookupDTO>> GetCombinedCaseNumbers(string systemType, string text);
        Task<List<QDCaseTypeLookupDTO>> GetCombinedCaseTypes(string systemType, string text);
        Task<List<QDRespOfficeLookupDTO>> GetCombinedRespOffices(string systemType, string text);
        Task<List<QDClientRefLookupDTO>> GetCombinedClientRefs(string systemType, string text);
        Task<List<QDDeDocketInstructionLookupDTO>> GetCombinedDeDocketInstructions(string systemType, string text);
        Task<List<QDDeDocketInstructedByLookupDTO>> GetCombinedDeDocketInstructedBy(string systemType, string text);
        Task<List<QDStatusLookupDTO>> GetCombinedStatuses(string systemType, string text);
        Task<List<QDTitleLookupDTO>> GetCombinedTitles(string systemType, string text);
        Task<List<QDIndicatorLookupDTO>> GetCombinedIndicators(string systemType, string text);
        Task<List<QDCountryLookupDTO>> GetCombinedCountries(string systemType, string text);

        Task<List<QDActionTypeLookupDTO>> GetCombinedDefaultActionTypes(string systemType, string text);
        Task<List<QDActionDueLookupDTO>> GetCombinedDefaultActionDues(string systemType, string text);
        Task<List<QDCaseNumberLookupDTO>> GetCombinedDefaultCaseNumbers(string systemType, string text);
        Task<List<QDCaseTypeLookupDTO>> GetCombinedDefaultCaseTypes(string systemType, string text);
        Task<List<QDRespOfficeLookupDTO>> GetCombinedDefaultRespOffices(string systemType, string text);
        Task<List<QDClientRefLookupDTO>> GetCombinedDefaultClientRefs(string systemType, string text);
        Task<List<QDStatusLookupDTO>> GetCombinedDefaultStatuses(string systemType, string text);
        Task<List<QDTitleLookupDTO>> GetCombinedDefaultTitles(string systemType, string text);
        Task<List<QDIndicatorLookupDTO>> GetCombinedDefaultIndicators(string systemType, string text);
        Task<List<QDCountryLookupDTO>> GetCombinedDefaultCountries(string systemType, string text);

        Task<List<QDClientLookupDTO>> GetClientList(string systemType, string text);
        Task<List<QDAgentLookupDTO>> GetAgentList(string systemType, string text);
        Task<List<QDOwnerLookupDTO>> GetOwnerList(string systemType, string text);
        Task<List<QDAttorneyLookupDTO>> GetAttorneyList(string systemType, string text);

        Task UpdateQuickDocket(QuickDocketSearchCriteriaViewModel viewModel,string dateType, DateTime? specificDate, string updatedBy, List<string> recIds);
        Task<List<QuickDocketDeDocketBatchUpdateResultDTO>> UpdateQuickDocketDedocketInstruction(QuickDocketSearchCriteriaViewModel viewModel, string instruction, string? remarks, bool emptyInstructionOnly, string updatedBy,string userId, List<string> recIds);

    }
}

 