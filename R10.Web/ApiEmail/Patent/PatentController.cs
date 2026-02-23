using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Web.ApiEmail.Models;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Interfaces;
using R10.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Api.EmailApi
{

    [Route("~/emailapi/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = PatentAuthorizationPolicy.FullModify)]
    [EnableCors("EmailAddInCORSPolicy")]
    [ApiController]
    public class PatentController : Microsoft.AspNetCore.Mvc.Controller
    {

        protected readonly IInventionService _inventionService;
        protected readonly ICountryApplicationService _applicationService;
        protected readonly IInventionViewModelService _inventionViewModelService;
        protected readonly ICountryApplicationViewModelService _applicationViewModelService;
        
        protected readonly IActionDueService<PatActionDue, PatDueDate> _actionDueService;
        protected readonly IPatActionDueViewModelService _actionDueViewModelService;

        protected readonly ICostTrackingService<PatCostTrack> _costTrackingService;
        protected readonly IPatCostTrackingViewModelService _costTrackingViewModelService;

        public PatentController(
                IInventionService inventionService,
                ICountryApplicationService applicationService,
                IInventionViewModelService inventionViewModelService,
                ICountryApplicationViewModelService applicationViewModelService,
                IActionDueService<PatActionDue, PatDueDate> actionDueService,
                IPatActionDueViewModelService actionDueViewModelService,
                ICostTrackingService<PatCostTrack> costTrackingService,
                IPatCostTrackingViewModelService costTrackingViewModelService
                )
        {
            _inventionService = inventionService;
            _applicationService = applicationService;
            _inventionViewModelService = inventionViewModelService;
            _applicationViewModelService = applicationViewModelService;
            
            _actionDueService = actionDueService;
            _actionDueViewModelService = actionDueViewModelService;

            _costTrackingService = costTrackingService;
            _costTrackingViewModelService = costTrackingViewModelService;
        }

        // ---------- search picklists
        [HttpGet("getcountry")]
        public async Task<ActionResult<KeyTextDTO[]>> GetCountry()
        {
            var results = await _applicationService.CountryApplications.Select(c => new KeyTextDTO() { key = (c.Country ?? "").ToUpper(), text = c.Country })
                                    .Distinct().OrderBy(r => r.key).ToArrayAsync();
            return results;
        }

        [HttpGet("getcasetype")]
        public async Task<ActionResult<KeyTextDTO[]>> GetCaseType()
        {
            var results = await _applicationService.CountryApplications.Select(c => new KeyTextDTO() { key = (c.CaseType ?? "").ToUpper(), text = c.CaseType })
                                    .Distinct().OrderBy(r => r.key).ToArrayAsync();
            return results;
        }

        [HttpGet("getstatus")]
        public async Task<ActionResult<KeyTextDTO[]>> GetStatus()
        {
            var results = await _applicationService.CountryApplications.Select(c => new KeyTextDTO() { key = (c.ApplicationStatus ?? "").ToUpper(), text = c.ApplicationStatus })
                                    .Distinct().OrderBy(r => r.key).ToArrayAsync();
            return results;
        }

        [HttpGet("getactiontype")]
        public async Task<ActionResult<KeyTextDTO[]>> GetActionType()
        {
            var results = await _actionDueService.QueryableList.Select(ad => new KeyTextDTO() { key = (ad.ActionType ?? "").ToUpper(), text = ad.ActionType })
                                    .Distinct().OrderBy(r => r.key).ToArrayAsync();
            return results;
        }

        [HttpGet("getcosttype")]
        public async Task<ActionResult<KeyTextDTO[]>> GetCostType()
        {
            var results = await _costTrackingService.QueryableList.Select(ad => new KeyTextDTO() { key = (ad.CostType ?? "").ToUpper(), text = ad.CostType })
                                    .Distinct().OrderBy(r => r.key).ToArrayAsync();
            return results;
        }


        // ---------- search results
        [HttpGet("getctryapp")]
        public async Task<ApiEmailResult> GetPagedApp(int pageNo, int pageSize, string? caseNumber, string? country, string? caseType, string? status, string? clientRefNumber)
        {
            var config = new MapperConfiguration(cfg =>
               cfg.CreateMap<CountryApplication, KeyTextDTO>()
               .ForMember(vm => vm.text, domain => domain.MapFrom(c => c.CaseNumber + "/" + c.Country + (string.IsNullOrEmpty(c.SubCase) ? "" : "-" + c.SubCase) + "/" + c.CaseType))
               .ForMember(vm => vm.key, domain => domain.MapFrom(c => "AppId|" + c.AppId.ToString())));

            var ctryApp = _applicationViewModelService.AddCriteria( _applicationService.CountryApplications, BuildAppFilters(caseNumber, country, caseType, status, clientRefNumber))
                                .OrderBy(a => a.CaseNumber).ThenBy(a => a.Country).ThenBy(a => a.SubCase);
            var data = await ctryApp.ApplyPaging(pageNo, pageSize).ProjectTo<KeyTextDTO>(config).ToArrayAsync();
            var rowCount = await ctryApp.CountAsync();

            var results = new ApiEmailResult()
            {
                Results = data,
                PageCount = (int)Math.Ceiling((double)rowCount / pageSize),
                RowCount = rowCount
            };
            return results;
        }

        [HttpGet("getinv")]
        public async Task<ApiEmailResult> GetPagedInv(string? caseNumber, string? familyNumber, string? invTitle, string? clientRefNumber, int pageNo, int pageSize)
        {
            // NOTE: there is no auto-mapping between axios library in react and this controller; make sure the order of the fields match
            var config = new MapperConfiguration(cfg =>
               cfg.CreateMap<Invention, KeyTextDTO>()
               .ForMember(vm => vm.text, domain => domain.MapFrom(i => i.CaseNumber + (string.IsNullOrEmpty(i.FamilyNumber) ? "" : " (" + i.FamilyNumber + ")")))
               .ForMember(vm => vm.key, domain => domain.MapFrom(i => "InvId|" + i.InvId.ToString())));

            var inv =  _inventionViewModelService.AddCriteria(BuildInvFilters(caseNumber, familyNumber, invTitle, clientRefNumber), _inventionService.QueryableList)
                            .OrderBy(i => i.CaseNumber);
            var data = await inv.ApplyPaging(pageNo, pageSize).ProjectTo<KeyTextDTO>(config).ToArrayAsync();
            var rowCount = await inv.CountAsync();

            var results = new ApiEmailResult()
            {
                Results = data,
                PageCount = (int)Math.Ceiling((double)rowCount / pageSize),
                RowCount = rowCount
            };
            return results;
        }

        [HttpGet("getactions")]
        public async Task<ApiEmailResult> GetPagedAction(int pageNo, int pageSize, string? caseNumber, string? country, string? subCase, 
                                                            string? actionType, DateTime? baseDateFrom, DateTime? baseDateTo)
        {
            var config = new MapperConfiguration(cfg =>
               cfg.CreateMap<PatActionDue, KeyTextDTO>()
               .ForMember(vm => vm.text, domain => domain.MapFrom(ad => ad.CaseNumber + "/" + ad.Country + (string.IsNullOrEmpty(ad.SubCase) ? "" : "-" + ad.SubCase) + "/"
                                                                           + ad.ActionType + '/' + ad.BaseDate.ToString("dd-MMM-yyyy")))
               .ForMember(vm => vm.key, domain => domain.MapFrom(dd => "ActId|" + dd.ActId.ToString())));

            var actDue = _actionDueViewModelService.AddCriteria (BuildActionFilters(caseNumber, country, subCase, actionType, baseDateFrom, baseDateTo), _actionDueService.QueryableList)
                                .OrderBy(a => a.CaseNumber).ThenBy(a => a.Country).ThenBy(a => a.SubCase).ThenBy(a => a.ActionType).ThenBy(a => a.BaseDate);
            var data = await actDue.ApplyPaging(pageNo, pageSize).ProjectTo<KeyTextDTO>(config).ToArrayAsync();
            var rowCount = await actDue.CountAsync();

            var results = new ApiEmailResult()
            {
                Results = data,
                PageCount = (int)Math.Ceiling((double)rowCount / pageSize),
                RowCount = rowCount
            };
            return results;
        }

        [HttpGet("getcosts")]
        public async Task<ApiEmailResult> GetPagedCost(int pageNo, int pageSize, string? caseNumber, string? country, string? subCase,
                                                            string? costType, DateTime? invoiceDateFrom, DateTime? invoiceDateTo)
        {
            var config = new MapperConfiguration(cfg =>
               cfg.CreateMap<PatCostTrack, KeyTextDTO>()
               .ForMember(vm => vm.text, domain => domain.MapFrom(ct => ct.CaseNumber + "/" + ct.Country + (string.IsNullOrEmpty(ct.SubCase) ? "" : "-" + ct.SubCase) + "/"
                                                                           + ct.CostType + '/' + ct.InvoiceDate.ToString("dd-MMM-yyyy")))
               .ForMember(vm => vm.key, domain => domain.MapFrom(ct => "CostTrackId|" + ct.CostTrackId.ToString())));

            var costTrack = _costTrackingViewModelService.AddCriteria(BuildCostFilters(caseNumber, country, subCase, costType, invoiceDateFrom, invoiceDateTo), _costTrackingService.QueryableList)
                                .OrderBy(c => c.CaseNumber).ThenBy(c => c.Country).ThenBy(c => c.SubCase).ThenBy(c => c.CostType).ThenBy(c => c.InvoiceDate);
            var data = await costTrack.ApplyPaging(pageNo, pageSize).ProjectTo<KeyTextDTO>(config).ToArrayAsync();
            var rowCount = await costTrack.CountAsync();

            var results = new ApiEmailResult()
            {
                Results = data,
                PageCount = (int)Math.Ceiling((double)rowCount / pageSize),
                RowCount = rowCount
            };
            return results;
        }


        // ---------- search results filter
        private List<QueryFilterViewModel> BuildAppFilters(string? caseNumber, string? country, string? caseType, string? patStatus, string? clientRefNumber)
        {
            var filters = new List<QueryFilterViewModel>();

            if (!string.IsNullOrEmpty(caseNumber))
                filters.Add(new QueryFilterViewModel() { Property = "CaseNumber", Value = caseNumber });
            if (!string.IsNullOrEmpty(country))
            {
                filters.Add(new QueryFilterViewModel() { Property = "Country", Value = country });
                filters.Add(new QueryFilterViewModel() { Property = "CountryOp", Value = "eq" });
            }
            if (!string.IsNullOrEmpty(caseType))
            {
                filters.Add(new QueryFilterViewModel() { Property = "CaseType", Value = caseType });
                filters.Add(new QueryFilterViewModel() { Property = "CaseTypeOp", Value = "eq" });
            }
            if (!string.IsNullOrEmpty(patStatus))
            {
                filters.Add(new QueryFilterViewModel() { Property = "ApplicationStatus", Value = patStatus });
                filters.Add(new QueryFilterViewModel() { Property = "ApplicationStatusOp", Value = "eq" });
            }
            if (!string.IsNullOrEmpty(clientRefNumber))
            {
                filters.Add(new QueryFilterViewModel() { Property = "AppClientRef", Value = clientRefNumber });
            }
            return filters;
        }

        private List<QueryFilterViewModel> BuildInvFilters(string? caseNumber, string? familyNumber, string? invTitle, string? clientRefNumber)
        {
            var filters = new List<QueryFilterViewModel>();

            if (!string.IsNullOrEmpty(caseNumber))
                filters.Add(new QueryFilterViewModel() { Property = "CaseNumber", Value = caseNumber });
            if (!string.IsNullOrEmpty(familyNumber))
                filters.Add(new QueryFilterViewModel() { Property = "FamilyNumber", Value = familyNumber });
            if (!string.IsNullOrEmpty(invTitle))
                filters.Add(new QueryFilterViewModel() { Property = "InvTitle", Value = invTitle });
            if (!string.IsNullOrEmpty(clientRefNumber))
                filters.Add(new QueryFilterViewModel() { Property = "ClientRef", Value = clientRefNumber });

            return filters;
        }

        private List<QueryFilterViewModel> BuildActionFilters(string? caseNumber, string? country, string? subCase, string? actionType, DateTime? baseDateFrom, DateTime? baseDateTo)
        {
            // based on PatActionDueViewModel & Areas/Patent/Views/ActionDue/_SearchTabContent
            var filters = new List<QueryFilterViewModel>();

            if (!string.IsNullOrEmpty(caseNumber))
                filters.Add(new QueryFilterViewModel() { Property = "CaseNumber", Value = caseNumber });
            if (!string.IsNullOrEmpty(country))
            {
                filters.Add(new QueryFilterViewModel() { Property = "Country", Value = country });
                filters.Add(new QueryFilterViewModel() { Property = "CountryOp", Value = "eq" });
            }
            if (!string.IsNullOrEmpty(subCase))
                filters.Add(new QueryFilterViewModel() { Property = "SubCase", Value = subCase });

            if (!string.IsNullOrEmpty(actionType))
                filters.Add(new QueryFilterViewModel() { Property = "ActionType", Value = actionType });

            if (baseDateFrom != null)
                filters.Add(new QueryFilterViewModel() { Property = "BaseDateFrom", Value = baseDateFrom.ToString() });
            if (baseDateTo != null)
                filters.Add(new QueryFilterViewModel() { Property = "BaseDateTo", Value = baseDateTo.ToString() });

            return filters;
        }

        private List<QueryFilterViewModel> BuildCostFilters(string? caseNumber, string? country, string? subCase, string? costType, DateTime? invoiceDateFrom, DateTime? invoiceDateTo)
        {
            // based on PatCostTrackViewModel & Areas/Patent/Views/CostTracking/_SearchTabContent
            var filters = new List<QueryFilterViewModel>();

            if (!string.IsNullOrEmpty(caseNumber))
                filters.Add(new QueryFilterViewModel() { Property = "CaseNumber", Value = caseNumber });
            if (!string.IsNullOrEmpty(country))
            {
                filters.Add(new QueryFilterViewModel() { Property = "Country", Value = country });
                filters.Add(new QueryFilterViewModel() { Property = "CountryOp", Value = "eq" });
            }
            if (!string.IsNullOrEmpty(subCase))
                filters.Add(new QueryFilterViewModel() { Property = "SubCase", Value = subCase });

            if (!string.IsNullOrEmpty(costType))
            {
                filters.Add(new QueryFilterViewModel() { Property = "CostType", Value = costType });
                filters.Add(new QueryFilterViewModel() { Property = "CostTypeOp", Value = "eq" });
            }

            if (invoiceDateFrom != null)
                filters.Add(new QueryFilterViewModel() { Property = "InvoiceDateFrom", Value = invoiceDateFrom.ToString() });
            if (invoiceDateTo != null)
                filters.Add(new QueryFilterViewModel() { Property = "InvoiceDateTo", Value = invoiceDateTo.ToString() });

            return filters;
        }
    }
}
