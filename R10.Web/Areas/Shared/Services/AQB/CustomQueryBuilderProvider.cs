using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using R10.Core.DTOs;
using R10.Core.Interfaces;
using R10.Core.Services.Shared;
using R10.Web.Models;

namespace R10.Web.Areas.Shared.Services.AQB
{

    public class CustomQueryBuilderProvider : IQueryBuilderProvider, ICustomQueryBuilderProvider
    {
        public bool SaveState { get; private set; } = true;

        private readonly IDataQueryService _dataQueryService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IDistributedCache _cache;
        private readonly IStringLocalizer<SharedResource> _localizer;

        //static QueryBuilder? qb = null;      // 4-nov-2022 – make singleton to avoid RunQuery issue

        // 28-nov-2022 – make singletons to avoid RunQuery issue
        //static List<Core.DTOs.DQMetadataDTO>? dqTables = null;
        //static List<Core.DTOs.DQMetaRelationsDTO>? dqRelations = null;

        public CustomQueryBuilderProvider(IHttpContextAccessor contextAccessor, 
                                          IDataQueryService dataQueryService,
                                          IDistributedCache cache,
                                          IStringLocalizer<SharedResource> localizer
                                          )
        {
            _contextAccessor = contextAccessor;
            _dataQueryService = dataQueryService;
            _cache = cache;
            _localizer = localizer;
        }

        //public QueryBuilder Get(string id)
        //{
        //    if (qb == null)
        //    {
        //        qb = Create(id);

        //        var email = id.Split('|')[1];
        //        LoadMetadata(qb, email);
        //    }
        //    LoadState(qb);
        //    return qb;
        //}

        public QueryBuilder Get(string id)
        {
            var qb = Create(id);

            var email = id.Split('|')[1];
            LoadMetadata(qb, email);
            LoadState(qb);
            return qb;
        }

        private QueryBuilder Create(string tag)
        {
            return new QueryBuilder
            {
                SyntaxProvider = new MSSQLSyntaxProvider(),
                BehaviorOptions =
                {
                    AllowSleepMode = true
                },
                MetadataLoadingOptions =
                {
                    OfflineMode = true
                },
                Tag = tag
            };
        }

        private void LoadMetadata(QueryBuilder queryBuilder,string email)
        {
            MetadataItem db = queryBuilder.MetadataContainer.AddDatabase(_localizer["CPiDB"]);
            db.Default = true;
            MetadataObject metaTable = null;

            string svTableName = "";

            //var dqTables = _dataQueryService.GetDQMetadata(email).GetAwaiter().GetResult();                          // get tables/views list
            //var dqRelations = _dataQueryService.GetDQMetadataRelations(email).GetAwaiter().GetResult();              // get relationships list for tables

            //if (dqTables == null)
            //    dqTables = _dataQueryService.GetDQMetadata(email).GetAwaiter().GetResult();                          // get tables/views list

            //if (dqRelations == null)
            //    dqRelations = _dataQueryService.GetDQMetadataRelations(email).GetAwaiter().GetResult();              // get relationships list for tables

            //get metadata from cache
            var cachedTables = _cache.GetString(GetTablesCacheKey(email));
            var cachedRelations = _cache.GetString(GetRelationsCacheKey(email));

            if (cachedTables == null || cachedRelations == null)
                throw new Exception("Session has expired. Please reload the page and try again.");

            var dqTables = JsonConvert.DeserializeObject<List<DQMetadataDTO>>(cachedTables);
            var dqRelations = JsonConvert.DeserializeObject<List<DQMetaRelationsDTO>>(cachedRelations);

            // add tables/views & columns
            foreach (var row in dqTables)
            {
                if (svTableName != row.TableName)
                {
                    svTableName = row.TableName;
                    metaTable = db.AddObject(row.TableName, row.ObjectType == "T" ? MetadataType.Table : MetadataType.View);

                    metaTable.AltName = row.TableAlias;
                    metaTable.Visible = row.Visible;
                }
                MetadataField field = metaTable.AddField(row.ColumnName);
                field.FieldTypeName = row.DataType;
                field.Size = row.DataSize;
            }


            // add relationships
            foreach (var rel in dqRelations)
            {
                MetadataObject parentTable = queryBuilder.MetadataContainer.FindItem<MetadataObject>(rel.ParentTable);
                //MetadataItem parentTable = db.FindItem<MetadataItem>(rel.ParentTable);

                MetadataQualifiedName childTable = new MetadataQualifiedName();
                childTable.Add(rel.ChildTable);

                MetadataForeignKey relation = parentTable.AddForeignKey(rel.FKeyName);
                relation.ReferencedObjectName = childTable;

                relation.Fields.Add(rel.ParentKey);
                relation.ReferencedFields.Add(rel.ChildKey);
                relation.Cardinality = MetadataForeignKeyCardinality.Many;
            }
        }

        private void LoadState(QueryBuilder qb)
        {
            var state = GetState(qb.Tag);

            if (!string.IsNullOrEmpty(state))
                qb.LayoutSQL = state;
        }

        public void Put(QueryBuilder qb)
        {
            SetState(qb.LayoutSQL,qb.Tag);
        }

        public void Delete(string id)
        {

        }

        private string GetState(string tag)
        {
            var clientIP = GetClientIP();
            var email = tag.Split('|')[1];
            var cacheId = $"{email}|{clientIP}";
            return _cache.GetString(cacheId);
        }

        private void SetState(string state,string tag)
        {
            var clientIP = GetClientIP();
            var email = tag.Split('|')[1];
            var cacheId = $"{email}|{clientIP}";
            _cache.SetString(cacheId, state);
        }

        private string? GetClientIP()
        {
            var context =  _contextAccessor.HttpContext;
            var clientIP = context.Connection.RemoteIpAddress?.ToString();
            return clientIP;
        }

        public async Task InitializeMetadata(string email)
        {
            var dqTables = await _dataQueryService.GetDQMetadata(email);                          // get tables/views list
            var dqRelations = await _dataQueryService.GetDQMetadataRelations(email);              // get relationships list for tables

            //save metadata to cache
            _cache.SetString(GetTablesCacheKey(email), JsonConvert.SerializeObject(dqTables));
            _cache.SetString(GetRelationsCacheKey(email), JsonConvert.SerializeObject(dqRelations));
        }

        private string GetTablesCacheKey(string email) => $"{email}-dqTables";
        private string GetRelationsCacheKey(string email) => $"{email}-dqRelations";
    }

    public interface ICustomQueryBuilderProvider
    {
        Task InitializeMetadata(string email);
    }
}
