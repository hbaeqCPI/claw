using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;


namespace R10.Core.Interfaces.Patent
{
    public interface IPatCostTrackingImportRepository
    {
        DataTable GetStructure();
        Task UpdateMappings(int importId, List<PatCostTrackingImportMapping> mappings);
        Task UpdateErrors(int importId, List<PatCostTrackingImportError> errors);
        Task Import(DataTable table, int importId, string options, string userName);
    }
}
