using R10.Core.Entities.ForeignFiling;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.ForeignFiling
{
    public interface IFFDueCountryService : IEntityService<FFDueCountry>
    {
        IQueryable<FFDueCountry> GenAppList { get; }
        IQueryable<PatDesCaseType> DesCaseTypeList { get; }
        IQueryable<PatCountry> CountryList { get; }
        IQueryable<PatCountryLaw> CaseTypeList { get; }

        Task<byte[]> SaveExclude(int id, bool exclude, byte[] tStamp, string userName);
        Task<byte[]> SaveGenApp(int id, bool genApp, byte[] tStamp, string userName);

        Task<int> GenerateApplication(CountryApplication countryApplication, FFDueCountry updated);
        Task SaveUpdateDate(FFDueCountry updated);
    }
}
