using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Api.Models;
using R10.Web.Security;
using System.Text.Json;

namespace R10.Web.Api.Admin
{
    [Route("api/admin/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = CPiAuthorizationPolicy.CPiAdmin)]
    [ApiController]
    public class SystemStatusController : ControllerBase
    {
        private readonly ICPiSystemSettingManager _settingManager;
        private readonly UserManager<CPiUser> _userManager;

        public SystemStatusController(ICPiSystemSettingManager settingManager, UserManager<CPiUser> userManager)
        {
            _settingManager = settingManager;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> SetStatus([FromBody] SystemStatusParam? status)
        {
            if (status == null || status.SystemStatus == null)
                return BadRequest("Invalid parameters.");

            var cpiSetting = await _settingManager.GetCPiSetting("SystemStatus");
            if (cpiSetting == null)
                return NotFound("System Status setting not found.");

            var systemSetting = await _settingManager.QueryableList.Where(s => s.SystemId == "" && s.SettingId == cpiSetting.Id).FirstOrDefaultAsync();
            if (systemSetting == null)
            {
                systemSetting = new CPiSystemSetting()
                {
                    Id = 0,
                    SystemId = "",
                    SettingId = cpiSetting.Id
                };
            }

            systemSetting.Settings = JsonSerializer.Serialize(status.SystemStatus);

            if (systemSetting.Id > 0)
                await _settingManager.Update(systemSetting);
            else
                await _settingManager.Add(systemSetting);

            // Log out users after ValidationTimeSpan expires
            if (status.UpdateSecurityStamp)
            {
                foreach (var user in await _userManager.Users.ToListAsync())
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                }
            }

            return Ok();
        }
    }
}
