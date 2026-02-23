using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Filters;
using R10.Web.Interfaces;

namespace R10.Web.Api.Shared
{
    [ApiController]
    [ServiceFilter(typeof(RequestHeaderFilter))]
    [Route("api/sharepoint/[action]")]
    public class SharePointImageController : Controller
    {
        private readonly ISharePointViewModelService _sharePointViewModelService;

        public SharePointImageController(ISharePointViewModelService sharePointViewModelService)
        {
            _sharePointViewModelService = sharePointViewModelService;
        }

        [HttpGet]
        [ActionName("GetReportImages")]
        public async Task<List<SharePointReportImage>> GetReportImages(string data)
        {
            return await _sharePointViewModelService.GetReportImages(data);
        }

        [HttpGet]
        [ActionName("GetReportImageFile")]
        public async Task<IActionResult> GetReportImageFile(string system, string itemId, string fileName)
        {
            var file = await _sharePointViewModelService.GetReportImageFile(system, itemId, fileName);
            return File(file.Bytes, file.ContentType, file.FileName);
        }

        [HttpGet]
        [ActionName("GetReportDefaultImagesForPrintScreen")]
        public async Task<List<SharePointReportImage>> GetReportDefaultImagesForPrintScreen(string system, string module, string data)
        {
            return await _sharePointViewModelService.GetReportDefaultImagesForPrintScreen(system, module, data);
        }

        [HttpGet]
        [ActionName("GetReportImagesListForPrintScreen")]
        public async Task<List<SharePointImageList>> GetReportImagesListForPrintScreen(string system, string module, string data)
        {
            return await _sharePointViewModelService.GetReportImagesListForPrintScreen(system, module, data);
        }
    }
}
