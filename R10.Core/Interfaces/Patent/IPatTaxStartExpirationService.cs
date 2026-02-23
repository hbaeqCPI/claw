using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Core.Interfaces.Patent
{
    public interface IPatTaxStartExpirationService
    {
        Task<PatCountryLawTaxInfoDTO> ComputeTaxStart(int appId);
        Task<PatCountryLawTaxInfoDTO> ComputeExpiration(int appId);

        Task UpdateTaxInfo(TaxInfoUpdateType updateType, int appId, DateTime? taxStartDate, DateTime? expireDate, string updatedBy);

    }
}
