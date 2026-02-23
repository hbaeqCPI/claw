using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.DTOs
{

    // Audit Trail search fields
    [Keyless]
    public class AuditSearchDTO
    {
        public string? SystemType { get; set; }

        public string? TableId { get; set; }

        public string? TranxType { get; set; }


        public string? UserName { get; set; }

        public DateTime? FromTranxDate { get; set; }
        public DateTime? ToTranxDate { get; set; }

        public string? CaseNumber { get; set; }

        public string? Country { get; set; }

        public string? SubCase { get; set; }

        public string? DisclosureNumber { get; set; }

        public string? Remarks { get; set; }

        public int PageSize { get; set; }
        public int Page { get; set; }

    }

}
