using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Core.Interfaces;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.DashboardViewModels;
using R10.Web.Security;

namespace R10.Web.Controllers
{
    [Authorize(Policy = CPiAuthorizationPolicy.DashboardUser)]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IDashboardManager _dashboardManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;

        protected string UserId => User.GetUserIdentifier();

        public DashboardController(
            ILogger<DashboardController> logger,
            IDashboardManager dashboardManager,
            IAuthorizationService authorizationService,
            IStringLocalizer<SharedResource> localizer,
            ISystemSettings<PatSetting> patSettings,
            ISystemSettings<TmkSetting> tmkSettings)
        {
            _logger = logger;
            _dashboardManager = dashboardManager;
            _authorizationService = authorizationService;
            _localizer = localizer;
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
        }

        public async Task<IActionResult> Index()
        {
            if (User.SSORequired())
                return RedirectToAction("ExternalLogins", "Account");
            if (User.TwoFactorRequired())
                return RedirectToAction("TwoFactorAuthentication", "Account");

            var userWidgets = await GetUserWidgets(1);
            ViewData["HasWidgets"] = userWidgets.Count > 0;
            return View();
        }

        [HttpPost]
        public async Task<PartialViewResult> GetWidget(int id, bool queueNext, int widgetCategory, bool isRefresh = false)
        {
            var widgets = await GetUserWidgets(widgetCategory);
            if (widgets.Count < 1)
                return PartialView("_Widget", new WidgetViewModel());

            var widget = id == 0 ? widgets[0] : widgets.FirstOrDefault(w => w.CPiUserWidget.Id == id);
            if (widget == null)
                return PartialView("_Widget", new WidgetViewModel());

            var widgetViewModel = new WidgetViewModel()
            {
                Id = widget.CPiUserWidget.Id,
                WidgetId = widget.CPiUserWidget.WidgetId,
                Name = $"widget-{widget.CPiUserWidget.WidgetId}",
                Title = _localizer[widget.WidgetTitle],
                ViewName = $"./Widgets/{widget.CPiUserWidget.CPiWidget.ViewName}",
                SeriesColors = string.IsNullOrEmpty(widget.CPiUserWidget.CPiWidget.SeriesColors) ? null : widget.CPiUserWidget.CPiWidget.SeriesColors.Split('|'),
                Icon = string.IsNullOrEmpty(widget.CPiUserWidget.CPiWidget.Icon) ? DefaultWidgetIcon(widget.CPiUserWidget.CPiWidget.ViewName) : widget.CPiUserWidget.CPiWidget.Icon,
                CanExpand = widget.CPiUserWidget.CPiWidget.CanExpand,
                CanExportPDF = widget.CPiUserWidget.CPiWidget.CanCustomWidgetExport != false && widget.CPiUserWidget.CPiWidget.CanExport,
                CanExportExcel = widget.CPiUserWidget.CPiWidget.CanCustomWidgetExport != false && !string.IsNullOrEmpty(widget.CPiUserWidget.CPiWidget.ExportViewModel),
                UserSettings = new JObject(),
                WidgetSettings = new JObject(),
                Template = widget.CPiUserWidget.CPiWidget.Template,
                Policy = widget.CPiUserWidget.CPiWidget.Policy,
                CanDrillDown = widget.CPiUserWidget.CPiWidget.CanDrillDown,
                RowSpan = widget.CPiUserWidget.CPiWidget.RowSpan
            };

            if (queueNext && widgets.IndexOf(widget) + 1 < widgets.Count)
                widgetViewModel.NextId = widgets[widgets.IndexOf(widget) + 1].CPiUserWidget.Id;

            widget.HasRespOffice = User.HasRespOfficeFilter();
            widget.EntityFilterType = User.GetEntityFilterType();
            widget.IsAdmin = User.IsAdmin();
            widget.UserRoles = User.GetRoles();
            widget.MyUserType = User.GetUserType();
            widget.isRefresh = isRefresh;

            try { widgetViewModel.Data = await _dashboardManager.GetData(widget); }
            catch (Exception e) { _logger.LogError(e, "GetData failed"); }

            if (!string.IsNullOrEmpty(widget.CPiUserWidget.Settings))
            {
                try { widgetViewModel.UserSettings = JObject.Parse(widget.CPiUserWidget.Settings); }
                catch { }
            }

            return PartialView("_Widget", widgetViewModel);
        }

        [HttpPost]
        public async Task<object> GetUserWidgetData(int userWidgetId)
            => await _dashboardManager.GetData(await _dashboardManager.GetUserWidgetModel(UserId, userWidgetId));

        [HttpPost]
        public async Task<JsonResult> GetAvailableWidgets()
        {
            var menuItems = await GetAvailableWidgetList();
            return Json(menuItems.OrderBy(w => w.Title));
        }

        [HttpPost]
        public async Task<JsonResult> CopyWidget(int id, int loadedWidgets, int widgetCategory)
        {
            var widgets = await GetUserWidgets(widgetCategory);
            var widget = widgets.Find(w => w.CPiUserWidget.Id == id);
            if (widget == null) return Json(new { success = false, message = _localizer["Error copying widget."] });
            try
            {
                var lastWidget = widgets.LastOrDefault();
                var newUserWidget = new CPiUserWidget
                {
                    UserId = UserId,
                    WidgetId = widget.CPiUserWidget.WidgetId,
                    SortOrder = lastWidget == null ? 0 : lastWidget.CPiUserWidget.SortOrder + 1,
                    WidgetCategory = widgetCategory,
                    Settings = widget.CPiUserWidget.Settings,
                    UserTitle = widget.WidgetTitle
                };
                await _dashboardManager.AddUserWidget(newUserWidget);
                var userWidgets = await GetCPiUserWidgets(widgetCategory);
                await _dashboardManager.SortUserWidgets(userWidgets);
                return Json(new { success = true, message = _localizer["Widget successfully added."] });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "CopyWidget failed");
                return Json(new { success = false, message = _localizer["Unable to add widget."] });
            }
        }

        [HttpPost]
        public async Task<JsonResult> RemoveWidget(int id, int widgetCategory)
        {
            var widget = await _dashboardManager.GetUserWidget(UserId, id);
            if (widget == null) return Json(new { success = false, message = _localizer["Error removing widget."] });
            try
            {
                await _dashboardManager.RemoveUserWidget(widget);
                var userWidgets = await GetCPiUserWidgets(widgetCategory);
                await _dashboardManager.SortUserWidgets(userWidgets);
                return Json(new { success = true, message = _localizer["Widget successfully removed."] });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "RemoveWidget failed");
                return Json(new { success = false, message = _localizer["Unable to remove widget."] });
            }
        }

        [HttpPost]
        public async Task<JsonResult> RemoveWidgetsByCategory(int widgetCategory)
        {
            try
            {
                await _dashboardManager.RemoveWidgetsByCategory(UserId, widgetCategory);
                return Json(new { success = true, message = _localizer["Widget successfully deleted."] });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "RemoveWidgetsByCategory failed");
                return Json(new { success = false, message = _localizer["Unable to remove widget."] });
            }
        }

        [HttpPost]
        public async Task<JsonResult> SortUserWidget(int userWidgetId, int newIndex, int widgetCategory)
        {
            var widgets = await GetCPiUserWidgets(widgetCategory);
            var widget = widgets.Find(w => w.Id == userWidgetId);
            if (widget == null) return Json(new { success = false, message = _localizer["Error sorting widgets."] });
            try
            {
                await _dashboardManager.MoveUserWidget(widget, newIndex, widgets);
                return Json(new { success = true, message = _localizer["Widget successfully moved."] });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SortUserWidget failed");
                return Json(new { success = false, message = _localizer["Unable to move widget."] });
            }
        }

        [HttpPost]
        public async Task<JsonResult> SaveSetting(int id, string setting)
        {
            var widget = await _dashboardManager.GetUserWidget(UserId, id);
            if (widget == null) return Json(new { success = false });
            widget.Settings = setting;
            await _dashboardManager.UpdateWidget(widget);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> SaveWidgetTitle(int userWidgetId, string widgetTitle)
        {
            var widget = await _dashboardManager.GetUserWidget(UserId, userWidgetId);
            if (widget == null) return Json(new { success = false });
            widget.UserTitle = widgetTitle;
            await _dashboardManager.UpdateWidget(widget);
            return Json(new { success = true });
        }

        public async Task<IActionResult> DashboardSetting() => PartialView("_DashboardSettings");

        public async Task<IActionResult> GetWidgetSystem(string property, string text, FilterType filterType, string requiredRelation = "", int widgetCategory = 0)
        {
            var availableWidgets = await GetAvailableWidgetList();
            var availableSystems = availableWidgets.Select(d => d.System).Distinct().ToList();
            var widgetSystems = new List<SelectListItem> { new SelectListItem { Text = _localizer["All"].ToString(), Value = "all" } };
            if (widgetCategory != 7)
                widgetSystems.AddRange(WidgetSystem.Systems.Where(d => availableSystems.Contains(d.Name)).OrderBy(o => o.SortOrder).Select(d => new SelectListItem { Text = _localizer[d.DisplayName].ToString(), Value = d.Name }).ToList());
            return Json(widgetSystems.OrderBy(o => o.Text).ToList());
        }

        public async Task<IActionResult> GetWidgetTitles(string property, string text, FilterType filterType, string requiredRelation = "", string widgetSystem = "", int widgetCategory = 0)
        {
            var availableWidgets = await GetAvailableWidgetList();
            var data = availableWidgets.Where(d => (widgetCategory != 7 && (d.System == widgetSystem || widgetSystem == "all") && d.System != "customwidget") || (widgetCategory == 7 && d.System == "customwidget")).OrderBy(o => o.Title).Select(d => new { WidgetTitle = d.Title }).ToList();
            return Json(data);
        }

        public async Task<IActionResult> WidgetMenuRead([DataSourceRequest] DataSourceRequest request, string widgetSystem, int widgetCategory, List<int> selectedIds, string widgetTitle)
        {
            var availableWidgets = await GetAvailableWidgetList();
            var data = availableWidgets.Where(d => ((widgetCategory != 7 && (d.System == widgetSystem || widgetSystem == "all") && d.System != "customwidget") || (widgetCategory == 7 && d.System == "customwidget")) && (!selectedIds?.Any() ?? true || !selectedIds.Contains(d.Id)) && (string.IsNullOrEmpty(widgetTitle) || d.Title.ToLower().Contains(widgetTitle.ToLower()))).DistinctBy(d => d.Title).OrderBy(o => o.Title).ToList();
            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost]
        public async Task<IActionResult> SaveDashboardSetting(List<DashboardSettingViewModel> dashboardSettings, List<WidgetSettingViewModel>? widgetSettings)
            => Json(new { success = true });

        private async Task<List<UserWidgetViewModel>> GetUserWidgets(int widgetCategory)
        {
            var enabledUserWidgets = await _dashboardManager.GetUserWidgets(UserId, widgetCategory);
            var userWidgets = new List<UserWidgetViewModel>();
            foreach (var widget in enabledUserWidgets)
                if (await Authorize(widget.CPiUserWidget.CPiWidget.Policy))
                    userWidgets.Add(widget);
            return userWidgets;
        }

        private async Task<List<CPiUserWidget>> GetCPiUserWidgets(int widgetCategory)
        {
            var enabledUserWidgets = await _dashboardManager.GetUserWidgets(UserId, widgetCategory);
            var userWidgets = new List<CPiUserWidget>();
            foreach (var widget in enabledUserWidgets)
                if (await Authorize(widget.CPiUserWidget.CPiWidget.Policy))
                    userWidgets.Add(widget.CPiUserWidget);
            return userWidgets;
        }

        private async Task<List<AddWidgetMenuViewModel>> GetAvailableWidgetList()
        {
            var widgets = await _dashboardManager.GetAddWidgetMenuItems(UserId);
            var menuItems = new List<AddWidgetMenuViewModel>();

            foreach (var widget in widgets)
            {
                if (!await Authorize(widget.Policy)) continue;
                try
                {
                    var systemCategories = (widget.SystemCategory ?? "").Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();
                    foreach (var cat in systemCategories)
                    {
                        var widgetSystem = "";
                        if (cat.Equals(SystemType.Patent) && User.HasDashboardAccess(SystemType.Patent)) widgetSystem = SystemType.Patent;
                        else if (cat.Equals(SystemType.Trademark) && User.HasDashboardAccess(SystemType.Trademark)) widgetSystem = SystemType.Trademark;
                        else if (cat.Equals(SystemType.Shared) && User.HasDashboardAccess(SystemType.Shared)) widgetSystem = SystemType.Shared;
                        else if (cat.ToLower().Equals("customwidget")) widgetSystem = "customwidget";

                        if (!string.IsNullOrEmpty(widgetSystem))
                            menuItems.Add(new AddWidgetMenuViewModel { System = widgetSystem, Id = widget.Id, Title = _localizer[widget.Title].ToString(), Icon = string.IsNullOrEmpty(widget.Icon) ? DefaultWidgetIcon(widget.ViewName) : widget.Icon });
                    }
                }
                catch (Exception e) { _logger.LogError(e, "GetAvailableWidgetList"); }
            }
            return menuItems;
        }

        private async Task<bool> Authorize(string requirements)
        {
            try
            {
                var policies = (requirements ?? "").Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList();
                if (!policies.Any()) return true;
                foreach (var policy in policies)
                    if (policy == "*" || (await _authorizationService.AuthorizeAsync(User, policy)).Succeeded) return true;
                return false;
            }
            catch { return false; }
        }

        private static string DefaultWidgetIcon(string viewName)
        {
            var v = (viewName ?? "").ToLower();
            if (v.IndexOf("bar") >= 0 || v.IndexOf("column") >= 0) return "fa fa-chart-bar";
            if (v.IndexOf("donut") >= 0 || v.IndexOf("pie") >= 0) return "fa fa-chart-pie";
            return "fa fa-chart-area";
        }
    }
}
