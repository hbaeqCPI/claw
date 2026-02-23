using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.AMS
{
    public interface IAMSMainService : IEntityService<AMSMain>
    {
        bool AllowTaxScheduleEdit(string country);
        Task<bool> IsDataOverride();

        Task Update(AMSMain amsMain, string taxSchedChangeReason);
        Task<List<AMSTaxSchedHistory>> GetTaxScheduleHistory(int annid);

        Task<AMSMain> ValidatePermission(int annId, List<string> roles);
        Task<bool> IsProductsOn();
        Task<bool> CanAccessProducts();
        Task<bool> IsLicenseesOn();
        Task<bool> IsPatentScoreOn();
    }
}
