using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.AMS
{
    public interface IAMSFeeService : IParentEntityService<AMSFee, AMSFeeDetail>
    {
        Task RecalculateServiceFee(string? feeSetupName, string? clientCode);
        Task RecalculateServiceFee(string? feeSetupName);
        Task RecalculateServiceFee(int dueId);
    }
}
