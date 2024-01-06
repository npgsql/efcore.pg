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
    protected override ShapedQueryExpression TranslatePrimitiveCollection(
        SqlExpression sqlExpression,
        IProperty? property,
        string tableAlias)
    {
        var elementClrType = sqlExpression.Type.GetSequenceType();
        var elementTypeMapping = (RelationalTypeMapping?)sqlExpression.TypeMapping?.ElementTypeMapping;

        // If this is a collection property, get the element's nullability out of metadata. Otherwise, this is a parameter property, in
        // which case we only have the CLR type (note that we cannot produce different SQLs based on the nullability of an *element* in
        // a parameter collection - our caching mechanism only supports varying by the nullability of the parameter itself (i.e. the
        // collection).
        // TODO: if property is non-null, GetElementType() should never be null, but we have #31469 for shadow properties
        var isElementNullable = property?.GetElementType() is null
            ? elementClrType.IsNullableType()
            : property.GetElementType()!.IsNullable;

        // We support two kinds of primitive collections: the standard one with PostgreSQL arrays (where we use the unnest function), and
        // a special case for geometry collections, where we use
        SelectExpression selectExpression;

#pragma warning disable EF1001
        // TODO: Parameters have no type mapping. We can check whether the expression type is one of the NTS geometry collection types,
        // though in a perfect world we'd actually infer this. In other words, when the type mapping of the element is inferred further on,
        // we'd replace the unnest expression with ST_Dump. We could even have a special expression type which means "indeterminate, must be
        // inferred".
        if (sqlExpression.TypeMapping is { StoreTypeNameBase: "geometry" or "geography" })
        {
            // TODO: For geometry collection support (not yet supported), see #2850.
            selectExpression = new SelectExpression(
                new TableValuedFunctionExpression(tableAlias, "ST_Dump", new[] { sqlExpression }),
                "geom", elementClrType, elementTypeMapping, isElementNullable);
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
                new PgUnnestExpression(tableAlias, sqlExpression, "value"),
                columnName: "value",
                columnType: elementClrType,
                columnTypeMapping: elementTypeMapping,
                isColumnNullable: isElementNullable,
                identifierColumnName: "ordinality",
                identifierColumnType: typeof(int),
                identifierColumnTypeMapping: _typeMappingSource.FindMapping(typeof(int)));
        }
#pragma warning restore EF1001

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
    protected override ShapedQueryExpression TransformJsonQueryToTable(JsonQueryExpression jsonQueryExpression)
    {
        // Calculate the table alias for the jsonb_to_recordset function based on the last named path segment
        // (or the JSON column name if there are none)
        var lastNamedPathSegment = jsonQueryExpression.Path.LastOrDefault(ps => ps.PropertyName is not null);
        var tableAlias = char.ToLowerInvariant((lastNamedPathSegment.PropertyName ?? jsonQueryExpression.JsonColumn.Name)[0]).ToString();

        // TODO: This relies on nested JSON columns flowing across the type mapping of the top-most containing JSON column, check this.
        var functionName = jsonQueryExpression.JsonColumn switch
        {
            { TypeMapping.StoreType: "jsonb" } => "jsonb_to_recordset",
            { TypeMapping.StoreType: "json" } => "json_to_recordset",
            { TypeMapping: null } => throw new UnreachableException("Missing type mapping on JSON column"),

            _ => throw new UnreachableException()
        };

        var jsonTypeMapping = jsonQueryExpression.JsonColumn.TypeMapping!;
        Check.DebugAssert(jsonTypeMapping is NpgsqlOwnedJsonTypeMapping, "JSON column has a non-JSON mapping");

        // We now add all of projected entity's the properties and navigations into the jsonb_to_recordset's AS clause, which defines the
        // names and types of columns to come out of the JSON fragments.
        var columnInfos = new List<PgTableValuedFunctionExpression.ColumnInfo>();

        // We're only interested in properties which actually exist in the JSON, filter out uninteresting shadow keys
        foreach (var property in GetAllPropertiesInHierarchy(jsonQueryExpression.EntityType))
        {
            if (property.GetJsonPropertyName() is string jsonPropertyName)
            {
                columnInfos.Add(
                    new PgTableValuedFunctionExpression.ColumnInfo
                    {
                        Name = jsonPropertyName, TypeMapping = property.GetRelationalTypeMapping()
                    });
            }
        }

        // Navigations represent nested JSON owned entities, which we also add to the AS clause, but with the JSON type.
        foreach (var navigation in GetAllNavigationsInHierarchy(jsonQueryExpression.EntityType)
                     .Where(
                         n => n.ForeignKey.IsOwnership
                             && n.TargetEntityType.IsMappedToJson()
                             && n.ForeignKey.PrincipalToDependent == n))
        {
            var jsonNavigationName = navigation.TargetEntityType.GetJsonPropertyName();
            Check.DebugAssert(jsonNavigationName is not null, $"No JSON property name for navigation {navigation.Name}");

            columnInfos.Add(
                new PgTableValuedFunctionExpression.ColumnInfo { Name = jsonNavigationName, TypeMapping = jsonTypeMapping });
        }

        // json_to_recordset requires the nested JSON document - it does not accept a path within a containing JSON document (like SQL
        // Server OPENJSON or SQLite json_each). So we wrap json_to_recordset around a JsonScalarExpression which will extract the nested
        // document.
        var jsonScalarExpression = new JsonScalarExpression(
            jsonQueryExpression.JsonColumn, jsonQueryExpression.Path, typeof(string), jsonTypeMapping, jsonQueryExpression.IsNullable);

        // Construct the json_to_recordset around the JsonScalarExpression, and wrap it in a SelectExpression
        var jsonToRecordSetExpression = new PgTableValuedFunctionExpression(
            tableAlias, functionName, new[] { jsonScalarExpression }, columnInfos, withOrdinality: true);

#pragma warning disable EF1001 // Internal EF Core API usage.
        var selectExpression = new SelectExpression(
            jsonQueryExpression,
            jsonToRecordSetExpression,
            identifierColumnName: "ordinality",
            identifierColumnType: typeof(int),
            identifierColumnTypeMapping: _typeMappingSource.FindMapping(typeof(int))!);
#pragma warning restore EF1001 // Internal EF Core API usage.

        return new ShapedQueryExpression(
            selectExpression,
            new RelationalStructuralTypeShaperExpression(
                jsonQueryExpression.EntityType,
                new ProjectionBindingExpression(
                    selectExpression,
                    new ProjectionMember(),
                    typeof(ValueBuffer)),
                false));

        // TODO: Move these to IEntityType?
        static IEnumerable<IProperty> GetAllPropertiesInHierarchy(IEntityType entityType)
            => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                .SelectMany(t => t.GetDeclaredProperties());

        static IEnumerable<INavigation> GetAllNavigationsInHierarchy(IEntityType entityType)
            => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                .SelectMany(t => t.GetDeclaredNavigations());
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
        => new NpgsqlInferredTypeMappingApplier(
            RelationalDependencies.Model, _typeMappingSource, _sqlExpressionFactory, inferredTypeMappings).Visit(expression);

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
                Tables: [(PgUnnestExpression or ValuesExpression { ColumnNames: ["_ord", "Value"] }) and var sourceTable],
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
                        _sqlExpressionFactory.All(match, GetArray(sourceTable), PgAllOperatorType.Like));
                }

                // Pattern match for: new[] { "a", "b", "c" }.All(p => EF.Functions.Like(e.SomeText, p)),
                // which we translate to WHERE s.""SomeText"" LIKE ALL (ARRAY['a','b','c'])
                case PgILikeExpression
                    {
                        Match: var match,
                        Pattern: ColumnExpression pattern,
                        EscapeChar: SqlConstantExpression { Value: "" }
                    }
                    when pattern.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source,
                        _sqlExpressionFactory.All(match, GetArray(sourceTable), PgAllOperatorType.ILike));
                }

                // Pattern match for: e.SomeArray.All(p => ints.Contains(p)) over non-column,
                // using array containment (<@)
                case PgAnyExpression
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
                case PgBinaryExpression
                    {
                        OperatorType: PgExpressionType.Contains,
                        Left: var otherArray,
                        Right: PgNewArrayExpression { Expressions: [ColumnExpression sourceColumn] }
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
                Tables: [(PgUnnestExpression or ValuesExpression { ColumnNames: ["_ord", "Value"] }) and var sourceTable],
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

            if (TranslateLambdaExpression(source, predicate) is not SqlExpression translatedPredicate)
            {
                return null;
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
                        source, _sqlExpressionFactory.Any(match, GetArray(sourceTable), PgAnyOperatorType.Like));
                }

                // Pattern match: new[] { "a", "b", "c" }.Any(p => EF.Functions.Like(e.SomeText, p))
                // Translation: s.SomeText LIKE ANY (ARRAY['a','b','c'])
                case PgILikeExpression
                    {
                        Match: var match,
                        Pattern: ColumnExpression pattern,
                        EscapeChar: SqlConstantExpression { Value: "" }
                    }
                    when pattern.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source, _sqlExpressionFactory.Any(match, GetArray(sourceTable), PgAnyOperatorType.ILike));
                }

                // Array overlap over non-column
                // Pattern match: e.SomeArray.Any(p => ints.Contains(p))
                // Translation: @ints && s.SomeArray
                case PgAnyExpression
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
                case PgBinaryExpression
                    {
                        OperatorType: PgExpressionType.Contains,
                        Left: var otherArray,
                        Right: PgNewArrayExpression { Expressions: [ColumnExpression sourceColumn] }
                    }
                    when sourceColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(source, _sqlExpressionFactory.Overlaps(GetArray(sourceTable), otherArray));
                }

                #region LTree translations

                // Pattern match: new[] { "q1", "q2" }.Any(q => e.SomeLTree.MatchesLQuery(q))
                // Translation: s.SomeLTree ? ARRAY['q1','q2']
                case PgBinaryExpression
                    {
                        OperatorType: PgExpressionType.LTreeMatches,
                        Left: var ltree,
                        Right: SqlUnaryExpression { OperatorType: ExpressionType.Convert, Operand: ColumnExpression lqueryColumn }
                    }
                    when lqueryColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source,
                        new PgBinaryExpression(
                            PgExpressionType.LTreeMatchesAny,
                            ltree,
                            _sqlExpressionFactory.ApplyTypeMapping(GetArray(sourceTable), _typeMappingSource.FindMapping("lquery[]")),
                            typeof(bool),
                            typeMapping: _typeMappingSource.FindMapping(typeof(bool))));
                }

                // Pattern match: new[] { "t1", "t2" }.Any(t => t.IsAncestorOf(e.SomeLTree))
                // Translation: ARRAY['t1','t2'] @> s.SomeLTree
                // Pattern match: new[] { "t1", "t2" }.Any(t => t.IsDescendantOf(e.SomeLTree))
                // Translation: ARRAY['t1','t2'] <@ s.SomeLTree
                case PgBinaryExpression
                    {
                        OperatorType: (PgExpressionType.Contains or PgExpressionType.ContainedBy) and var operatorType,
                        Left: ColumnExpression ltreeColumn,
                        // Contains/ContainedBy can happen for non-LTree types too, so check that
                        Right: { TypeMapping: NpgsqlLTreeTypeMapping } ltree
                    }
                    when ltreeColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source,
                        new PgBinaryExpression(
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
                case PgBinaryExpression
                    {
                        OperatorType: PgExpressionType.LTreeMatches,
                        Left: ColumnExpression ltreeColumn,
                        Right: var lquery
                    }
                    when ltreeColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source,
                        new PgBinaryExpression(
                            PgExpressionType.LTreeMatches,
                            _sqlExpressionFactory.ApplyDefaultTypeMapping(GetArray(sourceTable)),
                            lquery,
                            typeof(bool),
                            typeMapping: _typeMappingSource.FindMapping(typeof(bool))));
                }

                // Any within Any (i.e. intersection)
                // Pattern match: ltrees.Any(t => lqueries.Any(q => t.MatchesLQuery(q)))
                // Translate: ltrees ? lqueries
                case PgBinaryExpression
                    {
                        OperatorType: PgExpressionType.LTreeMatchesAny,
                        Left: ColumnExpression ltreeColumn,
                        Right: var lqueries
                    }
                    when ltreeColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source,
                        new PgBinaryExpression(
                            PgExpressionType.LTreeMatchesAny,
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
                Tables:
                [
                    IntersectExpression
                    {
                        Source1:
                        {
                            Tables: [PgUnnestExpression { Array: var array1 }],
                            GroupBy: [],
                            Having: null,
                            IsDistinct: false,
                            Limit: null,
                            Offset: null
                        },
                        Source2:
                        {
                            Tables: [PgUnnestExpression { Array: var array2 }],
                            GroupBy: [],
                            Having: null,
                            IsDistinct: false,
                            Limit: null,
                            Offset: null
                        }
                    }
                ],
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
    protected override ShapedQueryExpression? TranslateContains(ShapedQueryExpression source, Expression item)
    {
        if (source.QueryExpression is SelectExpression
            {
                Tables: [(PgUnnestExpression or ValuesExpression { ColumnNames: ["_ord", "Value"] }) and var sourceTable],
                Predicate: null,
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null
            })
        {
            if (TranslateExpression(item, applyDefaultTypeMapping: false) is not SqlExpression translatedItem)
            {
                return null;
            }

            // Note that most other simplifications here convert ValuesExpression to unnest over array constructor, but we avoid doing that
            // here, since the relational translation for ValuesExpression is better.
            var array = GetArray(sourceTable);

            (translatedItem, array) = _sqlExpressionFactory.ApplyTypeMappingsOnItemAndArray(translatedItem, array);

            // When the array is a column, we translate Contains to array @> ARRAY[item]. GIN indexes on array are used, but null
            // semantics is impossible without preventing index use.
            switch (array)
            {
                case ColumnExpression:
                    if (translatedItem is SqlConstantExpression { Value: null })
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
                                    new[] { array, translatedItem },
                                    nullable: true,
                                    argumentsPropagateNullability: FalseArrays[2],
                                    typeof(int))));
                    }

                    return BuildSimplifiedShapedQuery(
                        source,
                        _sqlExpressionFactory.Contains(
                            array,
                            _sqlExpressionFactory.NewArrayOrConstant(new[] { translatedItem }, array.Type, array.TypeMapping)));

                // For constant arrays (new[] { 1, 2, 3 }) or inline arrays (new[] { 1, param, 3 }), don't do anything PG-specific for since
                // the general EF Core mechanism is fine for that case: item IN (1, 2, 3).
                case SqlConstantExpression or PgNewArrayExpression:
                    break;

                // Similar to ParameterExpression below, but when a bare subquery is present inside ANY(), PostgreSQL just compares
                // against each of its resulting rows (just like IN). To "extract" the array result of the scalar subquery, we need
                // to add an explicit cast (see #1803).
                case ScalarSubqueryExpression subqueryExpression:
                    return BuildSimplifiedShapedQuery(
                        source,
                        _sqlExpressionFactory.Any(
                            translatedItem,
                            _sqlExpressionFactory.Convert(
                                subqueryExpression, subqueryExpression.Type, subqueryExpression.TypeMapping),
                            PgAnyOperatorType.Equal));

                // For ParameterExpression, and for all other cases - e.g. array returned from some function -
                // translate to e.SomeText = ANY (@p). This is superior to the general solution which will expand
                // parameters to constants, since non-PG SQL does not support arrays.
                // Note that this will allow indexes on the item to be used.
                default:
                    return BuildSimplifiedShapedQuery(
                        source, _sqlExpressionFactory.Any(translatedItem, array, PgAnyOperatorType.Equal));
            }
        }

        return base.TranslateContains(source, item);
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
        if (predicate is null
            && source.QueryExpression is SelectExpression
            {
                Tables: [PgUnnestExpression { Array: var array }],
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
    protected override ShapedQueryExpression TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2)
    {
        // Simplify x.Array.Concat(y.Array) => x.Array || y.Array instead of:
        // SELECT u.value FROM unnest(x.Array) UNION ALL SELECT u.value FROM unnest(y.Array)
        if (source1.QueryExpression is SelectExpression
            {
                Tables: [PgUnnestExpression { Array: var array1 } unnestExpression1],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null,
                Orderings: []
            }
            && source2.QueryExpression is SelectExpression
            {
                Tables: [PgUnnestExpression { Array: var array2 }],
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
            Check.DebugAssert(
                projectedColumn1.TypeMapping is not null || projectedColumn2.TypeMapping is not null,
                "Concat with no type mapping on either side (operation should be client-evaluated over parameters/constants");

            // TODO: Conflicting type mappings from both sides?
            var inferredTypeMapping = projectedColumn1.TypeMapping ?? projectedColumn2.TypeMapping;

#pragma warning disable EF1001 // Internal EF Core API usage.
            var selectExpression = new SelectExpression(
                new PgUnnestExpression(unnestExpression1.Alias, _sqlExpressionFactory.Add(array1, array2), "value"),
                columnName: "value",
                columnType: projectedColumn1.Type,
                columnTypeMapping: inferredTypeMapping,
                isColumnNullable: projectedColumn1.IsNullable || projectedColumn2.IsNullable,
                identifierColumnName: "ordinality",
                identifierColumnType: typeof(int),
                identifierColumnTypeMapping: _typeMappingSource.FindMapping(typeof(int)));
#pragma warning restore EF1001 // Internal EF Core API usage.

            // TODO: Simplify by using UpdateQueryExpression after https://github.com/dotnet/efcore/issues/31511
            Expression shaperExpression = new ProjectionBindingExpression(
                selectExpression, new ProjectionMember(), source1.ShaperExpression.Type.MakeNullable());

            if (source1.ShaperExpression.Type != shaperExpression.Type)
            {
                Check.DebugAssert(
                    source1.ShaperExpression.Type.MakeNullable() == shaperExpression.Type,
                    "expression.Type must be nullable of targetType");

                shaperExpression = Expression.Convert(shaperExpression, source1.ShaperExpression.Type);
            }

            return new ShapedQueryExpression(selectExpression, shaperExpression);
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
        // Note that we have unnest over multiranges, not just arrays - but multiranges don't support subscripting/slicing.
        if (!returnDefault
            && source.QueryExpression is SelectExpression
            {
                Tables: [PgUnnestExpression { Array: var array }],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Orderings: [],
                Limit: null,
                Offset: null
            }
            && IsPostgresArray(array)
            && TryGetProjectedColumn(source, out var projectedColumn)
            && TranslateExpression(index) is { } translatedIndex)
        {
            // Note that PostgreSQL arrays are 1-based, so adjust the index.
            return source.UpdateQueryExpression(
                _sqlExpressionFactory.Select(
                    _sqlExpressionFactory.ArrayIndex(
                        array,
                        GenerateOneBasedIndexExpression(translatedIndex), projectedColumn.IsNullable)));
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
                Tables: [(PgUnnestExpression or ValuesExpression { ColumnNames: ["_ord", "Value"] }) and var sourceTable],
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
                case PgBinaryExpression
                    {
                        OperatorType: (PgExpressionType.Contains or PgExpressionType.ContainedBy) and var operatorType,
                        Left: ColumnExpression ltreeColumn,
                        // Contains/ContainedBy can happen for non-LTree types too, so check that
                        Right: { TypeMapping: NpgsqlLTreeTypeMapping } ltree
                    }
                    when ltreeColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source,
                        new PgBinaryExpression(
                            operatorType == PgExpressionType.Contains
                                ? PgExpressionType.LTreeFirstAncestor
                                : PgExpressionType.LTreeFirstDescendent,
                            _sqlExpressionFactory.ApplyDefaultTypeMapping(GetArray(sourceTable)),
                            ltree,
                            typeof(LTree),
                            _typeMappingSource.FindMapping(typeof(LTree))));
                }

                // Pattern match: new[] { "t1", "t2" }.FirstOrDefault(t => t.MatchesLQuery(lquery))
                // Translation: ARRAY['t1','t2'] ?~ e.lquery
                // Pattern match: new[] { "t1", "t2" }.FirstOrDefault(t => t.MatchesLQuery(ltxtquery))
                // Translation: ARRAY['t1','t2'] ?@ e.ltxtquery
                case PgBinaryExpression
                    {
                        OperatorType: PgExpressionType.LTreeMatches,
                        Left: ColumnExpression ltreeColumn,
                        Right: var lquery
                    }
                    when ltreeColumn.Table == sourceTable:
                {
                    return BuildSimplifiedShapedQuery(
                        source,
                        new PgBinaryExpression(
                            PgExpressionType.LTreeFirstMatches,
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
        // Note that we have unnest over multiranges, not just arrays - but multiranges don't support subscripting/slicing.
        if (source.QueryExpression is SelectExpression
            {
                Tables: [PgUnnestExpression { Array: var array } unnestExpression],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Orderings: [],
                Limit: null,
                Offset: null
            }
            && IsPostgresArray(array)
            && TryGetProjectedColumn(source, out var projectedColumn)
            && TranslateExpression(count) is { } translatedCount)
        {
#pragma warning disable EF1001 // Internal EF Core API usage.
            var selectExpression = new SelectExpression(
                new PgUnnestExpression(
                    unnestExpression.Alias,
                    _sqlExpressionFactory.ArraySlice(
                        array,
                        lowerBound: GenerateOneBasedIndexExpression(translatedCount),
                        upperBound: null,
                        projectedColumn.IsNullable),
                    "value"),
                "value",
                projectedColumn.Type,
                projectedColumn.TypeMapping,
                isColumnNullable: projectedColumn.IsNullable,
                // isColumnNullable: /*projectedColumn.IsNullable*/ true, // TODO: This fails because of a shaper check
                identifierColumnName: "ordinality",
                identifierColumnType: typeof(int),
                identifierColumnTypeMapping: _typeMappingSource.FindMapping(typeof(int)));
#pragma warning restore EF1001 // Internal EF Core API usage.

            // TODO: Simplify by using UpdateQueryExpression after https://github.com/dotnet/efcore/issues/31511
            Expression shaperExpression = new ProjectionBindingExpression(
                selectExpression, new ProjectionMember(), source.ShaperExpression.Type.MakeNullable());

            if (source.ShaperExpression.Type != shaperExpression.Type)
            {
                Check.DebugAssert(
                    source.ShaperExpression.Type.MakeNullable() == shaperExpression.Type,
                    "expression.Type must be nullable of targetType");

                shaperExpression = Expression.Convert(shaperExpression, source.ShaperExpression.Type);
            }

            return new ShapedQueryExpression(selectExpression, shaperExpression);
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
        // Note that we have unnest over multiranges, not just arrays - but multiranges don't support subscripting/slicing.
        if (source.QueryExpression is SelectExpression
            {
                Tables: [PgUnnestExpression { Array: var array } unnestExpression],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Orderings: [],
                Limit: null,
                Offset: null
            }
            && IsPostgresArray(array)
            && TryGetProjectedColumn(source, out var projectedColumn))
        {
            var translatedCount = TranslateExpression(count);
            if (translatedCount == null)
            {
                return base.TranslateTake(source, count);
            }

            PgArraySliceExpression sliceExpression;

            // If Skip has been called before, an array slice expression is already there; try to integrate this Take into it.
            // Note that we need to take the Skip (lower bound) into account for the Take (upper bound), since the slice upper bound
            // operates on the original array (Skip hasn't yet taken place).
            if (array is PgArraySliceExpression existingSliceExpression)
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
                sliceExpression = _sqlExpressionFactory.ArraySlice(
                    array,
                    lowerBound: null,
                    upperBound: translatedCount,
                    projectedColumn.IsNullable);
            }

#pragma warning disable EF1001 // Internal EF Core API usage.
            var selectExpression = new SelectExpression(
                new PgUnnestExpression(unnestExpression.Alias, sliceExpression, "value"),
                "value",
                projectedColumn.Type,
                projectedColumn.TypeMapping,
                isColumnNullable: projectedColumn.IsNullable,
                identifierColumnName: "ordinality",
                identifierColumnType: typeof(int),
                identifierColumnTypeMapping: _typeMappingSource.FindMapping(typeof(int)));
#pragma warning restore EF1001 // Internal EF Core API usage.

            // TODO: Simplify by using UpdateQueryExpression after https://github.com/dotnet/efcore/issues/31511
            Expression shaperExpression = new ProjectionBindingExpression(
                selectExpression, new ProjectionMember(), source.ShaperExpression.Type.MakeNullable());

            if (source.ShaperExpression.Type != shaperExpression.Type)
            {
                Check.DebugAssert(
                    source.ShaperExpression.Type.MakeNullable() == shaperExpression.Type,
                    "expression.Type must be nullable of targetType");

                shaperExpression = Expression.Convert(shaperExpression, source.ShaperExpression.Type);
            }

            return new ShapedQueryExpression(selectExpression, shaperExpression);
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
        TableExpressionBase targetTable,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        if (!base.IsValidSelectExpressionForExecuteUpdate(selectExpression, targetTable, out tableExpression))
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
        StructuralTypeShaperExpression shaper,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        // The default relational behavior is to allow only single-table expressions, and the only permitted feature is a predicate.
        // Here we extend this to also inner joins to tables, which we generate via the PostgreSQL-specific USING construct.
        if (selectExpression is
            {
                Orderings: [],
                Offset: null,
                Limit: null,
                GroupBy: [],
                Having: null
            })
        {
            TableExpressionBase? table = null;
            if (selectExpression.Tables.Count == 1)
            {
                table = selectExpression.Tables[0];
            }
            else if (selectExpression.Tables.All(t => t is TableExpression or InnerJoinExpression))
            {
                var projectionBindingExpression = (ProjectionBindingExpression)shaper.ValueBufferExpression;
                var entityProjectionExpression =
                    (StructuralTypeProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
                var column = entityProjectionExpression.BindProperty(shaper.StructuralType.GetProperties().First());
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
        => base.IsOrdered(selectExpression)
            || selectExpression.Tables is
                [PgTableValuedFunctionExpression { Name: "unnest" or "jsonb_to_recordset" or "json_to_recordset" }];

    /// <summary>
    ///     Checks whether the given expression maps to a PostgreSQL array, as opposed to a multirange type.
    /// </summary>
    private static bool IsPostgresArray(SqlExpression expression)
        => expression switch
        {
            { TypeMapping: NpgsqlArrayTypeMapping } => true,
            { TypeMapping: NpgsqlMultirangeTypeMapping } => false,
            { Type: var type } when type.IsMultirange() => false,
            _ => true
        };

    private bool TryGetProjectedColumn(
        ShapedQueryExpression shapedQueryExpression,
        [NotNullWhen(true)] out ColumnExpression? projectedColumn)
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
    ///     Extracts the <see cref="PgUnnestExpression.Array" /> out of <see cref="PgUnnestExpression" />.
    ///     If a <see cref="ValuesExpression" /> is given, converts its literal values into a <see cref="PgNewArrayExpression" />.
    /// </summary>
    private SqlExpression GetArray(TableExpressionBase tableExpression)
    {
        Check.DebugAssert(
            tableExpression is PgUnnestExpression or ValuesExpression { ColumnNames: ["_ord", "Value"] },
            "Bad tableExpression");

        switch (tableExpression)
        {
            case PgUnnestExpression unnest:
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

                return new PgNewArrayExpression(
                    elements, valuesExpression.RowValues[0].Values[1].Type.MakeArrayType(), typeMapping: null);
            }

            default:
                throw new ArgumentException(nameof(tableExpression));
        }
    }

    private sealed class OuterReferenceFindingExpressionVisitor(TableExpression mainTable) : ExpressionVisitor
    {
        private bool _containsReference;

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
                && columnExpression.Table == mainTable)
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
            IModel model,
            NpgsqlTypeMappingSource typeMappingSource,
            NpgsqlSqlExpressionFactory sqlExpressionFactory,
            IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping?> inferredTypeMappings)
            : base(model, sqlExpressionFactory, inferredTypeMappings)
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
                case PgUnnestExpression unnestExpression
                    when TryGetInferredTypeMapping(unnestExpression, unnestExpression.ColumnName, out var elementTypeMapping):
                {
                    var collectionTypeMapping = _typeMappingSource.FindMapping(unnestExpression.Array.Type, Model, elementTypeMapping);

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
