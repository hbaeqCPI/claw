using R10.Core.DTOs;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IDOCXService
    {
        #region DOCX Main CRUD
        IQueryable<DOCXMain> DOCXesMain { get; }

        Task<DOCXMain> GetDOCXMainById(int docxId);

        Task Add(DOCXMain docx);

        Task Update(DOCXMain docx);

        Task Delete(DOCXMain docx);

        #endregion

        #region Field List
        Task<List<DOCXFieldListDTO>> GetFieldList(int docxId, string sortColumn, string sortDirection);
        #endregion

        #region User Data
        IQueryable<DOCXUserData> DOCXUserData { get; }
        Task<bool> UserDataUpdate(int docxId, string userName, IEnumerable<DOCXUserData> updatedUserData, IEnumerable<DOCXUserData> newUserData, IEnumerable<DOCXUserData> deletedUserData);
        #endregion

        #region Data/Record Source
        IQueryable<DOCXRecordSource> DOCXRecordSources { get; }
        IQueryable<DOCXDataSource> DOCXDataSources { get; }
        Task<DOCXRecordSource> GetRecordSourceById(int recSourceId);
        Task<DOCXRecordSource> GetRecordSourceById(int dataSourceId, int docxId);
        Task<bool> ValidParentRecord(int parentRecSourceId, int docxId);
        Task<List<FamilyTreeDTO>> GetFamilyTree(int docxId, int? parentId);
        Task<bool> RecordSourceUpdate(int docxId, string userName, IEnumerable<DOCXRecordSource> updatedRecordSource, IEnumerable<DOCXRecordSource> newRecordSource, IEnumerable<DOCXRecordSource> deletedRecordSource);

        #endregion

        #region DOCX Filter
        IQueryable<DOCXRecordSourceFilter> DOCXRecordSourceFilters { get; }
        IQueryable<DOCXRecordSourceFilterUser> DOCXRecordSourceFiltersUser { get; }

        Task<bool> DOCXRecordSourceFilterUpdate(string userName, IEnumerable<DOCXRecordSourceFilter> updatedFilterData, IEnumerable<DOCXRecordSourceFilter> newFilterData, IEnumerable<DOCXRecordSourceFilter> deletedFilterData);
        Task<bool> DOCXRecordSourceFilterUserUpdate(int docxId, string userName, string userEmail, 
                        IEnumerable<DOCXRecordSourceFilterUser> updatedFilterData, IEnumerable<DOCXRecordSourceFilterUser> newFilterData, IEnumerable<DOCXRecordSourceFilterUser> deletedFilterData);

        Task<List<LookupDTO>> GetFilterFieldsList(int recSourceId);
        Task<DOCXFilterDataDTO> GetFilterDataList(int recSourceId, string fieldName, int pageNo, int pageSize, string filterData);
        Task<LookupDTO> FilterDataValueMapper(int recSourceId, string fieldName, string value);

        #endregion

        #region Main Screen DOCX Popup
        Task<SystemScreen> GetSystemScreen(int id);
        Task<SystemScreen> GetSystemScreen(string systemType, string screenCode);
        //Task<List<DOCXContactDTO>> GetDOCXContacts(int docxId, string userEmail);
        bool UpdatePopupFilter(int docxId, string fieldName, string operand, string userEmail, string userName);
        DataSet GenerateDOCXData(int docxId, bool includeGenerated, bool isLog, string returnType,// IEnumerable<DOCXEntityContactDTO> selectedContacts,
                                        string userEmail, bool hasRespOffice, bool hasEntityFilter, int id);
        long GetSessionId();
        int LogDOCX(long sessionId, string systemType, int docxId, string docxFile, string userName, string? itemId, string? signatory);

        #endregion

        #region Setup Preview Screen
        DataTable PreviewDOCXData(int docxId, bool includeGenerated, string userEmail, bool hasRespOffice, bool hasEntityFilter,
                                        string sortExpr, int page, int pageSize = 0);
        int PreviewDOCXCount();


        #endregion

        #region DOCXLog
        IQueryable<DOCXLog> DOCXLogs { get; }
        //IQueryable<DOCXLogDetail> DOCXLogDetails { get; }
        #endregion

        #region USPTO
        IQueryable<DOCXUSPTOHeader> DOCXUSPTOHeaders { get; }
        IQueryable<DOCXUSPTOHeaderKeyword> DOCXUSPTOHeaderKeywords { get; }
        Task<List<DOCXUSPTOHeaderKeywordDTO>> GetUSPTOHeaderKeywordList();
        Task<List<DOCXUSPTOHeaderKeywordExcelDTO>> GetUSPTOHeaderKeywordExcelList();
        #endregion
    }
}
