using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Shared;
using System.Net.Mail;
using System.Security.Claims;

namespace R10.Core.Services.Shared
{
    public class TradeSecretService : ITradeSecretService
    {
        protected readonly ICPiDbContext _cpiDbContext;
        protected readonly ClaimsPrincipal _user;

        public TradeSecretService(ICPiDbContext cpiDbContext, ClaimsPrincipal user)
        {
            _cpiDbContext = cpiDbContext;
            _user = user;
        }

        public IQueryable<TradeSecretRequest> QueryableList => _cpiDbContext.GetRepository<TradeSecretRequest>().QueryableList;
        public IQueryable<TradeSecretActivity> ActivityQueryableList => _cpiDbContext.GetRepository<TradeSecretActivity>().QueryableList;
        public IQueryable<TradeSecretAuditLog> AuditLogQueryableList => _cpiDbContext.GetRepository<TradeSecretAuditLog>().QueryableList;

        /// <summary>
        /// Returns true if most recent request is cleared and not expired
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="screenId"></param>
        /// <param name="recId"></param>
        /// <returns></returns>
        public async Task<bool> IsUserCleared(string screenId, int recId)
        {
            var userRequest = await GetUserRequest(_user.GetUserIdentifier(), screenId, recId);
            if (userRequest == null)
                return false;

            return userRequest.IsCleared;
        }

        /// <summary>
        /// Returns most recent request
        /// </summary>
        /// <param name="locator"></param>
        /// <returns></returns>
        public async Task<TradeSecretRequest?> GetUserRequest(string locator)
        {
            var (ScreenId, RecId) = GetLocator(locator);
            return await GetUserRequest(_user.GetUserIdentifier(), ScreenId, RecId);
        }

        /// <summary>
        /// Creates new request
        /// </summary>
        /// <param name="locator"></param>
        /// <returns></returns>
        public async Task<TradeSecretRequest?> CreateUserRequest(string locator)
        {
            var dateStamp = DateTime.Now;
            var (ScreenId, RecId) = GetLocator(locator);

            var userRequest = new TradeSecretRequest()
            {
                UserId = _user.GetUserIdentifier(),
                ScreenId = ScreenId,
                RecId = RecId,
                Status = TradeSecretRequestStatus.Pending,
                RequestDate = dateStamp,
                StatusDate = dateStamp,
                TimeStamp = dateStamp.ToString(TradeSecretHelper.TimeStampFormat)
            };
            _cpiDbContext.GetRepository<TradeSecretRequest>().Add(userRequest);
            await _cpiDbContext.SaveChangesAsync();

            var userActivity = new TradeSecretActivity()
            {
                UserId = _user.GetUserIdentifier(),
                ScreenId = ScreenId,
                RecId = RecId,
                Activity = TradeSecretActivityCode.Request,
                ActivityDate = dateStamp,
                Source = ScreenId,
                RequestId = userRequest.RequestId
            };
            _cpiDbContext.GetRepository<TradeSecretActivity>().Add(userActivity);
            await _cpiDbContext.SaveChangesAsync();

            //auto approve admins
            if (TradeSecretHelper.AutoApproveAdmins && _user.IsAdmin())
                await UpdateRequestStatus(userRequest, TradeSecretRequestStatus.Granted);

            return userRequest;
        }

        private async Task UpdateRequestStatus(TradeSecretRequest userRequest, string status)
        {
            if (userRequest != null)
            {
                var dateStamp = DateTime.Now;

                _cpiDbContext.GetRepository<TradeSecretRequest>().Update(userRequest);
                userRequest.Status = status;
                userRequest.StatusDate = dateStamp;

                if (status == TradeSecretRequestStatus.Granted || status == TradeSecretRequestStatus.Cleared)
                {
                    //reset expiry
                    userRequest.TimeStamp = dateStamp.ToString(TradeSecretHelper.TimeStampFormat);

                    if (status == TradeSecretRequestStatus.Granted)
                    {
                        userRequest.Token = GenerateToken();
                        userRequest.Approver = _user.GetUserName();
                    }
                }

                _cpiDbContext.GetRepository<TradeSecretActivity>().Add(new TradeSecretActivity()
                {
                    UserId = _user.GetUserIdentifier(),
                    ScreenId = userRequest.ScreenId,
                    RecId = userRequest.RecId,
                    Activity = userRequest.Status,
                    ActivityDate = dateStamp,
                    Source = userRequest.ScreenId,
                    RequestId = userRequest.RequestId
                });

                await _cpiDbContext.SaveChangesAsync();
            }
        }

        private async Task LogValidationFailed(TradeSecretRequest userRequest)
        {
            var validationFailedCount = (userRequest.ValidationFailedCount ?? 0) + 1;

            _cpiDbContext.GetRepository<TradeSecretRequest>().Update(userRequest);
            userRequest.ValidationFailedCount = validationFailedCount;
            if (validationFailedCount >= TradeSecretHelper.MaxValidationFailedCount)
                userRequest.Status = TradeSecretRequestStatus.Revoked;

            _cpiDbContext.GetRepository<TradeSecretActivity>().Add(new TradeSecretActivity()
            {
                UserId = _user.GetUserIdentifier(),
                ScreenId = userRequest.ScreenId,
                RecId = userRequest.RecId,
                Activity = TradeSecretActivityCode.Validate,
                ActivityDate = DateTime.Now,
                Source = userRequest.ScreenId,
                RequestId= userRequest.RequestId
            });

            await _cpiDbContext.SaveChangesAsync();
        }


        public async Task LogActivity(string source, string screenId, int recId, string activity, int requestId)
        {
            CreateActivity(source, screenId, recId, activity, requestId);
            await _cpiDbContext.SaveChangesAsync();
        }


        public TradeSecretActivity CreateActivity(string source, string screenId, int recId, string activity, int requestId, Dictionary<string, string?[]>? auditLogs = null)
        {
            var tradeSecretActivity = new TradeSecretActivity()
            {
                UserId = _user.GetUserIdentifier(),
                ScreenId = screenId,
                RecId = recId,
                Activity = activity,
                ActivityDate = DateTime.Now,
                Source = source,
                RequestId = requestId
            };
            _cpiDbContext.GetRepository<TradeSecretActivity>().Add(tradeSecretActivity);

            if (auditLogs?.Count > 0)
            {
                tradeSecretActivity.TradeSecretAuditLogs = new List<TradeSecretAuditLog>()
                    {
                        new TradeSecretAuditLog()
                        {
                            UpdatedFields = string.Join("|", auditLogs.Keys.ToArray()),
                            OldValues = string.Join("|", auditLogs.Values.Select(v => v[0]).ToArray()),
                            NewValues = string.Join("|", auditLogs.Values.Select(v => v[1]).ToArray())
                        }
                    };
            }

            return tradeSecretActivity;
        }

        /// <summary>
        /// Validates access code
        /// </summary>
        /// <param name="locator"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> ValidateAccessToken(string locator, string token)
        {
            var userRequest = await GetUserRequest(locator);
            if (userRequest == null || userRequest.IsExpired || string.IsNullOrEmpty(userRequest.Token))
                return false;

            var isCleared = userRequest.Token == token;
            if (isCleared)
                await UpdateRequestStatus(userRequest, TradeSecretRequestStatus.Cleared);
            else
                await LogValidationFailed(userRequest);

            return isCleared;
        }

        public async Task<TradeSecretRequest?> UpdateApprovalStatus(int requestId, string status)
        {
            var userRequest = await _cpiDbContext.GetRepository<TradeSecretRequest>().GetByIdAsync(requestId);

            if (userRequest == null || userRequest.IsExpired)
                return null;

            await UpdateRequestStatus(userRequest, status);

            return userRequest;
        }

        /// <summary>
        /// Creates encrypted request locator
        /// </summary>
        /// <param name="screenId"></param>
        /// <param name="recId"></param>
        /// <returns></returns>
        public string CreateLocator(string screenId, int recId)
        {
            return $"{screenId}/{recId}".Encrypt(_user.GetEncryptionKey());
        }

        /// <summary>
        /// Decrypts encrypted locator
        /// </summary>
        /// <param name="locator"></param>
        /// <returns>ScreenId and RecId</returns>
        public (string ScreenId, int RecId) GetLocator(string locator)
        {
            var decyptedLocator = locator.Decrypt(_user.GetEncryptionKey());
            var ids = decyptedLocator.Split("/");
            if (ids.Length == 2)
                return (ids[0] ?? "", int.Parse(ids[1]));

            return (string.Empty, 0);
        }

        /// <summary>
        /// Get most recent user request
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="screenId"></param>
        /// <param name="recId"></param>
        /// <returns></returns>
        private async Task<TradeSecretRequest?> GetUserRequest(string userId, string screenId, int recId)
        {
            return await QueryableList.Where(r => r.ScreenId == screenId && r.UserId == userId && r.RecId == recId).OrderByDescending(r => r.RequestDate).FirstOrDefaultAsync();
        }

        private static string GenerateToken()
        {
            const string chars = "0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, TradeSecretHelper.AccessTokenLength)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Get email addresses of request approvers
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public async Task<List<MailAddress>> GetApproverMailAddresses(string? screenId)
        {
            var mailAddresses = new List<MailAddress>();
            var approverUserTypes = new List<CPiUserType>() { CPiUserType.SuperAdministrator, CPiUserType.Administrator };

            if (screenId == TradeSecretScreen.Invention ||
                screenId == TradeSecretScreen.CountryApplication)
                mailAddresses = await _cpiDbContext.GetRepository<CPiUser>().QueryableList
                    .Where(u => approverUserTypes.Contains(u.UserType) && u.CPiUserSettings.Any(s => s.CPiSetting.Name == "UserAccountSettings" && s.Settings.Contains($"\"RestrictPatTradeSecret\":false")))
                    .Select(u => new MailAddress(u.Email ?? "", $"{u.FirstName} {u.LastName}"))
                    .ToListAsync();

            if (screenId == TradeSecretScreen.DMSDisclosure )
                mailAddresses = await _cpiDbContext.GetRepository<CPiUser>().QueryableList
                    .Where(u => approverUserTypes.Contains(u.UserType) && u.CPiUserSettings.Any(s => s.CPiSetting.Name == "UserAccountSettings" && s.Settings.Contains($"\"RestrictDMSTradeSecret\":false")))
                    .Select(u => new MailAddress(u.Email ?? "", $"{u.FirstName} {u.LastName}"))
                    .ToListAsync();

            return mailAddresses;
        }

        public async Task<Dictionary<string, string?>> GetUserEmails(List<string?> userId)
        {
            var users = await _cpiDbContext.GetReadOnlyRepositoryAsync<CPiUser>().QueryableList
                                .Where(u => userId.Contains(u.Id)).Select(u => new { u.Id, u.Email }).AsNoTracking().ToListAsync();
            return users.ToDictionary(u => u.Id, u => u.Email);
        }
        public async Task<List<CPiUserSettingLog>> GetUserSettingLogs(List<string?> userIds)
        {
            var logs = await _cpiDbContext.GetReadOnlyRepositoryAsync<CPiUserSettingLog>().QueryableList
                                .Where(u => userIds.Contains(u.UserId)).ToListAsync();
            return logs;
        }
    }
}
