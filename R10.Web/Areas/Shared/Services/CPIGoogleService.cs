using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Services;
using R10.Web.Services.DocumentStorage;
using R10.Web.Services.iManage;
using R10.Web.Services.NetDocuments;
using R10.Web.Services.SharePoint;
using System.Security.Claims;
using System.Text;

namespace R10.Web.Areas.Shared.Services
{
    public class CPIGoogleService : ICPIGoogleService
    {
        private readonly ClaimsPrincipal _user;

        private readonly IEntityService<CurrencyType> _currencyTypeService;

        private readonly ICountryApplicationService _applicationService;
        private readonly ICountryApplicationViewModelService _applicationViewModelService;
        private readonly IPatIDSService _idsService;
        private readonly GoogleIDSSettings _googleIDSSettings;

        private readonly ISystemSettings<DefaultSetting> _defaultSettings;

        private readonly IDocumentService _docService;
        private readonly IDocumentStorage _documentStorage;
        private readonly GraphSettings _graphSettings;
        private readonly ISharePointService _sharePointService;
        private readonly IiManageViewModelService _iManageViewModelService;
        private readonly INetDocumentsViewModelService _netDocsViewModelService;

        private readonly IEntityService<PatIDSDownloadWebSvc> _idsLogService;
        private readonly ServiceAccount _serviceAccount;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger _logger;

        public CPIGoogleService(
            ClaimsPrincipal user,
            IEntityService<CurrencyType> currencyTypeService,
            ICountryApplicationService applicationService,
            ICountryApplicationViewModelService applicationViewModelService,
            IPatIDSService idsService,
            IOptions<GoogleIDSSettings> googleIDSSettings,
            ISystemSettings<DefaultSetting> defaultSetting,
            IDocumentService docService,
            IDocumentStorage documentStorage,
            IOptions<GraphSettings> graphSettings,
            ISharePointService sharePointService,           
            IiManageViewModelService iManageViewModelService,
            IEntityService<PatIDSDownloadWebSvc> idsLogService,            
            IOptions<ServiceAccount> serviceAccount,
            IHttpClientFactory httpClientFactory,
            ILogger<CPIGoogleService> logger,
            INetDocumentsViewModelService netDocsViewModelService
            )
        {
            _user = user;

            _currencyTypeService = currencyTypeService;

            _applicationService = applicationService;
            _applicationViewModelService = applicationViewModelService;
            _idsService = idsService;
            _googleIDSSettings = googleIDSSettings.Value;

            _defaultSettings = defaultSetting;

            _docService = docService;
            _documentStorage = documentStorage;
            _sharePointService = sharePointService;
            _graphSettings = graphSettings.Value;
            _iManageViewModelService = iManageViewModelService;
            _netDocsViewModelService = netDocsViewModelService;

            _idsLogService = idsLogService;
            _serviceAccount = serviceAccount.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        #region Helpers
        private async Task<string> GetGoogleAPIAccessToken(string baseUrl)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("client_id", _googleIDSSettings.ClientId ?? ""),
                        new KeyValuePair<string, string>("client_secret", _googleIDSSettings.ClientSecret ?? "")
                    });

                var tokenUrl = new Uri(new Uri(baseUrl), "connect/token").ToString();

                HttpResponseMessage response = await httpClient.PostAsync(tokenUrl, content);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseBody);
                var token = json["access_token"]?.ToString();
                return token ?? string.Empty;
            }
        }
        #endregion

        #region Patent IDS Documents
        public async Task<int> GetIDSDocuments(int logId, PatIDSSearchApi? patIDSSearchApi = null)
        {
            var userName = _user.GetUserName();
            var searchInput = new List<PatIDSSearchInputDTO>();

            if (patIDSSearchApi == null) patIDSSearchApi = new PatIDSSearchApi();

            var appIds = string.Empty; 
            if (patIDSSearchApi != null && patIDSSearchApi.AppIds != null &&patIDSSearchApi.AppIds.Count > 0)
                appIds = string.Join(",", patIDSSearchApi.AppIds);

            if (patIDSSearchApi != null)
                searchInput = await _idsService.GetIDSDownloadList(patIDSSearchApi.MaxAttempts, appIds);

            if (searchInput == null || searchInput.Count <= 0) return 0;            

            searchInput = searchInput.Take(patIDSSearchApi!.MaxToSearch > 25 ? 25 : patIDSSearchApi.MaxToSearch).ToList();

            var settings = await _defaultSettings.GetSetting();
            string serviceUrl = settings.GoogleApiURL ?? ""; 

            var accessToken = await GetGoogleAPIAccessToken(serviceUrl);
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("Access token is empty");
            }

            //Passing data to api for getting pdf files in byte array
            var searchHttpClient = _httpClientFactory.CreateClient();
                    
            if (patIDSSearchApi != null && patIDSSearchApi.TimeOut == 0)
                searchHttpClient.Timeout = Timeout.InfiniteTimeSpan;
            else
                searchHttpClient.Timeout = TimeSpan.FromMinutes(patIDSSearchApi != null ? patIDSSearchApi.TimeOut : 5);

            var searchParam = searchInput.DistinctBy(d => new { d.SearchStr, d.KindCode }).ToList();                                       

            var jsonData = JsonConvert.SerializeObject(new PatIDSSearchParam
            {
                Criteria = searchParam,
                DiffTolerance = patIDSSearchApi != null ? patIDSSearchApi.DiffTolerance : 1
            });

            using (var request = new HttpRequestMessage())
            {
                //Prepare and send request for processing - START
                var searchUrl =  serviceUrl.TrimEnd('/') + "/SearchDocuments";
                request.RequestUri = new Uri(searchUrl);
                request.Method = HttpMethod.Post;
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken); 

                var postData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                request.Content = postData;

                HttpResponseMessage response = await searchHttpClient.SendAsync(request);
                var stringResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error calling SearchDocuments. Status: {response.StatusCode}. Content: {stringResponse}");
                }                                
                            
                var result = JsonConvert.DeserializeObject<List<PatIDSSearchOutDTO>>(stringResponse);
                //Prepare and send request for processing - END
                //Got processed data                                                        
                                                        
                //Keep log and download attempt
                var newLogs = await InitializeAndMergeLogsAsync(searchInput, logId);
                var updatedCount = 0;

                //Save files if found
                if (result != null && result.Count() > 0)
                {
                    var downloadLog = await ProcessAndSaveDocumentsAsync(result, searchInput, newLogs, settings, userName);

                    if (downloadLog.updatedCases.Any()) 
                        await _idsService.SaveIDSRelatedCaseDocs(downloadLog.updatedCases);

                    updatedCount = downloadLog.updatedCases.Count;
                }

                if (newLogs.Any()) 
                    await _idsLogService.Add(newLogs);
                                
                return updatedCount;
            }
        }

        private async Task<List<PatIDSDownloadWebSvc>> InitializeAndMergeLogsAsync(List<PatIDSSearchInputDTO> searchInput, int logId)
        {
            // 1. Initialize new log entries
            var newLogs = searchInput.Select(d => new PatIDSDownloadWebSvc()
            {
                LogId = logId,
                AppId = d.AppId,
                RelatedCasesId = d.RelatedCasesId,
                SearchStr = d.SearchStr,
                FileId = 0,
                Attempts = 1,
                DocFilePath = null,
                DownloadLink = null
            }).ToList();

            // 2. Fetch and calculate max attempts from existing logs
            var existingLogs = _idsLogService.QueryableList.AsEnumerable()
                .Where(d => searchInput.Any(s => s.AppId == d.AppId && s.RelatedCasesId == d.RelatedCasesId))
                .GroupBy(grp => new { grp.AppId, grp.RelatedCasesId })
                .Select(d => new
                {
                    d.Key.AppId,
                    d.Key.RelatedCasesId,
                    Attempts = d.Select(s => s.Attempts).Max()
                })
                .ToList();

            // 3. Merge max attempts into new logs
            foreach (var existingLog in existingLogs)
            {
                var newLog = newLogs.FirstOrDefault(d => d.AppId == existingLog.AppId && d.RelatedCasesId == existingLog.RelatedCasesId);
                if (newLog != null)
                {
                    newLog.Attempts = existingLog.Attempts + 1;
                }
            }

            return newLogs;
        }

        private async Task<(List<PatIDSRelatedCase> updatedCases, List<PatIDSDownloadWebSvc> newLogs)> ProcessAndSaveDocumentsAsync(
            List<PatIDSSearchOutDTO> finalResultList,
            List<PatIDSSearchInputDTO> searchInput,
            List<PatIDSDownloadWebSvc> newLogs,
            DefaultSetting settings,
            string userName)
        {
            var idsRelatedCases = new List<PatIDSRelatedCase>();

            foreach (var item in finalResultList)
            {
                // 1. Download document
                byte[]? fileData = null;
                try
                {
                    if (!string.IsNullOrEmpty(item.AzureFilePath))
                    {
                        var cpiGoogleDocument = await GetCPIGoogleDocumentFileStream(item.AzureFilePath);
                        if (cpiGoogleDocument != null && cpiGoogleDocument.Stream != null)
                        {
                            await using (cpiGoogleDocument.Stream)
                            {                                
                                await using var ms = new MemoryStream();
                                await cpiGoogleDocument.Stream.CopyToAsync(ms);
                                fileData = ms.ToArray();
                            }
                        }
                    }
                    
                    if ((fileData == null || fileData.Length == 0) && !string.IsNullOrEmpty(item.FileUrl))
                    {
                        var downloadHttpClient = _httpClientFactory.CreateClient();
                        fileData = await downloadHttpClient.GetByteArrayAsync(new Uri(item.FileUrl));                            
                    }
                }
                catch (Exception ex)
                {
                    //Avoid missing files/getting blocked by google
                    var errorMsg = ex.Message;
                    _logger.LogError(ex, $"Error downloading file for SearchStr: {item.SearchStr}, FileUrl: {item.FileUrl}");
                    continue;
                }

                if (fileData == null || fileData.Length == 0)
                {
                    // The file download failed or was empty; log the error (fileException) and continue to the next item.
                    continue;
                }
                
                // 2. File Naming
                var fileName = (item.FileUrl ?? "").Split("/").Last();
                var fileExtension = fileName.Split(".").Last();

                foreach (var foundItem in searchInput.Where(d => d.SearchStr == item.SearchStr))
                {
                    var fileId = 0;
                    var currentFileName = fileName;

                    // Create MemoryStream and IFormFile once per foundItem
                    using var ms = new MemoryStream(fileData);
                    IFormFile formFile = new FormFile(ms, 0, fileData.Length, foundItem.SearchStr ?? "", currentFileName);

                    // 3. Save physical file based on storage option
                    string? saveFileError = null;
                    try
                    {
                        if (settings.DocumentStorage == DocumentStorageOptions.SharePoint)
                        {
                            var existing = await _idsService.IDSRelatedCases
                                .Where(r => r.AppId == Convert.ToInt32(foundItem.AppId) && r.DocFilePath == currentFileName && r.RelatedCasesId != foundItem.RelatedCasesId)
                                .AnyAsync();

                            if (existing) currentFileName = "CPI_" + currentFileName;

                            await SaveToSharePoint(formFile, foundItem.AppId);
                        }
                        else if (settings.DocumentStorage == DocumentStorageOptions.iManage)
                        {
                            var (iManageFileId, iManageOrigFilName) = await _iManageViewModelService.SavePatentIDSDocumentToIManage(formFile, foundItem.AppId, settings, "References");
                            fileId = iManageFileId;
                            currentFileName = iManageOrigFilName;
                        }
                        else if (settings.DocumentStorage == DocumentStorageOptions.NetDocuments)
                        {
                            var (netDocsFileId, netDocsFilName) = await _netDocsViewModelService.SavePatentIDSDocument(formFile, foundItem.AppId, "References");
                            fileId = netDocsFileId;
                            currentFileName = netDocsFilName;
                        }
                        else if (settings.DocumentStorage == DocumentStorageOptions.BlobOrFileSystem)
                        {
                            fileId = await _docService.AddFileToFileHandler(currentFileName, userName);
                            currentFileName = fileId + "." + fileExtension;
                            await SaveToStorage(formFile, currentFileName, foundItem.AppId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error saving file for SearchStr: {item.SearchStr}, FileUrl: {item.FileUrl}");
                        saveFileError = ex.Message + "|StackTrace: " + ex.StackTrace;
                        if (ex.InnerException != null)
                        {
                            saveFileError += "|Inner Exception: " + ex.InnerException.Message != null ? ex.InnerException.Message.ToString() : ex.InnerException.ToString();
                            saveFileError += "|InnerStackTrace: " + ex.InnerException.StackTrace;
                        }

                        // reset tblPatIDSDownloadWebSvc.FileId and .DocFilePath to allow fixing bugs and retry
                        fileId = 0;
                        currentFileName = null;
                    }                    

                    // 4. Update tblPatRelatedCase record
                    var idsRelatedCase = await _idsService.IDSRelatedCases
                        .AsNoTracking()
                        .FirstOrDefaultAsync(r => r.RelatedCasesId == foundItem.RelatedCasesId);

                    if (idsRelatedCase != null)
                    {
                        idsRelatedCase.FileId = fileId;
                        idsRelatedCase.DocFilePath = currentFileName;
                        idsRelatedCase.UpdatedBy = userName;
                        idsRelatedCase.LastUpdate = DateTime.Now;
                        idsRelatedCases.Add(idsRelatedCase);
                    }

                    // 5. Update the in-memory log list (newLogs)
                    var newLog = newLogs.FirstOrDefault(d => d.AppId == foundItem.AppId && d.RelatedCasesId == foundItem.RelatedCasesId);
                    if (newLog != null)
                    {
                        newLog.FileId = fileId;
                        newLog.DocFilePath = currentFileName;
                        newLog.DownloadLink = item.FileUrl;
                        newLog.Remarks = saveFileError;
                    }
                }
            }

            return (idsRelatedCases, newLogs);
        }

        private async Task SaveToStorage(IFormFile formFile, string fileName, int appId)
        {
            var systemType = QuickEmailHelper.GetSystem(SystemTypeCode.Patent);
            var fullPath = _documentStorage.GetFilePath(systemType, fileName, ImageHelper.CPiSavedFileType.IDSReferences);
            await _documentStorage.SaveFile(formFile, fullPath, new DocumentStorageHeader
            {
                SystemType = SystemTypeCode.Patent,
                ScreenCode = ScreenCode.IDS,
                ParentId = appId.ToString(),
                DocumentType = DocumentLogType.IDSDoc
            });
        }

        private async Task SaveToSharePoint(IFormFile formFile, int appId)
        {
            const string idsRefSharePointFolder = "References";
            var ctryApp = await _applicationService.GetById(appId);
            if (ctryApp != null)
            {
                var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                var recKey = SharePointViewModelService.BuildRecKey(ctryApp.CaseNumber, ctryApp.Country, ctryApp.SubCase);
                var folders = SharePointViewModelService.GetDocumentFolders(idsRefSharePointFolder, recKey);

                using (var stream = new MemoryStream())
                {
                    formFile.CopyTo(stream);
                    stream.Position = 0;
                    await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.IDS, folders, stream, formFile.FileName);
                }
            }
        } 

        #region Google Document Azure Blob Container
        private async Task<CPIFile> GetCPIGoogleDocumentFileStream(string fileName)
        {
            var path = GetCPIGoogleDocumentFilePath(fileName);

            var container = GetContainer();
            var blob = container.GetBlobClient(path);

            if (blob.Exists())
            {
                var originalFileName = fileName;
                var stream = new MemoryStream();
                await blob.DownloadToAsync(stream);
                stream.Position = 0;
                return new CPIFile
                {
                    FileName = fileName,
                    OrigFileName = string.IsNullOrEmpty(originalFileName) ? fileName : originalFileName,
                    ContentType = "application/octet-stream",
                    Stream = stream
                };
            }
            else
                return new CPIFile();
        }

        private string GetCPIGoogleDocumentFilePath(string fileName)
        {            
            var rootFolder = _googleIDSSettings.PatentDocFolder;

            if (string.IsNullOrEmpty(rootFolder))
                return string.Empty;

            fileName = Path.GetFileName(fileName);
            string path = Path.Combine(rootFolder, fileName);
            path = path.Replace('\\', '/');
            return path;
        }

        private Azure.Storage.Blobs.BlobContainerClient GetContainer()
        {            
            var containerName = _googleIDSSettings.StorageContainerName;
            if (string.IsNullOrEmpty(containerName))
                containerName = "google-patents";

            if (string.IsNullOrEmpty(containerName))
                throw new Exception("Storage container name must be specified.");

            containerName = containerName.ToLower();
            
            var credential = new Azure.Identity.ClientSecretCredential(_googleIDSSettings.StorageADTenantID, _googleIDSSettings.StorageAppClientID, _googleIDSSettings.StorageAppClientSecret);
            var containerEndpoint = string.Format(_googleIDSSettings.StorageUrl!, _googleIDSSettings.StorageAccountName, containerName);
            var container = new Azure.Storage.Blobs.BlobContainerClient(new Uri(containerEndpoint), credential);
            if (!container.Exists())
                container.Create();

            return container;
        }
        #endregion
        #endregion

        #region Currency Type Exchange Rates
        public async Task<int> UpdateCurrencyExRates()
        {
            var settings = await _defaultSettings.GetSetting();
            string serviceUrl = settings.GoogleApiURL ?? throw new Exception("CPI Google API URL not configured");

            var accessToken = await GetGoogleAPIAccessToken(serviceUrl);
            if (string.IsNullOrEmpty(accessToken))
                throw new Exception("Access token is empty");

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(serviceUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            client.Timeout = TimeSpan.FromMinutes(5);

            // Stream the response directly for better performance
            var response = await client.GetAsync("GetExRates");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error calling UpdateCurrencyExchangeRates: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<List<CurrencyExRateUpdateDTO>>();

            if (result != null && result.Count > 0)
                return await UpdateExchangeRates(result, settings);
            else
                return 0;
        }

        private async Task<int> UpdateExchangeRates(List<CurrencyExRateUpdateDTO> exRateUpdateDTOs, DefaultSetting settings)
        {
            if (exRateUpdateDTOs == null || exRateUpdateDTOs.Count == 0) return 0;

            var currencyTypeCodes = exRateUpdateDTOs.Select(d => d.CurrencyType).Distinct().ToList();

            var existingCurrencyTypeLookup = await _currencyTypeService.QueryableList
                .Where(d => !string.IsNullOrEmpty(d.CurrencyTypeCode) && currencyTypeCodes.Contains(d.CurrencyTypeCode))
                .ToDictionaryAsync(d => d.CurrencyTypeCode!);

            var newCurrencyTypes = new List<CurrencyType>();
            var systemUserName = "CurrencyUpdate";
            var userName = "CPI";
            var today = DateTime.Now;
            var defaultCurrency = settings.DefaultCurrency ?? "USD";

            foreach (var item in exRateUpdateDTOs)
            {
                if (string.IsNullOrEmpty(item.CurrencyType)) continue;

                double? defaultExchangeRate = defaultCurrency switch
                {
                    "EUR" => item.EUR_ExRate,
                    "GBP" => item.GBP_ExRate,
                    "DKK" => item.DKK_ExRate,
                    _     => item.USD_ExRate
                };

                if (existingCurrencyTypeLookup.TryGetValue(item.CurrencyType, out var existingItem))
                {
                    // Update rates
                    existingItem.USD_ExRate = item.USD_ExRate;
                    existingItem.USD_ExRateLastUpdate = item.USD_ExRateLastUpdate;

                    existingItem.EUR_ExRate = item.EUR_ExRate;
                    existingItem.EUR_ExRateLastUpdate = item.EUR_ExRateLastUpdate;

                    existingItem.GBP_ExRate = item.GBP_ExRate;
                    existingItem.GBP_ExRateLastUpdate = item.GBP_ExRateLastUpdate;

                    existingItem.DKK_ExRate = item.DKK_ExRate;
                    existingItem.DKK_ExRateLastUpdate = item.DKK_ExRateLastUpdate;

                    // Only overwrite the main ExchangeRate if it was previously set by the system
                    if (string.IsNullOrEmpty(existingItem.ExRateUpdatedBy) || 
                        existingItem.ExRateUpdatedBy.Equals(systemUserName, StringComparison.OrdinalIgnoreCase))
                    {
                        existingItem.ExchangeRate = defaultExchangeRate;
                        existingItem.ExRateLastUpdate = today;
                        existingItem.ExRateUpdatedBy = systemUserName;
                    }
                }
                else
                {
                    newCurrencyTypes.Add(new CurrencyType()
                    {
                        CurrencyTypeCode = item.CurrencyType,
                        Description = item.Description,
                        Symbol = item.Symbol,
                        ExchangeRate = defaultExchangeRate,
                        USD_ExRate = item.USD_ExRate,
                        USD_ExRateLastUpdate = item.USD_ExRateLastUpdate,
                        EUR_ExRate = item.EUR_ExRate,
                        EUR_ExRateLastUpdate = item.EUR_ExRateLastUpdate,
                        GBP_ExRate = item.GBP_ExRate,
                        GBP_ExRateLastUpdate = item.GBP_ExRateLastUpdate,
                        DKK_ExRate = item.DKK_ExRate,
                        DKK_ExRateLastUpdate = item.DKK_ExRateLastUpdate,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = today,
                        LastUpdate = today,
                        ExRateUpdatedBy = systemUserName,
                        ExRateLastUpdate = today,
                        CPICurrencyType = true
                    });
                }
            }

            if (existingCurrencyTypeLookup != null && existingCurrencyTypeLookup.Count > 0)
                await _currencyTypeService.Update(existingCurrencyTypeLookup.Values.ToList());

            if (newCurrencyTypes != null && newCurrencyTypes.Count > 0)
                await _currencyTypeService.Add(newCurrencyTypes);

            return exRateUpdateDTOs.Count;
        }
        #endregion
    }
}