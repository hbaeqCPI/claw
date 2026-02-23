using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.Controllers;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Services.iManage;
using System.Security.Claims;
using R10.Core.Exceptions;
using R10.Core;
using R10.Web.Services.NetDocuments;

namespace R10.Web.Services
{
    public interface IDocumentImportService
    {
        Task<string> ImportDocument(IFormFile file, string documentLink, string sharePointLibrary, string sharePointFolder, string sharePointKey);

        /// <summary>
        /// Import document using Web API
        /// </summary>
        /// <param name="file"></param>
        /// <param name="webApiDoc"></param>
        /// <param name="sharePointLibrary"></param>
        /// <param name="sharePointFolder"></param>
        /// <param name="sharePointKey"></param>
        /// <param name="folderName"></param>
        /// <param name="updateDocTables"></param>
        /// <returns></returns>
        Task ImportDocument(IFormFile file, DocWebSvc webApiDoc, string sharePointLibrary, string sharePointFolder, string sharePointKey, string folderName = "", bool updateDocTables = true);

        /// <summary>
        /// Downloads DocFiles and returns list of file paths
        /// </summary>
        /// <param name="docFiles"></param>
        /// <param name="downloadFolder"></param>
        /// <param name="fileNameSuffix"></param>
        /// <param name="sharePointDocLibrary"></param>
        /// <returns></returns>
        Task<List<string>> DownloadDocFiles(List<DocFile?> docFiles, string downloadFolder, string fileNameSuffix, string sharePointDocLibrary);
    }

    public class DocumentImportService : IDocumentImportService
    {
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly IDocumentService _docService;
        private readonly IDocumentsViewModelService _docViewModelService;
        private readonly ISharePointViewModelService _sharePointViewModelService;
        private readonly IiManageViewModelService _iManageViewModelService;
        private readonly INetDocumentsViewModelService _netDocsViewModelService;
        private readonly ClaimsPrincipal _user;
        private readonly ICPiDbContext _cpiDbContext;

        public DocumentImportService(
            ISystemSettings<DefaultSetting> settings, 
            IDocumentService docService, 
            IDocumentsViewModelService docViewModelService, 
            ISharePointViewModelService sharePointViewModelService, 
            IiManageViewModelService iManageViewModelService,
            INetDocumentsViewModelService netDocsViewModelService,
            ClaimsPrincipal user,
            ICPiDbContext cpiDbContext)
        {
            _settings = settings;
            _docService = docService;
            _docViewModelService = docViewModelService;
            _sharePointViewModelService = sharePointViewModelService;
            _iManageViewModelService = iManageViewModelService;
            _netDocsViewModelService = netDocsViewModelService;
            _user = user;
            _cpiDbContext = cpiDbContext;
        }

        public async Task ImportDocument(IFormFile file, DocWebSvc webApiDoc, string sharePointLibrary, string sharePointFolder, string sharePointKey, string folderName = "", bool updateDocTables = true)
        {
            //check document link
            Guard.Against.NullOrEmpty(webApiDoc.DocumentLink, "DocumentLink");

            //import document
            var fileName = await ImportDocument(file, webApiDoc.DocumentLink ?? "", sharePointLibrary, sharePointFolder, sharePointKey, folderName, updateDocTables);

            //log api data            
            webApiDoc.FileName = fileName;
            _cpiDbContext.GetRepository<DocWebSvc>().Add(webApiDoc);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task<string> ImportDocument(IFormFile file, string documentLink, string sharePointLibrary, string sharePointFolder, string sharePointKey)
        {
            return await ImportDocument(file, documentLink, sharePointLibrary, sharePointFolder, sharePointKey, "", true);
        }

        private async Task<string> ImportDocument(IFormFile formFile, string documentLink, string sharePointLibrary, string sharePointFolder, string sharePointKey, string folderName, bool updateDocTables)
        {
            var settings = await _settings.GetSetting();
            var documentLinkArray = documentLink.Split("|");
            var systemType = documentLinkArray[0];
            var screenCode = documentLinkArray[1];
            var dataKey = documentLinkArray[2];
            var dataKeyValue = Convert.ToInt32(documentLinkArray[3]);
            var fullFileName = formFile.FileName;

            if (!(await _docViewModelService.CanModifyDocument(documentLink)))
                throw new UnauthorizedAccessException($"User {_user.GetUserName()} has no permission to upload document to {documentLink}.");

            //Check duplicate file name
            if (await _docService.DocDocuments.AnyAsync(d => d.DocFolder != null && d.DocFolder.DataKey == dataKey && d.DocFolder.ScreenCode == screenCode && d.DocFolder.DataKeyValue == dataKeyValue && d.DocName == fullFileName))
            {
                //Use current date time to avoid copy duplicate
                fullFileName = Path.GetFileNameWithoutExtension(fullFileName) + "_copy-" + DateTime.Now.ToString("dd-MMM-yyyy-HH-mm-ss") + "." + Path.GetExtension(fullFileName);
            }

            switch (settings.DocumentStorage)
            {
                case DocumentStorageOptions.SharePoint:
                    await _sharePointViewModelService.SaveImportedDocument(formFile, fullFileName, dataKeyValue, sharePointLibrary, sharePointFolder, sharePointKey);
                    break;

                case DocumentStorageOptions.iManage:
                    await _iManageViewModelService.SaveImportedDocument(formFile, documentLink, folderName, updateDocTables);
                    break;

                case DocumentStorageOptions.NetDocuments:
                    await _netDocsViewModelService.SaveImportedDocument(formFile, documentLink, folderName, updateDocTables: updateDocTables);
                    break;

                default:
                    await _docViewModelService.SaveImportedDocument(formFile, fullFileName, documentLink);
                    break;
            }

            return fullFileName;
        }

        public async Task<List<string>> DownloadDocFiles(List<DocFile?> docFiles, string downloadFolder, string fileNameSuffix, string sharePointDocLibrary)
        {
            var settings = await _settings.GetSetting();
            var attachments = new List<String>();

            foreach (var docFile in docFiles)
            {
                if (docFile == null || string.IsNullOrEmpty(docFile.DocFileName)) continue;

                var fileName = docFile.DocFileName;

                if (!string.IsNullOrEmpty(fileNameSuffix))
                    fileName = string.Concat(
                        Path.GetFileNameWithoutExtension(fileName),
                        "_",
                        fileNameSuffix,
                        Path.GetExtension(fileName)
                        );

                fileName = fileName.AppendTimeStamp();

                Stream? sourceStream = new MemoryStream();

                switch (settings.DocumentStorage)
                {
                    case DocumentStorageOptions.SharePoint:
                        if (!string.IsNullOrEmpty(docFile.DriveItemId))
                            sourceStream = await _sharePointViewModelService.GetDocumentAsStream(sharePointDocLibrary, docFile.DriveItemId);
                        break;

                    case DocumentStorageOptions.iManage:
                        if (!string.IsNullOrEmpty(docFile.DriveItemId))
                            sourceStream = await _iManageViewModelService.GetDocumentAsStream(docFile.DriveItemId);
                        break;

                    case DocumentStorageOptions.NetDocuments:
                        if (!string.IsNullOrEmpty(docFile.DriveItemId))
                            sourceStream = await _netDocsViewModelService.GetDocumentAsStream(docFile.DriveItemId);
                        break;

                    default:
                        sourceStream = await _docViewModelService.GetDocumentAsStream("", fileName, ImageHelper.CPiSavedFileType.DocMgt);
                        break;
                }

                if (sourceStream != null)
                {
                    var fullPath = Path.Combine(downloadFolder, fileName);

                    using (FileStream outputFileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        sourceStream.CopyTo(outputFileStream);
                    }

                    attachments.Add(fullPath);
                }
            }

            return attachments;
        }
    }
}
