using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public static class EmailTemplateExtensions
    {
        public static async Task<EmailMessage> GetEmailMessage<T>(this IEmailTemplateService emailTemplate, string emailTypeName, string languageCulture, T data) where T : EmailContent
        {
            var emailMessage = await emailTemplate.GetEmailMessage(emailTypeName, languageCulture);

            if (emailMessage != null)
                emailMessage.ApplyData(data);

            return emailMessage;
        }

        public static  void ApplyData<T>(this EmailMessage emailMessage, T data) where T : EmailContent
        {
            PropertyInfo[] properties = data.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var tag = "{{" + property.Name + "}}";
                var value = property.GetValue(data);

                if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                    value = ((DateTime?)value).FormatToDisplay();

                emailMessage.Subject = (emailMessage.Subject ?? "").Replace(tag, (value ?? "").ToString());
                emailMessage.Body = (emailMessage.Body ?? "").Replace(tag, (value ?? "").ToString());
            }
        }

        public static string RemoveSection(this string s, string tag)
        {
            while (s.Contains($"<{tag}>") && s.Contains($"</{tag}>"))
                s = s.Replace($"<{tag}>{s.ExtractString(tag)}</{tag}>", "");

            return s;
        }

        public static string ExtractString(this string s, string tag)
        {
            var startTag = "<" + tag + ">";
            int startIndex = s.IndexOf(startTag) + startTag.Length;
            int endIndex = s.IndexOf("</" + tag + ">", startIndex);
            return s.Substring(startIndex, endIndex - startIndex);
        }

        public static async Task<List<EmailTypeListViewModel>> GetEmailTypes(this IParentEntityService<EmailType, EmailSetup> emailTypeService, string contentType)
        {
            return await emailTypeService.QueryableList
                            .Where(t => t.IsEnabled && t.ContentType == contentType)
                            .Select(t => new EmailTypeListViewModel() { Name = t.Name, Description = t.Description ?? "" })
                            .OrderBy(t => t.Name)
                            .ToListAsync();
        }

        public static async Task<bool> Any(this IParentEntityService<EmailType, EmailSetup> emailTypeService, string name, string contentType)
        {
            return await emailTypeService.QueryableList
                            .AnyAsync(t => t.IsEnabled && t.Name == name && t.ContentType == contentType);
        }

        public static async Task<bool> IsAMSReminderCoverLetterExists(this IParentEntityService<EmailType, EmailSetup> emailTypeService, string name)
        {
            return await emailTypeService.Any(name, typeof(AMSReminderCoverLetter).Name);
        }

        public static async Task<bool> IsUserAccountEmailExists(this IParentEntityService<EmailType, EmailSetup> emailTypeService, string name)
        {
            return await emailTypeService.Any(name, typeof(UserAccountEmail).Name);
        }

        public static async Task<bool> IsAMSConfirmationCoverLetterExists(this IParentEntityService<EmailType, EmailSetup> emailTypeService, string name)
        {
            return await emailTypeService.Any(name, typeof(AMSConfirmationCoverLetter).Name);
        }

        public static async Task<bool> IsAMSAgentResponsibilityCoverLetterExists(this IParentEntityService<EmailType, EmailSetup> emailTypeService, string name)
        {
            return await emailTypeService.Any(name, typeof(AMSAgentResponsibilityCoverLetter).Name);
        }

        public static async Task<bool> IsRMSReminderCoverLetterExists(this IParentEntityService<EmailType, EmailSetup> emailTypeService, string name, bool hasInstructByDate)
        {
            return await emailTypeService.Any(name, hasInstructByDate ? typeof(RMSReminderWithInstructByDateCoverLetter).Name : typeof(RMSReminderCoverLetter).Name);
        }

        public static async Task<bool> IsFFReminderCoverLetterExists(this IParentEntityService<EmailType, EmailSetup> emailTypeService, string name, bool hasInstructByDate)
        {
            return await emailTypeService.Any(name, hasInstructByDate ? typeof(FFReminderWithInstructByDateCoverLetter).Name : typeof(FFReminderCoverLetter).Name);
        }

        public static async Task<bool> IsRMSAgentResponsibilityCoverLetterExists(this IParentEntityService<EmailType, EmailSetup> emailTypeService, string name)
        {
            return await emailTypeService.Any(name, typeof(RMSAgentResponsibilityCoverLetter).Name);
        }
    }
}
