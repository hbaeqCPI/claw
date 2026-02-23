using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace R10.Core.Services
{
    public class DataImportService : IDataImportService
    {
        private readonly IApplicationDbContext _repository;
        private readonly IDataImportRepository _importRepository;

        public DataImportService(IApplicationDbContext repository, IDataImportRepository importRepository)
        {
            _repository = repository;
            _importRepository = importRepository;
        }

        public async Task<List<DataImportType>> GetDataImportTypes(string systemType, bool isUpdate = false)
        {
            var list = await DataImportTypes.Where(t => (t.SystemType == systemType || string.IsNullOrEmpty(t.SystemType) || t.SystemType.Contains(systemType)) && t.InUse && t.IsUpdate == isUpdate).ToListAsync();
            return list;
        }

        public async Task InitializeImportJob(DataImportHistory importJob)
        {
            if (importJob.ImportId == 0)
                _repository.DataImportsHistory.Add(importJob);

            else {
                var entity = _repository.DataImportsHistory.Attach(importJob);
                entity.Property(c => c.ImportDate).IsModified = true;
                entity.Property(c => c.OrigFileName).IsModified = true;
                entity.Property(c => c.DataTypeId).IsModified = true;
                entity.Property(c => c.NoOfRecords).IsModified = true;
                entity.Property(c => c.Status).IsModified = true;
                entity.Property(c => c.ImportedBy).IsModified = true;
                entity.Property(c => c.SystemType).IsModified = true;
            }
            await _repository.SaveChangesAsync();
        }

        public async Task AddImportColumnNames(int importId, List<string> columns)
        {
            var importHistory = await GetImportHistory(importId);

            var typeColumns = await DataImportTypeColumns.Where(t => t.TableType == importHistory.DataType.TableType).OrderByDescending(t => t.ColumnName.Length).ToListAsync();
            var list = columns.Select(c => new DataImportMapping { ImportId = importId, YourField = c }).ToList();

            var order = 0;
            list.ForEach(c => {
                c.DisplayOrder = order++;
                var yourField = c.YourField.Replace(" ", "");
                var typeColumn = typeColumns.FirstOrDefault(tc => yourField.Replace("No.","Number").Replace(" ","").ToLower().Contains(tc.ColumnName.ToLower()));

                if (typeColumn == null) {
                    typeColumn = typeColumns.FirstOrDefault(tc => tc.ColumnName.ToLower().Contains(yourField.Replace("No.", "Number").Replace(" ", "").ToLower()));
                }

                if (typeColumn == null)
                {
                    typeColumn = typeColumns.FirstOrDefault(tc => tc.ColumnName.ToLower().Contains(yourField.Replace("Reference", "Ref").ToLower()));
                }

                if (typeColumn != null)
                    c.CPIField = typeColumn.ColumnName;
                
            });
            await _importRepository.UpdateMappings(importId, list);
        }

        public async Task SaveChanges()
        {
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateMappings(IEnumerable<DataImportMapping> updated)
        {
            if (updated.Any()) {
                _repository.DataImportMappings.UpdateRange(updated);
                await _repository.SaveChangesAsync();
            }
        }

        public async Task<string> GetImportStatus(int importId)
        {
            return await DataImportsHistory.Where(h => h.ImportId == importId).Select(h => h.Status).FirstOrDefaultAsync();
        }

        public async Task<DataImportHistory> GetImportHistory(int importId) {
            return await DataImportsHistory.Include(h => h.DataType).FirstOrDefaultAsync(h => h.ImportId == importId);
        }

        public async Task<DataImportType> GetDataImportType(int dataTypeId)
        {
            var dataType = await DataImportTypes.Where(t => t.DataTypeId == dataTypeId).FirstOrDefaultAsync();
            return dataType;
        }

        public async Task<List<DataImportTypeColumn>> GetDataImportTypeColumns(string tableType)
        {
            var list = await DataImportTypeColumns.Where(t => t.TableType == tableType).ToListAsync();
            return list;
        }


        public DataTable GetStructure(DataImportType type) {
            return _importRepository.GetStructure(type);
        }

        public async Task Import(DataImportType type, DataTable table, int importId, string options, string userName)
        {
            await _importRepository.Import(type, table, importId, options, userName);
        }

        public async Task UpdateErrors(int importId, List<DataImportError> errors, bool isUpdate)
        {
            await _importRepository.UpdateErrors(importId, errors, isUpdate);
        }

        public async Task UpdateImportHistory(DataImportHistory importHistory)
        {
            if (importHistory.ImportId > 0)
            {
                _repository.DataImportsHistory.Update(importHistory);
                await _repository.SaveChangesAsync();       
                _repository.Entry(importHistory).State = EntityState.Detached;
            }                
        }

        public async Task AddErrors(List<DataImportError> errors)
        {
            _repository.DataImportErrors.AddRange(errors);
            await _repository.SaveChangesAsync();       
        }

        public IQueryable<DataImportHistory> DataImportsHistory => _repository.DataImportsHistory;
        public IQueryable<DataImportType> DataImportTypes => _repository.DataImportTypes;
        public IQueryable<DataImportMapping> DataImportMappings => _repository.DataImportMappings;
        public IQueryable<DataImportTypeColumn> DataImportTypeColumns => _repository.DataImportTypeColumns;
        public IQueryable<DataImportError> DataImportErrors => _repository.DataImportErrors;
    }

    
}
