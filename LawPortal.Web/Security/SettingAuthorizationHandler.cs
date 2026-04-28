using Microsoft.AspNetCore.Authorization;
using LawPortal.Core.Entities.Shared;
using LawPortal.Core.Helpers;
using LawPortal.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Security
{
    public class SettingAuthorizationHandler : AuthorizationHandler<SettingPermissionRequirement>
    {
        protected readonly ISystemSettings<DefaultSetting> _settings;

        public SettingAuthorizationHandler(ISystemSettings<DefaultSetting> settings)
        {
            _settings = settings;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SettingPermissionRequirement requirement)
        {
            var isSettingOn = await _settings.GetValue<bool>(requirement.OptionKey, requirement.OptionSubKey);

            if (requirement.IsTrue ? isSettingOn : !isSettingOn)
                context.Succeed(requirement);

            return;
        }
    }
}
