using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEDmsActionDueDateView
    {
        public int ActId { get; set; }
        public string? DisclosureNumber { get; set; }
        public string? DisclosureStatus { get; set; }
        //public DateTime? DisclosureStatusDate { get; set; }
        public string? DisclosureTitle { get; set; }
        public DateTime? DisclosureDate { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? ActionType { get; set; }
        public DateTime BaseDate { get; set; }
        public DateTime? ResponseDate { get; set; }
        public DateTime? FinalDate { get; set; }
        public string? ActionRemarks { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? DateToday { get; set; }
        public DateTime? DateTimeToday { get; set; }

        public int DDId { get; set; }
        public string? ActionDue { get; set; }
        public DateTime DueDate { get; set; }
        public string? Indicator { get; set; }
        public DateTime? DateTaken { get; set; }
        public string? DueDateRemarks { get; set; }
        public DateTime? DueDateDateCreated { get; set; }
        public DateTime? DueDateLastUpdate { get; set; }
    }
}
