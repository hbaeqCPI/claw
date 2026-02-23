using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.Patent
{
    public interface IPatCostTrackingImportService
    {
        Task InitializeImportJob(PatCostTrackingImportHistory importJob);
        Task AddImportColumnNames(int importId, List<string> columns);
        Task UpdateMappings(IEnumerable<PatCostTrackingImportMapping> updated);
        Task<string> GetImportStatus(int importId);
        Task<PatCostTrackingImportHistory> GetImportHistory(int importId);
        //Task<DataImportType> GetDataImportType(int dataTypeId);
        Task<List<PatCostTrackingImportTypeColumn>> GetDataImportTypeColumns();
        DataTable GetStructure();
        Task Import(DataTable table, int importId, string options, string userName);
        Task UpdateErrors(int importId, List<PatCostTrackingImportError> errors);
        Task SaveChanges();

        IQueryable<PatCostTrackingImportHistory> PatCostTrackingImportsHistory { get; }
        //IQueryable<DataImportType> DataImportTypes { get; }
        IQueryable<PatCostTrackingImportMapping> PatCostTrackingImportMappings { get; }
        IQueryable<PatCostTrackingImportTypeColumn> PatCostTrackingImportTypeColumns { get; }
        IQueryable<PatCostTrackingImportError> PatCostTrackingImportErrors { get; }
        
    }
}
