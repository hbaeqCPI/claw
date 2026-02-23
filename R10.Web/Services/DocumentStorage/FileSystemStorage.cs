using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using static R10.Web.Helpers.ImageHelper;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace R10.Web.Services.DocumentStorage
{
    public class FileSystemStorage: DocumentStorage,IDocumentStorage
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IFileProvider _fileProvider;

        private const string _imageRootFolder = @"UserFiles\Searchable\Documents";
        private const string _efsLogFolder = @"UserFiles\Searchable\Logs\EFS";
        private const string _letterLogFolder = @"UserFiles\Searchable\Logs\Letters";
        private const string _docxLogFolder = @"UserFiles\Searchable\Logs\DOCXes";
        private const string _letterTemplateFolder = @"UserFiles\Letters\Templates";
        private const string _calendarFileFolder = @"UserFiles\Calendar";
        private const string _reportLogFolder = @"UserFiles\ReportLogs";

        public FileSystemStorage(IDocumentService docService,
                                 IHostingEnvironment hostingEnvironment,
                                 IAsyncRepository<DocFixedFolder> docFixFolderRepository,
                                 IFileProvider fileProvider) : base(docService, docFixFolderRepository)
        {
            _hostingEnvironment = hostingEnvironment;
            _fileProvider = fileProvider;
        }

        public string EFSLogFolder => _efsLogFolder;
        public string LetterLogFolder => _letterLogFolder;
        public string ImageRootFolder => _imageRootFolder;
        public string DOCXLogFolder => _docxLogFolder;
        public string LetterTemplateFolder => _letterTemplateFolder;
        public string CalendarFileFolder => _calendarFileFolder;
        public string ReportLogFolder => _reportLogFolder;

        public async Task<CPIFile> GetFileStream(string system, string fileName, CPiSavedFileType savedFileType) {
            
            var path = GetFilePath(system, fileName, savedFileType);
            FileInfo file = new FileInfo(path);

            if (file.Exists)
            {
                var originalFileName = await GetRealFileName(fileName);
                var stream = file.OpenRead();
                return new CPIFile
                {
                    FileName = fileName,
                    OrigFileName = string.IsNullOrEmpty(originalFileName) ? fileName : originalFileName,
                    ContentType = ImageHelper.GetContentType(fileName),
                    Stream = stream
                };
            }
            else
                return null;
        }
       
        public async Task SaveFile(byte[] buffer, string path, DocumentStorageHeader header)
        {
             await File.WriteAllBytesAsync(path, buffer);
        }

        public string BuildPath(string folder, string system, string fileName)
        {
            if (!Directory.Exists(Path.Combine(_hostingEnvironment.ContentRootPath, folder))) {
                var folders = folder.Split(@"\");
                var path = "";
                foreach (var item in folders) {
                    if (string.IsNullOrEmpty(path))
                        path = item;
                    else
                        path = @$"{path}\{item}";

                    if (!Directory.Exists(Path.Combine(_hostingEnvironment.ContentRootPath, path)))
                        Directory.CreateDirectory(Path.Combine(_hostingEnvironment.ContentRootPath, path));
                }
            }
            return Path.Combine(_hostingEnvironment.ContentRootPath, folder, fileName);
        }

        public async Task<MemoryStream> GetFileStream(string fileName)
        {
            FileInfo file = new FileInfo(fileName);

            if (file.Exists)
            {
                var memoryStream = new MemoryStream();
                using (var fileStream = file.OpenRead()) {
                    fileStream.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                }
                return memoryStream;
            }
            return null;
        }

        public async Task CopyImageFile(string imageFile, string sourceSystem, string destinationSystem, DocumentStorageHeader header)
        {
            if (!string.IsNullOrEmpty(imageFile))
            {
                var source = Path.Combine(Directory.GetCurrentDirectory(), _imageRootFolder, sourceSystem, imageFile);
                var destination = Path.Combine(Directory.GetCurrentDirectory(), _imageRootFolder, destinationSystem, imageFile);
                if (File.Exists(source) && !File.Exists(destination))
                {
                    File.Copy(source, destination);
                }
            }
        }

        #region abstract overrides
        public override string GetFilePath(string system, string fileName, CPiSavedFileType savedFileType)
        {
            return ImageHelper.GetPhysicalFilePath(system, fileName, savedFileType);
        }

        public override async Task SaveFile(IFormFile file, string path, DocumentStorageHeader header)
        {
            await ImageHelper.SaveFile(file, path);

            if (!string.IsNullOrEmpty(header.ThumbnailPath)) {
                await CreateAndSaveThumbnail(path, header.ThumbnailPath);
            }
        }

        protected async Task CreateAndSaveThumbnail(string imageFilePath, string fileName)
        {
            ImageHelper.CreateAndSaveThumbnail(imageFilePath, fileName);
        }

        public override async Task DeleteFile(string path)
        {
            ImageHelper.DeleteFile(path);
        }

        public override async Task CreateSnaphot(string path)
        {
        }

        public override List<LetterTemplateViewModel> GetListOfFiles(string path)
        {
            IDirectoryContents contents = _fileProvider.GetDirectoryContents(path);
            var result = contents.ToList().OrderBy(f => f.Name).Select(f => new LetterTemplateViewModel {TemplateFile = f.Name, FileSize = f.Length});
            return result.ToList();
        }

        public override async Task<bool> IsFileExists(string physicalPath)
        {
            return ImageHelper.IsFileExists(physicalPath);
        }

        public override async Task CopyFile(string sourcePath, string destinationPath, DocumentStorageHeader header)
        {
            ImageHelper.CopyFile(sourcePath, destinationPath);

            if (!string.IsNullOrEmpty(header.ThumbnailPath))
            {
                await CreateAndSaveThumbnail(destinationPath, header.ThumbnailPath);
            }
        }

        public override async Task<byte[]> ConvertImage(string system, string fileName)
        {
            var path = ImageHelper.GetPhysicalFilePath(system, fileName, CPiSavedFileType.Image);
            var bytes = File.ReadAllBytes(path);
            return bytes;
        }

        protected override async Task<long> GetSavedFileSize(string filePath) {
            return ImageHelper.GetSavedFileSize(filePath);
        }

        public override async Task SaveFiles(List<DocumentStorageFile> files)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateFileMetadata(List<DocumentStorageFile> files)
        {
            throw new NotImplementedException();
        }

        public byte[] CompressDocuments(List<DocDocumentListViewModel> files, string system,string userName)
        {

            var zipFile = BuildCompressFileName(userName);
            using (var archive = ZipFile.Open(zipFile, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    var path = GetFilePath(system, file.DocFileName, CPiSavedFileType.DocMgt);
                    FileInfo sourceFileInfo = new FileInfo(path);

                    if (sourceFileInfo.Exists) {
                        var fileName = file.UserFileName.Replace("?", "").Replace(@"\", "").Replace(@"/", "")
                                     .Replace(":", "").Replace("*", "").Replace("<", "")
                                     .Replace(">", "").Replace("|", "");
                        archive.CreateEntryFromFile(path, fileName);

                        //archive.CreateEntryFromFile(path, file.UserFileName);
                    }
                }
            }

            FileInfo fileInfo = new FileInfo(zipFile);
            var memoryStream = new MemoryStream();
            using (var fileStream = fileInfo.OpenRead())
            {
                fileStream.CopyTo(memoryStream);
            }
            try
            {
                fileInfo.Delete();
            }
            catch (Exception ex)
            {
            }
            memoryStream.Position = 0;
            return memoryStream.ToArray();
        }
        public byte[] CompressGSDocuments(List<GSDownloadDTO> file)
        {
            return null;
        }


        #endregion


    }
}
