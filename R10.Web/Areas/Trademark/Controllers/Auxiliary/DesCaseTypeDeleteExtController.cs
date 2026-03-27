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

namespace R10.Web.Areas.Trademark.Controllers
{
    [Area("Trademark"), Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessAuxiliary)]
    public class DesCaseTypeDeleteExtController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "tmkDesCaseTypeDeleteExtDetail";

        public DesCaseTypeDeleteExtController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _repository = repository;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel { Page = PageType.Search, PageId = "tmkDesCaseTypeDeleteExtSearch", Title = _localizer["Des Case Type Delete Ext Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "tmkDesCaseTypeDeleteExtSearchResults", Title = _localizer["Des Case Type Delete Ext Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request)
        {
            var data = await _repository.TmkDesCaseTypeDeleteExts.AsNoTracking().ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public IActionResult Add(bool fromSearch = false)
        {
            if (!Request.IsAjax()) return RedirectToAction("Index");
            var model = new PageViewModel { Page = fromSearch ? PageType.Detail : PageType.DetailContent, PageId = _dataContainer, Title = _localizer["New Des Case Type Delete Ext"].ToString(), Data = new TmkDesCaseTypeDeleteExt() };
            ModelState.Clear();
            return PartialView("Index", model);
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false)
        {
            var items = await _repository.TmkDesCaseTypeDeleteExts.AsNoTracking().ToListAsync();
            if (id < 0 || id >= items.Count) return new RecordDoesNotExistResult();
            var detail = items[id];
            var model = new PageViewModel { Page = PageType.Detail, PageId = _dataContainer, Title = _localizer["Des Case Type Delete Ext Detail"].ToString(), RecordId = id, SingleRecord = singleRecord || !Request.IsAjax(), Data = detail };
            if (Request.IsAjax() && !singleRecord && !fromSearch) model.Page = PageType.DetailContent;
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkDesCaseTypeDeleteExt entity)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });
            _repository.Set<TmkDesCaseTypeDeleteExt>().Add(entity);
            await _repository.SaveChangesAsync();
            return Json(0);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete), ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] TmkDesCaseTypeDeleteExt entity)
        {
            _repository.Set<TmkDesCaseTypeDeleteExt>().Remove(entity);
            await _repository.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public IActionResult DetailLink(int? id)
        {
            return id > 0 ? RedirectToAction(nameof(Detail), new { id = id, singleRecord = true, fromSearch = true }) : RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        [HttpPost]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            return File(Convert.FromBase64String(base64), contentType, fileName);
        }
    }
}
