using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IRTSActionSearchService
    {
        
        IQueryable<CountryApplication> CountryApplications { get; }
        IQueryable<RTSSearchAction> SearchActions { get; }
     

    }
}
