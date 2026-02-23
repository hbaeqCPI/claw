using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using R10.Core.DTOs;
using R10.Core.Interfaces.Shared;
using R10.Web.Extensions.ActionResults;
using R10.Web.Filters;
using R10.Web.Models;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared")]
    [ServiceFilter(typeof(ExceptionFilter))]
    public class WebLinksController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IWebLinksService _service;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public WebLinksController(IWebLinksService service, IStringLocalizer<SharedResource> localizer)
        {
            _service = service;
            _localizer = localizer;
        }

        public async Task<IActionResult> Search([DataSourceRequest] DataSourceRequest request, int id,string module,string subModule="FormLink", string subSystem="", int displayChoice=0)
        {
            var links = (await _service.GetWebLinks(id, module, subModule, subSystem));
            List<WebLinksDTO> filteredLinks;

            switch (displayChoice) {
                case 0:
                    filteredLinks = links.Where(w => w.RecordLink).ToList();
                    break;
                case 1:
                    filteredLinks = links.Where(w => !w.RecordLink).ToList();
                    break;

                default:
                    filteredLinks = links.Any(w=>w.RecordLink) ? links.Where(w => w.RecordLink).ToList() : links;
                    break;
            }
            
            var result = filteredLinks.ToDataSourceResult(request);
            return Json(result);
        }

        [HttpGet()]
        public async Task<IActionResult> Navigate(int mainId, int id, string module, string subModule = "FormLink", string subSystem = "",bool linkOnly=false)
        {
            var url = await _service.GetWebLinksUrl(mainId, id, module, subModule, subSystem);
            if (linkOnly) { 
                return Content(url);
            }
            return Redirect(url);
        }

        public async Task<IActionResult> GetCPCHelpUrl()
        {
            var result = await _service.GetUrl("US-CPCCode");
            return Content(result);
        }

        public async Task<IActionResult> GetIPCHelpUrl()
        {
            var result = await _service.GetUrl("WO-IPCCode");
            return Content(result);
        }
        

        public async Task<IActionResult> GetPatStatHelpUrl()
        {
            var result = await _service.GetUrl("EP-PatStat");
            return Content(result);
        }

        private async Task<string> GetUrl(int id, string module, string subModule = "FormLink", string subSystem = "")
        {
            var links = await _service.GetWebLinks(id, module, subModule, subSystem);
            var filteredLinks = links.Any(w => w.RecordLink) ? links.Where(w => w.RecordLink).ToList() : links;
            var mainId = filteredLinks.Select(l => l.MainId).FirstOrDefault();
            var url = string.Empty;

            if (mainId > 0)
                url = await _service.GetWebLinksUrl(mainId, id, module, subModule, subSystem);

            return url;
        }

        public async Task<IActionResult> GetWebLinksUrl(int id, string module, string subModule = "FormLink", string subSystem = "")
        {
            var url = await GetUrl(id, module, subModule, subSystem);

            if (string.IsNullOrEmpty(url))
                return new JsonBadRequest(_localizer["Unable to find weblink for this record."]);

            return Json(new { url });
        }

        public async Task<IActionResult> OpenWebLinks(int id, string module, string subModule = "FormLink", string subSystem = "")
        {
            var url = await GetUrl(id, module, subModule, subSystem);

            if (string.IsNullOrEmpty(url))
                return new JsonBadRequest(_localizer["Unable to find weblink for this application"]);

            return Redirect(url);
        }

        public async Task<IActionResult> AMS(int id)
        {
            return await OpenWebLinks(id, "AMS", "TicklerLink");
        }

        public async Task<IActionResult> Patent(int id)
        {
            return await OpenWebLinks(id, "Patent");
        }

        public async Task<IActionResult> Trademark(int id)
        {
            return await OpenWebLinks(id, "Trademark");
        }
    }
}