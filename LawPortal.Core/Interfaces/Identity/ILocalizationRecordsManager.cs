using LawPortal.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface ILocalizationRecordsManager
    {
        IQueryable<LocalizationRecords> LocalizationRecords { get; }
        IQueryable<LocalizationRecordsGrouping> LocalizationRecordsGrouping { get; }
        Task<List<string>> GetSystems();
        Task<List<string>> GetMenuItems(string system);

        void Update(List<LocalizationRecords> translates);
        void Add(List<LocalizationRecords> translates);
        Task Save();

    }
}
