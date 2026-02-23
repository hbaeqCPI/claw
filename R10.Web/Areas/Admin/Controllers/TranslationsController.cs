using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Kendo.Mvc.Extensions;
using R10.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using R10.Web.Security;
using Microsoft.AspNetCore.Mvc.Rendering;
using Kendo.Mvc.UI;
using System;
using R10.Core.Entities;
using R10.Core.Helpers;
using Microsoft.Extensions.Localization;
using R10.Web.Models;
using Localization.SqlLocalizer.DbStringLocalizer;
using R10.Web.Areas.Admin.Views;
using R10.Web.Models.PageViewModels;
using R10.Core.Helpers;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
    public class TranslationsController : Microsoft.AspNetCore.Mvc.Controller
    {
        protected readonly ILocalizationRecordsManager _localizationManager;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IStringExtendedLocalizerFactory _stringLocalizerFactory;

        public TranslationsController(ILocalizationRecordsManager localizationManager, 
                                      IStringLocalizer<SharedResource> localizer,
                                      IStringExtendedLocalizerFactory stringLocalizerFactor)
        {
            _localizationManager = localizationManager;
            _localizer = localizer;
            _stringLocalizerFactory = stringLocalizerFactor;
        }
        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_SidebarNav";

        public IActionResult Index()
        {
            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Translations"].ToString(),
                PageId = "translationsPage",
                MainPartialView = "_TranslationList",
                //MainViewModel = null,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.Translations
            };

            return View("Index", sidebarModel);

            //return View();
        }

        public async Task<IActionResult> GetSystems()
        {
            var systems = await _localizationManager.GetSystems();
            systems.Add("");

            var list = new List<SelectListItem>();

            foreach (var item in systems)
            {
                list.Add(new SelectListItem
                {
                    Value = item.ToString(),
                    Text = item.ToString()
                });
            }

            return Json(list.OrderBy(l => l.Text).ToList());
        }

        public async Task<IActionResult> GetMenuItems(string system)
        {
            var menu = await _localizationManager.GetMenuItems(system);
            menu.Add("");

            var list = new List<SelectListItem>();

            foreach (var item in menu)
            {
                list.Add(new SelectListItem
                {
                    Value = item.ToString(),
                    Text = item.ToString()
                });
            }
            return Json(list.OrderBy(l => l.Text).ToList());
        }

        [HttpPost]
        public async Task<JsonResult> TranslationsGrid_Read([DataSourceRequest] DataSourceRequest request, string system, string menu, string searchTxt, bool emptyOnly, string locale)
        {
            try
            {
                var translations = new List<LocalizationRecords>();
                if (!string.IsNullOrEmpty(locale))
                    translations = await GetTranslations(system, menu, searchTxt, locale, emptyOnly);

                return Json(await translations.ToDataSourceResultAsync(request));
            }
            catch (Exception e)
            {
                var error = e.Message;
            }
            return null;
        }
        

        [HttpPost]
        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> TranslationsGrid_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")] IEnumerable<LocalizationRecords> updates)
        {
            if (updates != null && ModelState.IsValid)
            {
                var updated = updates.Where(t => t.Id > 0).ToList();
                if (updated.Any())
                {
                    updated.Each(t => {
                        t.UpdatedTimestamp = DateTime.Now;
                        t.UpdatedBy = User.GetUserName();
                    });
                    _localizationManager.Update(updated);
                }

                var newTranslations = updates.Where(t => t.Id < 0).ToList();
                newTranslations.Each(t => {
                    t.Id = 0;
                    t.UpdatedTimestamp = DateTime.Now;
                    t.CreatedTimestamp = DateTime.Now;
                    t.CreatedBy = User.GetUserName();
                    t.UpdatedBy = t.CreatedBy;
                });

                if (newTranslations.Any())
                    _localizationManager.Add(newTranslations);

                await _localizationManager.Save();
                return Ok(new { success = _localizer["Translations have been saved successfully."].ToString() });

            }
            return Ok();
        }

        
        public Microsoft.AspNetCore.Mvc.ActionResult ResetCache()
        {
            _stringLocalizerFactory.ResetCache();
            return Ok("Localization cache has been refresh successfully.");
        }

        private async Task<List<LocalizationRecords>> GetTranslations(string system, string menu, string searchTxt, string locale, bool emptyOnly) {
            var locales = new string[] { locale };
            var translations = await _localizationManager.LocalizationRecords.Where(t => (locales.Contains(t.LocalizationCulture))
                                                                                        && (string.IsNullOrEmpty(searchTxt) || t.Key.Contains(searchTxt))
                                                                                        && (string.IsNullOrEmpty(system) || t.Group.System == system)
                                                                                        && (string.IsNullOrEmpty(menu) || t.Group.Menu == menu)
                                                                                    ).ToListAsync();

            var baseLanguage = await _localizationManager.LocalizationRecords.Where(t => t.LocalizationCulture=="en"
                                                                            && (string.IsNullOrEmpty(searchTxt) || t.Key.Contains(searchTxt))
                                                                            && (string.IsNullOrEmpty(system) || t.Group.System == system)
                                                                            && (string.IsNullOrEmpty(menu) || t.Group.Menu == menu)
                                                                        ).ToListAsync();

            var noTranslations = baseLanguage.Where(b => !translations.Any(t => t.Key == b.Key && t.ResourceKey == b.ResourceKey));
            noTranslations.Each(n=> {
                n.Id = n.Id * -1;
                n.LocalizationCulture = locale;
                n.Text = "";
            });

            if (emptyOnly)
            {
                return noTranslations.OrderBy(t => t.Key).ThenBy(t => t.ResourceKey).ToList();
            }
            else {
                var all = noTranslations.Concat(translations).OrderBy(t => t.Key).ThenBy(t => t.ResourceKey).ToList(); //.ThenBy(t => t.LocalizationCulture == "en" ? 1 : 2).ToList();
                return all;
            }
        }





    }
}