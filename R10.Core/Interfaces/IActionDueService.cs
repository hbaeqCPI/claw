using R10.Core.Entities.Patent;
using R10.Core.Services.GeneralMatter;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IActionDueService<T1, T2> : IEntityService<T1>
    {
        Task<bool> CanModifyAttorney(int responsibleId);
        Task<T2> GetRecurringDueDate(T1 actionDue, T2 dueDate);
        Task UpdateResponseDate(T1 entity);
        Task RetroGenerateActionDues(ActionDueRetroParam criteria);
        Task UpdateCheckDocket(T1 entity);
    }

    public interface IActionDueDeDocketService<T1, T2> : IActionDueService<T1, T2>
    {
        Task UpdateDeDocket(T1 actionDue);
    }
}
