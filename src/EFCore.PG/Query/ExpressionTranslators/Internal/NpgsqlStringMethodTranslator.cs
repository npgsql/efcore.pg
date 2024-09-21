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

    #region MethodInfo

    private static readonly MethodInfo IndexOfChar = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), [typeof(char)])!;
    private static readonly MethodInfo IndexOfString = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), [typeof(string)])!;

    private static readonly MethodInfo IsNullOrWhiteSpace =
        typeof(string).GetRuntimeMethod(nameof(string.IsNullOrWhiteSpace), [typeof(string)])!;

    private static readonly MethodInfo PadLeft = typeof(string).GetRuntimeMethod(nameof(string.PadLeft), [typeof(int)])!;

    private static readonly MethodInfo PadLeftWithChar = typeof(string).GetRuntimeMethod(
        nameof(string.PadLeft), [typeof(int), typeof(char)])!;

    private static readonly MethodInfo PadRight = typeof(string).GetRuntimeMethod(nameof(string.PadRight), [typeof(int)])!;

    private static readonly MethodInfo PadRightWithChar = typeof(string).GetRuntimeMethod(
        nameof(string.PadRight), [typeof(int), typeof(char)])!;

    private static readonly MethodInfo Replace = typeof(string).GetRuntimeMethod(
        nameof(string.Replace), [typeof(string), typeof(string)])!;

    private static readonly MethodInfo Substring = typeof(string).GetTypeInfo().GetDeclaredMethods(nameof(string.Substring))
        .Single(m => m.GetParameters().Length == 1);

    private static readonly MethodInfo SubstringWithLength = typeof(string).GetTypeInfo().GetDeclaredMethods(nameof(string.Substring))
        .Single(m => m.GetParameters().Length == 2);

    private static readonly MethodInfo ToLower = typeof(string).GetRuntimeMethod(nameof(string.ToLower), [])!;
    private static readonly MethodInfo ToUpper = typeof(string).GetRuntimeMethod(nameof(string.ToUpper), [])!;
    private static readonly MethodInfo TrimBothWithNoParam = typeof(string).GetRuntimeMethod(nameof(string.Trim), Type.EmptyTypes)!;
    private static readonly MethodInfo TrimBothWithChars = typeof(string).GetRuntimeMethod(nameof(string.Trim), [typeof(char[])])!;

    private static readonly MethodInfo TrimBothWithSingleChar =
        typeof(string).GetRuntimeMethod(nameof(string.Trim), [typeof(char)])!;

    private static readonly MethodInfo TrimEndWithNoParam = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), Type.EmptyTypes)!;

    private static readonly MethodInfo TrimEndWithChars = typeof(string).GetRuntimeMethod(
        nameof(string.TrimEnd), [typeof(char[])])!;

    private static readonly MethodInfo TrimEndWithSingleChar =
        typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), [typeof(char)])!;

    private static readonly MethodInfo TrimStartWithNoParam = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), Type.EmptyTypes)!;

    private static readonly MethodInfo TrimStartWithChars =
        typeof(string).GetRuntimeMethod(nameof(string.TrimStart), [typeof(char[])])!;

    private static readonly MethodInfo TrimStartWithSingleChar =
        typeof(string).GetRuntimeMethod(nameof(string.TrimStart), [typeof(char)])!;

    private static readonly MethodInfo Reverse = typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(
        nameof(NpgsqlDbFunctionsExtensions.Reverse), [typeof(DbFunctions), typeof(string)])!;

    private static readonly MethodInfo StringToArray = typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(
        nameof(NpgsqlDbFunctionsExtensions.StringToArray), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    private static readonly MethodInfo StringToArrayNullString = typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(
        nameof(NpgsqlDbFunctionsExtensions.StringToArray), [typeof(DbFunctions), typeof(string), typeof(string), typeof(string)])!;

    private static readonly MethodInfo ToDate = typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(
        nameof(NpgsqlDbFunctionsExtensions.ToDate), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    private static readonly MethodInfo ToTimestamp = typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(
        nameof(NpgsqlDbFunctionsExtensions.ToTimestamp), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    private static readonly MethodInfo FirstOrDefaultMethodInfoWithoutArgs
        = typeof(Enumerable).GetRuntimeMethods().Single(
            m => m.Name == nameof(Enumerable.FirstOrDefault)
                && m.GetParameters().Length == 1).MakeGenericMethod(typeof(char));

    private static readonly MethodInfo LastOrDefaultMethodInfoWithoutArgs
        = typeof(Enumerable).GetRuntimeMethods().Single(
            m => m.Name == nameof(Enumerable.LastOrDefault)
                && m.GetParameters().Length == 1).MakeGenericMethod(typeof(char));

    // ReSharper disable InconsistentNaming
    private static readonly MethodInfo String_Join1 =
        typeof(string).GetMethod(nameof(string.Join), [typeof(string), typeof(object[])])!;

    private static readonly MethodInfo String_Join2 =
        typeof(string).GetMethod(nameof(string.Join), [typeof(string), typeof(string[])])!;

    private static readonly MethodInfo String_Join3 =
        typeof(string).GetMethod(nameof(string.Join), [typeof(char), typeof(object[])])!;

    private static readonly MethodInfo String_Join4 =
        typeof(string).GetMethod(nameof(string.Join), [typeof(char), typeof(string[])])!;

    private static readonly MethodInfo String_Join5 =
        typeof(string).GetMethod(nameof(string.Join), [typeof(string), typeof(IEnumerable<string>)])!;

    private static readonly MethodInfo String_Join_generic1 =
        typeof(string).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(
                m => m is { Name: nameof(string.Join), IsGenericMethod: true }
                    && m.GetParameters().Length == 2
                    && m.GetParameters()[0].ParameterType == typeof(string));

    private static readonly MethodInfo String_Join_generic2 =
        typeof(string).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(
                m => m is { Name: nameof(string.Join), IsGenericMethod: true }
                    && m.GetParameters().Length == 2
                    && m.GetParameters()[0].ParameterType == typeof(char));
    // ReSharper restore InconsistentNaming

    #endregion

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

        if (method == FirstOrDefaultMethodInfoWithoutArgs)
        {
            var argument = arguments[0];
            return _sqlExpressionFactory.Function(
                "substr",
                [argument, _sqlExpressionFactory.Constant(1), _sqlExpressionFactory.Constant(1)],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[3],
                method.ReturnType);
        }

        if (method == LastOrDefaultMethodInfoWithoutArgs)
        {
            var argument = arguments[0];
            return _sqlExpressionFactory.Function(
                "substr",
                [
                    argument,
                    _sqlExpressionFactory.Function(
                        "length",
                        [argument],
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
        if (method == IndexOfString || method == IndexOfChar)
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

        if (method == Replace)
        {
            var oldValue = arguments[0];
            var newValue = arguments[1];
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

        if (method == ToLower || method == ToUpper)
        {
            return _sqlExpressionFactory.Function(
                method == ToLower ? "lower" : "upper",
                [instance!],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                method.ReturnType,
                instance!.TypeMapping);
        }

        if (method == Substring || method == SubstringWithLength)
        {
            var args =
                method == Substring
                    ? [instance!, GenerateOneBasedIndexExpression(arguments[0])]
                    : new[] { instance!, GenerateOneBasedIndexExpression(arguments[0]), arguments[1] };
            return _sqlExpressionFactory.Function(
                "substring",
                args,
                nullable: true,
                argumentsPropagateNullability: TrueArrays[args.Length],
                method.ReturnType,
                instance!.TypeMapping);
        }

        if (method == IsNullOrWhiteSpace)
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

        var isTrimStart = method == TrimStartWithNoParam || method == TrimStartWithChars || method == TrimStartWithSingleChar;
        var isTrimEnd = method == TrimEndWithNoParam || method == TrimEndWithChars || method == TrimEndWithSingleChar;
        var isTrimBoth = method == TrimBothWithNoParam || method == TrimBothWithChars || method == TrimBothWithSingleChar;
        if (isTrimStart || isTrimEnd || isTrimBoth)
        {
            char[]? trimChars = null;

            if (method == TrimStartWithChars
                || method == TrimStartWithSingleChar
                || method == TrimEndWithChars
                || method == TrimEndWithSingleChar
                || method == TrimBothWithChars
                || method == TrimBothWithSingleChar)
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

        if (method == PadLeft || method == PadLeftWithChar || method == PadRight || method == PadRightWithChar)
        {
            var args =
                method == PadLeft || method == PadRight
                    ? [instance!, arguments[0]]
                    : new[] { instance!, arguments[0], arguments[1] };

            var padFunc = _sqlExpressionFactory.Function(
                method == PadLeft || method == PadLeftWithChar ? "lpad" : "rpad",
                args,
                nullable: true,
                argumentsPropagateNullability: TrueArrays[args.Length],
                instance!.Type,
                instance.TypeMapping);
            var lengthFunc = _sqlExpressionFactory.Function("length", [instance], true, [true], typeof(int));
            return _sqlExpressionFactory.Case([new CaseWhenClause(_sqlExpressionFactory.MakeBinary(ExpressionType.GreaterThanOrEqual, lengthFunc, arguments[0], null)!, instance)], padFunc);
        }

        if (method.DeclaringType == typeof(string)
            && (method == String_Join1
                || method == String_Join2
                || method == String_Join3
                || method == String_Join4
                || method == String_Join5
                || method.IsClosedFormOf(String_Join_generic1)
                || method.IsClosedFormOf(String_Join_generic2))
            && arguments[1].TypeMapping is NpgsqlArrayTypeMapping)
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

        return null;
    }

    private SqlExpression? TranslateDbFunctionsMethod(SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
    {
        if (method == Reverse)
        {
            return _sqlExpressionFactory.Function(
                "reverse",
                [arguments[1]],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                typeof(string),
                arguments[1].TypeMapping);
        }

        if (method == StringToArray)
        {
            // Note that string_to_array always returns text[], regardless of the input type
            return _sqlExpressionFactory.Function(
                "string_to_array",
                [arguments[1], arguments[2]],
                nullable: true,
                argumentsPropagateNullability: [true, false],
                typeof(string[]),
                _typeMappingSource.FindMapping(typeof(string[])));
        }

        if (method == StringToArrayNullString)
        {
            // Note that string_to_array always returns text[], regardless of the input type
            return _sqlExpressionFactory.Function(
                "string_to_array",
                [arguments[1], arguments[2], arguments[3]],
                nullable: true,
                argumentsPropagateNullability: [true, false, false],
                typeof(string[]),
                _typeMappingSource.FindMapping(typeof(string[])));
        }

        if (method == ToDate)
        {
            return _sqlExpressionFactory.Function(
                "to_date",
                [arguments[1], arguments[2]],
                nullable: true,
                argumentsPropagateNullability: [true, true],
                typeof(DateOnly),
                _typeMappingSource.FindMapping(typeof(DateOnly))
            );
        }

        if (method == ToTimestamp)
        {
            return _sqlExpressionFactory.Function(
                "to_timestamp",
                [arguments[1], arguments[2]],
                nullable: true,
                argumentsPropagateNullability: [true, true],
                typeof(DateTime),
                _typeMappingSource.FindMapping(typeof(DateTime))
            );
        }

        return null;
    }

    private SqlExpression GenerateOneBasedIndexExpression(SqlExpression expression)
        => expression is SqlConstantExpression constant
            ? _sqlExpressionFactory.Constant(Convert.ToInt32(constant.Value) + 1, constant.TypeMapping)
            : _sqlExpressionFactory.Add(expression, _sqlExpressionFactory.Constant(1));
}
