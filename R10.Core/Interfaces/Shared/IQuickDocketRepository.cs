using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Queries.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IQuickDocketRepository 
    {
        Task<List<QuickDocketDTO>> GetQuickDocket(QuickDocketSearchCriteriaDTO criteria);

        IQueryable<QuickDocketSchedulerDTO> GetQuickDocketScheduler(QuickDocketSearchCriteriaDTO criteria);

        //Task<List<QuickDocketLookUpDTO>> GetPickListData(int action, string text, string systemType);

        Task<List<QDActionTypeLookupDTO>> CombinedActionTypes(string systemType);
        Task<List<QDActionDueLookupDTO>> CombinedActionDues(string systemType);
        Task<List<QDCaseNumberLookupDTO>> CombinedCaseNumbers(string systemType);
        Task<List<QDCaseTypeLookupDTO>> CombinedCaseTypes(string systemType);
        Task<List<QDRespOfficeLookupDTO>> CombinedRespOffices(string systemType);
        Task<List<QDClientRefLookupDTO>> CombinedClientRefs(string systemType);
        Task<List<QDDeDocketInstructionLookupDTO>> CombinedDeDocketInstructions(string systemType);
        Task<List<QDDeDocketInstructedByLookupDTO>> CombinedDeDocketInstructedBy(string systemType);
        Task<List<QDStatusLookupDTO>> CombinedStatuses(string systemType);
        Task<List<QDTitleLookupDTO>> CombinedTitles(string systemType);
        Task<List<QDIndicatorLookupDTO>> CombinedIndicators(string systemType);
        Task<List<QDCountryLookupDTO>> CombinedCountries(string systemType);

        Task<List<QDActionTypeLookupDTO>> CombinedDefaultActionTypes(string systemType);
        Task<List<QDActionDueLookupDTO>> CombinedDefaultActionDues(string systemType);
        Task<List<QDCaseNumberLookupDTO>> CombinedDefaultCaseNumbers(string systemType);
        Task<List<QDCaseTypeLookupDTO>> CombinedDefaultCaseTypes(string systemType);
        Task<List<QDRespOfficeLookupDTO>> CombinedDefaultRespOffices(string systemType);
        Task<List<QDClientRefLookupDTO>> CombinedDefaultClientRefs(string systemType);
        Task<List<QDStatusLookupDTO>> CombinedDefaultStatuses(string systemType);
        Task<List<QDTitleLookupDTO>> CombinedDefaultTitles(string systemType);
        Task<List<QDIndicatorLookupDTO>> CombinedDefaultIndicators(string systemType);

        Task<List<QDCountryLookupDTO>> CombinedDefaultCountries(string systemType);
        Task<List<QDClientLookupDTO>> GetClientList(string systemType);
        Task<List<QDAgentLookupDTO>> GetAgentList(string systemType);
        Task<List<QDOwnerLookupDTO>> GetOwnerList(string systemType);
        Task<List<QDAttorneyLookupDTO>> GetAttorneyList(string systemType);

        Task UpdateQuickDocket(QuickDocketUpdateCriteriaDTO criteria);
        Task<List<QuickDocketDeDocketBatchUpdateResultDTO>> UpdateQuickDocketDeDocketBatch(QuickDocketUpdateCriteriaDTO criteria);

    }
}
