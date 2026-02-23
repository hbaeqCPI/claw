using R10.Core.DTOs;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface ILetterService
    {
        #region Letter Main CRUD
        IQueryable<LetterMain> LettersMain { get; }
        IQueryable<LetterMain> FilteredLettersMain { get; }

        Task<LetterMain> GetLetterMainById(int letId);

        Task Add(LetterMain letter);

        Task Update(LetterMain letter);

        Task Delete(LetterMain letter);

        IQueryable<LetterTag> LetterTags { get; }

        #endregion

        #region Field List
        Task<List<LetterFieldListDTO>> GetFieldList(int letId, string sortColumn, string sortDirection);
        #endregion

        #region User Data
        IQueryable<LetterUserData> LetterUserData { get; }
        Task<bool> UserDataUpdate(int letId, string userName, IEnumerable<LetterUserData> updatedUserData, IEnumerable<LetterUserData> newUserData, IEnumerable<LetterUserData> deletedUserData);
        #endregion

        #region Data/Record Source
        IQueryable<LetterRecordSource> LetterRecordSources { get; }
        IQueryable<LetterDataSource> LetterDataSources { get; }
        IQueryable<LetterDataSource> FilteredLetterDataSources { get; }
        Task<LetterRecordSource> GetRecordSourceById(int recSourceId);
        Task<LetterRecordSource> GetRecordSourceById(int recSourceId, int letId);
        Task<bool> ValidParentRecord(int parentRecSourceId, int letId);

        Task<List<FamilyTreeDTO>> GetFamilyTree(int letId, int? parentId);
        Task<bool> RecordSourceUpdate(int letId, string userName, IEnumerable<LetterRecordSource> updatedRecordSource, IEnumerable<LetterRecordSource> newRecordSource, IEnumerable<LetterRecordSource> deletedRecordSource);

        #endregion

        #region Letter Filter
        IQueryable<LetterRecordSourceFilter> LetterRecordSourceFilters { get; }
        IQueryable<LetterRecordSourceFilterUser> LetterRecordSourceFiltersUser { get; }

        Task<bool> LetterRecordSourceFilterUpdate(string userName, IEnumerable<LetterRecordSourceFilter> updatedFilterData, IEnumerable<LetterRecordSourceFilter> newFilterData, IEnumerable<LetterRecordSourceFilter> deletedFilterData);
        Task<bool> LetterRecordSourceFilterUserUpdate(int letId, string userName, string userEmail, 
                        IEnumerable<LetterRecordSourceFilterUser> updatedFilterData, IEnumerable<LetterRecordSourceFilterUser> newFilterData, IEnumerable<LetterRecordSourceFilterUser> deletedFilterData);

        Task<List<LookupDTO>> GetFilterFieldsList(int recSourceId);
        Task<LetterFilterDataDTO> GetFilterDataList(int recSourceId, string fieldName, int pageNo, int pageSize, string filterData);
        Task<LookupDTO> FilterDataValueMapper(int recSourceId, string fieldName, string value);

        #endregion

        #region Main Screen Letter Popup
        Task<SystemScreen> GetSystemScreen(int id);
        Task<SystemScreen> GetSystemScreen(string systemType, string screenCode);
        Task<List<LetterContactDTO>> GetLetterContacts(int letId, string userEmail);
        bool UpdatePopupFilter(int letId, string fieldName, string operand, string userEmail, string userName);
        DataSet GenerateLetterData(int letId, bool includeGenerated, bool isLog, string returnType, IEnumerable<LetterEntityContactDTO> selectedContacts,
                                        string userEmail, bool hasRespOffice, bool hasEntityFilter, string? previewSelection);
        long GetSessionId();
        int LogLetter(long sessionId, string systemType, int letId, string letFile, string userName);
        Task LogItemId(int letLogId, string itemId);
        Task<List<LookupIntDTO>> GetDataKeyValuesToLog(long sessionId);

        #endregion

        #region Setup Preview Screen
        DataTable PreviewLetterData(int letId, bool includeGenerated, string userEmail, bool hasRespOffice, bool hasEntityFilter,
                                        string sortExpr, int page, int pageSize = 0);
        int PreviewLetterCount();


        #endregion

        #region LetterLog
        IQueryable<LetterLog> LetterLogs { get; }
        IQueryable<LetterLogDetail> LetterLogDetails { get; }
        #endregion 

        #region Custom Fields
        IQueryable<LetterCustomField> LetterCustomFields { get; }
        Task<LetterDataSource> GetDataSourceById(int dataSourceId);
        Task<bool> LetterCustomFieldUpdate(int dataSourceId, string userName, string userEmail,
            IEnumerable<LetterCustomField> updatedData, IEnumerable<LetterCustomField> newData, IEnumerable<LetterCustomField> deletedData);
        Task<List<LetterFieldListDTO>> GetDataSourceFieldList(int dataSourceId, string sortColumn, string sortDirection);

        Task<List<LetterFieldListDTO>> GetDataSourceFieldList(int dataSourceId);
        Task<List<LetterFieldListDTO>> GetDataSourceDateFieldList(int dataSourceId);
        #endregion
    }
}
