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
using SmartFormat.Utilities;
using R10.Core.Entities.Shared;
using R10.Web.Services;
using R10.Web.Services.SharePoint;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using R10.Core.Services;
using DocumentFormat.OpenXml.Wordprocessing;
using R10.Web.Services.iManage;
using R10.Web.Services.NetDocuments;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessLetters)]
    public class LetterController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ILetterService _letterService;
        private readonly ILetterViewModelService _letterViewModelService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IMapper _mapper;
        private readonly IDocumentStorage _documentStorage;
        private readonly IDocumentHelper _documentHelper;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly ISharePointService _sharePointService;
        private readonly ISharePointViewModelService _sharePointViewModelService;
        private readonly GraphSettings _graphSettings;
        private readonly IDocumentService _docService;
        private readonly IiManageViewModelService _iManageViewModelService;
        private readonly INetDocumentsViewModelService _netDocsViewModelService;

        private readonly string[] _imageTypes = { "bmp", "gif", "jpeg", "jpg", "png", "tiff" };

        public LetterController(
            IAuthorizationService authService,
            IStringLocalizer<SharedResource> localizer,
            ILetterService letterService,
            ILetterViewModelService letterViewModelService,
            IHostingEnvironment hostingEnvironment,
            IMapper mapper,
            IDocumentStorage documentStorage,
            IDocumentHelper documentHelper,
            ISystemSettings<DefaultSetting> settings,
            ISharePointService sharePointService,
            ISharePointViewModelService sharePointViewModelService,
            IOptions<GraphSettings> graphSettings,
            IDocumentService docService,
            IiManageViewModelService iManageViewModelService,
            INetDocumentsViewModelService netDocsViewModelService
            )
        {
            _authService = authService;
            _localizer = localizer;
            _letterService = letterService;
            _letterViewModelService = letterViewModelService;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
            _documentStorage = documentStorage;
            _documentHelper = documentHelper;
            _settings = settings;
            _sharePointService = sharePointService;
            _sharePointViewModelService = sharePointViewModelService;
            _graphSettings = graphSettings.Value;
            _docService = docService;
            _iManageViewModelService = iManageViewModelService;
            _netDocsViewModelService = netDocsViewModelService;
        }

        public async Task<IActionResult> OpenLetterPopup(string systemType, string screenCode, string recordInfo, string recordKey, int recordId)
        {
            var canAccess = await LetterHelper.CanAccessLetter(systemType, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var systemScreen = await _letterService.GetSystemScreen(systemType, screenCode);
            var screenName = $"{_localizer[systemScreen.ScreenName]}: {recordInfo}";

            var viewModel = new LetterPopupViewModel { SystemType = systemType, ScreenCode = screenCode, ScreenName = screenName, RecordKey = recordKey, RecordId = recordId };
            return PartialView("_Letter", viewModel);
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, string systemType, string screenCode, string? letterName, int? letCatId, int? letSubCatId, List<string>? tags = null)
        {
            if (ModelState.IsValid)
            {
                var result = await _letterViewModelService.CreateViewModelForLetterGrid(request, systemType, screenCode, letterName, letCatId, letSubCatId, tags);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<IActionResult> GridContactRead([DataSourceRequest] DataSourceRequest request, int letId)
        {
            var contacts = await _letterService.GetLetterContacts(letId, User.GetEmail());
            var result = _mapper.Map<List<LetterContactViewModel>>(contacts);

            return Json(result.ToDataSourceResult(request));
        }

        public bool UpdatePopupFilter(int letId, string recordKey, string recordId)
        {
            return _letterService.UpdatePopupFilter(letId, recordKey, recordId, User.GetEmail(), User.GetUserName());
        }

        public async Task<IActionResult> GetTagPickListData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_letterService.LetterTags.Select(s => new { Tag = s.Tag}), request, property, text, filterType, requiredRelation, false);
        }

        public async Task<IActionResult> Generate(string letterFormParams)
        {
            var objParam = JsonConvert.DeserializeObject<LetterGenParamViewModel>(letterFormParams);
            var dataSource = _letterService.GenerateLetterData(objParam.LetId, true, objParam.IsLog, objParam.ScreenSource, objParam.SelectedContacts,
                                                                User.GetEmail(), User.HasRespOfficeFilter(), User.HasEntityFilter(), objParam.PreviewSelection);
            var letterInfo = await _letterService.LettersMain.SingleOrDefaultAsync(l => l.LetId == objParam.LetId);
            await TransformImage(objParam.SystemType, dataSource);


            var settings = await _settings.GetSetting();

            var templateStream = new MemoryStream();
            var sharePointSystemFolder = "";
            
            if (settings.IsSharePointIntegrationOn)
            {
                sharePointSystemFolder = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(objParam.SystemType);
                //var graphClient = _sharePointService.GetGraphClient();
                var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                var stream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.LetterTemplates, sharePointSystemFolder, letterInfo.TemplateFile);
                if (stream == null)
                    return BadRequest(_localizer["Template file not found"].ToString());

                stream.CopyTo(templateStream);
            }
            else
            {
                var systemFolder = GetSystemName(objParam.SystemType);
                var templateFile = Path.Combine(_documentStorage.LetterTemplateFolder, systemFolder, letterInfo.TemplateFile);
                var stream = await _documentStorage.GetFileStream(templateFile);
                if (stream == null)
                    return BadRequest(_localizer["Template file not found"].ToString());

                stream.CopyTo(templateStream);
            }
            templateStream.Position = 0;
            var letterHelper = new LetterGenerationHelper();

            if (templateStream == null)
                return BadRequest(_localizer["Template file not found"].ToString());

            var memoryStream = letterHelper.MergeLetterDataSet(templateStream, dataSource, letterInfo.HasImage, letterInfo.TemplateFile.Split('.').Last());

            var fileName = "";
            if (settings.IsSharePointIntegrationOn)
            {
                fileName = letterHelper.BuildSharePointLogFileName(letterInfo.TemplateFile);
            }
            else {
                fileName = letterHelper.GetFileName(objParam.SystemType, Path.GetExtension(letterInfo.TemplateFile), User.GetUserName());
            }
            var mimeType = letterHelper.MimeType(fileName);

            var letterFile = File(memoryStream, mimeType, fileName);

            if (objParam.IsLog)
            {
                // log generated letters
                var sessionId = _letterService.GetSessionId();

                memoryStream.Position = 0;
                var logStream = new MemoryStream();
                memoryStream.CopyTo(logStream);
                memoryStream.Position = 0;

                //var recsToProcess = (await _letterService.GetDataKeyValuesToLog(sessionId)).Select(d => d.Value).ToList();
                var letLogId = _letterService.LogLetter(sessionId, objParam.SystemType, objParam.LetId, fileName, User.GetUserName());
                string screenCode;
                if (objParam.ScreenSource == "gensetup")
                {
                    var sysScreens = await _letterService.GetSystemScreen(Int32.Parse(objParam.LetterScreenCode));
                    objParam.LetterScreenCode = sysScreens.ScreenCode;
                }
                screenCode = objParam.LetterScreenCode.Split("-")[0];

                if (settings.IsSharePointIntegrationOn && settings.IsSharePointLoggingOn)
                {
                    logStream.Position = 0;
                    var folders = new List<string> { sharePointSystemFolder };

                    //var graphClient = _sharePointService.GetGraphClient();
                    var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                    var result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.LetterLog, folders, logStream, fileName);
                    await _letterService.LogItemId(letLogId, result.DriveItemId);

                    //foreach (var rec in recsToProcess) {
                    //} 
                }
                else
                {
                    var systemName = QuickEmailHelper.GetSystem(objParam.SystemType);
                    var outputFile = _documentStorage.BuildPath(_documentStorage.LetterLogFolder, systemName, fileName);

                    var header = new DocumentStorageHeader
                    {
                        SystemType = objParam.SystemType,                       // global search consistency
                        ScreenCode = screenCode,                                // global search consistency
                        DocumentType = DocumentLogType.LetterLog,
                        ParentId = objParam.RecordId.ToString(),                // global search consistency
                        LogId = letLogId.ToString(),
                        FileName = fileName
                    };
                    await _documentStorage.SaveFile(logStream.GetBuffer(), outputFile, header);
                }

            }

            return letterFile;


        }



        private async Task TransformImage(string systemType, DataSet dataSource)
        {
            var settings = await _settings.GetSetting();

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
                        byte[]? image=null;
                        switch (settings.DocumentStorage)
                        {
                            case DocumentStorageOptions.SharePoint:
                                var docFile = await _docService.GetFileByFileName(imageFile);
                                if (docFile != null) {
                                    var sharePointDocLibrary = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(systemType);
                                    var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                                    var stream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, sharePointDocLibrary, docFile.DriveItemId);
                                    if (stream != null) {
                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            stream.CopyTo(ms);
                                            image = ms.ToArray();
                                        }
                                    }
                                }
                                break;

                            case DocumentStorageOptions.iManage:
                                image = await _iManageViewModelService.GetDocumentAsByteArrayByFileName(imageFile);
                                break;

                            case DocumentStorageOptions.NetDocuments:
                                image = await _netDocsViewModelService.GetDocumentAsByteArrayByFileName(imageFile);
                                break;

                            default:
                                image = await _documentStorage.ConvertImage(systemName, imageFile);
                                break;
                        }
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