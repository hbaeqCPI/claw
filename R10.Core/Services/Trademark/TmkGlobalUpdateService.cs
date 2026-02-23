using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Trademark;

namespace R10.Core.Services
{
    public class TmkGlobalUpdateService : ITmkGlobalUpdateService
    {
        private readonly IGlobalUpdateRepository _globalUpdateRepository;
        private readonly ITmkGlobalUpdateRepository _tmkGlobalUpdateRepository;

        public TmkGlobalUpdateService(IGlobalUpdateRepository globalUpdateRepository, ITmkGlobalUpdateRepository tmkGlobalUpdateRepository)
        {
            _globalUpdateRepository = globalUpdateRepository;
            _tmkGlobalUpdateRepository = tmkGlobalUpdateRepository;
        }
        public async Task<List<LookupDTO>> GetUpdateFields()
        {
            return await _globalUpdateRepository.GetUpdateFields("T");
        }

        public async Task<IList<GlobalUpdateLookupDTO>> GetFromData(string updateField)
        {
            return await _tmkGlobalUpdateRepository.GetFromData(updateField);
        }

        public async Task<IList<GlobalUpdateLookupDTO>> GetToData(string updateField)
        {
            return await _tmkGlobalUpdateRepository.GetToData(updateField);
        }

        public async Task<(IList<TmkGlobalUpdatePreviewDTO>,int)> GetPreviewList(TmkGlobalUpdateCriteriaDTO searchCriteria, int page, int pageSize)
        {
            return await _tmkGlobalUpdateRepository.GetPreviewList(searchCriteria, page, pageSize);
        }

        public async Task<int> RunUpdate(TmkGlobalUpdateCriteriaDTO searchCriteria)
        {
            return await _tmkGlobalUpdateRepository.RunUpdate(searchCriteria);
        }

        public IQueryable<TmkGlobalUpdateLog> TmkGlobalUpdateLogs
        {
            get { return _tmkGlobalUpdateRepository.TmkGlobalUpdateLogs; }
        }
    }
}
