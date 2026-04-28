using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using LawPortal.Core;
using LawPortal.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using LawPortal.Web.Areas;
using System.Text;

namespace LawPortal.Web.Helpers
{
    public static class QueryHelper
    {
        #region Automapper helpers to fix version 11.0.1 breaking changes
        public static MapperConfiguration MapperConfig = new MapperConfiguration(cfg => {
            cfg.AddProfile(new AutoMapperPatentProfileConfig());
            cfg.AddProfile(new AutoMapperSharedProfileConfig());
            cfg.AddProfile(new AutoMapperTrademarkProfileConfig());
            cfg.AddProfile(new AutoMapperAdminProfileConfig());
        });

        public static IQueryable<T> ProjectTo<T>(this IQueryable queryable)
        {
            return queryable.ProjectTo<T>(MapperConfig);
        }

        public static IQueryable<T> ProjectTo<T>(this IQueryable queryable, object parameters)
        {
            return queryable.ProjectTo<T>(MapperConfig, parameters);
        }
        #endregion

        public static IQueryable BuildQueryablePicklist<T>(IQueryable<T> source, string property, string filter, FilterType filterType = FilterType.StartsWith, string requiredRelation = "", bool distinct = true)
        {
            var propertyExpression = ExpressionHelper.GetStringPropertyExpression<T>(property);
            if ((propertyExpression) == null)
                return null;

            var result = ApplyFilter(source, property, filter, filterType, requiredRelation, distinct).OrderBy(propertyExpression) as IQueryable<T>;

            if (distinct)
                return result.Select(propertyExpression).Distinct().OrderBy(s=> s);
            else
                return result.Select(propertyExpression).OrderBy(s=> s);

        }

        public static IQueryable<string> BuildQueryableStringPicklist<T>(IQueryable<T> source, string property, string filter, FilterType filterType = FilterType.StartsWith, string requiredRelation = "", bool distinct = true)
        {
            var propertyExpression = ExpressionHelper.GetStringPropertyExpression<T>(property);
            if ((propertyExpression) == null)
                return null;

            var result = ApplyFilter(source, property, filter, filterType, requiredRelation, distinct).OrderBy(propertyExpression) as IQueryable<T>;

            if (distinct)
                return result.Select(propertyExpression).Distinct().OrderBy(s => s);
            else
                return result.Select(propertyExpression).OrderBy(s => s);

        }

        public async static Task<List<T>> GetPicklistDataAsync<T>(IQueryable<T> source, string property, string filter, FilterType filterType = FilterType.StartsWith, string requiredRelation = "")
        {
            return await source
                            .BuildCriteria(property, filter, filterType, requiredRelation)
                            .OrderBy(property)
                            .Select<T>(property)
                            .Distinct()
                            .ToListAsync();
        }

        public static IQueryable<T> BuildCriteria<T>(this IQueryable<T> source, string property, string filter, FilterType filterType = FilterType.StartsWith, string requiredRelation = "")
        {
            var result = ApplyFilter(source, property, filter, filterType, requiredRelation, true);

            if (typeof(T).GetProperty(property)?.PropertyType == typeof(string))
            {
                result = result.Where(ExpressionHelper.BuildNotEmptyStringPredicate<T>(property));
            }

            return result;
        }

        public static IQueryable<T> BuildCriteria<T>(this IQueryable<T> source, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (mainSearchFilters == null) return source;

            ApplyDefaultCriteriaSettings(mainSearchFilters);

            foreach (var filter in mainSearchFilters)
            {
                // Skip filters with no value
                if (string.IsNullOrEmpty(filter.Value)) continue;

                var propertyName = filter.Property.StartsWith("MultiSelect_")
                    ? filter.Property.Replace("MultiSelect_", "")
                    : filter.Property;
                bool isMultiValue = filter.Property.StartsWith("MultiSelect_")
                    || (filter.Value.StartsWith("[") && filter.Value.EndsWith("]"));

                if (isMultiValue)
                {
                    var parsedFilterValue = filter.GetValueListForLoop();
                    if (parsedFilterValue.Count > 0)
                    {
                        // Build WHERE property IN (val1, val2, ...) using Contains
                        var param = Expression.Parameter(typeof(T));
                        var prop = propertyName.Split('.').Aggregate((Expression)param, Expression.PropertyOrField);
                        var listExpr = Expression.Constant(parsedFilterValue);
                        var containsCall = Expression.Call(listExpr, typeof(List<string>).GetMethod("Contains", new[] { typeof(string) })!, prop);
                        var notNull = Expression.NotEqual(prop, Expression.Constant(null, typeof(string)));
                        var combined = Expression.AndAlso(notNull, containsCall);
                        source = source.Where(Expression.Lambda<Func<T, bool>>(combined, param));
                    }
                }
                else if (filter.Property.Contains("."))
                    source = source.Where(ExpressionHelper.BuildNestedPredicate<T>(filter.Property, filter.Value, false, filter.Operator));
                else
                    source = source.Where(ExpressionHelper.BuildPredicate<T>(filter.Property, filter.Value, false, filter.Operator));
            }
            return source;
        }

        public static IQueryable<T> Select<T>(this IQueryable source, string property)
        {
            return source.Select<T>(new string[] { property });
        }

        //TODO: TRY TO FIX --> Returns entire schema of T with only the passed columns populated
        public static IQueryable<T> Select<T>(this IQueryable source, params string[] columns)
        {
            var sourceType = source.ElementType;
            var resultType = typeof(T);
            var parameter = Expression.Parameter(sourceType, "e");
            var bindings = columns.Select(column => Expression.Bind(
                resultType.GetProperty(column), Expression.PropertyOrField(parameter, column)));
            var body = Expression.MemberInit(Expression.New(resultType), bindings);
            var selector = Expression.Lambda(body, parameter);
            return source.Provider.CreateQuery<T>(
                Expression.Call(typeof(Queryable), "Select", new Type[] { sourceType, resultType },
                    source.Expression, Expression.Quote(selector)));
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
        {
            return source.OrderBy(ExpressionHelper.GetPropertyExpression<T>(property));
        }

        public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
        {
            return source.OrderByDescending(ExpressionHelper.GetPropertyExpression<T>(property));
        }       

        private static string BuildFilter(string filter, FilterType filterType) {
            switch (filterType)
            {
                case FilterType.StartsWith:
                    filter = filter + "%";
                    break;
                case FilterType.EndsWith:
                    filter = "%" + filter;
                    break;
                default:
                    filter = "%" + filter + "%";
                    break;
            }
            return filter;
        }

        public static bool IsIEnumerableOfT(Type type)
        {
            return type.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        public static bool PropertyExists<T>(string propertyName)
        {
            return typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) != null;
        }

        public static Type GetPropertyType<T>(string propertyName)
        {
            var property = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            return property.PropertyType;
        }
      
        private static IQueryable<T> ApplyFilter<T>(IQueryable<T> source, string property, string filter, FilterType filterType = FilterType.StartsWith, string requiredRelation = "", bool distinct = true)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                filter = BuildFilter(filter, filterType);
                source = source.Where(ExpressionHelper.BuildPredicate<T>(property, filter, true)); //includes not null checking
            }
            else
                source = source.Where(ExpressionHelper.BuildNotNullPredicate<T>(property));

            if (!string.IsNullOrEmpty(requiredRelation))
            {
                source = source.Where(Core.ExpressionHelper.BuildAnyPredicate<T>(requiredRelation));
            }
            return source;
        }
        
        private static void ApplyDefaultCriteriaSettings(List<QueryFilterViewModel> mainSearchFilters)
        {
            if (mainSearchFilters == null) return;

            DateTime dateTime;

            // Remove filters with empty values before processing
            mainSearchFilters.RemoveAll(f => string.IsNullOrEmpty(f.Value));

            foreach (var filter in mainSearchFilters)
            {
                filter.Value = filter.Value.Replace("*", "%").Replace("?", "_");

                if (!string.IsNullOrEmpty(filter.Operator))
                    continue;

                var propertyLower = filter.Property.ToLower();
                if ((propertyLower.EndsWith("from") || propertyLower.EndsWith("to")) && DateTime.TryParse(filter.Value, out dateTime))
                {
                    if (propertyLower.EndsWith("from"))
                    {
                        filter.Property = filter.Property.Substring(0, filter.Property.Length - 4);
                        filter.Operator = "gte";
                    }
                    else {
                        filter.Property = filter.Property.Substring(0, filter.Property.Length - 2);
                        filter.Operator = "lte";
                        dateTime = dateTime.AddDays(1).AddSeconds(-1);
                    }
                    filter.Value = dateTime.ToString("o"); // ISO 8601 format for reliable parsing
                }
                else {
                    filter.Operator = "like";
                }

            }
        }

        /// <summary>
        /// Extracts and removes the SystemName filter from mainSearchFilters,
        /// then applies a Systems LIKE filter (supports single value or JSON array of values).
        /// </summary>
        public static IQueryable<T> ApplySystemsFilter<T>(IQueryable<T> source, List<QueryFilterViewModel> mainSearchFilters, Expression<Func<T, string>> systemsProperty)
        {
            var systemName = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemName");
            if (systemName != null && !string.IsNullOrEmpty(systemName.Value))
            {
                var values = new List<string>();
                if (systemName.Value.StartsWith("[") && systemName.Value.EndsWith("]"))
                {
                    try { values = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(systemName.Value); }
                    catch { values = new List<string> { systemName.Value }; }
                }
                else
                {
                    values.Add(systemName.Value);
                }

                // Build: x => Systems.Contains(v1) || Systems.Contains(v2) ...
                var param = systemsProperty.Parameters[0];
                var body = systemsProperty.Body;
                Expression orExpr = null;
                foreach (var v in values)
                {
                    var clean = v.Replace("%", "");
                    var likePattern = Expression.Constant("%" + clean + "%");
                    var likeCall = Expression.Call(
                        typeof(DbFunctionsExtensions), "Like", Type.EmptyTypes,
                        Expression.Constant(EF.Functions), body, likePattern);
                    var notNull = Expression.NotEqual(body, Expression.Constant(null, typeof(string)));
                    var combined = Expression.AndAlso(notNull, likeCall);
                    orExpr = orExpr == null ? combined : Expression.OrElse(orExpr, combined);
                }
                if (orExpr != null)
                    source = source.Where(Expression.Lambda<Func<T, bool>>(orExpr, param));
            }
            if (systemName != null) mainSearchFilters.Remove(systemName);
            return source;
        }

        public static string GetToken(this List<QueryFilterViewModel> mainSearchFilters)
        {
            var token = mainSearchFilters.FirstOrDefault(f => f.Property == "Token");
            if (token != null)
            {
                mainSearchFilters.Remove(token);
                return token.Value;
            }

            return "";
        }

        public static DateTime? GetInstructByDate(this List<QueryFilterViewModel> mainSearchFilters)
        {
            var token = mainSearchFilters.FirstOrDefault(f => f.Property == "InstructByDate");
            if (token != null)
            {
                mainSearchFilters.Remove(token);
                return DateTime.Parse(token.Value);
            }

            return null;
        }

        public static IQueryable<T> BuildOrCriteria<T>(this IQueryable<T> source, List<QueryFilterViewModel> mainSearchFilters)
        {
            Expression<Func<T, bool>> predicate = (item) => false;
            foreach (var filter in mainSearchFilters)
            {
                predicate = predicate.Or(LawPortal.Web.Helpers.ExpressionHelper.BuildPredicate<T>(filter.Property, filter.Value, false, filter.Operator));
            }
            return source.Where(predicate);
        }


        public static string ExtractSignificantNumbers(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var output = new StringBuilder();
            char prevChar = '\0';

            for (int i = 0; i < input.Length; i++)
            {
                char currentChar = input[i];

                if (char.IsDigit(currentChar))
                {
                    // Exclude if it's the last character and preceded by a letter or a dot (to avoid KD or checkdigit)
                    if (!(i == input.Length - 1 && (char.IsLetter(prevChar) || prevChar == '.')))
                    {
                        output.Append(currentChar);
                    }
                }
                
                //wild card
                else if (currentChar == '%') {
                    output.Append(currentChar);
                }

                prevChar = currentChar;
            }

            return output.ToString();
        }

    }
}
