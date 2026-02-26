using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using R10.Core.DTOs;
using R10.Core.Entities.Shared;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Extensions;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class DocumentsController : Microsoft.AspNetCore.Mvc.Controller
    {

        private readonly IDocumentService _docService;
        private readonly IDocumentViewModelService _docViewModelService;
        private readonly IAuthorizationService _authService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ISystemSettings<DefaultSetting> _settings;

        private readonly string searchPageId = "documentSearch";
        private readonly string searchTitle = "Document Search Results";

        public DocumentsController(
                    IDocumentService docService,
                    IDocumentViewModelService docViewModelService,
                    IAuthorizationService authService,
                    IStringLocalizer<SharedResource> localizer,
                    ISystemSettings<DefaultSetting> settings
                    )
        {
            _docService = docService;
            _docViewModelService = docViewModelService;
            _authService = authService;
            _localizer = localizer;
            _settings = settings;
        }

        [Authorize(Policy = PatentAuthorizationPolicy.CanAccessSystem)]
        public IActionResult Patent()
        {
            TempData["DetailData"] = "P";
            return RedirectToAction("Index");
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessSystem)]
        public IActionResult Trademark()
        {
            TempData["DetailData"] = "T";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var detailData = TempData["DetailData"] == null ? "" : TempData["DetailData"].ToString();
            var systemType = "";
            var screenCode = "";
            var dataKey = "";
            var dataKeyValue = "";

            if (!string.IsNullOrEmpty(detailData))
            {
                var tmpData = detailData.Split("|");        // factor-out for efficiency
                var tmpLen = tmpData.Length;
                systemType = tmpData[0];
                if (tmpLen > 1) screenCode = tmpData[1];
                if (tmpLen > 2) dataKey = tmpData[2];
                if (tmpLen > 3) dataKeyValue = tmpData[3];
            }
            ViewData["DetailSystemType"] = systemType;
            ViewData["DetailScreenCode"] = screenCode;
            ViewData["DetailDataKey"] = dataKey;
            ViewData["DetailDataKeyValue"] = dataKeyValue;


            var model = new DocPageViewModel()
            {
                Page = PageType.Search,
                PageId = searchPageId,
                Title = _localizer[searchTitle].ToString(),
                CanAddRecord = false,
                DocSystemList = await _docViewModelService.GetSystemList(User.GetSystems()),
                DocDetailLink = new DocDetailLink { SystemType = systemType, ScreenCode = screenCode, DataKey = dataKey, DataKeyValue = Convert.ToInt32(string.IsNullOrEmpty(dataKeyValue) ? "0" : dataKeyValue) }
            };

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View(model);
        }

        //public IActionResult DetailLink(string systemType, string screenCode, string dataKey, int dataKeyValue)
        public IActionResult DetailLink(string data)
        {
            TempData["DetailData"] = data;
            return RedirectToAction("Index");
        }

        #region Search
        //[HttpGet]
        //public IActionResult Search()
        //{
        //    return RedirectToAction("Index");
        //}

        public IActionResult GetSubSearchView(string systemType, string screenCode)                // don't make async to avoid concurrency issues
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = searchPageId,
                Title = _localizer[searchTitle].ToString(),
            };

            var view = _docViewModelService.GetSubSearchView(systemType, screenCode);
            if (string.IsNullOrEmpty(view) || view == "_EmptyView")
            {
                return PartialView("_EmptyView", _localizer["Sorry, the search form for this screen is missing."]);
            }

            ViewBag.PageId = searchPageId;                              // used by FilterOperatorList component combo
            return PartialView(view, model);
        }

        //public async Task<IActionResult> GetSearchResultsView(string id)
        public IActionResult GetSearchResultsView(string systemType, string screenCode, int? recordId)
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = searchPageId,
                Title = _localizer[searchTitle].ToString(),
            };

            var view = _docViewModelService.GetSearchResultView(systemType, screenCode);
            if (string.IsNullOrEmpty(view) || view == "_EmptyView")
            {
                return PartialView("_EmptyView", _localizer["Sorry, the result form for this screen is missing."]);
            }

            ViewBag.PageId = searchPageId;                              // used by FilterOperatorList component combo

            ViewData["RecordId"] = recordId;
            ViewData["ScreenCode"] = screenCode;

            return PartialView(view, model);
        }
        #endregion

        #region Document Tree
        public IActionResult LoadTree()
        {
            return PartialView("_DocumentTree");
        }

        public async Task<Microsoft.AspNetCore.Mvc.ActionResult> GetDocumentTree(string systemType, string screenCode, string dataKey, int dataKeyValue, string id)
        {
            var subTree = await _docService.GetDocumentTree(systemType, screenCode, dataKey, dataKeyValue, id);
            return Json(subTree);
        }


        #endregion

        #region Tree Node Detail - Root
        public async Task<IActionResult> InventionDetail(string id)
        {
            var model = await _docViewModelService.GetInventionDetail(id);
            return PartialView("_DetailInvention", model);
        }

        public async Task<IActionResult> CtryAppDetail(string id)
        {
            var model = await _docViewModelService.GetCtryAppDetail(id);
            return PartialView("_DetailCtryApp", model);
        }
        
        public async Task<IActionResult> TrademarkDetail(string id)
        {
            var model = await _docViewModelService.GetTrademarkDetail(id);
            return PartialView("_DetailTrademark", model);
        }

        public async Task<IActionResult> PatActionDetail(string id)
        {
            var model = await _docViewModelService.GetPatActionDetail(id);
            return PartialView("_DetailPatAction", model);
        }

        public async Task<IActionResult> TmkActionDetail(string id)
        {
            var model = await _docViewModelService.GetTmkActionDetail(id);
            return PartialView("_DetailTmkAction", model);
        }

        public async Task<IActionResult> PatCostDetail(string id)
        {
            var model = await _docViewModelService.GetPatCostDetail(id);
            return PartialView("_DetailPatCostTrack", model);
        }

        public async Task<IActionResult> PatCostInvDetail(string id)
        {
            var model = await _docViewModelService.GetPatCostInvDetail(id);
            return PartialView("_DetailPatCostTrackInv", model);
        }

        public async Task<IActionResult> TmkCostDetail(string id)
        {
            var model = await _docViewModelService.GetTmkCostDetail(id);
            return PartialView("_DetailTmkCostTrack", model);
        }

        #endregion

        #region Tree Node Detail - Fixed
        public async Task<IActionResult> FixedFolderDetail(string id)
        {
            var model = await _docViewModelService.GetFixedFolderView(id);
            return PartialView("_DetailFixedFolder", model);
        }

        public async Task<IActionResult> ImageDetail(string id)
        {
            var model = await _docViewModelService.GetFixedDocDetail<DocImageDetailDTO>(id);
            // mark viewable/linkable file
            if (model.IsFile)
            {
                var settings = await _settings.GetSetting();
                var viewableExts = settings.ViewableDocs.Split("|");
                if (viewableExts.Any(x => model.DocFileName.ToLower().EndsWith(x)))
                    model.IsDocViewable = true;
            }
            if (!string.IsNullOrEmpty(model.DocUrl))
                model.IsDocLinkable = true;
            
            model.DocumentLink = id;
            return PartialView("_DetailImage", model);
        }

        public async Task<IActionResult> LetterDetail(string id)
        {
            var model = await _docViewModelService.GetFixedDocDetail<DocLetterLogDetailDTO>(id);
            model.DocumentLink = id;
            return PartialView("_DetailLetter", model);
        }

        public async Task<IActionResult> QEDetail(string id)
        {
            var model = await _docViewModelService.GetFixedDocDetail<DocQELogDetailDTO>(id);
            model.DocumentLink = id;
            return PartialView("_DetailQE", model);
        }

        public async Task<IActionResult> EFSDetail(string id)
        {
            var model = await _docViewModelService.GetFixedDocDetail<DocEFSLogDetailDTO>(id);
            model.DocumentLink = id;
            return PartialView("_DetailEFS", model);
        }

        public async Task<IActionResult> IDSDetail(string id)
        {
            if (id.Contains("|rc~"))
            {
                var model = await _docViewModelService.GetIDSDetail<DocIDSRelCasesDTO>(id);
                model.DocumentLink = id;
                return PartialView("_DetailIDSRelCases", model);
            }
            else
            {
                var model = await _docViewModelService.GetIDSDetail<DocIDSNonPatLitDTO>(id);
                model.DocumentLink = id;
                return PartialView("_DetailIDSNonPatLit", model);
            }
        }

        #endregion

        #region Tree Node Detail - User

        public IActionResult UserFolderDetail(string id)
        {
            var model = _docViewModelService.GetUserFolderView(id);

            return PartialView("_DetailUserFolder", model);
        }

        public IActionResult UserFolderDetailEdit(string id)
        {
            var model = _docViewModelService.GetUserFolderView(id);
            return PartialView("_DetailUserFolderEdit", model);
        }

        public async Task<IActionResult> UserDocumentDetail(string id)
        {
            var docId = _docViewModelService.GetNodeId(id);
            var model = await _docViewModelService.CreateDocumentEditorViewModel(0, docId);
            return PartialView("_DetailUserDocument", model);
        }

        #endregion

        #region Tree Node Document View
        public async Task<IActionResult> NodeDocumentView(string id)
        {
            var result = await _docService.GetDocViewInfo(id);
            return Json(result);
        }
        #endregion

        public async Task<IActionResult> GetScreenPicklistData(string systemType)
        {
            var result = await _docViewModelService.GetScreenList(systemType);
            return Json(result);
        }


    }
}
