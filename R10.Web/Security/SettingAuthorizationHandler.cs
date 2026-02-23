using Microsoft.AspNetCore.Authorization;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Security
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
