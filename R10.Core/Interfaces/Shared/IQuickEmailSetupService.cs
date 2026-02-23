using R10.Core.DTOs;
using R10.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IQuickEmailSetupService
    {
        Task<ModuleMain> GetModuleByIdAsync(int id);
        IQueryable<ModuleMain> GetModulesBySystemTypeAsync(string systemType);

        Task<ModuleMain> GetModuleByNameAndSystemTypeAsync(string name, string systemType);

        IQueryable<SystemScreen> GetSystemScreensBySystemTypeAsync(string systemType);
        IQueryable<SystemScreen> SystemScreens { get; }

        IQueryable<QEMain> GetQEMains();
        Task<QEMain> GetQEMainByIdAsync(int id);
        IQueryable<QEMain> GetQEMainBySystemType(string systemType);
        IQueryable<QEMain> GetQEMainByScreenId(int screenId);

        Task<QEMain> AddQEMainAsync(QEMain quickEmail);
        Task UpdateQEMainAsync(QEMain quickEmail);
        Task DeleteQEMainAsync(QEMain quickEmail);

        Task<QERecipient> GetRecipientByIdAsync(int id);
        Task<List<QERecipient>> GetRecipientsByParentIdAsync(int id);
        Task<int> GetRecipientCount(int qeSetupId);

        Task<QERecipient> AddRecipientAsync(QERecipient recipient);
        Task UpdateRecipientAsync(QERecipient recipient);
        Task DeleteRecipientAsync(QERecipient recipient);
        Task<bool> RecipientsUpdateAsync(int qeSetupId, string userName, byte[] tStamp, IList<QERecipient> updatedRecipients,
            IList<QERecipient> newRecipients, IList<QERecipient> deletedRecipients);

        Task<QEDataSource> GetQEDataSourceByIdAsync(int id);
        IQueryable<QEDataSource> GetQEDataSourcesBySystemTypeAsync(string systemType);
        Task<QEDataSource> GetQEDataSourceByNameAndSystemTypeAsync(string name, string systemType);
        Task<List<QEDataSource>> GetQEDataSourceByScreenCodeAndSystemTypeAsync(string screenCode, string systemType);

        Task<QERoleSource> GetQERoleSourceByIdAsync(int id);
        Task<QERoleSource> GetQERoleSourceByNameAsync(string name);
        Task<QERoleSource> AddRoleSourceAsync(QERoleSource roleSource);
        Task DeleteUnuseRoleSourcesAsync(List<int> roleSourceIds);
        Task DeleteRoleSourcesAsync(List<QERoleSource> roleSources);

        Task<CPiLanguage> GetCPiLanguageByLanguageAsync(string language);
        IQueryable<CPiLanguage> GetCPiLanguages();
        IQueryable<Language> GetLanguages();

        Task<List<QEColumnDTO>> GetDataFields(int dataSourceId);

        IQueryable<QETag> QETags { get; }
        Task<bool> TagsUpdateAsync(int qeSetupId, string userName, byte[] tStamp,
            IEnumerable<QETag> updatedTags,
            IEnumerable<QETag> newTags, IEnumerable<QETag> deletedTags);
        Task DeleteTagAsync(int qeSetupId, string userName, byte[] tStamp, QETag Tag);

    }
}
