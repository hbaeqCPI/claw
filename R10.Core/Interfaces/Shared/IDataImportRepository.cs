using R10.Core.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;


namespace R10.Core.Interfaces
{
    public interface IDataImportRepository
    {
        DataTable GetStructure(DataImportType type);
        Task UpdateMappings(int importId, List<DataImportMapping> mappings);
        Task UpdateErrors(int importId, List<DataImportError> errors, bool isUpdate);
        Task Import(DataImportType type, DataTable table, int importId, string options, string userName);
    }
}
