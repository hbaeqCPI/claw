using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Shared;
using R10.Web.Controllers;

namespace Microsoft.AspNetCore.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string ApplicationBaseUrl(this IUrlHelper urlHelper, string scheme)
        {
            var baseUrl =  urlHelper.Action("", "", new { area = "", id = "" }, scheme);
            return baseUrl.TrimEnd('/');
        }

        public static string EmailConfirmationLink(this IUrlHelper urlHelper, string id, string token, string scheme)
        {
            return urlHelper.Action(
                action: nameof(AccountController.ConfirmEmail),
                controller: "Account",
                values: new { id, token },
                protocol: scheme);
        }

        public static string ResetPasswordCallbackLink(this IUrlHelper urlHelper, string id, string token, string scheme)
        {
            return urlHelper.Action(
                action: nameof(AccountController.ResetPassword),
                controller: "Account",
                values: new { id, token },
                protocol: scheme);
        }

        public static string ForgotPasswordLink(this IUrlHelper urlHelper, string scheme)
        {
            return urlHelper.Action(
                action: nameof(AccountController.ForgotPassword),
                controller: "Account",
                values: null,
                protocol: scheme);
        }

        public static string LoginLink(this IUrlHelper urlHelper, string scheme)
        {
            return urlHelper.Action(
                action: nameof(AccountController.Login),
                controller: "Account",
                values: null,
                protocol: scheme);
        }

        public static string DeleteConfirmWithCodeLink(this IUrlHelper urlHelper)
        {
            return urlHelper.Action("ConfirmationCode", "DeleteConfirmation", new { Area = "" });
        }

        public static string DeleteConfirmWithCheckboxLink(this IUrlHelper urlHelper)
        {
            return urlHelper.Action("Index", "DeleteConfirmation", new { Area = "" });
        }

        public static string ImageLink(this IUrlHelper urlHelper, string imageFolder, string imageFile)
        {
            return $"{urlHelper.Action("Images", "UserFiles", new { area = "", id = imageFolder })}/{imageFile}";
        }

        public static string ImageNotFoundLink(this IUrlHelper urlHelper)
        {
            return $"{urlHelper.Action("Images", "UserFiles", new { area = "", id = "" })}/ImageNotFound.png";
        }

        public static string FileViewerLink(this IUrlHelper urlHelper, string system, string thumbnailFile, int key, string screenCode)
        {
            return urlHelper.Action("ViewImage", "FileViewer", new { area = "", system = system, thumbnailFile = thumbnailFile, screenCode = screenCode, key = key.ToString() });
        }

        public static string CPiLogoLink(this IUrlHelper urlHelper, string scheme)
        {
            return urlHelper.Action("site_banner.png", "images", new { area = "", id = "" }, scheme);
        }

        public static string ReportLogoLink(this IUrlHelper urlHelper, string scheme)
        {
            return urlHelper.Action("site_report_logo", "images", new { area = "", id = "" }, scheme);
        }

        public static string AMSInstructionPageLink(this IUrlHelper urlHelper, string scheme)
        {
            return urlHelper.Action("Index", "Instructions", new { area = "AMS" }, scheme);
        }

        public static string AMSPortfolioReviewPageLink(this IUrlHelper urlHelper, string scheme)
        {
            return urlHelper.Action("Index", "PortfolioReview", new { area = "AMS" }, scheme);
        }

        public static string RMSInstructionPageLink(this IUrlHelper urlHelper, string scheme)
        {
            return urlHelper.Action("Index", "Instructions", new { area = "RMS" }, scheme);
        }

        public static string RMSPortfolioReviewPageLink(this IUrlHelper urlHelper, int remId, string scheme)
        {
            return urlHelper.Action("Index", "PortfolioReview", new { area = "RMS", remId = remId }, scheme);
        }

        public static string FFInstructionPageLink(this IUrlHelper urlHelper, string scheme)
        {
            return urlHelper.Action("Index", "Instructions", new { area = "ForeignFiling" }, scheme);
        }

        public static string FFPortfolioReviewPageLink(this IUrlHelper urlHelper, int remId, string scheme)
        {
            return urlHelper.Action("Index", "PortfolioReview", new { area = "ForeignFiling", remId = remId }, scheme);
        }

        public static string TokenEndpointLink(this IUrlHelper urlHelper)
        {
            return urlHelper.Action("Token", "Connect", new { Area = "" });
        }

        public static string UserSetupLink(this IUrlHelper urlHelper, string scheme)
        {
            return urlHelper.Action("Index", "User", new { area = "Admin" }, scheme);
        }

        public static string? GetTradeSecretDetailLink(this IUrlHelper urlHelper, string? screenId, int recId, string? scheme = "")
        {
            return "";
        }

    }
}
