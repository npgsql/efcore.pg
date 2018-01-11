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
               SupportedTypes.Contains(
                   AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue9894", out var enabled)
                   && enabled
                       ? methodCallExpression.Object.Type.UnwrapNullableType().UnwrapEnumType()
                       : methodCallExpression.Object.Type.UnwrapNullableType()
               )
                ? new ExplicitCastExpression(methodCallExpression.Object, typeof(string))
                : null;
    }
}
