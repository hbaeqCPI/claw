using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using R10.Core;
using R10.Core.Entities;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Security;
using R10.Web.Services.DocumentStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Data;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Microsoft.Extensions.Options;
using R10.Web.Services;
using R10.Web.Services.SharePoint;
using R10.Core.Entities.Shared;
using DocuSign.eSign.Model;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessLetters)]
    public class DOCXController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IDOCXService _docxService;
        private readonly IDOCXViewModelService _docxViewModelService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        private readonly IDocumentStorage _documentStorage;
        private readonly IDocumentHelper _documentHelper;
        private readonly GraphSettings _graphSettings;
        private readonly ISharePointService _sharePointService;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly ISharePointViewModelService _sharePointViewModelService;
        private readonly IEFSService _efsService;

        private readonly string[] _imageTypes = { "bmp", "gif", "jpeg", "jpg", "png", "tiff" };

        public DOCXController(
            IAuthorizationService authService,
            IStringLocalizer<SharedResource> localizer,
            IDOCXService docxService,
            IDOCXViewModelService docxViewModelService,
            IHostingEnvironment hostingEnvironment,
            IMapper mapper,
            IDocumentStorage documentStorage,
            IDocumentHelper documentHelper,
            IOptions<GraphSettings> graphSettings,
            ISharePointService sharePointService,
            ISystemSettings<DefaultSetting> settings,
            ISharePointViewModelService sharePointViewModelService,
            IEFSService efsService
            )
        {
            _authService = authService;
            _localizer = localizer;
            _docxService = docxService;
            _docxViewModelService = docxViewModelService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
            _documentStorage = documentStorage;
            _documentHelper = documentHelper;
            _graphSettings = graphSettings.Value;
            _sharePointService = sharePointService;
            _settings = settings;
            _sharePointViewModelService = sharePointViewModelService;
            _efsService = efsService;
        }

        public async Task<IActionResult> OpenDOCXPopup(string systemType, string screenCode, string recordInfo, string recordKey, int recordId)
        {
            var canAccess = await DOCXHelper.CanAccessDOCX(systemType, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var systemScreen = await _docxService.GetSystemScreen(systemType, screenCode);
            var screenName = $"{_localizer[systemScreen.ScreenName]}: {recordInfo}";

            var viewModel = new DOCXPopupViewModel { SystemType = systemType, ScreenCode = screenCode, ScreenName = screenName, RecordKey = recordKey, RecordId = recordId };
            return PartialView("_DOCX", viewModel);
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, string systemType, string screenCode, string? docxName)
        {
            if (ModelState.IsValid)
            {
                var result = await _docxViewModelService.CreateViewModelForDOCXGrid(request, systemType, screenCode, docxName);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        //public async Task<IActionResult> GridContactRead([DataSourceRequest] DataSourceRequest request, int docxId)
        //{
        //    var contacts = await _docxService.GetDOCXContacts(docxId, User.GetEmail());
        //    var result = _mapper.Map<List<DOCXContactViewModel>>(contacts);

        //    return Json(result.ToDataSourceResult(request));
        //}  

        public bool UpdatePopupFilter(int docxId, string recordKey, string recordId)
        {
            return _docxService.UpdatePopupFilter(docxId, recordKey, recordId, User.GetEmail(), User.GetUserName());
        }

        public async Task<IActionResult> Generate(string docxFormParams)
        {
            var objParam = JsonConvert.DeserializeObject<DOCXGenParamViewModel>(docxFormParams);
            var docxInfo = await _docxService.DOCXesMain.SingleOrDefaultAsync(l => l.DOCXId == objParam.DOCXId);
            var dataSource = _docxService.GenerateDOCXData(objParam.DOCXId, true, objParam.IsLog, objParam.ScreenSource,// objParam.SelectedContacts,
                                                                User.GetEmail(), User.HasRespOfficeFilter(), User.HasEntityFilter(), objParam.RecordId);
            await TransformImage(objParam.SystemType, dataSource);

            var docxHelper = new DOCXGenerationHelper();

            var memoryStream = docxHelper.MergeDOCXes(_hostingEnvironment.ContentRootPath, objParam.SystemType, docxInfo.TemplateFile, dataSource, docxInfo.HasImage);
            //var fileName = docxHelper.GetFileName(objParam.SystemType, Path.GetExtension(docxInfo.TemplateFile), User.GetUserName());
            var fileName = $"{objParam.SystemType}-{objParam.DocDesc}-{DateTime.Now:yy-MM-dd-hhmmsstt}-{User.GetUserName()}{Path.GetExtension(docxInfo.TemplateFile)}";
            var mimeType = docxHelper.MimeType(fileName);
            var docxFile = File(memoryStream, mimeType, fileName);

            if (objParam.IsLog)
            {
                // log generated docxes to efs table

                var sessionId = _docxService.GetSessionId();
                ////var outputFile = Path.Combine(docxHelper.GetLogFolder(_hostingEnvironment.ContentRootPath, objParam.SystemType), fileName);
                ////System.IO.File.WriteAllBytes(outputFile, memoryStream.GetBuffer());

                //var systemName = QuickEmailHelper.GetSystem(objParam.SystemType);
                //var outputFile = _documentStorage.BuildPath(_documentStorage.DOCXLogFolder, systemName, fileName);

                memoryStream.Position = 0;
                var logStream = new MemoryStream();
                memoryStream.CopyTo(logStream);
                memoryStream.Position = 0;
                var buffer = logStream.GetBuffer() as byte[];                

                string screenCode;
                int parentId;
                if (objParam.ScreenSource == "gensetup")
                {
                    var sysScreens = await _docxService.GetSystemScreen(Int32.Parse(objParam.DOCXScreenCode));
                    objParam.DOCXScreenCode = sysScreens.ScreenCode;
                    screenCode = objParam.DOCXScreenCode.Split("-")[0];
                }
                else
                {
                    screenCode = _documentHelper.DataKeyToScreenCode(objParam.DataKey);
                }                

                //var header = new DocumentStorageHeader
                //{
                //    SystemType = objParam.SystemType,                       // global search consistency
                //    ScreenCode = screenCode,                                // global search consistency
                //    DocumentType = DocumentLogType.DOCXLog,
                //    ParentId = objParam.RecordId.ToString(),                // global search consistency
                //    LogId = docxLogId.ToString(),
                //    FileName = fileName
                //};
                //await _documentStorage.SaveFile(logStream.GetBuffer(), outputFile, header);

                var settings = await _settings.GetSetting();
                var itemId = "";
                if (settings.IsSharePointIntegrationOn && settings.IsSharePointLoggingOn)
                {
                    var sharePointSystemFolder = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(objParam.SystemType);
                    //var sharePointFolder = new SharePointFolderViewModel { Folder = SharePointDocLibraryFolder.Application, RecKey = objParam.SharePointRecKey };
                    var folders = new List<string> { sharePointSystemFolder };
                    //folders.AddRange(SharePointViewModelService.GetDocumentFolders(sharePointFolder.Folder, sharePointFolder.RecKey));

                    using (var stream = new MemoryStream(buffer))
                    {
                        //var graphClient = _sharePointService.GetGraphClient();
                        var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                        var result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.IPFormsLog, folders, stream, fileName);
                        itemId = result.DriveItemId;
                    }
                }
                else
                {
                    var header = new DocumentStorageHeader
                    {
                        SystemType = objParam.SystemType,                                       // global search consistency
                        ScreenCode = screenCode,     // global search consistency
                        DocumentType = DocumentLogType.EFSLog,
                        ParentId = objParam.RecordId.ToString(),
                        FileName = fileName
                    };
                    var outputFile = _documentStorage.BuildPath(_documentStorage.EFSLogFolder, objParam.SystemName, fileName);
                    await _documentStorage.SaveFile(buffer, outputFile, header);
                }
                //await _efsService.LogEFSDoc(objParam.SystemType, objParam.DOCXId, objParam.DataKey, objParam.RecordId, fileName, User.GetUserName(), -1, -1, itemId, objParam.Signatory);
                var docxLogId = _docxService.LogDOCX(sessionId, objParam.SystemType, objParam.DOCXId, fileName, User.GetUserName(), itemId, objParam.Signatory);
            }

            return docxFile;
        }

        private async Task TransformImage(string systemType, DataSet dataSource)
        {
            var dataTable = dataSource.Tables[0];
            if (dataTable.Columns.Contains("ImageFile"))
            {
                dataTable.Columns.Add(new DataColumn("ImageFile2", typeof(byte[])));
                var systemName = GetSystemName(systemType);

                foreach (DataRow dr in dataTable.Rows)
                {
                    var imageFile = dr["ImageFile"].ToString();
                    if (_imageTypes.Any(x => imageFile.ToLower().EndsWith(x)))
                    {
                        var image = await _documentStorage.ConvertImage(systemName, imageFile);
                        dr["ImageFile2"] = image;
                    }
                }
                dataTable.Columns.Remove("ImageFile");
                dataTable.Columns["ImageFile2"].ColumnName = "ImageFile";
            }
        }

        private string GetSystemName(string systemType)
        {
            return systemType == "P" ? "Patent" : systemType == "T" ? "Trademark" : systemType == "G" ? "GeneralMatter" : "";
        }

    }



}