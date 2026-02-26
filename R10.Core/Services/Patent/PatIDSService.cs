using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Documents;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Core.Services.Shared;
using R10.Core.Interfaces.Shared;

namespace R10.Core.Services
{
    public class PatIDSService : IPatIDSService
    {
        private readonly IPatIDSRepository _repository;
        private readonly ICountryApplicationService _countryApplicationService;
        private readonly INumberFormatService _numberFormatService;

        public PatIDSService(IPatIDSRepository repository,
                             ICountryApplicationService countryApplicationService,
                             INumberFormatService numberFormatService)
        {
            _repository = repository;
            _countryApplicationService = countryApplicationService;
            _numberFormatService = numberFormatService;
        }

        public IQueryable<PatIDSRelatedCasesInfo> IDSRelatedCasesInfos => _repository.IDSRelatedCasesInfos;
        public IQueryable<PatIDSRelatedCaseDTO> IDSRelatedCasesDTO => _repository.IDSRelatedCasesDTO;
        public IQueryable<PatIDSRelatedCase> IDSRelatedCases => _repository.IDSRelatedCases;
        public IQueryable<PatIDSNonPatLiterature> IDSNonPatLiteratures => _repository.NonPatLiteratures;
        public IQueryable<DocFile> DocFiles => _repository.DocFiles;


        #region IDS RelatedCases

        public async Task SaveIDSInfo(PatIDSRelatedCasesInfo idsInfo)
        {
            await _repository.SaveIDSInfo(idsInfo);
        }

        public async Task<List<PatIDSRelatedCase>> GetIDSRelatedCases(int appId)
        {
            return await _repository.GetIDSRelatedCases(appId);
        }

        public async Task<List<PatIDSRelatedCase>> GetIDSRelatedCasesForStandardization() {
            return await _repository.GetIDSRelatedCasesForStandardization();
        }

        public async Task StandardizeIDSRelatedCases(List<PatIDSRelatedCase> relatedCases,bool saveResult=true)
        {
            foreach (var item in relatedCases)
            {
                var numberInfo = new WebLinksNumberInfoDTO
                {
                    Country = item.RelatedCountry,
                    SystemType = "P"
                };
                if (!string.IsNullOrEmpty(item.RelatedPubNumber))
                {
                    numberInfo.PubNumber = item.RelatedPubNumber;
                    numberInfo.NumberType = WebLinksNumberType.PubNo;
                    numberInfo.NumberDate = item.RelatedPubDate;
                    numberInfo.Number = item.RelatedPubNumber;

                    var parsedInfo = await _numberFormatService.StandardizeNumber(numberInfo);
                    if (parsedInfo != null)
                    {
                        var parsedYear = parsedInfo.Year;
                        if (!String.IsNullOrEmpty(parsedYear) && parsedYear.Length==2) {

                            if (item.RelatedPubDate != null)
                            {
                                parsedYear = item.RelatedPubDate.Value.Year.ToString();
                            }
                            else {
                                int.TryParse(parsedYear, out int yr);
                                if (yr > 0)
                                {
                                    if (yr <= 35)
                                       parsedYear = $"20{parsedYear}";
                                    else
                                        parsedYear = $"19{parsedYear}";
                                }
                            }
                        }

                        item.RelatedPubNumberStandard = parsedInfo.Number;
                        item.RelatedPubNumberStandardYear = parsedYear;

                    }
                    else
                    {
                        item.RelatedPubNumberStandard = numberInfo.PubNumber;
                    }
                }

                //possible that both pubno and patno are entered using the import routine, should not be right, but still handle it here
                if (!string.IsNullOrEmpty(item.RelatedPatNumber))
                {
                    numberInfo.PatRegNumber = item.RelatedPatNumber;
                    numberInfo.NumberType = WebLinksNumberType.PatRegNo;
                    numberInfo.NumberDate = item.RelatedIssDate;
                    numberInfo.Number = item.RelatedPatNumber;

                    var parsedInfo = await _numberFormatService.StandardizeNumber(numberInfo);
                    if (parsedInfo != null)
                    {
                        item.RelatedPatNumberStandard = parsedInfo.Number;
                    }
                    else
                    {
                        item.RelatedPatNumberStandard = numberInfo.PatRegNumber;
                    }
                }
            }
            if (relatedCases.Any() && saveResult)
            {
                await SaveStandardizedReferences(relatedCases);
            }
        }

        public async Task<PatIDSRelatedCaseDTO> GetIDSRelatedCase(int relatedCasesId, int appId)
        {
            //return await _repository.GetIDSRelatedCase(relatedCasesId);
            return await IDSRelatedCasesDTO.Where(r => r.RelatedCasesId == relatedCasesId && r.AppIdConnect == appId).FirstOrDefaultAsync();
        }

        public async Task<List<CaseListDTO>> GetApplications(int appId, string caseNumber)
        {
            return await _repository.GetApplications(appId, caseNumber);
        }

        public async Task<PatIDSRelatedCase> GetIDSApplicationInfo(int appId)
        {
            return await _repository.GetIDSApplicationInfo(appId);
        }

        public async Task IDSRelatedCasesDelete(PatIDSRelatedCase deletedIdsRelatedCase)
        {
            await _repository.IDSRelatedCasesDelete(deletedIdsRelatedCase);
        }

        public async Task IDSRelatedCasesUpdate(PatIDSRelatedCase idsRelatedCase)
        {
            idsRelatedCase.RelatedPubNumber = idsRelatedCase.RelatedPubNumber ?? "";
            idsRelatedCase.RelatedPatNumber = idsRelatedCase.RelatedPatNumber ?? "";
            await _repository.IDSRelatedCasesUpdate(idsRelatedCase);
        }

        public async Task IDSRelatedCasesUpdate(int appId, string userName,
            IEnumerable<PatIDSRelatedCase> updatedRelatedCases)
        {
            await _repository.IDSRelatedCasesUpdate(appId, userName, updatedRelatedCases);
        }

        public async Task IDSRelatedCasesSave(int appId, string userName,
            IEnumerable<PatIDSRelatedCase> relatedCases)
        {
            foreach (var item in relatedCases)
            {
                if (item.RelatedCasesId < 0)
                    item.RelatedCasesId = 0;

                //if(!item.CopyToFamily)
                //    item.SourceRelatedCasesId = 0;
            }
            await _repository.IDSRelatedCasesSave(appId, userName, relatedCases);
        }

        public async Task<List<LookupDTO>> GetCopyRefCaseNumberList(int excludeAppId)
        {
            return await _repository.GetCopyRefCaseNumberList(excludeAppId);
        }

        public async Task<List<LookupDTO>> GetCopyRefCountryList(int excludeAppId)
        {
            return await _repository.GetCopyRefCountryList(excludeAppId);
        }

        public async Task<List<LookupDTO>> GetCopyRefSubCaseList(int excludeAppId)
        {
            return await _repository.GetCopyRefSubCaseList(excludeAppId);
        }

        public async Task<List<LookupDTO>> GetCopyRefKeywordList(int excludeAppId)
        {
            return await _repository.GetCopyRefKeywordList(excludeAppId);
        }

        public async Task<List<LookupDTO>> GetCopyRefInventorList(int excludeAppId)
        {
            return await _repository.GetCopyRefInventorList(excludeAppId);
        }

        public async Task<List<LookupDTO>> GetCopyRefArtUnitList(int excludeAppId)
        {
            return await _repository.GetCopyRefArtUnitList(excludeAppId);
        }

        public async Task<List<PatIDSCopyFamilyDTO>> GetCopyToFamilyList(int appId, int relatedCasesId, string relatedBy)
        {
            return await _repository.GetCopyToFamilyList(appId, relatedCasesId, relatedBy);
        }

        public async Task CopyIDSRelatedCasesToFamily(PatIDSCopyFamilyActionDTO selection, string userId)
        {
            if (string.IsNullOrEmpty(selection.RecordType) || selection.RecordType == "R")
                await _repository.CopyIDSRelatedCasesToFamily(selection, userId);
            else
                await _repository.CopyIDSNonPatLiteratureToFamily(selection, userId);
        }

        public async Task<List<PatIDSRelatedCaseCopyDTO>> GetCopyIDSReferencesSources(string? caseNumber, string? country, string? subCase, string? inventor, string? keyword, string? artUnit, bool activeOnly, int excludeAppId)
        {
            var relatedCases = _repository.IDSRelatedCasesDTO;

            if (!string.IsNullOrEmpty(caseNumber))
                relatedCases = relatedCases.Where(r => r.CountryApplication.CaseNumber == caseNumber);

            if (!string.IsNullOrEmpty(country))
                relatedCases = relatedCases.Where(r => r.CountryApplication.Country == country);

            if (!string.IsNullOrEmpty(subCase))
                relatedCases = relatedCases.Where(r => r.CountryApplication.SubCase == subCase);

            if (activeOnly)
                relatedCases = relatedCases.Where(r => r.CountryApplication.PatApplicationStatus.ActiveSwitch);

            if (!string.IsNullOrEmpty(keyword))
                relatedCases = relatedCases.Where(r => r.CountryApplication.Invention.Keywords.Any(k => k.Keyword == keyword));

            if (!string.IsNullOrEmpty(inventor))
                relatedCases = relatedCases.Where(r => r.CountryApplication.Inventors.Any(i => i.InventorAppInventor.Inventor == inventor));

            if (!string.IsNullOrEmpty(artUnit))
                relatedCases = relatedCases.Where(r => r.CountryApplication.IDSRelatedCasesInfo.GroupArtUnit == artUnit);

            if (excludeAppId > 0)
                relatedCases = relatedCases.Where(r => r.CountryApplication.AppId != excludeAppId);

            var uniques = await relatedCases.GroupBy(g => new
            {
                g.MatchTypeUsed,
                g.RelatedCaseNumber,
                g.RelatedCountry,
                g.RelatedSubCase,
                g.RelatedCaseType,
                g.RelatedPubNumber,
                g.RelatedPatNumber
            }, (key, group) => new PatIDSRelatedCaseCopyDTO
            {
                MatchTypeUsed = key.MatchTypeUsed,
                RelatedCaseNumber = key.RelatedCaseNumber,
                RelatedCountry = key.RelatedCountry,
                RelatedSubCase = key.RelatedSubCase,
                RelatedCaseType = key.RelatedCaseType,
                RelatedPubNumber = key.RelatedPubNumber,
                RelatedPatNumber = key.RelatedPatNumber,
                AppId = group.Min(c => c.RelatedAppId),
                RelatedCasesId = group.Min(c => c.RelatedCasesId)
            }).ToListAsync();
            return uniques;
        }

        public IQueryable<PatRelatedCaseDTO> GetCopyRelatedCasesSources(string? caseNumber, string? country, string? subCase, string? inventor, string? keyword, string? artUnit, bool activeOnly, int excludeAppId)
        {
            var relatedCases = _countryApplicationService.PatRelatedCaseDTO;

            if (!string.IsNullOrEmpty(caseNumber))
                relatedCases = relatedCases.Where(rc => rc.CountryApplication.CaseNumber == caseNumber);

            if (!string.IsNullOrEmpty(country))
                relatedCases = relatedCases.Where(rc => rc.CountryApplication.Country == country);

            if (!string.IsNullOrEmpty(subCase))
                relatedCases = relatedCases.Where(rc => rc.CountryApplication.SubCase == subCase);

            if (activeOnly)
                relatedCases = relatedCases.Where(r => r.CountryApplication.PatApplicationStatus.ActiveSwitch);

            if (!string.IsNullOrEmpty(keyword))
                relatedCases = relatedCases.Where(rc => rc.CountryApplication.Invention.Keywords.Any(k => k.Keyword == keyword));

            if (!string.IsNullOrEmpty(inventor))
                relatedCases = relatedCases.Where(rc => rc.CountryApplication.Inventors.Any(i => i.InventorAppInventor.Inventor == inventor));

            if (!string.IsNullOrEmpty(artUnit))
                relatedCases = relatedCases.Where(rc => rc.CountryApplication.IDSRelatedCasesInfo.GroupArtUnit == artUnit);

            if (excludeAppId > 0)
                relatedCases = relatedCases.Where(rc => rc.AppId != excludeAppId);

            return relatedCases.AsNoTracking();
        }

        //public IQueryable<PatRelatedCase> GetCopyRelatedCasesSources(string caseNumber, string country, string subCase, string inventor, string keyword, string artUnit, int excludeAppId)
        //{
        //    var relatedCases = _repository.RelatedCases;

        //    if (!string.IsNullOrEmpty(caseNumber))
        //        relatedCases = relatedCases.Where(rc => rc.CountryApplication.CaseNumber == caseNumber);

        //    if (!string.IsNullOrEmpty(country))
        //        relatedCases = relatedCases.Where(rc => rc.CountryApplication.Country == country);

        //    if (!string.IsNullOrEmpty(subCase))
        //        relatedCases = relatedCases.Where(rc => rc.CountryApplication.SubCase == subCase);

        //    if (!string.IsNullOrEmpty(keyword))
        //        relatedCases = relatedCases.Where(rc => rc.CountryApplication.Invention.Keywords.Any(k => k.Keyword == keyword));

        //    if (!string.IsNullOrEmpty(inventor))
        //        relatedCases = relatedCases.Where(rc => rc.CountryApplication.Inventors.Any(i => i.InventorAppInventor.Inventor == inventor));

        //    if (!string.IsNullOrEmpty(artUnit))
        //        relatedCases = relatedCases.Where(rc => rc.CountryApplication.IDSRelatedCasesInfo.GroupArtUnit == artUnit);

        //    if (excludeAppId > 0)
        //        relatedCases = relatedCases.Where(rc => rc.AppId != excludeAppId);

        //    return relatedCases.AsNoTracking();
        //}

        // Removed during deep clean
        // public async Task CopyIDSRelatedCases(int[] to, RTSIDSCrossCheckCopyDTO[] from, int[] fromRelated, string userId)
        // {
        //     await _repository.CopyIDSRelatedCases(to, from, fromRelated, userId);
        // }

        // Removed during deep clean
        // public async Task CopyIDSRelatedCases(RTSIDSCrossCheckCopyDTO[] copyInfo, int[] actionInfo, string userId)
        // {
        //     await _repository.CopyIDSRelatedCases(copyInfo, actionInfo, userId);
        // }

        public async Task SaveStandardizedReferences(List<PatIDSRelatedCase> relatedCases) {
            await _repository.SaveStandardizedReferences(relatedCases);
        }

        #endregion
        #region IDS NonPatLiterature

        public async Task<List<PatIDSNonPatLiterature>> GetNonPatLiteratures(int appId)
        {
            return await _repository.GetNonPatLiteratures(appId);
        }

        public async Task<PatIDSNonPatLiterature> GetNonPatLiterature(int nonPatLiteratureId)
        {
            return await _repository.GetNonPatLiterature(nonPatLiteratureId);
        }

        public async Task NonPatLiteratureDelete(PatIDSNonPatLiterature deletedIdsNonPatLiterature)
        {
            await _repository.NonPatLiteratureDelete(deletedIdsNonPatLiterature);
        }

        public async Task NonPatLiteratureUpdate(PatIDSNonPatLiterature idsNonPatLiterature)
        {
            await _repository.NonPatLiteratureUpdate(idsNonPatLiterature);
        }

        public async Task NonPatLiteratureUpdate(int appId, string userName,
            IEnumerable<PatIDSNonPatLiterature> updatedNonPatLiteratures, IEnumerable<PatIDSNonPatLiterature> newNonPatLiteratures)
        {
            await _repository.NonPatLiteratureUpdate(appId, userName, updatedNonPatLiteratures, newNonPatLiteratures);
        }

        public async Task<List<LookupDTO>> GetCopyNonPatCaseNumberList(int excludeAppId)
        {
            return await _repository.GetCopyNonPatCaseNumberList(excludeAppId);
        }

        public async Task<List<LookupDTO>> GetCopyNonPatCountryList(int excludeAppId)
        {
            return await _repository.GetCopyNonPatCountryList(excludeAppId);
        }

        public async Task<List<LookupDTO>> GetCopyNonPatSubCaseList(int excludeAppId)
        {
            return await _repository.GetCopyNonPatSubCaseList(excludeAppId);
        }

        public async Task<List<LookupDTO>> GetCopyNonPatKeywordList(int excludeAppId)
        {
            return await _repository.GetCopyNonPatKeywordList(excludeAppId);
        }

        public async Task<List<LookupDTO>> GetCopyNonPatInventorList(int excludeAppId)
        {
            return await _repository.GetCopyNonPatInventorList(excludeAppId);
        }

        public async Task<List<LookupDTO>> GetCopyNonPatArtUnitList(int excludeAppId)
        {
            return await _repository.GetCopyNonPatArtUnitList(excludeAppId);
        }

        public IQueryable<PatIDSNonPatLiterature> GetCopyNonPatLiteratureSources(string? caseNumber, string? country, string? subCase,
            string? inventor, string? keyword, string? artUnit, string? searchText, int excludeAppId)
        {
            var nonPatLiteratures = _repository.NonPatLiteratures;

            if (!string.IsNullOrEmpty(caseNumber))
                nonPatLiteratures = nonPatLiteratures.Where(r => r.CountryApplication.CaseNumber == caseNumber);
            if (!string.IsNullOrEmpty(country))
                nonPatLiteratures = nonPatLiteratures.Where(r => r.CountryApplication.Country == country);
            if (!string.IsNullOrEmpty(subCase))
                nonPatLiteratures = nonPatLiteratures.Where(r => r.CountryApplication.SubCase == subCase);
            if (!string.IsNullOrEmpty(keyword))
                nonPatLiteratures = nonPatLiteratures.Where(r => r.CountryApplication.Invention.Keywords.Any(k => k.Keyword == keyword));
            if (!string.IsNullOrEmpty(inventor))
                nonPatLiteratures = nonPatLiteratures.Where(r =>
                    r.CountryApplication.Inventors.Any(i => i.InventorAppInventor.Inventor == inventor));
            if (!string.IsNullOrEmpty(artUnit))
                nonPatLiteratures = nonPatLiteratures.Where(r => r.CountryApplication.IDSRelatedCasesInfo.GroupArtUnit == artUnit);
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.Replace("*", "");
                nonPatLiteratures = nonPatLiteratures.Where(r => r.NonPatLiteratureInfo.Contains(searchText));
            }

            if (excludeAppId > 0)
                nonPatLiteratures = nonPatLiteratures.Where(r => r.AppId != excludeAppId);

            return nonPatLiteratures;
        }

        public async Task CopyNonPatLiteratures(int appId, int[] from, string userId)
        {
            await _repository.CopyNonPatLiteratures(appId, from, userId);
        }

        #endregion

        public async Task<List<PatIDSSearchInputDTO>> GetIDSDownloadList(int maxAttempts, string? appIds = "")
        {
            return await _repository.GetIDSDownloadList(maxAttempts, appIds);
        }

        public async Task SaveIDSRelatedCaseDocs(List<PatIDSRelatedCase> patIDSRelatedCases)
        {
            await _repository.SaveIDSRelatedCaseDocs(patIDSRelatedCases);
        }

        public async Task<IDSTotalDTO> GetIDSTotal(int appId)
        {
            return await _repository.GetIDSTotal(appId);
        }

        public async Task<int> GetIDSReferencesTotal(int appId) {
            return await _repository.GetIDSReferencesTotal(appId);
        }

        public async Task<bool> HasReferencesInStaging()
        {
            return await _repository.HasReferencesInStaging();
        }

        public async Task LoadReferencesFromStaging()
        {
            await _repository.LoadReferencesFromStaging();
        }
    }
}
