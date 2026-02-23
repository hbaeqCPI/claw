using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class RTSCompareExportViewModel
    {
        [Display(Name = "LabelCaseNumber")]
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }

        public string? YourCaseType { get; set; }
        public string? PubCaseType { get; set; }
        public string? MarkCaseType { get; set; }

        public string? YourAppNo { get; set; }
        public string? PubAppNo { get; set; }
        public string? MarkAppNo { get; set; }

        public DateTime? YourFilDate { get; set; }
        public DateTime? PubFilDate { get; set; }
        public string? MarkFilDate { get; set; }

        public string? YourPubNo { get; set; }
        public string? PubPubNo { get; set; }
        public string? MarkPubNo { get; set; }

        public DateTime? YourPubDate { get; set; }
        public DateTime? PubPubDate { get; set; }
        public string? MarkPubDate { get; set; }

        public string? YourPatNo { get; set; }
        public string? PubPatNo { get; set; }
        public string? MarkPatNo { get; set; }

        public DateTime? YourIssDate { get; set; }
        public DateTime? PubIssDate { get; set; }
        public string? MarkIssDate { get; set; }

        public DateTime? YourParentPCTDate { get; set; }
        public DateTime? PubParentPCTDate { get; set; }
        public string? MarkParentPCTDate { get; set; }

    }
}
