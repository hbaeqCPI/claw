using LawPortal.Core.DTOs;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Models.ManageViewModels
{
    public class SettingsViewModel
    {
        public DefaultPage DefaultPage { get; set; }
        public UserPreferences UserPreferences { get; set; }
        public UserNotificationSettings UserNotificationSettings { get; set; }
        public Theme Theme { get; set; }
    }
}
