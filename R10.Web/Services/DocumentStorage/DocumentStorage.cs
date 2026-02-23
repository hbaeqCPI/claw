using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace R10.Web.Services.DocumentStorage
{
    public abstract class DocumentStorage
    {
        protected readonly IAsyncRepository<DocFixedFolder> _docFixFolderRepository;
        protected readonly IDocumentService _docService;


        public DocumentStorage(IDocumentService docService,
                               IAsyncRepository<DocFixedFolder> docFixFolderRepository)
        {
            _docService = docService;
            _docFixFolderRepository = docFixFolderRepository;
        }

        public bool IsImageOnFile(string system, string fileName)
        {
            //var path = GetFilePath(system, fileName, ImageHelper.CPiSavedFileType.Image);
            var path = GetFilePath(system, fileName, ImageHelper.CPiSavedFileType.DocMgt);
            return IsFileExists(path).GetAwaiter().GetResult();
        }

        protected async Task<string> GetRealFileName(string fileName)
        {
            var fileId = ImageHelper.ExtractFileId(fileName);
            if (fileId == 0)
                return string.Empty;

            var realFileName = (await _docService.GetFileById(fileId)).UserFileName;
            return (realFileName ?? "");
            
        }

        protected string BuildCompressFileName(string userName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\Temporary Folder", userName);
            var newFileName = DateTime.Now.ToString().Replace(" ", "").Replace("/","").Replace(":","");
            var fullPath = Path.Combine(path, newFileName +".zip");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return fullPath;
        }


        //not the best approach
        public async Task<ImageHelper.CPiSavedFileType> GetFileType(int folderId)
        {
            var folderInfo = await _docFixFolderRepository.QueryableList.FirstOrDefaultAsync(f => f.FolderId == folderId);
            switch (folderInfo.FolderName.ToLower())
            {
                case "letters":
                    return ImageHelper.CPiSavedFileType.Letter;
                case "quick emails":
                    return ImageHelper.CPiSavedFileType.QE;
                case "electronic forms":
                    return ImageHelper.CPiSavedFileType.EFS;
                case "references":
                    return ImageHelper.CPiSavedFileType.IDSReferences;
                case "docx":
                    return ImageHelper.CPiSavedFileType.Letter;
                default:
                    return ImageHelper.CPiSavedFileType.Image;
            }
        }

        public abstract string GetFilePath(string system, string fileName, ImageHelper.CPiSavedFileType savedFileType);
        public abstract Task DeleteFile(string path);
        public abstract Task CreateSnaphot(string path);
        public abstract Task SaveFile(IFormFile file, string path, DocumentStorageHeader header);
        public abstract Task SaveFiles(List<DocumentStorageFile> files);
        public abstract Task<bool> IsFileExists(string physicalPath);
        public abstract Task CopyFile(string sourcePath, string destinationPath, DocumentStorageHeader header);
        protected abstract Task<long> GetSavedFileSize(string filePath);
        public abstract Task<byte[]> ConvertImage(string system, string fileName);
        public abstract List<LetterTemplateViewModel> GetListOfFiles(string path);

    }

    public class DocumentStorageHeader
    {
        public string SystemType { get; set; }
        public string ScreenCode { get; set; }
        public string ParentId { get; set; }
        public string DocumentType { get; set; }
        public string ThumbnailPath { get; set; }
        public string LogId { get; set; }                    // originally LetLogId
        public string FileName { get; set; }
        public string DataKey { get; set; }
    }

    public class DocumentDBUpdateInfo
    {
        public string SystemType { get; set; }
        public string ScreenCode { get; set; }
        public int RecordId { get; set; }
    }

    public class DocumentStorageFile
    {
        //public Stream File { get; set; }
        public byte[] Buffer { get; set; }
        public string FileName { get; set; }
        public DocumentStorageHeader Header  { get; set; }
    }

    
}
