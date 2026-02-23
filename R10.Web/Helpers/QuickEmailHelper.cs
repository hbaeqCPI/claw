using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Web.Models;
using R10.Web.Extensions;
namespace R10.Web.Helpers
{
    public static class QuickEmailHelper
    {
        public static AreaControllerDTO GetQESetupAreaController(string systemType)
        {
            switch (systemType)
            {
                case "P":
                    return new AreaControllerDTO { Area = "Patent", Controller = "PatQuickEmailSetup" };
                case "T":
                    return new AreaControllerDTO { Area = "Trademark", Controller = "TmkQuickEmailSetup" };
                case "G":
                    return new AreaControllerDTO { Area = "GeneralMatter", Controller = "MatterQuickEmailSetup" };
                case "A":
                    return new AreaControllerDTO { Area = "AMS", Controller = "AMSQuickEmailSetup" };
                case "D":
                    return new AreaControllerDTO { Area = "DMS", Controller = "DMSQuickEmailSetup" };
                default:
                    return new AreaControllerDTO { Area = "", Controller = "" };
            }
        }

        public static AreaControllerDTO GetQEAreaController(string systemType)
        {
            switch (systemType)
            {
                case "P":
                    return new AreaControllerDTO { Area = "Patent", Controller = "PatQuickEmail" };
                case "T":
                    return new AreaControllerDTO { Area = "Trademark", Controller = "TmkQuickEmail" };
                case "G":
                    return new AreaControllerDTO { Area = "GeneralMatter", Controller = "MatterQuickEmail" };
                case "A":
                    return new AreaControllerDTO { Area = "AMS", Controller = "AMSQuickEmail" };
                case "D":
                    return new AreaControllerDTO { Area = "DMS", Controller = "DMSQuickEmail" };
                default:
                    return new AreaControllerDTO { Area = "", Controller = "" };
            }
        }

        public static string GetSystem(string systemType)
        {
            switch (systemType)
            {
                case "P":
                    return "Patent";
                case "T":
                    return "Trademark";
                case "G":
                    return "GeneralMatter";
                case "A":
                    return "AMS";
                case "D":
                    return "DMS";
                case "C":
                    return "Clearance";
                case "E":
                    return "PatClearance";
                default:
                    return String.Empty;
            }
        }

        public static string GetPrefix(string systemType)
        {
            switch (systemType)
            {
                case "P":
                    return "Pat";
                case "T":
                    return "Tmk";
                case "G":
                    return "GM";
                case "A":
                    return "AMS";
                case "D":
                    return "DMS";
                default:
                    return "";
            }
        }

        public static ScreenName GetParentScreenName(string systemType, string screenName)
        {
            if (systemType == "P")
            {
                switch (screenName)
                {
                    case "Invention":
                        return ScreenName.PatInvention;
                    case "Invention Action Due":
                        return ScreenName.PatActionDueInv;
                    case "Invention Action Due Date":
                        return ScreenName.PatActionDueDateInv;
                    case "Invention Cost Tracking":
                        return ScreenName.PatCostTrackingInv;

                    case "Country Application":
                        return ScreenName.PatCountryApplication;
                    case "Action Due":
                        return ScreenName.PatActionDue;
                    case "Action Due Date":
                        return ScreenName.PatActionDueDate;
                    case "Cost Tracking":
                        return ScreenName.PatCostTracking;

                    case "Patent Search":
                        return ScreenName.PatSearch;
                    
                    case "Attorney Modified":
                        return ScreenName.PatInventionAttyChanged;

                    case "Country App Documents":
                        return ScreenName.PatImagesApp;
                    case "Action Documents":
                        return ScreenName.PatImagesAct;
                    case "Action Delegation":
                        return ScreenName.PatActionDelegation;
                    case "Deleted Action Delegation":
                        return ScreenName.PatActionDeletedDelegation;
                    case "Reassigned Action Delegation":
                        return ScreenName.PatActionReassignedDelegation;
                    case "Delegated Action Changed":
                        return ScreenName.PatActionDelegation;
                    case "Delegated Action Completed":
                        return ScreenName.PatActionDelegation;

                    case "Patent Invention Action Images":
                        return ScreenName.PatImagesActInv;
                    case "Invention Due Date Dedocket":
                        return ScreenName.PatActionDueDateInvDedocket;
                    case "Invention Action Delegation":
                        return ScreenName.PatActionInvDelegation;
                    case "Reassigned Invention Action Delegation":
                        return ScreenName.PatActionInvReassignedDelegation;
                    case "Deleted Invention Action Delegation":
                        return ScreenName.PatActionInvDeletedDelegation;
                    case "Invention Action Due Deleted":
                        return ScreenName.PatActionDueInvDeleted;

                    case "DeDocket Instruction":
                        return ScreenName.PatActionDueDateDedocket;
                    case "Dedocket Instruction Completed":
                        return ScreenName.PatActionDueDateDedocket;
                    case "Deleted Country App":
                        return ScreenName.PatCountryAppDeleted;
                    case "Deleted Action Due":
                        return ScreenName.PatActionDueDeleted;                    

                    case "Inventor App Award":
                        return ScreenName.PatInventorAppAward;
                    case "Lump Sum Award":
                        return ScreenName.PatRemunerationLumpSumAward;
                    case "Distribution Award":
                        return ScreenName.PatRemunerationDistributionAward;
                    case "Yearly Award":
                        return ScreenName.PatRemunerationYearlyAward;
                    case "French Remuneration":
                        return ScreenName.PatIRFRRemuneration;

                    case "Request Docket":
                        return ScreenName.PatRequestDocket;

                    default:
                        return ScreenName.NotSet;
                }
            }
            else if (systemType == "T")
            {
                switch (screenName)
                {
                    case "Trademark":
                        return ScreenName.TmkTrademark;
                    case "Conflict/Opposition":
                        return ScreenName.TmkConflict;
                    case "Action Due":
                        return ScreenName.TmkActionDue;
                    case "Action Due Date":
                        return ScreenName.TmkActionDueDate;
                    case "Cost Tracking":
                        return ScreenName.TmkCostTracking;

                    case "Attorney Modified":
                        return ScreenName.TmkTrademarktAttyChanged;

                    case "Trademark Documents":
                        return ScreenName.TmkImages;
                    case "Action Documents":
                        return ScreenName.TmkImagesAct;
                    case "Action Delegation":
                        return ScreenName.TmkActionDelegation;
                    case "Deleted Action Delegation":
                        return ScreenName.TmkActionDeletedDelegation;
                    case "Reassigned Action Delegation":
                        return ScreenName.TmkActionReassignedDelegation;
                    case "Delegated Action Changed":
                        return ScreenName.TmkActionDelegation;
                    case "Delegated Action Completed":
                        return ScreenName.TmkActionDelegation;

                    case "DeDocket Instruction":
                        return ScreenName.TmkActionDueDateDedocket;
                    case "Dedocket Instruction Completed":
                        return ScreenName.TmkActionDueDateDedocket;
                    case "Deleted Trademark":
                        return ScreenName.TmkTrademarkDeleted;
                    case "Deleted Action Due":
                        return ScreenName.TmkActionDueDeleted;

                    case "Request Docket":
                        return ScreenName.TmkRequestDocket;

                    default:
                        return ScreenName.NotSet;
                }
            }
            else if (systemType == "G")
            {
                switch (screenName)
                {
                    case "General Matters":
                        return ScreenName.GmMatter;
                    case "Action Due":
                        return ScreenName.GmActionDue;
                    case "Action Due Date":
                        return ScreenName.GmActionDueDate;
                    case "Cost Tracking":
                        return ScreenName.GmCostTracking;

                    case "General Matter Documents":
                        return ScreenName.GMImages;
                    case "Action Documents":
                        return ScreenName.GMImagesAct;
                    case "Action Delegation":
                        return ScreenName.GmActionDelegation;
                    case "Deleted Action Delegation":
                        return ScreenName.GmActionDeletedDelegation;
                    case "Reassigned Action Delegation":
                        return ScreenName.GmActionReassignedDelegation;
                    case "Delegated Action Changed":
                        return ScreenName.GmActionDelegation;
                    case "Delegated Action Completed":
                        return ScreenName.GmActionDelegation;

                    case "DeDocket Instruction":
                        return ScreenName.GmActionDueDateDedocket;
                    case "Dedocket Instruction Completed":
                        return ScreenName.GmActionDueDateDedocket;

                    case "Deleted General Matter":
                        return ScreenName.GmMatterDeleted;
                    case "Deleted Action Due":
                        return ScreenName.GmActionDueDeleted;

                    case "Request Docket":
                        return ScreenName.GmRequestDocket;

                    default:
                        return ScreenName.NotSet;
                }
            }
            else if (systemType == "D")
            {
                switch (screenName)
                {
                    case "Invention Disclosure":                    
                    case "Disclosure Authorize":
                    case "Disclosure Submit":
                        return ScreenName.DmsDisclosure;
                    case "Disclosure Review":
                        return ScreenName.DmsReview;
                    case "Action Due":
                        return ScreenName.DmsActionDue;
                    case "Action Due Date":
                        return ScreenName.DmsActionDueDate;
                    case "Inventor DMS Award":
                        return ScreenName.PatInventorDMSAward;
                    case "Disclosure Agenda Meeting":
                        return ScreenName.DmsAgenda;

                    default:
                        return ScreenName.NotSet;
                }
            }
            else if (systemType == "A")
            {
                switch (screenName)
                {
                    case "AMS Main":
                        return ScreenName.AmsMain;
                    case "AMS Due":
                        return ScreenName.AmsDue;
                    default:
                        return ScreenName.NotSet;
                }
            }
            else if (systemType == "C")
            {
                switch (screenName)
                {
                    case "Clearance":
                    case "Trademark Search":
                        return ScreenName.TmcClearance;
                    default:
                        return ScreenName.NotSet;
                }
            }
            else if (systemType == "E")
            {
                switch (screenName)
                {
                    case "PatClearance":
                    case "Patent Clearance Search":
                        return ScreenName.PatClearance;
                    default:
                        return ScreenName.NotSet;
                }
            }
            else if (systemType == SystemTypeCode.Shared)
            {
                switch (screenName)
                {
                    case "Doc Verification Documents":
                        return ScreenName.SharedDocVerification;
                }
            }
            return ScreenName.NotSet;
        }

        public static string GetQuickEmailPath(string system, string fileName)
        {
            // Get the filename including the extension of a full file path
            fileName = Path.GetFileName(fileName);
            return Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\QuickEmails", system, fileName);
        }

        //Display child records as html table
        public static string ToHtmlTable<T>(this List<T> list)
        {
            var ret = string.Empty;

            return list == null || !list.Any()
                ? string.Empty
                : "<table style='width:100%; border:1px solid silver; border-collapse:collapse;' cellspacing='2' cellpadding='2'>" +
                  list.First().GetType().GetProperties().Select(p => p.GetCustomAttribute<DisplayAttribute>() == null ? p.Name : p.GetCustomAttribute<DisplayAttribute>().Name).ToList().ToColumnHeaders() +
                  "<tbody>" +
                  list.Aggregate(ret, (current, t) => current + t.ToHtmlTableRow()) +
                  "</tbody></table>";
        }
        public static string ToColumnHeaders<T>(this List<T> list)
        {
            var ret = string.Empty;

            return list == null || !list.Any()
                ? ret
                : "<thead><tr>" +
                  list.Aggregate(ret,
                      (current, propValue) =>
                          current +
                          "<th style='white-space:nowrap; border:1px solid silver; font-size:12px'>" +
                           Convert.ToString(propValue) +
                           "</th>"
                        ) +
                  "</tr></thead>";
        }
        public static string ToHtmlTableRow<T>(this T item)
        {
            var ret = string.Empty;

            return item == null
                ? ret
                : "<tr>" +
                  item.GetType()
                      .GetProperties()
                      .Aggregate(ret, (current, prop) => current +
                        "<td style='white-space:nowrap; border:1px solid silver; font-size:12px; " +
                        "text-align:" + GetTextAlign(prop.GetValue(item, null)) + ";'>" +
                        ToLocalizedString(prop.GetValue(item, null)) +
                        "</td>") +
                "</tr>";
        }
        public static string ToLocalizedString(object value)
        {
            if (value == null)
                return string.Empty;

            var culture = Thread.CurrentThread.CurrentCulture.Name;

            if (value is DateTime)
                return ((DateTime?)Convert.ToDateTime(value)).FormatToDisplay(culture);

            if (value is double)
                return Convert.ToDouble(value).FormatToDisplay(culture);

            if (value is decimal)
                return Convert.ToDecimal(value).FormatToDisplay(culture);

            return Convert.ToString(value);
        }
        public static string GetTextAlign(object value)
        {
            if (value is int || value is uint ||
                value is short || value is ushort ||
                value is long || value is ulong ||
                value is float || value is double || value is decimal ||
                value is DateTime)
                return "right";

            return "left";
        }
    }

    
}

