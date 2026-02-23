using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Options;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using R10.Web.Services;
using R10.Web.Services.DocumentStorage;
using R10.Web.Services.FormExtract;
using R10.Web.Services.SharePoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.Services
{
    public class DocumentsAIViewModelService: IDocumentsAIViewModelService
    {
        private ISystemSettings<DefaultSetting> _settings;
        private readonly IDocumentService _docService;
        private readonly IDocumentStorage _documentStorage;
        private readonly ClaimsPrincipal _user;
        private readonly AzureFormRecognizer _azureFormRecognizer;
        private readonly IFormIFWService _ifwService;
        private readonly ISharePointService _sharePointService;
        private readonly GraphSettings _graphSettings;
        private readonly ICountryApplicationService _ctryAppService;

        public DocumentsAIViewModelService(
                    ISystemSettings<DefaultSetting> settings,
                    IDocumentService docService,
                    IDocumentStorage documentStorage,
                    ClaimsPrincipal user, AzureFormRecognizer azureFormRecognizer, IFormIFWService ifwService,
                    ISharePointService sharePointService, IOptions<GraphSettings> graphSettings, ICountryApplicationService ctryAppService)
        {
            _settings = settings;
            _docService = docService;
            _documentStorage = documentStorage;
            _user = user;
            _azureFormRecognizer = azureFormRecognizer;
            _ifwService = ifwService;
            _sharePointService = sharePointService;
            _graphSettings = graphSettings.Value;
            _ctryAppService = ctryAppService;
        }

        public async Task ProcessUploadedDocuments(List<DocDocument> documents) {
            var docsToProcess = documents.Where(d=> d.DocFile != null && d.DocFile.DocFileName != null && d.DocFile.DocFileName.ToLower().EndsWith(".pdf")).ToList();
            if (docsToProcess.Any()) {
                var systemType = docsToProcess.First().DocFolder.SystemType;

                var docsForAI = (await _ifwService.GetDocumentsForAI()).Where(d => d.SystemType == systemType).ToList();
                var idsCitedByExaminerDesc = await _settings.GetValue<string>("RTS", "IFWReferencesFromExaminerDesc");
                var userName = "PO";
                var settings = await _settings.GetSetting();

                foreach (var doc in docsToProcess)
                {
                    var docForAI = docsForAI.Where(d => doc.DocName.ToLower().Contains(d.DocDesc.ToLower()) || (doc.DocFile.UserFileName !=null && doc.DocFile.UserFileName.ToLower().Contains(d.DocDesc.ToLower()))).FirstOrDefault();
                    if (docForAI != null)
                    {
                        var fileName = doc.DocFile.DocFileName;
                        var scanPages = docForAI.ScanPages;

                        var file = new CPIFile();
                        if (settings.IsSharePointIntegrationOn)
                        {
                            var ctryApp = await _ctryAppService.GetById(documents.First().DocFolder.DataKeyValue);
                            if (ctryApp != null) {
                                var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                                var driveItemId = documents.First().DocFile.DriveItemId;

                                var stream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Patent, driveItemId);
                                if (stream != null)
                                {
                                    file.Stream = stream;
                                    file.FileName = fileName;
                                }
                                else file = null;
                            }
                        }
                        else {
                            file = await _documentStorage.GetFileStream("", fileName, ImageHelper.CPiSavedFileType.DocMgt);
                        }
                        
                        if (file != null) {
                            var noPages = Helper.ExtractPdfPageCount(file.Stream);
                            if (noPages > 0) {
                                var pageStart = 1;
                                if (docForAI.DocDesc.ToLower() == idsCitedByExaminerDesc.ToLower())
                                {
                                    //parse it by page
                                    for (int i = 0; i < noPages; i++)
                                    {
                                        var tempFilePath = await CreateTempFile(file.Stream, fileName, pageStart + i, 1, scanPages);
                                        if (!string.IsNullOrEmpty(tempFilePath))
                                        {
                                            var modelId = docForAI.ModelId;
                                            var scanPageList = ScanPagesToList(scanPages, 1);
                                            var extractedData = await _azureFormRecognizer.AnalyzeFormFile(modelId, tempFilePath, scanPageList);

                                            // save analysis result
                                            await _ifwService.SaveExtractedDocData(doc.DocId, docForAI.DocTypeId, extractedData, userName, false);
                                            
                                            if (File.Exists(tempFilePath))
                                                File.Delete(tempFilePath);
                                        }
                                    }
                                    await _ifwService.GenDocIDSRecords(doc.DocId, doc.DocFolder.DataKeyValue, userName);
                                }
                                else
                                {
                                    var tempFilePath = await CreateTempFile(file.Stream, fileName, pageStart, noPages, scanPages);
                                    if (!string.IsNullOrEmpty(tempFilePath))
                                    {
                                        // analyze
                                        var modelId = docForAI.ModelId;
                                        var scanPageList = ScanPagesToList(scanPages, noPages);

                                        var extractedData = await _azureFormRecognizer.AnalyzeFormFile(modelId, tempFilePath, scanPageList);

                                        // save analysis result
                                        await _ifwService.SaveExtractedDocData(doc.DocId, docForAI.DocTypeId, extractedData, userName);

                                        //generate actions
                                        if (docForAI.FormIFWActMaps != null && docForAI.FormIFWActMaps.Any(m => m.IsGenAction))
                                        {
                                            if (systemType == SystemTypeCode.Patent)
                                            {
                                                await _ifwService.GenDocAction(doc.DocId, doc.DocFolder.DataKeyValue, userName);
                                            }
                                            else {
                                                await _ifwService.GenDocActionTmk(doc.DocId, doc.DocFolder.DataKeyValue, userName);
                                            }
                                        }
                                    }
                                }

                            }
                        }

                    }
                }
            }
            
        }

        #region Helpers
        // extract pages from pdf file, save and return file/path
        private async Task<string> CreateTempFile(Stream stream, string fileName, int pageStart, int noPages, string scanPages = "")
        {
            // extract applicable doc pages  to a temp file
            var folder = Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\Temporary Folder", _user.GetUserName());
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var tempFilePath = Path.Combine(folder, fileName);

            ExtractPdfPages(stream, tempFilePath, pageStart, noPages, scanPages);

            if (System.IO.File.Exists(tempFilePath))
                return tempFilePath;
            return string.Empty;

        }


        // extract IFW pdf file pages
        private void ExtractPdfPages(Stream fileStream, string tempFilePath, int pageStart, int noPages, string scanPages)
        {
            int[] extractPages = new int[noPages];
            for (var i = 0; i <= noPages - 1; i++)
            {
                extractPages[i] = pageStart + i;
            }

            if (scanPages.Length > 0)
            {
                var recognizerPages = RecognizerScanPages(extractPages, scanPages);
                Helper.ExtractPdfPage(fileStream, recognizerPages, tempFilePath);
            }
            else
            {
                Helper.ExtractPdfPage(fileStream, extractPages, tempFilePath);
            }
        }

        private int[] RecognizerScanPages(int[] pdfPages, string scanPages)
        {
            var pageList = new List<int>();
            int maxPage = pdfPages.Length;

            var aPages = scanPages.Split(",");
            for (int i = 0; i < aPages.Length; i++)
            {
                if (aPages[i].Contains("-"))
                {
                    var aSubPages = aPages[i].Split("-");
                    int first;
                    int second;
                    if (Int32.TryParse(aSubPages[0], out first) && Int32.TryParse(aSubPages[1], out second))
                    {
                        for (int j = first; j <= second && j >= 1 && j <= maxPage; j++)
                        {
                            pageList.Add(pdfPages[j - 1]);
                        }
                    }
                }
                else
                {
                    int num;
                    if (Int32.TryParse(aPages[i], out num))
                    {
                        if (num >= 1 && num <= maxPage)
                            pageList.Add(pdfPages[num - 1]);
                    }

                }
            }
            return pageList.ToArray();
        }

        // filters the pages to submit to Azure Form Recognizer
        private List<string> ScanPagesToList(string scanPages, int maxPage)
        {
            var pageList = new List<string>();

            var aPages = scanPages.Split(",");
            for (int i = 0; i < aPages.Length; i++)
            {
                if (aPages[i].Contains("-"))
                {
                    var aSubPages = aPages[i].Split("-");
                    int first;
                    int second;
                    if (Int32.TryParse(aSubPages[0], out first) && Int32.TryParse(aSubPages[1], out second))
                    {
                        for (int j = first; j <= second && j >= 1 && j <= maxPage; j++)
                        {
                            pageList.Add(j.ToString());
                        }
                    }
                }
                else
                {
                    int num;
                    if (Int32.TryParse(aPages[i], out num))
                    {
                        if (num >= 1 && num <= maxPage)
                            pageList.Add(num.ToString());
                    }

                }
            }
            return pageList;
        }

        #endregion
    }


    public interface IDocumentsAIViewModelService {
        Task ProcessUploadedDocuments(List<DocDocument> documents);
    }
}
