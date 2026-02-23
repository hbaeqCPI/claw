using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEPacClearanceView
    {
        public int PacId { get; set; }
        public string? CaseNumber { get; set; }
        public string? ClearanceStatus { get; set; }
        public DateTime? ClearanceStatusDate { get; set; }

        public DateTime? DateRequested { get; set; }
        
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? Attorney { get; set; }
        public string? AttorneyName { get; set; }
        
        public string? Remarks { get; set; }

        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime LastUpdate { get; set; }

        public string? DateToday { get; set; }
        public DateTime? DateTimeToday { get; set; }

        public string? Discussions { get; set; }
        public string? DiscussionReplies { get; set; }

        public string? Requestors { get; set; }
        public string? Keywords { get; set; }
        public string? Images { get; set; }
    }
}