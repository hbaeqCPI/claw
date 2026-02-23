using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Admin.ViewModels
{
    public class SystemSettingsViewModel
    {
        public string ActivePage { get; set; }
        public string ActivePartialViewName { get; set; }

        public SystemStatus SystemStatus { get; set; }
        public SystemNotification CookieConsent { get; set; }
        public ActionIndicator ActionIndicator { get; set; }
        public DeDocketFields DeDocketFields { get; set; }
        public InventionDisclosureStatus InventionDisclosureStatus { get; set; }
    }
}
