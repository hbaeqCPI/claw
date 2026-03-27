using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Web.Helpers;
using R10.Web.Security;
using R10.Web.Extensions;
using R10.Web.Interfaces;
using AutoMapper.QueryableExtensions;
using R10.Web.Areas.Shared.ViewModels;
using R10.Core.Entities.Trademark;
using R10.Core.DTOs;
using R10.Core.Helpers;
using R10.Core.Entities;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class CountryController : BaseController
    {
        private readonly ICountryLookupViewModelService _countryLookupService;
        private readonly IEntityService<TmkCountry> _tmkCountryService;
        private readonly IEntityService<PatCountry> _patCountryService;

        public CountryController(ICountryLookupViewModelService countryLookupService, IEntityService<TmkCountry> tmkCountryService, IEntityService<PatCountry> patCountryService)
        {
            _countryLookupService = countryLookupService;
            _tmkCountryService = tmkCountryService;
            _patCountryService = patCountryService;
        }

        public async Task<IActionResult> GetCountryList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            //return await GetPicklistData(_countryLookupService.Countries, request, property, text, filterType, new string[] { "CountryID", "Country", "CountryName" }, requiredRelation);
            return await GetPicklistData(_countryLookupService.Countries, request, property, text, filterType, requiredRelation, false);
        }

        [HttpGet()]
        public IActionResult DetailLink(string id)
        {
            return RedirectToAction("DetailLink", "Country", new { area = _countryLookupService.CountrySource, id = id });
        }

        public async Task<IActionResult> GetCECountryCurrencyList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var countryList = new List<LookupDescDTO>();

            if (User.IsInSystem(SystemType.Patent))
            {
                countryList.AddRange(await _patCountryService.QueryableList.AsNoTracking().Where(d => !string.IsNullOrEmpty(d.Country)).Select(d => new LookupDescDTO() { Text = d.Country, Value = d.CountryName, Description = "" }).ToListAsync());
            }

            if (User.IsInSystem(SystemType.Trademark))
            {
                countryList.AddRange(await _tmkCountryService.QueryableList.AsNoTracking().Where(d => !string.IsNullOrEmpty(d.Country)).Select(d => new LookupDescDTO() { Text = d.Country, Value = d.CountryName, Description = "" }).ToListAsync());
            }

            return Json(countryList.DistinctBy(d => new { d.Text, d.Value, d.Description })
                .Where(d => string.IsNullOrEmpty(text) || (!string.IsNullOrEmpty(d.Text) && d.Text.StartsWith(text, StringComparison.OrdinalIgnoreCase))).ToList());
        }
    }
}