using LawPortal.Core.DTOs;
using LawPortal.Core.Entities.Trademark;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface ITmkDesignationRepository      //: IEntityFilterRepository 
    {

        // below are defined in TmkTrademarkService
        //Task<bool> CanHaveDesignatedCountry(string country, string caseType);
        //Task<List<PatParentCaseDTO>> GetPossibleFamilyReferences(int tmkId, string trademarkName);

        Task<object[]> GetSelectableDesignatedCountries(string country, string caseType, int tmkId);
        Task<object[]> GetSelectableDesignatedCountriesMultiple(string country, string caseType);
        Task<string[]> GetSelectableDesignatedCaseTypes(string country, string caseType, string desCountry);
        Task DesignateCountries(int tmkId, bool fromCountryLaw, string createdBy);
        Task<List<TmkDesignatedCountry>> GetSelectableCountries(int tmkId);
        Task GenerateTrademarks(int parentTmkId, string desCountries, string updatedBy);
        Task<TmkDesignatedCountry> GetDesignatedCountry(int desId);
        Task<List<TmkDesignatedCountry>> GetDesignatedCountries(int tmkId);
        Task DesignatedCountriesUpdate(int tmkId, string userName, IEnumerable<TmkDesignatedCountry> updatedDesignatedCountries,
                IEnumerable<TmkDesignatedCountry> newDesignatedCountries, IEnumerable<TmkDesignatedCountry> deletedDesignatedCountries);
        Task DesignatedCountriesDelete(TmkDesignatedCountry deletedDesignatedCountry);

    }
}
