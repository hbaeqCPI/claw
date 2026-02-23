using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IEmailSender
    {
        MailAddress From { get; set; }
        List<MailAddress> To { get; set; }
        List<MailAddress> ReplyTo { get; set; }
        List<MailAddress> Cc { get; set; }
        List<MailAddress> Bcc { get; set; }
        List<string> Attachments { get; set; }

        Task<EmailSenderResult> SendEmailAsync(string subject, string message, Attachment attachment = null);
        Task<EmailSenderResult> SendEmailAsync(string to, string subject, string message, Attachment attachment = null);
        Task<EmailSenderResult> SendEmailAsync(List<MailAddress> to, string subject, string message, Attachment attachment = null);
        Task<EmailSenderResult> SendEmailAsync(string from, string to, string subject, string message, Attachment attachment = null);
        Task<EmailSenderResult> SaveEmail(string subject, string message, string path);
    }

    public class EmailSenderResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
