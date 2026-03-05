// Entire file commented out - IEmailTemplateService and EmailMessage types deleted from R10.Core
/*
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using System;
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
