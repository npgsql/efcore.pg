using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates methods defined on <see cref="T:System.Convert"/> into PostgreSQL CAST expressions.
    /// </summary>
    public class NpgsqlConvertTranslator : IMethodCallTranslator
    {
        static readonly Dictionary<string, DbType> TypeMapping = new Dictionary<string, DbType>
        {
            [nameof(Convert.ToByte)] = DbType.Byte,
            [nameof(Convert.ToDecimal)] = DbType.Decimal,
            [nameof(Convert.ToDouble)] = DbType.Double,
            [nameof(Convert.ToInt16)] = DbType.Int16,
            [nameof(Convert.ToInt32)] = DbType.Int32,
            [nameof(Convert.ToInt64)] = DbType.Int64,
            [nameof(Convert.ToString)] = DbType.String
        };

        static readonly List<Type> SupportedTypes = new List<Type>
        {
            typeof(bool),
            typeof(byte),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(string)
        };

        static readonly IEnumerable<MethodInfo> SupportedMethods =
            TypeMapping.Keys
                .SelectMany(t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
                    .Where(m => m.GetParameters().Length == 1
                                && SupportedTypes.Contains(m.GetParameters().First().ParameterType)));

        /// <inheritdoc />
        [CanBeNull]
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => SupportedMethods.Contains(methodCallExpression.Method)
                ? new ExplicitCastExpression(methodCallExpression.Arguments[0], methodCallExpression.Type)
                : null;
    }
}
