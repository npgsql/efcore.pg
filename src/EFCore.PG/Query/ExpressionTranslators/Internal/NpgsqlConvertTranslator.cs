using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Translates methods defined on <see cref="T:System.Convert"/> into PostgreSQL CAST expressions.
    /// </summary>
    public class NpgsqlConvertTranslator : IMethodCallTranslator
    {
        static readonly Dictionary<string, string> TypeMapping = new Dictionary<string, string>
        {
            [nameof(Convert.ToBoolean)] = "bool",
            [nameof(Convert.ToByte)]    = "smallint",
            [nameof(Convert.ToDecimal)] = "numeric",
            [nameof(Convert.ToDouble)]  = "double precision",
            [nameof(Convert.ToInt16)]   = "smallint",
            [nameof(Convert.ToInt32)]   = "int",
            [nameof(Convert.ToInt64)]   = "bigint",
            [nameof(Convert.ToString)]  = "text"
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

        static readonly List<MethodInfo> SupportedMethods
            = TypeMapping.Keys
                .SelectMany(
                    t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
                        .Where(
                            m => m.GetParameters().Length == 1
                                 && SupportedTypes.Contains(m.GetParameters().First().ParameterType)))
                .ToList();

        readonly ISqlExpressionFactory _sqlExpressionFactory;

        public NpgsqlConvertTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            => SupportedMethods.Contains(method)
                ? _sqlExpressionFactory.Convert(arguments[0], method.ReturnType)
                : null;
    }
}
