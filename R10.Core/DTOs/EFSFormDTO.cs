using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class EFSFormDTO
    {
        [Display(Name="Type")]
        public string? GroupDesc { get; set; }
        public int? DisplayOrder { get; set; }
        public int EfsDocId { get; set; }
        public string? DocType { get; set; }
        public string? SubType { get; set; }
        public int RecId { get; set; }

        [Display(Name = "Form Name")]
        public string? DocDesc { get; set; }
        public string? DocPath { get; set; }
        public string? MapFile { get; set; }
        public int PageNo { get; set; }
        public int NoOfPages { get; set; }
        public string? SourceTables { get; set; }

        public int? DOCXId { get; set; }

    }
}
