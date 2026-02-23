using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class TLNumberFormatDTO
    {
        public int TLTmkId { get; set; }
        public string? TMSCaseNumber { get; set; }
        public string? TMSCountry { get; set; }
        public string? TMSSubCase { get; set; }
        public string? TMSCaseType { get; set; }

        public string? TMSAppNo { get; set; }
        public string? TMSPubNo { get; set; }
        public string? TMSRegNo { get; set; }

        public DateTime? TMSFilDate { get; set; }
        public DateTime? TMSPubDate { get; set; }
        public DateTime? TMSRegDate { get; set; }

        public string? TMSStdAppNo { get; set; }
        public string? TMSStdPubNo { get; set; }
        public string? TMSStdRegNo { get; set; }
    }

    
}
