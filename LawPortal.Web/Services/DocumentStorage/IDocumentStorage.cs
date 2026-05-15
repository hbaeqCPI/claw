using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LawPortal.Web.Services.DocumentStorage
{
    // Storage abstraction — two implementations (FileSystemStorage, AzureStorage)
    // selected via DI based on DocumentStorage:UseFileSystem in appsettings.
    // Trimmed compared to R10 to only the methods LawPortal actually uses.
    public interface IDocumentStorage
    {
        Task SaveFile(byte[] buffer, string path, DocumentStorageHeader? header);
        Task SaveFile(IFormFile file, string path, DocumentStorageHeader? header);
        Task SaveFile(MemoryStream stream, string path, DocumentStorageHeader? header);

        Task<MemoryStream?> GetFileStream(string path);
        Task<CPIFile?> GetFileStream(string system, string fileName, Helpers.ImageHelper.CPiSavedFileType savedFileType);

        Task<bool> IsFileExists(string physicalPath);
        Task DeleteFile(string path);
        Task CopyFile(string sourcePath, string destinationPath, DocumentStorageHeader? header);

        // Build a storage path (local FS path, or blob-relative path) for the given (system, fileName, type).
        string GetFilePath(string system, string fileName, Helpers.ImageHelper.CPiSavedFileType savedFileType);

        // Download to a known local path (so 32-bit Mdb sidecar can read it). Returns local path or empty.
        Task<string> SaveFileStreamToPath(string fileName, string downloadFilePath);

        string DocumentRootFolder { get; }
        string ImageRootFolder { get; }
    }
}
