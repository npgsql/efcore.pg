using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     Translates Regex method calls into their corresponding PostgreSQL equivalent for database-side processing.
/// </summary>
/// <remarks>
///     http://www.postgresql.org/docs/current/static/functions-matching.html
/// </remarks>
public class NpgsqlRegexTranslator : IMethodCallTranslator
{
    private const RegexOptions UnsupportedRegexOptions = RegexOptions.RightToLeft | RegexOptions.ECMAScript;

    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly bool _supportRegexCount;
    private readonly NpgsqlTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlRegexTranslator(
        NpgsqlTypeMappingSource typeMappingSource,
        NpgsqlSqlExpressionFactory sqlExpressionFactory,
        bool supportRegexCount)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _supportRegexCount = supportRegexCount;
        _typeMappingSource = typeMappingSource;
    }

    /// <inheritdoc />
    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType != typeof(Regex) || !method.IsStatic)
        {
            return null;
        }

        return method.Name switch
        {
            nameof(Regex.IsMatch) when arguments.Count == 2
                && arguments[0].Type == typeof(string)
                && arguments[1].Type == typeof(string)
                => TranslateIsMatch(arguments),
            nameof(Regex.IsMatch) when arguments.Count == 3
                && arguments[0].Type == typeof(string)
                && arguments[1].Type == typeof(string)
                && TryGetOptions(arguments[2], out var options)
                => TranslateIsMatch(arguments, options),

            nameof(Regex.Replace) when arguments.Count == 3
                && arguments[0].Type == typeof(string)
                && arguments[1].Type == typeof(string)
                && arguments[2].Type == typeof(string)
                => TranslateReplace(arguments),
            nameof(Regex.Replace) when arguments.Count == 4
                && arguments[0].Type == typeof(string)
                && arguments[1].Type == typeof(string)
                && arguments[2].Type == typeof(string)
                && TryGetOptions(arguments[3], out var options)
                => TranslateReplace(arguments, options),

            nameof(Regex.Count) when _supportRegexCount
                && arguments.Count == 2
                && arguments[0].Type == typeof(string)
                && arguments[1].Type == typeof(string)
                => TranslateCount(arguments),
            nameof(Regex.Count) when _supportRegexCount
                && arguments.Count == 3
                && arguments[0].Type == typeof(string)
                && arguments[1].Type == typeof(string)
                && TryGetOptions(arguments[2], out var options)
                => TranslateCount(arguments, options),

            _ => null
        };

        static bool TryGetOptions(SqlExpression argument, out RegexOptions options)
        {
            if (argument is SqlConstantExpression { Value: RegexOptions o } && (o & UnsupportedRegexOptions) is 0)
            {
                options = o;
                return true;
            }

            options = default;
            return false;
        }
    }

    private PgRegexMatchExpression TranslateIsMatch(IReadOnlyList<SqlExpression> arguments, RegexOptions regexOptions = RegexOptions.None)
        => _sqlExpressionFactory.RegexMatch(
            _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]),
            _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
            regexOptions);

    private SqlExpression TranslateReplace(IReadOnlyList<SqlExpression> arguments, RegexOptions regexOptions = RegexOptions.None)
    {
        var (input, pattern, replacement) = (arguments[0], arguments[1], arguments[2]);

        List<SqlExpression> passingArguments =
        [
            _sqlExpressionFactory.ApplyDefaultTypeMapping(input),
            _sqlExpressionFactory.ApplyDefaultTypeMapping(pattern),
            _sqlExpressionFactory.ApplyDefaultTypeMapping(replacement)
        ];

        if (TranslateOptions(regexOptions) is { Length: not 0 } translatedOptions)
        {
            passingArguments.Add(_sqlExpressionFactory.Constant(translatedOptions));
        }

        return _sqlExpressionFactory.Function(
            "regexp_replace",
            passingArguments,
            nullable: true,
            TrueArrays[passingArguments.Count],
            typeof(string),
            _typeMappingSource.FindMapping(typeof(string)));
    }

    private SqlExpression TranslateCount(IReadOnlyList<SqlExpression> arguments, RegexOptions regexOptions = RegexOptions.None)
    {
        var (input, pattern) = (arguments[0], arguments[1]);

        List<SqlExpression> passingArguments =
        [
            _sqlExpressionFactory.ApplyDefaultTypeMapping(input),
            _sqlExpressionFactory.ApplyDefaultTypeMapping(pattern)
        ];

        if (TranslateOptions(regexOptions) is { Length: not 0 } translatedOptions)
        {
            passingArguments.AddRange(
            [
                _sqlExpressionFactory.Constant(1), // The starting position has to be set to use the options in PostgreSQL
                _sqlExpressionFactory.Constant(translatedOptions)
            ]);
        }

        return _sqlExpressionFactory.Function(
            "regexp_count",
            passingArguments,
            nullable: true,
            TrueArrays[passingArguments.Count],
            typeof(int),
            _typeMappingSource.FindMapping(typeof(int)));
    }

    private static string TranslateOptions(RegexOptions options)
    {
        string? result;

        switch (options)
        {
            case RegexOptions.Singleline:
                return string.Empty;
            case var _ when options.HasFlag(RegexOptions.Multiline):
                result = "n";
                break;
            case var _ when !options.HasFlag(RegexOptions.Singleline):
                result = "p";
                break;
            default:
                result = string.Empty;
                break;
        }

        if (options.HasFlag(RegexOptions.IgnoreCase))
        {
            result += "i";
        }

        if (options.HasFlag(RegexOptions.IgnorePatternWhitespace))
        {
            result += "x";
        }

        return result;
    }
}
