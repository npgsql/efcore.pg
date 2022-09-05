using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlTrigramsMethodTranslator : IMethodCallTranslator
{
    private static readonly Dictionary<MethodInfo, string> Functions = new()
    {
        [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsShow), typeof(DbFunctions), typeof(string))] = "show_trgm",
        [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsSimilarity), typeof(DbFunctions), typeof(string), typeof(string))] = "similarity",
        [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsWordSimilarity), typeof(DbFunctions), typeof(string), typeof(string))] = "word_similarity",
        [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsStrictWordSimilarity), typeof(DbFunctions), typeof(string), typeof(string))] = "strict_word_similarity"
    };

    private static readonly Dictionary<MethodInfo, string> BoolReturningOperators = new()
    {
        [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsAreSimilar), typeof(DbFunctions), typeof(string), typeof(string))] = "%",
        [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsAreWordSimilar), typeof(DbFunctions), typeof(string), typeof(string))] = "<%",
        [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsAreNotWordSimilar), typeof(DbFunctions), typeof(string), typeof(string))] = "%>",
        [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsAreStrictWordSimilar), typeof(DbFunctions), typeof(string), typeof(string))] = "<<%",
        [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsAreNotStrictWordSimilar), typeof(DbFunctions), typeof(string), typeof(string))] = "%>>"
    };

    private static readonly Dictionary<MethodInfo, string> FloatReturningOperators = new()
    {
        [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsSimilarityDistance), typeof(DbFunctions), typeof(string), typeof(string))] = "<->",
        [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsWordSimilarityDistance), typeof(DbFunctions), typeof(string), typeof(string))] = "<<->",
        [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsWordSimilarityDistanceInverted), typeof(DbFunctions), typeof(string), typeof(string))] = "<->>",
        [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsStrictWordSimilarityDistance), typeof(DbFunctions), typeof(string), typeof(string))] = "<<<->",
        [GetRuntimeMethod(nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsStrictWordSimilarityDistanceInverted), typeof(DbFunctions), typeof(string), typeof(string))] = "<->>>"
    };

    private static MethodInfo GetRuntimeMethod(string name, params Type[] parameters)
        => typeof(NpgsqlTrigramsDbFunctionsExtensions).GetRuntimeMethod(name, parameters)!;

    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _boolMapping;
    private readonly RelationalTypeMapping _floatMapping;

    private static readonly bool[][] TrueArrays =
    {
        Array.Empty<bool>(),
        new[] { true },
        new[] { true, true }
    };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlTrigramsMethodTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        NpgsqlSqlExpressionFactory sqlExpressionFactory,
        IModel model)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _boolMapping = typeMappingSource.FindMapping(typeof(bool), model)!;
        _floatMapping = typeMappingSource.FindMapping(typeof(float), model)!;
    }

#pragma warning disable EF1001
    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (Functions.TryGetValue(method, out var function))
        {
            return _sqlExpressionFactory.Function(
                function,
                arguments.Skip(1),
                nullable: true,
                argumentsPropagateNullability: TrueArrays[arguments.Count - 1],
                method.ReturnType);
        }

        if (BoolReturningOperators.TryGetValue(method, out var boolOperator))
        {
            return new PostgresUnknownBinaryExpression(
                _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                boolOperator,
                _boolMapping.ClrType,
                _boolMapping);
        }

        if (FloatReturningOperators.TryGetValue(method, out var floatOperator))
        {
            return new PostgresUnknownBinaryExpression(
                _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                floatOperator,
                _floatMapping.ClrType,
                _floatMapping);
        }

        return null;
    }
#pragma warning restore EF1001
}
