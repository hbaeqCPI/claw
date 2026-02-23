using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.Trademark
{
    public interface ITmkCostTrackingImportService
    {
        Task InitializeImportJob(TmkCostTrackingImportHistory importJob);
        Task AddImportColumnNames(int importId, List<string> columns);
        Task UpdateMappings(IEnumerable<TmkCostTrackingImportMapping> updated);
        Task<string> GetImportStatus(int importId);
        Task<TmkCostTrackingImportHistory> GetImportHistory(int importId);
        //Task<DataImportType> GetDataImportType(int dataTypeId);
        Task<List<TmkCostTrackingImportTypeColumn>> GetDataImportTypeColumns();
        DataTable GetStructure();
        Task Import(DataTable table, int importId, string options, string userName);
        Task UpdateErrors(int importId, List<TmkCostTrackingImportError> errors);
        Task SaveChanges();

        IQueryable<TmkCostTrackingImportHistory> TmkCostTrackingImportsHistory { get; }
        //IQueryable<DataImportType> DataImportTypes { get; }
        IQueryable<TmkCostTrackingImportMapping> TmkCostTrackingImportMappings { get; }
        IQueryable<TmkCostTrackingImportTypeColumn> TmkCostTrackingImportTypeColumns { get; }
        IQueryable<TmkCostTrackingImportError> TmkCostTrackingImportErrors { get; }
        
    }
}
