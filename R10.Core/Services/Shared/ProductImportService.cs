using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Data;
using R10.Core.Interfaces;

namespace R10.Core.Services
{
    public class ProductImportService : IProductImportService
    {
        private readonly IApplicationDbContext _repository;
        private readonly IProductImportRepository _importRepository;

        public ProductImportService(IApplicationDbContext repository, IProductImportRepository importRepository)
        {
            _repository = repository;
            _importRepository = importRepository;
        }

        public async Task InitializeImportJob(ProductImportHistory importJob)
        {
            if (importJob.ImportId == 0)
                _repository.ProductImportHistory.Add(importJob);

            else {
                var entity = _repository.ProductImportHistory.Attach(importJob);
                entity.Property(c => c.ImportDate).IsModified = true;
                entity.Property(c => c.OrigFileName).IsModified = true;
                entity.Property(c => c.NoOfRecords).IsModified = true;
                entity.Property(c => c.Status).IsModified = true;
                entity.Property(c => c.ImportedBy).IsModified = true;
            }
            await _repository.SaveChangesAsync();
        }

        public async Task AddImportColumnNames(int importId, List<string> columns)
        {
            var importHistory = await GetImportHistory(importId);

            var typeColumns = await ProductImportTypeColumns.OrderByDescending(t => t.ColumnName.Length).ToListAsync();
            var list = columns.Select(c => new ProductImportMapping { ImportId = importId, YourField = c }).ToList();

            var order = 0;
            list.ForEach(c => {
                c.DisplayOrder = order++;
                var yourField = c.YourField.Replace(" ", "");
                var typeColumn = typeColumns.FirstOrDefault(tc => yourField.ToLower().Contains(tc.ColumnName.ToLower()));
                if (typeColumn != null)
                    c.CPIField = typeColumn.ColumnName;
            });
            await _importRepository.UpdateMappings(importId, list);
        }

        public async Task SaveChanges()
        {
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateMappings(IEnumerable<ProductImportMapping> updated)
        {
            if (updated.Any()) {
                _repository.ProductImportMappings.UpdateRange(updated);
                await _repository.SaveChangesAsync();
            }
        }

        public async Task<string> GetImportStatus(int importId)
        {
            return await ProductImportsHistory.Where(h => h.ImportId == importId).Select(h => h.Status).FirstOrDefaultAsync();
        }

        public async Task<ProductImportHistory> GetImportHistory(int importId) {
            return await ProductImportsHistory.FirstOrDefaultAsync(h => h.ImportId == importId);
        }

        public async Task<List<ProductImportTypeColumn>> GetDataImportTypeColumns()
        {
            var list = await ProductImportTypeColumns.ToListAsync();
            return list;
        }

        public DataTable GetStructure() {
            return _importRepository.GetStructure();
        }

        public async Task Import(DataTable table, int importId, string options, string userName)
        {
            await _importRepository.Import(table, importId, options, userName);
        }

        public async Task UpdateErrors(int importId, List<ProductImportError> errors)
        {
            await _importRepository.UpdateErrors(importId, errors);
        }

        public IQueryable<ProductImportHistory> ProductImportsHistory => _repository.ProductImportHistory;        
        public IQueryable<ProductImportMapping> ProductImportMappings => _repository.ProductImportMappings;
        public IQueryable<ProductImportTypeColumn> ProductImportTypeColumns => _repository.ProductImportTypeColumns;
        public IQueryable<ProductImportError> ProductImportErrors => _repository.ProductImportErrors;
    }

    
}
