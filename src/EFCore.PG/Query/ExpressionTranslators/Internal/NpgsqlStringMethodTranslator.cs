using System.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
/// Provides translation services for PostgreSQL string functions.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/static/functions-string.html
/// </remarks>
public class NpgsqlStringMethodTranslator : IMethodCallTranslator
{
    // Note: This is the PostgreSQL default and does not need to be explicitly specified
    private const char LikeEscapeChar = '\\';

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly SqlConstantExpression _whitespace;
    private readonly RelationalTypeMapping _textTypeMapping;

    #region MethodInfo

    private static readonly MethodInfo Contains                = typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) })!;
    private static readonly MethodInfo DbFunctionsReverse      = typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(nameof(NpgsqlDbFunctionsExtensions.Reverse), new[] { typeof(DbFunctions), typeof(string) })!;
    private static readonly MethodInfo EndsWith                = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) })!;
    private static readonly MethodInfo IndexOfChar             = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), new[] { typeof(char) })!;
    private static readonly MethodInfo IndexOfString           = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), new[] { typeof(string) })!;
    private static readonly MethodInfo IsNullOrWhiteSpace      = typeof(string).GetRuntimeMethod(nameof(string.IsNullOrWhiteSpace), new[] { typeof(string) })!;
    private static readonly MethodInfo PadLeft                 = typeof(string).GetRuntimeMethod(nameof(string.PadLeft), new[] { typeof(int) })!;
    private static readonly MethodInfo PadLeftWithChar         = typeof(string).GetRuntimeMethod(nameof(string.PadLeft), new[] { typeof(int), typeof(char) })!;
    private static readonly MethodInfo PadRight                = typeof(string).GetRuntimeMethod(nameof(string.PadRight), new[] { typeof(int) })!;
    private static readonly MethodInfo PadRightWithChar        = typeof(string).GetRuntimeMethod(nameof(string.PadRight), new[] { typeof(int), typeof(char) })!;
    private static readonly MethodInfo Replace                 = typeof(string).GetRuntimeMethod(nameof(string.Replace), new[] { typeof(string), typeof(string) })!;
    private static readonly MethodInfo StartsWith              = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) })!;
    private static readonly MethodInfo Substring               = typeof(string).GetTypeInfo().GetDeclaredMethods(nameof(string.Substring)).Single(m => m.GetParameters().Length == 1);
    private static readonly MethodInfo SubstringWithLength     = typeof(string).GetTypeInfo().GetDeclaredMethods(nameof(string.Substring)).Single(m => m.GetParameters().Length == 2);
    private static readonly MethodInfo ToLower                 = typeof(string).GetRuntimeMethod(nameof(string.ToLower), Array.Empty<Type>())!;
    private static readonly MethodInfo ToUpper                 = typeof(string).GetRuntimeMethod(nameof(string.ToUpper), Array.Empty<Type>())!;
    private static readonly MethodInfo TrimBothWithNoParam     = typeof(string).GetRuntimeMethod(nameof(string.Trim), Type.EmptyTypes)!;
    private static readonly MethodInfo TrimBothWithChars       = typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char[]) })!;
    private static readonly MethodInfo TrimBothWithSingleChar  = typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char) })!;
    private static readonly MethodInfo TrimEndWithNoParam      = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new Type[0])!;
    private static readonly MethodInfo TrimEndWithChars        = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new[] { typeof(char[]) })!;
    private static readonly MethodInfo TrimEndWithSingleChar   = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new[] { typeof(char) })!;
    private static readonly MethodInfo TrimStartWithNoParam    = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new Type[0])!;
    private static readonly MethodInfo TrimStartWithChars      = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char[]) })!;
    private static readonly MethodInfo TrimStartWithSingleChar = typeof(string).GetRuntimeMethod(nameof(string.TrimStart), new[] { typeof(char) })!;

    private static readonly MethodInfo FirstOrDefaultMethodInfoWithoutArgs
        = typeof(Enumerable).GetRuntimeMethods().Single(
            m => m.Name == nameof(Enumerable.FirstOrDefault)
                && m.GetParameters().Length == 1).MakeGenericMethod(typeof(char));

    private static readonly MethodInfo LastOrDefaultMethodInfoWithoutArgs
        = typeof(Enumerable).GetRuntimeMethods().Single(
            m => m.Name == nameof(Enumerable.LastOrDefault)
                && m.GetParameters().Length == 1).MakeGenericMethod(typeof(char));

    #endregion

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlStringMethodTranslator(
        NpgsqlTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory,
        IModel model)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _whitespace = _sqlExpressionFactory.Constant(
            @" \t\n\r",  // TODO: Complete this
            typeMappingSource.EStringTypeMapping);
        _textTypeMapping = typeMappingSource.FindMapping(typeof(string), model)!;
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
        if (method == IndexOfString || method == IndexOfChar)
        {
            var argument = arguments[0];
            var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance!, argument);

            return _sqlExpressionFactory.Subtract(
                _sqlExpressionFactory.Function(
                    "strpos",
                    new[]
                    {
                        _sqlExpressionFactory.ApplyTypeMapping(instance!, stringTypeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(argument, stringTypeMapping)
                    },
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
                new[]
                {
                    _sqlExpressionFactory.ApplyTypeMapping(instance!, stringTypeMapping),
                    _sqlExpressionFactory.ApplyTypeMapping(oldValue, stringTypeMapping),
                    _sqlExpressionFactory.ApplyTypeMapping(newValue, stringTypeMapping)
                },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[3],
                method.ReturnType,
                stringTypeMapping);
        }

        if (method == ToLower || method == ToUpper)
        {
            return _sqlExpressionFactory.Function(
                method == ToLower ? "lower" : "upper",
                new[] { instance! },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                method.ReturnType,
                instance!.TypeMapping);
        }

        if (method == Substring || method == SubstringWithLength)
        {
            var args =
                method == Substring
                    ? new[] { instance!, GenerateOneBasedIndexExpression(arguments[0]) }
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
                        new[]
                        {
                            argument,
                            _whitespace
                        },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        argument.Type,
                        argument.TypeMapping),
                    _sqlExpressionFactory.Constant(string.Empty, argument.TypeMapping)));
        }

        var isTrimStart = method == TrimStartWithNoParam || method == TrimStartWithChars || method == TrimStartWithSingleChar;
        var isTrimEnd   = method == TrimEndWithNoParam   || method == TrimEndWithChars   || method == TrimEndWithSingleChar;
        var isTrimBoth  = method == TrimBothWithNoParam  || method == TrimBothWithChars  || method == TrimBothWithSingleChar;
        if (isTrimStart || isTrimEnd || isTrimBoth)
        {
            char[]? trimChars = null;

            if (method == TrimStartWithChars || method == TrimStartWithSingleChar ||
                method == TrimEndWithChars   || method == TrimEndWithSingleChar   ||
                method == TrimBothWithChars  || method == TrimBothWithSingleChar)
            {
                var constantTrimChars = arguments[0] as SqlConstantExpression;
                if (constantTrimChars is null)
                {
                    return null; // Don't translate if trim chars isn't a constant
                }

                trimChars = constantTrimChars.Value is char c
                    ? new[] { c }
                    : (char[]?)constantTrimChars.Value;
            }

            return _sqlExpressionFactory.Function(
                isTrimStart ? "ltrim" : isTrimEnd ? "rtrim" : "btrim",
                new[]
                {
                    instance!,
                    trimChars is null || trimChars.Length == 0
                        ? _whitespace
                        : _sqlExpressionFactory.Constant(new string(trimChars))
                },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[2],
                instance!.Type,
                instance.TypeMapping);
        }

        if (method == Contains)
        {
            var pattern = arguments[0];
            var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance!, pattern);
            instance = _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);
            pattern = _sqlExpressionFactory.ApplyTypeMapping(pattern, stringTypeMapping);

            var strposCheck = _sqlExpressionFactory.GreaterThan(
                _sqlExpressionFactory.Function(
                    "strpos",
                    new[]
                    {
                        _sqlExpressionFactory.ApplyTypeMapping(instance!, stringTypeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(pattern, stringTypeMapping)
                    },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    typeof(int)),
                _sqlExpressionFactory.Constant(0));

            if (pattern is SqlConstantExpression constantPattern)
            {
                return (string?)constantPattern.Value == string.Empty
                    ? _sqlExpressionFactory.Constant(true)
                    : strposCheck;
            }

            return _sqlExpressionFactory.OrElse(
                _sqlExpressionFactory.Equal(
                    pattern,
                    _sqlExpressionFactory.Constant(string.Empty, stringTypeMapping)),
                strposCheck);
        }

        if (method == PadLeft || method == PadLeftWithChar || method == PadRight || method == PadRightWithChar)
        {
            var args =
                method == PadLeft || method == PadRight
                    ? new[] { instance!, arguments[0] }
                    : new[] { instance!, arguments[0], arguments[1] };

            return _sqlExpressionFactory.Function(
                method == PadLeft || method == PadLeftWithChar ? "lpad" : "rpad",
                args,
                nullable: true,
                argumentsPropagateNullability: TrueArrays[args.Length],
                instance!.Type,
                instance.TypeMapping);
        }

        if (method == FirstOrDefaultMethodInfoWithoutArgs)
        {
            var argument = arguments[0];
            return _sqlExpressionFactory.Function(
                "substr",
                new[] { argument, _sqlExpressionFactory.Constant(1), _sqlExpressionFactory.Constant(1) },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[3],
                method.ReturnType);
        }


        if (method == LastOrDefaultMethodInfoWithoutArgs)
        {
            var argument = arguments[0];
            return _sqlExpressionFactory.Function(
                "substr",
                new[]
                {
                    argument,
                    _sqlExpressionFactory.Function(
                        "length",
                        new[] { argument },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        typeof(int)),
                    _sqlExpressionFactory.Constant(1)
                },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[3],
                method.ReturnType);
        }

        if (method == DbFunctionsReverse)
        {
            return _sqlExpressionFactory.Function(
                "reverse",
                new[] { arguments[1] },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                typeof(string),
                arguments[1].TypeMapping);
        }

        if (method == StartsWith)
        {
            return TranslateStartsEndsWith(instance!, arguments[0], true);
        }

        if (method == EndsWith)
        {
            return TranslateStartsEndsWith(instance!, arguments[0], false);
        }

        return null;
    }

    private SqlExpression TranslateStartsEndsWith(SqlExpression instance, SqlExpression pattern, bool startsWith)
    {
        var stringTypeMapping = ExpressionExtensions.InferTypeMapping(instance, pattern);

        instance = _sqlExpressionFactory.ApplyTypeMapping(instance, stringTypeMapping);
        pattern = _sqlExpressionFactory.ApplyTypeMapping(pattern, stringTypeMapping);

        if (pattern is SqlConstantExpression constantExpression)
        {
            // The pattern is constant. Aside from null, we escape all special characters (%, _, \)
            // in C# and send a simple LIKE
            return constantExpression.Value is string constantPattern
                ? _sqlExpressionFactory.Like(
                    instance,
                    _sqlExpressionFactory.Constant(
                        startsWith
                            ? EscapeLikePattern(constantPattern) + '%'
                            : '%' + EscapeLikePattern(constantPattern)))
                : _sqlExpressionFactory.Like(instance, _sqlExpressionFactory.Constant(null, stringTypeMapping));
        }

        // The pattern is non-constant, we use LEFT or RIGHT to extract substring and compare.
        // For StartsWith we also first run a LIKE to quickly filter out most non-matching results (sargable, but imprecise
        // because of wildchars).
        SqlExpression leftRight = _sqlExpressionFactory.Function(
            startsWith ? "left" : "right",
            new[]
            {
                instance,
                _sqlExpressionFactory.Function(
                    "length",
                    new[] { pattern },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(int))
            },
            nullable: true,
            argumentsPropagateNullability: TrueArrays[2],
            typeof(string),
            stringTypeMapping);

        // LEFT/RIGHT of a citext return a text, so for non-default text mappings we apply an explicit cast.
        if (instance.TypeMapping != _textTypeMapping)
        {
            leftRight = _sqlExpressionFactory.Convert(leftRight, typeof(string), instance.TypeMapping);
        }

        // Also add an explicit cast on the pattern; this is only required because of
        // The following is only needed because of https://github.com/aspnet/EntityFrameworkCore/issues/19120
        var castPattern = pattern.TypeMapping == _textTypeMapping
            ? pattern
            : _sqlExpressionFactory.Convert(pattern, typeof(string), pattern.TypeMapping);

        return startsWith
            ? _sqlExpressionFactory.AndAlso(
                _sqlExpressionFactory.Like(
                    instance,
                    _sqlExpressionFactory.Add(
                        pattern,
                        _sqlExpressionFactory.Constant("%")),
                    _sqlExpressionFactory.Constant(string.Empty)),
                _sqlExpressionFactory.Equal(leftRight, castPattern))
            : _sqlExpressionFactory.Equal(leftRight, castPattern);
    }

    private bool IsLikeWildChar(char c) => c == '%' || c == '_';

    private string EscapeLikePattern(string pattern)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];
            if (IsLikeWildChar(c) || c == LikeEscapeChar)
            {
                builder.Append(LikeEscapeChar);
            }

            builder.Append(c);
        }

        return builder.ToString();
    }

    private SqlExpression GenerateOneBasedIndexExpression(SqlExpression expression)
        => expression is SqlConstantExpression constant
            ? _sqlExpressionFactory.Constant(Convert.ToInt32(constant.Value) + 1, constant.TypeMapping)
            : _sqlExpressionFactory.Add(expression, _sqlExpressionFactory.Constant(1));
}
