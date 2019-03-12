using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates Enumerable.SequenceEqual on arrays into PostgreSQL array equality operations.
    /// </summary>
    /// <remarks>
    /// https://www.postgresql.org/docs/current/static/functions-array.html
    /// </remarks>
    public class NpgsqlArraySequenceEqualTranslator : IMethodCallTranslator
    {
        static readonly MethodInfo SequenceEqualMethodInfo = typeof(Enumerable).GetTypeInfo().GetDeclaredMethods(nameof(Enumerable.SequenceEqual)).Single(m =>
                m.IsGenericMethodDefinition &&
                m.GetParameters().Length == 2
        );

        [CanBeNull]
        public Expression Translate(MethodCallExpression methodCallExpression, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            var method = methodCallExpression.Method;
            if (method.IsGenericMethod &&
                ReferenceEquals(method.GetGenericMethodDefinition(), SequenceEqualMethodInfo) &&
                methodCallExpression.Arguments.All(a => a.Type.IsArray))
            {
                return Expression.MakeBinary(ExpressionType.Equal,
                    methodCallExpression.Arguments[0],
                    methodCallExpression.Arguments[1]);
            }

            return null;
        }
    }
}
