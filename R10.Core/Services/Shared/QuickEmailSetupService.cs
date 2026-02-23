using System;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;

namespace R10.Core.Services.Shared
{
    public class QuickEmailSetupService : IQuickEmailSetupService
    {
        private readonly IApplicationDbContext _repository;
        private readonly IQEDataSourceRepository _qeDataSourceRepository;
        

        public QuickEmailSetupService(
            IApplicationDbContext repository,
            IQEDataSourceRepository qeDataSourceRepository)
        {
            _repository = repository;
            _qeDataSourceRepository = qeDataSourceRepository;
        }

        #region Module Main

        public async Task<ModuleMain> GetModuleByIdAsync(int id)
        {
            return await _repository.ModulesMain.FindAsync(id);
        }

        public IQueryable<ModuleMain> GetModulesBySystemTypeAsync(string systemType)
        {
            return _repository.ModulesMain.Where(e => e.SystemType == systemType && e.DocClass == "QE");
        }

        public async Task<ModuleMain> GetModuleByNameAndSystemTypeAsync(string name, string systemType)
        {
            return await _repository.ModulesMain.Where(e => e.ModuleName == name).FirstOrDefaultAsync();
        }

        #endregion

        #region System Screen

        public IQueryable<SystemScreen> GetSystemScreensBySystemTypeAsync(string systemType)
        {
            return _repository.SystemScreens.Where(e => (e.SystemType == systemType || e.SystemType == "S") && e.FeatureType == "QE");
        }

        public IQueryable<SystemScreen> SystemScreens => _repository.SystemScreens;
        #endregion

        #region Main

        public async Task<QEMain> GetQEMainByIdAsync(int id)
        {
            return await _repository.QEMains.FindAsync(id);
        }

        public IQueryable<QEMain> GetQEMainBySystemType(string systemType)
        {
            return _repository.QEMains.Where(e => e.SystemScreen.SystemType == systemType || e.SystemScreen.SystemType == "S");
        }

        public IQueryable<QEMain> GetQEMains()
        {
            return _repository.QEMains;
        }

        public IQueryable<QEMain> GetQEMainByScreenId(int screenId)
        {
            return _repository.QEMains.Where(e => e.ScreenId == screenId);
        }

        public async Task<QEMain> AddQEMainAsync(QEMain quickEmail)
        {
            if (quickEmail.IsDefault)
                await ClearExistingDefault(quickEmail);

            await _repository.QEMains.AddAsync(quickEmail);
            await _repository.SaveChangesAsync();
            return quickEmail;
        }

        public async Task UpdateQEMainAsync(QEMain quickEmail)
        {
           _repository.QEMains.Update(quickEmail);
            await _repository.SaveChangesAsync();

            if (quickEmail.IsDefault)
                await ClearExistingDefault(quickEmail);

        }

        public async Task DeleteQEMainAsync(QEMain quickEmail)
        {
            _repository.QEMains.Remove(quickEmail);
            await _repository.SaveChangesAsync();
        }

        private async Task ClearExistingDefault(QEMain quickEmail)
        {
            var defaultQuickEmails = _repository.QEMains
                .Where(e => e.ScreenId == quickEmail.ScreenId && e.QESetupID != quickEmail.QESetupID).ToList();

            defaultQuickEmails.ForEach(
                qe =>
                {
                    qe.IsDefault = false;
                    qe.LastUpdate = quickEmail.LastUpdate;
                    qe.UpdatedBy = quickEmail.UpdatedBy;
                    _repository.QEMains.Update(qe);
                }
                );
            await _repository.SaveChangesAsync();
        }

        #endregion

        #region Recipient

        public async Task<QERecipient> GetRecipientByIdAsync(int id)
        {
            return await _repository.QERecipients.AsNoTracking().FirstOrDefaultAsync(r=> r.RecipientID==id);
        }
        public async Task<List<QERecipient>> GetRecipientsByParentIdAsync(int parentId)
        {
            var recipients = await _repository.QERecipients.AsNoTracking().Where(e => e.QESetupID == parentId).ToListAsync();
            return recipients;
        }

        public async Task<int> GetRecipientCount(int qeSetupId)
        {
            var recipients = await _repository.QERecipients.AsNoTracking().Where(e => e.QESetupID == qeSetupId).ToListAsync();
            return recipients.Count();
        }

        public async Task<QERecipient> AddRecipientAsync(QERecipient recipient)
        {
            _repository.QERecipients.Add(recipient);
            await _repository.SaveChangesAsync();
            return recipient;
        }

        public async Task DeleteRecipientAsync(QERecipient recipient)
        {
            _repository.QERecipients.Remove(recipient);
            await _repository.SaveChangesAsync();
        }


        public async Task UpdateRecipientAsync(QERecipient recipient)
        {
            _repository.QERecipients.Update(recipient);
            await _repository.SaveChangesAsync();
        }

        public async Task<bool> RecipientsUpdateAsync(int qeSetupId, string userName, byte[] tStamp,
            IList<QERecipient> updatedRecipients,
            IList<QERecipient> newRecipients, IList<QERecipient> deletedRecipients)
        {
            if (updatedRecipients.Any())
            {
                _repository.QERecipients.UpdateRange(updatedRecipients);
            }

            if (newRecipients.Any())
            {
                _repository.QERecipients.AddRange(newRecipients);
            }

            var qeMain = new QEMain() { QESetupID = qeSetupId, UpdatedBy = userName, LastUpdate = DateTime.Now, tStamp = tStamp };
            var entity = _repository.QEMains.Attach(qeMain);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;
            
            await _repository.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Layout


        public async Task<QELayout> GetQELayoutByIdAsync(int id)
        {
            return await _repository.QELayouts.FindAsync(id);
        }

        public async Task<QELayout> AddUserLayoutAsync(QELayout layout)
        {
             await _repository.QELayouts.AddAsync(layout);
            await _repository.SaveChangesAsync();
            return layout;
        }

        public async Task DeleteLayoutAsync(QELayout layout)
        {
            _repository.QELayouts.Remove(layout);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateLayoutAsync(QELayout layout)
        {
            _repository.QELayouts.Update(layout);
            await _repository.SaveChangesAsync();
            
        }
        #endregion

        #region Data Source

        public async Task<QEDataSource> GetQEDataSourceByIdAsync(int id)
        {
            return await _repository.QEDataSources.AsNoTracking().FirstOrDefaultAsync(q => q.DataSourceID == id);
        }

        public IQueryable<QEDataSource> GetQEDataSourcesBySystemTypeAsync(string systemType)
        {
            return _repository.QEDataSources.AsNoTracking().Where(e => (e.SystemType == systemType || e.SystemType == "S") && e.InUse == true);
        }

        public async Task<QEDataSource> GetQEDataSourceByNameAndSystemTypeAsync(string name, string systemType)
        {
            return await _repository.QEDataSources.AsNoTracking().Where(e => e.DataSourceName == name && e.SystemType == systemType && e.InUse == true).FirstOrDefaultAsync();
        }

        public async Task<List<QEDataSource>> GetQEDataSourceByScreenCodeAndSystemTypeAsync(string screenCode, string systemType) {
            return await _repository.QEDataSources.AsNoTracking().Where(e => e.SystemType == systemType && e.InUse == true && _repository.QEDataSourceScreens.Any(ds=> ds.DataSourceId==e.DataSourceID && ds.ScreenCode==screenCode)).ToListAsync();
        }

        public async Task<List<QEColumnDTO>> GetDataFields(int dataSourceId)
        {
            var dataSource = await _repository.QEDataSources.AsNoTracking()
                .FirstOrDefaultAsync(q => q.DataSourceID == dataSourceId);
            return await _qeDataSourceRepository.GetDataFields(dataSource.ViewName);
        }


        #endregion

        #region Role Source

        public async Task<QERoleSource> GetQERoleSourceByIdAsync(int id)
        {
            return await _repository.QERoleSources.FindAsync(id);
        }

        public async Task<QERoleSource> GetQERoleSourceByNameAsync(string name)
        {
            return await _repository.QERoleSources.Where(e => e.RoleName == name).FirstOrDefaultAsync();
        }

        public async Task<QERoleSource> AddRoleSourceAsync(QERoleSource roleSource)
        {
            if (!(await _repository.QERoleSources.AnyAsync(r => r.RoleName == roleSource.RoleName && r.SystemType == roleSource.SystemType && r.RoleType == roleSource.RoleType)))
            {
                _repository.QERoleSources.Add(roleSource);
                await _repository.SaveChangesAsync();                
            }
            return roleSource;
        }

        public async Task DeleteUnuseRoleSourcesAsync(List<int> roleSourceIds)
        {
            if (roleSourceIds.Count > 0)
            {
                foreach (var id in roleSourceIds)
                {
                    var inUse = await _repository.QERecipients.AsNoTracking().AnyAsync(r => r.RoleSourceID == id);
                    if (!inUse)
                    {
                        var roleSource = await GetQERoleSourceByIdAsync(id);
                        _repository.QERoleSources.Remove(roleSource);
                        await _repository.SaveChangesAsync();
                    }
                }                
            }
        }

        public async Task DeleteRoleSourcesAsync(List<QERoleSource> roleSources)
        {
            if (roleSources.Count > 0)
            {
                foreach (var roleSource in roleSources)
                {                    
                    var recipients = await _repository.QERecipients.AsNoTracking().Where(r => r.RoleSourceID == roleSource.RoleSourceID).ToListAsync();
                    if (recipients.Count > 0)
                    {
                        _repository.QERecipients.RemoveRange(recipients);                        
                        _repository.QERoleSources.Remove(roleSource);
                        await _repository.SaveChangesAsync();
                    }
                }
            }
        }

        #endregion

        #region CPI Language

        public async Task<CPiLanguage> GetCPiLanguageByLanguageAsync(string language)
        {
            return await _repository.CPiLanguages.Where(e => e.Language == language).FirstOrDefaultAsync();
        }

        public IQueryable<CPiLanguage> GetCPiLanguages()
        {
            return _repository.CPiLanguages;
        }

        public IQueryable<Language> GetLanguages()
        {
            return _repository.Languages;
        }

        #endregion

        #region Category and Tag
        public IQueryable<QETag> QETags => _repository.QETags.AsNoTracking();

        public async Task<bool> TagsUpdateAsync(int qeSetupId, string userName, byte[] tStamp,
            IEnumerable<QETag> updatedTags,
            IEnumerable<QETag> newTags, IEnumerable<QETag> deletedTags)
        {
            if (updatedTags.Any())
                _repository.QETags.UpdateRange(updatedTags);

            if (newTags.Any())
                _repository.QETags.AddRange(newTags);

            var qeMain = new QEMain() { QESetupID = qeSetupId, UpdatedBy = userName, LastUpdate = DateTime.Now, tStamp = tStamp };
            var entity = _repository.QEMains.Attach(qeMain);
            entity.Property(c => c.UpdatedBy).IsModified = true;
            entity.Property(c => c.LastUpdate).IsModified = true;

            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task DeleteTagAsync(int qeSetupId, string userName, byte[] tStamp, QETag Tag)
        {
            _repository.QETags.Remove(Tag);
            //var qeMain = new QEMain() { QESetupID = qeSetupId, UpdatedBy = userName, LastUpdate = DateTime.Now, tStamp = tStamp };
            //var entity = _repository.QEMains.Attach(qeMain);
            //entity.Property(c => c.UpdatedBy).IsModified = true;
            //entity.Property(c => c.LastUpdate).IsModified = true;
            await _repository.SaveChangesAsync();
        }
        #endregion

    }
}
