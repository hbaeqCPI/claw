using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Core.Interfaces.Patent
{
    public interface IPatIDSManageService
    {
        Task IDSUpdateFilDate(int appId, string filDateType, string recordType, DateTime? filDate, DateTime? specificFilDate,bool consideredByExaminer);
        Task UpdateConsideredByExaminer(int appId, string filDateType, string recordType, DateTime? filDateFrom, DateTime? filDateTo, DateTime? specificFilDate);

        IQueryable<PatIDSManageDTO> IDSManageCases { get; }
        IQueryable<PatKeyword> InventionKeywords { get; }
        IQueryable<PatInventorApp> PatInventorsApps { get; }

        
    }
}
