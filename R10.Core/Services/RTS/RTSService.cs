using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using R10.Core.Interfaces.Patent;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System.Linq.Expressions;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using R10.Core.DTOs;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces.Shared;
using R10.Core.Services.Shared;
using System.Net;
using R10.Core.Entities.Trademark;
using R10.Core.Entities.ReportScheduler;

namespace R10.Core.Services
{
    public class RTSService : IRTSService
    {
        private readonly IRTSInfoRepository _rtsInfoRepository;
        private readonly ICountryApplicationService _applicationService;
        private readonly INumberFormatService _numberFormatService;
        private readonly IApplicationDbContext _repository;
        private readonly ISystemSettings<PatSetting> _settings;
        readonly ClaimsPrincipal _user;

        public RTSService(IRTSInfoRepository rtsInfoRepository, ICountryApplicationService applicationService,
            INumberFormatService numberFormatService, ISystemSettings<PatSetting> settings,
            IApplicationDbContext repository, ClaimsPrincipal user)
        {
            _rtsInfoRepository = rtsInfoRepository;
            _applicationService = applicationService;
            _numberFormatService = numberFormatService;
            _repository = repository;
            _settings = settings;
            _user = user;
        }

        public async Task<List<RTSInfoSettingsMenu>> GetMenu(string country)
        {
            return await _rtsInfoRepository.GetMenu(country);
        }

        public RTSSearchBiblioDTO GetCaseInfo(int plAppId)
        {
            var biblio = _rtsInfoRepository.GetBiblio(plAppId);
            return biblio;
        }

        public async Task<List<RTSSearchInventorDTO>> GetInventors(int plAppId)
        {
            var inventors = await _rtsInfoRepository.GetInventors(plAppId);
            return inventors;
        }

        public async Task<List<RTSSearchApplicantDTO>> GetApplicants(int plAppId)
        {
            var applicants = await _rtsInfoRepository.GetApplicants(plAppId);
            return applicants;
        }

        public async Task<List<RTSSearchIPClassDTO>> GetIPClasses(int plAppId)
        {
            var ipClasses = await _rtsInfoRepository.GetIPClasses(plAppId);
            return ipClasses;
        }

        public async Task<List<RTSSearchTitleDTO>> GetTitles(int plAppId)
        {
            var titles = await _rtsInfoRepository.GetTitles(plAppId);
            return titles;
        }

        public RTSSearchBiblioUSDTO GetBiblioUS(int plAppId)
        {
            var biblio = _rtsInfoRepository.GetBiblioUS(plAppId);
            return biblio;
        }

        public async Task<List<RTSSearchBiblioUSDTO>> GetBiblioUSs(int plAppId)
        {
            var biblio = await _rtsInfoRepository.GetBiblioUSs(plAppId);
            return biblio;
        }

        public async Task<List<RTSSearchAssignmentDTO>> GetAssignments(int plAppId)
        {
            var assignments = await _rtsInfoRepository.GetAssignments(plAppId);
            return assignments;
        }

        public async Task<List<RTSSearchPriorityDTO>> GetPriorities(int plAppId)
        {
            var priorities = await _rtsInfoRepository.GetPriorities(plAppId);
            return priorities;
        }

        public RTSSearchAbstractDTO GetAbstractClaims(int plAppId)
        {
            var abstractClaims = _rtsInfoRepository.GetAbstractClaims(plAppId);
            return abstractClaims;
        }

        public async Task<List<RTSSearchDocCitedDTO>> GetDocsCited(int plAppId)
        {
            return await _rtsInfoRepository.GetDocsCited(plAppId);
        }

        public async Task<List<RTSSearchDocRefByDTO>> GetDocsRefBy(int plAppId)
        {
            return await _rtsInfoRepository.GetDocsRefBy(plAppId);
        }

        public async Task<List<RTSSearchPTADTO>> GetPTAs(int plAppId)
        {
            return await _rtsInfoRepository.GetPTAs(plAppId);
        }

        public async Task<List<RTSSearchContinuityParentDTO>> GetContinuitiesParent(int plAppId)
        {
            return await _rtsInfoRepository.GetContinuitiesParent(plAppId);
        }

        public async Task<List<RTSSearchContinuityChildDTO>> GetContinuitiesChild(int plAppId)
        {
            return await _rtsInfoRepository.GetContinuitiesChild(plAppId);
        }

        public async Task<List<RTSSearchIFWDTO>> GetIFWs(int plAppId)
        {
            return await _rtsInfoRepository.GetIFWs(plAppId);
        }

        //Use for Dashboard widget criteria
        public async Task<List<RTSSearchIFWDTO>> GetSearchPTODocuments()
        {
            return await _rtsInfoRepository.GetSearchPTODocuments();
        }

        public RTSSearchUSCorrespondenceDTO GetCorrespondence(int plAppId)
        {
            return _rtsInfoRepository.GetCorrespondence(plAppId);
        }

        public async Task<List<RTSSearchUSCorrespondenceDTO>> GetCorrespondences(int plAppId)
        {
            return await _rtsInfoRepository.GetCorrespondences(plAppId);
        }

        public async Task<List<RTSSearchAgentDTO>> GetAgents(int plAppId)
        {
            return await _rtsInfoRepository.GetAgents(plAppId);
        }

        public async Task<List<RTSSearchDesCountryDTO>> GetDesCountries(int plAppId)
        {
            return await _rtsInfoRepository.GetDesCountries(plAppId);
        }

        public async Task<List<RTSSearchPFSDocDTO>> GetPFSDocs(int appId)
        {
            return await _rtsInfoRepository.GetPFSDocs(appId);
        }

        public async Task<List<RTSSearchLSDDTO>> GetLSDs(int appId)
        {
            return await _rtsInfoRepository.GetLSDs(appId);
        }

        public async Task<List<RTSSearchIPCDTO>> GetIPCs(int appId)
        {
            return await _rtsInfoRepository.GetIPCs(appId);
        }

        public async Task<List<RTSSearchCPCDTO>> GetCPCs(int appId)
        {
            return await _rtsInfoRepository.GetCPCs(appId);
        }

        public async Task<int> GetClaimsCount(int appId)
        {
            return await _rtsInfoRepository.GetClaimsCount(appId);
        }


        public async Task<List<RTSPFSTitleUpdHistoryDTO>> GetPFSTitleUpdHistory(int appId)
        {
            return await _rtsInfoRepository.GetPFSTitleUpdHistory(appId);
        }

        public async Task<List<RTSPFSAbstractUpdHistoryDTO>> GetPFSAbstractUpdHistory(int appId)
        {
            return await _rtsInfoRepository.GetPFSAbstractUpdHistory(appId);
        }

        public RTSPFSCountryAppUpdHistoryDTO GetPFSCtryAppUpdHistory(int appId)
        {
            return _rtsInfoRepository.GetPFSCtryAppUpdHistory(appId);
        }

        public async Task<List<UpdateHistoryBatchDTO>> GetActionUpdHistoryBatches(int plAppId,
            int revertType)
        {
            return await _rtsInfoRepository.GetActionUpdHistoryBatches(plAppId, revertType, 0);
        }

        public async Task<List<RTSSearchActionUpdHistoryDTO>> GetActionUpdHistory(int plAppId, int revertType,
            int jobId)
        {
            await ValidateRecordFilterPermission(plAppId);
            return await _rtsInfoRepository.GetActionUpdHistory(plAppId, revertType, jobId);
        }

        public async Task<List<UpdateHistoryBatchDTO>> GetActionClosedUpdHistoryBatches(int plAppId)
        {
            return await _rtsInfoRepository.GetActionClosedUpdHistoryBatches(plAppId, 0);
        }

        public async Task<List<RTSSearchActionClosedUpdHistoryDTO>> GetActionClosedUpdHistory(int plAppId, int jobId)
        {
            await ValidateRecordFilterPermission(plAppId);
            return await _rtsInfoRepository.GetActionClosedUpdHistory(plAppId, jobId);
        }

        public async Task<List<RTSSearchActionAsDownloadedDTO>> GetActionsAsDownloaded(int plAppId)
        {
            await ValidateRecordFilterPermission(plAppId);
            return await _rtsInfoRepository.GetActions(plAppId, true);
        }

        public async Task<List<RTSSearchActionAsDownloadedDTO>> GetActionsAsMatched(int plAppId)
        {
            await ValidateRecordFilterPermission(plAppId);
            return await _rtsInfoRepository.GetActions(plAppId, false);
        }

        public async Task UndoActions(int jobId, int plAppId, string updatedBy)
        {
            await ValidateRecordFilterPermission(plAppId);
            await _rtsInfoRepository.UndoActions(jobId, plAppId, updatedBy);
        }

        #region Workflow
        public async Task<List<RTSPFSWorkflowBatch>> GetPFSUpdatesForWorkflow()
        {
            return await _rtsInfoRepository.GetPFSUpdatesForWorkflow();
        }
        public async Task<List<RTSPFSWorkflowApp>> GetPFSUpdatesToPublishedForWorkflow(string batchId)
        {
            return await _rtsInfoRepository.GetPFSUpdatesToPublishedForWorkflow(batchId);
        }
        public async Task<List<RTSPFSWorkflowApp>> GetPFSUpdatesToGrantedForWorkflow(string batchId)
        {
            return await _rtsInfoRepository.GetPFSUpdatesToGrantedForWorkflow(batchId);
        }
        public void MarkPFSUpdatesWorkflowBatchAsGenerated(string batchId)
        {
            _rtsInfoRepository.MarkPFSUpdatesWorkflowBatchAsGenerated(batchId);
        }
        public void MarkPFSUpdatesWorkflowAsGenerated(string batchId, int appId)
        {
            _rtsInfoRepository.MarkPFSUpdatesWorkflowAsGenerated(batchId, appId);
        }
        public void MarkRTSAutoDocketActionWorkflowAsGenerated(int actId)
        {
            _rtsInfoRepository.MarkRTSAutoDocketActionWorkflowAsGenerated(actId);
        }
        #endregion


        public async Task<List<RTSPFSSearchOutput>> SearchInpadoc(RTSPFSSearchInput searchInput)
        {
            await PrepareInpadocSearchInput(searchInput);
            var result = await SearchInpadocApi(searchInput);
            return result;
        }

        public async Task<List<RTSPFSCitationHeader>> SearchInpadocCitation(RTSPFSSearchInput searchInput)
        {
            await PrepareInpadocSearchInput(searchInput);
            var result = await SearchInpadocCitationApi(searchInput);
            return result;
        }


        private async Task PrepareInpadocSearchInput(RTSPFSSearchInput searchInput)
        {
            var numberInfo = new WebLinksNumberInfoDTO
            {
                SystemType = WebLinksSystemType.Patent,
                Country = searchInput.SearchCountry,
                CaseType = searchInput.SearchCaseType,
                NumberType = searchInput.SearchNumberType,
                Number = searchInput.SearchNo,
                NumberDate = searchInput.SearchDate
            };

            var parsedInfo = await _numberFormatService.StandardizeNumber(numberInfo);
            string yearNo = null;

            if (parsedInfo != null)
            {
                if (searchInput.SearchNumberType == WebLinksNumberType.AppNo)
                {
                    if (searchInput.SearchCountry == "US" &&
                        searchInput.SearchNo.IndexOf("/", StringComparison.Ordinal) > -1)
                        yearNo = searchInput.SearchNo.Substring(1);
                    else
                    {
                        string refinedNo = searchInput.SearchNo;
                        if (searchInput.SearchNo.IndexOf(".", StringComparison.Ordinal) > -1)
                            refinedNo = searchInput.SearchNo.Substring(
                                searchInput.SearchNo.IndexOf(".", StringComparison.Ordinal));

                        if (searchInput.SearchCountry == "EP" && refinedNo.Length == 8)
                            yearNo = searchInput.SearchNo.Substring(1);
                        else if (string.IsNullOrEmpty(parsedInfo.Year) && searchInput.SearchDate != null)
                            yearNo = (searchInput.SearchDate?.Year).ToString();
                        else
                            yearNo = parsedInfo.Year;
                    }
                }
                else if (searchInput.SearchNumberType == WebLinksNumberType.PubNo)
                {
                    if (parsedInfo.Year != "")
                        yearNo = parsedInfo.Year;
                    else
                    {
                        // extract pub. number if number has a 4-digit number before slash or dash
                        // i.e. WO2006/030213 or WO-2006-030213 or 2006-030213
                        Regex regex = new Regex(@"[\/-A-Za-z]*(\d{4,4})[\/-]+");
                        Match match = regex.Match(searchInput.SearchNo);
                        if (match.Success)
                            yearNo = match.Groups[1].Value;
                    }
                }

                searchInput.SearchNo = parsedInfo.Number;
            }

            //no matching template, just get the numbers
            else
            {
                searchInput.SearchNo = Regex.Replace(searchInput.SearchNo, "[^0-9]", "");
            }
            searchInput.SearchYearNo = yearNo;
        }

        public async Task<List<RTSPFSPatentWatchOutput>> MultipleSearchInpadoc(RTSPFSMultipleSearchInput searchInput)
        {
            var result = await MultipleSearchInpadocApi(searchInput);
            return result;
        }

        public async Task<List<RTSLSDEventOutput>> SearchInpadocLSD(string header)
        {
            var result = await SearchInpadocLSDApi(header);
            return result;
        }

        public async Task<List<RTSPFSStatisticsSearchOutput>> StatisticsSearchInpadoc(RTSPFSStatisticsSearchInput searchInput)
        {
            var result = await StatisticsSearchInpadocApi(searchInput);

            var id = 0;
            var countries = await _repository.PatCountries.ToListAsync();
            result.ForEach(r =>
            {
                id++;
                r.Id = id;
                r.CountryName = countries.FirstOrDefault(c => c.Country == r.Country)?.CountryName;
            });

            //if (searchInput.ReportType != "4") {
            //    var countries = await _repository.PatCountries.ToListAsync();
            //    result.ForEach(r => {
            //        id++;
            //        r.Id = id;
            //        r.CountryName = countries.FirstOrDefault(c => c.Country == r.Country)?.CountryName;
            //    });
            //}
            //else {
            //    result.ForEach(r => {
            //        id++;
            //        r.Id = id;
            //    });
            //}
            return result;
        }

        public List<RTSPFSStatisticsSearchOutput> StatisticsSearchInpadocLocal(RTSPFSStatisticsSearchInput searchInput)
        {
            var result = _rtsInfoRepository.StatisticsSearchInpadoc(searchInput);
            return result;
        }

        public async Task FormatInpadoc(string country, string caseType, string numberType,
            RTSPFSSearchOutput selectedRecord)
        {
            var numberInfo = new WebLinksNumberInfoDTO
            {
                SystemType = WebLinksSystemType.Patent,
                Country = country,
                CaseType = caseType
            };

            //if (numberType != WebLinksNumberType.AppNo && !string.IsNullOrEmpty(selectedRecord.AppNo))
            if (!string.IsNullOrEmpty(selectedRecord.AppNo))
            {
                numberInfo.NumberType = WebLinksNumberType.AppNo;
                numberInfo.Number = selectedRecord.AppNo;
                numberInfo.NumberDate = selectedRecord.FilDate;
                selectedRecord.AppNo =
                    await _numberFormatService.FormatNumber(numberInfo, WebLinksTemplateType.Display);
            }

            //if (numberType != WebLinksNumberType.PubNo && !string.IsNullOrEmpty(selectedRecord.PubNo))
            if (!string.IsNullOrEmpty(selectedRecord.PubNo))
            {
                numberInfo.NumberType = WebLinksNumberType.PubNo;
                numberInfo.Number = selectedRecord.PubNo;
                numberInfo.NumberDate = selectedRecord.PubDate;
                selectedRecord.PubNo =
                    await _numberFormatService.FormatNumber(numberInfo, WebLinksTemplateType.Display);
            }

            //if (numberType != WebLinksNumberType.PatRegNo && !string.IsNullOrEmpty(selectedRecord.PatNo))
            if (!string.IsNullOrEmpty(selectedRecord.PatNo))
            {
                numberInfo.NumberType = WebLinksNumberType.PatRegNo;
                numberInfo.Number = selectedRecord.PatNo;
                numberInfo.NumberDate = selectedRecord.IssDate;
                selectedRecord.PatNo =
                    await _numberFormatService.FormatNumber(numberInfo, WebLinksTemplateType.Display);
            }
        }

        public async Task FormatInpadocIDS(string country, string caseType, string numberType, RTSPFSSearchOutput selectedRecord)
        {
            var numberInfo = new WebLinksNumberInfoDTO
            {
                SystemType = WebLinksSystemType.Patent,
                Country = country,
                CaseType = caseType
            };

            //format the entered no only
            if (numberType == WebLinksNumberType.PubNo && !string.IsNullOrEmpty(selectedRecord.PubNo))
            {
                numberInfo.NumberType = WebLinksNumberType.PubNo;
                numberInfo.Number = selectedRecord.PubNo;
                numberInfo.NumberDate = selectedRecord.PubDate;
                selectedRecord.PubNo =
                    await _numberFormatService.FormatNumber(numberInfo, WebLinksTemplateType.Display);
            }
            else if (numberType == WebLinksNumberType.PatRegNo && !string.IsNullOrEmpty(selectedRecord.PatNo))
            {
                numberInfo.NumberType = WebLinksNumberType.PatRegNo;
                numberInfo.Number = selectedRecord.PatNo;
                numberInfo.NumberDate = selectedRecord.IssDate;
                selectedRecord.PatNo =
                    await _numberFormatService.FormatNumber(numberInfo, WebLinksTemplateType.Display);
            }
        }

        public async Task<List<RTSSearchUSIFW>> GetUntransferredIFWs()
        {
            return await _repository.RTSSearchUSIFWs.Where(i => (bool)i.HasDocument && i.FileName.Length > 0 && (!(bool)i.Transferred || i.Transferred == null))
                                                   .Select(i =>
                                                   new RTSSearchUSIFW
                                                   {
                                                       PLAppID = i.RTSSearch.PMSAppId,
                                                       FileName = i.FileName
                                                   }).Distinct().ToListAsync();
        }


        public void MarkIFWAsTransferred(string fileName)
        {
            _rtsInfoRepository.MarkIFWAsTransferred(fileName);
        }

        public IQueryable<RTSSearch> RTSSearchRecords
        {
            get
            {
                var list = _rtsInfoRepository.RTSSearchRecords;
                if (_user.HasRespOfficeFilter(SystemType.Patent) || _user.HasEntityFilter())
                {
                    list = list.Where(s => this.CountryApplications.Any(c => c.AppId == s.PMSAppId));
                }
                return list;
            }
        }

        public IQueryable<RTSSearchAction> RTSSearchActions
        {
            get
            {
                var list = _rtsInfoRepository.RTSSearchActions;
                if (_user.HasRespOfficeFilter(SystemType.Patent) || _user.HasEntityFilter())
                {
                    list = list.Where(a => RTSSearchRecords.Any(s =>
                        s.PLAppId == a.PLAppId && this.CountryApplications.Any(c => c.AppId == s.PMSAppId)));
                }

                return list;
            }
        }

        //no resp office/entity filter
        public IQueryable<RTSSearchUSIFW> RTSSearchApplicableIFWs
        {
            get
            {
                var list = _rtsInfoRepository.RTSSearchUSIFWs.Where(i => (bool)i.HasDocument && i.FileName.Length > 0 && (bool)i.Transferred && (bool)i.AIInclude).AsNoTracking();
                return list;
            }
        }

        public IQueryable<RTSSearchUSIFW> RTSSearchIFWs
        {
            get
            {
                var list = _rtsInfoRepository.RTSSearchUSIFWs.Where(i => (bool)i.HasDocument && i.FileName.Length > 0 && (bool)i.Transferred).AsNoTracking();
                if (_user.HasRespOfficeFilter(SystemType.Patent) || _user.HasEntityFilter())
                {
                    list = list.Where(a => RTSSearchRecords.Any(s =>
                        s.PLAppId == a.PLAppID && this.CountryApplications.Any(c => c.AppId == s.PMSAppId)));
                }

                return list;
            }
        }

        private async Task ValidateRecordFilterPermission(int plAppId)
        {
            if (_user.HasRespOfficeFilter(SystemType.Patent) || _user.HasEntityFilter())
            {
                Guard.Against.NoRecordPermission(await RTSSearchRecords.AnyAsync(s =>
                    s.PLAppId == plAppId && this.CountryApplications.Any(c => c.AppId == s.PMSAppId)));
            }
        }

        private async Task<List<RTSPFSSearchOutput>> SearchInpadocApi(RTSPFSSearchInput searchInput)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.InpadocURL;
            string searchUrl = $"{serviceUrl}/{(searchInput.ForIDS ? "SearchIDS" : "Search")}";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;

                    HttpResponseMessage response = await client.SendAsync(request);
                    var stringResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<List<RTSPFSSearchOutput>>(stringResponse);
                    return result;

                }
            }
        }

        private async Task<List<RTSPFSCitationHeader>> SearchInpadocCitationApi(RTSPFSSearchInput searchInput)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.InpadocURL;
            string searchUrl = $"{serviceUrl}/SearchCitation";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;

                    HttpResponseMessage response = await client.SendAsync(request);
                    var stringResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<List<RTSPFSCitationHeader>>(stringResponse);
                    return result;

                }
            }
        }

        private async Task<List<RTSPFSPatentWatchOutput>> MultipleSearchInpadocApi(RTSPFSMultipleSearchInput searchInput)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.InpadocURL;
            string searchUrl = $"{serviceUrl}/IFDSearch";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;

                    HttpResponseMessage response = await client.SendAsync(request);
                    var stringResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<List<RTSPFSPatentWatchOutput>>(stringResponse);
                    return result;

                }
            }
        }


        private async Task<List<RTSPFSStatisticsSearchOutput>> StatisticsSearchInpadocApi(RTSPFSStatisticsSearchInput searchInput)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.StatisticsURL;
            string searchUrl = $"{serviceUrl}/StatisticsSearch";

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(10);

                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;

                    HttpResponseMessage response = await client.SendAsync(request);
                    var stringResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<List<RTSPFSStatisticsSearchOutput>>(stringResponse);
                    return result;

                }
            }
        }

        public async Task<List<RTSPFSStatisticsAuxSearchOutput>> StatisticsAuxSearchInpadoc(RTSPFSStatisticsAuxSearchInput searchInput)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.StatisticsURL;
            string searchUrl = $"{serviceUrl}/StatisticsAuxSearch";

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(10);

                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;

                    HttpResponseMessage response = await client.SendAsync(request);
                    var stringResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<List<RTSPFSStatisticsAuxSearchOutput>>(stringResponse);
                    return result;
                }
            }
        }

        private async Task<List<RTSLSDEventOutput>> SearchInpadocLSDApi(string header)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.InpadocURL;
            string searchUrl = $"{serviceUrl}/LSDSearch";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(header);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;

                    HttpResponseMessage response = await client.SendAsync(request);
                    var stringResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<List<RTSLSDEventOutput>>(stringResponse);
                    return result;

                }
            }
        }

        private IQueryable<CountryApplication> CountryApplications => _applicationService.CountryApplications;

        #region PatentWatch
        public async Task<List<RTSPatentWatch>> PatentWatchGetlist(string cpiClientCode)
        {
            var result = await PatentWatchGetListApi(new RTSPFSPatentWatchSearchInput { CPIClientCode = cpiClientCode });
            return result;
        }

        public async Task<bool> PatentWatchDelete(string cpiClientCode, int watchId)
        {
            return await PatentWatchDeleteListApi(new RTSPFSPatentWatchSearchInput { CPIClientCode = cpiClientCode, WatchId = watchId });
        }

        public async Task<List<RTSPFSPatentWatchNotify>> PatentWatchGetUsersToNotify(string cpiClientCode, int watchId)
        {
            return await PatentWatchGetUsersToNotifyApi(new RTSPFSPatentWatchSearchInput { CPIClientCode = cpiClientCode, WatchId = watchId });
        }

        //public async Task<bool> PatentWatchUpdateUsersToNotify(string cpiClientCode, int watchId, string notify)
        //{
        //    return await PatentWatchUpdateUsersToNotify(new RTSPFSPatentWatchSearchInput { CPIClientCode = cpiClientCode, WatchId = watchId,Notify=notify });
        //}

        public async Task<bool> PatentWatchUpdateUserToNotify(string cpiClientCode, RTSPFSPatentWatchNotify notify)
        {
            return await PatentWatchUpdateUserToNotifyApi(new RTSPFSPatentWatchSearchInput { CPIClientCode = cpiClientCode, WatchId = notify.WatchId, NotifyId = notify.NotifyId, Notify = notify.Email }, 1);
        }

        public async Task<bool> PatentWatchDeleteUserToNotify(string cpiClientCode, RTSPFSPatentWatchNotify notify)
        {
            return await PatentWatchUpdateUserToNotifyApi(new RTSPFSPatentWatchSearchInput { CPIClientCode = cpiClientCode, WatchId = notify.WatchId, NotifyId = notify.NotifyId, Notify = notify.Email }, 2);
        }

        public async Task PatentWatchMarkUpdateAsViewed(string cpiClientCode, int watchId, string userName)
        {
            await PatentWatchMarkAsViewedApi(new RTSPFSPatentWatchSearchInput { CPIClientCode = cpiClientCode, WatchId = watchId,UserName=userName });
        }

        public async Task PatentWatchUpdateRemarks(string cpiClientCode, int watchId, string remarks, string updatedBy)
        {
            await PatentWatchUpdateRemarksApi(new RTSPFSPatentWatchSearchInput { CPIClientCode = cpiClientCode, WatchId = watchId, Remarks = remarks, UserName = updatedBy });
        }

        public async Task PatentWatchUpdateKeywords(string cpiClientCode, int watchId, string keywords, string updatedBy)
        {
            await PatentWatchUpdateKeywordsApi(new RTSPFSPatentWatchSearchInput { CPIClientCode = cpiClientCode, WatchId = watchId, Keywords = keywords, UserName = updatedBy });
        }

        public async Task<List<RTSPFSPatentWatchOutput>> PatentWatchGetUpdates(string cpiClientCode, string userName)
        {
            var result = await PatentWatchGetUpdatesApi(new RTSPFSPatentWatchSearchInput { CPIClientCode = cpiClientCode,UserName= userName });
            return result;
        }

        public async Task<List<RTSPFSPatentWatchOutput>> PatentWatchGetUpdatesForEmail(string cpiClientCode, string email)
        {
            var result = await PatentWatchGetUpdatesApi(new RTSPFSPatentWatchSearchInput { CPIClientCode = cpiClientCode, UserName = email }, true);
            return result;
        }


        public async Task<List<RTSPFSPatentWatchOutput>> PatentWatchSearchInpadoc(RTSPFSPatentWatchSearchInput searchInput)
        {
            var result = await PatentWatchSearchInpadocApi(searchInput);
            return result;
        }

        public async Task<int> PatentWatchGetLimit(string cpiClientCode)
        {
            var result = await PatentWatchGetLimitApi(new RTSPFSPatentWatchSearchInput { CPIClientCode = cpiClientCode }, 1);
            return result;

        }

        public async Task<int> PatentWatchGetRemaining(string cpiClientCode)
        {
            var result = await PatentWatchGetLimitApi(new RTSPFSPatentWatchSearchInput { CPIClientCode = cpiClientCode }, 2);
            return result;
        }

        public async Task<List<CPiUser>> GetUsers()
        {
            return await _repository.CPiUser.Where(u => u.Status == CPiUserStatus.Approved).OrderBy(u => u.Email).ToListAsync();
        }

        public async Task<bool> PatentWatchSearchFromPS(RTSPFSPatentWatchFromPSInput searchInput)
        {
            return await PatentWatchSearchFromPSApi(searchInput);

        }

        private async Task<List<RTSPatentWatch>> PatentWatchGetListApi(RTSPFSPatentWatchSearchInput searchInput)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.InpadocURL;
            string searchUrl = $"{serviceUrl}/PatentWatchList";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;

                    HttpResponseMessage response = await client.SendAsync(request);
                    var stringResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<List<RTSPatentWatch>>(stringResponse);
                    return result;
                }
            }
        }

        private async Task<bool> PatentWatchDeleteListApi(RTSPFSPatentWatchSearchInput searchInput)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.InpadocURL;
            string searchUrl = $"{serviceUrl}/PatentWatchListDelete";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;
                    HttpResponseMessage response = await client.SendAsync(request);
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
        }

        private async Task<List<RTSPFSPatentWatchOutput>> PatentWatchSearchInpadocApi(RTSPFSPatentWatchSearchInput searchInput)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.InpadocURL;
            string searchUrl = $"{serviceUrl}/PatentWatchSearch";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;

                    HttpResponseMessage response = await client.SendAsync(request);
                    var stringResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<List<RTSPFSPatentWatchOutput>>(stringResponse);
                    return result;

                }
            }
        }

        private async Task<List<RTSPFSPatentWatchOutput>> PatentWatchGetUpdatesApi(RTSPFSPatentWatchSearchInput searchInput, bool forEmail = false)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.InpadocURL;
            string searchUrl = $"{serviceUrl}/PatentWatchUpdates{(forEmail ? "ForEmail" : "")}";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;

                    HttpResponseMessage response = await client.SendAsync(request);
                    var stringResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<List<RTSPFSPatentWatchOutput>>(stringResponse);
                    return result;

                }
            }
        }


        private async Task<bool> PatentWatchMarkAsViewedApi(RTSPFSPatentWatchSearchInput searchInput)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.InpadocURL;
            string searchUrl = $"{serviceUrl}/PatentWatchMarkViewed";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;
                    HttpResponseMessage response = await client.SendAsync(request);
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
        }

        private async Task<bool> PatentWatchUpdateRemarksApi(RTSPFSPatentWatchSearchInput searchInput)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.InpadocURL;
            string searchUrl = $"{serviceUrl}/PatentWatchUpdateRemarks";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;
                    HttpResponseMessage response = await client.SendAsync(request);
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
        }

        private async Task<bool> PatentWatchUpdateKeywordsApi(RTSPFSPatentWatchSearchInput searchInput)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.InpadocURL;
            string searchUrl = $"{serviceUrl}/PatentWatchUpdateKeywords";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;
                    HttpResponseMessage response = await client.SendAsync(request);
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
        }

        private async Task<int> PatentWatchGetLimitApi(RTSPFSPatentWatchSearchInput searchInput, int type)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.InpadocURL;
            string searchUrl = $"{serviceUrl}/{(type == 1 ? "PatentWatchGetLimit" : "PatentWatchGetRemaining")}";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;

                    HttpResponseMessage response = await client.SendAsync(request);
                    var stringResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<int>(stringResponse);
                    return result;

                }
            }
        }

        private async Task<List<RTSPFSPatentWatchNotify>> PatentWatchGetUsersToNotifyApi(RTSPFSPatentWatchSearchInput searchInput)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.InpadocURL;
            string searchUrl = $"{serviceUrl}/PatentWatchGetUsersToNotifyList";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;

                    HttpResponseMessage response = await client.SendAsync(request);
                    var stringResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<List<RTSPFSPatentWatchNotify>>(stringResponse);
                    return result;
                }
            }
        }

        private async Task<bool> PatentWatchUpdateUserToNotifyApi(RTSPFSPatentWatchSearchInput searchInput, int type)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.InpadocURL;
            string searchUrl = $"{serviceUrl}/{(type == 1 ? "PatentWatchUpdateUserToNotify" : "PatentWatchDeleteUserToNotify")}";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;
                    HttpResponseMessage response = await client.SendAsync(request);
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
        }

        private async Task<bool> PatentWatchSearchFromPSApi(RTSPFSPatentWatchFromPSInput searchInput)
        {
            var settings = await _settings.GetSetting();
            string serviceUrl = settings.InpadocURL;
            string searchUrl = $"{serviceUrl}/PatentWatchFromPSSearch";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;
                    HttpResponseMessage response = await client.SendAsync(request);
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
        }
        #endregion
        #region RTSUpdate
        public IQueryable<RTSBiblioUpdate> RTSBiblioUpdates
        {
            get
            {
                var updates = _repository.RTSBiblioUpdates.AsNoTracking();

                if (_user.HasRespOfficeFilter(SystemType.Patent))
                    updates = updates.Where(RespOfficeFilter<RTSBiblioUpdate>());

                if (_user.HasEntityFilter())
                    updates = updates.Where(EntityFilter<RTSBiblioUpdate>());
                return updates;

            }
        }

        public async Task BiblioUpdateSetting(int appId, string fieldName, bool update, string tStamp)
        {
            var pubNumberConverted = new PubNumberConverted { AppId = appId, tStamp = Convert.FromBase64String(tStamp) };
            var entity = _repository.PubNumberConverteds.Attach(pubNumberConverted);

            switch (fieldName)
            {
                case RTSBiblioUpdateField.UpdateAppNo:
                    pubNumberConverted.UpdateAppNo = update;
                    entity.Property(s => s.UpdateAppNo).IsModified = true;
                    break;
                case RTSBiblioUpdateField.UpdatePubNo:
                    pubNumberConverted.UpdatePubNo = update;
                    entity.Property(s => s.UpdatePubNo).IsModified = true;
                    break;
                case RTSBiblioUpdateField.UpdatePatNo:
                    pubNumberConverted.UpdatePatNo = update;
                    entity.Property(s => s.UpdatePatNo).IsModified = true;
                    break;
                case RTSBiblioUpdateField.UpdateFilDate:
                    pubNumberConverted.UpdateFilDate = update;
                    entity.Property(s => s.UpdateFilDate).IsModified = true;
                    break;
                case RTSBiblioUpdateField.UpdatePubDate:
                    pubNumberConverted.UpdatePubDate = update;
                    entity.Property(s => s.UpdatePubDate).IsModified = true;
                    break;
                case RTSBiblioUpdateField.UpdateIssDate:
                    pubNumberConverted.UpdateIssDate = update;
                    entity.Property(s => s.UpdateIssDate).IsModified = true;
                    break;
                case RTSBiblioUpdateField.UpdateParentPCTDate:
                    pubNumberConverted.UpdateParentPCTDate = update;
                    entity.Property(s => s.UpdateParentPCTDate).IsModified = true;
                    break;

                case RTSBiblioUpdateField.UpdateCaseType:
                    pubNumberConverted.UpdateCaseType = update;
                    entity.Property(s => s.UpdateCaseType).IsModified = true;
                    break;

                case RTSBiblioUpdateField.ExcludeUpdate:
                    pubNumberConverted.ExcludeUpdate = update;
                    entity.Property(s => s.ExcludeUpdate).IsModified = true;
                    break;
            }
            await _repository.SaveChangesAsync();

        }

        public async Task<int> UpdateBiblioRecord(int appId, string updatedBy)
        {
            return await _rtsInfoRepository.UpdateBiblioRecord(appId, updatedBy);
        }

        public async Task<int> UpdateBiblioRecords(RTSUpdateCriteria criteria) {
            return await _rtsInfoRepository.UpdateBiblioRecords(criteria);
        }

        public async Task MarkBiblioRecords() {
             await _rtsInfoRepository.MarkBiblioRecords();
        }

        public async Task<List<RTSUpdateWorkflow>> GetUpdateWorkflowRecs(int jobId, int appId) {
            return await _rtsInfoRepository.GetUpdateWorkflowRecs(jobId, appId);
        }

        public async Task<List<RTSBiblioUpdateHistory>> GetBiblioUpdHistory(int appId, int revertType, int jobId)
        {
            return await _repository.RTSBiblioUpdatesHistory.Where(h => (appId == 0) && (revertType == 2 || h.Reverted == revertType) && (jobId == 0 || h.JobId == jobId)).ToListAsync();
        }

        public async Task<bool> UndoBiblio(int jobId, int appId, int logId, string updatedBy) {
            return await _rtsInfoRepository.UndoBiblio(jobId, appId, logId, updatedBy);
        }

        public async Task<List<UpdateHistoryBatchDTO>> GetBiblioUpdHistoryBatches(int appId, int revertType)
        {
            return await _repository.RTSBiblioUpdatesHistory.Where(h => (appId == 0) && (revertType == 2 || h.Reverted == revertType))
                 .Select(h => new UpdateHistoryBatchDTO { JobId = h.JobId, ChangeDate = h.ChangeDate }).Distinct().ToListAsync();
        }

        protected Expression<Func<T, bool>> RespOfficeFilter<T>() where T : RTSEntityFilter
        {
            return a => _repository.CPiUserSystemRoles.AsNoTracking().Any(r => r.UserId == _user.GetUserIdentifier() && r.SystemId == SystemType.Patent && a.RespOffice == r.RespOffice);
        }

        protected Expression<Func<T, bool>> EntityFilter<T>() where T : RTSEntityFilter
        {
            string userIdentifier = _user.GetUserIdentifier();
            var userEntityFilters = _repository.CPiUserEntityFilters.AsNoTracking();

            switch (_user.GetEntityFilterType())
            {
                case CPiEntityType.Client:
                    return a => userEntityFilters.Any(f =>
                        f.UserId == userIdentifier && f.EntityId == a.ClientId);

                case CPiEntityType.Agent:
                    return a => userEntityFilters.Any(f => f.UserId == userIdentifier && f.EntityId == a.AgentId);

                case CPiEntityType.Owner:
                    return a => userEntityFilters.Any(f => f.UserId == userIdentifier && _repository.PatOwnerApps.Any(o => o.OwnerID == f.EntityId && o.AppId == a.AppId));

                case CPiEntityType.Inventor:
                    return a => userEntityFilters.Any(f => f.UserId == userIdentifier && _repository.PatInventorsApp.Any(o => o.InventorID == f.EntityId && o.AppId == a.AppId));

                case CPiEntityType.Attorney:
                    return a => userEntityFilters.Any(f =>
                        f.UserId == userIdentifier && (f.EntityId == a.Attorney1Id ||
                                                       f.EntityId == a.Attorney2Id ||
                                                       f.EntityId == a.Attorney3Id ||
                                                       f.EntityId == a.Attorney4Id ||
                                                       f.EntityId == a.Attorney5Id));

            }
            return null;
        }

        #endregion        
    }

    public class RTSBiblioUpdateField
    {
        public const string UpdateAppNo = "UpdateAppNo";
        public const string UpdatePubNo = "UpdatePubNo";
        public const string UpdatePatNo = "UpdatePatNo";
        public const string UpdateFilDate = "UpdateFilDate";
        public const string UpdatePubDate = "UpdatePubDate";
        public const string UpdateIssDate = "UpdateIssDate";
        public const string UpdateParentPCTDate = "UpdateParentPCTDate";
        public const string UpdateCaseType = "UpdateCaseType";
        public const string ExcludeUpdate = "ExcludeUpdate";
        
    }

    public class RTSUpdateCriteria
    {
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }
        public string? Client { get; set; }
        public bool ActiveSwitch { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
