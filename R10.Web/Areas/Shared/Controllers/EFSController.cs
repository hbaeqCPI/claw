using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions.ActionResults;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Security;
using R10.Web.Services;
using R10.Web.Services.DocumentStorage;
using R10.Web.Services.SharePoint;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Web.Models;
using Sustainsys.Saml2.Metadata;
using R10.Core.DTOs;
using R10.Core.Services.Shared;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared")]
    public class EFSController : BaseController
    {
        private readonly IEFSService _service;
        private readonly IEFSHelper _efsHelper;
        private readonly IDocumentStorage _documentStorage;
        private readonly IDocumentHelper _documentHelper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly ISharePointViewModelService _sharePointViewModelService;
        private readonly ISharePointService _sharePointService;
        private readonly GraphSettings _graphSettings;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IAuthorizationService _authService;
        private readonly IQuickEmailSetupService _quickEmailSetupService;

        public EFSController(IEFSService service, IHostingEnvironment hostingEnvironment, 
                             IEFSHelper efsHelper, IDocumentStorage documentStorage,
                             IDocumentHelper documentHelper,
                             ISystemSettings<DefaultSetting> settings, ISharePointViewModelService sharePointViewModelService,
                             ISharePointService sharePointService, IOptions<GraphSettings> graphSettings, IStringLocalizer<SharedResource> localizer, 
                             IAuthorizationService authService, IQuickEmailSetupService quickEmailSetupService)
        {
            _service = service;
            _hostingEnvironment = hostingEnvironment;
            _efsHelper = efsHelper;
            _documentStorage = documentStorage;
            _documentHelper = documentHelper;
            _settings = settings;
            _sharePointViewModelService = sharePointViewModelService;
            _sharePointService = sharePointService;
            _graphSettings = graphSettings.Value;
            _localizer = localizer;
            _authService = authService;
            _quickEmailSetupService = quickEmailSetupService;
        }

        public IActionResult IDSPrintForms(int appId, string country)
        {
            string docType = "|PatIDS|";
            return RedirectToAction("GenerateUSForms",new {appId, country,docType});
        }

        public async Task<IActionResult> GenerateUSForms(int appId, string country, string docType,string? sharePointRecKey)
        {
            if (string.IsNullOrEmpty(docType))
            {
                docType = "|PatEFS|" + (User.IsInSystem(SystemType.IDS) ? "|PatIDS|" : "");
            }

            var efsViewModel = new EFSGenerationViewModel
            {
                DocType = docType,
                Country = country,
                DataKey = "AppId",
                SystemType = SystemType.Patent,
                RecId = appId,
                SharePointRecKey= sharePointRecKey
            };
            efsViewModel.Forms = await _service.GetForms(efsViewModel.SystemType, efsViewModel.DocType,
                efsViewModel.Country, efsViewModel.RecId);
            efsViewModel.Signatories = await _service.GetSignatories(efsViewModel.SystemType, appId);

            return PartialView("_EFSGeneration", efsViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GenEFSDoc(string formParams)
        {
            var objParam = JsonConvert.DeserializeObject<EFSGenerationParamViewModel>(formParams);
            var userId = User.GetUserIdentifier();
            
            var dataSource = await _service.GetPrintData(objParam.DocType, objParam.SubType, objParam.Signatory, objParam.RecId, objParam.PageNo, objParam.PageCount, userId);
            var isManualMerge = string.IsNullOrEmpty(objParam.SourceTables);

            objParam.MapFile = Path.Combine(_hostingEnvironment.ContentRootPath, EFSHelper.MapFolder, objParam.MapFile);

            if (!isManualMerge)
            {
                //table names must match the XSL mapping file table names
                objParam.SourceTables?.Split(',').Each((table, index) => dataSource.Tables[index].TableName = table);
            }

            _efsHelper.DataSource = dataSource;
            _efsHelper.SourceDocumentPath = objParam.DocPath;
            _efsHelper.MapFilePath = objParam.MapFile;

            var compressed = false;
            var buffer = _efsHelper.FillPdfWithData(isManualMerge, objParam.DocType, objParam.SubType, ref compressed);
            var fileName = $"{objParam.DocType}-{objParam.SubType}-{DateTime.Now:yy-MM-dd-hhmmsstt}.{(compressed ? "zip" : "pdf")}";
            
            if (!objParam.Preview)
            {
                var settings = await _settings.GetSetting();
                var itemId = "";
                if (settings.IsSharePointIntegrationOn && settings.IsSharePointLoggingOn)
                {
                    var sharePointSystemFolder = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(objParam.SystemType);
                    //var sharePointFolder = new SharePointFolderViewModel { Folder = SharePointDocLibraryFolder.Application, RecKey = objParam.SharePointRecKey };
                    var folders = new List<string> { sharePointSystemFolder };
                    //folders.AddRange(SharePointViewModelService.GetDocumentFolders(sharePointFolder.Folder, sharePointFolder.RecKey));

                    using (var stream = new MemoryStream(buffer)) {
                        //var graphClient = _sharePointService.GetGraphClient();
                        var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                        var result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.IPFormsLog, folders, stream, fileName);
                        itemId = result.DriveItemId;
                    }
                }
                else {
                    var header = new DocumentStorageHeader
                    {
                        SystemType = objParam.SystemType,                                       // global search consistency
                        ScreenCode = _documentHelper.DataKeyToScreenCode(objParam.DataKey),     // global search consistency
                        DocumentType = DocumentLogType.EFSLog,
                        ParentId = objParam.RecId.ToString(),
                        FileName = fileName
                    };
                    var outputFile = _documentStorage.BuildPath(_documentStorage.EFSLogFolder, objParam.SystemName, fileName);
                    await _documentStorage.SaveFile(buffer, outputFile, header);
                }
                await _service.LogEFSDoc(objParam.SystemType, objParam.EfsDocId, objParam.DataKey, objParam.RecId, fileName, User.GetUserName(), objParam.PageNo, objParam.PageCount, itemId,objParam.Signatory);
            }

            Response.Headers.Add("content-disposition", $"attachment;filename={fileName}");
            return new FileContentResult(buffer, new MediaTypeHeaderValue("application/pdf"));


        }

        #region eSignature
        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessESignatureAuxiliary)]
        public async Task<IActionResult> eSignatureSetup()
        {
            ViewData["CanModify"] = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded;
            return View("eSignatureSetup");
        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessESignatureAuxiliary)]
        public async Task<IActionResult> eSignatureRead([DataSourceRequest] DataSourceRequest request)
        {
            var result = await _service.QueryableList.Where(e => e.SystemType=="Patent" && e.ForSignature).OrderBy(e=> e.Country).ThenBy(e=> e.GroupDesc).ThenBy(e=> e.DisplayOrder).ProjectTo<EFSViewModel>().ToListAsync();
            return Json(result.ToDataSourceResult(request));
        }


        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> eSignatureUpdate(
            [Bind(Prefix = "updated")] IList<EFS> updated,
            [Bind(Prefix = "new")] IList<EFS> added,
            [Bind(Prefix = "deleted")] IList<EFS> deleted)
        {
            if (updated.Any())
            {
                updated.Each(u=> u.SignatureQESetupId= u.SignatureQESetupId2);

                await _service.UpdateEFS(updated, User.GetUserName());
                var success = updated.Count() == 1 ?
                    _localizer["IP Form has been saved successfully."].ToString() :
                    _localizer["IP Forms have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = SharedAuthorizationPolicy.CanAccessESignatureAuxiliary)]
        public async Task<IActionResult> eSignatureQESetupList([DataSourceRequest] DataSourceRequest request)
        {
            var screen = await _quickEmailSetupService.GetSystemScreensBySystemTypeAsync(SystemTypeCode.Patent).Where(s => s.ScreenCode == "CA-eSignature-QE").FirstOrDefaultAsync();

            var result = await _quickEmailSetupService.GetQEMainBySystemType(SystemTypeCode.Patent)
                            .Where(e => e.ScreenId == screen.ScreenId)
                            .Select(e => new LookupDTO
                            {
                                Text = e.TemplateName,
                                Value = e.QESetupID.ToString()
                            })
                            .OrderBy(e => e.Text)
                            .ToListAsync();
            return Json(result);
        }


        #endregion



    }
}