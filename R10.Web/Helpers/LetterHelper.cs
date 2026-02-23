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
    public static class LetterHelper
    {
        public static string SendLetterDesc(int? sendLetterType) {
            string desc;

            switch (sendLetterType) {
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

        public static string LetterSendAsDesc(string letterSendAs)
        {
            string desc;

            switch (letterSendAs.ToLower())
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

        public static async Task<bool> CanUpdateLetter(string sys, ClaimsPrincipal user, IAuthorizationService authService)
        {
            var modifyPolicy = GetLetterModifyPolicy(sys);
            return (await authService.AuthorizeAsync(user, modifyPolicy)).Succeeded;
        }

        public static async Task<bool> CanAccessLetter(string sys, ClaimsPrincipal user, IAuthorizationService authService)
        {
            var modifyPolicy = GetLetterAccessPolicy(sys);
            return (await authService.AuthorizeAsync(user, modifyPolicy)).Succeeded;
        }

        public static string GetLetterModifyPolicy(string sys)
        {
            switch (sys)
            {
                case "P": return PatentAuthorizationPolicy.LetterModify;
                case "T": return TrademarkAuthorizationPolicy.LetterModify;
                case "G": return GeneralMatterAuthorizationPolicy.LetterModify;
                default: return "";
            }
        }
        public static string GetLetterAccessPolicy(string sys)
        {
            switch (sys)
            {
                case "P": return PatentAuthorizationPolicy.CanAccessLetters;
                case "T": return TrademarkAuthorizationPolicy.CanAccessLetters;
                case "G": return GeneralMatterAuthorizationPolicy.CanAccessLetters;
                default: return "";
            }
        }

        public static string GetLetterPath(string system, string fileName)
        {
            // Get the filename including the extension of a full file path
            fileName = Path.GetFileName(fileName);
            return Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\Letters", system, fileName);
        }

        public static string GetLetterLogPath(string system, string fileName)
        {
            // Get the filename including the extension of a full file path
            fileName = Path.GetFileName(fileName);
            return Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles\Logs\Letters", system, fileName);
        }

    }
}
