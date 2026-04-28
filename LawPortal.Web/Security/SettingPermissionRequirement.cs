using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Security
{
    public class SettingPermissionRequirement : IAuthorizationRequirement
    {
        public string OptionKey { get; set; }
        public string OptionSubKey { get; set; }
        public bool IsTrue { get; set; }

        public SettingPermissionRequirement(string optionKey, string optionSubKey, bool isTrue = true)
        {
            OptionKey = optionKey;
            OptionSubKey = optionSubKey;
            IsTrue = isTrue;
        }
    }
}
