using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Sustainsys.Saml2.Metadata;
using Newtonsoft.Json;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Extensions.Options;
using System.Net;
using R10.Web.Services.SharePoint;
using Microsoft.Graph;
using R10.Web.Services;
using R10.Web.Areas.Shared.ViewModels;
using R10.Core.Interfaces.Patent;
using R10.Core.Interfaces;
using R10.Web.Security;
using ActiveQueryBuilder.View.DatabaseSchemaView;
using R10.Core.Entities.Documents;
using static iText.Svg.SvgConstants;
using R10.Web.Areas.Shared.ViewModels.SharePoint;
using System.IO;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Bibliography;
using R10.Core.DTOs;
using R10.Core.Helpers;
using R10.Web.Services.DocumentStorage;
using R10.Web.Helpers;
using System.Diagnostics;
using R10.Core.Entities.Shared;
using ActiveQueryBuilder.Web.Server.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Kendo.Mvc.Extensions;
using DocumentFormat.OpenXml.Presentation;
using DocuSign.eSign.Model;
using R10.Web.Interfaces;
using Microsoft.AspNetCore.Server.IISIntegration;
using SmartFormat.Core.Extensions;

namespace R10.Web.Areas.Shared.Controllers.SharePoint
{


    [Area("Shared"), Authorize(AuthenticationSchemes = AuthSchemes, Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class SharePointUtilityController : Controller
    {
        private readonly ICountryApplicationService _applicationService;
        private readonly ITmkTrademarkService _trademarkService;
        private readonly IApplicationDbContext _repository;
        private readonly GraphSettings _graphSettings;
        private readonly ISharePointService _sharePointService;
        private readonly ISharePointRepository _sharePointRepository;
        private readonly AzureStorage _azureStorage;
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        private readonly ISharePointViewModelService _sharePointViewModelService;

        private const string AuthSchemes = "Identity.Application" + "," + OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        private List<SharePointSyncDTO> _sharePointSyncItems;

        public SharePointUtilityController(
            ISharePointService sharePointService,
            IOptions<GraphSettings> graphSettings,
            ICountryApplicationService applicationService,
            ITmkTrademarkService trademarkService,
            IApplicationDbContext repository,
            ISharePointRepository sharePointRepository,
            AzureStorage azureStorage,
            ISystemSettings<DefaultSetting> defaultSettings,
            ISharePointViewModelService sharePointViewModelService
            )
        {
            _sharePointService = sharePointService;
            _graphSettings = graphSettings.Value;
            _applicationService = applicationService;
            _trademarkService = trademarkService;
            _repository = repository;
            _sharePointRepository = sharePointRepository;
            _azureStorage = azureStorage;
            _defaultSettings = defaultSettings;
            _sharePointViewModelService = sharePointViewModelService;
        }

        //[HttpPost]
        public async Task<IActionResult> SaveIFW()
        {
            var systems = User.GetSystems();
            if (!systems.Any(s=> s=="Patent")) {
                return Ok();
            }

            if (Request.HasFormContentType)
            {
                var form = Request.Form;
                if (form.Files.Count > 0)
                {
                    form.TryGetValue("data", out var data);
                    var file = JsonConvert.DeserializeObject<SharePointStorageViewModel>(data);
                    var formFile = form.Files[0];
                    if (formFile != null)
                    {
                        var settings = await _defaultSettings.GetSetting();

                        var ctryApp = await _applicationService.GetById(file.ParentId);
                        if (ctryApp != null)
                        {
                            var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                            var recKey = SharePointViewModelService.BuildRecKey(ctryApp.CaseNumber, ctryApp.Country, ctryApp.SubCase);
                            var folders = SharePointViewModelService.GetDocumentFolders(SharePointDocLibraryFolder.Application, recKey);

                            using (var stream = new MemoryStream())
                            {
                                formFile.CopyTo(stream);
                                stream.Position = 0;

                                var result = new SharePointGraphDriveItemKeyViewModel();
                                if (settings.IsSharePointIntegrationByMetadataOn)
                                {
                                    result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Patent, new List<string>(), stream, file.DocName);
                                }
                                else
                                {
                                    result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Patent, folders, stream, file.DocName);
                                }

                                var driveItem = await graphClient.Drives[result.DriveId].Items[result.DriveItemId].Request().Expand("listItem").GetAsync();

                                var additionalData = new Dictionary<string, object>();
                                if (settings.IsSharePointIntegrationByMetadataOn)
                                {
                                    additionalData.Add("CPIScreen", SharePointDocLibraryFolder.Application);
                                    additionalData.Add("CPIRecordKey", recKey.Replace(SharePointSeparator.Folder, SharePointSeparator.Field));
                                };
                                if (SharePointViewModelService.IsSharePointIntegrationHasSyncField)
                                {
                                    additionalData.Add("CPISyncCompleted", true);
                                }

                                var requestBody = new FieldValueSet
                                {
                                    AdditionalData = additionalData
                                };

                                //not necessary because the records were already added by the downloader
                                //var sync = new SharePointSyncToDocViewModel
                                //{
                                //    DocLibrary = SharePointDocLibrary.Patent,
                                //    DocLibraryFolder = SharePointDocLibraryFolder.Application,
                                //    DriveItemId = result.DriveItemId,
                                //    ParentId = file.ParentId,
                                //    FileName = file.DocName,
                                //    CreatedBy = User.GetUserName().Left(20),
                                //    Remarks = "",
                                //    Tags = "",
                                //    IsImage = driveItem.Image != null,
                                //    IsPrivate = false,
                                //    IsDefault = false,
                                //    IsPrintOnReport = false,
                                //    IsVerified = false,
                                //    IncludeInWorkflow = false,
                                //    IsActRequired = false,
                                //    CheckAct = false,
                                //    SendToClient = false,
                                //    Source = DocumentSourceType.Manual,
                                //    ProcessAI = false
                                //};
                                //await _sharePointViewModelService.SyncToDocumentTables(sync);

                                if (additionalData.Count > 0) {
                                    var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;
                                    var list = site.Lists.Where(l => l.Name == SharePointDocLibrary.Patent).FirstOrDefault();
                                    if (list != null)
                                    {
                                       await graphClient.Sites[site.Id].Lists[list.Id].Items[driveItem.ListItem.Id].Fields.Request().UpdateAsync(requestBody);
                                    }
                                }
                                await _repository.RTSSearchUSIFWs.Where(ifw => ifw.PLAppID == file.Id && ifw.FileName == file.FileName).ExecuteUpdateAsync(p => p.SetProperty(ifw => ifw.DocName, s => file.DocName)
                                                                     .SetProperty(ifw => ifw.Transferred, s => true));
                                await _repository.DocFiles.Where(f => f.DocFileName == file.FileName && string.IsNullOrEmpty(f.DriveItemId))
                                             .ExecuteUpdateAsync(p => p.SetProperty(f => f.DriveItemId, s => result.DriveItemId));
                            }
                        }
                    }
                }
            }
            return Ok();
        }

        //[HttpPost]
        public async Task<IActionResult> SaveTLDoc()
        {
            var systems = User.GetSystems();
            if (!systems.Any(s => s == "Trademark"))
            {
                return Ok();
            }

            if (Request.HasFormContentType)
            {
                var form = Request.Form;
                if (form.Files.Count > 0)
                {
                    form.TryGetValue("data", out var data);
                    var file = JsonConvert.DeserializeObject<SharePointStorageViewModel>(data);
                    var formFile = form.Files[0];
                    if (formFile != null)
                    {

                        var tmk = await _trademarkService.GetByIdAsync(file.ParentId);
                        if (tmk != null)
                        {
                            var graphClient = _sharePointService.GetGraphClientByClientCredentials();

                            var recKey = SharePointViewModelService.BuildRecKey(tmk.CaseNumber, tmk.Country, tmk.SubCase);
                            var folders = SharePointViewModelService.GetDocumentFolders(SharePointDocLibraryFolder.TmkLinks, recKey);

                            using (var stream = new MemoryStream())
                            {
                                formFile.CopyTo(stream);
                                stream.Position = 0;
                                var result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Trademark, folders, stream, file.DocName);
                                await _repository.TLSearchDocuments.Where(d => d.TLTmkId == file.Id && d.FileName == file.FileName).ExecuteUpdateAsync(p => p.SetProperty(ifw => ifw.DocName, s => file.DocName)
                                                                     .SetProperty(ifw => ifw.Transferred, s => true));
                                await _repository.DocFiles.Where(f => f.DocFileName == file.FileName && string.IsNullOrEmpty(f.DriveItemId))
                                             .ExecuteUpdateAsync(p => p.SetProperty(f => f.DriveItemId, s => result.DriveItemId));
                            }
                        }
                    }
                }
            }
            return Ok();
        }


        //[HttpPost]
        public async Task<IActionResult> SaveTLImage()
        {
            var systems = User.GetSystems();
            if (!systems.Any(s => s == "Trademark"))
            {
                return Ok();
            }

            if (Request.HasFormContentType)
            {
                var form = Request.Form;
                if (form.Files.Count > 0)
                {
                    form.TryGetValue("data", out var data);
                    var file = JsonConvert.DeserializeObject<SharePointStorageViewModel>(data);
                    var formFile = form.Files[0];
                    if (formFile != null)
                    {
                        var tmk = await _trademarkService.GetByIdAsync(file.ParentId);
                        if (tmk != null)
                        {
                            var graphClient = _sharePointService.GetGraphClientByClientCredentials();
                            var recKey = SharePointViewModelService.BuildRecKey(tmk.CaseNumber, tmk.Country, tmk.SubCase);
                            var folders = SharePointViewModelService.GetDocumentFolders(SharePointDocLibraryFolder.Trademark, recKey);
                            var settings = await _defaultSettings.GetSetting();

                            using (var stream = new MemoryStream())
                            {
                                formFile.CopyTo(stream);
                                stream.Position = 0;

                                var uploadedFile = new SharePointGraphDriveItemKeyViewModel();
                                var defaultImage = new DefaultImageViewModel();

                                if (settings.IsSharePointIntegrationByMetadataOn)
                                {
                                    uploadedFile = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Trademark, new List<string>(), stream, file.DocName);
                                    defaultImage = await graphClient.GetDefaultImageByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Trademark, SharePointDocLibraryFolder.Trademark, recKey.Replace(SharePointSeparator.Folder, SharePointSeparator.Field));

                                }
                                else {
                                    uploadedFile = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Trademark, folders, stream, file.DocName);
                                    defaultImage = await graphClient.GetDefaultImage(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Trademark, folders);
                                }
                                var driveItem = await graphClient.GetSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Trademark, uploadedFile.DriveItemId);

                                var additionalData = new Dictionary<string, object>();
                                if (settings.IsSharePointIntegrationByMetadataOn)
                                {
                                    additionalData.Add("CPIScreen", SharePointDocLibraryFolder.Trademark);
                                    additionalData.Add("CPIRecordKey", recKey.Replace(SharePointSeparator.Folder, SharePointSeparator.Field));
                                };
                                if (SharePointViewModelService.IsSharePointIntegrationHasSyncField)
                                {
                                    additionalData.Add("CPISyncCompleted", true);
                                }

                                var sync = new SharePointSyncToDocViewModel
                                {
                                    DocLibrary = SharePointDocLibrary.Trademark,
                                    DocLibraryFolder = SharePointDocLibraryFolder.Trademark,
                                    ParentId = file.ParentId,
                                    FileName = file.DocName,
                                    CreatedBy = User.GetUserName().Left(20),
                                    Remarks = "",
                                    Tags = "",
                                    IsImage = true,
                                    IsPrivate = false,
                                    IsPrintOnReport = false,
                                    IsVerified = false,
                                    IncludeInWorkflow = false,
                                    IsActRequired = false,
                                    CheckAct = false,
                                    SendToClient = false,
                                    Source = DocumentSourceType.Manual,
                                    ProcessAI = false,
                                    DriveItemId= uploadedFile.DriveItemId
                                };

                                if (defaultImage == null)
                                {
                                    sync.IsDefault = true;
                                    if (!settings.IsSharePointIntegrationKeyFieldsOnly)
                                    {
                                        additionalData.Add("IsDefault", true);
                                    }
                                }
                                var requestBody = new FieldValueSet
                                {
                                    AdditionalData = additionalData
                                };

                                if (requestBody.AdditionalData.Count > 0) {
                                    var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;
                                    var list = site.Lists.Where(l => l.Name == SharePointDocLibrary.Trademark).FirstOrDefault();
                                    await graphClient.Sites[site.Id].Lists[list.Id].Items[driveItem.ListItem.Id].Fields.Request().UpdateAsync(requestBody);
                                }

                                await _repository.TLSearchImages.Where(d => d.TLTmkId == file.Id).ExecuteUpdateAsync(p => p.SetProperty(ifw => ifw.Transferred, s => true));
                                await _repository.DocFiles.Where(f => f.DocFileName == file.FileName && string.IsNullOrEmpty(f.DriveItemId))
                                .ExecuteUpdateAsync(p => p.SetProperty(f => f.DriveItemId, s => uploadedFile.DriveItemId));

                                await _sharePointViewModelService.SyncToDocumentTables(sync);
                            }
                        }
                    }
                }
            }
            return Ok();
        }


        //public async Task<IActionResult> SyncToAzureBlobPrepare()
        //{
        //    var settings = await _defaultSettings.GetSetting();
        //    if (!settings.IsCognitiveSearchOn)
        //    {
        //        return Ok();
        //    }

        //    var systems = User.GetSystems();
        //    var docLibraryFolders = new List<LookupDTO>();

        //    if (systems.Any(s => s == "Patent"))
        //    {
        //        if (SharePointViewModelService.IsSharePointIntegrationMainScreenOnly) {
        //            docLibraryFolders.Add(new LookupDTO { Value = SharePointDocLibrary.Patent, Text = SharePointDocLibraryFolder.Application });
        //        }
        //        else {
        //            docLibraryFolders.AddRange(
        //            new List<LookupDTO> {
        //            new LookupDTO { Value = SharePointDocLibrary.Patent, Text = SharePointDocLibraryFolder.Invention },
        //            new LookupDTO { Value = SharePointDocLibrary.Patent, Text = SharePointDocLibraryFolder.Application },
        //            new LookupDTO { Value = SharePointDocLibrary.Patent, Text = SharePointDocLibraryFolder.Action },
        //            new LookupDTO { Value = SharePointDocLibrary.Patent, Text = SharePointDocLibraryFolder.Cost },
        //            new LookupDTO { Value = SharePointDocLibrary.Patent, Text = SharePointDocLibraryFolder.InventionCostTracking },
        //            new LookupDTO { Value = SharePointDocLibrary.Patent, Text = SharePointDocLibraryFolder.InventionAction }
        //            });
        //        }
        //    }

        //    if (systems.Any(s => s == "Trademark"))
        //    {
        //        if (SharePointViewModelService.IsSharePointIntegrationMainScreenOnly)
        //        {
        //            docLibraryFolders.Add(new LookupDTO { Value = SharePointDocLibrary.Trademark, Text = SharePointDocLibraryFolder.Trademark });
        //        }
        //        else
        //        {
        //            docLibraryFolders.AddRange(
        //            new List<LookupDTO> {
        //                new LookupDTO{Value=SharePointDocLibrary.Trademark,Text=SharePointDocLibraryFolder.Trademark },
        //                new LookupDTO{Value=SharePointDocLibrary.Trademark,Text=SharePointDocLibraryFolder.Action },
        //                new LookupDTO{Value=SharePointDocLibrary.Trademark,Text=SharePointDocLibraryFolder.Cost },
        //            });
        //        }
        //    }

        //    if (systems.Any(s => s == "GeneralMatter"))
        //    {
        //        if (SharePointViewModelService.IsSharePointIntegrationMainScreenOnly)
        //        {
        //            docLibraryFolders.Add(new LookupDTO { Value = SharePointDocLibrary.GeneralMatter, Text = SharePointDocLibraryFolder.GeneralMatter });
        //        }
        //        else
        //        {
        //            docLibraryFolders.AddRange(
        //            new List<LookupDTO> {
        //            new LookupDTO{Value=SharePointDocLibrary.GeneralMatter,Text=SharePointDocLibraryFolder.GeneralMatter },
        //            new LookupDTO{Value=SharePointDocLibrary.GeneralMatter,Text=SharePointDocLibraryFolder.Action },
        //            new LookupDTO{Value=SharePointDocLibrary.GeneralMatter,Text=SharePointDocLibraryFolder.Cost },
        //            });
        //        }
        //    }

        //    if (systems.Any(s => s == "DMS"))
        //    {
        //        docLibraryFolders.Add(new LookupDTO { Value = SharePointDocLibrary.DMS, Text = SharePointDocLibraryFolder.DMS });
        //    }

        //    if (systems.Any(s => s == "PatClearance"))
        //    {
        //        docLibraryFolders.Add(new LookupDTO { Value = SharePointDocLibrary.DMS, Text = SharePointDocLibraryFolder.PatClearance });
        //    }

        //    if (systems.Any(s => s == "SearchRequest"))
        //    {
        //        docLibraryFolders.Add(new LookupDTO { Value = SharePointDocLibrary.DMS, Text = SharePointDocLibraryFolder.TmkRequest });
        //    }

        //    var graphClient = _sharePointService.GetGraphClientByClientCredentials();
        //    _sharePointSyncItems = new List<SharePointSyncDTO>();

        //    foreach (var docFolder in docLibraryFolders)
        //    {
        //        var docLibrary = docFolder.Value;
        //        var docLibraryFolder = docFolder.Text;

        //        _sharePointSyncItems.Clear();
        //        var drive = (await graphClient.GetSiteByPath(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName)).Drives.FirstOrDefault(d => d.Name == docLibrary);
        //        if (drive != null)
        //        {
        //            //Search function below has a bug, it excludes image files(png,jpeg, etc..) inside a sub folder, it's okay to use here
        //            //because we don't include image files in cognitive search index
        //            IDriveItemSearchCollectionPage pages;
        //            if (SharePointViewModelService.IsSharePointIntegrationMainScreenOnly)
        //                pages = await graphClient.Drives[drive.Id].Root.Search("").Request().GetAsync();
        //            else
        //                pages = await graphClient.Drives[drive.Id].Root.ItemWithPath(docLibraryFolder).Search("").Request().GetAsync();

        //            var driveItems = new List<DriveItem>();
        //            driveItems.AddRange(pages.CurrentPage);

        //            while (pages.NextPageRequest != null)
        //            {
        //                pages = await pages.NextPageRequest.GetAsync();
        //                driveItems.AddRange(pages.CurrentPage);
        //            }

        //            foreach (var item in driveItems)
        //            {
        //                var parentId = String.Empty;
        //                var parentFolder = String.Empty;
        //                var type = "File";
        //                int level = -1;

        //                if (item.ParentReference != null)
        //                {
        //                    parentId = item.ParentReference.Id;
        //                    var parent = driveItems.FirstOrDefault(p => p.Id == item.ParentReference.Id);
        //                    if (parent != null)
        //                    {
        //                        parentFolder = parent.Name;
        //                    }
        //                    else level = 0;
        //                }
        //                if (item.Folder != null)
        //                {
        //                    type = "Folder";
        //                }

        //                _sharePointSyncItems.Add(new SharePointSyncDTO
        //                {
        //                    Id = item.Id,
        //                    Name = item.Name,
        //                    ParentId = parentId,
        //                    ParentFolder = parentFolder,
        //                    Type = type,
        //                    Level = level,
        //                    DocLibrary = docLibrary,
        //                    DocLibraryFolder = docLibraryFolder,
        //                    ModifiedDate = item.LastModifiedDateTime != null ? item.LastModifiedDateTime.Value.DateTime : null
        //                });
        //            }


        //            if (SharePointViewModelService.IsSharePointIntegrationMainScreenOnly)
        //            {
        //                var topFolders = _sharePointSyncItems.Where(l => l.Level == 0).ToList();
        //                foreach (var item in topFolders) { 
        //                }
        //                var docLibraryFolderForSync = SharePointViewModelService.GetSyncDocLibraryFolder(docLibrary);
        //                foreach (var topFolder in topFolders)
        //                {
        //                    topFolder.DocLibraryFolder = docLibraryFolderForSync;
        //                    GetChildrenDriveItems(topFolder, topFolder.Name);
        //                }
        //            }
        //            else {

        //                var topFolder = _sharePointSyncItems.Where(l => l.Level == 0 && l.Name != "Trademark Links").First();
        //                if (topFolder != null)
        //                {
        //                    var keys = new List<SharePointSyncDTO>();
        //                    if (docLibraryFolder == SharePointDocLibraryFolder.Invention || docLibraryFolder == SharePointDocLibraryFolder.DMS ||
        //                        docLibraryFolder == SharePointDocLibraryFolder.GeneralMatter ||
        //                        docLibraryFolder == SharePointDocLibraryFolder.PatClearance || docLibraryFolder == SharePointDocLibraryFolder.TmkRequest)
        //                    {
        //                        keys = _sharePointSyncItems.Where(l => topFolder.Id == l.ParentId).ToList();
        //                        foreach (var item in keys)
        //                        {
        //                            item.Key = item.Name;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        var caseNumbers = _sharePointSyncItems.Where(l => topFolder.Id == l.ParentId).ToList();
        //                        keys = _sharePointSyncItems.Where(l => caseNumbers.Any(c => c.Id == l.ParentId)).ToList();

        //                        foreach (var item in caseNumbers)
        //                        {
        //                            var children = keys.Where(c => c.ParentId == item.Id).ToList();
        //                            foreach (var child in children)
        //                            {
        //                                child.Key = item.Name + SharePointSeparator.Folder + child.Name;
        //                            }
        //                        }
        //                    }
        //                    foreach (var item in keys)
        //                    {
        //                        GetChildrenDriveItems(item, item.Key);
        //                    }
        //                    var list = _sharePointSyncItems.Where(z => z.Type == "File" && z.Key != null).ToList();
        //                    if (list.Any())
        //                    {
        //                        var lastModifiedDateTime = (list.OrderByDescending(i => i.ModifiedDate).FirstOrDefault()).ModifiedDate;
        //                        await _sharePointRepository.SyncToDocumentTablesSave(User.GetUserName(), lastModifiedDateTime, list);
        //                    }
        //                }
        //            }
        //        }

        //    }

        //    return Ok();
        //}

        /* if client has cognitive search*/
        public async Task<IActionResult> SyncToAzureBlobProcess()
        {
            var settings = await _defaultSettings.GetSetting();
            if (!settings.IsCognitiveSearchOn)
            {
                return Ok();
            }

            var systems = User.GetSystems();
            var docLibraries = new List<string>();

            if (systems.Any(s => s == "Patent")) docLibraries.Add(SharePointDocLibrary.Patent);
            if (systems.Any(s => s == "Trademark")) docLibraries.Add(SharePointDocLibrary.Trademark);
            if (systems.Any(s => s == "GeneralMatter")) docLibraries.Add(SharePointDocLibrary.GeneralMatter);
            if (systems.Any(s => s == "DMS")) docLibraries.Add(SharePointDocLibrary.DMS);
            if (systems.Any(s => s == "PatClearance")) docLibraries.Add(SharePointDocLibrary.PatClearance);
            if (systems.Any(s => s == "SearchRequest")) docLibraries.Add(SharePointDocLibrary.TmkRequest);

            var graphClient = _sharePointService.GetGraphClientByClientCredentials();

            foreach (var docLibrary in docLibraries)
            {
                var list = await _sharePointRepository.GetSharePointToAzureBlobList(docLibrary);
                var forInsert = list.Where(d => d.UpdateType == "I").ToList();
                var forDelete = list.Where(d => d.UpdateType == "D").ToList();

                if (forInsert.Any())
                {
                    var storageFiles = new List<DocumentStorageFile>();
                    foreach (var doc in forInsert)
                    {
                        var fileName = doc.LogId.ToString() + Path.GetExtension(doc.FileName);
                        var azureFile = _azureStorage.BuildPath(_azureStorage.DocumentRootFolder, string.Empty, fileName);

                        var systemType = "";
                        switch (doc.DocLibrary)
                        {
                            case SharePointDocLibrary.Patent:
                                systemType = SystemTypeCode.Patent;
                                break;
                            case SharePointDocLibrary.Trademark:
                                systemType = SystemTypeCode.Trademark;
                                break;
                            case SharePointDocLibrary.GeneralMatter:
                                systemType = SystemTypeCode.GeneralMatter;
                                break;
                            case SharePointDocLibrary.DMS:
                                systemType = SystemTypeCode.DMS;
                                break;
                            case SharePointDocLibrary.PatClearance:
                                systemType = SystemTypeCode.PatClearance;
                                break;
                            case SharePointDocLibrary.TmkRequest:
                                systemType = SystemTypeCode.Clearance;
                                break;
                        }

                        var screenCode = "";
                        switch (doc.DocLibraryFolder)
                        {
                            case SharePointDocLibraryFolder.Invention:
                                screenCode = ScreenCode.Invention;
                                break;

                            case SharePointDocLibraryFolder.Application:
                                screenCode = ScreenCode.Application;
                                break;

                            case SharePointDocLibraryFolder.Trademark:
                                screenCode = ScreenCode.Trademark;
                                break;

                            case SharePointDocLibraryFolder.GeneralMatter:
                                screenCode = ScreenCode.GeneralMatter;
                                break;

                            case SharePointDocLibraryFolder.Action:
                                screenCode = ScreenCode.Action;
                                break;

                            case SharePointDocLibraryFolder.Cost:
                                screenCode = ScreenCode.CostTracking;
                                break;
                            case SharePointDocLibraryFolder.DMS:
                                screenCode = ScreenCode.DMS;
                                break;
                            case SharePointDocLibraryFolder.PatClearance:
                                screenCode = ScreenCode.PatClearance;
                                break;
                            case SharePointDocLibraryFolder.TmkRequest:
                                screenCode = ScreenCode.Clearance;
                                break;
                        }


                        if (!string.IsNullOrEmpty(systemType))
                        {
                            var header = new DocumentStorageHeader
                            {
                                SystemType = systemType,
                                ScreenCode = screenCode,
                                DocumentType = DocumentLogType.DocMgt,
                                ParentId = doc.RecordId.ToString(),
                                FileName = fileName
                            };

                            byte[] buffer = null;

                            var stream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, doc.DocLibrary, doc.DriveItemId);
                            if (stream != null)
                            {
                                using (var memoryStream = new MemoryStream())
                                {
                                    stream.CopyTo(memoryStream);
                                    buffer = memoryStream.ToArray();
                                }
                            }

                            var storageFile = new DocumentStorageFile
                            {
                                Buffer = buffer,
                                FileName = azureFile,
                                Header = header
                            };
                            storageFiles.Add(storageFile);

                            //process per 20
                            if (storageFiles.Count == 20)
                            {
                                await _azureStorage.SaveFiles(storageFiles);
                                foreach (var item in storageFiles)
                                {
                                    var logId = Convert.ToInt32(item.Header.FileName.Split(".")[0]);
                                    await _sharePointRepository.MarkSharePointToAzureBlobAsProcessed(logId);
                                }
                                storageFiles.Clear();
                            }
                        }

                    }
                    if (storageFiles.Count > 0)
                    {
                        await _azureStorage.SaveFiles(storageFiles);
                        foreach (var item in storageFiles)
                        {
                            var logId = Convert.ToInt32(item.Header.FileName.Split(".")[0]);
                            await _sharePointRepository.MarkSharePointToAzureBlobAsProcessed(logId);
                        }
                    }
                }

                if (forDelete.Any())
                {
                    foreach (var doc in forDelete)
                    {
                        var fileName = doc.LogId.ToString() + Path.GetExtension(doc.FileName);
                        var azureFile = _azureStorage.BuildPath(_azureStorage.DocumentRootFolder, string.Empty, fileName);
                        await _azureStorage.DeleteFile(azureFile);
                        await _sharePointRepository.MarkSharePointToAzureBlobAsProcessed(doc.LogId);
                    }
                }
            }

            return Ok();
        }

        private void GetChildrenDriveItems(SharePointSyncDTO parent, string? key)
        {
            var children = _sharePointSyncItems.Where(c => c.ParentId == parent.Id).ToList();
            foreach (var child in children)
            {
                child.Key = key;
                child.DocLibraryFolder = parent.DocLibraryFolder;
                var children2 = _sharePointSyncItems.Where(c => c.ParentId == child.Id).ToList();
                if (children2.Any())
                {
                    foreach (var child2 in children2)
                    {
                        child2.DocLibraryFolder = parent.DocLibraryFolder;
                        GetChildrenDriveItems(child2, key);
                    }
                }
            }
            if (string.IsNullOrEmpty(parent.Key))
                parent.Key = key;
        }

        public async Task<IActionResult> SyncToDocumentTables()
        {
            await _sharePointViewModelService.SyncToDocumentTables(User);
            //await _sharePointViewModelService.SyncToDocumentTables(new List<string> {SharePointDocLibrary.Trademark });
            return Ok();
        }

        public async Task<IActionResult> SyncToDocumentTablesUpdateDelete()
        {
            await _sharePointViewModelService.SyncToDocumentTablesUpdateDelete(User);
            //await _sharePointViewModelService.SyncToDocumentTablesUpdateDelete(new List<string> {SharePointDocLibrary.Trademark});
            return Ok();
        }

        public async Task<IActionResult> ClearSyncFlagToDocumentTables()
        {
            if (SharePointViewModelService.IsSharePointIntegrationHasSyncField) {
                await _sharePointViewModelService.ClearSyncFlagToDocumentTables(User);
            }
            return Ok();
        }



    }
}