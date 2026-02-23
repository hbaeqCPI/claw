using R10.Core.Entities.Shared;
using System.Collections.Generic;

namespace R10.Core.Entities.Trademark
{
    public class TLInfoSettingsMenu : PublicDataInfoSettingsMenu
    {
        public List<TLInfoSettingsMenuCountry>? CountryInfoSettings { get; set; }
    }

    public class TLInfoSettingsMenuCountry : PublicDataInfoSettingsMenuCountry
    {
        public TLInfoSettingsMenu? InfoSettingsMenu { get; set; }
    }
    
}
