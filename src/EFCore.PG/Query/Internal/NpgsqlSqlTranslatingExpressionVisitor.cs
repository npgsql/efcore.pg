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
public class NpgsqlSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
{
    private readonly QueryCompilationContext _queryCompilationContext;
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly NpgsqlJsonPocoTranslator _jsonPocoTranslator;

    private readonly RelationalTypeMapping _timestampMapping;
    private readonly RelationalTypeMapping _timestampTzMapping;

    private static Type? _nodaTimePeriodType;

    private static readonly ConstructorInfo DateTimeCtor1 =
        typeof(DateTime).GetConstructor([typeof(int), typeof(int), typeof(int)])!;

    private static readonly ConstructorInfo DateTimeCtor2 =
        typeof(DateTime).GetConstructor([typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int)])!;

    private static readonly ConstructorInfo DateTimeCtor3 =
        typeof(DateTime).GetConstructor(
            [typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(DateTimeKind)])!;

    private static readonly ConstructorInfo DateOnlyCtor =
        typeof(DateOnly).GetConstructor([typeof(int), typeof(int), typeof(int)])!;

    private static readonly MethodInfo StringStartsWithMethod
        = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [typeof(string)])!;

    private static readonly MethodInfo StringEndsWithMethod
        = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [typeof(string)])!;

    private static readonly MethodInfo StringContainsMethod
        = typeof(string).GetRuntimeMethod(nameof(string.Contains), [typeof(string)])!;

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
    public NpgsqlSqlTranslatingExpressionVisitor(
        RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
        QueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        : base(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
    {
        _queryCompilationContext = queryCompilationContext;
        _sqlExpressionFactory = (NpgsqlSqlExpressionFactory)dependencies.SqlExpressionFactory;
        _jsonPocoTranslator = ((NpgsqlMemberTranslatorProvider)Dependencies.MemberTranslatorProvider).JsonPocoTranslator;
        _typeMappingSource = dependencies.TypeMappingSource;
        _timestampMapping = _typeMappingSource.FindMapping("timestamp without time zone")!;
        _timestampTzMapping = _typeMappingSource.FindMapping("timestamp with time zone")!;
    }

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
                if (_jsonPocoTranslator.TranslateArrayLength(sqlOperand) is SqlExpression translation)
                {
                    return translation;
                }

                // Note that Length over PG arrays (not within JSON) gets translated by QueryableMethodTranslatingEV, since arrays are
                // primitive collections
                break;

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
                && (unaryExpression.Operand.Type == typeof(IPAddress) || unaryExpression.Operand.Type == typeof(NpgsqlCidr)):
                return Visit(unaryExpression.Operand);
        }

        return base.VisitUnary(unaryExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
    {
        if (base.VisitNewArray(newArrayExpression) is SqlExpression visitedNewArrayExpression)
        {
            return visitedNewArrayExpression;
        }

        if (newArrayExpression.NodeType == ExpressionType.NewArrayInit)
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
        switch (binaryExpression.NodeType)
        {
            case ExpressionType.Subtract
                when binaryExpression.Left.Type.UnwrapNullableType().FullName == "NodaTime.LocalDate"
                && binaryExpression.Right.Type.UnwrapNullableType().FullName == "NodaTime.LocalDate":
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

            case ExpressionType.ArrayIndex:
            {
                // During preprocessing, ArrayIndex and List[] get normalized to ElementAt; see NpgsqlArrayTranslator
                Check.DebugFail(
                    "During preprocessing, ArrayIndex and List[] get normalized to ElementAt; see NpgsqlArrayTranslator. "
                    + "Should never see ArrayIndex.");
                break;
            }
        }

        var translation = base.VisitBinary(binaryExpression);

        switch (translation)
        {
            // Optimize (x - c) - (y - c) to x - y.
            // This is particularly useful for DateOnly.DayNumber - DateOnly.DayNumber, which is the way to express DateOnly subtraction
            // (the subtraction operator isn't defined over DateOnly in .NET). The translation of x.DayNumber is x - DATE '0001-01-01',
            // so the below is a useful simplification.
            // TODO: As this is a generic mathematical simplification, we should move it to a generic optimization phase in EF Core.
            case SqlBinaryExpression
            {
                OperatorType: ExpressionType.Subtract,
                Left: SqlBinaryExpression { OperatorType: ExpressionType.Subtract, Left: var left1, Right: var right1 },
                Right: SqlBinaryExpression { OperatorType: ExpressionType.Subtract, Left: var left2, Right: var right2 }
            } originalBinary when right1.Equals(right2):
            {
                return new SqlBinaryExpression(ExpressionType.Subtract, left1, left2, originalBinary.Type, originalBinary.TypeMapping);
            }

            // A somewhat hacky workaround for #2942.
            // When an optional owned JSON entity is compared to null, we get WHERE (x -> y) IS NULL.
            // The -> operator (returning jsonb) is used rather than ->> (returning text), since an entity type is being extracted, and
            // further JSON operations may need to be composed. However, when the value extracted is a JSON null, a non-NULL jsonb value is
            // returned, and comparing that to relational NULL returns false.
            // Pattern-match this and force the use of ->> by changing the mapping to be a scalar rather than an entity type.
            case SqlUnaryExpression
            {
                OperatorType: ExpressionType.Equal or ExpressionType.NotEqual,
                Operand: JsonScalarExpression { TypeMapping: NpgsqlOwnedJsonTypeMapping } operand
            } unary:
            {
                return unary.Update(
                    new JsonScalarExpression(
                        operand.Json, operand.Path, operand.Type, _typeMappingSource.FindMapping("text"), operand.IsNullable));
            }
        }

        return translation;
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

        if (method == StringStartsWithMethod
            && TryTranslateStartsEndsWithContains(
                methodCallExpression.Object!, methodCallExpression.Arguments[0], StartsEndsWithContains.StartsWith, out var translation1))
        {
            return translation1;
        }

        if (method == StringEndsWithMethod
            && TryTranslateStartsEndsWithContains(
                methodCallExpression.Object!, methodCallExpression.Arguments[0], StartsEndsWithContains.EndsWith, out var translation2))
        {
            return translation2;
        }

        if (method == StringContainsMethod
            && TryTranslateStartsEndsWithContains(
                methodCallExpression.Object!, methodCallExpression.Arguments[0], StartsEndsWithContains.Contains, out var translation3))
        {
            return translation3;
        }

        return base.VisitMethodCall(methodCallExpression);
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

        // Translate new DateTime(...) -> make_timestamp/make_date
        if (newExpression.Constructor?.DeclaringType == typeof(DateTime))
        {
            if (newExpression.Constructor == DateTimeCtor1)
            {
                return TryTranslateArguments(out var sqlArguments)
                    ? _sqlExpressionFactory.Function(
                        "make_date", sqlArguments, nullable: true, TrueArrays[3], typeof(DateTime), _timestampMapping)
                    : QueryCompilationContext.NotTranslatedExpression;
            }

            if (newExpression.Constructor == DateTimeCtor2)
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

            if (newExpression.Constructor == DateTimeCtor3
                && newExpression.Arguments[6] is ConstantExpression { Value : DateTimeKind kind })
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

        // Translate new DateOnly(...) -> make_date
        if (newExpression.Constructor == DateOnlyCtor)
        {
            return TryTranslateArguments(out var sqlArguments)
                ? _sqlExpressionFactory.Function(
                    "make_date", sqlArguments, nullable: true, TrueArrays[3], typeof(DateOnly))
                : QueryCompilationContext.NotTranslatedExpression;
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

    private bool TryTranslateStartsEndsWithContains(
        Expression instance,
        Expression pattern,
        StartsEndsWithContains methodType,
        [NotNullWhen(true)] out SqlExpression? translation)
    {
        if (Visit(instance) is not SqlExpression translatedInstance
            || Visit(pattern) is not SqlExpression translatedPattern)
        {
            translation = null;
            return false;
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
                translation = patternConstant.Value switch
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

                return true;
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

                translation = _sqlExpressionFactory.Like(
                    translatedInstance,
                    new SqlParameterExpression(escapedPatternParameter.Name!, escapedPatternParameter.Type, stringTypeMapping));

                return true;
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
                        translation =
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
                        translation =
                            _sqlExpressionFactory.AndAlso(
                                _sqlExpressionFactory.IsNotNull(translatedInstance),
                                _sqlExpressionFactory.AndAlso(
                                    _sqlExpressionFactory.IsNotNull(translatedPattern),
                                    _sqlExpressionFactory.Equal(translation, translatedPattern)));

                        break;

                    // For Contains, just use strpos and check if the result is greater than 0. Note that strpos returns 1 when the pattern
                    // is an empty string, just like .NET Contains (so no need to compensate)
                    case StartsEndsWithContains.Contains:
                        translation =
                            _sqlExpressionFactory.AndAlso(
                                _sqlExpressionFactory.IsNotNull(translatedInstance),
                                _sqlExpressionFactory.AndAlso(
                                    _sqlExpressionFactory.IsNotNull(translatedPattern),
                                    _sqlExpressionFactory.GreaterThan(
                                        _sqlExpressionFactory.Function(
                                            "strpos", [translatedInstance, translatedPattern], nullable: true,
                                            argumentsPropagateNullability: [true, true], typeof(int)),
                                        _sqlExpressionFactory.Constant(0))));
                        break;

                    default:
                        throw new UnreachableException();
                }

                return true;
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
