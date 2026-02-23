using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEPatActionImageView
    {
        public int ActId { get; set; }
        public int DocId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? CountryName { get; set; }
        public string? SubCase { get; set; }
        public string? CaseType { get; set; }
        public string? ApplicationStatus { get; set; }
        public DateTime? ApplicationStatusDate { get; set; }
        public string? InvTitle { get; set; }
        public string? AppTitle { get; set; }
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
        public string? Agent { get; set; }
        public string? AgentName { get; set; }
        public string? AgentRef { get; set; }
        public string? AppNumber { get; set; }
        public DateTime? FilDate { get; set; }

        public string? PubNumber { get; set; }
        public DateTime? PubDate { get; set; }
        public string? PatNumber { get; set; }
        public DateTime? IssDate { get; set; }
        public DateTime? ExpDate { get; set; }
        public string? GroupArtUnit { get; set; }
        public string? Examiner { get; set; }
        public string? AttorneyDocketNo { get; set; }
        public string? CustomerNo { get; set; }
        public string? ActionType { get; set; }
        public DateTime? BaseDate { get; set; }
        public string? DocName { get; set; }
        public string? DocFileName { get; set; }
        public string? ThumbFileName { get; set; }
        public DateTime? ImageDate { get; set; }
        //public string? ImageSource { get; set; }
        public string? Remarks { get; set; }
        public string? UserFileName { get; set; }
        public string? DateToday { get; set; }
        public DateTime? DateTimeToday { get; set; }


    }
}
