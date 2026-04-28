using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using LawPortal.Core.Identity;
using LawPortal.Core.Interfaces;
using LawPortal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Web.Services
{
    public class CPiSystemSettingManager : BaseService<CPiSystemSetting>, ICPiSystemSettingManager
    {
        public CPiSystemSettingManager(ICPiDbContext cpiDbContext) : base(cpiDbContext)
        {
        }

        public async Task<CPiSetting> GetCPiSetting(string settingName)
        {
            return await _cpiDbContext.GetReadOnlyRepositoryAsync<CPiSetting>().QueryableList.Where(s => s.Name == settingName).FirstOrDefaultAsync();
        }

        public async Task<T> GetSystemSetting<T>(string systemId = "") where T : new()
        {
            return await GetSystemSetting<T>(systemId, typeof(T).Name);
        }

        public async Task<T> GetSystemSetting<T>(string systemId, string settingName) where T : new()
        {
            var systemSetting = await _cpiDbContext.GetReadOnlyRepositoryAsync<CPiSystemSetting>().QueryableList.Where(s => s.SystemId == systemId && s.CPiSetting.Name == settingName).Include(x => x.CPiSetting).FirstOrDefaultAsync();

            return systemSetting == null ? new T() : JsonConvert.DeserializeObject<T>(systemSetting.Settings);
        }
    }
}
