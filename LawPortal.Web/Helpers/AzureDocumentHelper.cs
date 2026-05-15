using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LawPortal.Core.DTOs;
using LawPortal.Core.Entities.Documents;
using LawPortal.Web.Services.DocumentStorage;
using System.IO;
using System.Threading.Tasks;
using static LawPortal.Web.Helpers.ImageHelper;

namespace LawPortal.Web.Helpers
{
    // Azure-mode helper. Inherits the base helper's "viewer model / DataKeyToScreenCode"
    // logic (which is storage-independent) and overrides every file-touching method to
    // route through the AzureStorage blob client instead of File.* APIs.
    //
    // Path convention inside the container:
    //   Searchable/Documents/<DocFileName>
    public class AzureDocumentHelper : DocumentHelper
    {
        private const string DocumentBaseFolder = "Searchable/Documents";

        private readonly AzureStorage _documentStorage;
        private readonly ILogger<DocumentHelper> _azureLogger;

        public AzureDocumentHelper(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ILogger<DocumentHelper> logger,
            AzureStorage documentStorage) : base(environment, configuration, logger)
        {
            _documentStorage = documentStorage;
            _azureLogger = logger;
        }

        public override string GetDocumentBasePath()
        {
            return DocumentBaseFolder;
        }

        // For Azure mode, "GetDocumentPath" returns a blob-relative path (e.g.
        // "Searchable/Documents/120.mdb") if the blob exists, else empty string.
        // Callers that need a local file (e.g. the 32-bit MDB sidecar) should call
        // AzureStorage.SaveFileStreamToPath to download to a temp file first.
        //
        // Two-stage lookup:
        //   1. Canonical path (where new uploads land) — fast, single ExistsAsync.
        //   2. Container scan by file-name suffix — slower but tolerant of unknown
        //      legacy layouts. Old files from before the canonical convention are
        //      expected to live somewhere; this finds them no matter where.
        public override string GetDocumentPath(string docFileName)
        {
            if (string.IsNullOrEmpty(docFileName)) return string.Empty;

            var canonical = $"{DocumentBaseFolder}/{docFileName}".Replace('\\', '/');
            if (_documentStorage.IsFileExists(canonical).GetAwaiter().GetResult())
                return canonical;

            // Fallback: scan the container for any blob ending in this filename.
            // We type-check rather than calling on the interface so the slower
            // search only happens in Azure mode.
            if (_documentStorage is AzureStorage azure)
            {
                var found = azure.FindByFileName(docFileName).GetAwaiter().GetResult();
                if (!string.IsNullOrEmpty(found)) return found;
            }
            return string.Empty;
        }

        public override async Task<bool> SaveDocumentFileUpload(IFormFile uploadedFile, string docFileName, string thumbFileName, DocFolderHeader folderHeader)
        {
            try
            {
                var blobPath = _documentStorage.GetFilePath(string.Empty, docFileName, CPiSavedFileType.DocMgt);
                var header = new DocumentStorageHeader
                {
                    SystemType = folderHeader?.SystemType ?? "",
                    ScreenCode = folderHeader?.ScreenCode ?? "",
                    DocumentType = "DocMgt",
                    FileName = docFileName,
                    ParentId = folderHeader?.ParentId.ToString() ?? ""
                };
                await _documentStorage.SaveFile(uploadedFile, blobPath, header);
                return true;
            }
            catch (System.Exception ex)
            {
                _azureLogger.LogError(ex, "Azure: error saving document file upload: {FileName}", docFileName);
                throw;
            }
        }

        public override async Task<bool> SaveDocumentFromStream(MemoryStream stream, string docFileName, DocFolderHeader docFolder)
        {
            try
            {
                var blobPath = _documentStorage.GetFilePath(string.Empty, docFileName, CPiSavedFileType.DocMgt);
                var header = new DocumentStorageHeader
                {
                    SystemType = docFolder?.SystemType ?? "",
                    ScreenCode = docFolder?.ScreenCode ?? "",
                    DocumentType = "DocMgt",
                    FileName = docFileName,
                    ParentId = docFolder?.ParentId.ToString() ?? ""
                };
                await _documentStorage.SaveFile(stream, blobPath, header);
                return true;
            }
            catch (System.Exception ex)
            {
                _azureLogger.LogError(ex, "Azure: error saving document from stream: {FileName}", docFileName);
                throw;
            }
        }

        public override bool DeleteDocumentFile(string docFileName, string thumbFileName, bool hasImage)
        {
            try
            {
                var docPath = _documentStorage.GetFilePath(string.Empty, docFileName, CPiSavedFileType.DocMgt);
                _documentStorage.DeleteFile(docPath).GetAwaiter().GetResult();

                if (hasImage && !string.IsNullOrEmpty(thumbFileName))
                {
                    var thumbPath = _documentStorage.GetFilePath(string.Empty, thumbFileName, CPiSavedFileType.DocMgtThumbnail);
                    _documentStorage.DeleteFile(thumbPath).GetAwaiter().GetResult();
                }
                return true;
            }
            catch (System.Exception ex)
            {
                _azureLogger.LogError(ex, "Azure: error deleting document file: {FileName}", docFileName);
                return false;
            }
        }

        public override bool DeleteLetterLogFile(string docFileName)
        {
            try
            {
                var path = _documentStorage.GetFilePath(string.Empty, docFileName, CPiSavedFileType.Letter);
                _documentStorage.DeleteFile(path).GetAwaiter().GetResult();
                return true;
            }
            catch (System.Exception ex)
            {
                _azureLogger.LogError(ex, "Azure: error deleting letter log file: {FileName}", docFileName);
                return false;
            }
        }

        public override bool DeleteEFSLogFile(string docFileName)
        {
            try
            {
                var path = _documentStorage.GetFilePath(string.Empty, docFileName, CPiSavedFileType.EFS);
                _documentStorage.DeleteFile(path).GetAwaiter().GetResult();
                return true;
            }
            catch (System.Exception ex)
            {
                _azureLogger.LogError(ex, "Azure: error deleting EFS log file: {FileName}", docFileName);
                return false;
            }
        }
    }
}
