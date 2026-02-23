using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using R10.Web.Security;
using R10.Core.Interfaces;
using R10.Core.Entities;
using Microsoft.Extensions.Localization;
using R10.Web.Models;
using Kendo.Mvc.UI;
using R10.Core.Helpers;
using R10.Web.Extensions.ActionResults;
using Kendo.Mvc.Extensions;
using R10.Web.Extensions;
using R10.Web.Areas.Admin.Views;
using R10.Web.Models.PageViewModels;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = CPiAuthorizationPolicy.CPiAdmin)]
    public class HelpPageController : BaseController
    {
        private readonly IBaseService<Help> _helpService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public HelpPageController(IBaseService<Help> helpService, IStringLocalizer<SharedResource> localizer)
        {
            _helpService = helpService;
            _localizer = localizer;
        }
        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_SidebarNav";

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = _localizer["Help Pages"].ToString(),
                PageId = "helpPagesList",
                MainPartialView = "List",
                //MainViewModel = null,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.HelpPages
            };

            return View("Index", sidebarModel);
        }

        [HttpPost]
        public async Task<JsonResult> PageRead([DataSourceRequest] DataSourceRequest request)
        {
            return Json(await _helpService.QueryableList.Where(h => h.ClientType == User.GetClientType()).ToDataSourceResultAsync(request));
        }

        public async Task<IActionResult> Update(
           [Bind(Prefix = "updated")] IList<Help> updated,
           [Bind(Prefix = "new")] IList<Help> added,
           [Bind(Prefix = "deleted")] IList<Help> deleted)
        {
            if (updated.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                if (updated.Any())
                    await _helpService.Update(updated);

                var success = updated.Count() == 1 ?
                    _localizer["Help page has been saved successfully."].ToString() :
                    _localizer["Help pages have been saved successfully"].ToString();

                return Ok(new { success = success });
            }
            return Ok();
        }
    }
}