using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using LawPortal.Web.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static LawPortal.Web.Helpers.ImageHelper;

namespace LawPortal.Web.Services.DocumentStorage
{
    // Azure Blob Storage implementation. Mirrors R10's AzureStorage but trimmed
    // to the methods LawPortal actually uses (no compression / GS / thumbnail logic).
    // Authenticates using a service principal (Tenant + ClientId + ClientSecret) bound
    // from appsettings DocumentStorage section.
    public class AzureStorage : IDocumentStorage
    {
        private readonly IConfiguration _configuration;

        // Folder layout inside the blob container — slash-separated (blob convention).
        private const string _imageRootFolder = "Searchable/Documents";
        private const string _imageThumbnailRootFolder = "Thumbnails";
        private const string _letterLogFolder = "Searchable/Logs/Letters";
        private const string _qeLogFolder = "Searchable/Logs/QuickEmails";
        private const string _qeLogAttachmentFolder = "Logs/QuickEmails";
        private const string _efsLogFolder = "Searchable/Logs/EFS";
        private const string _documentRootFolder = "Searchable/Documents";
        private const string _documentThumbnailFolder = "Thumbnails";
        private const string _calendarFileFolder = "Calendar";
        private const string _reportLogFolder = "ReportLogs";

        public AzureStorage(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ImageRootFolder => _imageRootFolder;
        public string DocumentRootFolder => _documentRootFolder;

        public string GetFilePath(string system, string fileName, CPiSavedFileType savedFileType)
        {
            // For blob storage we ignore "system" subfolder, just use type-based root.
            var rootFolder = savedFileType switch
            {
                CPiSavedFileType.Image => _imageRootFolder,
                CPiSavedFileType.Thumbnail => _imageThumbnailRootFolder,
                CPiSavedFileType.QE => _qeLogFolder,
                CPiSavedFileType.QELoggedImage => _qeLogAttachmentFolder,
                CPiSavedFileType.IDSReferences => _imageRootFolder,
                CPiSavedFileType.Letter => _letterLogFolder,
                CPiSavedFileType.EFS => _efsLogFolder,
                CPiSavedFileType.DocMgt => _documentRootFolder,
                CPiSavedFileType.DocMgtThumbnail => _documentThumbnailFolder,
                CPiSavedFileType.QELoggedImageThumbnail => _imageThumbnailRootFolder,
                CPiSavedFileType.Calendar => _calendarFileFolder,
                CPiSavedFileType.ReportFile => _reportLogFolder,
                _ => _imageRootFolder,
            };

            fileName = Path.GetFileName(fileName);
            var path = Path.Combine(rootFolder, fileName).Replace('\\', '/');
            return path;
        }

        public async Task<CPIFile?> GetFileStream(string system, string fileName, CPiSavedFileType savedFileType)
        {
            var path = GetFilePath(system, fileName, savedFileType);
            var container = GetContainer();
            var blob = container.GetBlobClient(path);
            if (!blob.Exists()) return null;

            var stream = new MemoryStream();
            await blob.DownloadToAsync(stream);
            stream.Position = 0;
            return new CPIFile
            {
                FileName = fileName,
                OrigFileName = fileName,
                ContentType = ImageHelper.GetContentType(fileName),
                Stream = stream
            };
        }

        public async Task<MemoryStream?> GetFileStream(string path)
        {
            path = path.Replace('\\', '/');
            var container = GetContainer();
            var blob = container.GetBlobClient(path);
            if (!blob.Exists()) return null;

            var stream = new MemoryStream();
            await blob.DownloadToAsync(stream);
            stream.Position = 0;
            return stream;
        }

        public async Task SaveFile(byte[] buffer, string path, DocumentStorageHeader? header)
        {
            path = path.Replace('\\', '/');
            var container = GetContainer();
            var blob = container.GetBlobClient(path);
            using var stream = new MemoryStream(buffer);
            stream.Position = 0;
            await blob.UploadAsync(stream, overwrite: true);
            await ApplyMetadata(blob, header);
        }

        public async Task SaveFile(IFormFile file, string path, DocumentStorageHeader? header)
        {
            path = path.Replace('\\', '/');
            var container = GetContainer();
            var blob = container.GetBlobClient(path);
            using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, overwrite: true);
            await ApplyMetadata(blob, header);
        }

        public async Task SaveFile(MemoryStream stream, string path, DocumentStorageHeader? header)
        {
            path = path.Replace('\\', '/');
            var container = GetContainer();
            var blob = container.GetBlobClient(path);
            stream.Position = 0;
            await blob.UploadAsync(stream, overwrite: true);
            await ApplyMetadata(blob, header);
        }

        public async Task<bool> IsFileExists(string physicalPath)
        {
            physicalPath = physicalPath.Replace('\\', '/');
            var container = GetContainer();
            var blob = container.GetBlobClient(physicalPath);
            return await blob.ExistsAsync();
        }

        public async Task DeleteFile(string path)
        {
            path = path.Replace('\\', '/');
            var container = GetContainer();
            var blob = container.GetBlobClient(path);
            if (await blob.ExistsAsync())
                await blob.DeleteAsync();
        }

        public async Task CopyFile(string sourcePath, string destinationPath, DocumentStorageHeader? header)
        {
            sourcePath = sourcePath.Replace('\\', '/');
            destinationPath = destinationPath.Replace('\\', '/');
            var container = GetContainer();
            var sourceBlob = container.GetBlobClient(sourcePath);
            if (!await sourceBlob.ExistsAsync()) return;

            var destBlob = container.GetBlobClient(destinationPath);
            await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);
            await ApplyMetadata(destBlob, header);
        }

        // Search the entire container for any blob whose name ends in the given file name.
        // Used as a fallback when files were uploaded under unknown path conventions (old
        // pre-LawPortal manual uploads, etc.). Returns the full blob path or empty if nothing
        // matches. Linear scan — fine when the container is small; cache externally if needed.
        public async Task<string> FindByFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(fileName)) return string.Empty;

            var container = GetContainer();
            var suffix = "/" + fileName;
            await foreach (var blob in container.GetBlobsAsync())
            {
                if (blob.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase)
                    || blob.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return blob.Name;
                }
            }
            return string.Empty;
        }

        // List blobs in the container, optionally filtered by prefix. Diagnostic helper —
        // surfaces actual storage layout when path conventions drift over time.
        public async Task<List<string>> ListBlobs(string? prefix = null, int max = 100)
        {
            var container = GetContainer();
            var results = new List<string>();
            await foreach (var blob in container.GetBlobsAsync(prefix: prefix))
            {
                results.Add(blob.Name);
                if (results.Count >= max) break;
            }
            return results;
        }

        // Download a blob to a known local file path. Returns the local path on success
        // (so callers like the 32-bit MDB sidecar can pass it to Process.Start), or "" if missing.
        public async Task<string> SaveFileStreamToPath(string fileName, string downloadFilePath)
        {
            fileName = fileName.Replace('\\', '/');
            var container = GetContainer();
            var blob = container.GetBlobClient(fileName);
            if (!await blob.ExistsAsync()) return string.Empty;

            var dir = Path.GetDirectoryName(downloadFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            await blob.DownloadToAsync(downloadFilePath);
            return downloadFilePath;
        }

        public BlobContainerClient GetContainer()
        {
            var settings = _configuration.GetSection("DocumentStorage").Get<DocumentStorageSettings>()
                            ?? throw new InvalidOperationException("DocumentStorage settings missing in appsettings.json.");

            var containerName = settings.StorageContainerName.ToLower();
            if (string.IsNullOrEmpty(containerName))
                throw new InvalidOperationException("DocumentStorage:StorageContainerName must be specified.");

            var credential = new ClientSecretCredential(
                settings.StorageADTenantID,
                settings.StorageAppClientID,
                settings.StorageAppClientSecret);

            var containerEndpoint = string.Format(settings.StorageUrl, settings.StorageAccountName, containerName);
            var container = new BlobContainerClient(new Uri(containerEndpoint), credential);

            if (!container.Exists())
                container.Create();

            return container;
        }

        private static async Task ApplyMetadata(BlobClient blob, DocumentStorageHeader? header)
        {
            if (header == null) return;
            var metadata = header.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.Name != nameof(DocumentStorageHeader.ThumbnailPath))
                .Select(p => new { p.Name, Value = (string?)p.GetValue(header) })
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .ToDictionary(x => x.Name, x => x.Value!);
            if (metadata.Count == 0) return;
            try
            {
                await blob.SetMetadataAsync(metadata);
            }
            catch
            {
                // Best-effort: if the SP can write blob data but not metadata, we still
                // want the upload to count as a success. The blob is already in place.
            }
        }
    }
}
