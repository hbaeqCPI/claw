using Microsoft.EntityFrameworkCore;
using R10.Core.Interfaces;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using System;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net.Sockets;
using Microsoft.Data.SqlClient.Server;
using R10.Core.Services;

namespace R10.Infrastructure.Data
{

    public class RTSInfoRepository:IRTSInfoRepository 
    {
        private readonly ApplicationDbContext _dbContext;
        public RTSInfoRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<RTSInfoSettingsMenu>> GetMenu(string country)
        {
            var menu = await _dbContext.RTSInfoCountrySettings.Where(m => m.Country == country || m.Country == "ALL")
                .OrderBy(m => m.Sequence).Select(m => m.InfoSettingsMenu).ToListAsync();
            return menu;
        }

        public RTSSearchBiblioDTO GetBiblio(int plAppId)
        {
            var biblio =  _dbContext.RTSSearchBiblioDTO.FromSqlInterpolated($"procPLPTO_CaseInfo @PLAppId={plAppId}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return biblio;
        }

        public async Task<List<RTSSearchInventorDTO>> GetInventors(int plAppId)
        {
            var inventors = await _dbContext.RTSSearchInventorDTO.FromSqlInterpolated($"procPLPTO_Inventor @PLAppId={plAppId}")
                .AsNoTracking().ToListAsync();
            return inventors;
        }

        public async Task<List<RTSSearchApplicantDTO>> GetApplicants(int plAppId)
        {
            var applicants = await _dbContext.RTSSearchApplicantDTO.FromSqlInterpolated($"procPLPTO_Applicant @PLAppId={plAppId}")
                .AsNoTracking().ToListAsync();
            return applicants;
        }

        public async Task<List<RTSSearchIPClassDTO>> GetIPClasses(int plAppId)
        {
            var ipClasses = await _dbContext.RTSSearchIPClassDTO.FromSqlInterpolated($"procPLPTO_IPClass @PLAppId={plAppId}")
                .AsNoTracking().ToListAsync();
            return ipClasses;
        }

        public async Task<List<RTSSearchTitleDTO>> GetTitles(int plAppId)
        {
            var titles = await _dbContext.RTSSearchTitleDTO.FromSqlInterpolated($"procPLPTO_Title @PLAppId={plAppId}")
                .AsNoTracking().ToListAsync();
            return titles;
        }
        
        public RTSSearchBiblioUSDTO GetBiblioUS(int plAppId)
        {
            var biblioUS = _dbContext.RTSSearchBiblioUSDTO.FromSqlInterpolated($"procPLPTO_USBiblio @PLAppId={plAppId}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return biblioUS;
        }

        public async Task<List<RTSSearchBiblioUSDTO>> GetBiblioUSs(int plAppId)
        {
            var biblioUSs = await _dbContext.RTSSearchBiblioUSDTO.FromSqlInterpolated($"procPLPTO_USBiblio @PLAppId={plAppId}").AsNoTracking().ToListAsync();
            return biblioUSs;
        }

        public async Task<List<RTSSearchAssignmentDTO>> GetAssignments(int plAppId)
        {
            var assignments = await _dbContext.RTSSearchAssignmentDTO.FromSqlInterpolated($"procPLPTO_Assignment @PLAppId={plAppId}").AsNoTracking().ToListAsync();
            return assignments;
        }

        public async Task<List<RTSSearchPriorityDTO>> GetPriorities(int plAppId)
        {
            var priorities = await _dbContext.RTSSearchPriorityDTO.FromSqlInterpolated($"procPLPTO_Priority @PLAppId={plAppId}").AsNoTracking().ToListAsync();
            return priorities;
        }

        

        public RTSSearchAbstractDTO GetAbstractClaims(int plAppId)
        {
            var abstractClaims =  _dbContext.RTSSearchAbstractDTO.FromSqlInterpolated($"procPLPTO_AbstractClaims @PLAppId={plAppId}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return abstractClaims;
        }

        public async Task<int> GetClaimsCount(int appId)
        {
            var noOfClaims = 0;
            var appIdParam = new SqlParameter("AppId", appId);
            var noOfClaimsParam = new SqlParameter("NoOfClaims", SqlDbType.Int);
            noOfClaimsParam.Direction = ParameterDirection.Output;
            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync("EXEC procPLPTO_ClaimsCount @AppId, @NoOfClaims output", appIdParam, noOfClaimsParam);
                noOfClaims = Convert.ToInt32(noOfClaimsParam.Value);
                return noOfClaims;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<List<RTSSearchDocCitedDTO>> GetDocsCited(int plAppId)
        {
            var docs = await _dbContext.RTSSearchDocCitedDTO.FromSqlInterpolated($"procPLPTO_DocCited @PLAppId={plAppId}").AsNoTracking().ToListAsync();
            return docs;
        }

        public async Task<List<RTSSearchDocRefByDTO>> GetDocsRefBy(int plAppId)
        {
            var docs = await _dbContext.RTSSearchDocRefByDTO.FromSqlInterpolated($"procPLPTO_DocRefBy @PLAppId={plAppId}").AsNoTracking().ToListAsync();
            return docs;
        }

        
        public async Task<List<RTSSearchPTADTO>> GetPTAs(int plAppId)
        {
            var pta = await _dbContext.RTSSearchPTADTO.FromSqlInterpolated($"procPLPTO_PTA @PLAppId={plAppId}").AsNoTracking().ToListAsync();
            return pta;
        }

        public async Task<List<RTSSearchContinuityParentDTO>> GetContinuitiesParent(int plAppId)
        {
            var continuity = await _dbContext.RTSSearchContinuityParentDTO.FromSqlInterpolated($"procPLPTO_USContinuityParent @PLAppId={plAppId}").AsNoTracking().ToListAsync();
            return continuity;
        }

        public async Task<List<RTSSearchContinuityChildDTO>> GetContinuitiesChild(int plAppId)
        {
            var continuity = await _dbContext.RTSSearchContinuityChildDTO.FromSqlInterpolated($"procPLPTO_USContinuityChild @PLAppId={plAppId}").AsNoTracking().ToListAsync();
            return continuity;
        }
        
        public async Task<List<RTSSearchIFWDTO>> GetIFWs(int plAppId)
        {
            var ifws = await _dbContext.RTSSearchIFWDTO.FromSqlInterpolated($"procPLPTO_USIFW @PLAppId={plAppId}").AsNoTracking().ToListAsync();
            return ifws;
        }

        //Use for Dashboard widget criteria
        public async Task<List<RTSSearchIFWDTO>> GetSearchPTODocuments()
        {
            var recipient = await _dbContext.RTSSearchIFWDTO.FromSqlRaw($"Select Distinct 0 AS PLAppId, 0 AS OrderOfEntry, Description, NULL AS MailRoomDate, NULL AS Filename, 0 AS NoPages, 0 as PageStart From tblPLSearchUSIFW")
                .Where(p => p.Description != null).AsNoTracking().ToListAsync();
            return recipient;
        }       

        public RTSSearchUSCorrespondenceDTO GetCorrespondence(int plAppId)
        {
            var correspondence = _dbContext.RTSSearchUSCorrespondenceDTO.FromSqlInterpolated($"procPLPTO_Correspondence @PLAppId={plAppId}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return correspondence;
        }

        public async Task<List<RTSSearchUSCorrespondenceDTO>> GetCorrespondences(int plAppId)
        {
            var correspondences = await _dbContext.RTSSearchUSCorrespondenceDTO.FromSqlInterpolated($"procPLPTO_Correspondence @PLAppId={plAppId}").AsNoTracking().ToListAsync();
            return correspondences;
        }

        public async Task<List<RTSSearchAgentDTO>> GetAgents(int plAppId)
        {
            var agents = await _dbContext.RTSSearchAgentDTO.FromSqlInterpolated($"procPLPTO_Agent @PLAppId={plAppId}").AsNoTracking().ToListAsync();
            return agents;
        }

        public async Task<List<RTSSearchDesCountryDTO>> GetDesCountries(int plAppId)
        {
            var agents = await _dbContext.RTSSearchDesCountryDTO.FromSqlInterpolated($"procPLPTO_DesignatedCountry @PLAppId={plAppId}").AsNoTracking().ToListAsync();
            return agents;
        }

        public async Task<List<RTSSearchPFSDocDTO>> GetPFSDocs(int appId)
        {
            var docs = await _dbContext.RTSSearchPFSDocDTO.FromSqlInterpolated($"procPDTPFS_Doc @AppId={appId}").AsNoTracking().ToListAsync();
            return docs;
        }
        public async Task<List<RTSSearchLSDDTO>> GetLSDs(int appId)
        {
            var docs = await _dbContext.RTSSearchLSDDTO.FromSqlInterpolated($"procPDTLSDData @AppId={appId}").AsNoTracking().ToListAsync();
            return docs;
        }

        public async Task<List<RTSSearchIPCDTO>> GetIPCs(int appId)
        {
            var docs = await _dbContext.RTSSearchIPCDTO.FromSqlInterpolated($"procPDTIPCData @AppId={appId}").AsNoTracking().ToListAsync();
            return docs;
        }

        public async Task<List<RTSSearchCPCDTO>> GetCPCs(int appId)
        {
            var docs = await _dbContext.RTSSearchCPCDTO.FromSqlInterpolated($"procPDTCPCData @AppId={appId}").AsNoTracking().ToListAsync();
            return docs;
        }

        public async Task<List<RTSPFSTitleUpdHistoryDTO>> GetPFSTitleUpdHistory(int appId)
        {
            var updates = await _dbContext.RTSPFSTitleUpdHistoryDTO.FromSqlInterpolated($"procPDTPFS_TitleUpdHist @AppId={appId}").AsNoTracking().ToListAsync();
            return updates;
        }

        public async Task<List<RTSPFSAbstractUpdHistoryDTO>> GetPFSAbstractUpdHistory(int appId)
        {
            var updates = await _dbContext.RTSPFSAbstractUpdHistoryDTO.FromSqlInterpolated($"procPDTPFS_AbstractUpdHist @AppId={appId}").AsNoTracking().ToListAsync();
            return updates;
        }

        public RTSPFSCountryAppUpdHistoryDTO GetPFSCtryAppUpdHistory(int appId)
        {
            var updates = _dbContext.RTSPFSCountryAppUpdHistoryDTO.FromSqlInterpolated($"procPDTPFS_CtryAppUpdHist @AppId={appId}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return updates;
        }

        public async Task<List<RTSSearchActionUpdHistoryDTO>> GetActionUpdHistory(int plAppId, int revertType, int jobId)
        {
            var updates = await _dbContext.RTSSearchActionUpdHistoryDTO
                .FromSqlInterpolated($"procPLPTO_UpdLogActionSearch @PLAppId={plAppId},@RevertType={revertType},@JobId={jobId},@ReturnType=0").AsNoTracking().ToListAsync();
            return updates;
        }

        public async Task<List<UpdateHistoryBatchDTO>> GetActionUpdHistoryBatches(int plAppId, int revertType, int jobId)
        {
            var updates = await _dbContext.RTSSearchActionUpdHistoryBatchDTO
                .FromSqlInterpolated($"procPLPTO_UpdLogActionSearch @PLAppId={plAppId},@RevertType={revertType},@JobId={jobId},@ReturnType=1").AsNoTracking().ToListAsync();
            return updates;
        }

        public async Task<List<RTSSearchActionClosedUpdHistoryDTO>> GetActionClosedUpdHistory(int plAppId, int jobId)
        {
            var updates = await _dbContext.RTSSearchActionClosedUpdHistoryDTO
                .FromSqlInterpolated($"procPLPTO_UpdLogActionClosedSearch @PLAppId={plAppId},@JobId={jobId}").AsNoTracking().ToListAsync();
            return updates;
        }

        public async Task<List<UpdateHistoryBatchDTO>> GetActionClosedUpdHistoryBatches(int plAppId, int jobId)
        {
            var updates = await _dbContext.RTSSearchActionUpdHistoryBatchDTO
                .FromSqlInterpolated($"procPLPTO_UpdLogActionClosedSearch @PLAppId={plAppId},@JobId={jobId},@ReturnType=1").AsNoTracking().ToListAsync();
            return updates;
        }

        public async Task<List<RTSSearchActionAsDownloadedDTO>> GetActions(int plAppId, bool asDownloaded)
        {
            var actions = await _dbContext.RTSSearchActionAsDownloadedDTO.FromSqlInterpolated($"procPLPTO_Actions @PLAppId={plAppId},@Action={(asDownloaded ? 1:2)}").AsNoTracking().ToListAsync();
            return actions;
        }

        #region Workflow
        public async Task<List<RTSPFSWorkflowBatch>> GetPFSUpdatesForWorkflow()
        {
            var updates = await _dbContext.RTSPFSWorkflowBatches.FromSqlRaw($"Select BatchId From tblPDTSentlog l Where l.ReportType='U' and l.GenerateWorkflow=1 and l.SentDate is not null and l.WorkflowGenerated <> 1").AsNoTracking().ToListAsync();
            return updates;
        }

        public async Task<List<RTSPFSWorkflowApp>> GetPFSUpdatesToPublishedForWorkflow(string batchId)
        {
            var updates = await _dbContext.RTSPFSWorkflowApps.FromSqlRaw($"Select u.AppId,u.InvId,i.ClientId,u.BatchId,u.PubDate,u.IssDate From tblPFSIfdUpdCtryAppUpdateHist u Inner Join tblPatInvention i on i.InvId=u.InvId Inner Join tblPatCountryApplication ca on ca.AppId=u.AppId  Where ca.ApplicationStatus='Published' and (u.MarkPubNo='*' or u.MarkPubDate='*') And (WorkflowGenerated is null or WorkflowGenerated=0) And BatchId='{batchId}'").AsNoTracking().ToListAsync();
            return updates;
        }

        public async Task<List<RTSPFSWorkflowApp>> GetPFSUpdatesToGrantedForWorkflow(string batchId)
        {
            var updates = await _dbContext.RTSPFSWorkflowApps.FromSqlRaw($"Select u.AppId,u.InvId,i.ClientId,u.BatchId,u.PubDate,u.IssDate From tblPFSIfdUpdCtryAppUpdateHist u Inner Join tblPatInvention i on i.InvId=u.InvId Inner Join tblPatCountryApplication ca on ca.AppId=u.AppId  Where ca.ApplicationStatus='Granted' and (u.MarkPatNo='*' or u.MarkIssDate='*') And (WorkflowGenerated is null or WorkflowGenerated=0)  And BatchId='{batchId}'").AsNoTracking().ToListAsync();
            return updates;
        }

        public void MarkPFSUpdatesWorkflowBatchAsGenerated(string batchId)
        {
            var sql = "Update tblPDTSentlog Set WorkflowGenerated=1 Where ReportType='U' and BatchId=@BatchId";
            var param = new SqlParameter("@BatchId", batchId);
            _dbContext.Database.ExecuteSqlRaw(sql, param);
        }

        public void MarkPFSUpdatesWorkflowAsGenerated(string batchId, int appId) {
            var sql = "Update tblPFSIfdUpdCtryAppUpdateHist Set WorkflowGenerated=1 Where BatchId=@BatchId and AppId=@AppId";
            var batchParam = new SqlParameter("@BatchId", batchId);
            var appIdParam = new SqlParameter("@AppId", appId);
            _dbContext.Database.ExecuteSqlRaw(sql, batchParam, appIdParam);
        }
        public void MarkRTSAutoDocketActionWorkflowAsGenerated(int actId) {
            var sql = "Update tblPatActionDue Set AutoDocketWorkflowStatus=Case When AutoDocketWorkflowStatus=1 then 2 When AutoDocketWorkflowStatus=3 then 4 else AutoDocketWorkflowStatus  end Where ActId=@ActId";
            var actIdParam = new SqlParameter("@ActId", actId);
            _dbContext.Database.ExecuteSqlRaw(sql, actIdParam);
        }

        #endregion

        public void MarkIFWAsTransferred(string fileName)
        {
            var sql = "Update tblPLSearchUSIFW Set Transferred=1 Where FileName=@FileName";
            var param = new SqlParameter("@FileName", fileName);
            _dbContext.Database.ExecuteSqlRaw(sql, param);
        }

        public List<RTSPFSStatisticsSearchOutput> StatisticsSearchInpadoc(RTSPFSStatisticsSearchInput criteria)
        {
            using (SqlCommand cmd = new SqlCommand("procPdtStatReports"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@ReportType", criteria.ReportType);
                cmd.Parameters.AddWithValue("@YearFrom", criteria.YearFrom);
                cmd.Parameters.AddWithValue("@YearTo", criteria.YearTo);

                if (!string.IsNullOrEmpty(criteria.Countries))
                {
                    cmd.Parameters.AddWithValue("@CountryOp", criteria.CountryOp);
                    cmd.Parameters.AddWithValue("@Countries", criteria.Countries);
                }

                if (!string.IsNullOrEmpty(criteria.IPCs))
                {
                    cmd.Parameters.AddWithValue("@IPCs", criteria.IPCs);
                }

                if (!string.IsNullOrEmpty(criteria.CPCs))
                {
                    cmd.Parameters.AddWithValue("@CPCs", criteria.CPCs);
                }

                if (!string.IsNullOrEmpty(criteria.YourApplicants))
                {
                    cmd.Parameters.AddWithValue("@Applicants", criteria.YourApplicants);
                }

                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    List<RTSPFSStatisticsSearchOutput> result = new List<RTSPFSStatisticsSearchOutput>();
                    while (reader.Read())
                    {
                        result.Add(new RTSPFSStatisticsSearchOutput
                        {
                            //Country = criteria.ReportType != "4" ? reader["Country"].ToString() : "",
                            Country = reader["Country"].ToString(),
                            //CountryName = criteria.ReportType != "4" ? reader["CountryName"].ToString() : "",
                            //Applicant = criteria.ReportType == "4" ? reader["Applicant"].ToString() : "",
                            CountryName = reader["CountryName"].ToString(),
                            IPC = criteria.ReportType == "3" || criteria.ReportType == "3b" || criteria.ReportType == "5" || criteria.ReportType == "6" || criteria.ReportType == "7" ? reader["IPC"].ToString() : "",
                            Company = criteria.ReportType == "4" || criteria.ReportType == "5" || criteria.ReportType == "7" ? reader["Company"].ToString() : "",
                            Year = reader["Year"].ToString(),
                            Count = Convert.ToInt32(reader["Count"]) 
                        });
                    }
                    return result;
                }
            }
        }

        public async Task  UndoActions(int jobId, int plAppId, string updatedBy)
        {
            using (SqlCommand cmd = new SqlCommand("procPLPTO_UpdLogActionUndo"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@JobId", jobId);
                cmd.Parameters.AddWithValue("@PLAppId", plAppId);
                cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                await cmd.ExecuteNonQueryAsync();
            }
        }


        public IQueryable<RTSSearch> RTSSearchRecords => _dbContext.RTSSearchRecords;

        public IQueryable<RTSSearchAction> RTSSearchActions => _dbContext.RTSSearchActions;

        public IQueryable<RTSSearchUSIFW> RTSSearchUSIFWs => _dbContext.RTSSearchUSIFWs;        // for form extraction

        #region RTS Update
        public async Task<int> UpdateBiblioRecord(int appId, string updatedBy)
        {
            var parameters = SqlHelper.BuildSqlParameters(new { AppId = appId, UpdatedBy = updatedBy });
            parameters.Add(new SqlParameter
            {
                ParameterName = "Result",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            });
            await _dbContext.Database.ExecuteSqlRawAsync("exec procPdtUpdateBiblio @AppId,@UpdatedBy, @Result Out", parameters.ToArray());
            var result = (int)parameters[2].Value;
            return result;
        }

        public async Task<int> UpdateBiblioRecords(RTSUpdateCriteria criteria)
        {
            using (SqlCommand cmd = new SqlCommand("procPdtUpdateBiblio"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.FillParamValues(criteria);
                cmd.Parameters.RemoveAt("@Result");
                var resultParam = new SqlParameter
                {
                    ParameterName = "Result",
                    DbType = DbType.Int32,
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(resultParam);
                await cmd.ExecuteNonQueryAsync();
                var result = (int)resultParam.Value;
                return result;
            }
        }

        public async Task<List<RTSUpdateWorkflow>> GetUpdateWorkflowRecs(int jobId, int appId)
        {
            var ids = new List<SqlDataRecord>();
            var result = new List<RTSUpdateWorkflow>();

            using (SqlCommand cmd = new SqlCommand("procPdtUpdateWorkflow"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@JobId", jobId);
                cmd.Parameters.AddWithValue("@AppId", appId);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result.Add(new RTSUpdateWorkflow
                        {
                            AppId = reader.GetInt32(0),
                            OldApplicationStatus = reader.GetString(1),
                            TriggerDate = reader.IsDBNull(2) ? (DateTime?)null : (DateTime?)reader.GetDateTime(2)
                        });
                    }
                }
                cmd.Connection?.Close();
            }
            return result;
        }

        public async Task MarkBiblioRecords() {
            var sql = @"Update t1 
		   Set t1.UpdateAppNo = case when t1.MarkAppNo > '' and isnull(t1.ExcludeAppNo,0) <> 1 and isnull(t2.AppNumber,'') <> isnull(t1.DispAppNo,'') then 1 else 0 end,
		   t1.UpdatePubNo = case when t1.MarkPubNo > '' and isnull(t1.ExcludePubNo,0) <> 1 and isnull(t2.PubNumber,'') <> isnull(t1.DispPubNo,'') then 1 else 0 end,
		   t1.UpdatePatNo = case when t1.MarkPatNo > '' and isnull(t1.ExcludePatNo,0) <> 1 and isnull(t2.PatNumber,'') <> isnull(t1.DispPatNo,'') then 1 else 0 end,
		   t1.UpdateFilDate = case when t1.MarkFilDate > '' and isnull(t1.ExcludeFilDate,0) <> 1 and isnull(t2.FilDate,'1/1/1900') <> isnull(t1.PubFilDate,'1/1/1900') then 1 else 0 end,
		   t1.UpdatePubDate = case when t1.MarkPubDate > '' and isnull(t1.ExcludePubDate,0) <> 1 and isnull(t2.PubDate,'1/1/1900') <> isnull(t1.PubPubDate,'1/1/1900') then 1 else 0 end,
		   t1.UpdateIssDate = case when t1.MarkIssDate > '' and isnull(t1.ExcludeIssDate,0) <> 1 and isnull(t2.IssDate,'1/1/1900') <> isnull(t1.PubIssDate,'1/1/1900') then 1 else 0 end,
		   t1.UpdateParentPCTDate = case when t1.MarkPCTDate > '' and isnull(t1.ExcludeParentDate,0) <> 1 and 
		   isnull((Case when t2.CaseType in('PCT','EPP') then t2.PCTDate else t2.ParentFilDate end),'1/1/1900') <> isnull(t1.PubParentPCTDate,'1/1/1900') then 1 else 0 end,
		   t1.UpdateCaseType = case when t1.MarkCaseType > '' and isnull(t1.ExcludeCaseType,0) <> 1 and t1.PubCaseType <> t2.CaseType then 1 else 0 end
		   from tblPubNumberConverted t1
           Inner Join tblPatCountryApplication t2 on t1.AppId=t2.AppId
		   Where t1.MarkAppNo > '' or t1.MarkPubNo > '' or t1.MarkPatNo > '' or t1.MarkFilDate > '' or t1.MarkPubDate > '' or t1.MarkIssDate > '' or
		   t1.MarkPCTDate > '' or t1.MarkCaseType > ''";
            await _dbContext.Database.ExecuteSqlRawAsync(sql);
        }

        public async Task<bool> UndoBiblio(int jobId, int appId, int logId, string updatedBy)
        {
            using (SqlCommand cmd = new SqlCommand("procPdtUpdLogBiblioUndo"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@JobId", jobId);
                cmd.Parameters.AddWithValue("@AppId", appId);
                cmd.Parameters.AddWithValue("@LogId", logId);
                cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                cmd.Parameters.Add("@Return_Value", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                await cmd.ExecuteNonQueryAsync();
                var result = (int)cmd.Parameters["@Return_Value"].Value;
                return result > 0;
            }
        }


        #endregion
    }
}


