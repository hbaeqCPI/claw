using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;

using Newtonsoft.Json;
using R10.Web.Areas;

namespace R10.Web.Areas.Trademark.Controllers
{
    [Area("Trademark"), Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessAuxiliary)]
    public class DesCaseTypeFieldsController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "tmkDesCaseTypeFieldsDetail";

        public DesCaseTypeFieldsController(
            IAuthorizationService authService,
            IApplicationDbContext repository,
            IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _repository = repository;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "tmkDesCaseTypeFieldsSearch",
                Title = _localizer["Des Case Type Fields Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded
            };

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel()
            {
                Page = PageType.SearchResults,
                PageId = "tmkDesCaseTypeFieldsSearchResults",
                Title = _localizer["Des Case Type Fields Search Results"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded
            };

            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search()
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request)
        {
            var data = await _repository.TmkDesCaseTypeFields.AsNoTracking().ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false)
        {
            var items = await _repository.TmkDesCaseTypeFields.AsNoTracking().ToListAsync();
            if (id < 0 || id >= items.Count)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            var detail = items[id];
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = _dataContainer,
                Title = _localizer["Des Case Type Fields Detail"].ToString(),
                RecordId = id,
                SingleRecord = singleRecord || !Request.IsAjax(),
                Data = detail
            };

            if (Request.IsAjax())
            {
                if (!singleRecord && !fromSearch)
                    model.Page = PageType.DetailContent;

                return PartialView("Index", model);
            }

            return View("Index", model);
        }

        [HttpPost()]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public IActionResult Add(bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Des Case Type Fields"].ToString(),
                Data = new TmkDesCaseTypeFields()
            };
            ModelState.Clear();

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkDesCaseTypeFields entity)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            _repository.Set<TmkDesCaseTypeFields>().Add(entity);
            await _repository.SaveChangesAsync();
            return Json(0);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] TmkDesCaseTypeFields entity)
        {
            _repository.Set<TmkDesCaseTypeFields>().Remove(entity);
            await _repository.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public IActionResult DetailLink(int? id)
        {
            return id > 0
                ? RedirectToAction(nameof(Detail), new { id = id, singleRecord = true, fromSearch = true })
                : RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_repository.TmkDesCaseTypeFields.AsQueryable(), request, property, text, filterType, requiredRelation);
        }
    }
}
