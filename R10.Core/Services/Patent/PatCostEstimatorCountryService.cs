using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using R10.Core.Entities.Patent;
using System;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using System.Transactions;
using R10.Core.Exceptions;
using System.Security.Claims;

namespace R10.Core.Services
{
    public class PatCostEstimatorCountryService : PatCostEstimatorChildService<PatCostEstimatorCountry>, IPatCostEstimatorCountryService
    {        
        public PatCostEstimatorCountryService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, IPatCostEstimatorService costEstimatorService) : base(cpiDbContext, user, costEstimatorService)
        {
        }

    }
}
