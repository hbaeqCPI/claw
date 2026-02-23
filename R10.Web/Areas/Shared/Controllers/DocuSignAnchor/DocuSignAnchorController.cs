using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using R10.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using R10.Core.DTOs;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Services;
using R10.Core.Entities.AMS;
using R10.Core;
using R10.Core.Entities;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessESignatureAuxiliary)]
    public class DocuSignAnchorController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<DocuSignAnchor> _viewModelService;
        private readonly IParentEntityService<DocuSignAnchor,DocuSignAnchorTab> _docuSignAnchorService;
        
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IReportService _reportService;

        private readonly string _dataContainer = "docuSignAnchorDetail";

        public DocuSignAnchorController(
            IAuthorizationService authService, 
            IViewModelService<DocuSignAnchor> viewModelService,
            IParentEntityService<DocuSignAnchor, DocuSignAnchorTab> anchorCodeService,

            IStringLocalizer<SharedResource> localizer,
            IReportService reportService)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _docuSignAnchorService = anchorCodeService;
            _localizer = localizer;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "docuSignAnchorSearch",
                Title = _localizer["eSignature Anchor Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded
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
                PageId = "docuSignAnchorSearchResults",
                Title = _localizer["eSignature Anchor Search Results"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded
            };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search()
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var anchorCodes = _viewModelService.AddCriteria(mainSearchFilters);
                var result = await _viewModelService.CreateViewModelForGrid(request, anchorCodes, "AnchorCode", "DocuSignAnchorId");
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost()]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["eSignature Anchor Detail"].ToString(),
                RecordId = detail.DocuSignAnchorId,
                SingleRecord = singleRecord || !Request.IsAjax(),
                ActiveTab = tab,
                PagePermission = page,
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

        [HttpGet]
        public IActionResult Print()
        {
            ViewBag.Url = Url.Action("Print");
            ViewBag.DownloadName = "eSignature Anchor Print Screen";
            return View();
        }

        //[HttpPost]
        //public IActionResult Print([FromBody] PrintViewModel docuSignAnchorPrintModel)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        return _reportService.GetReport(docuSignAnchorPrintModel, ReportType.DocuSignAnchorPrintScreen).Result;
        //    }

        //    return BadRequest("Unhandled error.");
        //}

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(string anchorCode = "", bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen(anchorCode);
            if (page.Detail == null)
                return RedirectToAction("Index");

            //if (TempData["CopyOptions"] != null)
            //{
            //    await ExtractCopyParams(page);
            //}

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New eSignature Anchor"].ToString(),
                RecordId = detail.DocuSignAnchorId,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };
            ModelState.Clear();

            return PartialView("Index", model);
        }


        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string tStamp)
        {
            var entity = await _docuSignAnchorService.GetByIdAsync(id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            entity.tStamp = Convert.FromBase64String(tStamp);
            await _docuSignAnchorService.Delete(entity);

            return Ok();
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] DocuSignAnchor docuSignAnchor)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(docuSignAnchor, docuSignAnchor.DocuSignAnchorId);

                if (docuSignAnchor.DocuSignAnchorId > 0)
                    await _docuSignAnchorService.Update(docuSignAnchor);
                else {
                    await _docuSignAnchorService.Add(docuSignAnchor);
                }
                    

                return Json(docuSignAnchor.DocuSignAnchorId);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var docuSignAnchor = await _docuSignAnchorService.GetByIdAsync(id);
            if (docuSignAnchor == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = docuSignAnchor.CreatedBy, dateCreated = docuSignAnchor.DateCreated, updatedBy = docuSignAnchor.UpdatedBy, lastUpdate = docuSignAnchor.LastUpdate, tStamp = docuSignAnchor.tStamp });
        }

        private async Task<DetailPageViewModel<DocuSignAnchor>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<DocuSignAnchor>();
            var anchorCode = await _docuSignAnchorService.QueryableList.Where(a => a.DocuSignAnchorId == id).FirstOrDefaultAsync();
          
            viewModel.Detail =anchorCode;
            if (viewModel.Detail != null)
            {
                viewModel.AddSharedSecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                this.AddDefaultNavigationUrls(viewModel);
                
                viewModel.CanEmail = false;
                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.SearchScreenUrl = this.Url.Action("Index");
                viewModel.Container = _dataContainer;
                viewModel.CanPrintRecord = false; //just to hide the print button
                viewModel.CanCopyRecord = false;
                viewModel.CanDeleteRecord = viewModel.CanDeleteRecord && !viewModel.Detail.IsCPIAnchor;
            }
            return viewModel;
        }
        

        private async Task<DetailPageViewModel<DocuSignAnchor>> PrepareAddScreen(string anchorCode)
        {
            var viewModel = new DetailPageViewModel<DocuSignAnchor>();
            viewModel.Detail = new DocuSignAnchor();

            viewModel.Detail.AnchorCode = anchorCode;
            

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        public async Task<IActionResult> DocuSignAnchorTabsRead([DataSourceRequest] DataSourceRequest request, int anchorCodeId)
        {
            var result = (await _docuSignAnchorService.ChildService.QueryableList.Where(p => p.DocuSignAnchorId == anchorCodeId).ToListAsync()).ToDataSourceResult(request);
            return Json(result);
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> DocuSignAnchorTabsUpdate(int docuSignAnchorId, 
            [Bind(Prefix = "updated")]IEnumerable<DocuSignAnchorTab> updated,
            [Bind(Prefix = "new")]IEnumerable<DocuSignAnchorTab> added, 
            [Bind(Prefix = "deleted")]IEnumerable<DocuSignAnchorTab> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _docuSignAnchorService.ChildService.Update(docuSignAnchorId, User.GetUserName(), updated, added, deleted);
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                _localizer["eSignature Anchor tab has been saved successfully."].ToString() :
                _localizer["eSignature Anchor tabs have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> DocuSignAnchorTabsDelete([Bind(Prefix = "deleted")] DocuSignAnchorTab deleted)
        {
            if (deleted.DocuSignTabId > 0)
            {
                await _docuSignAnchorService.ChildService.Update(deleted.DocuSignAnchorId, User.GetUserName(), new List<DocuSignAnchorTab>(), new List<DocuSignAnchorTab>(), new List<DocuSignAnchorTab>() { deleted });
                return Ok(new { success = _localizer["eSignature Anchor tab has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_docuSignAnchorService.QueryableList, request, property, text, filterType, requiredRelation);
        }


        [HttpGet()]
        public async Task<IActionResult> DetailLink(string anchorCode)
        {
            if (!string.IsNullOrEmpty(anchorCode))
            {
                var anchorCodes = await _docuSignAnchorService.QueryableList.Where(a => a.AnchorCode == anchorCode).ToListAsync();
                if (anchorCodes.Any()) {
                    var anchorCodeId = anchorCodes.FirstOrDefault().DocuSignAnchorId;
                    if (anchorCodeId > 0)
                        return RedirectToAction(nameof(Detail), new { id = anchorCodeId, singleRecord = true, fromSearch = true });
                }
            }
            if ((await _authService.AuthorizeAsync(User, SharedAuthorizationPolicy.FullModify)).Succeeded)
                return RedirectToAction(nameof(Add), new { fromSearch = true, anchorCode = anchorCode});
            else
                return new RecordDoesNotExistResult();
        }

        [HttpGet()]
        public async Task<IActionResult> DetailLinkId(int id)
        {
            if (id > 0)
            {
                var anchorCode = await _docuSignAnchorService.QueryableList.Where(a => a.DocuSignAnchorId == id).FirstOrDefaultAsync();
                if (anchorCode != null)
                    return RedirectToAction(nameof(Detail), new { id = anchorCode.DocuSignAnchorId, singleRecord = true, fromSearch = true });
            }
            return new RecordDoesNotExistResult();
        }

    }
}