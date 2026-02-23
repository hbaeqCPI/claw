using R10.Core.DTOs;
using R10.Core.Entities.Trademark;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface ITLUpdateLookupService
    {
        IQueryable<LookupDTO> GetClientList<T>() where T : TMSEntityFilter;
        IQueryable<LookupDTO> GetCountryList<T>() where T : TMSEntityFilter;
        IQueryable<T> TLUpdates<T>() where T : TMSEntityFilter;
        Task<IQueryable<TLActionComparePTO>> TLActionUpdates();

    }
}
