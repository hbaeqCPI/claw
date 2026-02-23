using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using R10.Core.DTOs;
using R10.Core.Entities.GlobalSearch;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Services.Shared
{
    public class GlobalSearchService : IGlobalSearchService
    {
        private readonly IApplicationDbContext _repository;
        private readonly ICPiDbContext _cPiDbContext;
        public GlobalSearchService(IApplicationDbContext repository, ICPiDbContext cPiDbContext)
        {
            _repository = repository;
            _cPiDbContext = cPiDbContext;
        }

        public IQueryable<GSSystem> GSSystems => _repository.GSSystems.AsNoTracking();
        public IQueryable<GSScreen> GSScreens => _repository.GSScreens.AsNoTracking();
        public IQueryable<GSField> GSFields => _repository.GSFields.AsNoTracking();
        public IQueryable<CPiSystem> CPiSystems => _cPiDbContext.GetRepository<CPiSystem>().QueryableList.AsNoTracking();

        public async Task<IEnumerable<GSSearchDTO>> RunGlobalSearchDB(string userName, bool hasRespOfficeOn, bool hasEntityFilterOn, GSParamDTO parameters)
        {
            var filterFields = BuildDBFilter(parameters.DataFilters);
            var moreFilters = BuildMoreFilter(parameters.MoreFilters);

            using (SqlDataAdapter da = new SqlDataAdapter("procGS_Search", new SqlConnection(_repository.Database.GetDbConnection().ConnectionString)))
            {
                DataTable dt = new DataTable();
                var cmd = da.SelectCommand;
                cmd.CommandType = CommandType.StoredProcedure;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@SearchMode", parameters.SearchMode);
                cmd.Parameters.AddWithValue("@SysScreenTypes", parameters.SystemScreens);
                cmd.Parameters.AddWithValue("@SearchTerm", parameters.BasicSearchTerm);

                cmd.Parameters.AddWithValue("@TVPFilterFields", filterFields).SqlDbType = SqlDbType.Structured;

                if (moreFilters.Count > 0)
                    cmd.Parameters.AddWithValue("@TVPAndFilters", moreFilters).SqlDbType = SqlDbType.Structured;

                cmd.Parameters.AddWithValue("@UserName", userName);
                cmd.Parameters.AddWithValue("@HasRespOfficeOn", hasRespOfficeOn);
                cmd.Parameters.AddWithValue("@HasEntityFilterOn", hasEntityFilterOn);
                //cmd.Parameters.AddWithValue("@DMSUserType", criteria.DMSUserType);
                //cmd.Parameters.AddWithValue("@DMSUserId", criteria.DMSUserId);
                //da.Fill(dt);

                await Task.Run(() => da.Fill(dt));

                var result = dt.AsEnumerable().Select(m => new GSSearchDTO()
                {
                    Link = m.Field<string>("Link"),
                    SystemName = m.Field<string>("SystemName"),
                    ScreenName = m.Field<string>("ScreenName"),
                    FieldValues = m.Field<string>("FieldValues")
                }).ToList();

                return result;
            }

        }

        public async Task<IEnumerable<GSSearchDocDTO>> RunGlobalSearchDoc(string userName, bool hasRespOfficeOn, bool hasEntityFilterOn, List<GSDocParamDTO> parameters, IEnumerable<GSMoreFilter> screenMoreFilters)
        {
            if (parameters.Count() < 1)
            {
                return new List<GSSearchDocDTO>();
            }

            var docList = BuildDocList(parameters);
            var moreFilters = BuildMoreFilter(screenMoreFilters);

            using (SqlDataAdapter da = new SqlDataAdapter("procGS_SearchDoc", new SqlConnection(_repository.Database.GetDbConnection().ConnectionString)))
            {
                DataTable dt = new DataTable();
                var cmd = da.SelectCommand;
                cmd.CommandType = CommandType.StoredProcedure;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@TVPDocList", docList).SqlDbType = SqlDbType.Structured;
                if (moreFilters.Count > 0)
                    cmd.Parameters.AddWithValue("@TVPAndFilters", moreFilters).SqlDbType = SqlDbType.Structured;

                cmd.Parameters.AddWithValue("@UserName", userName);
                cmd.Parameters.AddWithValue("@HasRespOfficeOn", hasRespOfficeOn);
                cmd.Parameters.AddWithValue("@HasEntityFilterOn", hasEntityFilterOn);
                //cmd.Parameters.AddWithValue("@DMSUserType", criteria.DMSUserType);
                //cmd.Parameters.AddWithValue("@DMSUserId", criteria.DMSUserId);
                //da.Fill(dt);
                await Task.Run(() => da.Fill(dt));
                //await da.FillAsync(dt);

                var dtDTO = dt.AsEnumerable().Select(m => new GSSearchDocDTO()
                {
                    RecordId = m.Field<int>("RecordId"),
                    Link = m.Field<string>("Link"),
                    SystemName = m.Field<string>("SystemName"),
                    ScreenName = m.Field<string>("ScreenName"),
                    DocumentTypeName = m.Field<string>("DocumentTypeName"),
                    FieldValues = m.Field<string>("FieldValues")
                    //FileSize = m.Field<int>("FileSize")
                }).ToList();

                var result = dtDTO.Join(parameters, dto => dto.RecordId, param => param.RecordId,
                        (dto, param) => new GSSearchDocDTO {
                                    RecordKey = $"{param.SystemType}|{param.ScreenCode}|{param.DocumentType}|{param.ParentId}|{param.LogId}|{param.FileName}",
                                    RecordId = dto.RecordId, SystemType = param.SystemType, ScreenCode = param.ScreenCode, DocumentType = param.DocumentType,
                                    ParentId = param.ParentId, Link = dto.Link, SystemName = dto.SystemName, ScreenName = dto.ScreenName, FieldValues = dto.FieldValues, 
                                    DocumentTypeName = dto.DocumentTypeName, FileName = param.FileName, SearchScore = param.SearchScore } );
                return result;
            }
        }

        public async Task<IEnumerable<GSDownloadDTO>> GetDownloadDocInfo(List<GSDownloadParamDTO> parameters)
        {
           
            var docList = BuildDownloadList(parameters);

            using (SqlDataAdapter da = new SqlDataAdapter("procGS_DocFileInfo", new SqlConnection(_repository.Database.GetDbConnection().ConnectionString)))
            {
                DataTable dt = new DataTable();
                var cmd = da.SelectCommand;
                cmd.CommandType = CommandType.StoredProcedure;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@TVPDocFiles", docList).SqlDbType = SqlDbType.Structured;
                await Task.Run(() => da.Fill(dt));

                var dtDTO = dt.AsEnumerable().Select(m => new GSDownloadDTO()
                {
                    RecordId = m.Field<int>("RecordId"),
                    SystemType = m.Field<string>("SystemType"),
                    DocumentType = m.Field<string>("DocumentType"),
                    DocFileName = m.Field<string>("DocFileName"),
                    UserFileName = m.Field<string>("UserFileName")
                }).ToList();

                dtDTO.ForEach(dto => dto.UserFileName = dto.UserFileName + "-" + dto.DocFileName);
                return dtDTO;
            }
        }

        private List<SqlDataRecord> BuildMoreFilter(IEnumerable<GSMoreFilter> filters)
        {
            var filterList = new List<SqlDataRecord>();
            foreach (GSMoreFilter item in filters)
            {
                var record = new SqlDataRecord(
                    new SqlMetaData("FieldName", SqlDbType.VarChar, 50),
                    new SqlMetaData("FieldValue", SqlDbType.NVarChar, SqlMetaData.Max));

                record.SetString(0, item.FieldName);
                record.SetString(1, item.FieldValue);
                filterList.Add(record);
            }

            return filterList;
        }

        private List<SqlDataRecord> BuildDBFilter(IEnumerable<GSDataFilterBase> filters)
        {
            var filterList = new List<SqlDataRecord>();
            foreach (GSDataFilterBase item in filters)
            {
                var record = new SqlDataRecord(
                    new SqlMetaData("OrderEntry", SqlDbType.Int),
                    new SqlMetaData("LogicalOperator", SqlDbType.VarChar, 10),
                    new SqlMetaData("LeftParen", SqlDbType.VarChar, 10),
                    new SqlMetaData("FieldId", SqlDbType.Int),
                    new SqlMetaData("SearchTerm", SqlDbType.NVarChar, 1000),
                    new SqlMetaData("RightParen", SqlDbType.VarChar, 10));


                record.SetInt32(0, item.OrderEntry);
                record.SetString(1, item.LogicalOperator);
                record.SetString(2, item.LeftParen);
                record.SetInt32(3, item.FieldId);
                record.SetString(4, item.Criteria);
                record.SetString(5, item.RightParen);
                filterList.Add(record);
            }

            return filterList;
        }

        private List<SqlDataRecord> BuildDocList(IEnumerable<GSDocParamDTO> filters)
        {
            var filterList = new List<SqlDataRecord>();
            foreach (GSDocParamDTO item in filters)
            {
                var record = new SqlDataRecord(
                    new SqlMetaData("RecordId", SqlDbType.Int),
                    new SqlMetaData("SystemType", SqlDbType.VarChar, 5),
                    new SqlMetaData("ScreenCode", SqlDbType.VarChar, 20),
                    new SqlMetaData("ParentId", SqlDbType.Int),
                    new SqlMetaData("DocumentType", SqlDbType.VarChar, 20),
                    new SqlMetaData("LogId", SqlDbType.Int),
                    new SqlMetaData("DocFileName", SqlDbType.NVarChar, 255)
                    //new SqlMetaData("DocumentSize", SqlDbType.BigInt),
                    //new SqlMetaData("DocumentPath", SqlDbType.NVarChar, 500),
                    //new SqlMetaData("SearchScore", SqlDbType.Decimal)
                    );

                // FileName

                record.SetInt32(0, item.RecordId);
                record.SetString(1, item.SystemType);
                record.SetString(2, item.ScreenCode);
                record.SetInt32(3, item.ParentId);
                record.SetString(4, item.DocumentType);
                record.SetInt32(5, item.LogId);
                record.SetString(6, item.FileName);
                //record.SetInt64(7, item.FileSize);
                //record.SetString(8, item.FilePath);
                //record.SetDecimal(9, item.SearchScore);
                filterList.Add(record);
            }

            return filterList;
        }

        private List<SqlDataRecord> BuildDownloadList(IEnumerable<GSDownloadParamDTO> filters)
        {
            var filterList = new List<SqlDataRecord>();
            foreach (GSDownloadParamDTO item in filters)
            {
                var record = new SqlDataRecord(
                    new SqlMetaData("RecordId", SqlDbType.Int),
                    new SqlMetaData("SystemType", SqlDbType.VarChar, 5),
                    new SqlMetaData("ScreenCode", SqlDbType.VarChar, 20),
                    new SqlMetaData("ParentId", SqlDbType.Int),
                    new SqlMetaData("DocumentType", SqlDbType.VarChar, 20),
                    new SqlMetaData("LogId", SqlDbType.Int),
                    new SqlMetaData("ScreenCode", SqlDbType.NVarChar, 255)
                    );

                record.SetInt32(0, item.RecordId);
                record.SetString(1, item.SystemType);
                record.SetString(2, item.ScreenCode);
                record.SetInt32(3, item.ParentId);
                record.SetString(4, item.DocumentType);
                record.SetInt32(5, item.LogId);
                record.SetString(6, item.DocFileName);
                filterList.Add(record);
            }
            return filterList;
        }

        public async Task LogGlobalSearch(string userName, string searchCriteria)
        {
            using (SqlCommand cmd = new SqlCommand("procGS_Log"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(_repository.Database.GetDbConnection().ConnectionString);
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.Add(new SqlParameter("@UserName", userName));
                cmd.Parameters.Add(new SqlParameter("@SearchCriteria", searchCriteria));
                await cmd.ExecuteNonQueryAsync();
            }
        }

    }


}
