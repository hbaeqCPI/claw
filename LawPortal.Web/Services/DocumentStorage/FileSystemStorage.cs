using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using LawPortal.Web.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static LawPortal.Web.Helpers.ImageHelper;

namespace LawPortal.Web.Services.DocumentStorage
{
    // Local file system implementation. Mirrors the structure of the R10 reference
    // but trimmed to the methods LawPortal actually calls.
    public class FileSystemStorage : IDocumentStorage
    {
        private readonly IWebHostEnvironment _env;

        private const string _imageRootFolder = @"UserFiles\Searchable\Documents";
        private const string _documentRootFolder = @"UserFiles\Searchable\Documents";

        public FileSystemStorage(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string ImageRootFolder => _imageRootFolder;
        public string DocumentRootFolder => _documentRootFolder;

        public string GetFilePath(string system, string fileName, CPiSavedFileType savedFileType)
        {
            return ImageHelper.GetPhysicalFilePath(system, fileName, savedFileType);
        }

        public async Task<CPIFile?> GetFileStream(string system, string fileName, CPiSavedFileType savedFileType)
        {
            var path = GetFilePath(system, fileName, savedFileType);
            var file = new FileInfo(path);
            if (!file.Exists) return null;

            var stream = file.OpenRead();
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
            var file = new FileInfo(path);
            if (!file.Exists) return null;

            var memoryStream = new MemoryStream();
            using (var fileStream = file.OpenRead())
            {
                fileStream.CopyTo(memoryStream);
                memoryStream.Position = 0;
            }
            return memoryStream;
        }

        public async Task SaveFile(byte[] buffer, string path, DocumentStorageHeader? header)
        {
            EnsureDirectory(path);
            await File.WriteAllBytesAsync(path, buffer);
        }

        public async Task SaveFile(IFormFile file, string path, DocumentStorageHeader? header)
        {
            EnsureDirectory(path);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        public async Task SaveFile(MemoryStream stream, string path, DocumentStorageHeader? header)
        {
            EnsureDirectory(path);
            stream.Position = 0;
            using var fileStream = new FileStream(path, FileMode.Create);
            await stream.CopyToAsync(fileStream);
        }

        public async Task<bool> IsFileExists(string physicalPath)
        {
            return File.Exists(physicalPath);
        }

        public async Task DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        public async Task CopyFile(string sourcePath, string destinationPath, DocumentStorageHeader? header)
        {
            EnsureDirectory(destinationPath);
            File.Copy(sourcePath, destinationPath, true);
        }

        public async Task<string> SaveFileStreamToPath(string fileName, string downloadFilePath)
        {
            // For file system mode, "downloading" is the same as referencing the existing local file.
            return File.Exists(fileName) ? fileName : string.Empty;
        }

        private static void EnsureDirectory(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}
