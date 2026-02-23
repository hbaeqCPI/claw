using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface ICPiSystemSettingManager : IBaseService<CPiSystemSetting>
    {
        Task<CPiSetting> GetCPiSetting(string settingName);
        Task<T> GetSystemSetting<T>(string systemId = "") where T : new();
        Task<T> GetSystemSetting<T>(string systemId, string settingName) where T : new();
    }
}
