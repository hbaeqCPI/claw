using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Data;
using R10.Core.Interfaces.Patent;
using R10.Core.Interfaces;
using R10.Core.Entities.Patent;

namespace R10.Core.Services
{
    public class PatCostTrackingImportService : IPatCostTrackingImportService
    {
        private readonly IApplicationDbContext _repository;
        private readonly IPatCostTrackingImportRepository _importRepository;

        public PatCostTrackingImportService(IApplicationDbContext repository, IPatCostTrackingImportRepository importRepository)
        {
            _repository = repository;
            _importRepository = importRepository;
        }

        public async Task InitializeImportJob(PatCostTrackingImportHistory importJob)
        {
            if (importJob.ImportId == 0)
                _repository.PatCostTrackingImportsHistory.Add(importJob);

            else {
                var entity = _repository.PatCostTrackingImportsHistory.Attach(importJob);
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

            var typeColumns = await PatCostTrackingImportTypeColumns.OrderByDescending(t => t.ColumnName.Length).ToListAsync();
            var list = columns.Select(c => new PatCostTrackingImportMapping { ImportId = importId, YourField = c }).ToList();

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

        public async Task UpdateMappings(IEnumerable<PatCostTrackingImportMapping> updated)
        {
            if (updated.Any()) {
                _repository.PatCostTrackingImportMappings.UpdateRange(updated);
                await _repository.SaveChangesAsync();
            }
        }

        public async Task<string> GetImportStatus(int importId)
        {
            return await PatCostTrackingImportsHistory.Where(h => h.ImportId == importId).Select(h => h.Status).FirstOrDefaultAsync();
        }

        public async Task<PatCostTrackingImportHistory> GetImportHistory(int importId) {
            return await PatCostTrackingImportsHistory.FirstOrDefaultAsync(h => h.ImportId == importId);
        }

        public async Task<List<PatCostTrackingImportTypeColumn>> GetDataImportTypeColumns()
        {
            var list = await PatCostTrackingImportTypeColumns.ToListAsync();
            return list;
        }

        public DataTable GetStructure() {
            return _importRepository.GetStructure();
        }

        public async Task Import(DataTable table, int importId, string options, string userName)
        {
            await _importRepository.Import(table, importId, options, userName);
        }

        public async Task UpdateErrors(int importId, List<PatCostTrackingImportError> errors)
        {
            await _importRepository.UpdateErrors(importId, errors);
        }

        public IQueryable<PatCostTrackingImportHistory> PatCostTrackingImportsHistory => _repository.PatCostTrackingImportsHistory;        
        public IQueryable<PatCostTrackingImportMapping> PatCostTrackingImportMappings => _repository.PatCostTrackingImportMappings;
        public IQueryable<PatCostTrackingImportTypeColumn> PatCostTrackingImportTypeColumns => _repository.PatCostTrackingImportTypeColumns;
        public IQueryable<PatCostTrackingImportError> PatCostTrackingImportErrors => _repository.PatCostTrackingImportErrors;
    }

    
}
