using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class RSHistoryService : IRSHistoryService
    {
        private readonly IApplicationDbContext _repository;
        private readonly IRSActionService _rSActionService;
        private readonly IRSCriteriaService _rSCriteriaService;
        private readonly IRSPrintOptionService _rSPrintOptionService;
        private readonly IRSMainService _rSMainService;
        private readonly IEntityService<RSHistory> _entityService;
        private readonly IConfiguration _configuration;

        public RSHistoryService(
            IApplicationDbContext repository
            , IRSActionService rSActionService
            , IRSCriteriaService rSCriteriaService
            , IRSPrintOptionService rSPrintOptionService
            , IRSMainService rSMainService
            , IEntityService<RSHistory> entityService
            , IConfiguration configuration
            )
        {
            _repository = repository;
            _rSActionService = rSActionService;
            _rSCriteriaService = rSCriteriaService;
            _rSPrintOptionService = rSPrintOptionService;
            _rSMainService = rSMainService;
            _entityService = entityService;
            _configuration = configuration;
        }

        public IQueryable<RSHistory> RSHistorys
        {
            get
            {
                return _repository.RSHistorys.AsNoTracking();
            }
        }

        public IQueryable<RSActionHistory> RSActionHistorys
        {
            get
            {
                return _repository.RSActionHistorys.AsNoTracking();
            }
        }

        public IQueryable<RSCriteriaHistory> RSCriteriaHistorys
        {
            get
            {
                return _repository.RSCriteriaHistorys.AsNoTracking();
            }
        }

        public IQueryable<RSPrintOptionHistory> RSPrintOptionHistorys
        {
            get
            {
                return _repository.RSPrintOptionHistorys.AsNoTracking();
            }
        }

        public RSHistory GetRSHistory(int logId)
        {
            return RSHistorys.FirstOrDefault(c => c.LogId == logId);
        }

        public IQueryable<RSActionHistory> GetRSActionHistorys(int logId)
        {
            return RSActionHistorys.Where(c => c.LogId == logId).OrderBy(c => c.ActionHistoryId).AsNoTracking();
        }

        public IQueryable<RSCriteriaHistory> GetRSCriteriaHistorys(int logId)
        {
            return RSCriteriaHistorys.Where(c => c.LogId == logId).OrderBy(c => c.CritHistoryId).AsNoTracking();
        }

        public IQueryable<RSPrintOptionHistory> GetRSPrintOptionHistorys(int logId)
        {
            return RSPrintOptionHistorys.Where(c => c.LogId == logId).OrderBy(c => c.OptionHistoryId).AsNoTracking();
        }

        public bool AddRSHistory(RSHistoryView history)
        {
            int newId = 0;
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "procWebRSHistoryInsertUpdate";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;

                        foreach (PropertyInfo propertyInfo in history.GetType().GetProperties())
                        {
                                SqlParameter param = new SqlParameter();
                                param.ParameterName = "@" + propertyInfo.Name;
                                param.Value = propertyInfo.GetValue(history, null);
                                cmd.Parameters.Add(param);
                        }

                        sqlConnection.Open();

                        SqlDataReader reader = cmd.ExecuteReader();
                        reader.Read();
                        newId = ((int)reader["newID"]);
                    }
                }
                history.LogId = newId;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> UpdateRSHistory(RSHistory history)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "procWebRSHistoryInsertUpdate";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;

                        foreach (PropertyInfo propertyInfo in history.GetType().GetProperties())
                        {
                            if (propertyInfo.Name.Equals("RSActionHistorys") || propertyInfo.Name.Equals("RSCriteriaHistorys") || propertyInfo.Name.Equals("RSPrintOptionHistorys"))
                                continue;
                            SqlParameter param = new SqlParameter();
                            param.ParameterName = "@" + propertyInfo.Name;
                            param.Value = propertyInfo.GetValue(history, null);
                            cmd.Parameters.Add(param);
                        }

                        sqlConnection.Open();

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public IQueryable<RSHistory> GetRSHistorys(int taskId)
        {
            return RSHistorys.Where(c => c.TaskId == taskId);
        }
    }
}
