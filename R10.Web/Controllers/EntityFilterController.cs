using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Extensions;

namespace R10.Web.Controllers
{
    [Authorize]
    public class EntityFilterController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ICPiUserPermissionManager _userPermissionManager;

        public EntityFilterController(ICPiUserPermissionManager userPermissionManager)
        {
            _userPermissionManager = userPermissionManager;
        }

        public IActionResult Index()
        {
            if (!Request.IsAjax())
                return new BadRequestResult();

            return PartialView("_MultiSelect");
        }

        [HttpPost]
        public async Task<JsonResult> GetUserEntityFilterList()
        {
            var entities = await _userPermissionManager.UserEntityFilter(User.GetEntityFilterType(), User.GetUserIdentifier()).OrderBy(o => o.Code).ToListAsync();
            return Json(entities);
        }
    }
}