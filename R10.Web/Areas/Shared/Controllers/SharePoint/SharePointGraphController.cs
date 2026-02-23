using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using R10.Core.DTOs;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Services.SharePoint;
using R10.Web.Services;
using System.Text;
using Microsoft.Extensions.Options;
using R10.Web.Filters;
using AutoMapper;
using R10.Web.Areas.Shared.ViewModels.SharePoint;
using R10.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using R10.Core.Entities.Documents;
using R10.Core.Helpers;
using R10.Core.Services.Shared;
using R10.Web.Areas.Shared.Services;
using DocuSign.eSign.Model;
using System.IO;
using System.Collections.Generic;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using Microsoft.AspNetCore.StaticFiles;
using DocumentFormat.OpenXml.Wordprocessing;
using ActiveQueryBuilder.View.DatabaseSchemaView;
using iText.Layout.Element;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Identity;
using System.Drawing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ActiveQueryBuilder.Web.Server.Models;
using System.Security.Policy;
using System;
using Newtonsoft.Json;
using R10.Core.Entities.GlobalSearch;
using System.IO.Compression;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using R10.Core.Entities.Trademark;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;
using Microsoft.SharePoint.News.DataModel;
using R10.Core;
using R10.Core.Exceptions;
using R10.Web.Services.DocumentStorage;
using DocumentFormat.OpenXml.Spreadsheet;
using R10.Web.Services.MailDownload;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using R10.Web.Security;
using Microsoft.AspNetCore.Http.HttpResults;
using R10.Core.Interfaces.RMS;
using DocumentFormat.OpenXml.Office2013.PowerPoint.Roaming;
using GleamTech.IO;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc.Rendering;
using R10.Core.Interfaces.ForeignFiling;
using OpenIddict.Validation.AspNetCore;

namespace R10.Web.Areas.Shared.Controllers.SharePoint
{
    [Area("Shared"), Authorize(AuthenticationSchemes = $"Identity.Application,{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme}")]
    [SharePointAuthorizationFilter()]
    public class SharePointGraphController : DocumentUploadController
    {
        private readonly ISharePointService _sharePointService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;
        private readonly IDocumentService _documentService;
        private readonly ICPiUserSettingManager _userSettingManager;
        private readonly ILetterService _letterService;
        private readonly ISharePointViewModelService _sharePointViewModelService;
        private readonly IRMSDueDocService _rmsDueDocService;
        private readonly IFFDueDocService _ffDueDocService;

        private readonly EPOMailboxSettings _epoMailboxSettings;
        private readonly IParentEntityService<EPOCommunication, EPOCommunicationDoc> _epoCommunicationDocService;

        private readonly IDocumentsAIViewModelService _documentsAIViewModelService;
        public SharePointGraphController(ISharePointService sharePointService,
                    IOptions<GraphSettings> graphSettings, IStringLocalizer<SharedResource> localizer,
                    IMapper mapper, IDocumentService documentService, ISystemSettings<DefaultSetting> settings, ICPiUserSettingManager userSettingManager,
                    ISystemSettings<PatSetting> patSettings, ISystemSettings<TmkSetting> tmkSettings, ISystemSettings<GMSetting> gmSettings,
                    IDocumentsViewModelService docViewModelService, IAuthorizationService authService, ILetterService letterService,
                    ISharePointViewModelService sharePointViewModelService,
                    IMailDownloadService mailDownloadService,
                    ILogger<DocDocumentsController> logger,
                    IOptions<ServiceAccount> serviceAccount,
                    IRMSDueDocService rmsDueDocService,
                    IFFDueDocService ffDueDocService,
                    IOptions<EPOMailboxSettings> epoMailboxSettings,
                    IParentEntityService<EPOCommunication, EPOCommunicationDoc> epoCommunicationDocService,
                    IEPOService epoService,
                    IEntityService<EPOCommunication> epoCommunicationService,
                    IDocumentsAIViewModelService documentsAIViewModelService
            ) : base(mailDownloadService, docViewModelService, graphSettings, serviceAccount, logger, authService, settings, patSettings, tmkSettings, gmSettings, epoService, epoCommunicationService)
        {
            _sharePointService = sharePointService;
            _localizer = localizer;
            _mapper = mapper;
            _documentService = documentService;
            _userSettingManager = userSettingManager;
            _letterService = letterService;
            _sharePointViewModelService = sharePointViewModelService;
            _rmsDueDocService = rmsDueDocService;
            _ffDueDocService = ffDueDocService;

            _epoMailboxSettings = epoMailboxSettings.Value;
            _epoCommunicationDocService = epoCommunicationDocService;

            _documentsAIViewModelService = documentsAIViewModelService;
        }

        public async Task<IActionResult> ImageRead([DataSourceRequest] DataSourceRequest request, string docLibrary, string? docLibraryFolder, string recKey, string screenCode, int parentId, bool isGallery, List<QueryFilterViewModel> criteria)
        {
            if (ModelState.IsValid)
            {
                Microsoft.Graph.Site site = null;

                var graphClient = _sharePointService.GetGraphClient();
                var spDocs = new List<SharePointGraphDriveItemViewModel>();
                var documents = new List<SharePointDocumentViewModel>();

                var fromTreeView = criteria.FirstOrDefault(c => c.Property == "FromTreeView");
                var treeNodeId = criteria.FirstOrDefault(c => c.Property == "TreeNodeId");

                var settings = await _settings.GetSetting();
                if (settings.IsSharePointIntegrationByMetadataOn)
                    recKey = recKey.Replace(SharePointSeparator.Folder, SharePointSeparator.Field);

                if (fromTreeView == null || fromTreeView.Value == "0" || (treeNodeId != null && treeNodeId.Value.Contains("sp-root")))
                {
                    var recKeys = new List<LookupDTO>();

                    //selected from tree
                    if (treeNodeId != null && treeNodeId.Value.Contains("sp-root"))
                    {
                        var nodeArray = treeNodeId.Value.Replace("|sp-root", "").Split("`");
                        docLibraryFolder = nodeArray[1];
                        recKey = String.Join("`", nodeArray.Skip(2).ToArray());
                        recKeys.Add(new LookupDTO { Value = docLibraryFolder, Text = recKey });
                    }

                    //all
                    else
                    {
                        recKeys.Add(new LookupDTO { Value = docLibraryFolder, Text = recKey });
                        if (!SharePointViewModelService.IsSharePointIntegrationMainScreenOnly)
                        {
                            if (docLibraryFolder == SharePointDocLibraryFolder.Application || docLibraryFolder == SharePointDocLibraryFolder.Trademark || docLibraryFolder == SharePointDocLibraryFolder.GeneralMatter)
                            {
                                recKeys.AddRange(await _sharePointViewModelService.GetChildrenRecKeys(docLibrary, parentId));
                            }
                        }
                    }

                    foreach (var rK in recKeys)
                    {
                        var folders = SharePointViewModelService.GetDocumentFolders(rK.Value, rK.Text);
                        var folderDocs = new List<SharePointGraphDriveItemViewModel>();

                        if (settings.IsSharePointIntegrationByMetadataOn)
                        {
                            folderDocs = await graphClient.GetSiteDocumentsByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, rK.Value, rK.Text.Replace(SharePointSeparator.Folder, SharePointSeparator.Field));
                        }
                        else
                            folderDocs = await graphClient.GetSiteDocuments(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders);

                        folderDocs.ForEach(d => { d.DocLibraryFolder = rK.Value; d.RecKey = rK.Text; });
                        spDocs.AddRange(folderDocs);
                    }

                }
                else
                {
                    if (treeNodeId != null)
                    {
                        var id = treeNodeId.Value.Split("|");
                        var isFolder = id.Contains("folder") || id.Length == 1; //length=1 if newly added

                        //specific item
                        spDocs = await graphClient.GetSiteDocuments(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, id[0], isFolder);
                        spDocs.ForEach(d => { d.DocLibraryFolder = docLibraryFolder; d.RecKey = recKey; });
                    }
                }
                documents = _mapper.Map<List<SharePointGraphDriveItemViewModel>, List<SharePointDocumentViewModel>>(spDocs);

                if (documents.Any())
                {
                    var docIcons = await _documentService.DocIcons.ToListAsync();
                    var signatures = new List<SharePointFileSignature>();

                    if (settings.IsSharePointIntegrationKeyFieldsOnly)
                    {
                        await _sharePointViewModelService.GetIsPrivateDocumentInfoFromDocTable(documents);
                    }

                    if (settings.IsESignatureOn)
                    {
                        signatures = await _documentService.SharePointFileSignatures.Where(s => s.ScreenCode == screenCode && s.ParentId == parentId).ToListAsync();
                    }

                    documents.ForEach((i) =>
                    {
                        i.ParentId = parentId;
                        i.DateCreated = i.DateCreated_Offset.HasValue ? i.DateCreated_Offset.Value.DateTime : null;
                        i.DateModified = i.DateModified_Offset.HasValue ? i.DateModified_Offset.Value.DateTime : null;

                        var file = i.Name.Split(".");
                        if (file.Length > 1)
                        {
                            var icon = docIcons.FirstOrDefault(i => i.FileExt.ToLower() == file[file.Length - 1].ToLower());
                            if (icon != null)
                            {
                                i.IconClass = icon.IconClass;
                            }
                        }
                        i.IsImage = ImageHelper.IsImageFile(i.Name);

                        i.DocLibrary = docLibrary;
                        //i.DocLibraryFolder = docLibraryFolder;
                        //i.RecKey = recKey;
                        if (!i.EditUrl.ToLower().Contains("_layouts"))
                        {
                            i.EditUrl = "";
                        }
                        i.DownloadUrl = Url.Action("DownloadFile", new { docLibrary, name = i.Name, id = i.Id });

                        if (i.ListItemFields != null)
                        {
                            var user = i.ListItemFields.GetValueOrDefault("CheckoutUserLookupId");
                            if (user != null)
                            {
                                var userId = user.ToString();

                                if (site == null)
                                {
                                    site = graphClient.GetSiteByPath(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;
                                }

                                var checkedOutBy = (graphClient.GetSiteUserInformation(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, site.Id, userId)).Result;
                                var fields = checkedOutBy.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                                i.CheckOutUser = fields.GetValueOrDefault("Title").ToString();
                                if (fields.GetValueOrDefault("UserName") != null && fields.GetValueOrDefault("UserName").ToString().ToLower() == User.GetEmail().ToLower())
                                {
                                    i.IsCheckedOut = true;
                                }
                            }

                            if (!settings.IsSharePointIntegrationKeyFieldsOnly && i.ListItemFields.ContainsKey("IsPrivate"))
                            {
                                i.IsPrivate = Convert.ToBoolean(i.ListItemFields.GetValueOrDefault("IsPrivate").ToString());
                            }
                        }

                        if (settings.IsESignatureOn)
                        {
                            var signature = signatures.FirstOrDefault(s => s.DriveItemId == i.Id);
                            if (signature != null)
                            {
                                i.ForSignature = true;
                                i.SignatureCompleted = signature.SignatureCompleted;
                                i.SentToDocuSign = !string.IsNullOrEmpty(signature.EnvelopeId);
                                i.DocLibrary = signature.DocLibrary;
                                i.DocLibraryFolder = signature.DocLibraryFolder;
                                i.ParentId = signature.ParentId;
                                i.EnvelopeId = signature.EnvelopeId;
                                i.ScreenCode = signature.ScreenCode;
                                i.RoleLink = signature.RoleLink;
                                i.SystemTypeCode = signature.SystemTypeCode;
                                i.QESetupId = signature.QESetupId;
                                i.SignatureReviewed = (bool)signature.SignatureReviewed;
                            }
                        }

                    });

                    var canViewPublicOnly = settings.IsRestrictPrivateDocAccessOn && await IsUserRestrictedFromPrivateDocuments();
                    if (canViewPublicOnly)
                    {
                        documents = documents.Where(d => !d.IsPrivate || d.CreatedBy.ToLower() == User.GetEmail().ToLower()).ToList();
                    }

                    if (criteria.Count > 0 && (fromTreeView == null || fromTreeView.Value == "0"))
                    {
                        documents = await ApplyCriteria(documents.AsQueryable(), criteria, parentId, docLibrary, docLibraryFolder);
                    }

                    if (isGallery)
                    {
                        await graphClient.GetSiteDriveItemsPreviewUrl(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, documents);
                        var images = documents.Where(d => d.IsImage.HasValue && (bool)d.IsImage).ToList();
                        await graphClient.GetSiteDriveItemsThumbnailUrl(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, images);
                    }

                    var userAccountSettings = await _userSettingManager.GetUserSetting<UserAccountSettings>(User.GetUserIdentifier());
                    if (_epoMailboxSettings.IsAPIOn && userAccountSettings.AllowHandleMyEPOCommunications)
                    {
                        var driveItemIds = documents.Select(d => d.Id).ToList();
                        var docList = await _documentService.DocDocuments.AsNoTracking()
                                                .Where(d => d.DocFile != null && driveItemIds.Contains(d.DocFile.DriveItemId))
                                                .Select(d => new { d.DocId, DriveItemId = d.DocFile != null ? d.DocFile.DriveItemId : "" }).ToListAsync();
                        var docIds = docList.Select(d => d.DocId).ToList();
                        var communications = await _epoCommunicationDocService.ChildService.QueryableList.AsNoTracking()
                                                    .Where(d => docIds.Contains(d.DocId) && d.Communication != null && d.Communication.Handled == false)
                                                    .Select(d => new { d.CommunicationId, d.DocId }).ToListAsync();

                        if (communications != null && communications.Count > 0)
                        {
                            foreach (var communication in communications)
                            {
                                var tempDoc = docList.Where(d => d.DocId == communication.DocId).FirstOrDefault();
                                if (tempDoc != null)
                                {                                    
                                    foreach (var doc in documents.Where(d => d.Id == tempDoc.DriveItemId))
                                    {
                                        doc.CommunicationId = communication.CommunicationId;
                                    }
                                }                                
                            }
                        }
                    }
                }

                return Json(documents.ToDataSourceResult(request));
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<IActionResult> GetImageSearchData(string property, string text, FilterType filterType, string docLibrary, string? docLibraryFolder, string recKey, int parentId)
        {
            var settings = await _settings.GetSetting();
            var graphClient = _sharePointService.GetGraphClient();

            var spDocs = new List<SharePointGraphDriveItemViewModel>();
            var recKeys = new List<LookupDTO>();

            recKeys.Add(new LookupDTO { Value = docLibraryFolder, Text = recKey });
            if (!SharePointViewModelService.IsSharePointIntegrationMainScreenOnly) {
                if (docLibraryFolder == SharePointDocLibraryFolder.Application || docLibraryFolder == SharePointDocLibraryFolder.Trademark || docLibraryFolder == SharePointDocLibraryFolder.GeneralMatter)
                {
                    recKeys.AddRange(await _sharePointViewModelService.GetChildrenRecKeys(docLibrary, parentId));
                }
            }

            foreach (var rK in recKeys)
            {
                var folders = SharePointViewModelService.GetDocumentFolders(rK.Value, rK.Text);
                var folderDocs = new List<SharePointGraphDriveItemViewModel>();

                if (settings.IsSharePointIntegrationByMetadataOn)
                {
                    folderDocs = await graphClient.GetSiteDocumentsByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, rK.Value, rK.Text.Replace(SharePointSeparator.Folder, SharePointSeparator.Field));
                }
                else
                    folderDocs = await graphClient.GetSiteDocuments(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders);

                folderDocs.ForEach(d => { d.DocLibraryFolder = rK.Value; d.RecKey = rK.Text; });
                spDocs.AddRange(folderDocs);
            }

            var list = (_mapper.Map<List<SharePointGraphDriveItemViewModel>, List<SharePointDocumentViewModel>>(spDocs));
            if (settings.IsSharePointIntegrationKeyFieldsOnly)
            {
                await _sharePointViewModelService.GetIsPrivateDocumentInfoFromDocTable(list);
            }
            else
            {
                foreach (var item in list)
                {
                    if (item.ListItemFields.ContainsKey("IsPrivate"))
                    {
                        item.IsPrivate = Convert.ToBoolean(item.ListItemFields.GetValueOrDefault("IsPrivate").ToString());
                    }
                }
            }
            var images = list.AsQueryable();

            var canViewPublicOnly = settings.IsRestrictPrivateDocAccessOn && await IsUserRestrictedFromPrivateDocuments();
            if (canViewPublicOnly)
            {
                images = images.Where(d => !d.IsPrivate || d.CreatedBy.ToLower() == User.GetEmail().ToLower());
            }

            images = QueryHelper.BuildCriteria(images, property, text, filterType, "");
            var propertyExpression = R10.Web.Helpers.ExpressionHelper.GetStringPropertyExpression<SharePointDocumentViewModel>(property);
            var result = images.GroupBy(propertyExpression).Select(x => x.First()).OrderBy(property).ToList(); //distinct not working
            return Json(result);
        }

        public async Task<IActionResult> IsAuthenticated()
        {
            return Ok();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDropped(IEnumerable<IFormFile> droppedFiles, string docLibrary, string? docLibraryFolder, string recKey, string folderId, int parentId, string? roleLink)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            recKey = recKey.Replace("`~`", "&"); //issue when filename has &

            var settings = await _settings.GetSetting();
            if (!settings.IsSharePointIntegrationByMetadataOn && settings.SharePointInvalidCharacters.Any(s => recKey.Contains(s)))
            {
                return BadRequest(_localizer[$"The record key {recKey} should not contain any of the invalid SharePoint characters"].Value + "  " + $"{settings.SharePointInvalidCharacters}");
            }

            return await SaveDroppedFiles(droppedFiles, docLibrary, docLibraryFolder, recKey, folderId, parentId, roleLink, false, null);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDroppedDefaultImage(IFormFile droppedFile, string docLibrary, string folders, int parentId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (ImageHelper.IsImageFile(droppedFile.FileName))
            {
                var folderList = folders.Split(SharePointSeparator.Folder).ToList();
                var docLibraryFolder = folderList[0];
                var recFolders = String.Join(SharePointSeparator.Folder, folderList.Skip(1));

                var settings = await _settings.GetSetting();
                if (!settings.IsSharePointIntegrationByMetadataOn && settings.SharePointInvalidCharacters.Any(s => recFolders.Contains(s)))
                {
                    return BadRequest(_localizer[$"The record key {recFolders} should not contain any of the invalid SharePoint characters"].Value + "  " + $"{settings.SharePointInvalidCharacters}");
                }

                return await SaveDroppedFiles(new List<IFormFile> { droppedFile }, docLibrary, docLibraryFolder, recFolders, "", parentId, "", true, null);
            }
            else
            {
                return BadRequest(_localizer["Please upload an image file."].Value);
            }
        }

        public async Task<IActionResult> GetDefaultImage(string activePage, string docLibrary, string folders, bool canUploadImage, int parentId)
        {
            var graphClient = _sharePointService.GetGraphClient();
            var image = new DefaultImageViewModel();

            var folderList = folders.Split(SharePointSeparator.Folder).ToList();

            var settings = await _settings.GetSetting();
            if (settings.IsSharePointIntegrationKeyFieldsOnly)
            {
                image = await _sharePointViewModelService.GetDefaultImageDocumentInfoFromDocTable(docLibrary, folderList[0], parentId);
            }
            else
            {
                if (settings.IsSharePointIntegrationByMetadataOn)
                {
                    image = await graphClient.GetDefaultImageByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folderList[0], String.Join(SharePointSeparator.Field, folderList.Skip(1)));
                }
                else
                {
                    image = await graphClient.GetDefaultImage(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folderList);
                }
            }

            //check default image permission
            var userAccountSettings = await _userSettingManager.GetUserSetting<UserAccountSettings>(User.GetUserIdentifier());
            if (image != null && !image.IsPublic && userAccountSettings.RestrictPrivateDocuments)
                image = null;

            ViewBag.CanUploadImage = canUploadImage;
            ViewBag.ActivePage = activePage;
            ViewBag.SharePointDocLibrary = docLibrary;
            ViewBag.SharePointFolders = folders;
            ViewBag.ParentId = parentId;
            return ViewComponent("DefaultSPImage", image);
        }

        public async Task<IActionResult> DeleteFile(string docLibrary, string id)
        {
            var graphClient = _sharePointService.GetGraphClient();

            try
            {
                await graphClient.DeleteSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, id);
                await _documentService.DeleteSharePointForSignature(id);

                var success = _localizer["File has been deleted successfully"].ToString();
                await _sharePointViewModelService.SyncToDocumentTablesDelete(docLibrary, id);
                return Ok(new { success = success });
            }
            catch (Exception ex)
            {

                if (ex.Message.Contains("is locked"))
                {
                    var message = _localizer["File is locked. If you are the last person who modified the file, try to wait for a few seconds."].ToString();
                    return BadRequest(message);
                }
                throw;
            }
        }

        public async Task<IActionResult> GetPreviewUrl(string docLibrary, string id)
        {
            var graphClient = _sharePointService.GetGraphClient();
            var previewUrl = await graphClient.GetSiteDriveItemPreviewUrl(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, id);
            return Json(new { previewUrl });
        }

        public async Task<IActionResult> GetPreviewUrlForLinks(string docLibrary, string id)
        {
            var graphClient = _sharePointService.GetGraphClient();
            var previewUrl = await graphClient.GetSiteDriveItemPreviewUrlForLink(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, id);
            return Json(new { previewUrl });
        }

        public async Task<IActionResult> GetThumbnailUrl(string docLibrary, string id)
        {
            var graphClient = _sharePointService.GetGraphClient();
            var url = await graphClient.GetSiteDriveItemThumbnailUrl(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, id);
            return Json(new { url });
        }

        public async Task<IActionResult> GetDefaultWithThumbnailUrl(string docLibrary, string? docLibraryFolder, string? driveId, SharePointGraphDefaultImageParamViewModel recKey)
        {
            var graphClient = _sharePointService.GetGraphClient();

            var folders = SharePointViewModelService.GetDocumentFolders(docLibraryFolder, recKey.RecKey);
            var image = await graphClient.GetDefaultImage(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders, driveId);
            if (image != null)
            {
                var urls = await graphClient.GetSiteDriveItemThumbnailUrl(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, image.SharePointDriveItemId);
                var output = new SharePointGraphDefaultImageViewModel
                {
                    DriveId = image.SharePointDriveId,
                    RecKey = recKey.RecKey,
                    ImageFile = image.SharePointDriveItemId,
                    ThumbnailUrl = urls.SmallThumbnailUrl,
                    DisplayUrl = urls.BigThumbnailUrl,
                    Id = recKey.Id
                };
                return Json(output);
            }

            return Json(null);
        }



        public async Task<IActionResult> CheckoutFile(string docLibrary, string id)
        {
            var graphClient = _sharePointService.GetGraphClient();

            try
            {
                await graphClient.CheckoutSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, id);
                var success = _localizer["File has been checked out successfully"].ToString();
                return Ok(new { success = success });
            }
            catch (Exception ex)
            {
                return BadRequest(_localizer["File cannot be checked out, probably locked by another user."].ToString());

            }

        }

        public async Task<IActionResult> CheckinFile(string docLibrary, string id)
        {
            var graphClient = _sharePointService.GetGraphClient();

            try
            {
                await graphClient.CheckinSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, id);
                var success = _localizer["File has been checked in successfully"].ToString();
                return Ok(new { success = success });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The file is not checked out"))
                    return BadRequest(_localizer["The file is not checked out"].ToString());
                else
                    return BadRequest(_localizer["File cannot be checked in, an error was encountered."].ToString());
            }


        }

        public async Task<IActionResult> DownloadFile(string docLibrary, string name, string id)
        {
            // log trade secret download
            await LogDocTradeSecretActivity(id);

            var graphClient = _sharePointService.GetGraphClient();
            var stream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, id);
            if (stream != null)
            {
                var mimeType = GetMimeTypeForFileExtension(name);
                return new FileStreamResult(stream, mimeType) { FileDownloadName = name };
            }
            return BadRequest();
        }

        public async Task<IActionResult> DownloadPTOFile(string systemType, string caseNumber, string country, string subCase, string fileName, int noPages, int pageStart, int zoom = 0)
        {
            var graphClient = _sharePointService.GetGraphClient();
            var recKey = SharePointViewModelService.BuildRecKey(caseNumber, country, subCase);
            var docLibrary = systemType == SystemTypeCode.Patent ? SharePointDocLibrary.Patent : SharePointDocLibrary.Trademark;
            var docLibraryFolder = systemType == SystemTypeCode.Patent ? SharePointDocLibraryFolder.Application : SharePointDocLibraryFolder.TmkLinks;

            var folders = SharePointViewModelService.GetDocumentFolders(docLibraryFolder, recKey);
            folders = folders.Where(f => !string.IsNullOrEmpty(f)).ToList();
            var path = string.Join("/", folders);
            var existing = await graphClient.GetSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, path, fileName);
            if (existing != null)
            {
                var stream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, existing.Id);
                if (stream != null)
                {
                    var folder = Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"UserFiles\Temporary Folder", User.GetUserName());
                    if (!System.IO.Directory.Exists(folder))
                        System.IO.Directory.CreateDirectory(folder);
                    var temporaryPath = Path.Combine(folder, fileName);

                    if (noPages > 0)
                    {
                        int[] pages = new int[noPages];
                        for (var i = 0; i <= noPages - 1; i++)
                        {
                            pages[i] = pageStart + i;
                        }
                        Helper.ExtractPdfPage(stream, pages, temporaryPath);
                    }
                    else
                    {
                        using (var fileStream = new FileStream(temporaryPath, FileMode.Create))
                        {
                            stream.CopyTo(fileStream);
                            fileStream.Position = 0;
                        }
                    }

                    if (zoom == 0)
                        return new PhysicalFileResult(temporaryPath, ImageHelper.GetContentType(temporaryPath)) { FileDownloadName = fileName };
                    else
                    {
                        return RedirectToAction("ZoomTempFile", "DocViewer", new { Area = "Shared", filename = temporaryPath });
                    }

                }
            }
            return BadRequest("File not found.");
        }

        public async Task<IActionResult> DownloadEPOMailFile(string systemType, string caseNumber, string country, string subCase, string driveItemId, string fileName, int zoom = 0)
        {
            var graphClient = _sharePointService.GetGraphClient();
            var docLibrary = systemType == SystemTypeCode.Patent ? SharePointDocLibrary.Patent : SharePointDocLibrary.Orphanage;
            if (zoom == 0)
            {
                var stream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId);
                if (stream != null)
                {
                    var mimeType = GetMimeTypeForFileExtension(fileName);
                    return new FileStreamResult(stream, mimeType) { FileDownloadName = fileName };
                }
            }
            else
            {
                var previewUrl = await graphClient.GetSiteDriveItemPreviewUrl(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId);
                return Json(new { previewUrl });
            }
            return BadRequest("File not found.");
        }

        [HttpPost]
        public async Task<IActionResult> DownloadFiles(string docLibrary, string selection)
        {
            var selectionList = JsonConvert.DeserializeObject<List<SharePointDocumentDownloadViewModel>>(selection);

            if (selectionList != null && selectionList.Count > 0)
            {
                // log trade secret download
                var settings = await _patSettings.GetSetting();
                if (settings.IsTradeSecretOn)
                    await _documentService.LogDocTradeSecretActivityByDriveItemIds(selectionList.Select(s => s.DriveItemId).ToList());

                var graphClient = _sharePointService.GetGraphClient();

                await graphClient.DownloadSiteDriveItems(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, selectionList);

                byte[] compressedFiles = null;
                using (var memoryStream = new MemoryStream())
                {
                    using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var file in selectionList)
                        {
                            if (file.FileBytes != null)
                            {
                                var fileName = file.Name.Replace("?", "").Replace(@"\", "").Replace(@"/", "")
                                          .Replace(":", "").Replace("*", "").Replace("<", "")
                                          .Replace(">", "").Replace("|", "");

                                ZipArchiveEntry zipItem = zip.CreateEntry(fileName);
                                using (var originalStream = new MemoryStream(file.FileBytes))
                                {
                                    using (var entryStream = zipItem.Open())
                                    {
                                        originalStream.CopyTo(entryStream);
                                    }
                                }
                            }
                        }
                    }
                    compressedFiles = memoryStream.ToArray();
                }
                var stream = new MemoryStream(compressedFiles);
                return new FileStreamResult(stream, "application/zip") { FileDownloadName = "Data.zip" };
            }
            return BadRequest();
        }

        [HttpGet()]
        public async Task<IActionResult> FileMerge()
        {
            return PartialView("_MergeFile");
        }

        [HttpPost]
        public async Task<IActionResult> MergeFiles(string docLibrary, string docLibraryFolder, string recKey, int parentId, string mergedDocName, List<SharePointDocumentViewModel> docList)
        {
            if (string.IsNullOrEmpty(docLibrary) || string.IsNullOrEmpty(docLibraryFolder) || string.IsNullOrEmpty(recKey))
                return BadRequest(_localizer["Missing SharePoint link."]);

            if (string.IsNullOrEmpty(mergedDocName))
                return BadRequest(_localizer["Merged Document Name is required."]);

            if (docList == null || docList.Count < 1)
                return BadRequest(_localizer["No documents selected to merge."]);

            if (docList.Any(d => string.IsNullOrEmpty(d.Name) || !d.Name.EndsWith(".pdf")))
                return BadRequest(_localizer["Please selecte PDF files only."]);

            var docBytes = new List<byte[]>();
            var graphClient = _sharePointService.GetGraphClient();

            foreach (var doc in docList)
            {
                if (string.IsNullOrEmpty(doc.DocLibrary) || string.IsNullOrEmpty(doc.Id)) continue;
                               
                if(_graphSettings.Site != null)
                {
                    var spStream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, doc.DocLibrary, doc.Id);
                    if (spStream != null)
                        docBytes.Add(spStream.ToBytes());
                }                
            }

            if (docBytes.Count > 0)
            {
                byte[]? mergedPdf = null;

                // Create a MemoryStream to hold the combined PDF
                using (MemoryStream ms = new MemoryStream())
                {
                    // Initialize PDF writer
                    PdfWriter writer = new PdfWriter(ms);
                    // Initialize PDF document
                    PdfDocument pdf = new PdfDocument(writer);
                    foreach (byte[] pdfBytes in docBytes)
                    {
                        // Create a PdfReader
                        PdfReader reader = new PdfReader(new MemoryStream(pdfBytes));
                        // Initialize source PDF document
                        PdfDocument sourcePdf = new PdfDocument(reader);
                        // Copy pages from source PDF to the destination PDF
                        sourcePdf.CopyPagesTo(1, sourcePdf.GetNumberOfPages(), pdf);
                        // Close the source PDF
                        sourcePdf.Close();
                    }
                    // Close the destination PDF
                    pdf.Close();

                    mergedPdf = ms.ToArray();
                }

                if (mergedPdf != null && mergedPdf.Length > 0)
                {
                    if (!mergedDocName.EndsWith(".pdf")) mergedDocName += ".pdf";

                    using (var ms = new MemoryStream(mergedPdf))
                    {                        
                        var folders = SharePointViewModelService.GetDocumentFolders(docLibraryFolder, recKey);
                        if (_graphSettings.Site != null)
                        {
                            var result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders, ms, mergedDocName);
                            if (result != null)
                            {
                                var driveItem = await graphClient.Drives[result.DriveId].Items[result.DriveItemId].Request().Expand("listItem").GetAsync();

                                var sync = new SharePointSyncToDocViewModel
                                {
                                    DocLibrary = docLibrary,
                                    DocLibraryFolder = docLibraryFolder,
                                    DriveItemId = driveItem.Id,
                                    ParentId = parentId,
                                    FileName = mergedDocName,
                                    CreatedBy = User.GetUserName(),
                                    Remarks = "",
                                    Tags = "",
                                    IsImage = driveItem.Image != null,
                                    IsPrivate = false,
                                    IsDefault = false,
                                    IsPrintOnReport = false,
                                    IsVerified = false,
                                    IncludeInWorkflow = false,
                                    IsActRequired = false,
                                    CheckAct = false,
                                    SendToClient = false,
                                    Source = DocumentSourceType.Manual,
                                    Author = User.GetEmail()
                                };
                                await _sharePointViewModelService.SyncToDocumentTables(sync);
                            }
                        }
                    }
                    return Ok(_localizer["Documents merged successfully."]);
                }
            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> Version(string docLibrary, string name, string driveItemId)
        {
            var model = new SharePointGraphDriveItemParamViewModel { DocLibrary = docLibrary, Name = name, DriveItemId = driveItemId };
            return PartialView("_FileVersion", model);
        }

        public async Task<IActionResult> GetVersionHistory([DataSourceRequest] DataSourceRequest request, string docLibrary, string name, string driveItemId)
        {
            if (ModelState.IsValid)
            {
                var graphClient = _sharePointService.GetGraphClient();
                var result = await graphClient.GetSiteDriveItemVersionHistory(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId);
                var isCheckedOut = await graphClient.IsSiteDriveItemCheckedOut(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId);

                var counter = 0;
                foreach (var item in result)
                {
                    item.Name = name;

                    //can't download the latest 
                    if (counter > 0)
                    {
                        item.DownloadUrl = Url.Action("GetVersionContent", new { docLibrary, name, driveItemId, versionId = item.Id });

                        if (!isCheckedOut)
                            item.RestoreUrl = Url.Action("RestoreVersion", new { docLibrary, name, driveItemId, versionId = item.Id });
                    }
                    else
                    {
                        item.DownloadUrl = Url.Action("DownloadFile", new { docLibrary, name, id = driveItemId });
                        item.RestoreUrl = "";
                    }
                    counter++;
                }
                return Json(result.ToDataSourceResult(request));
            }
            return new JsonBadRequest(new
            {
                errors = ModelState.Errors()
            });
        }

        public async Task<IActionResult> GetVersionContent(string docLibrary, string name, string driveItemId, string versionId)
        {
            var graphClient = _sharePointService.GetGraphClient();
            var stream = await graphClient.GetSiteDriveItemVersionContent(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId, versionId);
            if (stream != null)
            {
                var mimeType = GetMimeTypeForFileExtension(name);
                return new FileStreamResult(stream, mimeType) { FileDownloadName = name };
            }
            return BadRequest();
        }

        public async Task<IActionResult> RestoreVersion(string docLibrary, string name, string driveItemId, string versionId)
        {
            var graphClient = _sharePointService.GetGraphClient();
            await graphClient.RestoreSiteDriveItemVersion(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId, versionId);
            return Ok();
        }


        [HttpGet]
        public IActionResult AddFile(string docLibrary, string? docLibraryFolder, string recKey, string folderId, int parentId, string roleLink)
        {
            var viewModel = new SharePointDocumentEntryViewModel
            {
                DocLibrary = docLibrary,
                DocLibraryFolder = docLibraryFolder,
                RecKey = recKey,
                FolderId = folderId,
                ParentId = parentId,
                RoleLink = roleLink,
                Source = DocumentSourceType.Manual,
                RandomGuid = Guid.NewGuid().ToString()
            };
            return PartialView("_ModifyFile", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddFile(SharePointDocumentEntryViewModel viewModel)
        {
            if (viewModel.UploadedFiles == null && viewModel.Type == "File")
                return BadRequest(_localizer["Please select a file to upload."].ToString());

            var fileName = "";
            IFormFile formFile = null;
            if (viewModel.UploadedFiles != null)
            {
                formFile = viewModel.UploadedFiles.First();
                if (formFile.Length <= 0 && viewModel.Type == "File")
                    return BadRequest(_localizer["File uploaded has zero length."].ToString());
                fileName = formFile.FileName;
            }

            var patSettings = await _patSettings.GetSetting();
            var tmkSettings = await _tmkSettings.GetSetting();
            var gmSettings = await _gmSettings.GetSetting();
            var settings = await _settings.GetSetting();

            var success = false;
            var result = new SharePointGraphDriveItemKeyViewModel();
            var graphClient = _sharePointService.GetGraphClient();
            var folders = SharePointViewModelService.GetDocumentFolders(viewModel.DocLibraryFolder, viewModel.RecKey);

            if (viewModel.CheckAct && !string.IsNullOrEmpty(viewModel.DocLibrary) && !string.IsNullOrEmpty(viewModel.DocLibraryFolder) 
                    && ((patSettings.IsDocumentVerificationOn && viewModel.DocLibrary.ToLower() == SharePointDocLibrary.Patent.ToLower() 
                        && viewModel.DocLibraryFolder.ToLower() == SharePointDocLibraryFolder.Application.ToLower()) 
                    || (tmkSettings.IsDocumentVerificationOn && viewModel.DocLibrary.ToLower() == SharePointDocLibrary.Trademark.ToLower() 
                        && viewModel.DocLibraryFolder.ToLower() == SharePointDocLibraryFolder.Trademark.ToLower()) 
                    || (gmSettings.IsDocumentVerificationOn && viewModel.DocLibrary.ToLower() == SharePointDocLibrary.GeneralMatter.ToLower() 
                        && viewModel.DocLibraryFolder.ToLower() == SharePointDocLibraryFolder.GeneralMatter.ToLower())))
            {
                var docVerificationFolderName = string.Empty;
                switch (viewModel.DocLibraryFolder)
                {
                    case SharePointDocLibraryFolder.Application:
                        docVerificationFolderName = patSettings.DocVerificationDefaultFolderName;
                        break;
                    case SharePointDocLibraryFolder.Trademark:
                        docVerificationFolderName = tmkSettings.DocVerificationDefaultFolderName;
                        break;
                    case SharePointDocLibraryFolder.GeneralMatter:
                        docVerificationFolderName = gmSettings.DocVerificationDefaultFolderName;
                        break;
                }

                if (string.IsNullOrEmpty(docVerificationFolderName)) docVerificationFolderName = "Dockets for Verification";

                folders.Add(docVerificationFolderName);
                    
            }
            
            if (!string.IsNullOrEmpty(viewModel.RecKey) && !settings.IsSharePointIntegrationByMetadataOn && settings.SharePointInvalidCharacters.Any(s => viewModel.RecKey.Contains(s)))
            {
                return BadRequest(_localizer[$"The record key {viewModel.RecKey} should not contain any of the invalid SharePoint characters"].Value + "  " + $"{settings.SharePointInvalidCharacters}");
            }

            if (viewModel.Type == "File")
            {
                if (!string.IsNullOrEmpty(viewModel.DocName))
                {
                    fileName = viewModel.DocName + Path.GetExtension(fileName);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(viewModel.DocName) || string.IsNullOrEmpty(viewModel.DocUrl))
                    return BadRequest(_localizer["Document Name and URL should not be empty."]);
                fileName = viewModel.DocName + ".url";
            }

            if (settings.SharePointInvalidCharacters.Any(s => fileName.Contains(s)))
            {
                return BadRequest(_localizer[$"The file name {fileName} should not contain any of the invalid SharePoint characters"].Value + "  " + $"{settings.SharePointInvalidCharacters}");
            }

            var attachments = new List<WorkflowEmailAttachmentViewModel>();
            using (var stream = new MemoryStream())
            {
                if (viewModel.Type == "File")
                {
                    formFile.CopyTo(stream);
                }
                else
                {
                    var link = $"[InternetShortcut]{System.Environment.NewLine}URL={viewModel.DocUrl}";
                    stream.Write(Encoding.UTF8.GetBytes(link));
                }
                stream.Position = 0;

                if (settings.IsSharePointIntegrationByMetadataOn)
                {
                    var existing = await graphClient.FileExists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, viewModel.DocLibrary, fileName);
                    if (existing)
                        return BadRequest(_localizer["File already exists. Please upload a file with different filename."].ToString());
                    result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, viewModel.DocLibrary, new List<string>(), stream, fileName);
                }
                else
                {
                    var existing = await graphClient.FileExists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, viewModel.DocLibrary, folders, fileName);
                    if (existing)
                        return BadRequest(_localizer["File already exists. Please upload a file with different filename."].ToString());

                    if (string.IsNullOrEmpty(viewModel.FolderId) || viewModel.FolderId == "0")
                    {
                        result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, viewModel.DocLibrary, folders, stream, fileName);
                    }
                    else
                    {
                        result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, viewModel.DocLibrary, viewModel.FolderId, stream, fileName);
                    }
                }
                success = true;
            }

            //any custom property is supplied
            //if ((settings.IsSharePointIntegrationByMetadataOn) ||
            //    (!string.IsNullOrEmpty(result.DriveItemId) && (!string.IsNullOrEmpty(viewModel.Remarks) || !string.IsNullOrEmpty(viewModel.Tags) ||
            //    viewModel.IsPrintOnReport || viewModel.IsDefault || viewModel.IsPrivate || viewModel.IsVerified || viewModel.IncludeInWorkflow
            //    || viewModel.IsActRequired))
            //    ){}

            var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;
            var list = site.Lists.Where(l => l.Name == viewModel.DocLibrary).FirstOrDefault();
            if (list != null)
            {
                var driveItem = await graphClient.Drives[result.DriveId].Items[result.DriveItemId].Request().Expand("listItem").GetAsync();
                if (driveItem != null)
                {
                    var sync = new SharePointSyncToDocViewModel
                    {
                        DocLibrary = viewModel.DocLibrary,
                        DocLibraryFolder = viewModel.DocLibraryFolder,
                        DriveItemId = driveItem.Id,
                        ParentId = viewModel.ParentId,
                        FileName = fileName,
                        CreatedBy = User.GetUserName().Left(20),
                        Remarks = viewModel.Remarks,
                        IsImage = driveItem.Image != null,
                        IsPrivate = viewModel.IsPrivate,
                        IsDefault = viewModel.IsDefault,
                        IsPrintOnReport = viewModel.IsPrintOnReport,
                        IsVerified = viewModel.IsVerified,
                        IncludeInWorkflow = viewModel.IncludeInWorkflow,
                        IsActRequired = viewModel.IsActRequired,
                        CheckAct = viewModel.CheckAct,
                        SendToClient = viewModel.SendToClient,
                        Source = viewModel.Source ?? DocumentSourceType.Manual,
                        Author = driveItem.CreatedBy.User.AdditionalData["email"].ToString()
                    };
                    await _sharePointViewModelService.SyncToDocumentTables(sync);
                    
                    //return file id
                    viewModel.FileId = sync.FileId;

                    var requestBody = new FieldValueSet
                    {
                        AdditionalData = new Dictionary<string, object>()
                    };

                    if (SharePointViewModelService.IsSharePointIntegrationHasSyncField)
                    {
                        requestBody.AdditionalData.Add("CPISyncCompleted", true);
                    }

                    if (!settings.IsSharePointIntegrationKeyFieldsOnly)
                    {
                        requestBody.AdditionalData.Add("Remarks", viewModel.Remarks ?? "");
                        requestBody.AdditionalData.Add("IsPrintOnReport", viewModel.IsPrintOnReport);
                        requestBody.AdditionalData.Add("IsDefault", viewModel.IsDefault);
                        requestBody.AdditionalData.Add("IsPrivate", viewModel.IsPrivate);
                        requestBody.AdditionalData.Add("IsVerified", viewModel.IsVerified);
                        requestBody.AdditionalData.Add("IncludeInWorkflow", viewModel.IncludeInWorkflow);

                        requestBody.AdditionalData.Add("IsActRequired", viewModel.IsActRequired);
                        requestBody.AdditionalData.Add("CheckAct", viewModel.CheckAct);
                        requestBody.AdditionalData.Add("SendToClient", viewModel.SendToClient);
                        requestBody.AdditionalData.Add("Source", viewModel.Source ?? DocumentSourceType.Manual);
                    }

                    if (settings.IsSharePointIntegrationByMetadataOn)
                    {
                        requestBody.AdditionalData.Add("CPIScreen", viewModel.DocLibraryFolder);
                        requestBody.AdditionalData.Add("CPIRecordKey", viewModel.RecKey.Replace(SharePointSeparator.Folder, SharePointSeparator.Field));
                    }

                    if (requestBody.AdditionalData.Count > 0)
                       await graphClient.Sites[site.Id].Lists[list.Id].Items[driveItem.ListItem.Id].Fields.Request().UpdateAsync(requestBody);

                }
            }

            if (success)
            {
                if (!string.IsNullOrEmpty(result.DriveItemId))
                {
                    if (viewModel.ParentId > 0)
                    {
                        var docDocument = await _documentService.DocDocuments.AsNoTracking().Where(d => d.DocFile.DriveItemId == result.DriveItemId).FirstOrDefaultAsync();

                        if (viewModel.IsActRequired && docDocument != null) await _documentService.LinkDocWithVerifications(docDocument.DocId, viewModel.RandomGuid ?? "");

                        if (viewModel.IsActRequired && !string.IsNullOrEmpty(viewModel.VerificationActionList) && docDocument != null)
                        {
                            var verifications = new List<DocVerification>();
                            foreach (var item in viewModel.VerificationActionList.Split(",").ToList())
                            {
                                var keyIdArr = item.Split("|");
                                var dataKey = keyIdArr[0];
                                var dataKeyValue = keyIdArr[1];
                                var keyValue = 0;
                                var temp = int.TryParse(dataKeyValue, out keyValue);
                                var verification = new DocVerification
                                {
                                    DocId = docDocument.DocId,
                                    ActionTypeID = dataKey.ToLower() == "actiontypeid" ? keyValue : 0,
                                    ActId = dataKey.ToLower() == "actid" ? keyValue : 0,
                                    CreatedBy = docDocument.CreatedBy,
                                    UpdatedBy = docDocument.UpdatedBy,
                                    DateCreated = docDocument.DateCreated,
                                    LastUpdate = docDocument.LastUpdate
                                };
                                verifications.Add(verification);
                            }
                            await _documentService.AddDocVerifications(verifications);
                        }

                        attachments.Add(new WorkflowEmailAttachmentViewModel
                        {
                            Id = result.DriveItemId,
                            FileName = fileName,
                            DocParent = viewModel.ParentId,
                            DocDate = DateTime.Now
                        });

                        //Responsible Docketing
                        var hasNewRespDocketing = false;
                        if (viewModel.RespDocketings != null && viewModel.RespDocketings.Length > 0 && !string.IsNullOrEmpty(viewModel.RespDocketings[0]))
                        {
                            //Weird issue with string[] and ajax
                            var respDocketingList = viewModel.RespDocketings[0].Split(",").Where(d => !string.IsNullOrEmpty(d)).ToList();
                            if (docDocument != null)
                                await _documentService.UpdateDocRespDocketing(respDocketingList, User.GetUserName(), docDocument.DocId);
                            hasNewRespDocketing = true;
                        }

                        //Responsible Reporting
                        var hasNewRespReporting = false;
                        if (viewModel.RespReportings != null && viewModel.RespReportings.Length > 0 && !string.IsNullOrEmpty(viewModel.RespReportings[0]))
                        {
                            //Weird issue with string[] and ajax
                            var respReportingList = viewModel.RespReportings[0].Split(",").Where(d => !string.IsNullOrEmpty(d)).ToList();
                            if (docDocument != null)
                                await _documentService.UpdateDocRespReporting(respReportingList, User.GetUserName(), docDocument.DocId);
                            hasNewRespReporting = true;
                        }

                        var workflowHeader = await GenerateWorkflow(viewModel.DocLibrary, viewModel.DocLibraryFolder, attachments, true, hasNewRespDocketing, false, hasNewRespReporting, false);

                        var eSignatureWorkflows = new List<WorkflowSignatureViewModel>();
                        if (settings.IsESignatureOn)
                        {
                            eSignatureWorkflows = await GenerateSignatureWorkflow(workflowHeader, viewModel.DocLibrary, viewModel.DocLibraryFolder, viewModel.RecKey, attachments, viewModel.ParentId, viewModel.RoleLink);
                        }

                        var emailWorkflows = GenerateEmailWorkflow(workflowHeader, attachments, viewModel.ParentId);

                        //DocVerification - process new action workflows                    
                        var hasDocVerificationEmailWorkflows = new List<WorkflowEmailViewModel>();
                        if (docDocument != null && docDocument.DocId > 0) hasDocVerificationEmailWorkflows = await ProcessDocVerificationNewActWorkflow(viewModel.DocId);

                        if (hasDocVerificationEmailWorkflows != null && hasDocVerificationEmailWorkflows.Count > 0) emailWorkflows.AddRange(hasDocVerificationEmailWorkflows);

                        if (emailWorkflows != null || eSignatureWorkflows != null)
                        {
                            var emailUrl = "";
                            if (emailWorkflows != null && emailWorkflows.Any())
                                emailUrl = emailWorkflows.First().emailUrl;

                            return Json(new { id = viewModel.ParentId, sendEmail = true, folderId = viewModel.FolderId, emailUrl, emailWorkflows, eSignatureWorkflows });
                        }
                        return Json(new
                        {
                            folderId = viewModel.FolderId
                        });

                    }
                }

                var successMsg = _localizer["File has been added successfully"].ToString();
                return Ok(new { success = successMsg });
            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> ModifyFile(string docLibrary, string? docLibraryFolder, string recKey, string driveItemId, string listItemId, int parentId = 0)
        {
            var graphClient = _sharePointService.GetGraphClient();
            var driveItem = await graphClient.GetSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId);

            if (driveItem != null)
            {
                var isImage = driveItem.File.MimeType.Contains("image");
                var viewModel = new SharePointDocumentEntryViewModel
                {
                    DocLibrary = docLibrary,
                    DocLibraryFolder = docLibraryFolder,
                    RecKey = recKey,
                    DriveItemId = driveItemId,
                    ListItemId = listItemId,
                    IsImage = isImage,
                    ParentId = parentId,
                    DocName = Path.GetFileNameWithoutExtension(driveItem.Name),
                    OrigDocName = Path.GetFileNameWithoutExtension(driveItem.Name),
                    Type = driveItem.Name.EndsWith("url") ? "Link" : "File"
                };

                if (driveItem.Name.EndsWith("url"))
                {
                    viewModel.DocUrl = await graphClient.GetSiteDriveItemPreviewUrlForLink(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId);
                    viewModel.OrigDocUrl = viewModel.DocUrl;
                }

                if (viewModel.DocLibraryFolder == SharePointDocLibraryFolder.Invention || viewModel.DocLibraryFolder == SharePointDocLibraryFolder.Application ||
                    viewModel.DocLibraryFolder == SharePointDocLibraryFolder.Trademark)
                {
                    viewModel.HasDefault = true;
                }

                var settings = await _settings.GetSetting();
                if (settings.IsSharePointIntegrationKeyFieldsOnly)
                {
                    viewModel.Author = driveItem.CreatedBy.User.AdditionalData["email"].ToString();
                    viewModel.FileName = driveItem.Name;
                    viewModel.CreatedBy = User.GetUserName();
                    await _sharePointViewModelService.SyncToDocumentTablesInit(viewModel);
                }
                else
                {
                    var fields = driveItem.ListItem.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    if (fields.ContainsKey("IsDefault"))
                    {
                        viewModel.IsDefault = Convert.ToBoolean(fields.GetValueOrDefault("IsDefault").ToString());
                        viewModel.IsDefaultPrev = viewModel.IsDefault;
                    }
                    if (fields.ContainsKey("IsPrintOnReport"))
                    {
                        viewModel.IsPrintOnReport = Convert.ToBoolean(fields.GetValueOrDefault("IsPrintOnReport").ToString());
                    }
                    if (fields.ContainsKey("IsVerified"))
                    {
                        viewModel.IsVerified = Convert.ToBoolean(fields.GetValueOrDefault("IsVerified").ToString());
                    }
                    if (fields.ContainsKey("IncludeInWorkflow"))
                    {
                        viewModel.IncludeInWorkflow = Convert.ToBoolean(fields.GetValueOrDefault("IncludeInWorkflow").ToString());
                    }
                    if (fields.ContainsKey("IsPrivate"))
                    {
                        viewModel.IsPrivate = Convert.ToBoolean(fields.GetValueOrDefault("IsPrivate").ToString());
                    }
                    if (fields.ContainsKey("Remarks"))
                    {
                        viewModel.Remarks = fields.GetValueOrDefault("Remarks").ToString();
                    }
                    if (fields.ContainsKey("IsActRequired"))
                    {
                        viewModel.IsActRequired = Convert.ToBoolean(fields.GetValueOrDefault("IsActRequired").ToString());
                    }
                    if (fields.ContainsKey("Source"))
                    {
                        viewModel.Source = fields.GetValueOrDefault("Source").ToString();
                    }
                    if (fields.ContainsKey("CheckAct"))
                    {
                        viewModel.CheckAct = Convert.ToBoolean(fields.GetValueOrDefault("CheckAct").ToString());
                    }
                    if (fields.ContainsKey("SendToClient"))
                    {
                        viewModel.SendToClient = Convert.ToBoolean(fields.GetValueOrDefault("SendToClient").ToString());
                    }
                }

                var docDocument = await _documentService.DocDocuments.AsNoTracking().Where(d => d.DocFile.DriveItemId == viewModel.DriveItemId).FirstOrDefaultAsync();
                if (docDocument != null)
                {
                    viewModel.DefaultRespDocketings = await _documentService.GetDocRespDocketingList(docDocument.DocId);
                    viewModel.DefaultRespReportings = await _documentService.GetDocRespReportingList(docDocument.DocId);
                }                    

                return PartialView("_ModifyFile", viewModel);
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> ModifyFile(SharePointDocumentEntryViewModel viewModel)
        {
            var patSettings = await _patSettings.GetSetting();
            var tmkSettings = await _tmkSettings.GetSetting();
            var gmSettings = await _gmSettings.GetSetting();
            var settings = await _settings.GetSetting();

            var fileName = "";
            IFormFile formFile = null;
            if (viewModel.UploadedFiles != null && viewModel.Type == "File")
            {
                formFile = viewModel.UploadedFiles.First();
                if (formFile.Length <= 0 && viewModel.Type == "File")
                    return BadRequest(_localizer["File uploaded has zero length."].ToString());
                fileName = formFile.FileName;

            }

            var userName = User.GetUserName();
            var author = "";

            if (!string.IsNullOrEmpty(viewModel.DocName))
            {
                fileName = viewModel.DocName;
                if (settings.SharePointInvalidCharacters.Any(s => fileName.Contains(s)))
                {
                    return BadRequest(_localizer[$"The file name {fileName} should not contain any of the invalid SharePoint characters"].Value + "  " + $"{settings.SharePointInvalidCharacters}");
                }
            }

            if (viewModel.Type == "Link")
            {
                if (string.IsNullOrEmpty(viewModel.DocName) || string.IsNullOrEmpty(viewModel.DocUrl))
                    return BadRequest(_localizer["Document Name and URL should not be empty."]);
                fileName = viewModel.DocName + ".url";
            }

            var graphClient = _sharePointService.GetGraphClient();
            if ((viewModel.UploadedFiles != null && viewModel.UploadedFiles.Any() && viewModel.Type == "File") ||
                (viewModel.Type == "Link" && viewModel.OrigDocUrl != viewModel.DocUrl))
            {

                using (var stream = new MemoryStream())
                {
                    if (viewModel.Type == "File")
                    {
                        formFile.CopyTo(stream);
                    }
                    else
                    {
                        var link = $"[InternetShortcut]{System.Environment.NewLine}URL={viewModel.DocUrl}";
                        stream.Write(Encoding.UTF8.GetBytes(link));
                    }
                    stream.Position = 0;

                    var driveItem = await graphClient.UpdateSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, viewModel.DocLibrary, viewModel.DriveItemId, stream);
                    if (driveItem != null)
                    {
                        author = driveItem.CreatedBy.User.AdditionalData["email"].ToString();
                    }
                }
            }

            var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;
            var list = site.Lists.Where(l => l.Name == viewModel.DocLibrary).FirstOrDefault();

            if (list != null)
            {
                //we need to unmark other image as default
                if (viewModel.IsDefault && !viewModel.IsDefaultPrev)
                {
                    if (!settings.IsSharePointIntegrationKeyFieldsOnly)
                    {
                        if (settings.IsSharePointIntegrationByMetadataOn)
                        {
                            await graphClient.UnmarkDefaultImageByMetadata(site.Id, list.Id, viewModel.DocLibraryFolder, viewModel.RecKey);
                        }
                        else
                        {
                            var folders = SharePointViewModelService.GetDocumentFolders(viewModel.DocLibraryFolder, viewModel.RecKey);
                            await graphClient.UnmarkDefaultImage(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, viewModel.DocLibrary, folders, site.Id, list.Id);
                        }
                    }
                }

                // Move file to "Dockets for Verification" folder if CheckAct is checked and DocVerification is on
                // only apply to files from CtryApplication/Trademark/GeneralMatter and when CheckAct value changes
                // if CheckDocket is checked move to "Dockets for Verification" folder
                // else move to default folder
                if (!string.IsNullOrEmpty(viewModel.DocLibrary) && !string.IsNullOrEmpty(viewModel.DocLibraryFolder) 
                    && ((patSettings.IsDocumentVerificationOn && viewModel.DocLibrary.ToLower() == SharePointDocLibrary.Patent.ToLower() 
                        && viewModel.DocLibraryFolder.ToLower() == SharePointDocLibraryFolder.Application.ToLower()) 
                    || (tmkSettings.IsDocumentVerificationOn && viewModel.DocLibrary.ToLower() == SharePointDocLibrary.Trademark.ToLower() 
                        && viewModel.DocLibraryFolder.ToLower() == SharePointDocLibraryFolder.Trademark.ToLower()) 
                    || (gmSettings.IsDocumentVerificationOn && viewModel.DocLibrary.ToLower() == SharePointDocLibrary.GeneralMatter.ToLower() 
                        && viewModel.DocLibraryFolder.ToLower() == SharePointDocLibraryFolder.GeneralMatter.ToLower())))
                {
                    var docVerificationFolderName = string.Empty;
                    switch (viewModel.DocLibraryFolder)
                    {
                        case SharePointDocLibraryFolder.Application:
                            docVerificationFolderName = patSettings.DocVerificationDefaultFolderName;
                            break;
                        case SharePointDocLibraryFolder.Trademark:
                            docVerificationFolderName = tmkSettings.DocVerificationDefaultFolderName;
                            break;
                        case SharePointDocLibraryFolder.GeneralMatter:
                            docVerificationFolderName = gmSettings.DocVerificationDefaultFolderName;
                            break;
                    }

                    if (string.IsNullOrEmpty(docVerificationFolderName)) docVerificationFolderName = "Dockets for Verification";
                    
                    var drive = (await graphClient.GetSiteByPath(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName)).Drives.FirstOrDefault(d => d.Name == viewModel.DocLibrary);

                    if (drive != null)
                    {
                        var currentCheckAct = await _documentService.DocDocuments.AsNoTracking()
                        .Where(d => d.DocFile != null && d.DocFile.DriveItemId == viewModel.DriveItemId)
                        .Select(d => d.CheckAct).FirstOrDefaultAsync();

                        var driveItem = await graphClient.Drives[drive.Id].Items[viewModel.DriveItemId].Request().GetAsync();
                        var parentReference = driveItem.ParentReference;
                        var tempFolders = new List<string>();
                        var moveItem = false;                      

                        if (viewModel.CheckAct && currentCheckAct != viewModel.CheckAct && !parentReference.Path.Contains(docVerificationFolderName))
                        {
                            //Move file to Dockets for Verification folder
                            tempFolders = SharePointViewModelService.GetDocumentFolders(viewModel.DocLibraryFolder, viewModel.RecKey);
                            tempFolders.Add(docVerificationFolderName);
                            moveItem = true;                            
                        }
                        else if (!viewModel.CheckAct && currentCheckAct != viewModel.CheckAct && parentReference.Path.Contains(docVerificationFolderName))
                        {
                            //Move file to parent folder
                            tempFolders = SharePointViewModelService.GetDocumentFolders(viewModel.DocLibraryFolder, viewModel.RecKey);
                            moveItem = true;
                        }

                        if (moveItem)
                        {
                            DriveItem? targetFolderDrive = null;
                            try
                            {
                                targetFolderDrive = await graphClient.Drives[drive.Id].Root.ItemWithPath(string.Join("/", tempFolders)).Request().GetAsync();
                            }
                            catch (ServiceException ex)
                            {
                                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                                {
                                    targetFolderDrive = await graphClient.CreateSiteLibraryFolder(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, viewModel.DocLibrary, tempFolders);
                                }
                            }

                            if (targetFolderDrive != null)
                            {
                                var tempRequestBody = new DriveItem
                                {
                                    ParentReference = new Microsoft.Graph.ItemReference
                                    {
                                        Id = targetFolderDrive.Id
                                    }
                                };

                                try
                                {
                                    await graphClient.Drives[drive.Id].Items[viewModel.DriveItemId].Request().UpdateAsync(tempRequestBody);                                    
                                }
                                catch (ServiceException ex)
                                {
                                    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                                    {
                                        //move successfully    
                                    }                                        
                                } 
                            } 
                        }
                    }                   
                }

                var listItemId = viewModel.ListItemId;

                if (string.IsNullOrEmpty(listItemId) || string.IsNullOrEmpty(fileName) || !fileName.Contains("."))
                {
                    var drive = (await graphClient.GetSiteByPath(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName)).Drives.FirstOrDefault(d => d.Name == viewModel.DocLibrary);
                    var driveItem = await graphClient.Drives[drive.Id].Items[viewModel.DriveItemId].Request().Expand("listItem").GetAsync();
                    if (driveItem != null)
                    {
                        listItemId = driveItem.ListItem.Id;
                        if (string.IsNullOrEmpty(fileName))
                        {
                            fileName = driveItem.Name;
                        }
                        else if (!fileName.Contains("."))
                        {
                            fileName = fileName + Path.GetExtension(driveItem.Name);
                        }
                    }
                }

                var sync = new SharePointSyncToDocViewModel
                {
                    DocLibrary = viewModel.DocLibrary,
                    DocLibraryFolder = viewModel.DocLibraryFolder,
                    DriveItemId = viewModel.DriveItemId,
                    ParentId = viewModel.ParentId,
                    FileName = fileName,
                    CreatedBy = userName.Left(20),
                    Remarks = viewModel.Remarks,
                    //Tags = "",
                    IsImage = ImageHelper.IsImageFile(fileName),
                    IsPrivate = viewModel.IsPrivate,
                    IsDefault = viewModel.IsDefault,
                    IsPrintOnReport = viewModel.IsPrintOnReport,
                    IsVerified = viewModel.IsVerified,
                    IncludeInWorkflow = viewModel.IncludeInWorkflow,
                    IsActRequired = viewModel.IsActRequired,
                    CheckAct = viewModel.CheckAct,
                    SendToClient = viewModel.SendToClient,
                    Source = viewModel.Source ?? DocumentSourceType.Manual,
                    Author = author
                };
                await _sharePointViewModelService.SyncToDocumentTables(sync);

                var requestBody = new FieldValueSet
                {
                    AdditionalData = new Dictionary<string, object>()
                };

                if (SharePointViewModelService.IsSharePointIntegrationHasSyncField)
                {
                    requestBody.AdditionalData.Add("CPISyncCompleted", true);
                }

                if (!settings.IsSharePointIntegrationKeyFieldsOnly)
                {
                    requestBody.AdditionalData.Add("Remarks", viewModel.Remarks ?? "");
                    requestBody.AdditionalData.Add("IsPrintOnReport", viewModel.IsPrintOnReport);
                    requestBody.AdditionalData.Add("IsDefault", viewModel.IsDefault);
                    requestBody.AdditionalData.Add("IsPrivate", viewModel.IsPrivate);
                    requestBody.AdditionalData.Add("IsVerified", viewModel.IsVerified);
                    requestBody.AdditionalData.Add("IncludeInWorkflow", viewModel.IncludeInWorkflow);

                    requestBody.AdditionalData.Add("IsActRequired", viewModel.IsActRequired);
                    requestBody.AdditionalData.Add("CheckAct", viewModel.CheckAct);
                    requestBody.AdditionalData.Add("SendToClient", viewModel.SendToClient);
                    requestBody.AdditionalData.Add("Source", viewModel.Source ?? DocumentSourceType.Manual);
                }

                if (viewModel.DocName != viewModel.OrigDocName)
                {
                    requestBody.AdditionalData.Add("FileLeafRef", fileName);
                }
                if (requestBody.AdditionalData.Count > 0)
                    await graphClient.Sites[site.Id].Lists[list.Id].Items[listItemId].Fields.Request().UpdateAsync(requestBody);
                                
                var docDocument = await _documentService.DocDocuments.AsNoTracking().Where(d => d.DocFile.DriveItemId == viewModel.DriveItemId).FirstOrDefaultAsync();

                //Responsible Docketing
                var hasNewRespDocketing = false;
                var hasRespDocketingReassigned = false;
                if (docDocument != null)
                {
                    var tempVM = new DocDocumentViewModel() { DocId = docDocument.DocId, RespDocketings = viewModel.RespDocketings };
                    if (viewModel.RespDocketings != null && viewModel.RespDocketings.Length == 1)
                        tempVM.RespDocketings = viewModel.RespDocketings[0].Split(",").Where(d => !string.IsNullOrEmpty(d)).ToArray();

                    var respDocketing = await _docViewModelService.SaveRespDocketing(tempVM, User.GetUserName());
                    hasNewRespDocketing = respDocketing.IsNew;
                    hasRespDocketingReassigned = respDocketing.IsReassigned;
                }

                //Responsible Reporting
                var hasNewRespReporting = false;
                var hasRespReportingReassigned = false;
                if (docDocument != null)
                {
                    var tempVM = new DocDocumentViewModel() { DocId = docDocument.DocId, RespReportings = viewModel.RespReportings };
                    if (viewModel.RespReportings != null && viewModel.RespReportings.Length == 1)
                        tempVM.RespReportings = viewModel.RespReportings[0].Split(",").Where(d => !string.IsNullOrEmpty(d)).ToArray();

                    var respReporting = await _docViewModelService.SaveRespReporting(tempVM, User.GetUserName());
                    hasNewRespReporting = respReporting.IsNew;
                    hasRespReportingReassigned = respReporting.IsReassigned;
                }

                if (patSettings.IsDocumentVerificationOn || tmkSettings.IsDocumentVerificationOn || gmSettings.IsDocumentVerificationOn)
                {
                    var docVerifications = await _documentService.DocVerifications.Where(d => d.DocDocument.DocFile.DriveItemId == viewModel.DriveItemId).ToListAsync();
                    if (docVerifications.Any() && viewModel.IsActRequired == false && docDocument != null)
                    {
                        //Delete existing DocVerification records linked to DocId if IsActRequired is unchecked
                        await _documentService.UpdateDocVerifications(docDocument.DocId, userName, new List<DocVerification>(), new List<DocVerification>(), docVerifications);
                    }
                }

                var attachments = new List<WorkflowEmailAttachmentViewModel>();
                attachments.Add(new WorkflowEmailAttachmentViewModel
                {
                    Id = viewModel.DriveItemId,
                    FileName = !string.IsNullOrEmpty(fileName) ? fileName : "",
                    DocParent = viewModel.ParentId,
                    DocDate = DateTime.Now
                });

                var workflowHeader = await GenerateWorkflow(viewModel.DocLibrary, viewModel.DocLibraryFolder, attachments, true, hasNewRespDocketing, hasRespDocketingReassigned, hasNewRespReporting, hasRespReportingReassigned);
                var emailWorkflows = GenerateEmailWorkflow(workflowHeader, attachments, viewModel.ParentId);

                //DocVerification - process new action workflows                    
                var hasDocVerificationEmailWorkflows = new List<WorkflowEmailViewModel>();
                if (docDocument != null && docDocument.DocId > 0) hasDocVerificationEmailWorkflows = await ProcessDocVerificationNewActWorkflow(docDocument.DocId);

                if (hasDocVerificationEmailWorkflows != null && hasDocVerificationEmailWorkflows.Count > 0) emailWorkflows.AddRange(hasDocVerificationEmailWorkflows);

                if (emailWorkflows != null)
                {
                    var emailUrl = "";
                    if (emailWorkflows != null && emailWorkflows.Any())
                        emailUrl = emailWorkflows.First().emailUrl;

                    return Json(new { id = viewModel.ParentId, sendEmail = true, folderId = viewModel.FolderId, emailUrl, emailWorkflows });
                }

                var success = _localizer["File has been modified successfully"].ToString();
                return Ok(new { success = success });
            }
            return BadRequest();
        }
        #region Document Tags
        public async Task<IActionResult> GetDocumentTags(string docLibrary)
        {
            var graphClient = _sharePointService.GetGraphClient();
            var list = await graphClient.GetDocLibraryTags(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary);
            var distinctListItem = list.Distinct().ToList();
            var items = distinctListItem.Select(i => i.Trim().Split(";")).SelectMany(i => i).Distinct().ToList();

            var tags = items.AsQueryable().OrderBy(t => t);
            return Json(tags);
        }

        public async Task<IActionResult> DocumentTagsRead([DataSourceRequest] DataSourceRequest request, string docLibrary, string? driveItemId)
        {
            var result = new List<DocumentTagViewModel>();

            if (!string.IsNullOrEmpty(driveItemId))
            {
                var graphClient = _sharePointService.GetGraphClient();
                var driveItem = await graphClient.GetSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId);
                if (driveItem != null)
                {
                    var fields = driveItem.ListItem.Fields;
                    if (fields.AdditionalData.TryGetValue("CPiTags", out var objTags))
                    {
                        var tags = objTags.ToString();
                        result.AddRange(tags.Split(";").Select(t => new DocumentTagViewModel { Tag = t }));
                    }
                }
            }
            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> DocumentTagsUpdate(string docLibrary, string driveItemId, string[] remaining)
        {
            var tags = string.Join(";", remaining.Distinct());
            var graphClient = _sharePointService.GetGraphClient();
            var driveItem = await graphClient.GetSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId);
            if (driveItem != null)
            {
                var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;
                var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();

                if (list != null)
                {
                    var requestBody = new FieldValueSet
                    {
                        AdditionalData = new Dictionary<string, object>
                    {
                    {
                        "CPiTags" , tags ?? ""
                        }
                    }
                    };
                    var result = await graphClient.Sites[site.Id].Lists[list.Id].Items[driveItem.ListItem.Id].Fields.Request().UpdateAsync(requestBody);
                }
            }

            return Ok();
        }

        #endregion

        #region DeDocket
        public async Task<IActionResult> ImageAddDedocket(string documentLink, string system, string respOffice)
        {
            if (User.IsDeDocketer(system, respOffice))
            {
                var documentLinkArray = documentLink.Split("|");
                var actId = Convert.ToInt32(documentLinkArray[3]);

                var docLibrary = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(documentLinkArray[0]);
                var recKey = await _sharePointViewModelService.GetActionDueRecKey(documentLinkArray[0], actId);
                var docLibraryFolder = SharePointDocLibraryFolder.Action;

                var viewModel = new SharePointDocumentEntryViewModel
                {
                    DocLibrary = docLibrary,
                    DocLibraryFolder = docLibraryFolder,
                    RecKey = recKey
                };
                return PartialView("_ModifyFile", viewModel);
            }
            return BadRequest();
        }
        #endregion


        [Authorize(Policy = RMSAuthorizationPolicy.DecisionMaker)]
        public async Task<IActionResult> ImageAddRMS(string documentLink, int dueId, int docId)
        {
            var documentLinkArray = documentLink.Split("|");
            var actId = Convert.ToInt32(documentLinkArray[3]);

            var docLibrary = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(documentLinkArray[0]);
            var recKey = await _sharePointViewModelService.GetActionDueRecKey(documentLinkArray[0], actId);
            var docLibraryFolder = SharePointDocLibraryFolder.Action;

            var viewModel = new SharePointDocumentEntryViewModel
            {
                DocLibrary = docLibrary,
                DocLibraryFolder = docLibraryFolder,
                RecKey = recKey
            };

            ViewData["DueId"] = dueId;
            ViewData["RequiredDocId"] = docId;
            ViewData["RequiredDocs"] = Array.Empty<object>();
            ViewData["FormAction"] = "AddFileRMS";

            return PartialView("_ModifyFile", viewModel);
        }

        public async Task<IActionResult> AddFileRMS(SharePointDocumentEntryViewModel viewModel, int dueId, int requiredDocId)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (dueId == 0 || requiredDocId == 0)
                return BadRequest("Missing parameters. Unable to upload document.");

            var result = await AddFile(viewModel);

            if (((ObjectResult)result).StatusCode == 200)
            {
                //save RMSDueDoc, add RMSDueDocUploadLog
                var formFile = viewModel.UploadedFiles?.First();
                var userFileName = formFile == null ? viewModel.RecKey : formFile.FileName;
                await _rmsDueDocService.SaveUploaded(dueId, requiredDocId, User.GetUserName(), true, viewModel.FileId ?? 0, userFileName ?? "", System.Convert.FromBase64String(""));
            }

            return result;
        }

        [Authorize(Policy = ForeignFilingAuthorizationPolicy.DecisionMaker)]
        public async Task<IActionResult> ImageAddFF(string documentLink, int dueId, int docId)
        {
            var documentLinkArray = documentLink.Split("|");
            var actId = Convert.ToInt32(documentLinkArray[3]);

            var docLibrary = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(documentLinkArray[0]);
            var recKey = await _sharePointViewModelService.GetActionDueRecKey(documentLinkArray[0], actId);
            var docLibraryFolder = SharePointDocLibraryFolder.Action;

            var viewModel = new SharePointDocumentEntryViewModel
            {
                DocLibrary = docLibrary,
                DocLibraryFolder = docLibraryFolder,
                RecKey = recKey
            };

            ViewData["DueId"] = dueId;
            ViewData["RequiredDocId"] = docId;
            ViewData["RequiredDocs"] = Array.Empty<object>();
            ViewData["FormAction"] = "AddFileFF";

            return PartialView("_ModifyFile", viewModel);
        }

        public async Task<IActionResult> AddFileFF(SharePointDocumentEntryViewModel viewModel, int dueId, int requiredDocId)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (dueId == 0 || requiredDocId == 0)
                return BadRequest("Missing parameters. Unable to upload document.");

            var result = await AddFile(viewModel);

            if (((ObjectResult)result).StatusCode == 200)
            {
                //save FFDueDoc, add FFDueDocUploadLog
                var formFile = viewModel.UploadedFiles?.First();
                var userFileName = formFile == null ? viewModel.RecKey : formFile.FileName;
                await _ffDueDocService.SaveUploaded(dueId, requiredDocId, User.GetUserName(), true, viewModel.FileId ?? 0, userFileName ?? "", System.Convert.FromBase64String(""));
            }

            return result;
        }

        #region Tree Events
        public async Task<ActionResult> GetApplicableDocTree(string documentLink, string? id, int? parentId = 0)
        {
            var documentLinkArray = documentLink.Split(SharePointSeparator.Folder);
            var docLibrary = documentLinkArray[0];
            var docLibraryFolder = documentLinkArray[1];
            var tree = new List<DocTreeDTO>();

            var recKey = "";
            if (documentLinkArray.Length > 2)
            {
                var recKeys = documentLinkArray.Skip(2).ToArray();
                recKey = String.Join(SharePointSeparator.Folder, recKeys);
            }
            if (string.IsNullOrEmpty(recKey)) return BadRequest();


            var folders = SharePointViewModelService.GetDocumentFolders(docLibraryFolder, recKey);
            var graphClient = _sharePointService.GetGraphClient();
            var docs = new List<SharePointGraphTreeViewModel>();
            recKey = recKey.Replace(SharePointSeparator.Folder, "-"); //display only

            if (id == null)
            {
                docs = await graphClient.GetDriveItemsTree(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders);
                tree.Add(new DocTreeDTO
                {
                    id = $"{documentLink}|sp-root",
                    text = recKey,
                    hasChildren = docs.Count() > 0,
                    iconClass = "fal fa-folders",
                });

                if (parentId > 0 && !SharePointViewModelService.IsSharePointIntegrationMainScreenOnly && (docLibraryFolder == SharePointDocLibraryFolder.Application || docLibraryFolder == SharePointDocLibraryFolder.Trademark || docLibraryFolder == SharePointDocLibraryFolder.GeneralMatter))
                {
                    var recKeys = await _sharePointViewModelService.GetChildrenRecKeys(docLibrary, (int)parentId);
                    foreach (var rK in recKeys)
                    {
                        var rKFolders = SharePointViewModelService.GetDocumentFolders(rK.Value, rK.Text);
                        var folderDocs = await graphClient.GetSiteDocuments(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, rKFolders);
                        if (folderDocs.Count > 0)
                        {
                            var childDocLink = SharePointViewModelService.BuildFolders(docLibrary, rK.Value, rK.Text);
                            tree.Add(new DocTreeDTO
                            {
                                id = $"{childDocLink}|sp-root",
                                text = rK.Text.Replace(SharePointSeparator.Folder, "-"),
                                hasChildren = true,
                                iconClass = "fal fa-folders",
                            });
                        }
                    }
                }
            }
            else
            {

                //root node
                if (id.Contains("sp-root"))
                {
                    var rootFolders = (id.Replace("|sp-root", "").Split(SharePointSeparator.Folder)).Skip(1).ToList();
                    var nodeFolders = SharePointViewModelService.GetDocumentFolders(rootFolders[0], String.Join(SharePointSeparator.Folder, rootFolders.Skip(1).ToList()));
                    docs = await graphClient.GetDriveItemsTree(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, nodeFolders);
                }
                else
                {
                    var driveItemId = id.Split("|")[0];
                    docs = await graphClient.GetDriveItemsTree(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId);
                }

                if (docs.Any())
                {
                    var docIcons = await _documentService.DocIcons.ToListAsync();
                    foreach (var doc in docs)
                    {
                        var treeNode = new DocTreeDTO
                        {
                            id = $"{doc.Id}|doc|user",
                            text = doc.Name,
                        };

                        if (doc.IsFolder)
                        {
                            treeNode.iconClass = "fal fa-folders";
                            treeNode.id = $"{doc.Id}|folder|user";
                            var childFolders = folders.Select(f => f).ToList();
                            childFolders.Add(doc.Name);
                            treeNode.hasChildren = await graphClient.FolderHasChildren(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, doc.Id);
                        }
                        else
                        {
                            treeNode.hasChildren = false;
                            var file = doc.Name.Split(".");
                            if (file.Length > 1)
                            {
                                var icon = docIcons.FirstOrDefault(i => i.FileExt.ToLower() == file[file.Length - 1].ToLower());
                                if (icon != null)
                                {
                                    treeNode.iconClass = icon.IconClass;
                                }
                            }
                        }
                        tree.Add(treeNode);
                    }
                }

            }

            return Json(tree);
        }


        public async Task<ActionResult> GetApplicableDocTreeByMetadata(string documentLink, string? id, int? parentId = 0)
        {
            var documentLinkArray = documentLink.Split(SharePointSeparator.Folder);
            var docLibrary = documentLinkArray[0];
            var docLibraryFolder = documentLinkArray[1];
            var tree = new List<DocTreeDTO>();

            var recKey = "";
            if (documentLinkArray.Length > 2)
            {
                var recKeys = documentLinkArray.Skip(2).ToArray();
                recKey = String.Join(SharePointSeparator.Folder, recKeys);
            }
            if (string.IsNullOrEmpty(recKey)) return BadRequest();

            var graphClient = _sharePointService.GetGraphClient();
            var docs = new List<SharePointGraphTreeViewModel>();

            if (id == null)
            {
                docs = await graphClient.GetDriveItemsTreeByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, docLibraryFolder, recKey);
                tree.Add(new DocTreeDTO
                {
                    id = $"{documentLink}|sp-root",
                    text = recKey,
                    hasChildren = docs.Count() > 0,
                    iconClass = "fal fa-folders",
                });

                if (parentId > 0 && (docLibraryFolder == SharePointDocLibraryFolder.Application || docLibraryFolder == SharePointDocLibraryFolder.Trademark || docLibraryFolder == SharePointDocLibraryFolder.GeneralMatter))
                {
                    var recKeys = await _sharePointViewModelService.GetChildrenRecKeys(docLibrary, (int)parentId);
                    foreach (var rK in recKeys)
                    {
                        var folderDocs = await graphClient.GetSiteDocumentsByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, rK.Value, rK.Text.Replace(SharePointSeparator.Folder, SharePointSeparator.Field));
                        if (folderDocs.Count > 0)
                        {
                            var childDocLink = SharePointViewModelService.BuildFolders(docLibrary, rK.Value, rK.Text);
                            tree.Add(new DocTreeDTO
                            {
                                id = $"{childDocLink}|sp-root",
                                text = rK.Text.Replace(SharePointSeparator.Folder, SharePointSeparator.Field),
                                hasChildren = true,
                                iconClass = "fal fa-folders",
                            });
                        }
                    }
                }
            }
            else
            {

                //root node
                if (id.Contains("sp-root"))
                {
                    var rootFolders = (id.Replace("|sp-root", "").Split(SharePointSeparator.Folder)).Skip(1).ToList();
                    docs = await graphClient.GetDriveItemsTreeByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, rootFolders[0], String.Join("~", rootFolders.Skip(1)));
                }
                else
                {
                    var driveItemId = id.Split("|")[0];
                    docs = await graphClient.GetDriveItemsTree(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId);
                }

                if (docs.Any())
                {
                    var docIcons = await _documentService.DocIcons.ToListAsync();
                    foreach (var doc in docs)
                    {
                        var treeNode = new DocTreeDTO
                        {
                            id = $"{doc.Id}|doc|user",
                            text = doc.Name,
                        };

                        treeNode.hasChildren = false;
                        var file = doc.Name.Split(".");
                        if (file.Length > 1)
                        {
                            var icon = docIcons.FirstOrDefault(i => i.FileExt.ToLower() == file[file.Length - 1].ToLower());
                            if (icon != null)
                            {
                                treeNode.iconClass = icon.IconClass;
                            }
                        }
                        tree.Add(treeNode);
                    }
                }
            }
            return Json(tree);
        }

        public async Task<ActionResult> AddFolder(string docLibrary, string? docLibraryFolder, string recKey, string id, string folderName)
        {
            var settings = await _settings.GetSetting();
            if (!settings.IsSharePointIntegrationByMetadataOn && settings.SharePointInvalidCharacters.Any(s => folderName.Contains(s)))
            {
                return BadRequest(_localizer[$"The folder {folderName} should not contain any of the invalid SharePoint characters"].Value + "  " + $"{settings.SharePointInvalidCharacters}");
            }

            var graphClient = _sharePointService.GetGraphClient();
            DriveItem driveItem;

            if (id.Contains("|sp-root"))
            {
                var folders = SharePointViewModelService.GetDocumentFolders(docLibraryFolder, recKey);
                folders.Add(folderName);
                driveItem = await graphClient.CreateSiteLibraryFolder(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders);
            }
            else
            {
                id = id.Split("|")[0];
                driveItem = await graphClient.CreateSiteLibraryFolder(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, id, folderName);
            }

            if (driveItem == null)
            {
                return new JsonBadRequest("Folder name exist. Please use a different name.");
            }
            var folderNode = new DocTreeDTO
            {
                //id = driveItem.Id, 
                id = driveItem.Id + "|folder|user",
                text = driveItem.Name,
                hasChildren = false
            };
            return Json(folderNode);
        }

        public async Task<ActionResult> DeleteFolderDoc(string docLibrary, string id)
        {
            var graphClient = _sharePointService.GetGraphClient();

            id = id.Split("|")[0];
            if (await graphClient.DeleteSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, id))
                return Ok();
            else
                return BadRequest(_localizer["Unable to delete the document. The file may be locked by another process. Please try again later."].ToString());

        }


        public async Task<ActionResult> RenameFolderDoc(string docLibrary, string id, string newName)
        {
            if (string.IsNullOrEmpty(newName))
                return BadRequest(_localizer["Folder name is empty"].ToString());

            var settings = await _settings.GetSetting();
            if (!settings.IsSharePointIntegrationByMetadataOn && settings.SharePointInvalidCharacters.Any(s => newName.Contains(s)))
            {
                return BadRequest(_localizer[$"The folder {newName} should not contain any of the invalid SharePoint characters"].Value + "  " + $"{settings.SharePointInvalidCharacters}");
            }

            var graphClient = _sharePointService.GetGraphClient();
            id = id.Split("|")[0];
            if (await graphClient.RenameSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, id, newName))
                return Ok();

            return BadRequest();
        }

        public async Task<IActionResult> MoveFolderDoc(string docLibrary, string sourceId, string destId)
        {
            var graphClient = _sharePointService.GetGraphClient();

            sourceId = sourceId.Split("|")[0];
            destId = destId.Split("|")[0];

            var destFolder = destId.Split(SharePointSeparator.Folder).ToList();
            destFolder.RemoveAll(d => d.ToLower() == docLibrary.ToLower());
            var destPath = string.Join("/", destFolder);

            await graphClient.MoveSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, sourceId, destPath);
            var success = _localizer["File has been moved successfully"].ToString();
            return Ok(new { success = success });
        }
        #endregion

        public async Task<IActionResult> ForSignatureRead([DataSourceRequest] DataSourceRequest request, string screenCode, int parentId)
        {
            if (ModelState.IsValid)
            {
                var images = _documentService.SharePointFileSignatures.Where(s => s.ScreenCode == screenCode && s.ParentId == parentId)
                            .Select(s => new DocDocumentListViewModel
                            {
                                UserFileName = s.FileName,
                                DocName = s.FileName,
                                DateCreated = s.DateCreated,
                                ForSignature = !s.SignatureCompleted,
                                SignatureCompleted = s.SignatureCompleted,
                                SentToDocuSign = !string.IsNullOrEmpty(s.EnvelopeId),
                                DocLibrary = s.DocLibrary,
                                DocLibraryFolder = s.DocLibraryFolder,
                                // RecKey=s.RecKey,
                                ParentId = s.ParentId,
                                EnvelopeId = s.EnvelopeId,
                                Id = s.DriveItemId,
                                ScreenCode = s.ScreenCode,
                                RoleLink = s.RoleLink,
                                SystemTypeCode = s.SystemTypeCode,
                                QESetupId = s.QESetupId
                            });
                return Json(images.ToDataSourceResult(request));
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<List<SharePointDocumentViewModel>> ApplyCriteria(IQueryable<SharePointDocumentViewModel> documents, List<QueryFilterViewModel> criteria, int parentId, string docLibrary, string? docLibraryFolder)
        {
            var results = new List<SharePointDocumentViewModel>();
            foreach (var cri in criteria)
            {
                if (cri.Property == "Folder")
                    documents = documents.Where(c => c.Folder.Contains(cri.Value));
                else if (cri.Property == "Title")
                    documents = documents.Where(c => c.Title != null && c.Title.ToLower().Contains(cri.Value.ToLower()));
                else if (cri.Property == "Name")
                    documents = documents.Where(c => c.Name.ToLower().Contains(cri.Value.ToLower()));
                else if (cri.Property == "DateModifiedFrom")
                    documents = documents.Where(c => c.DateModified >= DateTime.Parse(cri.Value));

                else if (cri.Property == "DateModifiedTo")
                    documents = documents.Where(c => c.DateModified <= DateTime.Parse(cri.Value).AddSeconds(86399));
            }

            var tag = criteria.FirstOrDefault(f => f.Property == "Tag");
            if (tag != null)
            {
                var systemType = _sharePointViewModelService.GetSystemCodeFromDocLibrary(docLibrary);
                var dataKey = _sharePointViewModelService.GetDataKeyFromDocLibraryFolder(docLibraryFolder);
                var savedDocFiles = await _documentService.DocDocuments.Where(d => d.DocFolder.SystemType == systemType && d.DocFolder.DataKey == dataKey && d.DocFolder.DataKeyValue == parentId && d.DocDocumentTags.Any(t => t.Tag.Contains(tag.Value.Replace("*", "")))).Select(d => d.DocFile).ToListAsync();
                documents = documents.Where(d => savedDocFiles.Any(f => f.DriveItemId == d.Id));
            }
            return documents.ToList();
        }

        private async Task<IActionResult> SaveDroppedFiles(IEnumerable<IFormFile> droppedFiles, string docLibrary, string? docLibraryFolder, string recKey, string folderId, int parentId, string? roleLink, bool isDefault, List<string>? responsibles, string source = DocumentSourceType.Manual)
        {
            if (droppedFiles.Count() <= 0)
            {
                return BadRequest(_localizer["No Document to upload"].ToString());
            }

            var folders = SharePointViewModelService.GetDocumentFolders(docLibraryFolder, recKey);

            var graphClient = _sharePointService.GetGraphClient();
            var attachments = new List<WorkflowEmailAttachmentViewModel>();
            var userName = User.GetUserName();
            var today = DateTime.Now;
            var docIds = new List<int>();
            var settings = await _settings.GetSetting();

            foreach (var file in droppedFiles)
            {
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    stream.Position = 0;
                    var result = new SharePointGraphDriveItemKeyViewModel();

                    if (settings.IsSharePointIntegrationByMetadataOn)
                    {
                        var existing = await graphClient.FileExists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, file.FileName);
                        if (existing)
                            return BadRequest(_localizer["File already exists. Please upload a file with different filename."].ToString());

                        result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, new List<string>(), stream, file.FileName);
                    }
                    else
                    {
                        var existing = await graphClient.FileExists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders, file.FileName);
                        if (existing)
                            return BadRequest(_localizer["File already exists. Please upload a file with different filename."].ToString());

                        if (string.IsNullOrEmpty(folderId) || folderId == "0")
                        {
                            result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders, stream, file.FileName);
                        }
                        else
                        {
                            result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folderId, stream, file.FileName);
                        }
                    }

                    if (!string.IsNullOrEmpty(result.DriveItemId))
                    {
                        attachments.Add(new WorkflowEmailAttachmentViewModel
                        {
                            Id = result.DriveItemId,
                            FileName = file.FileName,
                            DocParent = parentId,
                            DocDate = DateTime.Now
                        });

                        var driveItem = await graphClient.Drives[result.DriveId].Items[result.DriveItemId].Request().Expand("listItem").GetAsync();

                        var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;
                        var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();
                        if (list != null)
                        {
                            var sync = new SharePointSyncToDocViewModel
                            {
                                DocLibrary = docLibrary,
                                DocLibraryFolder = docLibraryFolder,
                                DriveItemId = driveItem.Id,
                                ParentId = parentId,
                                FileName = file.FileName,
                                CreatedBy = userName.Left(20),
                                Remarks = "",
                                Tags = "",
                                IsImage = driveItem.Image != null,
                                IsPrivate = false,
                                IsDefault = isDefault,
                                IsPrintOnReport = false,
                                IsVerified = false,
                                IncludeInWorkflow = false,
                                IsActRequired = false,
                                CheckAct = false,
                                SendToClient = false,
                                Source = source,
                                Author = User.GetEmail()
                            };
                            await _sharePointViewModelService.SyncToDocumentTables(sync);

                            var requestBody = new FieldValueSet
                            {
                                AdditionalData = new Dictionary<string, object>()
                            };
                            if (SharePointViewModelService.IsSharePointIntegrationHasSyncField)
                            {
                                requestBody.AdditionalData.Add("CPISyncCompleted", true);
                            }

                            if (!settings.IsSharePointIntegrationKeyFieldsOnly)
                            {
                                requestBody.AdditionalData.Add("IsDefault", isDefault);
                                requestBody.AdditionalData.Add("Source", source);
                            }

                            if (settings.IsSharePointIntegrationByMetadataOn)
                            {
                                requestBody.AdditionalData.Add("CPIScreen", docLibraryFolder);
                                requestBody.AdditionalData.Add("CPIRecordKey", recKey.Replace(SharePointSeparator.Folder, SharePointSeparator.Field));
                            }
                            if (requestBody.AdditionalData.Count > 0)
                                await graphClient.Sites[site.Id].Lists[list.Id].Items[driveItem.ListItem.Id].Fields.Request().UpdateAsync(requestBody);

                            var docDocument = await _documentService.DocDocuments.AsNoTracking().Where(d => d.DocFile.DriveItemId == driveItem.Id).FirstOrDefaultAsync();
                            if (docDocument != null)
                                docIds.Add(docDocument.DocId);

                        }
                    }
                }
            }

            var hasNewRespDocketings = false;
            if (docIds.Any())
            {
                //Add/populate tblDocResponsibles
                if (responsibles != null && responsibles.Any())
                {
                    foreach (var docId in docIds)
                    {
                        await _documentService.UpdateDocRespDocketing(responsibles, userName, docId);
                    }
                    hasNewRespDocketings = true;
                }
            }

            var workflowHeader = await GenerateWorkflow(docLibrary, docLibraryFolder, attachments, true, hasNewRespDocketings);

            var eSignatureWorkflows = new List<WorkflowSignatureViewModel>();
            if (settings.IsESignatureOn)
            {
                eSignatureWorkflows = await GenerateSignatureWorkflow(workflowHeader, docLibrary, docLibraryFolder, recKey, attachments, parentId, roleLink);
            }
            var emailWorkflows = GenerateEmailWorkflow(workflowHeader, attachments, parentId);
            if (emailWorkflows != null || eSignatureWorkflows != null)
            {
                var emailUrl = "";
                if (emailWorkflows != null && emailWorkflows.Any())
                    emailUrl = emailWorkflows.First().emailUrl;

                return Json(new { id = parentId, sendEmail = true, folderId = folderId, emailUrl, emailWorkflows, eSignatureWorkflows });
            }
            return Json(new
            {
                folderId = folderId
            });

        }

        protected async Task<WorkflowHeaderViewModel> GenerateWorkflow(string docLibrary, string? docLibraryFolder, List<WorkflowEmailAttachmentViewModel> attachments, bool isNewFileUpload, bool hasNewRespDocketing = false, bool hasRespDocketingReassigned = false, bool hasNewRespReporting = false, bool hasRespReportingReassigned = false)
        {
            switch (docLibraryFolder)
            {
                case SharePointDocLibraryFolder.Application:
                    return await GenerateCountryAppWorkflow(attachments, isNewFileUpload, hasNewRespDocketing, hasRespDocketingReassigned, hasNewRespReporting, hasRespReportingReassigned);

                case SharePointDocLibraryFolder.Trademark:
                    return await GenerateTrademarkWorkflow(attachments, isNewFileUpload, hasNewRespDocketing, hasRespDocketingReassigned, hasNewRespReporting, hasRespReportingReassigned);

                case SharePointDocLibraryFolder.GeneralMatter:
                    return await GenerateGMWorkflow(attachments, isNewFileUpload, hasNewRespDocketing, hasRespDocketingReassigned, hasNewRespReporting, hasRespReportingReassigned);

                case SharePointDocLibraryFolder.Action:
                    string systemTypeCode = "";
                    switch (docLibrary)
                    {
                        case SharePointDocLibrary.Patent:
                            systemTypeCode = SystemTypeCode.Patent;
                            break;
                        case SharePointDocLibrary.Trademark:
                            systemTypeCode = SystemTypeCode.Trademark;
                            break;
                        case SharePointDocLibrary.GeneralMatter:
                            systemTypeCode = SystemTypeCode.GeneralMatter;
                            break;
                    }
                    if (!string.IsNullOrEmpty(systemTypeCode))
                    {
                        return await GenerateActionWorkflow(systemTypeCode, attachments);
                    }
                    return null;
            }
            return null;
        }

        protected async Task<List<WorkflowSignatureViewModel>> GenerateSignatureWorkflow(WorkflowHeaderViewModel workflowHeader, string docLibrary, string? docLibraryFolder, string recKey, List<WorkflowEmailAttachmentViewModel> attachments, int parentId, string? roleLink)
        {
            if (workflowHeader != null && workflowHeader.Workflows != null)
            {
                //Same enum setup for all systems
                var eSignatureWorkflows = workflowHeader.Workflows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.eSignature).ToList();
                if (eSignatureWorkflows.Any())
                {
                    var systemTypeCode = "";
                    switch (docLibrary)
                    {
                        case SharePointDocLibrary.Patent:
                            systemTypeCode = SystemTypeCode.Patent;
                            break;
                        case SharePointDocLibrary.Trademark:
                            systemTypeCode = SystemTypeCode.Trademark;
                            break;
                        case SharePointDocLibrary.GeneralMatter:
                            systemTypeCode = SystemTypeCode.GeneralMatter;
                            break;
                        case SharePointDocLibrary.PatClearance:
                            systemTypeCode = SystemTypeCode.PatClearance;
                            break;
                        case SharePointDocLibrary.DMS:
                            systemTypeCode = SystemTypeCode.DMS;
                            break;
                        case SharePointDocLibrary.TmkRequest:
                            systemTypeCode = SystemTypeCode.Clearance;
                            break;
                    }

                    var screenCode = "";
                    switch (docLibraryFolder)
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
                    }

                    var wfs = new List<WorkflowSignatureViewModel>();
                    var settings = await _settings.GetSetting();

                    foreach (var wf in eSignatureWorkflows)
                    {
                        if (wf.Attachments != null && wf.Attachments.Any())
                        {
                            foreach (var attachment in wf.Attachments)
                            {
                                if (!settings.IsESignatureReviewOn)
                                {
                                    var pos = attachment.FileName.LastIndexOf(".");
                                    var fileName = attachment.FileName.Substring(0, pos);

                                    wfs.Add(new WorkflowSignatureViewModel
                                    {
                                        QESetupId = wf.ActionValueId,
                                        ParentId = parentId,
                                        UserFile = new WorkflowSignatureDocViewModel { Name = fileName, FileName = attachment.FileName, StrId = attachment.Id },
                                        ScreenCode = screenCode,
                                        RoleLink = roleLink,
                                        SystemTypeCode = systemTypeCode,
                                        SharePointDocLibrary = docLibrary,
                                    });
                                }
                                await _documentService.MarkSharePointFileForSignature(attachment.Id, docLibrary, docLibraryFolder, recKey, screenCode, wf.ActionValueId, attachment.FileName, attachment.DocDate, attachment.DocParent, systemTypeCode, roleLink);
                            }
                        }
                    }
                    if (wfs.Any())
                        return wfs;
                }
            }
            return null;
        }

        private string GetMimeTypeForFileExtension(string filePath)
        {
            const string DefaultContentType = "application/octet-stream";

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(filePath, out string contentType))
            {
                contentType = DefaultContentType;
            }

            return contentType;
        }

        private async Task<bool> IsUserRestrictedFromPrivateDocuments()
        {
            var userAccountSettings = await _userSettingManager.GetUserSetting<UserAccountSettings>(User.GetUserIdentifier());
            if (userAccountSettings?.RestrictPrivateDocuments == null)
                return false;
            return userAccountSettings.RestrictPrivateDocuments;
        }

        private string GetSystemName(string systemTypeCode)
        {
            return systemTypeCode == SystemTypeCode.Patent ? SystemType.Patent : systemTypeCode == SystemTypeCode.Trademark ? SystemType.Trademark : systemTypeCode == SystemTypeCode.GeneralMatter ? "General Matter" : "";
        }

        #region Letters
        public async Task<IActionResult> LetterTemplateGridRead([DataSourceRequest] DataSourceRequest request, string sys)
        {
            var canAccess = await LetterHelper.CanAccessLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var systemFolder = GetSystemName(sys);
            var graphClient = _sharePointService.GetGraphClient();
            var templates = await graphClient.GetSiteDocuments(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.LetterTemplates, new List<string> { systemFolder });
            var files = templates.Select(t => new LetterTemplateViewModel
            {
                TemplateFile = t.DriveItem.Name,
                FileSize = t.DriveItem.Size.HasValue ? (long)t.DriveItem.Size : 0,
                Id = t.DriveItem.Id,
                SystemType = sys
            }).ToList();
            return Json(files.ToDataSourceResult(request));
        }

        [HttpPost]
        public async Task<IActionResult> LetterTemplateSave(IEnumerable<IFormFile> droppedFiles, string sys, bool overwriteExisting)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            // return custom error messages via Content
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            var uploadedFile = droppedFiles.First();
            var fileName = Path.GetFileName(uploadedFile.FileName);

            var systemFolder = GetSystemName(sys);
            var graphClient = _sharePointService.GetGraphClient();
            var existing = await graphClient.GetSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.LetterTemplates, systemFolder, fileName);

            if (existing != null && !overwriteExisting)
                return Content(_localizer["The template file exists. Please check 'Overwrite if existing' to overwrite previously uploaded template."].ToString());

            if (existing != null)
            {
                await graphClient.DeleteSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.LetterTemplates, existing.Id);
            }

            using (var stream = new MemoryStream())
            {
                uploadedFile.CopyTo(stream);
                stream.Position = 0;
                var result = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.LetterTemplates, new List<string> { systemFolder }, stream, fileName);
                return Ok(new { success = _localizer["The template file has been uploaded."].ToString() });
            }
        }

        public async Task<IActionResult> LetterTemplateGridDelete([Bind(Prefix = "deleted")] LetterTemplateViewModel deletedTemplate)
        {
            var canUpdate = await LetterHelper.CanUpdateLetter(deletedTemplate.SystemType, User, _authService);
            Guard.Against.NoRecordPermission(canUpdate);

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            var inUseByLetter = _letterService.LettersMain.Where(l => l.TemplateFile == deletedTemplate.TemplateFile && l.SystemScreen.SystemType == deletedTemplate.SystemType).Any();

            if (inUseByLetter)
                return new JsonBadRequest(_localizer["You cannot delete this template, it is used by one of the letters."].ToString());

            var systemFolder = GetSystemName(deletedTemplate.SystemType);
            var graphClient = _sharePointService.GetGraphClient();
            await graphClient.DeleteSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.LetterTemplates, deletedTemplate.Id);

            return Ok(new { success = _localizer["The template file has been successfully deleted."].ToString() });

        }

        public async Task<IActionResult> LetterTemplateGridDownload(string sys, string templateFile, string id)
        {
            var canAccess = await LetterHelper.CanAccessLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var systemFolder = GetSystemName(sys);

            var graphClient = _sharePointService.GetGraphClient();
            var stream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.LetterTemplates, id);
            if (stream != null)
            {
                var mimeType = GetMimeTypeForFileExtension(templateFile);
                return new FileStreamResult(stream, mimeType) { FileDownloadName = templateFile };
            }
            return BadRequest();
        }

        public async Task<IActionResult> LetterTemplateGetEditUrl(string sys, string templateFile)
        {
            var canAccess = await LetterHelper.CanAccessLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var systemFolder = GetSystemName(sys);

            var graphClient = _sharePointService.GetGraphClient();
            var result = await graphClient.GetSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.LetterTemplates, systemFolder, templateFile);
            return Json(new { editUrl = result.WebUrl });
        }

        public async Task<IActionResult> GetLetterTemplateList(string sys)
        {
            var canAccess = await LetterHelper.CanAccessLetter(sys, User, _authService);
            Guard.Against.NoRecordPermission(canAccess);

            var systemFolder = GetSystemName(sys);
            var graphClient = _sharePointService.GetGraphClient();
            var templates = await graphClient.GetSiteDocuments(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.LetterTemplates, new List<string> { systemFolder });
            var files = templates.Select(t => new LetterTemplateViewModel
            {
                TemplateFile = t.DriveItem.Name,
            }).ToList();

            var pickList = new List<LookupDTO>();
            files.Each(f => pickList.Add(new LookupDTO { Value = f.TemplateFile }));

            if (sys == "P")
            {
                if (!_patSettings.GetSetting().Result.IsInventorRemunerationOn)
                {
                    pickList.RemoveAll(c => c.Value.StartsWith("Pat-Remuneration"));
                }
                if (!_patSettings.GetSetting().Result.IsInventorFRRemunerationOn)
                {
                    pickList.RemoveAll(c => c.Value.StartsWith("Pat-FRRemuneration"));
                }
            }
            return Json(pickList);
        }
        #endregion

        #region reports
        public async Task<List<SharePointReportImage>> GetReportImages(string data)
        {
            return await _sharePointViewModelService.GetReportImages(data);
        }

        public async Task<IActionResult> GetReportImageFile(string system, string fileName, string itemId)
        {
            if (itemId.StartsWith("logo_"))
            {
                var filePath = "wwwroot/images/" + itemId;
                byte[] bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, ImageHelper.GetContentType(itemId), itemId);
            }
            else
            {
                var graphClient = _sharePointService.GetGraphClient();
                var docLibrary = _sharePointViewModelService.GetDocLibrary(system);

                return await DownloadFile(docLibrary, fileName, itemId);
            }
        }

        #endregion

        protected override async Task<IActionResult> SaveDroppedEmails(IEnumerable<IFormFile> droppedFiles, string documentLink, string folderId, string? roleLink, List<string>? responsibles)
        {
            var docLink = documentLink.Split("|"); //SystemType|ScreenCode|DataKey|DataKeyValue
            var recordId = int.Parse(docLink[3]);
            var docLibrary = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(docLink[0]);
            var docFolder = await _sharePointViewModelService.GetFolders(docLink[1], recordId, docLink[0]);
            var docLibraryFolder = docFolder.Folder ?? "";
            var recKey = docFolder.RecKey ?? "";
            var parentId = recordId;

            return await SaveDroppedFiles(droppedFiles, docLibrary, docLibraryFolder, recKey, folderId, parentId, roleLink, false, responsibles, DocumentSourceType.CPIMail);
        }

        #region Document Verification        
        [HttpGet]
        public IActionResult AddSPFile(string driveItemId)
        {
            //From New Document tab in Document Verification
            var viewModel = new SharePointDocumentEntryViewModel
            {
                DocLibrary = SharePointDocLibrary.Orphanage,
                ParentId = 0
            };
            ViewData["IsNewDocVerification"] = true;
            return PartialView("_ModifyFile", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ModifySPFile(string driveItemId)
        {
            var docDocument = await _documentService.DocDocuments.AsNoTracking().Where(d => d.DocFile.DriveItemId == driveItemId)
                                        .Select(d => new
                                        {
                                            d.DocId,
                                            d.DocFolder.DataKeyValue,
                                            d.DocFolder.SystemType,
                                            d.DocFolder.ScreenCode
                                        }).FirstOrDefaultAsync();
            if (docDocument == null) return BadRequest(_localizer["File not found."]);

            var docLibrary = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(docDocument.SystemType);
            var docLibraryFolder = _sharePointViewModelService.GetDocLibraryFolderFromScreenCode(docDocument.ScreenCode);

            var recKey = await _sharePointViewModelService.GetRecKey(docLibrary, docLibraryFolder, docDocument.DataKeyValue);

            var graphClient = _sharePointService.GetGraphClient();
            var driveItem = await graphClient.GetSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId);

            if (driveItem != null)
            {
                var viewModel = new SharePointDocumentEntryViewModel
                {
                    DocLibrary = docLibrary,
                    DocLibraryFolder = docLibraryFolder,
                    RecKey = recKey,
                    DriveItemId = driveItemId,
                    IsImage = driveItem.File.MimeType.Contains("image"),
                    ParentId = docDocument.DataKeyValue
                };

                if (viewModel.DocLibraryFolder == SharePointDocLibraryFolder.Invention || viewModel.DocLibraryFolder == SharePointDocLibraryFolder.Application ||
                    viewModel.DocLibraryFolder == SharePointDocLibraryFolder.Trademark)
                {
                    viewModel.HasDefault = true;
                }

                var fields = driveItem.ListItem.Fields.AdditionalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                if (fields.ContainsKey("IsDefault"))
                {
                    viewModel.IsDefault = Convert.ToBoolean(fields.GetValueOrDefault("IsDefault").ToString());
                }
                if (fields.ContainsKey("IsPrintOnReport"))
                {
                    viewModel.IsPrintOnReport = Convert.ToBoolean(fields.GetValueOrDefault("IsPrintOnReport").ToString());
                }
                if (fields.ContainsKey("IsVerified"))
                {
                    viewModel.IsVerified = Convert.ToBoolean(fields.GetValueOrDefault("IsVerified").ToString());
                }
                if (fields.ContainsKey("IncludeInWorkflow"))
                {
                    viewModel.IncludeInWorkflow = Convert.ToBoolean(fields.GetValueOrDefault("IncludeInWorkflow").ToString());
                }
                if (fields.ContainsKey("IsPrivate"))
                {
                    viewModel.IsPrivate = Convert.ToBoolean(fields.GetValueOrDefault("IsPrivate").ToString());
                }
                if (fields.ContainsKey("Remarks"))
                {
                    viewModel.Remarks = fields.GetValueOrDefault("Remarks").ToString();
                }
                if (fields.ContainsKey("CPiTags"))
                {
                    viewModel.Tags = fields.GetValueOrDefault("CPiTags").ToString();
                }
                if (fields.ContainsKey("IsActRequired"))
                {
                    viewModel.IsActRequired = Convert.ToBoolean(fields.GetValueOrDefault("IsActRequired").ToString());
                }
                if (fields.ContainsKey("CheckAct"))
                {
                    viewModel.CheckAct = Convert.ToBoolean(fields.GetValueOrDefault("CheckAct").ToString());
                }
                if (fields.ContainsKey("SendToClient"))
                {
                    viewModel.SendToClient = Convert.ToBoolean(fields.GetValueOrDefault("SendToClient").ToString());
                }

                viewModel.DefaultRespDocketings = await _documentService.GetDocRespDocketingList(docDocument.DocId);
                viewModel.DefaultRespReportings = await _documentService.GetDocRespReportingList(docDocument.DocId);

                if (docLibrary == SharePointDocLibrary.Orphanage)
                {
                    ViewData["IsNewDocVerification"] = true;
                }

                // Get temporary file for viewing
                var tempFileName = (driveItem.CTag ?? "").ReplaceInvalidFilenameChars() + "_" + driveItem.Name.ReplaceInvalidFilenameChars();
                viewModel.ViewFilePath = await PrepareTemporaryFile(graphClient, docLibrary, driveItemId, tempFileName);                                

                return PartialView("_ModifyFileZoom", viewModel);
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> SaveSearchLink(string ids, List<DocVerificationSearchLinkViewModel> selectedRecords)
        {
            if (string.IsNullOrEmpty(ids) || !selectedRecords.Any()) return Ok();

            var graphClient = _sharePointService.GetGraphClient();
            var site = graphClient.GetSiteWithLists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName).Result;
            var sourceDrive = (await graphClient.GetSiteByPath(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName)).Drives.FirstOrDefault(d => d.Name == SharePointDocLibrary.Orphanage);

            if (sourceDrive == null)
                return BadRequest();

            var emailWorkflows = new List<WorkflowEmailViewModel>();
            var hasNewRespDocketing = false;
            var hasNewRespReporting = false;

            var userName = User.GetUserName();
            var userEmail = User.GetEmail();
            var spFileIdList = ids.Split("|").Where(d => !string.IsNullOrEmpty(d)).ToList();
            var settings = await _settings.GetSetting();

            var newDriveItemIdList = new List<SharePointDocumentEntryViewModel>();

            foreach (var recArr in selectedRecords)
            {
                var newDriveItemIds = new List<string>();
                hasNewRespDocketing = false;
                hasNewRespReporting = false;

                if (recArr.RespDocketings != null && recArr.RespDocketings.Length > 0)
                    hasNewRespDocketing = true;

                if (recArr.RespReportings != null && recArr.RespReportings.Length > 0)
                    hasNewRespReporting = true;

                var documentLink = await GetDocumentLink(recArr.Link ?? "");
                if (!await _docViewModelService.CanModifyDocument(documentLink))
                    continue;

                var docLibrary = "";
                var docLibraryFolder = "";
                var recId = 0;

                GetDocLibrary(documentLink, ref docLibrary, ref docLibraryFolder, ref recId);

                var spFileList = await _documentService.DocDocuments.AsNoTracking()
                                            .Where(d => d.DocFile != null && spFileIdList.Contains(d.DocFile.DriveItemId ?? ""))
                                            .Select(d => new
                                            {
                                                DocId = d.DocId,
                                                DriveItemId = d.DocFile != null ? d.DocFile.DriveItemId : "",
                                                FileName = d.DocName,
                                                DocLibrary = docLibrary,
                                                DocLibraryFolder = docLibraryFolder,
                                                ParentId = recId,
                                                IsActRequired = recArr.IsActRequired,
                                                CreatedBy = userName,
                                                UpdatedBy = userName,
                                                DateCreated = DateTime.Now,
                                                LastUpdate = DateTime.Now,
                                                Source = d.Source ?? DocumentSourceType.Manual
                                            }).ToListAsync();


                //Get record folder driveId               
                var list = site.Lists.Where(l => l.Name == docLibrary).FirstOrDefault();
                var recKey = await _sharePointViewModelService.GetRecKey(docLibrary, docLibraryFolder ?? "", recId);
                var folders = SharePointViewModelService.GetDocumentFolders(docLibraryFolder, recKey);
                //Try to update folderId if folder exists first before doing upload-delete
                //Copy from Orphanage library to record library
                //Get stream of file to be copy and upload/create new file(s)
                //Not using Copy method since have to check for copy status and get DriveItemId
                //https://learn.microsoft.com/en-us/graph/api/driveitem-copy?view=graph-rest-1.0&tabs=csharp
                //https://learn.microsoft.com/en-us/graph/long-running-actions-overview?tabs=csharp
                foreach (var spFile in spFileList)
                {
                    //Update item folder
                    DriveItem? targetFolderDrive = null;  
                    Drive? targetDrive = null;
                    try 
                    {
                        targetDrive = (await graphClient.GetSiteByPath(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName)).Drives.FirstOrDefault(d => d.Name == docLibrary);
                        if (targetDrive == null) throw new Exception("Folder drive not found.");

                        targetFolderDrive = await graphClient.Drives[targetDrive.Id].Root.ItemWithPath(string.Join("/", folders)).Request().GetAsync();
                    }
                    catch (Exception)
                    {
                        //ignore if folder not found
                    }
                    var isMoveSuccess = false;
                    if (targetFolderDrive != null && targetDrive != null)
                    {
                        var requestBody = new DriveItem
                        {
                            ParentReference = new Microsoft.Graph.ItemReference
                            {
                                Id = targetFolderDrive.Id
                            }
                        };

                        try
                        {
                            await graphClient.Drives[sourceDrive.Id].Items[spFile.DriveItemId].Request().UpdateAsync(requestBody);
                            isMoveSuccess = true;
                        }
                        catch (ServiceException ex)
                        {
                            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                                isMoveSuccess = true;
                        }

                        if (isMoveSuccess)
                        {
                            var driveItem = await graphClient.Drives[targetDrive.Id].Items[spFile.DriveItemId].Request().Expand("listItem").GetAsync(); 

                            if (list != null && (recArr.IsActRequired))
                            {
                                var fieldRequestBody = new FieldValueSet
                                {
                                    AdditionalData = new Dictionary<string, object>
                                    {
                                        {
                                            "IsActRequired" , recArr.IsActRequired
                                        }
                                    }
                                };
                                if (settings.IsSharePointIntegrationByMetadataOn)
                                {
                                    requestBody.AdditionalData.Add("CPIScreen", docLibraryFolder);
                                    requestBody.AdditionalData.Add("CPIRecordKey", recKey.Replace(SharePointSeparator.Folder, SharePointSeparator.Field));
                                }                            
                                var updateResult = await graphClient.Sites[site.Id].Lists[list.Id].Items[driveItem.ListItem.Id].Fields.Request().UpdateAsync(fieldRequestBody);
                            }
                            spFileIdList.RemoveAll(d => d == spFile.DriveItemId);

                            var docDocument = await _documentService.DocDocuments.Where(d => d.DocId == spFile.DocId).FirstOrDefaultAsync();
                            if (docDocument != null)
                            {
                                var defaultDocFolder = await _docViewModelService.GetOrAddDefaultFolder(documentLink);
                                if (defaultDocFolder != null)
                                {
                                    docDocument.FolderId = defaultDocFolder.FolderId;
                                    docDocument.IsActRequired = recArr.IsActRequired;
                                    await _documentService.UpdateDocuments(userName, new List<DocDocument>() { docDocument }, new List<DocDocument>(), new List<DocDocument>());

                                    //Save Responsible Docketing
                                    if (hasNewRespDocketing && recArr.RespDocketings != null)
                                    {
                                        await _documentService.UpdateDocRespDocketing(recArr.RespDocketings.ToList(), userName, docDocument.DocId);
                                    }

                                    //Save Responsible Reporting
                                    if (hasNewRespReporting && recArr.RespReportings != null)
                                    {
                                        await _documentService.UpdateDocRespReporting(recArr.RespReportings.ToList(), userName, docDocument.DocId);
                                    }

                                    var attachments = new List<WorkflowEmailAttachmentViewModel>() {
                                        new WorkflowEmailAttachmentViewModel { Id = spFile.DriveItemId, FileName = docDocument.DocName, DocParent = recId, DocDate = DateTime.Now }
                                    };
                                    var workflowHeader = await GenerateWorkflow(docLibrary, docLibraryFolder, attachments, true, hasNewRespDocketing, false, hasNewRespReporting, false);
                                    var emailWFs = GenerateEmailWorkflow(workflowHeader, attachments, recId);
                                    if (emailWFs != null)
                                        emailWorkflows.AddRange(emailWFs);
                                }
                            }
                        }                        
                    }

                    if (targetFolderDrive == null || targetDrive == null || isMoveSuccess == false)
                    {
                        var stream = await graphClient.Drives[sourceDrive.Id].Items[spFile.DriveItemId].Content.Request().GetAsync();
                        if (stream != null)
                        {
                            stream.Position = 0;
                            var addResult = new SharePointGraphDriveItemKeyViewModel();

                            if (settings.IsSharePointIntegrationByMetadataOn)
                            {
                                var existing = await graphClient.FileExists(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, spFile.FileName ?? "");
                                if (!existing)
                                    addResult = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, new List<string>(), stream, spFile.FileName ?? "");
                            }
                            else
                                addResult = await graphClient.UploadSiteFile(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, folders, stream, spFile.FileName ?? "");

                            if (addResult != null)
                            {
                                var driveItem = await graphClient.Drives[addResult.DriveId].Items[addResult.DriveItemId].Request().Expand("listItem").GetAsync();

                                if (list != null)
                                {
                                    var sync = new SharePointSyncToDocViewModel
                                    {
                                        DocLibrary = docLibrary,
                                        DocLibraryFolder = docLibraryFolder,
                                        DriveItemId = driveItem.Id,
                                        ParentId = recId,
                                        FileName = spFile.FileName,
                                        CreatedBy = userName.Left(20),
                                        Remarks = "",
                                        Tags = "",
                                        IsImage = driveItem.Image != null,
                                        IsPrivate = false,
                                        IsDefault = false,
                                        IsPrintOnReport = false,
                                        IsVerified = false,
                                        IncludeInWorkflow = false,
                                        IsActRequired = spFile.IsActRequired,
                                        CheckAct = false,
                                        SendToClient = false,
                                        Source = spFile.Source,
                                        Author = userEmail
                                    };
                                    await _sharePointViewModelService.SyncToDocumentTables(sync);

                                    newDriveItemIds.Add(driveItem.Id);

                                    newDriveItemIdList.Add(new SharePointDocumentEntryViewModel() { DocId = spFile.DocId, DriveItemId = driveItem.Id });
                                }

                                if (list != null && (recArr.IsActRequired))
                                {
                                    var requestBody = new FieldValueSet
                                    {
                                        AdditionalData = new Dictionary<string, object>
                                        {
                                            {
                                                "IsActRequired" , recArr.IsActRequired
                                            }
                                        }
                                    };
                                    if (settings.IsSharePointIntegrationByMetadataOn)
                                    {
                                        requestBody.AdditionalData.Add("CPIScreen", docLibraryFolder);
                                        requestBody.AdditionalData.Add("CPIRecordKey", recKey.Replace(SharePointSeparator.Folder, SharePointSeparator.Field));
                                    }

                                    var updateResult = await graphClient.Sites[site.Id].Lists[list.Id].Items[driveItem.ListItem.Id].Fields.Request().UpdateAsync(requestBody);
                                }
                            }
                        }
                    }
                }

                if (newDriveItemIds != null && newDriveItemIds.Count > 0)
                {
                    //Prepare workflows
                    foreach (var spDriveItemId in newDriveItemIds)
                    {
                        var newDoc = await _documentService.DocDocuments.AsNoTracking().Where(d => d.DocFile != null && d.DocFile.DriveItemId == spDriveItemId)
                                                                        .Select(d => new
                                                                        {
                                                                            d.DocId,
                                                                            d.DocName,
                                                                            DataKeyValue = d.DocFolder != null ? d.DocFolder.DataKeyValue : 0,
                                                                            DriveItemId = d.DocFile != null ? d.DocFile.DriveItemId : ""
                                                                        }).FirstOrDefaultAsync();
                        if (newDoc != null)
                        {
                            //Save Responsible Docketing
                            if (hasNewRespDocketing && recArr.RespDocketings != null)
                            {
                                await _documentService.UpdateDocRespDocketing(recArr.RespDocketings.ToList(), userName, newDoc.DocId);
                            }

                            //Save Responsible Reporting
                            if (hasNewRespReporting && recArr.RespReportings != null)
                            {
                                await _documentService.UpdateDocRespReporting(recArr.RespReportings.ToList(), userName, newDoc.DocId);
                            }

                            var attachments = new List<WorkflowEmailAttachmentViewModel>() {
                                new WorkflowEmailAttachmentViewModel { Id = newDoc.DriveItemId, FileName = newDoc.DocName, DocParent = newDoc.DataKeyValue, DocDate = DateTime.Now }
                            };
                            var workflowHeader = await GenerateWorkflow(docLibrary, docLibraryFolder, attachments, true, hasNewRespDocketing, false, hasNewRespReporting, false);
                            var emailWFs = GenerateEmailWorkflow(workflowHeader, attachments, newDoc.DataKeyValue);
                            if (emailWFs != null)
                                emailWorkflows.AddRange(emailWFs);
                        }
                    }
                }
            }

            //Delete old docs after linked to record(s)            
            if (spFileIdList.Count > 0)
            {
                //Update docId for MyEPO if module is on
                if (_epoMailboxSettings.IsAPIOn && (newDriveItemIdList != null && newDriveItemIdList.Count > 0))
                {
                    var oldDocIds = newDriveItemIdList.Select(d => d.DocId).ToList();
                    var epoDocList = await _epoCommunicationDocService.ChildService.QueryableList.Where(d => oldDocIds.Contains(d.DocId)).ToListAsync();
                    if (epoDocList != null && epoDocList.Count > 0)
                    {                        
                        var newIdList = newDriveItemIdList.Select(d => d.DriveItemId).ToList();
                        var newDocList = await _documentService.DocDocuments.AsNoTracking()
                                                        .Where(d => d.DocFile != null && newIdList.Contains(d.DocFile.DriveItemId))
                                                        .Select(d => new { d.DocId, DriveItemId = d.DocFile != null ? d.DocFile.DriveItemId : "" })
                                                        .ToListAsync();
                        foreach (var epoDoc in epoDocList)
                        {
                            var oldDoc = newDriveItemIdList.Where(d => d.DocId == epoDoc.DocId).FirstOrDefault();
                            if (oldDoc != null)
                            {
                                var newDoc = newDocList.Where(d => d.DriveItemId == oldDoc.DriveItemId).FirstOrDefault();
                                if (newDoc != null)
                                    epoDoc.DocId = newDoc.DocId;
                            }
                        }
                        await _epoCommunicationDocService.ChildService.Update(epoDocList);
                    }
                }

                foreach (var id in spFileIdList)
                {
                    await graphClient.DeleteSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Orphanage, id);

                    var deleteDoc = await _documentService.DocDocuments.Where(d => d.DocFile.DriveItemId == id).FirstOrDefaultAsync();
                    var deleteFile = await _documentService.DocFiles.Where(d => d.DriveItemId == id).FirstOrDefaultAsync();
                    if (deleteFile != null && deleteDoc != null)
                        await _documentService.DeleteDoc(deleteDoc, deleteFile);
                }
            }

            //Process AI            
            if (settings.IsDocumentUploadAIOn && newDriveItemIdList != null && newDriveItemIdList.Count > 0)
            {
                var newDocIdList = newDriveItemIdList.Select(d => d.DocId).ToHashSet();
                var patTmkDocuments = await _documentService.DocDocuments.AsNoTracking()
                    .Where(d => newDocIdList.Contains(d.DocId) && d.DocFolder != null && (d.DocFolder.DataKey == DataKey.Application || d.DocFolder.DataKey == DataKey.Trademark))
                    .Include(d => d.DocFolder).Include(d => d.DocFile).ToListAsync();

                await _documentsAIViewModelService.ProcessUploadedDocuments(patTmkDocuments);
            }

            if (emailWorkflows != null && emailWorkflows.Any())
            {
                var emailUrl = emailWorkflows.First().emailUrl;
                return Json(new { id = 0, sendEmail = true, folderId = 0, emailUrl, emailWorkflows });
            }

            return Json(new
            {
                ids = ids
            });
        }

        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> SaveDroppedDocVerification(IEnumerable<IFormFile> droppedFiles)
        {
            if (droppedFiles.Count() <= 0)
            {
                return BadRequest(_localizer["No Document to upload"].ToString());
            }

            await SaveDroppedFiles(droppedFiles, SharePointDocLibrary.Orphanage, null, null, null, 0, null, false, null);

            return Json(new
            {
                folderId = 0
            });
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DVDeleteDocuments(List<string> ids)
        {
            var graphClient = _sharePointService.GetGraphClient();
            try
            {
                foreach (var id in ids)
                {
                    var docLibrary = SharePointDocLibrary.Orphanage;
                    var systemType = await _documentService.DocDocuments.AsNoTracking().Where(d => d.DocFile.DriveItemId == id).Select(d => d.DocFolder.SystemType).FirstOrDefaultAsync();
                    if (!string.IsNullOrEmpty(systemType)) docLibrary = _sharePointViewModelService.GetDocLibraryFromSystemTypeCode(systemType);
                    await graphClient.DeleteSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, id);

                    var deleteDoc = await _documentService.DocDocuments.Where(d => d.DocFile.DriveItemId == id).FirstOrDefaultAsync();
                    var deleteFile = await _documentService.DocFiles.Where(d => d.DriveItemId == id).FirstOrDefaultAsync();
                    if (deleteFile != null && deleteDoc != null)
                        await _documentService.DeleteDoc(deleteDoc, deleteFile);
                }
                return Ok(new { success = _localizer["Document(s) has been deleted successfully."].ToString() });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> GetListContentTypes(string docLibrary)
        {
            var graphClient = _sharePointService.GetGraphClient();
            var result = await graphClient.GetSiteContentTypes(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary);
            return Json(result);

        }

        private async Task<string> GetDocumentLink(string recordLink)
        {
            var documentLink = "";

            if (!string.IsNullOrEmpty(recordLink))
            {
                var recordArr = recordLink.Split("/");
                var systemType = recordArr[0];
                var screenType = recordArr[1];
                var recordId = recordArr[3];

                switch (systemType)
                {
                    case SystemType.Patent:
                        {
                            documentLink = SystemTypeCode.Patent;
                            if (screenType.ToLower() == "countryapplication")
                                documentLink += "|" + ScreenCode.Application + "|AppId";
                            else if (screenType.ToLower() == "invention")
                                documentLink += "|" + ScreenCode.Invention + "|InvId";
                            break;
                        }
                    case SystemType.Trademark:
                        {
                            documentLink = SystemTypeCode.Trademark;
                            if (screenType.ToLower() == "tmktrademark")
                                documentLink += "|" + ScreenCode.Trademark + "|TmkId";
                            break;
                        }
                    case SystemType.GeneralMatter:
                        {
                            documentLink = SystemTypeCode.GeneralMatter;
                            if (screenType.ToLower() == "matter")
                                documentLink += "|" + ScreenCode.GeneralMatter + "|MatId";
                            break;
                        }
                    default:
                        break;
                }

                documentLink += "|" + recordId;
            }
            return documentLink;
        }

        private static void GetDocLibrary(string documentLink, ref string docLibrary, ref string? docLibraryFolder, ref int recId)
        {
            try
            {
                var docLinkArr = documentLink.Split("|");
                var systemType = docLinkArr[0];
                var screenCode = docLinkArr[1];
                recId = int.Parse(docLinkArr[3]);
                switch (systemType)
                {
                    case SystemTypeCode.Patent:
                        docLibrary = SharePointDocLibrary.Patent;
                        break;
                    case SystemTypeCode.Trademark:
                        docLibrary = SharePointDocLibrary.Trademark;
                        break;
                    case SystemTypeCode.GeneralMatter:
                        docLibrary = SharePointDocLibrary.GeneralMatter;
                        break;
                    default:
                        break;
                }
                switch (screenCode)
                {
                    case ScreenCode.Application:
                        docLibraryFolder = SharePointDocLibraryFolder.Application;
                        break;
                    case ScreenCode.Trademark:
                        docLibraryFolder = SharePointDocLibraryFolder.Trademark;
                        break;
                    case ScreenCode.GeneralMatter:
                        docLibraryFolder = SharePointDocLibraryFolder.GeneralMatter;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                docLibrary = "";
                docLibraryFolder = "";
                recId = 0;
            }
        }
        
        /// <summary>
        /// Prepares a temporary file for viewing by either returning the path to an existing temporary file
        /// or downloading the document from SharePoint and saving it to a temporary location.
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="docLibrary"></param>
        /// <param name="driveItemId"></param>
        /// <param name="tempFileName"></param>
        /// <returns></returns>
        private async Task<string> PrepareTemporaryFile(GraphServiceClient graphClient, string docLibrary, string driveItemId, string tempFileName)
        {
            var folder = FileHelper.GetTemporaryFolder(User.GetUserName());
            var tempFilePath = Path.Combine(folder, tempFileName);

            if (System.IO.File.Exists(tempFilePath)) return tempFilePath;

            try
            {
                using (var stream = await graphClient.DownloadSiteDriveItem(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, docLibrary, driveItemId))
                {
                    if (stream == null) return string.Empty;

                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                    {
                        await stream.CopyToAsync(fileStream);                        
                    }
                }                               
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return string.Empty;
                }
                throw;
            }            

            return tempFilePath;
        }
        #endregion

        private async Task LogDocTradeSecretActivity(string driveItemId)
        {
            // log trade secret download
            var settings = await _patSettings.GetSetting();
            if (settings.IsTradeSecretOn)
                await _documentService.LogDocTradeSecretActivityByDriveItemId(driveItemId);
        }
    }
}
