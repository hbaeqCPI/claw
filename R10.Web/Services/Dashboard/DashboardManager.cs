using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using R10.Core.DTOs;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using R10.Web.Interfaces;
using R10.Web.Models.DashboardViewModels;
using R10.Core.Entities.Shared;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Localization;
using R10.Web.Models;
using R10.Core.Helpers;
using DocuSign.eSign.Model;
using System.Collections;

namespace R10.Web.Services
{
    public class DashboardManager : IDashboardManager
    {
        private readonly ICPiDbContext _cpiDbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataQueryService _dataQueryService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ISystemSettings<DefaultSetting> _defaultSettings;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public DashboardManager(
            ISystemSettings<DefaultSetting> defaultSettings,
            ICPiDbContext cpiDbContext,
            IServiceProvider serviceProvider,
            IDataQueryService dataQueryService,
            IHttpContextAccessor httpContextAccessor,
            IStringLocalizer<SharedResource> localizer)
        {
            _cpiDbContext = cpiDbContext;
            _serviceProvider = serviceProvider;
            _dataQueryService = dataQueryService;
            _httpContextAccessor = httpContextAccessor;
            _defaultSettings = defaultSettings;
            _localizer = localizer;
        }

        public async Task<object> GetData(UserWidgetViewModel widget)
        {
            const string baseStore = "IWidgetDataService";

            string className = widget.CPiUserWidget.CPiWidget.RepositoryClassName;
            string methodName = widget.CPiUserWidget.CPiWidget.RepositoryMethodName;
            Type returnType = null;

            bool execStoredProc = className.ToLower() == baseStore.ToLower();

            if (string.IsNullOrEmpty(className))
            {
                execStoredProc = true;
                className = baseStore;
            }

            if (string.IsNullOrEmpty(methodName))
            {
                methodName = "GetData";
            }

            if (execStoredProc)
            {
                methodName = "GetFirstOrDefault";

                string repositoryReturnType = widget.CPiUserWidget.CPiWidget.RepositoryReturnType;

                Match match = new Regex("(?<=<)(.*?)(?=>)").Match(repositoryReturnType);
                if (match.Success)
                {
                    methodName = "GetList";
                    repositoryReturnType = match.Groups[1].Value;
                }

                returnType = Type.GetType(repositoryReturnType);

                if (returnType == null)
                {
                    try
                    {
                        string assemblyQualifiedName = AppDomain.CurrentDomain.GetAssemblies()
                                            .ToList()
                                            .SelectMany(x => x.GetTypes())
                                            .Where(x => x.Name == repositoryReturnType)
                                            .Select(x => x.AssemblyQualifiedName)
                                            .FirstOrDefault();
                        returnType = Type.GetType(assemblyQualifiedName);
                    }
                    catch (Exception ex)
                    {
                        var assemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies().ToList();
                        foreach (var item in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
                        {
                            if (!item.Name.Contains("R10."))
                                continue;

                            Assembly assembly = Assembly.Load(item);
                            var targetAssembly = assembly.GetTypes().ToList().Where(x => x.Name == repositoryReturnType);
                            if (!targetAssembly.Any())
                                continue;

                            var assemblyQualifiedNameNew = targetAssembly.Select(x => x.AssemblyQualifiedName).FirstOrDefault();
                            returnType = Type.GetType(assemblyQualifiedNameNew);
                            break;
                        }
                    }
                }
            }
            string assemblyName = AppDomain.CurrentDomain.GetAssemblies()
                                        .ToList()
                                        .SelectMany(x => x.GetTypes())
                                        .Where(x => x.Name == className)
                                        .Select(x => x.AssemblyQualifiedName)
                                        .FirstOrDefault();

            Type classType = Type.GetType(assemblyName);
            if (classType == null)
                return null;

            object widgetDataService = _serviceProvider.GetService(classType);
            if (widgetDataService == null)
                return null;

            MethodInfo methodInfo = classType.GetMethod(methodName);
            if (methodInfo == null)
                return null;

            dynamic awaitable;

            if (execStoredProc)
            {
                MethodInfo generic = methodInfo.MakeGenericMethod(returnType);
                awaitable = generic.Invoke(widgetDataService, new object[] { widget });
            }
            else
            {
                awaitable = methodInfo.Invoke(widgetDataService, new object[] { widget });
            }

            await awaitable;
            var data = awaitable.GetAwaiter().GetResult();

            //todo: dispose ???
            awaitable.Dispose();

            return data;
        }

        private IQueryable<CPiUserWidget> CPiUserWidgets => _cpiDbContext.GetRepository<CPiUserWidget>().QueryableList;
        public IQueryable<CPiWidget> CPiWidgets => _cpiDbContext.GetRepository<CPiWidget>().QueryableList;

        public async Task<List<UserWidgetViewModel>> GetUserWidgets(string userId, int widgetCategory)
        {
            var userWidgets = await CPiUserWidgets
                                    .Where(widget => widget.UserId == userId && widget.CPiWidget.IsEnabled && widget.WidgetCategory == widgetCategory)
                                    .OrderBy(widget => widget.SortOrder)
                                    .Include(widget => widget.CPiWidget)
                                    .Select(widget => new UserWidgetViewModel()
                                    {
                                        CPiUserWidget = widget
                                    })
                                    .ToListAsync();
            //Replace labels in Title
            foreach (UserWidgetViewModel item in userWidgets)
            {
                //item.widgetTitle = item.CPiUserWidget.CPiWidget.Title;
                item.WidgetTitle = item.CPiUserWidget.UserTitle ?? item.CPiUserWidget.CPiWidget.Title;

                if (Regex.IsMatch(item.WidgetTitle, @"\s*\[.*?\]\s*"))
                {
                    item.WidgetTitle = WidgetTitleLocalizer(item.WidgetTitle).Result;
                }
            }

            return userWidgets;
        }

        public async Task<CPiUserWidget> GetUserWidget(string userId, int userWidgetId)
        {
            return await CPiUserWidgets.FirstOrDefaultAsync(u => u.UserId == userId && u.Id == userWidgetId);
        }

        public async Task AddUserWidget(CPiUserWidget userWidget)
        {
            //Set default viewmode for new add widget
            var cpiWidget = CPiWidgets.Where(widget => widget.Id == userWidget.WidgetId).FirstOrDefault();
            JObject widgetSettings = new JObject();
            JObject settings = new JObject();
            if (!string.IsNullOrEmpty(cpiWidget.Settings))
            {
                widgetSettings = JObject.Parse(cpiWidget.Settings);
            }
            foreach (var token in widgetSettings)
            {
                if (token.Key == "ViewMode")
                {
                    settings[token.Key] = token.Value;
                    break;
                }
            }

            if (string.IsNullOrEmpty(userWidget.Settings))
            {
                userWidget.Settings = JsonConvert.SerializeObject(settings);
            }

            if (string.IsNullOrEmpty(userWidget.UserTitle))
            {
                userWidget.UserTitle = cpiWidget.Title;
            }

            _cpiDbContext.GetRepository<CPiUserWidget>().Add(userWidget);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task RemoveUserWidget(CPiUserWidget userWidget)
        {
            _cpiDbContext.GetRepository<CPiUserWidget>().Delete(userWidget);
            await _cpiDbContext.SaveChangesAsync();
            _cpiDbContext.Detach(userWidget);
        }

        public async Task SortUserWidgets(List<CPiUserWidget> userWidgets)
        {
            int sortOrder = 0;
            userWidgets.ForEach(w => { w.SortOrder = sortOrder++; w.CPiWidget = null; });

            _cpiDbContext.GetRepository<CPiUserWidget>().Update(userWidgets);
            await _cpiDbContext.SaveChangesAsync();
            _cpiDbContext.Detach(userWidgets);
        }

        public async Task MoveUserWidget(CPiUserWidget userWidget, int newIndex, List<CPiUserWidget> userWidgets)
        {
            List<CPiUserWidget> widgets;
            if (userWidget.SortOrder > newIndex)
            {
                widgets = userWidgets.FindAll(w => w.SortOrder >= newIndex && w.SortOrder < userWidget.SortOrder);
                widgets.ForEach(widget => widget.SortOrder = widget.SortOrder + 1);
            }
            else
            {
                widgets = userWidgets.FindAll(w => w.SortOrder <= newIndex && w.SortOrder > userWidget.SortOrder);
                widgets.ForEach(widget => widget.SortOrder = widget.SortOrder - 1);
            }

            userWidget.SortOrder = newIndex;
            widgets.Add(userWidget);
            widgets.ForEach(d => d.CPiWidget = null);

            _cpiDbContext.GetRepository<CPiUserWidget>().Update(widgets);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task<List<CPiWidget>> GetAddWidgetMenuItems(string userId)
        {
            var defaultSettings = await _defaultSettings.GetSetting();
            var DQMains = _dataQueryService.DataQueriesMain;
            var widgetMenuItems = await _cpiDbContext.GetReadOnlyRepositoryAsync<CPiWidget>().QueryableList
                                .Where(w => w.IsEnabled
                                    && (!w.CPiUserWidgets.Any(uw => uw.UserId == userId) || w.CPiUserWidgets.Any(uw => uw.WidgetCategory == 0 && uw.UserId == userId))
                                    && ((defaultSettings.IsCorporation && w.SystemType.Contains("CORPORATION")) || (!defaultSettings.IsCorporation && w.SystemType.Contains("LAWFIRM")))
                                    && (w.QueryId == null || ((w.CreatorId == userId || w.SharedWidget == true) && (DQMains.Any(d => d.QueryId == w.QueryId && (d.OwnedBy == _httpContextAccessor.HttpContext.User.GetEmail() || d.IsShared == true)))))
                                )
                                .ToListAsync();

            //Replace labels in Title
            foreach (CPiWidget item in widgetMenuItems)
            {
                if (Regex.IsMatch(item.Title, @"\s*\[.*?\]\s*"))
                {
                    item.Title = WidgetTitleLocalizer(item.Title).Result;
                }
            }

            return widgetMenuItems;
        }

        public async Task UpdateWidget(CPiUserWidget userWidget)
        {
            _cpiDbContext.GetRepository<CPiUserWidget>().Update(userWidget);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task RemoveWidgetsByCategory(string userId, int widgetCategory)
        {
            var userWidgets = await CPiUserWidgets
                                    .Where(widget => widget.UserId == userId && widget.CPiWidget.IsEnabled && widget.WidgetCategory == widgetCategory)
                                    .ToListAsync();
            if (userWidgets.Count > 0)
            {
                //userWidgets.ForEach(w => w.WidgetCategory = 0);
                //_cpiDbContext.GetRepository<CPiUserWidget>().Update(userWidgets);
                _cpiDbContext.GetRepository<CPiUserWidget>().Delete(userWidgets);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task<string> WidgetTitleLocalizer(string title)
        {
            var defaultSettings = await _defaultSettings.GetSetting();
            MatchCollection matches = Regex.Matches(title, @"\[.*?\]");
            string[] arr = matches.Cast<Match>()
                                  .Select(m => m.Groups[0].Value.Trim(new char[] { '[', ']' }))
                                  .ToArray();

            foreach (string str in arr)
            {
                string system = str.Split('.')[0];
                string label = str.Split('.')[1];

                string labelValue = await _cpiDbContext.GetReadOnlyRepositoryAsync<Option>().QueryableList
                                        .Where(o => o.OptionKey == system && o.OptionSubKey == label)
                                        .Select(o => o.OptionValue)
                                        .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(labelValue))
                {
                    title = title.Replace("[" + str + "]", labelValue);
                }

                if (label.ToLower() == "labelattorney")
                {
                    if (defaultSettings.IsCorporation)
                    {
                        title = title.Replace("[" + str + "]", "Outside Counsel");
                    }
                    else
                    {
                        title = title.Replace("[" + str + "]", "Attorney");
                    }
                }
            }

            return _localizer[title].ToString();
        }

        public async Task<UserWidgetViewModel> GetUserWidgetModel(string userId, int userWidgetId)
        {
            var userWidget = await CPiUserWidgets
                                    .Where(widget => widget.UserId == userId && widget.CPiWidget.IsEnabled && widget.Id == userWidgetId)
                                    .OrderBy(widget => widget.SortOrder)
                                    .Include(widget => widget.CPiWidget)
                                    .Select(widget => new UserWidgetViewModel()
                                    {
                                        CPiUserWidget = widget
                                    }).FirstOrDefaultAsync();

            if (userWidget == null) return new UserWidgetViewModel();

            //userWidget.widgetTitle = userWidget.CPiUserWidget.CPiWidget.Title;
            userWidget.WidgetTitle = userWidget.CPiUserWidget.UserTitle ?? userWidget.CPiUserWidget.CPiWidget.Title;

            if (Regex.IsMatch(userWidget.WidgetTitle, @"\s*\[.*?\]\s*"))
            {
                userWidget.WidgetTitle = WidgetTitleLocalizer(userWidget.WidgetTitle).Result;
            }

            return userWidget;
        }

        public async Task AddCPiWidget(CPiWidget widget)
        {
            _cpiDbContext.GetRepository<CPiWidget>().Add(widget);
            await _cpiDbContext.SaveChangesAsync();
        }
        public async Task UpdateCPiWidget(CPiWidget widget)
        {
            _cpiDbContext.GetRepository<CPiWidget>().Update(widget);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task<List<CPiWidget>> GetCPiWidgetByTitle(string title)
        {
            return await _cpiDbContext.GetRepository<CPiWidget>().QueryableList.AsNoTracking().Where(c => title.Equals(c.Title)).ToListAsync();
        }

        public async Task<CPiWidget> GetCPiWidget(int widgetId)
        {
            return await _cpiDbContext.GetRepository<CPiWidget>().QueryableList.AsNoTracking().FirstOrDefaultAsync(c => c.Id == widgetId);
        }

        public async Task<List<CPiWidget>> GetCPiWidgetByQueryId(int queryId)
        {
            return await _cpiDbContext.GetRepository<CPiWidget>().QueryableList.AsNoTracking().Where(c => c.QueryId == queryId).ToListAsync();
        }

        public async Task<List<CPiUserWidget>> GetUserCustomWidgets(string userId)
        {
            return await _cpiDbContext.GetRepository<CPiUserWidget>().QueryableList.AsNoTracking().Where(c => userId.Equals(c.UserId)).ToListAsync();
        }

        public async Task<List<CPiUserWidget>> GetUserCustomWidgets(int widgetId)
        {
            return await _cpiDbContext.GetRepository<CPiUserWidget>().QueryableList.AsNoTracking().Where(c => c.WidgetId == widgetId).ToListAsync();
        }

        public async Task RemoveCPiWidget(CPiWidget widget)
        {
            _cpiDbContext.GetRepository<CPiWidget>().Delete(widget);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task RemoveUserWidget(List<CPiUserWidget> userWidgets)
        {
            _cpiDbContext.GetRepository<CPiUserWidget>().Delete(userWidgets);
            await _cpiDbContext.SaveChangesAsync();
        }
    }
}
