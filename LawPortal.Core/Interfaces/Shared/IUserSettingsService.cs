using LawPortal.Core.DTOs;
using LawPortal.Core.Entities.Patent;
using LawPortal.Core.Entities.Trademark;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface IUserSettingsService
    {
        Task<CPiSetting> GetSettingByIdAsync(int id);
        Task<CPiSetting> GetSettingByNameAsync(string name);
        Task<CPiSetting> AddSettingAsync(CPiSetting setting);
        Task UpdateSettingAsync(CPiSetting setting);
        Task DeleteSettingAsync(CPiSetting setting);

        Task<CPiUserSetting> GetUserSettingsByIdAsync(int id);
        Task<CPiUserSetting> GetUserSettingsAsync(string userId, int settingId);
        Task<List<CPiUserSetting>> GetAllUsersSettingsAsync(int settingId);
        Task<CPiUserSetting> AddUserSettingsAsync(CPiUserSetting userSetting);
        Task UpdateUserSettingsAsync(CPiUserSetting userSetting);
        Task DeleteUserSettingsAsync(CPiUserSetting userSetting);
    }
}
