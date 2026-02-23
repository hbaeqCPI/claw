using R10.Core.Entities.Shared;
using System.Net.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.Identity;

namespace R10.Core.Interfaces.Shared
{
    public interface ITradeSecretService
    {
        IQueryable<TradeSecretRequest> QueryableList { get; }
        IQueryable<TradeSecretActivity> ActivityQueryableList { get; }
        IQueryable<TradeSecretAuditLog> AuditLogQueryableList { get; }
        Task<bool> IsUserCleared(string screenId, int recId);
        Task<TradeSecretRequest?> GetUserRequest(string locator);
        Task<TradeSecretRequest?> CreateUserRequest(string locator);
        Task<bool> ValidateAccessToken(string locator, string token);
        string CreateLocator(string screenId, int recId);
        (string ScreenId, int RecId) GetLocator(string locator);
        Task<List<MailAddress>> GetApproverMailAddresses(string? screenId);
        Task<TradeSecretRequest?> UpdateApprovalStatus(int requestId, string status);
        Task LogActivity(string source, string screenId, int recId, string activity, int requestId);
        TradeSecretActivity CreateActivity(string source, string screenId, int recId, string activity, int requestId, Dictionary<string, string?[]>? auditLogs);
        Task<Dictionary<string, string?>> GetUserEmails(List<string?> userId);
        Task<List<CPiUserSettingLog>> GetUserSettingLogs(List<string?> userId);
    }
}
