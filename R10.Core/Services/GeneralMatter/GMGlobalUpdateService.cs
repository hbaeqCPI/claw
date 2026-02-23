using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Interfaces;
using R10.Core.Interfaces.GeneralMatter;

namespace R10.Core.Services
{
    public class GMGlobalUpdateService : IGMGlobalUpdateService
    {
        private readonly IGlobalUpdateRepository _globalUpdateRepository;
        private readonly IGMGlobalUpdateRepository _gmGlobalUpdateRepository;

        public GMGlobalUpdateService(IGlobalUpdateRepository globalUpdateRepository, IGMGlobalUpdateRepository gmGlobalUpdateRepository)
        {
            _globalUpdateRepository = globalUpdateRepository;
            _gmGlobalUpdateRepository = gmGlobalUpdateRepository;
        }
        public async Task<List<LookupDTO>> GetUpdateFields()
        {
            return await _globalUpdateRepository.GetUpdateFields("G");
        }

        public async Task<IList<GlobalUpdateLookupDTO>> GetFromData(string updateField)
        {
            return await _gmGlobalUpdateRepository.GetFromData(updateField);
        }

        public async Task<IList<GlobalUpdateLookupDTO>> GetToData(string updateField)
        {
            return await _gmGlobalUpdateRepository.GetToData(updateField);
        }

        public async Task<(IList<GMGlobalUpdatePreviewDTO>,int)> GetPreviewList(GMGlobalUpdateCriteriaDTO searchCriteria, int page, int pageSize)
        {
            return await _gmGlobalUpdateRepository.GetPreviewList(searchCriteria, page, pageSize);
        }

        public async Task<int> RunUpdate(GMGlobalUpdateCriteriaDTO searchCriteria)
        {
            return await _gmGlobalUpdateRepository.RunUpdate(searchCriteria);
        }

        public IQueryable<GMGlobalUpdateLog> GMGlobalUpdateLogs
        {
            get { return _gmGlobalUpdateRepository.GMGlobalUpdateLogs; }
        }
    }
}
