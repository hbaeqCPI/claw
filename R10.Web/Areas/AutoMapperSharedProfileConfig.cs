using AutoMapper;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.FormExtract;
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Entities.Patent;
// using R10.Core.Entities.DMS; // Removed during deep clean
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Trademark;
using R10.Core.Queries.Shared;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Shared.ViewModels.ReportScheduler;
using R10.Web.Extensions;
using System.Linq;
using Microsoft.Graph;
using R10.Web.Models.MailViewModels;
using R10.Web.Services;
using R10.Web.Services.MailDownload;
using R10.Web.Areas.Shared.ViewModels.SharePoint;
using R10.Web.Models.IManageModels;
using R10.Core.Identity;
using R10.Web.Areas.Shared.ViewModels.Report;

namespace R10.Web.Areas
{
    public class AutoMapperSharedProfileConfig : Profile
    {
        public AutoMapperSharedProfileConfig()
        {
            #region Shared Auxiliaries
            CreateMap<Client, ClientSearchResultViewModel>();
            CreateMap<Client, ClientDetailViewModel>()
                 .ForMember(vm => vm.CountryName, domain => domain.MapFrom(c => c.AddressCountry.CountryName))
                 .ForMember(vm => vm.POCountryName, domain => domain.MapFrom(c => c.POAddressCountry.CountryName))
                 .ForMember(vm => vm.PatDefaultAtty1Code, domain => domain.MapFrom(c => c.PatDefaultAtty1.AttorneyCode))
                 .ForMember(vm => vm.PatDefaultAtty2Code, domain => domain.MapFrom(c => c.PatDefaultAtty2.AttorneyCode))
                 .ForMember(vm => vm.PatDefaultAtty3Code, domain => domain.MapFrom(c => c.PatDefaultAtty3.AttorneyCode))
                 .ForMember(vm => vm.PatDefaultAtty4Code, domain => domain.MapFrom(c => c.PatDefaultAtty4.AttorneyCode))
                 .ForMember(vm => vm.PatDefaultAtty5Code, domain => domain.MapFrom(c => c.PatDefaultAtty5.AttorneyCode))

                 .ForMember(vm => vm.TmkDefaultAtty1Code, domain => domain.MapFrom(c => c.TmkDefaultAtty1.AttorneyCode))
                 .ForMember(vm => vm.TmkDefaultAtty2Code, domain => domain.MapFrom(c => c.TmkDefaultAtty2.AttorneyCode))
                 .ForMember(vm => vm.TmkDefaultAtty3Code, domain => domain.MapFrom(c => c.TmkDefaultAtty3.AttorneyCode))
                 .ForMember(vm => vm.TmkDefaultAtty4Code, domain => domain.MapFrom(c => c.TmkDefaultAtty4.AttorneyCode))
                 .ForMember(vm => vm.TmkDefaultAtty5Code, domain => domain.MapFrom(c => c.TmkDefaultAtty5.AttorneyCode))

                 .ForMember(vm => vm.PatCEGeneralSetupName, domain => domain.MapFrom(c => c.PatCEGeneralSetup.CostSetup))
                 .ForMember(vm => vm.TmkCEGeneralSetupName, domain => domain.MapFrom(c => c.TmkCEGeneralSetup.CostSetup))
                 .ForMember(vm => vm.RemunerationSettingName, domain => domain.MapFrom(c => c.RemunerationSetting.Name));

            CreateMap<AgentContact, AgentContactViewModel>()
                .ForMember(vm => vm.ContactName, domain => domain.MapFrom(cc => cc.Contact.ContactName))
                .ForMember(vm => vm.ReceiveAgentResponsibilityLetter, domain => domain.MapFrom(cc => (cc.ReceiveAgentResponsibilityLetter ?? false)))
                .ForMember(vm => vm.RMSReceiveAgentResponsibilityLetter, domain => domain.MapFrom(cc => (cc.RMSReceiveAgentResponsibilityLetter ?? false)))
                .ForMember(vm => vm.FFReceiveAgentResponsibilityLetter, domain => domain.MapFrom(cc => (cc.FFReceiveAgentResponsibilityLetter ?? false)))
                .ForMember(vm => vm.IsPatentContact, domain => domain.MapFrom(cc => (cc.IsPatentContact ?? false)))
                .ForMember(vm => vm.IsTrademarkContact, domain => domain.MapFrom(cc => (cc.IsTrademarkContact ?? false)))
                .ForMember(vm => vm.IsGeneralMatterContact, domain => domain.MapFrom(cc => (cc.IsGeneralMatterContact ?? false)))
                .ForMember(m => m.Contact, opt => opt.Ignore())
                .ForMember(m => m.Agent, opt => opt.Ignore());
            CreateMap<AgentContactViewModel, AgentContact>()
                .ForMember(m => m.Contact, opt => opt.Ignore())
                .ForMember(m => m.Agent, opt => opt.Ignore());

            CreateMap<ClientContact, ClientContactViewModel>()
                .ForMember(vm => vm.ContactName, domain => domain.MapFrom(cc => cc.Contact.ContactName))
                .ForMember(vm => vm.ReceiveConfirmationLetter, domain => domain.MapFrom(cc => (cc.ReceiveConfirmationLetter ?? false)))
                .ForMember(vm => vm.IsDecisionMaker, domain => domain.MapFrom(cc => (cc.IsDecisionMaker ?? false)))
                .ForMember(vm => vm.RMSReceiveConfirmationLetter, domain => domain.MapFrom(cc => (cc.RMSReceiveConfirmationLetter ?? false)))
                .ForMember(vm => vm.RMSIsDecisionMaker, domain => domain.MapFrom(cc => (cc.RMSIsDecisionMaker ?? false)))
                .ForMember(vm => vm.FFReceiveConfirmationLetter, domain => domain.MapFrom(cc => (cc.FFReceiveConfirmationLetter ?? false)))
                .ForMember(vm => vm.IsDMSReviewer, domain => domain.MapFrom(cc => cc.Contact.EntityFilters.Any(ef => ef.EntityId == cc.ContactID && ef.CPiUser.UserType == Core.Identity.CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.DMS && usr.RoleId == "Reviewer"))))
                .ForMember(vm => vm.IsAMSDecisionMaker, domain => domain.MapFrom(cc => cc.Contact.EntityFilters.Any(ef => ef.EntityId == cc.ContactID && ef.CPiUser.UserType == Core.Identity.CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.AMS && usr.RoleId == "DecisionMaker"))))
                .ForMember(vm => vm.IsTmkSearchReviewer, domain => domain.MapFrom(cc => cc.Contact.EntityFilters.Any(ef => ef.EntityId == cc.ContactID && ef.CPiUser.UserType == Core.Identity.CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.SearchRequest && usr.RoleId == "Reviewer"))))
                .ForMember(vm => vm.IsPatClearanceReviewer, domain => domain.MapFrom(cc => cc.Contact.EntityFilters.Any(ef => ef.EntityId == cc.ContactID && ef.CPiUser.UserType == Core.Identity.CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.PatClearance && usr.RoleId == "Reviewer"))))
                .ForMember(vm => vm.IsRMSDecisionMaker, domain => domain.MapFrom(cc => cc.Contact.EntityFilters.Any(ef => ef.EntityId == cc.ContactID && ef.CPiUser.UserType == Core.Identity.CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.RMS && usr.RoleId == "DecisionMaker"))))
                .ForMember(vm => vm.IsFFDecisionMaker, domain => domain.MapFrom(cc => cc.Contact.EntityFilters.Any(ef => ef.EntityId == cc.ContactID && ef.CPiUser.UserType == Core.Identity.CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.ForeignFiling && usr.RoleId == "DecisionMaker"))))
                .ForMember(vm => vm.IsPatentContact, domain => domain.MapFrom(cc => (cc.IsPatentContact ?? false)))
                .ForMember(vm => vm.IsTrademarkContact, domain => domain.MapFrom(cc => (cc.IsTrademarkContact ?? false)))
                .ForMember(vm => vm.IsGeneralMatterContact, domain => domain.MapFrom(cc => (cc.IsGeneralMatterContact ?? false)))
                .ForMember(m => m.Contact, opt => opt.Ignore())
                .ForMember(m => m.Client, opt => opt.Ignore());
            CreateMap<ClientContactViewModel, ClientContact>()
                .ForMember(m => m.Contact, opt => opt.Ignore())
                .ForMember(m => m.Client, opt => opt.Ignore());

            CreateMap<ContactPerson, ContactPersonViewModel>()
                .ForMember(vm => vm.CountryName, domain => domain.MapFrom(cp => cp.AddressCountry.CountryName))
                .ForMember(vm => vm.UserId, domain => domain.MapFrom(cp => cp.EntityFilters.Where(ef => ef.EntityId == cp.ContactID && ef.CPiUser.UserType == Core.Identity.CPiUserType.ContactPerson).Select(ef => ef.UserId).FirstOrDefault()))
                .ForMember(vm => vm.IsDMSReviewer, domain => domain.MapFrom(cp => cp.EntityFilters.Any(ef => ef.EntityId == cp.ContactID && ef.CPiUser.UserType == Core.Identity.CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.DMS && usr.RoleId == "Reviewer"))))
                .ForMember(vm => vm.IsAMSDecisionMaker, domain => domain.MapFrom(cp => cp.EntityFilters.Any(ef => ef.EntityId == cp.ContactID && ef.CPiUser.UserType == Core.Identity.CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.AMS && usr.RoleId == "DecisionMaker"))))
                .ForMember(vm => vm.IsTmkSearchReviewer, domain => domain.MapFrom(cp => cp.EntityFilters.Any(ef => ef.EntityId == cp.ContactID && ef.CPiUser.UserType == Core.Identity.CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.SearchRequest && usr.RoleId == "Reviewer"))))
                .ForMember(vm => vm.IsPatClearanceReviewer, domain => domain.MapFrom(cp => cp.EntityFilters.Any(ef => ef.EntityId == cp.ContactID && ef.CPiUser.UserType == Core.Identity.CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.PatClearance && usr.RoleId == "Reviewer"))))
                .ForMember(vm => vm.IsRMSDecisionMaker, domain => domain.MapFrom(cp => cp.EntityFilters.Any(ef => ef.EntityId == cp.ContactID && ef.CPiUser.UserType == Core.Identity.CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.RMS && usr.RoleId == "DecisionMaker"))))
                .ForMember(vm => vm.IsFFDecisionMaker, domain => domain.MapFrom(cp => cp.EntityFilters.Any(ef => ef.EntityId == cp.ContactID && ef.CPiUser.UserType == Core.Identity.CPiUserType.ContactPerson && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.ForeignFiling && usr.RoleId == "DecisionMaker"))))
                .ForMember(m => m.AddressCountry, opt => opt.Ignore())
                .ForMember(m => m.ContactLanguage, opt => opt.Ignore())
                .ForMember(m => m.ClientContacts, opt => opt.Ignore())
                .ForMember(m => m.AgentContacts, opt => opt.Ignore())
                .ForMember(m => m.OwnerContacts, opt => opt.Ignore())
                .ForMember(m => m.EntityFilters, opt => opt.Ignore());

            CreateMap<OwnerContact, OwnerContactViewModel>()
                .ForMember(vm => vm.ContactName, domain => domain.MapFrom(cc => cc.Contact.ContactName))
                .ForMember(vm => vm.IsPatentContact, domain => domain.MapFrom(cc => (cc.IsPatentContact ?? false)))
                .ForMember(vm => vm.IsTrademarkContact, domain => domain.MapFrom(cc => (cc.IsTrademarkContact ?? false)))
                .ForMember(vm => vm.IsGeneralMatterContact, domain => domain.MapFrom(cc => (cc.IsGeneralMatterContact ?? false)))
                .ForMember(m => m.Contact, opt => opt.Ignore())
                .ForMember(m => m.Owner, opt => opt.Ignore());
            CreateMap<OwnerContactViewModel, OwnerContact>()
                .ForMember(m => m.Contact, opt => opt.Ignore())
                .ForMember(m => m.Owner, opt => opt.Ignore());

            CreateMap<ContactPerson, ContactSearchResultViewModel>();

            CreateMap<Owner, OwnerSearchResultViewModel>();
            CreateMap<Owner, OwnerDetailViewModel>()
                 .ForMember(vm => vm.CountryName, domain => domain.MapFrom(c => c.AddressCountry.CountryName))
                 .ForMember(vm => vm.POCountryName, domain => domain.MapFrom(c => c.POAddressCountry.CountryName));

            CreateMap<Agent, AgentSearchResultViewModel>();
            CreateMap<Agent, AgentDetailViewModel>()
                 .ForMember(vm => vm.CountryName, domain => domain.MapFrom(c => c.AddressCountry.CountryName))
                 .ForMember(vm => vm.POCountryName, domain => domain.MapFrom(c => c.POAddressCountry.CountryName));

            CreateMap<Attorney, AttorneySearchResultViewModel>();
            CreateMap<Attorney, AttorneyDetailViewModel>()
                .ForMember(vm => vm.CountryName, domain => domain.MapFrom(c => c.AddressCountry.CountryName))
                .ForMember(vm => vm.POCountryName, domain => domain.MapFrom(c => c.POAddressCountry.CountryName))
                .ForMember(vm => vm.UserId, domain => domain.MapFrom(a => a.EntityFilters.Where(ef => ef.EntityId == a.AttorneyID && ef.CPiUser.UserType == Core.Identity.CPiUserType.Attorney).Select(ef => ef.UserId).FirstOrDefault()))
                .ForMember(vm => vm.IsPatentUser, domain => domain.MapFrom(a => a.EntityFilters.Any(ef => ef.EntityId == a.AttorneyID && ef.CPiUser.UserType == Core.Identity.CPiUserType.Attorney && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.Patent))))
                .ForMember(vm => vm.IsTrademarkUser, domain => domain.MapFrom(a => a.EntityFilters.Any(ef => ef.EntityId == a.AttorneyID && ef.CPiUser.UserType == Core.Identity.CPiUserType.Attorney && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.Trademark))))
                .ForMember(vm => vm.IsGeneralMatterUser, domain => domain.MapFrom(a => a.EntityFilters.Any(ef => ef.EntityId == a.AttorneyID && ef.CPiUser.UserType == Core.Identity.CPiUserType.Attorney && ef.CPiUser.CPiUserSystemRoles.Any(usr => usr.CPiSystem.IsEnabled && usr.SystemId == SystemType.GeneralMatter))));

            CreateMap<ClientDesignatedCountry, ClientDesignatedCountryViewModel>()
              .ForMember(vm => vm.SystemTypeName, domain => domain.MapFrom(d => (d.SystemType == "P" ? SystemType.Patent : SystemType.Trademark)))
              .ForMember(vm => vm.DesCtryName, domain => domain.MapFrom(d => (d.SystemType == "P" ? d.PatCountry.CountryName : d.TmkCountry.CountryName)));

            CreateMap<ClientDesignatedCountryViewModel, ClientDesignatedCountry>()
              .ForMember(domain => domain.SystemType, vm => vm.MapFrom(v => v.SystemTypeName.Substring(0, 1)));

            CreateMap<EmailType, EmailTypeDetailViewModel>()
               .ForMember(vm => vm.TemplateName, domain => domain.MapFrom(e => e.EmailTemplate.Name))
               .ForMember(vm => vm.ContentTypeDescription, domain => domain.MapFrom(e => e.EmailContentType.Description))
               .ForMember(vm => vm.Policy, domain => domain.MapFrom(e => e.EmailContentType.Policy));

            CreateMap<EmailType, EmailTypeSearchResultViewModel>()
               .ForMember(vm => vm.ContentTypeDescription, domain => domain.MapFrom(e => e.EmailContentType.Description))
               .ForMember(vm => vm.Template, domain => domain.MapFrom(e => e.EmailTemplate.Name));

            CreateMap<EmailSetup, EmailSetupDetailViewModel>()
               .ForMember(vm => vm.Name, domain => domain.MapFrom(e => e.EmailType.Name))
               .ForMember(vm => vm.Description, domain => domain.MapFrom(e => e.EmailType.Description))
               .ForMember(vm => vm.ContentType, domain => domain.MapFrom(e => e.EmailType.ContentType))
               .ForMember(vm => vm.ContentTypeDescription, domain => domain.MapFrom(e => e.EmailType.EmailContentType.Description))
               .ForMember(vm => vm.LanguageCulture, domain => domain.MapFrom(e => e.LanguageLookup.LanguageCulture));

            CreateMap<EmailSetup, EmailSetupListViewModel>()
               .ForMember(vm => vm.LanguageCulture, domain => domain.MapFrom(e => e.LanguageLookup.LanguageCulture));
            #endregion

            #region Reports
            CreateMap<CustomReport, CustomReportViewModel>();
            CreateMap<CustomReport, CustomReportSearchResultViewModel>();
            CreateMap<CustomReport, CustomReportDetailViewModel>();
            CreateMap<CustomReportUploadViewModel, CustomReportDetailViewModel>();
            CreateMap<Invention, TradeSecretMasterListReportViewModel>()
               .ForMember(vm => vm.Id, domain => domain.MapFrom(d => d.InvId))
               .ForMember(vm => vm.CaseNumber, domain => domain.MapFrom(d => d.CaseNumber))
               .ForMember(vm => vm.Title, domain => domain.MapFrom(d => d.TradeSecret.InvTitle))
               .ForMember(vm => vm.TradeSecretDate, domain => domain.MapFrom(d => d.TradeSecretDate))
               .ForMember(vm => vm.TradeSecretDate_Fmt, domain => domain.MapFrom(d => d.TradeSecretDate.HasValue ?
                                                                                      d.TradeSecretDate.Value.ToString("MM/dd/yyyy HH:mm:ss") :
                                                                                      string.Empty))
               .ForMember(vm => vm.Sys, domain => domain.MapFrom(d => "Patent"))
               .ForMember(vm => vm.Inventors, domain => domain.MapFrom(d => string.Join(", ", d.Inventors.Select(i => i.InventorInvInventor.Inventor))))
               .ForMember(vm => vm.AbstractConcat, domain => domain.MapFrom(d => string.Join("; ", d.Abstracts.Select(a => a.TradeSecret.Abstract))))
               .ForMember(vm => vm.Abstracts, domain => domain.MapFrom(d => d.Abstracts
                                                           .Select(a => new AbstractExport()
                                                           {
                                                               Abstract = a.TradeSecret.Abstract,
                                                               Language = a.LanguageName,
                                                               OrderOfEntry = a.OrderOfEntry
                                                           })));
            #endregion

            #region Quick Docket
            CreateMap<QuickDocketDTO, QuickDocketExportViewModel>()
               .ForMember(vm => vm.FilDateString, opt => opt.MapFrom(d => d.FilDate.FormatToDisplay()))
               .ForMember(vm => vm.IssRegDateString, opt => opt.MapFrom(d => d.IssRegDate.FormatToDisplay()))
               .ForMember(vm => vm.InstructionDateString, opt => opt.MapFrom(d => d.InstructionDate.FormatToDisplay()))
               .ForMember(vm => vm.InstructionCompletedString, opt => opt.MapFrom(d => d.InstructedBy == null ? "" : (d.InstructionCompleted != null && d.InstructionCompleted == true ? "Yes" : "No")));

            CreateMap<QuickDocketDefaultSettingsViewModel, QuickDocketSearchCriteriaViewModel>()
               .ForMember(domain => domain.FromDueDate, opt => opt.Ignore())
               .ForMember(domain => domain.ToDueDate, opt => opt.Ignore())
               .ForMember(domain => domain.FromBaseDate, opt => opt.Ignore())
               .ForMember(domain => domain.ToBaseDate, opt => opt.Ignore())
               .ForMember(domain => domain.FromInstrxDate, opt => opt.Ignore())
               .ForMember(domain => domain.ToInstrxDate, opt => opt.Ignore());

            CreateMap<QuickDocketSearchCriteriaDTO, QuickDocketSearchCriteriaViewModel>()
               .ForMember(vm => vm.Patent, opt => opt.Ignore())
               .ForMember(vm => vm.PTOActions, opt => opt.Ignore())
               .ForMember(vm => vm.Trademark, opt => opt.Ignore())
               .ForMember(vm => vm.TrademarkLinks, opt => opt.Ignore())
               .ForMember(vm => vm.GeneralMatter, opt => opt.Ignore());

            //CreateMap<QuickDocketSearchCriteriaViewModel, QuickDocketSearchCriteriaDTO>()
            //    .ForMember(dto => dto.Indicator, vm => vm.MapFrom(c => c.Indicators.Length > 0 ? "|" + string.Join("|", c.Indicators) + "|" : null))
            //    .ForMember(dto => dto.Attorney, vm => vm.MapFrom(c => c.Attorneys.Length > 0 ? "|" + string.Join("|", c.Attorneys) + "|" : null))
            //    .ForMember(dto => dto.Client, vm => vm.MapFrom(c => c.Clients.Length > 0 ? "|" + string.Join("|", c.Clients) + "|" : null))
            //    .ForMember(dto => dto.ActionType, vm => vm.MapFrom(c => c.ActionTypes.Length > 0 ? "|" + string.Join("|", c.ActionTypes) + "|" : null))
            //    .ForMember(dto => dto.FilterAtty, vm => vm.MapFrom(c => "|" + c.AttorneyFilter1 + "|" + c.AttorneyFilter2 + "|" + c.AttorneyFilter3 + "|" + c.AttorneyFilter4 + "|" + c.AttorneyFilter5 + "|" + c.AttorneyFilterR + "|" + c.AttorneyFilterD + "|"));


            CreateMap<QuickDocketSearchCriteriaViewModel, QuickDocketSearchCriteriaDTO>()
                .ForMember(dto => dto.Indicator, opt => opt.MapFrom(c =>
                    c.Indicators != null && c.Indicators.Length > 0 ? "|" + string.Join("|", c.Indicators) + "|" : null))
                .ForMember(dto => dto.Attorney, opt => opt.MapFrom(c =>
                    c.Attorneys != null && c.Attorneys.Length > 0 ? "|" + string.Join("|", c.Attorneys) + "|" : null))
                .ForMember(dto => dto.Client, opt => opt.MapFrom(c =>
                    c.Clients != null && c.Clients.Length > 0 ? "|" + string.Join("|", c.Clients) + "|" : null))
                .ForMember(dto => dto.ActionType, opt => opt.MapFrom(c =>
                    c.ActionTypes != null && c.ActionTypes.Length > 0 ? "|" + string.Join("|", c.ActionTypes) + "|" : null))
                .ForMember(dto => dto.FilterAtty, opt => opt.MapFrom(c =>
                    "|" + (c.AttorneyFilter1 ?? "")
                    + "|" + (c.AttorneyFilter2 ?? "")
                    + "|" + (c.AttorneyFilter3 ?? "")
                    + "|" + (c.AttorneyFilter4 ?? "")
                    + "|" + (c.AttorneyFilter5 ?? "")
                    + "|" + (c.AttorneyFilterR ?? "")
                    + "|" + (c.AttorneyFilterD ?? "")
                    + "|"));

            CreateMap<QuickDocketSearchCriteriaDTO, QuickDocketUpdateCriteriaDTO>();

            CreateMap<QuickDocketDefaultSettingsViewModel, QuickDocketDefaultSettingsDTO>();
            CreateMap<QuickDocketSearchCriteriaDTO, QuickDocketPrintViewModel>();
            CreateMap<QuickDocketSchedulerDTO, QuickDocketSchedulerViewModel>();

            #endregion

            #region Report Scheduler
            CreateMap<RSMain, RSMainDetailViewModel>()
                .ForMember(vm => vm.TaskId, domain => domain.MapFrom(c => c.TaskId));
            #endregion                       

            #region Quick Email
            CreateMap<QEMain, QuickEmailSearchResultViewModel>();
            CreateMap<QEMain, QuickEmailSetupDetailViewModel>()
                .ForMember(vm => vm.ScreenName, opt => opt.MapFrom(q => q.SystemScreen.ScreenName))
                .ForMember(vm => vm.SystemType, opt => opt.MapFrom(q => q.SystemScreen.SystemType))
                .ForMember(vm => vm.eSignature, opt => opt.MapFrom(q => q.SystemScreen.eSignature))
                .ForMember(vm => vm.DataSourceName, opt => opt.MapFrom(q => q.DataSource.DataSourceName))
                .ForMember(vm => vm.LanguageName, opt => opt.MapFrom(q => q.LanguageLookup.LanguageName))
                .ForMember(vm => vm.LanguageLookup, opt => opt.Ignore())
                .ForMember(vm => vm.DataSource, opt => opt.Ignore())
                .ForMember(vm => vm.SystemScreen, opt => opt.Ignore())
                .ForMember(vm => vm.Tags, domain => domain.MapFrom(dq => dq.QETags.Select(t => t.Tag)))
                .ForMember(vm => vm.QECat, domain => domain.MapFrom(dq => dq.QECategory.QECat))
                ;

            CreateMap<QuickEmailSetupDetailViewModel, QEMain>()
                .ForMember(m => m.SystemScreen, opt => opt.Ignore())
                .ForMember(m => m.DataSource, opt => opt.Ignore())
                .ForMember(m => m.QECategory, opt => opt.Ignore());
            //.ForMember(m => m.Language, opt => opt.Ignore());

            CreateMap<QEDetailView, QEMain>()
                .ForMember(m => m.SystemScreen, opt => opt.Ignore())
                .ForMember(m => m.DataSource, opt => opt.Ignore());

            CreateMap<QERecipient, QuickEmailSetupRecipientViewModel>()
                .ForMember(q => q.RoleName, vm => vm.MapFrom(v => v.QERoleSource.RoleName))
                .ForMember(q => q.RoleType, vm => vm.MapFrom(v => v.QERoleSource.RoleType))
                .ForMember(q => q.Description, vm => vm.MapFrom(v => v.QERoleSource.Description));

            CreateMap<QuickEmailSetupRecipientViewModel, QERecipient>()
                .ForMember(q => q.QERoleSource, opt => opt.Ignore());

            CreateMap<QEMain, QuickEmailDetailViewModel>()
                .ForMember(q => q.ScreenName, vm => vm.MapFrom(v => v.SystemScreen.ScreenName))
                .ForMember(q => q.SystemType, vm => vm.MapFrom(v => v.SystemScreen.SystemType))
                //.ForMember(q => q.LanguageCulture, vm => vm.MapFrom(v => v.LanguageLookup.LanguageCulture))
                .ForMember(q => q.Body, vm => vm.MapFrom(v => v.Detail));

            CreateMap<QuickEmailDetailViewModel, QEMain>()
                .ForMember(m => m.SystemScreen, opt => opt.Ignore())
                .ForMember(m => m.DataSource, opt => opt.Ignore());

            CreateMap<QERecipientRoleDTO, QuickEmailRecipientRoleViewModel>()
                .ForMember(q => q.SendAs, opt => opt.Ignore());
            CreateMap<QEImagesLinksDTO, QuickEmailImageLinkViewModel>();
            CreateMap<QuickEmailImageLinkViewModel, AttachedFileDTO>()
                .ForMember(q => q.FileName, vm => vm.MapFrom(v => v.FilePath))
                .ForMember(q => q.FileTitle, vm => vm.MapFrom(v => v.ImageTitle));

            CreateMap<QEDataSource, QEDataSourceDetailViewModel>();
            CreateMap<QEDataSource, QEDataSourceSearchResultViewModel>();
            CreateMap<QECustomField, QECustomFieldViewModel>();
            CreateMap<QECustomFieldViewModel, QECustomField>()
                .ForMember(vw => vw.QEDataSource, opt => opt.Ignore());
            CreateMap<QEMain, QuickEmailListViewModel>()
                .ForMember(vm => vm.SystemType, domain => domain.MapFrom(m => m.SystemScreen.SystemType));
            #endregion

            #region Letters
            CreateMap<LetterEntitySetting, LetterEntitySettingViewModel>()
                .ForMember(vm => vm.LetterCategory, domain => domain.MapFrom(s => s.LetterCategory));
            CreateMap<LetterEntitySettingViewModel, LetterEntitySetting>()
                .ForMember(m => m.LetterCategory, opt => opt.Ignore());

            CreateMap<LetterMain, LetterMainDetailViewModel>()
                .ForMember(vm => vm.ScreenName, domain => domain.MapFrom(letmain => letmain.SystemScreen.ScreenName))
                .ForMember(vm => vm.LetCatDesc, domain => domain.MapFrom(letmain => letmain.LetterCategory.LetCatDesc))
                .ForMember(vm => vm.LetSubCat, domain => domain.MapFrom(letmain => letmain.LetterSubCategory.LetSubCat))
                .ForMember(vm => vm.SignatureQESetupName, domain => domain.MapFrom(letmain => letmain.QEMain.TemplateName))
                .ForMember(vm => vm.Tags, domain => domain.MapFrom(d => d.LetterTags.Select(t => t.Tag)))
                ;

            CreateMap<LetterDataSource, LetterDataSourceDetailViewModel>();


            CreateMap<LetterMain, LetterListViewModel>()
                .ForMember(vm => vm.SystemType, domain => domain.MapFrom(letmain => letmain.SystemScreen.SystemType));

            CreateMap<LetterMainDetailViewModel, LetterMain>()
                .ForMember(model => model.LetterCategory, opt => opt.Ignore())
                .ForMember(model => model.LetterSubCategory, opt => opt.Ignore())
                .ForMember(model => model.SystemScreen, opt => opt.Ignore());

            CreateMap<LetterUserDataViewModel, LetterUserData>()
                .ForMember(model => model.LetterMain, opt => opt.Ignore());

            //CreateMap<LetterRecordSourceFilter, LetterRecordSourceFilterViewModel>()
            //    .ForMember(vm => vm.LetterRecordSource, domain => domain.MapFrom(f => f.LetterRecordSource.LetterDataSource));
            //CreateMap<LetterRecordSourceFilter, LetterRecordSourceFilterViewModel>();
            CreateMap<LetterRecordSourceFilter, LetterRecordSourceFilterViewModel>();
            CreateMap<LetterRecordSourceFilterViewModel, LetterRecordSourceFilter>()
                .ForMember(f => f.LetterRecordSource, opt => opt.Ignore())
                .ForMember(f => f.RecSourceId, vw => vw.MapFrom(f => f.LetterRecordSource.RecSourceId));

            //CreateMap<LetterRecordSourceFilterUser, LetterRecordSourceFilterUserViewModel>()
            //    .ForMember(vm => vm.LetterRecordSource, domain => domain.MapFrom(f => f.LetterRecordSource.LetterDataSource));
            //CreateMap<LetterRecordSourceFilterUser, LetterRecordSourceFilterUserViewModel>();
            CreateMap<LetterRecordSourceFilterUser, LetterRecordSourceFilterUserViewModel>();
            CreateMap<LetterRecordSourceFilterUserViewModel, LetterRecordSourceFilterUser>()
                .ForMember(f => f.LetterRecordSource, opt => opt.Ignore())
                .ForMember(f => f.RecSourceId, vw => vw.MapFrom(f => f.LetterRecordSource.RecSourceId));

            CreateMap<LetterRecordSourceFilter, LetterFilterListViewModel>()
                .ForMember(vm => vm.RecSource, domain => domain.MapFrom(f => f.LetterRecordSource.LetterDataSource.DataSourceDescMain));

            CreateMap<LetterRecordSource, LetterDataSourceListViewModel>()
                .ForMember(vm => vm.RecSourceId, domain => domain.MapFrom(rs => rs.RecSourceId))
                .ForMember(vm => vm.DataSourceDescMain, domain => domain.MapFrom(rs => rs.LetterDataSource.DataSourceDescMain));
            //.ForMember(vm => vm.DataSourceDescDtl, domain => domain.MapFrom(rs => rs.LetterDataSource.DataSourceDescDtl));

            CreateMap<LetterContactDTO, LetterContactViewModel>()
                .ForMember(vm => vm.IsGenerate, domain => domain.MapFrom(c => c.IsPrevGen != true));
            CreateMap<LetterMain, LetterSearchResultViewModel>();
            CreateMap<LetterDataSource, LetterDataSourceViewModel>();
            CreateMap<LetterDataSource, LetterDataSourceSearchResultViewModel>();
            CreateMap<LetterCustomField, LetterCustomFieldViewModel>();
            CreateMap<LetterCustomFieldViewModel, LetterCustomField>()
                .ForMember(vw => vw.LetterDataSource, opt => opt.Ignore());

            #endregion

            #region DOCXs
            //CreateMap<DOCXEntitySetting, DOCXEntitySettingViewModel>()
            //    .ForMember(vm => vm.DOCXCategory, domain => domain.MapFrom(s => s.DOCXCategory));
            //CreateMap<DOCXEntitySettingViewModel, DOCXEntitySetting>()
            //    .ForMember(m => m.DOCXCategory, opt => opt.Ignore());

            CreateMap<DOCXMain, DOCXMainDetailViewModel>()
                .ForMember(vm => vm.ScreenName, domain => domain.MapFrom(DOCXmain => DOCXmain.SystemScreen.ScreenName))
                .ForMember(vm => vm.DOCXCatDesc, domain => domain.MapFrom(DOCXmain => DOCXmain.DOCXCategory.DOCXCatDesc));

            CreateMap<DOCXMain, DOCXListViewModel>()
                .ForMember(vm => vm.SystemType, domain => domain.MapFrom(DOCXmain => DOCXmain.SystemScreen.SystemType));

            CreateMap<DOCXMainDetailViewModel, DOCXMain>()
                .ForMember(model => model.DOCXCategory, opt => opt.Ignore())
                .ForMember(model => model.SystemScreen, opt => opt.Ignore());

            CreateMap<DOCXUserDataViewModel, DOCXUserData>()
                .ForMember(model => model.DOCXMain, opt => opt.Ignore());

            CreateMap<DOCXRecordSourceFilter, DOCXFilterListViewModel>()
                .ForMember(vm => vm.RecSource, domain => domain.MapFrom(f => f.DOCXRecordSource.DOCXDataSource.DataSourceDescMain));

            //CreateMap<DOCXContactDTO, DOCXContactViewModel>()
            //    .ForMember(vm => vm.IsGenerate, domain => domain.MapFrom(c => c.IsPrevGen != true));
            CreateMap<DOCXMain, DOCXSearchResultViewModel>();
            CreateMap<DOCXDataSource, DOCXDataSourceViewModel>();

            CreateMap<DOCXRecordSource, DOCXDataSourceListViewModel>()
                .ForMember(vm => vm.RecSourceId, domain => domain.MapFrom(rs => rs.RecSourceId))
                .ForMember(vm => vm.DataSourceDescMain, domain => domain.MapFrom(rs => rs.DOCXDataSource.DataSourceDescMain));
            //.ForMember(vm => vm.DataSourceDescDtl, domain => domain.MapFrom(rs => rs.DOCXDataSource.DataSourceDescDtl));
            CreateMap<DOCXRecordSourceFilter, DOCXRecordSourceFilterViewModel>();
            CreateMap<DOCXRecordSourceFilterViewModel, DOCXRecordSourceFilter>()
                .ForMember(f => f.DOCXRecordSource, opt => opt.Ignore())
                .ForMember(f => f.RecSourceId, vw => vw.MapFrom(f => f.DOCXRecordSource.RecSourceId));
            CreateMap<DOCXRecordSourceFilterUser, DOCXRecordSourceFilterUserViewModel>();
            CreateMap<DOCXRecordSourceFilterUserViewModel, DOCXRecordSourceFilterUser>()
                .ForMember(f => f.DOCXRecordSource, opt => opt.Ignore())
                .ForMember(f => f.RecSourceId, vw => vw.MapFrom(f => f.DOCXRecordSource.RecSourceId));

            #endregion

            #region Documents
            CreateMap<Invention, DocumentInventionResultsViewModel>()
                .ForMember(vm => vm.DataKey, domain => domain.MapFrom(i => "InvId"))
                .ForMember(vm => vm.DataKeyValue, domain => domain.MapFrom(i => i.InvId));

            CreateMap<CountryApplication, DocumentCtryAppResultsViewModel>()
                .ForMember(vm => vm.DataKey, domain => domain.MapFrom(a => "AppId"))
                .ForMember(vm => vm.DataKeyValue, domain => domain.MapFrom(a => a.AppId));

            CreateMap<TmkTrademark, DocumentTrademarkResultsViewModel>()
               .ForMember(vm => vm.DataKey, domain => domain.MapFrom(i => "TmkId"))
               .ForMember(vm => vm.DataKeyValue, domain => domain.MapFrom(i => i.TmkId));

            CreateMap<PatActionDue, DocumentActionResultsViewModel>()
                .ForMember(vm => vm.DataKey, domain => domain.MapFrom(a => "ActId"))
                .ForMember(vm => vm.DataKeyValue, domain => domain.MapFrom(a => a.ActId))
                .ForMember(vm => vm.Status, domain => domain.MapFrom(a => a.CountryApplication.ApplicationStatus));

            CreateMap<TmkActionDue, DocumentActionResultsViewModel>()
                .ForMember(vm => vm.DataKey, domain => domain.MapFrom(a => "ActId"))
                .ForMember(vm => vm.DataKeyValue, domain => domain.MapFrom(a => a.ActId))
                .ForMember(vm => vm.Status, domain => domain.MapFrom(a => a.TmkTrademark.TrademarkStatus));

            CreateMap<PatCostTrack, DocumentCostResultsViewModel>()
                .ForMember(vm => vm.DataKey, domain => domain.MapFrom(a => "CostTrackId"))
                .ForMember(vm => vm.DataKeyValue, domain => domain.MapFrom(c => c.CostTrackId));

            CreateMap<TmkCostTrack, DocumentCostResultsViewModel>()
                .ForMember(vm => vm.DataKey, domain => domain.MapFrom(a => "CostTrackId"))
                .ForMember(vm => vm.DataKeyValue, domain => domain.MapFrom(c => c.CostTrackId));

            CreateMap<DocDocument, DocDocumentListViewModel>()
                .ForMember(vm => vm.DocId, domain => domain.MapFrom(d => d.DocId))
                .ForMember(vm => vm.DocName, domain => domain.MapFrom(d => d.DocName ?? ""))
                .ForMember(vm => vm.UserFileName, domain => domain.MapFrom(d => d.DocFile.UserFileName))
                .ForMember(vm => vm.DocFileName, domain => domain.MapFrom(d => d.DocFile.DocFileName))
                .ForMember(vm => vm.DateCreated, domain => domain.MapFrom(d => d.LastUpdate))
                .ForMember(vm => vm.DocTypeName, domain => domain.MapFrom(d => d.DocType != null ? d.DocType.DocTypeName : ""))
                .ForMember(vm => vm.ThumbFileName, domain => domain.MapFrom(d => d.DocFile.ThumbFileName ?? (!string.IsNullOrEmpty(d.DocUrl) ? "logo_url.png" : null)))
                .ForMember(vm => vm.IsPrivate, domain => domain.MapFrom(d => d.IsPrivate))
                .ForMember(vm => vm.LockedBy, domain => domain.MapFrom(d => d.LockedBy ?? ""))
                .ForMember(vm => vm.tStamp, domain => domain.MapFrom(d => d.tStamp))
                .ForMember(vm => vm.FolderId, domain => domain.MapFrom(d => d.FolderId))
                .ForMember(vm => vm.CreatedBy, domain => domain.MapFrom(d => d.CreatedBy))
                .ForMember(vm => vm.ScreenCode, domain => domain.MapFrom(d => d.DocFolder.ScreenCode))
                .ForMember(vm => vm.ParentId, domain => domain.MapFrom(d => d.DocFolder != null ? d.DocFolder.DataKeyValue : 0))
                .ForMember(vm => vm.DocUrl, domain => domain.MapFrom(d => d.DocUrl))
                .ForMember(vm => vm.SystemType, domain => domain.MapFrom(d => d.DocFolder.SystemType))
                .ForMember(vm => vm.FolderIsPublic, domain => domain.MapFrom(d => !d.DocFolder.IsPrivate))
                .ForMember(vm => vm.FolderName, domain => domain.MapFrom(d => d.DocFolder.FolderName))
                .ForMember(vm => vm.FolderCreatedBy, domain => domain.MapFrom(d => d.DocFolder.CreatedBy))
                .ForMember(vm => vm.IconClass, domain => domain.MapFrom(d => d.DocFile.DocIcon.IconClass))
                .ForMember(vm => vm.Tags, domain => domain.MapFrom(d => d.DocDocumentTags.Select(t => t.Tag)))
                .ForMember(vm => vm.ForSignature, domain => domain.MapFrom(d => d.DocFile.ForSignature))
                .ForMember(vm => vm.SignedDoc, domain => domain.MapFrom(d => d.DocFile.SignedDoc))
                .ForMember(vm => vm.EnvelopeId, domain => domain.MapFrom(d => d.DocFile.DocFileSignature.EnvelopeId ?? ""))
                .ForMember(vm => vm.SignatureCompleted, domain => domain.MapFrom(d => d.DocFile.DocFileSignature.SignatureCompleted ?? false))
                .ForMember(vm => vm.SentToDocuSign, domain => domain.MapFrom(d => !string.IsNullOrEmpty(d.DocFile.DocFileSignature.EnvelopeId)))
                .ForMember(vm => vm.DataKey, domain => domain.MapFrom(d => d.DocFile.DocFileSignature.DataKey))
                .ForMember(vm => vm.QESetupId, domain => domain.MapFrom(d => d.DocFile.DocFileSignature.QESetupId))
                .ForMember(vm => vm.RoleLink, domain => domain.MapFrom(d => d.DocFile.DocFileSignature.RoleLink))
                .ForMember(vm => vm.SignatureReviewed, domain => domain.MapFrom(d => d.DocFile.DocFileSignature.SignatureReviewed))
                .ForMember(vm => vm.UploadedDate, domain => domain.MapFrom(d => d.DocFile.DateCreated))
                .ForMember(vm => vm.Source, domain => domain.MapFrom(d => d.Source));

            CreateMap<DocDocumentListViewModel, DocDocument>()
                .ForMember(m => m.DocFile, opt => opt.Ignore())
                .ForMember(m => m.DocType, opt => opt.Ignore());

            CreateMap<DocDocumentViewModel, DocDocument>()
                .ForMember(m => m.DocFile, opt => opt.Ignore())
                .ForMember(m => m.DocType, opt => opt.Ignore());

            CreateMap<DocDocument, DocDocumentViewModel>()
                .ForMember(vm => vm.DocTypeName, domain => domain.MapFrom(d => d.DocType.DocTypeName))
                .ForMember(vm => vm.DocFileName, domain => domain.MapFrom(d => d.DocFile.DocFileName))
                .ForMember(vm => vm.UserFileName, domain => domain.MapFrom(d => d.DocFile.UserFileName))
                .ForMember(vm => vm.ThumbFileName, domain => domain.MapFrom(d => d.DocFile.ThumbFileName))
                .ForMember(vm => vm.IsImage, domain => domain.MapFrom(d => d.DocFile.IsImage))
                .ForMember(vm => vm.FileSize, domain => domain.MapFrom(d => d.DocFile.FileSize))
                .ForMember(vm => vm.SystemType, domain => domain.MapFrom(d => d.DocFolder.SystemType))
                .ForMember(vm => vm.ScreenCode, domain => domain.MapFrom(d => d.DocFolder.ScreenCode))
                .ForMember(vm => vm.ParentId, domain => domain.MapFrom(d => d.DocFolder != null ? d.DocFolder.DataKeyValue : 0));

            //iManage
            CreateMap<DocDocument, DocumentViewModel>();
            CreateMap<DocumentViewModel, Document>();
            CreateMap<DocumentViewModel, DocDocumentViewModel>();

            //NetDocs
            CreateMap<DocDocument, R10.Web.Models.NetDocumentsModels.DocumentViewModel>();
            CreateMap<R10.Web.Models.NetDocumentsModels.DocumentViewModel, R10.Web.Models.NetDocumentsModels.Document>();
            CreateMap<R10.Web.Models.NetDocumentsModels.DocumentViewModel, DocDocumentViewModel>();

            CreateMap<ImageViewModel, DocDocument>()
                .ForMember(m => m.DocId, vm => vm.MapFrom(d => d.ImageId));

            CreateMap<DocFolderViewModel, DocFolder>()
                .ForMember(m => m.SystemType, opt => opt.Ignore())
                .ForMember(m => m.DataKey, opt => opt.Ignore())
                .ForMember(m => m.DataKeyValue, opt => opt.Ignore())
                .ForMember(m => m.ParentFolderId, opt => opt.Ignore());

            CreateMap<CountryApplication, DocCtryAppViewModel>()
                .ForMember(vm => vm.ClientName, domain => domain.MapFrom(ca => ca.Invention.Client.ClientName))
                .ForMember(vm => vm.Attorney1, domain => domain.MapFrom(ca => ca.Invention.Attorney1.AttorneyCode))
                .ForMember(vm => vm.Attorney2, domain => domain.MapFrom(ca => ca.Invention.Attorney2.AttorneyCode))
                .ForMember(vm => vm.Attorney3, domain => domain.MapFrom(ca => ca.Invention.Attorney3.AttorneyCode));

            CreateMap<Invention, DocInventionViewModel>()
                .ForMember(vm => vm.ClientName, domain => domain.MapFrom(i => i.Client.ClientName))
                .ForMember(vm => vm.OwnerName, domain => domain.MapFrom(i => i.Owners.FirstOrDefault().Owner.OwnerName))
                .ForMember(vm => vm.Attorney1, domain => domain.MapFrom(i => i.Attorney1.AttorneyName))
                .ForMember(vm => vm.Attorney2, domain => domain.MapFrom(i => i.Attorney2.AttorneyName))
                .ForMember(vm => vm.Attorney3, domain => domain.MapFrom(i => i.Attorney3.AttorneyName));

            CreateMap<TmkTrademark, DocTrademarkViewModel>()
                .ForMember(vm => vm.ClientName, domain => domain.MapFrom(t => t.Client.ClientName))
                //.ForMember(vm => vm.OwnerName, domain => domain.MapFrom(t => t.Owner.OwnerName))
                //MULTIPLE OWNER TABLE??
                .ForMember(vm => vm.OwnerName, domain => domain.MapFrom(t => t.Owners.FirstOrDefault().Owner.OwnerName))
                .ForMember(vm => vm.AgentName, domain => domain.MapFrom(t => t.Agent.AgentName))
                .ForMember(vm => vm.Attorney1, domain => domain.MapFrom(t => t.Attorney1.AttorneyName))
                .ForMember(vm => vm.Attorney2, domain => domain.MapFrom(t => t.Attorney2.AttorneyName))
                .ForMember(vm => vm.Attorney3, domain => domain.MapFrom(t => t.Attorney3.AttorneyName))
                .ForMember(vm => vm.Attorney4, domain => domain.MapFrom(t => t.Attorney4.AttorneyName))
                .ForMember(vm => vm.Attorney5, domain => domain.MapFrom(t => t.Attorney5.AttorneyName));

            CreateMap<PatActionDue, DocPatActViewModel>()
               .ForMember(vm => vm.ClientName, domain => domain.MapFrom(ad => ad.CountryApplication.Invention.Client.ClientName))
               .ForMember(vm => vm.Attorney1, domain => domain.MapFrom(ad => ad.CountryApplication.Invention.Attorney1.AttorneyCode))
               .ForMember(vm => vm.Attorney2, domain => domain.MapFrom(ad => ad.CountryApplication.Invention.Attorney2.AttorneyCode))
               .ForMember(vm => vm.Attorney3, domain => domain.MapFrom(ad => ad.CountryApplication.Invention.Attorney3.AttorneyCode))
               .ForMember(vm => vm.AppNumber, domain => domain.MapFrom(ad => ad.CountryApplication.AppNumber))
               .ForMember(vm => vm.FilDate, domain => domain.MapFrom(ad => ad.CountryApplication.FilDate))
               .ForMember(vm => vm.PatNumber, domain => domain.MapFrom(ad => ad.CountryApplication.PatNumber))
               .ForMember(vm => vm.IssDate, domain => domain.MapFrom(ad => ad.CountryApplication.IssDate))
               .ForMember(vm => vm.CaseType, domain => domain.MapFrom(ad => ad.CountryApplication.CaseType))
               .ForMember(vm => vm.Status, domain => domain.MapFrom(ad => ad.CountryApplication.ApplicationStatus))
               .ForMember(vm => vm.AppTitle, domain => domain.MapFrom(ad => ad.CountryApplication.AppTitle));

            CreateMap<TmkActionDue, DocTmkActViewModel>()
              .ForMember(vm => vm.ClientName, domain => domain.MapFrom(ad => ad.TmkTrademark.Client.ClientName))
              .ForMember(vm => vm.Attorney1, domain => domain.MapFrom(ad => ad.TmkTrademark.Attorney1.AttorneyCode))
              .ForMember(vm => vm.Attorney2, domain => domain.MapFrom(ad => ad.TmkTrademark.Attorney2.AttorneyCode))
              .ForMember(vm => vm.Attorney3, domain => domain.MapFrom(ad => ad.TmkTrademark.Attorney3.AttorneyCode))
              .ForMember(vm => vm.Attorney4, domain => domain.MapFrom(ad => ad.TmkTrademark.Attorney4.AttorneyCode))
              .ForMember(vm => vm.Attorney5, domain => domain.MapFrom(ad => ad.TmkTrademark.Attorney5.AttorneyCode))
              .ForMember(vm => vm.AppNumber, domain => domain.MapFrom(ad => ad.TmkTrademark.AppNumber))
              .ForMember(vm => vm.FilDate, domain => domain.MapFrom(ad => ad.TmkTrademark.FilDate))
              .ForMember(vm => vm.RegNumber, domain => domain.MapFrom(ad => ad.TmkTrademark.RegNumber))
              .ForMember(vm => vm.RegDate, domain => domain.MapFrom(ad => ad.TmkTrademark.RegDate))
              .ForMember(vm => vm.CaseType, domain => domain.MapFrom(ad => ad.TmkTrademark.CaseType))
              .ForMember(vm => vm.Status, domain => domain.MapFrom(ad => ad.TmkTrademark.TrademarkStatus))
              .ForMember(vm => vm.TrademarkName, domain => domain.MapFrom(ad => ad.TmkTrademark.TrademarkName));

            CreateMap<PatCostTrack, DocPatCostViewModel>()
              .ForMember(vm => vm.ClientName, domain => domain.MapFrom(ct => ct.CountryApplication.Invention.Client.ClientName))
              .ForMember(vm => vm.AppNumber, domain => domain.MapFrom(ct => ct.CountryApplication.AppNumber))
              .ForMember(vm => vm.FilDate, domain => domain.MapFrom(ct => ct.CountryApplication.FilDate))
              .ForMember(vm => vm.PatNumber, domain => domain.MapFrom(ct => ct.CountryApplication.PatNumber))
              .ForMember(vm => vm.IssDate, domain => domain.MapFrom(ct => ct.CountryApplication.IssDate))
              .ForMember(vm => vm.CaseType, domain => domain.MapFrom(ct => ct.CountryApplication.CaseType))
              .ForMember(vm => vm.Status, domain => domain.MapFrom(ct => ct.CountryApplication.ApplicationStatus))
              .ForMember(vm => vm.AppTitle, domain => domain.MapFrom(ct => ct.CountryApplication.AppTitle));

            CreateMap<TmkCostTrack, DocTmkCostViewModel>()
              .ForMember(vm => vm.ClientName, domain => domain.MapFrom(ct => ct.TmkTrademark.Client.ClientName))
              .ForMember(vm => vm.AppNumber, domain => domain.MapFrom(ct => ct.TmkTrademark.AppNumber))
              .ForMember(vm => vm.FilDate, domain => domain.MapFrom(ct => ct.TmkTrademark.FilDate))
              .ForMember(vm => vm.RegNumber, domain => domain.MapFrom(ct => ct.TmkTrademark.RegNumber))
              .ForMember(vm => vm.RegDate, domain => domain.MapFrom(ct => ct.TmkTrademark.RegDate))
              .ForMember(vm => vm.CaseType, domain => domain.MapFrom(ct => ct.TmkTrademark.CaseType))
              .ForMember(vm => vm.Status, domain => domain.MapFrom(ct => ct.TmkTrademark.TrademarkStatus))
              .ForMember(vm => vm.TrademarkName, domain => domain.MapFrom(ct => ct.TmkTrademark.TrademarkName));

            CreateMap<DocFixedFolder, DocFixedFolderViewModel>();
            CreateMap<DocFolder, DocFolderViewModel>();
            CreateMap<EFS, EFSViewModel>()
                    .ForMember(vm => vm.SignatureQETemplateName, domain => domain.MapFrom(d => d.QEMain.TemplateName));

            #endregion

            #region Data Query
            CreateMap<DataQueryMain, DataQuerySearchResultViewModel>();
            CreateMap<DataQueryMain, DataQueryDetailViewModel>();
            CreateMap<DataQueryMain, DataQueryDetailViewModel>()
                .ForMember(vm => vm.Tags, domain => domain.MapFrom(dq => dq.DataQueryTags.Select(t => t.Tag)))
                .ForMember(vm => vm.DQCat, domain => domain.MapFrom(dq => dq.DataQueryCategory.DQCat))
                ;
            CreateMap<DataQueryDetailViewModel, DataQueryMain>()
                 .ForMember(model => model.DataQueryCategory, opt => opt.Ignore());
            #endregion
            #region Forms Extraction
            CreateMap<FormIFWActMap, FormIFWActionMapViewModel>()
                .ForMember(vm => vm.DocDesc, domain => domain.MapFrom(f => f.FormIFWDocType.DocDesc))
                .ForMember(vm => vm.IsGenActionDE, domain => domain.MapFrom(f => f.IsGenAction))
                .ForMember(vm => vm.IsCompareDE, domain => domain.MapFrom(f => f.IsCompare))
                .ForMember(vm => vm.SystemType, domain => domain.MapFrom(f => f.FormIFWDocType.SystemType));

            CreateMap<FormIFWActionMapViewModel, FormIFWActMap>()
               .ForMember(dom => dom.IsCompare, dom => dom.MapFrom(vm => vm.IsCompareDE))
               .ForMember(dom => dom.IsGenAction, dom => dom.MapFrom(vm => vm.IsGenActionDE))
               .ForMember(dom => dom.FormIFWDocType, opt => opt.Ignore());

            CreateMap<PatActionParameter, FormIFWDueDateViewModel>();

            #endregion

            #region Time Tracker
            CreateMap<TimeTracker, TimeTrackerViewModel>()
                .ForMember(vm => vm.TimeTrackerClientCode, domain => domain.MapFrom(f => f.SystemType.Equals("P") ? f.CountryApplication.Invention.Client.ClientCode : f.TmkTrademark.Client.ClientCode))
                .ForMember(vm => vm.SystemType, domain => domain.MapFrom(f => f.SystemType.Equals("P") ? "Patent" : "Trademark"))
                .ForMember(vm => vm.CaseNumber, domain => domain.MapFrom(f => f.SystemType.Equals("P") ? f.CountryApplication.CaseNumber : f.TmkTrademark.CaseNumber))
                .ForMember(vm => vm.SubCase, domain => domain.MapFrom(f => f.SystemType.Equals("P") ? f.CountryApplication.SubCase : f.TmkTrademark.SubCase))
                .ForMember(vm => vm.Country, domain => domain.MapFrom(f => f.SystemType.Equals("P") ? f.CountryApplication.Country : f.TmkTrademark.Country))
                .ForMember(vm => vm.Title, domain => domain.MapFrom(f => f.SystemType.Equals("P") ? f.CountryApplication.AppTitle : f.TmkTrademark.TrademarkName))
                .ForMember(vm => vm.CaseType, domain => domain.MapFrom(f => f.SystemType.Equals("P") ? f.CountryApplication.CaseType : f.TmkTrademark.CaseType))
                .ForMember(vm => vm.Status, domain => domain.MapFrom(f => f.SystemType.Equals("P") ? f.CountryApplication.ApplicationStatus : f.TmkTrademark.TrademarkStatus))
                .ForMember(vm => vm.ApplicationNumber, domain => domain.MapFrom(f => f.SystemType.Equals("P") ? f.CountryApplication.AppNumber : f.TmkTrademark.AppNumber))
                .ForMember(vm => vm.FilDate, domain => domain.MapFrom(f => f.SystemType.Equals("P") ? f.CountryApplication.FilDate : f.TmkTrademark.FilDate));
            CreateMap<TimeTrackerViewModel, TimeTrackerExportToExcelViewModel>();
            #endregion

            CreateMap<Product, ProductSearchResultViewModel>();
            CreateMap<Product, ProductDetailViewModel>();


            CreateMap<Message, MailListViewModel>()
                .ForMember(vm => vm.Id, domain => domain.MapFrom(m => m.Id))
                .ForMember(vm => vm.InternetMessageId, domain => domain.MapFrom(m => m.InternetMessageId))
                .ForMember(vm => vm.Name, domain => domain.MapFrom(m => m.From == null ? "" : m.From.EmailAddress.Name))
                .ForMember(vm => vm.Address, domain => domain.MapFrom(m => m.From == null ? "" : m.From.EmailAddress.Address))
                .ForMember(vm => vm.ToRecipients, domain => domain.MapFrom(m => string.Join(", ", m.ToRecipients.Select(r => r.EmailAddress.Name).ToArray())))
                .ForMember(vm => vm.CcRecipients, domain => domain.MapFrom(m => string.Join(", ", m.CcRecipients.Select(r => r.EmailAddress.Name).ToArray())))
                .ForMember(vm => vm.Subject, domain => domain.MapFrom(m => m.Subject))
                .ForMember(vm => vm.BodyPreview, domain => domain.MapFrom(m => m.BodyPreview))
                .ForMember(vm => vm.ReceivedDateTime, domain => domain.MapFrom(m => ((DateTimeOffset)m.ReceivedDateTime).DateTime))
                .ForMember(vm => vm.HasAttachments, domain => domain.MapFrom(m => m.HasAttachments))
                .ForMember(vm => vm.IsRead, domain => domain.MapFrom(m => m.IsRead))
                .ForMember(vm => vm.DownloadFileName, domain => domain.MapFrom(m => m.GetDownloadFileName()));

            CreateMap<SharePointGraphDriveItemViewModel, SharePointDocumentViewModel>()
               .ForMember(vm => vm.Folder, domain => domain.MapFrom(m => m.Path))
               .ForMember(vm => vm.Id, domain => domain.MapFrom(m => m.DriveItem.Id))
               //.ForMember(vm => vm.Title, domain => domain.MapFrom(m => m.DriveItem.Title))
               .ForMember(vm => vm.Name, domain => domain.MapFrom(m => m.DriveItem.Name))
               .ForMember(vm => vm.DateCreated_Offset, domain => domain.MapFrom(m => m.DriveItem.CreatedDateTime))
               .ForMember(vm => vm.DateModified_Offset, domain => domain.MapFrom(m => m.DriveItem.LastModifiedDateTime))
               .ForMember(vm => vm.CreatedBy, domain => domain.MapFrom(m => m.DriveItem.CreatedBy.User.AdditionalData["email"]))
               // .ForMember(vm => vm.CreatedBy, domain => domain.MapFrom(m => m.DriveItem.CreatedBy.User.DisplayName))
               .ForMember(vm => vm.ModifiedBy, domain => domain.MapFrom(m => m.DriveItem.LastModifiedBy == null ? "" : m.DriveItem.LastModifiedBy.User.DisplayName))
               .ForMember(vm => vm.ListItemId, domain => domain.MapFrom(m => m.DriveItem.ListItem.Id))
               .ForMember(vm => vm.ListItemFields, domain => domain.MapFrom(m => m.DriveItem.ListItem.Fields.AdditionalData))
               //.ForMember(vm => vm.EditUrl, domain => domain.MapFrom(m => m.DriveItem.EditUrl))
               //.ForMember(vm => vm.IsCheckedOut, domain => domain.MapFrom(m => m.DriveItem.IsCheckedOut))
               //.ForMember(vm => vm.CheckOutUser, domain => domain.MapFrom(m => m.DriveItem.CheckOutUser))
               //.ForMember(vm => vm.DownloadUrl, domain => domain.MapFrom(m => m.DriveItem.DownloadUrl))
               //.ForMember(vm => vm.PreviewUrl, domain => domain.MapFrom(m => m.DriveItem.PreviewUrl))
               .ForMember(vm => vm.EditUrl, domain => domain.MapFrom(m => m.DriveItem.WebUrl));

            #region Document Verification
            CreateMap<DocVerification, DocVerificationViewModel>();
            CreateMap<DocVerificationViewModel, DocVerification>()
                .ForMember(m => m.DocDocument, opt => opt.Ignore());
            CreateMap<DocVerificationDetail, DocVerification>()
                .ForMember(m => m.DocDocument, opt => opt.Ignore());

            CreateMap<DocumentVerificationSearchCriteriaDTO, DocumentVerificationSearchCriteriaViewModel>()
               .ForMember(vm => vm.Patent, opt => opt.Ignore())
               .ForMember(vm => vm.Trademark, opt => opt.Ignore())
               .ForMember(vm => vm.GeneralMatter, opt => opt.Ignore());

            CreateMap<DocumentVerificationSearchCriteriaViewModel, DocumentVerificationSearchCriteriaDTO>()
                .ForMember(dto => dto.DocName, vm => vm.MapFrom(c => c.DocNames != null && c.DocNames.Length > 0 ? "|" + string.Join("|", c.DocNames) + "|" : null))
                .ForMember(dto => dto.DocUploadedBy, vm => vm.MapFrom(c => c.DocUploadedBys != null && c.DocUploadedBys.Length > 0 ? "|" + string.Join("|", c.DocUploadedBys) + "|" : null))
                .ForMember(dto => dto.Country, vm => vm.MapFrom(c => c.Countries != null && c.Countries.Length > 0 ? "|" + string.Join("|", c.Countries) + "|" : null))
                .ForMember(dto => dto.Source, vm => vm.MapFrom(c => c.Sources != null && c.Sources.Length > 0 ? "|" + string.Join("|", c.Sources) + "|" : null))
                .ForMember(dto => dto.ActCreatedBy, vm => vm.MapFrom(c => c.ActCreatedBys != null && c.ActCreatedBys.Length > 0 ? "|" + string.Join("|", c.ActCreatedBys) + "|" : null))
                .ForMember(dto => dto.Client, vm => vm.MapFrom(c => c.Clients != null && c.Clients.Length > 0 ? "|" + string.Join("|", c.Clients) + "|" : null))
                .ForMember(dto => dto.Attorney, vm => vm.MapFrom(c => c.Attorneys != null && c.Attorneys.Length > 0 ? "|" + string.Join("|", c.Attorneys) + "|" : null))
                .ForMember(dto => dto.ActionType, vm => vm.MapFrom(c => c.ActionTypes != null && c.ActionTypes.Length > 0 ? "|" + string.Join("|", c.ActionTypes) + "|" : null))
                .ForMember(dto => dto.FilterAtty, vm => vm.MapFrom(c => "|" + c.AttorneyFilter1 + "|" + c.AttorneyFilter2 + "|" + c.AttorneyFilter3 + "|" + c.AttorneyFilter4 + "|" + c.AttorneyFilter5 + "|" + c.AttorneyFilterR + "|" + c.AttorneyFilterD + "|" + c.AttorneyFilterRD + "|"))
                .ForMember(dto => dto.RespDocketing, vm => vm.MapFrom(c => c.RespDocketings != null && c.RespDocketings.Length > 0 ? "|" + string.Join("|", c.RespDocketings) + "|" : null))
                .ForMember(dto => dto.RespReporting, vm => vm.MapFrom(c => c.RespReportings != null && c.RespReportings.Length > 0 ? "|" + string.Join("|", c.RespReportings) + "|" : null));

            CreateMap<DocumentVerificationNewDTO, DocVerificationNewPrintViewModel>();
            CreateMap<DocumentVerificationDTO, DocVerificationDocPrintViewModel>();
            CreateMap<DocumentVerificationActionDTO, DocVerificationActionDocPrintViewModel>();
            CreateMap<DocumentVerificationCommunicationDTO, DocVerificationCommDocPrintViewModel>();
            CreateMap<DocVerificationReviewFilters, DocVerificationReviewFilterViewModel>();
            CreateMap<DocVerificationReviewFilterViewModel, DocVerificationReviewFilters>()
                .ForMember(vm => vm.CountryFilter, vm => vm.MapFrom(c => c.Countries != null && c.Countries.Length > 0 ? "|" + string.Join("|", c.Countries) + "|" : ""))
                .ForMember(vm => vm.ClientFilter, vm => vm.MapFrom(c => c.Clients != null && c.Clients.Length > 0 ? "|" + string.Join("|", c.Clients) + "|" : ""))
                .ForMember(vm => vm.CaseTypeFilter, vm => vm.MapFrom(c => c.CaseTypes != null && c.CaseTypes.Length > 0 ? "|" + string.Join("|", c.CaseTypes) + "|" : ""));
            #endregion

        }
    }



}
