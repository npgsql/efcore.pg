using System.Numerics;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

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
    private static readonly Dictionary<MethodInfo, string> SupportedMethodTranslations = new()
    {
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(decimal) })!, "abs" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(double) })!, "abs" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(float) })!, "abs" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(int) })!, "abs" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(long) })!, "abs" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(short) })!, "abs" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Abs), new[] { typeof(float) })!, "abs" },
        { typeof(BigInteger).GetRuntimeMethod(nameof(BigInteger.Abs), new[] { typeof(BigInteger) })!, "abs" },

        { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(decimal) })!, "ceiling" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(double) })!, "ceiling" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Ceiling), new[] { typeof(float) })!, "ceiling" },

        { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(decimal) })!, "floor" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(double) })!, "floor" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Floor), new[] { typeof(float) })!, "floor" },

        { typeof(Math).GetRuntimeMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) })!, "power" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Pow), new[] { typeof(float), typeof(float) })!, "power" },
        { typeof(BigInteger).GetRuntimeMethod(nameof(BigInteger.Pow), new[] { typeof(BigInteger), typeof(int) })!, "power" },

        { typeof(Math).GetRuntimeMethod(nameof(Math.Exp), new[] { typeof(double) })!, "exp" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Exp), new[] { typeof(float) })!, "exp" },

        { typeof(Math).GetRuntimeMethod(nameof(Math.Log10), new[] { typeof(double) })!, "log" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Log10), new[] { typeof(float) })!, "log" },

        { typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double) })!, "ln" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Log), new[] { typeof(float) })!, "ln" },
        // Note: PostgreSQL has log(x,y) but only for decimal, whereas .NET has it only for double/float

        { typeof(Math).GetRuntimeMethod(nameof(Math.Sqrt), new[] { typeof(double) })!, "sqrt" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Sqrt), new[] { typeof(float) })!, "sqrt" },

        { typeof(Math).GetRuntimeMethod(nameof(Math.Acos), new[] { typeof(double) })!, "acos" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Acos), new[] { typeof(float) })!, "acos" },

        { typeof(Math).GetRuntimeMethod(nameof(Math.Asin), new[] { typeof(double) })!, "asin" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Asin), new[] { typeof(float) })!, "asin" },

        { typeof(Math).GetRuntimeMethod(nameof(Math.Atan), new[] { typeof(double) })!, "atan" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Atan), new[] { typeof(float) })!, "atan" },

        { typeof(Math).GetRuntimeMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) })!, "atan2" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Atan2), new[] { typeof(float), typeof(float) })!, "atan2" },

        { typeof(Math).GetRuntimeMethod(nameof(Math.Cos), new[] { typeof(double) })!, "cos" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Cos), new[] { typeof(float) })!, "cos" },

        { typeof(Math).GetRuntimeMethod(nameof(Math.Sin), new[] { typeof(double) })!, "sin" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Sin), new[] { typeof(float) })!, "sin" },

        { typeof(Math).GetRuntimeMethod(nameof(Math.Tan), new[] { typeof(double) })!, "tan" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Tan), new[] { typeof(float) })!, "tan" },

        // https://www.postgresql.org/docs/current/functions-conditional.html#FUNCTIONS-GREATEST-LEAST
        { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(decimal), typeof(decimal) })!, "GREATEST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(double), typeof(double) })!, "GREATEST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(float), typeof(float) })!, "GREATEST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(int), typeof(int) })!, "GREATEST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(long), typeof(long) })!, "GREATEST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(short), typeof(short) })!, "GREATEST" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Max), new[] { typeof(float), typeof(float) })!, "GREATEST" },
        { typeof(BigInteger).GetRuntimeMethod(nameof(BigInteger.Max), new[] { typeof(BigInteger), typeof(BigInteger) })!, "GREATEST" },

        // https://www.postgresql.org/docs/current/functions-conditional.html#FUNCTIONS-GREATEST-LEAST
        { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(decimal), typeof(decimal) })!, "LEAST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(double), typeof(double) })!, "LEAST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(float), typeof(float) })!, "LEAST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(int), typeof(int) })!, "LEAST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(long), typeof(long) })!, "LEAST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(short), typeof(short) })!, "LEAST" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Min), new[] { typeof(float), typeof(float) })!, "LEAST" },
        { typeof(BigInteger).GetRuntimeMethod(nameof(BigInteger.Min), new[] { typeof(BigInteger), typeof(BigInteger) })!, "LEAST" },
    };

    private static readonly IEnumerable<MethodInfo> TruncateMethodInfos = new[]
    {
        typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Truncate), typeof(decimal)),
        typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Truncate), typeof(double)),
        typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Truncate), typeof(float))
    };

    private static readonly IEnumerable<MethodInfo> RoundMethodInfos = new[]
    {
        typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Round), typeof(decimal)),
        typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Round), typeof(double)),
        typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Round), typeof(float))
    };

    private static readonly IEnumerable<MethodInfo> SignMethodInfos = new[]
    {
        typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(decimal) })!,
        typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(double) })!,
        typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(float) })!,
        typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(int) })!,
        typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(long) })!,
        typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(sbyte) })!,
        typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(short) })!,
        typeof(MathF).GetRuntimeMethod(nameof(MathF.Sign), new[] { typeof(float) })!,
    };

    private static readonly MethodInfo RoundDecimalTwoParams
        = typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal), typeof(int) })!;

    private static readonly MethodInfo DoubleIsNanMethodInfo
        = typeof(double).GetRuntimeMethod(nameof(double.IsNaN), new[] { typeof(double) })!;
    private static readonly MethodInfo DoubleIsPositiveInfinityMethodInfo
        = typeof(double).GetRuntimeMethod(nameof(double.IsPositiveInfinity), new[] { typeof(double) })!;
    private static readonly MethodInfo DoubleIsNegativeInfinityMethodInfo
        = typeof(double).GetRuntimeMethod(nameof(double.IsNegativeInfinity), new[] { typeof(double) })!;

    private static readonly MethodInfo FloatIsNanMethodInfo
        = typeof(float).GetRuntimeMethod(nameof(float.IsNaN), new[] { typeof(float) })!;
    private static readonly MethodInfo FloatIsPositiveInfinityMethodInfo
        = typeof(float).GetRuntimeMethod(nameof(float.IsPositiveInfinity), new[] { typeof(float) })!;
    private static readonly MethodInfo FloatIsNegativeInfinityMethodInfo
        = typeof(float).GetRuntimeMethod(nameof(float.IsNegativeInfinity), new[] { typeof(float) })!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _intTypeMapping;
    private readonly RelationalTypeMapping _decimalTypeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlMathTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory,
        IModel model)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _intTypeMapping = typeMappingSource.FindMapping(typeof(int), model)!;
        _decimalTypeMapping = typeMappingSource.FindMapping(typeof(decimal), model)!;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (SupportedMethodTranslations.TryGetValue(method, out var sqlFunctionName))
        {
            var typeMapping = arguments.Count == 1
                ? ExpressionExtensions.InferTypeMapping(arguments[0])
                : ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);

            var newArguments = new SqlExpression[arguments.Count];
            newArguments[0] = _sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping);

            if (arguments.Count == 2)
            {
                newArguments[1] = _sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping);
            }

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

        if (TruncateMethodInfos.Contains(method))
        {
            var argument = arguments[0];

            // C# has Round over decimal/double/float only so our argument will be one of those types (compiler puts convert node)
            // In database result will be same type except for float which returns double which we need to cast back to float.
            var result = (SqlExpression)_sqlExpressionFactory.Function(
                "trunc",
                new[] { argument },
                nullable: true,
                argumentsPropagateNullability: new[] { true, false, false },
                argument.Type == typeof(float) ? typeof(double) : argument.Type);

            if (argument.Type == typeof(float))
            {
                result = _sqlExpressionFactory.Convert(result, typeof(float));
            }

            return _sqlExpressionFactory.ApplyTypeMapping(result, argument.TypeMapping);
        }

        if (RoundMethodInfos.Contains(method))
        {
            var argument = arguments[0];

            // C# has Round over decimal/double/float only so our argument will be one of those types (compiler puts convert node)
            // In database result will be same type except for float which returns double which we need to cast back to float.
            var result = (SqlExpression) _sqlExpressionFactory.Function(
                "round",
                new[] { argument },
                nullable: true,
                argumentsPropagateNullability: new[] { true, true },
                argument.Type == typeof(float) ? typeof(double) : argument.Type);

            if (argument.Type == typeof(float))
            {
                result = _sqlExpressionFactory.Convert(result, typeof(float));
            }

            return _sqlExpressionFactory.ApplyTypeMapping(result, argument.TypeMapping);
        }

        // PostgreSQL sign() returns 1, 0, -1, but in the same type as the argument, so we need to convert
        // the return type to int.
        if (SignMethodInfos.Contains(method))
        {
            return
                _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Function(
                        "sign",
                        arguments,
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[1],
                        method.ReturnType),
                    typeof(int),
                    _intTypeMapping);
        }

        if (method == RoundDecimalTwoParams)
        {
            return _sqlExpressionFactory.Function("round", new[]
                {
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1])
                },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[2],
                method.ReturnType,
                _decimalTypeMapping);
        }

        // PostgreSQL treats NaN values as equal, against IEEE754
        if (method == DoubleIsNanMethodInfo)
        {
            return _sqlExpressionFactory.Equal(arguments[0], _sqlExpressionFactory.Constant(double.NaN));
        }

        if (method == FloatIsNanMethodInfo)
        {
            return _sqlExpressionFactory.Equal(arguments[0], _sqlExpressionFactory.Constant(float.NaN));
        }

        if (method == DoubleIsPositiveInfinityMethodInfo)
        {
            return _sqlExpressionFactory.Equal(arguments[0], _sqlExpressionFactory.Constant(double.PositiveInfinity));
        }

        if (method == FloatIsPositiveInfinityMethodInfo)
        {
            return _sqlExpressionFactory.Equal(arguments[0], _sqlExpressionFactory.Constant(float.PositiveInfinity));
        }

        if (method == DoubleIsNegativeInfinityMethodInfo)
        {
            return _sqlExpressionFactory.Equal(arguments[0], _sqlExpressionFactory.Constant(double.NegativeInfinity));
        }

        if (method == FloatIsNegativeInfinityMethodInfo)
        {
            return _sqlExpressionFactory.Equal(arguments[0], _sqlExpressionFactory.Constant(float.NegativeInfinity));
        }

        return null;
    }
}
