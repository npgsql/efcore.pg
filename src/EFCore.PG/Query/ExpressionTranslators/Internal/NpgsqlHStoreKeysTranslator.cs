using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class NpgsqlHStoreKeysTranslator : IMethodCallTranslator
    {
        public Expression Translate(MethodCallExpression methodCallExpression)
        {
            if(methodCallExpression.Method == 
                typeof(NpgsqlDbFunctionsExtensions)
                .GetRuntimeMethod(
                    nameof(NpgsqlDbFunctionsExtensions.HStoreKeys), new Type[] { typeof(DbFunctions), typeof(IDictionary<string, string>) }))
            {
                return new SqlFunctionExpression("akeys", typeof(string[]), new Expression[] { methodCallExpression.Arguments[1] });
            }

            if (methodCallExpression.Method ==
                typeof(NpgsqlDbFunctionsExtensions)
                .GetRuntimeMethod(
                    nameof(NpgsqlDbFunctionsExtensions.HStoreValues), new Type[] { typeof(DbFunctions), typeof(IDictionary<string, string>) }))
            {
                return new SqlFunctionExpression("avals", typeof(string[]), new Expression[] { methodCallExpression.Arguments[1] });
            }

            if(typeof(IDictionary<string, string>).IsAssignableFrom(methodCallExpression.Method.DeclaringType)
                && methodCallExpression.Method.Name == nameof(IDictionary<string, string>.ContainsKey))
            {
                return new DictionaryContainsKeyExpression(methodCallExpression.Arguments.Single(), methodCallExpression.Object);
            }

            return null;
        }
    }
}
