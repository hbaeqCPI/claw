using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas.Shared.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IImageController
    {
        Task<IActionResult> ImageRead([DataSourceRequest] DataSourceRequest request, int parentId, List<QueryFilterViewModel> criteria);
        Task<IActionResult> GetImageSearchData(string property, string text, FilterType filterType, string requiredRelation = "", int parentId = 0);
        
    }

}
