using LawPortal.Core.DTOs;
using LawPortal.Core.Entities.Shared;
using LawPortal.Core.Entities.Trademark;
using LawPortal.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Services
{
    public class TmkDesignationService : ITmkDesignationService             // , BaseService
    {
        public readonly ITmkDesignationRepository _designationRepository;
        private readonly ISystemSettings<DefaultSetting> _settings;
        //private readonly ClaimsPrincipal _user;

        public TmkDesignationService(ITmkDesignationRepository designationRepository,
                                     ISystemSettings<DefaultSetting> settings)
        {
            _designationRepository = designationRepository;
            _settings = settings;
         //   _user = user;
        }

        public async Task<object[]> GetSelectableDesignatedCountries(string country, string caseType, int tmkId)
        {
            var countries = await _settings.GetValue<string>("TMS", "Countries_That_Allow_Multiple_Designation");
            if (!string.IsNullOrEmpty(countries) && countries.Contains($"|{country}|"))
                return await _designationRepository.GetSelectableDesignatedCountriesMultiple(country, caseType);
            else
              return await _designationRepository.GetSelectableDesignatedCountries(country, caseType, tmkId);
        }

        public async Task<string[]> GetSelectableDesignatedCaseTypes(string country, string caseType, string desCountry)
        {
            return await _designationRepository.GetSelectableDesignatedCaseTypes(country, caseType, desCountry);
        }

        public async Task DesignateCountries(int tmkId, bool fromCountryLaw, string createdBy)
        {
            await _designationRepository.DesignateCountries(tmkId, fromCountryLaw, createdBy);
        }

        public async Task<List<TmkDesignatedCountry>> GetSelectableCountries(int tmkId)
        {
            return await _designationRepository.GetSelectableCountries(tmkId);
        }

        public async Task GenerateTrademarks(int parentTmkId, string desCountries, string updatedBy)
        {
            await _designationRepository.GenerateTrademarks(parentTmkId, desCountries, updatedBy);
        }

        public async Task<TmkDesignatedCountry> GetDesignatedCountry(int desId)
        {
            return await _designationRepository.GetDesignatedCountry(desId);
        }

        public async Task<List<TmkDesignatedCountry>> GetDesignatedCountries(int tmkId)
        {
            return await _designationRepository.GetDesignatedCountries(tmkId);
        }

        public async Task DesignatedCountriesUpdate(int tmkId, string userName, IEnumerable<TmkDesignatedCountry> updatedDesignatedCountries, 
                        IEnumerable<TmkDesignatedCountry> newDesignatedCountries, IEnumerable<TmkDesignatedCountry> deletedDesignatedCountries)
        {
            await _designationRepository.DesignatedCountriesUpdate(tmkId, userName, updatedDesignatedCountries, newDesignatedCountries, deletedDesignatedCountries);
        }

        public async Task DesignatedCountriesDelete(TmkDesignatedCountry deletedDesignatedCountry)
        {
            await _designationRepository.DesignatedCountriesDelete(deletedDesignatedCountry);
        }
    }
}
