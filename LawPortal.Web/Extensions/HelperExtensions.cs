using AutoMapper;
using Kendo.Mvc;
using Kendo.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;
using LawPortal.Core.Entities;
using LawPortal.Core.Helpers;
using LawPortal.Web.Areas;
using LawPortal.Web.Helpers;
using LawPortal.Web.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LawPortal.Web.Extensions
{
    public static class HelperExtensions
    {
        public static bool IsAjax(this HttpRequest request)
        {
            return (request.Headers.ContainsKey("X-Requested-With") && request.Headers["X-Requested-With"] == "XMLHttpRequest");
        }

        public static string GetBrowserLocale(this HttpRequest request)
        {

            var browserLang = request.Headers["Accept-Language"].ToString().Split(";").FirstOrDefault()?.Split(",").FirstOrDefault();
            return string.IsNullOrEmpty(browserLang) ? CultureInfo.CurrentCulture.Name : browserLang;
        }

        public static string GetBearerToken(this HttpRequest request)
        {
            var token = "";
            if (request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = request.Headers["Authorization"][0];
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    token = authHeader.Substring("Bearer ".Length);
            }

            return token;
        }

        public async static Task<string> GetErrorMessage(this HttpResponseMessage response)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(errorMessage))
            {
                // Check for localizer object
                var localizedMessage = "";
                try
                {
                    localizedMessage = JObject.Parse(errorMessage)?["Value"]?.ToString();
                } catch { }

                if (!string.IsNullOrEmpty(localizedMessage))
                    errorMessage = localizedMessage;
                else
                {
                    // Check for ProblemDetails object
                    try
                    {
                        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(errorMessage);
                        if (problemDetails != null)
                            errorMessage = problemDetails.Title;
                    }
                    catch { }
                }
            }
            else
                errorMessage = response.ReasonPhrase;

            return errorMessage ?? "";
        }

        public static async Task ApplyDetailPagePermission<T>(this T pagePermission, ClaimsPrincipal user, IAuthorizationService authService) where T: DetailPagePermission
        {
            var isRemarksOnlyModify = false;
           
            var isFullModify = !string.IsNullOrEmpty(pagePermission.FullModifyPolicy) &&  (await authService.AuthorizeAsync(user, pagePermission.FullModifyPolicy)).Succeeded;

            if (!isFullModify)
                isRemarksOnlyModify = !string.IsNullOrEmpty(pagePermission.RemarksOnlyModifyPolicy) && (await authService.AuthorizeAsync(user, pagePermission.RemarksOnlyModifyPolicy)).Succeeded;

            if (!(isFullModify || isRemarksOnlyModify))
                pagePermission.HasLimitedRead = !string.IsNullOrEmpty(pagePermission.LimitedReadPolicy) && (await authService.AuthorizeAsync(user, pagePermission.LimitedReadPolicy)).Succeeded;

            pagePermission.CanAddRecord = isFullModify;
            pagePermission.CanEditRecord = isFullModify || isRemarksOnlyModify;
            pagePermission.CanEditRemarksOnly = isRemarksOnlyModify;

            pagePermission.CanEmail = !pagePermission.HasLimitedRead;
            if (!string.IsNullOrEmpty(pagePermission.CopyPolicy)) {
                pagePermission.CanCopyRecord = (await authService.AuthorizeAsync(user, pagePermission.CopyPolicy)).Succeeded; 
            }
            else {
                pagePermission.CanCopyRecord = isFullModify;
            }

            if (!isRemarksOnlyModify)
                pagePermission.CanDeleteRecord = !string.IsNullOrEmpty(pagePermission.DeletePolicy) && (await authService.AuthorizeAsync(user, pagePermission.DeletePolicy)).Succeeded;

            if (!string.IsNullOrEmpty(pagePermission.CanUploadDocumentsPolicy))
                pagePermission.CanUploadDocuments = (await authService.AuthorizeAsync(user, pagePermission.CanUploadDocumentsPolicy)).Succeeded;
        }

        public static async Task ApplyDetailPagePermission<T>(this T pagePermission, ClaimsPrincipal user, string respOffice, IAuthorizationService authService) where T : DetailPagePermission
        {
            var isRemarksOnlyModify = false;

            var isFullModify = (await authService.AuthorizeAsync(user, respOffice, pagePermission.FullModifyPolicy)).Succeeded;

            if (!isFullModify)
                isRemarksOnlyModify = (await authService.AuthorizeAsync(user, respOffice, pagePermission.RemarksOnlyModifyPolicy)).Succeeded;

            if (!(isFullModify || isRemarksOnlyModify))
                pagePermission.HasLimitedRead = (await authService.AuthorizeAsync(user, respOffice, pagePermission.LimitedReadPolicy)).Succeeded;

            pagePermission.CanAddRecord = isFullModify;
            pagePermission.CanEditRecord = isFullModify || isRemarksOnlyModify;
            pagePermission.CanEditRemarksOnly = isRemarksOnlyModify;

            pagePermission.CanEmail = !pagePermission.HasLimitedRead;
            if (!string.IsNullOrEmpty(pagePermission.CopyPolicy))
            {
                pagePermission.CanCopyRecord = (await authService.AuthorizeAsync(user, respOffice, pagePermission.CopyPolicy)).Succeeded;
            }
            else
            {
                pagePermission.CanCopyRecord = isFullModify;
            }

            if (!isRemarksOnlyModify)
                pagePermission.CanDeleteRecord = (await authService.AuthorizeAsync(user, respOffice, pagePermission.DeletePolicy)).Succeeded;

            if (!string.IsNullOrEmpty(pagePermission.CanUploadDocumentsPolicy))
                pagePermission.CanUploadDocuments = (await authService.AuthorizeAsync(user, respOffice, pagePermission.CanUploadDocumentsPolicy)).Succeeded;
        }

        public static void AddSharedSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = SharedAuthorizationPolicy.FullModify;
            viewModel.RemarksOnlyModifyPolicy = SharedAuthorizationPolicy.RemarksOnlyModify;
            viewModel.DeletePolicy = SharedAuthorizationPolicy.CanDelete;
            viewModel.LimitedReadPolicy = SharedAuthorizationPolicy.LimitedRead;
            viewModel.CanUploadDocumentsPolicy = SharedAuthorizationPolicy.CanUploadDocuments;
        }

        public static void AddPatentSecurityPolicies<T>(this T viewModel,bool hasRespOfficeFilter=false) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = hasRespOfficeFilter ? PatentAuthorizationPolicy.FullModifyByRespOffice : PatentAuthorizationPolicy.FullModify;
            viewModel.RemarksOnlyModifyPolicy = hasRespOfficeFilter ? PatentAuthorizationPolicy.RemarksOnlyModifyByRespOffice : PatentAuthorizationPolicy.RemarksOnlyModify;
            viewModel.DeletePolicy = hasRespOfficeFilter ? PatentAuthorizationPolicy.CanDeleteByRespOffice : PatentAuthorizationPolicy.CanDelete;
            viewModel.LimitedReadPolicy = hasRespOfficeFilter ? PatentAuthorizationPolicy.LimitedReadByRespOffice : PatentAuthorizationPolicy.LimitedRead;
            viewModel.CanUploadDocumentsPolicy = hasRespOfficeFilter ? PatentAuthorizationPolicy.FullModifyByRespOffice : PatentAuthorizationPolicy.CanUploadDocuments;
        }

        public static void AddIDSSecurityPolicies<T>(this T viewModel, bool hasRespOfficeFilter = false) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = hasRespOfficeFilter ? IDSAuthorizationPolicy.FullModifyByRespOffice : IDSAuthorizationPolicy.FullModify;
            viewModel.DeletePolicy = hasRespOfficeFilter ? IDSAuthorizationPolicy.FullModifyByRespOffice : IDSAuthorizationPolicy.FullModify;
        }

        //USE AddDMSDisclosureSecurityPolicies FOR DISCLOSURE SCREEN
        public static void AddDMSSecurityPolicies<T>(this T viewModel, bool hasRespOfficeFilter = false) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = DMSAuthorizationPolicy.FullModify;
            viewModel.RemarksOnlyModifyPolicy = DMSAuthorizationPolicy.RemarksOnlyModify;
            viewModel.DeletePolicy = DMSAuthorizationPolicy.CanDelete;
            viewModel.CanUploadDocumentsPolicy = DMSAuthorizationPolicy.FullModify;
        }

        public static void AddDMSDisclosureSecurityPolicies<T>(this T viewModel, bool hasRespOfficeFilter = false) where T : DetailPagePermission
        {
            //DISCLOSURE CAN BE ADD BY INVENTOR
            viewModel.FullModifyPolicy = DMSAuthorizationPolicy.CanAddDisclosure;            

            //DISCLOSURE REMARKS CAN BE EDITED BY REVIEWERS AND MODIFY USERS AFTER DISCLOSURE IS SUBMITTED
            //viewModel.RemarksOnlyModifyPolicy = DMSAuthorizationPolicy.RemarksOnlyModify;
            viewModel.DeletePolicy = DMSAuthorizationPolicy.CanAddDisclosure;
            viewModel.CanUploadDocumentsPolicy = DMSAuthorizationPolicy.Inventor;
        }

        public static void AddTrademarkSecurityPolicies<T>(this T viewModel, bool hasRespOfficeFilter = false) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = hasRespOfficeFilter ? TrademarkAuthorizationPolicy.FullModifyByRespOffice : TrademarkAuthorizationPolicy.FullModify;
            viewModel.RemarksOnlyModifyPolicy = hasRespOfficeFilter ? TrademarkAuthorizationPolicy.RemarksOnlyModifyByRespOffice : TrademarkAuthorizationPolicy.RemarksOnlyModify;
            viewModel.DeletePolicy = hasRespOfficeFilter ? TrademarkAuthorizationPolicy.CanDeleteByRespOffice : TrademarkAuthorizationPolicy.CanDelete;
            viewModel.LimitedReadPolicy = hasRespOfficeFilter ? TrademarkAuthorizationPolicy.LimitedReadByRespOffice : TrademarkAuthorizationPolicy.LimitedRead;
            viewModel.CanUploadDocumentsPolicy = hasRespOfficeFilter ? TrademarkAuthorizationPolicy.FullModifyByRespOffice : TrademarkAuthorizationPolicy.CanUploadDocuments;
        }
        public static void AddGeneralMatterSecurityPolicies<T>(this T viewModel, bool hasRespOfficeFilter = false) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = hasRespOfficeFilter ? GeneralMatterAuthorizationPolicy.FullModifyByRespOffice : GeneralMatterAuthorizationPolicy.FullModify;
            viewModel.RemarksOnlyModifyPolicy = hasRespOfficeFilter ? GeneralMatterAuthorizationPolicy.RemarksOnlyModifyByRespOffice : GeneralMatterAuthorizationPolicy.RemarksOnlyModify;
            viewModel.DeletePolicy = hasRespOfficeFilter ? GeneralMatterAuthorizationPolicy.CanDeleteByRespOffice : GeneralMatterAuthorizationPolicy.CanDelete;
            viewModel.LimitedReadPolicy = hasRespOfficeFilter ? GeneralMatterAuthorizationPolicy.LimitedReadByRespOffice : GeneralMatterAuthorizationPolicy.LimitedRead;
            viewModel.CanUploadDocumentsPolicy = hasRespOfficeFilter ? GeneralMatterAuthorizationPolicy.FullModifyByRespOffice : GeneralMatterAuthorizationPolicy.CanUploadDocuments;
        }
        public static void AddAMSSecurityPolicies<T>(this T viewModel, bool hasRespOfficeFilter = false) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = hasRespOfficeFilter ? AMSAuthorizationPolicy.FullModifyByRespOffice : AMSAuthorizationPolicy.FullModify;
            viewModel.RemarksOnlyModifyPolicy = hasRespOfficeFilter ? AMSAuthorizationPolicy.RemarksOnlyModifyByRespOffice : AMSAuthorizationPolicy.RemarksOnlyModify;
            viewModel.DeletePolicy = hasRespOfficeFilter ? AMSAuthorizationPolicy.CanDeleteByRespOffice : AMSAuthorizationPolicy.CanDelete;
            viewModel.LimitedReadPolicy = hasRespOfficeFilter ? AMSAuthorizationPolicy.LimitedReadByRespOffice : AMSAuthorizationPolicy.LimitedRead;
        }

        public static void AddClearanceSecurityPolicies<T>(this T viewModel, bool hasRespOfficeFilter = false) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = SearchRequestAuthorizationPolicy.FullModify;
            viewModel.RemarksOnlyModifyPolicy = SearchRequestAuthorizationPolicy.RemarksOnlyModify;
            viewModel.DeletePolicy = SearchRequestAuthorizationPolicy.CanDelete;
            viewModel.CanUploadDocumentsPolicy = SearchRequestAuthorizationPolicy.FullModify;
        }
        public static void AddTMCClearanceSecurityPolicies<T>(this T viewModel, bool hasRespOfficeFilter = false) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = SearchRequestAuthorizationPolicy.Reviewer;
            viewModel.DeletePolicy = SearchRequestAuthorizationPolicy.Reviewer;
            viewModel.CanUploadDocumentsPolicy = SearchRequestAuthorizationPolicy.Reviewer;
        }

        public static void AddPatentClearanceSecurityPolicies<T>(this T viewModel, bool hasRespOfficeFilter = false) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = PatentClearanceAuthorizationPolicy.FullModify;
            viewModel.RemarksOnlyModifyPolicy = PatentClearanceAuthorizationPolicy.RemarksOnlyModify;
            viewModel.DeletePolicy = PatentClearanceAuthorizationPolicy.CanDelete;
            viewModel.CanUploadDocumentsPolicy = PatentClearanceAuthorizationPolicy.FullModify;
        }
        public static void AddPACClearanceSecurityPolicies<T>(this T viewModel, bool hasRespOfficeFilter = false) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = PatentClearanceAuthorizationPolicy.Reviewer;
            viewModel.DeletePolicy = PatentClearanceAuthorizationPolicy.Reviewer;
            viewModel.CanUploadDocumentsPolicy = PatentClearanceAuthorizationPolicy.Reviewer;
        }

        public static void AddPatentClearanceAuxiliarySecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = PatentClearanceAuthorizationPolicy.AuxiliaryModify;
            viewModel.RemarksOnlyModifyPolicy = PatentClearanceAuthorizationPolicy.AuxiliaryRemarksOnly;
            viewModel.DeletePolicy = PatentClearanceAuthorizationPolicy.AuxiliaryCanDelete;
            viewModel.LimitedReadPolicy = PatentClearanceAuthorizationPolicy.AuxiliaryLimited;
        }

        public static void AddClearanceAuxiliarySecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = SearchRequestAuthorizationPolicy.AuxiliaryModify;
            viewModel.RemarksOnlyModifyPolicy = SearchRequestAuthorizationPolicy.AuxiliaryRemarksOnly;
            viewModel.DeletePolicy = SearchRequestAuthorizationPolicy.AuxiliaryCanDelete;
            viewModel.LimitedReadPolicy = SearchRequestAuthorizationPolicy.AuxiliaryLimited;
        }
        public static void AddAMSAuxiliarySecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = AMSAuthorizationPolicy.AuxiliaryModify;
            viewModel.RemarksOnlyModifyPolicy = AMSAuthorizationPolicy.AuxiliaryRemarksOnly;
            viewModel.DeletePolicy = AMSAuthorizationPolicy.AuxiliaryCanDelete;
            viewModel.LimitedReadPolicy = AMSAuthorizationPolicy.AuxiliaryLimited;
        }
        public static void AddPatentAuxiliarySecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = PatentAuthorizationPolicy.AuxiliaryModify;
            viewModel.RemarksOnlyModifyPolicy = PatentAuthorizationPolicy.AuxiliaryRemarksOnly;
            viewModel.DeletePolicy = PatentAuthorizationPolicy.AuxiliaryCanDelete;
            viewModel.LimitedReadPolicy = PatentAuthorizationPolicy.AuxiliaryLimited;
        }
        public static void AddTrademarkAuxiliarySecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = TrademarkAuthorizationPolicy.AuxiliaryModify;
            viewModel.RemarksOnlyModifyPolicy = TrademarkAuthorizationPolicy.AuxiliaryRemarksOnly;
            viewModel.DeletePolicy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete;
            viewModel.LimitedReadPolicy = TrademarkAuthorizationPolicy.AuxiliaryLimited;
        }
        public static void AddReleaseAuxiliarySecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = ReleaseAuthorizationPolicy.AuxiliaryModify;
            viewModel.RemarksOnlyModifyPolicy = ReleaseAuthorizationPolicy.AuxiliaryRemarksOnly;
            viewModel.DeletePolicy = ReleaseAuthorizationPolicy.AuxiliaryCanDelete;
            viewModel.LimitedReadPolicy = ReleaseAuthorizationPolicy.AuxiliaryLimited;
        }
        public static void AddGeneralmatterAuxiliarySecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = GeneralMatterAuthorizationPolicy.AuxiliaryModify;
            viewModel.RemarksOnlyModifyPolicy = GeneralMatterAuthorizationPolicy.AuxiliaryRemarksOnly;
            viewModel.DeletePolicy = GeneralMatterAuthorizationPolicy.AuxiliaryCanDelete;
            viewModel.LimitedReadPolicy = GeneralMatterAuthorizationPolicy.AuxiliaryLimited;
        }
        public static void AddDMSAuxiliarySecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = DMSAuthorizationPolicy.AuxiliaryModify;
            viewModel.RemarksOnlyModifyPolicy = DMSAuthorizationPolicy.AuxiliaryRemarksOnly;
            viewModel.DeletePolicy = DMSAuthorizationPolicy.AuxiliaryCanDelete;
            viewModel.LimitedReadPolicy = DMSAuthorizationPolicy.AuxiliaryLimited;
        }
        public static void AddRMSAuxiliarySecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = RMSAuthorizationPolicy.AuxiliaryModify;
            viewModel.RemarksOnlyModifyPolicy = RMSAuthorizationPolicy.AuxiliaryRemarksOnly;
            viewModel.DeletePolicy = RMSAuthorizationPolicy.AuxiliaryCanDelete;
            viewModel.LimitedReadPolicy = RMSAuthorizationPolicy.AuxiliaryLimited;
        }
        public static void AddForeignFilingAuxiliarySecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = ForeignFilingAuthorizationPolicy.AuxiliaryModify;
            viewModel.RemarksOnlyModifyPolicy = ForeignFilingAuthorizationPolicy.AuxiliaryRemarksOnly;
            viewModel.DeletePolicy = ForeignFilingAuthorizationPolicy.AuxiliaryCanDelete;
            viewModel.LimitedReadPolicy = ForeignFilingAuthorizationPolicy.AuxiliaryLimited;
        }

        public static void AddPatentCountryLawSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = PatentAuthorizationPolicy.CountryLawModify;
            viewModel.RemarksOnlyModifyPolicy = PatentAuthorizationPolicy.CountryLawRemarksOnly;
            viewModel.DeletePolicy = PatentAuthorizationPolicy.CountryLawCanDelete;
            viewModel.LimitedReadPolicy = string.Empty;
        }
        public static void AddTrademarkCountryLawSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = TrademarkAuthorizationPolicy.CountryLawModify;
            viewModel.RemarksOnlyModifyPolicy = TrademarkAuthorizationPolicy.CountryLawRemarksOnly;
            viewModel.DeletePolicy = TrademarkAuthorizationPolicy.CountryLawCanDelete;
            viewModel.LimitedReadPolicy = string.Empty;
        }
        public static void AddGeneralMatterCountryLawSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = GeneralMatterAuthorizationPolicy.CountryLawModify;
            viewModel.RemarksOnlyModifyPolicy = GeneralMatterAuthorizationPolicy.CountryLawRemarksOnly;
            viewModel.DeletePolicy = GeneralMatterAuthorizationPolicy.CountryLawCanDelete;
            viewModel.LimitedReadPolicy = string.Empty;
        }

        public static void AddPatentActionTypeSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = PatentAuthorizationPolicy.ActionTypeModify;
            viewModel.RemarksOnlyModifyPolicy = PatentAuthorizationPolicy.ActionTypeRemarksOnly;
            viewModel.DeletePolicy = PatentAuthorizationPolicy.ActionTypeCanDelete;
            viewModel.LimitedReadPolicy = string.Empty;
        }
        public static void AddTrademarkActionTypeSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = TrademarkAuthorizationPolicy.ActionTypeModify;
            viewModel.RemarksOnlyModifyPolicy = TrademarkAuthorizationPolicy.ActionTypeRemarksOnly;
            viewModel.DeletePolicy = TrademarkAuthorizationPolicy.ActionTypeCanDelete;
            viewModel.LimitedReadPolicy = string.Empty;
        }
        public static void AddGeneralMatterActionTypeSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = GeneralMatterAuthorizationPolicy.ActionTypeModify;
            viewModel.RemarksOnlyModifyPolicy = GeneralMatterAuthorizationPolicy.ActionTypeRemarksOnly;
            viewModel.DeletePolicy = GeneralMatterAuthorizationPolicy.ActionTypeCanDelete;
            viewModel.LimitedReadPolicy = string.Empty;
        }

        public static void AddPatentCostEstimatorSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = PatentAuthorizationPolicy.CostEstimatorModify;
            viewModel.RemarksOnlyModifyPolicy = PatentAuthorizationPolicy.CostEstimatorRemarksOnly;
            viewModel.DeletePolicy = PatentAuthorizationPolicy.CostEstimatorCanDelete;
            viewModel.LimitedReadPolicy = string.Empty;
        }

        public static void AddTrademarkCostEstimatorSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = TrademarkAuthorizationPolicy.CostEstimatorModify;
            viewModel.RemarksOnlyModifyPolicy = TrademarkAuthorizationPolicy.CostEstimatorRemarksOnly;
            viewModel.DeletePolicy = TrademarkAuthorizationPolicy.CostEstimatorCanDelete;
            viewModel.LimitedReadPolicy = string.Empty;
        }

        public static void AddPatentGermanRemunerationSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = PatentAuthorizationPolicy.GermanRemunerationModify;
            viewModel.RemarksOnlyModifyPolicy = PatentAuthorizationPolicy.GermanRemunerationRemarksOnly;
            viewModel.DeletePolicy = PatentAuthorizationPolicy.GermanRemunerationCanDelete;
            viewModel.LimitedReadPolicy = string.Empty;
        }

        public static void AddPatentFrenchRemunerationSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = PatentAuthorizationPolicy.FrenchRemunerationModify;
            viewModel.RemarksOnlyModifyPolicy = PatentAuthorizationPolicy.FrenchRemunerationRemarksOnly;
            viewModel.DeletePolicy = PatentAuthorizationPolicy.FrenchRemunerationCanDelete;
            viewModel.LimitedReadPolicy = string.Empty;
        }

        public static void AddLettersSecurityPolicies<T>(this T viewModel, string sys) where T : DetailPagePermission
        {
            switch(sys)
            {
                case "P":
                    viewModel.FullModifyPolicy = PatentAuthorizationPolicy.LetterModify;
                    viewModel.DeletePolicy = PatentAuthorizationPolicy.LetterModify;
                    viewModel.CopyPolicy = PatentAuthorizationPolicy.LetterModify;
                    break;
                case "T":
                    viewModel.FullModifyPolicy = TrademarkAuthorizationPolicy.LetterModify;
                    viewModel.DeletePolicy = TrademarkAuthorizationPolicy.LetterModify;
                    viewModel.CopyPolicy = TrademarkAuthorizationPolicy.LetterModify;
                    break;
                case "G":
                    viewModel.FullModifyPolicy = GeneralMatterAuthorizationPolicy.LetterModify;
                    viewModel.DeletePolicy = GeneralMatterAuthorizationPolicy.LetterModify;
                    viewModel.CopyPolicy = GeneralMatterAuthorizationPolicy.LetterModify;
                    break;
            }
        }

        //TO DO: DOCX permission
        public static void AddDOCXSecurityPolicies<T>(this T viewModel, string sys) where T : DetailPagePermission
        {
            switch (sys)
            {
                case "P":
                    viewModel.FullModifyPolicy = PatentAuthorizationPolicy.LetterModify;
                    viewModel.DeletePolicy = PatentAuthorizationPolicy.LetterModify;
                    viewModel.CopyPolicy = PatentAuthorizationPolicy.LetterModify;
                    break;
                    //case "T":
                    //    viewModel.FullModifyPolicy = TrademarkAuthorizationPolicy.LetterModify;
                    //    viewModel.DeletePolicy = TrademarkAuthorizationPolicy.LetterModify;
                    //    viewModel.CopyPolicy = TrademarkAuthorizationPolicy.LetterModify;
                    //    break;
                    //case "G":
                    //    viewModel.FullModifyPolicy = GeneralMatterAuthorizationPolicy.LetterModify;
                    //    viewModel.DeletePolicy = GeneralMatterAuthorizationPolicy.LetterModify;
                    //    viewModel.CopyPolicy = GeneralMatterAuthorizationPolicy.LetterModify;
                    //    break;
            }
        }

        public static void AddDataQuerySecurityPolicies<T>(this T viewModel, bool isPatentModify, bool isTrademarkModify, bool isGenMatterModify, bool isAMSModify) where T : DetailPagePermission
        {
            if (isPatentModify)
            { 
                viewModel.FullModifyPolicy = PatentAuthorizationPolicy.CustomQueryModify;
                viewModel.DeletePolicy = PatentAuthorizationPolicy.CustomQueryModify;
                viewModel.CopyPolicy = PatentAuthorizationPolicy.CustomQueryModify;
            }
            else if (isTrademarkModify)
            {
                viewModel.FullModifyPolicy = TrademarkAuthorizationPolicy.CustomQueryModify;
                viewModel.DeletePolicy = TrademarkAuthorizationPolicy.CustomQueryModify;
                viewModel.CopyPolicy = TrademarkAuthorizationPolicy.CustomQueryModify;
            }
            else if (isGenMatterModify)
            {
                viewModel.FullModifyPolicy = GeneralMatterAuthorizationPolicy.CustomQueryModify;
                viewModel.DeletePolicy = GeneralMatterAuthorizationPolicy.CustomQueryModify;
                viewModel.CopyPolicy = GeneralMatterAuthorizationPolicy.CustomQueryModify;
            }
            else if (isAMSModify)
            {
                viewModel.FullModifyPolicy = AMSAuthorizationPolicy.CustomQueryModify;
                viewModel.DeletePolicy = AMSAuthorizationPolicy.CustomQueryModify;
                viewModel.CopyPolicy = AMSAuthorizationPolicy.CustomQueryModify;
            }
        }

        public static void AddPatentCostTrackingSecurityPolicies<T>(this T viewModel, bool hasRespOfficeFilter = false) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = hasRespOfficeFilter ? PatentAuthorizationPolicy.CostTrackingModifyByRespOffice : PatentAuthorizationPolicy.CostTrackingModify;
            viewModel.RemarksOnlyModifyPolicy = hasRespOfficeFilter ? PatentAuthorizationPolicy.RemarksOnlyModifyByRespOffice : PatentAuthorizationPolicy.RemarksOnlyModify;
            viewModel.DeletePolicy = hasRespOfficeFilter ? PatentAuthorizationPolicy.CostTrackingDeleteByRespOffice : PatentAuthorizationPolicy.CostTrackingDelete;
            viewModel.LimitedReadPolicy = hasRespOfficeFilter ? PatentAuthorizationPolicy.LimitedReadByRespOffice : PatentAuthorizationPolicy.LimitedRead;
            viewModel.CanUploadDocumentsPolicy = hasRespOfficeFilter ? PatentAuthorizationPolicy.CostTrackingModifyByRespOffice : PatentAuthorizationPolicy.CostTrackingUpload;
        }

        public static void AddTrademarkCostTrackingSecurityPolicies<T>(this T viewModel, bool hasRespOfficeFilter = false) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = hasRespOfficeFilter ? TrademarkAuthorizationPolicy.CostTrackingModifyByRespOffice : TrademarkAuthorizationPolicy.CostTrackingModify;
            viewModel.RemarksOnlyModifyPolicy = hasRespOfficeFilter ? TrademarkAuthorizationPolicy.RemarksOnlyModifyByRespOffice : TrademarkAuthorizationPolicy.RemarksOnlyModify;
            viewModel.DeletePolicy = hasRespOfficeFilter ? TrademarkAuthorizationPolicy.CostTrackingDeleteByRespOffice : TrademarkAuthorizationPolicy.CostTrackingDelete;
            viewModel.LimitedReadPolicy = hasRespOfficeFilter ? TrademarkAuthorizationPolicy.LimitedReadByRespOffice : TrademarkAuthorizationPolicy.LimitedRead;
            viewModel.CanUploadDocumentsPolicy = hasRespOfficeFilter ? TrademarkAuthorizationPolicy.CostTrackingModifyByRespOffice : TrademarkAuthorizationPolicy.CostTrackingUpload;
        }

        public static void AddGeneralMatterCostTrackingSecurityPolicies<T>(this T viewModel, bool hasRespOfficeFilter = false) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = hasRespOfficeFilter ? GeneralMatterAuthorizationPolicy.CostTrackingModifyByRespOffice : GeneralMatterAuthorizationPolicy.CostTrackingModify;
            viewModel.RemarksOnlyModifyPolicy = hasRespOfficeFilter ? GeneralMatterAuthorizationPolicy.RemarksOnlyModifyByRespOffice : GeneralMatterAuthorizationPolicy.RemarksOnlyModify;
            viewModel.DeletePolicy = hasRespOfficeFilter ? GeneralMatterAuthorizationPolicy.CostTrackingDeleteByRespOffice : GeneralMatterAuthorizationPolicy.CostTrackingDelete;
            viewModel.LimitedReadPolicy = hasRespOfficeFilter ? GeneralMatterAuthorizationPolicy.LimitedReadByRespOffice : GeneralMatterAuthorizationPolicy.LimitedRead;
            viewModel.CanUploadDocumentsPolicy = hasRespOfficeFilter ? GeneralMatterAuthorizationPolicy.CostTrackingModifyByRespOffice : GeneralMatterAuthorizationPolicy.CostTrackingUpload;
        }

        public static void AddPatentWorkflowSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = PatentAuthorizationPolicy.WorkflowModify;
            viewModel.DeletePolicy = PatentAuthorizationPolicy.WorkflowModify;
            viewModel.CopyPolicy = PatentAuthorizationPolicy.WorkflowModify;
        }

        public static void AddTrademarkWorkflowSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = TrademarkAuthorizationPolicy.WorkflowModify;
            viewModel.DeletePolicy = TrademarkAuthorizationPolicy.WorkflowModify;
            viewModel.CopyPolicy = TrademarkAuthorizationPolicy.WorkflowModify;
        }

        public static void AddGeneralMatterWorkflowSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = GeneralMatterAuthorizationPolicy.WorkflowModify;
            viewModel.DeletePolicy = GeneralMatterAuthorizationPolicy.WorkflowModify;
            viewModel.CopyPolicy = GeneralMatterAuthorizationPolicy.WorkflowModify;
        }

        public static void AddDMSWorkflowSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = DMSAuthorizationPolicy.WorkflowModify;
            viewModel.DeletePolicy = DMSAuthorizationPolicy.WorkflowModify;
            viewModel.CopyPolicy = DMSAuthorizationPolicy.WorkflowModify;
        }

        public static void AddPatClearanceWorkflowSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = PatentClearanceAuthorizationPolicy.WorkflowModify;
            viewModel.DeletePolicy = PatentClearanceAuthorizationPolicy.WorkflowModify;
            viewModel.CopyPolicy = PatentClearanceAuthorizationPolicy.WorkflowModify;
        }

        public static void AddClearanceWorkflowSecurityPolicies<T>(this T viewModel) where T : DetailPagePermission
        {
            viewModel.FullModifyPolicy = SearchRequestAuthorizationPolicy.WorkflowModify;
            viewModel.DeletePolicy = SearchRequestAuthorizationPolicy.WorkflowModify;
            viewModel.CopyPolicy = SearchRequestAuthorizationPolicy.WorkflowModify;
        }


        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> data,
                  IList<SortDescriptor> sortDescriptors)
        {
            if (sortDescriptors != null && sortDescriptors.Any())
            {
                foreach (SortDescriptor sortDescriptor in sortDescriptors)
                {
                    data = AddSortExpression(data, sortDescriptor.SortDirection, sortDescriptor.Member);
                }
            }
            return data;
        }

        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> data,
                int page, int pageSize)
        {
            data = data.Skip((page - 1) * pageSize);
            data = data.Take(pageSize);
            return data;
        }

        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> data,
                int page, int pageSize, int maxPageSize)
        {
            page = page == 0 ? 1 : page;
            pageSize = pageSize == 0 || pageSize > maxPageSize ? maxPageSize : pageSize;

            data = data.Skip((page - 1) * pageSize);
            data = data.Take(pageSize);
            return data;
        }

        public static IEnumerable Errors(this ModelStateDictionary modelState)
        {
            if (!modelState.IsValid)
            {
                JavaScriptEncoder jsEncoder = JavaScriptEncoder.Default; //mitigate xss 
                return modelState.ToDictionary(kvp => kvp.Key,
                    kvp => kvp.Value.Errors
                                    .Select(e => jsEncoder.Encode(e.ErrorMessage)).ToArray())
                                    .Where(m => m.Value.Count() > 0);
            }
            return null;
        }

        private static IQueryable<T> AddSortExpression<T>(IQueryable<T> data, ListSortDirection
              sortDirection, string memberName)
        {
            //var orderByExpression = ExpressionHelper.GetPropertyExpression<T>(memberName);
            //if (orderByExpression != null)
            //{
            //    if (sortDirection == ListSortDirection.Ascending)
            //        data = data.OrderBy(orderByExpression);
            //    else
            //        data = data.OrderByDescending(orderByExpression);
            //}
            //return data;

            //FIX ISSUE WHEN QUERY IS USING DISTINCT
            return data.OrderByDynamic(memberName, (sortDirection == ListSortDirection.Descending));
        }

        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string sortColumn, bool descending)
        {
            // Dynamically creates a call like this: query.OrderBy(p => p.SortColumn)
            var parameter = Expression.Parameter(typeof(T), "p");

            string command = "OrderBy";

            if (descending)
            {
                command = "OrderByDescending";
            }

            Expression resultExpression = null;

            var property = typeof(T).GetProperty(sortColumn);
            // this is the part p.SortColumn
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);

            // this is the part p => p.SortColumn
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            // finally, call the "OrderBy" / "OrderByDescending" method with the order by lamba expression
            resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { typeof(T), property.PropertyType },
               query.Expression, Expression.Quote(orderByExpression));

            return query.Provider.CreateQuery<T>(resultExpression);
        }

        public static string FormatToDisplay(this DateTime? date)
        {
            return date?.ToString("dd-MMM-yyyy");
        }

        public static string FormatToDisplay(this DateTime date)
        {
            return date.ToString("dd-MMM-yyyy");
        }

        public static string FormatToDisplay(this DateTime? date, string culture)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture(culture);
            return date?.ToString("dd-MMM-yyyy",cultureInfo);
        }

        public static string FormatToDisplayWithTime(this DateTime? date)
        {
            return date?.ToString("dd-MMM-yyyy hh:mm tt");
        }

        public static string FormatToSave(this DateTime? date)
        {
            return date?.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        public static string FormatToDisplay(this Decimal value)
        {
            return value.ToString("N2");
        }

        public static string FormatToDisplay(this Decimal? value)
        {
            if (value == null)
                return String.Empty;

            return Convert.ToDecimal(value).ToString("N2");
        }

        public static string FormatToDisplay(this Decimal value, string culture)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture(culture);
            return value.ToString("N2", cultureInfo);
        }

        public static string FormatToDisplay(this double value, string culture)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture(culture);
            return value.ToString("N2", cultureInfo);
        }

        public static double RoundTo2ndDecimals(this double value)
        {
            return Math.Round(value, 2);
        }
        public static double? RoundTo2ndDecimals(this double? value)
        {
            if (value == null)
                return value;
            return Math.Round((double)value, 2);
        }

        //public static string Humanize(this string input)
        //{
        //    if (!string.IsNullOrEmpty(input))
        //    {
        //        return Regex.Replace(
        //       input,
        //       "(?<!^)" +
        //       "(" +
        //       "  [A-Z][a-z] |" +
        //       "  (?<=[a-z])[A-Z] |" +
        //       "  (?<![A-Z])[A-Z]$" +
        //       ")",
        //       " $1",
        //       RegexOptions.IgnorePatternWhitespace);
        //    }
        //    return string.Empty;
        //}

        public static string[] GetKendoDateParseFormats(this HttpContext context) {
            var rqf = context.Features.Get<IRequestCultureFeature>();
            var culture = rqf.RequestCulture.Culture;
            var dateTimeFormat = culture.DateTimeFormat;
            var dateSeparator = dateTimeFormat.DateSeparator;
            var dateFormat = dateTimeFormat.ShortDatePattern.Split(dateSeparator);

            List<string> parseFormats = new List<string>();
            parseFormats.Add("{0:dd-MMM-yyyy}");
            parseFormats.Add("d");
            if (dateFormat[0].Substring(0, 1).ToLower() == "m")
            {
                parseFormats.Add("M d yyyy");
                parseFormats.Add("M/d/yyyy");
                parseFormats.Add("M-d-yyyy");
                parseFormats.Add("M.d.yyyy");
                parseFormats.Add("MMMM d, yyyy");
                parseFormats.Add("MMMM d yyyy");
                parseFormats.Add("d MMMM, yyyy");
                parseFormats.Add("d MMMM yyyy");
                parseFormats.Add("MMM d, yyyy");
                parseFormats.Add("MMM. d, yyyy");
                parseFormats.Add("M d yy");
                parseFormats.Add("M/d/yy");
                parseFormats.Add("M-d-yy");
                parseFormats.Add("M.d.yy");
                parseFormats.Add("M d");
                parseFormats.Add("M/d");
                parseFormats.Add("M-d");
                parseFormats.Add("M.d");
                parseFormats.Add("yyyy-M-d");
            }
            else if (dateFormat[0].Substring(0, 1).ToLower() == "d")
            {
                parseFormats.Add("d M yyyy");
                parseFormats.Add("d/M/yyyy");
                parseFormats.Add("d-M-yyyy");
                parseFormats.Add("d.M.yyyy");
                parseFormats.Add("d M yy");
                parseFormats.Add("d/M/yy");
                parseFormats.Add("d-M-yy");
                parseFormats.Add("d.M.yy");
                parseFormats.Add("d M");
                parseFormats.Add("d/M");
                parseFormats.Add("d-M");
                parseFormats.Add("d.M");
                parseFormats.Add("yyyy-d-M");
            }
            return parseFormats.ToArray();
        }

        public static string ToSize(this int bytes)
        {
            var sizes = new string[] { "Bytes", "KB", "MB", "GB", "TB" };
            if (bytes == 0) return "0 Byte";
            var i = Convert.ToInt32(Math.Floor(Math.Log(bytes) / Math.Log(1024))); 
            return $"{Math.Round(bytes / Math.Pow(1024, i), 2)} {sizes[i]}"; 
        }

        //equivalent to SQL Like (example 'abc%')
        public static bool Like(this string toSearch, string toFind)
        {
            return new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(toFind, ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z", RegexOptions.Singleline).IsMatch(toSearch);
        }

        /// <summary>
        /// Split comma or semi-colon delimited list of email addresses
        /// </summary>
        /// <param name="emails"></param>
        /// <returns></returns>
        public static List<MailAddress> SplitEmails(this string? emails)
        {
            var mailAddresses = new List<MailAddress>();

            if (!string.IsNullOrEmpty(emails))
            {
                foreach (var email in emails.Replace(";", ",").Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    mailAddresses.Add(new MailAddress(email));
                }
            }

            return mailAddresses;
        }

        public static FileContentResult ToJsonFileContentResult(this NJsonSchema.JsonSchema schema)
        {
            return new FileContentResult(System.Text.Encoding.ASCII.GetBytes(schema.ToJson()), "application/json");
        }
    }
}
