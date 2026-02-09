using System.Numerics;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     Provides translation services for static <see cref="Math" /> methods..
/// </summary>
/// <remarks>
///     See:
///     - https://www.postgresql.org/docs/current/static/functions-math.html
///     - https://www.postgresql.org/docs/current/static/functions-conditional.html#FUNCTIONS-GREATEST-LEAST
/// </remarks>
public class NpgsqlMathTranslator(
    IRelationalTypeMappingSource typeMappingSource,
    ISqlExpressionFactory sqlExpressionFactory,
    IModel model) : IMethodCallTranslator
{
    private readonly RelationalTypeMapping _intTypeMapping = typeMappingSource.FindMapping(typeof(int), model)!;
    private readonly RelationalTypeMapping _decimalTypeMapping = typeMappingSource.FindMapping(typeof(decimal), model)!;

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        var declaringType = method.DeclaringType;

        if (declaringType != typeof(Math)
            && declaringType != typeof(MathF)
            && declaringType != typeof(BigInteger)
            && declaringType != typeof(double)
            && declaringType != typeof(float))
        {
            return null;
        }

        return method.Name switch
        {
            nameof(Math.Abs) when arguments is [var arg]
                && (arg.Type == typeof(decimal) || arg.Type == typeof(double) || arg.Type == typeof(float)
                    || arg.Type == typeof(int) || arg.Type == typeof(long) || arg.Type == typeof(short)
                    || arg.Type == typeof(BigInteger))
                => TranslateFunction("abs", arg),
            nameof(Math.Ceiling) when arguments is [var arg]
                && (arg.Type == typeof(decimal) || arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("ceiling", arg),
            nameof(Math.Floor) when arguments is [var arg]
                && (arg.Type == typeof(decimal) || arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("floor", arg),
            nameof(Math.Pow) when arguments is [var arg1, var arg2]
                && (arg1.Type == typeof(double) || arg1.Type == typeof(float) || arg1.Type == typeof(BigInteger))
                => TranslateBinaryFunction("power", arg1, arg2),
            nameof(Math.Exp) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("exp", arg),
            nameof(Math.Log10) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("log", arg),
            nameof(Math.Log) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("ln", arg),
            // Note: PostgreSQL has log(x,y) but only for decimal, whereas .NET has it only for double/float
            nameof(Math.Sqrt) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("sqrt", arg),
            nameof(Math.Acos) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("acos", arg),
            nameof(Math.Asin) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("asin", arg),
            nameof(Math.Atan) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("atan", arg),
            nameof(Math.Atan2) when arguments is [var arg1, var arg2]
                && (arg1.Type == typeof(double) || arg1.Type == typeof(float))
                => TranslateBinaryFunction("atan2", arg1, arg2),
            nameof(Math.Cos) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("cos", arg),
            nameof(Math.Sin) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("sin", arg),
            nameof(Math.Tan) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("tan", arg),
            nameof(double.DegreesToRadians) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("radians", arg),
            nameof(double.RadiansToDegrees) when arguments is [var arg]
                && (arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateFunction("degrees", arg),
            // https://www.postgresql.org/docs/current/functions-conditional.html#FUNCTIONS-GREATEST-LEAST
            nameof(Math.Max) when arguments is [var arg1, var arg2]
                && (arg1.Type == typeof(decimal) || arg1.Type == typeof(double) || arg1.Type == typeof(float)
                    || arg1.Type == typeof(int) || arg1.Type == typeof(long) || arg1.Type == typeof(short)
                    || arg1.Type == typeof(BigInteger))
                => TranslateBinaryFunction("GREATEST", arg1, arg2),
            nameof(Math.Min) when arguments is [var arg1, var arg2]
                && (arg1.Type == typeof(decimal) || arg1.Type == typeof(double) || arg1.Type == typeof(float)
                    || arg1.Type == typeof(int) || arg1.Type == typeof(long) || arg1.Type == typeof(short)
                    || arg1.Type == typeof(BigInteger))
                => TranslateBinaryFunction("LEAST", arg1, arg2),

            nameof(Math.Truncate) when arguments is [var arg]
                && (arg.Type == typeof(decimal) || arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateTruncate(arg),
            nameof(Math.Round) when arguments is [var arg]
                && (arg.Type == typeof(decimal) || arg.Type == typeof(double) || arg.Type == typeof(float))
                => TranslateRound(arg),
            nameof(Math.Round) when arguments is [var arg, var digits]
                && arg.Type == typeof(decimal) && digits.Type == typeof(int)
                => TranslateRoundWithDigits(arg, digits),
            nameof(Math.Sign) when arguments is [var arg]
                && (arg.Type == typeof(decimal) || arg.Type == typeof(double) || arg.Type == typeof(float)
                    || arg.Type == typeof(int) || arg.Type == typeof(long) || arg.Type == typeof(sbyte) || arg.Type == typeof(short))
                => TranslateSign(arg),

            // PostgreSQL treats NaN values as equal, against IEEE754
            nameof(double.IsNaN) when arguments is [var arg]
                => sqlExpressionFactory.Equal(
                    arg,
                    sqlExpressionFactory.Constant(declaringType == typeof(double) ? double.NaN : (object)float.NaN)),
            nameof(double.IsPositiveInfinity) when arguments is [var arg]
                => sqlExpressionFactory.Equal(
                    arg,
                    sqlExpressionFactory.Constant(
                        declaringType == typeof(double) ? double.PositiveInfinity : (object)float.PositiveInfinity)),
            nameof(double.IsNegativeInfinity) when arguments is [var arg]
                => sqlExpressionFactory.Equal(
                    arg,
                    sqlExpressionFactory.Constant(
                        declaringType == typeof(double) ? double.NegativeInfinity : (object)float.NegativeInfinity)),

            _ => null
        };

        SqlExpression TranslateFunction(string sqlFunctionName, SqlExpression arg)
        {
            var typeMapping = ExpressionExtensions.InferTypeMapping(arg);
            return sqlExpressionFactory.Function(
                sqlFunctionName,
                [sqlExpressionFactory.ApplyTypeMapping(arg, typeMapping)],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                method.ReturnType,
                typeMapping);
        }

        SqlExpression TranslateBinaryFunction(string sqlFunctionName, SqlExpression arg1, SqlExpression arg2)
        {
            var typeMapping = ExpressionExtensions.InferTypeMapping(arg1, arg2);
            // Note: GREATEST/LEAST only return NULL if *all* arguments are null, but we currently can't convey this.
            return sqlExpressionFactory.Function(
                sqlFunctionName,
                [
                    sqlExpressionFactory.ApplyTypeMapping(arg1, typeMapping),
                    sqlExpressionFactory.ApplyTypeMapping(arg2, typeMapping)
                ],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[2],
                method.ReturnType,
                typeMapping);
        }

        SqlExpression TranslateTruncate(SqlExpression argument)
        {
            // C# has Truncate over decimal/double/float only so our argument will be one of those types (compiler puts convert node)
            // In database result will be same type except for float which returns double which we need to cast back to float.
            var result = sqlExpressionFactory.Function(
                "trunc",
                [argument],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                argument.Type == typeof(float) ? typeof(double) : argument.Type);

            if (argument.Type == typeof(float))
            {
                result = sqlExpressionFactory.Convert(result, typeof(float));
            }

            return sqlExpressionFactory.ApplyTypeMapping(result, argument.TypeMapping);
        }

        SqlExpression TranslateRound(SqlExpression argument)
        {
            // C# has Round over decimal/double/float only so our argument will be one of those types (compiler puts convert node)
            // In database result will be same type except for float which returns double which we need to cast back to float.
            var result = sqlExpressionFactory.Function(
                "round",
                [argument],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                argument.Type == typeof(float) ? typeof(double) : argument.Type);

            if (argument.Type == typeof(float))
            {
                result = sqlExpressionFactory.Convert(result, typeof(float));
            }

            return sqlExpressionFactory.ApplyTypeMapping(result, argument.TypeMapping);
        }

        SqlExpression TranslateRoundWithDigits(SqlExpression argument, SqlExpression digits)
            => sqlExpressionFactory.Function(
                "round",
                [
                    sqlExpressionFactory.ApplyDefaultTypeMapping(argument),
                    sqlExpressionFactory.ApplyDefaultTypeMapping(digits)
                ],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[2],
                method.ReturnType,
                _decimalTypeMapping);

        // PostgreSQL sign() returns 1, 0, -1, but in the same type as the argument, so we need to convert
        // the return type to int.
        SqlExpression TranslateSign(SqlExpression argument)
            => sqlExpressionFactory.Convert(
                sqlExpressionFactory.Function(
                    "sign",
                    [argument],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    method.ReturnType),
                typeof(int),
                _intTypeMapping);
    }
}
