using GleamTech.DocumentUltimate.AspNet.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MsgKit;
using MsgKit.Enums;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Services.DocumentStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static R10.Web.Helpers.ImageHelper;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace R10.Web.Helpers
{
    public class DocumentHelper : IDocumentHelper
    {
        private const string UserFileBaseFolder = "UserFiles";
        private const string DocumentBaseFolder = @"UserFiles\Searchable\Documents";
        private const string DocumentThumbnailFolder = @"UserFiles\Thumbnails";
        private const string LetterLogFolder = @"UserFiles\Searchable\Logs\Letters";
        private const string EFSLogFolder = @"UserFiles\Searchable\Logs\EFS";

        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger _logger;

        public DocumentHelper(ILogger<DocumentHelper> logger, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        public virtual async Task<bool> SaveDocumentFileUpload(IFormFile uploadedFile, string docFileName, string thumbFileName, DocFolderHeader folderHeader)
        {
            var isImage = uploadedFile.ContentType.Contains("image");
            string rootPath = _hostingEnvironment.ContentRootPath;
            var docFilePath = Path.Combine(rootPath, DocumentBaseFolder, docFileName);
            if (!await FileHelper.SaveFileUpload(docFilePath, uploadedFile)) return false;

            if (isImage)
            {
                var thumbFilePath = Path.Combine(rootPath, DocumentThumbnailFolder, thumbFileName);
                FileHelper.CreateAndSaveThumbnail(docFilePath, thumbFilePath);
            }
            return true;
        }

        public virtual bool DeleteDocumentFile(string docFileName, string thumbFileName, bool hasImage)
        {
            string rootPath = _hostingEnvironment.ContentRootPath;
            var docFilePath = Path.Combine(rootPath, DocumentBaseFolder, docFileName);
            var deleted = FileHelper.DeleteFile(docFilePath);
            if (hasImage)
            {
                var thumbFilePath = Path.Combine(rootPath, DocumentThumbnailFolder, thumbFileName);
                deleted = deleted && FileHelper.DeleteFile(thumbFilePath);
            }
            return deleted;
        }

        public virtual bool DeleteLetterLogFile(string docFileName)
        {
            string rootPath = _hostingEnvironment.ContentRootPath;
            var docFilePath = Path.Combine(rootPath, LetterLogFolder, docFileName);
            var deleted = FileHelper.DeleteFile(docFilePath);
            return deleted;
        }

        public virtual bool DeleteEFSLogFile(string docFileName)
        {
            string rootPath = _hostingEnvironment.ContentRootPath;
            var docFilePath = Path.Combine(rootPath, EFSLogFolder, docFileName);
            var deleted = FileHelper.DeleteFile(docFilePath);
            return deleted;
        }

        public virtual string GetDocumentPath(string docFileName)
        {
            string rootPath = _hostingEnvironment.ContentRootPath;
            var docFilePath = Path.Combine(rootPath, UserFileBaseFolder, docFileName.Replace("/", "\\"));
            if (!File.Exists(docFilePath))
                docFilePath = "";
            return docFilePath;

        }

        public virtual string GetDocumentBasePath()
        {
            return DocumentBaseFolder;
        }

        public virtual string SaveEmailToMsgFile(MsgEmailModel message, string systemType, DocumentStorageHeader header)
        {
            try
            {
                var email = CreateMsgFile(message);

                // save to file
                var fileName = $"QuickEmail-{DateTime.Now:yyyy-MM-dd-hhmmssfffftt}-{header.SystemType}-{header.ParentId}.msg";
                string logFolder = _hostingEnvironment.GetQuickEmailLogFolder(systemType);
                var docFilePath = Path.Combine(logFolder, fileName);
                email.Save(docFilePath);

                //int fileSize = (int)email.MessageSize;
                return fileName;

            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}:{ex.InnerException?.Message}");
                return string.Empty;
            }
        }

        public virtual int SaveOutlookEmailToMsgFile(MsgEmailModel message, string fileName, DocumentStorageHeader header)
        {
            try
            {
                var email = CreateMsgFile(message);

                // save to file
                string docFilePath = _hostingEnvironment.GetDocMgtPath(fileName);
                email.Save(docFilePath);

                int fileSize = (int)email.MessageSize;
                return fileSize;

            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}:{ex.InnerException?.Message}");
                return 0;
            }
        }

        public virtual DocumentViewer GetDocumentViewerModel(string docFilePath, int width, int height)
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

        public Email CreateMsgFile(MsgEmailModel message)
        {

            var email = new Email(new Sender(message.From.Address, message.From.DisplayName), message.Subject, message.IsDraft, message.IsReadReceiptRequested);
            email.SentOn = message.SentDate;
            email.ReceivedOn = message.ReceiptDate;

            message.To.ForEach(recipient => email.Recipients.AddTo(recipient.Address, recipient.DisplayName));
            message.Cc.ForEach(recipient => email.Recipients.AddCc(recipient.Address, recipient.DisplayName));
            message.Bcc.ForEach(recipient => email.Recipients.AddBcc(recipient.Address, recipient.DisplayName));

            if (message.IsHtml)
                email.BodyHtml = message.Body;
            else
                email.BodyText = message.Body;

            email.Importance = message.Importance == "High" ? MsgKit.Enums.MessageImportance.IMPORTANCE_HIGH :
                                        message.Importance == "Low" ? MsgKit.Enums.MessageImportance.IMPORTANCE_LOW : MsgKit.Enums.MessageImportance.IMPORTANCE_NORMAL;

            email.IconIndex = message.IsSent ? MessageIconIndex.SubmittedMail : MessageIconIndex.ReadMail;


            if (message.Attachments.Count() > 0)
            {
                //email.Attachments.Add("Images\\peterpan.jpg");
                message.Attachments.ForEach(attachment => email.Attachments.Add(attachment));
            }

            if (message.ByteAttachments.Count() > 0)
            {
                message.ByteAttachments.ForEach(attachment => {
                    var fileStream = new MemoryStream(attachment.ContentBytes);
                    email.Attachments.Add(fileStream, attachment.Name, -1, attachment.IsInline, attachment.ContentId);
                });
            }

            return email;
        }

        public async virtual System.Threading.Tasks.Task LogEmailImageAttachmentFromStream(QELog qeLog, MemoryStream sourceStream, string newFileName, string newThumbNail)
        {
            var system = qeLog.SystemTypeName ?? "";
            var logFullPath = _hostingEnvironment.GetQuickEmailLogPath(system, newFileName);

            using (Stream destination = File.Create(logFullPath))
            {
                await sourceStream.CopyToAsync(destination);
            }

            if (!string.IsNullOrEmpty(newThumbNail))
            {
                var thumbNailFullPath = ImageHelper.GetPhysicalFilePath(system, newThumbNail, ImageHelper.CPiSavedFileType.Thumbnail);
                ImageHelper.CreateAndSaveThumbnail(logFullPath, thumbNailFullPath);
            }
        }

        public async virtual System.Threading.Tasks.Task LogEmailImageAttachment(QELog qeLog, string sourceFile, string newFileName, string newThumbNail)
        {
            var system = qeLog.SystemTypeName ?? "";
            var imageFullPath = ImageHelper.GetPhysicalFilePath(system, sourceFile, ImageHelper.CPiSavedFileType.Image);
            var logFullPath = _hostingEnvironment.GetQuickEmailLogPath(system, newFileName);

            // create a new copy of image/doc file from Images directory to Logs\QuickEmail\System directory
            await ImageHelper.CopyFileAsync(imageFullPath, logFullPath);

            if (!string.IsNullOrEmpty(newThumbNail))
            {
                var thumbNailFullPath = ImageHelper.GetPhysicalFilePath(system, newThumbNail, ImageHelper.CPiSavedFileType.Thumbnail);
                ImageHelper.CreateAndSaveThumbnail(logFullPath, thumbNailFullPath);
            }
        }

        public async virtual System.Threading.Tasks.Task LogEmailUploadedAttachment(QELog qeLog, IFormFile file, string fileName, string thumbNail)
        {
            var logFullPath = _hostingEnvironment.GetQuickEmailLogPath(qeLog.SystemTypeName, fileName);
            await ImageHelper.SaveFile(file, logFullPath);
            var fileExt = Path.GetExtension(fileName);

            if (ImageHelper.IsImageFile(fileExt))
            {
                var thumbNailFullPath = ImageHelper.GetPhysicalFilePath(qeLog.SystemTypeName, thumbNail, ImageHelper.CPiSavedFileType.Thumbnail);
                ImageHelper.CreateAndSaveThumbnail(logFullPath, thumbNailFullPath);
            }
        }

        public string DataKeyToScreenCode(string dataKey)
        {
            // for blob storage screencode
            switch (dataKey)
            {
                case "InvId": return "Inv";
                case "AppId": return "CA";
                case "TmkId": return "Tmk";
                case "MatId": return "GM";
                case "ActId": return "Act";
                case "CostTrackId": return "Cost";
                case "ConflictId": return "Conf";
                case "DMSId": return "DMS";
                case "TmcId": return "Tmc";
                case "AnnId": return "AMS";
                default: return "";
            }
        }

        public async virtual Task<bool> SaveDocumentFromStream(MemoryStream stream, string docFileName, DocFolderHeader docFolder)
        {            
            string rootPath = _hostingEnvironment.ContentRootPath;
            var docFilePath = Path.Combine(rootPath, DocumentBaseFolder, docFileName);            
            if (File.Exists(docFilePath))
                File.Delete(docFilePath);
            using (var fileStream = new FileStream(docFilePath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }
            return true;
        }
    }

    public static class DocumentLogType
    {
        // values here are tied to global search table tblGSField.FieldName values
        public const string ImageDoc = "ImageDoc";
        public const string IDSDoc = "IDSDoc";
        public const string LetterLog = "LetterLog";
        public const string EFSLog = "EFSLog";
        public const string EmailLog = "EmailLog";
        public const string EmailLogAttachment = "EmailLogAttachment";
        public const string DocMgt = "DocMgt";
        public const string DOCXLog = "DOCXLog";

    }

}
