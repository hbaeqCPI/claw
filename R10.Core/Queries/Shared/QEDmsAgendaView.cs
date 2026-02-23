using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEDmsAgendaView
    {
        public int AgendaId { get; set; }
        public DateTime? MeetingDate { get; set; }
        public string? Area { get; set; }
        public string? AreaDescription { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? Remarks { get; set; }
        public string? RespOffice { get; set; }        
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? DateToday { get; set; }
        public DateTime? DateTimeToday { get; set; }        
        public string? AgendaUrl { get; set; }
        public string? Reviewers { get; set; }        
        public string? Disclosures { get; set; }
    }
}
