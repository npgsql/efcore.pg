using System.Collections.ObjectModel;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
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
    private readonly RelationalTypeMapping _ltreeArrayTypeMapping;
    private readonly RelationalTypeMapping _lqueryTypeMapping;
    private readonly RelationalTypeMapping _lqueryArrayTypeMapping;
    private readonly RelationalTypeMapping _ltxtqueryTypeMapping;

    private static readonly MethodInfo IsAncestorOf =
        typeof(LTree).GetRuntimeMethod(nameof(LTree.IsAncestorOf), new[] { typeof(LTree) })!;

    private static readonly MethodInfo IsDescendantOf =
        typeof(LTree).GetRuntimeMethod(nameof(LTree.IsDescendantOf), new[] { typeof(LTree) })!;

    private static readonly MethodInfo MatchesLQuery =
        typeof(LTree).GetRuntimeMethod(nameof(LTree.MatchesLQuery), new[] { typeof(string) })!;

    private static readonly MethodInfo MatchesLTxtQuery =
        typeof(LTree).GetRuntimeMethod(nameof(LTree.MatchesLTxtQuery), new[] { typeof(string) })!;

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
        _ltreeArrayTypeMapping = typeMappingSource.FindMapping(typeof(LTree[]), model)!;
        _lqueryTypeMapping = typeMappingSource.FindMapping("lquery")!;
        _lqueryArrayTypeMapping = typeMappingSource.FindMapping("lquery[]")!;
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
                    => new PostgresBinaryExpression(
                        PostgresExpressionType.Contains,
                        ApplyTypeMappingOrConvert(instance!, _ltreeTypeMapping),
                        ApplyTypeMappingOrConvert(arguments[0], _ltreeTypeMapping),
                        typeof(bool),
                        _boolTypeMapping),

                nameof(LTree.IsDescendantOf)
                    => new PostgresBinaryExpression(
                        PostgresExpressionType.ContainedBy,
                        ApplyTypeMappingOrConvert(instance!, _ltreeTypeMapping),
                        ApplyTypeMappingOrConvert(arguments[0], _ltreeTypeMapping),
                        typeof(bool),
                        _boolTypeMapping),

                nameof(LTree.MatchesLQuery)
                    => new PostgresBinaryExpression(
                        PostgresExpressionType.LTreeMatches,
                        ApplyTypeMappingOrConvert(instance!, _ltreeTypeMapping),
                        ApplyTypeMappingOrConvert(arguments[0], _lqueryTypeMapping),
                        typeof(bool),
                        _boolTypeMapping),

                nameof(LTree.MatchesLTxtQuery)
                    => new PostgresBinaryExpression(
                        PostgresExpressionType.LTreeMatches,
                        ApplyTypeMappingOrConvert(instance!, _ltreeTypeMapping),
                        ApplyTypeMappingOrConvert(arguments[0], _ltxtqueryTypeMapping),
                        typeof(bool),
                        _boolTypeMapping),

                nameof(LTree.Subtree)
                    => _sqlExpressionFactory.Function(
                        "subltree",
                        new[] { instance!, arguments[0], arguments[1] },
                        nullable: true,
                        TrueArrays[3],
                        typeof(LTree),
                        _ltreeTypeMapping),

                nameof(LTree.Subpath)
                    => _sqlExpressionFactory.Function(
                        "subpath",
                        arguments.Count == 2
                            ? new[] { instance!, arguments[0], arguments[1] }
                            : new[] { instance!, arguments[0] },
                        nullable: true,
                        arguments.Count == 2 ? TrueArrays[3] : TrueArrays[2],
                        typeof(LTree),
                        _ltreeTypeMapping),

                nameof(LTree.Index)
                    => _sqlExpressionFactory.Function(
                        "index",
                        arguments.Count == 2
                            ? new[] { instance!, arguments[0], arguments[1] }
                            : new[] { instance!, arguments[0] },
                        nullable: true,
                        arguments.Count == 2 ? TrueArrays[3] : TrueArrays[2],
                        typeof(int)),

                nameof(LTree.LongestCommonAncestor)
                    => _sqlExpressionFactory.Function(
                        "lca",
                        new[] { arguments[0] },
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
                new[] { instance! },
                nullable: true,
                TrueArrays[1],
                typeof(int))
            : null;

    /// <summary>
    /// Called directly from <see cref="NpgsqlSqlTranslatingExpressionVisitor"/> to translate LTree array-related constructs which
    /// cannot be translated in regular method translators, since they require accessing lambdas.
    /// </summary>
    public virtual Expression? VisitArrayMethodCall(
        NpgsqlSqlTranslatingExpressionVisitor sqlTranslatingExpressionVisitor,
        MethodInfo method,
        ReadOnlyCollection<Expression> arguments)
    {
        var array = arguments[0];
        {
            if (method.IsClosedFormOf(EnumerableMethods.AnyWithPredicate) &&
                arguments[1] is LambdaExpression wherePredicate &&
                wherePredicate.Body is MethodCallExpression wherePredicateMethodCall)
            {
                var predicateMethod = wherePredicateMethodCall.Method;
                var predicateInstance = wherePredicateMethodCall.Object!;
                var predicateArguments = wherePredicateMethodCall.Arguments;

                // Pattern match: new[] { "q1", "q2" }.Any(q => e.SomeLTree.MatchesLQuery(q))
                // Translation: s.SomeLTree ? ARRAY['q1','q2']
                if (predicateMethod == MatchesLQuery && predicateArguments[0] == wherePredicate.Parameters[0])
                {
                    return new PostgresBinaryExpression(
                        PostgresExpressionType.LTreeMatchesAny,
                        ApplyTypeMappingOrConvert(Visit(predicateInstance), _ltreeTypeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(Visit(array), _lqueryArrayTypeMapping),
                        typeof(bool),
                        _boolTypeMapping);
                }

                // Pattern match: new[] { "t1", "t2" }.Any(t => t.IsAncestorOf(e.SomeLTree))
                // Translation: ARRAY['t1','t2'] @> s.SomeLTree
                if (predicateMethod == IsAncestorOf && predicateInstance == wherePredicate.Parameters[0])
                {
                    return new PostgresBinaryExpression(
                        PostgresExpressionType.Contains,
                        _sqlExpressionFactory.ApplyTypeMapping(Visit(array), _ltreeArrayTypeMapping),
                        ApplyTypeMappingOrConvert(Visit(predicateArguments[0]), _ltreeTypeMapping),
                        typeof(bool),
                        _boolTypeMapping);
                }

                // Pattern match: new[] { "t1", "t2" }.Any(t => t.IsDescendantOf(e.SomeLTree))
                // Translation: s.SomeLTree <@ ARRAY['t1','t2']
                if (predicateMethod == IsDescendantOf && predicateInstance == wherePredicate.Parameters[0])
                {
                    return new PostgresBinaryExpression(
                        PostgresExpressionType.ContainedBy,
                        _sqlExpressionFactory.ApplyTypeMapping(Visit(array), _ltreeArrayTypeMapping),
                        ApplyTypeMappingOrConvert(Visit(predicateArguments[0]), _ltreeTypeMapping),
                        typeof(bool),
                        _boolTypeMapping);
                }

                // Pattern match: new[] { "t1", "t2" }.Any(t => t.MatchesLQuery(lquery))
                // Translation: ARRAY['t1','t2'] ~ lquery
                if (predicateMethod == MatchesLQuery && predicateInstance == wherePredicate.Parameters[0])
                {
                    return new PostgresBinaryExpression(
                        PostgresExpressionType.LTreeMatches,
                        _sqlExpressionFactory.ApplyTypeMapping(Visit(array), _ltreeArrayTypeMapping),
                        ApplyTypeMappingOrConvert(Visit(predicateArguments[0]), _lqueryTypeMapping),
                        typeof(bool),
                        _boolTypeMapping);
                }

                // Pattern match: new[] { "t1", "t2" }.Any(t => t.MatchesLTxtQuery(ltxtquery))
                // Translation: ARRAY['t1','t2'] @ ltxtquery
                if (predicateMethod == MatchesLTxtQuery && predicateInstance == wherePredicate.Parameters[0])
                {
                    return new PostgresBinaryExpression(
                        PostgresExpressionType.LTreeMatches,
                        _sqlExpressionFactory.ApplyTypeMapping(Visit(array), _ltreeArrayTypeMapping),
                        ApplyTypeMappingOrConvert(Visit(predicateArguments[0]), _ltxtqueryTypeMapping),
                        typeof(bool),
                        _boolTypeMapping);
                }

                // Any within Any (i.e. intersection)
                if (predicateMethod.IsClosedFormOf(EnumerableMethods.AnyWithPredicate) &&
                    predicateArguments[1] is LambdaExpression nestedWherePredicate &&
                    nestedWherePredicate.Body is MethodCallExpression nestedWherePredicateMethodCall)
                {
                    var nestedPredicateMethod = nestedWherePredicateMethodCall.Method;
                    var nestedPredicateInstance = nestedWherePredicateMethodCall.Object;
                    var nestedPredicateArguments = nestedWherePredicateMethodCall.Arguments;

                    // Pattern match: new[] { "t1", "t2" }.Any(t => lqueries.Any(q => t.MatchesLQuery(q)))
                    // Translation: ARRAY['t1','t2'] ~ ARRAY['q1', 'q2']
                    if (nestedPredicateMethod == MatchesLQuery &&
                        nestedPredicateInstance == wherePredicate.Parameters[0] &&
                        nestedPredicateArguments[0] == nestedWherePredicate.Parameters[0])
                    {
                        return new PostgresBinaryExpression(
                            PostgresExpressionType.LTreeMatchesAny,
                            _sqlExpressionFactory.ApplyTypeMapping(Visit(array), _ltreeArrayTypeMapping),
                            _sqlExpressionFactory.ApplyTypeMapping(Visit(predicateArguments[0]), _lqueryArrayTypeMapping),
                            typeof(bool),
                            _boolTypeMapping);
                    }
                }
            }
        }

        {
            if (method.IsClosedFormOf(EnumerableMethods.FirstOrDefaultWithPredicate) &&
                arguments[1] is LambdaExpression wherePredicate &&
                wherePredicate.Body is MethodCallExpression wherePredicateMethodCall)
            {
                var predicateMethod = wherePredicateMethodCall.Method;
                var predicateInstance = wherePredicateMethodCall.Object;
                var predicateArguments = wherePredicateMethodCall.Arguments;

                // Pattern match: new[] { "t1", "t2" }.FirstOrDefault(t => t.IsAncestorOf(e.SomeLTree))
                // Translation: ARRAY['t1','t2'] ?@> e.SomeLTree
                if (predicateMethod == IsAncestorOf && predicateInstance == wherePredicate.Parameters[0])
                {
                    return new PostgresBinaryExpression(
                        PostgresExpressionType.LTreeFirstAncestor,
                        _sqlExpressionFactory.ApplyTypeMapping(Visit(array), _ltreeArrayTypeMapping),
                        ApplyTypeMappingOrConvert(Visit(predicateArguments[0]), _ltreeTypeMapping),
                        typeof(LTree),
                        _ltreeTypeMapping);
                }

                // Pattern match: new[] { "t1", "t2" }.FirstOrDefault(t => t.IsDescendant(e.SomeLTree))
                // Translation: ARRAY['t1','t2'] ?<@ e.SomeLTree
                if (predicateMethod == IsDescendantOf && predicateInstance == wherePredicate.Parameters[0])
                {
                    return new PostgresBinaryExpression(
                        PostgresExpressionType.LTreeFirstDescendent,
                        _sqlExpressionFactory.ApplyTypeMapping(Visit(array), _ltreeArrayTypeMapping),
                        ApplyTypeMappingOrConvert(Visit(predicateArguments[0]), _ltreeTypeMapping),
                        typeof(LTree),
                        _ltreeTypeMapping);
                }

                // Pattern match: new[] { "t1", "t2" }.FirstOrDefault(t => t.MatchesLQuery(lquery))
                // Translation: ARRAY['t1','t2'] ?~ e.lquery
                if (predicateMethod == MatchesLQuery && predicateInstance == wherePredicate.Parameters[0])
                {
                    return new PostgresBinaryExpression(
                        PostgresExpressionType.LTreeFirstMatches,
                        _sqlExpressionFactory.ApplyTypeMapping(Visit(array), _ltreeArrayTypeMapping),
                        ApplyTypeMappingOrConvert(Visit(predicateArguments[0]), _lqueryTypeMapping),
                        typeof(LTree),
                        _ltreeTypeMapping);
                }

                // Pattern match: new[] { "t1", "t2" }.FirstOrDefault(t => t.MatchesLQuery(ltxtquery))
                // Translation: ARRAY['t1','t2'] ?@ e.ltxtquery
                if (predicateMethod == MatchesLTxtQuery && predicateInstance == wherePredicate.Parameters[0])
                {
                    return new PostgresBinaryExpression(
                        PostgresExpressionType.LTreeFirstMatches,
                        _sqlExpressionFactory.ApplyTypeMapping(Visit(array), _ltreeArrayTypeMapping),
                        ApplyTypeMappingOrConvert(Visit(predicateArguments[0]), _ltxtqueryTypeMapping),
                        typeof(string),
                        _ltreeTypeMapping);
                }
            }
        }

        return null;

        SqlExpression Visit(Expression expression)
            => (SqlExpression)sqlTranslatingExpressionVisitor.Visit(expression);
    }

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
