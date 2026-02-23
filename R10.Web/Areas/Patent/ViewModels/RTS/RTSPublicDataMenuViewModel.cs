using R10.Core.Entities.Patent;
using System.Collections.Generic;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class RTSPublicDataMenuViewModel
    {
        public int AppId { get; set; }
        public int PLAppId { get; set; }
        public string? ActiveTab { get; set; }
        public List<RTSInfoSettingsMenu> InfoMenu { get; set; }

    }
}
