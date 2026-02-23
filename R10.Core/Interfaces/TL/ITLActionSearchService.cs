using R10.Core.DTOs;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface ITLActionSearchService
    {
        //Task<TmkTrademark> GetByIdAsync(int tmkId);
        
        IQueryable<TmkTrademark> TmkTrademarks { get; }
        IQueryable<TLSearchAction> SearchActions { get; }
     

    }
}
