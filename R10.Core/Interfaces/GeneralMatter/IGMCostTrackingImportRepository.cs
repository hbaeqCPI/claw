using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;


namespace R10.Core.Interfaces.GeneralMatter
{
    public interface IGMCostTrackingImportRepository
    {
        DataTable GetStructure();
        Task UpdateMappings(int importId, List<GMCostTrackingImportMapping> mappings);
        Task UpdateErrors(int importId, List<GMCostTrackingImportError> errors);
        Task Import(DataTable table, int importId, string options, string userName);
    }
}
