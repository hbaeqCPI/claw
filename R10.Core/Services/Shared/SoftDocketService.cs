using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using R10.Core.Identity;
using System.Linq.Expressions;
using System;
using System.Data;
using R10.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using System.Text.RegularExpressions;


namespace R10.Core.Services
{
    public class SoftDocketService : ISoftDocketService
    {
        private readonly IApplicationDbContext _repository;

        public SoftDocketService(IApplicationDbContext repository)
        {
            _repository = repository;
        }

        public async Task<CountryApplication?> GetApplication(int appId) {
            return await _repository.CountryApplications
                .Include(ca=> ca.Invention)
                .ThenInclude(i => i.Attorney1)
                .Include(ca => ca.Invention)
                .ThenInclude(i => i.Attorney2)
                .Include(ca => ca.Invention)
                .ThenInclude(i => i.Attorney3)
                .Include(ca => ca.Invention)
                .ThenInclude(i => i.Attorney4)
                .Include(ca => ca.Invention)
                .ThenInclude(i => i.Attorney5)
                .FirstOrDefaultAsync(r=> r.AppId==appId);
        }

        public async Task<TmkTrademark?> GetTrademark(int tmkId) {
            return await _repository.TmkTrademarks
                .Include(tmk => tmk.Attorney1)
                .Include(tmk => tmk.Attorney2)
                .Include(tmk => tmk.Attorney3)
                .Include(tmk => tmk.Attorney4)
                .Include(tmk => tmk.Attorney5)
                .FirstOrDefaultAsync(r => r.TmkId == tmkId);
        }
        public async Task<GMMatter?> GetMatter(int matId) {
            return await _repository.GMMatters.FirstOrDefaultAsync(r => r.MatId == matId);
        }

        public async Task<Invention?> GetInvention(int invId)
        {
            return await _repository.Inventions
                .Include(i=> i.Attorney1)
                .Include(i => i.Attorney2)
                .Include(i => i.Attorney3)
                .Include(i => i.Attorney4)
                .Include(i => i.Attorney5)
                .FirstOrDefaultAsync(r => r.InvId == invId);
        }
    }
}
