using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using R10.Core;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.DMS;
using R10.Core.Interfaces.Patent;
using R10.Infrastructure.Data;
using R10.Infrastructure.Identity;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.DashboardViewModels;
using System.Linq.Expressions;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using R10.Web.Extensions.ActionResults;
using R10.Web.Security;
using R10.Web.Areas.Admin.Views;
using R10.Web.Models.PageViewModels;
using R10.Web.Areas.Admin.Services;
using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
    public class DefaultWidgetsController : BaseController
    {
        private readonly IDefaultWidgetManager _defaultWidgetManager;
        private readonly ILogger _logger;
        private readonly IDashboardManager _dashboardManager;
        private readonly IAuthorizationService _authorizationService;

        private readonly IStringLocalizer<SharedResource> _localizer;

        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;
        private readonly ISystemSettings<GMSetting> _gmSettings;
        private readonly ISystemSettings<DefaultSetting> _defaultSettings;

        protected string UserId { get => User.GetUserIdentifier(); }

        public DefaultWidgetsController(
            IDefaultWidgetManager defaultWidgetManager,
            ILogger<DefaultWidgetsController> logger,
            IDashboardManager dashboardManager,
            IAuthorizationService authorizationService,
            IStringLocalizer<SharedResource> localizer,
            ISystemSettings<PatSetting> patSettings,
            ISystemSettings<TmkSetting> tmkSettings,
            ISystemSettings<GMSetting> gmSettings,
            ISystemSettings<DefaultSetting> defaultSettings)
        {
            _defaultWidgetManager = defaultWidgetManager;
            _logger = logger;
            _dashboardManager = dashboardManager;
            _authorizationService = authorizationService;
            _localizer = localizer;
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            _gmSettings = gmSettings;
            _defaultSettings = defaultSettings;
        }

        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_SidebarNav";

        public async Task<IActionResult> Index()
        {
            //default (Admin-Recent Activity)
            var userWidgets = await GetUserWidgets(CPiUserType.Administrator, WidgetCategory.DefaultCategoryId);

            ViewData["HasWidgets"] = (userWidgets.Count() > 0);

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Default Widgets"].ToString(),
                PageId = "defaultWidgets",
                MainPartialView = "_Index",
                //MainViewModel = null,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.DefaultWidgets
            };

            return View("Index", sidebarModel);
        }

        [HttpPost]
        public async Task<PartialViewResult> GetWidget(int id, bool queueNext, int widgetCategory, CPiUserType userType, bool isRefresh = false)
        {
            var widgets = await GetUserWidgets(userType, widgetCategory);

            UserWidgetViewModel widget;
            WidgetViewModel widgetViewModel;

            if (widgets.Count() < 1)
                return PartialView("_Widget", new WidgetViewModel());

            if (id == 0)
            {
                widget = widgets[0];
                queueNext = true;
            }
            else
                widget = widgets.FirstOrDefault(w => w.CPiUserWidget.WidgetId == id);

            if (widget == null)
                return PartialView("_Widget", new WidgetViewModel());

            widgetViewModel = new WidgetViewModel()
            {
                Id = widget.CPiUserWidget.WidgetId,
                Name = $"widget-{widget.CPiUserWidget.WidgetId}",
                Title = _localizer[widget.WidgetTitle].ToString(),
                ViewName = $"/Views/Dashboard/Widgets/{widget.CPiUserWidget.CPiWidget.ViewName}",
                SeriesColors = string.IsNullOrEmpty(widget.CPiUserWidget.CPiWidget.SeriesColors) ? null : widget.CPiUserWidget.CPiWidget.SeriesColors.Split('|'),
                Icon = string.IsNullOrEmpty(widget.CPiUserWidget.CPiWidget.Icon) ? DefaultWidgetIcon(widget.CPiUserWidget.CPiWidget.ViewName) : widget.CPiUserWidget.CPiWidget.Icon,
                CanExpand = widget.CPiUserWidget.CPiWidget.CanExpand,
                CanExportPDF = false, //widget.CPiUserWidget.CanExport == null || widget.CPiUserWidget.CanExport == true ? widget.CPiUserWidget.CPiWidget.CanExport : false,
                CanExportExcel = false, //widget.CPiUserWidget.CanExport == null || widget.CPiUserWidget.CanExport == true ? !string.IsNullOrEmpty(widget.CPiUserWidget.CPiWidget.ExportViewModel) : false,
                UserSettings = new JObject(),
                WidgetSettings = new JObject(),
                Template = widget.CPiUserWidget.CPiWidget.Template,
                LabelTemplate = widget.CPiUserWidget.CPiWidget.LabelTemplate,
                TooltipTemplate = widget.CPiUserWidget.CPiWidget.TooltipTemplate,
                Policy = widget.CPiUserWidget.CPiWidget.Policy,
                CanDrillDown = false, //widget.CPiUserWidget.CPiWidget.CanDrillDown,
                CanExportPpt = false, //widget.CPiUserWidget.CanExport == null || widget.CPiUserWidget.CanExport == true ? widget.CPiUserWidget.CPiWidget.CanExportPpt : false             
                CanEmail = false,
                RowSpan = widget.CPiUserWidget.CPiWidget.RowSpan
            };

            if (queueNext)
            {
                int nextIndex = widgets.IndexOf(widget) + 1;
                if (nextIndex < widgets.Count())
                    widgetViewModel.NextId = widgets[nextIndex].CPiUserWidget.WidgetId;
            }

            try
            {
                widget.HasRespOffice = User.HasRespOfficeFilter();
                widget.EntityFilterType = User.GetEntityFilterType();

                widget.IsAdmin = User.IsAdmin();
                widget.UserRoles = User.GetRoles();
                widget.MyUserType = User.GetUserType();

                widget.isRefresh = isRefresh;
                
                widgetViewModel.Data = await _dashboardManager.GetData(widget);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            if (!string.IsNullOrEmpty(widget.CPiUserWidget.CPiWidget.Settings))
            {
                try
                {
                    widgetViewModel.WidgetSettings = JObject.Parse(widget.CPiUserWidget.CPiWidget.Settings);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
            }

            if (!string.IsNullOrEmpty(widget.CPiUserWidget.Settings))
            {
                try
                {
                    JObject settings = JObject.Parse(widget.CPiUserWidget.Settings);
                    widgetViewModel.UserSettings = settings;

                    if (settings.Property("ViewMode") != null)
                        widgetViewModel.ViewMode = settings["ViewMode"].Value<int>();
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
            }
            else if (string.IsNullOrEmpty(widget.CPiUserWidget.Settings) && widgetViewModel.WidgetSettings.Property("ViewMode") != null)
            {
                try
                {
                    widgetViewModel.ViewMode = widgetViewModel.WidgetSettings["ViewMode"].Value<int>();
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
            }

            return PartialView("_Widget", widgetViewModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> RemoveWidget(int id, int widgetCategory, CPiUserType userType)
        {
            var widget = await _defaultWidgetManager.GetDefaultWidget(id, userType, widgetCategory);

            if (widget != null)
            {
                try
                {
                    await _defaultWidgetManager.Delete(widget);

                    var defaultWidgets = await _defaultWidgetManager.GetDefaultWidgets(userType, widgetCategory);
                    await _defaultWidgetManager.Sort(defaultWidgets);

                    return Json(new { message = _localizer["Widget successfully removed."].ToString() });
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    return BadRequest(_localizer["Unable to remove widget."].ToString());
                }

            }

            return BadRequest(_localizer["Error removing widget."].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> RemoveWidgetsByCategory(int widgetCategory, CPiUserType userType)
        {
            try
            {
                await _defaultWidgetManager.Clear(userType, widgetCategory);
                return Json(new { message = _localizer["Widgets successfully removed."].ToString() });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(_localizer["Unable to remove widgets."].ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SortDefaultWidget(int id, int newIndex, int widgetCategory, CPiUserType userType)
        {
            try
            {
                await _defaultWidgetManager.Move(id, newIndex, userType, widgetCategory);
                return Json(new { message = _localizer["Widget successfully moved."].ToString() });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(_localizer["Unable to move widget."].ToString());
            }
        }

        //[HttpPost]
        //public async Task<JsonResult> GetAvailableWidgets(CPiUserType userType)
        //{
        //    var tmkSettings = await _tmkSettings.GetSetting();
        //    var patSettings = await _patSettings.GetSetting();
        //    var gmSettings = await _gmSettings.GetSetting();
        //    var menuItems = new List<AddWidgetMenuViewModel>();
        //    var widgets = await _defaultWidgetManager.GetAvailableWidgets(userType, User.GetClientType().ToLower() == "corporation");

        //    //Replace labels in Title
        //    foreach (var item in widgets)
        //    {
        //        if (Regex.IsMatch(item.Title, @"\s*\[.*?\]\s*"))
        //            item.Title = _dashboardManager.WidgetTitleLocalizer(item.Title).Result;
        //    }

        //    foreach (CPiWidget widget in widgets)
        //    {
        //        if (await Authorize(widget.Policy))
        //        {
        //            try
        //            {
        //                var systemCategories = widget.SystemCategory.Split(',').ToList().Select(t => t.Trim());
        //                if (!systemCategories.Any()) continue;

        //                foreach (var cat in systemCategories)
        //                {
        //                    var widgetSystem = "";
        //                    if (cat.ToLower().Equals("patent")) widgetSystem = "pat";
        //                    else if (cat.ToLower().Equals("rts") && patSettings.IsRTSOn) widgetSystem = "pat";
        //                    else if (cat.ToLower().Equals("trademark")) widgetSystem = "tmk";
        //                    else if (cat.ToLower().Equals("trademarklink") && tmkSettings.IsTLOn) widgetSystem = "tmk";
        //                    else if (cat.ToLower().Equals("generalmatter")) widgetSystem = "gm";
        //                    else if (cat.ToLower().Equals("ams")) widgetSystem = "ams";
        //                    else if (cat.ToLower().Equals("amsstandalone") && !User.IsSystemEnabled(SystemType.Patent)) widgetSystem = "ams";
        //                    else if (cat.ToLower().Equals("dms")) widgetSystem = "dms";
        //                    else if (cat.ToLower().Equals("shared")) widgetSystem = "shared";
        //                    else if (cat.ToLower().Equals("shareddedocket") && (patSettings.IsDeDocketOn || tmkSettings.IsDeDocketOn || gmSettings.IsDeDocketOn)) widgetSystem = "shared";
        //                    else if (cat.ToLower().Equals("searchrequest")) widgetSystem = "searchrequest";
        //                    else if (cat.ToLower().Equals("patclearance")) widgetSystem = "patclearance";
        //                    else if (cat.ToLower().Equals("product") && ((patSettings.IsProductsOn && widget.Policy.Contains("CanAccessPatentProducts")) || (tmkSettings.IsProductsOn && widget.Policy.Contains("CanAccessTrademarkProducts")) || (gmSettings.IsProductsOn && widget.Policy.Contains("CanAccessGeneralMatterProducts")))) widgetSystem = "product";

        //                    if (!string.IsNullOrEmpty(widgetSystem))
        //                        menuItems.Add(new AddWidgetMenuViewModel()
        //                        {
        //                            System = widgetSystem,
        //                            Id = widget.Id,
        //                            Title = _localizer[widget.Title].ToString(),
        //                            Icon = string.IsNullOrEmpty(widget.Icon) ? DefaultWidgetIcon(widget.ViewName) : widget.Icon
        //                        });
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                _logger.LogError(e.Message);
        //            }
        //        }
        //    }

        //    return Json(menuItems.OrderBy(w => w.Title));
        //}

        private string DefaultWidgetIcon(string viewName)
        {
            string defaultIcon;

            viewName = viewName.ToLower();

            if (viewName.IndexOf("bar") >= 0 || viewName.IndexOf("column") >= 0)
            {
                defaultIcon = "fa fa-chart-bar";
            }
            else if (viewName.IndexOf("donut") >= 0 || viewName.IndexOf("pie") >= 0)
            {
                defaultIcon = "fa fa-chart-pie";
            }
            else
            {
                defaultIcon = "fa fa-chart-area";
            }

            return defaultIcon;
        }

        private async Task<bool> Authorize(string requirements)
        {
            try
            {
                var policies = requirements.Split(',').ToList().Select(p => p.Trim());
                if (!policies.Any())
                    return true;

                foreach (var policy in policies)
                {
                    if (string.IsNullOrEmpty(policy) || policy == "*" || (await _authorizationService.AuthorizeAsync(User, policy)).Succeeded)
                        return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            return false;
        }

        private async Task<List<UserWidgetViewModel>> GetUserWidgets(CPiUserType userType, int widgetCategory)
        {
            var userWidgets = await _defaultWidgetManager.DefaultWidgets
                                    .Where(widget => widget.UserType == userType && widget.CPiWidget.IsEnabled && widget.WidgetCategory == widgetCategory)
                                    .OrderBy(widget => widget.SortOrder)
                                    .Select(widget => new UserWidgetViewModel()
                                    {
                                        CPiUserWidget = new CPiUserWidget()
                                        {
                                            CPiWidget = widget.CPiWidget,
                                            SortOrder = widget.SortOrder,
                                            UserId = UserId,
                                            WidgetId = widget.WidgetId
                                        },
                                        IsAdmin = true,
                                        WidgetTitle = widget.CPiWidget.Title
                                    })
                                    .ToListAsync();

            //Replace labels in Title
            foreach (var item in userWidgets)
            {
                if (Regex.IsMatch(item.WidgetTitle, @"\s*\[.*?\]\s*"))
                        item.WidgetTitle = _dashboardManager.WidgetTitleLocalizer(item.WidgetTitle).Result;
            }

            return userWidgets;
        }

        private async Task<List<AddWidgetMenuViewModel>> GetAvailableWidgetList(CPiUserType userType)
        {            
            var widgets = await _defaultWidgetManager.GetAvailableWidgets(userType, User.GetClientType().ToLower() == "corporation");
            List<AddWidgetMenuViewModel> menuItems = new List<AddWidgetMenuViewModel>();

            var tmkSettings = await _tmkSettings.GetSetting();
            var patSettings = await _patSettings.GetSetting();
            var gmSettings = await _gmSettings.GetSetting();
            var defaultSettings = await _defaultSettings.GetSetting();
            var isAMSIntegrated = User.IsAMSIntegrated();

            //Replace labels in Title
            foreach (var item in widgets)
            {
                if (Regex.IsMatch(item.Title, @"\s*\[.*?\]\s*"))
                    item.Title = _dashboardManager.WidgetTitleLocalizer(item.Title).Result;
            }

            foreach (CPiWidget widget in widgets)
            {
                if (await Authorize(widget.Policy))
                {
                    try
                    {
                        var systemCategories = widget.SystemCategory.Split(',').ToList().Select(t => t.Trim());
                        if (!systemCategories.Any()) continue;

                        foreach (var cat in systemCategories)
                        {
                            var widgetSystem = "";
                            //patent
                            if (cat.Equals(SystemType.Patent)) 
                                widgetSystem = SystemType.Patent;
                            else if (cat.ToLower().Equals("rts") && patSettings.IsRTSOn) 
                                widgetSystem = SystemType.Patent;
                            else if (cat.ToLower().Equals("inventorawards") && patSettings.IsInventorAwardOn) 
                                widgetSystem = "inventorawards";
                            //trademark
                            else if (cat.Equals(SystemType.Trademark)) 
                                widgetSystem = SystemType.Trademark;
                            else if (cat.ToLower().Equals("trademarklink") && tmkSettings.IsTLOn) 
                                widgetSystem = SystemType.Trademark;
                            //generalmatter
                            else if (cat.Equals(SystemType.GeneralMatter)) 
                                widgetSystem = SystemType.GeneralMatter;
                            //ams
                            else if (cat.Equals(SystemType.AMS) || (cat.ToLower().Equals("amsintegrated") && isAMSIntegrated)) 
                                widgetSystem = SystemType.AMS;
                            else if (cat.ToLower().Equals("amsstandalone") && !User.IsSystemEnabled(SystemType.Patent)) 
                                widgetSystem = SystemType.AMS;
                            //dms
                            else if (cat.Equals(SystemType.DMS) || (cat.ToLower().Equals("dmsinitialdate") && defaultSettings.IsESignatureOn)) 
                                widgetSystem = SystemType.DMS;
                            //shared
                            else if (cat.Equals(SystemType.Shared)) 
                                widgetSystem = SystemType.Shared;
                            else if (cat.ToLower().Equals("shareddedocket") && (patSettings.IsDeDocketOn || tmkSettings.IsDeDocketOn || gmSettings.IsDeDocketOn)) 
                                widgetSystem = SystemType.Shared;
                            else if (cat.ToLower().Equals("shareddelegation") && (patSettings.IsDelegationOn || tmkSettings.IsDelegationOn || gmSettings.IsDelegationOn || defaultSettings.IsDelegationOn))
                                widgetSystem = SystemType.Shared;
                            //search request
                            else if (cat.Equals(SystemType.SearchRequest)) 
                                widgetSystem = SystemType.SearchRequest;
                            //patent clearance
                            else if (cat.Equals(SystemType.PatClearance)) 
                                widgetSystem = SystemType.PatClearance;
                            //product
                            else if (cat.ToLower().Equals("product") 
                                && ((patSettings.IsProductsOn && widget.Policy.Contains("CanAccessPatentProducts")) 
                                    || (tmkSettings.IsProductsOn && widget.Policy.Contains("CanAccessTrademarkProducts")) 
                                    || (gmSettings.IsProductsOn && widget.Policy.Contains("CanAccessGeneralMatterProducts")))) 
                                widgetSystem = "product";
                           
                            if (!string.IsNullOrEmpty(widgetSystem)) 
                                menuItems.Add(new AddWidgetMenuViewModel() { 
                                    System = widgetSystem, 
                                    Id = widget.Id, 
                                    Title = _localizer[widget.Title].ToString(), 
                                    Icon = string.IsNullOrEmpty(widget.Icon) ? DefaultWidgetIcon(widget.ViewName) : widget.Icon 
                                });

                            // If systemCategory starts with "shared", check and add to other systemCategories (Pat/Tmk/Gnm/Ams/Dms/Pac/Tmc) depending on the Policy
                            if (cat.StartsWith(SystemType.Shared))
                            {
                                var policies = widget.Policy.Split(',').ToList().Select(p => p.Trim());
                                foreach (var policy in policies)
                                {
                                    if (!string.IsNullOrEmpty(policy))
                                    {        
                                        var subSystem = string.Empty;
                                        if (policy.Contains(SystemType.Patent) && User.HasDashboardAccess(SystemType.Patent)) subSystem = SystemType.Patent;
                                        else if (policy.Contains(SystemType.Trademark) && User.HasDashboardAccess(SystemType.Trademark)) subSystem = SystemType.Trademark;
                                        else if (policy.Contains(SystemType.GeneralMatter) && User.HasDashboardAccess(SystemType.GeneralMatter)) subSystem = SystemType.GeneralMatter;
                                        else if (policy.Contains(SystemType.AMS) && User.HasDashboardAccess(SystemType.AMS)) subSystem = SystemType.AMS;
                                        else if (policy.Contains(SystemType.DMS) && User.HasDashboardAccess(SystemType.DMS)) subSystem = SystemType.DMS;
                                        else if (policy.Contains(SystemType.PatClearance) && User.HasDashboardAccess(SystemType.PatClearance)) subSystem = SystemType.PatClearance;
                                        else if (policy.Contains(SystemType.SearchRequest) && User.HasDashboardAccess(SystemType.SearchRequest)) subSystem = SystemType.SearchRequest;

                                        if (!string.IsNullOrEmpty(subSystem))
                                        {
                                            menuItems.Add(new AddWidgetMenuViewModel() 
                                            { 
                                                System = subSystem, 
                                                Id = widget.Id, 
                                                Title = _localizer[widget.Title].ToString(), 
                                                Icon = string.IsNullOrEmpty(widget.Icon) ? DefaultWidgetIcon(widget.ViewName) : widget.Icon 
                                            });
                                        }                                        
                                    }                                        
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.Message);
                    }
                }
            }
            return menuItems;
        }

        #region Setting Popup
        [HttpGet()]
        public async Task<IActionResult> DefaultWidgetSetting(CPiUserType userType)
        {
            return PartialView("_Settings", userType);
        }

        public async Task<IActionResult> GetWidgetSystem(string property, string text, FilterType filterType, CPiUserType userType, string requiredRelation = "", int widgetCategory = 0)
        {
            var availableWidgets = await GetAvailableWidgetList(userType);
            var availableSystems = availableWidgets.Select(d => d.System).Distinct().ToList();

            var widgetSystems = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Text = _localizer["All"].ToString(),
                    Value = "all"
                }
            };

            widgetSystems.AddRange(WidgetSystem.Systems.Where(d => availableSystems.Contains(d.Name))
                                            .OrderBy(o => o.SortOrder)
                                            .Select(d => new SelectListItem()
                                            {
                                                Text = _localizer[d.DisplayName].Value.ToString(),
                                                Value = d.Name
                                            })
                                            .OrderBy(o => o.Text)
                                            .ToList());

            return Json(widgetSystems);
        }

        public async Task<IActionResult> GetWidgetTitles(string property, string text, FilterType filterType, CPiUserType userType, string requiredRelation = "", string widgetSystem = "", int widgetCategory = 0)
        {
            var availableWidgets = await GetAvailableWidgetList(userType);

            var data = availableWidgets
                .Where(d => (widgetCategory != 7 && (d.System == widgetSystem || widgetSystem == "all") && d.System != "customwidget"))
                .OrderBy(o => o.Title)
                .Select(d => new { WidgetTitle = d.Title })
                .ToList();

            return Json(data);
        }

        public async Task<IActionResult> WidgetMenuRead([DataSourceRequest] DataSourceRequest request, string widgetSystem, int widgetCategory, List<int> selectedIds, string widgetTitle, CPiUserType userType)
        {
            var availableWidgets = await GetAvailableWidgetList(userType);

            var data = availableWidgets
                    .Where(d => ((widgetCategory != 7 && (d.System == widgetSystem || widgetSystem == "all") && d.System != "customwidget"))
                        && (!selectedIds.Any() || !selectedIds.Contains(d.Id))
                        && (string.IsNullOrEmpty(widgetTitle) || d.Title.ToLower().Contains(widgetTitle.ToLower()))
                    )
                    .DistinctBy(d => d.Title)
                    .OrderBy(o => o.Title)
                    .ToList();

            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost]
        public async Task<IActionResult> SaveDefaultWidgetSetting(CPiUserType userType, List<DashboardSettingViewModel> dashboardSettings, List<WidgetSettingViewModel>? widgetSettings)
        {
            if (dashboardSettings.Any())
            {
                var categoryList = WidgetCategory.Categories.Select(d => d.Id).ToList();
                var widgetCounter = 0;
                
                foreach (var category in dashboardSettings)
                {
                    if (categoryList.Any(d => d == category.CatId) && category.WidgetIds != null)
                    {
                        var widgets = await GetUserWidgets(userType, category.CatId ?? 0);
                        var widgetInCatCounter = widgets != null && widgets.Any() && widgets.Count > 0 ? widgets.Last().CPiUserWidget.SortOrder + 1 : 0;

                        foreach (var widgetId in category.WidgetIds)
                        {
                            if (widgets != null && widgets.Count > 0)
                            {
                                var widget = widgets.Find(w => w.CPiUserWidget.WidgetId == widgetId);
                                if (widget != null) continue;
                            }                            

                            var defaultWidget = new CPiUserTypeDefaultWidget()
                            {
                                UserType = userType,
                                WidgetCategory = category.CatId ?? 0,
                                WidgetId = widgetId,
                                SortOrder = widgetInCatCounter
                            };

                            await _defaultWidgetManager.Add(defaultWidget);
                            widgetInCatCounter++;
                            widgetCounter++;
                        }
                    }
                }

                if (widgetCounter > 1)
                    return Ok(new { success = _localizer["Widgets have been added successfully"] });
                else if (widgetCounter == 1)
                    return Ok(new { success = _localizer["Widget has been added successfully"] });
            }
            return Ok();
        }

        [HttpPost]
        public async Task<PartialViewResult> GetPreviewWidgetDetail(int widgetId, bool isRefresh = false, string widgetSettings = "{}", string? title = null)
        {
            var cpiWidget = await _dashboardManager.GetCPiWidget(widgetId);
            var widgetTitle = title ?? cpiWidget.Title;
            if (Regex.IsMatch(widgetTitle, @"\s*\[.*?\]\s*"))
            {
                widgetTitle = _dashboardManager.WidgetTitleLocalizer(widgetTitle).Result;
            }

            UserWidgetViewModel widget = new UserWidgetViewModel()
            {
                CPiUserWidget = new CPiUserWidget()
                {
                    Id = 0,
                    UserId = User.GetUserIdentifier(),
                    WidgetId = widgetId,
                    SortOrder = 0,
                    Settings = widgetSettings,
                    WidgetCategory = 0,
                    UserTitle = widgetTitle,
                    CPiWidget = cpiWidget
                },
                WidgetTitle = widgetTitle
            };

            WidgetViewModel widgetViewModel;

            widgetViewModel = new WidgetViewModel()
            {
                Id = widget.CPiUserWidget.CPiWidget.Id,
                WidgetId = widget.CPiUserWidget.CPiWidget.Id,
                Name = $"widget-{widget.CPiUserWidget.CPiWidget.Id}",
                Title = _localizer[widget.WidgetTitle],                
                ViewName = $"/Views/Dashboard/Widgets/{widget.CPiUserWidget.CPiWidget.ViewName}",
                SeriesColors = string.IsNullOrEmpty(widget.CPiUserWidget.CPiWidget.SeriesColors) ? null : widget.CPiUserWidget.CPiWidget.SeriesColors.Split('|'),
                Icon = string.IsNullOrEmpty(widget.CPiUserWidget.CPiWidget.Icon) ? DefaultWidgetIcon(widget.CPiUserWidget.CPiWidget.ViewName) : widget.CPiUserWidget.CPiWidget.Icon,
                CanExpand = widget.CPiUserWidget.CPiWidget.CanExpand,                
                UserSettings = new JObject(),
                WidgetSettings = new JObject(),
                Template = widget.CPiUserWidget.CPiWidget.Template ?? "",
                LabelTemplate = widget.CPiUserWidget.CPiWidget.LabelTemplate ?? "",
                TooltipTemplate = widget.CPiUserWidget.CPiWidget.TooltipTemplate ?? "",
                Policy = widget.CPiUserWidget.CPiWidget.Policy,
                CreatorId = widget.CPiUserWidget.CPiWidget.CreatorId,
                CanDrillDown = false,
                CanExportPpt = false,                
                CanEmail = false,
                CanCopy = false,
                CanEditTitle = false,
                CanExportPDF = false,
                CanExportExcel = false,
                RowSpan = widget.CPiUserWidget.CPiWidget.RowSpan
            };

            try
            {
                widget.HasRespOffice = User.HasRespOfficeFilter();
                widget.EntityFilterType = User.GetEntityFilterType();

                widget.IsAdmin = User.IsAdmin();
                widget.UserRoles = User.GetRoles();
                widget.MyUserType = User.GetUserType();

                widget.isRefresh = isRefresh;

                widgetViewModel.Data = await _dashboardManager.GetData(widget);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            if (!string.IsNullOrEmpty(widget.CPiUserWidget.CPiWidget.Settings))
            {
                try
                {
                    widgetViewModel.WidgetSettings = JObject.Parse(widget.CPiUserWidget.CPiWidget.Settings);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
            }

            if (!string.IsNullOrEmpty(widget.CPiUserWidget.Settings))
            {
                try
                {
                    JObject settings = JObject.Parse(widget.CPiUserWidget.Settings);
                    widgetViewModel.UserSettings = settings;
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
            }

            return PartialView("_WidgetPreview", widgetViewModel);
        }
        #endregion
    }
}