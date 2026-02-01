using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlSqlTranslatingExpressionVisitor(
    RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
    QueryCompilationContext queryCompilationContext,
    QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
    : RelationalSqlTranslatingExpressionVisitor(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
{
    private readonly QueryCompilationContext _queryCompilationContext = queryCompilationContext;
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory = (NpgsqlSqlExpressionFactory)dependencies.SqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource = dependencies.TypeMappingSource;
    private readonly NpgsqlJsonPocoTranslator _jsonPocoTranslator
        = ((NpgsqlMemberTranslatorProvider)dependencies.MemberTranslatorProvider).JsonPocoTranslator;

    private readonly RelationalTypeMapping _timestampMapping = dependencies.TypeMappingSource.FindMapping("timestamp without time zone")!;
    private readonly RelationalTypeMapping _timestampTzMapping = dependencies.TypeMappingSource.FindMapping("timestamp with time zone")!;

    private static Type? _nodaTimePeriodType;

    private static readonly MethodInfo EscapeLikePatternParameterMethod =
        typeof(NpgsqlSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ConstructLikePatternParameter))!;

    // Note: This is the PostgreSQL default and does not need to be explicitly specified
    private const char LikeEscapeChar = '\\';

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
    {
        var test = Visit(conditionalExpression.Test);
        var ifTrue = Visit(conditionalExpression.IfTrue);
        var ifFalse = Visit(conditionalExpression.IfFalse);

        if (TranslationFailed(conditionalExpression.Test, test, out var sqlTest)
            || TranslationFailed(conditionalExpression.IfTrue, ifTrue, out var sqlIfTrue)
            || TranslationFailed(conditionalExpression.IfFalse, ifFalse, out var sqlIfFalse))
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        // Translate:
        // a == b ? null : a -> NULLIF(a, b)
        // a != b ? a : null -> NULLIF(a, b)
        if (sqlTest is SqlBinaryExpression binary && sqlIfTrue is not null && sqlIfFalse is not null)
        {
            switch (binary.OperatorType)
            {
                case ExpressionType.Equal
                    when ifTrue is SqlConstantExpression { Value: null } && TryTranslateToNullIf(sqlIfFalse, out var nullIfTranslation):
                case ExpressionType.NotEqual
                    when ifFalse is SqlConstantExpression { Value: null } && TryTranslateToNullIf(sqlIfTrue, out nullIfTranslation):
                    return nullIfTranslation;
            }
        }

        return _sqlExpressionFactory.Case([new CaseWhenClause(sqlTest!, sqlIfTrue!)], sqlIfFalse);

        bool TryTranslateToNullIf(SqlExpression conditionalResult, [NotNullWhen(true)] out Expression? nullIfTranslation)
        {
            var (left, right) = (binary.Left, binary.Right);

            if (left.Equals(conditionalResult))
            {
                nullIfTranslation = _sqlExpressionFactory.Function(
                    "NULLIF", [left, right], true, [false, false], left.Type, left.TypeMapping);
                return true;
            }

            if (right.Equals(conditionalResult))
            {
                nullIfTranslation = _sqlExpressionFactory.Function(
                    "NULLIF", [right, left], true, [false, false], right.Type, right.TypeMapping);
                return true;
            }

            nullIfTranslation = null;
            return false;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        switch (unaryExpression.NodeType)
        {
            // We have row value comparison methods such as EF.Functions.GreaterThan, which accept two ValueTuples/Tuples.
            // Since they accept ITuple parameters, the arguments have a Convert node casting up from the concrete argument to ITuple;
            // this node causes translation failure in RelationalSqlTranslatingExpressionVisitor, so unwrap here.
            case ExpressionType.Convert
                when unaryExpression.Type == typeof(ITuple) && unaryExpression.Operand.Type.IsAssignableTo(typeof(ITuple)):
                return Visit(unaryExpression.Operand);

            // We map both IPAddress and NpgsqlInet to PG inet, and translate many methods accepting NpgsqlInet, so ignore casts from
            // IPAddress to NpgsqlInet.
            // On the PostgreSQL side, cidr is also implicitly convertible to inet, and at the ADO.NET level NpgsqlCidr has a similar
            // implicit conversion operator to NpgsqlInet. So remove that cast as well.
            case ExpressionType.Convert
                when unaryExpression.Type == typeof(NpgsqlInet)
                && (unaryExpression.Operand.Type == typeof(IPAddress)
                    || unaryExpression.Operand.Type == typeof(IPNetwork)
#pragma warning disable CS0618 // NpgsqlCidr is obsolete, replaced by .NET IPNetwork
                    || unaryExpression.Operand.Type == typeof(NpgsqlCidr)):
#pragma warning restore CS0618
                return Visit(unaryExpression.Operand);
        }

        if (base.VisitUnary(unaryExpression) is var translation
            && translation != QueryCompilationContext.NotTranslatedExpression)
        {
            return translation;
        }

        switch (unaryExpression.NodeType)
        {
            case ExpressionType.ArrayLength:
                if (TranslationFailed(unaryExpression.Operand, Visit(unaryExpression.Operand), out var sqlOperand))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                // Translate Length on byte[], but only if the type mapping is for bytea.
                // For byte[] mapped to an actual PG array (smallint[]), that's a primitive collection, and ArrayLength gets transformed to
                // Count() which gets translated to cardinality() as usual in NpgsqlQueryableMethodTranslatingExpressionVisitor.
                if (sqlOperand!.Type == typeof(byte[]) && sqlOperand.TypeMapping is NpgsqlByteArrayTypeMapping or null)
                {
                    return _sqlExpressionFactory.Function(
                        "length",
                        [sqlOperand],
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[1],
                        typeof(int));
                }

                // Attempt to translate Length on a JSON POCO array
                if (_jsonPocoTranslator.TranslateArrayLength(sqlOperand) is SqlExpression translation2)
                {
                    return translation2;
                }

                // Note that Length over PG arrays (not within JSON) gets translated by QueryableMethodTranslatingEV, since arrays are
                // primitive collections
                break;
        }

        // return base.VisitUnary(unaryExpression);
        return QueryCompilationContext.NotTranslatedExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
    {
        if (base.VisitNewArray(newArrayExpression) is var translation
            && translation != QueryCompilationContext.NotTranslatedExpression)
        {
            return translation;
        }

        if (newArrayExpression.NodeType is ExpressionType.NewArrayInit)
        {
            var visitedExpressions = new SqlExpression[newArrayExpression.Expressions.Count];
            for (var i = 0; i < newArrayExpression.Expressions.Count; i++)
            {
                if (Visit(newArrayExpression.Expressions[i]) is SqlExpression visited)
                {
                    visitedExpressions[i] = visited;
                }
                else
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }
            }

            return _sqlExpressionFactory.NewArray(visitedExpressions, newArrayExpression.Type);
        }

        return QueryCompilationContext.NotTranslatedExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        Debug.Assert(
            binaryExpression.NodeType != ExpressionType.ArrayIndex,
            "During preprocessing, ArrayIndex and List[] get normalized to ElementAt; see NpgsqlArrayTranslator. Should never see ArrayIndex.");

        // We pattern match date subtraction before calling base.VisitBinary() as that would produce the default a - b translation, but that
        // yields the number of days as an integer; this is incompatible with the .NET side, where LocalDate - LocalDate = Period, and Period
        // is mapped to PG interval. So we override subtraction to add make_interval.
        if (binaryExpression.NodeType is ExpressionType.Subtract
            && binaryExpression.Left.Type.UnwrapNullableType().FullName == "NodaTime.LocalDate"
                && binaryExpression.Right.Type.UnwrapNullableType().FullName == "NodaTime.LocalDate")
        {
            if (TranslationFailed(binaryExpression.Left, Visit(TryRemoveImplicitConvert(binaryExpression.Left)), out var sqlLeft)
                || TranslationFailed(binaryExpression.Right, Visit(TryRemoveImplicitConvert(binaryExpression.Right)), out var sqlRight))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            var subtraction = _sqlExpressionFactory.MakeBinary(
                ExpressionType.Subtract, sqlLeft!, sqlRight!, _typeMappingSource.FindMapping(typeof(int)))!;

            return PgFunctionExpression.CreateWithNamedArguments(
                "make_interval",
                [subtraction],
                ["days"],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                builtIn: true,
                _nodaTimePeriodType ??= binaryExpression.Left.Type.Assembly.GetType("NodaTime.Period")!,
                typeMapping: null);

            // Note: many other date/time arithmetic operators are fully supported as-is by PostgreSQL - see NpgsqlSqlExpressionFactory
        }

        return base.VisitBinary(binaryExpression) switch
        {
            // Optimize (x - c) - (y - c) to x - y.
            // This is particularly useful for DateOnly.DayNumber - DateOnly.DayNumber, which is the way to express DateOnly subtraction
            // (the subtraction operator isn't defined over DateOnly in .NET). The translation of x.DayNumber is x - DATE '0001-01-01',
            // so the below is a useful simplification.
            // TODO: As this is a generic mathematical simplification, we should move it to a generic optimization phase in EF Core.
            SqlBinaryExpression
            {
                OperatorType: ExpressionType.Subtract,
                Left: SqlBinaryExpression { OperatorType: ExpressionType.Subtract, Left: var left1, Right: var right1 },
                Right: SqlBinaryExpression { OperatorType: ExpressionType.Subtract, Left: var left2, Right: var right2 }
            } originalBinary when right1.Equals(right2)
                => new SqlBinaryExpression(ExpressionType.Subtract, left1, left2, originalBinary.Type, originalBinary.TypeMapping),

            // A somewhat hacky workaround for #2942.
            // When an optional owned JSON entity is compared to null, we get WHERE (x -> y) IS NULL.
            // The -> operator (returning jsonb) is used rather than ->> (returning text), since an entity type is being extracted, and
            // further JSON operations may need to be composed. However, when the value extracted is a JSON null, a non-NULL jsonb value is
            // returned, and comparing that to relational NULL returns false.
            // Pattern-match this and force the use of ->> by changing the mapping to be a scalar rather than an entity type.
            SqlBinaryExpression
            {
                OperatorType: ExpressionType.Equal or ExpressionType.NotEqual,
                Left: JsonScalarExpression { TypeMapping: NpgsqlStructuralJsonTypeMapping } operand,
                Right: SqlConstantExpression { Value: null }
            } binary
                => binary.Update(
                    new JsonScalarExpression(
                        operand.Json, operand.Path, operand.Type, _typeMappingSource.FindMapping("text"), operand.IsNullable),
                    binary.Right),

            SqlBinaryExpression
            {
                OperatorType: ExpressionType.Equal or ExpressionType.NotEqual,
                Left: SqlConstantExpression { Value: null },
                Right: JsonScalarExpression { TypeMapping: NpgsqlStructuralJsonTypeMapping } operand
            } binary
                => binary.Update(
                    binary.Left,
                    new JsonScalarExpression(
                        operand.Json, operand.Path, operand.Type, _typeMappingSource.FindMapping("text"), operand.IsNullable)),

            // Unfortunately EF isn't consistent in its representation of X IS NULL in the SQL tree - sometimes it's a SqlUnaryExpression with Equals,
            // sometimes it's an X = NULL SqlBinaryExpression that later gets transformed to SqlUnaryExpression, in SqlNullabilityProcessor. We recognize
            // both of these here.
            SqlUnaryExpression
            {
                Operand: JsonScalarExpression { TypeMapping: NpgsqlStructuralJsonTypeMapping } operand
            } unary
                => unary.Update(
                     new JsonScalarExpression(
                         operand.Json, operand.Path, operand.Type, _typeMappingSource.FindMapping("text"), operand.IsNullable)),

            var translation => translation
        };
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        var method = methodCallExpression.Method;

        // Pattern-match: cube.LowerLeft[index] or cube.UpperRight[index]
        // This appears as: get_Item method call on a MemberExpression of LowerLeft/UpperRight.
        // We match this before calling base.VisitMethodCall, because we throw an informative InvalidOperationException when
        // we see LowerLeft/UpperRight, and only support get_Item over those.
        if (methodCallExpression is
            {
                Method.Name: "get_Item",
                Object: MemberExpression
                {
                    Member.Name: nameof(NpgsqlCube.LowerLeft) or nameof(NpgsqlCube.UpperRight)
                } memberExpression
            } && memberExpression.Member.DeclaringType == typeof(NpgsqlCube))
        {
            // Translate the cube instance and index argument
            if (Visit(memberExpression.Expression) is not SqlExpression sqlCubeInstance
                || Visit(methodCallExpression.Arguments[0]) is not SqlExpression sqlIndex)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            // Convert zero-based to one-based index
            // For constants, optimize at translation time; for parameters/columns, add at runtime
            var pgIndex = sqlIndex is SqlConstantExpression { Value: int index }
                ? _sqlExpressionFactory.Constant(index + 1)
                : _sqlExpressionFactory.Add(sqlIndex, _sqlExpressionFactory.Constant(1));

            // Determine which function to call
            var functionName = memberExpression.Member.Name == nameof(NpgsqlCube.LowerLeft)
                ? "cube_ll_coord"
                : "cube_ur_coord";

            return _sqlExpressionFactory.Function(
                functionName,
                [sqlCubeInstance, pgIndex],
                nullable: true,
                argumentsPropagateNullability: TrueArrays[2],
                typeof(double));
        }

        if (base.VisitMethodCall(methodCallExpression) is var translation
            && translation != QueryCompilationContext.NotTranslatedExpression)
        {
            return translation;
        }

        var declaringType = method.DeclaringType;
        var @object = methodCallExpression.Object;
        var arguments = methodCallExpression.Arguments;

        switch (method.Name)
        {
            // Pattern-match: cube.ToSubset(indexes)
            case nameof(NpgsqlCube.ToSubset)
                when declaringType == typeof(NpgsqlCube)
                    && @object is not null
                    && arguments is [Expression indexes]:
            {
                // Translate cube instance and indexes array
                return Visit(@object) is SqlExpression sqlCubeInstance && Visit(indexes) is SqlExpression sqlIndexes
                    ? TranslateCubeToSubset(sqlCubeInstance, sqlIndexes) ?? QueryCompilationContext.NotTranslatedExpression
                    : QueryCompilationContext.NotTranslatedExpression;
            }

            // https://learn.microsoft.com/dotnet/api/system.string.startswith#system-string-startswith(system-string)
            // https://learn.microsoft.com/dotnet/api/system.string.startswith#system-string-startswith(system-char)
            // https://learn.microsoft.com/dotnet/api/system.string.endswith#system-string-endswith(system-string)
            // https://learn.microsoft.com/dotnet/api/system.string.endswith#system-string-endswith(system-char)
            // https://learn.microsoft.com/dotnet/api/system.string.contains#system-string-contains(system-string)
            // https://learn.microsoft.com/dotnet/api/system.string.contains#system-string-contains(system-char)
            case nameof(string.StartsWith) or nameof(string.EndsWith) or nameof(string.Contains)
                when declaringType == typeof(string)
                    && @object is not null
                    && arguments is [Expression value]
                    && value.Type == typeof(string):
            {
                return TranslateStartsEndsWithContains(
                    @object,
                    value,
                    method.Name switch
                    {
                        nameof(string.StartsWith) => StartsEndsWithContains.StartsWith,
                        nameof(string.EndsWith) => StartsEndsWithContains.EndsWith,
                        nameof(string.Contains) => StartsEndsWithContains.Contains,
                        _ => throw new UnreachableException()
                    });
            }
        }

        return QueryCompilationContext.NotTranslatedExpression;
    }

    private SqlExpression? TranslateCubeToSubset(SqlExpression cubeExpression, SqlExpression indexesExpression)
    {
        SqlExpression convertedIndexes;

        switch (indexesExpression)
        {
            // Parameters or columns - create subquery to convert 0-based to 1-based at runtime
            case SqlParameterExpression or ColumnExpression:
            {
                // Apply type mapping to the indexes array
                var intArrayTypeMapping = _typeMappingSource.FindMapping(typeof(int[]))!;
                var typedIndexes = _sqlExpressionFactory.ApplyTypeMapping(indexesExpression, intArrayTypeMapping);

                // Generate table alias and create unnest table
                var tableAlias = ((RelationalQueryCompilationContext)_queryCompilationContext).SqlAliasManager.GenerateTableAlias("u");
                var unnestTable = new PgUnnestExpression(tableAlias, typedIndexes, "x", withOrdinality: false);

                // Create column reference for unnested value
                var intTypeMapping = _typeMappingSource.FindMapping(typeof(int))!;
                var xColumn = new ColumnExpression("x", tableAlias, typeof(int), intTypeMapping, nullable: false);

                // Create increment expression: x + 1
                var xPlusOne = _sqlExpressionFactory.Add(xColumn, _sqlExpressionFactory.Constant(1, intTypeMapping));

                // Create array_agg(x + 1) function
                var arrayAggFunction = _sqlExpressionFactory.Function(
                    "array_agg",
                    [xPlusOne],
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(int[]),
                    intArrayTypeMapping);

                // Construct SelectExpression
#pragma warning disable EF1001 // SelectExpression constructors are pubternal
                var selectExpression = new SelectExpression(
                    [unnestTable],
                    arrayAggFunction,
                    [],
                    ((RelationalQueryCompilationContext)_queryCompilationContext).SqlAliasManager);
#pragma warning restore EF1001

                // Finalize and wrap in ScalarSubqueryExpression
                selectExpression.ApplyProjection();
                convertedIndexes = new ScalarSubqueryExpression(selectExpression);
                break;
            }

            // Constant arrays - convert directly at compile time
            case SqlConstantExpression { Value: int[] constantArray }:
            {
                var oneBasedValues = constantArray.Select(i => i + 1).ToArray();
                convertedIndexes = _sqlExpressionFactory.Constant(oneBasedValues);
                break;
            }

            // Inline arrays (new[] { ... }) - convert each element
            case PgNewArrayExpression { Expressions: var expressions }:
            {
                var convertedExpressions = expressions
                    .Select(e => e is SqlConstantExpression { Value: int index }
                        ? _sqlExpressionFactory.Constant(index + 1)  // Constant element
                        : _sqlExpressionFactory.Add(e, _sqlExpressionFactory.Constant(1)))  // Non-constant element
                    .ToArray();
                convertedIndexes = _sqlExpressionFactory.NewArray(convertedExpressions, typeof(int[]));
                break;
            }

            default:
                // Unexpected case - cannot translate
                return null;
        }

        // Build final cube_subset function call
        return _sqlExpressionFactory.Function(
            "cube_subset",
            [cubeExpression, convertedIndexes],
            nullable: true,
            argumentsPropagateNullability: TrueArrays[2],
            typeof(NpgsqlCube),
            _typeMappingSource.FindMapping(typeof(NpgsqlCube)));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNew(NewExpression newExpression)
    {
        var visitedNewExpression = base.VisitNew(newExpression);

        if (visitedNewExpression != QueryCompilationContext.NotTranslatedExpression)
        {
            return visitedNewExpression;
        }

        // We translate new ValueTuple<T1, T2...>(x, y...) to a SQL row value expression: (x, y).
        // This is notably done to support row value comparisons: WHERE (x, y) > (3, 4) (see e.g. NpgsqlDbFunctionsExtensions.GreaterThan)
        if (newExpression.Type.IsAssignableTo(typeof(ITuple)))
        {
            return TryTranslateArguments(out var sqlArguments)
                ? new PgRowValueExpression(sqlArguments, newExpression.Type)
                : QueryCompilationContext.NotTranslatedExpression;
        }

        var constructor = newExpression.Constructor;

        // Translate new DateTime(...) -> make_timestamp/make_date
        if (constructor?.DeclaringType == typeof(DateTime))
        {
            switch (newExpression.Arguments)
            {
                // https://learn.microsoft.com/dotnet/api/system.datetime.-ctor#system-datetime-ctor(system-int32-system-int32-system-int32)
                case [var year, var month, var day]
                    when year.Type == typeof(int) && month.Type == typeof(int) && day.Type == typeof(int):
                {
                    return TryTranslateArguments(out var sqlArguments)
                        ? _sqlExpressionFactory.Function(
                            "make_date", sqlArguments, nullable: true, TrueArrays[3], typeof(DateTime), _timestampMapping)
                        : QueryCompilationContext.NotTranslatedExpression;
                }

                // https://learn.microsoft.com/dotnet/api/system.datetime.-ctor#system-datetime-ctor(system-int32-system-int32-system-int32-system-int32-system-int32-system-int32)
                case [var year, var month, var day, var hour, var minute, var second]
                    when year.Type == typeof(int) && month.Type == typeof(int) && day.Type == typeof(int)
                        && hour.Type == typeof(int) && minute.Type == typeof(int) && second.Type == typeof(int):
                {
                    if (!TryTranslateArguments(out var sqlArguments))
                    {
                        return QueryCompilationContext.NotTranslatedExpression;
                    }

                    // DateTime's second component is an int, but PostgreSQL's MAKE_TIMESTAMP accepts a double precision
                    sqlArguments[5] = _sqlExpressionFactory.Convert(sqlArguments[5], typeof(double));

                    return _sqlExpressionFactory.Function(
                        "make_timestamp", sqlArguments, nullable: true, TrueArrays[6], typeof(DateTime), _timestampMapping);
                }

                // https://learn.microsoft.com/dotnet/api/system.datetime.-ctor#system-datetime-ctor(system-int32-system-int32-system-int32-system-int32-system-int32-system-int32-system-datetimekind)
                case [var year, var month, var day, var hour, var minute, var second, ConstantExpression { Value: DateTimeKind kind }]
                    when year.Type == typeof(int) && month.Type == typeof(int) && day.Type == typeof(int)
                        && hour.Type == typeof(int) && minute.Type == typeof(int) && second.Type == typeof(int):
                {
                    if (!TryTranslateArguments(out var sqlArguments))
                    {
                        return QueryCompilationContext.NotTranslatedExpression;
                    }

                    // DateTime's second component is an int, but PostgreSQL's make_timestamp/make_timestamptz accepts a double precision.
                    // Also chop off the last Kind argument which does not get sent to PostgreSQL
                    var rewrittenArguments = new List<SqlExpression>
                    {
                        sqlArguments[0],
                        sqlArguments[1],
                        sqlArguments[2],
                        sqlArguments[3],
                        sqlArguments[4],
                        _sqlExpressionFactory.Convert(sqlArguments[5], typeof(double))
                    };

                    if (kind == DateTimeKind.Utc)
                    {
                        rewrittenArguments.Add(_sqlExpressionFactory.Constant("UTC"));
                    }

                    return _sqlExpressionFactory.Function(
                        kind == DateTimeKind.Utc ? "make_timestamptz" : "make_timestamp",
                        rewrittenArguments,
                        nullable: true,
                        TrueArrays[rewrittenArguments.Count],
                        typeof(DateTime),
                        kind == DateTimeKind.Utc ? _timestampTzMapping : _timestampMapping);
                }
            }
        }

        // Translate new DateOnly(...) -> make_date
        if (constructor?.DeclaringType == typeof(DateOnly))
        {
            if (newExpression.Arguments is [var year, var month, var day]
                && year.Type == typeof(int) && month.Type == typeof(int) && day.Type == typeof(int))
            {
                return TryTranslateArguments(out var sqlArguments)
                    ? _sqlExpressionFactory.Function(
                        "make_date", sqlArguments, nullable: true, TrueArrays[3], typeof(DateOnly))
                    : QueryCompilationContext.NotTranslatedExpression;
            }
        }

        // Translate new NpgsqlCube(...) -> cube(...)
        if (constructor?.DeclaringType == typeof(NpgsqlCube))
        {
            if (!TryTranslateArguments(out var sqlArguments))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            var cubeTypeMapping = _typeMappingSource.FindMapping(typeof(NpgsqlCube));
            var cubeParameters = constructor.GetParameters();

            // Distinguish constructor overloads by parameter patterns
            switch (cubeParameters)
            {
                case [var pCoords] when pCoords.ParameterType.IsAssignableFrom(typeof(double))
                    || typeof(IEnumerable<double>).IsAssignableFrom(pCoords.ParameterType):
                    // NpgsqlCube(double coord) or NpgsqlCube(IEnumerable<double> coords)
                case [var pCoord1, var pCoord2] when pCoord1.ParameterType.IsAssignableFrom(typeof(double))
                    && pCoord2.ParameterType.IsAssignableFrom(typeof(double)):
                    // NpgsqlCube(double coord1, double coord2)
                case [var pLowerLeft, var pUpperRight]
                    when typeof(IEnumerable<double>).IsAssignableFrom(pLowerLeft.ParameterType)
                        && typeof(IEnumerable<double>).IsAssignableFrom(pUpperRight.ParameterType):
                    // NpgsqlCube(IEnumerable<double> lowerLeft, IEnumerable<double> upperRight)
                case [var pCube, var pCoord] when pCube.ParameterType.IsAssignableFrom(typeof(NpgsqlCube))
                    && pCoord.ParameterType.IsAssignableFrom(typeof(double)):
                    // NpgsqlCube(NpgsqlCube cube, double coord)
                case [var pCube2, var pCoord12, var pCoord22]
                    when pCube2.ParameterType.IsAssignableFrom(typeof(NpgsqlCube))
                        && pCoord12.ParameterType.IsAssignableFrom(typeof(double))
                        && pCoord22.ParameterType.IsAssignableFrom(typeof(double)):
                    // NpgsqlCube(NpgsqlCube cube, double coord1, double coord2)
                    // All cases fallthrough to single cube() expression
                    // cube() is a STRICT function - returns NULL if any argument is NULL
                    return _sqlExpressionFactory.Function(
                        "cube",
                        sqlArguments,
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[sqlArguments.Length],
                        typeof(NpgsqlCube),
                        cubeTypeMapping);
            }
        }

        return QueryCompilationContext.NotTranslatedExpression;

        bool TryTranslateArguments(out SqlExpression[] sqlArguments)
        {
            sqlArguments = new SqlExpression[newExpression.Arguments.Count];
            for (var i = 0; i < sqlArguments.Length; i++)
            {
                var argument = newExpression.Arguments[i];
                if (TranslationFailed(argument, Visit(argument), out var sqlArgument))
                {
                    return false;
                }

                sqlArguments[i] = sqlArgument!;
            }

            return true;
        }
    }

    #region StartsWith/EndsWith/Contains

    private Expression TranslateStartsEndsWithContains(Expression instance, Expression pattern, StartsEndsWithContains methodType)
    {
        if (Visit(instance) is not SqlExpression translatedInstance || Visit(pattern) is not SqlExpression translatedPattern)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        var stringTypeMapping = ExpressionExtensions.InferTypeMapping(translatedInstance, translatedPattern);

        translatedInstance = _sqlExpressionFactory.ApplyTypeMapping(translatedInstance, stringTypeMapping);
        translatedPattern = _sqlExpressionFactory.ApplyTypeMapping(translatedPattern, stringTypeMapping);

        switch (translatedPattern)
        {
            case SqlConstantExpression patternConstant:
            {
                // The pattern is constant. Aside from null and empty string, we escape all special characters (%, _, \) and send a
                // simple LIKE
                return patternConstant.Value switch
                {
                    null => _sqlExpressionFactory.Like(
                        translatedInstance,
                        _sqlExpressionFactory.Constant(null, typeof(string), stringTypeMapping)),

                    // In .NET, all strings start with/end with/contain the empty string, but SQL LIKE return false for empty patterns.
                    // Return % which always matches instead.
                    // Note that we don't just return a true constant, since null strings shouldn't match even an empty string
                    // (but SqlNullabilityProcess will convert this to a true constant if the instance is non-nullable)
                    "" => _sqlExpressionFactory.Like(translatedInstance, _sqlExpressionFactory.Constant("%")),

                    string s => _sqlExpressionFactory.Like(
                        translatedInstance,
                        _sqlExpressionFactory.Constant(
                            methodType switch
                            {
                                StartsEndsWithContains.StartsWith => EscapeLikePattern(s) + '%',
                                StartsEndsWithContains.EndsWith => '%' + EscapeLikePattern(s),
                                StartsEndsWithContains.Contains => $"%{EscapeLikePattern(s)}%",

                                _ => throw new ArgumentOutOfRangeException(nameof(methodType), methodType, null)
                            })),

                    _ => throw new UnreachableException()
                };
            }

            case SqlParameterExpression patternParameter:
            {
                // The pattern is a parameter, register a runtime parameter that will contain the rewritten LIKE pattern, where
                // all special characters have been escaped.
                var lambda = Expression.Lambda(
                    Expression.Call(
                        EscapeLikePatternParameterMethod,
                        QueryCompilationContext.QueryContextParameter,
                        Expression.Constant(patternParameter.Name),
                        Expression.Constant(methodType)),
                    QueryCompilationContext.QueryContextParameter);

                var escapedPatternParameter =
                    _queryCompilationContext.RegisterRuntimeParameter(
                        $"{patternParameter.Name}_{methodType.ToString().ToLower(CultureInfo.InvariantCulture)}", lambda);

                return _sqlExpressionFactory.Like(
                    translatedInstance,
                    new SqlParameterExpression(escapedPatternParameter.Name!, escapedPatternParameter.Type, stringTypeMapping));
            }

            default:
                // The pattern is a column or a complex expression; the possible special characters in the pattern cannot be escaped,
                // preventing us from translating to LIKE.
                switch (methodType)
                {
                    // For StartsWith/EndsWith, use LEFT or RIGHT instead to extract substring and compare:
                    // WHERE instance IS NOT NULL AND pattern IS NOT NULL AND LEFT(instance, LEN(pattern)) = pattern
                    // This is less efficient than LIKE (i.e. StartsWith does an index scan instead of seek), but we have no choice.
                    case StartsEndsWithContains.StartsWith or StartsEndsWithContains.EndsWith:
                        var translation =
                            _sqlExpressionFactory.Function(
                                methodType is StartsEndsWithContains.StartsWith ? "left" : "right",
                                [
                                    translatedInstance,
                                    _sqlExpressionFactory.Function(
                                        "length", [translatedPattern], nullable: true,
                                        argumentsPropagateNullability: [true], typeof(int))
                                ], nullable: true, argumentsPropagateNullability: [true, true], typeof(string),
                                stringTypeMapping);

                        // LEFT/RIGHT of a citext return a text, so for non-default text mappings we apply an explicit cast.
                        if (translatedInstance.TypeMapping is { StoreType: not "text" })
                        {
                            translation = _sqlExpressionFactory.Convert(translation, typeof(string), translatedInstance.TypeMapping);
                        }

                        // We compensate for the case where both the instance and the pattern are null (null.StartsWith(null)); a simple
                        // equality would yield true in that case, but we want false.
                        return
                            _sqlExpressionFactory.AndAlso(
                                _sqlExpressionFactory.IsNotNull(translatedInstance),
                                _sqlExpressionFactory.AndAlso(
                                    _sqlExpressionFactory.IsNotNull(translatedPattern),
                                    _sqlExpressionFactory.Equal(translation, translatedPattern)));

                    // For Contains, just use strpos and check if the result is greater than 0. Note that strpos returns 1 when the pattern
                    // is an empty string, just like .NET Contains (so no need to compensate)
                    case StartsEndsWithContains.Contains:
                        return
                            _sqlExpressionFactory.AndAlso(
                                _sqlExpressionFactory.IsNotNull(translatedInstance),
                                _sqlExpressionFactory.AndAlso(
                                    _sqlExpressionFactory.IsNotNull(translatedPattern),
                                    _sqlExpressionFactory.GreaterThan(
                                        _sqlExpressionFactory.Function(
                                            "strpos", [translatedInstance, translatedPattern], nullable: true,
                                            argumentsPropagateNullability: [true, true], typeof(int)),
                                        _sqlExpressionFactory.Constant(0))));

                    default:
                        throw new UnreachableException();
                }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string? ConstructLikePatternParameter(
        QueryContext queryContext,
        string baseParameterName,
        StartsEndsWithContains methodType)
        => queryContext.Parameters[baseParameterName] switch
        {
            null => null,

            // In .NET, all strings start/end with the empty string, but SQL LIKE return false for empty patterns.
            // Return % which always matches instead.
            "" => "%",

            string s => methodType switch
            {
                StartsEndsWithContains.StartsWith => EscapeLikePattern(s) + '%',
                StartsEndsWithContains.EndsWith => '%' + EscapeLikePattern(s),
                StartsEndsWithContains.Contains => $"%{EscapeLikePattern(s)}%",
                _ => throw new ArgumentOutOfRangeException(nameof(methodType), methodType, null)
            },

            _ => throw new UnreachableException()
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public enum StartsEndsWithContains
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        StartsWith,

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        EndsWith,

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        Contains
    }

    private static bool IsLikeWildChar(char c)
        => c is '%' or '_';

    private static string EscapeLikePattern(string pattern)
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

    #endregion StartsWith/EndsWith/Contains

    #region GREATEST/LEAST

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override SqlExpression GenerateGreatest(IReadOnlyList<SqlExpression> expressions, Type resultType)
    {
        // Docs: https://www.postgresql.org/docs/current/functions-conditional.html#FUNCTIONS-GREATEST-LEAST
        var resultTypeMapping = ExpressionExtensions.InferTypeMapping(expressions);

        // If one or more arguments aren't NULL, then NULL arguments are ignored during comparison.
        // If all arguments are NULL, then GREATEST returns NULL.
        return _sqlExpressionFactory.Function(
            "GREATEST", expressions, nullable: true, Enumerable.Repeat(false, expressions.Count), resultType, resultTypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override SqlExpression GenerateLeast(IReadOnlyList<SqlExpression> expressions, Type resultType)
    {
        // Docs: https://www.postgresql.org/docs/current/functions-conditional.html#FUNCTIONS-GREATEST-LEAST
        var resultTypeMapping = ExpressionExtensions.InferTypeMapping(expressions);

        // If one or more arguments aren't NULL, then NULL arguments are ignored during comparison.
        // If all arguments are NULL, then LEAST returns NULL.
        return _sqlExpressionFactory.Function(
            "LEAST", expressions, nullable: true, Enumerable.Repeat(false, expressions.Count), resultType, resultTypeMapping);
    }

    #endregion GREATEST/LEAST

    #region Copied from RelationalSqlTranslatingExpressionVisitor

    private static Expression TryRemoveImplicitConvert(Expression expression)
    {
        if (expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression)
        {
            var innerType = unaryExpression.Operand.Type.UnwrapNullableType();
            if (innerType.IsEnum)
            {
                innerType = Enum.GetUnderlyingType(innerType);
            }

            var convertedType = unaryExpression.Type.UnwrapNullableType();

            if (innerType == convertedType
                || (convertedType == typeof(int)
                    && (innerType == typeof(byte)
                        || innerType == typeof(sbyte)
                        || innerType == typeof(char)
                        || innerType == typeof(short)
                        || innerType == typeof(ushort))))
            {
                return TryRemoveImplicitConvert(unaryExpression.Operand);
            }
        }

        return expression;
    }

    [DebuggerStepThrough]
    private static bool TranslationFailed(Expression? original, Expression? translation, out SqlExpression? castTranslation)
    {
        if (original is not null && !(translation is SqlExpression))
        {
            castTranslation = null;
            return true;
        }

        castTranslation = translation as SqlExpression;
        return false;
    }

    #endregion Copied from RelationalSqlTranslatingExpressionVisitor
}
