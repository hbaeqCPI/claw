using R10.Core.Entities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace R10.Core.Interfaces
{
    public interface IDataImportService
    {
        Task<List<DataImportType>> GetDataImportTypes(string systemType, bool isUpdate = false);
        Task InitializeImportJob(DataImportHistory importJob);
        Task AddImportColumnNames(int importId, List<string> columns);
        Task UpdateMappings(IEnumerable<DataImportMapping> updated);
        Task<string> GetImportStatus(int importId);
        Task<DataImportHistory> GetImportHistory(int importId);
        Task<DataImportType> GetDataImportType(int dataTypeId);
        Task<List<DataImportTypeColumn>> GetDataImportTypeColumns(string tableType);
        DataTable GetStructure(DataImportType type);
        Task Import(DataImportType type, DataTable table, int importId, string options, string userName);
        Task UpdateErrors(int importId, List<DataImportError> errors, bool isUpdate);
        Task SaveChanges();

        Task UpdateImportHistory(DataImportHistory importHistory);

        Task AddErrors(List<DataImportError> erors);

        IQueryable<DataImportHistory> DataImportsHistory { get; }
        IQueryable<DataImportType> DataImportTypes { get; }
        IQueryable<DataImportMapping> DataImportMappings { get; }
        IQueryable<DataImportTypeColumn> DataImportTypeColumns { get; }
        IQueryable<DataImportError> DataImportErrors { get; }
        
    }
}
