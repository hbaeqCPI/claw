using GleamTech.DocumentUltimate.AspNet.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using R10.Core.DTOs;
using R10.Core.Entities.Documents;
using System;
using System.IO;
using System.Threading.Tasks;

namespace R10.Web.Helpers
{
    public class DocumentHelper : IDocumentHelper
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DocumentHelper> _logger;

        public DocumentHelper(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ILogger<DocumentHelper> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public string GetDocumentBasePath()
        {
            return Path.Combine(_environment.ContentRootPath, "UserFiles", "Documents");
        }

        public string GetDocumentPath(string docFileName)
        {
            if (string.IsNullOrEmpty(docFileName))
                return string.Empty;

            return Path.Combine(GetDocumentBasePath(), docFileName);
        }

        public async Task<bool> SaveDocumentFileUpload(IFormFile uploadedFile, string docFileName, string thumbFileName, DocFolderHeader folderHeader)
        {
            try
            {
                var basePath = GetDocumentBasePath();
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);

                var filePath = Path.Combine(basePath, docFileName);
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(stream);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving document file upload: {FileName}", docFileName);
                return false;
            }
        }

        public async Task<bool> SaveDocumentFromStream(MemoryStream stream, string docFileName, DocFolderHeader docFolder)
        {
            try
            {
                var basePath = GetDocumentBasePath();
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);

                var filePath = Path.Combine(basePath, docFileName);
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.Position = 0;
                    await stream.CopyToAsync(fileStream);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving document from stream: {FileName}", docFileName);
                return false;
            }
        }

        public bool DeleteDocumentFile(string docFileName, string thumbFileName, bool hasImage)
        {
            try
            {
                var filePath = GetDocumentPath(docFileName);
                if (File.Exists(filePath))
                    File.Delete(filePath);

                if (!string.IsNullOrEmpty(thumbFileName) && hasImage)
                {
                    var thumbPath = GetDocumentPath(thumbFileName);
                    if (File.Exists(thumbPath))
                        File.Delete(thumbPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document file: {FileName}", docFileName);
                return false;
            }
        }

        public bool DeleteLetterLogFile(string docFileName)
        {
            try
            {
                var filePath = GetDocumentPath(docFileName);
                if (File.Exists(filePath))
                    File.Delete(filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting letter log file: {FileName}", docFileName);
                return false;
            }
        }

        public bool DeleteEFSLogFile(string docFileName)
        {
            try
            {
                var filePath = GetDocumentPath(docFileName);
                if (File.Exists(filePath))
                    File.Delete(filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting EFS log file: {FileName}", docFileName);
                return false;
            }
        }

        public DocumentViewer GetDocumentViewerModel(string docFilePath, int width, int height)
        {
            var documentViewer = new DocumentViewer
            {
                Width = width,
                Height = height,
                Resizable = true,
                Document = docFilePath
            };
            return documentViewer;
        }

        public string DataKeyToScreenCode(string dataKey)
        {
            if (string.IsNullOrEmpty(dataKey))
                return string.Empty;

            // Strip trailing "Id" or "ID" to get screen code
            if (dataKey.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                return dataKey.Substring(0, dataKey.Length - 2);

            return dataKey;
        }
    }
}
