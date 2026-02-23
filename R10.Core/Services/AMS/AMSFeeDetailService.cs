using R10.Core.Entities.AMS;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.AMS
{
    public class AMSFeeDetailService : ChildEntityService<AMSFee, AMSFeeDetail>, IChildEntityService<AMSFee, AMSFeeDetail>
    {
        protected readonly IAMSFeeService _feeService;

        public AMSFeeDetailService(
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user,
            IAMSFeeService feeService) : base(cpiDbContext, user)
        {
            _feeService = feeService;
        }


        public override async Task Add(AMSFeeDetail entity)
        {
            await base.Add(entity);
            await _feeService.RecalculateServiceFee(entity.FeeSetupName);

        }

        public override async Task Delete(AMSFeeDetail entity)
        {
            await base.Delete(entity);
            await _feeService.RecalculateServiceFee(entity.FeeSetupName);
        }

        public override async Task Update(AMSFeeDetail entity)
        {
            await base.Update(entity);
            await _feeService.RecalculateServiceFee(entity.FeeSetupName);
        }

        public override async Task<bool> Update(object key, string userName, IEnumerable<AMSFeeDetail> updated, IEnumerable<AMSFeeDetail> added, IEnumerable<AMSFeeDetail> deleted)
        {
            //do not save NULL to avoid combobox issues
            foreach(var item in updated)
            {
                item.Country = item.Country ?? "";
                item.CaseType = item.CaseType ?? "";
            }
            foreach (var item in added)
            {
                item.Country = item.Country ?? "";
                item.CaseType = item.CaseType ?? "";
            }

            var success = await base.Update(key, userName, updated, added, deleted);
            if (success)
                await _feeService.RecalculateServiceFee(key.ToString());

            return success;
        }
    }
}
