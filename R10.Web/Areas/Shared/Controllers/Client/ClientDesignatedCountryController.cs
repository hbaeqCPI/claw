using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Filters;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Security;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class ClientDesignatedCountryController : BaseController
    {
        private readonly IClientDesignatedCountryService _service;
        private readonly IBaseService<TmkDesCaseType> _tmkDesCaseTypeService;
        private readonly IBaseService<PatDesCaseType> _patDesCaseTypeService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ClientDesignatedCountryController(
            IClientDesignatedCountryService service, 
            IBaseService<PatDesCaseType> patDesCaseTypeService,
            IBaseService<TmkDesCaseType> tmkDesCaseTypeService,
            IMapper mapper,
            IStringLocalizer<SharedResource> localizer)
        {
            _service = service;
            _patDesCaseTypeService = patDesCaseTypeService;
            _tmkDesCaseTypeService = tmkDesCaseTypeService;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<IActionResult> DesignatedCountryRead([DataSourceRequest] DataSourceRequest request, int clientId)
        {
            var result = await _service.QueryableList.Where(c => c.ClientID == clientId && (c.ParentDesCtryID ?? 0) == 0).ProjectTo<ClientDesignatedCountryViewModel>().ToListAsync();
            //Disable system not assigned to user
            result.ForEach(vm => vm.ReadOnly = !User.GetSystems().Any(s =>  s== vm.SystemTypeName));

            return Json(result.ToDataSourceResult(request));
        }
        
        public async Task<IActionResult> ChildDesignatedCountryRead([DataSourceRequest] DataSourceRequest request, int parentId)
        {
            var result = await _service.QueryableList.Where(c => c.ParentDesCtryID == parentId).ProjectTo<ClientDesignatedCountryViewModel>().ToListAsync();
            //Disable system not assigned to user
           result.ForEach(vm => vm.ReadOnly = !User.GetSystems().Any(s => s == vm.SystemTypeName));
            return Json(result.ToDataSourceResult(request));
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> DesignatedCountriesUpdate(int clientId, 
            [Bind(Prefix = "updated")]IEnumerable<ClientDesignatedCountryViewModel> updated,
            [Bind(Prefix = "new")]IEnumerable<ClientDesignatedCountryViewModel> added, 
            [Bind(Prefix = "deleted")]IEnumerable<ClientDesignatedCountryViewModel> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _service.Update(clientId, User.GetUserName(),
                    _mapper.Map<List<ClientDesignatedCountry>>(updated),
                    _mapper.Map<List<ClientDesignatedCountry>>(added),
                    _mapper.Map<List<ClientDesignatedCountry>>(deleted)
                    );
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer["Designated Country has been saved successfully."].ToString() :
                    _localizer["Designated Countries have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> DesignatedCountryDelete([Bind(Prefix = "deleted")] ClientDesignatedCountryViewModel deleted)
        {
            if (deleted.EntityDesCtryID >= 0 && deleted.ClientID != 0)
            {
                await _service.Update(deleted.ClientID, User.GetUserName(), new List<ClientDesignatedCountry>(), new List<ClientDesignatedCountry>(), new List<ClientDesignatedCountry>() { _mapper.Map<ClientDesignatedCountry>(deleted) });
                return Ok(new { success = _localizer["Designated Country has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> ChildDesignatedCountriesUpdate(int parentId, int clientId, string systemType, 
            [Bind(Prefix = "updated")]IEnumerable<ClientDesignatedCountryViewModel> updated,
            [Bind(Prefix = "new")]IEnumerable<ClientDesignatedCountryViewModel> added, 
            [Bind(Prefix = "deleted")]IEnumerable<ClientDesignatedCountryViewModel> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _service.Update(clientId, parentId, systemType, User.GetUserName(),
                    _mapper.Map<List<ClientDesignatedCountry>>(updated),
                    _mapper.Map<List<ClientDesignatedCountry>>(added),
                    _mapper.Map<List<ClientDesignatedCountry>>(deleted)
                    );
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer["Designated Country has been saved successfully."].ToString() :
                    _localizer["Designated Countries have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        public IActionResult GetActiveSystemsWithDesignatedCountry() {
            var systems = User.GetSystemTypes()
                        .Where(s => _service.ValidSystems.Any(v => v.ToLower() == s.SystemId.ToLower()))
                        .Select(s=> new {SystemType=s.TypeId,SystemTypeName=s.SystemId})
                        .ToList();
            return Json(systems);
        }

        public async Task<IActionResult> GetParentCountryList([DataSourceRequest] DataSourceRequest request, string systemType, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            IQueryable<CountryLookupViewModel> countries;
            if (User.GetSystemTypes().Any(s => s.TypeId == systemType))
            {
                if (systemType == "P")
                    countries = _patDesCaseTypeService.QueryableList.Select(c => new CountryLookupViewModel() { Country = c.IntlCode, CountryName = c.ParentCountry.CountryName });
                else
                    countries = _tmkDesCaseTypeService.QueryableList.Select(c => new CountryLookupViewModel() { Country = c.IntlCode, CountryName = c.ParentCountry.CountryName });

                return await GetPicklistData(countries, request, property, text, filterType, requiredRelation, false);
            }
            return new JsonBadRequest(_localizer["Invalid request"].ToString());
        }

        public async Task<IActionResult> GetParentCaseTypeList([DataSourceRequest] DataSourceRequest request, string systemType, string parentCountry, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            IQueryable<CaseTypeLookupViewModel> caseTypes;
            if (User.GetSystemTypes().Any(s => s.TypeId == systemType))
            {
                if (systemType == "P")
                    caseTypes = _patDesCaseTypeService.QueryableList.Where(c => c.IntlCode == parentCountry).Select(c => new CaseTypeLookupViewModel() { CaseType = c.CaseType, Description = c.ParentCaseType.Description });
                else
                    caseTypes = _tmkDesCaseTypeService.QueryableList.Where(c => c.IntlCode == parentCountry).Select(c => new CaseTypeLookupViewModel() { CaseType = c.CaseType, Description = c.ParentCaseType.Description });

                return await GetPicklistData(caseTypes, request, property, text, filterType, requiredRelation, false);
            }
            return new JsonBadRequest(_localizer["Invalid request"].ToString());
        }

        public async Task<IActionResult> GetChildCountryList([DataSourceRequest] DataSourceRequest request, string systemType, string parentCountry, string parentCaseType, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            IQueryable<CountryLookupViewModel> countries;
            if (User.GetSystemTypes().Any(s => s.TypeId == systemType))
            {
                if (systemType == "P")
                    countries = _patDesCaseTypeService.QueryableList.Where(c => c.IntlCode == parentCountry && c.CaseType == parentCaseType).Select(c => new CountryLookupViewModel() { Country = c.DesCountry, CountryName = c.ChildCountry.CountryName, DesCaseType = c.DesCaseType });
                else
                    countries = _tmkDesCaseTypeService.QueryableList.Where(c => c.IntlCode == parentCountry && c.CaseType == parentCaseType).Select(c => new CountryLookupViewModel() { Country = c.DesCountry, CountryName = c.ChildCountry.CountryName, DesCaseType = c.DesCaseType });

                return await GetPicklistData(countries, request, property, text, filterType, requiredRelation, false);
            }
            return new JsonBadRequest(_localizer["Invalid request"].ToString());
        }

        public async Task<IActionResult> GetChildCaseTypeList([DataSourceRequest] DataSourceRequest request, string systemType, string parentCountry, string parentCaseType, string desCountry, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            IQueryable<CaseTypeLookupViewModel> caseTypes;
            if (User.GetSystemTypes().Any(s => s.TypeId == systemType))
            {
                if (systemType == "P")
                    caseTypes = _patDesCaseTypeService.QueryableList.Where(c => c.IntlCode == parentCountry && c.CaseType == parentCaseType && c.DesCountry == desCountry).Select(c => new CaseTypeLookupViewModel() { CaseType = c.DesCaseType, Description = c.ParentCaseType.Description });
                else
                    caseTypes = _tmkDesCaseTypeService.QueryableList.Where(c => c.IntlCode == parentCountry && c.CaseType == parentCaseType && c.DesCountry == desCountry).Select(c => new CaseTypeLookupViewModel() { CaseType = c.DesCaseType, Description = c.ParentCaseType.Description });

                return await GetPicklistData(caseTypes, request, property, text, filterType, requiredRelation, false);
            }
            return new JsonBadRequest(_localizer["Invalid request"].ToString());
        }
    }
}