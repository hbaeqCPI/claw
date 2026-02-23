using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core
{
    public class EntityFilterParam
    {

        public string UserIdentifier { get; set; }
        public bool HasRespOfficeFilter { get; set; }
        public bool HasEntityFilter { get; set; }
        public CPiEntityType EntityFilterType { get; set; }
        public bool ApplyAgentFilter { get; set; }


        public EntityFilterParam(string userIdentifier, bool hasRespOfficeFilter, bool hasEntityFilter, CPiEntityType entityFilterType, bool applyAgentFilter=false)
        {
            UserIdentifier = userIdentifier;
            HasRespOfficeFilter = hasRespOfficeFilter;
            HasEntityFilter = hasEntityFilter;
            EntityFilterType = entityFilterType;
            ApplyAgentFilter = applyAgentFilter;
        }
    }
}
