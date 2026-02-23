using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEGmActionImageView
    {
        public int ActId { get; set; }
        public int DocId { get; set; }
        public string? CaseNumber { get; set; }
        public string? SubCase { get; set; }
        public string? MatterType { get; set; }
        public string? MatterStatus { get; set; }
        public DateTime? MatterStatusDate { get; set; }
        public string? MatterTitle { get; set; }
        public string? MatterNumber { get; set; }
        public DateTime? EffectiveOpenDate { get; set; }
        public DateTime? TerminationEndDate { get; set; }
        public string? AgreementType { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? Agent { get; set; }
        public string? AgentName { get; set; }
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
