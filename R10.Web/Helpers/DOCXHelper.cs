using Microsoft.AspNetCore.Authorization;
using R10.Web.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace R10.Web.Helpers
{
    public static class DOCXHelper
    {
        public static string SendDOCXDesc(int? sendDOCXType) {
            string desc;

            switch (sendDOCXType) {
                case 1:
                    desc = "All";
                    break;
                case 2:
                    desc = "Specific";
                    break;
                default:
                    desc = "None";
                    break;
            }
            return desc;
        }

        public static string DOCXSendAsDesc(string docxSendAs)
        {
            string desc;

            switch (docxSendAs.ToLower())
            {
                case "t":
                    desc = "To";
                    break;
                case "c":
                    desc = "Cc";
                    break;
                default:
                    desc = "";
                    break;
            }
            return desc;
        }

        public static async Task<bool> CanUpdateDOCX(string sys, ClaimsPrincipal user, IAuthorizationService authService)
        {
            var modifyPolicy = GetDOCXModifyPolicy(sys);
            return (await authService.AuthorizeAsync(user, modifyPolicy)).Succeeded;
        }

        public static async Task<bool> CanAccessDOCX(string sys, ClaimsPrincipal user, IAuthorizationService authService)
        {
            var modifyPolicy = GetDOCXAccessPolicy(sys);
            return (await authService.AuthorizeAsync(user, modifyPolicy)).Succeeded;
        }


        //TO DO Check permission.
        public static string GetDOCXModifyPolicy(string sys)
        {
            switch (sys)
            {
                case "P": return PatentAuthorizationPolicy.LetterModify;
                case "T": return TrademarkAuthorizationPolicy.LetterModify;
                case "G": return GeneralMatterAuthorizationPolicy.LetterModify;
                default: return "";
            }
        }
        public static string GetDOCXAccessPolicy(string sys)
        {
            switch (sys)
            {
                case "P": return PatentAuthorizationPolicy.CanAccessLetters;
                case "T": return TrademarkAuthorizationPolicy.CanAccessLetters;
                case "G": return GeneralMatterAuthorizationPolicy.CanAccessLetters;
                default: return "";
            }
        }

        public static string GetDOCXPath(string system, string fileName)
        {
            // Get the filename including the extension of a full file path
            fileName = Path.GetFileName(fileName);
            return Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\DOCXes", system, fileName);
        }

        public static string GetDOCXLogPath(string system, string fileName)
        {
            // Get the filename including the extension of a full file path
            fileName = Path.GetFileName(fileName);
            return Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\Logs\DOCXes", system, fileName);
        }

    }
}
