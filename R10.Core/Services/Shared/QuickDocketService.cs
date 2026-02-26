
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
// using R10.Core.Entities.DMS; // Removed during deep clean
using R10.Core.Entities.Documents;
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.Shared
{
    public class QuickDocketService : IQuickDocketService
    {

        private readonly IApplicationDbContext _repository;

        public QuickDocketService(IApplicationDbContext repository)
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

        #region Default Page

        public async Task<CPiDefaultPage> GetDefaultPageByIdAsync(int id)
        {
            return await _repository.CPiDefaultPages.FindAsync(id);
        }

        public async Task<CPiDefaultPage> GetDefaultPageByNameAsync(string name)
        {
            return await _repository.CPiDefaultPages.Where(d => d.Name == name).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<CPiDefaultPage> AddDefaultPageAsync(CPiDefaultPage defaultPage)
        {
            _repository.CPiDefaultPages.Add(defaultPage);
            await _repository.SaveChangesAsync();
            return defaultPage;
        }

        public async Task UpdateDefaultPageAsync(CPiDefaultPage defaultPage)
        {
            _repository.CPiDefaultPages.Update(defaultPage);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteDefaultPageAsync(CPiDefaultPage defaultPage)
        {
            _repository.CPiDefaultPages.Remove(defaultPage);
            await _repository.SaveChangesAsync();
        }

        #endregion

        #region PatDueDate
        public async Task UpdatePatDueDateRemarks(PatDueDate dueDate)
        {
            var entity = _repository.PatDueDates.Attach(dueDate);
            entity.Property(c => c.Remarks).IsModified = true;
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
            await _repository.SaveChangesAsync();
        }

        public async Task UpdatePatDueRemarks(PatActionDue actionDue)
        {
            var entity = _repository.PatActionDues.Attach(actionDue);
            entity.Property(c => c.Remarks).IsModified = true;
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
            await _repository.SaveChangesAsync();
        }

        public async Task UpdatePatDateTaken(PatDueDate dueDate)
        {
            var entity = _repository.PatDueDates.Attach(dueDate);
            entity.Property(c => c.DateTaken).IsModified = true;
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
            await _repository.SaveChangesAsync();
        }

        public async Task UpdatePatDateTakenInv(PatDueDateInv dueDate)
        {
            var entity = _repository.PatDueDateInvs.Attach(dueDate);
            entity.Property(c => c.DateTaken).IsModified = true;
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
            await _repository.SaveChangesAsync();
        }

        #endregion

        #region TmkDueDate
        public async Task UpdateTmkDueDateRemarks(TmkDueDate dueDate)
        {
            var entity = _repository.TmkDueDates.Attach(dueDate);
            entity.Property(c => c.Remarks).IsModified = true;
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
            await _repository.SaveChangesAsync();
        }
        public async Task UpdateTmkDueRemarks(TmkActionDue actionDue)
        {
            var entity = _repository.TmkActionDues.Attach(actionDue);
            entity.Property(c => c.Remarks).IsModified = true;
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
            await _repository.SaveChangesAsync();
        }
        public async Task UpdateTmkDateTaken(TmkDueDate dueDate)
        {
            var entity = _repository.TmkDueDates.Attach(dueDate);
            entity.Property(c => c.DateTaken).IsModified = true;
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
            await _repository.SaveChangesAsync();
        }
        #endregion

        // Removed during deep clean - GMDueDate, GMActionDue, DMSDueDate, DMSActionDue no longer exist
        // #region GMDueDate
        // public async Task UpdateGMDueDateRemarks(GMDueDate dueDate)
        // {
        //     var entity = _repository.GMDueDates.Attach(dueDate);
        //     entity.Property(c => c.Remarks).IsModified = true;
        //     entity.Property(c => c.UpdatedBy).IsModified = true;
        //     entity.Property(c => c.LastUpdate).IsModified = true;
        //     await _repository.SaveChangesAsync();
        // }
        // public async Task UpdateGMDueRemarks(GMActionDue actionDue)
        // {
        //     var entity = _repository.GMActionsDue.Attach(actionDue);
        //     entity.Property(c => c.Remarks).IsModified = true;
        //     entity.Property(c => c.UpdatedBy).IsModified = true;
        //     entity.Property(c => c.LastUpdate).IsModified = true;
        //     await _repository.SaveChangesAsync();
        // }
        // public async Task UpdateGMDateTaken(GMDueDate dueDate)
        // {
        //     var entity = _repository.GMDueDates.Attach(dueDate);
        //     entity.Property(c => c.DateTaken).IsModified = true;
        //     entity.Property(c => c.UpdatedBy).IsModified = true;
        //     entity.Property(c => c.LastUpdate).IsModified = true;
        //     await _repository.SaveChangesAsync();
        // }
        // #endregion

        // #region DMSDueDate
        // public async Task UpdateDMSDueDateRemarks(DMSDueDate dueDate)
        // {
        //     var entity = _repository.DMSDueDates.Attach(dueDate);
        //     entity.Property(c => c.Remarks).IsModified = true;
        //     entity.Property(c => c.UpdatedBy).IsModified = true;
        //     entity.Property(c => c.LastUpdate).IsModified = true;
        //     await _repository.SaveChangesAsync();
        // }
        // public async Task UpdateDMSDueRemarks(DMSActionDue actionDue)
        // {
        //     var entity = _repository.DMSActionDues.Attach(actionDue);
        //     entity.Property(c => c.Remarks).IsModified = true;
        //     entity.Property(c => c.UpdatedBy).IsModified = true;
        //     entity.Property(c => c.LastUpdate).IsModified = true;
        //     await _repository.SaveChangesAsync();
        // }
        // public async Task UpdateDMSDateTaken(DMSDueDate dueDate)
        // {
        //     var entity = _repository.DMSDueDates.Attach(dueDate);
        //     entity.Property(c => c.DateTaken).IsModified = true;
        //     entity.Property(c => c.UpdatedBy).IsModified = true;
        //     entity.Property(c => c.LastUpdate).IsModified = true;
        //     await _repository.SaveChangesAsync();
        // }
        // #endregion

        #region DeDocket
        public async Task UpdatePatDueDateDeDocket(PatDueDateDeDocket dueDateDeDocket)
        {
            if (dueDateDeDocket.DeDocketId == 0)
                _repository.PatDueDateDeDockets.Add(dueDateDeDocket);
            else
                _repository.PatDueDateDeDockets.Update(dueDateDeDocket);

            if (dueDateDeDocket.DateTaken != null) {
                await _repository.PatDueDates.Where(d => d.DDId == dueDateDeDocket.DDId).ExecuteUpdateAsync(d => d.SetProperty(p => p.DateTaken, p => dueDateDeDocket.DateTaken));
            }
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateTmkDueDateDeDocket(TmkDueDateDeDocket dueDateDeDocket)
        {
            if (dueDateDeDocket.DeDocketId == 0)
                _repository.TmkDueDateDeDockets.Add(dueDateDeDocket);
            else
                _repository.TmkDueDateDeDockets.Update(dueDateDeDocket);

            if (dueDateDeDocket.DateTaken != null)
            {
                await _repository.TmkDueDates.Where(d => d.DDId == dueDateDeDocket.DDId).ExecuteUpdateAsync(d => d.SetProperty(p => p.DateTaken, p => dueDateDeDocket.DateTaken));
            }

            await _repository.SaveChangesAsync();
        }

        // Removed during deep clean - GMDueDateDeDocket no longer exists
        // public async Task UpdateGMDueDateDeDocket(GMDueDateDeDocket dueDateDeDocket)
        // {
        //     if (dueDateDeDocket.DeDocketId == 0)
        //         _repository.GMDueDateDeDockets.Add(dueDateDeDocket);
        //     else
        //         _repository.GMDueDateDeDockets.Update(dueDateDeDocket);
        //
        //     if (dueDateDeDocket.DateTaken != null)
        //     {
        //         await _repository.GMDueDates.Where(d => d.DDId == dueDateDeDocket.DDId).ExecuteUpdateAsync(d => d.SetProperty(p => p.DateTaken, p => dueDateDeDocket.DateTaken));
        //     }
        //
        //     await _repository.SaveChangesAsync();
        // }
        #endregion

        public IQueryable<PatDueDateDeDocket> PatDueDateDeDockets => _repository.PatDueDateDeDockets.AsNoTracking();
        public IQueryable<TmkDueDateDeDocket> TmkDueDateDeDockets => _repository.TmkDueDateDeDockets.AsNoTracking();
        // Removed during deep clean
        // public IQueryable<GMDueDateDeDocket> GMDueDateDeDockets => _repository.GMDueDateDeDockets.AsNoTracking();
        public IQueryable<DeDocketInstruction> DeDocketInstructions =>  _repository.DeDocketInstructions.AsNoTracking();
        

    }
}


