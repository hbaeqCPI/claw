using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using R10.Core.DTOs;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R10.Core.Entities;
using System.Threading.Tasks;
using R10.Core.Helpers;

namespace R10.Web.Services
{
    public class CPiUserSettingManager : ICPiUserSettingManager
    {
        private readonly ICPiDbContext _cpiDbContext;

        public CPiUserSettingManager(ICPiDbContext cpiDbContext)
        {
            _cpiDbContext = cpiDbContext;
        }

        private IQueryable<CPiUserSetting> CPiUserSettings => _cpiDbContext.GetRepository<CPiUserSetting>().QueryableList;

        public async Task<CPiSetting> GetCPiSetting(string settingName)
        {
            return await _cpiDbContext.GetReadOnlyRepositoryAsync<CPiSetting>().QueryableList.Where(s => s.Name == settingName).FirstOrDefaultAsync();
        }

        public async Task<List<CPiUserSetting>> GetUserSettings(string userId)
        {
            return await CPiUserSettings.Where(s => s.UserId == userId).ToListAsync();
        }

        public async Task<CPiUserSetting> GetUserSetting(string userId, string settingName)
        {
            return await CPiUserSettings.Where(s => s.UserId == userId && s.CPiSetting.Name == settingName).Include(x => x.CPiSetting).FirstOrDefaultAsync();
        }

        public async Task<T> GetUserSetting<T>(string userId) where T : new()
        {
            string settingName = typeof(T).Name;
            var userSetting = await CPiUserSettings.Where(s => s.UserId == userId && s.CPiSetting.Name == settingName).Include(x => x.CPiSetting).FirstOrDefaultAsync();

            return userSetting == null ? new T() : JsonConvert.DeserializeObject<T>(userSetting.Settings);
        }

        public async Task<CPiUserSetting> SaveUserSetting(string userId, string settingName, JObject settings)
        {
            var userSetting = await GetUserSetting(userId, settingName);

            if (userSetting == null)
            {
                CPiSetting cpiSetting = await GetCPiSetting(settingName);
                userSetting = new CPiUserSetting()
                {
                    UserId = userId,
                    SettingId = cpiSetting.Id
                };
            }

            JObject userSettings = new JObject();
            if (!string.IsNullOrEmpty(userSetting?.Settings))
            {
                userSettings = JObject.Parse(userSetting.Settings);
            }

            foreach (var token in settings)
            {
                if(_loggableSettings.Contains(token.Key))
                {
                    await LogSettingChange(userId, token.Key, token.Value);
                }
                userSettings[token.Key] = token.Value;
            }

            userSetting.Settings = JsonConvert.SerializeObject(userSettings);

            return await SaveUserSetting(userSetting);
        }

        public async Task<CPiUserSetting> SaveUserSetting(CPiUserSetting userSetting)
        {
            if (userSetting.Id == 0)
                _cpiDbContext.GetRepository<CPiUserSetting>().Add(userSetting);
            else
                _cpiDbContext.GetRepository<CPiUserSetting>().Update(userSetting);

            await _cpiDbContext.SaveChangesAsync();

            if (userSetting.CPiSetting != null)
                _cpiDbContext.Detach(userSetting.CPiSetting); //.net 6 detach included entity (see GetUserSetting)

            _cpiDbContext.Detach(userSetting);

            return userSetting;
        }

        public async Task RemoveUserSetting(CPiUserSetting userSetting)
        {
            _cpiDbContext.GetRepository<CPiUserSetting>().Delete(userSetting);
            await _cpiDbContext.SaveChangesAsync();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CPiUserSettingManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        #region Setting Change Log
        private readonly HashSet<string> _loggableSettings = new HashSet<string>
        {
            "RestrictPatTradeSecret",
            "RestrictDMSTradeSecret"
        };

        private async Task LogSettingChange(string userId, string settingName, JToken? setting)
        {
            string newValue = setting?.ToString() ?? "";

            var log = new CPiUserSettingLog()
            {
                UserId = userId,
                SettingName = settingName,
                NewValue = newValue,
                ChangeDate = DateTime.Now
            };

            _cpiDbContext.GetRepository<CPiUserSettingLog>().Add(log);
            await _cpiDbContext.SaveChangesAsync();
        }
        #endregion
    }
}
