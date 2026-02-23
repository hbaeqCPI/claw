using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.DTOs
{
    public class AttachedFileDTO
    {
        public string? FileId { get; set; }
        public string? FileName { get; set; }
        
        [Display(Name="Image")]
        public string? Thumbnail { get; set; }
        public string? FileTitle { get; set; } = "";
        
        [NotMapped]
        public bool Send { get; set; }

        [NotMapped]
        public string? ItemId { get; set; }
        [NotMapped]
        public string? SharePointDocLibrary { get; set; }
        
        [NotMapped]
        public string? IconClass { get; set; }

    }

  
}
