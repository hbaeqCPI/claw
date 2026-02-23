using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QETmcClearanceView
    {
        public int TmcId { get; set; }
        public string? CaseNumber { get; set; }
        public string? SearchRequestStatus { get; set; }
        public DateTime? SearchRequestStatusDate { get; set; }

        public DateTime? DateRequested { get; set; }

        public string? Requestor { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? Attorney { get; set; }
        public string? AttorneyName { get; set; }
        public string? Keywords { get; set; }
        public string? RequestedTerms { get; set; }

        public string? NAmerica { get; set; }
        public string? CSAmerica { get; set; }
        public string? Europe { get; set; }
        public string? MiddleEast { get; set; }
        public string? Africa { get; set; }
        public string? Asia { get; set; }
        public string? Oceana { get; set; }
        public string? Remarks { get; set; }

        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime LastUpdate { get; set; }

        public string? DateToday { get; set; }
        public DateTime? DateTimeToday { get; set; }


        public string? MarkInfo { get; set; }
        public string? BrandInfo { get; set; }
        public string? UsePlans { get; set; }
        public string? Artwork { get; set; }
        public string? General { get; set; }
        public string? Images { get; set; }

        public string? Countries { get; set; }

        public string? Discussions { get; set; }
        public string? DiscussionReplies { get; set; }
    }
}