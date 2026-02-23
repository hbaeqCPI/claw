using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;


namespace R10.Core.Interfaces.Trademark
{
    public interface ITmkCostTrackingImportRepository
    {
        DataTable GetStructure();
        Task UpdateMappings(int importId, List<TmkCostTrackingImportMapping> mappings);
        Task UpdateErrors(int importId, List<TmkCostTrackingImportError> errors);
        Task Import(DataTable table, int importId, string options, string userName);
    }
}
