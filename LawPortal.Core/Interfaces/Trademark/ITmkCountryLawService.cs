using LawPortal.Core.DTOs;
using LawPortal.Core.Entities;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Core.Interfaces
{
    public interface ITmkCountryLawService
    {
        Task AddCountryLaw(TmkCountryLaw countryLaw);
        Task UpdateCountryLaw(TmkCountryLaw countryLaw);
        Task UpdateCountryLawRemarks(TmkCountryLaw countryLaw);
        Task DeleteCountryLaw(TmkCountryLaw countryLaw);
        Task<bool> HasDesignatedCountries(string country, string caseType);
        Task UpdateChild<T>(string country, string caseType, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : class;
        Task AddChildren<T>(IEnumerable<T> entities) where T : class;
        Task DeleteCountryDue(string country, string caseType, string userName, IEnumerable<TmkCountryDue> deleted);

        List<LookupDTO> GetBasedOnList();
        List<LookupDTO> GetRecurringOptions();
        string GetRecurringDesc(float value);
        Task<List<LookupDTO>> GetFollowupList(string country);

        Task<List<TmkCountryDue>> GetCountryDues(string country, string caseType);
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
