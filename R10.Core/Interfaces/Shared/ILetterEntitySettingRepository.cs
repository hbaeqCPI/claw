using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface ILetterEntitySettingRepository : IAsyncRepository<LetterEntitySetting>
    {
        Task<bool> SettingsUpdate(IEnumerable<LetterEntitySetting> updatedSettings,
                                  IEnumerable<LetterEntitySetting> newSettings, IEnumerable<LetterEntitySetting> deletedSettings);
    }
}
