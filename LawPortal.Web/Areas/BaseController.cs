using Microsoft.AspNetCore.Mvc;
using LawPortal.Core.Entities;
using LawPortal.Core;
using LawPortal.Core.Identity;
using LawPortal.Infrastructure.Identity;
using LawPortal.Web.Filters;
using LawPortal.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Microsoft.EntityFrameworkCore;
using LawPortal.Core.Helpers;
using LawPortal.Core.Exceptions;
using LawPortal.Web.Extensions.ActionResults;
using ActiveQueryBuilder.View.DatabaseSchemaView;

namespace LawPortal.Web.Areas
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

        /// <summary>
        /// Pulls any "Year" criteria out of the search filter list (single value
        /// or a JSON array of values) and returns the parsed years, removing the
        /// Year entries from the list. Year is an int, which the generic
        /// string-oriented criteria builder can't filter, so callers apply the
        /// returned years directly, e.g. Where(x =&gt; years.Contains(x.Year)).
        /// </summary>
        protected static List<int> ExtractYearFilter(List<LawPortal.Web.Areas.Shared.ViewModels.QueryFilterViewModel> filters)
        {
            var years = new List<int>();
            if (filters == null) return years;

            foreach (var f in filters.Where(f => string.Equals(f.Property, "Year", StringComparison.OrdinalIgnoreCase)
                                                 && !string.IsNullOrWhiteSpace(f.Value)))
            {
                var v = f.Value.Trim();
                if (v.StartsWith("[") && v.EndsWith("]"))
                {
                    // Multi-select posts a JSON array whose elements may be numbers
                    // (2026) or strings ("2026") depending on the widget — handle both.
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(v);
                        foreach (var el in doc.RootElement.EnumerateArray())
                        {
                            if (el.ValueKind == System.Text.Json.JsonValueKind.Number && el.TryGetInt32(out var ni))
                                years.Add(ni);
                            else if (el.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(el.GetString(), out var si))
                                years.Add(si);
                        }
                    }
                    catch { /* ignore malformed array */ }
                }
                else if (int.TryParse(v, out var y))
                {
                    years.Add(y);
                }
            }

            filters.RemoveAll(f => string.Equals(f.Property, "Year", StringComparison.OrdinalIgnoreCase));
            return years;
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
