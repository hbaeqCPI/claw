using R10.Core.Entities.Shared;
using System.Collections.Generic;

namespace R10.Core.Entities.Patent
{
    public class RTSInfoSettingsMenu : PublicDataInfoSettingsMenu
    {
        public List<RTSInfoSettingsMenuCountry>? CountryInfoSettings { get; set; }
    }

    public class RTSInfoSettingsMenuCountry : PublicDataInfoSettingsMenuCountry
    {
        public RTSInfoSettingsMenu? InfoSettingsMenu { get; set; }
    }
    
}
