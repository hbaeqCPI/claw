using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using LawPortal.Core.Entities;
using LawPortal.Core.Entities.Shared;
using LawPortal.Core.Identity;
using LawPortal.Core.Interfaces;
using LawPortal.Web.Areas.Admin.ViewModels;
using LawPortal.Web.Areas.Shared.ViewModels;
using LawPortal.Web.Extensions;
using LawPortal.Web.Interfaces;
using LawPortal.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Claims;
using LawPortal.Core.Helpers;
using LawPortal.Web.Services;

namespace LawPortal.Web.Areas.Admin.Services
{
    public class UserAccountService : IUserAccountService
    {
        protected readonly CPiUserManager _userManager;
        protected readonly ICPiUserPermissionManager _permissionManager;
        protected readonly IEmailSender _emailSender;
        //protected readonly IEmailTemplateService _emailTemplateService;
        protected readonly ISystemSettings<DefaultSetting> _defaultSettings;
        protected readonly INotificationSettingManager _settingsManager;
        protected readonly IStringLocalizer<SharedResource> _localizer;
        protected readonly ClaimsPrincipal _user;

        public UserAccountService(
            CPiUserManager userManager,
            ICPiUserPermissionManager permissionManager,
            IEmailSender emailSender,
            ISystemSettings<DefaultSetting> defaultSettings,
            INotificationSettingManager settingsManager,
            IStringLocalizer<SharedResource> localizer,
            ClaimsPrincipal user
            )
        {
            _userManager = userManager;
            _permissionManager = permissionManager;
            _emailSender = emailSender;
            _defaultSettings = defaultSettings;
            _settingsManager = settingsManager;
            _localizer = localizer;
            _user = user;
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

        // The DB-driven EmailTemplateService was removed, so these notifications are built in code
        // the same way as the ones in EmailSenderExtensions.
        public async Task<EmailSenderResult> SendNewPassword(string locale, string emailType, UserAccountEmail data)
        {
            if (string.IsNullOrEmpty(data?.Email))
                return new EmailSenderResult() { ErrorMessage = "Unable to send login information: no email address." };

            var defaultSettings = await _defaultSettings.GetSetting();
            var isTemporary = string.Equals(emailType, defaultSettings.TemporaryPasswordNotification, StringComparison.OrdinalIgnoreCase);

            var loginUrl = data.CallToActionUrl;
            var body = Logo(data.LogoUrl) +
                $"<p>{Text("Hi", locale)} {data.FirstName},</p>" +
                $"<p>{Text("Your CPI account has been successfully setup. Please click the button below to login or copy and paste the URL to your browser's address bar:", locale)}</p>" +
                $"<p>{EmailSenderExtensions.LinkButton(CallToAction(data.CallToAction, locale), loginUrl)}</p>" +
                $"<p><strong>{Text("CPI URL", locale)}:</strong> {loginUrl}<br>" +
                $"<strong>{Text("User Name", locale)}:</strong> {data.Email}<br>" +
                $"<strong>{Text(isTemporary ? "Your temporary CPI password" : "Your CPI password", locale)}:</strong> {data.Password}</p>" +
                (isTemporary ? $"<p>{Text("You will be asked to change your password after successfully logging in.", locale)}</p>" : "");

            return await _emailSender.SendEmailAsync(data.Email, Text("CPI Login Information", locale), body);
        }

        public async Task<EmailSenderResult> SendApprovalNotification(string locale, UserAccountApprovalNotification data)
        {
            if (string.IsNullOrEmpty(data?.Email))
                return new EmailSenderResult() { ErrorMessage = "Unable to send account approval notification: no email address." };

            var body = Logo(data.LogoUrl) +
                $"<p>{Text("Hi", locale)} {data.FirstName},</p>" +
                $"<p>{Text("Your CPI account has been approved. Please click the button below to login:", locale)}</p>" +
                $"<p>{EmailSenderExtensions.LinkButton(CallToAction(data.CallToAction, locale), data.CallToActionUrl)}</p>" +
                $"<p><strong>{Text("User Name", locale)}:</strong> {data.Email}</p>";

            return await _emailSender.SendEmailAsync(data.Email, Text("CPI Account Approved", locale), body);
        }

        // OutlookService was removed during debloat, so there is nothing to register or notify about.
        public Task<EmailSenderResult> SendOutlookAddInRegistration(string locale, string emailType, OutlookAddInRegistration data)
        {
            return Task.FromResult(new EmailSenderResult() { ErrorMessage = "Outlook Add-In registration is not available." });
        }

        public async Task<EmailSenderResult> SendUserRegistrationNotification(UserRegistrationNotification data)
        {
            var recipients = await _settingsManager.GetRegistrationApprovalNotificationRecipients();

            if (recipients == null || !recipients.Any())
                return new EmailSenderResult() { ErrorMessage = "No registration approval notification recipients configured." };

            var locale = recipients.First().Locale;
            var body = Logo(data.LogoUrl) +
                $"<p>{Text("A new user has registered and is waiting for approval:", locale)}</p>" +
                $"<p><strong>{Text("Name", locale)}:</strong> {data.UserFirstName} {data.UserLastName}<br>" +
                $"<strong>{Text("Email", locale)}:</strong> {data.UserEmail}<br>" +
                $"<strong>{Text("User Type", locale)}:</strong> {Text(data.UserType.ToString(), locale)}<br>" +
                $"<strong>{Text("Status", locale)}:</strong> {Text(data.UserStatus.ToString(), locale)}</p>" +
                $"<p>{EmailSenderExtensions.LinkButton(CallToAction(data.CallToAction, locale), data.CallToActionUrl, 240)}</p>";

            return await _emailSender.SendEmailAsync(recipients.Select(r => r.MailAddress).ToList(), Text("CPI User Registration Approval", locale), body);
        }

        private string Text(string text, string locale) => _localizer.GetStringWithCulture(text, locale);

        private string CallToAction(string callToAction, string locale)
            => string.IsNullOrEmpty(callToAction) ? Text("Login", locale) : callToAction;

        private static string Logo(string logoUrl)
            => string.IsNullOrEmpty(logoUrl) ? "" : $"<p><img src=\"{logoUrl}\" alt=\"Computer Packages Inc.\" style=\"max-height:60px;\"></p>";

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

                // PatInventorService removed during debloat - inventor linking not available
            }
            // ContactPersonService and AttorneyService removed during debloat
            /*else if (user.UserType == CPiUserType.ContactPerson)
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
            }*/
        }
    }
}
