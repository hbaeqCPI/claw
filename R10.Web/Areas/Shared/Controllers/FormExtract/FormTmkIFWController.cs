using AutoMapper.QueryableExtensions;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OpenIddict.Validation.AspNetCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.FormExtract;
using R10.Core.Entities.Trademark;
using R10.Core.Entities.Shared;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Trademark;
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
using R10.Web.Services.SharePoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static R10.Web.Helpers.ImageHelper;
using R10.Core.Services;
using Microsoft.Graph;
using MsgKit;
using R10.Core.Services.Shared;
using R10.Web.Services.iManage;
using R10.Web.Services.NetDocuments;


using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(AuthenticationSchemes = AuthSchemes, Policy = TrademarkAuthorizationPolicy.CanAccessSystem)]
    public class FormTmkIFWController : BaseController
    {
        private readonly AzureFormRecognizer _azureFormRecognizer;
        private readonly IFormIFWService _ifwService;
        // private readonly ITLInfoService _tlInfoService; // Removed during deep clean
        private readonly ITmkTrademarkService _tmkService;
        private readonly IDocumentStorage _documentStorage;
        private readonly ISystemSettings<TmkSetting> _settings;

        private readonly GraphSettings _graphSettings;
        private readonly ISharePointService _sharePointService;
        private readonly INotificationService _notificationService;

        private readonly IiManageClientFactory _iManageClientFactory;
        private readonly INetDocumentsClientFactory _netDocsClientFactory;
        private readonly IDocumentService _docService;

        private const string AuthSchemes = "Identity.Application" + "," + OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;

        public FormTmkIFWController(
                    AzureFormRecognizer azureFormRecognizer, 
                    IFormIFWService ifwService,
                    // ITLInfoService tlInfoService, // Removed during deep clean
                    ITmkTrademarkService tmkService,
                    IDocumentStorage documentStorage,
                    ISystemSettings<TmkSetting> settings,
                    ISharePointService sharePointService,
                    IOptions<GraphSettings> graphSettings,
                    INotificationService notificationService,
                    IiManageClientFactory iManageClientFactory,
                    INetDocumentsClientFactory netDocsClientFactory,
                    IDocumentService docService)
        {
            _azureFormRecognizer = azureFormRecognizer;
            _ifwService = ifwService;
            // _tlInfoService = tlInfoService; // Removed during deep clean
            _tmkService = tmkService;
            _documentStorage = documentStorage;
            _settings = settings;
            _sharePointService = sharePointService;
            _graphSettings = graphSettings.Value;
            _notificationService = notificationService;
            _iManageClientFactory = iManageClientFactory;
            _netDocsClientFactory = netDocsClientFactory;
            _docService = docService;
        }

        //[HttpPost]
        public async Task<IActionResult> IFWMassExtract()
        {
            var settings = await _settings.GetSetting();

            // Removed during deep clean - TL service no longer available
            // var ifws = await _tlInfoService.TLSearchApplicableIFWs
            //                       .Where(r => r.FormIFWDocType.IsEnabled && r.FormIFWDocType.SystemType == "T" && r.AIParseDate == null)
            //                       .Include(r=> r.TLSearch)
            //                       .Include(r=>r.FormIFWDocType).ThenInclude(d=> d.FormIFWActMaps)
            //                       .ToListAsync();
            var ifws = new List<object>(); // Placeholder - TL removed during deep clean
            var userName = "PO";

            // Removed during deep clean - TL IFW processing loop
            // foreach (var ifw in ifws) { ... }

            return Ok();
        }

        #region Helpers
        // extract pages from IFW pdf file, save and return file/path
        private async Task<TLFileInfo> CreateTempFile(int tmkId, string fileName, string scanPages = "")
        {
            var file = new CPIFile();

            var settings = await _settings.GetSetting();

            if (settings.DocumentStorage == DocumentStorageOptions.SharePoint) {
                var docFile = await _docService.DocFiles.FirstOrDefaultAsync(d => d.DocFileName == fileName);
                if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
                {
                    var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                    file.Stream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Trademark, docFile.DriveItemId);
                    file.FileName = fileName;
                }
                else file = null;
            }
            else if (settings.DocumentStorage == DocumentStorageOptions.iManage)
            {
                var docFile = await _docService.DocFiles.FirstOrDefaultAsync(d => d.DocFileName == fileName);
                if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
                {
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
                file = await _documentStorage.GetFileStream("Trademark", fileName, ImageHelper.CPiSavedFileType.Image);
            }
            
            if (file != null)
            {
                // extract applicable doc pages of the IFW to a temp file
                var folder = Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"UserFiles\Temporary Folder", User.GetUserName());
                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);
                var tempFilePath = Path.Combine(folder, file.FileName);

                var noPages = Helper.ExtractPdfPageCount(file.Stream);
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write)) {
                    file.Stream.CopyTo(fileStream);
                }

                //var noPages = ExtractPdfPages(file.Stream, tempFilePath, scanPages);
                if (System.IO.File.Exists(tempFilePath))
                   return new TLFileInfo { FilePath = tempFilePath, NoOfPages = noPages };

            }
            return null;
        }

        // extract IFW pdf file pages
        private int ExtractPdfPages(Stream fileStream, string tempFilePath, string scanPages)
        {
            int noPages = Helper.ExtractPdfPageCount(fileStream);
            int[] extractPages = new int[noPages];
            for (var i = 0; i <= noPages - 1; i++)
            {
                extractPages[i] = i;
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
            return noPages;
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

        private class TLFileInfo { 
            public string FilePath { get; set; }
            public int NoOfPages { get; set; }
        }        

        
    }
}