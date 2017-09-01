using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class NpgsqlDictionaryIndexTranslator : IMethodCallTranslator
    {
        static readonly PropertyInfo DictionaryPropertyIndexAccessor =
            typeof(IDictionary<string, string>).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Single(p => p.Name == "Item");

        public Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.NodeType == ExpressionType.Call
                && methodCallExpression.Method.Name == "get_Item"
                && typeof(IDictionary<string, string>).IsAssignableFrom(methodCallExpression.Method.DeclaringType))
            {
                return Expression.MakeIndex(methodCallExpression.Object, DictionaryPropertyIndexAccessor, methodCallExpression.Arguments);
            }
            return null;
        }
    }
}
