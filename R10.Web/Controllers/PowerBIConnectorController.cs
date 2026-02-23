using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using R10.Core.Entities.Shared;
using R10.Core.Interfaces;
using R10.Web.Models;
using R10.Web.Security;

namespace R10.Web.Controllers
{
    [Authorize(Policy = SharedAuthorizationPolicy.CanAccessPowerBIConnector)]
    public class PowerBIConnectorController : Controller
    {
        private readonly ISystemSettings<DefaultSetting> _settings;

        public PowerBIConnectorController(ISystemSettings<DefaultSetting> settings)
        {
            _settings = settings;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Download()
        {
            var settings = await _settings.GetSetting();
            var path = "Resources\\CPIConnector.mez";

            if (System.IO.File.Exists(path))
                return File(System.IO.File.OpenRead(path), "application/octet-stream", Path.GetFileName(path));

            return NotFound();
        }
    }
}
