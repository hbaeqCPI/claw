using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient.Server;
using R10.Core.DTOs;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Interfaces.GeneralMatter;
using R10.Core.Identity;

namespace R10.Infrastructure.Data.GeneralMatter
{
    public class GMDelegationUtilityRepository : IGMDelegationUtilityRepository
    {
        protected readonly ApplicationDbContext _dbContext;

        public GMDelegationUtilityRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<CPiUser> CPiUsers => _dbContext.CPiUser.AsNoTracking();
        public IQueryable<GMDueDateDelegation> GMDueDateDelegations => _dbContext.GMDueDateDelegations.AsNoTracking();
        public IQueryable<CPiGroup> CPiGroups => _dbContext.CPiGroups.AsNoTracking();

        public IQueryable<DelegationUtilityPreviewDTO> GetPreviewList(DelegationUtilityCriteriaDTO searchCriteria)
        {
            var sourceQuery = "";
            if (searchCriteria.Mode.ToLower() == "c")
            {
                sourceQuery = "Select * From vwGMActionDueDateDelegation";
                var query = _dbContext.DelegationUtilityPreviewDTO.FromSqlRaw(sourceQuery).AsNoTracking();

                if (searchCriteria.DueDateFrom != null)
                {
                    query = query.Where(d => d.DueDate >= searchCriteria.DueDateFrom);
                }
                if (searchCriteria.DueDateTo != null)
                {
                    query = query.Where(d => d.DueDate <= searchCriteria.DueDateTo);
                }

                if (!string.IsNullOrEmpty(searchCriteria.ActionType))
                {
                    query = query.Where(d => EF.Functions.Like(d.ActionType, searchCriteria.ActionType));
                }
                if (!string.IsNullOrEmpty(searchCriteria.ActionDue))
                {
                    query = query.Where(d => EF.Functions.Like(d.ActionDue, searchCriteria.ActionDue));
                }
                if (!string.IsNullOrEmpty(searchCriteria.Indicator))
                {
                    query = query.Where(d => EF.Functions.Like(d.Indicator, searchCriteria.Indicator));
                }

                if (!string.IsNullOrEmpty(searchCriteria.UserId))
                {
                    query = query.Where(d => d.UserId == searchCriteria.UserId);
                }
                if (searchCriteria.GroupId > 0)
                {
                    query = query.Where(d => d.GroupId == searchCriteria.GroupId);
                }
                return query;
            }
            else
            {
                sourceQuery = @"Select a.CaseNumber,'' as Country, a.SubCase, a.ActId, d.DDId as Id, d.DueDate,a.ActionType,d.ActionDue,d.Indicator, '' as DelegatedUser, '' as DelegatedGroup, u.Id as UserId, grp.Id as GroupId 
                               From tblGMActionDue AS a WITH (NOLOCK) 
                               INNER JOIN tblGMDueDate AS d WITH (NOLOCK) ON d.ActId = a.ActId
                               LEFT JOIN  tblGMDueDateDelegation AS ddd WITH (NOLOCK) ON d.DdId = ddd.DdId 
                               LEFT JOIN  tblCPiGroups AS grp WITH (NOLOCK) ON ddd.GroupId = grp.Id 
                               LEFT JOIN  tblCPIUsers AS u WITH (NOLOCK) ON ddd.UserId = u.Id Where d.DateTaken is null";

                var query = _dbContext.DelegationUtilityPreviewDTO.FromSqlRaw(sourceQuery).AsNoTracking();

                if (searchCriteria.DueDateFromDelegate != null)
                {
                    query = query.Where(d => d.DueDate >= searchCriteria.DueDateFromDelegate);
                }
                if (searchCriteria.DueDateToDelegate != null)
                {
                    query = query.Where(d => d.DueDate <= searchCriteria.DueDateToDelegate);
                }

                if (!string.IsNullOrEmpty(searchCriteria.ActionTypeDelegate))
                {
                    query = query.Where(d => EF.Functions.Like(d.ActionType, searchCriteria.ActionTypeDelegate));
                }
                if (!string.IsNullOrEmpty(searchCriteria.ActionDueDelegate))
                {
                    query = query.Where(d => EF.Functions.Like(d.ActionDue, searchCriteria.ActionDueDelegate));
                }
                if (!string.IsNullOrEmpty(searchCriteria.IndicatorDelegate))
                {
                    query = query.Where(d => EF.Functions.Like(d.Indicator, searchCriteria.IndicatorDelegate));
                }

                if (!string.IsNullOrEmpty(searchCriteria.UserIdDelegate))
                {
                    query = query.Where(d => d.UserId == searchCriteria.UserIdDelegate);
                }
                if (searchCriteria.GroupIdDelegate > 0)
                {
                    query = query.Where(d => d.GroupId == searchCriteria.GroupIdDelegate);
                }
                return query;
            }

        }

        public async Task<DelegationUtilityResultDTO> RunUpdate(string updateMode, int[] ids, string[] delegateTo, string userName, string? fromUser, int fromGroup, bool reassign)
        {
            var idParams = new List<SqlDataRecord>();
            foreach (var item in ids)
            {
                var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.Int) });
                record.SetInt32(0, item);
                idParams.Add(record);
            }

            var delegateToParams = new List<SqlDataRecord>();
            foreach (var item in delegateTo)
            {
                var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.VarChar,500) });
                record.SetString(0, item);
                delegateToParams.Add(record);
            }

            using (SqlCommand cmd = new SqlCommand("procGMDelegationUtility"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@UpdateMode", updateMode);
                cmd.Parameters.AddWithValue("@CreatedBy", userName);
                cmd.Parameters.AddWithValue("@FromUser", fromUser);
                cmd.Parameters.AddWithValue("@FromGroup", fromGroup);
                cmd.Parameters.AddWithValue("@Reassign", reassign);

                cmd.Parameters.AddWithValue("@Ids", idParams).SqlDbType = SqlDbType.Structured;
                cmd.Parameters.AddWithValue("@UserIds", delegateToParams.Count > 0 ? delegateToParams : null).SqlDbType = SqlDbType.Structured;
              
                cmd.Parameters.Add(new SqlParameter("@NewDelegationIds", SqlDbType.VarChar)
                {
                    Direction = ParameterDirection.Output,
                    Size = -1
                });
                cmd.Parameters.Add(new SqlParameter("@DeletedDelegationIds", SqlDbType.VarChar)
                {
                    Direction = ParameterDirection.Output,
                    Size = -1
                });
                await cmd.ExecuteNonQueryAsync();

                var result = new DelegationUtilityResultDTO
                {
                    NewDelegationIds = (cmd.Parameters["@NewDelegationIds"].Value).ToString(),
                    DeletedDelegationIds = (cmd.Parameters["@DeletedDelegationIds"].Value).ToString()
                };
                return result;
            }
        }

        


    }
}
