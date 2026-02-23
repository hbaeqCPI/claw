using Microsoft.AspNetCore.Authorization;
using R10.Core.Entities.Identity;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System.Security.Claims;
using R10.Web.Models.DashboardViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using R10.Core.Entities.AMS;
using Microsoft.Extensions.Localization;
using R10.Web.Models;
using R10.Core.Entities.Shared;

namespace R10.Web.Areas.Admin.Services
{
    public class DefaultWidgetManager : IDefaultWidgetManager
    {
        private readonly ICPiDbContext _cpiDbContext;
        private readonly IAuthorizationService _authorizationService;

        public DefaultWidgetManager(
            ICPiDbContext cpiDbContext, 
            IAuthorizationService authorizationService)
        {
            _cpiDbContext = cpiDbContext;
            _authorizationService = authorizationService;
        }

        public IQueryable<CPiUserTypeDefaultWidget> DefaultWidgets => _cpiDbContext.GetRepository<CPiUserTypeDefaultWidget>().QueryableList;

        public async Task<List<CPiUserTypeDefaultWidget>> GetDefaultWidgets(CPiUserType userType, int widgetCategory)
        {
            return await DefaultWidgets
                                    .Where(widget => widget.UserType == userType && widget.CPiWidget.IsEnabled && widget.WidgetCategory == widgetCategory)
                                    .OrderBy(widget => widget.SortOrder)
                                    .ToListAsync();
        }

        public async Task<CPiUserTypeDefaultWidget> GetDefaultWidget(int widgetId, CPiUserType userType, int widgetCategory)
        {
            return await DefaultWidgets.Where(w => w.WidgetId == widgetId && w.UserType == userType && w.WidgetCategory == widgetCategory).FirstOrDefaultAsync();
        }

        public async Task<List<CPiWidget>> GetAvailableWidgets(CPiUserType userType, bool isCorporation)
        {
            var widgetMenuItems = await _cpiDbContext.GetReadOnlyRepositoryAsync<CPiWidget>().QueryableList
                                .Where(w => w.IsEnabled
                                    && (!w.CPiUserTypeDefaultWidgets.Any(uw => uw.UserType == userType))
                                    && ((isCorporation && w.SystemType.Contains("CORPORATION")) || (!isCorporation && w.SystemType.Contains("LAWFIRM")))
                                )
                                .ToListAsync();

            return widgetMenuItems;
        }

        public virtual async Task Add(CPiUserTypeDefaultWidget defaultWidget)
        {
            _cpiDbContext.GetRepository<CPiUserTypeDefaultWidget>().Add(defaultWidget);
            await _cpiDbContext.SaveChangesAsync();
        }

        public virtual async Task Update(CPiUserTypeDefaultWidget defaultWidget)
        {
            _cpiDbContext.GetRepository<CPiUserTypeDefaultWidget>().Update(defaultWidget);
            await _cpiDbContext.SaveChangesAsync();
        }

        public virtual async Task Delete(CPiUserTypeDefaultWidget defaultWidget)
        {
            _cpiDbContext.GetRepository<CPiUserTypeDefaultWidget>().Delete(defaultWidget);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task Move(int widgetId, int newIndex, CPiUserType userType, int widgetCategory)
        {
            var defaultWidgets = await GetDefaultWidgets(userType, widgetCategory);
            var defaultWidget = defaultWidgets.Where(w => w.WidgetId == widgetId).FirstOrDefault();

            if (defaultWidget != null)
            {
                List<CPiUserTypeDefaultWidget> widgets;
                if (defaultWidget.SortOrder > newIndex)
                {
                    widgets = defaultWidgets.FindAll(w => w.SortOrder >= newIndex && w.SortOrder < defaultWidget.SortOrder);
                    widgets.ForEach(widget => widget.SortOrder = widget.SortOrder + 1);
                }
                else
                {
                    widgets = defaultWidgets.FindAll(w => w.SortOrder <= newIndex && w.SortOrder > defaultWidget.SortOrder);
                    widgets.ForEach(widget => widget.SortOrder = widget.SortOrder - 1);
                }

                defaultWidget.SortOrder = newIndex;
                widgets.Add(defaultWidget);
                _cpiDbContext.GetRepository<CPiUserTypeDefaultWidget>().Update(widgets);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task Sort(List<CPiUserTypeDefaultWidget> defaultWidgets)
        {
            int sortOrder = 0;

            defaultWidgets.ForEach(w => w.SortOrder = sortOrder++);

            _cpiDbContext.GetRepository<CPiUserTypeDefaultWidget>().Update(defaultWidgets);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task Clear(CPiUserType userType, int widgetCategory)
        {
            var defaultWidgets = await DefaultWidgets.Where(w => w.UserType == userType && w.WidgetCategory == widgetCategory).ToListAsync();

            if (defaultWidgets.Count > 0)
            {
                _cpiDbContext.GetRepository<CPiUserTypeDefaultWidget>().Delete(defaultWidgets);
                await _cpiDbContext.SaveChangesAsync();
            }
        }
    }

    public interface IDefaultWidgetManager
    {
        IQueryable<CPiUserTypeDefaultWidget> DefaultWidgets { get; }

        Task<List<CPiUserTypeDefaultWidget>> GetDefaultWidgets(CPiUserType userType, int widgetCategory);
        Task<CPiUserTypeDefaultWidget> GetDefaultWidget(int widgetId, CPiUserType userType, int widgetCategory);
        Task<List<CPiWidget>> GetAvailableWidgets(CPiUserType userType, bool isCorporation);

        Task Add(CPiUserTypeDefaultWidget defaultWidget);
        Task Update(CPiUserTypeDefaultWidget defaultWidget);
        Task Delete(CPiUserTypeDefaultWidget defaultWidget);

        Task Move(int widgetId, int newIndex, CPiUserType userType, int widgetCategory);
        Task Sort(List<CPiUserTypeDefaultWidget> userWidgets);
        Task Clear(CPiUserType userType, int widgetCategory);
    }
}
