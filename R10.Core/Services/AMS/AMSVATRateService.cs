using R10.Core.Entities.AMS;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.AMS
{
    public class AMSVATRateService : EntityService<AMSVATRate>, IAMSVATRateService
    {
        protected readonly ISystemSettings<AMSSetting> _amsSettings;

        public AMSVATRateService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, ISystemSettings<AMSSetting> amsSettings) : base(cpiDbContext, user)
        {
            _amsSettings = amsSettings;
        }

        public override async Task Add(AMSVATRate entity)
        {
            await base.Add(entity);
            await RecalculateVATRate();
        }

        public override async Task Update(AMSVATRate entity)
        {
            await base.Update(entity);
            await RecalculateVATRate();
        }

        public override async Task Delete(AMSVATRate entity)
        {
            await base.Delete(entity);
            await RecalculateVATRate();
        }

        private async Task RecalculateVATRate()
        {
            await RecalculateVATRate(string.Empty);
        }

        public async Task RecalculateVATRate(string clientCode)
        {
            var settings = await _amsSettings.GetSetting();

            if (settings.HasVAT)
            {
                using (SqlCommand cmd = new SqlCommand("procAMSComputeVAT"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Connection = new SqlConnection(_cpiDbContext.GetDbConnection().ConnectionString);
                    if (cmd.Connection?.State == ConnectionState.Closed)
                        cmd.Connection.Open();

                    if (!string.IsNullOrEmpty(clientCode))
                        cmd.Parameters.Add(new SqlParameter("@Client", clientCode));

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
