using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Shared;
using R10.Core.Interfaces;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static R10.Web.Helpers.ImageHelper;

namespace R10.Web.Services.DocumentStorage
{
    public class AzureStorage : DocumentStorage, IDocumentStorage
    {
        protected readonly ISystemSettings<DefaultSetting> _settings;
        protected readonly IConfiguration _configuration;

        //private const string _imageContainerName = "userfiles"; //must be lowercase
        private const string _imageRootFolder = "Searchable/Documents";
        private const string _imageThumbnailRootFolder = "Thumbnails";
        private const string _letterLogFolder = "Searchable/Logs/Letters";
        private const string _qeLogFolder = "Searchable/Logs/QuickEmails";
        private const string _qeLogAttachmentFolder = "Logs/QuickEmails";
        private const string _efsLogFolder = "Searchable/Logs/EFS";
        private const string _documentRootFolder = "Searchable/Documents";
        private const string _documentThumbnailFolder = "Thumbnails";
        private const string _docxLogFolder = "Searchable/Logs/DOCXes";
        private const string _letterTemplateFolder = "Letters/Templates";
        private const string _calendarFileFolder = "Calendar";
        private const string _reportLogFolder = "ReportLogs";

        public AzureStorage(ISystemSettings<DefaultSetting> settings,
                            IDocumentService docService,
                            IAsyncRepository<DocFixedFolder> docFixFolderRepository,
                            IConfiguration configuration) : base(docService, docFixFolderRepository)
        {
            _settings = settings;
            _configuration = configuration;
        }

        public string EFSLogFolder => _efsLogFolder;
        public string LetterLogFolder => _letterLogFolder;
        public string ImageRootFolder => _imageRootFolder;
        public string ImageThumbnailRootFolder => _imageThumbnailRootFolder;
        public string EmailLogFolder => _qeLogFolder;
        public string DocumentRootFolder => _documentRootFolder;
        public string DocumentThumbnailFolder => _documentThumbnailFolder;
        public string DOCXLogFolder => _docxLogFolder;
        public string LetterTemplateFolder => _letterTemplateFolder;
        public string CalendarFileFolder => _calendarFileFolder;
        public string ReportLogFolder => _reportLogFolder;

        public async Task<CPIFile> GetFileStream(string system, string fileName, CPiSavedFileType savedFileType)
        {
            var path = GetFilePath(system, fileName, savedFileType);

            var container = GetContainer();
            var blob = container.GetBlobClient(path);

            if (blob.Exists())
            {

                var originalFileName = await GetRealFileName(fileName);
                var stream = new MemoryStream();
                blob.DownloadTo(stream);
                stream.Position = 0;
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

        public async Task<MemoryStream> GetFileStream(string fileName)
        {
            fileName = fileName.Replace(@"\", "/");

            var container = GetContainer();
            var blob = container.GetBlobClient(fileName);

            if (blob.Exists())
            {
                var stream = new MemoryStream();
                blob.DownloadTo(stream);
                stream.Position = 0;
                return stream;
            }
            else
                return null;
        }

        public async Task SaveFile(byte[] buffer, string path, DocumentStorageHeader header)
        {
            var container = GetContainer();
            var blob = container.GetBlobClient(path);
            var stream = new MemoryStream(buffer);
            stream.Position = 0;
            await blob.UploadAsync(stream);

            if (header != null)
            {
                var metadata = HeaderToDictionary(header);
                await blob.SetMetadataAsync(metadata);
            }
        }

        public async Task SaveFile(MemoryStream stream, string path, DocumentStorageHeader header)
        {

            var container = GetContainer();
            var blob = container.GetBlobClient(path);
            if (!blob.Exists())
            {
                stream.Position = 0;
                await blob.UploadAsync(stream);

                if (header != null)
                {
                    var metadata = HeaderToDictionary(header);
                    await blob.SetMetadataAsync(metadata);
                }

                if (!string.IsNullOrEmpty(header.ThumbnailPath))
                {
                    var thumbnailStream = CreateThumbnail(stream, header.ThumbnailPath);

                    var thumbnailBlob = container.GetBlobClient(header.ThumbnailPath);
                    if (!thumbnailBlob.Exists())
                    {
                        thumbnailStream.Position = 0;
                        await thumbnailBlob.UploadAsync(thumbnailStream);
                    }
                }
            }
        }

        public string BuildPath(string folder, string system, string fileName)
        {
            return Path.Combine(folder, fileName).Replace(@"\", "/");
            //return Path.Combine(folder, system, fileName).Replace(@"\", "/");
        }

        public async Task CopyImageFile(string imageFile, string sourceSystem, string destinationSystem, DocumentStorageHeader header)
        {
            if (!string.IsNullOrEmpty(imageFile))
            {
                var source = $@"{_imageRootFolder}/{sourceSystem}/{imageFile}";
                var destination = $@"{_imageRootFolder}/{destinationSystem}/{imageFile}";

                var container = GetContainer();
                var sourceBlob = container.GetBlobClient(source);
                if (sourceBlob.Exists())
                {
                    var destBlob = container.GetBlobClient(destination);
                    if (!destBlob.Exists())
                    {
                        await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);
                        if (header != null)
                        {
                            var metadata = HeaderToDictionary(header);
                            await destBlob.SetMetadataAsync(metadata);
                        }
                    }
                }
            }
        }

        protected BlobContainerClient GetContainer()
        {
            var settings = _settings.GetSetting().GetAwaiter().GetResult();
            var storageSettings = _configuration.GetSection("DocumentStorage").Get<DocumentStorageSettings>();
            var containerName = GetBlobContainerName(storageSettings);
            
            var credential = new ClientSecretCredential(storageSettings.StorageADTenantID, storageSettings.StorageAppClientID, storageSettings.StorageAppClientSecret);
            var containerEndpoint = string.Format(storageSettings.StorageUrl, storageSettings.StorageAccountName, containerName);
            var container = new BlobContainerClient(new Uri(containerEndpoint), credential);
            if (!container.Exists())
                container.Create();

            return container;
        }


        public string GetBlobContainerName(DocumentStorageSettings storageSettings = null)      // factored out for re-use by search indexer creation
        {
            if (storageSettings == null)
                storageSettings = _configuration.GetSection("DocumentStorage").Get<DocumentStorageSettings>();

            var containerName = storageSettings.StorageContainerName.ToLower();
            if (string.IsNullOrEmpty(containerName))
            {
                var clientCode = _settings.GetValue<string>("CPIClientCode", "1").GetAwaiter().GetResult();
                if (!(clientCode.ToLower() == "demo"))
                    containerName = clientCode.ToLower();
            }
            
            if (string.IsNullOrEmpty(containerName))
                throw new Exception("Storage container name must be specified.");

            return containerName;
        }

        public async Task CreateAndSaveThumbnail(Stream source, string filename)
        {
            var thumbnailStream = CreateThumbnail(source, filename);

            var container = GetContainer();
            var thumbnailBlob = container.GetBlobClient(filename);
            if (!thumbnailBlob.Exists())
            {
                thumbnailStream.Position = 0;
                await thumbnailBlob.UploadAsync(thumbnailStream);
            }
        }

        private Stream CreateThumbnail(Stream source, string filename)
        {
            var width = 80; //128;
            var height = 80; //128;

            var stream = new MemoryStream();

            using (var image = new Bitmap(source))
            {
                var resized = new Bitmap(width, height);
                using (var graphics = Graphics.FromImage(resized))
                {
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.DrawImage(image, 0, 0, width, height);
                    resized.Save(stream, GetThumbnailImageFormat(filename));
                    return stream;
                }
            }
        }

        private static ImageFormat GetThumbnailImageFormat(string filename)
        {
            var extension = Path.GetExtension(filename).Replace(".", "").ToLower();
            switch (extension)
            {
                case "bmp": return ImageFormat.Bmp;
                case "jpeg": return ImageFormat.Jpeg;
                case "jpg": return ImageFormat.Jpeg;
                default:
                    return ImageFormat.Png;
            }
        }

        private Dictionary<string, string> HeaderToDictionary(DocumentStorageHeader header)
        {
            var properties = header.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         .Where(p => p.Name != "ThumbnailPath" && !string.IsNullOrEmpty((string)p.GetValue(header, null)))
                         .ToDictionary(prop => prop.Name, prop => (string)prop.GetValue(header, null));
            return properties;
        }

        #region abstract overrides
        public override string GetFilePath(string system, string fileName, CPiSavedFileType savedFileType)
        {
            var rootFolder = "";
            system = "";

            switch (savedFileType)
            {

                case CPiSavedFileType.Image:
                    rootFolder = _imageRootFolder;
                    break;

                case CPiSavedFileType.Thumbnail:
                    rootFolder = _imageThumbnailRootFolder;
                    break;

                case CPiSavedFileType.QE:
                    rootFolder = _qeLogFolder;
                    break;

                case CPiSavedFileType.QELoggedImage:
                    rootFolder = _qeLogAttachmentFolder;
                    break;

                case CPiSavedFileType.IDSReferences:
                    rootFolder = _imageRootFolder;
                    break;

                case CPiSavedFileType.Letter:
                    rootFolder = _letterLogFolder;
                    break;

                case CPiSavedFileType.EFS:
                    rootFolder = _efsLogFolder;
                    break;

                case CPiSavedFileType.DocMgt:
                    rootFolder = _documentRootFolder;
                    break;

                case CPiSavedFileType.DocMgtThumbnail:
                    rootFolder = _documentThumbnailFolder;
                    break;

                case CPiSavedFileType.QELoggedImageThumbnail:
                    rootFolder = _imageThumbnailRootFolder;
                    break;
                
                case CPiSavedFileType.Assignment:
                    rootFolder = _imageRootFolder;
                    break;

                case CPiSavedFileType.Licensees:
                    rootFolder = _imageRootFolder;
                    break;

                case CPiSavedFileType.Calendar:
                    rootFolder = _calendarFileFolder;
                    break;

                case CPiSavedFileType.ReportFile:
                    rootFolder = _reportLogFolder;
                    break;

                default:
                    rootFolder = _imageRootFolder;
                    break;
            }

            if (string.IsNullOrEmpty(rootFolder))
                return string.Empty;


            fileName = Path.GetFileName(fileName);
            string path = Path.Combine(rootFolder, system,  fileName);
            path = path.Replace('\\', '/');
            return path;
        }

        public override async Task DeleteFile(string path)
        {
            var container = GetContainer();
            var blob = container.GetBlobClient(path);
            if (blob.Exists())
                await blob.DeleteAsync();
        }

        public override async Task CreateSnaphot(string path)
        {
            var container = GetContainer();
            var blob = container.GetBlobClient(path);
            if (blob.Exists())
                await blob.CreateSnapshotAsync();
        }

        public override List<LetterTemplateViewModel> GetListOfFiles(string path)
        {
            var container = GetContainer();
            path = path.Replace('\\', '/');

            var files =  container.GetBlobs(BlobTraits.None, BlobStates.None, path);
            var list = new List<LetterTemplateViewModel>();
            foreach (var file in files) {
                var paths = file.Name.Split('/');
                var fileName = paths.Length > 0 ? paths[paths.Length-1] : string.Empty;

                var properties = file.Properties;

                if (!string.IsNullOrEmpty(fileName)) { 
                    list.Add(new LetterTemplateViewModel {TemplateFile=fileName,FileSize=Convert.ToInt32(properties.ContentLength)});
                }
            }
            return list;
        }

        public override async Task SaveFile(IFormFile file, string path, DocumentStorageHeader header)
        {
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                await SaveFile(stream, path, header);
            }
        }

        public override async Task<bool> IsFileExists(string physicalPath)
        {
            var container = GetContainer();
            var blob = container.GetBlobClient(physicalPath);
            return blob.Exists();
        }

        public override async Task CopyFile(string sourcePath, string destinationPath, DocumentStorageHeader header)
        {
            var sourceStream = await GetFileStream(sourcePath);

            if (sourceStream != null)
            {
                await SaveFile(sourceStream, destinationPath, header);
            }
        }

        public override async Task SaveFiles(List<DocumentStorageFile> files)
        {
            var container = GetContainer();
            foreach (var item in files)
            {
                var updateHeader = item.Header != null;
                var blob = container.GetBlobClient(item.FileName);
                if (!blob.Exists())
                {
                    if (item.Buffer != null && item.Buffer.Length > 0)
                    {
                        var stream = new MemoryStream(item.Buffer);
                        stream.Position = 0;
                        await blob.UploadAsync(stream);
                    }
                    else {
                        updateHeader = false;
                    }
                }

                if (updateHeader)
                {
                    var metadata = HeaderToDictionary(item.Header);
                    await blob.SetMetadataAsync(metadata);
                }
            }
        }

        public async Task UpdateFileMetadata(List<DocumentStorageFile> files)
        {
            var container = GetContainer();
            foreach (var item in files)
            {
                var blob = container.GetBlobClient(item.FileName);
                if (blob.Exists())
                {
                    var metadata = HeaderToDictionary(item.Header);
                    await blob.SetMetadataAsync(metadata);
                }
            }
        }

        public byte[] CompressDocuments(List<DocDocumentListViewModel> files, string system, string userName)
        {
            byte[] compressedFiles = null;

            GetDocuments(files, system);
            using (var memoryStream = new MemoryStream())
            {
                using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        if (file.FileBytes != null) {
                            var fileName = file.UserFileName.Replace("?", "").Replace(@"\", "").Replace(@"/", "")
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
            return compressedFiles;
        }

        public byte[] CompressGSDocuments(List<GSDownloadDTO> files)
        {
            byte[] compressedFiles = null;

            GetGSDocuments(files);
            using (var memoryStream = new MemoryStream())
            {
                using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        ZipArchiveEntry zipItem = zip.CreateEntry(file.UserFileName);
                        using (var originalStream = new MemoryStream(file.FileBytes))
                        {
                            using (var entryStream = zipItem.Open())
                            {
                                originalStream.CopyTo(entryStream);
                            }
                        }
                    }
                }
                compressedFiles = memoryStream.ToArray();
            }
            return compressedFiles;

        }

        public override async Task<byte[]> ConvertImage(string system, string fileName)
        {
            var path = GetFilePath(system, fileName, CPiSavedFileType.Image);

            var container = GetContainer();
            var blob = container.GetBlobClient(path);
            if (blob.Exists())
            {
                using (var stream = new MemoryStream())
                {
                    blob.DownloadTo(stream);
                    stream.Position = 0;
                    return stream.ToArray();
                }
            }
            return null;
        }

        protected override async Task<long> GetSavedFileSize(string filePath)
        {
            var container = GetContainer();
            var blob = container.GetBlobClient(filePath);
            if (blob.Exists())
            {
                BlobProperties properties = await blob.GetPropertiesAsync();
                return properties.ContentLength;
            }
            return 0;
        }

        protected void GetDocuments(List<DocDocumentListViewModel> files, string system)
        {
            var container = GetContainer();

            foreach (var file in files)
            {
                var path = GetFilePath(system, file.DocFileName, CPiSavedFileType.DocMgt);
                var blob = container.GetBlobClient(path);
                if (blob.Exists())
                {
                    using (var stream = new MemoryStream())
                    {
                        blob.DownloadTo(stream);
                        stream.Position = 0;
                        file.FileBytes = stream.ToArray();
                    }
                }
            }
        }

        protected void GetGSDocuments(List<GSDownloadDTO> files)
        {
            var container = GetContainer();

            foreach (var file in files)
            {
                var system = ImageHelper.GetSystemName(file.SystemType);
                var path = GetFilePath(system, file.DocFileName, DocumentTypeToSavedFileType(file.DocumentType));
                var blob = container.GetBlobClient(path);
                if (blob.Exists())
                {
                    using (var stream = new MemoryStream())
                    {
                        blob.DownloadTo(stream);
                        stream.Position = 0;
                        file.FileBytes = stream.ToArray();
                    }
                }
            }

        }

        private CPiSavedFileType DocumentTypeToSavedFileType(string docType)
        {
            switch (docType)
            {
                case "IDSDoc": return CPiSavedFileType.IDSReferences;
                case "LetterLog": return CPiSavedFileType.Letter;
                case "EFSLog": return CPiSavedFileType.EFS;
                case "EmailLog": return CPiSavedFileType.QE;
                case "EmailLogAttachment": return CPiSavedFileType.QELoggedImage;
                case "DocMgt": return CPiSavedFileType.DocMgt;
                default: return CPiSavedFileType.Image;
            }
        }

        #endregion
    }
}
