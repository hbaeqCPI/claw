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
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;

namespace R10.Web.Areas.Patent.Controllers
{
    [Area("Patent"), Authorize(Policy = PatentAuthorizationPolicy.CanAccessAuxiliary)]
    public class AreaDeleteController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "patAreaDeleteDetail";

        public AreaDeleteController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _repository = repository;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel { Page = PageType.Search, PageId = "patAreaDeleteSearch", Title = _localizer["Area Delete Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "patAreaDeleteSearchResults", Title = _localizer["Area Delete Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request)
        {
            var data = await _repository.PatAreaDeletes.AsNoTracking().ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public IActionResult Add(bool fromSearch = false)
        {
            if (!Request.IsAjax()) return RedirectToAction("Index");
            var model = new PageViewModel { Page = fromSearch ? PageType.Detail : PageType.DetailContent, PageId = _dataContainer, Title = _localizer["New Area Delete"].ToString(), Data = new PatAreaDelete() };
            ModelState.Clear();
            return PartialView("Index", model);
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false)
        {
            var items = await _repository.PatAreaDeletes.AsNoTracking().ToListAsync();
            if (id < 0 || id >= items.Count) return new RecordDoesNotExistResult();
            var detail = items[id];
            var model = new PageViewModel { Page = PageType.Detail, PageId = _dataContainer, Title = _localizer["Area Delete Detail"].ToString(), RecordId = id, SingleRecord = singleRecord || !Request.IsAjax(), Data = detail };
            if (Request.IsAjax() && !singleRecord && !fromSearch) model.Page = PageType.DetailContent;
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatAreaDelete entity)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });
            _repository.Set<PatAreaDelete>().Add(entity);
            await _repository.SaveChangesAsync();
            return Json(0);
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete), ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] PatAreaDelete entity)
        {
            _repository.Set<PatAreaDelete>().Remove(entity);
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
