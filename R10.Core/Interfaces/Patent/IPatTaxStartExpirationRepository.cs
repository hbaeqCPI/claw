using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;

namespace R10.Core.Interfaces
{
    public interface IPatTaxStartExpirationRepository
    {
        Task<CountryApplication> GetCountryApplicationToCompute(int appId);
        Task<PatCountryLawTaxInfoDTO.UserResponse> CanComputeExpirationBeforeIssue(CountryApplication app);
        Task<PatCountryLawTaxInfoDTO> ComputeExpiration(int appId);
        Task<PatCountryLawTaxInfoDTO> ComputeTaxStart(int appId);

        Task UpdateTaxInfo(TaxInfoUpdateType updateType, int appId, DateTime? taxStartDate, DateTime? expireDate,
            string updatedBy);
    }
}
