using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Entities.GeneralMatter;

namespace R10.Core.Interfaces
{
    public interface ISoftDocketService
    {
        Task<CountryApplication?> GetApplication(int appId);
        Task<TmkTrademark?> GetTrademark(int tmkId);
        Task<GMMatter?> GetMatter(int matId);
        Task<Invention?> GetInvention(int invId);
    }
        
}
