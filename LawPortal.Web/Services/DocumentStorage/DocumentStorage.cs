using System.Collections.Generic;

namespace LawPortal.Web.Services.DocumentStorage
{
    // Metadata stored alongside saved files — for blob mode this becomes blob metadata
    // (queryable via Azure Search etc.); for file system mode it's currently ignored.
    public class DocumentStorageHeader
    {
        public string SystemType { get; set; } = "";
        public string ScreenCode { get; set; } = "";
        public string ParentId { get; set; } = "";
        public string DocumentType { get; set; } = "";
        public string ThumbnailPath { get; set; } = "";
        public string LogId { get; set; } = "";
        public string FileName { get; set; } = "";
        public string DataKey { get; set; } = "";
    }

    // Bulk-save unit — used when uploading multiple files at once.
    public class DocumentStorageFile
    {
        public byte[]? Buffer { get; set; }
        public string FileName { get; set; } = "";
        public DocumentStorageHeader? Header { get; set; }
    }
}
