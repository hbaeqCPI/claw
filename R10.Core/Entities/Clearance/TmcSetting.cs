using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Entities.Clearance
{
    public class TmcSetting : DefaultSetting
    {
        public string? DefaultClearanceCountry { get; set; }
        public string? DefaultLanguageTranslation { get; set; }
    }
}
