using System;
using System.Linq.Expressions;

namespace GHPCommerce.Domain.Domain.Queries
{
    public static class BinaryExpressionHelper
    {
        public static BinaryExpression GetBinaryExpression(MemberExpression memberExpression, string @operator, object value)
        {
            var isOffset = value is DateTimeOffset;
            if (value is DateTimeOffset timeOffset)
                value = timeOffset.DateTime.Date;

            BinaryExpression bn;
            if (GetOperator(@operator) == MethodCall.Equals)
            {
                bn = Expression.Equal(
                    memberExpression
                    , Expression.Constant(value));
            }
            else if (GetOperator(@operator) == MethodCall.GreaterThan)
            {
                if(value is DateTime time  && !isOffset)
                    value =time.Date.AddTicks(-1);
                bn = Expression.GreaterThan(
                    memberExpression
                    , Expression.Constant(value));
            }
            else if (GetOperator(@operator) == MethodCall.GreaterThanOrEqual)
            {
                bn = Expression.GreaterThanOrEqual(
                    memberExpression
                    , Expression.Constant(value));
            }
            else if (GetOperator(@operator) == MethodCall.LessThan)
            {
                bn = Expression.LessThan(
                    memberExpression
                    , Expression.Constant(value));
            }
            else
            {
                bn = Expression.LessThanOrEqual(
                    memberExpression
                    , Expression.Constant(value));
            }
            return bn;
        }

        private static string GetOperator(string @operator)
        {
            switch (@operator)
            {
                case "startswith":
                case "endswtih":
                case "contains": return MethodCall.Contains;
                case "equal": return MethodCall.Equals;
                case "greaterthan": return MethodCall.GreaterThan;
                case "greaterthanorequal": return MethodCall.GreaterThanOrEqual;
                case "lessthanorequal": return MethodCall.LessThanOrEqual;
                case "lessthan": return MethodCall.LessThan;
            }

            throw new System.NotImplementedException();
        }
    }
}