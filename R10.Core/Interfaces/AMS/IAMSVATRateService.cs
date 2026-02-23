using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.AMS
{
    public interface IAMSVATRateService : IEntityService<AMSVATRate>
    {
        Task RecalculateVATRate(string clientCode);
    }
}
