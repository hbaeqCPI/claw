using Newtonsoft.Json.Linq;
using LawPortal.Core.DTOs;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LawPortal.Core.Entities;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface ICPiUserSettingManager : IDisposable
    {
        Task<CPiSetting> GetCPiSetting(string settingName);

        Task<List<CPiUserSetting>> GetUserSettings(string userId);
        Task<CPiUserSetting> GetUserSetting(string userId, string settingName);
        Task<T> GetUserSetting<T>(string userId) where T : new();

        Task<CPiUserSetting> SaveUserSetting(string userId, string settingName, JObject settings);
        Task<CPiUserSetting> SaveUserSetting(CPiUserSetting userSetting);
        
        Task RemoveUserSetting(CPiUserSetting userSetting);
    }
}
