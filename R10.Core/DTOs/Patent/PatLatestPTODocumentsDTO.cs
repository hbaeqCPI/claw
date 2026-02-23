using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class PatLatestPTODocumentsDTO
    {
        public int AppId { get; set; }

        public string CaseNumber { get; set; }

        public string Country { get; set; }

        public string SubCase { get; set; }       

        public string? AppTitle { get; set; }

        public string? Description { get; set; }

        public string? FileName { get; set; }

        public int NoPages { get; set; }

        public int PageStart { get; set; }

        public int PLAppID { get; set; }
    }
}
