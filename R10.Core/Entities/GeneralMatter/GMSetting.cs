using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMSetting : DefaultSetting
    {
        public bool IsMultipleOwnerOn { get; set; }
        public bool IsClientMatterOn { get; set; }
        public string? ClientMatterDivider { get; set; }
        public string? GmCustomFieldsTabLabel { get; set; }
    }
}
