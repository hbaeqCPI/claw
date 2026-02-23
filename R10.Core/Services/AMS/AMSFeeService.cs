using R10.Core.Entities.AMS;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.AMS
{
    public class AMSFeeService : AuxService<AMSFee>, IAMSFeeService
    {
        protected readonly ISystemSettings<AMSSetting> _amsSettings;

        public AMSFeeService(
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user,
            ISystemSettings<AMSSetting> amsSettings) : base(cpiDbContext, user)
        {
            _amsSettings = amsSettings;
            ChildService = new AMSFeeDetailService(_cpiDbContext, _user, this);
        }

        public IChildEntityService<AMSFee, AMSFeeDetail> ChildService { get; }

        public async Task RecalculateServiceFee(string? feeSetupName)
        {
            await RecalculateServiceFee(feeSetupName, null);
        }

        public async Task RecalculateServiceFee(string? feeSetupName, string? clientCode)
        {
            await RecalculateServiceFee(feeSetupName, clientCode, 0);
        }

        public async Task RecalculateServiceFee(int dueId)
        {
            await RecalculateServiceFee(null, null, dueId);
        }

        private async Task RecalculateServiceFee(string? feeSetupName, string? clientCode, int dueId)
        {
            var settings = await _amsSettings.GetSetting();

            if (settings.HasServiceFee)
            {
                using (var cmd = new SqlCommand("procAMSGetServiceFee"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Connection = new SqlConnection(_cpiDbContext.GetDbConnection().ConnectionString);
                    if (cmd.Connection?.State == ConnectionState.Closed)
                        cmd.Connection.Open();

                    cmd.Parameters.Add(new SqlParameter("@bolUpdateAMSDueTable", true));

                    if (!string.IsNullOrEmpty(feeSetupName))
                        cmd.Parameters.Add(new SqlParameter("@FeeSetupName", feeSetupName));

                    if (!string.IsNullOrEmpty(clientCode))
                        cmd.Parameters.Add(new SqlParameter("@Client", clientCode));

                    if (dueId > 0)
                        cmd.Parameters.Add(new SqlParameter("@DueId", dueId));

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
