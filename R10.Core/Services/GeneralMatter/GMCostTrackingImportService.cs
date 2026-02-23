using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Data;
using R10.Core.Interfaces.GeneralMatter;
using R10.Core.Interfaces;
using R10.Core.Entities.GeneralMatter;

namespace R10.Core.Services.GeneralMatter
{
    public class GMCostTrackingImportService : IGMCostTrackingImportService
    {
        private readonly IApplicationDbContext _repository;
        private readonly IGMCostTrackingImportRepository _importRepository;

        public GMCostTrackingImportService(IApplicationDbContext repository, IGMCostTrackingImportRepository importRepository)
        {
            _repository = repository;
            _importRepository = importRepository;
        }

        public async Task InitializeImportJob(GMCostTrackingImportHistory importJob)
        {
            if (importJob.ImportId == 0)
                _repository.GMCostTrackingImportHistory.Add(importJob);

            else {
                var entity = _repository.GMCostTrackingImportHistory.Attach(importJob);
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

            var typeColumns = await GMCostTrackingImportTypeColumns.OrderByDescending(t => t.ColumnName.Length).ToListAsync();
            var list = columns.Select(c => new GMCostTrackingImportMapping { ImportId = importId, YourField = c }).ToList();

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

        public async Task UpdateMappings(IEnumerable<GMCostTrackingImportMapping> updated)
        {
            if (updated.Any()) {
                _repository.GMCostTrackingImportMappings.UpdateRange(updated);
                await _repository.SaveChangesAsync();
            }
        }

        public async Task<string> GetImportStatus(int importId)
        {
            return await GMCostTrackingImportsHistory.Where(h => h.ImportId == importId).Select(h => h.Status).FirstOrDefaultAsync();
        }

        public async Task<GMCostTrackingImportHistory> GetImportHistory(int importId) {
            return await GMCostTrackingImportsHistory.FirstOrDefaultAsync(h => h.ImportId == importId);
        }

        public async Task<List<GMCostTrackingImportTypeColumn>> GetDataImportTypeColumns()
        {
            var list = await GMCostTrackingImportTypeColumns.ToListAsync();
            return list;
        }

        public DataTable GetStructure() {
            return _importRepository.GetStructure();
        }

        public async Task Import(DataTable table, int importId, string options, string userName)
        {
            await _importRepository.Import(table, importId, options, userName);
        }

        public async Task UpdateErrors(int importId, List<GMCostTrackingImportError> errors)
        {
            await _importRepository.UpdateErrors(importId, errors);
        }

        public IQueryable<GMCostTrackingImportHistory> GMCostTrackingImportsHistory => _repository.GMCostTrackingImportHistory;        
        public IQueryable<GMCostTrackingImportMapping> GMCostTrackingImportMappings => _repository.GMCostTrackingImportMappings;
        public IQueryable<GMCostTrackingImportTypeColumn> GMCostTrackingImportTypeColumns => _repository.GMCostTrackingImportTypeColumns;
        public IQueryable<GMCostTrackingImportError> GMCostTrackingImportErrors => _repository.GMCostTrackingImportErrors;
    }

    
}
