using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GHPCommerce.Domain.Domain.Queries
{
    //https://stackoverflow.com/questions/34906437/how-to-construct-order-by-expression-dynamically-in-entity-framework
    public static class OrderByQueryExtension
    {
        public static IQueryable<T> OrderByProperty<T>(this IQueryable<T> source, string propertyName)
        {
            
            if (typeof (T).GetProperty(propertyName, BindingFlags.IgnoreCase | 
                                                     BindingFlags.Public | BindingFlags.Instance) == null)
            {
                return source;
            }
            ParameterExpression parameterExpression = Expression.Parameter(typeof (T));
            Expression orderByProperty = Expression.Property(parameterExpression, propertyName);
            LambdaExpression lambda = Expression.Lambda(orderByProperty, parameterExpression);
            MethodInfo genericMethod = OrderByMethod.MakeGenericMethod(typeof (T), orderByProperty.Type);
            object ret = genericMethod.Invoke(null, new object[] {source, lambda});
            return (IQueryable<T>) ret;
        }
        private static readonly MethodInfo OrderByMethod =
            typeof (Queryable).GetMethods().Single(method => 
                method.Name == "OrderBy" && method.GetParameters().Length == 2);
        private static readonly MethodInfo OrderByDescendingMethod =
            typeof (Queryable).GetMethods().Single(method => 
                method.Name == "OrderByDescending" && method.GetParameters().Length == 2);

        public static IQueryable<T> OrderByPropertyDescending<T>(this IQueryable<T> source, string propertyName)
        {
            if (typeof (T).GetProperty(propertyName, BindingFlags.IgnoreCase | 
                                                     BindingFlags.Public | BindingFlags.Instance) == null)
            {
                return source;
            }
            ParameterExpression parameterExpression = Expression.Parameter(typeof (T));
            Expression orderByProperty = Expression.Property(parameterExpression, propertyName);
            LambdaExpression lambda = Expression.Lambda(orderByProperty, parameterExpression);
            MethodInfo genericMethod = 
                OrderByDescendingMethod.MakeGenericMethod(typeof (T), orderByProperty.Type);
            object ret = genericMethod.Invoke(null, new object[] {source, lambda});
            return (IQueryable<T>) ret;
        }

    }
}