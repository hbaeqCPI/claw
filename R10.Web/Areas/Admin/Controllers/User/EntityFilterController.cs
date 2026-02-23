using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Extensions.ActionResults;
using R10.Web.Models;
using R10.Web.Security;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
    public class EntityFilterController : BaseController
    {
        private readonly UserManager<CPiUser> _userManager;
        private readonly ICPiUserPermissionManager _permissionManager;
        private readonly IStringLocalizer<AdminResource> _localizer;
        private readonly ILogger _logger;

        public EntityFilterController(
            UserManager<CPiUser> userManager, 
            ICPiUserPermissionManager permissionManager, 
            IStringLocalizer<AdminResource> localizer,
            ILogger<EntityFilterController> logger)
        {
            _userManager = userManager;
            _permissionManager = permissionManager;
            _localizer = localizer;
            _logger = logger;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEntityFilter_Update(string userId, CPiEntityType entityFilterType)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null && user.EntityFilterType != entityFilterType)
            {
                user.EntityFilterType = entityFilterType;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                    await _permissionManager.RemoveEntityFilters(user.Id);
                else
                    return new JsonBadRequest(new { errors = LogErrors(result) });

                return Ok(new { message = _localizer["Entity type successfully updated."].ToString() });
            }
            else
                return BadRequest(_localizer["Invalid request."].ToString());

        }

        [HttpPost]
        public async Task<JsonResult> EntityList_Read([DataSourceRequest] DataSourceRequest request, CPiEntityType entityFilterType, string entity, string userId)
        {
            //get available entity filter list based on entity type
            var entities = _permissionManager.AvailableEntityFilter(entityFilterType, entity, userId);
            return Json(await entities.ToDataSourceResultAsync(request));

        }

        [HttpPost]
        public async Task<JsonResult> UserEntityList_Read([DataSourceRequest] DataSourceRequest request, CPiEntityType entityFilterType, string userId)
        {
            //get user entity filter list
            var entities = _permissionManager.UserEntityFilter(entityFilterType, userId);
            return Json(await entities.ToDataSourceResultAsync(request));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEntityList_Add(string userId, List<int> selectedItems)
        {
            //add entity filters
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                var result = await _permissionManager.AddEntityFilters(user.Id, selectedItems);
                if (result.Succeeded)
                    return Ok(new { message = _localizer["{0} entity filter(s) successfully added.", selectedItems.Count].ToString() });
                else
                    return new JsonBadRequest(new { errors = LogErrors(result) });

            }
            return BadRequest(_localizer["Invalid request."].ToString());

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEntityList_Remove(string userId, List<int> selectedItems)
        {
            //remove entity filters
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                var result = await _permissionManager.RemoveEntityFilters(user.Id, selectedItems);
                if (result.Succeeded)
                    return Ok(new { message = _localizer["{0} entity filter(s) successfully removed.", selectedItems.Count].ToString() });
                else
                    return new JsonBadRequest(new { errors = LogErrors(result) });
            }
            return BadRequest(_localizer["Invalid request."].ToString());

        }

        private List<string> LogErrors(IdentityResult result)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();

            foreach (var error in errors)
            {
                _logger.LogError(error);
            }
            return errors;
        }
    }
}