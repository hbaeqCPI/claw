using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using R10.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Shared;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Shared;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Security;
using R10.Web.Services.DocumentStorage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class ProductImageController : BaseController, IImageController
    {
        private readonly IProductImageViewModelService _imageViewModelService;
        private readonly IDocumentsViewModelService _docViewModelService;

        public ProductImageController(
                IProductImageViewModelService imageViewModelService,
                IDocumentsViewModelService docViewModelService,
                IDocumentService docService) 
        {
            _imageViewModelService = imageViewModelService;
            _docViewModelService = docViewModelService;
        }

        public async Task<IActionResult> ImageRead([DataSourceRequest] DataSourceRequest request, int parentId, List<QueryFilterViewModel> criteria)
        {
            if (ModelState.IsValid)
            {
                var images = await _imageViewModelService.CreateViewModelForList(parentId);
                if (criteria.Count > 0)
                {
                    images = await _docViewModelService.ApplyCriteria(images, criteria);
                    var filteredImages = QueryHelper.BuildCriteria(images.AsQueryable(), criteria);
                    return Json(filteredImages.ToDataSourceResult(request));
                }
                return Json(images.ToDataSourceResult(request));
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        public async Task<IActionResult> GetImageSearchData(string property, string text, FilterType filterType, string requiredRelation = "", int parentId = 0)
        {
            var images = (await _imageViewModelService.CreateViewModelForList(parentId)).AsQueryable();
            images = QueryHelper.BuildCriteria(images, property, text, filterType, requiredRelation);
            var propertyExpression = ExpressionHelper.GetStringPropertyExpression<DocDocumentListViewModel>(property);
            var result = images.GroupBy(propertyExpression).Select(x => x.First()).OrderBy(property).ToList(); //distinct not working
            return Json(result);
        }

    }
}