using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Web.Areas.Admin.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Interfaces;
using R10.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using R10.Core.Entities.Patent;
using System.Security.Claims;
using R10.Core.Helpers;
using R10.Web.Services;

namespace R10.Web.Areas.Admin.Services
{
    public class UserAccountService : IUserAccountService
    {
        protected readonly CPiUserManager _userManager;
        protected readonly ICPiUserPermissionManager _permissionManager;
        protected readonly IEmailSender _emailSender;
        protected readonly IEmailTemplateService _emailTemplateService;
        protected readonly ISystemSettings<DefaultSetting> _defaultSettings;
        protected readonly INotificationSettingManager _settingsManager;
        protected readonly IStringLocalizer<SharedResource> _localizer;
        //protected readonly IEntityService<PatInventor> _inventorService;
        protected readonly IPatInventorService _inventorService;
        protected readonly IContactPersonService _contactPersonService;
        protected readonly IAttorneyService _attorneyService;
        protected readonly ClaimsPrincipal _user;

        public UserAccountService(
            CPiUserManager userManager,
            ICPiUserPermissionManager permissionManager,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService,
            ISystemSettings<DefaultSetting> defaultSettings,
            INotificationSettingManager settingsManager,
            IStringLocalizer<SharedResource> localizer,
            ClaimsPrincipal user,
            //IEntityService<PatInventor> inventorService,
            IPatInventorService inventorService,
            IContactPersonService contactPersonService,
            IAttorneyService attorneyService
            )
        {
            _userManager = userManager;
            _permissionManager = permissionManager;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _defaultSettings = defaultSettings;
            _settingsManager = settingsManager;
            _localizer = localizer;
            _user = user;
            _inventorService = inventorService;
            _contactPersonService = contactPersonService;
            _attorneyService = attorneyService;
        }

        public async Task<string> CreateDecisionMakerUser(CPiUserType userType, string systemType, string email, string firstName, string lastName, int contactId, bool requireChangePassword)
        {
            var newPassword = string.Empty;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new CPiUser()
                {
                    UserName = email,
                    Email = email,
                    FirstName = string.IsNullOrEmpty(firstName) ? lastName : firstName, //First name is not a required field in contact person table
                    LastName = lastName,
                    UserType = userType,
                    Status = CPiUserStatus.Approved
                };
                user.EntityFilterType = user.DefaultEntityFilterType;

                if (!requireChangePassword)
                    user.LastPasswordChangeDate = DateTime.Now;

                newPassword = _userManager.GenerateRandomPassword();
                var result = await _userManager.CreateAsync(user, newPassword);

                if (!result.Succeeded)
                    throw new Exception(result.Errors.FirstOrDefault()?.Description);
            }

            if (user.UserType == CPiUserType.ContactPerson || user.UserType == CPiUserType.Attorney)
            {
                try
                {
                    await _permissionManager.LinkEntity(user, contactId, user.UserType == CPiUserType.Attorney ? CPiEntityType.Attorney : CPiEntityType.ContactPerson);
                    await _permissionManager.SetDecisionMakerRole(user, true, systemType);
                }
                catch
                {
                    if (!string.IsNullOrEmpty(newPassword))
                        await _userManager.DeleteAsync(user);

                    throw;
                }
            }

            return newPassword;
        }

        public async Task<string> GetDefaultNewPasswordNotification(bool requireChangePassword)
        {
            var defaultSettings = await _defaultSettings.GetSetting();
            return requireChangePassword ? defaultSettings.TemporaryPasswordNotification : defaultSettings.NewPasswordNotification;
        }

        public async Task<EmailSenderResult> SendNewPassword(string locale, string emailType, UserAccountEmail data)
        {
            var emailMessage = await _emailTemplateService.GetEmailMessage(emailType, locale, data);

            if (emailMessage == null)
                return new EmailSenderResult() { ErrorMessage = "Email template not found. Unable to send email." };
            else
                return await _emailSender.SendEmailAsync(data.Email, emailMessage.Subject, emailMessage.Body);
        }

        public async Task<EmailSenderResult> SendApprovalNotification(string locale, UserAccountApprovalNotification data)
        {
            var defaultSettings = await _defaultSettings.GetSetting();
            var emailMessage = await _emailTemplateService.GetEmailMessage(defaultSettings.AccountApprovalNotification, locale, data);

            if (emailMessage == null)
                return new EmailSenderResult() { ErrorMessage = "Email template not found. Unable to send email." };
            else
                return await _emailSender.SendEmailAsync(data.Email, emailMessage.Subject, emailMessage.Body);
        }

        public async Task<EmailSenderResult> SendOutlookAddInRegistration(string locale, string emailType, OutlookAddInRegistration data)
        {
            var emailMessage = await _emailTemplateService.GetEmailMessage(emailType, locale, data);

            if (emailMessage == null)
                return new EmailSenderResult() { ErrorMessage = "Email template not found. Unable to send email." };
            else
                return await _emailSender.SendEmailAsync(data.Email, emailMessage.Subject, emailMessage.Body);
        }

        public async Task<EmailSenderResult> SendUserRegistrationNotification(UserRegistrationNotification data)
        {
            var recipients = await _settingsManager.GetRegistrationApprovalNotificationRecipients();

            if (recipients.Count > 0)
            {
                var defaultSettings = await _defaultSettings.GetSetting();

                //group email by locale
                foreach (var locale in recipients.Select(m => m.Locale).Distinct().ToList())
                {
                    data.CallToAction = _localizer.GetStringWithCulture(data.CallToAction, locale);
                    var emailMessage = await _emailTemplateService.GetEmailMessage(defaultSettings.PendingRegistrationNotification, locale, data);

                    if (emailMessage != null)
                        await _emailSender.SendEmailAsync(recipients.Where(m => m.Locale == locale).Select(m => m.MailAddress).ToList(), emailMessage.Subject, emailMessage.Body);
                    else
                        return new EmailSenderResult() { ErrorMessage = $"Email template '{defaultSettings.PendingRegistrationNotification}' with language '{locale}' not found. Unable to send email." };
                }
                return new EmailSenderResult() { Success = true };
            }
            else
                return new EmailSenderResult() { ErrorMessage = "User registration notification recipient not found. Unable to send email." };
        }

        public Task<RegisterClientResult> RegisterOutlookAddInClient(string email)
        {
            // OutlookService removed during debloat
            return Task.FromResult(new RegisterClientResult());
        }

        public Task<bool> DeleteOutlookAddInClient(string clientId)
        {
            // OutlookService removed during debloat
            return Task.FromResult(false);
        }

        private string GenerateUserCode(CPiUser user, int length)
        {
            var code = ($"{(user.FirstName ?? "")[0]}{user.LastName}").Replace(" ", "").ToUpper();

            if (user.LastName?.Length > length)
                return code.Substring(0, length);

            return code;
        }

        public async Task LinkUserAccount(CPiUser user)
        {
            if (user.UserType == CPiUserType.Inventor)
            {
                //check if already linked
                var hasLink = await _permissionManager.CPiUserEntityFilters.AnyAsync(e => e.UserId == user.Id);

                if (!hasLink)
                {
                    //get inventor by email
                    var inventorId = await _inventorService.QueryableList.Where(e => !string.IsNullOrEmpty(e.EMail) && e.EMail.ToLower() == user.Email.ToLower()).Select(e => e.InventorID).FirstOrDefaultAsync();

                    //create new inventor if email not found
                    if (inventorId == 0)
                    {
                        var createdBy = _user.GetUserName();
                        var dateCreated = DateTime.Now;
                        var inventor = new PatInventor()
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            EMail = user.Email,
                            Language = await _inventorService.GetLanguage(user.Locale),
                            CreatedBy = createdBy,
                            DateCreated = dateCreated,
                            UpdatedBy = createdBy,
                            LastUpdate = dateCreated
                        };

                        await _inventorService.Add(inventor);
                        inventorId = inventor.InventorID;
                    }

                    //link inventor
                    await _permissionManager.LinkEntity(user, inventorId, CPiEntityType.Inventor);
                }
            }
            else if (user.UserType == CPiUserType.ContactPerson)
            {
                //check if already linked
                var hasLink = await _permissionManager.CPiUserEntityFilters.AnyAsync(e => e.UserId == user.Id);

                if (!hasLink)
                {
                    //get contact person by email
                    var contactId = await _contactPersonService.QueryableList.Where(e => !string.IsNullOrEmpty(e.EMail) && e.EMail.ToLower() == user.Email.ToLower()).Select(e => e.ContactID).FirstOrDefaultAsync();

                    //create new contact person if email not found
                    if (contactId == 0)
                    {
                        var createdBy = _user.GetUserName();
                        var dateCreated = DateTime.Now;
                        var contactPerson = new ContactPerson()
                        {
                            Contact = GenerateUserCode(user, 10),
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            EMail = user.Email,
                            Language = await _contactPersonService.GetLanguage(user.Locale),
                            CreatedBy = createdBy,
                            DateCreated = dateCreated,
                            UpdatedBy = createdBy,
                            LastUpdate = dateCreated
                        };

                        await _contactPersonService.Add(contactPerson);
                        contactId = contactPerson.ContactID;
                    }

                    //link contact person
                    await _permissionManager.LinkEntity(user, contactId, CPiEntityType.ContactPerson);
                }
            }
            else if (user.UserType == CPiUserType.Attorney)
            {
                //check if already linked
                var hasLink = await _permissionManager.CPiUserEntityFilters.AnyAsync(e => e.UserId == user.Id);

                if (!hasLink)
                {
                    //get attorney by email
                    var attorneyId = await _attorneyService.QueryableList.Where(e => !string.IsNullOrEmpty(e.EMail) && e.EMail.ToLower() == user.Email.ToLower()).Select(e => e.AttorneyID).FirstOrDefaultAsync();

                    //create new attorney if email not found
                    if (attorneyId == 0)
                    {
                        var createdBy = _user.GetUserName();
                        var dateCreated = DateTime.Now;
                        var attorney = new Attorney()
                        {
                            AttorneyCode = GenerateUserCode(user, 5),
                            AttorneyName = $"{user.FirstName} {user.LastName}",
                            EMail = user.Email,
                            Language = await _contactPersonService.GetLanguage(user.Locale),
                            CreatedBy = createdBy,
                            DateCreated = dateCreated,
                            UpdatedBy = createdBy,
                            LastUpdate = dateCreated
                        };

                        await _attorneyService.Add(attorney);
                        attorneyId = attorney.AttorneyID;
                    }

                    //link attorney
                    await _permissionManager.LinkEntity(user, attorneyId, CPiEntityType.Attorney);
                }
            }
        }
    }
}
