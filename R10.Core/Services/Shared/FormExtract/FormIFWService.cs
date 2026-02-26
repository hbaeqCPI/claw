using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.FormExtract;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class FormIFWService : IFormIFWService
    {
        private readonly IApplicationDbContext _repository;
        private const int _dataExtractMaxLen = 50;

        public FormIFWService(IApplicationDbContext repository)
        {
            _repository = repository;
        }

        public IQueryable<FormIFWFormType> FormIFWFormTypes => _repository.FormIFWFormTypes.AsNoTracking();
        public IQueryable<FormIFWDocType> FormIFWDocTypes => _repository.FormIFWDocTypes.AsNoTracking();
        public IQueryable<FormIFWDataExtract> FormIFWDataExtracts => _repository.FormIFWDataExtracts.AsNoTracking();
        public IQueryable<FormIFWActionMap> FormIFWActionMaps => _repository.FormIFWActionMaps.AsNoTracking();
        public IQueryable<FormIFWActMap> FormIFWActMaps => _repository.FormIFWActMaps.AsNoTracking();
        public IQueryable<FormIFWActMapPat> FormIFWActMapsPat => _repository.FormIFWActMapsPat.AsNoTracking();
        public IQueryable<FormIFWActMapTmk> FormIFWActMapsTmk => _repository.FormIFWActMapsTmk.AsNoTracking();
        // RTS/TL MapActionDocument properties removed during deep clean

        #region Extracted Data Tab
        public async Task<bool> SaveExtractedData(int ifwId, int docTypeId, List<FormExtractDTO> formData, string userName, bool clearExisting = true)
        {
            // delete existing rows
            if (clearExisting)
            {
                var oldExtracts = await FormIFWDataExtracts.Where(e => e.IFWId == ifwId).ToListAsync();
                if (oldExtracts.Any())
                {
                    _repository.FormIFWDataExtracts.RemoveRange(oldExtracts);
                    await _repository.SaveChangesAsync();
                }
            }

            // insert new rows
            var extractedRows = new List<FormIFWDataExtract>();
            int seqNo = 0;
            var dateNow = DateTime.Now;

            formData.ForEach(r =>
            {
                seqNo++;
                var fieldData = r.FieldData;

                //if (fieldData != null && fieldData.Length > _dataExtractMaxLen)
                //    fieldData = fieldData.Substring(0, _dataExtractMaxLen);

                extractedRows.Add(new FormIFWDataExtract
                {
                    DocTypeId = docTypeId,
                    IFWId = ifwId,
                    SequenceNo = seqNo,
                    FieldName = r.FieldName,
                    FieldData = fieldData,
                    Confidence = r.Confidence,
                    CreatedBy = userName,
                    DateCreated = dateNow
                });
            });

            _repository.FormIFWDataExtracts.AddRange(extractedRows);

            // RTS IFW update removed during deep clean

            // save all
            await _repository.SaveChangesAsync();

            return true;
        }

        // SaveTLExtractedData removed during deep clean (TL module deleted)

        public void UpdateExtractedDataUsageId()
        {
            var sql = "Update x Set x.UsageId = u.UsageId From tblFRIFWExtract x Inner Join tblFRIFWFieldUsage u On x.DocTypeId = u.DocTypeId And x.FieldName = u.FieldName Where x.UsageId Is Null Or x.UsageId = 0";

            using (SqlCommand cmd = new SqlCommand(sql))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(_repository.Database.GetDbConnection().ConnectionString);
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region Action Tab (OLD)
        public async Task<FormIFWActionDueDTO> GetIFWActionDue(int ifwId)
        {
            var result = _repository.FormIFWActionDueDTO.FromSqlInterpolated($"procFR_GetIFWActionDue @ifwId={ifwId}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return result;
        }

        public async Task<FormIFWActionUpdateDTO> GenIFWAction(int ifwId, string mapIds, string userName)
        {
            var result = _repository.FormIFWActionUpdateDTO.FromSqlInterpolated($"procFR_GenIFWAction @ifwId={ifwId}, @mapIds={mapIds}, @createdBy={userName}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return result;
        }

        public async Task<bool> UpdateActionMap(string userName, IEnumerable<FormIFWActionMap> newActionMaps, IEnumerable<FormIFWActionMap> updatedActionMaps, IEnumerable<FormIFWActionMap> deletedActionMaps)
        {
            var dateNow = DateTime.Now;
            var dbSet = _repository.Set<FormIFWActionMap>();

            if (newActionMaps.Any())
            {
                foreach (var item in newActionMaps)
                {
                    item.UpdatedBy = userName;
                    item.LastUpdate = dateNow;
                }
                dbSet.AddRange(newActionMaps);
            }


            if (updatedActionMaps.Any())
            {
                foreach (var item in updatedActionMaps)
                {
                    item.UpdatedBy = userName;
                    item.LastUpdate = dateNow;
                }
                dbSet.UpdateRange(updatedActionMaps);
            }

            if (deletedActionMaps.Any())
                dbSet.RemoveRange(deletedActionMaps);

            await _repository.SaveChangesAsync();

            return true;

        }
        #endregion

        #region Action
        public async Task<FormIFWActionUpdateDTO> GenIFWAct(int ifwId, string userName)
        {
            var result = _repository.FormIFWActionUpdateDTO.FromSqlInterpolated($"procFR_GenIFWAct @ifwId={ifwId}, @createdBy={userName}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return result;
        }
        // GenTLIFWAct removed during deep clean (TL module deleted)
        public async Task<FormIFWActionUpdateDTO> GenIFWIDSRecords(int ifwId, string userName)
        {
            var result = _repository.FormIFWActionUpdateDTO.FromSqlInterpolated($"procFR_GenIFW_IDS @ifwId={ifwId}, @createdBy={userName}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return result;
        }
        #endregion

        #region Action Map (New)
        public async Task UpdateActMap(int mapHdrId, bool isGenAction, bool isCompare, string userName)
        {
            var actMap = await _repository.FormIFWActMaps.SingleOrDefaultAsync(m => m.MapHdrId == mapHdrId);
            actMap.UpdatedBy = userName;
            actMap.LastUpdate = DateTime.Now;
            actMap.IsCompare = isCompare;
            actMap.IsGenAction = isGenAction;
            await _repository.SaveChangesAsync();
        }

        // RTS/TL Map methods removed during deep clean


        public async Task<FormIFWActMap> GetByIdAsync(int mapHdrId)
        {
            return await FormIFWActMaps.SingleOrDefaultAsync(m => m.MapHdrId == mapHdrId);
        }

        #endregion

        // UpdateAIInclude removed during deep clean (RTS module deleted)

        public async Task<List<FormIFWActionRemarksDTO>> GetIFWActionRemarksData(int ifwId)
        {
            var result = await _repository.FormIFWActionRemarksDTO.FromSqlInterpolated($"procFR_GetIFWActionRemarksData @ifwId={ifwId}").AsNoTracking().ToListAsync();

            return result;
        }

        public async Task<List<FormPLMapDTO>> GetPLMapInfo()
        {
            string sql = "Select src.MapSourceId, doc.DocumentDescription, frFormType.FormType, frFormType.FormName " +
                            "From dbo.tblPLMapActionDueSource AS src Inner Join dbo.tblPLMapActionDocument AS doc ON src.MapCountry = doc.Country AND src.MapSearchAction = doc.SearchAction " +
                                "Inner Join dbo.tblFRIFWDocType AS frDocType ON doc.DocumentDescription = frDocType.DocDesc Inner Join dbo.tblFRIFWFormType AS frFormType ON frDocType.FormTypeId = frFormType.FormTypeId Where (frDocType.IsEnabled = 1);";

            var result = await _repository.FormPLMapDTO.FromSqlRaw(sql).AsNoTracking().ToListAsync();
            result.ForEach(r => r.FormExtractLink = $"P|IFW|{r.FormType}|{r.FormName}|{r.DocumentDescription}");     // SystemType|SourceCode|FormType|DocDesc

            return result;
        }

        #region Uploaded Doc AI processing
        public async Task<List<FormIFWDocType>> GetDocumentsForAI()
        {
            return await _repository.FormIFWDocTypes.Where(d => d.IsEnabled).Include(d => d.FormIFWActMaps).ToListAsync();
        }

        public async Task<bool> SaveExtractedDocData(int docId, int docTypeId, List<FormExtractDTO> formData, string userName, bool clearExisting = true)
        {
            // delete existing rows
            if (clearExisting)
            {
                var oldExtracts = await FormIFWDataExtracts.Where(e => e.DocId == docId).ToListAsync();
                if (oldExtracts.Any())
                {
                    _repository.FormIFWDataExtracts.RemoveRange(oldExtracts);
                    await _repository.SaveChangesAsync();
                }
            }

            // insert new rows
            var extractedRows = new List<FormIFWDataExtract>();
            int seqNo = 0;
            var dateNow = DateTime.Now;

            formData.ForEach(r => {
                seqNo++;
                var fieldData = r.FieldData;
                extractedRows.Add(new FormIFWDataExtract
                {
                    DocTypeId = docTypeId,
                    DocId = docId,
                    SequenceNo = seqNo,
                    FieldName = r.FieldName,
                    FieldData = fieldData,
                    Confidence = r.Confidence,
                    CreatedBy = userName,
                    DateCreated = dateNow
                });
            });

            _repository.FormIFWDataExtracts.AddRange(extractedRows);

            // save all
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<FormIFWActionUpdateDTO> GenDocIDSRecords(int docId, int appId, string userName)
        {
            var result = _repository.FormIFWActionUpdateDTO.FromSqlInterpolated($"procFR_GenDoc_IDS @DocId={docId}, @AppId={appId}, @createdBy={userName}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return result;
        }

        public async Task<FormIFWActionUpdateDTO> GenDocAction(int docId, int appId, string userName)
        {
            var result = _repository.FormIFWActionUpdateDTO.FromSqlInterpolated($"procFR_GenDocAction @DocId={docId}, @AppId={appId}, @createdBy={userName}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return result;
        }

        public async Task<FormIFWActionUpdateDTO> GenDocActionTmk(int docId, int tmkId, string userName) {
            var result = _repository.FormIFWActionUpdateDTO.FromSqlInterpolated($"procFR_GenDocActionTmk @DocId={docId}, @TmkId={tmkId}, @createdBy={userName}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return result;
        }
        #endregion  
    }
}
