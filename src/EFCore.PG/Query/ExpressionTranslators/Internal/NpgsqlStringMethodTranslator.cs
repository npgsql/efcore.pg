using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     Provides translation services for PostgreSQL string functions.
/// </summary>
/// <remarks>
///     See: https://www.postgresql.org/docs/current/static/functions-string.html
/// </remarks>
public class NpgsqlStringMethodTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly SqlExpression _whitespace;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlStringMethodTranslator(NpgsqlTypeMappingSource typeMappingSource, ISqlExpressionFactory sqlExpressionFactory)
    {
        _typeMappingSource = typeMappingSource;
        _sqlExpressionFactory = sqlExpressionFactory;
        _whitespace = _sqlExpressionFactory.Constant(
            @" \t\n\r", // TODO: Complete this
            typeMappingSource.EStringTypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType == typeof(string))
        {
            return TranslateStringMethod(instance, method, arguments);
        }

        if (method.DeclaringType == typeof(NpgsqlDbFunctionsExtensions))
        {
            return TranslateDbFunctionsMethod(instance, method, arguments);
        }

        if (method.DeclaringType == typeof(Enumerable)
            && method is { IsGenericMethod: true, Name: nameof(Enumerable.FirstOrDefault) or nameof(Enumerable.LastOrDefault) }
            && arguments is [var stringArg]
            && method.ReturnType == typeof(char))
        {
            if (method.Name == nameof(Enumerable.FirstOrDefault))
            {
                return _sqlExpressionFactory.Function(
                    "substr",
                    [stringArg, _sqlExpressionFactory.Constant(1), _sqlExpressionFactory.Constant(1)],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[3],
                    method.ReturnType);
            }

            return _sqlExpressionFactory.Function(
                "substr",
                [
                    stringArg,
                    _sqlExpressionFactory.Function(
                        "length",
                        [stringArg],
                        nullable: true,
                        argumentsPropagateNullability: [true],
                        typeof(int)),
                    _sqlExpressionFactory.Constant(1)
                ],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[3],
                method.ReturnType);
        }

        return null;
    }

    private SqlExpression? TranslateStringMethod(SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
    {
        switch (method.Name)
        {
            case nameof(string.IndexOf) when arguments is [_]:
            {
                var argument = arguments[0];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance!, argument);

                return _sqlExpressionFactory.Subtract(
                    _sqlExpressionFactory.Function(
                        "strpos",
                        [
                            _sqlExpressionFactory.ApplyTypeMapping(instance!, stringTypeMapping),
                            _sqlExpressionFactory.ApplyTypeMapping(argument, stringTypeMapping)
                        ],
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        method.ReturnType),
                    _sqlExpressionFactory.Constant(1));
            }

            case nameof(string.Replace) when arguments is [var oldValue, var newValue] && oldValue.Type == typeof(string):
            {
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance!, oldValue, newValue);

                return _sqlExpressionFactory.Function(
                    "replace",
                    [
                        _sqlExpressionFactory.ApplyTypeMapping(instance!, stringTypeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(oldValue, stringTypeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(newValue, stringTypeMapping)
                    ],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[3],
                    method.ReturnType,
                    stringTypeMapping);
            }

            case nameof(string.ToLower) or nameof(string.ToUpper) when arguments is []:
                return _sqlExpressionFactory.Function(
                    method.Name == nameof(string.ToLower) ? "lower" : "upper",
                    [instance!],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    method.ReturnType,
                    instance!.TypeMapping);

            case nameof(string.Substring):
            {
                var args =
                    arguments is [var startIndex]
                        ? new SqlExpression[] { instance!, GenerateOneBasedIndexExpression(startIndex) }
                        : [instance!, GenerateOneBasedIndexExpression(arguments[0]), arguments[1]];
                return _sqlExpressionFactory.Function(
                    "substring",
                    args,
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[args.Length],
                    method.ReturnType,
                    instance!.TypeMapping);
            }

            case nameof(string.IsNullOrWhiteSpace):
            {
                var argument = arguments[0];

                return _sqlExpressionFactory.OrElse(
                    _sqlExpressionFactory.IsNull(argument),
                    _sqlExpressionFactory.Equal(
                        _sqlExpressionFactory.Function(
                            "btrim",
                            [argument, _whitespace],
                            nullable: true,
                            argumentsPropagateNullability: TrueArrays[2],
                            argument.Type,
                            argument.TypeMapping),
                        _sqlExpressionFactory.Constant(string.Empty, argument.TypeMapping)));
            }

            case nameof(string.TrimStart) or nameof(string.TrimEnd) or nameof(string.Trim):
            {
                char[]? trimChars = null;

                if (arguments is [_])
                {
                    var constantTrimChars = arguments[0] as SqlConstantExpression;
                    if (constantTrimChars is null)
                    {
                        return null; // Don't translate if trim chars isn't a constant
                    }

                    trimChars = constantTrimChars.Value is char c
                        ? [c]
                        : (char[]?)constantTrimChars.Value;
                }

                var isTrimStart = method.Name is nameof(string.TrimStart);
                var isTrimEnd = method.Name is nameof(string.TrimEnd);

                return _sqlExpressionFactory.Function(
                    isTrimStart ? "ltrim" : isTrimEnd ? "rtrim" : "btrim",
                    [
                        instance!,
                        trimChars is null || trimChars.Length == 0
                            ? _whitespace
                            : _sqlExpressionFactory.Constant(new string(trimChars))
                    ],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    instance!.Type,
                    instance.TypeMapping);
            }

            case nameof(string.PadLeft) or nameof(string.PadRight):
            {
                var args =
                    arguments is [var padCount]
                        ? new SqlExpression[] { instance!, padCount }
                        : new[] { instance!, arguments[0], arguments[1] };

                return _sqlExpressionFactory.Function(
                    method.Name is nameof(string.PadLeft) ? "lpad" : "rpad",
                    args,
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[args.Length],
                    instance!.Type,
                    instance.TypeMapping);
            }

            case nameof(string.Join)
                when arguments is [_, var joinArray] && joinArray.TypeMapping is NpgsqlArrayTypeMapping:
            {
                // If the array of strings to be joined is a constant (NewArrayExpression), we translate to concat_ws.
                // Otherwise we translate to array_to_string, which also supports array columns and parameters.
                if (arguments[1] is PgNewArrayExpression newArrayExpression)
                {
                    var rewrittenArguments = new SqlExpression[newArrayExpression.Expressions.Count + 1];
                    rewrittenArguments[0] = arguments[0];

                    for (var i = 0; i < newArrayExpression.Expressions.Count; i++)
                    {
                        var argument = newArrayExpression.Expressions[i];

                        rewrittenArguments[i + 1] = argument switch
                        {
                            ColumnExpression { IsNullable: false } => argument,
                            SqlConstantExpression constantExpression => constantExpression.Value is null
                                ? _sqlExpressionFactory.Constant(string.Empty, typeof(string))
                                : constantExpression,
                            _ => _sqlExpressionFactory.Coalesce(argument, _sqlExpressionFactory.Constant(string.Empty, typeof(string)))
                        };
                    }

                    // Only the delimiter (first arg) propagates nullability - all others are non-nullable, since we wrap the others in coalesce
                    // (where needed).
                    var argumentsPropagateNullability = new bool[rewrittenArguments.Length];
                    argumentsPropagateNullability[0] = true;

                    return _sqlExpressionFactory.Function(
                        "concat_ws",
                        rewrittenArguments,
                        nullable: true,
                        argumentsPropagateNullability,
                        typeof(string));
                }

                return _sqlExpressionFactory.Function(
                    "array_to_string",
                    [arguments[1], arguments[0], _sqlExpressionFactory.Constant("")],
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[3],
                    typeof(string));
            }

            default:
                return null;
        }
    }

    private SqlExpression? TranslateDbFunctionsMethod(SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        => method.Name switch
        {
            nameof(NpgsqlDbFunctionsExtensions.Reverse) => _sqlExpressionFactory.Function(
                "reverse",
                [arguments[1]],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                typeof(string),
                arguments[1].TypeMapping),

            // Note that string_to_array always returns text[], regardless of the input type
            nameof(NpgsqlDbFunctionsExtensions.StringToArray) when arguments is [_, var strArg, var delimArg]
                => _sqlExpressionFactory.Function(
                    "string_to_array",
                    [strArg, delimArg],
                    nullable: true,
                    argumentsPropagateNullability: [true, false],
                    typeof(string[]),
                    _typeMappingSource.FindMapping(typeof(string[]))),

            // Note that string_to_array always returns text[], regardless of the input type
            nameof(NpgsqlDbFunctionsExtensions.StringToArray) when arguments is [_, _, _, _]
                => _sqlExpressionFactory.Function(
                    "string_to_array",
                    [arguments[1], arguments[2], arguments[3]],
                    nullable: true,
                    argumentsPropagateNullability: [true, false, false],
                    typeof(string[]),
                    _typeMappingSource.FindMapping(typeof(string[]))),

            nameof(NpgsqlDbFunctionsExtensions.ToDate) => _sqlExpressionFactory.Function(
                "to_date",
                [arguments[1], arguments[2]],
                nullable: true,
                argumentsPropagateNullability: [true, true],
                typeof(DateOnly),
                _typeMappingSource.FindMapping(typeof(DateOnly))),

            nameof(NpgsqlDbFunctionsExtensions.ToTimestamp) => _sqlExpressionFactory.Function(
                "to_timestamp",
                [arguments[1], arguments[2]],
                nullable: true,
                argumentsPropagateNullability: [true, true],
                typeof(DateTime),
                _typeMappingSource.FindMapping(typeof(DateTime))),
            _ => null,
        };

    private SqlExpression GenerateOneBasedIndexExpression(SqlExpression expression)
        => expression is SqlConstantExpression constant
            ? _sqlExpressionFactory.Constant(Convert.ToInt32(constant.Value) + 1, constant.TypeMapping)
            : _sqlExpressionFactory.Add(expression, _sqlExpressionFactory.Constant(1));
}
