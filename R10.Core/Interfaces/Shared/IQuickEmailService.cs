using R10.Core.DTOs;
using R10.Core.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IQuickEmailService
    {
        IQueryable<QEMain> GetQeMainByScreenId(int screenId);
        Task<QEDataSource> GetQeDataSourceByIdAsync(int id);
        Task<QELog> AddLogAsync(QELog log);
        //Task<FileHandler> AddFileHandlerAsync(FileHandler fileHandler);
        Task<string> GetDefaultThumbnailAsync(string thumbnailPath);
        IQueryable<QELog> QELogs { get; }
        IQueryable<SystemScreen> SystemScreens { get; }
        IQueryable<QEDataSource> QEDataSources { get; }
        IQueryable<QEDataSource> QEDataSourcesFiltered { get; }
        IQueryable<QECustomField> QECustomFields { get; }
        Task<bool> QECustomFieldUpdate(int dataSourceId, string userName, string userEmail, IEnumerable<QECustomField> updatedData, IEnumerable<QECustomField> newData, IEnumerable<QECustomField> deletedData);
        Task<List<QEFieldListDTO>> GetDataSourceFieldList(int dataSourceID, string sortColumn, string sortDirection);
        Task<List<QEFieldListDTO>> GetDataSourceFieldList(int dataSourceID);
        Task<List<QEFieldListDTO>> GetDataSourceDateFieldList(int dataSourceID);
    }
}
