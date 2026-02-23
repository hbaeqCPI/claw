using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using R10.Core.Interfaces;
using R10.Web.Areas.Admin.Services;
using R10.Web.Areas.Admin.Views;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
    public class CatalogController : Controller
    {
        private readonly ICatalogService _catalogService;
        private readonly IStringLocalizer<AdminResource> _localizer;

        public CatalogController(ICatalogService catalogService, IStringLocalizer<AdminResource> localizer)
        {
            _catalogService = catalogService;
            _localizer = localizer;
        }

        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_SidebarNav";

        public async Task<IActionResult> Index()
        {
            var model = await _catalogService.GetCatalog();
            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Catalog of Features"].ToString(),
                PageId = "catalogPage",
                MainPartialView = "_Catalog",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.Catalog
            };

            return View(sidebarModel);
        }
    }
}
