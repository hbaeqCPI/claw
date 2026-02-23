using Newtonsoft.Json;
using R10.Core.Entities;
using R10.Core.Interfaces;
using System.Threading.Tasks;

namespace R10.Core.Services.Shared
{
    public class ReportParameterService : IReportParameterService
    {
        protected readonly ICPiDbContext _cpiDbContext;
        protected readonly IRepository<ReportParameter> _repository;

        public ReportParameterService(ICPiDbContext cpiDbContext)
        {
            _cpiDbContext = cpiDbContext;
            _repository = _cpiDbContext.GetRepository<ReportParameter>();
        }

        public async Task<T> GetParameter<T>(string id)
        {
            var param = await _repository.GetByIdAsync(id);
            return param == null ? default(T) : JsonConvert.DeserializeObject<T>(param.Parameters);
        }

        public async Task<string> SaveParameter(object value)
        {
            var param = new ReportParameter() { Parameters = JsonConvert.SerializeObject(value) };
            _repository.Add(param);
            await _cpiDbContext.SaveChangesAsync();

            return param.Id;
        }

        public async Task DeleteParameter(string id)
        {
            _repository.Delete(new ReportParameter() { Id = id });
            await _cpiDbContext.SaveChangesAsync();
        }
    }
}
