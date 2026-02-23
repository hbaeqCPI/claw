using Microsoft.EntityFrameworkCore;

namespace R10.Core.Queries.Shared
{
    [Keyless]
    public class QuickDocketLookupDTO
    {
        public string? Text { get; set; }
    }

    [Keyless]
    public class QDActionTypeLookupDTO
    {
        public string? ActionType { get; set; }
    }
    [Keyless]
    public class QDActionDueLookupDTO
    {
        public string? ActionDue { get; set; }
    }
    [Keyless]
    public class QDCaseNumberLookupDTO
    {
        public string? CaseNumber { get; set; }
    }
    [Keyless]
    public class QDCaseTypeLookupDTO
    {
        public string? CaseType { get; set; }
        public string? Description { get; set; }
    }
    [Keyless]
    public class QDRespOfficeLookupDTO
    {
        public string? RespOffice { get; set; }
    }
    [Keyless]
    public class QDClientRefLookupDTO
    {
        public string? ClientRef { get; set; }
    }
    [Keyless]
    public class QDStatusLookupDTO
    {
        public string? Status { get; set; }
    }
    [Keyless]
    public class QDTitleLookupDTO
    {
        public string? Title { get; set; }
    }
    [Keyless]
    public class QDIndicatorLookupDTO
    {
        public string? Indicator { get; set; }
    }
    [Keyless]
    public class QDCountryLookupDTO
    {
        public string? Country { get; set; }
        public string? CountryName { get; set; }
    }
    [Keyless]
    public class QDClientLookupDTO
    {
        public string? Client { get; set; }
        public string? ClientName { get; set; }
    }
    [Keyless]
    public class QDAgentLookupDTO
    {
        public string? Agent { get; set; }
        public string? AgentName { get; set; }
    }
    [Keyless]
    public class QDOwnerLookupDTO
    {
        public string? Owner { get; set; }
        public string? OwnerName { get; set; }
    }
    [Keyless]
    public class QDAttorneyLookupDTO
    {
        public string? Attorney { get; set; }
        public string? AttorneyName { get; set; }
    }
    [Keyless]
    public class QDDeDocketInstructionLookupDTO
    {
        public string? Instruction { get; set; }
    }
    [Keyless]
    public class QDDeDocketInstructedByLookupDTO
    {
        public string? InstructedBy { get; set; }
    }
}
