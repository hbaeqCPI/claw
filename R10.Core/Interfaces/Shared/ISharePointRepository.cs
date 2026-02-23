using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using R10.Core.DTOs;

namespace R10.Core.Interfaces
{
    public interface ISharePointRepository
    {
        Task<List<string>> SyncToDocumentTablesSave(string userName, DateTime? lastModifiedDateTime, List<SharePointSyncDTO> sharePointSyncItems, bool fromCopy = false,bool mainRecordOnly= false, bool singleNode = false);
        Task<List<SharePointToAzureBlobSyncDTO>> GetSharePointToAzureBlobList(string docLibrary);
        Task MarkSharePointToAzureBlobAsProcessed(int logId);
        Task SyncToDocumentTablesUpdateDelete(string docLibrary, string systemType, List<SharePointSyncDTO> sharePointSyncItems);
        Task SyncToDocumentTablesCopy(string author, List<SharePointSyncCopyDTO> sharePointSyncItems);
        Task<string> GetDocLibraryLastSync(string docLibrary);
    }
}
