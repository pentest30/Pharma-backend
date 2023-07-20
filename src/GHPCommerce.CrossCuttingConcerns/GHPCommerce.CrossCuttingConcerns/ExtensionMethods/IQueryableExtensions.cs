using System;
using System.Linq;
using System.Linq.Expressions;

namespace GHPCommerce.CrossCuttingConcerns.ExtensionMethods
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paged<T>(this IQueryable<T> source, int page, int pageSize)
        {
            return source.Skip((page - 1) * pageSize).Take(pageSize);
        }
        public static IQueryable<TSource> Distinct<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, object>> predicate)
        {
            // TODO: Null-check arguments
            return from item in source.GroupBy(predicate) select item.First();
        }

       
    }
}
