using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.Documents;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Services.DocumentStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.Services
{
    public class DocumentViewModelService : IDocumentViewModelService
    {
        private readonly IDocumentService _docService;

        private readonly IInventionService _inventionService;
        private readonly ICountryApplicationService _applicationService;
        private readonly ITmkTrademarkService _trademarkService;
        private readonly IGMMatterService _gmMatterService;

        private readonly IActionDueService<PatActionDue, PatDueDate> _patActionDueService;
        private readonly IActionDueService<TmkActionDue, TmkDueDate> _tmkActionDueService;
        private readonly IActionDueService<GMActionDue, GMDueDate> _gmActionDueService;

        private readonly ICostTrackingService<PatCostTrack> _patCostTrackService;
        private readonly ICostTrackingService<PatCostTrackInv> _patCostTrackInvService;
        private readonly ICostTrackingService<TmkCostTrack> _tmkCostTrackService;
        private readonly ICostTrackingService<GMCostTrack> _gmCostTrackService;


        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly IDocumentHelper _documentHelper;

        private readonly IMapper _mapper;

        private const string defaultDocTypeName = "Image";

        private const string docNode = "doc";
        private const string rootNode = "root";
        private const string folderNode = "folder";

        //add for deduplicate doc name --Yin
        private const string docNameSeparator = "--";

        private enum DocTreeIndex : int
        {
            SystemType = 0,
            ScreenType,
            DataKey,
            DataKeyValue,
            DocSource,
            NodeType,
            ParentId,
            NodeId
        }

        public DocumentViewModelService(
                    IDocumentService docService,
                    IInventionService inventionService,
                    ICountryApplicationService applicationService,
                    ITmkTrademarkService trademarkService,
                    IGMMatterService gmMatterService,
                    IActionDueService<PatActionDue, PatDueDate> patActionDueService,
                    IActionDueService<TmkActionDue, TmkDueDate> tmkActionDueService,
                    IActionDueService<GMActionDue, GMDueDate> gmActionDueService,
                    ICostTrackingService<PatCostTrack> patCostTrackService,
                    ICostTrackingService<PatCostTrackInv> patCostTrackInvService,
                    ICostTrackingService<TmkCostTrack> tmkCostTrackService,
                    ICostTrackingService<GMCostTrack> gmCostTrackService,
                    ISystemSettings<DefaultSetting> settings,
                    IDocumentHelper documentHelper,
                    IMapper mapper)
        {
            _docService = docService;
            _inventionService = inventionService;
            _applicationService = applicationService;
            _trademarkService = trademarkService;
            _gmMatterService = gmMatterService;

            _patActionDueService = patActionDueService;
            _tmkActionDueService = tmkActionDueService;
            _gmActionDueService = gmActionDueService;

            _patCostTrackService = patCostTrackService;
            _patCostTrackInvService = patCostTrackInvService;
            _tmkCostTrackService = tmkCostTrackService;
            _gmCostTrackService = gmCostTrackService;

            _settings = settings;
            _documentHelper = documentHelper;
            _mapper = mapper;
        }


        #region Search
        public async Task<List<LookupDTO>> GetSystemList(List<string> userSystems)
        {
            //var list = await _docService.DocSystems.Where(s => s.IsEnabled && s.CPiSystem.IsEnabled).OrderBy(s => s.EntryOrder)
            var list = await _docService.DocSystems.Where(s => s.IsEnabled && userSystems.Any(us => us == s.CPiSystem.Id)).OrderBy(s => s.EntryOrder)
                        .Select(s => new LookupDTO() { Value = s.SystemType, Text = s.SystemName }).ToListAsync();
            return list;
        }

        public async Task<List<LookupDTO>> GetSystemShortNameList(List<string> userSystems)
        {
            //var list = await _docService.DocSystems.Where(s => s.IsEnabled && s.CPiSystem.IsEnabled).OrderBy(s => s.EntryOrder)
            var list = await _docService.DocSystems.Where(s => s.IsEnabled && userSystems.Any(us => us == s.CPiSystem.Id)).OrderBy(s => s.EntryOrder)
                        .Select(s => new LookupDTO() { Value = s.SystemType, Text = s.SystemNameShort }).ToListAsync();
            return list;
        }

        public async Task<List<LookupDTO>> GetScreenList(string systemType)
        {
            var list = await _docService.DocMatterTrees.Where(t => t.SystemType == systemType && t.InUse).OrderBy(t => t.EntryOrder)
                            .Select(t => new LookupDTO() { Value = t.ScreenCode, Text = t.ScreenName }).ToListAsync();
            return list;
        }

        public string GetSubSearchView(string systemType, string screenCode)
        {
            //var view = await _docService.DocMatterTrees.Where(t => t.ScreenCode == screenCode).Select(t => t.SearchTabView).FirstOrDefaultAsync();
            var view = _docService.DocMatterTrees.Where(t => t.SystemType == systemType && t.ScreenCode == screenCode).Select(t => t.SearchTabView).FirstOrDefault();
            return view;
        }

        public string GetSearchResultView(string systemType, string screenCode)
        {
            //var view = await _docService.DocMatterTrees.Where(t => t.ScreenCode == screenCode).Select(t => t.SearchResultView).FirstOrDefaultAsync();
            var view = _docService.DocMatterTrees.Where(t => t.SystemType == systemType && t.ScreenCode == screenCode).Select(t => t.SearchResultView).FirstOrDefault();
            return view;
        }
        #endregion Search

        #region Tree Node

        public DocFolderViewModel GetUserFolderView(string treeNodeId)
        {
            var nodeId = GetNodeId(treeNodeId);

            var model = _docService.DocFolders.ProjectTo<DocFolderViewModel>().SingleOrDefault(f => f.FolderId == nodeId);

            return model;
        }

        public async Task<bool> DeleteTreeNode(string treeNodeId)
        {
            var treeParam = treeNodeId.Split("|");
            var nodetype = treeParam[(int)DocTreeIndex.NodeType];
            var nodeId = int.Parse(treeParam[(int)DocTreeIndex.NodeId]);

            if (nodetype == docNode)
            {
                var document = await _docService.GetDocumentById(nodeId);
                if (document == null) return false;

                if (document.FileId != null && document.FileId != 0)
                {
                    var fileId = document.FileId.Value;
                    var docFile = await _docService.GetFileById(fileId);
                    if (docFile == null) return false;

                    // delete physical file only if there are no references to it by other documents
                    if (await _docService.GetFileOtherRefCount(nodeId, fileId) == 0)
                    {
                        if (_documentHelper.DeleteDocumentFile(docFile.DocFileName, docFile.ThumbFileName, docFile.IsImage))
                        {
                            return await _docService.DeleteDoc(document, docFile);
                        }
                    }
                    else
                    {
                        return await _docService.DeleteDoc(document, docFile);
                    }

                    return false;
                }
                else
                {
                    return await _docService.DeleteDoc(document, null);
                }
            }
            else
            {
                var folder = await _docService.GetFolderByIdAsync(nodeId);
                return await _docService.UpdateFolders("", new List<DocFolder>(), new List<DocFolder>(), new List<DocFolder>() { folder });
            }
        }

        public async Task<bool> RenameTreeNode(string treeNodeId, string newName, string userName)
        {
            var treeParam = treeNodeId.Split("|");
            var nodeType = treeParam[(int)DocTreeIndex.NodeType];
            var nodeId = int.Parse(treeParam[(int)DocTreeIndex.NodeId]);

            if (nodeType == docNode)
            {
                return await _docService.RenameDocument(userName, nodeId, newName);
            }
            else
            {
                return await _docService.RenameFolder(userName, nodeId, newName);
            }
        }

        public async Task<bool> DropTreeNode(string sourceId, string destId, string userName)
        {
            var destParam = destId.Split("|");
            var destNodeType = destParam[(int)DocTreeIndex.NodeType];

            // cannot drop anything to a document node
            if (destNodeType == docNode)
                return false;

            var destNodeId = int.Parse(destParam[(int)DocTreeIndex.NodeId]);

            var sourceParam = sourceId.Split("|");
            var sourceNodeType = sourceParam[(int)DocTreeIndex.NodeType];
            var sourceNodeId = int.Parse(sourceParam[(int)DocTreeIndex.NodeId]);

            if (sourceNodeType == docNode)
            {
                var document = await _docService.GetDocumentById(sourceNodeId);
                if (document == null) return false;

                document.FolderId = destNodeId;
                return await _docService.UpdateDocuments(userName, new List<DocDocument>() { document }, new List<DocDocument>(), new List<DocDocument>());
            }
            else if (sourceNodeType == folderNode)
            {
                var folder = await _docService.GetFolderByIdAsync(sourceNodeId);
                if (folder.FolderId != destNodeId) {
                    folder.ParentFolderId = destNodeId;
                    return await _docService.UpdateFolders(userName, new List<DocFolder>() { folder }, new List<DocFolder>(), new List<DocFolder>());
                }
            }
            return false;
        }

        public int GetNodeId(string treeNodeId)
        {
            return int.Parse(treeNodeId.Split("|")[(int)DocTreeIndex.NodeId]);
        }

        public int GetDataKeyValue(string treeNodeId)
        {
            return int.Parse(treeNodeId.Split("|")[(int)DocTreeIndex.DataKeyValue]);
        }

        #endregion

        #region Documents
        public async Task<List<DocDocumentListViewModel>> GetDocumentsByFolderId(int folderId)
        {
            var model = await _docService.DocDocuments.Where(d => d.FolderId == folderId).ProjectTo<DocDocumentListViewModel>().ToListAsync();
            var defaultFileIcon = await _docService.DocIcons.Where(i => i.FileExt == "filedefault").Select(i => i.IconClass).FirstOrDefaultAsync();
            model.Where(r => string.IsNullOrEmpty(r.IconClass)).Each(r => r.IconClass = defaultFileIcon);

            // mark viewable/linkable files
            var settings = await _settings.GetSetting();
            var viewableExts = settings.ViewableDocs.Split("|");
            model.Where(r => !string.IsNullOrEmpty(r.DocFileName)).Each(r => r.IsDocViewable = viewableExts.Any(x => r.DocFileName.ToLower().EndsWith(x)));
            model.Where(r => !string.IsNullOrEmpty(r.DocUrl)).Each(r => r.IsDocLinkable = true);

            return model;
        }

        public async Task<DocDocumentViewModel> CreateDocumentEditorViewModel(string documentLink,int folderId, int parentFolderId, int docId)
        {
            var model = new DocDocumentViewModel();
            if (docId > 0)
            {
                model = await _docService.DocDocuments.ProjectTo<DocDocumentViewModel>().FirstOrDefaultAsync(d => d.DocId == docId);
                // mark viewable/linkable file
                if (!string.IsNullOrEmpty(model.DocFileName))
                {
                    var settings = await _settings.GetSetting();
                    var viewableExts = settings.ViewableDocs.Split("|");
                    model.IsDocViewable = viewableExts.Any(x => model.DocFileName.ToLower().EndsWith(x));
                }
                if (model.ThumbFileName == null && !string.IsNullOrEmpty(model.DocUrl))
                    model.ThumbFileName = "logo_url.png";

                model.IsDocLinkable = !string.IsNullOrEmpty(model.DocUrl);
            }
            else {
                if (folderId == 0) {
                    folderId = (await GetDefaultFolder(documentLink)).FolderId;
                }

                var defaultDocType = await _docService.DocTypes.Where(d => d.DocTypeName == defaultDocTypeName).FirstOrDefaultAsync();
                if (defaultDocType !=null) {
                    model.DocTypeId = defaultDocType.DocTypeId;
                    model.DocTypeName = defaultDocType.DocTypeName;
                }
                model.FolderId = folderId;
            }
            model.HasDefault = HasDefault(documentLink,model.ScreenCode);
            return model;
        }

        public async Task<DefaultImageViewModel> GetDefaultImage(string system, string screenCode, string systemType, string dataKey, int dataKeyValue)
        {
            var defaultImage = await _docService.DocDocuments.Where(d => _docService.DocFolders.Where(f => f.SystemType==systemType && f.DataKey == dataKey && f.DataKeyValue == dataKeyValue)
               .Any(f => f.FolderId == d.FolderId) && d.IsDefault)
                   .Select(d => new DefaultImageViewModel()
                   {
                       ImageId = d.DocId,
                       ImageFile = d.DocFile.DocFileName,
                       ImageTitle = d.DocName ?? "",
                       ImageTypeName = d.DocType != null ? d.DocType.DocTypeName : "",
                       ThumbnailFile = d.DocFile.ThumbFileName,
                       IsPublic = !d.IsPrivate,
                       System = system,
                       ScreenCode = screenCode,
                       Key = dataKeyValue
                   }).FirstOrDefaultAsync();
            return defaultImage;
        }

        public async Task<DocDocumentViewModel> CreateDocumentEditorViewModel(int folderId, int docId)
        {
            var model = new DocDocumentViewModel();
            if (docId > 0)
            {
                model = await _docService.DocDocuments.ProjectTo<DocDocumentViewModel>().FirstOrDefaultAsync(d => d.DocId == docId);
                // mark viewable/linkable file
                if (!string.IsNullOrEmpty(model.DocFileName))
                {
                    var settings = await _settings.GetSetting();
                    var viewableExts = settings.ViewableDocs.Split("|");
                    model.IsDocViewable = viewableExts.Any(x => model.DocFileName.ToLower().EndsWith(x));
                }
                if (model.ThumbFileName == null && !string.IsNullOrEmpty(model.DocUrl))
                    model.ThumbFileName = "logo_url.png";

                model.IsDocLinkable = !string.IsNullOrEmpty(model.DocUrl);
            }
            else
                model.FolderId = folderId;

            return model;
        }

        // save file uploaded on user folder detail; creates document using filename
        public async Task<bool> SaveUploadedDocument(DocDocument document, IFormFile uploadedFile, string rootPath)
        {
            var fileName = uploadedFile.FileName.ToLower();
            document.DocName = Path.GetFileNameWithoutExtension(fileName);
            document.DocTypeId = await _docService.GetDocTypeIdFromFileName(fileName);
            //var isImage = await _docService.IsImageType(document.DocTypeId);
            var isImage = uploadedFile.ContentType.Contains("image");

            // save file info to db
            var docFile = new DocFile
            {
                FileExt = Path.GetExtension(fileName).Replace(".", ""),
                UserFileName = fileName,
                FileSize = (int)uploadedFile.Length,
                IsImage = isImage,
                CreatedBy = document.CreatedBy,
                DateCreated = document.DateCreated,
                UpdatedBy = document.UpdatedBy,
                LastUpdate = document.LastUpdate
            };
            var newFile = await _docService.AddDocFile(docFile);

            // get folder header for systemtype, screencode, etc.
            var folderHeader = await _docService.GetFolderHeader(document.FolderId);
   
            // save actual file
            if (await _documentHelper.SaveDocumentFileUpload(uploadedFile, newFile.DocFileName, newFile.ThumbFileName, folderHeader))
            {
                //save document info to db
                document.FileId = newFile.FileId;
                await _docService.UpdateDocuments(document.CreatedBy, new List<DocDocument>(), new List<DocDocument>() { document }, new List<DocDocument>());
                return true;
            }

            return false;
        }

        
        // saves new/updated document on popup from user folder detail document grid, and from _DetailUserDocument
        public async Task<bool> SaveDocumentPopup(DocDocumentViewModel viewModel, string rootPath)
        {

            // save any uploaded file
            if (viewModel.UploadedFiles != null)
            {
                var uploadedFile = viewModel.UploadedFiles.First();
                var fileName = uploadedFile.FileName.ToLower();
                //viewModel.DocTypeId = await _docService.GetDocTypeIdFromFileName(fileName);       // this is entered on screen

                var isImage = uploadedFile.ContentType.Contains("image");
                var docFile = new DocFile
                {
                    FileExt = Path.GetExtension(fileName).Replace(".", ""),
                    UserFileName = fileName,
                    FileSize = (int)uploadedFile.Length,
                    IsImage = isImage,
                    CreatedBy = viewModel.UpdatedBy,
                    DateCreated = viewModel.LastUpdate,
                    UpdatedBy = viewModel.UpdatedBy,
                    LastUpdate = viewModel.LastUpdate
                };
                var newFile = await _docService.AddDocFile(docFile);

                // get folder header for systemtype, screencode, etc.
                var folderHeader = await _docService.GetFolderHeader(viewModel.FolderId);

                if (await _documentHelper.SaveDocumentFileUpload(uploadedFile, newFile.DocFileName, newFile.ThumbFileName, folderHeader))
                {
                    //save document info to db
                    viewModel.FileId = newFile.FileId;
                }
            }

            // save/update document
            var document = _mapper.Map<DocDocumentViewModel, DocDocument>(viewModel);
            if (document.DocId > 0) {
                if (viewModel.ReleaseFileLock) {
                    document.LockedBy = "";
                }
                await _docService.UpdateDocuments(viewModel.UpdatedBy, new List<DocDocument>() { document }, new List<DocDocument>(), new List<DocDocument>());
            }
            else
                await _docService.UpdateDocuments(viewModel.CreatedBy, new List<DocDocument>(), new List<DocDocument>() { document }, new List<DocDocument>());

            return true;
        }

        public async Task<bool> SaveUploadedDocuments(List<DocDocumentViewModel> viewModels)
        {
            var folderHeader = new DocFolderHeader();
            var folder = new DocFolder();

            if (viewModels.Any()) {
                var viewModel = viewModels.First();
                folder = _docService.GetFolderById(viewModel.FolderId);
                folderHeader = new DocFolderHeader { SystemType = folder.SystemType, ScreenCode = folder.ScreenCode, ParentId = folder.DataKeyValue };
            }

            foreach (DocDocumentViewModel viewModel in viewModels)
            {
                var uploadedFile = viewModel.UploadedFile;
                var fileName = uploadedFile.FileName.ToLower();
                var isImage = uploadedFile.ContentType.Contains("image");
                var docFile = new DocFile
                {
                    FileExt = Path.GetExtension(fileName).Replace(".", ""),
                    UserFileName = fileName,
                    FileSize = (int)uploadedFile.Length,
                    IsImage = isImage,
                    CreatedBy = viewModel.CreatedBy,
                    DateCreated = viewModel.LastUpdate,
                    UpdatedBy = viewModel.UpdatedBy,
                    LastUpdate = viewModel.LastUpdate
                };
                var newFile = await _docService.AddDocFile(docFile);

                if (await _documentHelper.SaveDocumentFileUpload(uploadedFile, newFile.DocFileName, newFile.ThumbFileName, folderHeader))
                {
                    //save document info to db
                    viewModel.FileId = newFile.FileId;
                }
            }

            List<DocDocument> documents = new List<DocDocument>();
            Dictionary<string, int> docNameDict = await GetDocNameListByFolderId(viewModels[0].FolderId);
            foreach (DocDocumentViewModel viewModel in viewModels)
            {
                string tempDocName = Path.GetFileNameWithoutExtension(viewModel.UserFileName);
                viewModel.DocName = UpdateDictAndGetName(ref docNameDict, tempDocName);
                viewModel.DocTypeId = await _docService.GetDocTypeIdFromFileName(viewModel.UserFileName);
                var document = _mapper.Map<DocDocumentViewModel, DocDocument>(viewModel);
                documents.Add(document);
            }
            await _docService.UpdateDocuments(viewModels.First().CreatedBy, new List<DocDocument>(), documents, new List<DocDocument>(),folder);
            return true;

        }

        public async Task<DocFolder> GetDefaultFolder(string documentLink) {
            var documentLinkArray = documentLink.Split("|");
            var systemType = documentLinkArray[0];
            var screenCode = documentLinkArray[1];
            var dataKey = documentLinkArray[2];
            var dataKeyValue = Convert.ToInt32(documentLinkArray[3]);

            var existingFolder = await _docService.GetFolder(systemType, dataKey, dataKeyValue, "Documents", 0);
            if (existingFolder == null)
            {
                var folder = await _docService.AddFolder(systemType, dataKey, screenCode, dataKeyValue, "Documents", 0, true);
                return folder;
            }
            return existingFolder;
        }

        protected bool HasDefault(string documentLink,string? recScreenCode)
        {
            var documentLinkArray = documentLink.Split("|");
            var screenCode = documentLinkArray[1];
            bool hasDefault = screenCode== recScreenCode;
            if (hasDefault)
               hasDefault = screenCode == ScreenCode.Invention || screenCode == ScreenCode.Application || screenCode == ScreenCode.Trademark;
            
            return hasDefault;
        }

        //add for doc name deduplication --Yin
        public async Task<Dictionary<string, int>> GetDocNameListByFolderId(int folderId)
        {
            var documents = await GetDocumentsByFolderId(folderId);
            Dictionary<string, int> docNameList = new Dictionary<string, int>();
            Regex regex = new Regex(docNameSeparator + @"(\d+)$");
            foreach (var doc in documents)
            {
                string docName = doc.DocName;
                int docNo = 0;
                if (regex.Match(docName).Success)
                {
                    int lastOccurrOfSeparator = docName.LastIndexOf(docNameSeparator);
                    string newdocName = docName.Substring(0, lastOccurrOfSeparator);
                    docNo = Int32.Parse(docName.Substring(lastOccurrOfSeparator + docNameSeparator.Length));
                    if (docNameList.ContainsKey(newdocName))
                    {
                        if (docNameList[newdocName] < docNo) docNameList[newdocName] = docNo;
                    }
                    else
                    {
                        docNameList[newdocName] = 0;
                    }
                }
                else
                {
                    docNameList[docName] = (docNameList.ContainsKey(docName)) ? docNameList[docName] + 1 : 0;
                }
            }
            return docNameList;
        }

        public string UpdateDictAndGetName(ref Dictionary<string, int> dict, string docName)
        {
            if (dict.ContainsKey(docName))
            {
                int docNo = dict[docName] + 1;
                dict[docName] = docNo;
                docName = docName + docNameSeparator + docNo;
            }
            else
            {
                dict[docName] = 0;
            }
            return docName;
        }


        #endregion

        #region Fixed Folder/Documents
        public async Task<DocFixedFolderViewModel> GetFixedFolderView(string treeNodeId)
        {
            var nodeId = GetNodeId(treeNodeId);
            var model = await _docService.DocFixedFolders.ProjectTo<DocFixedFolderViewModel>().SingleOrDefaultAsync(f => f.FolderId == nodeId);
            return model;
        }
        public async Task<T> GetFixedDocDetail<T>(string id) where T : class
        {
            var docDetail = await _docService.GetFixedDocDetail<T>(id);

            var folder = await GetPhysicalFolder(id);

            Type docType = typeof(T);
            PropertyInfo docFileName = docType.GetProperty("DocFileName");
            var docFileValue = docFileName.GetValue(docDetail);
            if (docFileValue != null && docFileValue.ToString() != "")
                docFileName.SetValue(docDetail, folder + docFileName.GetValue(docDetail));
            else
                docFileName.SetValue(docDetail, "");

            return docDetail;
        }

        public async Task<T> GetIDSDetail<T>(string id) where T : class
        {
            var docDetail = await _docService.GetIDSDetail<T>(id);

            var folder = await GetPhysicalFolder(id);

            Type docType = typeof(T);
            PropertyInfo docFileName = docType.GetProperty("DocFileName");
            if (!string.IsNullOrEmpty(docFileName.GetValue(docDetail).ToString()))
                docFileName.SetValue(docDetail, folder + docFileName.GetValue(docDetail));

            return docDetail;
        }
        #endregion

        #region Root Node Detail

        public async Task<DocInventionViewModel> GetInventionDetail(string treeNodeId)
        {
            var invId = GetDataKeyValue(treeNodeId);
            var model = await _inventionService.QueryableList.ProjectTo<DocInventionViewModel>().SingleOrDefaultAsync(i => i.InvId == invId);
            return model;
        }

        public async Task<DocCtryAppViewModel> GetCtryAppDetail(string treeNodeId)
        {
            var appId = GetDataKeyValue(treeNodeId);
            var model = await _applicationService.CountryApplications.Where(ca => ca.AppId == appId).ProjectTo<DocCtryAppViewModel>().SingleOrDefaultAsync();
            return model;
        }

        public async Task<DocTrademarkViewModel> GetTrademarkDetail(string treeNodeId)
        {
            var tmkId = GetDataKeyValue(treeNodeId);
            var model = await _trademarkService.TmkTrademarks.ProjectTo<DocTrademarkViewModel>().SingleOrDefaultAsync(i => i.TmkId == tmkId);
            return model;
        }

        public async Task<DocGeneralMatterViewModel> GetGeneralMatterDetail(string treeNodeId)
        {
            var matId = GetDataKeyValue(treeNodeId);
            var model = await _gmMatterService.QueryableList.ProjectTo<DocGeneralMatterViewModel>().SingleOrDefaultAsync(i => i.MatId == matId);
            return model;
        }

        public async Task<DocPatActViewModel> GetPatActionDetail(string treeNodeId)
        {
            var actId = GetDataKeyValue(treeNodeId);
            var model = await _patActionDueService.QueryableList.Where(ad => ad.ActId == actId).ProjectTo<DocPatActViewModel>().SingleOrDefaultAsync();
            return model;
        }

        public async Task<DocTmkActViewModel> GetTmkActionDetail(string treeNodeId)
        {
            var actId = GetDataKeyValue(treeNodeId);
            var model = await _tmkActionDueService.QueryableList.Where(ad => ad.ActId == actId).ProjectTo<DocTmkActViewModel>().SingleOrDefaultAsync();
            return model;
        }

        public async Task<DocGMActViewModel> GetGMActionDetail(string treeNodeId)
        {
            var actId = GetDataKeyValue(treeNodeId);
            var model = await _gmActionDueService.QueryableList.Where(ad => ad.ActId == actId).ProjectTo<DocGMActViewModel>().SingleOrDefaultAsync();
            return model;
        }

        public async Task<DocPatCostViewModel> GetPatCostDetail(string treeNodeId)
        {
            var costTrackId = GetDataKeyValue(treeNodeId);
            var model = await _patCostTrackService.QueryableList.Where(ct => ct.CostTrackId == costTrackId).ProjectTo<DocPatCostViewModel>().SingleOrDefaultAsync();
            return model;
        }

        public async Task<DocPatCostInvViewModel> GetPatCostInvDetail(string treeNodeId)
        {
            var costTrackInvId = GetDataKeyValue(treeNodeId);
            var model = await _patCostTrackInvService.QueryableList.Where(ct => ct.CostTrackInvId == costTrackInvId).ProjectTo<DocPatCostInvViewModel>().SingleOrDefaultAsync();
            return model;
        }

        public async Task<DocTmkCostViewModel> GetTmkCostDetail(string treeNodeId)
        {
            var costTrackId = GetDataKeyValue(treeNodeId);
            var model = await _tmkCostTrackService.QueryableList.Where(ct => ct.CostTrackId == costTrackId).ProjectTo<DocTmkCostViewModel>().SingleOrDefaultAsync();
            return model;
        }

        public async Task<DocGMCostViewModel> GetGMCostDetail(string treeNodeId)
        {
            var costTrackId = GetDataKeyValue(treeNodeId);
            var model = await _gmCostTrackService.QueryableList.Where(ct => ct.CostTrackId == costTrackId).ProjectTo<DocGMCostViewModel>().SingleOrDefaultAsync();
            return model;
        }

        #endregion

        #region Miscellaneous

        public async Task<string> GetPhysicalFolder(string treeNodeId)
        {
            var treeParam = treeNodeId.Split("|");
            var parentId = int.Parse(treeParam[(int)DocTreeIndex.ParentId]);

            var folder = await _docService.DocFixedFolders.Where(f => f.FolderId == parentId).Select(f => f.PhysicalFolder).SingleOrDefaultAsync();
            if (!folder.EndsWith(@"\"))
                folder += @"\";
            return folder;
        }
        #endregion

    }
}
