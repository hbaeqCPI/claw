using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core;
using System.Transactions;
using Microsoft.Data.SqlClient.Server;
using System.Data;
using Microsoft.Data.SqlClient;
using R10.Core.Entities;
using R10.Core.Interfaces.Patent;
using R10.Core.Entities.Documents;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace R10.Infrastructure.Data.Patent
{

    public class InventionRepository : IInventionRepository
    {
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly IPatIDSRepository _idsRepository;
        private readonly ApplicationDbContext _dbContext;
        public InventionRepository(ApplicationDbContext dbContext, IPatIDSRepository idsRepository, ISystemSettings<PatSetting> settings)
        {
            _settings = settings;
            _idsRepository = idsRepository;
            _dbContext = dbContext;
        }

        #region Action

        public async Task<List<DelegationEmailDTO>> GetDelegationEmails(int delegationId)
        {
            var list = await _dbContext.DelegationEmailDTO.FromSqlInterpolated($@"Select Distinct DelegationId,AssignedBy,AssignedTo,FirstName,LastName From
                                (Select ddd.DelegationId,ddd.CreatedBy as AssignedBy,u.Email as AssignedTo,u.FirstName,u.LastName From tblCPiGroups g Inner Join tblCPiUserGroups ug on g.Id=ug.GroupId Inner Join tblCPIUsers u on u.Id=ug.UserId Inner Join tblPatDueDateInvDelegation ddd on ddd.GroupId=ug.GroupId Union
                                 Select ddd.DelegationId,ddd.CreatedBy as AssignedBy,u.Email as AssignedTo,u.FirstName,u.LastName From  tblCPIUsers u  Inner Join tblPatDueDateInvDelegation ddd on ddd.UserId=u.Id
                                ) t Where t.DelegationId={delegationId}").AsNoTracking().ToListAsync();
            return list;
        }

        public async Task MarkDelegationasEmailed(int delegationId)
        {
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"Update tblPatDueDateInvDelegation Set NotificationSent=1 Where DelegationId={delegationId}");
        }
        public async Task<List<LookupIntDTO>> GetDelegatedDdIds(int action, int[] recIds)
        {
            var ids = new List<SqlDataRecord>();
            var result = new List<LookupIntDTO>();
            foreach (var item in recIds)
            {
                var record = new SqlDataRecord(new SqlMetaData[] {
                    new SqlMetaData("Id", SqlDbType.Int),
                    new SqlMetaData("DueDate", SqlDbType.DateTime2)
                });

                record.SetInt32(0, item);
                record.SetDBNull(1);
                ids.Add(record);
            }
            using (SqlCommand cmd = new SqlCommand("procPatInvDelegatedTask"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Ids", ids).SqlDbType = SqlDbType.Structured;
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result.Add(new LookupIntDTO { Value = reader.GetInt32(0) });
                    }
                }
                cmd.Connection?.Close();
            }
            return result;
        }
        public async Task<List<DelegationEmailDTO>> GetDeletedDelegationEmails(int delegationId)
        {
            var list = await _dbContext.DelegationEmailDTO.FromSqlInterpolated($"exec procPatInvDelegatedTask @action = 3, @delegationid = {delegationId}").AsNoTracking().ToListAsync();
            return list;
        }

        public async Task<DelegationDetailDTO> GetDeletedDelegation(int delegationId)
        {
            var delegation = await _dbContext.DelegationDetailDTO.FromSqlInterpolated($"Select DataKeyValue as DelegationId, dddLog.ActID,dddLog.DDId,dddLog.GroupId,dddLog.UserId,dddLog.NotificationSent,ParentActId,ParentId    From tblDeleteLog d WITH (NOLOCK) cross apply openjson(d.record) With(ActId int '$.ActId',DDId int '$.DDId',GroupId int '$.GroupId',UserId nvarchar(450) '$.UserId',NotificationSent int '$.NotificationSent',ParentActId int '$.ParentActId',ParentId int '$.ParentId') as dddlog Where DataKey='DelegationId' and SystemType='P' And DataKeyValue={delegationId}").AsNoTracking().FirstOrDefaultAsync();
            return delegation;
        }

        public async Task<List<LookupIntDTO>> GetDuedateChangedDelegationIds(int action, List<PatDueDateInv> updated)
        {
            var ids = new List<SqlDataRecord>();
            var result = new List<LookupIntDTO>();
            foreach (var item in updated)
            {
                var record = new SqlDataRecord(new SqlMetaData[] {
                    new SqlMetaData("Id", SqlDbType.Int),
                    new SqlMetaData("DueDate", SqlDbType.DateTime2)
                });
                record.SetInt32(0, item.DDId);
                record.SetDateTime(1, item.DueDate);
                ids.Add(record);
            }
            using (SqlCommand cmd = new SqlCommand("procPatInvDelegatedTask"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Ids", ids).SqlDbType = SqlDbType.Structured;
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result.Add(new LookupIntDTO { Value = reader.GetInt32(0) });
                    }
                }
                cmd.Connection?.Close();
            }
            return result;
        }

        public async Task AddCustomFieldsAsCopyFields()
        {
            await _dbContext.Database.ExecuteSqlAsync(@$"Insert Into tblPatInventionCopySetting(FieldDesc,FieldName,[Copy],UserName)
                                                        Select cfs.ColumnLabel,cfs.ColumnName,0,cs.UserName from tblSysCustomFieldSetting cfs 
                                                        Cross Join(Select Distinct UserName From tblPatInventionCopySetting) cs
                                                        Where cfs.TableName='tblPatInvention' and cfs.Visible = 1 
                                                        And Not Exists(Select 1 From tblPatInventionCopySetting ecs Where ecs.FieldName=cfs.ColumnName and isnull(ecs.UserName,'')=isnull(cs.UserName,''))
                                                        Order By cfs.OrderOfEntry");

            await _dbContext.Database.ExecuteSqlAsync(@$"Delete ecs From tblPatInventionCopySetting ecs 
                    Where ecs.FieldName like 'CustomField%' And FieldName Not In(Select cfs.ColumnName from tblSysCustomFieldSetting cfs 
                    Where cfs.TableName='tblPatInvention' and cfs.Visible = 1) ");
        }

        #endregion
    }
}