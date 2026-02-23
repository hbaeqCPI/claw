using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEPatActionDueDateInvView
    {
        public int InvId { get; set; }
        public int ActId { get; set; }
        public string? CaseNumber { get; set; }
        public string? DisclosureStatus { get; set; }
        public DateTime? DisclosureDate { get; set; }
        public string? InvTitle { get; set; }
        public string? Attorney1 { get; set; }
        public string? Attorney2 { get; set; }
        public string? Attorney3 { get; set; }
        public string? Attorney4 { get; set; }
        public string? Attorney5 { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? ClientRef { get; set; }
        public string? Owner { get; set; }
        public string? OwnerName { get; set; }
        public string? ActionType { get; set; }
        public DateTime BaseDate { get; set; }
        public DateTime? ResponseDate { get; set; }
        public DateTime? VerifyDate { get; set; }
        public DateTime? FinalDate { get; set; }
        public bool ComputerGenerated { get; set; }
        public bool? IsElectronic { get; set; }
        public string? Responsible { get; set; }
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

        public string? PriorityCountry { get; set; }
        public string? PriorityNumber { get; set; }
        public DateTime? PriorityDate { get; set; }
    }
}
