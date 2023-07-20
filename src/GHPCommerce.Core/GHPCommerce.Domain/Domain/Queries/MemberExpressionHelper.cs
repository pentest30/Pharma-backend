using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GHPCommerce.Domain.Domain.Queries
{
    public static class MemberExpressionHelper
    {
        public static MemberExpression GetMemberExpression(PropertyInfo pi, ParameterExpression param)
        {
            MemberExpression property = Expression.Property(param, pi);

            if (pi.PropertyType != typeof(DateTime?) && pi.PropertyType != typeof(DateTime) &&
                pi.PropertyType != typeof(DateTimeOffset) && pi.PropertyType != typeof(DateTimeOffset?))
                return Nullable.GetUnderlyingType(pi.PropertyType) != null
                    ? Expression.Property(property, "Value")
                    : property;
            return pi.PropertyType == typeof(DateTime)
                                   || pi.PropertyType == typeof(DateTimeOffset)
                ? Expression.Property(property, "Date")
                : Expression.Property(Expression.Property(property, "Value"), "Date");
          
        }
        public static Expression<Func<TModel, T>> GenerateMemberExpression<TModel, T>(string propertyName)
        {
            var propertyInfo = typeof(TModel).GetProperty(propertyName);
        
            var entityParam = Expression.Parameter(typeof(TModel), "e"); 
            Expression columnExpr = Expression.Property(entityParam, propertyInfo);
        
            if (propertyInfo.PropertyType != typeof(T))
                columnExpr = Expression.Convert(columnExpr, typeof(T));
        
            return Expression.Lambda<Func<TModel, T>>(columnExpr, entityParam);
        }
    }
}