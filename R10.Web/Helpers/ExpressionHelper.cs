using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using ClosedXML;
using DocumentFormat.OpenXml.Vml;
using Kendo.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Newtonsoft.Json;
using R10.Core;
using R10.Web.Areas.Shared.ViewModels;

namespace R10.Web.Helpers
{
    public static class ExpressionHelper
    {
        
        public static Expression<Func<T, object>> GetPropertyExpression<T>(string property)
        {
            if (!QueryHelper.PropertyExists<T>(property))
                return null;

            var parameterExpression = Expression.Parameter(typeof(T), "c");
            var expression = Expression.PropertyOrField(parameterExpression, property);
            return Expression.Lambda<Func<T, object>>(Expression.Convert(expression, typeof(object)), new[] { parameterExpression });
        }

        public static Expression<Func<T, string>> GetStringPropertyExpression<T>(string property)
        {
            if (!QueryHelper.PropertyExists<T>(property))
                return null;

            var parameterExpression = Expression.Parameter(typeof(T), "c");
            return (Expression<Func<T, string>>)Expression.Lambda(Expression.PropertyOrField(parameterExpression, property), parameterExpression);
        }

        public static Expression<Func<T, DateTime?>> GetNullableDateTimePropertyExpression<T>(string property)
        {
            if (!QueryHelper.PropertyExists<T>(property))
                return null;

            var parameterExpression = Expression.Parameter(typeof(T));
            return (Expression<Func<T, DateTime?>>)Expression.Lambda(Expression.PropertyOrField(parameterExpression, property), parameterExpression);
        }

        public static Expression<Func<T, DateTime>> GetDateTimePropertyExpression<T>(string property)
        {
            if (!QueryHelper.PropertyExists<T>(property))
                return null;

            var parameterExpression = Expression.Parameter(typeof(T));
            return (Expression<Func<T, DateTime>>)Expression.Lambda(Expression.PropertyOrField(parameterExpression, property), parameterExpression);
        }

        public static Expression<Func<T, bool>> GetBooleanPropertyExpression<T>(string property)
        {
            if (!QueryHelper.PropertyExists<T>(property))
                return null;

            var parameterExpression = Expression.Parameter(typeof(T));
            return (Expression<Func<T, bool>>)Expression.Lambda(Expression.PropertyOrField(parameterExpression, property), parameterExpression);
        }

        public static Expression<Func<T, int>> GetIntPropertyExpression<T>(string property)
        {
            if (!QueryHelper.PropertyExists<T>(property))
                return null;

            var parameterExpression = Expression.Parameter(typeof(T));
            return (Expression<Func<T, int>>)Expression.Lambda(Expression.PropertyOrField(parameterExpression, property), parameterExpression);
        }

        public static Expression<Func<T, bool>> BuildPredicate<T>(string property, string filterValue, bool addNullCheck = true, string filterOperator = "like")
        {
            //ex. c => c.ClientCode = 'ABC'

            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
            var expression = BuildExpression(parameterExpression, property, filterValue, addNullCheck, filterOperator);
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(expression, parameterExpression);
            return lambda;
        }

        public static Expression<Func<T, bool>> BuildNestedPredicate<T>(string property, string filterValue, bool addNullCheck = true, string filterOperator = "like")
        {
            //ex. c => c.Application.Invention.Client = "ABC"

            var parameterExpression = Expression.Parameter(typeof(T));
            string childMember = string.Empty;
          
            Expression body = parameterExpression;
            foreach (var member in property.Split('.'))
            {
                if (property.EndsWith(member))
                {
                    var isList = QueryHelper.IsIEnumerableOfT(body.Type);
                    if (isList)
                    {
                        Type listItemType = body.Type.GetGenericArguments().Single();
                        var predicate = BuildPredicate(listItemType, member, filterValue, false, filterOperator);
                        return Core.ExpressionHelper.BuildAnyPredicate<T>(childMember, predicate);
                    }

                    var expression = BuildExpression(body, member, filterValue, addNullCheck, filterOperator);
                    Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(expression, parameterExpression);
                    return lambda;
                }
                else
                {
                        body = Expression.PropertyOrField(body, member);
                        childMember = member;
                }
            }
            return null;
        }

        public static Expression<Func<T, bool>> BuildLoopNestedPredicate<T>(string property, List<string> filterValue, bool addNullCheck = true, string filterOperator = "like")
        {
            //ex. c => c.Application.Invention.Client = "ABC"

            var parameterExpression = Expression.Parameter(typeof(T));
            string childMember = string.Empty;

            Expression body = parameterExpression;
            foreach (var member in property.Split('.'))
            {
                if (property.EndsWith(member))
                {
                    var isList = QueryHelper.IsIEnumerableOfT(body.Type);
                    if (isList)
                    {
                        Type listItemType = body.Type.GetGenericArguments().Single();                           
                        var valuePredicate = BuildLoopPredicate(listItemType, member, filterValue, false, filterOperator);
                        var valueAnyPredicate = Core.ExpressionHelper.BuildAnyPredicate<T>(childMember, valuePredicate);
                        return valueAnyPredicate;

                        //Expression<Func<T, bool>> predicate = (item) => false;
                        //foreach (var val in filterValue)
                        //{                            
                        //    var valuePredicate = BuildPredicate(listItemType, member, val, false, filterOperator);
                        //    var valueAnyPredicate = Core.ExpressionHelper.BuildAnyPredicate<T>(childMember, valuePredicate);
                        //    predicate = predicate.Or(valueAnyPredicate);
                        //}
                        //predicate = predicate.Or(valueAnyPredicate);
                        //return predicate;
                    }
                    else
                    {
                        Expression<Func<T, bool>> predicate = (item) => false;
                        foreach (var val in filterValue)
                        {
                            var valueExpression = BuildExpression(body, member, val, false, filterOperator);
                            Expression<Func<T, bool>> valueLambda = Expression.Lambda<Func<T, bool>>(valueExpression, parameterExpression);
                            predicate = predicate.Or(valueLambda);
                        }
                        return predicate;
                    }                    
                }
                else
                {
                    body = Expression.PropertyOrField(body, member);
                    childMember = member;
                }
            }
            return null;
        }

        //for types not known at compile time
        public static Expression BuildPredicate(Type type, string property, string filterValue, bool addNullCheck = true, string filterOperator = "like")
        {
            ParameterExpression parameterExpression = Expression.Parameter(type);
            var expression = BuildExpression(parameterExpression, property, filterValue, addNullCheck, filterOperator);

            var lambda = Expression.Lambda(expression, parameterExpression);
            return lambda;

        }

        public static Expression BuildLoopPredicate(Type type, string property, List<string> filterValues, bool addNullCheck = true, string filterOperator = "like")
        {
            ParameterExpression parameterExpression = Expression.Parameter(type);

            Expression expression = BuildExpression(parameterExpression, property, filterValues[0], addNullCheck, filterOperator);
            filterValues.RemoveAt(0);
            foreach (var filterValue in filterValues)
            {
                var valueExpression = BuildExpression(parameterExpression, property, filterValue, addNullCheck, filterOperator);
                expression = Expression.OrElse(expression, valueExpression);
            }            

            var lambda = Expression.Lambda(expression, parameterExpression);
            return lambda;

        }

        public static Expression BuildExpression(Expression parameterExpression, string property, string filterValue, bool addNullCheck = true, string filterOperator = "like")
        {
            MemberExpression me = Expression.Property(parameterExpression, property); //the member you want to evaluate (c => c.ClientCode)
            var propertyType = ((PropertyInfo)me.Member).PropertyType;
            object constantValue = filterValue;

            if (propertyType != typeof(string))
            {
                if (filterOperator == "like")
                    filterOperator = "eq";

                if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                {
                    DateTime.TryParse(filterValue, out var dateValue);
                    constantValue = dateValue;
                }
                else if (propertyType == typeof(Int32) || propertyType == typeof(Int32?))
                {
                    int.TryParse(filterValue, out var intValue);
                    constantValue = intValue;
                }
                else if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                {
                    bool.TryParse(filterValue, out var boolValue);
                    constantValue = boolValue;
                }
                else if (propertyType == typeof(Decimal) || propertyType == typeof(Decimal?))
                {
                    decimal.TryParse(filterValue, out var decimalValue);
                    constantValue = decimalValue;
                }
                else if (propertyType == typeof(Double) || propertyType == typeof(Double?))
                {
                    double.TryParse(filterValue, out var doubleValue);
                    constantValue = doubleValue;
                }
            }


                ConstantExpression value = Expression.Constant(constantValue, propertyType); //the value ex. 'ABC'
            Expression expression = GetFilterExpression(filterOperator, me, value);
            if (addNullCheck)
            {
                var notNull = Expression.NotEqual(me, Expression.Constant(null, typeof(object)));
                expression = Expression.AndAlso(notNull, expression);
            }

            return expression;
        }

        public static Expression<Func<T, bool>> BuildNotNullPredicate<T>(string property)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
            MemberExpression me = Expression.Property(parameterExpression, property);
            ConstantExpression value = Expression.Constant(null, typeof(object));
            Expression expression = Expression.NotEqual(me, value);

            var lambda = Expression.Lambda<Func<T, bool>>(expression, parameterExpression);
            return lambda;
        }

        public static Expression<Func<T, bool>> BuildNotEmptyStringPredicate<T>(string property)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
            MemberExpression me = Expression.Property(parameterExpression, property);
            ConstantExpression value = Expression.Constant("");
            Expression expression = Expression.NotEqual(me, value);

            var lambda = Expression.Lambda<Func<T, bool>>(expression, parameterExpression);
            return lambda;
        }

        public static Expression GetFilterExpression(string filterOperator, MemberExpression me, ConstantExpression value)
        {
            Expression expression;

            switch (GetFilterOperatorFromString(filterOperator))
            {
                case FilterOperator.Equals:
                    expression = Expression.Equal(me, value);
                    break;
                case FilterOperator.Contains:
                    expression = Expression.Call(me, containsMethod, value);
                    break;
                case FilterOperator.GreaterThan:
                    expression = Expression.GreaterThan(me, value);
                    break;
                case FilterOperator.GreaterThanOrEqual:
                    expression = Expression.GreaterThanOrEqual(me, value);
                    break;
                case FilterOperator.LessThan:
                    expression = Expression.LessThan(me, value);
                    break;
                case FilterOperator.LessThanOrEqualTo:
                    expression = Expression.LessThanOrEqual(me, value);
                    break;
                case FilterOperator.StartsWith:
                    expression = Expression.Call(me, startsWithMethod, value);
                    break;
                case FilterOperator.EndsWith:
                    expression = Expression.Call(me, endsWithMethod, value);
                    break;
                case FilterOperator.Like:
                    expression = Expression.Call(null, likeMethod, Expression.Constant(EF.Functions), me, value);
                    break;
                case FilterOperator.NotEqual:
                    expression = Expression.NotEqual(me, value);
                    break;
                default:
                    expression = Expression.Equal(me, value);
                    break;

            }
            return expression;
        }
        
        public static List<string> GetValueList(this QueryFilterViewModel model)
        {
            var values = new List<string>();
            try
            {
                values = JsonConvert.DeserializeObject<List<string>>(model.Value);
            }
            catch
            {
                //always use contains with neq operator
                if (model.Operator == "neq")
                    values = new List<string> { model.Value };
            }

            return values;
        }

        public static List<string> GetValueListForLoop(this QueryFilterViewModel model)
        {
            var values = new List<string>();
            try
            {
                values = (JsonConvert.DeserializeObject<List<string>>(model.Value) ?? new List<string>()).Select(d => d.Replace("*", "%").Replace("?", "_").Replace("[", "[[]")).ToList();
            }
            catch
            {
                //always use contains with neq operator
                if (!string.IsNullOrEmpty(model.Value))
                    values = new List<string> { model.Value };
            }

            return values ?? new List<string>();
        }

        public static string GetFilterOperator(this List<QueryFilterViewModel> mainSearchFilters, string name)
        {
            var filterOperator = mainSearchFilters.FirstOrDefault(f => f.Property == name);
            if (filterOperator != null)
            {
                mainSearchFilters.Remove(filterOperator);
                return filterOperator.Value;
            }

            return "eq";
        }

        private static FilterOperator GetFilterOperatorFromString(string filterOperator)
        {
            switch (filterOperator.ToLower())
            {
                case "eq":
                    return FilterOperator.Equals;
                case "contains":
                    return FilterOperator.Contains;
                case "gt":
                    return FilterOperator.GreaterThan;
                case "gte":
                    return FilterOperator.GreaterThanOrEqual;
                case "lt":
                    return FilterOperator.LessThan;
                case "lte":
                    return FilterOperator.LessThanOrEqualTo;
                case "start":
                    return FilterOperator.StartsWith;
                case "end":
                    return FilterOperator.EndsWith;
                case "any":
                    return FilterOperator.Any;
                case "like":
                    return FilterOperator.Like;
                case "neq":
                    return FilterOperator.NotEqual;
                default:
                    return FilterOperator.Equals;
            }
        }

        private static MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        private static MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        private static MethodInfo endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
        private static MethodInfo likeMethod = typeof(DbFunctionsExtensions).GetMethod("Like", new[] { typeof(DbFunctions), typeof(string), typeof(string) });


        public enum FilterOperator
        {
            Contains,
            GreaterThan,
            GreaterThanOrEqual,
            LessThan,
            LessThanOrEqualTo,
            StartsWith,
            EndsWith,
            Equals,
            NotEqual,
            Any,
            Like
        }
    }
}
