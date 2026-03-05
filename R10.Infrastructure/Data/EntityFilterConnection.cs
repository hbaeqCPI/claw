using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using R10.Core.Helpers;
using System.Data;

namespace R10.Infrastructure.Data
{
    public class EntityFilterConnection
     : SqlServerConnection
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private bool? _hasRespOffice;
        private bool? _hasEntityFilter;

        public EntityFilterConnection(RelationalConnectionDependencies dependencies, IHttpContextAccessor contextAccessor) : base(dependencies)
        {
            _contextAccessor = contextAccessor;
        }

        public override async Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false)
        {
            //bool setSessionState = this.DbConnection.State != ConnectionState.Open && (HasRespOffice || HasEntityFilter);
            bool setSessionState = this.DbConnection.State != ConnectionState.Open; //call always, needed by audit trail,queries, and letters
            bool opened = await base.OpenAsync(cancellationToken, errorsExpected);
            if (setSessionState && _contextAccessor?.HttpContext?.User?.Identity?.Name !=null)
            {
                SetSessionInfo();
            }
            return opened;
        }

        private bool HasRespOffice {
            get {
                if (_hasRespOffice == null) {
                    _hasRespOffice = _contextAccessor?.HttpContext?.User?.HasRespOfficeFilter();
                }
                return _hasRespOffice ?? false;
            }
        }

        private bool HasEntityFilter {
            get
            {
                if (_hasEntityFilter == null)
                {
                    _hasEntityFilter  = _contextAccessor?.HttpContext?.User?.HasEntityFilter();
                }
                return _hasEntityFilter ?? false;
            }
        }
        

        private void SetSessionInfo()
        {
            using (var cmd = base.DbConnection.CreateCommand())
            {
                cmd.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = base.DbConnection.CreateCommand())
            {
                cmd.Transaction = base.CurrentTransaction?.GetDbTransaction();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "procSysSetSession";
                var userNameParam = cmd.CreateParameter();
                cmd.Parameters.Add(userNameParam);
                userNameParam.ParameterName = "@UserName";
                userNameParam.Value = _contextAccessor?.HttpContext?.User?.Identity?.Name;
                var respOfficeParam = cmd.CreateParameter();
                cmd.Parameters.Add(respOfficeParam);
                respOfficeParam.ParameterName = "@HasRespOfficeOn";
                respOfficeParam.Value = HasRespOffice;
                var entityFilterParam = cmd.CreateParameter();
                cmd.Parameters.Add(entityFilterParam);
                entityFilterParam.ParameterName = "@HasEntityFilterOn";
                entityFilterParam.Value = HasEntityFilter;
                cmd.ExecuteNonQuery();
            }
            
        }

        public override bool Open(bool errorsExpected = false)
        {
            bool setSessionState = this.DbConnection.State != ConnectionState.Open;
            var opened = base.Open(errorsExpected);
            if (setSessionState && _contextAccessor?.HttpContext?.User?.Identity?.Name != null)
            {
                SetSessionInfo();
            }
            return opened;
        }
    }
}
