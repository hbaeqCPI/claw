using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;

namespace R10.Core.Services
{
    public class PatGlobalUpdateService : IPatGlobalUpdateService
    {
        private readonly IGlobalUpdateRepository _globalUpdateRepository;
        private readonly IPatGlobalUpdateRepository _patGlobalUpdateRepository;

        public PatGlobalUpdateService(IGlobalUpdateRepository globalUpdateRepository, IPatGlobalUpdateRepository patGlobalUpdateRepository)
        {
            _globalUpdateRepository = globalUpdateRepository;
            _patGlobalUpdateRepository = patGlobalUpdateRepository;
        }
        public async Task<List<LookupDTO>> GetUpdateFields()
        {
            return await _globalUpdateRepository.GetUpdateFields("P");
        }

        public async Task<IList<GlobalUpdateLookupDTO>> GetFromData(string updateField)
        {
            return await _patGlobalUpdateRepository.GetFromData(updateField);
        }

        public async Task<IList<GlobalUpdateLookupDTO>> GetToData(string updateField)
        {
            return await _patGlobalUpdateRepository.GetToData(updateField);
        }

        public async Task<(IList<PatGlobalUpdatePreviewDTO>,int)> GetPreviewList(PatGlobalUpdateCriteriaDTO searchCriteria, int page, int pageSize)
        {
            return await _patGlobalUpdateRepository.GetPreviewList(searchCriteria, page, pageSize);
        }

        public async Task<int> RunUpdate(PatGlobalUpdateCriteriaDTO searchCriteria)
        {
            return await _patGlobalUpdateRepository.RunUpdate(searchCriteria);
        }

        public IQueryable<PatGlobalUpdateLog> PatGlobalUpdateLogs
        {
            get { return _patGlobalUpdateRepository.PatGlobalUpdateLogs; }
        }
    }
}
