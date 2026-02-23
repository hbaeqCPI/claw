using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Areas.Admin.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Security;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
    public class GroupsController : BaseController
    {
        private readonly ICPiUserGroupManager _groupManager;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<AdminResource> _localizer;

        public GroupsController(ICPiUserGroupManager groupManager, IMapper mapper, IStringLocalizer<AdminResource> localizer)
        {
            _groupManager = groupManager;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<IActionResult> UserGroupsRead([DataSourceRequest] DataSourceRequest request, string id)
        {
            var result = await _groupManager.CPiUserGroups
                                            .Where(g => g.UserId == id)
                                            .OrderBy(g => g.CPiGroup.Name)
                                            .ProjectTo<UserGroupsViewModel>().ToListAsync();

            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> UserGroupsUpdate(string id,
            [Bind(Prefix = "updated")] IEnumerable<UserGroupsViewModel> updated,
            [Bind(Prefix = "new")] IEnumerable<UserGroupsViewModel> added,
            [Bind(Prefix = "deleted")] IEnumerable<UserGroupsViewModel> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _groupManager.UpdateUserGroup(id, User.GetUserName(),
                    _mapper.Map<List<CPiUserGroup>>(updated),
                    _mapper.Map<List<CPiUserGroup>>(added),
                    _mapper.Map<List<CPiUserGroup>>(deleted));
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                _localizer["Group has been saved successfully."].ToString() :
                _localizer["Groups have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        public async Task<IActionResult> UserGroupsDelete([Bind(Prefix = "deleted")] UserGroupsViewModel deleted)
        {
            if (deleted.Id > 0)
            {
                await _groupManager.UpdateUserGroup(deleted.UserId, User.GetUserName(), new List<CPiUserGroup>(), new List<CPiUserGroup>(), new List<CPiUserGroup>() { _mapper.Map<CPiUserGroup>(deleted) });
                return Ok(new { success = _localizer["Group has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        public async Task<JsonResult> UserGroups([DataSourceRequest] DataSourceRequest request, int groupId, string userId)
        {
            var users = await _groupManager.GetGroups().Select(g => new { Id = g.Id, Name = g.Name })
                                                      .OrderBy(g => g.Name)
                                                      .ToListAsync();
            return Json(await users.ToDataSourceResultAsync(request));
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var result = await GetPicklistData(_groupManager.QueryableList.Where(g => g.CPiUserGroups.Any()), request, property, text, filterType, requiredRelation);
            return result;
        }
    }
}