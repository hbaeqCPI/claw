using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.DTOs
{
    public class RSReportParameters
    {
        public int SortOrder { get; set; }
        public int ReportFormat { get; set; }
        public bool PrintActionDueRemarks { get; set; }
        public bool PrintDueDateRemarks { get; set; }
        public bool PrintRemarks { get; set; }
        public string? PrintGoods { get; set; }
        public bool PrintImage { get; set; }
        public bool PrintImageDetail { get; set; }
        public bool PrintPastReminders { get; set; }
        public bool PrintInventors { get; set; }             
        public bool PrintShowCriteria { get; set; }
        public string? PrintSystems { get; set; }
        public int LayoutFormat { get; set; }
        public DateTime? FromDate { get; set; }        
        public DateTime? ToDate { get; set; }
       
        public string? IndicatorOp { get; set; }
        public string? CountryOp { get; set; }
        public string? ActionTypeOp { get; set; }
        public string? ActionDueOp { get; set; }
        public string? ClientOp { get; set; }
        public string? Area { get; set; }
        public string? FilterAtty { get; set; }
        public string? ActiveSwitch { get; set; }
        public string? ApplicationStatusesOp { get; set; }
        public string? ApplicationStatuses { get; set; }
        public string? CaseTypesOp { get; set; }
        public string? CaseTypes { get; set; }
        public string? ActionTypes { get; set; }
        public string? ActionDues { get; set; }
        public string? DueIndicators { get; set; }
        public string? Clients { get; set; }
        public string? Agent { get; set; }
        public string? Agents { get; set; }
        public string? AgentName { get; set; }
        public string? AgentNames { get; set; }
        public string? CaseNumber { get; set; }
        public string? CaseNumbers { get; set; }
        public string? Countries { get; set; }
        public string? Attorneys { get; set; }
        public string? Owners { get; set; }        
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }
        public string? ClassesOp { get; set; }
        public string? Classes { get; set; }
        public string? Class { get; set; }
        public string? UserID { get; set; }
        public string? ClientNames { get; set; }
        public string? CountryNames { get; set; }
        public string? AttorneyNames { get; set; }
        public string? OwnerNames { get; set; }
        public string? ActionType { get; set; }
        public string? ActionDue { get; set; }
        public string? Client { get; set; }
        public string? Attorney { get; set; }
        public string? Owner { get; set; }
        public string? ClientName { get; set; }
        public string? CountryName { get; set; }
        public string? AttorneyName { get; set; }
        public string? OwnerName { get; set; }
        public bool DeDocketInstructionOnly { get; set; }
        public string? InstructedBy { get; set; }
        public string? Keyword { get; set; }
        public string? Keywords { get; set; }
        public bool DelegatedToMeOnly { get; set; }
    }

    public class RSReportAttorney
    {
        public string? Attorney { get; set; }
        public string? AttorneyName { get; set; }
        public string? AttorneyEmail { get; set; }
    }

    public class RSPatentListReportParameters
    {
        public int SortOrder { get; set; }
        public int ReportFormat { get; set; }
        public bool PrintRemarks { get; set; }
        public bool PrintImage { get; set; }
        public bool PrintImageDetail { get; set; }
        public bool PrintInventors { get; set; }
        public bool PrintShowCriteria { get; set; }
        public bool PrintAbstract { get; set; }
        public bool PrintDesCtrys { get; set; }
        public bool PrintProducts { get; set; }
        public bool PrintSubjectMatters { get; set; }
        public bool PrintRelatedMatter { get; set; }
        public bool PrintKeywords { get; set; }
        public string? PrintActions { get; set; }
        public int DateType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string? CaseNumber { get; set; }
        public string? CaseNumbers { get; set; }
        public string? Area { get; set; }
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }
        public string? CaseTypesOp { get; set; }
        public string? CaseTypes { get; set; }
        public string? CountriesOp { get; set; }
        public string? Countries { get; set; }
        public string? ActiveSwitch { get; set; }
        public string? ApplicationStatusesOp { get; set; }
        public string? ApplicationStatuses { get; set; }
        public string? Clients { get; set; }
        public string? Attorneys { get; set; }
        public string? Owners { get; set; }
        public string? Agents { get; set; }
        public string? Inventor { get; set; }
        public string? Inventors { get; set; }
        public string? UserID { get; set; }
        public string? ClientNames { get; set; }
        public string? CountryNames { get; set; }
        public string? AttorneyNames { get; set; }
        public string? OwnerNames { get; set; }
        public string? AgentNames { get; set; }
        public string? Client { get; set; }
        public string? Attorney { get; set; }
        public string? Owner { get; set; }
        public string? Agent { get; set; }
        public string? ClientName { get; set; }
        public string? CountryName { get; set; }
        public string? AttorneyName { get; set; }
        public string? OwnerName { get; set; }
        public string? AgentName { get; set; }
        public string? Product { get; set; }
        public string? Products { get; set; }
        public string? SubjectMatter { get; set; }
        public string? SubjectMatters { get; set; }
        public string? Keyword { get; set; }
        public string? Keywords { get; set; }
    }

    public class RSTrademarkListReportParameters
    {
        public int SortOrder { get; set; }
        public int ReportFormat { get; set; }
        public bool PrintRemarks { get; set; }
        public bool PrintImage { get; set; }
        public bool PrintImageDetail { get; set; }
        public bool PrintGoods { get; set; }
        public bool PrintShowCriteria { get; set; }
        public bool PrintAssignments { get; set; }
        public bool PrintShowDefaultImage { get; set; }
        public bool PrintDesCtrys { get; set; }
        public bool PrintProducts { get; set; }
        public bool PrintKeywords { get; set; }
        public bool PrintRelatedMatter { get; set; }
        public string? PrintActions { get; set; }
        public int DateType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string? CaseNumber { get; set; }
        public string? CaseNumbers { get; set; }
        public string? Area { get; set; }
        public string? MarkType { get; set; }
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }
        public string? CaseTypesOp { get; set; }
        public string? CaseTypes { get; set; }
        public string? CountriesOp { get; set; }
        public string? Countries { get; set; }
        public string? ActiveSwitch { get; set; }
        public string? TrademarkStatusesOp { get; set; }
        public string? TrademarkStatuses { get; set; }
        public string? Clients { get; set; }
        public string? Attorneys { get; set; }
        public string? Owners { get; set; }
        public string? Agents { get; set; }
        public string? TrademarkName { get; set; }
        public string? TrademarkNames { get; set; }
        public string? UserID { get; set; }
        public string? ClientNames { get; set; }
        public string? CountryNames { get; set; }
        public string? AttorneyNames { get; set; }
        public string? OwnerNames { get; set; }
        public string? AgentNames { get; set; }
        public string? Client { get; set; }
        public string? Attorney { get; set; }
        public string? Owner { get; set; }
        public string? Agent { get; set; }
        public string? ClientName { get; set; }
        public string? CountryName { get; set; }
        public string? AttorneyName { get; set; }
        public string? OwnerName { get; set; }
        public string? AgentName { get; set; }
        public string? Product { get; set; }
        public string? Products { get; set; }
        public string? ClassesOp { get; set; }
        public string? Classes { get; set; }
        public string? Class { get; set; }
        public string? Goods { get; set; }
        public string? Keyword { get; set; }
        public string? Keywords { get; set; }
    }

    public class RSMatterListReportParameters
    {
        public int SortOrder { get; set; }
        public int ReportFormat { get; set; }
        public bool PrintRemarks { get; set; }
        public bool PrintImage { get; set; }
        public bool PrintImageDetail { get; set; }
        public bool PrintOtherParty { get; set; }
        public bool PrintShowCriteria { get; set; }
        public bool PrintTrademark { get; set; }
        public bool PrintOtherPartyTrademark { get; set; }
        public bool PrintPatent { get; set; }
        public bool PrintOtherPartyPatent { get; set; }
        public bool PrintKeywords { get; set; }
        public string? PrintActions { get; set; }
        public int DateType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string? MatterTitle { get; set; }
        public string? MatterTitles { get; set; }
        public string? Area { get; set; }
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }
        public string? MatterTypesOp { get; set; }
        public string? MatterTypes { get; set; }
        public string? CountryOp { get; set; }
        public string? Countries { get; set; }
        public string? ActiveSwitch { get; set; }
        public string? MatterStatusesOp { get; set; }
        public string? MatterStatuses { get; set; }
        public string? Clients { get; set; }
        public string? Attorneys { get; set; }
        public string? Agents { get; set; }
        public string? UserID { get; set; }
        public string? ClientNames { get; set; }
        public string? CountryNames { get; set; }
        public string? AttorneyNames { get; set; }
        public string? AgentNames { get; set; }
        public string? Client { get; set; }
        public string? Attorney { get; set; }
        public string? Agent { get; set; }
        public string? ClientName { get; set; }
        public string? CountryName { get; set; }
        public string? AttorneyName { get; set; }
        public string? AgentName { get; set; }
        public string? Keyword { get; set; }
        public string? Keywords { get; set; }
        public string? CaseNumber { get; set; }
        public string? CaseNumbers { get; set; }
    }
}
