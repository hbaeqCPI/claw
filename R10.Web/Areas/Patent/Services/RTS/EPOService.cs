using AutoMapper;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using R10.Core.DTOs;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Patent;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using R10.Core.Interfaces.Patent;
using R10.Core.Interfaces.Shared;
using R10.Core.Services.Shared;
using R10.Web.Areas.Patent.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models.IManageModels;
using R10.Web.Services;
using R10.Web.Services.DocumentStorage;
using R10.Web.Services.iManage;
using R10.Web.Services.NetDocuments;
using R10.Web.Services.SharePoint;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace R10.Web.Areas.Patent.Services
{
    public class EPOService : IEPOService
    {
        private readonly IMapper _mapper;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ClaimsPrincipal _user;

        private readonly EPOMailboxSettings _epoMailboxSettings;
        private readonly EPOOPSSettings _epoOPSSettings;

        private readonly IEntityService<PatOPSLog> _epoOPSLogService;

        private readonly IEntityService<EPOPortfolio> _epoPortfolioService;
        private readonly IEntityService<EPOApplication> _epoApplicationService;
        private readonly IEntityService<EPODueDate> _epoDueDateService;

        private readonly IEntityService<EPODueDateTerm> _epoDueDateTermService;
        private readonly IParentEntityService<EPODueDateTerm, PatEPOActionMapAct> _epoActMapActService;
        private readonly IEntityService<PatEPOAppLog> _epoAppLogService;
        private readonly IEntityService<PatEPOCommActLog> _epoCommActLogService;
        private readonly IEntityService<PatEPODDActLog> _epoDDActLogService;

        private readonly IEntityService<EPOCommunication> _epoCommunicationService;
        private readonly IParentEntityService<EPOCommunication, EPOCommunicationDoc> _epoCommunicationDocService;

        private readonly IEntityService<PatEPOMailLog> _epoMailLogService;
        private readonly IEntityService<PatEPODocumentCombined> _epoDocCombinedService;

        private readonly IParentEntityService<PatEPODocumentMerge, PatEPODocumentMergeGuide> _epoDocMergeService;
        private readonly IParentEntityService<PatEPODocumentMergeGuide, PatEPODocumentMergeGuideSub> _epoDocMergeGuideService;
        private readonly IEntityService<PatEPODocumentMap> _epoDocMapService;
        private readonly IChildEntityService<PatEPODocumentMap, PatEPODocumentMapAct> _epoDocMapActService;
        private readonly IChildEntityService<PatEPODocumentMap, PatEPODocumentMapTag> _epoDocMapTagService;
        private readonly IEntityService<DocDocumentTag> _documentTagService;

        private readonly ICountryApplicationService _applicationService;
        private readonly IPatActionDueViewModelService _actionDueViewModelService;
        private readonly IActionDueService<PatActionDue, PatDueDate> _actionDueService;

        private readonly IAMSDueService _amsDueService;

        private readonly INumberFormatService _numberFormatService;

        private readonly IDocumentHelper _documentHelper;
        private readonly IDocumentService _docService;
        private readonly IDocumentsViewModelService _docViewModelService;
        private readonly IDocumentStorage _documentStorage;
        private readonly GraphSettings _graphSettings;
        private readonly ISharePointService _sharePointService;
        private readonly ISharePointViewModelService _sharePointViewModelService;

        private readonly iManageSettings _iManageSettings;
        private readonly IiManageClientFactory _iManageClientFactory;
        private readonly IiManageViewModelService _iManageViewModelService;

        private readonly IUrlHelper _url;
        private readonly ILogger _logger;

        private readonly INetDocumentsViewModelService _netDocsViewModelService;

        public EPOService(
            IMapper mapper,
            ISystemSettings<PatSetting> patSettings,
            ClaimsPrincipal user,
            IOptions<EPOMailboxSettings> epoMailboxSettings,
            IOptions<EPOOPSSettings> epoOPSSettings,
            IEntityService<PatOPSLog> epoOPSLogService,
            IEntityService<EPOPortfolio> epoPortfolioService,
            IEntityService<EPOApplication> epoApplicationService,
            IEntityService<EPODueDate> epoDueDateService,
            IEntityService<EPODueDateTerm> epoDueDateTermService,
            IParentEntityService<EPODueDateTerm, PatEPOActionMapAct> epoActMapActService,
            IEntityService<PatEPOAppLog> epoAppLogService,
            IEntityService<PatEPOCommActLog> epoCommActLogService,
            IEntityService<PatEPODDActLog> epoDDActLogService,
            IEntityService<EPOCommunication> epoCommunicationService,
            IParentEntityService<EPOCommunication, EPOCommunicationDoc> epoCommunicationDocService,
            IEntityService<PatEPOMailLog> epoMailLogService,
            IEntityService<PatEPODocumentCombined> epoDocCombinedService,
            IParentEntityService<PatEPODocumentMerge, PatEPODocumentMergeGuide> epoDocMergeService,
            IParentEntityService<PatEPODocumentMergeGuide, PatEPODocumentMergeGuideSub> epoDocMergeGuideService,
            IEntityService<PatEPODocumentMap> epoDocMapService,
            IChildEntityService<PatEPODocumentMap, PatEPODocumentMapAct> epoDocMapActService,
            IChildEntityService<PatEPODocumentMap, PatEPODocumentMapTag> epoDocMapTagService,
            IEntityService<DocDocumentTag> documentTagService,
            ICountryApplicationService applicationService,
            IPatActionDueViewModelService actionDueViewModelService,
            IActionDueService<PatActionDue, PatDueDate> actionDueService,
            IAMSDueService amsDueService,
            INumberFormatService numberFormatService,
            IDocumentHelper documentHelper,
            IDocumentService docService,
            IDocumentsViewModelService docViewModelService,
            IDocumentStorage documentStorage,
            ISharePointService sharePointService,
            ISharePointViewModelService sharePointViewModelService,
            IOptions<GraphSettings> graphSettings,
            IOptions<iManageSettings> iManageSettings,
            IiManageClientFactory iManageClientFactory,
            IiManageViewModelService iManageViewModelService,
            IUrlHelper url,
            ILogger<EPOService> logger,
            INetDocumentsViewModelService netDocsViewModelService
            )
        {
            _mapper = mapper;
            _patSettings = patSettings;
            _user = user;

            _epoMailboxSettings = epoMailboxSettings.Value;
            _epoOPSSettings = epoOPSSettings.Value;

            _epoOPSLogService = epoOPSLogService;

            _epoPortfolioService = epoPortfolioService;
            _epoApplicationService = epoApplicationService;
            _epoDueDateService = epoDueDateService;

            _epoDueDateTermService = epoDueDateTermService;
            _epoActMapActService = epoActMapActService;
            _epoAppLogService = epoAppLogService;
            _epoCommActLogService = epoCommActLogService;
            _epoDDActLogService = epoDDActLogService;

            _epoCommunicationService = epoCommunicationService;
            _epoCommunicationDocService = epoCommunicationDocService;

            _epoMailLogService = epoMailLogService;
            _epoDocCombinedService = epoDocCombinedService;
            _epoDocMergeService = epoDocMergeService;
            _epoDocMergeGuideService = epoDocMergeGuideService;

            _epoDocMapService = epoDocMapService;
            _epoDocMapActService = epoDocMapActService;
            _epoDocMapTagService = epoDocMapTagService;
            _documentTagService = documentTagService;

            _applicationService = applicationService;
            _actionDueViewModelService = actionDueViewModelService;
            _actionDueService = actionDueService;

            _amsDueService = amsDueService;

            _numberFormatService = numberFormatService;

            _documentHelper = documentHelper;
            _docService = docService;
            _docViewModelService = docViewModelService;
            _documentStorage = documentStorage;
            _sharePointService = sharePointService;
            _sharePointViewModelService = sharePointViewModelService;
            _graphSettings = graphSettings.Value;

            _iManageSettings = iManageSettings.Value;
            _iManageClientFactory = iManageClientFactory;
            _iManageViewModelService = iManageViewModelService;

            _netDocsViewModelService = netDocsViewModelService;

            _url = url;
            _logger = logger;
        }

        #region MyEPO
        public bool IsMyEPOAPIOn()
        {
            return _epoMailboxSettings.IsAPIOn;
        }

        public async Task ResetSandbox()
        {
            //Get access_token
            var access_token = await GetMyEPOAccessToken();

            if (string.IsNullOrEmpty(access_token)) throw new Exception("Missing access_token.");

            var settings = await _patSettings.GetSetting();
            string serviceUrl = settings.MyEPOURL ?? "";

            if (string.IsNullOrEmpty(serviceUrl)) throw new Exception("Missing cpi api url.");

            string searchUrl = $"{serviceUrl}/Mailbox/ResetSandbox";
            using (var client = new HttpClient())
            {
                client.Timeout = Timeout.InfiniteTimeSpan;
                using (var request = new HttpRequestMessage())
                {
                    //Prepare and send request - START
                    request.RequestUri = new Uri(searchUrl);
                    request.Method = HttpMethod.Post;

                    var jsonData = JsonConvert.SerializeObject(access_token);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;

                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        var responseMsg = response.Content.ReadAsStringAsync().Result;
                        throw new Exception("Error within API: " + responseMsg);
                    }
                    ;
                    //var stringResponse = await response.Content.ReadAsStringAsync();
                    //Prepare and send request - END
                    return;
                }
            }
        }

        public string CleanSearchNumber(string inputStr)
        {
            var parsedStr = inputStr;

            if (string.IsNullOrEmpty(parsedStr)) return inputStr;

            if (parsedStr.IndexOf(".") > -1) parsedStr = parsedStr.Split(".")[0];

            if (parsedStr.ToLower().StartsWith("ep")) parsedStr = parsedStr.Substring(2);
            else if (parsedStr.ToLower().StartsWith("pct/")) parsedStr = parsedStr.Substring(4);

            return parsedStr;
        }

        #region Mailbox

        public async Task<int> GetEPODocumentCodes()
        {
            var settings = await _patSettings.GetSetting();
            if (string.IsNullOrEmpty(settings?.MyEPOURL)) throw new InvalidOperationException("Missing EPO API URL setting.");

            var userName = _user.GetUserName();
            var serviceUrl = settings.MyEPOURL;

            using var client = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan,
                BaseAddress = new Uri(serviceUrl)
            };

            string searchUrl = $"{serviceUrl}/Mailbox/GetDocumentCodes";
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(searchUrl);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error calling EPO API at {searchUrl}: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException($"Request to {searchUrl} timed out.", ex);
            }

            var stringResponse = await response.Content.ReadAsStringAsync();

            List<EPODocumentCode>? remoteCodes;
            try
            {
                remoteCodes = JsonConvert.DeserializeObject<List<EPODocumentCode>>(stringResponse);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to deserialize API response: {ex.Message}", ex);
            }

            if (remoteCodes == null || !remoteCodes.Any()) return 0;


            var existingDocumentMaps = await _epoDocMapService.QueryableList.AsNoTracking().ToListAsync();
            var existingDocumentMapLookup = existingDocumentMaps
                .Where(d => !string.IsNullOrEmpty(d.DocumentCode) && !string.IsNullOrEmpty(d.Language))
                .ToDictionary(d => (d.DocumentCode!.ToLower(), d.Language!.ToLower()), d => d);

            var newEPODocumentMaps = new List<PatEPODocumentMap>();
            var updateEPODocumentMaps = new List<PatEPODocumentMap>();

            foreach (var epoDocumentCode in remoteCodes.Where(d => !string.IsNullOrEmpty(d.DocCode)))
            {
                AddOrUpdateDocumentMap(epoDocumentCode, "en", epoDocumentCode.EnglishDesc ?? "", existingDocumentMapLookup, newEPODocumentMaps, updateEPODocumentMaps, userName);
                AddOrUpdateDocumentMap(epoDocumentCode, "fr", epoDocumentCode.FrenchDesc ?? "", existingDocumentMapLookup, newEPODocumentMaps, updateEPODocumentMaps, userName);
                AddOrUpdateDocumentMap(epoDocumentCode, "de", epoDocumentCode.GermanDesc ?? "", existingDocumentMapLookup, newEPODocumentMaps, updateEPODocumentMaps, userName);
            }

            var totalAffected = 0;
            if (newEPODocumentMaps.Any())
            {
                var uniqueNewMaps = newEPODocumentMaps
                    .GroupBy(d => new { d.DocumentCode, d.Language })
                    .Select(g => g.First())
                    .ToList();

                await _epoDocMapService.Add(uniqueNewMaps);
                totalAffected += uniqueNewMaps.Count;
            }

            if (updateEPODocumentMaps.Any())
            {
                var uniqueUpdateMaps = updateEPODocumentMaps
                    .GroupBy(d => new { d.DocumentCode, d.Language })
                    .Select(g => g.First())
                    .ToList();

                await _epoDocMapService.Update(uniqueUpdateMaps);
                totalAffected += uniqueUpdateMaps.Count;
            }

            return totalAffected;
        }

        public async Task<List<EPOMailboxOutput>> GetUnhandledCommunications(EPOMailboxInput searchInput)
        {
            //Get access_token
            var access_token = await GetMyEPOAccessToken();

            if (string.IsNullOrEmpty(access_token)) throw new Exception("Missing access_token.");

            searchInput.access_token = access_token;

            searchInput.SearchField = _epoMailboxSettings.SearchOption == 3 ? "applicationNumber" : "userReference";

            var settings = await _patSettings.GetSetting();
            string serviceUrl = settings.MyEPOURL ?? "";

            if (string.IsNullOrEmpty(serviceUrl)) throw new Exception("Missing cpi api url.");

            if (DateTime.TryParseExact(settings.MyEPODownloadDateFrom, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                searchInput.DispatchDateFrom = parsedDate;

            string searchUrl = $"{serviceUrl}/Mailbox/GetCommunications";
            using (var client = new HttpClient())
            {
                client.Timeout = Timeout.InfiniteTimeSpan;
                //client.Timeout = TimeSpan.FromMinutes(patIDSSearchApi.TimeOut);

                using (var request = new HttpRequestMessage(HttpMethod.Post, new Uri(searchUrl)))
                {
                    //Prepare request body
                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    //Send request
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        var responseMsg = await response.Content.ReadAsStringAsync();
                        throw new Exception("Error within API: " + responseMsg);
                    }
                    ;
                    var stringResponse = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<List<EPOMailboxOutput>>(stringResponse);
                    if (result != null && result.Count > 0)
                        return result;
                }
            }
            return new List<EPOMailboxOutput>();
        }

        public async Task<int> DownloadEPOMailDocuments(List<int> appIds, int logId = 0)
        {
            var resultCount = 0;
            var settings = await _patSettings.GetSetting();
            DateTime runDate = DateTime.Now;
            var userName = _user.GetUserName();

            var searchInput = new EPOMailboxInput()
            {
                access_token = "",
                SearchField = "",
                DiffTolerance = _epoMailboxSettings.DiffTolerance,
                SearchInputs = new List<EPOMailboxSearchInput>()
            };

            //Check if record has AppNumber/PCTNumber and standardize value(s)
            //Get data from provided appIds
            var applications = await _applicationService.CountryApplications.AsNoTracking()
                                    .Where(d => appIds.Contains(d.AppId))
                                    .Select(app => new
                                    {
                                        app.AppId,
                                        app.CaseNumber,
                                        app.Country,
                                        app.SubCase,
                                        app.AppClientRef,
                                        app.PatNumber,
                                        app.PCTNumber,
                                        app.CaseType,
                                        app.AppNumber,
                                        app.PubNumber,
                                        app.FilDate,
                                        app.PubDate,
                                        app.IssDate,
                                        app.PCTDate
                                    }).Distinct().ToListAsync();

            if (applications.Count > 0)
            {
                foreach (var app in applications)
                {
                    var parsedData = string.Empty;

                    if (_epoMailboxSettings.SearchOption == 0)
                    {
                        parsedData = app.CaseNumber;
                    }
                    else if (_epoMailboxSettings.SearchOption == 1)
                    {
                        parsedData = app.CaseNumber + app.Country + (string.IsNullOrEmpty(app.SubCase) ? "" : app.SubCase);
                    }
                    else if (_epoMailboxSettings.SearchOption == 2)
                    {
                        parsedData = app.AppClientRef;
                    }
                    else if (_epoMailboxSettings.SearchOption == 3)
                    {
                        //Standardize number(s)                        
                        WebLinksNumberInfoDTO tempData = new WebLinksNumberInfoDTO();
                        tempData.SystemType = WebLinksSystemType.Patent;
                        tempData.Country = app.Country;
                        tempData.CaseType = app.CaseType;
                        tempData.AppNumber = app.AppNumber;
                        tempData.PubNumber = app.PubNumber;
                        tempData.PatRegNumber = app.PatNumber;
                        tempData.FilDate = app.FilDate;
                        tempData.PubDate = app.PubDate;
                        tempData.IssRegDate = app.IssDate;
                        tempData.Number = _numberFormatService.CleanUpNumber(app.AppNumber ?? "");
                        tempData.NumberDate = app.FilDate;
                        tempData.NumberType = WebLinksNumberType.AppNo;

                        if (string.IsNullOrEmpty(tempData.Number)) continue;

                        try
                        {
                            parsedData = await _numberFormatService.FormatNumber(tempData, WebLinksTemplateType.Web);
                        }
                        catch (Exception ex) { var error = ex.Message; continue; }

                        if (parsedData.StartsWith("EP"))
                        {
                            var standardTemplates = await _numberFormatService.GetNumberTemplates(tempData.SystemType, tempData.Country, tempData.CaseType ?? "", tempData.NumberType, WebLinksTemplateType.Web, "");
                            parsedData = _numberFormatService.FormatNumber(tempData, standardTemplates, "\"EP\"YYNNNNNN+");
                        }

                        if (string.IsNullOrEmpty(parsedData)) continue;
                    }
                    else
                        parsedData = app.CaseNumber;

                    searchInput.SearchInputs.Add(new EPOMailboxSearchInput() { AppId = app.AppId, SearchStr = parsedData });
                }
            }

            if (searchInput.SearchInputs == null || searchInput.SearchInputs.Count <= 0) return resultCount;

            searchInput.SearchInputs.ForEach(d => { d.SearchStr = CleanSearchNumber(d.SearchStr ?? ""); });

            //Get downloaded communicationIds
            var searchAppIds = searchInput.SearchInputs.Select(d => d.AppId).Distinct().ToList();
            var epoLogs = await _epoMailLogService.QueryableList.AsNoTracking().Where(d => searchAppIds.Contains(d.AppId)).ToListAsync();
            if (epoLogs != null && epoLogs.Count > 0)
            {
                foreach (var srchInput in searchInput.SearchInputs)
                {
                    var existing_commIds = epoLogs.Where(d => d.AppId == srchInput.AppId).Select(d => d.CommunicationId ?? "").Where(d => !string.IsNullOrEmpty(d)).ToList();
                    if (existing_commIds != null && existing_commIds.Count > 0) srchInput.ExistingCommIds = existing_commIds;
                }
            }

            //Get unhandled communications from MyEPO mailbox and filter out data
            var result = await GetUnhandledCommunications(searchInput);
            if (result != null && result.Count() > 0)
            {
                await ProcessDownloadedCommunication(result, logId, searchInput, runDate, epoLogs);
                resultCount = result.Count;
            }

            return resultCount;
        }

        public async Task ProcessDownloadedCommunication(List<EPOMailboxOutput> result, int logId, EPOMailboxInput searchInput, DateTime runDate, List<PatEPOMailLog>? epoLogs)
        {
            var settings = await _patSettings.GetSetting();
            var userName = _user.GetUserName();

            var newLogs = new List<PatEPOMailLog>();
            var newEPOCommunications = new List<EPOCommunication>();
            var newEPOCommuniocationDocs = new List<EPOCommunicationDoc>();

            //Get list of document codes/names for saving/downloading
            var epoDocumentMaps = await _epoDocMapService.QueryableList.AsNoTracking()
                                    .Where(d => d.Enabled && !string.IsNullOrEmpty(d.DocumentCode))
                                    .Select(d => new { d.DocumentCode, d.DocumentName, d.IsActRequired, d.CheckAct, d.SendToClient, d.IsGenAction })
                                    .ToListAsync();
            
            var epoDocumentMapLookup = epoDocumentMaps
                .GroupBy(grp => grp.DocumentCode!)
                .ToDictionary(
                    g => g.Key, 
                    v => new { 
                        IsActRequired = v.Any(e => e.IsActRequired), 
                        CheckAct = v.Any(e => e.CheckAct), 
                        SendToClient = v.Any(e => e.SendToClient) 
                    });
            var epoDocumentMapsCodeList = epoDocumentMapLookup.Keys.ToHashSet();
            
            // Get list of document codes where IsGenAction is true
            var genActionDocumentCodes = epoDocumentMaps
                .Where(d => d.IsGenAction)
                .Select(d => d.DocumentCode!)
                .ToHashSet();

            var epoDocumentMapTags = await _epoDocMapTagService.QueryableList.AsNoTracking()
                                    .Where(d => !string.IsNullOrEmpty(d.DocumentCode) && epoDocumentMapsCodeList.Contains(d.DocumentCode))
                                    .Select(d => new { d.DocumentCode, d.Tag }).Distinct().ToListAsync();
            var epoDocumentMapActs = await _epoDocMapActService.QueryableList.AsNoTracking()
                                    .Where(d => !string.IsNullOrEmpty(d.DocumentCode) && genActionDocumentCodes.Contains(d.DocumentCode))
                                    .Select(d => new { d.DocumentCode, d.ActionType, d.ActionDue, d.Yr, d.Mo, d.Dy, d.Indicator }).Distinct().ToListAsync();

            //List of creating new actions from mapped actions
            var mappedActions = new List<(string communicationId, int appId, string documentCode, DateTime? dispatchDate)>();

            //Get list of merged mappings with 1 Guide            
            var mergedMappings = await _epoDocMergeService.ChildService.QueryableList.AsNoTracking()
                                    .Where(d => d.Map != null && d.Map.Enabled && !string.IsNullOrEmpty(d.GuideFileName))
                                    .Select(d => new
                                    {
                                        MergeId = d.MergeId,
                                        MergeName = d.Map!.MergeName,
                                        StopProcessing = d.Map!.StopProcessing,
                                        DeleteSourceFiles = d.Map.DeleteSourceFiles,
                                        FileName = d.Map.FileName,
                                        MapOrderOfEntry = d.Map.OrderOfEntry,
                                        GuideId = d.GuideId,
                                        GuideCommunicationName = !string.IsNullOrEmpty(d.GuideFileName) ? d.GuideFileName.ToLower().Trim() : "",
                                        OrderOfEntry = d.OrderOfEntry,
                                        EPODocCodes = _epoDocMapService.QueryableList.AsNoTracking().Where(e => e.Enabled == true && !string.IsNullOrEmpty(e.DocumentCode) && !string.IsNullOrEmpty(e.DocumentName) && e.DocumentName.Trim() == (d.GuideFileName ?? "").Trim()).Select(e => e.DocumentCode).Distinct().ToList()
                                    })
                                    .ToListAsync();
            var mergeIdList = mergedMappings.GroupBy(d => d.MergeId).Select(d => new { d.Key, Count = d.Count() }).Where(d => d.Count == 1).Select(d => d.Key).ToList();
            mergedMappings.RemoveAll(d => !mergeIdList.Contains(d.MergeId));

            //Get list of guide-sub for merged mappings with 1 Guide
            //One communication could contains multiple files
            //One guide is 1 comm
            var guideIdList = mergedMappings.Select(d => d.GuideId).Distinct().ToList();
            var mergedGuideSubs = await _epoDocMergeGuideService.ChildService.QueryableList.AsNoTracking().Where(d => guideIdList.Contains(d.GuideId))
                .Select(d => new { d.GuideId, d.OrderOfEntry, d.SubFileName }).Distinct().ToListAsync();

            //**************************************************************************************************************************************************************
            foreach (var item in result)
            {
                if (item == null || item.Communications == null) continue;

                //Loop through each downloaded communication and save file(s) for each communication
                foreach (var comm in item.Communications)
                {
                    //Skip if communicationId is already in the log
                    if (epoLogs != null && epoLogs.Any(d => d.CommunicationId == comm.id)) continue;

                    //Skip if EPO Download List exists and the communication (documentCode/title) is not in the list
                    if (epoDocumentMaps != null && epoDocumentMaps.Count > 0 && !epoDocumentMaps.Any(d =>
                        (!string.IsNullOrEmpty(d.DocumentCode) && !string.IsNullOrEmpty(comm.documentCode) && d.DocumentCode.ToLower().Trim() == comm.documentCode.ToLower().Trim())
                        || (!string.IsNullOrEmpty(d.DocumentName) && !string.IsNullOrEmpty(comm.title) && d.DocumentName.ToLower().Trim() == comm.title.ToLower().Trim())))
                        continue;

                    //Prepare log record
                    var newLog = new PatEPOMailLog()
                    {
                        LogId = logId,
                        AppId = item.AppId,
                        SearchStr = searchInput.SearchInputs != null ? searchInput.SearchInputs.FirstOrDefault(d => d.AppId == item.AppId)?.SearchStr : "",
                        SearchField = _epoMailboxSettings.SearchOption == 3 ? "applicationNumber" : "userReference",
                        CommunicationId = comm.id,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = runDate,
                        LastUpdate = runDate
                    };

                    var newEPOCommunication = _mapper.Map<EPOCommunication>(comm);
                    newEPOCommunication.LogId = logId;
                    newEPOCommunication.CreatedBy = userName;
                    newEPOCommunication.UpdatedBy = userName;
                    newEPOCommunication.DateCreated = runDate;
                    newEPOCommunication.LastUpdate = runDate;

                    //Get EPO Document Mapped Tags and Actions list
                    var mappedTagList = new List<string>();
                    if (epoDocumentMaps != null && epoDocumentMaps.Count > 0)
                    {
                        // Get mapped tags to be added to new document(s) later
                        mappedTagList = epoDocumentMapTags.Where(d => !string.IsNullOrEmpty(d.Tag)
                                                            && epoDocumentMaps.Any(e => !string.IsNullOrEmpty(e.DocumentCode) && !string.IsNullOrEmpty(e.DocumentName)
                                                                                    && d.DocumentCode == e.DocumentCode
                                                                                    && ((!string.IsNullOrEmpty(comm.documentCode) && e.DocumentCode.ToLower().Trim() == comm.documentCode.ToLower().Trim())
                                                                                        || (!string.IsNullOrEmpty(comm.title) && e.DocumentName.ToLower().Trim() == comm.title.ToLower().Trim()))
                                                                                )
                                                            ).Select(d => d.Tag ?? "").Where(d => !string.IsNullOrEmpty(d))
                                                            .Distinct().ToList();

                        // Get mapped actions to be created later
                        mappedActions.AddRange(epoDocumentMapActs.Where(d => epoDocumentMaps.Any(e => !string.IsNullOrEmpty(e.DocumentCode) && !string.IsNullOrEmpty(e.DocumentName)
                                                                                    && d.DocumentCode == e.DocumentCode
                                                                                    && ((!string.IsNullOrEmpty(comm.documentCode) && e.DocumentCode.ToLower().Trim() == comm.documentCode.ToLower().Trim())
                                                                                        || (!string.IsNullOrEmpty(comm.title) && e.DocumentName.ToLower().Trim() == comm.title.ToLower().Trim()))
                                                                            )
                                                            ).Select(d => (communicationId: comm.id ?? "", appid: item.AppId, documentCode: d.DocumentCode ?? "", dispatchDate: comm.dispatchDate))
                                                            .Distinct().ToList());
                    }

                    //Skip if no documents to process
                    //If no documents to process, save the communication data and skip
                    //OR Skip if no matching CtryApp and DocVerification is off
                    if ((comm.Documents == null || comm.Documents.Count <= 0) || (item.AppId <= 0 && settings.IsDocumentVerificationOn == false && comm.Documents.Count > 0))
                    {
                        if (newLog.AppId > 0) newLogs.Add(newLog);

                        newEPOCommunications.Add(newEPOCommunication);

                        continue;
                    }

                    //Clean up white spaces in document names
                    comm.Documents.ForEach(d => { d.FileName = Regex.Replace(d.FileName ?? "", @"\s+", " "); });

                    //Prepare comm's document(s) and save - START
                    //Check Merged Mapping where mapping only has 1 Guide (also check DocumentName/Title against different language versions by checking DocumentCode)
                    //This is for combine multiple docs in 1 communication into 1 doc and rename
                    //Or just rename the 1 doc in 1 communication
                    var filteredMergeMappings = mergedMappings.Where(d => !string.IsNullOrEmpty(d.GuideCommunicationName)
                                                && !string.IsNullOrEmpty(comm.title)
                                                && (comm.title.Trim().Equals(d.GuideCommunicationName, StringComparison.InvariantCultureIgnoreCase)
                                                    || (d.EPODocCodes != null && d.EPODocCodes.Contains(comm.documentCode)))
                                                ).ToList();
                    var commDocs = new List<DocumentByte>();
                    var unmatchCommDocs = new List<DocumentByte>();
                    //1. Prepare documents to insert if no matching merged mappings to combine/rename
                    //If comm contains multiple documents, they will be saved as individual documents
                    if (filteredMergeMappings == null || filteredMergeMappings.Count <= 0)
                    {
                        foreach (var document in comm.Documents)
                        {
                            if (document.Data == null) continue;
                            var fileName = document.FileName;
                            if (string.IsNullOrEmpty(Path.GetExtension(fileName)) || Path.GetExtension(fileName) != ".pdf")
                                fileName += ".pdf";

                            commDocs.Add(new DocumentByte { FileName = fileName, Data = document.Data });
                        }
                    }
                    //2. Prepare documents with merged mappings: either combine then rename, or just rename
                    else
                    {
                        //Loop through each matching mergeMapping
                        foreach (var mergeMapping in filteredMergeMappings)
                        {
                            if (comm.Documents == null || comm.Documents.Count <= 0) break;

                            var docByteArr = new byte[] { };

                            //If comm contains multiple documents
                            if (comm.Documents.Count > 1)
                            {
                                //Check if there are guide subs (order) for documents inside single communication
                                var filteredGuideSubs = mergedGuideSubs.Where(d => d.GuideId == mergeMapping.GuideId).OrderBy(o => o.OrderOfEntry).ToList();

                                //If merge mapping guide subs exist
                                if (filteredGuideSubs != null && filteredGuideSubs.Count > 0)
                                {
                                    //Combine document with order based on Guide Subs
                                    var orderedFiles = new List<DocumentByte>();
                                    var processedFiles = new HashSet<string>();

                                    //Get documents match with guide subs, in order from Guide Subs
                                    foreach (var guideSub in filteredGuideSubs)
                                    {
                                        if (string.IsNullOrEmpty(guideSub.SubFileName)) continue;

                                        var unOrderedMatchingFiles = comm.Documents
                                            .Where(f => !string.IsNullOrEmpty(f.FileName)
                                                && f.FileName.StartsWith(guideSub.SubFileName, StringComparison.InvariantCultureIgnoreCase)
                                                && f.FileName.EndsWith(".pdf", StringComparison.InvariantCultureIgnoreCase))
                                            .ToList();

                                        if (unOrderedMatchingFiles != null && unOrderedMatchingFiles.Count > 0)
                                        {
                                            var naturalOrderedFileNames = await GetNaturalSorted(unOrderedMatchingFiles.Select(d => d.FileName ?? "").Where(d => !string.IsNullOrEmpty(d)).ToList());

                                            var orderedMatchingFiles = unOrderedMatchingFiles.OrderBy(o =>
                                            {
                                                int index = naturalOrderedFileNames != null ? naturalOrderedFileNames.IndexOf(o.FileName ?? "") : -1;
                                                return index == -1 ? int.MaxValue : index;
                                            }).ToList();

                                            orderedFiles.AddRange(orderedMatchingFiles);
                                            foreach (var file in orderedMatchingFiles)
                                            {
                                                if (!string.IsNullOrEmpty(file.FileName))
                                                    processedFiles.Add(file.FileName);
                                            }
                                        }
                                    }
                                    if (orderedFiles != null && orderedFiles.Count > 0)
                                        docByteArr = CombineByteData(orderedFiles.Where(d => d.Data != null && d.Data.Length > 0).Select(d => d.Data!).ToList());

                                    //Add documents not found in Guide Subs as individual documents
                                    var unmatchingFiles = comm.Documents.Where(d => !string.IsNullOrEmpty(d.FileName)
                                        && !processedFiles.Contains(d.FileName)
                                        && !unmatchCommDocs.Any(u => u.FileName == d.FileName)).ToList();
                                    if (unmatchingFiles != null && unmatchingFiles.Count > 0)
                                    {
                                        var naturalOrderedFileNames = await GetNaturalSorted(unmatchingFiles.Select(d => d.FileName ?? "").Where(d => !string.IsNullOrEmpty(d)).ToList());

                                        unmatchCommDocs.AddRange(unmatchingFiles.OrderBy(o =>
                                            {
                                                int index = naturalOrderedFileNames != null ? naturalOrderedFileNames.IndexOf(o.FileName ?? "") : -1;
                                                return index == -1 ? int.MaxValue : index;
                                            }).ToList());
                                    }
                                }
                                //If not guide subs exist, combine documents with order by file names
                                else
                                {
                                    var naturalOrderedFileNames = await GetNaturalSorted(comm.Documents.Select(d => d.FileName ?? "").Where(d => !string.IsNullOrEmpty(d)).ToList());

                                    docByteArr = CombineByteData(comm.Documents.OrderBy(o =>
                                    {
                                        int index = naturalOrderedFileNames != null ? naturalOrderedFileNames.IndexOf(o.FileName ?? "") : -1;
                                        return index == -1 ? int.MaxValue : index;
                                    }).Where(d => d.Data != null && d.Data.Length > 0).Select(d => d.Data!).ToList());
                                }
                            }
                            //If comm contains 1 document
                            else
                            {
                                docByteArr = comm.Documents.First().Data;
                            }

                            //Rename 
                            var newFileName = mergeMapping.FileName;
                            //Use comm name in case FileName on Merged Mapping is missing
                            if (string.IsNullOrEmpty(newFileName)) newFileName = comm.title;
                            if (string.IsNullOrEmpty(Path.GetExtension(newFileName)) || Path.GetExtension(newFileName) != ".pdf") newFileName += ".pdf";
                            commDocs.Add(new DocumentByte { FileName = newFileName, Data = docByteArr });
                        }
                    }

                    //Add documents without matching Guide Subs as individual documents
                    if (unmatchCommDocs != null && unmatchCommDocs.Count > 0)
                        commDocs.AddRange(unmatchCommDocs);

                    //Save all documents from EPO as indiviual doc for each communication
                    foreach (var document in commDocs)
                    {
                        if (string.IsNullOrEmpty(document.FileName) || document.Data == null) continue;

                        var ms = new MemoryStream(document.Data);

                        // Get EPO Mapping settings for new document
                        var epoMapSettings = epoDocumentMapLookup.GetValueOrDefault(comm.documentCode ?? "");
                        var isActRequired = epoMapSettings != null ? epoMapSettings.IsActRequired : false;
                        var checkAct = epoMapSettings != null ? epoMapSettings.CheckAct : false;
                        var sendToClient = epoMapSettings != null ? epoMapSettings.SendToClient : false;

                        //Save physical file using SharePoint
                        if (settings.DocumentStorage == Core.Entities.Shared.DocumentStorageOptions.SharePoint)
                            await SaveToSharePoint(ms, document.FileName, item.AppId, userName, settings.IsDocumentVerificationOn, DocumentSourceType.EPOMail, false, mappedTagList, 
                                isActRequired, checkAct, sendToClient);
                        else if (settings.DocumentStorage == Core.Entities.Shared.DocumentStorageOptions.iManage)
                            await SaveToIManage(ms, document.FileName, item.AppId, userName, settings.IsDocumentVerificationOn, DocumentSourceType.EPOMail, false, 
                                isActRequired, checkAct, sendToClient);
                        else if (settings.DocumentStorage == Core.Entities.Shared.DocumentStorageOptions.NetDocuments)
                            await _netDocsViewModelService.SaveEPODocument(new FormFile(ms, 0, ms.Length, document.FileName, document.FileName), item.AppId, settings.IsDocumentVerificationOn, DocumentSourceType.EPOMail, false, isActRequired, checkAct, sendToClient);
                        //Save physical file to storage (Azure/FileSystem)
                        else if (settings.DocumentStorage == Core.Entities.Shared.DocumentStorageOptions.BlobOrFileSystem)
                            await SaveToStorage(ms, document.FileName, item.AppId, userName, settings.IsDocumentVerificationOn, DocumentSourceType.EPOMail, false, 
                                isActRequired, checkAct, sendToClient);

                        ms.Dispose();

                        var newDocId = await _docService.DocDocuments.AsNoTracking()
                            .Where(d => d.DocFile != null && d.DocFile.UserFileName == document.FileName
                                && ((item.AppId > 0 && d.DocFolder != null && d.DocFolder.SystemType == SystemTypeCode.Patent && d.DocFolder.ScreenCode == ScreenCode.Application
                                        && d.DocFolder.DataKey == "AppId"
                                        && d.DocFolder.DataKeyValue == item.AppId)
                                    || (item.AppId <= 0 && settings.IsDocumentVerificationOn && d.DocFolder != null
                                        && string.IsNullOrEmpty(d.DocFolder.SystemType) && string.IsNullOrEmpty(d.DocFolder.ScreenCode)
                                        && string.IsNullOrEmpty(d.DocFolder.DataKey) && d.DocFolder.DataKeyValue == 0)
                                    )
                                )
                            .OrderByDescending(o => o.DateCreated).Select(d => d.DocId).FirstOrDefaultAsync();
                        if (newDocId > 0)
                        {
                            newEPOCommuniocationDocs.Add(new EPOCommunicationDoc() { CommunicationId = comm.id, DocId = newDocId, CreatedBy = userName, UpdatedBy = userName, DateCreated = runDate, LastUpdate = runDate, WorkflowStatus = 0 });

                            //Set read to true after downloaded document(s)
                            if (newEPOCommunication.Read == false) newEPOCommunication.Read = true;

                            //Add mapped Tags for blob storage mode
                            if (mappedTagList != null && mappedTagList.Count > 0 && settings.DocumentStorage != Core.Entities.Shared.DocumentStorageOptions.SharePoint)
                            {
                                var docDocumentTags = mappedTagList.Select(d => new DocDocumentTag()
                                {
                                    DocId = newDocId,
                                    Tag = d,
                                    CreatedBy = userName,
                                    DateCreated = runDate,
                                    UpdatedBy = userName,
                                    LastUpdate = runDate,
                                    DocDocument = null
                                }).ToList();
                                await _documentTagService.Add(docDocumentTags);
                            }
                        }
                    }
                    //Prepare comm's document(s) and save - END

                    if (newLog.AppId > 0) newLogs.Add(newLog);

                    newEPOCommunications.Add(newEPOCommunication);
                }
            }

            // Add download logs
            if (newLogs != null && newLogs.Count > 0) await _epoMailLogService.Add(newLogs.Where(d => !string.IsNullOrEmpty(d.CommunicationId)).ToList());

            // Add documents logs and process combining documents
            if (newEPOCommunications != null && newEPOCommunications.Count > 0)
            {
                newEPOCommunications = newEPOCommunications.DistinctBy(d => d.CommunicationId).ToList();

                var newEPOCommunicationIds = newEPOCommunications.Select(d => d.CommunicationId).Distinct().ToList();
                var existingCommIds = await _epoCommunicationService.QueryableList.AsNoTracking().Where(d => newEPOCommunicationIds.Contains(d.CommunicationId)).Select(d => d.CommunicationId).ToListAsync();
                if (existingCommIds != null && existingCommIds.Count > 0)
                    newEPOCommunications.RemoveAll(d => existingCommIds.Contains(d.CommunicationId));

                await _epoCommunicationService.Add(newEPOCommunications);
                var communicationIdList = newEPOCommunications.Select(d => d.CommunicationId ?? "").Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();

                if (newEPOCommuniocationDocs != null && newEPOCommuniocationDocs.Count > 0)
                    await _epoCommunicationDocService.ChildService.Add(newEPOCommuniocationDocs);

                if (communicationIdList != null && communicationIdList.Count > 0)
                    await CombineDownloadedDocuments(communicationIdList);
            }

            // Check and create mapped actions
            var epoCommActLogs = new List<PatEPOCommActLog>();
            foreach (var mappedAction in mappedActions.Distinct())
            {
                var actionBaseDate = mappedAction.dispatchDate != null ? mappedAction.dispatchDate.Value.Date : runDate.Date;
                var newActIds = await _applicationService.GenerateEPODocMappedAction(mappedAction.appId, mappedAction.documentCode, actionBaseDate);
                if (newActIds != null && newActIds.Count > 0)
                {
                    epoCommActLogs.AddRange(newActIds.Select(d => new PatEPOCommActLog()
                    {
                        LogId = logId,
                        CommunicationId = mappedAction.communicationId,
                        ActId = d,
                        CreatedBy = userName,
                        DateCreated = runDate,
                        UpdatedBy = userName,
                        LastUpdate = runDate
                    }).ToList());
                }
            }

            // Log created action(s)
            if (epoCommActLogs != null && epoCommActLogs.Count > 0)
            {
                //Filter out duplicates
                var uniqueCommIds = epoCommActLogs.Select(d => d.CommunicationId).Distinct().ToList();
                var existingLogs = await _epoCommActLogService.QueryableList.AsNoTracking().Where(d => uniqueCommIds.Contains(d.CommunicationId)).ToListAsync();
                epoCommActLogs.RemoveAll(d => existingLogs.Any(e => e.CommunicationId == d.CommunicationId && e.ActId == d.ActId));
                if (epoCommActLogs != null && epoCommActLogs.Count > 0)
                {
                    await _epoCommActLogService.Add(epoCommActLogs);

                    // Link action(s) to epo document(s) if DocVerification is on
                    if (settings.IsDocumentVerificationOn)
                        await LinkEPODocumentAction(epoCommActLogs);
                }                    
            }
        }

        private async Task CombineDownloadedDocuments(List<string> communicationIds)
        {
            var userName = _user.GetUserName();

            ///Checking for merge is based on communication name
            ///If 1 communication contains multiple files, order files by file names ASC

            ///Main Steps:
            ///1. Get all new downloaded EPO Communications from communicationIds parameter
            ///2. Get all merge mappings
            ///3. Filter out merge mappings where mappings contains communication name of new downloaded EPO communications
            ///4. Get all appIds linked to downloaded EPO communications
            ///5. Loop through all appIds from 4 to check/process merging            

            //1. Get all new downloaded EPO Communications from communicationIds parameters
            var newCommunications = await _epoCommunicationService.QueryableList.AsNoTracking()
                                .Where(d => !string.IsNullOrEmpty(d.CommunicationId) && communicationIds.Contains(d.CommunicationId) && d.CommunicationDocs != null)
                                .Select(d => new
                                {
                                    CommunicationId = d.CommunicationId,
                                    CommunicationName = (d.Title ?? "").ToLower()
                                }).Distinct().ToListAsync();

            if (newCommunications == null || newCommunications.Count() == 0) return;

            //2. Get all document merge mappings
            var epoMergeMappings = await _epoDocMergeService.ChildService.QueryableList.AsNoTracking()
                        .Where(d => d.Map != null && d.Map.Enabled && !string.IsNullOrEmpty(d.GuideFileName))
                        .Select(d => new
                        {
                            MergeId = d.MergeId,
                            MergeName = d.Map!.MergeName,
                            StopProcessing = d.Map!.StopProcessing,
                            DeleteSourceFiles = d.Map.DeleteSourceFiles,
                            FileName = d.Map.FileName,
                            MapOrderOfEntry = d.Map.OrderOfEntry,
                            GuideId = d.GuideId,
                            GuideCommunicationName = !string.IsNullOrEmpty(d.GuideFileName) ? d.GuideFileName.ToLower() : "",
                            OrderOfEntry = d.OrderOfEntry,
                            EPODocCodes = _epoDocMapService.QueryableList.AsNoTracking().Where(e => e.Enabled == true
                                                && !string.IsNullOrEmpty(e.DocumentCode) && !string.IsNullOrEmpty(e.DocumentName)
                                                && e.DocumentName.Trim() == (d.GuideFileName ?? "").Trim()
                                                ).Select(e => e.DocumentCode).Distinct().ToList()
                        })
                        .ToListAsync();

            //3. Filter out mappings that have matching communication names with new downloaded communications
            var matchMergeIds = epoMergeMappings.Where(d => !string.IsNullOrEmpty(d.GuideCommunicationName)
                                                && newCommunications.Any(e => !string.IsNullOrEmpty(e.CommunicationName)
                                                        && e.CommunicationName.Equals(d.GuideCommunicationName, StringComparison.InvariantCultureIgnoreCase))
                                                ).Select(d => d.MergeId).Distinct().ToList();

            //Filter out matching merged mappings only and have at least 2 guides  
            epoMergeMappings.RemoveAll(d => !matchMergeIds.Contains(d.MergeId));
            var toRemoveMergeIds = epoMergeMappings.GroupBy(d => d.MergeId).Select(d => new { d.Key, Count = d.Count() }).Where(d => d.Count <= 1).Select(d => d.Key).ToList();
            epoMergeMappings.RemoveAll(d => toRemoveMergeIds.Contains(d.MergeId));

            if (epoMergeMappings == null || epoMergeMappings.Count() == 0) return;

            //Get all possible Guide Subs to avoid repeating pulling data
            var epoMergeMappingGuideSubs = await _epoDocMergeGuideService.ChildService.QueryableList.AsNoTracking()
                .Where(d => d.PatEPODocumentMergeGuide != null && matchMergeIds.Contains(d.PatEPODocumentMergeGuide.MergeId))
                .Select(d => new { d.GuideId, d.SubFileName, d.OrderOfEntry }).ToListAsync();

            //4. Get unique appIds if there are mappings where downloaded epo communication have macthing communication name and mapped name
            var appIds = await _epoMailLogService.QueryableList.AsNoTracking().Where(d => !string.IsNullOrEmpty(d.CommunicationId) && communicationIds.Contains(d.CommunicationId))
                                    .Select(d => d.AppId).Distinct().ToListAsync();

            var settings = await _patSettings.GetSetting();
            var runDate = DateTime.Now;
            var docCombinedLog = new List<PatEPODocumentCombined>();
            var sourceDocIdForDeleteList = new List<DocumentViewModel>();

            //MAIN PROCESSING - START
            //5. Loop through each appId
            foreach (var appId in appIds)
            {
                ///Main Merge Steps:
                ///5.1. Get all EPO Communications linked to the current appId
                ///5.2. Get all existing combined in the current appId
                ///5.3. Filter out merge mappings for the current appId where the EPO Communications' names in the current appId contain the value of the GuideCommunicationName (GuideFileName - tblPatEPODocumentMergeGuide). *Using Contains() because it's similar to LIKE in SQL.
                ///5.4. Loop through all matching merge mappings where its guide count is more than 1

                //5.1. Get all epo communications linked to the appId and have document(s)/file(s)
                var appCommunicationList = await _epoMailLogService.QueryableList.AsNoTracking()
                    .Where(d => d.AppId == appId && d.EPOCommunication != null && d.EPOCommunication.CommunicationDocs != null)
                    .Select(d => new
                    {
                        d.CommunicationId,
                        DocumentCode = d.EPOCommunication != null ? d.EPOCommunication.DocumentCode : "",
                        CommunicationName = d.EPOCommunication != null ? (d.EPOCommunication.Title ?? "").ToLower() : "",
                        d.DateCreated
                    }).ToListAsync();

                //5.2. Get all existing combined communication(s) in the appId
                var uniqueAppCommIds = appCommunicationList.Select(d => d.CommunicationId).Distinct().ToList();
                var existingCombinedList = await _docService.PatEPODocumentCombineds.AsNoTracking()
                                        //.Where(d => d.DocDocument != null && d.DocDocument.DocFolder != null && d.DocDocument.DocFolder.FolderId == folder.FolderId)
                                        .Where(d => !string.IsNullOrEmpty(d.CommunicationId) && uniqueAppCommIds.Contains(d.CommunicationId))
                                        .GroupBy(grp => grp.CombinedDocId)
                                        .Select(d => new
                                        {
                                            CombinedDocId = d.Key,
                                            CombinedCommunications = d.ToList()
                                        })
                                        .ToListAsync();

                //5.3. Filter out mappings for current application based on communication names of the linked communications in the application
                var filteredMergeMappings = epoMergeMappings.Where(d => !string.IsNullOrEmpty(d.GuideCommunicationName)
                                            && appCommunicationList.Any(c => !string.IsNullOrEmpty(c.CommunicationName)
                                                                && (c.CommunicationName.Equals(d.GuideCommunicationName, StringComparison.InvariantCultureIgnoreCase)
                                                                    || (d.EPODocCodes != null && d.EPODocCodes.Contains(c.DocumentCode)))
                                                                )
                                        )
                                        .Select(d => new
                                        {
                                            d.MergeId,
                                            d.MergeName,
                                            d.StopProcessing,
                                            d.DeleteSourceFiles,
                                            d.FileName,
                                            d.MapOrderOfEntry
                                        })
                                        .OrderBy(o => o.MapOrderOfEntry).ThenBy(t => t.MergeName)
                                        .GroupBy(grp => grp.MergeId)
                                        .Select(d => new
                                        {
                                            MergeId = d.Key,
                                            Count = d.Count(),
                                            StopProcessing = d.First().StopProcessing,
                                            DeleteSourceFiles = d.First().DeleteSourceFiles,
                                            FileName = d.First().FileName,
                                            MapOrderOfEntry = d.First().MapOrderOfEntry
                                        })
                                        .ToList();

                bool stopProcessing = false;
                bool isCompleteMerge = false;
                //5.4. Loop through all mappings
                foreach (var mergeMap in filteredMergeMappings)
                {
                    //Stop Processing More Combinations checking - START
                    if (stopProcessing) break;

                    if (mergeMap.StopProcessing) stopProcessing = true;
                    //Stop Processing More Combinations checking - END

                    ///Get files and merge steps
                    ///5.4.1. Get all GuideCommunicationName in the merge mapping
                    ///5.4.2. Loop through all GuideCommunicationName to prepare for merging
                    ///5.4.3. Filter out new combination if already exists
                    ///5.4.4. Check to make sure new combination contains new downloaded communication, if not, remove
                    ///5.4.5. Check if new combination is a complete merge - contains all communications listed on the current mapping
                    ///5.4.6. Loop through new filtered combinations to get actual document files for merging
                    ///5.4.7. Merging files and save/log

                    //5.4.1 Get all GuideCommunicationName in the current merge mapping
                    var mappingGuides = epoMergeMappings.Where(d => d.MergeId == mergeMap.MergeId)
                        .Select(d => new
                        {
                            d.GuideId,
                            GuideCommunicationName = d.GuideCommunicationName.ToLower(),
                            d.OrderOfEntry,
                            DocumentCodes = d.EPODocCodes
                        })
                        .ToList();

                    if (mappingGuides != null && mappingGuides.Count() > 0)
                    {
                        var epoBytes = new List<byte[]>();
                        var tempDocCombinedLog = new List<PatEPODocumentCombined>();
                        var newCombineList = new List<PatEPODocumentCombined>();

                        //5.4.2. Get all matching documents for new combined document and check if the new combination is already existed
                        var logCounter = 1;
                        foreach (var mappingGuide in mappingGuides.OrderBy(o => o.OrderOfEntry).ToList())
                        {
                            if (string.IsNullOrEmpty(mappingGuide.GuideCommunicationName)) continue;

                            //Get all matching EPO Communications in the current appId for the GuideCommunicationName in the current merge mapping
                            newCombineList.AddRange(appCommunicationList
                                .Where(d => !string.IsNullOrEmpty(d.CommunicationName)
                                    && (d.CommunicationName.Equals(mappingGuide.GuideCommunicationName, StringComparison.InvariantCultureIgnoreCase)
                                        || (mappingGuide.DocumentCodes != null && mappingGuide.DocumentCodes.Contains(d.DocumentCode)))
                                )
                                .OrderBy(o => o.CommunicationName).ThenBy(t => t.DateCreated)
                                .Select(d => new PatEPODocumentCombined()
                                {
                                    CombinedDocId = 0,
                                    CommunicationId = d.CommunicationId,
                                    GuideId = mappingGuide.GuideId,
                                    OrderOfEntry = logCounter++,
                                    CreatedBy = userName,
                                    DateCreated = runDate,
                                    UpdatedBy = userName,
                                    LastUpdate = runDate
                                }).ToList());
                        }

                        if (newCombineList == null || newCombineList.Count == 0) continue;

                        var tempNewCombineList = newCombineList.GroupBy(grp => grp.CombinedDocId).Select(d => new
                        {
                            CombinedDocId = d.Key,
                            CombinedCommunications = d.ToList()
                        }).ToList();

                        //5.4.3. Filter out if the new combination is already existed in the system
                        //Ex: existing in system: [{A,B,C}, {A,B}, {A,C}]; new combination: {A,C,D}
                        //First, check if new combination does not exist in system yet
                        //Then, check combinations from system do not match with new combination
                        //Finally, check count between combination in system and new combination
                        if (existingCombinedList != null && existingCombinedList.Count > 0)
                        {
                            //Filter by rules below. If all conditions are true, remove the set of new combination -> new combination already exists;
                            //1. Comparing if set of new combination already exists in any set of existing combination in the system;
                            //2. Same as #1 but reverse to cover case where new combination include 1 or more document(s): comparing if set of existing combination in the system exists in set of new combination;
                            //3. Compare Count in set of new combination and set of existing combination                                    
                            foreach (var existingCombined in existingCombinedList)
                            {
                                foreach (var tempNewCombine in tempNewCombineList)
                                {
                                    if (tempNewCombine.CombinedCommunications.All(d => existingCombined.CombinedCommunications.Any(c => c.CommunicationId == d.CommunicationId && c.OrderOfEntry == d.OrderOfEntry && c.GuideId == d.GuideId))
                                        && existingCombined.CombinedCommunications.All(d => tempNewCombine.CombinedCommunications.Any(c => c.CommunicationId == d.CommunicationId && c.OrderOfEntry == d.OrderOfEntry && c.GuideId == d.GuideId))
                                        && tempNewCombine.CombinedCommunications.Count == existingCombined.CombinedCommunications.Count)
                                    {
                                        //Remove if combination already exists in the system
                                        newCombineList.RemoveAll(d => d.CombinedDocId == tempNewCombine.CombinedDocId);
                                    }
                                }
                            }
                        }

                        //5.4.4. Make sure all sets of new combination must include newly downloaded communications -> to avoid creating new merged files with old communications
                        foreach (var tempNewCombine in tempNewCombineList)
                        {
                            if (!tempNewCombine.CombinedCommunications.Any(d => newCommunications.Any(c => c.CommunicationId == d.CommunicationId)))
                            {
                                newCombineList.RemoveAll(d => d.CombinedDocId == tempNewCombine.CombinedDocId);
                            }
                        }

                        //5.4.5. Check if new combination is a complete merge - containing all communications listed on the mapping                        
                        foreach (var tempNewCombine in tempNewCombineList)
                        {
                            var communicationsForCombine = tempNewCombine.CombinedCommunications.Join(appCommunicationList, t => t.CommunicationId, a => a.CommunicationId, (t, a) => new
                            {
                                t.CommunicationId,
                                a.CommunicationName,
                                t.OrderOfEntry,
                                t.GuideId,
                                a.DocumentCode
                            }).ToList();

                            if (communicationsForCombine == null || communicationsForCombine.Count == 0) continue;

                            if (communicationsForCombine.All(d =>
                                    mappingGuides.Any(c => (d.CommunicationName.Equals(c.GuideCommunicationName, StringComparison.InvariantCultureIgnoreCase) || (c.DocumentCodes != null && c.DocumentCodes.Contains(d.DocumentCode)))
                                        && c.OrderOfEntry == d.OrderOfEntry && c.GuideId == d.GuideId))
                                && mappingGuides.All(d =>
                                    communicationsForCombine.Any(c => (c.CommunicationName.Equals(d.GuideCommunicationName, StringComparison.InvariantCultureIgnoreCase) || (d.DocumentCodes != null && d.DocumentCodes.Contains(c.DocumentCode)))
                                        && c.OrderOfEntry == d.OrderOfEntry && c.GuideId == d.GuideId))
                                && communicationsForCombine.Count == mappingGuides.Count)
                            {
                                isCompleteMerge = true;
                                break;
                            }
                        }

                        //5.4.6. Loop through new filtered combinations to get file streams for merging
                        foreach (var newCombine in newCombineList.OrderBy(o => o.OrderOfEntry).ToList())
                        {
                            //Get all documents/files linked to the EPO Communication
                            //One EPO Communication could contain multiple docs/files. If so, use order of file names as order for merging
                            var communicationDocIds = await _epoCommunicationDocService.ChildService.QueryableList.AsNoTracking()
                                                    .Where(d => d.CommunicationId == newCombine.CommunicationId).Select(d => d.DocId).ToListAsync();

                            if (communicationDocIds == null || communicationDocIds.Count == 0) continue;

                            var docList = await _docService.DocDocuments.AsNoTracking().Where(d => communicationDocIds.Contains(d.DocId) && d.Source == DocumentSourceType.EPOMail)
                                                    .Select(d => new
                                                    {
                                                        SystemType = d.DocFolder != null ? d.DocFolder.SystemType : "",
                                                        DocId = d.DocId,
                                                        DriveItemId = d.DocFile != null ? d.DocFile.DriveItemId : "",
                                                        DocFileName = d.DocFile != null ? d.DocFile.DocFileName : "",
                                                        UserFileName = d.DocFile != null ? (d.DocFile.UserFileName ?? "").ToLower() : ""
                                                    }).ToListAsync();

                            //Get file stream for merging
                            bool hasFileStream = false;

                            //Get Guide Subs
                            var mappingGuideSubs = epoMergeMappingGuideSubs.Where(d => d.GuideId == newCombine.GuideId).OrderBy(o => o.OrderOfEntry).ToList();

                            //If Guide Subs exist, use order from Guide Subs
                            //For unmatching, order by UserFileName and add at the end.
                            if (mappingGuideSubs != null && mappingGuideSubs.Count > 0)
                            {
                                var guideSubProcessedFiles = new HashSet<string>();
                                //Get documents match with guide subs, in order from Guide Subs
                                foreach (var guideSub in mappingGuideSubs)
                                {
                                    if (string.IsNullOrEmpty(guideSub.SubFileName)) continue;

                                    var unOrderedMatchingFiles = docList
                                        .Where(f => !string.IsNullOrEmpty(f.UserFileName)
                                            && f.UserFileName.StartsWith(guideSub.SubFileName, StringComparison.InvariantCultureIgnoreCase)
                                            && f.UserFileName.EndsWith(".pdf", StringComparison.InvariantCultureIgnoreCase))
                                        .ToList();

                                    if (unOrderedMatchingFiles != null && unOrderedMatchingFiles.Count > 0)
                                    {
                                        var naturalOrderedFileNames = await GetNaturalSorted(unOrderedMatchingFiles.Select(d => d.UserFileName ?? "").Where(d => !string.IsNullOrEmpty(d)).ToList());

                                        var orderedMatchingFiles = unOrderedMatchingFiles.OrderBy(o =>
                                        {
                                            int index = naturalOrderedFileNames != null ? naturalOrderedFileNames.IndexOf(o.UserFileName ?? "") : -1;
                                            return index == -1 ? int.MaxValue : index;
                                        }).ToList();

                                        foreach (var matchedFile in orderedMatchingFiles)
                                        {
                                            if (!string.IsNullOrEmpty(matchedFile.UserFileName))
                                                guideSubProcessedFiles.Add(matchedFile.UserFileName);

                                            var fileByteData = await GetFileByteData(matchedFile.SystemType, matchedFile.DriveItemId, matchedFile.DocFileName);
                                            if (fileByteData != null && fileByteData.Length > 0)
                                            {
                                                epoBytes.Add(fileByteData);
                                                hasFileStream = true;
                                            }

                                            if (isCompleteMerge && mergeMap.DeleteSourceFiles)
                                            {
                                                sourceDocIdForDeleteList.Add(new DocumentViewModel() { DocId = matchedFile.DocId, DriveItemId = matchedFile.DriveItemId });
                                            }
                                        }
                                    }
                                }

                                //Get documents with no match in guide subs, and add at the end with order by UserFileName
                                var unmatchingFiles = docList.Where(d => (!string.IsNullOrEmpty(d.DriveItemId) || !string.IsNullOrEmpty(d.DocFileName))
                                    && !string.IsNullOrEmpty(d.UserFileName)
                                    && !guideSubProcessedFiles.Contains(d.UserFileName)).ToList();
                                if (unmatchingFiles != null && unmatchingFiles.Count > 0)
                                {
                                    var naturalOrderedFileNames = await GetNaturalSorted(unmatchingFiles.Select(d => d.UserFileName ?? "").Where(d => !string.IsNullOrEmpty(d)).ToList());

                                    foreach (var file in unmatchingFiles.OrderBy(o =>
                                    {
                                        int index = naturalOrderedFileNames != null ? naturalOrderedFileNames.IndexOf(o.UserFileName ?? "") : -1;
                                        return index == -1 ? int.MaxValue : index;
                                    }).ToList())
                                    {
                                        var fileByteData = await GetFileByteData(file.SystemType, file.DriveItemId, file.DocFileName);
                                        if (fileByteData != null && fileByteData.Length > 0)
                                        {
                                            epoBytes.Add(fileByteData);
                                            hasFileStream = true;
                                        }

                                        if (isCompleteMerge && mergeMap.DeleteSourceFiles)
                                        {
                                            sourceDocIdForDeleteList.Add(new DocumentViewModel() { DocId = file.DocId, DriveItemId = file.DriveItemId });
                                        }
                                    }
                                }
                            }
                            //If Guide Subs don't exist, use order by UserFileName
                            else
                            {
                                var naturalOrderedFileNames = await GetNaturalSorted(docList.Select(d => d.UserFileName ?? "").Where(d => !string.IsNullOrEmpty(d)).ToList());

                                foreach (var commDoc in docList.OrderBy(o =>
                                {
                                    int index = naturalOrderedFileNames != null ? naturalOrderedFileNames.IndexOf(o.UserFileName ?? "") : -1;
                                    return index == -1 ? int.MaxValue : index;
                                }).ToList())
                                {
                                    var fileByteData = await GetFileByteData(commDoc.SystemType, commDoc.DriveItemId, commDoc.DocFileName);
                                    if (fileByteData != null && fileByteData.Length > 0)
                                    {
                                        epoBytes.Add(fileByteData);
                                        hasFileStream = true;
                                    }

                                    if (isCompleteMerge && mergeMap.DeleteSourceFiles)
                                    {
                                        sourceDocIdForDeleteList.Add(new DocumentViewModel() { DocId = commDoc.DocId, DriveItemId = commDoc.DriveItemId });
                                    }
                                }
                            }

                            //Log if epo communication has at least 1 documnet/file - avoid issue where communication doesn't have any documents/files
                            if (hasFileStream)
                                tempDocCombinedLog.Add(newCombine);
                        }

                        //5.4.7. Merging files and save
                        if (epoBytes.Count > 0)
                        {
                            byte[]? mergedPDF = CombineByteData(epoBytes);

                            if (mergedPDF != null && mergedPDF.Length > 0)
                            {
                                //Prepare file name for new merged file
                                var fileExtension = Path.GetExtension(mergeMap.FileName);
                                fileExtension = string.IsNullOrEmpty(fileExtension) || fileExtension != ".pdf" ? ".pdf" : fileExtension;
                                var fileName = (mergeMap.FileName ?? "").Replace(fileExtension ?? "", "") + fileExtension ?? "";

                                var ms = new MemoryStream(mergedPDF);
                                if (settings.DocumentStorage == Core.Entities.Shared.DocumentStorageOptions.SharePoint)
                                    await SaveToSharePoint(ms, fileName, Convert.ToInt32(appId), userName, settings.IsDocumentVerificationOn, DocumentSourceType.EPOMail, false);
                                else if (settings.DocumentStorage == Core.Entities.Shared.DocumentStorageOptions.iManage)
                                    await SaveToIManage(ms, fileName, appId, userName, settings.IsDocumentVerificationOn, DocumentSourceType.EPOMail, false);
                                else if (settings.DocumentStorage == Core.Entities.Shared.DocumentStorageOptions.NetDocuments)
                                    await _netDocsViewModelService.SaveEPODocument(new FormFile(ms, 0, ms.Length, fileName, fileName), appId, settings.IsDocumentVerificationOn, DocumentSourceType.EPOMail, false);
                                else if (settings.DocumentStorage == Core.Entities.Shared.DocumentStorageOptions.BlobOrFileSystem)
                                    await SaveToStorage(ms, fileName, appId, userName, settings.IsDocumentVerificationOn, DocumentSourceType.EPOMail, false);
                                ms.Dispose();

                                var newDocId = await _docService.DocDocuments.AsNoTracking().Where(d => d.DocFile != null && d.DocFile.UserFileName == fileName
                                                            && d.DocFolder != null && d.DocFolder.SystemType == SystemTypeCode.Patent && d.DocFolder.ScreenCode == ScreenCode.Application
                                                            && d.DocFolder.DataKey == "AppId"
                                                            && d.DocFolder.DataKeyValue == appId).OrderByDescending(o => o.DateCreated).Select(d => d.DocId).FirstOrDefaultAsync();
                                if (newDocId > 0)
                                {
                                    //Log
                                    tempDocCombinedLog.ForEach(d => { d.CombinedDocId = newDocId; });
                                    docCombinedLog.AddRange(tempDocCombinedLog);
                                }
                            }
                        }
                    }
                }
            }
            //MAIN PROCESSING - END

            //Save logs for new merged files
            if (docCombinedLog != null && docCombinedLog.Count > 0) await _epoDocCombinedService.Add(docCombinedLog);

            //Delete source files after complete full merge
            if (sourceDocIdForDeleteList != null && sourceDocIdForDeleteList.Count > 0)
            {
                await DeleteDocuments(userName, sourceDocIdForDeleteList);
            }

        }

        public async Task<List<string>> MarkCommunicationsHandled(List<string> communicationIds)
        {
            var handledIds = new List<string>();

            communicationIds.RemoveAll(d => string.IsNullOrEmpty(d));

            if (communicationIds == null || communicationIds.Count == 0) return handledIds;

            //Get access_token
            var access_token = await GetMyEPOAccessToken();

            if (string.IsNullOrEmpty(access_token)) throw new Exception("Missing access_token.");

            var requestInput = new EPOMailboxCommunicationInput() { access_token = access_token, CommunicationIds = communicationIds };

            var settings = await _patSettings.GetSetting();
            string serviceUrl = settings.MyEPOURL ?? "";

            if (string.IsNullOrEmpty(serviceUrl)) throw new Exception("Missing cpi api url.");

            string searchUrl = $"{serviceUrl}/Mailbox/HandleCommunications";
            using (var client = new HttpClient())
            {
                client.Timeout = Timeout.InfiniteTimeSpan;
                //client.Timeout = TimeSpan.FromMinutes(patIDSSearchApi.TimeOut);

                using (var request = new HttpRequestMessage(HttpMethod.Post, new Uri(searchUrl)))
                {
                    //Prepare request body
                    var jsonData = JsonConvert.SerializeObject(requestInput);
                    request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    //Send request
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        var responseMsg = await response.Content.ReadAsStringAsync();
                        throw new Exception("Error within API: " + responseMsg);
                    }
                    ;
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    //Prepare and send request - END

                    var result = JsonConvert.DeserializeObject<List<EPOmailboxHandleOutput>>(stringResponse);
                    if (result != null && result.Count > 0)
                    {
                        //log error?
                        handledIds = result.Where(d => string.IsNullOrEmpty(d.ErrorMessage) && !string.IsNullOrEmpty(d.CommunicationId))
                                            .Select(d => d.CommunicationId ?? "")
                                            .Where(d => !string.IsNullOrEmpty(d))
                                            .ToList();
                    }
                }
            }
            return handledIds;
        }

        public async Task<List<EPOWorkflowViewModel>> ProcessMailboxWorkflow(int logId)
        {
            var settings = await _patSettings.GetSetting();
            var emailWorkflows = new List<EPOWorkflowViewModel>();

            // --- 1. Workflows for downloaded files (Documents) - START ---
            // Efficiently get all new DocIds for the given LogId
            var newDocIds = await _epoCommunicationDocService.ChildService.QueryableList.AsNoTracking()
                .Where(d => d.Communication != null && d.Communication.LogId == logId && d.WorkflowStatus == 0)
                .Select(d => d.DocId)
                .ToListAsync();

            // Batch fetch document details with related DocFolder/DocFile data
            // Document link format: systemType|screenCode|dataKey|dataKeyValue
            var docList = await _docService.DocDocuments.AsNoTracking().Where(d => newDocIds.Contains(d.DocId))
                .Include(d => d.DocFolder).Include(d => d.DocFile)
                .Select(d => new
                {
                    d.DocId,
                    SystemType = d.DocFolder != null ? d.DocFolder.SystemType : "",
                    ScreenCode = d.DocFolder != null ? d.DocFolder.ScreenCode : "",
                    DataKey = d.DocFolder != null ? d.DocFolder.DataKey : "",
                    DataKeyValue = d.DocFolder != null ? d.DocFolder.DataKeyValue : 0,
                    d.FileId,
                    DocFileName = d.DocFile != null ? d.DocFile.DocFileName : "",
                    UserFileName = d.DocFile != null ? d.DocFile.UserFileName : "",
                })
                .ToListAsync();

            // Remove documents that don't satisfy the data linking criteria if document verification is off
            if (!settings.IsDocumentVerificationOn)
                docList.RemoveAll(d => string.IsNullOrEmpty(d.SystemType) || string.IsNullOrEmpty(d.ScreenCode) || string.IsNullOrEmpty(d.DataKey) || d.DataKeyValue == 0);

            // Get distinct document folder groups (SystemType/ScreenCode/DataKey/DataKeyValue combination)
            var docFolders = docList.Select(d => new { d.SystemType, d.ScreenCode, d.DataKey, d.DataKeyValue }).Distinct().ToList();

            var epoCommDocWorkflows = new List<(int DocId, EPOWorkflowViewModel EmailWorkflow)>();

            //Process workflows for patent
            foreach (var docFolder in docFolders)
            {
                // Identify attachments relevant to the current docFolder group
                var attachments = docList.Where(d => d.SystemType == docFolder.SystemType && d.ScreenCode == docFolder.ScreenCode
                                    && d.DataKey == docFolder.DataKey && d.DataKeyValue == docFolder.DataKeyValue
                                )
                                .Select(vm => new WorkflowEmailAttachmentViewModel { DocId = vm.DocId, FileId = vm.FileId, OrigFileName = vm.UserFileName, FileName = vm.DocFileName, DocParent = docFolder.DataKeyValue })
                                .ToList();

                // --- Patent Application Workflow ---
                if (!string.IsNullOrEmpty(docFolder.SystemType) && !string.IsNullOrEmpty(docFolder.ScreenCode) && !string.IsNullOrEmpty(docFolder.DataKey) && docFolder.DataKeyValue > 0)
                {
                    var countryAppWorkflows = await _docViewModelService.GenerateCountryAppWorkflow(attachments, docFolder.DataKeyValue, false, false, false, string.Empty, string.Empty, true, false, false, string.Empty, string.Empty);

                    if (countryAppWorkflows != null && countryAppWorkflows.Count > 0)
                    {
                        foreach (var wf in countryAppWorkflows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.SendEmail))
                        {
                            if (wf.Attachments != null && wf.Attachments.Any())
                            {
                                foreach (var attachment in wf.Attachments)
                                {
                                    var epoWorkflowVM = new EPOWorkflowViewModel
                                    {
                                        QESetupId = wf.ActionValueId,
                                        AutoAttachImages = wf.AutoAttachImages,
                                        DataKey = "AppId",
                                        DataKeyValue = docFolder.DataKeyValue,
                                        DocId = attachment.DocId,
                                        CommActId = 0,
                                        DDActId = 0,
                                        Error = "",
                                        AttachmentFilter = wf.AttachmentFilter
                                    };

                                    emailWorkflows.Add(epoWorkflowVM);
                                    epoCommDocWorkflows.Add((attachment.DocId, epoWorkflowVM));
                                }
                                ;
                            }
                        }
                    }
                }
                // --- Document Verification Workflow ---
                else if (settings.IsDocumentVerificationOn && (string.IsNullOrEmpty(docFolder.SystemType) && string.IsNullOrEmpty(docFolder.ScreenCode) && string.IsNullOrEmpty(docFolder.DataKey) && docFolder.DataKeyValue == 0))
                {
                    //Process workflows for DocVerification - send email only
                    var dvWorkflowActions = await _applicationService.CheckWorkflowAction(PatWorkflowTriggerType.NewEPOFileDownloaded);

                    // Filter for S-DOCVER-WORKFLOW actions
                    var dvWorkFlowActionsFiltered = dvWorkflowActions?.Where(d => d.Workflow?.SystemScreen != null && "s-docver-workflow".Equals(d.Workflow.SystemScreen.ScreenCode, StringComparison.OrdinalIgnoreCase)).ToList();

                    if (dvWorkFlowActionsFiltered?.Any() == true)
                    {
                        var dvWorkFlows = new List<WorkflowViewModel>();

                        foreach (var item in dvWorkFlowActionsFiltered)
                        {
                            var filteredAttachments = attachments;
                            if (item.Workflow != null && !string.IsNullOrEmpty(item.Workflow.TriggerValueName))
                            {
                                // File name filtering logic
                                var triggerNames = item.Workflow.TriggerValueName.ToLower().Replace("*", "").Split(',');
                                filteredAttachments = attachments.Where(f => triggerNames.Any(tv => !string.IsNullOrEmpty(f.FileName) && f.FileName.ToLower().Contains(tv))).ToList();
                            }

                            if (filteredAttachments.Any())
                            {
                                var dvWorkFlow = new WorkflowViewModel
                                {
                                    ActionTypeId = item.ActionTypeId,
                                    ActionValueId = item.ActionValueId,
                                    AutoAttachImages = item.IncludeAttachments,
                                    Attachments = filteredAttachments
                                };
                                dvWorkFlows.Add(dvWorkFlow);
                            }
                        }
                        foreach (var wf in dvWorkFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.SendEmail))
                        {
                            if (wf.Attachments != null && wf.Attachments.Any())
                            {
                                foreach (var attachment in wf.Attachments)
                                {
                                    var epoWorkflowVM = new EPOWorkflowViewModel
                                    {
                                        QESetupId = wf.ActionValueId,
                                        AutoAttachImages = wf.AutoAttachImages,
                                        DataKey = "AppId",
                                        DataKeyValue = docFolder.DataKeyValue,
                                        DocId = attachment.DocId,
                                        CommActId = 0,
                                        DDActId = 0,
                                        Error = "",
                                        AttachmentFilter = wf.AttachmentFilter
                                    };

                                    emailWorkflows.Add(epoWorkflowVM);
                                    epoCommDocWorkflows.Add((attachment.DocId, epoWorkflowVM));
                                }
                                ;
                            }
                        }
                    }
                }
            }
            // --- Workflows for downloaded files (Documents) - END ---

            // --- 2. Workflows for actions created (Actions) - START ---
            var epoCommActWorkflows = new Dictionary<int, string>();
            var epoCommunicationIds = await _epoCommunicationService.QueryableList.AsNoTracking().Where(d => d.LogId == logId).Select(d => d.CommunicationId).ToListAsync();

            if (epoCommunicationIds != null && epoCommunicationIds.Count > 0)
            {
                var newActIds = await _epoCommActLogService.QueryableList.AsNoTracking()
                    .Where(d => epoCommunicationIds.Contains(d.CommunicationId))
                    .Select(d => d.ActId).Distinct().ToListAsync();

                foreach (var newActId in newActIds)
                {
                    var actionDue = await _actionDueService.QueryableList.AsNoTracking().Where(d => d.ActId == newActId).FirstOrDefaultAsync();
                    if (actionDue != null)
                    {
                        var actEmailWFs = await _actionDueViewModelService.NewOrCompletedActionWorkflow(actionDue, "", true);
                        if (actEmailWFs != null && actEmailWFs.Count > 0)
                        {
                            var epoWorkflowVMs = actEmailWFs.Select(d => new EPOWorkflowViewModel()
                            {
                                QESetupId = d.qeSetupId,
                                AutoAttachImages = d.autoAttachImages,
                                DataKey = "ActId",
                                DataKeyValue = d.id,
                                DocId = 0,
                                CommActId = newActId,
                                DDActId = 0,
                                Error = "",
                                AttachmentFilter = d.attachmentFilter
                            }).ToList();

                            emailWorkflows.AddRange(epoWorkflowVMs);
                            epoCommActWorkflows.Add(newActId, JsonConvert.SerializeObject(epoWorkflowVMs));
                        }
                    }
                }
            }
            // --- Workflows for actions created (Actions) - END ---

            // --- 3. Save Workflows (Combined Logic) - START ---
            // Detach context if there is anything to save
            if ((epoCommActWorkflows != null && epoCommActWorkflows.Count > 0) || (epoCommDocWorkflows != null && epoCommDocWorkflows.Count > 0))
                _docService.DetachAllEntities();

            // Save Action Workflows
            if (epoCommActWorkflows != null && epoCommActWorkflows.Count > 0)
            {
                var epoCommActIds = epoCommActWorkflows.Keys.ToHashSet();

                var epoCommActs = await _epoCommActLogService.QueryableList.Where(d => epoCommActIds.Contains(d.ActId)).ToListAsync();
                if (epoCommActs != null && epoCommActs.Count > 0)
                {
                    epoCommActs.ForEach(d =>
                    {
                        if (epoCommActWorkflows.TryGetValue(d.ActId, out string? workflows))
                        {
                            d.EmailWorkflow = workflows ?? "";
                        }
                    });

                    await _epoCommActLogService.Update(epoCommActs);
                }
            }

            // Save Document Workflows
            if (epoCommDocWorkflows != null && epoCommDocWorkflows.Count > 0)
            {
                // Group all collected document workflows and serialize the list per DocId
                var epoCommDocWorkflowLookup = epoCommDocWorkflows.GroupBy(grp => grp.DocId)
                    .Select(d => new { DocId = d.Key, Workflows = JsonConvert.SerializeObject(d.Select(a => a.EmailWorkflow).ToList()) })
                    .ToDictionary(d => d.DocId, v => v.Workflows);

                var epoCommDocIds = epoCommDocWorkflowLookup.Keys.ToHashSet();
                var epoCommDocs = await _epoCommunicationDocService.ChildService.QueryableList.Where(d => epoCommDocIds.Contains(d.DocId)).ToListAsync();

                if (epoCommDocs != null && epoCommDocs.Count > 0)
                {
                    epoCommDocs.ForEach(d =>
                    {
                        if (epoCommDocWorkflowLookup.TryGetValue(d.DocId, out string? workflows))
                        {
                            d.EmailWorkflow = workflows ?? "";
                        }
                    });

                    await _epoCommunicationDocService.ChildService.Update(epoCommDocs);
                }
            }
            // --- Save Workflows - END ---

            return emailWorkflows;
        }

        private async Task LinkEPODocumentAction(List<PatEPOCommActLog> patEPOCommActLogs)
        {
            try
            {
                if (patEPOCommActLogs == null || patEPOCommActLogs.Count == 0) return;

                patEPOCommActLogs.RemoveAll(d => string.IsNullOrEmpty(d.CommunicationId) || d.ActId <= 0);

                if (patEPOCommActLogs.Count == 0) return;

                var userName = _user.GetUserName();
                var today = DateTime.Now;

                // Group by CommunicationId to get ActIds per CommunicationId
                var logLookup = patEPOCommActLogs
                    .GroupBy(d => d.CommunicationId!)
                    .ToDictionary(d => d.Key, v => v.Select(a => a.ActId).ToHashSet());

                var commIds = logLookup.Keys.ToHashSet();
                // Batch fetch DocIds for the CommunicationIds
                var commDocIdData = await _epoCommunicationDocService.ChildService.QueryableList
                    .Where(d => !string.IsNullOrEmpty(d.CommunicationId) && commIds.Contains(d.CommunicationId))
                    .GroupBy(grp => grp.CommunicationId!)
                    .Select(g => new { Key = g.Key, DocIds = g.Select(a => a.DocId).ToList() })
                    .ToListAsync();
                var commDocIdLookup = commDocIdData.ToDictionary(x => x.Key, x => x.DocIds.ToHashSet());

                var docIds = commDocIdLookup.Values.SelectMany(d => d).Where(d => d > 0).ToHashSet();
                // Batch fetch existing DocVerifications for the DocIds
                var existingVerifData = await _docService.DocVerifications.AsNoTracking()
                    .Where(d => d.DocId > 0 && docIds.Contains(d.DocId ?? 0) && d.ActId > 0)
                    .GroupBy(grp => grp.DocId ?? 0)
                    .Select(g => new { Key = g.Key, ActIds = g.Select(a => a.ActId).ToList() })
                    .ToListAsync();
                var existingDocVerificationLookup = existingVerifData.ToDictionary(x => x.Key, x => x.ActIds.ToHashSet());

                var newDocVerifications = new List<DocVerification>();

                foreach (var log in logLookup)
                {
                    if (!commDocIdLookup.TryGetValue(log.Key, out var commDocIds) || commDocIds == null) 
                        continue;

                    foreach (var commDocId in commDocIds)
                    {
                        // Get new ActIds to be added for the DocVerification
                        var newActIds = existingDocVerificationLookup.TryGetValue(commDocId, out var existingActIds)
                            ? log.Value.Where(d => !existingActIds.Contains(d))
                            : log.Value;

                        newDocVerifications.AddRange(newActIds.Select(actId => new DocVerification()
                        {
                            DocId = commDocId,
                            ActionTypeID = 0,
                            ActId = actId,
                            WorkflowStatus = DocVerificationWorkflowStatus.DoNotProcess,
                            CreatedBy = userName,
                            DateCreated = today,
                            UpdatedBy = userName,
                            LastUpdate = today
                        }));
                    }
                }

                if (newDocVerifications != null && newDocVerifications.Count > 0)
                {
                    await _docService.AddDocVerifications(newDocVerifications);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LinkEPODocumentAction");
            }            
        }
        #endregion

        #region Application

        public async Task<int> GetPortfolios(int logId)
        {
            var userName = _user.GetUserName();
            //Get access_token
            var access_token = await GetMyEPOAccessToken();

            if (string.IsNullOrEmpty(access_token)) throw new Exception("Missing access_token.");

            var settings = await _patSettings.GetSetting();
            string serviceUrl = settings.MyEPOURL ?? "";

            if (string.IsNullOrEmpty(serviceUrl)) throw new Exception("Missing cpi api url.");

            string searchUrl = $"{serviceUrl}/Mailbox/GetPortfolios";
            using (var client = new HttpClient())
            {
                client.Timeout = Timeout.InfiniteTimeSpan;
                //client.Timeout = TimeSpan.FromMinutes(patIDSSearchApi.TimeOut);

                using (var request = new HttpRequestMessage(HttpMethod.Post, new Uri(searchUrl)))
                {
                    // Prepare request body
                    var jsonData = JsonConvert.SerializeObject(new EPOMailboxInput() { access_token = access_token });
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;

                    // Send request
                    HttpResponseMessage response = await client.SendAsync(request);

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        var responseMsg = await response.Content.ReadAsStringAsync();
                        throw new Exception("Error within API: " + responseMsg);
                    }
                    ;
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    var apiPortfolios = JsonConvert.DeserializeObject<List<EPOPortfolioOutput>>(stringResponse);

                    if (apiPortfolios == null || !apiPortfolios.Any()) return 0;

                    // BATCH FETCHING
                    var portfolioIds = apiPortfolios.Select(p => p.id).ToHashSet();

                    var existingPortfolios = await _epoPortfolioService.QueryableList
                        .Where(d => !string.IsNullOrEmpty(d.PortfolioId) && portfolioIds.Contains(d.PortfolioId))
                        .ToDictionaryAsync(d => d.PortfolioId!, d => d);

                    var newEPOPortfolio = new List<EPOPortfolio>();
                    var updateEPOPortfolio = new List<EPOPortfolio>();
                    var updatedBy = string.Format("{0}_{1}", logId, userName);
                    if (updatedBy.Length > 20) updatedBy = updatedBy.Substring(0, 20);

                    // UPSERT LOGIC
                    foreach (var portfolio in apiPortfolios)
                    {
                        if (string.IsNullOrEmpty(portfolio.id)) continue;

                        if (!existingPortfolios.TryGetValue(portfolio.id, out var existingPortfolio))
                        {
                            // INSERT: Portfolio does not exist
                            newEPOPortfolio.Add(new EPOPortfolio()
                            {
                                LogId = logId,
                                PortfolioId = portfolio.id,
                                Name = portfolio.name,
                                Type = portfolio.type,
                                HasFullAccess = portfolio.hasFullAccess ?? false,
                                CreatedBy = userName,
                                UpdatedBy = userName,
                                DateCreated = DateTime.Now,
                                LastUpdate = DateTime.Now
                            });
                        }
                        else
                        {
                            // UPDATE: Portfolio exists
                            existingPortfolio.Name = portfolio.name;
                            existingPortfolio.Type = portfolio.type;
                            existingPortfolio.HasFullAccess = portfolio.hasFullAccess ?? false;
                            existingPortfolio.UpdatedBy = updatedBy;
                            existingPortfolio.LastUpdate = DateTime.Now;

                            updateEPOPortfolio.Add(existingPortfolio);
                        }
                    }

                    if (newEPOPortfolio != null && newEPOPortfolio.Count > 0)
                    {
                        await _epoPortfolioService.Add(newEPOPortfolio);
                    }

                    if (updateEPOPortfolio != null && updateEPOPortfolio.Count > 0)
                    {
                        await _epoPortfolioService.Update(updateEPOPortfolio);
                    }

                    return (newEPOPortfolio != null ? newEPOPortfolio.Count : 0) + (updateEPOPortfolio != null ? updateEPOPortfolio.Count : 0);
                }
            }
        }

        public async Task<int> GetApplications(int logId)
        {
            var userName = _user.GetUserName();            
            var access_token = await GetMyEPOAccessToken();

            if (string.IsNullOrEmpty(access_token)) throw new Exception("Missing access_token.");

            var settings = await _patSettings.GetSetting();
            string serviceUrl = settings.MyEPOURL ?? "";

            if (string.IsNullOrEmpty(serviceUrl)) throw new Exception("Missing cpi api url.");

            //Get list of Portfolio Id to pull application(s)
            var portfolioIds = await _epoPortfolioService.QueryableList.AsNoTracking()
                .Where(d => d.HasFullAccess == true)
                .Select(d => d.PortfolioId).Distinct().ToListAsync();

            if (portfolioIds == null || portfolioIds.Count == 0) throw new Exception("No portfolio Id to get application");

            var searchInput = new { access_token = access_token, PortfolioIds = portfolioIds };
            string searchUrl = $"{serviceUrl}/Mailbox/GetApplications";

            using (var client = new HttpClient())
            {
                client.Timeout = Timeout.InfiniteTimeSpan;

                using (var request = new HttpRequestMessage(HttpMethod.Post, new Uri(searchUrl)))
                {
                    //Prepare request body
                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;

                    //Send request
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        var responseMsg = await response.Content.ReadAsStringAsync();
                        throw new Exception("Error within API: " + responseMsg);
                    }

                    var stringResponse = await response.Content.ReadAsStringAsync();
                    var apiApplications = JsonConvert.DeserializeObject<List<EPOApplicationOutput>>(stringResponse);

                    if (apiApplications == null || !apiApplications.Any()) return 0;

                    var apiPortfolioIds = apiApplications.Select(app => app.portfolioId).ToHashSet();

                    var existingApplicationKeys = (await _epoApplicationService.QueryableList.AsNoTracking()                        
                        .Where(d => apiPortfolioIds.Contains(d.PortfolioId))
                        .ToListAsync()) 
                        .ToDictionary(
                            // Now safe to use the complex key generation in memory                            
                            key => $"{key.AppProcedure}|{key.IpOfficeCode}|{key.AppNumber}|{key.FilDate?.ToShortDateString()}|{key.PortfolioId?.ToString()}",
                            value => value
                        );

                    var newEPOApplications = new List<EPOApplication>();

                    foreach (var newApplication in apiApplications)
                    {
                        var lookupKey = $"{newApplication.appProcedure}|{newApplication.ipOfficeCode}|{newApplication.applicationNumber}|{newApplication.filingDate?.ToShortDateString()}|{newApplication.portfolioId?.ToString()}";

                        if (!existingApplicationKeys.ContainsKey(lookupKey))
                        {
                            newEPOApplications.Add(new EPOApplication()
                            {
                                LogId = logId,
                                AppProcedure = newApplication.appProcedure,
                                IpOfficeCode = newApplication.ipOfficeCode,
                                AppNumber = newApplication.applicationNumber,
                                AppNumberMyEPO = newApplication.applicationNumberMyEpo,
                                FilDate = newApplication.filingDate,
                                ApplicantFileRef = newApplication.applicantFileReference,
                                PortfolioId = newApplication.portfolioId,
                                PortfolioName = newApplication.portfolioName,
                                Procedure = newApplication.procedure,
                                CreatedBy = userName,
                                UpdatedBy = userName,
                                DateCreated = DateTime.Now,
                                LastUpdate = DateTime.Now
                            });
                        }
                    }

                    if (newEPOApplications != null && newEPOApplications.Count > 0)
                    {
                        var uniqueList = newEPOApplications.DistinctBy(d => new { d.AppProcedure, d.IpOfficeCode, d.AppNumber, d.AppNumberMyEPO, d.FilDate }).ToList();
                        await _epoApplicationService.Add(uniqueList);

                        return uniqueList.Count;
                    }

                    return 0;
                }
            }            
        }

        public async Task<int> GetDueDates(int logId)
        {
            var userName = _user.GetUserName();            
            var access_token = await GetMyEPOAccessToken();

            if (string.IsNullOrEmpty(access_token)) throw new Exception("Missing access_token.");

            var settings = await _patSettings.GetSetting();
            string serviceUrl = settings.MyEPOURL ?? "";

            if (string.IsNullOrEmpty(serviceUrl)) throw new Exception("Missing cpi api url.");

            //Get list of application to check for due dates
            var epoApplications = await _epoApplicationService.QueryableList.AsNoTracking()
                .Where(d => !string.IsNullOrEmpty(d.Procedure) && !string.IsNullOrEmpty(d.AppNumberMyEPO))
                .Select(d => new
                {
                    Procedure = d.Procedure,
                    AppNumber = !string.IsNullOrEmpty(d.AppNumberMyEPO) && d.AppNumberMyEPO.IndexOf(".") > -1 ? d.AppNumberMyEPO.Substring(0, d.AppNumberMyEPO.IndexOf(".")) : d.AppNumberMyEPO
                })
                .ToListAsync();

            if (epoApplications == null || epoApplications.Count == 0) throw new Exception("No applications found to check for due dates.");

            var searchInput = new { access_token = access_token, Applications = epoApplications };
            string searchUrl = $"{serviceUrl}/Mailbox/GetDueDates";

            using (var client = new HttpClient())
            {
                client.Timeout = Timeout.InfiniteTimeSpan;

                using (var request = new HttpRequestMessage(HttpMethod.Post, new Uri(searchUrl)))
                {
                    //Prepare request body
                    var jsonData = JsonConvert.SerializeObject(searchInput);
                    request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    //Send request
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        var responseMsg = await response.Content.ReadAsStringAsync();
                        throw new Exception("Error within API: " + responseMsg);
                    }

                    var stringResponse = await response.Content.ReadAsStringAsync();
                    var apiDueDates = JsonConvert.DeserializeObject<List<EPODueDateOutput>>(stringResponse);

                    if (apiDueDates == null || !apiDueDates.Any()) return 0;
                       
                    var apiTermKeys = apiDueDates.Select(d => d.termKey).Where(k => !string.IsNullOrEmpty(k)).ToHashSet();

                    var existingRecordKeys = (await _epoDueDateService.QueryableList.AsNoTracking()                        
                        .Where(d => apiTermKeys.Contains(d.TermKey))
                        .ToListAsync())                        
                        .Select(k => 
                            $"{k.Procedure}|{k.IpOfficeCode}|{k.AppNumber}|{k.FilDate?.Date.ToString("yyyyMMdd") ?? string.Empty}|{k.TermKey}|{k.DueDate?.Date.ToString("yyyyMMdd") ?? string.Empty}"
                        ).ToHashSet();

                    var newEPODueDates = new List<EPODueDate>();

                    foreach (var dueDate in apiDueDates)
                    {
                        var currentKey = $"{dueDate.procedure}|{dueDate.ipOfficeCode}|{dueDate.applicationNumber}|{dueDate.filingDate?.Date.ToString("yyyyMMdd") ?? string.Empty}|{dueDate.termKey}|{dueDate.dueDate?.Date.ToString("yyyyMMdd") ?? string.Empty}";

                        if (!existingRecordKeys.Contains(currentKey))
                        {
                            newEPODueDates.Add(new EPODueDate()
                            {
                                LogId = logId,
                                Procedure = dueDate.procedure,
                                IpOfficeCode = dueDate.ipOfficeCode,
                                AppNumber = dueDate.applicationNumber,
                                AppNumberMyEPO = dueDate.applicationNumberMyEpo,
                                FilDate = dueDate.filingDate,
                                TermKey = dueDate.termKey,
                                DueDate = dueDate.dueDate,
                                Actor = dueDate.actor,
                                CreatedBy = userName,
                                UpdatedBy = userName,
                                DateCreated = DateTime.Now,
                                LastUpdate = DateTime.Now
                            });
                        }
                    }

                    if (newEPODueDates != null && newEPODueDates.Count > 0)
                    {
                        var filteredDueDates = newEPODueDates.DistinctBy(d => new { d.Procedure, d.IpOfficeCode, d.AppNumber, d.AppNumberMyEPO, d.FilDate, d.TermKey, d.DueDate }).ToList();
                        await _epoDueDateService.Add(filteredDueDates);

                        return filteredDueDates.Count;
                    }
                }
            }
            return 0;
        }

        public async Task<int> GetDueDateTerms(int logId)
        {            
            var userName = _user.GetUserName();            
            var access_token = await GetMyEPOAccessToken();

            if (string.IsNullOrEmpty(access_token)) throw new Exception("Missing access_token.");

            var settings = await _patSettings.GetSetting();
            string serviceUrl = settings.MyEPOURL ?? "";

            if (string.IsNullOrEmpty(serviceUrl)) throw new Exception("Missing cpi api url.");

            string searchUrl = $"{serviceUrl}/Mailbox/GetDueDateTerms";
            using (var client = new HttpClient())
            {
                client.Timeout = Timeout.InfiniteTimeSpan;

                using (var request = new HttpRequestMessage(HttpMethod.Post, new Uri(searchUrl)))
                {
                    //Prepare request body
                    var jsonData = JsonConvert.SerializeObject(new { access_token = access_token });
                    var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    request.Content = postData;

                    //Send request
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        var responseMsg = await response.Content.ReadAsStringAsync();
                        throw new Exception("Error within API: " + responseMsg);
                    }
                    
                    var stringResponse = await response.Content.ReadAsStringAsync();                    
                    var apiDueDateTerms = JsonConvert.DeserializeObject<List<EPODueDateTermOutput>>(stringResponse);

                    if (apiDueDateTerms == null || !apiDueDateTerms.Any()) return 0;

                    var apiDueDateTermList = apiDueDateTerms.Where(d => !string.IsNullOrEmpty(d.termKey)).Select(d => d.termKey).ToHashSet();
                    var existingTermsDictionary = await _epoDueDateTermService.QueryableList
                        .Where(d => !string.IsNullOrEmpty(d.TermKey) && apiDueDateTermList.Contains(d.TermKey))
                        .ToDictionaryAsync(d => d.TermKey!, d => d);

                    var updatedBy = string.Format("{0}_{1}", logId.ToString(), userName);
                    if (updatedBy.Length > 20) updatedBy = updatedBy.Substring(0, 20);

                    var newEPODueDateTerms = new List<EPODueDateTerm>();
                    var updateEPODueDateTerms = new List<EPODueDateTerm>();

                    foreach (var term in apiDueDateTerms.Where(d => !string.IsNullOrEmpty(d.termKey)))
                    {
                        if (existingTermsDictionary.TryGetValue(term.termKey!, out var existingTerm))
                        {
                            if (existingTerm.DescriptionEN != term.descriptionEN || 
                                existingTerm.DescriptionFR != term.descriptionFR || 
                                existingTerm.DescriptionDE != term.descriptionDE)
                            {
                                existingTerm.DescriptionEN = term.descriptionEN;
                                existingTerm.DescriptionFR = term.descriptionFR;
                                existingTerm.DescriptionDE = term.descriptionDE;
                                existingTerm.UpdatedBy = updatedBy;
                                existingTerm.LastUpdate = DateTime.Now;

                                updateEPODueDateTerms.Add(existingTerm);
                            }
                        }
                        else
                        {
                            newEPODueDateTerms.Add(new EPODueDateTerm()
                            {
                                LogId = logId,
                                TermKey = term.termKey,
                                DescriptionEN = term.descriptionEN,
                                DescriptionFR = term.descriptionFR,
                                DescriptionDE = term.descriptionDE,
                                CreatedBy = userName,
                                UpdatedBy = userName,
                                DateCreated = DateTime.Now,
                                LastUpdate = DateTime.Now
                            });
                        }
                    }

                    if (newEPODueDateTerms != null && newEPODueDateTerms.Count > 0)
                    {
                        await _epoDueDateTermService.Add(newEPODueDateTerms);
                    }

                    if (updateEPODueDateTerms != null && updateEPODueDateTerms.Count > 0)
                    {
                        await _epoDueDateTermService.Update(updateEPODueDateTerms);
                    }

                    return (newEPODueDateTerms != null ? newEPODueDateTerms.Count : 0)+ (updateEPODueDateTerms != null ? updateEPODueDateTerms.Count : 0);
                }
            }
        }

        public async Task LinkEPOApplications(int logId)
        {
            var userName = _user.GetUserName();

            //Get downloaded applications
            var epoApplications = await _epoApplicationService.QueryableList.AsNoTracking().Select(d => new { d.AppProcedure, d.IpOfficeCode, d.AppNumber, d.FilDate }).Distinct().ToListAsync();

            //Prepare existing CtryApp for matching
            var applications = await _applicationService.CountryApplications.AsNoTracking()
                                               .Where(d => !string.IsNullOrEmpty(d.AppNumber) && (d.Country == "EP" || d.Country == "WO" || d.Country == "UP"))
                                               .Select(d => new { d.AppId, d.Country, d.CaseType, d.AppNumber, d.PubNumber, d.PatNumber, d.FilDate, d.PubDate, d.IssDate })
                                               .ToListAsync();

            var cleansedApps = new List<(int AppId, string Country, string AppNumber, DateTime? FilDate)>();
            foreach (var app in applications)
            {
                var parsedData = string.Empty;
                WebLinksNumberInfoDTO tempData = new WebLinksNumberInfoDTO();
                tempData.SystemType = WebLinksSystemType.Patent;
                tempData.Country = app.Country;
                tempData.CaseType = app.CaseType;
                tempData.AppNumber = app.AppNumber;
                tempData.PubNumber = app.PubNumber;
                tempData.PatRegNumber = app.PatNumber;
                tempData.FilDate = app.FilDate;
                tempData.PubDate = app.PubDate;
                tempData.IssRegDate = app.IssDate;
                tempData.Number = _numberFormatService.CleanUpNumber(app.AppNumber ?? "");
                tempData.NumberDate = app.FilDate;
                tempData.NumberType = WebLinksNumberType.AppNo;

                if (string.IsNullOrEmpty(tempData.Number)) continue;

                try
                {
                    parsedData = await _numberFormatService.FormatNumber(tempData, WebLinksTemplateType.Web);
                }
                catch (Exception ex) { var error = ex.Message; continue; }

                if (parsedData.StartsWith("EP"))
                {
                    var standardTemplates = await _numberFormatService.GetNumberTemplates(tempData.SystemType, tempData.Country, tempData.CaseType ?? "", tempData.NumberType, WebLinksTemplateType.Web, "");
                    parsedData = _numberFormatService.FormatNumber(tempData, standardTemplates, "\"EP\"YYNNNNNN+");
                }

                if (string.IsNullOrEmpty(parsedData)) continue;

                cleansedApps.Add((app.AppId, app.Country, CleanSearchNumber(parsedData), app.FilDate));
            }

            //Match existing data with downloaded applications
            var linkedApps = new List<PatEPOAppLog>();
            foreach (var epoApp in epoApplications)
            {
                if (string.IsNullOrEmpty(epoApp.IpOfficeCode) || string.IsNullOrEmpty(epoApp.AppNumber) || epoApp.FilDate == null) continue;

                var cleansedAppNumber = CleanSearchNumber(epoApp.AppNumber);
                linkedApps.AddRange(cleansedApps.Where(d => d.Country.ToLower() == epoApp.IpOfficeCode.ToLower()
                                            && d.AppNumber.ToLower() == cleansedAppNumber.ToLower()
                                            && d.FilDate != null && d.FilDate.Value.Date == epoApp.FilDate.Value.Date
                                        )
                                        .Select(d => new PatEPOAppLog()
                                        {
                                            LogId = logId,
                                            AppId = d.AppId,
                                            Procedure = epoApp.AppProcedure,
                                            IpOfficeCode = epoApp.IpOfficeCode,
                                            AppNumber = epoApp.AppNumber,
                                            FilDate = epoApp.FilDate,
                                            CreatedBy = userName,
                                            DateCreated = DateTime.Now,
                                            UpdatedBy = userName,
                                            LastUpdate = DateTime.Now
                                        })
                                        .Distinct().ToList());
            }

            //Save links
            if (linkedApps != null && linkedApps.Count > 0)
            {
                var newLinkedApps = new List<PatEPOAppLog>();
                foreach (var linkedApp in linkedApps)
                {
                    if (linkedApp.AppId <= 0 || string.IsNullOrEmpty(linkedApp.Procedure) || string.IsNullOrEmpty(linkedApp.IpOfficeCode) || string.IsNullOrEmpty(linkedApp.AppNumber) || linkedApp.FilDate == null)
                        continue;

                    if (!await _epoAppLogService.QueryableList.AsNoTracking().AnyAsync(d => d.AppId == linkedApp.AppId
                        && !string.IsNullOrEmpty(d.Procedure) && d.Procedure.ToLower() == linkedApp.Procedure.ToLower()
                        && !string.IsNullOrEmpty(d.IpOfficeCode) && d.IpOfficeCode.ToLower() == linkedApp.IpOfficeCode.ToLower()
                        && !string.IsNullOrEmpty(d.AppNumber) && d.AppNumber.ToLower() == linkedApp.AppNumber.ToLower()
                        && d.FilDate != null && d.FilDate.Value.Date == linkedApp.FilDate.Value.Date))
                    {
                        newLinkedApps.Add(linkedApp);
                    }
                }
                if (newLinkedApps != null && newLinkedApps.Count > 0)
                {
                    await _epoAppLogService.Add(newLinkedApps);
                }
            }
        }

        public async Task ProcessDownloadedDueDates(int logId)
        {
            var userName = _user.GetUserName();

            //Only process current download Due Dates, not any previous downloaded
            //Get downloaded due dates
            var epoDueDates = await _epoDueDateService.QueryableList.AsNoTracking().Where(d => d.LogId == logId).ToListAsync();
            var epoAppLinks = await _epoAppLogService.QueryableList.AsNoTracking().ToListAsync();

            if (epoDueDates == null || epoDueDates.Count <= 0 || epoAppLinks == null || epoAppLinks.Count <= 0) return;

            var mappedActions = new List<(int epoDDId, int appId, string termKey, DateTime dueDate)>();
            foreach (var epoDueDate in epoDueDates)
            {
                if (string.IsNullOrEmpty(epoDueDate.TermKey) || string.IsNullOrEmpty(epoDueDate.IpOfficeCode) || string.IsNullOrEmpty(epoDueDate.AppNumber) || epoDueDate.FilDate == null || epoDueDate.DueDate == null)
                    continue;

                var epoMappedActions = await _epoActMapActService.ChildService.QueryableList.AsNoTracking().Where(d => d.EPODueDateTerm != null && !string.IsNullOrEmpty(d.EPODueDateTerm.TermKey) && d.EPODueDateTerm.TermKey.ToLower() == epoDueDate.TermKey.ToLower()).ToListAsync();

                if (epoMappedActions == null || epoMappedActions.Count <= 0) continue;

                var linkedAppIds = epoAppLinks.Where(d => !string.IsNullOrEmpty(d.IpOfficeCode) && !string.IsNullOrEmpty(d.AppNumber) && d.FilDate != null
                                                && d.IpOfficeCode.ToLower() == epoDueDate.IpOfficeCode.ToLower()
                                                && CleanSearchNumber(d.AppNumber.ToLower()) == CleanSearchNumber(epoDueDate.AppNumber.ToLower())
                                                && d.FilDate.Value.Date == epoDueDate.FilDate.Value.Date
                                            )
                                            .Select(d => d.AppId).ToList();

                if (linkedAppIds == null || linkedAppIds.Count <= 0) continue;

                mappedActions.AddRange(linkedAppIds.Select(d => (epoDDId: epoDueDate.EPODDId, appId: d, termKey: epoDueDate.TermKey, dueDate: epoDueDate.DueDate.Value.Date)).ToList());
            }

            var epoDDActLogs = new List<PatEPODDActLog>();
            foreach (var mappedAction in mappedActions)
            {

                //Created actions based on mapped actions
                //Need to calculate BaseDate from DueDate
                var actIds = await _applicationService.GenerateEPOActMappedAction(mappedAction.appId, mappedAction.termKey, mappedAction.dueDate);
                if (actIds != null && actIds.Count > 0)
                {
                    epoDDActLogs.AddRange(actIds.Select(d => new PatEPODDActLog()
                    {
                        LogId = logId,
                        EPODDId = mappedAction.epoDDId,
                        ActId = d,
                        CreatedBy = userName,
                        DateCreated = DateTime.Now,
                        UpdatedBy = userName,
                        LastUpdate = DateTime.Now
                    }).ToList());
                }
            }

            if (epoDDActLogs != null && epoDDActLogs.Count > 0)
            {
                //Filter out duplicates
                var uniqueEPODDId = epoDDActLogs.Select(d => d.EPODDId).Distinct().ToList();
                var existingLogs = await _epoDDActLogService.QueryableList.AsNoTracking().Where(d => uniqueEPODDId.Contains(d.EPODDId)).ToListAsync();
                epoDDActLogs.RemoveAll(d => existingLogs.Any(e => e.EPODDId == d.EPODDId && e.ActId == d.ActId));
                if (epoDDActLogs != null && epoDDActLogs.Count > 0)
                    await _epoDDActLogService.Add(epoDDActLogs);
            }
        }

        public async Task<List<EPOWorkflowViewModel>> ProcessDueDateWorkflow(int logId)
        {
            var settings = await _patSettings.GetSetting();
            var emailWorkflows = new List<EPOWorkflowViewModel>();

            var epoDDActWorkflows = new Dictionary<int, string>();

            // Get all new EPODDIds associated with the LogId
            var newEPODDIds = await _epoDueDateService.QueryableList.AsNoTracking()
                .Where(d => d.LogId == logId).Select(d => d.EPODDId).ToListAsync();

            if (newEPODDIds != null && newEPODDIds.Count > 0)
            {
                // Get all unique ActIds linked to these EPODDIds
                var newActIds = await _epoDDActLogService.QueryableList.AsNoTracking()
                    .Where(d => newEPODDIds.Contains(d.EPODDId))
                    .Select(d => d.ActId).Distinct().ToListAsync();

                foreach (var newActId in newActIds)
                {
                    // Fetch the ActionDue record
                    var actionDue = await _actionDueService.QueryableList.AsNoTracking().FirstOrDefaultAsync(d => d.ActId == newActId);

                    if (actionDue != null)
                    {
                        var actEmailWFs = await _actionDueViewModelService.NewOrCompletedActionWorkflow(actionDue, "", true);
                        if (actEmailWFs != null && actEmailWFs.Count > 0)
                        {
                            var epoWorkflowVMs = actEmailWFs.Select(d => new EPOWorkflowViewModel()
                            {
                                QESetupId = d.qeSetupId,
                                AutoAttachImages = d.autoAttachImages,
                                DataKey = "ActId",
                                DataKeyValue = d.id,
                                DocId = 0,
                                CommActId = 0,
                                DDActId = newActId,
                                Error = "",
                                AttachmentFilter = d.attachmentFilter
                            }).ToList();

                            emailWorkflows.AddRange(epoWorkflowVMs);
                            epoDDActWorkflows.Add(newActId, JsonConvert.SerializeObject(epoWorkflowVMs));
                        }
                    }
                }
            }

            // --- SAVE WORKFLOWS ---
            if (epoDDActWorkflows != null && epoDDActWorkflows.Count > 0)
            {
                _docService.DetachAllEntities();

                var epoDDActIds = epoDDActWorkflows.Keys.ToHashSet();

                // Batch fetch log entities that need updating
                var epoDDActs = await _epoDDActLogService.QueryableList.Where(d => epoDDActIds.Contains(d.ActId)).ToListAsync();

                if (epoDDActs?.Count > 0)
                {
                    epoDDActs.ForEach(d =>
                    {
                        if (epoDDActWorkflows.TryGetValue(d.ActId, out var workflows))
                        {
                            d.EmailWorkflow = workflows;
                        }
                    });
                    await _epoDDActLogService.Update(epoDDActs);
                }
            }

            return emailWorkflows;
        }

        #endregion

        public async Task<IEnumerable<EPOWorkflowViewModel>> GetEPOWorkflows()
        {
            var unprocessedEPOWorkflows = new List<EPOWorkflowViewModel>();

            // --- 1. Communication Documents ---
            var commDocWorkflowsJson = await _epoCommunicationDocService.ChildService.QueryableList.AsNoTracking()
                .Where(d => d.WorkflowStatus == 0 && !string.IsNullOrEmpty(d.EmailWorkflow))
                .Select(d => d.EmailWorkflow ?? "")
                .ToListAsync();

            if (commDocWorkflowsJson?.Any() == true)
                unprocessedEPOWorkflows.AddRange(DeserializeEmailWorkflows(commDocWorkflowsJson));

            // --- 2. Communication Actions ---
            var commActWorkflowsJson = await _epoCommActLogService.QueryableList.AsNoTracking()
                .Where(d => d.WorkflowStatus == 0 && !string.IsNullOrEmpty(d.EmailWorkflow))
                .Select(d => d.EmailWorkflow ?? "")
                .ToListAsync();

            if (commActWorkflowsJson?.Any() == true)
                unprocessedEPOWorkflows.AddRange(DeserializeEmailWorkflows(commActWorkflowsJson));

            // --- 3. Due Date Actions ---
            var ddActWorkflowsJson = await _epoDDActLogService.QueryableList.AsNoTracking()
                .Where(d => d.WorkflowStatus == 0 && !string.IsNullOrEmpty(d.EmailWorkflow))
                .Select(d => d.EmailWorkflow ?? "")
                .ToListAsync();

            if (ddActWorkflowsJson?.Any() == true)
                unprocessedEPOWorkflows.AddRange(DeserializeEmailWorkflows(ddActWorkflowsJson));

            return unprocessedEPOWorkflows;
        }

        private IEnumerable<EPOWorkflowViewModel> DeserializeEmailWorkflows(IEnumerable<string> workflowJsonStrings)
        {
            var resultList = new List<EPOWorkflowViewModel>();

            if (workflowJsonStrings?.Any() != true)
            {
                return resultList;
            }

            foreach (var json in workflowJsonStrings)
            {
                if (string.IsNullOrEmpty(json)) continue;

                try
                {                    
                    var listWorkflows = JsonConvert.DeserializeObject<List<EPOWorkflowViewModel>>(json);

                    if (listWorkflows?.Any() == true)
                    {                        
                        var validWorkflows = listWorkflows.Where(wf => wf.QESetupId > 0).ToList();
                        resultList.AddRange(validWorkflows);
                    }
                }
                catch (JsonException ex)
                {                    
                    // _logger.LogError(ex, $"Failed to deserialize workflow JSON: {json.Substring(0, Math.Min(json.Length, 200))}...");
                }
                catch (Exception ex)
                {                    
                    // _logger.LogError(ex, "An unexpected error occurred during workflow deserialization.");
                }
            }

            return resultList;
        }

        public async Task LogEPOWorkflows(IEnumerable<EPOWorkflowViewModel> emailWorkflows)
        {
            var userName = _user.GetUserName();
            var runDate = DateTime.Now;

            _docService.DetachAllEntities();

            // --- 1. Communication Documents ---
            var commDocWorkflowDict = emailWorkflows.Where(d => d.DocId > 0)
                .GroupBy(d => d.DocId)
                .ToDictionary(g => g.Key, g => g.First().Error);
            var commDocIds = commDocWorkflowDict.Keys.ToHashSet();
            var commDocs = await _epoCommunicationDocService.ChildService.QueryableList.Where(d => commDocIds.Contains(d.DocId)).ToListAsync();

            foreach (var commDoc in commDocs)
            {
                var workflowError = string.Empty;
                if (commDocWorkflowDict.TryGetValue(commDoc.DocId, out var error))
                    workflowError = error;

                commDoc.WorkflowStatus = string.IsNullOrEmpty(workflowError) ? 1 : 2;
                commDoc.WorkflowError = workflowError;
                commDoc.UpdatedBy = userName;
                commDoc.LastUpdate = runDate;
            }
            await _epoCommunicationDocService.ChildService.Update(commDocs);

            // --- 2. Communication Actions ---
            var commActionWorkflowDict = emailWorkflows.Where(d => d.CommActId > 0)
                .GroupBy(d => d.CommActId)
                .ToDictionary(g => g.Key, g => g.First().Error);
            var commActIds = commActionWorkflowDict.Keys.ToHashSet();
            var commActions = await _epoCommActLogService.QueryableList.Where(d => commActIds.Contains(d.ActId)).ToListAsync();

            foreach (var commAct in commActions)
            {
                var workflowError = string.Empty;
                if (commActionWorkflowDict.TryGetValue(commAct.ActId, out var error))
                    workflowError = error;

                commAct.WorkflowStatus = string.IsNullOrEmpty(workflowError) ? 1 : 2;
                commAct.WorkflowError = workflowError;
                commAct.UpdatedBy = userName;
                commAct.LastUpdate = runDate;
            }
            await _epoCommActLogService.Update(commActions);

            // --- 3. Due Date Actions ---
            var ddActionWorkflowDict = emailWorkflows.Where(d => d.DDActId > 0)
                .GroupBy(d => d.DDActId)
                .ToDictionary(g => g.Key, g => g.First().Error);
            var ddActIds = ddActionWorkflowDict.Keys.ToHashSet();
            var ddActions = await _epoDDActLogService.QueryableList.Where(d => ddActIds.Contains(d.ActId)).ToListAsync();

            foreach (var ddAct in ddActions)
            {
                var workflowError = string.Empty;
                if (ddActionWorkflowDict.TryGetValue(ddAct.ActId, out var error))
                    workflowError = error;

                ddAct.WorkflowStatus = string.IsNullOrEmpty(workflowError) ? 1 : 2;
                ddAct.WorkflowError = workflowError;
                ddAct.UpdatedBy = userName;
                ddAct.LastUpdate = runDate;
            }
            await _epoDDActLogService.Update(ddActions);
        }

        private async Task<string> GetMyEPOAccessToken()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("grant_type", _epoMailboxSettings.GrantType ?? ""),
                        new KeyValuePair<string, string>("client_id", _epoMailboxSettings.ClientId ?? ""),
                        new KeyValuePair<string, string>("client_secret", _epoMailboxSettings.ClientSecret ?? ""),
                        new KeyValuePair<string, string>("scope", _epoMailboxSettings.Scope ?? "")
                    });

                HttpResponseMessage response = await httpClient.PostAsync(_epoMailboxSettings.TokenUrl, content);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseBody);
                var token = json["access_token"]?.ToString();
                return token ?? string.Empty;
            }
        }
        #endregion

        #region OPS
        public bool IsOPSAPIOn()
        {
            return _epoOPSSettings.IsAPIOn;
        }

        public async Task<int> GetFirstDrawings(int logId, List<int>? appIds = null)
        {
            int recordFound = 0;
            var epoOPSLogs = new List<PatOPSLog>();
            var userName = _user.GetUserName();

            //Get AppId and Number to search on EPO OPS;
            var appSearchInputList = await GetEPOOPSFirstImageSearchInputs(appIds);

            if (appSearchInputList == null || appSearchInputList.Count <= 0) return recordFound;

            //Get access_token
            var access_token = await GetOPSAccessToken();

            if (string.IsNullOrEmpty(access_token)) throw new Exception("Missing access_token.");

            var cpiOPSAPI_accessToken = await GetCPIOPSAPIAccessToken();

            if (string.IsNullOrEmpty(cpiOPSAPI_accessToken)) throw new Exception("Missing cpi ops api access_token");

            var settings = await _patSettings.GetSetting();
            string serviceUrl = settings.CPiOPSUrl ?? "";

            if (string.IsNullOrEmpty(serviceUrl)) throw new Exception("Missing cpi api url.");

            string searchUrl = $"{serviceUrl}/OPS/GetFirstDrawings";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cpiOPSAPI_accessToken);
                client.Timeout = Timeout.InfiniteTimeSpan;
                //client.Timeout = TimeSpan.FromMinutes(patIDSSearchApi.TimeOut);

                using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(searchUrl)))
                {
                    //Prepare request body
                    var jsonData = JsonConvert.SerializeObject(new EPOOPSFirstImageInput() { access_token = access_token, SearchInputs = appSearchInputList });
                    request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    //Send request
                    HttpResponseMessage response = await client.SendAsync(request);

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        var responseMsg = await response.Content.ReadAsStringAsync();
                        throw new Exception("Error within API: " + response.ToString() + "-------" + responseMsg);
                    }
                    ;
                    var stringResponse = await response.Content.ReadAsStringAsync();                    

                    var result = JsonConvert.DeserializeObject<List<EPOOPSFirstImageOutput>>(stringResponse);

                    //Prepare logs
                    var newLogs = appSearchInputList.Select(d => new PatOPSLog
                    {
                        LogId = logId,
                        AppId = d.AppId,
                        SearchStr = d.Number,
                        Attempts = 1,
                        FileId = null,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now
                    }).ToList();
                    var existingLogs = _epoOPSLogService.QueryableList.AsNoTracking()
                        .Select(d => new { d.AppId, d.SearchStr, d.Attempts })
                        .AsEnumerable()
                        .Where(d => newLogs.Any(a => a.AppId == d.AppId && a.SearchStr == d.SearchStr))
                        .GroupBy(grp => new { grp.AppId, grp.SearchStr })
                        .Select(d => new
                        {
                            AppId = d.Key.AppId,
                            SearchStr = d.Key.SearchStr,
                            Attempts = d.Select(s => s.Attempts).Max()
                        })
                        .ToList();
                    foreach (var existingLog in existingLogs)
                    {
                        var newLog = newLogs.Where(d => d.AppId == existingLog.AppId && d.SearchStr == existingLog.SearchStr).FirstOrDefault();
                        if (newLog != null)
                            newLog.Attempts = existingLog.Attempts + 1;
                    }

                    //Process downloaded images
                    if (result != null)
                    {
                        //Save images and logs
                        foreach (var epoItem in result)
                        {
                            if (epoItem.AppId <= 0 || string.IsNullOrEmpty(epoItem.Number) || epoItem.Data == null || epoItem.Data.Length <= 0 || string.IsNullOrEmpty(epoItem.FileName))
                                continue;

                            var hasDefaultImage = await _docService.DocDocuments.AsNoTracking().AnyAsync(d => d.IsDefault && d.DocFolder != null && d.DocFolder.SystemType == SystemTypeCode.Patent && d.DocFolder.ScreenCode == ScreenCode.Application && d.DocFolder.DataKey == "AppId" && d.DocFolder.DataKeyValue == epoItem.AppId);

                            var ms = new MemoryStream(epoItem.Data);
                            if (settings.DocumentStorage == Core.Entities.Shared.DocumentStorageOptions.SharePoint)
                                await SaveToSharePoint(ms, epoItem.FileName, epoItem.AppId, userName, false, DocumentSourceType.EPOOPS, !hasDefaultImage);
                            else if (settings.DocumentStorage == Core.Entities.Shared.DocumentStorageOptions.iManage)
                                await SaveToIManage(ms, epoItem.FileName, epoItem.AppId, userName, false, DocumentSourceType.EPOOPS, !hasDefaultImage);
                            else if (settings.DocumentStorage == Core.Entities.Shared.DocumentStorageOptions.NetDocuments)
                                await _netDocsViewModelService.SaveEPODocument(new FormFile(ms, 0, ms.Length, epoItem.FileName, epoItem.FileName), epoItem.AppId, false, DocumentSourceType.EPOOPS, !hasDefaultImage);
                            else
                            {
                                //await SaveToStorage(ms, epoItem.FileName, epoItem.AppId, userName, false, DocumentSourceType.EPOOPS, !hasDefaultImage);

                                var formFile = new FormFile(ms, 0, ms.Length, epoItem.FileName, epoItem.FileName)
                                {
                                    Headers = new HeaderDictionary(),
                                    ContentType = "image/png",
                                    ContentDisposition = $"form-data; name=\"{epoItem.FileName}\"; filename=\"{epoItem.FileName}\""
                                };
                                var docFolder = await _docViewModelService.GetOrAddDefaultFolder($"{SystemTypeCode.Patent}|{ScreenCode.Application}|AppId|{epoItem.AppId.ToString()}");
                                var docDocumentVMs = new List<DocDocumentViewModel>()
                                {
                                    new DocDocumentViewModel()
                                    {
                                        ParentId = docFolder.DataKeyValue,
                                        UploadedFile = formFile,
                                        Author = _user.GetEmail(),
                                        CreatedBy = userName,
                                        UpdatedBy = userName,
                                        DateCreated = DateTime.Now,
                                        LastUpdate = DateTime.Now,
                                        UserFileName = epoItem.FileName,
                                        FolderId = docFolder.FolderId,
                                        IsDefault = !hasDefaultImage,
                                        Source = DocumentSourceType.EPOOPS,
                                        DocFolder = docFolder
                                    }
                                };
                                await _docViewModelService.SaveUploadedDocuments(docDocumentVMs);
                            }
                            ms.Dispose();

                            var newFileId = await _docService.DocFiles.AsNoTracking().Where(d => d.UserFileName == epoItem.FileName && d.DocDocument != null && d.DocDocument.DocFolder != null && d.DocDocument.DocFolder.SystemType == SystemTypeCode.Patent && d.DocDocument.DocFolder.ScreenCode == ScreenCode.Application && d.DocDocument.DocFolder.DataKey == "AppId" && d.DocDocument.DocFolder.DataKeyValue == epoItem.AppId).OrderByDescending(o => o.DateCreated).Select(d => d.FileId).FirstOrDefaultAsync();

                            if (newFileId > 0)
                            {
                                var newLog = newLogs.Where(d => d.AppId == epoItem.AppId && d.SearchStr == epoItem.Number).FirstOrDefault();
                                if (newLog != null)
                                    newLog.FileId = newFileId;
                            }

                            recordFound++;
                        }
                    }

                    //Save logs
                    if (newLogs != null && newLogs.Count > 0)
                        await _epoOPSLogService.Add(newLogs);
                }
            }
            return recordFound;
        }

        private async Task<List<EPOOPSFirstImageSearchInput>> GetEPOOPSFirstImageSearchInputs(List<int>? appIds = null)
        {
            var searchInputs = new List<EPOOPSFirstImageSearchInput>();
            var batchSize = 100;

            //Only get applications that haven't reached the max attempts yet or don't have FileId (image downloaded)            

            var excludeAppIds = _epoOPSLogService.QueryableList.AsNoTracking()
                .Where(d => d.Attempts >= _epoOPSSettings.MaxAttempts || d.FileId > 0)
                .Select(d => d.AppId).Distinct().ToHashSet();

            var processedAppIds = _epoOPSLogService.QueryableList.AsNoTracking()
                .Select(d => d.AppId).Distinct().ToHashSet();

            //Prioritize records haven't been processed yet
            var applicationList = await _applicationService.CountryApplications.AsNoTracking()
                .Where(d => (excludeAppIds == null || !excludeAppIds.Any() || !excludeAppIds.Contains(d.AppId))
                    && (processedAppIds == null || !processedAppIds.Any() || !processedAppIds.Contains(d.AppId))
                    && d.PatApplicationStatus != null && d.PatApplicationStatus.ActiveSwitch
                    && (!string.IsNullOrEmpty(d.PatNumber) || !string.IsNullOrEmpty(d.PubNumber))
                    && (appIds == null || !appIds.Any() || appIds.Contains(d.AppId))
                )
                .Select(app => new CountryApplication()
                {
                    AppId = app.AppId,
                    CaseNumber = app.CaseNumber,
                    Country = app.Country,
                    SubCase = app.SubCase,
                    PatNumber = app.PatNumber,
                    CaseType = app.CaseType,
                    AppNumber = app.AppNumber,
                    PubNumber = app.PubNumber,
                    FilDate = app.FilDate,
                    PubDate = app.PubDate,
                    IssDate = app.IssDate
                }).Distinct().OrderBy(o => o.AppId).Take(batchSize).ToListAsync();

            int numToFill = batchSize - applicationList.Count;
            if (numToFill > 0)
            {
                //Include records that have been proccessed but haven't reached the max attempt yet
                applicationList.AddRange(await _applicationService.CountryApplications.AsNoTracking()
                .Where(d => !excludeAppIds.Contains(d.AppId) && processedAppIds.Contains(d.AppId)
                    && d.PatApplicationStatus != null && d.PatApplicationStatus.ActiveSwitch
                    && (!string.IsNullOrEmpty(d.PatNumber) || !string.IsNullOrEmpty(d.PubNumber))
                    && (appIds == null || !appIds.Any() || appIds.Contains(d.AppId))
                )
                .Select(app => new CountryApplication()
                {
                    AppId = app.AppId,
                    CaseNumber = app.CaseNumber,
                    Country = app.Country,
                    SubCase = app.SubCase,
                    PatNumber = app.PatNumber,
                    CaseType = app.CaseType,
                    AppNumber = app.AppNumber,
                    PubNumber = app.PubNumber,
                    FilDate = app.FilDate,
                    PubDate = app.PubDate,
                    IssDate = app.IssDate
                }).Distinct().OrderBy(o => o.AppId).Take(numToFill).ToListAsync());
            }

            if (applicationList.Count > 0)
            {
                foreach (var app in applicationList)
                {
                    //Standardize publication and patent number since we don't know which one will be found on EPO OPS
                    //Publication number
                    if (!string.IsNullOrEmpty(app.PubNumber))
                    {
                        WebLinksNumberInfoDTO tempData = new WebLinksNumberInfoDTO();
                        tempData.SystemType = WebLinksSystemType.Patent;
                        tempData.Country = app.Country;
                        tempData.CaseType = app.CaseType;
                        tempData.AppNumber = app.AppNumber;
                        tempData.PubNumber = app.PubNumber;
                        tempData.PatRegNumber = app.PatNumber;
                        tempData.FilDate = app.FilDate;
                        tempData.PubDate = app.PubDate;
                        tempData.IssRegDate = app.IssDate;
                        tempData.Number = _numberFormatService.CleanUpNumber(app.PubNumber ?? "");
                        tempData.NumberDate = app.PubDate;
                        tempData.NumberType = WebLinksNumberType.PubNo;

                        try
                        {
                            var parsedData = await _numberFormatService.FormatNumber(tempData, WebLinksTemplateType.Web);
                            if (!string.IsNullOrEmpty(parsedData))
                                searchInputs.Add(new EPOOPSFirstImageSearchInput() { AppId = app.AppId, Number = parsedData });

                        }
                        catch (Exception ex) { var error = ex.Message; continue; }
                    }

                    //Patent number
                    if (!string.IsNullOrEmpty(app.PatNumber))
                    {
                        WebLinksNumberInfoDTO tempData = new WebLinksNumberInfoDTO();
                        tempData.SystemType = WebLinksSystemType.Patent;
                        tempData.Country = app.Country;
                        tempData.CaseType = app.CaseType;
                        tempData.AppNumber = app.AppNumber;
                        tempData.PubNumber = app.PubNumber;
                        tempData.PatRegNumber = app.PatNumber;
                        tempData.FilDate = app.FilDate;
                        tempData.PubDate = app.PubDate;
                        tempData.IssRegDate = app.IssDate;
                        tempData.Number = _numberFormatService.CleanUpNumber(app.PatNumber ?? "");
                        tempData.NumberDate = app.IssDate;
                        tempData.NumberType = WebLinksNumberType.PatRegNo;

                        try
                        {
                            var parsedData = await _numberFormatService.FormatNumber(tempData, WebLinksTemplateType.Web);
                            if (!string.IsNullOrEmpty(parsedData))
                                searchInputs.Add(new EPOOPSFirstImageSearchInput() { AppId = app.AppId, Number = parsedData });

                        }
                        catch (Exception ex) { var error = ex.Message; continue; }
                    }
                }
            }

            return searchInputs.DistinctBy(d => new { d.AppId, d.Number }).ToList();
        }

        private async Task<string> GetOPSAccessToken()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("grant_type", _epoOPSSettings.GrantType ?? ""),
                        new KeyValuePair<string, string>("client_id", _epoOPSSettings.ClientId ?? ""),
                        new KeyValuePair<string, string>("client_secret", _epoOPSSettings.ClientSecret ?? ""),
                        new KeyValuePair<string, string>("scope", _epoOPSSettings.Scope ?? "")
                    });

                HttpResponseMessage response = await httpClient.PostAsync(_epoOPSSettings.TokenUrl, content);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseBody);
                var token = json["access_token"]?.ToString();
                return token ?? string.Empty;
            }
        }

        private async Task<string> GetCPIOPSAPIAccessToken()
        {
            var settings = await _patSettings.GetSetting();
            string tokenUrl = settings.CPiOPSUrl ?? "";
            tokenUrl = tokenUrl.Replace("/api", "/connect/token");

            using (HttpClient httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("client_id", _epoOPSSettings.CPIClientId ?? ""),
                        new KeyValuePair<string, string>("client_secret", _epoOPSSettings.CPIClientSecret ?? ""),
                    });

                HttpResponseMessage response = await httpClient.PostAsync(tokenUrl, content);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseBody);
                var token = json["access_token"]?.ToString();
                return token ?? string.Empty;
            }
        }

        #endregion

        #region Helpers
        private void AddOrUpdateDocumentMap(EPODocumentCode remoteCode, string language, string description, Dictionary<(string, string), PatEPODocumentMap> existingLookup, List<PatEPODocumentMap> newMaps, List<PatEPODocumentMap> updateMaps, string userName)
        {
            if (string.IsNullOrEmpty(remoteCode.DocCode)) return;

            var key = (remoteCode.DocCode.ToLower(), language.ToLower());
            var existingMap = existingLookup.GetValueOrDefault(key);

            if (existingMap == null)
            {
                newMaps.Add(new PatEPODocumentMap
                {
                    DocumentCode = remoteCode.DocCode,
                    DocumentName = description,
                    Enabled = true,
                    Language = language,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    DateCreated = DateTime.Now,
                    LastUpdate = DateTime.Now
                });
            }
            else if (string.IsNullOrEmpty(existingMap.DocumentName) || existingMap.DocumentName.ToLower() != description.ToLower())
            {

                existingMap.DocumentName = description;
                existingMap.UpdatedBy = userName;
                existingMap.LastUpdate = DateTime.Now;
                updateMaps.Add(existingMap);
            }
        }

        private async Task SaveToStorage(MemoryStream stream, string fileName, int appId, string userName, bool isDocVerificationOn, string docSource, bool isDefault = false, bool isActRequired = false, bool checkAct = false, bool sendToClient = false)
        {
            if (appId > 0 || (appId <= 0 && isDocVerificationOn))
            {
                var viewModel = new DocDocumentViewModel
                {
                    DocFileName = fileName,
                    Author = userName,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    DateCreated = DateTime.Now,
                    LastUpdate = DateTime.Now,
                    SystemType = appId > 0 ? SystemTypeCode.Patent : "",
                    ScreenCode = appId > 0 ? ScreenCode.Application : "",
                    ParentId = appId > 0 ? appId : 0,
                    DataKey = appId > 0 ? "AppId" : "",
                    Source = docSource,
                    IsDefault = isDefault,
                    IsActRequired = isActRequired,
                    CheckAct = checkAct,
                    SendToClient = sendToClient
                };

                await _docViewModelService.SaveDocumentFromStream(viewModel, stream, false);
            }
        }

        private async Task SaveToSharePoint(MemoryStream stream, string fileName, int appId, string userName, bool isDocVerificationOn, string docSource, bool isDefault = false, List<string>? tags = null, bool isActRequired = false, bool checkAct = false, bool sendToClient = false)
        {
            var settings = await _patSettings.GetSetting();

            if (appId > 0 || (appId <= 0 && isDocVerificationOn))
            {
                var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                var docLibrary = SharePointDocLibrary.Orphanage;
                var folders = new List<string>();
                var recKey = string.Empty;

                if (appId > 0)
                {
                    var ctryApp = await _applicationService.CountryApplications.AsNoTracking().Where(d => d.AppId == appId).Select(d => new { d.CaseNumber, d.Country, d.SubCase }).FirstOrDefaultAsync();
                    if (ctryApp != null)
                    {
                        recKey = SharePointViewModelService.BuildRecKey(ctryApp.CaseNumber, ctryApp.Country, ctryApp.SubCase);
                        folders = SharePointViewModelService.GetDocumentFolders(SharePointDocLibraryFolder.Application, recKey);
                        docLibrary = SharePointDocLibrary.Patent;
                    }
                }

                if (_graphSettings.Site != null)
                {
                    var result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders, stream, fileName);
                    if (result != null)
                    {
                        var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;
                        var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();
                        var driveItem = await graphClient.Drives[result.DriveId].Items[result.DriveItemId].Request().Expand("listItem").GetAsync();
                        if (list != null && driveItem != null)
                        {
                            if (isDefault)
                            {
                                if (!settings.IsSharePointIntegrationKeyFieldsOnly)
                                {
                                    if (settings.IsSharePointIntegrationByMetadataOn)
                                    {
                                        await graphClient.UnmarkDefaultImageByMetadata(site.Id, list.Id, SharePointDocLibraryFolder.Application, recKey);
                                    }
                                    else
                                    {
                                        await graphClient.UnmarkDefaultImage(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Patent, folders, site.Id, list.Id);
                                    }
                                }
                            }

                            var sync = new SharePointSyncToDocViewModel
                            {
                                DocLibrary = docLibrary,
                                DocLibraryFolder = appId > 0 ? SharePointDocLibraryFolder.Application : null,
                                DriveItemId = driveItem.Id,
                                ParentId = appId > 0 ? appId : 0,
                                FileName = fileName,
                                CreatedBy = userName,
                                Remarks = "",
                                Tags = "",
                                IsImage = driveItem.Image != null,
                                IsPrivate = false,
                                IsDefault = isDefault,
                                IsPrintOnReport = false,
                                IsVerified = false,
                                IncludeInWorkflow = false,
                                IsActRequired = isActRequired,
                                CheckAct = checkAct,
                                SendToClient = sendToClient,
                                Source = docSource,
                                Author = userName
                            };
                            await _sharePointViewModelService.SyncToDocumentTables(sync);

                            var requestBody = new FieldValueSet
                            {
                                AdditionalData = new Dictionary<string, object>()
                            };

                            if (SharePointViewModelService.IsSharePointIntegrationHasSyncField)
                            {
                                requestBody.AdditionalData.Add("CPISyncCompleted", true);
                            }

                            if (!settings.IsSharePointIntegrationKeyFieldsOnly)
                            {
                                requestBody.AdditionalData.Add("IsDefault", isDefault);
                                requestBody.AdditionalData.Add("Source", docSource);
                                requestBody.AdditionalData.Add("CPiTags", tags != null && tags.Count > 0 ? string.Join(";", tags.Distinct().ToList()) ?? "" : "");
                                requestBody.AdditionalData.Add("IsActRequired", isActRequired);
                                requestBody.AdditionalData.Add("CheckAct", checkAct);
                                requestBody.AdditionalData.Add("SendToClient", sendToClient);
                            }

                            if (requestBody.AdditionalData.Count > 0)
                                await graphClient.Sites[site.Id].Lists[list.Id].Items[driveItem.ListItem.Id].Fields.Request().UpdateAsync(requestBody);
                        }
                    }
                }
            }
        }

        private async Task SaveToIManage(MemoryStream stream, string fileName, int appId, string userName, bool isDocVerificationOn, string docSource, bool isDefault = false, bool isActRequired = false, bool checkAct = false, bool sendToClient = false)
        {
            if (appId > 0 || (appId <= 0 && isDocVerificationOn))
            {
                var client = await _iManageClientFactory.GetClient();

                var docFolder = new DocFolder();
                var documentLink = string.Empty;
                var iManageFolderId = string.Empty;

                if (appId > 0)
                {
                    documentLink = $"P|CA|AppId|{appId.ToString()}";
                    docFolder = await _docService.DocFolders.Where(d => d.SystemType == SystemTypeCode.Patent && d.ScreenCode == ScreenCode.Application && d.DataKey == "AppId" && d.DataKeyValue == appId).FirstOrDefaultAsync();
                }
                else if (appId <= 0 && isDocVerificationOn)
                {
                    documentLink = "|||0";
                    docFolder = await _docService.DocFolders.Where(d => string.IsNullOrEmpty(d.SystemType) && string.IsNullOrEmpty(d.ScreenCode) && string.IsNullOrEmpty(d.DataKey) && d.DataKeyValue == 0).FirstOrDefaultAsync();
                }

                if (docFolder == null)
                    docFolder = await _iManageViewModelService.GetOrAddDefaultFolderByDocumentLink(documentLink);

                iManageFolderId = _iManageViewModelService.GetDefaultDocumentFolder(docFolder);
                var parentId = int.Parse(documentLink.Split("|")[3] ?? "0");

                if (string.IsNullOrEmpty(iManageFolderId) && _iManageSettings.WorkspaceCreation == Web.Services.iManage.WorkspaceCreation.Auto)
                {
                    var rootDocumentLink = await _docViewModelService.GetRootDocumentLink(documentLink);
                    var name = await _docViewModelService.GenerateFolderName(rootDocumentLink);

                    docFolder = await _iManageViewModelService.CreateImanageWorkspace(client, rootDocumentLink, name, _iManageSettings.WorkspaceTemplateId, _iManageSettings.DefaultFolderName);
                    iManageFolderId = _iManageViewModelService.GetDefaultDocumentFolder(docFolder);
                }

                var viewModel = new DocDocumentViewModel
                {
                    DocFileName = fileName,
                    Author = userName,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    DateCreated = DateTime.Now,
                    LastUpdate = DateTime.Now,
                    SystemType = appId > 0 ? SystemTypeCode.Patent : "",
                    ScreenCode = appId > 0 ? ScreenCode.Application : "",
                    ParentId = parentId,
                    DataKey = parentId > 0 ? "AppId" : "",
                    Source = docSource,
                    IsDefault = isDefault,
                    IsActRequired = isActRequired,
                    CheckAct = checkAct,
                    SendToClient = sendToClient
                };

                var document = await client.UploadDocument(iManageFolderId, new ByteArrayContent(stream.ToArray()), fileName);

                if (document != null)
                {
                    //docDocument
                    viewModel.DocName = document.Name;
                    viewModel.FolderId = docFolder.FolderId;

                    //docFile
                    viewModel.UserFileName = fileName;
                    viewModel.FileSize = document.Size;
                    viewModel.IsImage = document.IsImage();
                    viewModel.DriveItemId = document.Id;

                    //create docFile and update docViewModel.FileId
                    var docFile = await _docViewModelService.AddDocFile(viewModel, viewModel.UserFileName ?? "", viewModel.FileSize ?? 0, viewModel.IsImage ?? false);

                    viewModel.DocTypeId = await _docViewModelService.GetDocTypeIdFromFileName(fileName);
                }

                //save docDocument
                var docDocument = await _docViewModelService.SaveDocument(viewModel);
            }
        }

        private async Task DeleteDocuments(string userName, List<DocumentViewModel> deleteDocs)
        {
            var settings = await _patSettings.GetSetting();

            if (settings.IsSharePointIntegrationOn)
            {
                var graphClient = _sharePointService.GetGraphClientByClientCredentials();

                var driveItemIds = deleteDocs.Where(d => !string.IsNullOrEmpty(d.DriveItemId)).Select(d => d.DriveItemId).Distinct().ToList();

                _applicationService.DetachAllEntities();

                foreach (var driveItemId in driveItemIds)
                {
                    if (!string.IsNullOrEmpty(driveItemId))
                    {
                        await graphClient.DeleteSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Patent, driveItemId);

                        var deleteDoc = await _docService.DocDocuments.Where(d => d.DocFile != null && d.DocFile.DriveItemId == driveItemId).FirstOrDefaultAsync();
                        var deleteFile = await _docService.DocFiles.Where(d => d.DriveItemId == driveItemId).FirstOrDefaultAsync();
                        if (deleteFile != null && deleteDoc != null)
                            await _docService.DeleteDoc(deleteDoc, deleteFile);
                    }
                }
            }
            else
            {
                var docIds = deleteDocs.Select(d => d.DocId).Distinct().ToList();

                var deleteList = await _docService.DocDocuments.AsNoTracking().Where(d => docIds.Contains(d.DocId)).ToListAsync();

                if (deleteList.Any())
                {
                    foreach (var item in deleteList)
                    {
                        var docFile = await _docService.DocFiles.AsNoTracking().Where(d => d.FileId == item.FileId).Select(d => new { d.DocFileName, d.ThumbFileName }).FirstOrDefaultAsync();
                        if (docFile != null && !string.IsNullOrEmpty(docFile.DocFileName))
                        {
                            if (await _docService.GetFileOtherRefCount(item.DocId, item.FileId ?? 0) == 0)
                            {
                                _documentHelper.DeleteDocumentFile((docFile.DocFileName ?? ""), (docFile.ThumbFileName ?? ""), ImageHelper.IsImageFile(docFile.DocFileName ?? ""));
                            }
                        }
                    }

                    _applicationService.DetachAllEntities();
                    await _docService.UpdateDocuments(userName, new List<DocDocument>(), new List<DocDocument>(), deleteList);
                }
            }
        }

        private byte[] CombineByteData(List<byte[]> documentBytes)
        {
            var combinedData = new byte[] { };
            // Create a MemoryStream to hold new merged PDF
            using (MemoryStream ms = new MemoryStream())
            {
                // Initialize PDF writer
                PdfWriter writer = new PdfWriter(ms);
                // Initialize PDF document
                PdfDocument pdf = new PdfDocument(writer);

                foreach (var pdfBytes in documentBytes)
                {
                    if (pdfBytes == null) continue;
                    // Create a PdfReader
                    PdfReader reader = new PdfReader(new MemoryStream(pdfBytes));
                    // Initialize source PDF document
                    PdfDocument sourcePdf = new PdfDocument(reader);
                    // Copy pages from source PDF to the destination PDF
                    sourcePdf.CopyPagesTo(1, sourcePdf.GetNumberOfPages(), pdf);
                    // Close the source PDF
                    sourcePdf.Close();
                }
                // Close the destination PDF
                pdf.Close();
                combinedData = ms.ToArray();
            }

            return combinedData;
        }

        private async Task<byte[]> GetFileByteData(string? systemType = null, string? driveItemId = null, string? docFileName = null)
        {
            var settings = await _patSettings.GetSetting();
            Stream? sourceStream = null;

            switch (settings.DocumentStorage)
            {
                case Core.Entities.Shared.DocumentStorageOptions.SharePoint:
                    if (string.IsNullOrEmpty(driveItemId))
                    {
                        throw new InvalidOperationException("driveItemId is required for SharePoint storage.");
                    }
                    var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                    var docLibrary = systemType == SystemTypeCode.Patent ? SharePointDocLibrary.Patent : SharePointDocLibrary.Orphanage;

                    sourceStream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId);
                    break;

                case Core.Entities.Shared.DocumentStorageOptions.iManage:
                    if (string.IsNullOrEmpty(driveItemId))
                    {
                        throw new InvalidOperationException("driveItemId is required for iManage storage.");
                    }
                    var iManageClient = await _iManageClientFactory.GetClient(true);

                    if (iManageClient != null)
                    {
                        sourceStream = await iManageClient.GetDocumentAsStream(driveItemId);
                    }
                    break;

                case Core.Entities.Shared.DocumentStorageOptions.NetDocuments:
                    if (string.IsNullOrEmpty(driveItemId))
                    {
                        throw new InvalidOperationException("driveItemId is required for NetDocuments storage.");
                    }
                    sourceStream = await  _netDocsViewModelService.GetDocumentAsStream(driveItemId);
                    break;

                case Core.Entities.Shared.DocumentStorageOptions.BlobOrFileSystem:
                    if (string.IsNullOrEmpty(docFileName))
                    {
                        throw new InvalidOperationException("docFileName is required for Blob or FileSystem storage.");
                    }
                    var docFile = await _documentStorage.GetFileStream("", docFileName, ImageHelper.CPiSavedFileType.DocMgt);

                    sourceStream = docFile?.Stream;
                    break;

                default:
                    break;
            }

            if (sourceStream == null)
            {
                return Array.Empty<byte>();
            }

            using (sourceStream)
            {
                if (sourceStream is MemoryStream memoryStream)
                {
                    return memoryStream.ToArray();
                }
                else
                {
                    await using var ms = new MemoryStream();
                    await sourceStream.CopyToAsync(ms);
                    return ms.ToArray();
                }
            }
        }

        private async Task<List<string>> GetNaturalSorted(List<string> stringList)
        {
            var naturalSorted = new List<string>();

            var settings = await _patSettings.GetSetting();
            string serviceUrl = settings.MyEPOURL ?? "";

            if (string.IsNullOrEmpty(serviceUrl)) return naturalSorted;

            if (stringList.Count <= 1)
                return stringList;

            string url = $"{serviceUrl}/Mailbox/SortWithNaturalSort";
            using (var client = new HttpClient())
            {
                client.Timeout = Timeout.InfiniteTimeSpan;
                using (var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url)))
                {
                    //Prepare request body
                    var jsonData = JsonConvert.SerializeObject(stringList);
                    request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    //Send request
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        var responseMsg = await response.Content.ReadAsStringAsync();
                        throw new Exception("Error within API: " + responseMsg);
                    }
                    ;
                    var stringResponse = await response.Content.ReadAsStringAsync();

                    naturalSorted = JsonConvert.DeserializeObject<List<string>>(stringResponse);
                }
            }

            return naturalSorted ?? stringList;
        }
        #endregion
    }
}
