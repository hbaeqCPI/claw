using AutoMapper.QueryableExtensions;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OpenIddict.Validation.AspNetCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.FormExtract;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Security;
using R10.Web.Services;
using R10.Web.Services.DocumentStorage;
using R10.Web.Services.FormExtract;
using R10.Web.Services.iManage;
using R10.Web.Services.NetDocuments;
using R10.Web.Services.SharePoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static R10.Web.Helpers.ImageHelper;


using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(AuthenticationSchemes = AuthSchemes, Policy = PatentAuthorizationPolicy.CanAccessSystem)]
    public class FormIFWController : BaseController
    {
        private readonly AzureFormRecognizer _azureFormRecognizer;
        private readonly IFormIFWService _ifwService;
        private readonly IRTSService _rtsService;
        private readonly ICountryApplicationService _ctryAppService;
        private readonly IDocumentStorage _documentStorage;
        private readonly ISystemSettings<PatSetting> _settings;

        private readonly GraphSettings _graphSettings;
        private readonly ISharePointService _sharePointService;
        private readonly IiManageClientFactory _iManageClientFactory;
        private readonly INetDocumentsClientFactory _netDocsClientFactory;
        private readonly IDocumentService _docService;

        private const string AuthSchemes = "Identity.Application" + "," + OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;

        public FormIFWController(
                    AzureFormRecognizer azureFormRecognizer, 
                    IFormIFWService ifwService, 
                    IRTSService rtsService,
                    ICountryApplicationService ctryAppService,
                    IDocumentStorage documentStorage,
                    ISystemSettings<PatSetting> settings,
                    ISharePointService sharePointService,
                    IiManageClientFactory iManageClientFactory,
                    INetDocumentsClientFactory netDocsClientFactory,
                    IOptions<GraphSettings> graphSettings,
                    IDocumentService docService)
        {
            _azureFormRecognizer = azureFormRecognizer;
            _ifwService = ifwService;
            _rtsService = rtsService;
            _ctryAppService = ctryAppService;

            _documentStorage = documentStorage;
            _settings = settings;
            _sharePointService = sharePointService;
            _iManageClientFactory = iManageClientFactory;
            _netDocsClientFactory = netDocsClientFactory;
            _graphSettings = graphSettings.Value;
            _docService = docService;
        }

        //[HttpPost]
        public async Task<IActionResult> IFWMassExtract()
        {
            var settings = await _settings.GetSetting();

            //if (!settings.IsCognitiveSearchOn) //same CPI client module package
            //    return Ok();

            var hasIDSAutoDocket = await _settings.GetValue<bool>("RTS", "HasPAIR_IDS_Download");
            var idsCitedByExaminerDesc = await _settings.GetValue<string>("RTS", "IFWReferencesFromExaminerDesc");

            var ifws = await _rtsService.RTSSearchApplicableIFWs
                                  .Where(r => r.FormIFWDocType.IsEnabled && r.FormIFWDocType.SystemType=="P"  && r.AIParseDate == null)
                                  .Include(r=> r.RTSSearch)
                                  .Include(r=>r.FormIFWDocType).ThenInclude(d=> d.FormIFWActMaps)
                                  .OrderBy(r => r.PLAppID).ThenByDescending(r => r.OrderOfEntry)
                                  .ToListAsync();
            var userName = "PO";

            foreach (var ifw in ifws) {
                //var fileName = settings.DocumentStorage != DocumentStorageOptions.BlobOrFileSystem ? ifw.DocName : ifw.FileName;
                var fileName = ifw.FileName;

                var scanPages = ifw.FormIFWDocType.ScanPages;

                if (hasIDSAutoDocket && ifw.Description.ToLower() == idsCitedByExaminerDesc.ToLower())
                {
                    //parse it by page
                    for (int i = 0; i < ifw.NoPages; i++)
                    {
                        var tempFilePath = await CreateTempFile(ifw.RTSSearch.PMSAppId, fileName, ifw.PageStart + i, 1, scanPages);
                        if (!string.IsNullOrEmpty(tempFilePath))
                        {
                            var modelId = ifw.FormIFWDocType.ModelId;
                            var scanPageList = ScanPagesToList(scanPages, 1);
                            var extractedData = await _azureFormRecognizer.AnalyzeFormFile(modelId, tempFilePath, scanPageList);

                            // save analysis result
                            await _ifwService.SaveExtractedData(ifw.IFWId, ifw.DocTypeId, extractedData, userName, false);
                        }
                    }
                    await _ifwService.GenIFWIDSRecords(ifw.IFWId, userName);
                }
                else
                {
                    var tempFilePath = await CreateTempFile(ifw.RTSSearch.PMSAppId, fileName, ifw.PageStart, ifw.NoPages, scanPages);
                    if (!string.IsNullOrEmpty(tempFilePath))
                    {

                        // analyze
                        var modelId = ifw.FormIFWDocType.ModelId;
                        var scanPageList = ScanPagesToList(scanPages, ifw.NoPages);

                        var extractedData = await _azureFormRecognizer.AnalyzeFormFile(modelId, tempFilePath, scanPageList);

                        // save analysis result
                        await _ifwService.SaveExtractedData(ifw.IFWId, ifw.DocTypeId, extractedData, userName);

                        //generate actions
                        if (ifw.FormIFWDocType.FormIFWActMaps != null && ifw.FormIFWDocType.FormIFWActMaps.Any(m => m.IsGenAction))
                        {
                            await _ifwService.GenIFWAct(ifw.IFWId, userName);
                        }
                    }
                }
                
            }
            return Ok();
        }

        




        

        

        
        #region Helpers
        // extract pages from IFW pdf file, save and return file/path
        private async Task<string> CreateTempFile(int appId, string fileName, int pageStart, int noPages, string scanPages = "")
        {
            var file = new CPIFile();
            var settings = await _settings.GetSetting();

            if (settings.DocumentStorage== DocumentStorageOptions.SharePoint)
            {
                var docFile = await _docService.DocFiles.FirstOrDefaultAsync(d => d.DocFileName == fileName);
                if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
                {
                    var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                    file.Stream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Patent,docFile.DriveItemId);
                    file.FileName = fileName;
                }
                else file = null;
            }
            else if (settings.DocumentStorage == DocumentStorageOptions.iManage) {
                var docFile = await _docService.DocFiles.FirstOrDefaultAsync(d=> d.DocFileName==fileName);
                if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId)) {
                    using (var client = await _iManageClientFactory.GetClient())
                    {
                        file.Stream = await client.GetDocumentAsStream(docFile.DriveItemId);
                        file.FileName = fileName;
                    }
                }
                else file = null;
            }
            else if (settings.DocumentStorage == DocumentStorageOptions.NetDocuments)
            {
                var docFile = await _docService.DocFiles.FirstOrDefaultAsync(d => d.DocFileName == fileName);
                if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
                {
                    using (var client = await _netDocsClientFactory.GetClient())
                    {
                        file.Stream = await client.GetDocumentAsStream(docFile.DriveItemId);
                        file.FileName = fileName;
                    }
                }
                else file = null;
            }
            else
            {
                file = await _documentStorage.GetFileStream("Patent", fileName, ImageHelper.CPiSavedFileType.Image);
            }
            
            if (file != null)
            {
                // extract applicable doc pages of the IFW to a temp file
                var folder = Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\Temporary Folder", User.GetUserName());
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                var tempFilePath = Path.Combine(folder, file.FileName);

                ExtractPdfPages(file.Stream, tempFilePath, pageStart, noPages, scanPages);

                if (System.IO.File.Exists(tempFilePath))
                   return tempFilePath;

            }
            return null;
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
}