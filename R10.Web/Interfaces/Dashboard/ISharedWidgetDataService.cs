using R10.Core.DTOs;
using R10.Web.Models.DashboardViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface ISharedWidgetDataService : IWidgetDataService
    {
        Task<IEnumerable<object>> CasesUpdateToday(UserWidgetViewModel widget);
        Task<IEnumerable<object>> UpcomingDueDates(UserWidgetViewModel widget);
        Task<IEnumerable<object>> InactiveCasesWithOpenActions(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> TopProductsBySales(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> ProductSalesByRegion(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> ProductSalesByQuarter(UserWidgetViewModel widget);
        Task<IEnumerable<ChartDTO>> ProductSalesGrowth(UserWidgetViewModel widget);
        Task<IEnumerable<WorldwideCoverageViewModel>> ProductCoverage(UserWidgetViewModel widget);
        Task<ChartDTO> ActiveByProduct(UserWidgetViewModel widget);
        Task<IEnumerable<CaseListViewModel>> PreviouslyViewdRecords(UserWidgetViewModel widget);
        Task<IEnumerable<CaseListViewModel>> LatestDeDocketInstructions(UserWidgetViewModel widget);
        Task<IEnumerable<object>> DelegatedActionsToYou(UserWidgetViewModel widget);
        Task<IEnumerable<object>> DelegatedActionsToOthers(UserWidgetViewModel widget);
        Task<ChartDTO> AverageDaysCompletingDelegatedActions(UserWidgetViewModel widget);
        Task<IEnumerable<object>> DocketCheck(UserWidgetViewModel widget);
        Task<IEnumerable<StackedChartViewModel>> DelegatedActionsWorkload(UserWidgetViewModel widget);
        Task<IEnumerable<object>> FavoriteRecords(UserWidgetViewModel widget);
        Task<IEnumerable<object>> RosterReport(UserWidgetViewModel widget);
    }
}
