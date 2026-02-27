using Microsoft.AspNetCore.Identity;
using R10.Core.Entities;
using R10.Core.Identity;
using R10.Web.Areas.Admin.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Services.EmailAddIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IUserAccountService
    {
        /// <summary>
        /// Creates new DecisionMaker user if account does not exists.
        /// </summary>
        /// <param name="userType"></param>
        /// <param name="systemType"></param>
        /// <param name="email"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="contactId"></param>
        /// <param name="requireChangePassword"></param>
        /// <returns>New password if new user account was created, otherwise empty string.</returns>
        Task<string> CreateDecisionMakerUser(CPiUserType userType, string systemType, string email, string firstName, string lastName, int contactId, bool requireChangePassword);

        Task<string> GetDefaultNewPasswordNotification(bool requireChangePassword);

        Task<RegisterClientResult> RegisterOutlookAddInClient(string email);

        Task<bool> DeleteOutlookAddInClient(string clientId);

        Task LinkUserAccount(CPiUser user);

        Task<EmailSenderResult> SendNewPassword(string locale, string emailType, UserAccountEmail model);

        Task<EmailSenderResult> SendApprovalNotification(string locale, UserAccountApprovalNotification model);

        Task<EmailSenderResult> SendOutlookAddInRegistration(string locale, string emailType, OutlookAddInRegistration model);

        Task<EmailSenderResult> SendUserRegistrationNotification(UserRegistrationNotification model);
    }
}
