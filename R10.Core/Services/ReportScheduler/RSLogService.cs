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
    public class RSLogService : IRSLogService
    {
        private readonly IApplicationDbContext _repository;
        private readonly IEntityService<RSLog> _entityService;
        private readonly IConfiguration _configuration;

        public RSLogService(
            IApplicationDbContext repository
            , IEntityService<RSLog> entityService
            , IConfiguration configuration
            )
        {
            _repository = repository;
            _entityService = entityService;
            _configuration = configuration;
        }

        public bool AddReportSchedulerLog(RSLog rsLog)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "procWebRSLogInsert";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;

                        foreach (PropertyInfo propertyInfo in rsLog.GetType().GetProperties())
                        {
                            SqlParameter param = new SqlParameter();
                            param.ParameterName = "@" + propertyInfo.Name;
                            param.Value = propertyInfo.GetValue(rsLog, null);
                            cmd.Parameters.Add(param);
                        }

                        sqlConnection.Open();

                        SqlDataReader reader = cmd.ExecuteReader();
                        reader.Read();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
}
