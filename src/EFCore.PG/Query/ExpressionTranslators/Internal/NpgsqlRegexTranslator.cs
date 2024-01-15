using System.Text.RegularExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     Translates Regex.IsMatch calls into PostgreSQL regex expressions for database-side processing.
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

    private const RegexOptions UnsupportedRegexOptions = RegexOptions.RightToLeft | RegexOptions.ECMAScript;

    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly NpgsqlTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlRegexTranslator(NpgsqlTypeMappingSource typeMappingSource, NpgsqlSqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;
    }

    /// <inheritdoc />
    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => TranslateIsMatch(instance, method, arguments, logger)
            ?? TranslateIsRegexMatch(method, arguments, logger);

    /// <inheritdoc />
    public virtual SqlExpression? TranslateIsMatch(
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

    private SqlExpression? TranslateIsRegexMatch(MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
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

        if (translatedOptions.Length > 0)
        {
            return _sqlExpressionFactory.Function(
                "regexp_replace",
                new[]
                {
                    _sqlExpressionFactory.ApplyTypeMapping(input, typeMapping),
                    _sqlExpressionFactory.ApplyTypeMapping(pattern, typeMapping),
                    _sqlExpressionFactory.ApplyTypeMapping(replacement, typeMapping),
                    _sqlExpressionFactory.Constant(TranslateOptions(options))
                },
                nullable: true,
                new[] { true, false, false, false },
                typeof(string),
                _typeMappingSource.FindMapping(typeof(string)));
        }

        return _sqlExpressionFactory.Function("regexp_replace",
            new[]
            {
                _sqlExpressionFactory.ApplyTypeMapping(input, typeMapping),
                _sqlExpressionFactory.ApplyTypeMapping(pattern, typeMapping),
                _sqlExpressionFactory.ApplyTypeMapping(replacement, typeMapping)
            },
            nullable: true,
            new[] { true, false, false},
            typeof(string),
            _typeMappingSource.FindMapping(typeof(string)));
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
