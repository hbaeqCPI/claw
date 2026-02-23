using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System.Net.Mail;

namespace R10.Core.Services
{
    public interface INotificationService : IBaseService<Notification>
    {
        Task<List<Notification>> GetUnreadMessages(string userName);
        Task<List<MailAddress>> GetRecipients();
        Task Dismiss(string userName, int messageId);
        Task<int> GetCount(string userName);
        Task SendAlert(string senderUserName, string recipient, string title, string message, string? url = "", double expireInMinutes = 1440);
        Task SendAlert(string senderUserName, List<MailAddress> recipients, string title, string message, string? url = "", double expireInMinutes = 1440);
    }

    public class NotificationService : BaseService<Notification>, INotificationService
    {
        public NotificationService(ICPiDbContext cpiDbContext) : base(cpiDbContext)
        {
        }

        public async Task<List<Notification>> GetUnreadMessages(string userName)
        {
            return await _cpiDbContext.GetRepository<Notification>().QueryableList
                                        .Where(n => !n.Viewed && n.UserName == userName && DateTime.Now >= n.EffectiveFrom && (n.EffectiveTo == null || DateTime.Now <= n.EffectiveTo))
                                        .OrderByDescending(n => n.DateCreated)
                                        .ToListAsync();
        }

        public async override Task Add(Notification notification)
        {
            var recipients = notification.UserName.Split(",");
            var notifications = new List<Notification>();
            foreach (var userName in recipients)
            {
                notifications.Add(new Notification
                {
                    Type = notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    UserName = userName,
                    EffectiveFrom = notification.EffectiveFrom,
                    EffectiveTo = notification.EffectiveTo,
                    NavigateToUrl = notification.NavigateToUrl,
                    CreatedBy = notification.CreatedBy,
                    UpdatedBy = notification.UpdatedBy,
                    DateCreated = notification.DateCreated,
                    LastUpdate = notification.LastUpdate
                });
            }
            if (notifications.Any())
            {
                _cpiDbContext.GetRepository<Notification>().Add(notifications);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task<List<MailAddress>> GetRecipients()
        {
            return await _cpiDbContext.GetRepository<CPiUser>().QueryableList
                .Select(u => new MailAddress(u.Email ?? "", $"{u.FirstName} {u.LastName} <{u.Email}>"))
                .ToListAsync();
        }

        public async Task Dismiss(string userName, int messageId)
        {
            var notification = await _cpiDbContext.GetRepository<Notification>().QueryableList.Where(n => n.MessageId == messageId && n.UserName == userName).FirstOrDefaultAsync();
            if (notification != null)
            {
                _cpiDbContext.GetRepository<Notification>().Update(notification);
                notification.Viewed = true;
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task<int> GetCount(string userName)
        {
            return await _cpiDbContext.GetRepository<Notification>().QueryableList.Where(n => !n.Viewed && n.UserName == userName && DateTime.Now >= n.EffectiveFrom && (n.EffectiveTo == null || DateTime.Now <= n.EffectiveTo)).CountAsync();
        }

        public async Task SendAlert(string senderUserName, string recipient, string title, string message, string? url = "", double expireInMinutes = 1440)
        {
            await SendAlert(senderUserName, new List<MailAddress>() { new MailAddress(recipient) }, title, message, url, expireInMinutes);
        }

        public async Task SendAlert(string senderUserName, List<MailAddress> recipients, string title, string message, string? url = "", double expireInMinutes = 1440)
        {
            var dateCreated = DateTime.Now;
            await Add(new Notification()
            {
                Type = "A",
                UserName = string.Join(',', recipients.Select(r => r.Address).ToList()),
                Title = title,
                Message = message,
                NavigateToUrl = url,
                EffectiveFrom = dateCreated,
                EffectiveTo = dateCreated.AddMinutes(expireInMinutes),
                CreatedBy = senderUserName,
                UpdatedBy = senderUserName,
                DateCreated = dateCreated,
                LastUpdate = dateCreated
            });
        }
    }
}
