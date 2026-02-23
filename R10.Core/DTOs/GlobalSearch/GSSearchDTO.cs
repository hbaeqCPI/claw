using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.DTOs
{
    public class GSSearchDTO
    {
        public string? Link { get; set; }

        public string? SystemName { get; set; }

        public string? ScreenName { get; set; }

        public string? FieldValues { get; set; }

    }

    public class GSSearchDocDTO : GSSearchDTO
    {
        public string? RecordKey { get; set; }
        public int RecordId { get; set; }
        public string? DocumentTypeName { get; set; }

        [NotMapped]
        public string? SystemType { get; set; }
        [NotMapped]
        public string? ScreenCode { get; set; }
        [NotMapped]
        public string? DocumentType { get; set; }
        [NotMapped]
        public int ParentId { get; set; }
        [NotMapped]
        public long FileSize { get; set; }
        [NotMapped]
        public string? FileName { get; set; }
        [NotMapped]
        public decimal SearchScore { get; set; }
    }

    public class GSDownloadDTO 
    {
        public int RecordId { get; set; }
        public string? SystemType { get; set; }
        public string? DocumentType { get; set; }
        public string? DocFileName { get; set; }
        public string? UserFileName { get; set; }
        [NotMapped]
        public byte[] FileBytes { get; set; }
    }
}
