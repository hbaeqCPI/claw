using R10.Core.DTOs;
using R10.Core.Entities.FormExtract;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IFormIFWService
    {
        IQueryable<FormIFWFormType> FormIFWFormTypes { get; }
        IQueryable<FormIFWDocType> FormIFWDocTypes { get; }
        IQueryable<FormIFWDataExtract> FormIFWDataExtracts { get;}
        IQueryable<FormIFWActionMap> FormIFWActionMaps { get; }
        IQueryable<FormIFWActMap> FormIFWActMaps { get; }
        IQueryable<FormIFWActMapPat> FormIFWActMapsPat { get; }
        IQueryable<FormIFWActMapTmk> FormIFWActMapsTmk { get; }
        IQueryable<RTSMapActionDocument> RTSMapActionDocuments { get; }
        IQueryable<RTSMapActionDocumentClient> RTSMapActionDocumentClients { get; }
        IQueryable<TLMapActionDocument> TLMapActionDocuments { get; }
        IQueryable<TLMapActionDocumentClient> TLMapActionDocumentClients { get; }

        Task<bool> SaveExtractedData(int ifwId, int docTypeId, List<FormExtractDTO> formData, string userName, bool clearExisting = true);
        Task<bool> SaveTLExtractedData(int tlDocId, int docTypeId, List<FormExtractDTO> formData, string userName, bool clearExisting = true);

        void UpdateExtractedDataUsageId();

        Task<FormIFWActionDueDTO> GetIFWActionDue(int ifwId);
        Task<List<FormIFWActionRemarksDTO>> GetIFWActionRemarksData(int ifwId);
        Task<FormIFWActionUpdateDTO> GenIFWAction(int ifwId, string mapIds, string userName);           // OLD 
        Task<FormIFWActionUpdateDTO> GenIFWAct(int ifwId, string userName);                             // NEW
        Task<FormIFWActionUpdateDTO> GenIFWIDSRecords(int ifwId, string userName);
        Task<FormIFWActionUpdateDTO> GenTLIFWAct(int tlDocId, string userName);

        Task UpdateActMap(int mapHdrId, bool IsGenAction, bool IsCompare, string userName);
        Task UpdateRTSMapActionDocument(List<RTSMapActionDocument> updated, string userName);
        Task UpdateRTSMapDocumentClient(List<RTSMapActionDocumentClient> inserted, List<RTSMapActionDocumentClient> updated);
        Task DeleteRTSMapDocumentClient(RTSMapActionDocumentClient deleted);

        Task UpdateTLMapActionDocument(List<TLMapActionDocument> updated, string userName);
        Task UpdateTLMapDocumentClient(List<TLMapActionDocumentClient> inserted, List<TLMapActionDocumentClient> updated);
        Task DeleteTLMapDocumentClient(TLMapActionDocumentClient deleted);

        Task<FormIFWActMap> GetByIdAsync(int mapHdrId);
        Task<bool> UpdateAIInclude(int ifwId, bool aiInclude);
        Task<bool> UpdateActionMap(string userName, IEnumerable<FormIFWActionMap> newActionMaps, IEnumerable<FormIFWActionMap> updatedActionMaps, IEnumerable<FormIFWActionMap> deletedActionMaps);
        Task<List<FormPLMapDTO>> GetPLMapInfo();

        Task<List<FormIFWDocType>> GetDocumentsForAI();
        Task<bool> SaveExtractedDocData(int docId, int docTypeId, List<FormExtractDTO> formData, string userName, bool clearExisting = true);
        Task<FormIFWActionUpdateDTO> GenDocIDSRecords(int docId, int appId, string userName);
        Task<FormIFWActionUpdateDTO> GenDocAction(int docId, int appId, string userName);
        Task<FormIFWActionUpdateDTO> GenDocActionTmk(int docId, int tmkId, string userName);
    }

}
