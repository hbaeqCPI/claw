using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using R10.Core.Identity;
using R10.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        public MailAddress From { get; set; }
        public List<MailAddress> To { get; set; } = new List<MailAddress>();
        public List<MailAddress> ReplyTo { get; set; } = new List<MailAddress>();
        public List<MailAddress> Cc { get; set; } = new List<MailAddress>();
        public List<MailAddress> Bcc { get; set; } = new List<MailAddress>();
        public List<string> Attachments { get; set; } = new List<string>();

        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger _logger;

        public EmailSender(IOptions<SmtpSettings> smtpSettings, ILogger<EmailSender> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _logger = logger;
        }

        public async Task<EmailSenderResult> SendEmailAsync(string to, string subject, string message, Attachment attachment = null)
        {
            To = new List<MailAddress>() { new MailAddress(to) };
            return await SendEmailAsync(subject, message, attachment);
        }

        public async Task<EmailSenderResult> SendEmailAsync(List<MailAddress> to, string subject, string message, Attachment attachment = null)
        {
            To = to;
            return await SendEmailAsync(subject, message, attachment);
        }

        public async Task<EmailSenderResult> SendEmailAsync(string from, string to, string subject, string message, Attachment attachment = null)
        {
            From = new MailAddress(from);
            return await SendEmailAsync(to, subject, message, attachment);
        }

        public async Task<EmailSenderResult> SendEmailAsync(string subject, string message, Attachment attachment = null)
        {
            try
            {
                using (SmtpClient client = new SmtpClient(_smtpSettings.Host))
                {
                    client.Port = _smtpSettings.Port;
                    client.EnableSsl = _smtpSettings.EnableSsl;

                    if (_smtpSettings.UseDefaultCredentials)
                        client.UseDefaultCredentials = _smtpSettings.UseDefaultCredentials;
                    else
                        client.Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password);

                    using (MailMessage mailMessage = new MailMessage{IsBodyHtml = true})
                    {
                        if (From != null && _smtpSettings.AllowSpoofing)
                        {
                            mailMessage.From = From;
                            if (_smtpSettings.SendOnBehalfOf)
                                mailMessage.Sender = new MailAddress(_smtpSettings.Sender);
                        }
                        else
                        {
                            mailMessage.From = new MailAddress(_smtpSettings.Sender);
                        }

                        foreach (MailAddress recipient in To)
                        {
                            mailMessage.To.Add(recipient);
                        }

                        foreach (MailAddress recipient in ReplyTo)
                        {
                            mailMessage.ReplyToList.Add(recipient);
                        }

                        foreach (MailAddress recipient in Cc)
                        {
                            mailMessage.CC.Add(recipient);
                        }

                        foreach (MailAddress recipient in Bcc)
                        {
                            mailMessage.Bcc.Add(recipient);
                        }

                        foreach (var item in Attachments)
                        {
                            mailMessage.Attachments.Add(new Attachment(item));
                        }

                        if(attachment != null)
                        {
                            mailMessage.Attachments.Add(attachment);
                        }

                        mailMessage.Body = message;
                        mailMessage.Subject = subject;

                        await client.SendMailAsync(mailMessage);
                    }
                }

                return new EmailSenderResult { Success = true, ErrorMessage = null };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{ex.Message}:{ex.InnerException?.Message}");
                return new EmailSenderResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<EmailSenderResult> SaveEmail(string subject, string message, string path)
        {
            try
            {
                using (SmtpClient client = new SmtpClient(_smtpSettings.Host))
                {
                    client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    client.PickupDirectoryLocation = path;

                    using (MailMessage mailMessage = new MailMessage { IsBodyHtml = true })
                    {
                        mailMessage.From = From;

                        foreach (MailAddress recipient in To)
                        {
                            mailMessage.To.Add(recipient);
                        }

                        foreach (MailAddress recipient in ReplyTo)
                        {
                            mailMessage.ReplyToList.Add(recipient);
                        }

                        foreach (MailAddress recipient in Cc)
                        {
                            mailMessage.CC.Add(recipient);
                        }

                        foreach (MailAddress recipient in Bcc)
                        {
                            mailMessage.Bcc.Add(recipient);
                        }

                        foreach (var item in Attachments)
                        {
                            mailMessage.Attachments.Add(new Attachment(item));
                        }

                        mailMessage.Body = message;
                        mailMessage.Subject = subject;

                        mailMessage.Headers.Add("X-Unsent", "1");

                        await client.SendMailAsync(mailMessage);
                    }
                }

                return new EmailSenderResult { Success = true, ErrorMessage = null };
            }
            catch (Exception ex)
            {
                var error = "Failure saving mail.";
                _logger.LogError(ex, $"{error}:{ex.InnerException?.Message}");
                return new EmailSenderResult { Success = false, ErrorMessage = error };
            }

        }
    }
}
