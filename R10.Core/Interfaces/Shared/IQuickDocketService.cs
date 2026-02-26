using R10.Core.DTOs;
using R10.Core.Entities;
// using R10.Core.Entities.DMS; // Removed during deep clean
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Identity;
using R10.Core.Queries.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IQuickDocketService
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

        Task<CPiDefaultPage> GetDefaultPageByIdAsync(int id);
        Task<CPiDefaultPage> GetDefaultPageByNameAsync(string name);
        Task<CPiDefaultPage> AddDefaultPageAsync(CPiDefaultPage defaultPage);
        Task UpdateDefaultPageAsync(CPiDefaultPage defaultPage);
        Task DeleteDefaultPageAsync(CPiDefaultPage defaultPage);

        Task UpdatePatDueDateRemarks(PatDueDate dueDate);
        Task UpdateTmkDueDateRemarks(TmkDueDate dueDate);
        // Removed during deep clean
        // Task UpdateGMDueDateRemarks(GMDueDate dueDate);
        Task UpdatePatDueRemarks(PatActionDue actionDue);
        Task UpdateTmkDueRemarks(TmkActionDue actionDue);
        // Removed during deep clean
        // Task UpdateGMDueRemarks(GMActionDue actionDue);

        Task UpdatePatDateTaken(PatDueDate dueDate);
        Task UpdatePatDateTakenInv(PatDueDateInv dueDate);
        Task UpdateTmkDateTaken(TmkDueDate dueDate);
        // Removed during deep clean
        // Task UpdateGMDateTaken(GMDueDate dueDate);
        // Task UpdateDMSDateTaken(DMSDueDate dueDate);

        Task UpdatePatDueDateDeDocket(PatDueDateDeDocket dueDateDeDocket);
        Task UpdateTmkDueDateDeDocket(TmkDueDateDeDocket dueDateDeDocket);
        // Removed during deep clean
        // Task UpdateGMDueDateDeDocket(GMDueDateDeDocket dueDateDeDocket);
        // Task UpdateDMSDueDateRemarks(DMSDueDate dueDate);
        // Task UpdateDMSDueRemarks(DMSActionDue actionDue);

        IQueryable<PatDueDateDeDocket> PatDueDateDeDockets { get; }
        IQueryable<TmkDueDateDeDocket> TmkDueDateDeDockets { get; }
        // Removed during deep clean
        // IQueryable<GMDueDateDeDocket> GMDueDateDeDockets { get; }
        IQueryable<DeDocketInstruction> DeDocketInstructions { get; }
    }
}
