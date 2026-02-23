using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace R10.Web.Views.Manage
{
    public static class ManageNavPages
    {
        public static string Profile => "Profile";

        public static string ChangePassword => "ChangePassword";

        public static string ExternalLogins => "ExternalLogins";

        public static string TwoFactorAuthentication => "TwoFactorAuthentication";

        public static string Settings => "Settings";

        public static string Notifications => "Notifications";

        public static string TimeTrackers => "TimeTrackers";

        public static string Alerts => "Alerts";
    }
}
