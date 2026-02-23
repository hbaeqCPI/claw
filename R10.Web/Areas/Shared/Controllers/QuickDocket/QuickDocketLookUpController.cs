using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R10.Core.Helpers;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessDueDateList)]
    public class QuickDocketLookUpController : BaseController
    {
        private readonly IOuickDocketViewModelService _quickDocketService;
        private readonly IAuthorizationService _authService;

        public QuickDocketLookUpController(IAuthorizationService authService, IOuickDocketViewModelService quickDocketService)
        {
            _authService = authService;
            _quickDocketService = quickDocketService;            
        }

        //public async Task<IActionResult> GetActionTypeList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        //{
        //    var actionTypes = await _quickDocketService.GetCombinedActionTypes(systemType,text);
        //    return Json(actionTypes);
        //}

        public async Task<IActionResult> GetActionTypeList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var actionTypes = await _quickDocketService.GetCombinedActionTypes(systemType, text);
            if (request.PageSize > 0)
            {
                return Json(await actionTypes.ToDataSourceResultAsync(request));
            }
            return Json(actionTypes);
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> ActionTypeValueMapper(string value, string systemType)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var actionTypes = await _quickDocketService.GetCombinedActionTypes(systemType, value);
                return Json(actionTypes);
            }
            return Ok();
        }

        //public async Task<IActionResult> GetActionDueList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        //{
        //    var actionDues = await _quickDocketService.GetCombinedActionDues(systemType, text); 
        //    return Json(actionDues);
        //}

        public async Task<IActionResult> GetActionDueList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var actionDues = await _quickDocketService.GetCombinedActionDues(systemType, text);
            if (request.PageSize > 0)
            {
                return Json(await actionDues.ToDataSourceResultAsync(request));
            }
            return Json(actionDues);
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> ActionDueValueMapper(string value, string systemType)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var actionDues = await _quickDocketService.GetCombinedActionDues(systemType, value);
                return Json(actionDues);
            }
            return Ok();
        }

        public async Task<IActionResult> GetCaseNumberList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var caseNumbers = await _quickDocketService.GetCombinedCaseNumbers(systemType, text);
            if (request.PageSize > 0)
            {
                return Json(await caseNumbers.ToDataSourceResultAsync(request));
            }
            return Json(caseNumbers);
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> CaseNumberValueMapper(string value, string systemType)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var caseNumbers = await _quickDocketService.GetCombinedCaseNumbers(systemType, value);
                return Json(caseNumbers);
            }
            return Ok();
        }

        public async Task<IActionResult> GetCaseTypeList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var caseTypes = await _quickDocketService.GetCombinedCaseTypes(systemType, text);
            return Json(caseTypes);
        }

        public async Task<IActionResult> GetRespOfficeList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var caseTypes = await _quickDocketService.GetCombinedRespOffices(systemType, text);
            return Json(caseTypes);
        }

        public async Task<IActionResult> GetClientRefList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var clientRefs = await _quickDocketService.GetCombinedClientRefs(systemType, text);
            return Json(clientRefs);
        }

        public async Task<IActionResult> GetDeDocketInstructionList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var instrxs = await _quickDocketService.GetCombinedDeDocketInstructions(systemType, text);
            return Json(instrxs);
        }

        public async Task<IActionResult> GetDeDocketInstructedByList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var instrxs = await _quickDocketService.GetCombinedDeDocketInstructedBy(systemType, text);
            return Json(instrxs);
        }

        public async Task<IActionResult> GetStatusList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var statuses = await _quickDocketService.GetCombinedStatuses(systemType,text);
            return Json(statuses);
        }

        public async Task<IActionResult> GetTitleList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var titles = await _quickDocketService.GetCombinedTitles(systemType, text);

            if (request.PageSize > 0)
            {
                return Json(await titles.ToDataSourceResultAsync(request));
            }
            return Json(titles);
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> TitleValueMapper(string value, string systemType)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var titles = await _quickDocketService.GetCombinedTitles(systemType, value);
                return Json(titles);
            }
            return Ok();
        }

        public async Task<IActionResult> GetIndicatorList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var indicators = await _quickDocketService.GetCombinedIndicators(systemType, text);
            return Json(indicators);
        }

        public async Task<IActionResult> GetCountryList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {            
            var countries = await _quickDocketService.GetCombinedCountries(systemType, text);
            return Json(countries);
        }

        public async Task<IActionResult> GetClientList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var result = await _quickDocketService.GetClientList(systemType, text);
            if (request.PageSize > 0)
            {
                return Json(await result.ToDataSourceResultAsync(request));
            }
            return Json(result);
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> ClientValueMapper(string value, string systemType)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var result = await _quickDocketService.GetClientList(systemType, value);
                return Json(result);
            }
            return Ok();
        }

        public async Task<IActionResult> GetOwnerList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var result = await _quickDocketService.GetOwnerList(systemType, text);
            if (request.PageSize > 0)
            {
                return Json(await result.ToDataSourceResultAsync(request));
            }
            return Json(result);
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> OwnerValueMapper(string value, string systemType)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var result = await _quickDocketService.GetOwnerList(systemType, value);
                return Json(result);
            }
            return Ok();
        }

        public async Task<IActionResult> GetAgentList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var result = await _quickDocketService.GetAgentList(systemType, text);
            if (request.PageSize > 0)
            {
                return Json(await result.ToDataSourceResultAsync(request));
            }
            return Json(result);
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> AgentValueMapper(string value, string systemType)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var result = await _quickDocketService.GetAgentList(systemType, value);
                return Json(result);
            }
            return Ok();
        }

        public async Task<IActionResult> GetAttorneyList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var result = await _quickDocketService.GetAttorneyList(systemType, text);
            if (request.PageSize > 0)
            {
                return Json(await result.ToDataSourceResultAsync(request));
            }
            return Json(result);
        }

        #region "Default Screen lookups"
        public async Task<IActionResult> GetDefaultActionTypeList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var actionTypes = await _quickDocketService.GetCombinedDefaultActionTypes(systemType, text);
            return Json(actionTypes);
        }

        public async Task<IActionResult> GetDefaultActionDueList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var actionDues = await _quickDocketService.GetCombinedDefaultActionDues(systemType, text);
            return Json(actionDues);
        }

        public async Task<IActionResult> GetDefaultCaseNumberList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var caseNumbers = await _quickDocketService.GetCombinedDefaultCaseNumbers(systemType, text);
            if (request.PageSize > 0)
            {
                return Json(await caseNumbers.ToDataSourceResultAsync(request));
            }
            return Json(caseNumbers);
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> DefaultCaseNumberValueMapper(string value, string systemType)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var caseNumbers = await _quickDocketService.GetCombinedDefaultCaseNumbers(systemType, value);
                return Json(caseNumbers);
            }
            return Ok();
        }


        public async Task<IActionResult> GetDefaultCaseTypeList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var caseTypes = await _quickDocketService.GetCombinedDefaultCaseTypes(systemType, text);
            return Json(caseTypes);
        }

        public async Task<IActionResult> GetDefaultRespOfficeList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var caseTypes = await _quickDocketService.GetCombinedDefaultRespOffices(systemType, text);
            return Json(caseTypes);
        }

        public async Task<IActionResult> GetDefaultClientRefList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {

            var clientRefs = await _quickDocketService.GetCombinedDefaultClientRefs(systemType, text);
            return Json(clientRefs);
        }

        public async Task<IActionResult> GetDefaultStatusList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var statuses = await _quickDocketService.GetCombinedDefaultStatuses(systemType, text);
            return Json(statuses);
        }

        public async Task<IActionResult> GetDefaultTitleList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var titles = await _quickDocketService.GetCombinedDefaultTitles(systemType, text);

            if (request.PageSize > 0)
            {
                return Json(await titles.ToDataSourceResultAsync(request));
            }
            return Json(titles);

        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> DefaultTitleValueMapper(string value, string systemType)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var titles = await _quickDocketService.GetCombinedDefaultTitles(systemType, value);
                return Json(titles);
            }
            return Ok();
        }

        public async Task<IActionResult> GetDefaultIndicatorList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var indicators = await _quickDocketService.GetCombinedDefaultIndicators(systemType, text);
            return Json(indicators);
        }

        public async Task<IActionResult> GetDefaultCountryList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var countries = await _quickDocketService.GetCombinedDefaultCountries(systemType, text);
            return Json(countries);
        }

        

        #endregion

    }
}