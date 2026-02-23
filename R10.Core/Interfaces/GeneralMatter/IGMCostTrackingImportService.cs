using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.GeneralMatter
{
    public interface IGMCostTrackingImportService
    {
        Task InitializeImportJob(GMCostTrackingImportHistory importJob);
        Task AddImportColumnNames(int importId, List<string> columns);
        Task UpdateMappings(IEnumerable<GMCostTrackingImportMapping> updated);
        Task<string> GetImportStatus(int importId);
        Task<GMCostTrackingImportHistory> GetImportHistory(int importId);
        //Task<DataImportType> GetDataImportType(int dataTypeId);
        Task<List<GMCostTrackingImportTypeColumn>> GetDataImportTypeColumns();
        DataTable GetStructure();
        Task Import(DataTable table, int importId, string options, string userName);
        Task UpdateErrors(int importId, List<GMCostTrackingImportError> errors);
        Task SaveChanges();

        IQueryable<GMCostTrackingImportHistory> GMCostTrackingImportsHistory { get; }
        //IQueryable<DataImportType> DataImportTypes { get; }
        IQueryable<GMCostTrackingImportMapping> GMCostTrackingImportMappings { get; }
        IQueryable<GMCostTrackingImportTypeColumn> GMCostTrackingImportTypeColumns { get; }
        IQueryable<GMCostTrackingImportError> GMCostTrackingImportErrors { get; }
        
    }
}
