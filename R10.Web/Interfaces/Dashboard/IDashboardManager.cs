using R10.Core.DTOs;
using R10.Core.Identity;
using R10.Web.Models.DashboardViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IDashboardManager
    {
        IQueryable<CPiWidget> CPiWidgets { get; }

        Task<object> GetData(UserWidgetViewModel userWidget);
        Task<List<UserWidgetViewModel>> GetUserWidgets(string userId, int widgetCategory);
        Task<CPiUserWidget> GetUserWidget(string userId, int userWidgetId);
        Task<List<CPiWidget>> GetAddWidgetMenuItems(string userId);

        Task AddUserWidget(CPiUserWidget userWidget);
        Task RemoveUserWidget(CPiUserWidget userWidget);
        Task SortUserWidgets(List<CPiUserWidget> userWidgets);
        Task MoveUserWidget(CPiUserWidget userWidget, int newIndex, List<CPiUserWidget> userWidgets);
        Task UpdateWidget(CPiUserWidget userWidget);
        Task RemoveWidgetsByCategory(string userId, int widgetCategory);
        Task<UserWidgetViewModel> GetUserWidgetModel(string userId, int userWidgetId);
        Task AddCPiWidget(CPiWidget widget);
        Task UpdateCPiWidget(CPiWidget widget);
        Task<List<CPiWidget>> GetCPiWidgetByTitle(string title);
        Task<CPiWidget> GetCPiWidget(int widgetId);
        Task<List<CPiWidget>> GetCPiWidgetByQueryId(int queryId);
        Task<List<CPiUserWidget>> GetUserCustomWidgets(string userId);
        Task<string> WidgetTitleLocalizer(string title);
        Task<List<CPiUserWidget>> GetUserCustomWidgets(int widgetId);
        Task RemoveCPiWidget(CPiWidget widget);
        Task RemoveUserWidget(List<CPiUserWidget> userWidgets);
    }
}
