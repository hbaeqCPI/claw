using R10.Core.DTOs;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IDataQueryService
    {

        IQueryable<DataQueryMain> DataQueriesMain { get; }
        IQueryable<DataQueryAllowedFunction> DataQueryAllowedFunction { get; }
        IQueryable<DataQueryTag> DataQueryTags { get; }

        Task<DataQueryMain> GetByIdAsync(int queryId);
        Task<DataQueryMain> GetByNameAsync(string queryName);

        Task Add(DataQueryMain query);

        Task Update(DataQueryMain query);

        Task Delete(DataQueryMain query);

        Task<List<DQMetadataDTO>> GetDQMetadata(string userEmail);

        Task<List<DQMetaRelationsDTO>> GetDQMetadataRelations(string userEmail);

        //Task<DataTable> RunQuery(string sql, string sortExpr, int page, int pageSize, string userName, bool hasEntityFilterOn, bool hasRespOfficeOn);
        DataTable RunQuery(string sql, string sourceTables, string sourceTablesWithAlias, string selectList, string sortExpr, int page, int pageSize, string userName, bool hasEntityFilterOn, bool hasRespOfficeOn);
        DataTable RunCRQuery(string sql, string userId, bool hasEntityFilterOn, bool hasRespOfficeOn);
        int RunQueryCount();
    }
}
