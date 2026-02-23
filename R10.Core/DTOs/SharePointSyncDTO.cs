using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Identity;
using System;

namespace R10.Core.DTOs
{
    [Keyless]
    public class SharePointSyncDTO
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? ParentId { get; set; }
        public string? ParentFolder { get; set; }
        public string? Type { get; set; }
        public int? Level { get; set; }
        public string? Key { get; set; }
        public string? DocLibrary { get; set; }
        public string? DocLibraryFolder { get; set; }
        public bool IsImage { get; set; }
        public string? Author { get; set; }
        public bool IsPrivate { get; set; }
        public string? DocUrl { get; set; }
        public string? Remarks { get; set; }
        public bool IsDefault { get; set; }
        public bool IsPrintOnReport { get; set; }
        public string? Tags { get; set; }
        public bool IsVerified { get; set; }
        public bool IncludeInWorkflow { get; set; }
        public bool IsActRequired { get; set; }        
        public string? Source { get; set; }
        public bool CheckAct { get; set; }
        public bool SendToClient { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }


    [Keyless]
    public class SharePointToAzureBlobSyncDTO
    {
        public int LogId { get; set; }
        public string? DriveItemId { get; set; }
        public string? DocLibrary { get; set; }
        public string? DocLibraryFolder { get; set; }
        public int RecordId { get; set; }
        public string? UpdateType { get; set; }
        public string? FileName { get; set; }
    }

    [Keyless]
    public class SharePointSyncCopyDTO
    {
        public string? DocLibrary { get; set; }
        public string? Screen { get; set; }
        public string? DataKey { get; set; }
        public int ParentId { get; set; }
        public int RecordId { get; set; }
        public string? SourceDriveItemId { get; set; }
        public string? NewDriveItemId { get; set; }
        public string? NewFileName { get; set; }
    }

}


