using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using R10.Core.DTOs;
using R10.Core.Entities.Trademark;
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

namespace R10.Web.ApiEmail.Trademark
{
    [Route("emailapi/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = TrademarkAuthorizationPolicy.FullModify)]
    [EnableCors("EmailAddInCORSPolicy")]
    [ApiController]
    public class TrademarkController : Microsoft.AspNetCore.Mvc.Controller
    {
        protected readonly ITmkTrademarkService _trademarkService;
        protected readonly ITmkTrademarkViewModelService _trademarkViewModelService;

        protected readonly IActionDueService<TmkActionDue, TmkDueDate> _actionDueService;
        protected readonly ITmkActionDueViewModelService _actionDueViewModelService;

        protected readonly ICostTrackingService<TmkCostTrack> _costTrackingService;
        protected readonly ITmkCostTrackingViewModelService _costTrackingViewModelService;

        public TrademarkController(
                ITmkTrademarkService trademarkService,
                ITmkTrademarkViewModelService trademarkViewModelService,
                IActionDueService<TmkActionDue, TmkDueDate> actionDueService,
                ITmkActionDueViewModelService actionDueViewModelService,
                ICostTrackingService<TmkCostTrack> costTrackingService,
                ITmkCostTrackingViewModelService costTrackingViewModelService
                )
        {
            _trademarkService = trademarkService;
            _trademarkViewModelService = trademarkViewModelService;

            _actionDueService = actionDueService;
            _actionDueViewModelService = actionDueViewModelService;

            _costTrackingService = costTrackingService;
            _costTrackingViewModelService = costTrackingViewModelService;
        }

        // ---------- search picklists
        [HttpGet("getcountry")]
        public async Task<ActionResult<KeyTextDTO[]>> GetCountry()
        {
            var results = await _trademarkService.TmkTrademarks.Select(c => new KeyTextDTO() { key = (c.Country ?? "").ToUpper(), text = c.Country })
                                    .Distinct().OrderBy(r => r.key).ToArrayAsync();
            return results;
        }

        [HttpGet("getcasetype")]
        public async Task<ActionResult<KeyTextDTO[]>> GetCaseType()
        {
            var results = await _trademarkService.TmkTrademarks.Select(c => new KeyTextDTO() { key = (c.CaseType ?? "").ToUpper(), text = c.CaseType })
                                    .Distinct().OrderBy(r => r.key).ToArrayAsync();
            return results;
        }

        [HttpGet("getstatus")]
        public async Task<ActionResult<KeyTextDTO[]>> GetStatus()
        {
            var results = await _trademarkService.TmkTrademarks.Select(c => new KeyTextDTO() { key = (c.TrademarkStatus ?? "").ToUpper(), text = c.TrademarkStatus })
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

        [HttpGet("getcosts")]
        public async Task<ApiEmailResult> GetPagedCost(int pageNo, int pageSize, string? caseNumber, string? country, string? subCase,
                                                            string? costType, DateTime? invoiceDateFrom, DateTime? invoiceDateTo)
        {
            var config = new MapperConfiguration(cfg =>
               cfg.CreateMap<TmkCostTrack, KeyTextDTO>()
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


        // ---------- search results
        [HttpGet("gettmk")]
        public async Task<ApiEmailResult> GetPagedTmk(string? caseNumber, string? country, string? caseType, string? status, string? clientRefNumber, int pageNo, int pageSize)
        {
            var config = new MapperConfiguration(cfg =>
               cfg.CreateMap<TmkTrademark, KeyTextDTO>()
               .ForMember(vm => vm.text, domain => domain.MapFrom(t => t.CaseNumber + "/" + t.Country + (string.IsNullOrEmpty(t.SubCase) ? "" : "-" + t.SubCase) + "/" + t.CaseType))
               .ForMember(vm => vm.key, domain => domain.MapFrom(t => "TmkId|" + t.TmkId.ToString())));

            var tmk = _trademarkViewModelService.AddCriteria(BuildTmkFilters(caseNumber, country, caseType, status, clientRefNumber), _trademarkService.TmkTrademarks)
                            .OrderBy(t => t.CaseNumber).ThenBy(t => t.Country).ThenBy(t => t.SubCase);
            var data = await tmk.ApplyPaging(pageNo, pageSize).ProjectTo<KeyTextDTO>(config).ToArrayAsync();
            var rowCount = await tmk.CountAsync();

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
               cfg.CreateMap<TmkActionDue, KeyTextDTO>()
               .ForMember(vm => vm.text, domain => domain.MapFrom(ad => ad.CaseNumber + "/" + ad.Country + (string.IsNullOrEmpty(ad.SubCase) ? "" : "-" + ad.SubCase) + "/"
                                                                           + ad.ActionType + '/' + ad.BaseDate.ToString("dd-MMM-yyyy")))
               .ForMember(vm => vm.key, domain => domain.MapFrom(dd => "ActId|" + dd.ActId.ToString())));

            var actDue = _actionDueViewModelService.AddCriteria(BuildActionFilters(caseNumber, country, subCase, actionType, baseDateFrom, baseDateTo), _actionDueService.QueryableList)
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

        // ---------- search results filter
        private List<QueryFilterViewModel> BuildTmkFilters(string? caseNumber, string? country, string? caseType, string? tmkStatus, string? clientRefNumber)
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
                filters.Add(new QueryFilterViewModel() { Property = "CaseType", Value = caseType });
                filters.Add(new QueryFilterViewModel() { Property = "CaseTypeOp", Value = "eq" });
            if (!string.IsNullOrEmpty(tmkStatus))
                filters.Add(new QueryFilterViewModel() { Property = "TrademarkStatus", Value = tmkStatus });
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
            // based on PatCostTrackViewModel & Areas/Trademark/Views/CostTracking/_SearchTabContent
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
