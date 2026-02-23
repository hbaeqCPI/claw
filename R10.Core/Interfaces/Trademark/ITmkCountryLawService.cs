using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.Entities.Trademark;

namespace R10.Core.Interfaces
{
    public interface ITmkCountryLawService
    {
        Task AddCountryLaw(TmkCountryLaw countryLaw);
        Task UpdateCountryLaw(TmkCountryLaw countryLaw);
        Task UpdateCountryLawRemarks(TmkCountryLaw countryLaw);
        Task DeleteCountryLaw(TmkCountryLaw countryLaw);
        Task UpdateChild<T>(int parentId, string country, string caseType, string userName, byte[] tStamp, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task DeleteCountryDue(int parentId, string country, string caseType, string userName, byte[] tStamp, IEnumerable<TmkCountryDue> deleted);

        Task<bool> HasDesignatedCountries(string country, string caseType);
        List<LookupDTO> GetBasedOnList();
        List<LookupDTO> GetRecurringOptions();
        string GetRecurringDesc(short? value);
        Task<List<LookupDTO>> GetFollowupList(string country);

        Task<List<TmkCountryDue>> GetCountryDues(int countryLawId);
        Task<TmkCountryDue> GetCountryDue(int cDueId);
        Task<List<LookupDTO>> GetActionDues();
        Task<List<LookupDTO>> GetActionTypes();
        Task CountryDueUpdate(TmkCountryDue countryDue);
        //Task CountryDuesUpdate(int parentId, string userName, byte[] tStamp,
        //    IList<TmkCountryDue> updatedCountryDues, IList<TmkCountryDue> newCountryDues,
        //    IList<TmkCountryDue> deletedCountryDues);
        //Task CountryDueDelete(TmkCountryDue deletedCountryDue);
        Task GenerateCountryLawActions(CountryLawRetroParam criteria);

        //Task DesCaseTypesUpdate(int parentId, string userName, byte[] tStamp, IList<TmkDesCaseType> updatedDesCaseTypes,
        //    IList<TmkDesCaseType> newDesCaseTypes, IList<TmkDesCaseType> deletedDesCaseTypes);
        //Task DesCaseTypeDelete(TmkDesCaseType deletedDesCaseType);

        IQueryable<TmkCountryLaw> TmkCountryLaws { get; }
        IQueryable<TmkCountryDue> TmkCountryDues { get; }
        IQueryable<TmkDesCaseType> TmkDesCaseTypes { get; }
        IQueryable<TmkCaseType> TmkCaseTypes { get; }
    }
}
