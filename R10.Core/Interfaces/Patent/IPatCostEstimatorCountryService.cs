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
    public interface IPatCostEstimatorCountryService : IChildEntityService<PatCostEstimator, PatCostEstimatorCountry>
    {
        
    }
}
