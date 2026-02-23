using Kendo.Mvc.UI;
using R10.Core.DTOs;
using R10.Core.Entities;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocumentVerificationSearchCriteriaViewModel : DocumentVerificationSearchCriteriaDTO
    {
        public bool ShowOrphanage { get; set; }        
        public string? Patent { get; set; }
        public string? Trademark { get; set; }
        public string? GeneralMatter { get; set; }
        public string? AttorneyFilter1 { get; set; }
        public string? AttorneyFilter2 { get; set; }
        public string? AttorneyFilter3 { get; set; }
        public string? AttorneyFilter4 { get; set; }
        public string? AttorneyFilter5 { get; set; }
        public string? AttorneyFilterR { get; set; }
        public string? AttorneyFilterD { get; set; }
        public string? AttorneyFilterRD { get; set; }
        public string[]? Attorneys { get; set; }
        public string[]? ActionTypes { get; set; }
        public string[]? DocNames { get; set; }
        public string[]? DocUploadedBys { get; set; }
        public string[]? ActCreatedBys { get; set; }
        public string[]? RespDocketings { get; set; }
        public string[]? RespReportings { get; set; }
        public string[]? Countries { get; set; }
        public string[]? Sources { get; set; }
        public string[]? Clients { get; set; }
        public List<QuickDocketSystemTypeViewModel>? Systems { get; set; }
    }

    public class DocumentVerificationSystemTypeViewModel : SystemType
    {
        public int OrderOfEntry { get; set; }
        public string? SystemName { get; set; }
    }
}
