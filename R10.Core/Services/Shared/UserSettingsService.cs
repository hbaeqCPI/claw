using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.DMS;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Identity;
using R10.Core.Interfaces;

namespace R10.Core.Services.Shared
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly IApplicationDbContext _repository;
        public UserSettingsService(IApplicationDbContext repository)
        {
            _repository = repository;
        }

        #region Setting
        public async Task<CPiSetting> GetSettingByIdAsync(int id)
        {
            return await _repository.CPiSettings.FindAsync(id);
        }

        public async Task<CPiSetting> GetSettingByNameAsync(string name)
        {
            return await _repository.CPiSettings.Where(d => d.Name == name).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<CPiSetting> AddSettingAsync(CPiSetting setting)
        {
            _repository.CPiSettings.Add(setting);
            await _repository.SaveChangesAsync();
            return setting;
        }

        public async Task UpdateSettingAsync(CPiSetting setting)
        {
            _repository.CPiSettings.Update(setting);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteSettingAsync(CPiSetting setting)
        {
            _repository.CPiSettings.Remove(setting);
            await _repository.SaveChangesAsync();
        }

        #endregion

        #region User Setting


        public async Task<CPiUserSetting> GetUserSettingsByIdAsync(int id)
        {
            return await _repository.CPiUserSettings.FindAsync(id);
        }

        public async Task<CPiUserSetting> GetUserSettingsAsync(string userId, int settingId)
        {
            return await _repository.CPiUserSettings.Where(u => u.UserId == userId && u.SettingId == settingId).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<List<CPiUserSetting>> GetAllUsersSettingsAsync(int settingId)
        {
            return await _repository.CPiUserSettings.Include(s=> s.CPiUser).Where(s => s.SettingId == settingId && _repository.CPiUser.Any(u => u.Status == 0 && u.Id == s.UserId)).AsNoTracking().ToListAsync();
        }

        public async Task<CPiUserSetting> AddUserSettingsAsync(CPiUserSetting userSetting)
        {
            _repository.CPiUserSettings.Add(userSetting);
            await _repository.SaveChangesAsync();
            return userSetting;
        }

        public async Task DeleteUserSettingsAsync(CPiUserSetting userSetting)
        {
            _repository.CPiUserSettings.Remove(userSetting);
            await _repository.SaveChangesAsync();
        }


        public async Task UpdateUserSettingsAsync(CPiUserSetting userSetting)
        {
            _repository.CPiUserSettings.Update(userSetting);
            await _repository.SaveChangesAsync();
        }

        #endregion


    }
}


