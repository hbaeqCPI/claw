using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.Shared
{
    public class EmailTemplateService : BaseService<EmailTemplate>, IEmailTemplateService
    {
        public EmailTemplateService(ICPiDbContext cpiDbContext) : base(cpiDbContext)
        {
        }

        public IQueryable<EmailContentType> ContentTypes => _cpiDbContext.GetReadOnlyRepositoryAsync<EmailContentType>().QueryableList;

        public async Task<EmailMessage> GetEmailMessage(string emailTypeName, string languageCulture)
        {
            var emailMessage = await _cpiDbContext.GetReadOnlyRepositoryAsync<EmailSetup>()
                                .QueryableList
                                .Where(e => e.EmailType.Name == emailTypeName && (e.LanguageLookup.LanguageCulture == languageCulture || e.Default))
                                .OrderBy(e => e.Default)
                                .Select(e => new { 
                                    Subject = e.Subject,
                                    Body = e.Body,
                                    Template = e.EmailType.EmailTemplate.Template
                                })
                                .FirstOrDefaultAsync();
            if (emailMessage == null)
                return null;

            return new EmailMessage(
                emailMessage.Subject,
                string.IsNullOrEmpty(emailMessage.Template) ? emailMessage.Body : emailMessage.Template.Replace("{{Subject}}", emailMessage.Subject).Replace("{{Body}}", emailMessage.Body)
                );
        }

        /// <summary>
        /// Use default Notification template for system 
        /// notifications that doesn't need email setup
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<EmailMessage> GetNotificationMessage(string subject, string body)
        {
            var template = await _cpiDbContext.GetReadOnlyRepositoryAsync<EmailTemplate>().QueryableList
                                    .Where(t => t.Name.ToLower() == "notification")
                                    .Select(t => t.Template)
                                    .FirstOrDefaultAsync();

            return new EmailMessage(
                subject,
                string.IsNullOrEmpty(template) ? body : template.Replace("{{Subject}}", subject).Replace("{{Body}}", body)
                );
        }
    }
}
