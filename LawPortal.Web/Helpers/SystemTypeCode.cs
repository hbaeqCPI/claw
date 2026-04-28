using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Helpers
{
    public static class SystemTypeCode
    {
        // values here are tied to global search tblGSSystem table
        public const string Patent = "P";
        public const string Trademark = "T";
        public const string GeneralMatter = "G";
        public const string PTOActions = "L";
        public const string TrademarkLinks = "M";
        public const string AMS = "A";
        public const string DMS = "D";
        public const string Clearance = "C";
        public const string IDS = "I";
        public const string PatClearance = "E";
        public const string Shared = "S";
        public const string PatInvention = "PI";
        public const string ForeignFiling = "F";
        public const string RMS = "R";
    }

    public static class ScreenCode
    {
        // values here are tied to global search tblGSScreen table
        public const string Invention = "Inv";
        public const string Application = "CA";
        public const string Trademark = "Tmk";
        public const string Action = "Act";
        public const string CostTracking = "Cost";
        public const string GeneralMatter = "GM";
        public const string DMS = "DMS";
        public const string AMS = "AMS";
        public const string IDS = "IDS";
        public const string RTS = "RTS";
        public const string TL = "TL";
        public const string Clearance = "Tmc";
        public const string PatClearance = "Pac";
        public const string Product = "Prd";
        public const string Assignment = "Asgmt";
        public const string Licensee = "Lce";
        public const string ActionInv = "ActInv";
        public const string ActionDueDate = "DueDate";
        public const string TmkConflict = "Conf";
        public const string CostInv = "CostInv";

    }

    public static class DataKey
    {
        public const string Invention = "InvId";
        public const string Application = "AppId";
        public const string Trademark = "TmkId";
        public const string Action = "ActId";
        public const string CostTracking = "CostTrackId";
        public const string GeneralMatter = "MatId";
        public const string DMS = "DMSId";
        public const string Clearance = "TmcId";
        public const string PatClearance = "PacId";
        public const string ActionInv = "ActInvId";
        public const string CostTrackingInv = "CostTrackInvId";

    }

}
