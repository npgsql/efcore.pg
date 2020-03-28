using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal
{
    /// <summary>
    /// Provides translation services for static <see cref="Math"/> methods..
    /// </summary>
    /// <remarks>
    /// See:
    ///   - https://www.postgresql.org/docs/current/static/functions-math.html
    ///   - https://www.postgresql.org/docs/current/static/functions-conditional.html#FUNCTIONS-GREATEST-LEAST
    /// </remarks>
    public class NpgsqlMathTranslator : IMethodCallTranslator
    {
        static readonly Dictionary<MethodInfo, string> SupportedMethodTranslations = new Dictionary<MethodInfo, string>
        {
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(decimal) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(double) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(float) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(int) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(long) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(short) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(decimal) }), "CEILING" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(double) }), "CEILING" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(decimal) }), "FLOOR" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(double) }), "FLOOR" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) }), "POWER" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Exp), new[] { typeof(double) }), "EXP" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Log10), new[] { typeof(double) }), "LOG" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double) }), "LN" },
            // Note: PostgreSQL has log(x,y) but only for decimal, whereas .NET has it only for double
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sqrt), new[] { typeof(double) }), "SQRT" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Acos), new[] { typeof(double) }), "ACOS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Asin), new[] { typeof(double) }), "ASIN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Atan), new[] { typeof(double) }), "ATAN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) }), "ATAN2" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Cos), new[] { typeof(double) }), "COS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sin), new[] { typeof(double) }), "SIN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Tan), new[] { typeof(double) }), "TAN" },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double) }), "ROUND" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal) }), "ROUND" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(double) }), "TRUNC" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(decimal) }), "TRUNC" },

            // https://www.postgresql.org/docs/current/functions-conditional.html#FUNCTIONS-GREATEST-LEAST
            { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(decimal), typeof(decimal) }), "GREATEST" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(double), typeof(double) }), "GREATEST" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(float), typeof(float) }), "GREATEST" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(int), typeof(int) }), "GREATEST" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(long), typeof(long) }), "GREATEST" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(short), typeof(short) }), "GREATEST" },

            // https://www.postgresql.org/docs/current/functions-conditional.html#FUNCTIONS-GREATEST-LEAST
            { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(decimal), typeof(decimal) }), "LEAST" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(double), typeof(double) }), "LEAST" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(float), typeof(float) }), "LEAST" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(int), typeof(int) }), "LEAST" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(long), typeof(long) }), "LEAST" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(short), typeof(short) }), "LEAST" },
        };

        static readonly IEnumerable<MethodInfo> SignMethodInfos = new[]
        {
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(decimal) }),
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(double) }),
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(float) }),
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(int) }),
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(long) }),
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(sbyte) }),
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(short) })
        };

        static readonly MethodInfo RoundDecimalTwoParams = typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal), typeof(int) });

        readonly IRelationalTypeMappingSource _typeMappingSource;
        readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NpgsqlMathTranslator"/> class.
        /// </summary>
        /// <param name="sqlExpressionFactory">The SQL expression factory to use when generating expressions..</param>
        public NpgsqlMathTranslator(
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _typeMappingSource = typeMappingSource;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <inheritdoc />
        [CanBeNull]
        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (SupportedMethodTranslations.TryGetValue(method, out var sqlFunctionName))
            {
                var typeMapping = arguments.Count == 1
                    ? ExpressionExtensions.InferTypeMapping(arguments[0])
                    : ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);

                var newArguments = new SqlExpression[arguments.Count];
                newArguments[0] = _sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping);

                if (arguments.Count == 2)
                    newArguments[1] = _sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping);

                // Note: GREATER/LEAST only return NULL if *all* arguments are null, but we currently can't
                // convey this.
                return _sqlExpressionFactory.Function(
                    sqlFunctionName,
                    newArguments,
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[newArguments.Length],
                    method.ReturnType,
                    typeMapping);
            }

            // PostgreSQL sign() returns 1, 0, -1, but in the same type as the argument, so we need to convert
            // the return type to int.
            if (SignMethodInfos.Contains(method))
            {
                return
                    _sqlExpressionFactory.Convert(
                        _sqlExpressionFactory.Function(
                            "SIGN",
                            arguments,
                            nullable: true,
                            argumentsPropagateNullability: TrueArrays[1],
                            method.ReturnType),
                        typeof(int),
                        _typeMappingSource.FindMapping(typeof(int)));
            }

            if (method == RoundDecimalTwoParams)
            {
                return _sqlExpressionFactory.Function("ROUND", new[]
                    {
                        _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]),
                        _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1])
                    },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    method.ReturnType,
                    _typeMappingSource.FindMapping(typeof(decimal)));
            }

            return null;
        }
    }
}
