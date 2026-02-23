using Microsoft.AspNetCore.Http;
using R10.Core.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static R10.Web.Helpers.ImageHelper;
using R10.Web.Areas.Shared.ViewModels;

namespace R10.Web.Services.DocumentStorage
{
    public interface IDocumentStorage
    {
        //Task SaveUploadedImage(ImageViewModel imageVM, bool deleteExisting, DocumentStorageHeader header);
        //void DeleteImageFiles(ImageViewModel imageVM);
        //Task CopyImageFile(ImageViewModel imageVM, DocumentStorageHeader header);
        Task CopyImageFile(string imageFile, string sourceSystem, string destinationSystem, DocumentStorageHeader header);
        Task<CPIFile> GetFileStream(string system, string fileName, CPiSavedFileType savedFileType);
        string GetFilePath(string system, string fileName, CPiSavedFileType savedFileType);
        Task SaveFile(byte[] buffer, string path, DocumentStorageHeader header);
        Task SaveFile(IFormFile file, string path, DocumentStorageHeader header);
        Task SaveFiles(List<DocumentStorageFile> files);
        Task UpdateFileMetadata(List<DocumentStorageFile> files);
        Task<byte[]> ConvertImage(string system, string fileName);

        string BuildPath(string folder, string system, string fileName);
        Task DeleteFile(string path);
        Task<MemoryStream> GetFileStream(string fileName);
        bool IsImageOnFile(string system, string fileName);
        Task<bool> IsFileExists(string physicalPath);
        Task CopyFile(string sourcePath, string destinationPath, DocumentStorageHeader header);
        Task<CPiSavedFileType> GetFileType(int folderId);
        List<LetterTemplateViewModel> GetListOfFiles(string path);

        byte[] CompressDocuments(List<DocDocumentListViewModel> files, string system, string userName);
        byte[] CompressGSDocuments(List<GSDownloadDTO> file);

        string EFSLogFolder { get; }
        string LetterLogFolder { get; }
        string ImageRootFolder { get; }
        string DOCXLogFolder { get; }
        string LetterTemplateFolder { get; }
        string CalendarFileFolder { get; }
        string ReportLogFolder { get; }
    }
}
