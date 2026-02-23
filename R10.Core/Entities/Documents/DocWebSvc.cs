using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Documents
{
    public class DocWebSvc
    {
        [Key]
        public int EntityId { get; set; }

        public int LogId { get; set; }

        public string? DocumentLink { get; set; }

        public string? FileName { get; set; }
    }
}
