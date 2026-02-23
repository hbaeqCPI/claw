using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatIDSDownloadWebSvc
    {
        [Key]
        public int EntityId { get; set; }
        public int LogId { get; set; }
        public int AppId { get; set; }
        public int RelatedCasesId { get; set; }
        public int FileId { get; set; }
        public string? SearchStr { get; set; }
        public int Attempts { get; set; }
        public string? DocFilePath { get; set; }
        public string? DownloadLink { get; set; }
        public string? Remarks { get; set; }
    }    
}
