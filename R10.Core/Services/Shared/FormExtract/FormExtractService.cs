using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.FormExtract;
using R10.Core.Entities.Shared;
using R10.Core.Interfaces;

namespace R10.Core.Services.FormExtract
{
    public class FormExtractService : IFormExtractService
    {
        private readonly IApplicationDbContext _repository;
        private readonly ISystemSettings<DefaultSetting> _settings;

        public FormExtractService(
            IApplicationDbContext repository,
            ISystemSettings<DefaultSetting> settings
            )
        {
            _repository = repository;
            _settings = settings;
        }

        public IQueryable<FormSystem> FormSystems => _repository.FormSystems.AsNoTracking();
        public IQueryable<FormSource> FRSources => _repository.FormSources.AsNoTracking();

     
    }
}
