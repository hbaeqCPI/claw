using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;

namespace R10.Core.Interfaces
{
    public interface IPatCountryLawService
    {
        Task AddCountryLaw(PatCountryLaw countryLaw);
        Task UpdateCountryLaw(PatCountryLaw countryLaw);
        Task UpdateCountryLawRemarks(PatCountryLaw countryLaw);
        Task DeleteCountryLaw(PatCountryLaw countryLaw);
        Task<bool> HasDesignatedCountries(string country, string caseType);
        Task UpdateChild<T>(int parentId, string country, string caseType, string userName, byte[] tStamp, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task DeleteCountryDue(int parentId, string country, string caseType, string userName, byte[] tStamp, IEnumerable<PatCountryDue> deleted);

        //not needed
        //use BasedOnOption and GetPublicConstantValues type extension
        //List<LookupDTO> GetBasedOnList();

        Task<List<LookupDTO>> GetExpirationTypeList();

        //not needed
        //use RecurringOption enum for Recurring values
        //List<LookupDTO> GetRecurringOptions();

        //not needed
        //use GetDisplayName enum extension to display Recurring description
        //string GetRecurringDesc(short? value);
        Task<List<LookupDTO>> GetFollowupList(string country);

        Task<List<PatCountryDue>> GetCountryDues(int countryLawId);
        Task<PatCountryDue> GetCountryDue(int cDueId);
        Task<List<LookupDTO>> GetActionDues();
        Task<List<LookupDTO>> GetActionTypes();
        Task CountryDueUpdate(PatCountryDue countryDue);
        Task GenerateCountryLawActions(CountryLawRetroParam criteria);
        Task<List<PatCountryExp>> GetCountryExps(int countryLawId);

        IQueryable<PatCountryLaw> PatCountryLaws { get; }
        IQueryable<PatCountryDue> PatCountryDues { get; }
        IQueryable<PatDesCaseType> PatDesCaseTypes { get; }
        IQueryable<PatCaseType> PatCaseTypes { get; }

    }
}
