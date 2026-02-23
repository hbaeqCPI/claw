using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using R10.Core.Entities;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    public class LocalizationRecordsManager : ILocalizationRecordsManager
    {
        private readonly IApplicationDbContext _repository;

        public LocalizationRecordsManager(IApplicationDbContext repository)
        {
            _repository = repository;            
        }

        public IQueryable<LocalizationRecords> LocalizationRecords => _repository.LocalizationRecords;
        public IQueryable<LocalizationRecordsGrouping> LocalizationRecordsGrouping => _repository.LocalizationRecordsGrouping;

        public async Task<List<string>> GetSystems()
        {
            return await LocalizationRecordsGrouping.Where(t => t.System != null && t.System != "").Select(t => t.System).Distinct().ToListAsync();
        }

        public async Task<List<string>> GetMenuItems(string system)
        {
            return await LocalizationRecordsGrouping.Where(t => t.System == system && t.Menu != null && t.Menu != "").Select(t => t.Menu).Distinct().ToListAsync();
        }

        public void Update(List<LocalizationRecords> translates)
        {
            _repository.LocalizationRecords.UpdateRange(translates);
        }

        public void Add(List<LocalizationRecords> translates)
        {
            _repository.LocalizationRecords.AddRange(translates);
        }

        public async Task Save()
        {
            await _repository.SaveChangesAsync();
        }
    }
}
