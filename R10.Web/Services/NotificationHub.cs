using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.SignalR;
using R10.Core.Entities;
using R10.Core.Interfaces;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using R10.Core.Identity;

namespace R10.Web.Services
{
   
    public class NotificationHub : Hub, INotificationHub
    {
        private readonly ICPiDbContext _cpiDbContext;
        private readonly IHubContext<NotificationHub> _context;

        public NotificationHub(ICPiDbContext cpiDbContext, IHubContext<NotificationHub> context) :base()
        {
            _cpiDbContext = cpiDbContext;
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var repository = _cpiDbContext.GetRepository<NotificationConnection>();
            var userName = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                var connection = await repository.QueryableList.Where(c => c.UserName == userName).FirstOrDefaultAsync();
                if (connection != null)
                {
                    repository.Update(connection);
                    connection.ConnectionId = Context.ConnectionId;
                    connection.ConnectedOn = DateTime.Now;
                    connection.Active = true;
                }
                else
                {
                    repository.Add(new NotificationConnection
                    {
                        UserName = userName,
                        ConnectionId = Context.ConnectionId,
                        ConnectedOn = DateTime.Now,
                        Active = true
                    });
                }
                await _cpiDbContext.SaveChangesAsync();
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var repository = _cpiDbContext.GetRepository<NotificationConnection>();
            var userName = Context.User?.Identity?.Name;
            var connection = await repository.QueryableList.Where(c => c.UserName == userName).FirstOrDefaultAsync();
            if (connection != null)
            {
                repository.Update(connection);
                connection.Active = false;
            }
            await _cpiDbContext.SaveChangesAsync();
            await base.OnDisconnectedAsync(exception);
        }

        public async Task RefreshCount(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                await _context.Clients.All.SendAsync("refreshCount");
            else {
                var connection = await _cpiDbContext.GetRepository<NotificationConnection>().QueryableList.Where(c => c.UserName == userName).FirstOrDefaultAsync();
                if (connection != null) {
                    await _context.Clients.Client(connection.ConnectionId).SendAsync("refreshCount");
                }
            }
        }

        public async Task SendMessage(string userName, string message)
        {
            if (string.IsNullOrEmpty(userName))
                await _context.Clients.All.SendAsync("newMessage", message);
            else
            {
                var connection = await _cpiDbContext.GetRepository<NotificationConnection>().QueryableList.Where(c => c.UserName == userName).FirstOrDefaultAsync();
                if (connection != null)
                {
                    await _context.Clients.Client(connection.ConnectionId).SendAsync("newMessage", message);
                }
            }
        }
    }

    public interface INotificationHub
    {
        Task RefreshCount(string userName);
        Task SendMessage(string userName, string message);
        Task OnConnectedAsync();
        Task OnDisconnectedAsync(Exception exception);
    }
}
