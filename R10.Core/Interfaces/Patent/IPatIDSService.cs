using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities.Documents;

namespace R10.Core.Interfaces.Patent
{
    public interface IPatIDSService
    {
        #region IDS RelatedCases
        Task SaveIDSInfo(PatIDSRelatedCasesInfo idsInfo);
        Task<List<PatIDSRelatedCase>> GetIDSRelatedCases(int appId);
        Task<List<PatIDSRelatedCase>> GetIDSRelatedCasesForStandardization();
        Task StandardizeIDSRelatedCases(List<PatIDSRelatedCase> relatedCases, bool saveResult = true);
        Task<PatIDSRelatedCaseDTO> GetIDSRelatedCase(int relatedCasesId, int appId);
        Task<List<CaseListDTO>> GetApplications(int appId, string caseNumber);
        Task<PatIDSRelatedCase> GetIDSApplicationInfo(int appId);
        Task<List<LookupDTO>> GetCopyRefCaseNumberList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyRefCountryList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyRefSubCaseList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyRefKeywordList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyRefInventorList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyRefArtUnitList(int excludeAppId);
        Task<List<PatIDSCopyFamilyDTO>> GetCopyToFamilyList(int appId, int relatedCasesId, string relatedBy);
        Task CopyIDSRelatedCasesToFamily(PatIDSCopyFamilyActionDTO selection, string userId);

        Task<List<PatIDSRelatedCaseCopyDTO>> GetCopyIDSReferencesSources(string? caseNumber, string? country, string? subCase,
            string? inventor, string? keyword, string? artUnit, bool activeOnly, int excludeAppId);

        IQueryable<PatRelatedCaseDTO> GetCopyRelatedCasesSources(string? caseNumber, string? country, string? subCase,
           string? inventor, string? keyword, string? artUnit, bool activeOnly, int excludeAppId);
        Task CopyIDSRelatedCases(int[] to, RTSIDSCrossCheckCopyDTO[] from, int[] fromRelated, string userId);
        Task CopyIDSRelatedCases(RTSIDSCrossCheckCopyDTO[] copyInfo, int[] actionInfo, string userId);
        Task IDSRelatedCasesDelete(PatIDSRelatedCase deletedIdsRelatedCase);
        Task IDSRelatedCasesUpdate(PatIDSRelatedCase idsRelatedCase);
        Task IDSRelatedCasesUpdate(int appId, string userName, IEnumerable<PatIDSRelatedCase> updatedRelatedCases);
        Task IDSRelatedCasesSave(int appId, string userName, IEnumerable<PatIDSRelatedCase> relatedCases);
        Task SaveStandardizedReferences(List<PatIDSRelatedCase> relatedCases);
        IQueryable<PatIDSRelatedCasesInfo> IDSRelatedCasesInfos { get; }
        IQueryable<PatIDSRelatedCaseDTO> IDSRelatedCasesDTO { get; }
        IQueryable<PatIDSRelatedCase> IDSRelatedCases { get;  }
        
        #endregion
        #region IDS NonPatLiterature
        Task<List<PatIDSNonPatLiterature>> GetNonPatLiteratures(int appId);
        Task<PatIDSNonPatLiterature> GetNonPatLiterature(int nonPatLiteratureId);
        Task NonPatLiteratureDelete(PatIDSNonPatLiterature deletedIdsNonPatLiterature);
        Task NonPatLiteratureUpdate(PatIDSNonPatLiterature idsNonPatLiterature);
        Task NonPatLiteratureUpdate(int appId, string userName, IEnumerable<PatIDSNonPatLiterature> updatedNonPatLiteratures, IEnumerable<PatIDSNonPatLiterature> newNonPatLiteratures);
        Task<List<LookupDTO>> GetCopyNonPatCaseNumberList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyNonPatCountryList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyNonPatSubCaseList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyNonPatKeywordList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyNonPatInventorList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyNonPatArtUnitList(int excludeAppId);
        IQueryable<PatIDSNonPatLiterature> GetCopyNonPatLiteratureSources(string? caseNumber, string? country,
            string? subCase, string? inventor, string? keyword, string? artUnit, string? searchText, int excludeAppId);
        Task CopyNonPatLiteratures(int appId, int[] from, string userId);
        IQueryable<PatIDSNonPatLiterature> IDSNonPatLiteratures { get; }
        IQueryable<DocFile> DocFiles { get; }
        #endregion

        Task<List<PatIDSSearchInputDTO>> GetIDSDownloadList(int maxAttempts, string? appIds = "");
        Task SaveIDSRelatedCaseDocs(List<PatIDSRelatedCase> patIDSRelatedCases);
        Task <IDSTotalDTO> GetIDSTotal(int appId);
        Task<int> GetIDSReferencesTotal(int appId);

        Task<bool> HasReferencesInStaging();
        Task LoadReferencesFromStaging();
    }
}
