using System;

namespace LawPortal.Web.Helpers
{
    public static class SystemTypeHelper
    {
        public static string GetSystem(string systemType)
        {
            switch (systemType)
            {
                case "P": return "Patent";
                case "T": return "Trademark";
                case "G": return "GeneralMatter";
                case "A": return "AMS";
                case "D": return "DMS";
                case "C": return "Clearance";
                case "E": return "PatClearance";
                default: return String.Empty;
            }
        }
    }
}
