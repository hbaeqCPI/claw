using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEDmsDisclosureView
    {
        public int DMSId { get; set; }
        public string? DisclosureNumber { get; set; }
        public string? DisclosureTitle { get; set; }
        public string? Attorney { get; set; }
        public string? AttorneyName { get; set; }
        public string? Owner { get; set; }
        public string? OwnerName { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? Area { get; set; }
        public string? AreaDescription { get; set; }
        public string? DisclosureStatus { get; set; }
        public DateTime? DisclosureStatusDate { get; set; }
        public DateTime? DisclosureDate { get; set; }
        public string? Recommendation { get; set; }
        public string? Remarks { get; set; }
        public string? Inventors { get; set; }
        public string? Reviewers { get; set; }
        public string? Keywords { get; set; }
        public string? Images { get; set; }
        public string? RespOffice { get; set; }
        public string? Abstract { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? DateToday { get; set; }
        public DateTime? DateTimeToday { get; set; }
        public string? Actions { get; set; }
        public string? DisclosureUrl { get; set; }
        public string? Discussions { get; set; }
        public string? DiscussionReplies { get; set; }
        public string? InventorChanged { get; set; }

    }
}
