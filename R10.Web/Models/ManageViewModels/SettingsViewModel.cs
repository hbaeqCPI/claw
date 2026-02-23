using R10.Core.DTOs;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.ManageViewModels
{
    public class SettingsViewModel
    {
        public DefaultPage DefaultPage { get; set; }
        public UserPreferences UserPreferences { get; set; }
        public UserNotificationSettings UserNotificationSettings { get; set; }
        public Theme Theme { get; set; }
    }
}
