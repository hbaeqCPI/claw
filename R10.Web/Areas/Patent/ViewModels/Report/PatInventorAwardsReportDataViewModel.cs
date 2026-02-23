using DocuSign.eSign.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels.Report
{
    public class PatInventorAwardsReportDataViewModel
    {
        public string SortKey { get; set; }
        public string SortField { get; set; }
        public int AwardId { get; set; }
        public string AwardSource { get; set; }
        public int? AppId { get; set; }
        public int? DMSId { get; set; }
        public int InventorId { get; set; }
        public string Inventor { get; set; }
        public string CaseNumber { get; set; }
        public string Country { get; set; }
        public string SubCase { get; set; }
        public string CaseType { get; set; }
        public string ApplicationStatus { get; set; }
        public DateTime? ApplicationStatusDate { get; set; }
        public string? AppNumber { get; set; }
        public DateTime? FilDate { get; set; }
        public string? PatNumber { get; set; }
        public DateTime? IssDate { get; set; }
        public DateTime? ExpDate { get; set; }
        public decimal? Amount { get; set; }
        public string? AwardType { get; set; }
        public DateTime? AwardDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Remarks{ get; set; }
        public DateTime? LastUpdate{ get; set; }
        public decimal TotalAmount { get; set; }
        public List<CaseGroupViewModel> Cases { get; set; }
    }
    public class CaseGroupViewModel
    {
        public string CaseNumber { get; set; }
        public string Country { get; set; }
        public string SubCase { get; set; }
        public List<PatInventorAwardsReportDataViewModel> Awards { get; set; }
    }
}