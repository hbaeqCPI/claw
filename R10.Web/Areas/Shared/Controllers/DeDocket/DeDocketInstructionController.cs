using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.DTOs;
using R10.Core.Entities;
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Services;
using SmartFormat.Utilities;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessDeDocketAuxiliary)]
    public class DeDocketInstructionController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<DeDocketInstruction> _viewModelService;
        private readonly IEntityService<DeDocketInstruction> _auxService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IReportService _reportService;
        private readonly IApplicationDbContext _repository;

        private readonly string _dataContainer = "deDocketInstructionDetail";

        public DeDocketInstructionController(
            IAuthorizationService authService, 
            IViewModelService<DeDocketInstruction> viewModelService,
            IEntityService<DeDocketInstruction> auxService,
            IStringLocalizer<SharedResource> localizer,
            IReportService reportService,
            IApplicationDbContext repository)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _auxService = auxService;
            _localizer = localizer;
            _reportService = reportService;
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "DeDocketInstructionSearch",
                Title = _localizer["DeDocket Instruction Search"].ToString(),
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
                PageId = "DeDocketInstructionSearchResults",
                Title = _localizer["DeDocket Instruction Search Results"].ToString(),
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
                var DeDocketInstructions = _viewModelService.AddCriteria(mainSearchFilters);
                var result = await _viewModelService.CreateViewModelForGrid(request, DeDocketInstructions,"Instruction", "InstructionId");
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<DetailPageViewModel<DeDocketInstruction>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<DeDocketInstruction>
            {
                Detail = await GetById(id)
            };

            if (viewModel.Detail != null)
            {
                viewModel.Detail.IndicatorList = viewModel.Detail.Indicators?.Split("|");
                viewModel.AddSharedSecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                //hide copy and email buttons
                viewModel.CanCopyRecord = false;
                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = _dataContainer;

                viewModel.EditScreenUrl = this.Url.Action("Detail", new {id = id}); // $"{viewModel.EditScreenUrl}/{id}";
                viewModel.SearchScreenUrl = this.Url.Action("Index");
            }
            return viewModel;
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false)
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            DeDocketInstruction detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["DeDocket Instruction Detail"].ToString(),
                RecordId = detail.InstructionId,
                SingleRecord = singleRecord || !Request.IsAjax(),
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

        [HttpPost()]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        private async Task<DetailPageViewModel<DeDocketInstruction>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<DeDocketInstruction>
            {
                Detail = new DeDocketInstruction()
            };

            viewModel.AddSharedSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        [Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        public async Task<IActionResult> Add(bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            DeDocketInstruction detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New DeDocket Instruction"].ToString(),
                RecordId = detail.InstructionId,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.FullModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] DeDocketInstruction instruction)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(instruction, instruction.InstructionId);

                if (instruction.IndicatorList != null && instruction.IndicatorList.Length > 0)
                    instruction.Indicators = String.Join("|", instruction.IndicatorList) + "|";
                else
                    instruction.Indicators = "";

                if (instruction.InstructionId > 0)
                {
                    await _auxService.Update(instruction);
                }
                else {
                    await _auxService.Add(instruction);
                }

                return Json(instruction.InstructionId);
            }
            else
                return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost, Authorize(Policy = SharedAuthorizationPolicy.CanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string tStamp)
        {
            var entity = await GetById(id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            entity.tStamp = Convert.FromBase64String(tStamp);
            await _auxService.Delete(entity);

            return Ok();
        }

        private async Task<DeDocketInstruction> GetById(int id)
        {
            return await _auxService.QueryableList.SingleOrDefaultAsync((c => c.InstructionId == id));
        }

        [HttpGet]
        public IActionResult Print()
        {
            ViewBag.Url = Url.Action("Print");
            ViewBag.DownloadName = "DeDocket Instruction Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintViewModel deDocketInstructionPrintModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(deDocketInstructionPrintModel, ReportType.SharedDeDocketInstructionPrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "", string systemType = "")
        {
            var instructionList = _auxService.QueryableList.Where(i => i.InUse);
            instructionList = instructionList.Where(i => string.IsNullOrEmpty(systemType) || (systemType == SystemType.Patent && i.Patent) || (systemType == SystemType.Trademark && i.Trademark) || (systemType == SystemType.GeneralMatter && i.GeneralMatter) || !(i.Patent || i.Trademark || i.GeneralMatter));
            return await GetPicklistData(instructionList, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetDeDocketInstructionList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "",string systemType="",string indicator="")
        {
            var list = GetInstructionList(property, text, filterType, requiredRelation, systemType, indicator);
            var result = await list.Select(i => i.Instruction).ToListAsync();
            return Json(result);
        }

        public async Task<IActionResult> GetQDDeDocketInstructionList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "", string systemType = "", string indicator = "")
        {
            var list = GetInstructionList(property, text, filterType, requiredRelation, systemType, indicator);
            var result = await list.ToListAsync();
            return Json(result);
        }

        public async Task<IActionResult> GetQDDeDocketInstructionBatchUpdateList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "", string systemTypes = "", string indicators = "")
        {
            var mainQuery = _auxService.QueryableList.Where(i => i.InUse);
            mainQuery = mainQuery.Where(i => (systemTypes.Contains(SystemTypeCode.Patent + "|") && i.Patent) || (systemTypes.Contains(SystemTypeCode.Trademark + "|") && i.Trademark) || (systemTypes.Contains(SystemTypeCode.GeneralMatter + "|") && i.GeneralMatter) || !(i.Patent || i.Trademark || i.GeneralMatter));
            mainQuery = mainQuery.Where(i => !i.DocumentRequired);

            var list = QueryHelper.BuildCriteria(mainQuery, property, text, filterType, requiredRelation).OrderBy(property);
            var result = await list.ToListAsync();

            var finalList = new List<DeDocketInstruction>();
            var qdIndicators = indicators.Split("|").ToList();
            foreach (var item in list) {
                var include = true;
                if (!string.IsNullOrEmpty(item.Indicators)) {
                    var instrxIndicators = item.Indicators.Split("|").ToList();
                    if (!instrxIndicators.Any(i => qdIndicators.Any(qd => qd.ToLower() == i.ToLower())))
                        include = false;
                }
                if (include)
                    finalList.Add(item);
            }
            return Json(finalList);
        }

        private IQueryable<DeDocketInstruction> GetInstructionList(string property, string text, FilterType filterType, string requiredRelation = "", string systemType = "", string indicator = "") {
            var instructionList = _auxService.QueryableList.Where(i => i.InUse);
            instructionList = instructionList.Where(i => string.IsNullOrEmpty(systemType) || (systemType == SystemType.Patent && i.Patent) || (systemType == SystemType.Trademark && i.Trademark) || (systemType == SystemType.GeneralMatter && i.GeneralMatter) || !(i.Patent || i.Trademark || i.GeneralMatter));
            instructionList = instructionList.Where(i => string.IsNullOrEmpty(i.Indicators) || i.Indicators.Contains(indicator + "|"));
            var list = QueryHelper.BuildCriteria(instructionList, property, text, filterType, requiredRelation).OrderBy(property);
            return list;
        }

        public async Task<IActionResult> GetIndicatorList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "", string systemType = "")
        {
            var indicators = await _repository.PatIndicators.Select(i => new LookupDTO { Value = i.Indicator }).ToListAsync();
            var tmkIndicators = await _repository.TmkIndicators.Select(i => new LookupDTO { Value = i.Indicator }).ToListAsync();
            indicators.AddRange(tmkIndicators);
            var result = indicators.GroupBy(i=> i.Value).Select(i=> new LookupDTO { Value = i.Key }).ToList();
            return Json(result);
        }


        [HttpGet()]
        public async Task<IActionResult> DetailLink(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var entity = await _viewModelService.GetEntityByCode("Instruction", id);
                if (entity == null)
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction(nameof(Detail), new { id = entity.InstructionId, singleRecord = true, fromSearch = true });
            }
            else
                return RedirectToAction(nameof(Add), new { fromSearch = true });
        }
    }
}
