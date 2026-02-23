using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.DTOs
{
    public class RTSPFSSearchInput
    {

        public string? SearchNo { get; set; }
        public DateTime? SearchDate { get; set; }
        public string? SearchCaseType { get; set; }
        public string? SearchCountry { get; set; }
        public string? SearchNumberType { get; set; }
        public string? SearchCaller { get; set; }
        public string? SearchYearNo { get; set; }
        public bool ForIDS { get; set; }
        public bool LoadFamily { get; set; }
    }

    #region "Statistics"
    public class RTSPFSStatisticsSearchInput
    {
        public string? ReportType { get; set; }
        public string? YearFrom { get; set; }
        public string? YearTo { get; set; }
        public string? CountryOp { get; set; }
        public string? Countries { get; set; }
        public string? SymbolType { get; set; }
        public string? IPCs { get; set; }
        public string? CPCs { get; set; }
        public string? Applicants { get; set; }
        public string? YourApplicants { get; set; }
    }

    public class RTSPFSStatisticsAuxSearchInput
    {
        public string? SearchType { get; set; }
        public string? SearchString { get; set; }
        public string? SearchMode { get; set; }
    }
    #endregion

    #region "Patent Watch"
    public class RTSPFSMultipleSearchInput
    {
        public string? Country { get; set; }
        public string[] Titles { get; set; }
        public string[] AppNos { get; set; }
        public string[] PubNos { get; set; }
        public string[] PatNos { get; set; }
    }

    public class RTSPFSPatentWatchSearchInput
    {
        public string? CPIClientCode { get; set; }
        public string? UserName { get; set; }
        public string? NumberType { get; set; }
        public string[] Numbers { get; set; }
        public int WatchId { get; set; }
        public string? Notify { get; set; }
        public int NotifyId { get; set; }
        public string? Remarks { get; set; }
        public string? Keywords { get; set; }
    }

    public class RTSPFSPatentWatchFromPSInput
    {
        public string? CPIClientCode { get; set; }
        public string? UserName { get; set; }
        public string? NumberType { get; set; }
        public List<RTSPFSPatentWatchFromPSNumbers> Numbers { get; set; }
        public string? Notify { get; set; }
    }

    public class RTSPFSPatentWatchFromPSNumbers
    {
        public int AppId { get; set; }
        public string? Term { get; set; }

    }
    #endregion

}
