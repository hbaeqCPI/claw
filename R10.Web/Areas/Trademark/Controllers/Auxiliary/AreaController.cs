using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Security;

using R10.Web.Areas;

namespace R10.Web.Areas.Trademark.Controllers
{
    [Area("Trademark"), Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessAuxiliary)]
    public class AreaController : BaseController
    {
        private readonly IViewModelService<TmkArea> _viewModelService;
        private readonly IParentEntityService<TmkArea, TmkAreaCountry> _areaService;

        public AreaController(
            IViewModelService<TmkArea> viewModelService,
            IParentEntityService<TmkArea, TmkAreaCountry> areaService)
        {
            _viewModelService = viewModelService;
            _areaService = areaService;
        }

        public async Task<IActionResult> GetAreaList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_areaService.QueryableList, request, property, text, filterType, new string[] { "AreaID", "Area", "Description" }, requiredRelation);
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_areaService.QueryableList, request, property, text, filterType, requiredRelation);
        }

        [HttpGet]
        public async Task<IActionResult> DetailLink(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var entity = await _viewModelService.GetEntityByCode("Area", id);
                if (entity != null)
                    return RedirectToAction("Detail", "Country", new { id = entity.AreaID, singleRecord = true, fromSearch = true, tab = "areas" });
            }
            return RedirectToAction("Index", "Country");
        }
    }
}
