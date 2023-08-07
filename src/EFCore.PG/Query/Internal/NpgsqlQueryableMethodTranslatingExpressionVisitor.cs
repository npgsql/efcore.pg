using System.Diagnostics.CodeAnalysis;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
{
    private readonly NpgsqlTypeMappingSource _typeMappingSource;
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

    #region MethodInfos

    private static readonly MethodInfo Like2MethodInfo =
        typeof(DbFunctionsExtensions).GetRuntimeMethod(
            nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string) })!;

    // ReSharper disable once InconsistentNaming
    private static readonly MethodInfo ILike2MethodInfo
        = typeof(NpgsqlDbFunctionsExtensions).GetRuntimeMethod(
            nameof(NpgsqlDbFunctionsExtensions.ILike), new[] { typeof(DbFunctions), typeof(string), typeof(string) })!;

    private static readonly MethodInfo MatchesLQuery =
        typeof(LTree).GetRuntimeMethod(nameof(LTree.MatchesLQuery), new[] { typeof(string) })!;

    #endregion

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _typeMappingSource = (NpgsqlTypeMappingSource)relationalDependencies.TypeMappingSource;
        _sqlExpressionFactory = (NpgsqlSqlExpressionFactory)relationalDependencies.SqlExpressionFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected NpgsqlQueryableMethodTranslatingExpressionVisitor(NpgsqlQueryableMethodTranslatingExpressionVisitor parentVisitor)
        : base(parentVisitor)
    {
        _typeMappingSource = parentVisitor._typeMappingSource;
        _sqlExpressionFactory = parentVisitor._sqlExpressionFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        => new NpgsqlQueryableMethodTranslatingExpressionVisitor(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression TranslateCollection(
        SqlExpression sqlExpression,
        RelationalTypeMapping? elementTypeMapping,
        string tableAlias)
    {
        var elementClrType = sqlExpression.Type.GetSequenceType();

        // We support two kinds of primitive collections: the standard one with PostgreSQL arrays (where we use the unnest function), and
        // a special case for geometry collections, where we use
        SelectExpression selectExpression;

        // TODO: Parameters have no type mapping. We can check whether the expression type is one of the NTS geometry collection types,
        // though in a perfect world we'd actually infer this. In other words, when the type mapping of the element is inferred further on,
        // we'd replace the unnest expression with ST_Dump. We could even have a special expression type which means "indeterminate, must be
        // inferred".
        if (sqlExpression.TypeMapping is { StoreTypeNameBase: "geometry" or "geography" })
        {
            selectExpression = new SelectExpression(
                new TableValuedFunctionExpression(tableAlias, "ST_Dump", new[] { sqlExpression }),
                "geom", elementClrType, elementTypeMapping, isColumnNullable: false);
        }
        else
        {
            // Note that for unnest we have a special expression type extending TableValuedFunctionExpression, adding the ability to provide
            // an explicit column name for its output (SELECT * FROM unnest(array) AS f(foo)).
            // This is necessary since when the column name isn't explicitly specified, it is automatically identical to the table alias
            // (f above); since the table alias may get uniquified by EF, this would break queries.

            // TODO: When we have metadata to determine if the element is nullable, pass that here to SelectExpression
            // Note also that with PostgreSQL unnest, the output ordering is guaranteed to be the same as the input array, so we don't need
            // to add ordering like in most other providers (https://www.postgresql.org/docs/current/functions-array.html)
            // We also don't need to apply any casts or typing, since PG arrays are fully typed (unlike e.g. a JSON string).
            selectExpression = new SelectExpression(
                new PostgresUnnestExpression(tableAlias, sqlExpression, "value"),
                "value", elementClrType, elementTypeMapping, isColumnNullable: null);
        }

        Expression shaperExpression = new ProjectionBindingExpression(
            selectExpression, new ProjectionMember(), elementClrType.MakeNullable());

        if (elementClrType != shaperExpression.Type)
        {
            Check.DebugAssert(
                elementClrType.MakeNullable() == shaperExpression.Type,
                "expression.Type must be nullable of targetType");

            shaperExpression = Expression.Convert(shaperExpression, elementClrType);
        }

        return new ShapedQueryExpression(selectExpression, shaperExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression ApplyInferredTypeMappings(
        Expression expression,
        IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping?> inferredTypeMappings)
        => new NpgsqlInferredTypeMappingApplier(_typeMappingSource, _sqlExpressionFactory, inferredTypeMappings).Visit(expression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateAll(ShapedQueryExpression source, LambdaExpression predicate)
    {
        if (source.QueryExpression is SelectExpression
            {
                Tables: [(PostgresUnnestExpression or ValuesExpression { ColumnNames: ["_ord", "Value"] }) and var sourceTable],
                Predicate: null,
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null
            }
            && TranslateLambdaExpression(source, predicate) is { } translatedPredicate)
        {
            switch (translatedPredicate)
            {
                // Pattern match for: new[] { "a", "b", "c" }.All(p => EF.Functions.Like(e.SomeText, p)),
                // which we translate to WHERE s.""SomeText"" LIKE ALL (ARRAY['a','b','c'])
                case LikeExpression
                    {
                        Match: var match,
                        Pattern: ColumnExpression pattern,
                        EscapeChar: SqlConstantExpression { Value: "" }
                    }
                    when pattern.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source,
                        _sqlExpressionFactory.All(match, GetArray(sourceTable), PostgresAllOperatorType.Like));
                }

                // Pattern match for: new[] { "a", "b", "c" }.All(p => EF.Functions.Like(e.SomeText, p)),
                // which we translate to WHERE s.""SomeText"" LIKE ALL (ARRAY['a','b','c'])
                case PostgresILikeExpression
                    {
                        Match: var match,
                        Pattern: ColumnExpression pattern,
                        EscapeChar: SqlConstantExpression { Value: "" }
                    }
                    when pattern.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source,
                        _sqlExpressionFactory.All(match, GetArray(sourceTable), PostgresAllOperatorType.ILike));
                }

                // Pattern match for: e.SomeArray.All(p => ints.Contains(p)) over non-column,
                // using array containment (<@)
                case PostgresAnyExpression
                    {
                        Item: ColumnExpression sourceColumn,
                        Array: var otherArray
                    }
                    when sourceColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(source, _sqlExpressionFactory.ContainedBy(GetArray(sourceTable), otherArray));
                }

                // Pattern match for: new[] { 4, 5 }.All(p => e.SomeArray.Contains(p)) over column,
                // using array containment (<@)
                case PostgresBinaryExpression
                    {
                        OperatorType: PostgresExpressionType.Contains,
                        Left: var otherArray,
                        Right: PostgresNewArrayExpression { Expressions: [ColumnExpression sourceColumn] }
                    }
                    when sourceColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(source, _sqlExpressionFactory.ContainedBy(GetArray(sourceTable), otherArray));
                }
            }
        }

        return base.TranslateAll(source, predicate);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateAny(ShapedQueryExpression source, LambdaExpression? predicate)
    {
        if (source.QueryExpression is SelectExpression
            {
                Tables: [(PostgresUnnestExpression or ValuesExpression { ColumnNames: ["_ord", "Value"] }) and var sourceTable],
                Predicate: null,
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null
            })
        {
            // Pattern match: x.Array.Any()
            // Translation: cardinality(x.array) > 0 instead of EXISTS (SELECT 1 FROM FROM unnest(x.Array))
            if (predicate is null)
            {
                return BuildSimplifiedShapedQuery(
                    source,
                    _sqlExpressionFactory.GreaterThan(
                        _sqlExpressionFactory.Function(
                            "cardinality",
                            new[] { GetArray(sourceTable) },
                            nullable: true,
                            argumentsPropagateNullability: TrueArrays[1],
                            typeof(int)),
                        _sqlExpressionFactory.Constant(0)));
            }

            var translatedPredicate = TranslateLambdaExpression(source, predicate);
            if (translatedPredicate is null)
            {
                return null;
            }

            // Simplify Contains / array.Any(i => i == x)
            // Note that most other simplifications here convert ValuesExpression to unnest over array constructor, but we avoid doing that
            // here, since the relational translation for ValuesExpression is better.
            if (sourceTable is PostgresUnnestExpression
                && translatedPredicate is SqlBinaryExpression
                {
                    OperatorType: ExpressionType.Equal,
                    Left: var left,
                    Right: var right
                })
            {
                var item =
                    left is ColumnExpression leftColumn && ReferenceEquals(leftColumn.Table, sourceTable)
                        ? right
                        : right is ColumnExpression rightColumn && ReferenceEquals(rightColumn.Table, sourceTable)
                            ? left
                            : null;

                if (item is not null)
                {
                    var array = GetArray(sourceTable);

                    // When the array is a column, we translate Contains to array @> ARRAY[item]. GIN indexes on array are used, but null
                    // semantics is impossible without preventing index use.
                    switch (array)
                    {
                        case ColumnExpression:
                            if (item is SqlConstantExpression { Value: null })
                            {
                                // We special-case null constant item and use array_position instead, since it does
                                // nulls correctly (but doesn't use indexes)
                                // TODO: once lambda-based caching is implemented, move this to NpgsqlSqlNullabilityProcessor
                                // (https://github.com/dotnet/efcore/issues/17598) and do for parameters as well.
                                return BuildSimplifiedShapedQuery(
                                    source,
                                    _sqlExpressionFactory.IsNotNull(
                                        _sqlExpressionFactory.Function(
                                            "array_position",
                                            new[] { array, item },
                                            nullable: true,
                                            argumentsPropagateNullability: FalseArrays[2],
                                            typeof(int))));
                            }

                            return BuildSimplifiedShapedQuery(
                                source,
                                _sqlExpressionFactory.Contains(
                                    array,
                                    _sqlExpressionFactory.NewArrayOrConstant(new[] { item }, array.Type)));

                        // Don't do anything PG-specific for constant arrays since the general EF Core mechanism is fine
                        // for that case: item IN (1, 2, 3).
                        // After https://github.com/aspnet/EntityFrameworkCore/issues/16375 is done we may not need the
                        // check any more.
                        case SqlConstantExpression:
                            return null;

                        // Similar to ParameterExpression below, but when a bare subquery is present inside ANY(), PostgreSQL just compares
                        // against each of its resulting rows (just like IN). To "extract" the array result of the scalar subquery, we need
                        // to add an explicit cast (see #1803).
                        case ScalarSubqueryExpression subqueryExpression:
                            return BuildSimplifiedShapedQuery(
                                source,
                                _sqlExpressionFactory.Any(
                                    item,
                                    _sqlExpressionFactory.Convert(
                                        subqueryExpression, subqueryExpression.Type, subqueryExpression.TypeMapping),
                                    PostgresAnyOperatorType.Equal));

                        // For ParameterExpression, and for all other cases - e.g. array returned from some function -
                        // translate to e.SomeText = ANY (@p). This is superior to the general solution which will expand
                        // parameters to constants, since non-PG SQL does not support arrays.
                        // Note that this will allow indexes on the item to be used.
                        default:
                            return BuildSimplifiedShapedQuery(
                                source, _sqlExpressionFactory.Any(item, array, PostgresAnyOperatorType.Equal));
                    }
                }
            }

            switch (translatedPredicate)
            {
                // Pattern match: new[] { "a", "b", "c" }.Any(p => EF.Functions.Like(e.SomeText, p))
                // Translation: s.SomeText LIKE ANY (ARRAY['a','b','c'])
                case LikeExpression
                    {
                        Match: var match,
                        Pattern: ColumnExpression pattern,
                        EscapeChar: SqlConstantExpression { Value: "" }
                    }
                    when pattern.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source, _sqlExpressionFactory.Any(match, GetArray(sourceTable), PostgresAnyOperatorType.Like));
                }

                // Pattern match: new[] { "a", "b", "c" }.Any(p => EF.Functions.Like(e.SomeText, p))
                // Translation: s.SomeText LIKE ANY (ARRAY['a','b','c'])
                case PostgresILikeExpression
                    {
                        Match: var match,
                        Pattern: ColumnExpression pattern,
                        EscapeChar: SqlConstantExpression { Value: "" }
                    }
                    when pattern.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source, _sqlExpressionFactory.Any(match, GetArray(sourceTable), PostgresAnyOperatorType.ILike));
                }

                // Array overlap over non-column
                // Pattern match: e.SomeArray.Any(p => ints.Contains(p))
                // Translation: @ints && s.SomeArray
                case PostgresAnyExpression
                {
                    Item: ColumnExpression sourceColumn,
                    Array: var otherArray
                }
                    when sourceColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(source, _sqlExpressionFactory.Overlaps(GetArray(sourceTable), otherArray));
                }

                // Array overlap over column
                // Pattern match: new[] { 4, 5 }.Any(p => e.SomeArray.Contains(p))
                // Translation: s.SomeArray && ARRAY[4, 5]
                case PostgresBinaryExpression
                    {
                        OperatorType: PostgresExpressionType.Contains,
                        Left: var otherArray,
                        Right: PostgresNewArrayExpression { Expressions: [ColumnExpression sourceColumn] }
                    }
                    when sourceColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(source, _sqlExpressionFactory.Overlaps(GetArray(sourceTable), otherArray));
                }

                #region LTree translations

                // Pattern match: new[] { "q1", "q2" }.Any(q => e.SomeLTree.MatchesLQuery(q))
                // Translation: s.SomeLTree ? ARRAY['q1','q2']
                case PostgresBinaryExpression
                    {
                        OperatorType: PostgresExpressionType.LTreeMatches,
                        Left: var ltree,
                        Right: SqlUnaryExpression { OperatorType: ExpressionType.Convert, Operand: ColumnExpression lqueryColumn }
                    }
                    when lqueryColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source,
                        new PostgresBinaryExpression(
                            PostgresExpressionType.LTreeMatchesAny,
                            ltree,
                            _sqlExpressionFactory.ApplyTypeMapping(GetArray(sourceTable), _typeMappingSource.FindMapping("lquery[]")),
                            typeof(bool),
                            typeMapping: _typeMappingSource.FindMapping(typeof(bool))));
                }

                // Pattern match: new[] { "t1", "t2" }.Any(t => t.IsAncestorOf(e.SomeLTree))
                // Translation: ARRAY['t1','t2'] @> s.SomeLTree
                // Pattern match: new[] { "t1", "t2" }.Any(t => t.IsDescendantOf(e.SomeLTree))
                // Translation: ARRAY['t1','t2'] <@ s.SomeLTree
                case PostgresBinaryExpression
                    {
                        OperatorType: (PostgresExpressionType.Contains or PostgresExpressionType.ContainedBy) and var operatorType,
                        Left: ColumnExpression ltreeColumn,
                        // Contains/ContainedBy can happen for non-LTree types too, so check that
                        Right: { TypeMapping: NpgsqlLTreeTypeMapping } ltree
                    }
                    when ltreeColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source,
                        new PostgresBinaryExpression(
                            operatorType,
                            _sqlExpressionFactory.ApplyDefaultTypeMapping(GetArray(sourceTable)),
                            ltree,
                            typeof(bool),
                            typeMapping: _typeMappingSource.FindMapping(typeof(bool))));
                }

                // Pattern match: new[] { "t1", "t2" }.Any(t => t.MatchesLQuery(lquery))
                // Translation: ARRAY['t1','t2'] ~ lquery
                // Pattern match: new[] { "t1", "t2" }.Any(t => t.MatchesLTxtQuery(ltxtquery))
                // Translation: ARRAY['t1','t2'] @ ltxtquery
                case PostgresBinaryExpression
                    {
                        OperatorType: PostgresExpressionType.LTreeMatches,
                        Left: ColumnExpression ltreeColumn,
                        Right: var lquery
                    }
                    when ltreeColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source,
                        new PostgresBinaryExpression(
                            PostgresExpressionType.LTreeMatches,
                            _sqlExpressionFactory.ApplyDefaultTypeMapping(GetArray(sourceTable)),
                            lquery,
                            typeof(bool),
                            typeMapping: _typeMappingSource.FindMapping(typeof(bool))));
                }

                // Any within Any (i.e. intersection)
                // Pattern match: ltrees.Any(t => lqueries.Any(q => t.MatchesLQuery(q)))
                // Translate: ltrees ? lqueries
                case PostgresBinaryExpression
                    {
                        OperatorType: PostgresExpressionType.LTreeMatchesAny,
                        Left: ColumnExpression ltreeColumn,
                        Right: var lqueries
                    }
                    when ltreeColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source,
                        new PostgresBinaryExpression(
                            PostgresExpressionType.LTreeMatchesAny,
                            _sqlExpressionFactory.ApplyDefaultTypeMapping(GetArray(sourceTable)),
                            lqueries,
                            typeof(bool),
                            typeMapping: _typeMappingSource.FindMapping(typeof(bool))));
                }

                #endregion LTree translations
            }
        }

        // Pattern match: x.Array1.Intersect(x.Array2).Any()
        // Translation: x.Array1 && x.Array2
        if (predicate is null
            && source.QueryExpression is SelectExpression
            {
                Tables: [IntersectExpression
                {
                    Source1:
                    {
                        Tables: [PostgresUnnestExpression { Array: var array1 }],
                        GroupBy: [],
                        Having: null,
                        IsDistinct: false,
                        Limit: null,
                        Offset: null
                    },
                    Source2:
                    {
                        Tables: [PostgresUnnestExpression { Array: var array2 }],
                        GroupBy: [],
                        Having: null,
                        IsDistinct: false,
                        Limit: null,
                        Offset: null
                    }
                }],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null
            })
        {
            return BuildSimplifiedShapedQuery(source, _sqlExpressionFactory.Overlaps(array1, array2));
        }

        return base.TranslateAny(source, predicate);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateCount(ShapedQueryExpression source, LambdaExpression? predicate)
    {
        // Simplify x.Array.Count() => cardinality(x.Array) instead of SELECT COUNT(*) FROM unnest(x.Array)
        if (predicate is null && source.QueryExpression is SelectExpression
            {
                Tables: [PostgresUnnestExpression { Array: var array }],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null
            })
        {
            var translation = _sqlExpressionFactory.Function(
                "cardinality",
                new[] { array },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                typeof(int));

            return source.Update(
                _sqlExpressionFactory.Select(translation),
                Expression.Convert(
                    new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(int?)),
                    typeof(int)));
        }

        return base.TranslateCount(source, predicate);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2)
    {
        // Simplify x.Array.Concat(y.Array) => x.Array || y.Array instead of:
        // SELECT u.value FROM unnest(x.Array) UNION ALL SELECT u.value FROM unnest(y.Array)
        if (source1.QueryExpression is SelectExpression
            {
                Tables: [PostgresUnnestExpression { Array: var array1 } unnestExpression1],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null,
                Orderings: []
            }
            && source2.QueryExpression is SelectExpression
            {
                Tables: [PostgresUnnestExpression { Array: var array2 }],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null,
                Orderings: []
            }
            && TryGetProjectedColumn(source1, out var projectedColumn1)
            && TryGetProjectedColumn(source2, out var projectedColumn2))
        {
            Check.DebugAssert(projectedColumn1.Type == projectedColumn2.Type, "projectedColumn1.Type == projectedColumn2.Type");
            Check.DebugAssert(projectedColumn1.TypeMapping is not null || projectedColumn2.TypeMapping is not null,
                "Concat with no type mapping on either side (operation should be client-evaluated over parameters/constants");

            // TODO: Conflicting type mappings from both sides?
            var inferredTypeMapping = projectedColumn1.TypeMapping ?? projectedColumn2.TypeMapping;
            var unnestExpression = new PostgresUnnestExpression(
                unnestExpression1.Alias, _sqlExpressionFactory.Add(array1, array2), "value");
            var selectExpression = new SelectExpression(unnestExpression, "value", projectedColumn1.Type, inferredTypeMapping);

            return source1.UpdateQueryExpression(selectExpression);
        }

        return base.TranslateConcat(source1, source2);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateElementAtOrDefault(
        ShapedQueryExpression source,
        Expression index,
        bool returnDefault)
    {
        // Simplify x.Array[1] => x.Array[1] (using the PG array subscript operator) instead of a subquery with LIMIT/OFFSET
        if (!returnDefault && source.QueryExpression is SelectExpression
            {
                Tables: [PostgresUnnestExpression { Array: var array }],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Orderings: [],
                Limit: null,
                Offset: null
            })
        {
            var translatedIndex = TranslateExpression(index);
            if (translatedIndex == null)
            {
                return base.TranslateElementAtOrDefault(source, index, returnDefault);
            }

            // Index on array - but PostgreSQL arrays are 1-based, so adjust the index.
            var translation = _sqlExpressionFactory.ArrayIndex(array, GenerateOneBasedIndexExpression(translatedIndex));
            return source.Update(_sqlExpressionFactory.Select(translation), source.ShaperExpression);
        }

        return base.TranslateElementAtOrDefault(source, index, returnDefault);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateFirstOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        // Some LTree translations (see LTreeQueryTest)
        // Note that preprocessing normalizes FirstOrDefault(predicate) to Where(predicate).FirstOrDefault(), so the source's
        // select expression should already contain our predicate.
        if (source.QueryExpression is SelectExpression
            {
                Tables: [(PostgresUnnestExpression or ValuesExpression { ColumnNames: ["_ord", "Value"] }) and var sourceTable],
                Predicate: var translatedPredicate,
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null,
                Orderings: []
            }
            && translatedPredicate is null ^ predicate is null)
        {
            if (translatedPredicate is null)
            {
                translatedPredicate = TranslateLambdaExpression(source, predicate!);
                if (translatedPredicate is null)
                {
                    return null;
                }
            }

            switch (translatedPredicate)
            {
                // Pattern match: new[] { "t1", "t2" }.FirstOrDefault(t => t.IsAncestorOf(e.SomeLTree))
                // Translation: ARRAY['t1','t2'] ?@> e.SomeLTree
                // Pattern match: new[] { "t1", "t2" }.FirstOrDefault(t => t.IsDescendant(e.SomeLTree))
                // Translation: ARRAY['t1','t2'] ?<@ e.SomeLTree
                case PostgresBinaryExpression
                    {
                        OperatorType: (PostgresExpressionType.Contains or PostgresExpressionType.ContainedBy) and var operatorType,
                        Left: ColumnExpression ltreeColumn,
                        // Contains/ContainedBy can happen for non-LTree types too, so check that
                        Right: { TypeMapping: NpgsqlLTreeTypeMapping } ltree
                    }
                    when ltreeColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source,
                        new PostgresBinaryExpression(
                            operatorType == PostgresExpressionType.Contains
                                ? PostgresExpressionType.LTreeFirstAncestor
                                : PostgresExpressionType.LTreeFirstDescendent,
                            _sqlExpressionFactory.ApplyDefaultTypeMapping(GetArray(sourceTable)),
                            ltree,
                            typeof(LTree),
                            _typeMappingSource.FindMapping(typeof(LTree))));
                }

                // Pattern match: new[] { "t1", "t2" }.FirstOrDefault(t => t.MatchesLQuery(lquery))
                // Translation: ARRAY['t1','t2'] ?~ e.lquery
                // Pattern match: new[] { "t1", "t2" }.FirstOrDefault(t => t.MatchesLQuery(ltxtquery))
                // Translation: ARRAY['t1','t2'] ?@ e.ltxtquery
                case PostgresBinaryExpression
                    {
                        OperatorType: PostgresExpressionType.LTreeMatches,
                        Left: ColumnExpression ltreeColumn,
                        Right: var lquery
                    }
                    when ltreeColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source,
                        new PostgresBinaryExpression(
                            PostgresExpressionType.LTreeFirstMatches,
                            _sqlExpressionFactory.ApplyDefaultTypeMapping(GetArray(sourceTable)),
                            lquery,
                            typeof(LTree),
                            _typeMappingSource.FindMapping(typeof(LTree))));
                }
            }
        }

        return base.TranslateFirstOrDefault(source, predicate, returnType, returnDefault);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateSkip(ShapedQueryExpression source, Expression count)
    {
        // Translate Skip over array to the PostgreSQL slice operator (array.Skip(2) -> array[3,])
        if (source.QueryExpression is SelectExpression
            {
                Tables: [PostgresUnnestExpression { Array: var array } unnestExpression],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Orderings: [],
                Limit: null,
                Offset: null
            }
            && TryGetProjectedColumn(source, out var projectedColumn)
            && TranslateExpression(count) is { } translatedCount)
        {
            var selectExpression = new SelectExpression(
                new PostgresUnnestExpression(
                    unnestExpression.Alias,
                    new PostgresArraySliceExpression(
                        array,
                        lowerBound: GenerateOneBasedIndexExpression(translatedCount),
                        upperBound: null),
                    "value"),
                "value",
                projectedColumn.Type,
                projectedColumn.TypeMapping);

            return source.Update(
                selectExpression,
                new ProjectionBindingExpression(selectExpression, new ProjectionMember(), projectedColumn.Type));
        }

        return base.TranslateSkip(source, count);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateTake(ShapedQueryExpression source, Expression count)
    {
        // Translate Take over array to the PostgreSQL slice operator (array.Take(2) -> array[,2])
        if (source.QueryExpression is SelectExpression
            {
                Tables: [PostgresUnnestExpression { Array: var array } unnestExpression],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Orderings: [],
                Limit: null,
                Offset: null
            }
            && TryGetProjectedColumn(source, out var projectedColumn))
        {
            var translatedCount = TranslateExpression(count);
            if (translatedCount == null)
            {
                return base.TranslateTake(source, count);
            }

            PostgresArraySliceExpression sliceExpression;

            // If Skip has been called before, an array slice expression is already there; try to integrate this Take into it.
            // Note that we need to take the Skip (lower bound) into account for the Take (upper bound), since the slice upper bound
            // operates on the original array (Skip hasn't yet taken place).
            if (array is PostgresArraySliceExpression existingSliceExpression)
            {
                if (existingSliceExpression is
                    {
                        LowerBound: SqlConstantExpression { Value: int lowerBoundValue } lowerBound,
                        UpperBound: null
                    })
                {
                    sliceExpression = existingSliceExpression.Update(
                        existingSliceExpression.Array,
                        existingSliceExpression.LowerBound,
                        translatedCount is SqlConstantExpression { Value: int takeCount }
                            ? _sqlExpressionFactory.Constant(lowerBoundValue + takeCount - 1, lowerBound.TypeMapping)
                            : _sqlExpressionFactory.Subtract(
                                _sqlExpressionFactory.Add(lowerBound, translatedCount),
                                _sqlExpressionFactory.Constant(1, lowerBound.TypeMapping)));
                }
                else
                {
                    // For any other case, we allow relational to translate with normal querying. For non-constant lower bounds, we could
                    // duplicate them into the upper bound, but that could cause expensive double evaluation.
                    return base.TranslateTake(source, count);
                }
            }
            else
            {
                sliceExpression = new PostgresArraySliceExpression(array, lowerBound: null, upperBound: translatedCount);
            }

            var selectExpression = new SelectExpression(
                new PostgresUnnestExpression(unnestExpression.Alias, sliceExpression, "value"),
                "value",
                projectedColumn.Type,
                projectedColumn.TypeMapping);

            return source.Update(
                selectExpression,
                new ProjectionBindingExpression(selectExpression, new ProjectionMember(), projectedColumn.Type));
        }

        return base.TranslateTake(source, count);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsValidSelectExpressionForExecuteUpdate(
        SelectExpression selectExpression,
        EntityShaperExpression entityShaperExpression,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        if (!base.IsValidSelectExpressionForExecuteUpdate(selectExpression, entityShaperExpression, out tableExpression))
        {
            return false;
        }

        // PostgreSQL doesn't support referencing the main update table from anywhere except for the UPDATE WHERE clause.
        // This specifically makes it impossible to have joins which reference the main table in their predicate (ON ...).
        // Because of this, we detect all such inner joins and lift their predicates to the main WHERE clause (where a reference to the
        // main table is allowed) - see NpgsqlQuerySqlGenerator.VisitUpdate.
        // For any other type of join which contains a reference to the main table, we return false to trigger a subquery pushdown instead.
        OuterReferenceFindingExpressionVisitor? visitor = null;

        for (var i = 0; i < selectExpression.Tables.Count; i++)
        {
            var table = selectExpression.Tables[i];

            if (ReferenceEquals(table, tableExpression))
            {
                continue;
            }

            visitor ??= new OuterReferenceFindingExpressionVisitor(tableExpression);

            // For inner joins, if the predicate contains a reference to the main table, NpgsqlQuerySqlGenerator will lift the predicate
            // to the WHERE clause; so we only need to check the inner join's table (i.e. subquery) for such a reference.
            // Cross join and cross/outer apply (lateral joins) don't have predicates, so just check the entire join for a reference to
            // the main table, and switch to subquery syntax if one is found.
            // Left join does have a predicate, but it isn't possible to lift it to the main WHERE clause; so also check the entire
            // join.
            if (table is InnerJoinExpression innerJoin)
            {
                table = innerJoin.Table;
            }

            if (visitor.ContainsReferenceToMainTable(table))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsValidSelectExpressionForExecuteDelete(
        SelectExpression selectExpression,
        EntityShaperExpression entityShaperExpression,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        // The default relational behavior is to allow only single-table expressions, and the only permitted feature is a predicate.
        // Here we extend this to also inner joins to tables, which we generate via the PostgreSQL-specific USING construct.
        if (selectExpression.Offset == null
            && selectExpression.Limit == null
            // If entity type has primary key then Distinct is no-op
            && (!selectExpression.IsDistinct || entityShaperExpression.EntityType.FindPrimaryKey() != null)
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Having == null
            && selectExpression.Orderings.Count == 0)
        {
            TableExpressionBase? table = null;
            if (selectExpression.Tables.Count == 1)
            {
                table = selectExpression.Tables[0];
            }
            else if (selectExpression.Tables.All(t => t is TableExpression or InnerJoinExpression))
            {
                var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;
                var entityProjectionExpression = (EntityProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
                var column = entityProjectionExpression.BindProperty(entityShaperExpression.EntityType.GetProperties().First());
                table = column.Table;
                if (table is JoinExpressionBase joinExpressionBase)
                {
                    table = joinExpressionBase.Table;
                }
            }

            if (table is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        tableExpression = null;
        return false;
    }

    // PostgreSQL unnest is guaranteed to return output rows in the same order as its input array,
    // https://www.postgresql.org/docs/current/functions-array.html.
    /// <inheritdoc />
    protected override bool IsOrdered(SelectExpression selectExpression)
        => base.IsOrdered(selectExpression) || selectExpression.Tables is [PostgresUnnestExpression];

    private bool TryGetProjectedColumn(
        ShapedQueryExpression shapedQueryExpression, [NotNullWhen(true)] out ColumnExpression? projectedColumn)
    {
        var shaperExpression = shapedQueryExpression.ShaperExpression;
        if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
            && unaryExpression.Operand.Type.IsNullableType()
            && unaryExpression.Operand.Type.UnwrapNullableType() == unaryExpression.Type)
        {
            shaperExpression = unaryExpression.Operand;
        }

        if (shaperExpression is ProjectionBindingExpression projectionBindingExpression
            && shapedQueryExpression.QueryExpression is SelectExpression selectExpression
            && selectExpression.GetProjection(projectionBindingExpression) is ColumnExpression c)
        {
            projectedColumn = c;
            return true;
        }

        projectedColumn = null;
        return false;
    }

    /// <summary>
    ///     PostgreSQL array indexing is 1-based. If the index happens to be a constant, just increment it. Otherwise, append a +1 in the
    ///     SQL.
    /// </summary>
    private SqlExpression GenerateOneBasedIndexExpression(SqlExpression expression)
        => expression is SqlConstantExpression constant
            ? _sqlExpressionFactory.Constant(Convert.ToInt32(constant.Value) + 1, constant.TypeMapping)
            : _sqlExpressionFactory.Add(expression, _sqlExpressionFactory.Constant(1));

    private ShapedQueryExpression BuildSimplifiedShapedQuery(ShapedQueryExpression source, SqlExpression translation)
        => source.Update(
            _sqlExpressionFactory.Select(translation),
            Expression.Convert(
                new ProjectionBindingExpression(translation, new ProjectionMember(), typeof(bool?)), typeof(bool)));

    /// <summary>
    ///     Extracts the <see cref="PostgresUnnestExpression.Array" /> out of <see cref="PostgresUnnestExpression" />.
    ///     If a <see cref="ValuesExpression" /> is given, converts its literal values into a <see cref="PostgresNewArrayExpression" />.
    /// </summary>
    private SqlExpression GetArray(TableExpressionBase tableExpression)
    {
        Check.DebugAssert(
            tableExpression is PostgresUnnestExpression or ValuesExpression { ColumnNames: ["_ord", "Value"] },
            "Bad tableExpression");

        switch (tableExpression)
        {
            case PostgresUnnestExpression unnest:
                return unnest.Array;

            case ValuesExpression valuesExpression:
            {
                // The source table was a constant collection, so translated by default to ValuesExpression. Convert it to an unnest over
                // an array constructor.
                var elements = new SqlExpression[valuesExpression.RowValues.Count];

                for (var i = 0; i < elements.Length; i++)
                {
                    // Skip the first column (_ord) and copy the second (Value)
                    elements[i] = valuesExpression.RowValues[i].Values[1];
                }

                return new PostgresNewArrayExpression(
                    elements, valuesExpression.RowValues[0].Values[1].Type.MakeArrayType(), typeMapping: null);
            }

            default:
                throw new ArgumentException(nameof(tableExpression));
        }
    }

    private sealed class OuterReferenceFindingExpressionVisitor : ExpressionVisitor
    {
        private readonly TableExpression _mainTable;
        private bool _containsReference;

        public OuterReferenceFindingExpressionVisitor(TableExpression mainTable)
            => _mainTable = mainTable;

        public bool ContainsReferenceToMainTable(TableExpressionBase tableExpression)
        {
            _containsReference = false;

            Visit(tableExpression);

            return _containsReference;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (_containsReference)
            {
                return expression;
            }

            if (expression is ColumnExpression columnExpression
                && columnExpression.Table == _mainTable)
            {
                _containsReference = true;

                return expression;
            }

            return base.Visit(expression);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected class NpgsqlInferredTypeMappingApplier : RelationalInferredTypeMappingApplier
    {
        private readonly NpgsqlTypeMappingSource _typeMappingSource;
        private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NpgsqlInferredTypeMappingApplier(
            NpgsqlTypeMappingSource typeMappingSource,
            NpgsqlSqlExpressionFactory sqlExpressionFactory,
            IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping?> inferredTypeMappings)
            : base(sqlExpressionFactory, inferredTypeMappings)
        {
            _typeMappingSource = typeMappingSource;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression expression)
        {
            switch (expression)
            {
                case PostgresUnnestExpression unnestExpression when TryGetInferredTypeMapping(unnestExpression, unnestExpression.ColumnName, out var elementTypeMapping):
                {
                    var collectionTypeMapping = _typeMappingSource.FindContainerMapping(unnestExpression.Array.Type, elementTypeMapping);

                    if (collectionTypeMapping is null)
                    {
                        throw new InvalidOperationException(RelationalStrings.NullTypeMappingInSqlTree(expression.Print()));
                    }

                    return unnestExpression.Update(
                        _sqlExpressionFactory.ApplyTypeMapping(unnestExpression.Array, collectionTypeMapping));
                }

                default:
                    return base.VisitExtension(expression);
            }
        }
    }
}
