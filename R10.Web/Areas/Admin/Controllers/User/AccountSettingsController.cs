using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Models;
using R10.Web.Security;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
    public class AccountSettingsController : BaseController
    {
        private readonly UserManager<CPiUser> _userManager;
        private readonly ICPiUserPermissionManager _permissionManager;
        private readonly ICPiUserSettingManager _settingManager;
        private readonly IStringLocalizer<AdminResource> _localizer;

        public AccountSettingsController(UserManager<CPiUser> userManager, ICPiUserPermissionManager permissionManager, ICPiUserSettingManager settingManager, IStringLocalizer<AdminResource> localizer)
        {
            _userManager = userManager;
            _permissionManager = permissionManager;
            _settingManager = settingManager;
            _localizer = localizer;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSetting(string userId, string settingName, string setting)
        {

            JObject settings = JObject.Parse(setting);
            await _settingManager.SaveUserSetting(userId, settingName, settings);
            await UpdateUserLastUpdate(userId);

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDecisionMakerRole(string userId, bool isDecisionMaker, string systemId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetDecisionMakerRole(user, isDecisionMaker, systemId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttorneyRole(string userId, bool canAccess, string systemId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetAttorneyRole(user, canAccess, systemId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSoftDocketRole(string userId, bool canModify, string systemId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetSoftDocketRole(user, canModify, systemId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRequestDocketRole(string userId, bool canModify, string systemId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetRequestDocketRole(user, canModify, systemId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveUploadRole(string userId, bool canUpload, string systemId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetUploadRole(user, canUpload, systemId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveInventorRole(string userId, bool canAccess, string systemId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetInventorRole(user, canAccess, systemId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkEntity(string userId, int entityId, int entityType)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && user.EntityFilterType == (CPiEntityType)entityType)
                await _permissionManager.LinkEntity(user, entityId, (CPiEntityType)entityType);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAuxiliaryRole(string userId, string systemId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetAuxiliaryRole(user, systemId, roleId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCountryLawRole(string userId, string systemId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetCountryLawRole(user, systemId, roleId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveActionTypeRole(string userId, string systemId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetActionTypeRole(user, systemId, roleId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLetterRole(string userId, string systemId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetLetterRole(user, systemId, roleId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCustomQueryRole(string userId, string systemId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetCustomQueryRole(user, systemId, roleId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProductsRole(string userId, string systemId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetProductsRole(user, systemId, roleId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCostEstimatorRole(string userId, string systemId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetCostEstimatorRole(user, systemId, roleId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGermanRemunerationRole(string userId, string systemId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetGermanRemunerationRole(user, systemId, roleId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFrenchRemunerationRole(string userId, string systemId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetFrenchRemunerationRole(user, systemId, roleId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDMSReviewerRole(string userId, bool isReviewer)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetReviewerRole(user, isReviewer);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDMSPreviewerRole(string userId, bool isPreviewer)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetPreviewerRole(user, isPreviewer);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveClearanceReviewerRole(string userId, bool isReviewer)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetClearanceReviewerRole(user, isReviewer);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePatClearanceReviewerRole(string userId, bool isReviewer)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetPatClearanceReviewerRole(user, isReviewer);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePatentScoreRole(string userId, bool canModify, string systemId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetPatentScoreRole(user, canModify, systemId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveMailboxAccess(string userId, bool canAccess, string mailbox)
        {
            var settings = await _settingManager.GetUserSetting<UserAccountSettings>(userId);

            if (canAccess && !settings.Mailboxes.Any(s => s.ToLower() == mailbox.ToLower()))
                settings.Mailboxes.Add(mailbox);
            else if (!canAccess)
                settings.Mailboxes.Remove(mailbox);

            await _settingManager.SaveUserSetting(userId, "UserAccountSettings", JObject.FromObject(settings));
            await UpdateUserLastUpdate(userId);

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDocumentVerificationRole(string userId, string systemId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetDocumentVerificationRole(user, systemId, roleId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveWorkflowRole(string userId, string systemId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                await _permissionManager.SetWorkflowRole(user, systemId, roleId);
            else
                return BadRequest(_localizer["Invalid request."].ToString());

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDashboardAccess(string userId, bool canAccess, string systemId)
        {
            var settings = await _settingManager.GetUserSetting<UserAccountSettings>(userId);

            if (canAccess && !settings.DashboardAccess.Any(s => s.ToLower() == systemId.ToLower()))
                settings.DashboardAccess.Add(systemId);
            else if (!canAccess)
                settings.DashboardAccess.Remove(systemId);

            await _settingManager.SaveUserSetting(userId, "UserAccountSettings", JObject.FromObject(settings));
            await UpdateUserLastUpdate(userId);

            return Ok(new { message = _localizer["Setting successfully updated."].ToString() });
        }

        private async Task UpdateUserLastUpdate(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.UpdateAsync(user);
            }
        }
    }
}