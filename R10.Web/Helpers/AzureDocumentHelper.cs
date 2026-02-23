using GleamTech.DocumentUltimate;
using GleamTech.DocumentUltimate.AspNet;
using GleamTech.DocumentUltimate.AspNet.UI;
using GleamTech.FileProviders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Web.Services.DocumentStorage;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace R10.Web.Helpers
{
    public class AzureDocumentHelper : DocumentHelper, IDocumentHelper
    {
        //private const string UserFileBaseFolder = "userfiles"; //must be lowercase
        private const string DocumentBaseFolder = "Searchable/Documents";
        private const string DocumentThumbnailFolder = "Thumbnails";
        private const string QuickEmailLogFolder = "Searchable/Logs/QuickEmails";
        private const string QuickEmailAttachmentLogFolder = "Logs/QuickEmails";

        private readonly AzureStorage _documentStorage;
        private readonly ILogger _logger;

        public AzureDocumentHelper(ILogger<DocumentHelper> logger,
                                   AzureStorage documentStorage,
                                   IHostingEnvironment hostingEnvironment) :base(logger, hostingEnvironment)
        {
            _logger = logger;
            _documentStorage = documentStorage;
        }

        public override async Task<bool> SaveDocumentFileUpload(IFormFile uploadedFile, string docFileName, string thumbFileName, DocFolderHeader docFolder)
        {
            var isImage = uploadedFile.ContentType.Contains("image");
            var docFilePath = _documentStorage.GetFilePath(string.Empty, docFileName, ImageHelper.CPiSavedFileType.DocMgt);
            //var sourceStream = await _documentStorage.GetFileStream(docFilePath);
            var documentHeader = new DocumentStorageHeader
            {
                SystemType = docFolder.SystemType,
                ScreenCode = docFolder.ScreenCode,
                DocumentType = DocumentLogType.DocMgt,
                FileName = docFileName,
                ParentId = docFolder.ParentId.ToString()
            };
            if (isImage)
            {
                documentHeader.ThumbnailPath = _documentStorage.GetFilePath(string.Empty, thumbFileName, ImageHelper.CPiSavedFileType.DocMgtThumbnail);
            }
            await _documentStorage.SaveFile(uploadedFile, docFilePath, documentHeader);
            return true;
            
        }

        public override async Task<bool> SaveDocumentFromStream(MemoryStream stream, string docFileName, DocFolderHeader docFolder)
        {
            var docFilePath = _documentStorage.GetFilePath(string.Empty, docFileName, ImageHelper.CPiSavedFileType.DocMgt);
            var documentHeader = new DocumentStorageHeader
            {
                SystemType = docFolder.SystemType,
                ScreenCode = docFolder.ScreenCode,
                DocumentType = DocumentLogType.DocMgt,
                FileName = docFileName,
                ParentId = docFolder.ParentId.ToString()
            };
            await _documentStorage.SaveFile(stream, docFilePath, documentHeader);
            return true;

        }

        public override bool DeleteDocumentFile(string docFileName, string thumbFileName, bool hasImage)
        {
            var docFilePath = _documentStorage.GetFilePath(string.Empty, docFileName, ImageHelper.CPiSavedFileType.DocMgt);
            _documentStorage.DeleteFile(docFilePath).GetAwaiter().GetResult();

            if (hasImage) {
                var thumbFilePath = _documentStorage.GetFilePath(string.Empty, thumbFileName, ImageHelper.CPiSavedFileType.DocMgtThumbnail);
                _documentStorage.DeleteFile(thumbFilePath).GetAwaiter().GetResult();
            }
            return true;
        }

        public override bool DeleteLetterLogFile(string docFileName)
        {
            var docFilePath = _documentStorage.GetFilePath(string.Empty, docFileName, ImageHelper.CPiSavedFileType.Letter);
            _documentStorage.DeleteFile(docFilePath).GetAwaiter().GetResult();
            return true;
        }

        public override bool DeleteEFSLogFile(string docFileName)
        {
            var docFilePath = _documentStorage.GetFilePath(string.Empty, docFileName, ImageHelper.CPiSavedFileType.EFS);
            _documentStorage.DeleteFile(docFilePath).GetAwaiter().GetResult();
            return true;
        }

        public override string GetDocumentPath(string docFileName)
        {
            docFileName = docFileName.Replace(@"\", "/");
            if (_documentStorage.IsFileExists(docFileName).GetAwaiter().GetResult())
                return docFileName;

            return "";
        }

        public override string GetDocumentBasePath()
        {
            return DocumentBaseFolder;
        }
        public override int SaveOutlookEmailToMsgFile(MsgEmailModel message, string fileName, DocumentStorageHeader header)
        {
            try
            {
                var email = CreateMsgFile(message);

                using (var stream = new MemoryStream())
                {
                    email.Save(stream);
                    header.FileName = fileName;
                    _documentStorage.SaveFile(stream, $"{DocumentBaseFolder}/{fileName}", header).GetAwaiter();
                }
                int fileSize = (int)email.MessageSize;
                return fileSize;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}:{ex.InnerException?.Message}");
                return 0;
            }
        }
        public override string SaveEmailToMsgFile(MsgEmailModel message, string systemType, DocumentStorageHeader header)
        {
            try
            {
                var email = CreateMsgFile(message);

                // save to file
                var fileName = $"QuickEmail-{DateTime.Now:yyyy-MM-dd-hhmmssfffftt}-{header.SystemType}-{header.ParentId}.msg";

                using (var stream = new MemoryStream()) {
                    email.Save(stream);
                    header.FileName = fileName;
                    _documentStorage.SaveFile(stream, $"{QuickEmailLogFolder}/{fileName}", header).GetAwaiter();
                }
                return fileName;

            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}:{ex.InnerException?.Message}");
                return string.Empty;
            }
        }

        public async virtual System.Threading.Tasks.Task LogEmailUploadedAttachment(QELog qeLog, IFormFile file, string fileName, string thumbNail)
        {
            var documentHeader = new DocumentStorageHeader
            {
                SystemType = qeLog.SystemType.ToUpper(),                        // global search consistency
                ScreenCode = DataKeyToScreenCode(qeLog.DataKey),                // global search consistency
                DocumentType = DocumentLogType.EmailLogAttachment,
                FileName = fileName,
                ParentId=qeLog.DataKeyValue.ToString()
            };
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                await _documentStorage.SaveFile(stream, $"{QuickEmailAttachmentLogFolder}/{fileName}", documentHeader);

                if (!string.IsNullOrEmpty(thumbNail))
                {
                    var thumbNailFullPath = _documentStorage.GetFilePath(qeLog.SystemTypeName, thumbNail, ImageHelper.CPiSavedFileType.Thumbnail);
                    await _documentStorage.CreateAndSaveThumbnail(stream, thumbNailFullPath);
                }
            }
        }

        public override async System.Threading.Tasks.Task LogEmailImageAttachmentFromStream(QELog qeLog, MemoryStream sourceStream, string newFileName, string newThumbNail)
        {
            var documentHeader = new DocumentStorageHeader
            {
                SystemType = (qeLog.SystemType ?? "").ToUpper(),                    // global search consistency
                ScreenCode = DataKeyToScreenCode(qeLog.DataKey ?? ""),            // global search consistency
                DocumentType = DocumentLogType.EmailLogAttachment,
                FileName = newFileName,
                //DataKey = qeLog.DataKey,                                  // replaced by ScreenCode
                ParentId = qeLog.DataKeyValue.ToString()
            };
            //await _documentStorage.SaveFile(sourceStream, $"{QuickEmailLogFolder}/{qeLog.SystemTypeName}/{newFileName}",documentHeader);
            await _documentStorage.SaveFile(sourceStream, $"{QuickEmailAttachmentLogFolder}/{newFileName}", documentHeader);

            if (!string.IsNullOrEmpty(newThumbNail))
            {
                var thumbNailFullPath = _documentStorage.GetFilePath(qeLog.SystemTypeName ?? "", newThumbNail, ImageHelper.CPiSavedFileType.Thumbnail);
                await _documentStorage.CreateAndSaveThumbnail(sourceStream, thumbNailFullPath);
            }
        }

        public override async System.Threading.Tasks.Task LogEmailImageAttachment(QELog qeLog, string sourceFile, string newFileName, string newThumbNail)
        {
            var imageFullPath = _documentStorage.GetFilePath(qeLog.SystemTypeName ?? "", sourceFile, ImageHelper.CPiSavedFileType.Image);
            var sourceStream = await _documentStorage.GetFileStream(imageFullPath);
            await LogEmailImageAttachmentFromStream(qeLog, sourceStream, newFileName, newThumbNail);            
        }

        public override DocumentViewer GetDocumentViewerModel(string docFilePath, int width, int height)
        {
            var fileProvider = new CustomFileProvider(_documentStorage);
            fileProvider.File = docFilePath;
            
            var documentViewer = new DocumentViewer
            {
                Width = width,
                Height = height,
                Resizable = true,
                Document = fileProvider,
                DisplayLanguage = "en"
            };
            return documentViewer;
        }
    }

    public class CustomFileProvider : FileProvider
    {
        private readonly AzureStorage _documentStorage;
        public CustomFileProvider(AzureStorage documentStorage)
        {
            _documentStorage = documentStorage;
        }

        public override string File { get; set; }

        //Return true if DoGetInfo method is implemented, and false if not.
        public override bool CanGetInfo => true;

        //Return true if DoOpenRead method is implemented, and false if not.
        public override bool CanOpenRead => true;

        //Return true if DoOpenWrite method is implemented, and false if not.
        public override bool CanOpenWrite => false;

        //Return true only if File identifier is usable across processes/machines.
        public override bool CanSerialize => false;
        
        
        protected override FileProviderInfo DoGetInfo()
        {
            //Return info here which corresponds to the identifier in File property.

            //When this file provider is used in DocumentViewer:
            //This method will be called every time DocumentViewer requests a document.
            //The cache key and document format will be determined according to the info you return here.

            string fileName = File;
            DateTime dateModified = DateTime.Now;
            long size = 8192;
            return new FileProviderInfo(fileName, dateModified, size);
        }
        

        protected override Stream DoOpenRead()
        {
            var file = _documentStorage.GetFileStream(File).GetAwaiter().GetResult();
            return file;
        }

        protected override Stream DoOpenWrite()
        {
            //Open and return a writable stream here which corresponds to the identifier in File property.
            throw new NotImplementedException();
        }
    }

    //public class AzureDocumentHandler : IDocumentHandler
    //{
    //    public AzureDocumentHandler()
    //    {
    //    }

    //    public DocumentInfo GetInfo(string inputFile, DocumentHandlerParameters handlerParameters)
    //    {
    //        return new DocumentInfo(inputFile, inputFile);
    //    }

    //    public StreamResult OpenRead(string inputFile, InputOptions inputOptions, DocumentHandlerParameters handlerParameters)
    //    {
    //        var file = handlerParameters.Get<string>("file");
    //        var fileAsBytes = Convert.FromBase64String(file);
    //        var stream = new MemoryStream(fileAsBytes);
    //        return new StreamResult(stream);
    //    }
    //}
}