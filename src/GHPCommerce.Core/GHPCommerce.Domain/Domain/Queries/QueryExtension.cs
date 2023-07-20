using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Common;
using LinqKit;

namespace GHPCommerce.Domain.Domain.Queries
{
    public static class QueryExtension
    {
        public static IQueryable<T> DynamicWhereQuery<T>(this IQueryable<T> queryable ,SyncDataGridQuery dataGridQuery) where T : Entity<Guid>
        {
            if (dataGridQuery.Where == null && dataGridQuery.Sorted ==null) return queryable;
            // gets the properties of our Model
            var properties =typeof(T) .GetProperties();
            var predicateBuilder = PredicateBuilder.New<T>();
            if (dataGridQuery.Where != null)
            {
                foreach (var predicate in dataGridQuery.Where[0].Predicates)
                {
                    if (predicate.IsComplex)
                    {
                        foreach (var complexPredicate in predicate.predicates)
                        {
                            if(complexPredicate.Value == null) continue;
                            // if the field does not exist among the names of the model properties continue
                            if (!properties.Any(x => string.Equals(x.Name, complexPredicate.Field, StringComparison.CurrentCultureIgnoreCase))) continue;
                            BuildPredicate(complexPredicate, predicateBuilder);
                            queryable = queryable.Where(predicateBuilder);
                        }
                    }
                    else
                    {
                        if(predicate.Value == null) continue;
                        // if the field does not exist among the names of the model properties continue
                        if (!properties.Any(x => string.Equals(x.Name, predicate.Field, StringComparison.CurrentCultureIgnoreCase))) continue; 
                        BuildPredicate(predicate, predicateBuilder);
                        queryable = queryable.Where(predicateBuilder);
                    }
                  
                }
            }

            if (dataGridQuery.Sorted == null) return queryable.OrderByDescending(x => x.CreatedDateTime);
            return dataGridQuery.Sorted.Aggregate(queryable, (current, sorted) => sorted.Direction == "ascending"
                ? current.OrderByProperty(sorted.Name)
                : current.OrderByPropertyDescending(sorted.Name));
        }
       
        private static void BuildPredicate<T>(Predicate pred, ExpressionStarter<T> predicate)
        {
            PropertyInfo pi = typeof(T).GetProperty(pred.Field.UppercaseFirst());
            if (pi == null) throw new InvalidOperationException();
            var func = MethodCallExpression<T>( pi ,pred.Operator, pred.Value.ToString());
            predicate?.And(func);
        }

        private static Expression<Func<T, bool>> MethodCallExpression<T>(PropertyInfo pi, string @operator, string term)
        {
            var param = Expression.Parameter(typeof(T));
            if (pi.PropertyType == typeof(string))
            {
                var body = Expression.Call(
                    Expression.Property(param, pi),
                    MethodCall.Contains,
                    null,
                    Expression.Constant(term)
                );
                return Expression.Lambda<Func<T, bool>>(body, param);
            }
            MemberExpression memberExpression = MemberExpressionHelper.GetMemberExpression(pi, param);
            var value = term.ConvertTo(pi.PropertyType);
            var bn = BinaryExpressionHelper.GetBinaryExpression(memberExpression, @operator, value);
            return Expression.Lambda<Func<T, bool>>(bn, param);
        }
        
    }
}