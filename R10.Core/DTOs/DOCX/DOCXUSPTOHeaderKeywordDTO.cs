using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class DOCXUSPTOHeaderKeywordDTO
    {
        [Display(Name = "DOCX keywords for section headers")]
        public string? HeaderKeyword { get; set; }
        public bool? IsHeader { get; set; }
    }

    [Keyless]
    public class DOCXUSPTOHeaderKeywordExcelDTO
    {
        [Display(Name = "DOCX Section Headers")]
        public string? Header { get; set; }
        [Display(Name = "Keywords")]
        public string? Keyword { get; set; }
    }
}
