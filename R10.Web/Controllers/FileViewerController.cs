using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
//using DocumentFormat.OpenXml.Vml;
using DocuSign.eSign.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Options;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Services.Documents;
using R10.Core.Services.Shared;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Filters;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Services;
using R10.Web.Services.DocumentStorage;
using R10.Web.Services.iManage;
using R10.Web.Services.SharePoint;
using SmartFormat.Utilities;
using static R10.Web.Helpers.ImageHelper;
using R10.Web.Services.NetDocuments;

namespace R10.Web.Controllers
{
    [Authorize]
    public class FileViewerController : Microsoft.AspNetCore.Mvc.Controller
    {
        protected readonly IAuthorizationService _authorizationService;
        protected readonly IDocumentStorage _documentStorage;
        protected readonly IDocumentPermission _documentPermission;
        private readonly ReportSettings _reportSettings;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly ISharePointService _sharePointService;
        private readonly GraphSettings _graphSettings;
        private readonly IiManageClientFactory _iManageClientFactory;
        protected readonly IDocumentsViewModelService _docViewModelService;
        private readonly IDocumentService _docService;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly INetDocumentsClientFactory _netDocumentsClientFactory;

        public FileViewerController(
            IAuthorizationService authorizationService, IOptions<ReportSettings> reportSettings,
            IDocumentStorage documentStorage, IDocumentPermission documentPermission,
            ISystemSettings<DefaultSetting> settings, ISharePointService sharePointService, 
            IOptions<GraphSettings> graphSettings, IiManageClientFactory iManageClientFactory,
            IDocumentsViewModelService docViewModelService, 
            IDocumentService docService, ISystemSettings<PatSetting> patSettings,
            INetDocumentsClientFactory netDocumentsClientFactory)
        {
            _authorizationService = authorizationService;
            _reportSettings = reportSettings.Value;
            _documentStorage = documentStorage;
            _documentPermission = documentPermission;
            _settings = settings;
            _sharePointService = sharePointService;
            _graphSettings = graphSettings.Value;
            _iManageClientFactory = iManageClientFactory;
            _docViewModelService = docViewModelService;
            _docService = docService;
            _patSettings = patSettings;
            _netDocumentsClientFactory = netDocumentsClientFactory;
        }

        public async Task<IActionResult> GetThumbnail(string system, string thumbnailFile, string screenCode, int key, 
                                                            CPiSavedFileType fileType = CPiSavedFileType.DocMgtThumbnail)
        {
            if (thumbnailFile==null || (!thumbnailFile.StartsWith("logo") && !await _documentPermission.HasPermission(User,system, screenCode, key, thumbnailFile, fileType)))
                return BadRequest("File not found.");

            // log trade secret download
            await LogDocTradeSecretActivity(thumbnailFile);

            var file = await _documentStorage.GetFileStream(system, thumbnailFile, fileType);
            if (file != null)
                return new FileStreamResult(file.Stream, file.ContentType) { FileDownloadName = thumbnailFile };
            else
            {
                var path = ImageHelper.GetPhysicalFilePath(system, thumbnailFile, ImageHelper.CPiSavedFileType.Thumbnail);
                if (!thumbnailFile.StartsWith("logo_"))
                    path = path.Replace(thumbnailFile, "logo_NoPreview.jpg");

                return new PhysicalFileResult(path, ImageHelper.GetContentType(path)) { FileDownloadName = "logo_NoPreview.jpg" };
            }

        }


        public IActionResult GetDocMgtThumbnail(string system, string thumbnailFile, string screenCode, int key)
        {
            return RedirectToAction("GetThumbnail", new { system = system, thumbnailFile = thumbnailFile, screenCode = screenCode, key = key, fileType = CPiSavedFileType.DocMgtThumbnail });
        }

        public async Task<IActionResult> GetImage(string system, string filename, string screenCode, int key)
        {
            if (!await _documentPermission.HasPermission(User,system, screenCode, key, filename, CPiSavedFileType.DocMgt))
                return BadRequest("File not found.");

            // log trade secret download
            await LogDocTradeSecretActivity(filename);

            var file = await _documentStorage.GetFileStream(system, filename, ImageHelper.CPiSavedFileType.DocMgt);
            if (file != null)
                return new FileStreamResult(file.Stream, file.ContentType) { FileDownloadName = filename };
            else
                return BadRequest("File not found.");
        }

        public async Task<IActionResult> GetPTODoc(string system, string filename, string screenCode, int key,int noPages, int pageStart,int zoom=0)
        {
            if (!await _documentPermission.HasPermission(User, system, screenCode, key, filename, CPiSavedFileType.Image))
                return BadRequest("File not found.");


            var file = new CPIFile();
            var settings = await _settings.GetSetting();

            if (settings.DocumentStorage == DocumentStorageOptions.SharePoint)
            {
                var docFile = await _docViewModelService.GetDocFileByDocFileName(filename);
                if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
                {
                    var graphClient = _sharePointService.GetGraphClient();
                    var docLibrary = system == SystemType.Patent ? SharePointDocLibrary.Patent : SharePointDocLibrary.Trademark;
                    file.Stream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, docFile.DriveItemId);
                    file.FileName = filename;
                }
                else file = null;
            }
            else if (settings.DocumentStorage == DocumentStorageOptions.iManage)
            {
                var docFile = await _docViewModelService.GetDocFileByDocFileName(filename);
                if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
                {
                    using (var client = await _iManageClientFactory.GetClient())
                    {
                        file.Stream = await client.GetDocumentAsStream(docFile.DriveItemId);
                        file.FileName = filename;
                    }
                }
                else file = null;
            }
            else if (settings.DocumentStorage == DocumentStorageOptions.NetDocuments)
            {
                var docFile = await _docViewModelService.GetDocFileByDocFileName(filename);
                if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
                {
                    using (var client = await _netDocumentsClientFactory.GetClient())
                    {
                        file.Stream = await client.GetDocumentAsStream(docFile.DriveItemId);
                        file.FileName = filename;
                    }
                }
                else file = null;
            }
            else
            {
                file = await _documentStorage.GetFileStream(system, filename, ImageHelper.CPiSavedFileType.Image);
            }
            
            if (file != null) {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\Temporary Folder", User.GetUserName());
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                var temporaryPath = Path.Combine(folder, file.FileName);

                if (noPages > 0)
                {
                    int[] pages = new int[noPages];
                    for (var i = 0; i <= noPages - 1; i++)
                    {
                        pages[i] = pageStart + i;
                    }
                    Helper.ExtractPdfPage(file.Stream, pages, temporaryPath);
                }
                else { 
                    using (var fileStream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write))
                    {
                        file.Stream.CopyTo(fileStream);
                    }
                }

                if (zoom == 0)
                    return new PhysicalFileResult(temporaryPath, ImageHelper.GetContentType(temporaryPath)) { FileDownloadName = file.FileName };
                else
                {
                    return RedirectToAction("ZoomTempFile", "DocViewer", new { Area = "Shared", filename = temporaryPath });
                }
            }
            else
                return BadRequest("File not found.");
        }

        public async Task<IActionResult> GetEPOMailFile(string system, string screenCode, int appId, string filename, int zoom = 0)
        {
            if (!string.IsNullOrEmpty(system) && !string.IsNullOrEmpty(screenCode) && appId > 0)
            {
                if (!await _documentPermission.HasPermission(User, system, screenCode, appId, filename, CPiSavedFileType.DocMgt))
                    return BadRequest("File not found.");
            }                

            if (zoom == 0)
            {
                var file = await _documentStorage.GetFileStream(system, filename, ImageHelper.CPiSavedFileType.DocMgt);
                if (file != null)
                {
                    var fileName = HttpUtility.UrlPathEncode(file.OrigFileName);
                    Response.Headers.Add("Content-Disposition", "inline; filename=" + fileName.Replace(";", " ").Replace(":", "-"));

                    return File(file.Stream, file.ContentType, file.OrigFileName);
                }
                else
                    return BadRequest("File not found.");
            }
            else
            {
                return RedirectToAction("ZoomDocument", "DocViewer", new { Area = "Shared", system = system, screenCode = screenCode, key = appId, filename = filename, fileType = CPiSavedFileType.DocMgt });
            }            
        }

        public async Task<IActionResult> ViewImage(string system, string thumbnailFile, string screenCode, int key, CPiSavedFileType fileType = CPiSavedFileType.DocMgt)
        {

            if (ImageHelper.IsUrl(thumbnailFile)) {
                //return Redirect(thumbnailFile);
                return BadRequest();
            }
                

            if (!await _documentPermission.HasPermission(User, system, screenCode, key, thumbnailFile, fileType))
                return BadRequest("File not found.");

            // log trade secret download
            await LogDocTradeSecretActivity(thumbnailFile);

            var file = await _documentStorage.GetFileStream(system, thumbnailFile, fileType);
            if (file != null)
            {
                var fileName = HttpUtility.UrlPathEncode(file.OrigFileName);
                Response.Headers.Add("Content-Disposition", "inline; filename=" + fileName.Replace(";"," ").Replace(":","-").Replace(",", " "));
                return File(file.Stream, file.ContentType);
            }
            else
                return BadRequest("File not found.");

        }

        //Quick Email
        public async Task<IActionResult> ViewLogImage(string system, string fileId, string screenCode, int key)
        {
            if (!await _documentPermission.HasPermission(User,system, screenCode, key, fileId, CPiSavedFileType.QELoggedImage))
                return BadRequest("File not found.");

            var file = await _documentStorage.GetFileStream(system, fileId, ImageHelper.CPiSavedFileType.QELoggedImage);
            if (file != null)
            {
                Response.Headers.Add("Content-Disposition", "inline; filename=" + fileId);
                return File(file.Stream, file.ContentType);
            }
            else
                return BadRequest("File not found.");
        }

        public async Task<IActionResult> DownloadLetterLog(string system, string letterName, string screenCode, int key)
        {
            if (!await _documentPermission.HasPermission(User,system, screenCode, key, letterName, CPiSavedFileType.Letter))
                return BadRequest("File not found.");

            var path = _documentStorage.BuildPath(_documentStorage.LetterLogFolder, system, letterName);
            var file = await _documentStorage.GetFileStream(path);
            if (file != null) {
                file.Position = 0;
                return new FileStreamResult(file, ImageHelper.GetContentType(letterName)) { FileDownloadName = letterName };
            }
                
            else
                return BadRequest("File not found.");

        }

        public async Task<IActionResult> DownloadEFSLog(string system, string fileName, string screenCode, int key)
        {
            if (!await _documentPermission.HasPermission(User, system, screenCode, key, fileName, CPiSavedFileType.EFS))
                return BadRequest("File not found.");

            var path = _documentStorage.BuildPath(_documentStorage.EFSLogFolder, system, fileName);
            var file = await _documentStorage.GetFileStream(path);
            if (file != null) {
                file.Position = 0;
                return new FileStreamResult(file, ImageHelper.GetContentType(fileName)) { FileDownloadName = fileName };
            }
            else
                return BadRequest("File not found.");
        }

        public async Task<IActionResult> GetLogPreviewUrl(string documentCode, string id)
        {
            //var graphClient = _sharePointService.GetGraphClient();
            var graphClient = _sharePointService.GetGraphClientByClientCredentials();
            var docLibrary = "";
            if (documentCode == "Let")
            {
                docLibrary = SharePointDocLibrary.LetterLog;
            }
            else if (documentCode == "QE")
            {
                docLibrary = SharePointDocLibrary.QELog;
            }

            else if (documentCode == "EFS")
            {
                docLibrary = SharePointDocLibrary.IPFormsLog;
            }

            if (!string.IsNullOrEmpty(docLibrary))
            {
                var previewUrl = await graphClient.GetSiteDriveItemPreviewUrl(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, id);
                return Json(new { previewUrl });
            }
            return BadRequest();
        }

        public async Task<IActionResult> GetLogEditUrl(string documentCode, string id)
        {
            //var graphClient = _sharePointService.GetGraphClient();
            var graphClient = _sharePointService.GetGraphClientByClientCredentials();

            var docLibrary = "";
            if (documentCode == "Let")
            {
                docLibrary = SharePointDocLibrary.LetterLog;
            }
            else if (documentCode == "QE")
            {
                docLibrary = SharePointDocLibrary.QELog;
            }

            if (!string.IsNullOrEmpty(docLibrary))
            {
                var driveItem = await graphClient.GetSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, id);
                return Json(new { editUrl = driveItem.WebUrl });
            }
            return BadRequest();
        }

        public IActionResult GetSSRSHtmlImage(string ReportServerUrl)
        {
            var fixedReportServerUrl = ReportServerUrl.Substring(0, ReportServerUrl.IndexOf("/ReportServer") + 13);
            string imgUrl = fixedReportServerUrl + "?" + Request.QueryString.ToString().Substring("?ReportServerUrl=".Length + fixedReportServerUrl.Length);

            var clientCredentials = new NetworkCredential();

            if (_reportSettings.UseNtlmAuthentication)
            {
                clientCredentials = new NetworkCredential(_reportSettings.UserName, _reportSettings.Password, _reportSettings.Domain);
            }
            else
            {
                clientCredentials = (NetworkCredential)CredentialCache.DefaultCredentials;
            }

            using (WebClient webClient = new WebClient())
            {
                webClient.Credentials = clientCredentials;
                byte[] data = webClient.DownloadData(imgUrl);

                using (MemoryStream memoryStream = new MemoryStream(data))
                {
                    return new FileContentResult(memoryStream.ToArray(), "application/octet-stream");
                }

            }
        }

        [AllowAnonymous(),AllowedIPOnly()]
        public async Task<IActionResult> GetSSRSImage(string system, string imageFile, string? driveItemId) 
        {
            var settings = await _settings.GetSetting();

            //use local logo icons if thumbnail is not an image
            if (system.ToLower() == "thumbnails" && imageFile.StartsWith("logo_"))
                return GetDocumentIcon(system, imageFile);

            //iManage integration
            if (settings.DocumentStorage == DocumentStorageOptions.iManage)
            {
                if (string.IsNullOrEmpty(driveItemId))
                    return await GetIManageDocumentByDocFileName(imageFile);

                return await GetIManageDocument(driveItemId);
            }
            //NetDocs integration
            else if (settings.DocumentStorage == DocumentStorageOptions.NetDocuments)
            {
                if (string.IsNullOrEmpty(driveItemId))
                    return await GetNetDocsDocumentByDocFileName(imageFile);

                return await GetNetDocsDocument(driveItemId);
            }
            //SharePoint integration
            else if (settings.DocumentStorage == DocumentStorageOptions.SharePoint)
                return await GetSharePointDocumentByDocFileName(system, imageFile);

            //show no preview icon if document is not an image
            if (!ImageHelper.IsImageFile(imageFile.Substring(imageFile.LastIndexOf('.')).ToLower()))
                return GetDocumentIcon(system, imageFile);

            //pass system and imageFile together in imageFile
            if (system == null && imageFile.Contains("/"))
            {
                system = imageFile.Substring(0, imageFile.IndexOf("/"));
                imageFile = imageFile.Substring(imageFile.IndexOf("/")+1);
            }     

            var file = await _documentStorage.GetFileStream(system, imageFile, "Thumbnails".ToLower().Equals(system.ToLower())? ImageHelper.CPiSavedFileType.Thumbnail: ImageHelper.CPiSavedFileType.Image);
            if (file != null) {
                return new FileStreamResult(file.Stream, file.ContentType) { FileDownloadName = imageFile };
            }
            else
                return BadRequest();
        }

        private IActionResult GetDocumentIcon(string system, string thumbnailFile)
        {
            var path = ImageHelper.GetPhysicalFilePath(system, thumbnailFile, ImageHelper.CPiSavedFileType.Thumbnail);
            if (!thumbnailFile.StartsWith("logo_"))
                path = path.Replace(thumbnailFile, "logo_NoPreview.jpg");

            return new PhysicalFileResult(path, ImageHelper.GetContentType(path)) { FileDownloadName = "logo_NoPreview.jpg" };
        }

        private async Task<IActionResult> GetIManageDocumentByDocFileName(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            var docFile = await _docViewModelService.GetDocFileByDocFileName(fileName);
            if (docFile == null || string.IsNullOrEmpty(docFile.DriveItemId))
                return BadRequest();

            return await GetIManageDocument(docFile.DriveItemId);
        }

        private async Task<IActionResult> GetIManageDocument(string? id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            using (var client = await _iManageClientFactory.GetServiceClient())
            {
                return await client.DownloadDocument(id);
            }
        }

        private async Task<IActionResult> GetNetDocsDocumentByDocFileName(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            var docFile = await _docViewModelService.GetDocFileByDocFileName(fileName);
            if (docFile == null || string.IsNullOrEmpty(docFile.DriveItemId))
                return BadRequest();

            return await GetNetDocsDocument(docFile.DriveItemId);
        }

        private async Task<IActionResult> GetNetDocsDocument(string? id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            using (var client = await _netDocumentsClientFactory.GetServiceClient())
            {
                return await client.DownloadDocument(id);
            }
        }

        private async Task<IActionResult> GetSharePointDocumentByDocFileName(string system, string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            var docFile = await _docViewModelService.GetDocFileByDocFileName(fileName);
            if (docFile == null || string.IsNullOrEmpty(docFile.DriveItemId) || _graphSettings.Site == null)
                return BadRequest();

            var graphClient = _sharePointService.GetGraphClientByClientCredentials();
            var stream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, system, docFile.DriveItemId);
            if (stream != null)
            {
                var contentType = ImageHelper.GetContentType(fileName);
                return new FileStreamResult(stream, contentType ?? "application/octet-stream") { FileDownloadName = fileName };
            }

            return BadRequest();
        }

        [AllowAnonymous()]
        public async Task<IActionResult> GetCalendarFile(string fileName)
        {
            var settings = await _settings.GetSetting();

            if (settings.IsSharePointIntegrationOn)
            {
                var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                var stream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Calendar, "", fileName);
                if (stream != null)
                    return new FileStreamResult(stream, "text/calendar") { FileDownloadName = fileName };
            }
            else {
                var file = await _documentStorage.GetFileStream("", fileName, ImageHelper.CPiSavedFileType.Calendar);
                if (file != null)
                {
                    return new FileStreamResult(file.Stream, file.ContentType) { FileDownloadName = fileName };
                }
            }
            return BadRequest();

        }

        public async Task<IActionResult> DownloadReportFile(string id)
        {
            var files = id.Split('~');
            var extension = id.StartsWith("__rptxlsx") ? ".xlsx" : ".pdf";

            //verify
            var docFile = await _docViewModelService.GetDocFileByIdAndFileName(Convert.ToInt32(files[1]), files[0]+ extension);
            if (docFile != null) {
                var fileName = $"{files[0]}{extension}";
                var path = _documentStorage.BuildPath(_documentStorage.ReportLogFolder, "", files[0]);
                var file = await _documentStorage.GetFileStream(path);
                if (file != null)
                {
                    file.Position = 0;
                    return new FileStreamResult(file, ImageHelper.GetContentType($"report{extension}")) { FileDownloadName = fileName };
                }
            }
            return BadRequest("File not found.");
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
