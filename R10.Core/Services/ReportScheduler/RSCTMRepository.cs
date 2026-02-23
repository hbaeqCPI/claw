using Microsoft.Extensions.Configuration;
using R10.Core.Entities.ReportScheduler;
using System;
using System.Data;
using System.Data.SqlClient;

namespace R10.Core.Services
{
    public class RSCTMRepository
    {
        private readonly IConfiguration _configuration;

        public RSCTMRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DataTable GetScheduleById(string taskName)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                SqlDataAdapter dar = new SqlDataAdapter();
                SqlCommand sqlcommand = new SqlCommand("procCTMTask", connection);
                dar.SelectCommand = sqlcommand;
                dar.SelectCommand.CommandType = CommandType.StoredProcedure;
                //dar.SelectCommand.CommandTimeout = SqlHelper.GetCommandTimeOut();

                try
                {
                    dar.SelectCommand.Connection.Open();
                    dar.SelectCommand.Parameters.AddWithValue("@Action", 4);
                    dar.SelectCommand.Parameters.AddWithValue("@TaskName", taskName);
                    DataTable table = new DataTable();
                    dar.Fill(table);
                    return table;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public bool Save(tblCTMMain entity, int updateType)
        {
            int action;
            SqlParameter[] parameters = null;
            //1 insert, 2 update, 3 delete
            switch (updateType)
            {
                case 3:
                    action = 3;
                    parameters = GetDeleteParameters(action, entity);
                    break;
                case 2:
                    action = 2;
                    parameters = GetInsertUpdateParameters(action, entity);
                    break;
                default:
                    action = 1;
                    parameters = GetInsertUpdateParameters(action, entity);
                    break;
            }

            try
            {

                using (SqlConnection sqlConnection = new SqlConnection(GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "procCTMTask";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;

                        cmd.Parameters.AddRange(parameters);

                        sqlConnection.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }

            //using (SqlConnection connection = new SqlConnection(ReportSchedulerHelper.GetConnectionString())
            //{
            //    // Create the Command and Parameter objects.
            //    SqlCommand command = new SqlCommand("procCTMTask", connection);
            //    command.Parameters.AddWithValue("@Action", 1);
            //    command.Parameters.AddWithValue("@SchedID", entity.SchedID);
            //    command.Parameters.AddWithValue("@TaskCode", entity.TaskCode);
            //    command.Parameters.AddWithValue("@TaskName", entity.TaskName);
            //    command.Parameters.AddWithValue("@TaskType", entity.TaskType);
            //    command.Parameters.AddWithValue("@SQLServer", entity.SQLServer);
            //    command.Parameters.AddWithValue("@DBName", entity.DBName);
            //    command.Parameters.AddWithValue("@DBConfigName", entity.DBConfigName);
            //    command.Parameters.AddWithValue("@Active", entity.Active);
            //    command.Parameters.AddWithValue("@NeedsRefresh", entity.NeedsRefresh);
            //    command.Parameters.AddWithValue("@WorkStationID", entity.WorStationID);
            //    try
            //    {
            //        connection.Open();
            //        SqlDataReader reader = command.ExecuteReader();
            //        while (reader.Read())
            //        {
            //            return Convert.ToInt32(reader[0]);
            //        }
            //        reader.Close();
            //    }
            //    catch (Exception ex)
            //    {
            //        return 0;
            //    }                
            //}
        }

        public bool SyncWithCTM(int CTMId, int ActionId, DateTime? NextRunTime, string? ErrorMessage)
        {
            SqlParameter[] parameters = { 
                    new SqlParameter("@CTMId", CTMId),
                    new SqlParameter("@ActionId", ActionId),
                    new SqlParameter("@NextRunTime", NextRunTime),
                    new SqlParameter("@ErrorMessage", ErrorMessage)
                   };

            try
            {

                using (SqlConnection sqlConnection = new SqlConnection(GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "procCTMRSActions";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = sqlConnection;

                        cmd.Parameters.AddRange(parameters);

                        sqlConnection.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        private SqlParameter[] GetInsertUpdateParameters(int action, tblCTMMain entity)
        {
            SqlParameter[] parameters = { new  SqlParameter("@Action", action),
                    new SqlParameter("@SchedID", entity.SchedID),
                    new SqlParameter("@TaskCode", entity.TaskCode),
                    new SqlParameter("@TaskName", entity.TaskName),
                    new SqlParameter("@TaskType", entity.TaskType),
                    new SqlParameter("@SQLServer", entity.SQLServer),
                    new SqlParameter("@DBName", entity.DBName),
                    new SqlParameter("@DBConfigName", entity.DBConfigName),
                    new SqlParameter("@Active", entity.Active),
                    new SqlParameter("@NeedsRefresh", entity.NeedsRefresh),
                    new SqlParameter("@WorkStationID", entity.WorkStationID),
                    new SqlParameter("@NextProcessDate", entity.NextProcessDate),
                    new SqlParameter("@URL", entity.URL),
                    new SqlParameter("@Notes", entity.Notes),
                    new SqlParameter("@TaskSubType", entity.TaskSubType)
                   };
            return parameters;
        }


        private SqlParameter[] GetDeleteParameters(int action, tblCTMMain entity)
        {
            SqlParameter[] parameters = { new  SqlParameter("@Action", action),
                     new SqlParameter("@SchedID", entity.SchedID),
                     new SqlParameter("@SQLServer", entity.SQLServer),
                     new SqlParameter("@DBName", entity.DBName)
                   };

            return parameters;
        }


        public DateTime GetCTMDateTime()
        {
            try
            {
                string sql = "select dbo.fnGetDateTime(GETDATE())";
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        return Convert.ToDateTime(reader[0]);
                    }
                    reader.Close();
                }
                return DateTime.MinValue;
            }
            catch (Exception)
            {
                throw;
            }

        }



        public int UpdateSchedule()
        {
            throw new NotImplementedException();
        }

        public int UpdateNeedsRefresh()
        {
            throw new NotImplementedException();
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("CTMConnection");
            //return ReportSchedulerHelper.GetConfigConnectionString("CTM");
        }

    }



}