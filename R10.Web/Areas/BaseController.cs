using Microsoft.AspNetCore.Mvc;
using R10.Core.Entities;
using R10.Core;
using R10.Core.Identity;
using R10.Infrastructure.Identity;
using R10.Web.Filters;
using R10.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Microsoft.EntityFrameworkCore;
using R10.Core.Helpers;
using R10.Core.Exceptions;
using R10.Web.Extensions.ActionResults;
using ActiveQueryBuilder.View.DatabaseSchemaView;

namespace R10.Web.Areas
{
    [ServiceFilter(typeof(ExceptionFilter))]
    public class BaseController: Microsoft.AspNetCore.Mvc.Controller
    {
        protected void UpdateEntityStamps(BaseEntity entity, int entityId)
        {
            var userName = User.GetUserName();
            var now = DateTime.Now;

            entity.UpdatedBy = userName;
            entity.LastUpdate = now;
            if (entityId <= 0)
            {
                entity.CreatedBy = userName;
                entity.DateCreated = now;
            }
        }

        protected void AddDefaultNavigationUrls(DetailPagePermission viewModel) 
        {
            //todo: add id to edit, copy, etc.
            //todo: change search to index
            viewModel.SearchScreenUrl = Url.Action("search"); 
            viewModel.PrintScreenUrl = Url.Action("print");

            viewModel.AddScreenUrl = viewModel.CanAddRecord ? Url.Action("add") : "";
            viewModel.DeleteScreenUrl = viewModel.CanDeleteRecord ? Url.Action("delete") : "";
            viewModel.CopyScreenUrl = viewModel.CanCopyRecord ? Url.Action("copy") : "";
            viewModel.EmailScreenUrl = viewModel.CanEmail ? Url.Action("email") : "";
            viewModel.LetterScreenUrl = viewModel.CanGenerateLetter ? Url.Action("letter") : "";

            if (viewModel.CanEditRecord)
            {
                var action = viewModel.CanEditRemarksOnly ? "editremarks" : "edit";
                viewModel.EditScreenUrl = Url.Action(action);
            }
        }

        protected int GetSearchPageSize()
        {
            return 15; //modify later to read from settings table
        }

        protected async Task<IActionResult> GetPicklistData<T>(IQueryable<T> source, DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "", bool selectProperty = true)
        {
            if (selectProperty)
                return await GetPicklistData(source
                                .BuildCriteria(property, text, filterType, requiredRelation)                                
                                .Select<T>(property)
                                .Distinct()
                                .OrderBy(property), request);
            else
                return await GetPicklistData(source
                                .Distinct()
                                .OrderBy(property)
                                .BuildCriteria(property, text, filterType, requiredRelation), request);
        }

        //TODO: TRY TO FIX --> Returns entire schema of T with only the passed columns populated
        protected async Task<IActionResult> GetPicklistData<T>(IQueryable<T> source, DataSourceRequest request, string property, string text, FilterType filterType, string[] columns, string requiredRelation = "")
        {
           
            var data = source
                            .BuildCriteria(property, text, filterType, requiredRelation)
                            .OrderBy(property)
                            .Select<T>(columns)
                            //.Distinct(); issue with orderby when sql column name is different from entity property name (ie Language vs LanguageName or Client vs ClientCode)
                            ;

            return await GetPicklistData(data, request);
        }

        protected async Task<IActionResult> GetPicklistData<T>(IQueryable<T> data, DataSourceRequest request)
        {
            if (request.PageSize > 0)
            {
                //request.Filters.Clear();
                var list = await data.ToDataSourceResultAsync(request);
                return Json(list);
            }

            return Json(await data.ToListAsync());
        }
    }

}
