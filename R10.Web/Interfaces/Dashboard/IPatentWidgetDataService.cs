using R10.Core.DTOs;
using R10.Web.Models.DashboardViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IPatentWidgetDataService : IWidgetDataService
    {
        Task<IEnumerable<object>> AttorneyCaseLoad(UserWidgetViewModel widget);
        Task<ChartDTO> OutstandingActions(UserWidgetViewModel widget);
        Task<ChartDTO> PortfolioCount(UserWidgetViewModel widget);        
        Task<object> PortfolioByStatus(UserWidgetViewModel widget);
        Task<IEnumerable<object>> PortfolioByClient(UserWidgetViewModel widget);
        Task<IEnumerable<object>> PortfolioByCountry(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> YearToDateCount(UserWidgetViewModel widget);
        Task<IEnumerable<AttorneyCaseStatusViewModel>> AttorneyCaseStatus(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> FilingHistoryByYear(UserWidgetViewModel widget);
        Task<IEnumerable<WorldwideCoverageViewModel>> WorldwideCoverage(UserWidgetViewModel widget);
        Task<IEnumerable<object>> InactiveInventionWithActiveApp(UserWidgetViewModel widget);
        Task<IEnumerable<object>> LatestPTOTransactionHistory(UserWidgetViewModel widget);
        Task<IEnumerable<object>> PortfolioByFilingType(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> TopInventors(UserWidgetViewModel widget);
        Task<IEnumerable<object>> ExpiringPatentsInAMonth(UserWidgetViewModel widget);
        Task<IEnumerable<object>> ExpiredPatentsWithActiveStatus(UserWidgetViewModel widget);
        Task<IEnumerable<AttorneyCaseStatusViewModel>> AgentCaseStatus(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> ActivePatentsByTechnology(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> CostYearToDate(UserWidgetViewModel widget);
        Task<ChartDTO> CountryLawInfo(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> TopValuedPatents(UserWidgetViewModel widget);
        Task<IEnumerable<object>> ForeignFilingDeadlinesDue(UserWidgetViewModel widget);
        Task<IEnumerable<object>> ActiveWOInactiveCountries(UserWidgetViewModel widget);
        Task<ChartDTO> AverageDaysFilingToPublication(UserWidgetViewModel widget);
        Task<ChartDTO> AverageDaysFilingToIssue(UserWidgetViewModel widget);
        Task<IEnumerable<StackedChartViewModel>> OutstandingActionByAttorney(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> KeyPatentsByProducts(UserWidgetViewModel widget);
        Task<IEnumerable<object>> NumberOfPatentsExpiring(UserWidgetViewModel widget);
        Task<IEnumerable<object>> DuplicateAppNumber(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> TopPriorArtReferences(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> TopValuedPatentsByFamilies(UserWidgetViewModel widget);
        Task<IEnumerable<object>> InactiveCasesByClient(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> TopLicensee(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> ProlificInventors(UserWidgetViewModel widget);
        Task<IEnumerable<object>> PatentWatchLegalEvents(UserWidgetViewModel widget);
        Task<IEnumerable<object>> PCTNationalFilingsDue(UserWidgetViewModel widget);
        Task<IEnumerable<StackedChartViewModel>> AttorneyWorkloadPerMonth(UserWidgetViewModel widget);
        Task<IEnumerable<object>> NewPatentThisYear(UserWidgetViewModel widget);
        Task<IEnumerable<object>> InventorAwardsNotYetPaid(UserWidgetViewModel widget);
        Task<ChartDTO> InventorAwardsYearToDatePaid(UserWidgetViewModel widget);
        Task<object> InventorAwardsYearToDateCost(UserWidgetViewModel widget);
        Task<object> InventorAwardsBreakdownByType(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> InventorAwardsKeyTechnologies(UserWidgetViewModel widget);
        Task<object> InventorAwardsPaymentsIn5Years(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> InventorAwardsTopInventors(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> InventorAwardsTopInventorTotal(UserWidgetViewModel widget);
        Task<IEnumerable<ChartViewModel>> InventorAwardsCountry(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> InventorAwardsTopFamilies(UserWidgetViewModel widget);
        Task<IEnumerable<object>> PatentCenterEGrant(UserWidgetViewModel widget);
        Task<IEnumerable<object>> AutoDocketedActions(UserWidgetViewModel widget);
        Task<IEnumerable<object>> ActiveCasesWithInactiveTerminalDisclaimer(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> DisclosureYTDCount(UserWidgetViewModel widget);
        Task<ChartDTO> AverageDaysDisclosureToFiling(UserWidgetViewModel widget);
        Task<IEnumerable<object>> RecentlyDownloadedEPODocuments(UserWidgetViewModel widget);
        Task<IEnumerable<object>> AttorneyCaseLoadByFilingAndActions(UserWidgetViewModel widget);
        Task<IEnumerable<StackedChartViewModel>> AttorneyFilingPerMonth(UserWidgetViewModel widget);
        Task<IEnumerable<object>> EPOCommunicationWithoutDoc(UserWidgetViewModel widget);
        Task<IEnumerable<object>> MissingClaimsCountries(UserWidgetViewModel widget);
        Task<IEnumerable<object>> EPWithoutValidatedCountries(UserWidgetViewModel widget);
        Task<object> EPOCommunicationToDate(UserWidgetViewModel widget);
        Task<IEnumerable<object>> EPOCommunicationByAttorney(UserWidgetViewModel widget);
        Task<IEnumerable<object>> EPOCriticalCommunications(UserWidgetViewModel widget);
    }
}