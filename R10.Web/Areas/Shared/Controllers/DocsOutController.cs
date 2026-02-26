using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R10.Web.Helpers;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Security;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using R10.Core.DTOs;
using R10.Core.Interfaces.Shared;
using R10.Web.Services.DocumentStorage;
// using R10.Core.Entities.AMS; // Removed during deep clean
using R10.Core.Entities;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
// using R10.Core.Entities.RMS; // Removed during deep clean
using R10.Core.Entities.Trademark;
using R10.Core.Entities.Patent;
// using R10.Core.Entities.ForeignFiling; // Removed during deep clean
using R10.Core.Helpers;
using Microsoft.Extensions.Localization;
using R10.Web.Areas.Patent.ViewModels;
using R10.Web.Models;
using R10.Core.Entities.Shared;
using R10.Core.Interfaces;
using R10.Core.Services.Shared;
using Microsoft.EntityFrameworkCore;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared")] //, Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    [Authorize] //DO NOT USE SHARED AUTH POLICY. SOME USERS MAY NOT HAVE SHARED SYSTEM/ROLE.
    public class DocsOutController : BaseController
    {
        private readonly IDocsOutService _service;
        private readonly IDocumentStorage _documentStorage;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IAuthorizationService _authService;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly ISharePointViewModelService _sharePointViewModelService;
        private readonly IDocumentHelper _documentHelper;
        private readonly IDocumentService _docService;

        private const string QuickEmailFolder = @"UserFiles\QuickEmails";
        private const string LetterFolder = @"UserFiles\Letters";

        public DocsOutController(IDocsOutService service, IDocumentStorage documentStorage,
            IHostingEnvironment hostingEnvironment, IStringLocalizer<SharedResource> localizer,
            IAuthorizationService authService, ISystemSettings<DefaultSetting> settings,
            ISharePointViewModelService sharePointViewModelService, IDocumentHelper documentHelper,
            IDocumentService docService)
        {
            _service = service;
            _documentStorage = documentStorage;
            _hostingEnvironment = hostingEnvironment;
            _localizer = localizer;
            _authService = authService;
            _settings = settings;
            _sharePointViewModelService = sharePointViewModelService;
            _documentHelper = documentHelper;
            _docService = docService;
        }

        public async Task<IActionResult> DocsOutRead([DataSourceRequest] DataSourceRequest request, string systemType, string parentKey, int parentValue, string documentCode )
        {
            if (ModelState.IsValid)
            {
                var criteria = new DocsOutCriteriaDTO
                {
                    SystemType = systemType,
                    DocumentCode = string.IsNullOrEmpty(documentCode) ? "All" : documentCode,
                    DataKey = parentKey,
                    DataKeyValue = parentValue
                };

                var docsOut = await _service.GetDocsOut(criteria);
                var result = docsOut.ToDataSourceResult(request);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }


        public async Task<IActionResult> DocsOutDelete([Bind(Prefix = "deleted")] DocsOutDTO deleted)
        {
            if (deleted.DocLogId > 0)
            {
                var canDeleteRecord = false;
                var settings = await _settings.GetSetting();
                var docLibrary = _sharePointViewModelService.GetDocLibraryFromDocumentCode(deleted.DocumentCode);

                if (deleted.SystemType == SystemTypeCode.Patent)
                    canDeleteRecord = (await _authService.AuthorizeAsync(User, deleted.DocumentCode == "Let" ? PatentAuthorizationPolicy.LetterModify : PatentAuthorizationPolicy.CanDelete)).Succeeded;
                else if (deleted.SystemType == SystemTypeCode.Trademark)
                    canDeleteRecord = (await _authService.AuthorizeAsync(User, deleted.DocumentCode == "Let" ? TrademarkAuthorizationPolicy.LetterModify : TrademarkAuthorizationPolicy.CanDelete)).Succeeded;

                if (canDeleteRecord) {
                    if (deleted.DocumentCode == "Let") {
                        await _service.LetterLogDelete(deleted.DocLogId);

                        if (settings.IsSharePointIntegrationOn)
                            return RedirectToAction("DeleteFile", "SharePointGraph", new { docLibrary = docLibrary, id = deleted.ItemId });
                        else
                            _documentHelper.DeleteLetterLogFile(deleted.LogFile);
                    }

                    else if (deleted.DocumentCode == "EFS") {
                        await _service.EFSLogDelete(deleted.DocLogId);
                        
                        if (settings.IsSharePointIntegrationOn)
                            return RedirectToAction("DeleteFile", "SharePointGraph", new { docLibrary = docLibrary, id = deleted.ItemId });
                        else
                            _documentHelper.DeleteEFSLogFile(deleted.LogFile);
                    }
                        
                }
                return Ok(new { success = _localizer["Document has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        public async Task<IActionResult> QEAttachmentsRead([DataSourceRequest] DataSourceRequest request, string system, int id)
        {
            if (ModelState.IsValid)
            {
                var qeLog = await _service.GetQELogByIdAsync(id);
                if (qeLog == null)
                    return new NoRecordFoundResult();

                var attachments = JsonConvert.DeserializeObject<List<AttachedFileDTO>>(qeLog.Attachments);

                //see QuickEmailController.AttachmentRead
                //attachments.ForEach(a => a.Thumbnail = ImageHelper.GetThumbnailIcon(a.Thumbnail));
                var docIcons = await _docService.DocIcons.ToListAsync();
                foreach (var item in attachments)
                {
                    var icon = docIcons.FirstOrDefault(i => i.FileExt.ToLower() == item.FileName.Split(".")[1].ToLower());
                    if (icon != null)
                    {
                        item.IconClass = icon.IconClass;
                    }
                    item.Send = true;
                }

                var result = attachments.ToDataSourceResult(request);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }


        public async Task<IActionResult> Open(string documentCode, int docLogId, string systemType, string parentKey, int parentValue)
        {
            var key = $"{systemType}|{parentKey}|{parentValue}|{docLogId}";
            switch (documentCode)
            {
                case "Let":
                {
                    return await DownloadLetter(docLogId,key);
                }
                case "EFS":
                {
                    return await DownloadEFS(docLogId,systemType,key);
                }
                case "REM":
                    return await OpenReminderLog(docLogId, systemType);
                default:
                {
                    return await OpenQuickEmail(key);
                }
            }
        }

        private async Task<IActionResult> DownloadLetter(int id, string key)
        {
            var letterLog = await _service.GetLetterLogByIdAsync(id);
            var system = QuickEmailHelper.GetSystem(letterLog.SystemType);
            var screen = await _service.GetScreenInfo(letterLog.ScreenId);
            var screenCode = screen==null ? "" : screen.ScreenCode.Split("-")[0];
            var keyInfo = key.Split("|");
            return RedirectToAction("DownloadLetterLog", "FileViewer", new { area = "", system, letterName = letterLog.LetFile,screenCode= screenCode, key= keyInfo[2]});
        }

        //private async Task<IActionResult> DownloadLetterOld(int id)
        //{
        //    var letterLog = await _service.GetLetterLogByIdAsync(id);
        //    var system = QuickEmailHelper.GetSystem(letterLog.SystemType);

        //    var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, LetterFolder, system, "Logs", letterLog.LetFile);

        //    if (Directory.Exists(filePath))
        //        return new PhysicalFileResult(filePath, ImageHelper.GetContentType(filePath))
        //        {
        //            FileDownloadName = letterLog.LetFile
        //        };
        //    else
        //        return BadRequest("File not found.");
        //}

        private async Task<IActionResult> DownloadEFS(int id, string systemType, string key)
        {
            var fileName = await _service.GetEFSLogFileNameByIdAsync(id);
            var systemName = QuickEmailHelper.GetSystem(systemType);
            
            var keyInfo = key.Split("|");
            return RedirectToAction("DownloadEFSLog", "FileViewer", new { area = "", system= systemName, fileName, screenCode = "CA", key = keyInfo[2] });
        }

        private async Task<IActionResult> OpenQuickEmail(string key)
        {
            var docLink = key.Split("|");
            int id = Convert.ToInt32(docLink[3]);

            var qeLog = await _service.GetQELogByIdAsync(id);
            if (qeLog == null)
                return new NoRecordFoundResult();
            else {
                ViewBag.Key = key;
                return PartialView("QuickEmailLog", qeLog);
            }
                
        }

        private Task<IActionResult> OpenReminderLog(int logEmailId, string systemType)
        {
            // Reminder log functionality removed - AMS/FF/RMS modules have been removed
            return Task.FromResult<IActionResult>(BadRequest("Reminder log is not available."));
        }

        [HttpPost]
        public Task<IActionResult> GetReminderAttachment(string systemType, string fileName)
        {
            // Reminder attachment functionality removed - AMS/FF/RMS modules have been removed
            return Task.FromResult<IActionResult>(BadRequest("File not found"));
        }

    }


}
