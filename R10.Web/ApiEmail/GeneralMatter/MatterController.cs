
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Interfaces;
using R10.Web.ApiEmail.Models;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Interfaces;
using R10.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenIddict.Validation.AspNetCore;

namespace R10.Web.ApiEmail.GeneralMatter
{
    [Route("~/emailapi/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = GeneralMatterAuthorizationPolicy.FullModify)]
    [EnableCors("EmailAddInCORSPolicy")]
    [ApiController]
    public class MatterController : Microsoft.AspNetCore.Mvc.Controller
    {
        protected readonly IGMMatterService _gmMatterService;
        protected readonly IGMMatterCountryService _gmCountryService;

        protected readonly IActionDueService<GMActionDue, GMDueDate> _actionDueService;
        protected readonly ICostTrackingService<GMCostTrack> _costTrackingService;

        public MatterController(
                IGMMatterService gmMatterService,
                IGMMatterCountryService gmCountryService,
                IActionDueService<GMActionDue, GMDueDate> actionDueService,
                ICostTrackingService<GMCostTrack> costTrackingService
            )
        {
            _gmMatterService = gmMatterService;
            _gmCountryService = gmCountryService;
            _actionDueService = actionDueService;
            _costTrackingService = costTrackingService;
        }

        // ---------- search picklists
        [HttpGet("getcountry")]
        public async Task<ActionResult<KeyTextDTO[]>> GetCountry()
        {
            var results = await _gmCountryService.QueryableList.Select(c => new KeyTextDTO() { key = (c.Country ?? "").ToUpper(), text = c.Country })
                                    .Distinct().OrderBy(r => r.key).ToArrayAsync();
            return results;
        }

        [HttpGet("getmattertype")]
        public async Task<ActionResult<KeyTextDTO[]>> GetMatterType()
        {
            var results = await _gmMatterService.QueryableList.Select(c => new KeyTextDTO() { key = (c.MatterType ?? "").ToUpper(), text = c.MatterType })
                                    .Distinct().OrderBy(r => r.key).ToArrayAsync();
            return results;
        }

        [HttpGet("getstatus")]
        public async Task<ActionResult<KeyTextDTO[]>> GetStatus()
        {
            var results = await _gmMatterService.QueryableList.Select(c => new KeyTextDTO() { key = (c.MatterStatus ?? "").ToUpper(), text = c.MatterStatus })
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
        [HttpGet("getgm")]
        public async Task<ApiEmailResult> GetPagedGM(string? caseNumber, string? country, string? matterType, string? matterStatus, string? clientRefNumber, int pageNo, int pageSize)
        {
            var config = new MapperConfiguration(cfg =>
               cfg.CreateMap<GMMatter, KeyTextDTO>()
               .ForMember(vm => vm.text, domain => domain.MapFrom(g => g.CaseNumber + (string.IsNullOrEmpty(g.SubCase) ? "" : "-" + g.SubCase) + "/" + (g.MatterType ?? "").ToString()))
               .ForMember(vm => vm.key, domain => domain.MapFrom(g => "MatId|" + g.MatId.ToString())));

            var gm = _gmMatterService.QueryableList
                            .OrderBy(m => m.CaseNumber).ThenBy(m => m.SubCase);
            var data = await gm.ApplyPaging(pageNo, pageSize).ProjectTo<KeyTextDTO>(config).ToArrayAsync();
            var rowCount = await gm.CountAsync();

            var results = new ApiEmailResult()
            {
                Results = data,
                PageCount = (int)Math.Ceiling((double)rowCount / pageSize),
                RowCount = rowCount
            };
            return results;
        }

        [HttpGet("getactions")]
        public async Task<ApiEmailResult> GetPagedAction(int pageNo, int pageSize, string? caseNumber, string? country, string? subCase, string? matterType,
                                                            string? actionType, DateTime? baseDateFrom, DateTime? baseDateTo)
        {
            var config = new MapperConfiguration(cfg =>
               cfg.CreateMap<GMActionDue, KeyTextDTO>()
               .ForMember(vm => vm.text, domain => domain.MapFrom(ad => ad.CaseNumber + (string.IsNullOrEmpty(ad.SubCase) ? "" : "-" + ad.SubCase) + "/"
                                                                           + ad.ActionType + '/' + ad.BaseDate.ToString("dd-MMM-yyyy")))
               .ForMember(vm => vm.key, domain => domain.MapFrom(dd => "ActId|" + dd.ActId.ToString())));

            var actDue = _actionDueService.QueryableList
                                .OrderBy(a => a.CaseNumber).ThenBy(a => a.SubCase).ThenBy(a => a.ActionType).ThenBy(a => a.BaseDate);
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
        public async Task<ApiEmailResult> GetPagedCost(int pageNo, int pageSize, string? caseNumber, string? invoiceNo, 
                                                            string? costType, DateTime? invoiceDateFrom, DateTime? invoiceDateTo)
        {
            var config = new MapperConfiguration(cfg =>
               cfg.CreateMap<GMCostTrack, KeyTextDTO>()
               .ForMember(vm => vm.text, domain => domain.MapFrom(ct => ct.CaseNumber + (string.IsNullOrEmpty(ct.SubCase) ? "" : "-" + ct.SubCase) + "/"
                                                                           + ct.CostType + '/' + ct.InvoiceDate.ToString("dd-MMM-yyyy")))
               .ForMember(vm => vm.key, domain => domain.MapFrom(ct => "CostTrackId|" + ct.CostTrackId.ToString())));

            var costTrack = _costTrackingService.QueryableList
                                .OrderBy(c => c.CaseNumber).ThenBy(c => c.SubCase).ThenBy(c => c.CostType).ThenBy(c => c.InvoiceDate);
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
        private List<QueryFilterViewModel> BuildGMFilters(string? caseNumber, string? country, string? matterType, string? matterStatus, string? clientRefNumber)
        {
            var filters = new List<QueryFilterViewModel>();

            if (!string.IsNullOrEmpty(caseNumber))
                filters.Add(new QueryFilterViewModel() { Property = "CaseNumber", Value = caseNumber });
            if (!string.IsNullOrEmpty(country))
            {
                filters.Add(new QueryFilterViewModel() { Property = "Countries.Country", Value = country });
                filters.Add(new QueryFilterViewModel() { Property = "CountryOp", Value = "eq" });
            }
            if (!string.IsNullOrEmpty(matterType))
            {
                filters.Add(new QueryFilterViewModel() { Property = "MatterType", Value = matterType });
                filters.Add(new QueryFilterViewModel() { Property = "MatterTypeOp", Value = "eq" });
            }
            if (!string.IsNullOrEmpty(matterStatus))
            {
                filters.Add(new QueryFilterViewModel() { Property = "MatterStatus", Value = matterStatus });
                filters.Add(new QueryFilterViewModel() { Property = "MatterStatusOp", Value = "eq" });
            }
            if (!string.IsNullOrEmpty(clientRefNumber))
                filters.Add(new QueryFilterViewModel() { Property = "ClientRef", Value = clientRefNumber });
            return filters;
        }

        private List<QueryFilterViewModel> BuildActionFilters(string? caseNumber, string? country, string? subCase, string? matterType, string? actionType, DateTime? baseDateFrom, DateTime? baseDateTo)
        {
            // based on GMActionDueViewModel & Areas/GeneralMatter/Views/ActionDue/_SearchTabContent
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

            if (!string.IsNullOrEmpty(matterType))
            {
                filters.Add(new QueryFilterViewModel() { Property = "GMMatter.MatterType", Value = matterType });
                filters.Add(new QueryFilterViewModel() { Property = "MatterStatusOp", Value = "eq" });
            }
            if (!string.IsNullOrEmpty(actionType))
                filters.Add(new QueryFilterViewModel() { Property = "ActionType", Value = actionType });

            if (baseDateFrom != null)
                filters.Add(new QueryFilterViewModel() { Property = "BaseDateFrom", Value = baseDateFrom.ToString() });
            if (baseDateTo != null)
                filters.Add(new QueryFilterViewModel() { Property = "BaseDateTo", Value = baseDateTo.ToString() });

            return filters;
        }

        private List<QueryFilterViewModel> BuildCostFilters(string? caseNumber, string? invoiceNo, string? costType, DateTime? invoiceDateFrom, DateTime? invoiceDateTo)
        {
            // based on PatCostTrackViewModel & Areas/Patent/Views/CostTracking/_SearchTabContent
            var filters = new List<QueryFilterViewModel>();

            if (!string.IsNullOrEmpty(caseNumber))
                filters.Add(new QueryFilterViewModel() { Property = "CaseNumber", Value = caseNumber });
            if (!string.IsNullOrEmpty(invoiceNo))
            {
                filters.Add(new QueryFilterViewModel() { Property = "InvoiceNumber", Value = invoiceNo });    
            }
            
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
