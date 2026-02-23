using R10.Core.DTOs;
using R10.Core.Entities.Trademark;
using R10.Web.Models.DashboardViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface ITrademarkWidgetDataService : IWidgetDataService
    {        
        Task<IEnumerable<AttorneyCaseStatusViewModel>> AttorneyCaseStatus(UserWidgetViewModel widget);
        Task<IEnumerable<object>> AttorneyCaseLoad(UserWidgetViewModel widget);
        Task<IEnumerable<object>> RenewalTrademarks(UserWidgetViewModel widget);
        Task<IEnumerable<object>> PortfolioByStatus(UserWidgetViewModel widget);
        Task<IEnumerable<object>> PortfolioByCountry(UserWidgetViewModel widget);
        Task<IEnumerable<object>> PortfolioByClient(UserWidgetViewModel widget);
        Task<IEnumerable<object>> PortfolioByMarkType(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> PortfolioByClass(UserWidgetViewModel widget);
        Task<IEnumerable<WorldwideCoverageViewModel>> TmkCoverage(UserWidgetViewModel widget);
        Task<IEnumerable<AttorneyCaseStatusViewModel>> AgentCaseStatus(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> CostYearToDate(UserWidgetViewModel widget);
        Task<ChartDTO> CountryLawInfo(UserWidgetViewModel widget);
        Task<ChartDTO> AverageDaysFilingToPublication(UserWidgetViewModel widget);
        Task<ChartDTO> AverageDaysFilingToIssue(UserWidgetViewModel widget);
        Task<IEnumerable<StackedChartViewModel>> OutstandingActionByAttorney(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> KeyTrademarksByProducts(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> FilingHistoryByYear(UserWidgetViewModel widget);
        Task<IEnumerable<object>> LatestPTOActionDownload(UserWidgetViewModel widget);
        Task<IEnumerable<AttorneyCaseStatusViewModel>> AverageClassPerApp(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> TopCountriesActiveMarkCount(UserWidgetViewModel widget);
        Task<ChartDTO> TotalRenewalCount(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> ClassesByCountry(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> TrademarksYTDCount(UserWidgetViewModel widget);
        Task<ChartDTO> TotalPortfolioCount(UserWidgetViewModel widget);
        Task<IEnumerable<object>> InactiveCasesByClient(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> TopLicensee(UserWidgetViewModel widget);
        Task<IEnumerable<object>> PortfolioRenewalRate(UserWidgetViewModel widget);
        Task<IEnumerable<StackedChartViewModel>> AttorneyWorkloadPerMonth(UserWidgetViewModel widget);
        Task<IEnumerable<object>> NewTrademarkThisYear(UserWidgetViewModel widget);
        Task<ChartDTO> AverageLifeInYears(UserWidgetViewModel widget);
        Task<ChartDTO> AverageLifeInMonths(UserWidgetViewModel widget);
        Task<IEnumerable<object>> ActiveTrademarksWithoutClass(UserWidgetViewModel widget);
        Task<IEnumerable<object>> TopLicenseTrademarks(UserWidgetViewModel widget);
        Task<IEnumerable<object>> AttorneyCaseLoadByFilingAndActions(UserWidgetViewModel widget);
    }
}
