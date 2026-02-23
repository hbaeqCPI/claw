using GleamTech.DocumentUltimate.AspNet.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Shared;
using R10.Core.Services.Shared;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Security;
using R10.Web.Services.DocumentStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static R10.Web.Helpers.ImageHelper;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class DocViewerController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IDocsOutService _docsOutService;
        private readonly IDocumentHelper _documentHelper;
        private readonly IDocumentStorage _documentStorage;
        private readonly IDocumentPermission _documentPermission;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly IDocumentService _docService;
        private readonly ISystemSettings<PatSetting> _patSettings;

        public DocViewerController(
                IDocsOutService docsOutService,
                IDocumentHelper documentHelper,
                IDocumentStorage documentStorage,
                IStringLocalizer<SharedResource> localizer,
                ISystemSettings<DefaultSetting> settings,
                IDocumentPermission documentPermission,
                IDocumentService docService, 
                ISystemSettings<PatSetting> patSettings
        )
        {
            _docsOutService = docsOutService;
            _documentHelper = documentHelper;
            _documentStorage = documentStorage;
            _localizer = localizer;
            _settings = settings;
            _documentPermission = documentPermission;
            _docService = docService;
            _patSettings = patSettings;
        }

        public async Task<IActionResult> GetDocumentViewer(string system, string docFileName, string screenCode, int key, CPiSavedFileType fileType, bool isPartialView = false, DocumentViewerSidebarView sideBarView = DocumentViewerSidebarView.None)
        {
            if (!string.IsNullOrEmpty(docFileName))
            {
                var allowed = true;
                var pathSeparator = docFileName.Contains("/") ? "/" : @"\";
                var fileInfo = docFileName.Split(pathSeparator);
                var fileName = fileInfo[fileInfo.Length - 1];

                //temporary, check permission only when screencode is passed
                var pScreenCode = screenCode == "null" ? null : screenCode;
                if (!string.IsNullOrEmpty(pScreenCode))
                    allowed = await _documentPermission.HasPermission(User, system, screenCode, key, fileName, fileType);

                if (allowed) {
                    var settings = await _settings.GetSetting();
                    var docFilePath = _documentHelper.GetDocumentPath(docFileName);
                    if (!string.IsNullOrEmpty(docFilePath))
                    {
                        // log trade secret download
                        await LogDocTradeSecretActivity(docFileName);

                        var model = _documentHelper.GetDocumentViewerModel(docFilePath, settings.DocViewerWidth, settings.DocViewerHeight);
                        model.SidebarView = sideBarView;

                        if (isPartialView)
                            model.Id = "DocumentViewerPartial";                        

                        return PartialView("../Documents/_DocumentViewer", model);                            
                    }
                }                

            }

            var errorMsg = _localizer["Sorry, the document file is missing."];
            return PartialView("../Documents/_EmptyView", errorMsg);
        }

        public IActionResult PreviewDocument(string system, string screenCode, string docFileName, int key)
        {
            if (system.Length == 1)
                system = QuickEmailHelper.GetSystem(system);
            docFileName = _documentStorage.GetFilePath(system, docFileName, CPiSavedFileType.DocMgt);

            return RedirectToAction("GetDocumentViewer", new { system = system, screenCode = screenCode, docFileName = docFileName, key = key, fileType = CPiSavedFileType.DocMgt });
        }
        public IActionResult ZoomDocument(string system, string screenCode, string fileName, int key, CPiSavedFileType fileType)
        {
            ViewBag.System = system.Length == 1 ? QuickEmailHelper.GetSystem(system) : system;
            ViewBag.ScreenCode = screenCode;
            ViewBag.Key = key;
            ViewBag.FileType = fileType;
            fileName = _documentStorage.GetFilePath(system, fileName, fileType);

            return PartialView("../Documents/_DocumentZoom", fileName);
        }

        public IActionResult ZoomTempFile(string fileName)
        {
            ViewBag.FromTemp = true;
            return PartialView("../Documents/_DocumentZoom", fileName);
        }

        public async Task<IActionResult> GetTempFileDocumentViewer(string docFileName)
        {
            if (!string.IsNullOrEmpty(docFileName))
            {
                var settings = await _settings.GetSetting();
                var documentViewer = new DocumentViewer
                {
                    Width = settings.DocViewerWidth,
                    Height = settings.DocViewerHeight,
                    Resizable = true,
                    Document = docFileName
                };

                return PartialView("../Documents/_DocumentViewer", documentViewer);
            }
            var errorMsg = _localizer["Sorry, the document file is missing."];
            return PartialView("../Documents/_EmptyView", errorMsg);
        }


        public async Task<IActionResult> ViewDocument(string fileName, string key )
        {
            var docLink = key.Split("|");
            var recordKey = docLink[3] != null ? Convert.ToInt32(docLink[3]) : 0;
            var folderId = docLink[6] != null ? Convert.ToInt32(docLink[6]) : 0;
            var docType = docLink[4];
            ViewBag.System = QuickEmailHelper.GetSystem(docLink[0]);
            ViewBag.ScreenCode = docLink[1];
            ViewBag.Key = recordKey;

            if (docType == "user")
                ViewBag.FileType = CPiSavedFileType.DocMgt;
            else
                ViewBag.FileType = await _documentStorage.GetFileType(folderId);    //not the best approach
            
            return PartialView("../Documents/_DocumentZoom", fileName);
        }

        public async Task<IActionResult> ZoomImageLink(string system, string imageFile, string screenCode, int key, CPiSavedFileType fileType= CPiSavedFileType.Image)         // images/links tab (aka Documents tab on main screens)
        {
            if (ImageHelper.IsUrl(imageFile))
                return Redirect(imageFile);
           
            ViewBag.System = system;
            ViewBag.ScreenCode = screenCode;
            ViewBag.Key = key;
            ViewBag.FileType = fileType;

            // log trade secret download
            await LogDocTradeSecretActivity(imageFile);

            var fileName = _documentStorage.GetFilePath(system, imageFile, fileType);

            return PartialView("../Documents/_DocumentZoom", fileName);
        }

        public IActionResult ZoomIDSLink(string docfile, int key)
        {
            var system = "Patent";

            ViewBag.System = system;
            ViewBag.ScreenCode = "IDS";
            ViewBag.Key = key;
            ViewBag.FileType = CPiSavedFileType.Image;

            var fileName = _documentStorage.GetFilePath(system, docfile, CPiSavedFileType.Image);
            return PartialView("../Documents/_DocumentZoom", fileName);
        }

        public IActionResult ZoomAssignmentLink(string system, string docfile, int key)
        {
            ViewBag.System = system;
            ViewBag.ScreenCode = ScreenCode.Assignment;
            ViewBag.Key = key;
            ViewBag.FileType = CPiSavedFileType.Assignment;

            var fileName = _documentStorage.GetFilePath(system, docfile, CPiSavedFileType.Assignment);
            return PartialView("../Documents/_DocumentZoom", fileName);
        }

        public IActionResult ZoomLicenseeLink(string system, string docfile, int key)
        {
            ViewBag.System = system;
            ViewBag.ScreenCode = ScreenCode.Licensee;
            ViewBag.Key = key;
            ViewBag.FileType = CPiSavedFileType.Licensees;

            var fileName = _documentStorage.GetFilePath(system, docfile, CPiSavedFileType.Licensees);
            return PartialView("../Documents/_DocumentZoom", fileName);
        }

        public async Task<IActionResult> ZoomDocsOut(string systemType, string documentCode, int docLogId, string screenCode, int key)
        {
            var systemName = QuickEmailHelper.GetSystem(systemType);
            string fileName = "";

            ViewBag.System = systemName;
            ViewBag.ScreenCode = screenCode;
            ViewBag.Key = key;

            switch (documentCode)
            {
                case "Let":
                    var letterLog = await _docsOutService.GetLetterLogByIdAsync(docLogId);
                    fileName = @"Searchable\Logs\Letters\" + letterLog.LetFile;
                    ViewBag.FileType = CPiSavedFileType.Letter;
                    break;
                case "EFS":
                    var logFile = await _docsOutService.GetEFSLogFileNameByIdAsync(docLogId);
                    fileName = @"Searchable\Logs\EFS\" + logFile;
                    ViewBag.FileType = CPiSavedFileType.EFS;
                    break;
                default:
                    var qeLog = await _docsOutService.GetQELogByIdAsync(docLogId);
                    if (!string.IsNullOrEmpty(qeLog.QEFile)) {
                        ViewBag.FileType = CPiSavedFileType.QE;
                        fileName = @"Searchable\Logs\QuickEmails\" + qeLog.QEFile;
                    }
                        
                    break;
            }
            return PartialView("../Documents/_DocumentZoom", fileName);
        }

        public IActionResult ZoomGSDoc(string systemType, string screenCode, string documentType, int parentId, string fileName)         // global search zoom
        {

            var systemName = QuickEmailHelper.GetSystem(systemType);
            var fileType = GetCPiSavedFileType(documentType);
            ViewBag.System = systemName;
            ViewBag.ScreenCode = screenCode;
            ViewBag.Key = parentId;
            ViewBag.FileType = fileType;
            
            var filePath = _documentStorage.GetFilePath(systemName, fileName, fileType);
            return PartialView("../Documents/_DocumentZoom", filePath);
        }

        private CPiSavedFileType GetCPiSavedFileType(string documentType)
        {
            switch (documentType)
            {
                case DocumentLogType.ImageDoc: return CPiSavedFileType.Image;
                case DocumentLogType.IDSDoc: return CPiSavedFileType.IDSReferences;
                case DocumentLogType.LetterLog: return CPiSavedFileType.Letter;
                case DocumentLogType.EFSLog: return CPiSavedFileType.EFS;
                case DocumentLogType.EmailLog: return CPiSavedFileType.QE;
                case DocumentLogType.EmailLogAttachment: return CPiSavedFileType.QELoggedImage;
                case DocumentLogType.DocMgt: return CPiSavedFileType.DocMgt;
                default: return CPiSavedFileType.Thumbnail;
            }
        }

        private async Task LogDocTradeSecretActivity(string fileName)
        {
            // log trade secret download
            var settings = await _patSettings.GetSetting();
            if (settings.IsTradeSecretOn)
                await _docService.LogDocTradeSecretActivityByFileName(fileName);
        }
    }
}
