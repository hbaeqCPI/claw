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
    public interface IPatIDSRepository
    {
        #region IDS IDSRelatedCasesDTO
        Task SaveIDSInfo(PatIDSRelatedCasesInfo idsInfo);
        Task<List<PatIDSRelatedCase>> GetIDSRelatedCases(int appId);
        Task<List<PatIDSRelatedCase>> GetIDSRelatedCasesForStandardization();
        Task<PatIDSRelatedCase> GetIDSRelatedCase(int relatedCasesId);
        Task<List<CaseListDTO>> GetApplications(int appId, string caseNumber);
        Task<PatIDSRelatedCase> GetIDSApplicationInfo(int appId);
        Task IDSRelatedCasesDelete(PatIDSRelatedCase deletedIdsRelatedCase);
        Task IDSRelatedCasesUpdate(PatIDSRelatedCase idsRelatedCase);
        Task IDSRelatedCasesUpdate(int appId, string userName, IEnumerable<PatIDSRelatedCase> updatedRelatedCases);
        Task IDSRelatedCasesSave(int appId, string userName, IEnumerable<PatIDSRelatedCase> relatedCases);
        Task<List<LookupDTO>> GetCopyRefCaseNumberList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyRefCountryList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyRefSubCaseList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyRefKeywordList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyRefInventorList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyRefArtUnitList(int excludeAppId);
        Task<List<PatIDSCopyFamilyDTO>> GetCopyToFamilyList(int appId, int relatedCasesId, string relatedBy);
        Task CopyIDSRelatedCasesToFamily(PatIDSCopyFamilyActionDTO selection, string userId);
        Task CopyIDSNonPatLiteratureToFamily(PatIDSCopyFamilyActionDTO selection, string userId);

        Task CopyIDSRelatedCases(int[] to, RTSIDSCrossCheckCopyDTO[] from, int[] fromRelated, string userId);
        Task CopyIDSRelatedCases(RTSIDSCrossCheckCopyDTO[] copyInfo, int[] actionInfo, string userId);
        Task SaveStandardizedReferences(List<PatIDSRelatedCase> relatedCases);
        IQueryable<PatIDSRelatedCaseDTO> IDSRelatedCasesDTO { get; }
        IQueryable<PatIDSRelatedCase> IDSRelatedCases { get; }
        IQueryable<PatIDSRelatedCasesInfo> IDSRelatedCasesInfos { get; }
        IQueryable<PatIDSRelatedCaseCopyDTO> PatIDSRelatedCasesCopyDTO { get; }
        IQueryable<PatRelatedCase> RelatedCases { get; }

        #endregion
        #region IDS NonPatLiterature
        Task<List<PatIDSNonPatLiterature>> GetNonPatLiteratures(int appId);
        Task<PatIDSNonPatLiterature> GetNonPatLiterature(int nonPatLiteratureId);
        Task<List<LookupDTO>> GetCopyNonPatCaseNumberList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyNonPatCountryList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyNonPatSubCaseList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyNonPatKeywordList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyNonPatInventorList(int excludeAppId);
        Task<List<LookupDTO>> GetCopyNonPatArtUnitList(int excludeAppId);
        IQueryable<PatIDSNonPatLiterature> NonPatLiteratures { get; }
        Task NonPatLiteratureDelete(PatIDSNonPatLiterature deletedIdsNonPatLiterature);
        Task NonPatLiteratureUpdate(PatIDSNonPatLiterature idsNonPatLiterature);
        Task NonPatLiteratureUpdate(int appId, string userName,
            IEnumerable<PatIDSNonPatLiterature> updatedNonPatLiteratures, IEnumerable<PatIDSNonPatLiterature> newNonPatLiteratures);
        Task CopyNonPatLiteratures(int appId, int[] from, string userId);
        IQueryable<DocFile> DocFiles { get; }
        #endregion

        Task<List<PatIDSSearchInputDTO>> GetIDSDownloadList(int maxAttempts, string? appIds = "");
        Task SaveIDSRelatedCaseDocs(List<PatIDSRelatedCase> patIDSRelatedCases);
        Task<IDSTotalDTO> GetIDSTotal(int appId);
        Task<int> GetIDSReferencesTotal(int appId);

        Task<bool> HasReferencesInStaging();
        Task LoadReferencesFromStaging();
    }
}
