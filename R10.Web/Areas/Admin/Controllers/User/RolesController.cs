using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Areas.Admin.ViewModels;
using R10.Web.Security;
using R10.Web.Services;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
    public class RolesController : BaseController
    {
        private readonly CPiUserManager _userManager;
        private readonly ICPiUserPermissionManager _permissionManager;

        public RolesController(
            CPiUserManager userManager, 
            ICPiUserPermissionManager permissionManager)
        {
            _userManager = userManager;
            _permissionManager = permissionManager;
        }

        [HttpPost]
        public async Task<JsonResult> UserSystemRoleGrid_Read([DataSourceRequest] DataSourceRequest request, string userId)
        {
            //get user system/role/respoffice
            if (string.IsNullOrEmpty(userId))
                return Json(new object[] { new object() });

            var userSystemRoles = await _userManager.GetUserSystemRoles(userId).Select(r =>
                            new UserSystemRoleViewModel()
                            {
                                Id = r.Id,
                                UserId = r.UserId,
                                System = new PickListViewModel(r.SystemId, r.CPiSystem.Name),
                                Role = new PickListViewModel(r.RoleId, r.CPiRole.Name),
                                RespOffice = r.RespOffice ?? ""
                            }).ToListAsync();

            if (request.Sorts.Any())
            {
                switch (request.Sorts[0].Member)
                {
                    case "System":
                        request.Sorts[0].Member = "System.Name";
                        break;

                    case "Role":
                        request.Sorts[0].Member = "Role.Name";
                        break;
                }
            }
            return Json(await userSystemRoles.ToDataSourceResultAsync(request));
        }

        [HttpPost]
        public async Task<JsonResult> UserSystemRoleGrid_Create([DataSourceRequest] DataSourceRequest request, UserSystemRoleViewModel userSystemRole, string userId)
        {
            //add user system/role/respoffice
            if (userSystemRole != null && ModelState.IsValid)
            {
                CPiUserSystemRole entity = new CPiUserSystemRole
                {
                    Id = userSystemRole.Id,
                    UserId = userId,
                    SystemId = userSystemRole.System.Id,
                    RoleId = userSystemRole.Role.Id,
                    RespOffice = userSystemRole.RespOffice
                };

                var result = await _permissionManager.AddRole(entity);

                if (result.Succeeded)
                {
                    userSystemRole.Id = entity.Id;                                  //get new id
                    userSystemRole.RespOffice = userSystemRole.RespOffice ?? "";    //issue: update fires max length validation if null is not set to empty string
                }
                else
                {
                    AddModelErrors(result);
                }
            }
            return Json(new[] { userSystemRole }.ToDataSourceResult(request, ModelState));
        }

        [HttpPost]
        public async Task<JsonResult> UserSystemRoleGrid_Update([DataSourceRequest] DataSourceRequest request, UserSystemRoleViewModel userSystemRole)
        {
            //update user system/role/respoffice
            if (userSystemRole != null && ModelState.IsValid)
            {
                CPiUserSystemRole entity = new CPiUserSystemRole
                {
                    Id = userSystemRole.Id,
                    UserId = userSystemRole.UserId,
                    SystemId = userSystemRole.System.Id,
                    RoleId = userSystemRole.Role.Id,
                    RespOffice = userSystemRole.RespOffice
                };

                var result = await _permissionManager.UpdateRole(entity);

                if (result.Succeeded)
                {
                    //reset role-based settings
                    var user = await _userManager.FindByIdAsync(userSystemRole.UserId);
                    await _permissionManager.ResetSettings(user);
                }
                else
                {
                    AddModelErrors(result);
                }

                userSystemRole.RespOffice = userSystemRole.RespOffice ?? ""; //issue: update fires max length validation if null is not set to empty string
            }

            return Json(new[] { userSystemRole }.ToDataSourceResult(request, ModelState));
        }

        [HttpPost]
        public async Task<JsonResult> UserSystemRoleGrid_Delete([DataSourceRequest] DataSourceRequest request, UserSystemRoleViewModel userSystemRole)
        {
            //delete user system/role/respoffice
            if (userSystemRole != null && ModelState.IsValid)
            {
                CPiUserSystemRole entity = new CPiUserSystemRole
                {
                    Id = userSystemRole.Id,
                    UserId = userSystemRole.UserId,
                    SystemId = userSystemRole.System.Id,
                    RoleId = userSystemRole.Role.Id,
                    RespOffice = userSystemRole.RespOffice
                };

                var result = await _permissionManager.RemoveRole(entity);

                if (result.Succeeded)
                {
                    //reset role-based settings
                    var user = await _userManager.FindByIdAsync(userSystemRole.UserId);
                    await _permissionManager.ResetSettings(user);
                }
                else
                {
                    AddModelErrors(result);
                }
            }

            return Json(new[] { userSystemRole }.ToDataSourceResult(request, ModelState));
        }
        private void AddModelErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
        }

        [HttpPost]
        public async Task<JsonResult> UserSystems([DataSourceRequest] DataSourceRequest request, string userId, string systemId)
        {
            //user systems dropdown list
            var systems = await _permissionManager.AvailableSystems(userId, systemId);
            return Json(await systems.ToDataSourceResultAsync(request));
        }

        [HttpPost]
        public async Task<JsonResult> UserRoles([DataSourceRequest] DataSourceRequest request, string systemId)
        {
            //user roles dropdown list
            var roles = await _permissionManager.AvailableRoles(systemId);
            return Json(await roles.ToDataSourceResultAsync(request));
        }

        [HttpPost]
        public async Task<JsonResult> UserRespOffices([DataSourceRequest] DataSourceRequest request, string systemId)
        {
            //user respoffice dropdown list
            var respOffices = await _permissionManager.AvailableRespOffices(systemId);
            return Json(await respOffices.ToDataSourceResultAsync(request));
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var result = await GetPicklistData(_permissionManager.CPiUserSystemRoles, request, property, text, filterType, requiredRelation);
            return result;
        }
    }
}