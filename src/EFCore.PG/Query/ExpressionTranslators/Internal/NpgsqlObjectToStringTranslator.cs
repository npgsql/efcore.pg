using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class NpgsqlObjectToStringTranslator : IMethodCallTranslator
    {
        static readonly List<Type> SupportedTypes = new List<Type>
        {
            typeof(int),
            typeof(long),
            typeof(DateTime),
            typeof(Guid),
            typeof(bool),
            typeof(byte),
            //typeof(byte[])
            typeof(double),
            typeof(DateTimeOffset),
            typeof(char),
            typeof(short),
            typeof(float),
            typeof(decimal),
            typeof(TimeSpan),
            typeof(uint),
            typeof(ushort),
            typeof(ulong),
            typeof(sbyte),
        };

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => methodCallExpression.Method.Name == nameof(ToString) &&
               methodCallExpression.Arguments.Count == 0 &&
               methodCallExpression.Object != null &&
               SupportedTypes.Contains(methodCallExpression.Object.Type.UnwrapNullableType().UnwrapEnumType())
                ? new ExplicitCastExpression(methodCallExpression.Object, typeof(string))
                : null;
    }
}
