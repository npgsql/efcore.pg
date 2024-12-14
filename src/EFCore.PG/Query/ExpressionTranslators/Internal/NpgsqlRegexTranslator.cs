using System.Text.RegularExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     Translates Regex method calls into their corresponding PostgreSQL equivalent for database-side processing.
/// </summary>
/// <remarks>
///     http://www.postgresql.org/docs/current/static/functions-matching.html
/// </remarks>
public class NpgsqlRegexTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo IsMatch =
        typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), [typeof(string), typeof(string)])!;

    private static readonly MethodInfo IsMatchWithRegexOptions =
        typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), [typeof(string), typeof(string), typeof(RegexOptions)])!;

    private static readonly MethodInfo Replace =
        typeof(Regex).GetRuntimeMethod(nameof(Regex.Replace), [typeof(string), typeof(string), typeof(string)])!;

    private static readonly MethodInfo ReplaceWithRegexOptions =
        typeof(Regex).GetRuntimeMethod(nameof(Regex.Replace), [typeof(string), typeof(string), typeof(string), typeof(RegexOptions)])!;

    private static readonly MethodInfo Count =
        typeof(Regex).GetRuntimeMethod(nameof(Regex.Count), [typeof(string), typeof(string)])!;

    private static readonly MethodInfo CountWithRegexOptions =
        typeof(Regex).GetRuntimeMethod(nameof(Regex.Count), [typeof(string), typeof(string), typeof(RegexOptions)])!;

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
        => TranslateIsMatch(instance, method, arguments, logger)
            ?? TranslateRegexReplace(method, arguments, logger)
            ?? TranslateCount(method, arguments, logger);

    private SqlExpression? TranslateIsMatch(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method != IsMatch && method != IsMatchWithRegexOptions)
        {
            return null;
        }

        var (input, pattern) = (arguments[0], arguments[1]);
        var typeMapping = ExpressionExtensions.InferTypeMapping(input, pattern);

        RegexOptions options;

        if (method == IsMatch)
        {
            options = RegexOptions.None;
        }
        else if (arguments[2] is SqlConstantExpression { Value: RegexOptions regexOptions })
        {
            options = regexOptions;
        }
        else
        {
            return null; // We don't support non-constant regex options
        }

        return (options & UnsupportedRegexOptions) == 0
            ? _sqlExpressionFactory.RegexMatch(
                _sqlExpressionFactory.ApplyTypeMapping(input, typeMapping),
                _sqlExpressionFactory.ApplyTypeMapping(pattern, typeMapping),
                options)
            : null;
    }

    private SqlExpression? TranslateRegexReplace(
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method != Replace && method != ReplaceWithRegexOptions)
        {
            return null;
        }

        var (input, pattern, replacement) = (arguments[0], arguments[1], arguments[2]);
        var typeMapping = ExpressionExtensions.InferTypeMapping(input, pattern, replacement);

        RegexOptions options;

        if (method == Replace)
        {
            options = RegexOptions.None;
        }
        else if (arguments[3] is SqlConstantExpression { Value: RegexOptions regexOptions })
        {
            options = regexOptions;
        }
        else
        {
            return null; // We don't support non-constant regex options
        }

        if ((options & UnsupportedRegexOptions) != 0)
        {
            return null;
        }

        var translatedOptions = TranslateOptions(options);

        if (translatedOptions.Length is 0)
        {
            return _sqlExpressionFactory.Function("regexp_replace",
                [
                    _sqlExpressionFactory.ApplyTypeMapping(input, typeMapping),
                    _sqlExpressionFactory.ApplyTypeMapping(pattern, typeMapping),
                    _sqlExpressionFactory.ApplyTypeMapping(replacement, typeMapping)
                ],
                nullable: true,
                [true, true, true],
                typeof(string),
                _typeMappingSource.FindMapping(typeof(string)));
        }

        return _sqlExpressionFactory.Function(
            "regexp_replace",
            [
                _sqlExpressionFactory.ApplyTypeMapping(input, typeMapping),
                _sqlExpressionFactory.ApplyTypeMapping(pattern, typeMapping),
                _sqlExpressionFactory.ApplyTypeMapping(replacement, typeMapping),
                _sqlExpressionFactory.Constant(translatedOptions)
            ],
            nullable: true,
            [true, true, true, true],
            typeof(string),
            _typeMappingSource.FindMapping(typeof(string)));
    }

    private SqlExpression? TranslateCount(
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (!_supportRegexCount || (method != Count && method != CountWithRegexOptions))
        {
            return null;
        }

        var (input, pattern) = (arguments[0], arguments[1]);
        var typeMapping = ExpressionExtensions.InferTypeMapping(input, pattern);

        RegexOptions options;

        if (method == Count)
        {
            options = RegexOptions.None;
        }
        else if (arguments[2] is SqlConstantExpression { Value: RegexOptions regexOptions })
        {
            options = regexOptions;
        }
        else
        {
            return null; // We don't support non-constant regex options
        }

        if ((options & UnsupportedRegexOptions) != 0)
        {
            return null;
        }

        var translatedOptions = TranslateOptions(options);

        if (translatedOptions.Length is 0)
        {
            return _sqlExpressionFactory.Function(
                "regexp_count",
                [
                    _sqlExpressionFactory.ApplyTypeMapping(input, typeMapping),
                    _sqlExpressionFactory.ApplyTypeMapping(pattern, typeMapping)
                ],
                nullable: true,
                [true, true],
                typeof(int?),
                _typeMappingSource.FindMapping(typeof(int)));
        }

        return _sqlExpressionFactory.Function(
            "regexp_count",
            [
                _sqlExpressionFactory.ApplyTypeMapping(input, typeMapping),
                _sqlExpressionFactory.ApplyTypeMapping(pattern, typeMapping),
                //starting position has to be set to use the options in postgres
                _sqlExpressionFactory.Constant(1),
                _sqlExpressionFactory.Constant(translatedOptions)
            ],
            nullable: true,
            [true, true, true, true],
            typeof(int?),
            _typeMappingSource.FindMapping(typeof(int)));
    }

    private static string TranslateOptions(RegexOptions options)
    {
        if (options is RegexOptions.Singleline)
        {
            return string.Empty;
        }

        var result = string.Empty;

        if (options.HasFlag(RegexOptions.Multiline))
        {
            result += "n";
        }else if(!options.HasFlag(RegexOptions.Singleline))
        {
            result += "p";
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
