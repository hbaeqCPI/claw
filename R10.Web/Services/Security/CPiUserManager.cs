using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using R10.Core;
using R10.Core.Entities.Identity;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    public class CPiUserManager : UserManager<CPiUser>
    {
        protected readonly ICPiUserPasswordHistoryRepository _userPasswordHistoryStore;
        protected readonly ICPiUserSettingManager _settingManager;
        protected readonly ICPiDbContext _cpiDbContext;
        protected readonly IPasswordHasher<CPiUser> _passwordHasher;
        protected readonly CPiIdentitySettings _cpiSettings;
        protected readonly IdentityOptions _identityOptions;
        protected readonly HttpContext _context;
        protected readonly EPOMailboxSettings _epoMailboxSettings;

        public CPiUserManager(IUserStore<CPiUser> store, 
            IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<CPiUser> passwordHasher, 
            IEnumerable<IUserValidator<CPiUser>> userValidators, 
            IEnumerable<IPasswordValidator<CPiUser>> passwordValidators, 
            ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, 
            IServiceProvider services, 
            ILogger<CPiUserManager> logger, 
            ICPiUserPasswordHistoryRepository userPasswordHistoryStore,
            ICPiUserSettingManager settingManager,
            ICPiDbContext cpiDbContext,
            IOptions<CPiIdentitySettings> cpiSettings,
            IOptions<IdentityOptions> identityOptions,
            IHttpContextAccessor contextAccessor,
            IOptions<EPOMailboxSettings> epoMailboxSettings
            ) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _userPasswordHistoryStore = userPasswordHistoryStore;
            _settingManager = settingManager;
            _cpiDbContext = cpiDbContext;
            _passwordHasher = passwordHasher;
            _cpiSettings = cpiSettings.Value;
            _identityOptions = identityOptions.Value;
            _context = contextAccessor.HttpContext;
            _epoMailboxSettings = epoMailboxSettings.Value;
        }

        public override async Task<IdentityResult> UpdateAsync(CPiUser user)
        {
            //user name is null when user is trying to login
            var userName = _context.User.GetUserName();
            if (!string.IsNullOrEmpty(userName))
            {
                user.UpdatedBy = userName;
                user.LastUpdate = DateTime.Now;
            }

            return await base.UpdateAsync(user);
        }

        public override async Task<IdentityResult> CreateAsync(CPiUser user, string password)
        {
            var userName = _context.User.GetUserName();
            var now = DateTime.Now;

            user.UpdatedBy = userName;
            user.LastUpdate = now;
            user.CreatedBy = userName;
            user.DateCreated = now;

            IdentityResult result = await ValidatePasswordRequirements(user, password);
            if (!result.Succeeded)
            {
                return result;
            }

            result = await base.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await SavePasswordHistory(user.Id);
            }

            return result;
        }

        public override async Task<IdentityResult> CreateAsync(CPiUser user)
        {
            var result = await base.CreateAsync(user);
            if (result.Succeeded)
            {
                await AddDefaultPage(user);
                await AddDefaultWidgets(user);

                //add default user-system-role by userType
                result = await AddDefaultRolesAsync(user);

                await AddDefaultSettings(user);

                //rollback if something went wrong
                if (!result.Succeeded)
                {
                    await base.DeleteAsync(user);
                }
            }

            return result;
        }

        public override async Task<IdentityResult> ChangePasswordAsync(CPiUser user, string currentPassword, string newPassword)
        {
            IdentityResult result = await ValidatePasswordRequirements(user, newPassword);
            if (!result.Succeeded)
            {
                return result;
            }

            user.LastPasswordChangeDate = DateTime.Now;
            result = await base.ChangePasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                await SavePasswordHistory(user.Id);
            }
            return result;
        }

        public override async Task<IdentityResult> ResetPasswordAsync(CPiUser user, string token, string newPassword)
        {
            IdentityResult result = await ValidatePasswordRequirements(user, newPassword);
            if (!result.Succeeded)
            {
                return result;
            }

            user.LastPasswordChangeDate = DateTime.Now;
            result = await base.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                await SavePasswordHistory(user.Id);
            }
            return result;
        }

        public override async Task<IdentityResult> AddPasswordAsync(CPiUser user, string password)
        {
            IdentityResult result = await ValidatePasswordRequirements(user, password);
            if (!result.Succeeded)
            {
                return result;
            }

            user.LastPasswordChangeDate = DateTime.Now;
            result = await base.AddPasswordAsync(user, password);

            if (result.Succeeded)
            {
                await SavePasswordHistory(user.Id);
            }
            return result;
        }

        public override async Task<IdentityResult> RemovePasswordAsync(CPiUser user)
        {
            user.LastPasswordChangeDate = null;
            return await base.RemovePasswordAsync(user);
        }

        private async Task SavePasswordHistory(string userId)
        {
            var user = await FindByIdAsync(userId);
            await _userPasswordHistoryStore.CreateAsync(new CPiUserPasswordHistory
            {
                UserId = user.Id,
                PasswordHash = user.PasswordHash,
                LastPasswordChangeDate = DateTime.Now
            });
        }

        private async Task<IdentityResult> ValidatePasswordRequirements(CPiUser user, string newPassword)
        {
            int lastUniquePasswords = _cpiSettings.Password.LastUnique;

            List<IdentityError> errors = new List<IdentityError>();

            if (lastUniquePasswords > 0)
            {
                var userPasswordHistory = await _userPasswordHistoryStore.UserPasswordHistory
                                                                            .Where(x => x.UserId == user.Id)
                                                                            .OrderByDescending(x => x.LastPasswordChangeDate)
                                                                            .Take(lastUniquePasswords)
                                                                            .ToListAsync();

                foreach (CPiUserPasswordHistory passwordHistory in userPasswordHistory)
                {
                    if (_passwordHasher.VerifyHashedPassword(user, passwordHistory.PasswordHash, newPassword) == PasswordVerificationResult.Success)
                    {
                        errors.Add(new IdentityError { Code = "", Description = $"Password must be different from your previous {lastUniquePasswords} passwords." });
                        break;
                    }
                }
            }

            if (!_cpiSettings.Password.CanHavePartsOfName)
            {
                MailAddress email = new MailAddress(user.Email);

                //remove top level domain from host name
                int pos = email.Host.LastIndexOf('.');
                string host = email.Host;
                if (pos >= 0)
                {
                    host = host.Substring(0, pos);
                }

                char[] namePartsSeparator = _cpiSettings.Password.NamePartsSeparator.ToCharArray();

                if (PasswordContains(newPassword, email.User, namePartsSeparator))
                    errors.Add(new IdentityError { Code = "", Description = "Password cannot contain part of your email address." });

                if (PasswordContains(newPassword, host, new char[] { ' ' }))
                    errors.Add(new IdentityError { Code = "", Description = "Password cannot contain part of your email address." });

                if (PasswordContains(newPassword, string.Concat(user.FirstName, " ", user.LastName), namePartsSeparator))
                    errors.Add(new IdentityError { Code = "", Description = "Password cannot contain part of your name." });
            }

            if (errors.Count() > 0)
            {
                return IdentityResult.Failed(errors.ToArray());
            }

            return IdentityResult.Success;
        }

        private bool PasswordContains(string password, string value, char[] namePartsSeparator)
        {
            int minCharsFromName = _cpiSettings.Password.MinimumCharsFromName;
            
            foreach (string part in value.Split(namePartsSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                if (minCharsFromName > 0)
                {
                    if (part.SplitIntoParts(minCharsFromName + 1)
                            .Any(p => password.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        return true;
                    }
                }
                else if (part.Length > 2 && password.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        #region Extensions
        private IQueryable<CPiSystem> CPiSystems => _cpiDbContext.GetRepository<CPiSystem>().QueryableList;
        private IQueryable<CPiUserSystemRole> CPiUserSystemRoles => _cpiDbContext.GetRepository<CPiUserSystemRole>().QueryableList;

        public async Task<List<CPiSystem>> GetSystems()
        {
            return await CPiSystems.Where(system => system.IsEnabled).ToListAsync();
        }

        public async Task<List<CPiUserEntityFilter>> GetEntityFilters(string userId)
        {
            return await _cpiDbContext.GetRepository<CPiUserEntityFilter>().QueryableList.Where(ef => ef.UserId == userId).ToListAsync();
        }

        /// <summary>
        /// Resets user permissions
        /// Removes existing CPiUserEntityFilter, CPiUserSystemRoles, and CPiUserClaims
        /// Add Default CPiUserSystemRole and CPiUserClaim based on UserType
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<IdentityResult> AddDefaultRolesAsync(CPiUser user)
        {
            var entityFilters = await _cpiDbContext.GetRepository<CPiUserEntityFilter>().QueryableList.Where(e => e.UserId == user.Id).ToListAsync();
            var roles = await _cpiDbContext.GetRepository<CPiUserSystemRole>().QueryableList.Where(e => e.UserId == user.Id).ToListAsync();
            var claims = await _cpiDbContext.GetRepository<CPiUserClaim>().QueryableList.Where(e => e.UserId == user.Id).ToListAsync();

            var defaultRoles = await _cpiDbContext.GetRepository<CPiUserTypeSystemRole>().QueryableList.Where(u => u.UserType == user.UserType && u.System.IsEnabled).ToListAsync();

            var userRoles = new List<CPiUserSystemRole>();

            foreach (CPiUserTypeSystemRole role in defaultRoles)
            {
                userRoles.Add(new CPiUserSystemRole { UserId = user.Id, SystemId = role.SystemId, RoleId = role.RoleId, RespOffice = role.RespOffice });
            }

            var userClaims = new List<CPiUserClaim>();
            foreach (CPiUserSystemRole userRole in userRoles)
            {
                userClaims.AddRange(userRole.ToCPiUserClaims());
            }
            _cpiDbContext.GetRepository<CPiUserSystemRole>().Add(userRoles);
            _cpiDbContext.GetRepository<CPiUserClaim>().Add(userClaims);

            if (entityFilters.Any())
                _cpiDbContext.GetRepository<CPiUserEntityFilter>().Delete(entityFilters);

            if (roles.Any())
                _cpiDbContext.GetRepository<CPiUserSystemRole>().Delete(roles);

            if (claims.Any())
                _cpiDbContext.GetRepository<CPiUserClaim>().Delete(claims);

            try
            {
                await _cpiDbContext.SaveChangesAsync();
                //detach for possible tracking on same call
                _cpiDbContext.Detach(userRoles);
                _cpiDbContext.Detach(userClaims);
            }
            catch (Exception e) //catch (DbUpdateException e)
            {
                var message = e.Message;
                IdentityError err = new IdentityError();
                err.Code = "AddDefaultRolesAsync";
                err.Description = "An error occurred while saving to the database.";

                return IdentityResult.Failed(err);
            }

            return IdentityResult.Success;
        }

        public async Task AddDefaultPage(CPiUser user)
        {
            var defaultPageId = await _cpiDbContext.GetRepository<CPiUserTypeDefaultPage>().QueryableList
                                                        .Where(u => u.UserType == user.UserType)
                                                        .Select(u => u.DefaultPageId)
                                                        .FirstOrDefaultAsync();
            if (defaultPageId > 0)
                await _settingManager.SaveUserSetting(user.Id, "DefaultPage", JObject.Parse($"{{ DefaultPageId: {defaultPageId}}}"));
        }

        public async Task AddDefaultWidgets(CPiUser user)
        {
            var userWidgets = await _cpiDbContext.GetRepository<CPiUserTypeDefaultWidget>().QueryableList
                                                        .Where(w => w.UserType == user.UserType)
                                                        .Select(w => new CPiUserWidget()
                                                        {
                                                            UserId = user.Id,
                                                            WidgetCategory = w.WidgetCategory,
                                                            WidgetId = w.WidgetId,
                                                            SortOrder = w.SortOrder,
                                                            Settings = w.CPiWidget.Settings ?? "{}",
                                                            UserTitle = w.CPiWidget.Title
                                                        })
                                                        .ToListAsync();

            _cpiDbContext.GetRepository<CPiUserWidget>().Add(userWidgets);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task ResetDefaultWidgets(CPiUser user)
        {
            var existingWidgets = await _cpiDbContext.GetRepository<CPiUserWidget>().QueryableList
                                                        .Where(w => w.UserId == user.Id)
                                                        .ToListAsync();

            var newWidgets = await _cpiDbContext.GetRepository<CPiUserTypeDefaultWidget>().QueryableList
                                                        .Where(w => w.UserType == user.UserType)
                                                        .Select(w => new CPiUserWidget()
                                                        {
                                                            UserId = user.Id,
                                                            WidgetCategory = w.WidgetCategory,
                                                            WidgetId = w.WidgetId,
                                                            SortOrder = w.SortOrder,
                                                            Settings = w.CPiWidget.Settings ?? "{}"
                                                        })
                                                        .ToListAsync();

            _cpiDbContext.GetRepository<CPiUserWidget>().Delete(existingWidgets);
            _cpiDbContext.GetRepository<CPiUserWidget>().Add(newWidgets);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task AddDefaultSettings(CPiUser user)
        {            
            var userAccountSettings = await _settingManager.GetUserSetting<UserAccountSettings>(user.Id);

            //Enable AllowHandleMyEPOCommunications for admin
            if (_epoMailboxSettings.IsAPIOn && (user.UserType == CPiUserType.Administrator || user.UserType == CPiUserType.SuperAdministrator))
            {
                userAccountSettings.AllowHandleMyEPOCommunications = true;
            }

            await _settingManager.SaveUserSetting(user.Id, CPiSettings.UserAccountSettings, JObject.FromObject(userAccountSettings));
        }

        /// <summary>
        /// Regular users without any system roles can have adhoc dashboard access
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>True if user is regular user and has no system roles</returns>
        public async Task<bool> HasDashboardAccessOptions(CPiUser user)
        {
            return user.UserType == CPiUserType.User && !(await GetUserSystemRoles(user.Id).AnyAsync());
        }

        public IQueryable<CPiUserSystemRole> GetUserSystemRoles(string userId)
        {
            return CPiUserSystemRoles.Where(r => r.UserId == userId && r.CPiSystem.IsEnabled && r.CPiRole.IsEnabled &&
                _cpiDbContext.GetRepository<CPiSystemRole>().QueryableList.Any(sr => sr.SystemId == r.SystemId && sr.RoleId == r.RoleId));
        }

        public async Task<List<string>> GetUserRespOfficesBySystem(string userId, string systemId)
        {
            return await CPiUserSystemRoles.Where(r => r.UserId == userId && r.SystemId == systemId)
                            .OrderBy(r => r.Id)
                            .Select(r => r.RespOffice)
                            .ToListAsync();
        }

        public string GenerateRandomPassword()
        {
            //if (passwordOptions == null) passwordOptions = _identityOptions.Password;
            PasswordOptions passwordOptions = _identityOptions.Password;

            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-"                        // non-alphanumeric
            };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (passwordOptions.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (passwordOptions.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (passwordOptions.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (passwordOptions.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < passwordOptions.RequiredLength
                || chars.Distinct().Count() < passwordOptions.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        public bool IsSSORequired(CPiUser user)
        {
            var isDomainSSORequired = false;
            try
            {
                var userDomain = user.Email?.Split('@')[1];
                isDomainSSORequired = _cpiSettings.SignIn.RequireSSODomains != null && !string.IsNullOrEmpty(userDomain) && _cpiSettings.SignIn.RequireSSODomains.Contains(userDomain);
            }
            catch { }

            return (user.ExternalLoginOnly ?? false) || isDomainSSORequired;
        }
        #endregion
    }
}
