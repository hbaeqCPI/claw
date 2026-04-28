// Entire file commented out - IEmailTemplateService and EmailMessage types deleted from LawPortal.Core
/*
using LawPortal.Core.Entities;
using LawPortal.Core.Interfaces;
using LawPortal.Web.Areas.Shared.ViewModels;
using LawPortal.Web.Extensions;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
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

        public static void ApplyData<T>(this EmailMessage emailMessage, T data) where T : EmailContent
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
    }
}
*/
