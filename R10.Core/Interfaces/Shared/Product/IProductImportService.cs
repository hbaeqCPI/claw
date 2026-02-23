using R10.Core.Entities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IProductImportService
    {
        Task InitializeImportJob(ProductImportHistory importJob);
        Task AddImportColumnNames(int importId, List<string> columns);
        Task UpdateMappings(IEnumerable<ProductImportMapping> updated);
        Task<string> GetImportStatus(int importId);
        Task<ProductImportHistory> GetImportHistory(int importId);
        //Task<DataImportType> GetDataImportType(int dataTypeId);
        Task<List<ProductImportTypeColumn>> GetDataImportTypeColumns();
        DataTable GetStructure();
        Task Import(DataTable table, int importId, string options, string userName);
        Task UpdateErrors(int importId, List<ProductImportError> errors);
        Task SaveChanges();

        IQueryable<ProductImportHistory> ProductImportsHistory { get; }
        //IQueryable<DataImportType> DataImportTypes { get; }
        IQueryable<ProductImportMapping> ProductImportMappings { get; }
        IQueryable<ProductImportTypeColumn> ProductImportTypeColumns { get; }
        IQueryable<ProductImportError> ProductImportErrors { get; }
        
    }
}
