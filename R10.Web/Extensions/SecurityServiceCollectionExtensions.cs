using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Infrastructure.Identity;
using R10.Web.Security;
using R10.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Extensions
{
    public static class SecurityServiceCollectionExtensions
    {
        public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
        {

            //authorization
            services.AddAuthorization(options =>
            {
                //Administrator
                options.AddPolicy(CPiAuthorizationPolicy.Administrator, policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsAdmin()));

                options.AddPolicy(CPiAuthorizationPolicy.CPiAdmin, policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsSuper()));

                options.AddPolicy(CPiAuthorizationPolicy.SystemUser, policy =>
                    policy.RequireAssertion(context =>
                        context.User.GetSystems().Any()));

                //Task Scheduler Service Account
                var serviceAccount = configuration.GetSection("ServiceAccount").Get<ServiceAccount>();
                options.AddPolicy(
                    CPiAuthorizationPolicy.ScheduledTask,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiServiceAccountRequirement(serviceAccount?.UserName, typeof(ScheduledTaskService).Name)));

                // AMS
                options.AddPolicy(
                    AMSAuthorizationPolicy.RegularUser,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.RegularUser)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.CanAccessSystem,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.FullModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.RemarksOnlyModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.CanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.LimitedRead,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.LimitedRead, SystemStatusType.Active, true)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.FullRead,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.FullRead, SystemStatusType.Active)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.DecisionMaker,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.DecisionMaker, SystemStatusType.Active)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.FullModifyByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.AMS, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                   AMSAuthorizationPolicy.RemarksOnlyModifyByRespOffice,
                   policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.AMS, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                   AMSAuthorizationPolicy.CanDeleteByRespOffice,
                   policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.AMS, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                  AMSAuthorizationPolicy.LimitedReadByRespOffice,
                  policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.AMS, CPiPermissions.LimitedRead, SystemStatusType.Active, true)));
                options.AddPolicy(
                  AMSAuthorizationPolicy.FullReadByRespOffice,
                  policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.AMS, CPiPermissions.FullRead, SystemStatusType.Active)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.DecisionMakerByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.AMS, CPiPermissions.DecisionMaker, SystemStatusType.Active)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.CanAccessPatent,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent)));

                //AMS Settings
                options.AddPolicy(AMSAuthorizationPolicy.IsStandalone, policy =>
                    policy.RequireAssertion(context =>
                        !context.User.IsAMSIntegrated()));
                options.AddPolicy(
                    AMSAuthorizationPolicy.IsCorporation,
                    policyBuilder => policyBuilder.AddRequirements(
                        new SettingPermissionRequirement("AMS", "IsCorporation"),
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.RegularUser)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.IsLawFirm,
                    policyBuilder => policyBuilder.AddRequirements(
                        new SettingPermissionRequirement("AMS", "IsCorporation", false),
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.RegularUser)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.FullModifyLawFirm,
                    policyBuilder => policyBuilder.AddRequirements(
                        new SettingPermissionRequirement("AMS", "IsCorporation", false),
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.FullModify)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.CanAccessFeeSetup,
                    policyBuilder => policyBuilder.AddRequirements(
                        new SettingPermissionRequirement("AMS", "HasServiceFee"),
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.Auxiliary)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.CanAccessVATRateSetup,
                    policyBuilder => policyBuilder.AddRequirements(
                        new SettingPermissionRequirement("AMS", "HasVAT"),
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.Auxiliary)));

                //Settings
                options.AddPolicy(
                    SharedAuthorizationPolicy.IsCorporation,
                    policyBuilder => policyBuilder.AddRequirements(
                        new SettingPermissionRequirement("General", "IsCorporation")));
                options.AddPolicy(
                    SharedAuthorizationPolicy.IsLawFirm,
                    policyBuilder => policyBuilder.AddRequirements(
                        new SettingPermissionRequirement("General", "IsCorporation", false)));

                // Trademark
                options.AddPolicy(
                   TrademarkAuthorizationPolicy.CanAccessMainMenu, policy =>
                   policy.RequireAssertion(context =>
                       context.User.IsInSystem(SystemType.Trademark) && (context.User.IsInRoles(SystemType.Trademark, CPiPermissions.CanAccessSystem) || context.User.IsInRoles(SystemType.Trademark, CPiPermissions.CostEstimator))));
                options.AddPolicy(
                     TrademarkAuthorizationPolicy.CanAccessSystem,
                     policyBuilder => policyBuilder.AddRequirements(
                         new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.CanAccessSystem)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.FullModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.RemarksOnlyModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.LimitedRead,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.LimitedRead, SystemStatusType.Active, true)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.FullRead,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.FullRead, SystemStatusType.Active)));
                //options.AddPolicy(
                //    TrademarkAuthorizationPolicy.CanUploadDocuments,
                //    policyBuilder => policyBuilder.AddRequirements(
                //        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.CanUploadDocuments, SystemStatusType.Active)));
                //Allow RMS Decision Makers to upload attachments from RMS Portfolio Review screen
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanUploadDocuments, policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c=> c.Type==CPiClaimTypes.UseOutlookAddIn && Convert.ToBoolean(c.Value)) ||
                        context.User.IsInRoles(SystemType.Trademark, CPiPermissions.CanUploadDocuments) ||
                        context.User.IsInRoles(SystemType.RMS, CPiPermissions.DecisionMaker)));
                options.AddPolicy(
                   TrademarkAuthorizationPolicy.FullModifyByRespOffice,
                   policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.Trademark, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                   TrademarkAuthorizationPolicy.RemarksOnlyModifyByRespOffice,
                   policyBuilder => policyBuilder.AddRequirements(
                       new CPiRespOfficePermissionRequirement(SystemType.Trademark, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                   TrademarkAuthorizationPolicy.CanDeleteByRespOffice,
                   policyBuilder => policyBuilder.AddRequirements(
                       new CPiRespOfficePermissionRequirement(SystemType.Trademark, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                  TrademarkAuthorizationPolicy.LimitedReadByRespOffice,
                  policyBuilder => policyBuilder.AddRequirements(
                      new CPiRespOfficePermissionRequirement(SystemType.Trademark, CPiPermissions.LimitedRead, SystemStatusType.Active, true)));
                options.AddPolicy(
                  TrademarkAuthorizationPolicy.FullReadByRespOffice,
                  policyBuilder => policyBuilder.AddRequirements(
                      new CPiRespOfficePermissionRequirement(SystemType.Trademark, CPiPermissions.FullRead, SystemStatusType.Active)));

                // Clearance
                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.RegularUser,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.SearchRequest, CPiPermissions.RegularUser)));
                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.CanAccessSystem,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.SearchRequest)));
                options.AddPolicy(
                  SearchRequestAuthorizationPolicy.FullModify,
                  policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.SearchRequest, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.RemarksOnlyModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.SearchRequest, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.CanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.SearchRequest, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.Reviewer,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.SearchRequest, CPiPermissions.Reviewer, SystemStatusType.Active)));
                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.CanAccessReview,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.SearchRequest, CPiPermissions.CanAccessReview, SystemStatusType.Active)));
                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.CanAddClearance,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.SearchRequest, CPiPermissions.CanAddClearance, SystemStatusType.Active)));

                // General Matter
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanAccessSystem,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.CanAccessSystem)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.FullModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.RemarksOnlyModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.LimitedRead,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.LimitedRead, SystemStatusType.Active, true)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.FullRead,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.FullRead, SystemStatusType.Active)));

                options.AddPolicy(
                   GeneralMatterAuthorizationPolicy.CanUploadDocuments, policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c => c.Type == CPiClaimTypes.UseOutlookAddIn && Convert.ToBoolean(c.Value)) ||
                       context.User.IsInRoles(SystemType.GeneralMatter, CPiPermissions.CanUploadDocuments)));

                options.AddPolicy(
                   GeneralMatterAuthorizationPolicy.FullModifyByRespOffice,
                   policyBuilder => policyBuilder.AddRequirements(
                       new CPiRespOfficePermissionRequirement(SystemType.GeneralMatter, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                   GeneralMatterAuthorizationPolicy.RemarksOnlyModifyByRespOffice,
                   policyBuilder => policyBuilder.AddRequirements(
                       new CPiRespOfficePermissionRequirement(SystemType.GeneralMatter, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                   GeneralMatterAuthorizationPolicy.CanDeleteByRespOffice,
                   policyBuilder => policyBuilder.AddRequirements(
                       new CPiRespOfficePermissionRequirement(SystemType.GeneralMatter, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                  GeneralMatterAuthorizationPolicy.LimitedReadByRespOffice,
                  policyBuilder => policyBuilder.AddRequirements(
                      new CPiRespOfficePermissionRequirement(SystemType.GeneralMatter, CPiPermissions.LimitedRead, SystemStatusType.Active, true)));
                options.AddPolicy(
                  GeneralMatterAuthorizationPolicy.FullReadByRespOffice,
                  policyBuilder => policyBuilder.AddRequirements(
                      new CPiRespOfficePermissionRequirement(SystemType.GeneralMatter, CPiPermissions.FullRead, SystemStatusType.Active)));

                // DMS
                options.AddPolicy(
                    DMSAuthorizationPolicy.RegularUser,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.RegularUser)));
                options.AddPolicy(
                    DMSAuthorizationPolicy.CanAccessSystem,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.DMS)));
                //TODO: USE DIFFERENT HANDLER?
                //      FULL MODIFY AUTH HANDLER INCLUDES MODIFY ROLE
                //      FULL MODIFY AUTH HANDLER DOES NOT INCLUDE INVENTOR ROLE
                options.AddPolicy(
                  DMSAuthorizationPolicy.FullModify,
                  policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.FullModify, SystemStatusType.Active)));
                //TODO: NOT NEEDED? REMOVE? 
                options.AddPolicy(
                    DMSAuthorizationPolicy.RemarksOnlyModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                //TODO: USE DIFFERENT HANDLER?
                //      DELETE AUTH HANDLER INCLUDES MODIFY ROLE
                //      DELETE AUTH HANDLER DOES NOT INCLUDE INVENTOR ROLE
                options.AddPolicy(
                    DMSAuthorizationPolicy.CanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    DMSAuthorizationPolicy.Reviewer,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.Reviewer, SystemStatusType.Active)));
                options.AddPolicy(
                   DMSAuthorizationPolicy.Inventor,
                   policyBuilder => policyBuilder.AddRequirements(
                       new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.Inventor, SystemStatusType.Active)));
                options.AddPolicy(
                    DMSAuthorizationPolicy.CanAccessReview,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.CanAccessReview, SystemStatusType.Active)));                
                options.AddPolicy(
                   DMSAuthorizationPolicy.CanUploadDocuments, policy =>
                   policy.RequireAssertion(context =>
                       context.User.HasClaim(c => c.Type == CPiClaimTypes.UseOutlookAddIn && Convert.ToBoolean(c.Value)) ||
                       context.User.IsInRoles(SystemType.DMS, CPiPermissions.CanUploadDocuments)));
                options.AddPolicy(
                    DMSAuthorizationPolicy.CanAddDisclosure,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.CanAddDisclosure, SystemStatusType.Active)));
                options.AddPolicy(
                    DMSAuthorizationPolicy.CanAccessPreview,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.DMSPreview)));               
                options.AddPolicy(
                    DMSAuthorizationPolicy.CanAccessPreview,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.DMSPreview, SystemType.DMS, CPiPermissions.CanAccessPreview)));
                options.AddPolicy(
                    DMSAuthorizationPolicy.Previewer,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.DMSPreview, SystemType.DMS, CPiPermissions.Previewer, SystemStatusType.Active)));


                // Patent
                options.AddPolicy(
                   PatentAuthorizationPolicy.CanAccessMainMenu, policy =>
                   policy.RequireAssertion(context =>
                       context.User.IsInSystem(SystemType.Patent) && (context.User.IsInRoles(SystemType.Patent, CPiPermissions.CanAccessSystem) || context.User.IsInRoles(SystemType.Patent, CPiPermissions.CostEstimator))));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessSystem,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.CanAccessSystem)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.FullModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.RemarksOnlyModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.LimitedRead,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.LimitedRead, SystemStatusType.Active, true)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.FullRead,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.FullRead, SystemStatusType.Active)));

                //options.AddPolicy(
                //   PatentAuthorizationPolicy.CanUploadDocuments, policy =>
                //   policy.RequireAssertion(context =>
                //       context.User.HasClaim(c => c.Type == CPiClaimTypes.UseOutlookAddIn && Convert.ToBoolean(c.Value)) ||
                //       context.User.IsInRoles(SystemType.Patent, CPiPermissions.CanUploadDocuments)));
                //Allow Foreign Filing Decision Makers to upload attachments from Foreign Filing Portfolio Review screen
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanUploadDocuments, policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == CPiClaimTypes.UseOutlookAddIn && Convert.ToBoolean(c.Value)) ||
                        context.User.IsInRoles(SystemType.Patent, CPiPermissions.CanUploadDocuments) ||
                        context.User.IsInRoles(SystemType.ForeignFiling, CPiPermissions.DecisionMaker)));

                options.AddPolicy(
                    PatentAuthorizationPolicy.FullModifyByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.Patent, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.RemarksOnlyModifyByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.Patent, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanDeleteByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.Patent, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.LimitedReadByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.Patent, CPiPermissions.LimitedRead, SystemStatusType.Active, true)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.FullReadByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.Patent, CPiPermissions.FullRead, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.PatentScoreModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.PatentScoreModify, SystemStatusType.Active)));
                options.AddPolicy(
                   PatentAuthorizationPolicy.CanAccessPatentScore, policy =>
                   policy.RequireAssertion(context =>
                       context.User.IsInRoles(SystemType.Patent, CPiPermissions.RegularUser) ||
                       context.User.IsInRoles(SystemType.AMS, CPiPermissions.DecisionMaker)));

                //IDS
                options.AddPolicy(
                    IDSAuthorizationPolicy.CanAccessSystem,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.IDS)));
                options.AddPolicy(
                    IDSAuthorizationPolicy.FullModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.IDS, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                    IDSAuthorizationPolicy.FullModifyByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.IDS, CPiPermissions.FullModify, SystemStatusType.Active)));

                // Patent Clearance Search
                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.RegularUser,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.PatClearance, CPiPermissions.RegularUser)));
                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.CanAccessSystem,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.PatClearance)));
                options.AddPolicy(
                  PatentClearanceAuthorizationPolicy.FullModify,
                  policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.PatClearance, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.RemarksOnlyModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.PatClearance, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.CanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.PatClearance, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.Reviewer,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.PatClearance, CPiPermissions.Reviewer, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.CanAccessReview,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.PatClearance, CPiPermissions.CanAccessReview, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.CanAddClearance,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.PatClearance, CPiPermissions.CanAddClearance, SystemStatusType.Active)));

                // Shared
                options.AddPolicy(
                   SharedAuthorizationPolicy.CanAccessSystem,
                   policyBuilder => policyBuilder.AddRequirements(
                       new CPiPermissionRequirement(SystemType.Shared)));
                options.AddPolicy(
                    SharedAuthorizationPolicy.FullModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Shared, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                    SharedAuthorizationPolicy.RemarksOnlyModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Shared, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    SharedAuthorizationPolicy.CanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Shared, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                   SharedAuthorizationPolicy.LimitedRead,
                   policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Shared, CPiPermissions.LimitedRead, SystemStatusType.Active, true)));
                options.AddPolicy(
                   SharedAuthorizationPolicy.FullRead,
                   policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Shared, CPiPermissions.FullRead, SystemStatusType.Active)));
                // Decision Maker
                options.AddPolicy(
                    SharedAuthorizationPolicy.DecisionMaker, policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRoles(new List<string>() { "decisionmaker", "reviewer", "costestimator" }))); //use CPiPermissions.DecisionMaker to allow modify (Pat/Tmk/AMS/DMS)

                // Letters
                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessLetters, policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRoles(CPiPermissions.Letters)));

                options.AddPolicy(
                SharedAuthorizationPolicy.CanUploadDocuments, policy =>
                    policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == CPiClaimTypes.UseOutlookAddIn && Convert.ToBoolean(c.Value)) ||
                    context.User.IsInRoles(SystemType.Shared, CPiPermissions.CanUploadDocuments)));

                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessLetters,
                    policyBuilder => policyBuilder.AddRequirements(
                       new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.Letters)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessLettersSetup, policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRoles(CPiPermissions.Letters) &&
                        context.User.IsInRoles(SystemType.Patent, CPiPermissions.FullRead)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.LetterModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.LetterModify, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessLetters,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.Letters)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessLettersSetup, policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRoles(CPiPermissions.Letters) &&
                        context.User.IsInRoles(SystemType.Trademark, CPiPermissions.FullRead)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.LetterModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.LetterModify, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanAccessLetters,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.Letters)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanAccessLettersSetup, policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRoles(CPiPermissions.Letters) &&
                        context.User.IsInRoles(SystemType.GeneralMatter, CPiPermissions.FullRead)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.LetterModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.LetterModify, SystemStatusType.Active)));

                // Custom Query
                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessCustomQuery, policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRoles(CPiPermissions.CustomQuery)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessCustomQuery,
                    policyBuilder => policyBuilder.AddRequirements(
                       new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.CustomQuery)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CustomQueryModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.CustomQueryModify, SystemStatusType.Active)));

                options.AddPolicy(
                    AMSAuthorizationPolicy.CanAccessCustomQuery,
                    policyBuilder => policyBuilder.AddRequirements(
                       new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.CustomQuery)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.CustomQueryModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.CustomQueryModify, SystemStatusType.Active)));

                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessCustomQuery,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.CustomQuery)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CustomQueryModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.CustomQueryModify, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanAccessCustomQuery,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.CustomQuery)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CustomQueryModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.CustomQueryModify, SystemStatusType.Active)));

                // Auxiliary
                options.AddPolicy(
                    AMSAuthorizationPolicy.CanAccessAuxiliary,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.Auxiliary)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.AuxiliaryModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.AuxiliaryModify, SystemStatusType.Active)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.AuxiliaryRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.AuxiliaryRemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    DMSAuthorizationPolicy.CanAccessAuxiliary,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.Auxiliary)));
                options.AddPolicy(
                    DMSAuthorizationPolicy.AuxiliaryModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.AuxiliaryModify, SystemStatusType.Active)));
                options.AddPolicy(
                    DMSAuthorizationPolicy.AuxiliaryRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.AuxiliaryRemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessAuxiliary,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.Auxiliary)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.AuxiliaryRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.AuxiliaryRemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.AuxiliaryModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.AuxiliaryModify, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessAuxiliary,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.Auxiliary)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.AuxiliaryRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.AuxiliaryRemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.AuxiliaryModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.AuxiliaryModify, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanAccessAuxiliary,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.Auxiliary)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.AuxiliaryRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.AuxiliaryRemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.AuxiliaryModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.AuxiliaryModify, SystemStatusType.Active)));
                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.CanAccessAuxiliary,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.SearchRequest, CPiPermissions.Auxiliary)));
                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.AuxiliaryModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.SearchRequest, CPiPermissions.AuxiliaryModify, SystemStatusType.Active)));
                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.AuxiliaryRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.SearchRequest, CPiPermissions.AuxiliaryRemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.CanAccessAuxiliary,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.PatClearance, CPiPermissions.Auxiliary)));
                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.AuxiliaryModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.PatClearance, CPiPermissions.AuxiliaryModify, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.AuxiliaryRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.PatClearance, CPiPermissions.AuxiliaryRemarksOnly, SystemStatusType.Active)));
                //rms auxiliary
                options.AddPolicy(
                    RMSAuthorizationPolicy.CanAccessAuxiliary,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.RMS, CPiPermissions.Auxiliary)));
                options.AddPolicy(
                    RMSAuthorizationPolicy.AuxiliaryModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.RMS, CPiPermissions.AuxiliaryModify, SystemStatusType.Active)));
                options.AddPolicy(
                    RMSAuthorizationPolicy.AuxiliaryRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.RMS, CPiPermissions.AuxiliaryRemarksOnly, SystemStatusType.Active)));
                //foreign filing auxiliary
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.CanAccessAuxiliary,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.ForeignFiling, CPiPermissions.Auxiliary)));
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.AuxiliaryModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.ForeignFiling, CPiPermissions.AuxiliaryModify, SystemStatusType.Active)));
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.AuxiliaryRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.ForeignFiling, CPiPermissions.AuxiliaryRemarksOnly, SystemStatusType.Active)));
                //limited auxiliary
                options.AddPolicy(
                    AMSAuthorizationPolicy.AuxiliaryLimited,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.AuxiliaryLimited, SystemStatusType.Active, true)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.AuxiliaryLimited,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.AuxiliaryLimited, SystemStatusType.Active, true)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.AuxiliaryLimited,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.AuxiliaryLimited, SystemStatusType.Active, true)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.AuxiliaryLimited,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.AuxiliaryLimited, SystemStatusType.Active, true)));
                options.AddPolicy(
                    DMSAuthorizationPolicy.AuxiliaryLimited,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.AuxiliaryLimited, SystemStatusType.Active, true)));
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.AuxiliaryLimited,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.ForeignFiling, CPiPermissions.AuxiliaryLimited, SystemStatusType.Active, true)));
                options.AddPolicy(
                    RMSAuthorizationPolicy.AuxiliaryLimited,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.RMS, CPiPermissions.AuxiliaryLimited, SystemStatusType.Active, true)));
                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.AuxiliaryLimited,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.SearchRequest, CPiPermissions.AuxiliaryLimited, SystemStatusType.Active, true)));
                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.AuxiliaryLimited,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.PatClearance, CPiPermissions.AuxiliaryLimited, SystemStatusType.Active, true)));
                //can delete auxiliary
                options.AddPolicy(
                    AMSAuthorizationPolicy.AuxiliaryCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.AuxiliaryCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.AuxiliaryCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.AuxiliaryCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.AuxiliaryCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.AuxiliaryCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.AuxiliaryCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.AuxiliaryCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    DMSAuthorizationPolicy.AuxiliaryCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.AuxiliaryCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.AuxiliaryCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.ForeignFiling, CPiPermissions.AuxiliaryCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    RMSAuthorizationPolicy.AuxiliaryCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.RMS, CPiPermissions.AuxiliaryCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.AuxiliaryCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.SearchRequest, CPiPermissions.AuxiliaryCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.AuxiliaryCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.PatClearance, CPiPermissions.AuxiliaryCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    RMSAuthorizationPolicy.AuxiliaryCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.RMS, CPiPermissions.AuxiliaryCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.AuxiliaryCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.ForeignFiling, CPiPermissions.AuxiliaryCanDelete, SystemStatusType.Active)));

                // Country Law
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessCountryLaw,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.CountryLaw)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CountryLawRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.CountryLawRemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CountryLawModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.CountryLawModify, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessCountryLaw,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.CountryLaw)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CountryLawRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.CountryLawRemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CountryLawModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.CountryLawModify, SystemStatusType.Active)));
                // can delete country law
                options.AddPolicy(
                    PatentAuthorizationPolicy.CountryLawCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.CountryLawCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CountryLawCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.CountryLawCanDelete, SystemStatusType.Active)));

                // Action Type
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessActionType,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.ActionType)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.ActionTypeRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.ActionTypeRemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.ActionTypeModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.ActionTypeModify, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessActionType,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.ActionType)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.ActionTypeRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.ActionTypeRemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.ActionTypeModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.ActionTypeModify, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanAccessActionType,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.ActionType)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.ActionTypeRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.ActionTypeRemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.ActionTypeModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.ActionTypeModify, SystemStatusType.Active)));
                //can delete action type 
                options.AddPolicy(
                    PatentAuthorizationPolicy.ActionTypeCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.ActionTypeCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.ActionTypeCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.ActionTypeCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.ActionTypeCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.ActionTypeCanDelete, SystemStatusType.Active)));


                //Due Date List
                //Pat, Tmk, and GM use CanAccessSystem policy
                //AMS integrated only
                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessDueDateList, policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRoles(SystemType.Patent, CPiPermissions.RegularUser) ||
                        context.User.IsInRoles(SystemType.Trademark, CPiPermissions.RegularUser) ||
                        context.User.IsInRoles(SystemType.GeneralMatter, CPiPermissions.RegularUser) ||
                        //AMS integrated only
                        //context.User.IsInRoles(SystemType.AMS, CPiPermissions.RegularUser) ||
                        context.User.IsInRoles(SystemType.DMS, CPiPermissions.DMSDueDateList)));
                options.AddPolicy(
                    DMSAuthorizationPolicy.CanAccessDueDateList,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.DMSDueDateList, SystemStatusType.Active)));

                //Modules
                //Audit
                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessAudit,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatAudit, CPiModule.TmkAudit, CPiModule.GMAudit, CPiModule.DMSAudit, CPiModule.FFAudit, CPiModule.PacAudit, CPiModule.AMSAudit, CPiModule.RMSAudit, CPiModule.TmcAudit)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessAudit,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatAudit, SystemType.Patent, CPiPermissions.FullRead))); //deny limited
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessAudit,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TmkAudit, SystemType.Trademark, CPiPermissions.FullRead))); //deny limited
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanAccessAudit,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.GMAudit, SystemType.GeneralMatter, CPiPermissions.FullRead))); //deny limited
                options.AddPolicy(
                    DMSAuthorizationPolicy.CanAccessAudit,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.DMSAudit, SystemType.DMS, CPiPermissions.FullRead))); //deny limited
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.CanAccessAudit,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.FFAudit, SystemType.ForeignFiling, CPiPermissions.FullRead))); //deny limited
                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.CanAccessAudit,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PacAudit, SystemType.PatClearance, CPiPermissions.FullRead))); //deny limited
                options.AddPolicy(
                    AMSAuthorizationPolicy.CanAccessAudit,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.AMSAudit, SystemType.AMS, CPiPermissions.FullRead))); //deny limited
                options.AddPolicy(
                    RMSAuthorizationPolicy.CanAccessAudit,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.RMSAudit, SystemType.RMS, CPiPermissions.FullRead))); //deny limited
                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.CanAccessAudit,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TmcAudit, SystemType.SearchRequest, CPiPermissions.FullRead))); //deny limited

                //DeDocket
                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessDeDocket,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatDeDocket, CPiModule.TmkDeDocket, CPiModule.GMDeDocket)));

                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessDeDocketAuxiliary,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatDeDocket, CPiModule.TmkDeDocket, CPiModule.GMDeDocket),
                        new CPiPermissionRequirement(SystemType.Shared)));

                //Portfolio Onboarding
                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessPortfolioOnboarding,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatPortfolioOnboarding, CPiModule.TmkPortfolioOnboarding, CPiModule.GMPortfolioOnboarding)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessPortfolioOnboarding,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatPortfolioOnboarding, SystemType.Patent, CPiPermissions.FullModify))); //full modify only
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessPortfolioOnboarding,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TmkPortfolioOnboarding, SystemType.Trademark, CPiPermissions.FullModify))); //full modify only
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanAccessPortfolioOnboarding,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.GMPortfolioOnboarding, SystemType.GeneralMatter, CPiPermissions.FullModify))); //full modify only

                //Inventor Award
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessInventorAward,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.InventorAward, SystemType.Patent, CPiPermissions.FullRead))); //deny limited

                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessInventorAwardAuxiliary,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.InventorAward, SystemType.Patent, CPiPermissions.Auxiliary))); //deny limited

                //Trademark Links
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessTrademarkLinks,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TrademarkLinks, SystemType.Trademark, CPiPermissions.FullRead))); //deny limited

                //RTS
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessRTS,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.RTS, SystemType.Patent, CPiPermissions.FullRead, userTypes: CPiPermissions.InternalUsers))); //deny limited

                //Custom Report
                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessCustomReport,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiPermissions.CustomQuery, CPiModule.PatCustomReport, CPiModule.TmkCustomReport, CPiModule.GMCustomReport)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessCustomReport,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatCustomReport, SystemType.Patent, CPiPermissions.CustomQuery))); //check custom query role
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessCustomReport,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TmkCustomReport, SystemType.Trademark, CPiPermissions.CustomQuery))); //check custom query role
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanAccessCustomReport,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.GMCustomReport, SystemType.GeneralMatter, CPiPermissions.CustomQuery))); //check custom query role

                //Products
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessProducts,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatProducts, SystemType.Patent, CPiPermissions.Products, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.ProductsRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatProducts, SystemType.Patent, CPiPermissions.ProductsRemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.ProductsModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatProducts, SystemType.Patent, CPiPermissions.ProductsModify, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessProducts,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TmkProducts, SystemType.Trademark, CPiPermissions.Products, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.ProductsRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TmkProducts, SystemType.Trademark, CPiPermissions.ProductsRemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.ProductsModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TmkProducts, SystemType.Trademark, CPiPermissions.ProductsModify, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanAccessProducts,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.GMProducts, SystemType.GeneralMatter, CPiPermissions.Products, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.ProductsRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.GMProducts, SystemType.GeneralMatter, CPiPermissions.ProductsRemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.ProductsModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.GMProducts, SystemType.GeneralMatter, CPiPermissions.ProductsModify, SystemStatusType.Active)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.CanAccessProducts,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.AMSProducts, SystemType.AMS, CPiPermissions.Products, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.ProductsRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.AMSProducts, SystemType.AMS, CPiPermissions.ProductsRemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.ProductsModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.AMSProducts, SystemType.AMS, CPiPermissions.ProductsModify, SystemStatusType.Active)));
                //can delete products
                options.AddPolicy(
                    PatentAuthorizationPolicy.ProductsCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatProducts, SystemType.Patent, CPiPermissions.ProductsCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.ProductsCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TmkProducts, SystemType.Trademark, CPiPermissions.ProductsCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.ProductsCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.GMProducts, SystemType.GeneralMatter, CPiPermissions.ProductsCanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.ProductsCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.AMSProducts, SystemType.AMS, CPiPermissions.ProductsCanDelete, SystemStatusType.Active)));

                //Document Verification
                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessDocumentVerification,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatDocumentVerification, CPiModule.TmkDocumentVerification, CPiModule.GMDocumentVerification)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessDocumentVerification,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatDocumentVerification, SystemType.Patent, CPiPermissions.DocumentVerification, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.DocumentVerificationModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatDocumentVerification, SystemType.Patent, CPiPermissions.DocumentVerificationModify, SystemStatusType.Active, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessDocumentVerification,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TmkDocumentVerification, SystemType.Trademark, CPiPermissions.DocumentVerification, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.DocumentVerificationModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TmkDocumentVerification, SystemType.Trademark, CPiPermissions.DocumentVerificationModify, SystemStatusType.Active, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanAccessDocumentVerification,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.GMDocumentVerification, SystemType.GeneralMatter, CPiPermissions.DocumentVerification, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.DocumentVerificationModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.GMDocumentVerification, SystemType.GeneralMatter, CPiPermissions.DocumentVerificationModify, SystemStatusType.Active, userTypes: CPiPermissions.InternalUsers)));

                //IDS Import
                options.AddPolicy(
                    IDSAuthorizationPolicy.CanAccessIDSImport,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.IDSImport, SystemType.IDS, CPiPermissions.FullModify, SystemStatusType.Active, userTypes: CPiPermissions.InternalUsers)));

                //Power BI Connector
                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessPowerBIConnector,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PowerBIConnector, null, CPiPermissions.CustomQuery, SystemStatusType.Active, userTypes: CPiPermissions.InternalUsers)));

                // RMS
                options.AddPolicy(
                    RMSAuthorizationPolicy.RegularUser,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.RMS, CPiPermissions.RegularUser)));
                options.AddPolicy(
                    RMSAuthorizationPolicy.CanAccessSystem,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.RMS)));
                options.AddPolicy(
                    RMSAuthorizationPolicy.FullModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.RMS, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                    RMSAuthorizationPolicy.RemarksOnlyModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.RMS, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    RMSAuthorizationPolicy.CanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.RMS, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    RMSAuthorizationPolicy.LimitedRead,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.RMS, CPiPermissions.LimitedRead, SystemStatusType.Active, true)));
                options.AddPolicy(
                    RMSAuthorizationPolicy.FullRead,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.RMS, CPiPermissions.FullRead, SystemStatusType.Active)));
                options.AddPolicy(
                    RMSAuthorizationPolicy.DecisionMaker,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.RMS, CPiPermissions.DecisionMaker, SystemStatusType.Active)));
                options.AddPolicy(
                   RMSAuthorizationPolicy.FullModifyByRespOffice,
                   policyBuilder => policyBuilder.AddRequirements(
                       new CPiRespOfficePermissionRequirement(SystemType.RMS, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                   RMSAuthorizationPolicy.RemarksOnlyModifyByRespOffice,
                   policyBuilder => policyBuilder.AddRequirements(
                       new CPiRespOfficePermissionRequirement(SystemType.RMS, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                   RMSAuthorizationPolicy.CanDeleteByRespOffice,
                   policyBuilder => policyBuilder.AddRequirements(
                       new CPiRespOfficePermissionRequirement(SystemType.RMS, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                  RMSAuthorizationPolicy.LimitedReadByRespOffice,
                  policyBuilder => policyBuilder.AddRequirements(
                      new CPiRespOfficePermissionRequirement(SystemType.RMS, CPiPermissions.LimitedRead, SystemStatusType.Active, true)));
                options.AddPolicy(
                  RMSAuthorizationPolicy.FullReadByRespOffice,
                  policyBuilder => policyBuilder.AddRequirements(
                      new CPiRespOfficePermissionRequirement(SystemType.RMS, CPiPermissions.FullRead, SystemStatusType.Active)));
                options.AddPolicy(
                    RMSAuthorizationPolicy.DecisionMakerByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.RMS, CPiPermissions.DecisionMaker, SystemStatusType.Active)));
                options.AddPolicy(
                    RMSAuthorizationPolicy.CanAccessTrademark,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark)));

                // Foreign Filing
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.RegularUser,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.ForeignFiling, CPiPermissions.RegularUser)));
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.CanAccessSystem,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.ForeignFiling)));
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.FullModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.ForeignFiling, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.RemarksOnlyModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.ForeignFiling, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.CanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.ForeignFiling, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.LimitedRead,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.ForeignFiling, CPiPermissions.LimitedRead, SystemStatusType.Active, true)));
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.FullRead,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.ForeignFiling, CPiPermissions.FullRead, SystemStatusType.Active)));
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.DecisionMaker,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.ForeignFiling, CPiPermissions.DecisionMaker, SystemStatusType.Active)));
                options.AddPolicy(
                   ForeignFilingAuthorizationPolicy.FullModifyByRespOffice,
                   policyBuilder => policyBuilder.AddRequirements(
                       new CPiRespOfficePermissionRequirement(SystemType.ForeignFiling, CPiPermissions.FullModify, SystemStatusType.Active)));
                options.AddPolicy(
                   ForeignFilingAuthorizationPolicy.RemarksOnlyModifyByRespOffice,
                   policyBuilder => policyBuilder.AddRequirements(
                       new CPiRespOfficePermissionRequirement(SystemType.ForeignFiling, CPiPermissions.RemarksOnly, SystemStatusType.Active)));
                options.AddPolicy(
                   ForeignFilingAuthorizationPolicy.CanDeleteByRespOffice,
                   policyBuilder => policyBuilder.AddRequirements(
                       new CPiRespOfficePermissionRequirement(SystemType.ForeignFiling, CPiPermissions.CanDelete, SystemStatusType.Active)));
                options.AddPolicy(
                  ForeignFilingAuthorizationPolicy.LimitedReadByRespOffice,
                  policyBuilder => policyBuilder.AddRequirements(
                      new CPiRespOfficePermissionRequirement(SystemType.ForeignFiling, CPiPermissions.LimitedRead, SystemStatusType.Active, true)));
                options.AddPolicy(
                  ForeignFilingAuthorizationPolicy.FullReadByRespOffice,
                  policyBuilder => policyBuilder.AddRequirements(
                      new CPiRespOfficePermissionRequirement(SystemType.ForeignFiling, CPiPermissions.FullRead, SystemStatusType.Active)));
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.DecisionMakerByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.ForeignFiling, CPiPermissions.DecisionMaker, SystemStatusType.Active)));
                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.CanAccessPatent,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent)));

                // Cost Estimator
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessCostEstimator,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatCostEstimator)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessCostEstimator,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatCostEstimator, SystemType.Patent, CPiPermissions.CostEstimator)));                
                options.AddPolicy(
                    PatentAuthorizationPolicy.CostEstimatorRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatCostEstimator, SystemType.Patent, CPiPermissions.CostEstimatorRemarksOnly)));                
                options.AddPolicy(
                    PatentAuthorizationPolicy.CostEstimatorModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatCostEstimator, SystemType.Patent, CPiPermissions.CostEstimatorModify)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessCostEstimator,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TmkCostEstimator)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessCostEstimator,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TmkCostEstimator, SystemType.Trademark, CPiPermissions.CostEstimator)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CostEstimatorRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TmkCostEstimator, SystemType.Trademark, CPiPermissions.CostEstimatorRemarksOnly)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CostEstimatorModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TmkCostEstimator, SystemType.Trademark, CPiPermissions.CostEstimatorModify)));
                //can delete cost estimator
                options.AddPolicy(
                    PatentAuthorizationPolicy.CostEstimatorCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.PatCostEstimator, SystemType.Patent, CPiPermissions.CostEstimatorCanDelete)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CostEstimatorCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.TmkCostEstimator, SystemType.Trademark, CPiPermissions.CostEstimatorCanDelete)));

                // German Inventor Remuneration
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessGermanRemuneration,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.GermanRemuneration, SystemType.Patent, CPiPermissions.GermanRemuneration)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.GermanRemunerationRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.GermanRemuneration, SystemType.Patent, CPiPermissions.GermanRemunerationRemarksOnly)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.GermanRemunerationModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.GermanRemuneration, SystemType.Patent, CPiPermissions.GermanRemunerationModify)));
                //can delete german remuneration
                options.AddPolicy(
                    PatentAuthorizationPolicy.GermanRemunerationCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.GermanRemuneration, SystemType.Patent, CPiPermissions.GermanRemunerationCanDelete)));

                // French Inventor Remuneration
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessFrenchRemuneration,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.FrenchRemuneration, SystemType.Patent, CPiPermissions.FrenchRemuneration)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.FrenchRemunerationRemarksOnly,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.FrenchRemuneration, SystemType.Patent, CPiPermissions.FrenchRemunerationRemarksOnly)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.FrenchRemunerationModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.FrenchRemuneration, SystemType.Patent, CPiPermissions.FrenchRemunerationModify)));
                //can delete french remuneration
                options.AddPolicy(
                    PatentAuthorizationPolicy.FrenchRemunerationCanDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.FrenchRemuneration, SystemType.Patent, CPiPermissions.FrenchRemunerationCanDelete)));

                //Global Search
                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessGlobalSearch,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.IsSystemEnabled(SystemType.Patent) || context.User.IsSystemEnabled(SystemType.Trademark) || context.User.IsSystemEnabled(SystemType.GeneralMatter) || context.User.IsSystemEnabled(SystemType.DMS)) &&
                        //context.User.IsInSystems(new List<string>() { SystemType.Patent, SystemType.Trademark, SystemType.GeneralMatter, SystemType.DMS }) &&
                        (context.User.IsInRoles(SystemType.Patent, CPiPermissions.FullRead) ||
                         context.User.IsInRoles(SystemType.Trademark, CPiPermissions.FullRead) ||
                         context.User.IsInRoles(SystemType.GeneralMatter, CPiPermissions.FullRead) ||
                         context.User.IsInRoles(SystemType.DMS, CPiPermissions.FullRead)) &&
                        context.User.IsInUserTypes(new List<CPiUserType>() { CPiUserType.User, CPiUserType.Inventor, CPiUserType.ContactPerson })));

                //Recently Viewed
                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessRecentViewed,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.IsSystemEnabled(SystemType.Patent) || context.User.IsSystemEnabled(SystemType.Trademark) || context.User.IsSystemEnabled(SystemType.GeneralMatter)) &&
                        context.User.IsInSystems(new List<string>() { SystemType.Patent, SystemType.Trademark, SystemType.GeneralMatter }) &&
                        context.User.IsInUserTypes(new List<CPiUserType>() { CPiUserType.User })));

                //Custom field
                options.AddPolicy(SharedAuthorizationPolicy.CanAccessCustomFieldSetup, policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsAdmin() && context.User.IsModuleEnabled(CPiModule.CustomField)));

                options.AddPolicy(
                    AMSAuthorizationPolicy.Internal,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.Internal,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.InternalRTS,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.RTS, SystemType.Patent, CPiPermissions.FullRead, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.Internal,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.Internal,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    SharedAuthorizationPolicy.Internal,
                    policyBuilder => policyBuilder.RequireAssertion(
                        context => context.User.IsInUserTypes(CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    AMSAuthorizationPolicy.InternalFullModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.AMS, CPiPermissions.FullModify, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.InternalFullModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.FullModify, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.InternalFullModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.FullModify, userTypes: CPiPermissions.InternalUsers)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.InternalFullModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.FullModify, userTypes: CPiPermissions.InternalUsers)));

                //Cost Tracking
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessCostTracking,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.CostTracking)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CostTrackingModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.CostTrackingModify, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CostTrackingDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.CostTrackingDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CostTrackingModifyByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.Patent, CPiPermissions.CostTrackingModify, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CostTrackingDeleteByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.Patent, CPiPermissions.CostTrackingDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.CostTrackingUpload,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.CostTrackingUpload, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessCostTracking,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.CostTracking)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CostTrackingModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.CostTrackingModify, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CostTrackingDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.CostTrackingDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CostTrackingModifyByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.Trademark, CPiPermissions.CostTrackingModify, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CostTrackingDeleteByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.Trademark, CPiPermissions.CostTrackingDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CostTrackingUpload,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.CostTrackingUpload, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanAccessCostTracking,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.CostTracking)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CostTrackingModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.CostTrackingModify, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CostTrackingDelete,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.CostTrackingDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CostTrackingModifyByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.GeneralMatter, CPiPermissions.CostTrackingModify, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CostTrackingDeleteByRespOffice,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiRespOfficePermissionRequirement(SystemType.GeneralMatter, CPiPermissions.CostTrackingDelete, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CostTrackingUpload,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.CostTrackingUpload, SystemStatusType.Active)));

                //Action Delegation can be assigned to:
                //User and DocketService users with modify, readonly, or nodelete roles to pat, tmk, or gm systems
                //Attorney users with access to pat, tmk, or gm systems
                //Admin users
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanReceiveActionDelegation,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.IsInRoles(SystemType.Patent, CPiPermissions.CanReceiveActionDelegationRoles) && context.User.IsInUserTypes(CPiPermissions.CanReceiveActionDelegationUsers)) ||
                        (context.User.IsInSystem(SystemType.Patent) && context.User.IsInUserType(CPiUserType.Attorney))));

                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanReceiveActionDelegation,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.IsInRoles(SystemType.Trademark, CPiPermissions.CanReceiveActionDelegationRoles) && context.User.IsInUserTypes(CPiPermissions.CanReceiveActionDelegationUsers)) ||
                        (context.User.IsInSystem(SystemType.Trademark) && context.User.IsInUserType(CPiUserType.Attorney))));

                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanReceiveActionDelegation,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.IsInRoles(SystemType.GeneralMatter, CPiPermissions.CanReceiveActionDelegationRoles) && context.User.IsInUserTypes(CPiPermissions.CanReceiveActionDelegationUsers)) ||
                        (context.User.IsInSystem(SystemType.GeneralMatter) && context.User.IsInUserType(CPiUserType.Attorney))));

                //Mail
                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessMail, policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsAdmin() ||
                        context.User.HasClaim(c => c.Type == CPiClaimTypes.Mailbox)));

                //Mail policy per mailbox
                var graph = configuration.GetSection("Graph").Get<GraphSettings>();
                if (graph != null && graph.Mailboxes != null && graph.Mailboxes.Any())
                {
                    foreach(var mailbox in graph.Mailboxes)
                    {
                        options.AddPolicy(
                            SharedAuthorizationPolicy.GetMailboxPolicyName(mailbox.MailboxName), policy =>
                            policy.RequireAssertion(context =>
                                context.User.IsAdmin() ||
                                context.User.HasClaim(c => c.Type == CPiClaimTypes.Mailbox && c.Value == mailbox.MailboxName)));
                    }
                }

                //Dashboard access if user has no system roles
                options.AddPolicy(CPiAuthorizationPolicy.DashboardUser, policy =>
                    policy.RequireAssertion(context =>
                        context.User.GetSystems().Any() || context.User.GetDashboardAccess().Any()));

                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessDashboard,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.HasDashboardAccess(SystemType.Patent))));

                options.AddPolicy(
                    IDSAuthorizationPolicy.CanAccessDashboard,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.HasDashboardAccess(SystemType.IDS))));

                options.AddPolicy(
                    ForeignFilingAuthorizationPolicy.CanAccessDashboard,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.HasDashboardAccess(SystemType.ForeignFiling))));

                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.CanAccessDashboard,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.HasDashboardAccess(SystemType.PatClearance))));

                options.AddPolicy(
                    AMSAuthorizationPolicy.CanAccessDashboard,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.HasDashboardAccess(SystemType.AMS))));

                options.AddPolicy(
                    DMSAuthorizationPolicy.CanAccessDashboard,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.HasDashboardAccess(SystemType.DMS))));

                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessDashboard,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.HasDashboardAccess(SystemType.Trademark))));

                options.AddPolicy(
                    RMSAuthorizationPolicy.CanAccessDashboard,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.HasDashboardAccess(SystemType.RMS))));

                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.CanAccessDashboard,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.HasDashboardAccess(SystemType.SearchRequest))));

                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanAccessDashboard,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.HasDashboardAccess(SystemType.GeneralMatter))));

                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessDashboard,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.HasDashboardAccess(SystemType.Shared))));

                //AMS Decision Management
                options.AddPolicy(
                    AMSAuthorizationPolicy.HasDecisionManagement,
                    policyBuilder => policyBuilder.AddRequirements(
                        new ModulePermissionRequirement(CPiModule.AMSDecisionManagement, SystemType.AMS)));

                //eSignature
                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessESignature,
                    policyBuilder => policyBuilder.AddRequirements(
                        new SettingPermissionRequirement("General", "IsESignatureOn")));

                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessESignatureAuxiliary,
                    policyBuilder => policyBuilder.AddRequirements(
                        new SettingPermissionRequirement("General", "IsESignatureOn"),
                        new CPiPermissionRequirement(SystemType.Shared)));

                //time tracker
                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessTimeTrackerAuxiliary,
                    policyBuilder => policyBuilder.AddRequirements(
                        new SettingPermissionRequirement("General", "IsCorporation", false),
                        new SettingPermissionRequirement("General", "IsTimeTrackerOn"),
                        new CPiPermissionRequirement(SystemType.Shared, CPiPermissions.FullModify)));

                //trade secret
                options.AddPolicy(
                    SharedAuthorizationPolicy.TradeSecretAdmin,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.IsAdmin() && context.User.HasClaim(c => c.Type == CPiClaimTypes.TradeSecret))));

                options.AddPolicy(
                    SharedAuthorizationPolicy.CanAccessTradeSecret,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.IsInUserTypes(CPiPermissions.CanHaveTSClearance.Union(CPiPermissions.CanHaveDMSTSClearance).ToList()) 
                        && context.User.HasClaim(c => c.Type == CPiClaimTypes.TradeSecret))));

                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessTradeSecret,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.CanAccessPatTradeSecret())));

                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessTradeSecretReports,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.CanAccessPatTradeSecretReports())));

                options.AddPolicy(
                    DMSAuthorizationPolicy.CanAccessTradeSecret,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.CanAccessDMSTradeSecret())));

                options.AddPolicy(
                    DMSAuthorizationPolicy.CanAccessTradeSecretReports,
                    policyBuilder => policyBuilder.RequireAssertion(context =>
                        (context.User.CanAccessDMSTradeSecretReports())));

                // Workflow
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanAccessWorkflow,
                    policyBuilder => policyBuilder.AddRequirements(
                       new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.Workflow)));
                options.AddPolicy(
                    PatentAuthorizationPolicy.WorkflowModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Patent, CPiPermissions.WorkflowModify, SystemStatusType.Active)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanAccessWorkflow,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.Workflow)));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.WorkflowModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.Trademark, CPiPermissions.WorkflowModify, SystemStatusType.Active)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanAccessWorkflow,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.Workflow)));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.WorkflowModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.GeneralMatter, CPiPermissions.WorkflowModify, SystemStatusType.Active)));
                options.AddPolicy(
                    DMSAuthorizationPolicy.CanAccessWorkflow,
                    policyBuilder => policyBuilder.AddRequirements(
                       new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.Workflow)));
                options.AddPolicy(
                    DMSAuthorizationPolicy.WorkflowModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.DMS, CPiPermissions.WorkflowModify, SystemStatusType.Active)));
                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.CanAccessWorkflow,
                    policyBuilder => policyBuilder.AddRequirements(
                       new CPiPermissionRequirement(SystemType.PatClearance, CPiPermissions.Workflow)));
                options.AddPolicy(
                    PatentClearanceAuthorizationPolicy.WorkflowModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.PatClearance, CPiPermissions.WorkflowModify, SystemStatusType.Active)));
                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.CanAccessWorkflow,
                    policyBuilder => policyBuilder.AddRequirements(
                       new CPiPermissionRequirement(SystemType.SearchRequest, CPiPermissions.Workflow)));
                options.AddPolicy(
                    SearchRequestAuthorizationPolicy.WorkflowModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new CPiPermissionRequirement(SystemType.SearchRequest, CPiPermissions.WorkflowModify, SystemStatusType.Active)));

                // SoftDocket
                options.AddPolicy(
                    PatentAuthorizationPolicy.SoftDocketAdd, policy =>
                    policy.RequireAssertion(context =>
                        context.User.GetSystemStatus() == SystemStatusType.Active && 
                        (context.User.IsInRoles(SystemType.Patent, CPiPermissions.SoftDocket) || context.User.IsSoftDocketUser())));
                options.AddPolicy(
                    PatentAuthorizationPolicy.SoftDocketModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new SoftDocketPermissionRequirement(SystemType.Patent)));

                options.AddPolicy(
                    TrademarkAuthorizationPolicy.SoftDocketAdd, policy =>
                    policy.RequireAssertion(context =>
                        context.User.GetSystemStatus() == SystemStatusType.Active && 
                        (context.User.IsInRoles(SystemType.Trademark, CPiPermissions.SoftDocket) || context.User.IsSoftDocketUser())));
                options.AddPolicy(
                    TrademarkAuthorizationPolicy.SoftDocketModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new SoftDocketPermissionRequirement(SystemType.Trademark)));

                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.SoftDocketAdd, policy =>
                    policy.RequireAssertion(context =>
                        context.User.GetSystemStatus() == SystemStatusType.Active && 
                        (context.User.IsInRoles(SystemType.GeneralMatter, CPiPermissions.SoftDocket) || context.User.IsSoftDocketUser())));
                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.SoftDocketModify,
                    policyBuilder => policyBuilder.AddRequirements(
                        new SoftDocketPermissionRequirement(SystemType.GeneralMatter)));

                // Request Docket
                options.AddPolicy(
                    PatentAuthorizationPolicy.CanRequestDocket, policy =>
                    policy.RequireAssertion(context =>
                        context.User.GetSystemStatus() == SystemStatusType.Active &&
                        (context.User.IsInRoles(SystemType.Patent, CPiPermissions.RequestDocket) || context.User.IsRequestDocketUser())));

                options.AddPolicy(
                    TrademarkAuthorizationPolicy.CanRequestDocket, policy =>
                    policy.RequireAssertion(context =>
                        context.User.GetSystemStatus() == SystemStatusType.Active &&
                        (context.User.IsInRoles(SystemType.Trademark, CPiPermissions.RequestDocket) || context.User.IsRequestDocketUser())));

                options.AddPolicy(
                    GeneralMatterAuthorizationPolicy.CanRequestDocket, policy =>
                    policy.RequireAssertion(context =>
                        context.User.GetSystemStatus() == SystemStatusType.Active &&
                        (context.User.IsInRoles(SystemType.GeneralMatter, CPiPermissions.RequestDocket) || context.User.IsRequestDocketUser())));
            });

            services.AddScoped<IAuthorizationHandler, CPiAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, CPiRespOfficeAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, SettingAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, ModuleAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, SoftDocketAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, CPiServiceAccountHandler>();

            services.AddScoped<IAsyncRepository<CPiSetting>, CPiEfRepository<CPiSetting>>();
            services.AddScoped<IAsyncRepository<CPiUserSetting>, CPiEfRepository<CPiUserSetting>>();
            services.AddScoped<IAsyncRepository<CPiDefaultPage>, CPiEfRepository<CPiDefaultPage>>();
            services.AddScoped<IAsyncRepository<CPiUser>, CPiEfRepository<CPiUser>>();

            return services;
        }
    }
}
