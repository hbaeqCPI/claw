using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IEmailTemplateService : IBaseService<EmailTemplate>
    {
        IQueryable<EmailContentType> ContentTypes { get; }
        Task<EmailMessage> GetEmailMessage(string emailTypeName, string languageCulture);
        Task<EmailMessage> GetNotificationMessage(string subject, string body);
    }
}
