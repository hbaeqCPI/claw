using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using R10.Core.DTOs;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class RSMainService : EntityService<RSMain>, IRSMainService
    {
        private readonly IApplicationDbContext _repository;
        private readonly IRSActionService _rSActionService;
        private readonly IRSCriteriaService _rSCriteriaService;
        private readonly IRSPrintOptionService _rSPrintOptionService;

        public RSMainService(
            IApplicationDbContext repository
            , IRSActionService rSActionService
            , IRSCriteriaService rSCriteriaService
            , IRSPrintOptionService rSPrintOptionService
            , ICPiDbContext cpiDbContext
            , ClaimsPrincipal user
            ) : base(cpiDbContext, user)
        {
            _repository = repository;
            _rSActionService = rSActionService;
            _rSCriteriaService = rSCriteriaService;
            _rSPrintOptionService = rSPrintOptionService;
        }

        public IQueryable<RSReportType> RSReportTypes
        {
            get
            {
                return _repository.RSReportTypes.AsNoTracking();
            }
        }


        public IQueryable<RSFrequencyType> RSFrequencyTypes
        {
            get
            {
                return _repository.RSFrequencyTypes.AsNoTracking();
            }
        }

        public IQueryable<RSDateTypeControl> RSDateTypeControls
        {
            get
            {
                return _repository.RSDateTypeControls.AsNoTracking();
            }
        }

        public IQueryable<RSPrintOptionControl> RSPrintOptionControls
        {
            get
            {
                return _repository.RSPrintOptionControls.AsNoTracking();
            }
        }

        public IQueryable<RSCriteriaControl> RSCriteriaControls
        {
            get
            {
                return _repository.RSCriteriaControls.AsNoTracking();
            }
        }


        public IQueryable<RSAction> RSActions
        {
            get
            {
                return _repository.RSActions.AsNoTracking();
            }
        }

        public IQueryable<RSCriteria> RSCriterias
        {
            get
            {
                return _repository.RSCriterias.AsNoTracking();
            }
        }

        public IQueryable<RSPrintOption> RSPrintOptions
        {
            get
            {
                return _repository.RSPrintOptions.AsNoTracking();
            }
        }

        public IQueryable<RSMain> RSMains
        {
            get
            {
                return _repository.RSMains.AsNoTracking();
            }
        }

        //public bool AddRSMain(RSMain rSMain)
        //{
        //    try
        //    {
        //        _repository.RSMains.Add(rSMain);
        //        int taskId = _repository.RSMains.FirstOrDefault(c => c.Name == rSMain.Name).TaskId;
        //        // add default criteria and print options
        //        IQueryable<RSCriteriaControl> rSCriteriaControls = _repository.RSCriteriaControls.Where(c => c.DefaultField==true&&c.ReportId==rSMain.ReportId);
        //        foreach (RSCriteriaControl rSCriteriaControl in rSCriteriaControls)
        //        {
        //            _repository.RSCriterias.Add(new RSCriteria(rSCriteriaControl, taskId, "creator", DateTime.Now));
        //        }

        //        IQueryable<RSPrintOptionControl> rSPrintOptionControls = _repository.RSPrintOptionControls.Where(c => c.ReportId == rSMain.ReportId);
        //        foreach(RSPrintOptionControl rSPrintOptionControl in rSPrintOptionControls)
        //        {
        //            _repository.RSPrintOptions.Add(new RSPrintOption(rSPrintOptionControl, taskId, "creator", DateTime.Now));
        //        }

        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        return false;
        //    }
        //}

        public RSHistory CreateRSHistory(int taskId, int actionId)
        {
            RSMain rSMain = GetRSMainById(taskId);
            RSHistory rSHistory = new RSHistory(rSMain, actionId);

            return rSHistory;
        }

        //public bool DeleteRSMainById(int taskId)
        //{
        //    try
        //    {
        //        _rSActionService.DeleteRSActionByTaskId(taskId);
        //        _rSCriteriaService.DeleteRSCriteriaByTaskId(taskId);
        //        _rSPrintOptionService.DeleteRSPrintOptionByTaskId(taskId);
        //        _repository.RSMains.Remove(RSMains.FirstOrDefault(c=>c.TaskId==taskId));
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        return false;
        //    }
        //}

        public override async Task Delete(RSMain rSMain)
        {
            //await ValidatePermission(invention.InvId, CPiPermissions.CanDelete);

            _cpiDbContext.GetRepository<RSMain>().Delete(rSMain);
            await _cpiDbContext.SaveChangesAsync();
        }

        public IQueryable<RSFrequencyType> GetRSFrequencyTypes()
        {
            return _repository.RSFrequencyTypes;
        }

        public RSMain GetRSMainById(int taskId)
        {
            if (taskId ==0)
                return new RSMain();
            return RSMains.FirstOrDefault(c => c.TaskId == taskId);
        }

        public override async Task Update(RSMain rSMain)
        {
            //await ValidatePermission(rSMain.TaskId, CPiPermissions.FullModify);
            //await ValidateInvention(invention);
            _cpiDbContext.GetRepository<RSMain>().Update(rSMain);
            await _cpiDbContext.SaveChangesAsync();
        }

        public override async Task Add(RSMain rSMain)
        {
            //Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Patent, CPiPermissions.FullModify, invention.RespOffice));

            //if (IsOwnerRequired || IsInventorRequired)
            //    Guard.Against.Null(null, IsOwnerRequired ? "Owner" : "Inventor");

            //await ValidateInvention(invention);
            await base.Add(rSMain);
        }


        public RSMain GetRSMainByName(string name)
        {
            return _repository.RSMains.AsNoTracking().First(c=>c.Name.Equals(name));
        }

        public async Task<Tuple<string, string>> CopySchedule(int CopyTaskId, string newScheduleName, bool CopySettings, bool CopyActions, bool CopyCriteria, bool CopyPrintOptions, string createdBy)
        {
            //var ids = new List<SqlDataRecord> { };
            //foreach (var countryId in countryIds)
            //{
            //    var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.Int) });
            //    record.SetInt32(0, countryId);
            //    ids.Add(record);
            //}
            using (SqlCommand cmd = new SqlCommand("procWebRSCopySchedule"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = GetSqlConnection();
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters["@CopyTaskId"].Value = CopyTaskId;
                cmd.Parameters["@newScheduleName"].Value = newScheduleName;
                cmd.Parameters["@CopySettings"].Value = CopySettings;
                cmd.Parameters["@CopyActions"].Value = CopyActions;
                cmd.Parameters["@CopyCriteria"].Value = CopyCriteria;
                cmd.Parameters["@CopyPrintOptions"].Value = CopyPrintOptions;
                cmd.Parameters["@CreatedBy"].Value = createdBy;
                cmd.Parameters["@TaskCreatorId"].Value = _user.Claims.FirstOrDefault(a => a.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                cmd.Parameters["@AddedRecords"].Value = string.Empty;
                cmd.Parameters["@NewTaskId"].Value = string.Empty;

                await cmd.ExecuteNonQueryAsync();

                var addedRecords = (string)cmd.Parameters["@AddedRecords"].Value;
                var newTaskId = (string)cmd.Parameters["@NewTaskId"].Value;

                return Tuple.Create(addedRecords, newTaskId);
            }
        }

        public int GetReportId(int TaskId)
        {
            if (TaskId == 0)
                return 0;
            return RSMains.FirstOrDefault(c => c.TaskId == TaskId).ReportId;
        }

        public List<RSDueDate> GetDueDates(int TaskId)
        {
            List<RSDueDate> rSDueDates = new List<RSDueDate>();
            using (SqlCommand cmd = new SqlCommand("procWebRSDueDates"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = GetSqlConnection();
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                //SqlCommandBuilder.DeriveParameters(cmd);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@TaskId";
                param.Value = TaskId;
                cmd.Parameters.Add(param);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader["CaseNumber"] != DBNull.Value && (Int64)reader["DATA_OrderOfEntry"] == 1)
                    {
                        RSDueDate rSDueDate = new RSDueDate();

                        rSDueDate.CaseNumber = (string)reader["CaseNumber"];
                        rSDueDate.Country = (string)reader["Country"];
                        rSDueDate.CountryName = (string)reader["CountryName"];
                        rSDueDate.SubCase = (string)reader["SubCase"];
                        rSDueDate.ActionType = (string)reader["ActionType"];
                        rSDueDate.BaseDate = (DateTime)reader["BaseDate"];
                        rSDueDate.ActionDue = (string)reader["ActionDue"];
                        rSDueDate.DueDate = (DateTime)reader["DueDate"];
                        rSDueDate.Indicator = (string)reader["Indicator"];
                        rSDueDate.Responsible = (string)reader["Responsible"];
                        rSDueDate.Status = (string)reader["Status"];
                        rSDueDate.SysSrc = (string)reader["SysSrc"];
                        rSDueDate.RespOffice = (string)reader["RespOffice"];
                        rSDueDate.CaseType = (string)reader["CaseType"];

                        rSDueDates.Add(rSDueDate);
                    }
                }

                return rSDueDates;
            }
        }

        public List<RSPatentListPreview> GetPatentListPreviewList(int TaskId)
        {
            List<RSPatentListPreview> rSPatentListPreview = new List<RSPatentListPreview>();
            using (SqlCommand cmd = new SqlCommand("procWebRSPatentListPreview"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = GetSqlConnection();
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                //SqlCommandBuilder.DeriveParameters(cmd);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@TaskId";
                param.Value = TaskId;
                cmd.Parameters.Add(param);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader["CA_CaseNumber"] != DBNull.Value && (Int64)reader["DATA_OrderOfEntry"] == 1)
                    {
                        RSPatentListPreview preview = new RSPatentListPreview();

                        preview.CaseNumber = (string)reader["CA_CaseNumber"];
                        preview.Country = (string)reader["CA_Country"];
                        preview.CountryName = reader["CA_CountryName"] == DBNull.Value ? "" : (string)reader["CA_CountryName"];
                        preview.SubCase = reader["CA_SubCase"]==DBNull.Value?"":(string)reader["CA_SubCase"];
                        preview.AppNumber = reader["CA_AppNumber"] == DBNull.Value ? "" : (string)reader["CA_AppNumber"];
                        preview.FilDate = reader["CA_FilDate"] == DBNull.Value ? null : (DateTime?)reader["CA_FilDate"];
                        preview.PubNumber = reader["CA_PubNumber"] == DBNull.Value ? "" : (string)reader["CA_PubNumber"];
                        preview.PubDate = reader["CA_PubDate"] == DBNull.Value ? null : (DateTime?)reader["CA_PubDate"];
                        preview.PatNumber = reader["CA_PatNumber"] == DBNull.Value ? "" : (string)reader["CA_PatNumber"];
                        preview.IssDate = reader["CA_IssDate"] == DBNull.Value ? null : (DateTime?)reader["CA_IssDate"];
                        preview.Status = reader["CA_ApplicationStatus"] == DBNull.Value ? "" : (string)reader["CA_ApplicationStatus"];
                        preview.RespOffice = reader["CA_RespOffice"] == DBNull.Value ? "" : (string)reader["CA_RespOffice"];
                        preview.CaseType = reader["CA_CaseType"] == DBNull.Value ? "" : (string)reader["CA_CaseType"];

                        rSPatentListPreview.Add(preview);
                    }
                }

                return rSPatentListPreview;
            }
        }

        public List<RSTrademarkListPreview> GetTrademarkListPreviewList(int TaskId)
        {
            List<RSTrademarkListPreview> rSTrademarkListPreview = new List<RSTrademarkListPreview>();
            using (SqlCommand cmd = new SqlCommand("procWebRSTrademarkListPreview"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = GetSqlConnection();
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                //SqlCommandBuilder.DeriveParameters(cmd);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@TaskId";
                param.Value = TaskId;
                cmd.Parameters.Add(param);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader["Tmk_CaseNumber"] != DBNull.Value && (Int64)reader["DATA_OrderOfEntry"] == 1)
                    {
                        RSTrademarkListPreview preview = new RSTrademarkListPreview();

                        preview.CaseNumber = (string)reader["Tmk_CaseNumber"];
                        preview.Country = (string)reader["Tmk_Country"];
                        preview.CountryName = reader["Tmk_CountryName"] == DBNull.Value ? "" : (string)reader["Tmk_CountryName"];
                        preview.SubCase = reader["Tmk_SubCase"] == DBNull.Value ? "" : (string)reader["Tmk_SubCase"];
                        preview.AppNumber = reader["Tmk_AppNumber"] == DBNull.Value ? "" : (string)reader["Tmk_AppNumber"];
                        preview.FilDate = reader["Tmk_FilDate"] == DBNull.Value ? null : (DateTime?)reader["Tmk_FilDate"];
                        preview.PubNumber = reader["Tmk_PubNumber"] == DBNull.Value ? "" : (string)reader["Tmk_PubNumber"];
                        preview.PubDate = reader["Tmk_PubDate"] == DBNull.Value ? null : (DateTime?)reader["Tmk_PubDate"];
                        preview.RegNumber = reader["Tmk_RegNumber"] == DBNull.Value ? "" : (string)reader["Tmk_RegNumber"];
                        preview.RegDate = reader["Tmk_RegDate"] == DBNull.Value ? null : (DateTime?)reader["Tmk_RegDate"];
                        preview.Status = reader["Tmk_TrademarkStatus"] == DBNull.Value ? "" : (string)reader["Tmk_TrademarkStatus"];
                        preview.RespOffice = reader["Tmk_RespOffice"] == DBNull.Value ? "" : (string)reader["Tmk_RespOffice"];
                        preview.CaseType = reader["Tmk_CaseType"] == DBNull.Value ? "" : (string)reader["Tmk_CaseType"];

                        rSTrademarkListPreview.Add(preview);
                    }
                }

                return rSTrademarkListPreview;
            }
        }

        public List<RSMatterListPreview> GetMatterListPreviewList(int TaskId)
        {
            List<RSMatterListPreview> rSTrademarkListPreview = new List<RSMatterListPreview>();
            using (SqlCommand cmd = new SqlCommand("procWebRSMatterListPreview"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = GetSqlConnection();
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                //SqlCommandBuilder.DeriveParameters(cmd);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@TaskId";
                param.Value = TaskId;
                cmd.Parameters.Add(param);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader["Mat_CaseNumber"] != DBNull.Value && (Int64)reader["DATA_OrderOfEntry"] == 1)
                    {
                        RSMatterListPreview preview = new RSMatterListPreview();

                        preview.CaseNumber = (string)reader["Mat_CaseNumber"];
                        preview.SubCase = reader["Mat_SubCase"] == DBNull.Value ? "" : (string)reader["Mat_SubCase"];
                        preview.Countries = reader["Mat_Countries"] == DBNull.Value ? "" : (string)reader["Mat_Countries"];
                        preview.Attorneys = reader["Mat_Attorneys"] == DBNull.Value ? "" : (string)reader["Mat_Attorneys"];
                        preview.EffectiveOpenDate = reader["Mat_EffectiveOpenDate"] == DBNull.Value ? null : (DateTime?)reader["Mat_EffectiveOpenDate"];
                        preview.TerminationEndDate = reader["Mat_TerminationEndDate"] == DBNull.Value ? null : (DateTime?)reader["Mat_TerminationEndDate"];
                        preview.Status = reader["Mat_MatterStatus"] == DBNull.Value ? "" : (string)reader["Mat_MatterStatus"];
                        preview.RespOffice = reader["Mat_RespOffice"] == DBNull.Value ? "" : (string)reader["Mat_RespOffice"];
                        preview.MatterType = reader["Mat_MatterType"] == DBNull.Value ? "" : (string)reader["Mat_MatterType"];

                        rSTrademarkListPreview.Add(preview);
                    }
                }

                return rSTrademarkListPreview;
            }
        }

        public DataTable GetReportParameters(int taskId, int actionId, int reportId)
        {
            string storedProcedureName;
            switch (reportId)
            {
                case 1:
                    storedProcedureName = "procWebRSPatentListReportParameters";
                    break;
                case 2:
                    storedProcedureName = "procWebRSReportParameters";
                    break;
                case 5:
                    storedProcedureName = "procWebRSTrademarkListReportParameters";
                    break;
                case 6:
                    storedProcedureName = "procWebRSMatterListReportParameters";
                    break;
                default:
                    storedProcedureName = "procWebRSReportParameters";
                    break;
            }

            using (SqlDataAdapter da = new SqlDataAdapter(storedProcedureName, GetSqlConnection()))
            {
                DataTable dt = new DataTable();
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@TaskId", taskId);
                da.SelectCommand.Parameters.AddWithValue("@ActionId", actionId);

                da.Fill(dt);
                return dt;
            }
        }

        public List<RSReportAttorney> GetReportAttorneys(int taskId, int actionId, int reportId)
        {
            string storedProcedureName;
            switch (reportId)
            {
                case 1:
                    storedProcedureName = "procWebRSPatentListPreview";
                    break;
                case 2:
                    storedProcedureName = "procWebRSDueDates";
                    break;
                case 5:
                    storedProcedureName = "procWebRSTrademarkListPreview";
                    break;
                //case 6:
                //    storedProcedureName = "procWebRSMatterListReportParameters";
                //    break;
                default:
                    storedProcedureName = "procWebRSDueDates";
                    break;
            }

            List<RSReportAttorney> rsAttorneys = new List<RSReportAttorney>();
            using (SqlCommand cmd = new SqlCommand(storedProcedureName))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = GetSqlConnection();
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@TaskId", taskId);
                cmd.Parameters.AddWithValue("@ActionId", actionId);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (reader["Attorney"] != DBNull.Value)
                    {
                        rsAttorneys.Add(ConvertToAttorney(reader));
                    }
                }
                return rsAttorneys;
            }
        }

        private RSReportAttorney ConvertToAttorney(SqlDataReader reader)
        {
            return new RSReportAttorney
            {
                Attorney = (string)reader["Attorney"],
                AttorneyName = (string)reader["AttorneyName"],
                AttorneyEmail = (string)reader["EMail"]
            };
        }

        private SqlConnection GetSqlConnection()
        {
            return new SqlConnection(_repository.Database.GetDbConnection().ConnectionString);
        }
    }
}
