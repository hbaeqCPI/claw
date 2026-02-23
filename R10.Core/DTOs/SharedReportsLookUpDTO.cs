using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Queries.Shared
{
    class SharedReportsLookUpDTO
    {
        public string? Text { get; set; }
    }

    public class SharedReportActionTypeLookupDTO
    {
        public string? ActionType { get; set; }
    }

    public class SharedReportActionDueLookupDTO
    {
        public string? ActionDue { get; set; }
    }

    public class SharedReportIndicatorLookupDTO
    {
        public string? Indicator { get; set; }
    }

    public class SharedReportCountryLookupDTO
    {
        public string? Country { get; set; }
        public string? CountryName { get; set; }
    }

    public class SharedReportAreaLookupDTO
    {
        public string? Area { get; set; }
    }

    public class SharedReportClientLookupDTO
    {
        public string? ClientCode { get; set; }
        public string? ClientName { get; set; }
        public int? ClientID { get; set; }
    }

    public class SharedReportOwnerLookupDTO
    {
        public string? OwnerCode { get; set; }
        public string? OwnerName { get; set; }
        public int OwnerID { get; set; }
    }

    public class SharedReportAttorneyLookupDTO
    {
        public string? AttorneyCode { get; set; }
        public string? AttorneyName { get; set; }
        public int AttorneyID { get; set; }
    }

    public class SharedReportAgentLookupDTO
    {
        public string? AgentCode { get; set; }
        public string? AgentName { get; set; }
        public int AgentID { get; set; }
    }

    public class SharedReportStatusLookupDTO
    {
        public string? Status { get; set; }
        public bool ActiveSwitch { get; set; }
    }

    public class SharedReportCaseTypeLookupDTO
    {
        public string? CaseType { get; set; }
    }

    public class SharedReportResponsibleOfficeLookupDTO
    {
        public string? RespOffice { get; set; }
    }

    public class SharedReportCaseNumberLookupDTO
    {
        public string? CaseNumber { get; set; }
    }

    public class SharedReportCostTypeLookupDTO
    {
        public string? CostType { get; set; }
    }
}
