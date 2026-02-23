using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Core.DTOs;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;

namespace R10.Core.Services.Shared
{
    public class QuickEmailService : IQuickEmailService
    {
        
        private readonly IApplicationDbContext _repository;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<DefaultSetting> _settings;

        public QuickEmailService(
            IApplicationDbContext repository,
            ISystemSettings<PatSetting> patSettings,
            ISystemSettings<DefaultSetting> settings)
        {
            _repository = repository;
            _patSettings = patSettings;
            _settings = settings;
        }

        public IQueryable<QEMain> GetQeMainByScreenId(int screenId)
        {
            return _repository.QEMains.AsNoTracking().Where(e => e.ScreenId == screenId);
        }

        public async Task<QEDataSource> GetQeDataSourceByIdAsync(int id)
        {
            return await _repository.QEDataSources.AsNoTracking().FirstOrDefaultAsync(d => d.DataSourceID == id);
        }

        public async Task<QELog> AddLogAsync(QELog log)
        {
            _repository.QELogs.Add(log);
            await _repository.SaveChangesAsync();
            return log;
        }


        //public async Task<FileHandler> AddFileHandlerAsync(FileHandler file)
        //{
        //    _repository.FileHandler.Add(file);
        //    await _repository.SaveChangesAsync();
        //    return file;
        //}

        public async Task<string> GetDefaultThumbnailAsync(string thumbnailPath)
        {
            var imageType = await _repository.ImageTypes.AsNoTracking().FirstOrDefaultAsync(i => i.DefaultImage == thumbnailPath);

            if (imageType != null)
            {
                if (!string.IsNullOrEmpty(imageType.DefaultImage))
                    return imageType.DefaultImage;
            }

            return string.Empty;
        }
        public IQueryable<QELog> QELogs => _repository.QELogs.AsNoTracking();
        public IQueryable<SystemScreen> SystemScreens => _repository.SystemScreens.AsNoTracking();
        public IQueryable<QEDataSource> QEDataSources => _repository.QEDataSources.AsNoTracking();
        public IQueryable<QEDataSource> QEDataSourcesFiltered
        {
            get
            {
                var dataSources = _repository.QEDataSources.Where(ds => ds.InUse == true).AsNoTracking();

                if (!_patSettings.GetSetting().Result.IsInventorRemunerationOn)
                {
                    dataSources = dataSources.Where(c => !c.DataSourceName.StartsWith("DE Remuneration"));
                }

                if (!_patSettings.GetSetting().Result.IsInventorFRRemunerationOn)
                {
                    dataSources = dataSources.Where(c => !c.DataSourceName.StartsWith("FR Remuneration"));
                }

                if (!_patSettings.GetSetting().Result.IsInventionActionOn)
                {
                    dataSources = dataSources.Where(c => !(c.DataSourceName.Contains("Invention Action") || c.DataSourceName.Contains("Invention Due") || c.DataSourceName.Contains("Invention DeDocket Instruction")));
                }

                if (!_patSettings.GetSetting().Result.IsInventionCostTrackingOn)
                {
                    dataSources = dataSources.Where(c => !(c.DataSourceName.StartsWith("Invention Cost Tracking")));
                }

                if (!_patSettings.GetSetting().Result.IsInventorAwardOn)
                {
                    dataSources = dataSources.Where(c => !(c.DataSourceName.Contains("Inventor App Award")));
                }

                if (!_settings.GetSetting().Result.IsDeDocketOn)
                {
                    dataSources = dataSources.Where(c => !(c.DataSourceName.Contains("DeDocket")));
                }

                if (!_settings.GetSetting().Result.IsDelegationOn)
                {
                    dataSources = dataSources.Where(c => !(c.DataSourceName.Contains("Delegation")));
                }   

                return dataSources;

            }
        }
        public IQueryable<QECustomField> QECustomFields => _repository.QECustomFields.AsNoTracking();

        public async Task<bool> QECustomFieldUpdate(int dataSourceId, string userName, string userEmail, IEnumerable<QECustomField> updatedData, IEnumerable<QECustomField> newData, IEnumerable<QECustomField> deletedData)
        {
            if (newData.Any())
            {
                //recSourceId = newFilterData.First().QERecordSource.RecSourceId; //correct automapper
                foreach (var item in newData)
                {
                    item.DataSourceID = dataSourceId;
                }
            }
            if (deletedData.Any())
            {
                dataSourceId = deletedData.First().DataSourceID;
            }

            var dataSource = await GetQeDataSourceByIdAsync(dataSourceId);
            //await UpdateChild<BaseEntity>(userName, recordSource, updatedFilterData, newFilterData, deletedFilterData);
            await UpdateChild(userName, dataSource, updatedData, newData, deletedData);
            return true;
        }
        private async Task UpdateChild<T1, T2>(string userName, T1 mainRecord, IEnumerable<T2> updated, IEnumerable<T2> added, IEnumerable<T2> deleted) where T1 : BaseEntity where T2 : BaseEntity
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                mainRecord.UpdatedBy = userName;
                mainRecord.LastUpdate = DateTime.Now;

                // update parent stamp fields
                var parent = _repository.Set<T1>().Attach(mainRecord);
                parent.Property(c => c.UpdatedBy).IsModified = true;
                parent.Property(c => c.LastUpdate).IsModified = true;

                foreach (var item in updated)
                {
                    item.UpdatedBy = userName;
                    item.LastUpdate = mainRecord.LastUpdate;
                }

                foreach (var item in added)
                {
                    item.CreatedBy = mainRecord.UpdatedBy;
                    item.DateCreated = mainRecord.LastUpdate;
                    item.UpdatedBy = mainRecord.UpdatedBy;
                    item.LastUpdate = mainRecord.LastUpdate;
                }

                var dbSet = _repository.Set<T2>();

                if (updated.Any())
                    dbSet.UpdateRange(updated);

                if (added.Any())
                    dbSet.AddRange(added);

                if (deleted.Any())
                    dbSet.RemoveRange(deleted);
                await _repository.SaveChangesAsync();

                scope.Complete();
            }
        }

        public async Task<List<QEFieldListDTO>> GetDataSourceFieldList(int dataSourceID, string sortColumn, string sortDirection)
        {
            var result = await _repository.QEFieldListDTO.FromSqlInterpolated($"procQE_DataSorceFieldList @DataSourceID={dataSourceID},  @SortColumn={sortColumn},  @SortDirection={sortDirection}, @DataType=DEFAULT")
                            .AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QEFieldListDTO>> GetDataSourceFieldList(int dataSourceID)
        {
            var result = await _repository.QEFieldListDTO.FromSqlInterpolated($"procQE_DataSorceFieldList @DataSourceID={dataSourceID},  @SortColumn=DEFAULT,  @SortDirection=DEFAULT, @DataType=DEFAULT")
                            .AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QEFieldListDTO>> GetDataSourceDateFieldList(int dataSourceID)
        {
            var result = await _repository.QEFieldListDTO.FromSqlInterpolated($"procQE_DataSorceFieldList @DataSourceID={dataSourceID},  @SortColumn=DEFAULT,  @SortDirection=DEFAULT, @DataType='datetime2'")
                            .AsNoTracking().ToListAsync();
            return result;
        }


    }
}
