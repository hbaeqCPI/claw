using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IReportParameterService
    {
        Task<T> GetParameter<T>(string id);
        Task<string> SaveParameter(object value);
        Task DeleteParameter(string id);
    }
}
