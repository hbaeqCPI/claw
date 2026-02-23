using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IDueDateService<T1, T2> : IChildEntityService<T1, T2>
    {
        IQueryable<T1> ActionsDue { get; }
        Task<bool> Update(int parentId, string userName, IEnumerable<T2> updated, IEnumerable<T2> deleted);
        Task<bool> UpdateDeDocket(string userName, IEnumerable<T2> updated);
        Task<bool> UpdateExtensionSetting(DueDateExtension setting);
        Task<DueDateExtension> GetExtensionSetting(int ddId, int parentId);
        IQueryable<DueDateExtension> DueDateExtensions { get; }
        Task<DueDateDeDocket> UpdateDeDocketFileInfo(int ddId, int deDocketId, string? docFile, int fileId, string userName,string? driveItemId);
        IQueryable<DueDateDeDocket> DueDateDeDockets { get; }
        IQueryable<DueDateDeDocketResp> DueDateDeDocketResps { get; }
        Task UpdateDeDocketResp(List<string> responsibleList, string userName, int deDocketId);
        Task MarkDeDocketInstructionsAsCompleted(List<int> deDocketIds, DateTime? completedDate);
    }
}
