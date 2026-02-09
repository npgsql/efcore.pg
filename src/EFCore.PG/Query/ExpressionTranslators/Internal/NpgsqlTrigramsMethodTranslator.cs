using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlTrigramsMethodTranslator(
    IRelationalTypeMappingSource typeMappingSource,
    NpgsqlSqlExpressionFactory sqlExpressionFactory,
    IModel model)
    : IMethodCallTranslator
{
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory = sqlExpressionFactory;
    private readonly RelationalTypeMapping _boolMapping = typeMappingSource.FindMapping(typeof(bool), model)!;
    private readonly RelationalTypeMapping _floatMapping = typeMappingSource.FindMapping(typeof(float), model)!;

    private static readonly bool[][] TrueArrays = [[], [true], [true, true]];

#pragma warning disable EF1001
    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType != typeof(NpgsqlTrigramsDbFunctionsExtensions))
        {
            return null;
        }

        return method.Name switch
        {
            nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsShow) => Function("show_trgm"),
            nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsSimilarity) => Function("similarity"),
            nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsWordSimilarity) => Function("word_similarity"),
            nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsStrictWordSimilarity) => Function("strict_word_similarity"),

            nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsAreSimilar) => BoolOperator("%"),
            nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsAreWordSimilar) => BoolOperator("<%"),
            nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsAreNotWordSimilar) => BoolOperator("%>"),
            nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsAreStrictWordSimilar) => BoolOperator("<<%"),
            nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsAreNotStrictWordSimilar) => BoolOperator("%>>"),

            nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsSimilarityDistance) => FloatOperator("<->"),
            nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsWordSimilarityDistance) => FloatOperator("<<->"),
            nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsWordSimilarityDistanceInverted) => FloatOperator("<->>"),
            nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsStrictWordSimilarityDistance) => FloatOperator("<<<->"),
            nameof(NpgsqlTrigramsDbFunctionsExtensions.TrigramsStrictWordSimilarityDistanceInverted) => FloatOperator("<->>>"),

            _ => null
        };

        SqlExpression Function(string name)
            => _sqlExpressionFactory.Function(
                name,
                arguments.Skip(1),
                nullable: true,
                argumentsPropagateNullability: TrueArrays[arguments.Count - 1],
                method.ReturnType);

        PgUnknownBinaryExpression BoolOperator(string op)
            => new(
                _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                op,
                _boolMapping.ClrType,
                _boolMapping);

        PgUnknownBinaryExpression FloatOperator(string op)
            => new(
                _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                op,
                _floatMapping.ClrType,
                _floatMapping);
    }
#pragma warning restore EF1001
}
