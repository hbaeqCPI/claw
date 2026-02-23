using R10.Core.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;


namespace R10.Core.Interfaces
{
    public interface IProductImportRepository
    {
        DataTable GetStructure();
        Task UpdateMappings(int importId, List<ProductImportMapping> mappings);
        Task UpdateErrors(int importId, List<ProductImportError> errors);
        Task Import(DataTable table, int importId, string options, string userName);
    }
}
