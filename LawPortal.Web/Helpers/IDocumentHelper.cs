using GleamTech.DocumentUltimate.AspNet.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using LawPortal.Core.DTOs;
using LawPortal.Core.Entities;
using LawPortal.Core.Entities.Documents;
using LawPortal.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Helpers
{
    public interface IDocumentHelper
    {

        Task<bool> SaveDocumentFileUpload(IFormFile uploadedFile, string docFileName, string thumbFileName, DocFolderHeader folderHeader);
        bool DeleteDocumentFile(string docFileName, string thumbFileName, bool hasImage);
        bool DeleteLetterLogFile(string docFileName);
        bool DeleteEFSLogFile(string docFileName);
        string GetDocumentPath(string docFileName);
        string GetDocumentBasePath();
        // SaveEmailToMsgFile, SaveOutlookEmailToMsgFile, CreateMsgFile removed (MsgEmailModel/DocumentStorageHeader from deleted DocumentStorage)

        DocumentViewer GetDocumentViewerModel(string docFilePath, int width, int height);
        //Task LogEmailImageAttachmentFromStream(QELog qeLog, MemoryStream sourceStream, string newFileName, string newThumbNail);
        //Task LogEmailImageAttachment(QELog qeLog, string sourceFile, string newFileName,string newThumbNail);
        //Task LogEmailUploadedAttachment(QELog qeLog, IFormFile file, string fileName, string thumbNail);

        string DataKeyToScreenCode(string dataKey);
        Task<bool> SaveDocumentFromStream(MemoryStream stream, string docFileName, DocFolderHeader docFolder);
    }
}
