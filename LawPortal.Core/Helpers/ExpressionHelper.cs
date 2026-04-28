using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LawPortal.Core
{
    public static class ExpressionHelper
    {

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> BuildAnyPredicate<T>(string collection, Expression predicate = null)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
            MemberExpression me = Expression.PropertyOrField(parameterExpression, collection);

            var propertyType = ((PropertyInfo)me.Member).PropertyType;

            MethodCallExpression expression;
            if (predicate != null)
                expression = Expression.Call(typeof(Enumerable), "Any", new[] { propertyType.GenericTypeArguments[0] }, me, predicate); //any with condition
            else
                expression = Expression.Call(typeof(Enumerable), "Any", new[] { propertyType.GenericTypeArguments[0] }, me);

            var lambda = Expression.Lambda<Func<T, bool>>(expression, parameterExpression);
            return lambda;
        }

       
    }
}
