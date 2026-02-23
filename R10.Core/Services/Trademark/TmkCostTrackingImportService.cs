using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Data;
using R10.Core.Interfaces.Trademark;
using R10.Core.Interfaces;
using R10.Core.Entities.Trademark;

namespace R10.Core.Services
{
    public class TmkCostTrackingImportService : ITmkCostTrackingImportService
    {
        private readonly IApplicationDbContext _repository;
        private readonly ITmkCostTrackingImportRepository _importRepository;

        public TmkCostTrackingImportService(IApplicationDbContext repository, ITmkCostTrackingImportRepository importRepository)
        {
            _repository = repository;
            _importRepository = importRepository;
        }

        public async Task InitializeImportJob(TmkCostTrackingImportHistory importJob)
        {
            if (importJob.ImportId == 0)
                _repository.TmkCostTrackingImportHistory.Add(importJob);

            else {
                var entity = _repository.TmkCostTrackingImportHistory.Attach(importJob);
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

            var typeColumns = await TmkCostTrackingImportTypeColumns.OrderByDescending(t => t.ColumnName.Length).ToListAsync();
            var list = columns.Select(c => new TmkCostTrackingImportMapping { ImportId = importId, YourField = c }).ToList();

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

        public async Task UpdateMappings(IEnumerable<TmkCostTrackingImportMapping> updated)
        {
            if (updated.Any()) {
                _repository.TmkCostTrackingImportMappings.UpdateRange(updated);
                await _repository.SaveChangesAsync();
            }
        }

        public async Task<string> GetImportStatus(int importId)
        {
            return await TmkCostTrackingImportsHistory.Where(h => h.ImportId == importId).Select(h => h.Status).FirstOrDefaultAsync();
        }

        public async Task<TmkCostTrackingImportHistory> GetImportHistory(int importId) {
            return await TmkCostTrackingImportsHistory.FirstOrDefaultAsync(h => h.ImportId == importId);
        }

        public async Task<List<TmkCostTrackingImportTypeColumn>> GetDataImportTypeColumns()
        {
            var list = await TmkCostTrackingImportTypeColumns.ToListAsync();
            return list;
        }

        public DataTable GetStructure() {
            return _importRepository.GetStructure();
        }

        public async Task Import(DataTable table, int importId, string options, string userName)
        {
            await _importRepository.Import(table, importId, options, userName);
        }

        public async Task UpdateErrors(int importId, List<TmkCostTrackingImportError> errors)
        {
            await _importRepository.UpdateErrors(importId, errors);
        }

        public IQueryable<TmkCostTrackingImportHistory> TmkCostTrackingImportsHistory => _repository.TmkCostTrackingImportHistory;        
        public IQueryable<TmkCostTrackingImportMapping> TmkCostTrackingImportMappings => _repository.TmkCostTrackingImportMappings;
        public IQueryable<TmkCostTrackingImportTypeColumn> TmkCostTrackingImportTypeColumns => _repository.TmkCostTrackingImportTypeColumns;
        public IQueryable<TmkCostTrackingImportError> TmkCostTrackingImportErrors => _repository.TmkCostTrackingImportErrors;
    }

    
}
