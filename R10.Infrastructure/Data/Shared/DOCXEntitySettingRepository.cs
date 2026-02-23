//using Microsoft.EntityFrameworkCore;
//using R10.Core.Entities;
//using R10.Core.Interfaces;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace R10.Infrastructure.Data
//{

//    public class DOCXEntitySettingRepository : EFRepository<DOCXEntitySetting>, IDOCXEntitySettingRepository
//    {
//        public DOCXEntitySettingRepository(ApplicationDbContext dbContext) : base(dbContext) { }

//        public async Task<bool> SettingsUpdate(IEnumerable<DOCXEntitySetting> updatedSettings,
//                                              IEnumerable<DOCXEntitySetting> newSettings, IEnumerable<DOCXEntitySetting> deletedSettings)
//        {
//            foreach (var setting in deletedSettings)
//            {
//                _dbContext.Set<DOCXEntitySetting>().Remove(setting);
             
//            }

//            foreach (var setting in updatedSettings)
//            {
//                if (_dbContext.Entry(setting).State != EntityState.Deleted && _dbContext.Entry(setting).State != EntityState.Modified)
//                    _dbContext.Entry(setting).State = EntityState.Modified;
//            }

//            foreach (var setting in newSettings)
//            {
//                _dbContext.Add(setting);
//            }
//            await _dbContext.SaveChangesAsync();
//            return true;
//        }
//    }
//}
