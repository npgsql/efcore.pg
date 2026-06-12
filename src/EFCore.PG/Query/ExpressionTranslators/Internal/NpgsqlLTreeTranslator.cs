using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlLTreeTranslator : IMethodCallTranslator, IMemberTranslator
{
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _boolTypeMapping;
    private readonly RelationalTypeMapping _ltreeTypeMapping;
    private readonly RelationalTypeMapping _lqueryTypeMapping;
    private readonly RelationalTypeMapping _ltxtqueryTypeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlLTreeTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        NpgsqlSqlExpressionFactory sqlExpressionFactory,
        IModel model)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _boolTypeMapping = typeMappingSource.FindMapping(typeof(bool), model)!;
        _ltreeTypeMapping = typeMappingSource.FindMapping(typeof(LTree), model)!;
        _lqueryTypeMapping = typeMappingSource.FindMapping("lquery")!;
        _ltxtqueryTypeMapping = typeMappingSource.FindMapping("ltxtquery")!;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType == typeof(LTree))
        {
            return method.Name switch
            {
                nameof(LTree.IsAncestorOf)
                    => new PgBinaryExpression(
                        PgExpressionType.Contains,
                        ApplyTypeMappingOrConvert(instance!, _ltreeTypeMapping),
                        ApplyTypeMappingOrConvert(arguments[0], _ltreeTypeMapping),
                        typeof(bool),
                        _boolTypeMapping),

                nameof(LTree.IsDescendantOf)
                    => new PgBinaryExpression(
                        PgExpressionType.ContainedBy,
                        ApplyTypeMappingOrConvert(instance!, _ltreeTypeMapping),
                        ApplyTypeMappingOrConvert(arguments[0], _ltreeTypeMapping),
                        typeof(bool),
                        _boolTypeMapping),

                nameof(LTree.MatchesLQuery)
                    => new PgBinaryExpression(
                        PgExpressionType.LTreeMatches,
                        ApplyTypeMappingOrConvert(instance!, _ltreeTypeMapping),
                        ApplyTypeMappingOrConvert(arguments[0], _lqueryTypeMapping),
                        typeof(bool),
                        _boolTypeMapping),

                nameof(LTree.MatchesLTxtQuery)
                    => new PgBinaryExpression(
                        PgExpressionType.LTreeMatches,
                        ApplyTypeMappingOrConvert(instance!, _ltreeTypeMapping),
                        ApplyTypeMappingOrConvert(arguments[0], _ltxtqueryTypeMapping),
                        typeof(bool),
                        _boolTypeMapping),

                nameof(LTree.Subtree)
                    => _sqlExpressionFactory.Function(
                        "subltree",
                        [instance!, arguments[0], arguments[1]],
                        nullable: true,
                        TrueArrays[3],
                        typeof(LTree),
                        _ltreeTypeMapping),

                nameof(LTree.Subpath)
                    => _sqlExpressionFactory.Function(
                        "subpath",
                        arguments.Count == 2
                            ? [instance!, arguments[0], arguments[1]]
                            : new[] { instance!, arguments[0] },
                        nullable: true,
                        arguments.Count == 2 ? TrueArrays[3] : TrueArrays[2],
                        typeof(LTree),
                        _ltreeTypeMapping),

                nameof(LTree.Index)
                    => _sqlExpressionFactory.Function(
                        "index",
                        arguments.Count == 2
                            ? [instance!, arguments[0], arguments[1]]
                            : new[] { instance!, arguments[0] },
                        nullable: true,
                        arguments.Count == 2 ? TrueArrays[3] : TrueArrays[2],
                        typeof(int)),

                nameof(LTree.LongestCommonAncestor)
                    => _sqlExpressionFactory.Function(
                        "lca",
                        [arguments[0]],
                        nullable: true,
                        TrueArrays[1],
                        typeof(LTree),
                        _ltreeTypeMapping),

                _ => null
            };
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => member.DeclaringType == typeof(LTree) && member.Name == nameof(LTree.NLevel)
            ? _sqlExpressionFactory.Function(
                "nlevel",
                [instance!],
                nullable: true,
                TrueArrays[1],
                typeof(int))
            : null;

    // Applying e.g. the LQuery type mapping on a function operator is a bit tricky.
    // If it's a constant, we can just apply the mapping: the constant will get rendered as an untyped string literal, and PG will
    // coerce it as the function parameter.
    // If it's a parameter, we can also just apply the mapping (which causes NpgsqlDbType to be set to LQuery).
    // For anything else, we may need an explicit cast to LQuery, e.g. a plain text column or a concatenation between strings;
    // apply the default type mapping and then apply an additional Convert node if the resulting mapping isn't what we need.
    private SqlExpression ApplyTypeMappingOrConvert(SqlExpression sqlExpression, RelationalTypeMapping typeMapping)
        => sqlExpression is SqlConstantExpression or SqlParameterExpression
            ? _sqlExpressionFactory.ApplyTypeMapping(sqlExpression, typeMapping)
            : _sqlExpressionFactory.ApplyDefaultTypeMapping(sqlExpression) is var expressionWithDefaultTypeMapping
            && expressionWithDefaultTypeMapping.TypeMapping!.StoreType == typeMapping.StoreType
                ? expressionWithDefaultTypeMapping
                : _sqlExpressionFactory.Convert(expressionWithDefaultTypeMapping, typeMapping.ClrType, typeMapping);
}
