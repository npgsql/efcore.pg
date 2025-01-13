using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

/// <inheritdoc />
public class NpgsqlSqlExpressionFactory : SqlExpressionFactory
{
    private readonly NpgsqlTypeMappingSource _typeMappingSource;
    private readonly RelationalTypeMapping _boolTypeMapping;

    private static Type? _nodaTimeDurationType;
    private static Type? _nodaTimePeriodType;

    /// <summary>
    ///     Creates a new instance of the <see cref="NpgsqlSqlExpressionFactory" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    public NpgsqlSqlExpressionFactory(SqlExpressionFactoryDependencies dependencies)
        : base(dependencies)
    {
        _typeMappingSource = (NpgsqlTypeMappingSource)dependencies.TypeMappingSource;
        _boolTypeMapping = _typeMappingSource.FindMapping(typeof(bool), dependencies.Model)!;
    }

    #region Expression factory methods

    /// <summary>
    ///     Creates a new <see cref="PgRegexMatchExpression" />, corresponding to the PostgreSQL-specific <c>~</c> operator.
    /// </summary>
    public virtual PgRegexMatchExpression RegexMatch(
        SqlExpression match,
        SqlExpression pattern,
        RegexOptions options)
        => (PgRegexMatchExpression)ApplyDefaultTypeMapping(new PgRegexMatchExpression(match, pattern, options, null));

    /// <summary>
    ///     Creates a new <see cref="PgAnyExpression" />, corresponding to the PostgreSQL-specific <c>= ANY</c> operator.
    /// </summary>
    public virtual PgAnyExpression Any(
        SqlExpression item,
        SqlExpression array,
        PgAnyOperatorType operatorType)
        => (PgAnyExpression)ApplyDefaultTypeMapping(new PgAnyExpression(item, array, operatorType, null));

    /// <summary>
    ///     Creates a new <see cref="PgAllExpression" />, corresponding to the PostgreSQL-specific <c>LIKE ALL</c> operator.
    /// </summary>
    public virtual PgAllExpression All(
        SqlExpression item,
        SqlExpression array,
        PgAllOperatorType operatorType)
        => (PgAllExpression)ApplyDefaultTypeMapping(new PgAllExpression(item, array, operatorType, null));

    /// <summary>
    ///     Creates a new <see cref="PgArrayIndexExpression" />, corresponding to the PostgreSQL-specific array subscripting operator.
    /// </summary>
    public virtual PgArrayIndexExpression ArrayIndex(
        SqlExpression array,
        SqlExpression index,
        bool nullable,
        RelationalTypeMapping? typeMapping = null)
    {
        if (!array.Type.TryGetElementType(out var elementType))
        {
            throw new ArgumentException("Array expression must be of an array or List<> type", nameof(array));
        }

        return (PgArrayIndexExpression)ApplyTypeMapping(
            new PgArrayIndexExpression(array, index, nullable, elementType, typeMapping: null),
            typeMapping);
    }

    /// <summary>
    ///     Creates a new <see cref="PgArrayIndexExpression" />, corresponding to the PostgreSQL-specific array subscripting operator.
    /// </summary>
    public virtual PgArraySliceExpression ArraySlice(
        SqlExpression array,
        SqlExpression? lowerBound,
        SqlExpression? upperBound,
        bool nullable,
        RelationalTypeMapping? typeMapping = null)
        => (PgArraySliceExpression)ApplyTypeMapping(
            new PgArraySliceExpression(array, lowerBound, upperBound, nullable, array.Type, typeMapping: null),
            typeMapping);

    /// <summary>
    ///     Creates a new <see cref="AtTimeZoneExpression" />, for converting a timestamp to UTC.
    /// </summary>
    public virtual AtTimeZoneExpression AtUtc(
        SqlExpression timestamp,
        RelationalTypeMapping? typeMapping = null)
        => AtTimeZone(timestamp, Constant("UTC"), timestamp.Type);

    /// <summary>
    ///     Creates a new <see cref="AtTimeZoneExpression" />, for converting a timestamp to another time zone.
    /// </summary>
    public virtual AtTimeZoneExpression AtTimeZone(
        SqlExpression timestamp,
        SqlExpression timeZone,
        Type type,
        RelationalTypeMapping? typeMapping = null)
    {
        if (typeMapping is null)
        {
            // PostgreSQL AT TIME ZONE flips the given type from timestamptz to timestamp and vice versa
            // See https://www.postgresql.org/docs/current/functions-datetime.html#FUNCTIONS-DATETIME-ZONECONVERT
            typeMapping = timestamp.TypeMapping ?? _typeMappingSource.FindMapping(timestamp.Type, Dependencies.Model)!;
            var storeType = typeMapping.StoreType;

            typeMapping = storeType.StartsWith("timestamp with time zone", StringComparison.Ordinal)
                || storeType.StartsWith("timestamptz", StringComparison.Ordinal)
                    ? _typeMappingSource.FindMapping("timestamp without time zone")!
                    : storeType.StartsWith("timestamp without time zone", StringComparison.Ordinal)
                    || storeType.StartsWith("timestamp", StringComparison.Ordinal)
                        ? _typeMappingSource.FindMapping("timestamp with time zone")!
                        : throw new ArgumentException(
                            $"timestamp argument to AtTimeZone had unknown store type {storeType}", nameof(timestamp));
        }

        return new AtTimeZoneExpression(
            ApplyDefaultTypeMapping(timestamp),
            ApplyDefaultTypeMapping(timeZone),
            type,
            typeMapping);
    }

    /// <summary>
    ///     Creates a new <see cref="AtTimeZoneExpression" />, for performing a PostgreSQL-specific case-insensitive string match
    ///     (<c>ILIKE</c>).
    /// </summary>
    public virtual PgILikeExpression ILike(
        SqlExpression match,
        SqlExpression pattern,
        SqlExpression? escapeChar = null)
        => (PgILikeExpression)ApplyDefaultTypeMapping(new PgILikeExpression(match, pattern, escapeChar, null));

    /// <summary>
    ///     Creates a new <see cref="PgJsonTraversalExpression" />, for traversing inside a JSON document.
    /// </summary>
    public virtual PgJsonTraversalExpression JsonTraversal(
        SqlExpression expression,
        bool returnsText,
        Type type,
        RelationalTypeMapping? typeMapping = null)
        => JsonTraversal(expression, [], returnsText, type, typeMapping);

    /// <summary>
    ///     Creates a new <see cref="PgJsonTraversalExpression" />, for traversing inside a JSON document.
    /// </summary>
    public virtual PgJsonTraversalExpression JsonTraversal(
        SqlExpression expression,
        IEnumerable<SqlExpression> path,
        bool returnsText,
        Type type,
        RelationalTypeMapping? typeMapping = null)
        => new(
            ApplyDefaultTypeMapping(expression),
            path.Select(ApplyDefaultTypeMapping).ToArray()!,
            returnsText,
            type,
            typeMapping);

    /// <summary>
    ///     Constructs either a <see cref="PgNewArrayExpression" />, or, if all provided expressions are constants, a single
    ///     <see cref="SqlConstantExpression" /> for the entire array.
    /// </summary>
    public virtual SqlExpression NewArrayOrConstant(
        IReadOnlyList<SqlExpression> elements,
        Type type,
        RelationalTypeMapping? typeMapping = null)
    {
        var elementType = type.TryGetElementType(typeof(IEnumerable<>));
        if (elementType is null)
        {
            throw new ArgumentException($"{type.Name} isn't an IEnumerable<T>", nameof(type));
        }

        var newArrayExpression = NewArray(elements, type, typeMapping);

        if (newArrayExpression.Expressions.Any(e => e is not SqlConstantExpression))
        {
            return newArrayExpression;
        }

        // All elements are constants; extract their values and return an SqlConstantExpression over the array/list
        if (type.IsGenericList())
        {
            var list = (IList)Activator.CreateInstance(type, elements.Count)!;
            var addMethod = type.GetMethod("Add")!;
            for (var i = 0; i < elements.Count; i++)
            {
                addMethod.Invoke(list, [((SqlConstantExpression)newArrayExpression.Expressions[i]).Value]);
            }

            return Constant(list, newArrayExpression.TypeMapping);
        }

        // We support any arbitrary IEnumerable<T> as the expression type, but only support arrays and Lists as *concrete* types.
        // So unless the type was a List<T> (handled above), we return an array constant here.
        var array = Array.CreateInstance(elementType, elements.Count);
        for (var i = 0; i < elements.Count; i++)
        {
            array.SetValue(((SqlConstantExpression)newArrayExpression.Expressions[i]).Value, i);
        }

        return Constant(array, newArrayExpression.TypeMapping);
    }

    /// <summary>
    ///     Creates a new <see cref="PgNewArrayExpression" />, for creating a new PostgreSQL array.
    /// </summary>
    public virtual PgNewArrayExpression NewArray(
        IReadOnlyList<SqlExpression> expressions,
        Type type,
        RelationalTypeMapping? typeMapping = null)
        => (PgNewArrayExpression)ApplyTypeMapping(new PgNewArrayExpression(expressions, type, typeMapping), typeMapping);

    /// <inheritdoc />
    public override SqlExpression? MakeBinary(
        ExpressionType operatorType,
        SqlExpression left,
        SqlExpression right,
        RelationalTypeMapping? typeMapping,
        SqlExpression? existingExpr = null)
    {
        switch (operatorType)
        {
            case ExpressionType.Subtract
                when left.Type == typeof(DateTime) && right.Type == typeof(DateTime)
                || left.Type == typeof(DateTimeOffset) && right.Type == typeof(DateTimeOffset)
                || left.Type == typeof(TimeOnly) && right.Type == typeof(TimeOnly):
            {
                return (SqlBinaryExpression)ApplyTypeMapping(
                    new SqlBinaryExpression(ExpressionType.Subtract, left, right, typeof(TimeSpan), null), typeMapping);
            }

            case ExpressionType.Subtract
                when left.Type.FullName == "NodaTime.Instant" && right.Type.FullName == "NodaTime.Instant"
                || left.Type.FullName == "NodaTime.ZonedDateTime" && right.Type.FullName == "NodaTime.ZonedDateTime":
            {
                _nodaTimeDurationType ??= left.Type.Assembly.GetType("NodaTime.Duration");
                return (SqlBinaryExpression)ApplyTypeMapping(
                    new SqlBinaryExpression(ExpressionType.Subtract, left, right, _nodaTimeDurationType!, null), typeMapping);
            }

            case ExpressionType.Subtract
                when left.Type.FullName == "NodaTime.LocalDateTime" && right.Type.FullName == "NodaTime.LocalDateTime"
                || left.Type.FullName == "NodaTime.LocalTime" && right.Type.FullName == "NodaTime.LocalTime":
            {
                _nodaTimePeriodType ??= left.Type.Assembly.GetType("NodaTime.Period");
                return (SqlBinaryExpression)ApplyTypeMapping(
                    new SqlBinaryExpression(ExpressionType.Subtract, left, right, _nodaTimePeriodType!, null), typeMapping);
            }

            case ExpressionType.Subtract
                when left.Type.FullName == "NodaTime.LocalDate" && right.Type.FullName == "NodaTime.LocalDate":
            {
                return (SqlBinaryExpression)ApplyTypeMapping(
                    new SqlBinaryExpression(ExpressionType.Subtract, left, right, typeof(int), null), typeMapping);
            }
        }

        return base.MakeBinary(operatorType, left, right, typeMapping, existingExpr);
    }

    /// <summary>
    ///     Creates a new <see cref="PgBinaryExpression" /> with the given arguments.
    /// </summary>
    /// <param name="operatorType">An <see cref="T:System.Linq.Expressions.ExpressionType" /> representing SQL unary operator.</param>
    /// <param name="left">The left operand of binary operation.</param>
    /// <param name="right">The right operand of binary operation.</param>
    /// <param name="typeMapping">A type mapping to be assigned to the created expression.</param>
    /// <returns>A <see cref="PgBinaryExpression" /> with the given arguments.</returns>
    public virtual SqlExpression MakePostgresBinary(
        PgExpressionType operatorType,
        SqlExpression left,
        SqlExpression right,
        RelationalTypeMapping? typeMapping = null)
    {
        Check.NotNull(left, nameof(left));
        Check.NotNull(right, nameof(right));

        var returnType = left.Type;
        switch (operatorType)
        {
            case PgExpressionType.Contains:
            case PgExpressionType.ContainedBy:
            case PgExpressionType.Overlaps:
            case PgExpressionType.NetworkContainedByOrEqual:
            case PgExpressionType.NetworkContainsOrEqual:
            case PgExpressionType.NetworkContainsOrContainedBy:
            case PgExpressionType.RangeIsStrictlyLeftOf:
            case PgExpressionType.RangeIsStrictlyRightOf:
            case PgExpressionType.RangeDoesNotExtendRightOf:
            case PgExpressionType.RangeDoesNotExtendLeftOf:
            case PgExpressionType.RangeIsAdjacentTo:
            case PgExpressionType.TextSearchMatch:
            case PgExpressionType.JsonExists:
            case PgExpressionType.JsonExistsAny:
            case PgExpressionType.JsonExistsAll:
            case PgExpressionType.DictionaryContainsAnyKey:
            case PgExpressionType.DictionaryContainsAllKeys:
            case PgExpressionType.DictionaryContainsKey:
                returnType = typeof(bool);
                break;

            case PgExpressionType.Distance:
                returnType = typeof(double);
                break;
        }

        return (PgBinaryExpression)ApplyTypeMapping(
            new PgBinaryExpression(operatorType, left, right, returnType, null), typeMapping);
    }

    /// <summary>
    ///     Creates a new <see cref="PgBinaryExpression" />, for checking whether one value contains another.
    /// </summary>
    public virtual SqlExpression Contains(SqlExpression left, SqlExpression right)
    {
        Check.NotNull(left, nameof(left));
        Check.NotNull(right, nameof(right));

        return MakePostgresBinary(PgExpressionType.Contains, left, right);
    }

    /// <summary>
    ///     Creates a new <see cref="PgBinaryExpression" />, for checking whether one value is contained by another.
    /// </summary>
    public virtual SqlExpression ContainedBy(SqlExpression left, SqlExpression right)
    {
        Check.NotNull(left, nameof(left));
        Check.NotNull(right, nameof(right));

        return MakePostgresBinary(PgExpressionType.ContainedBy, left, right);
    }

    /// <summary>
    ///     Creates a new <see cref="PgBinaryExpression" />, for checking whether one value overlaps with another.
    /// </summary>
    public virtual SqlExpression Overlaps(SqlExpression left, SqlExpression right)
    {
        Check.NotNull(left, nameof(left));
        Check.NotNull(right, nameof(right));

        return MakePostgresBinary(PgExpressionType.Overlaps, left, right);
    }

    /// <summary>
    ///     Creates a new <see cref="PgFunctionExpression" /> for a PostgreSQL aggregate function call..
    /// </summary>
    public virtual PgFunctionExpression AggregateFunction(
        string name,
        IEnumerable<SqlExpression> arguments,
        EnumerableExpression aggregateEnumerableExpression,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
    {
        var typeMappedArguments = new List<SqlExpression>();

        foreach (var argument in arguments)
        {
            typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
        }

        return new PgFunctionExpression(
            name,
            typeMappedArguments,
            argumentNames: null,
            argumentSeparators: null,
            aggregateEnumerableExpression.IsDistinct,
            aggregateEnumerableExpression.Predicate,
            aggregateEnumerableExpression.Orderings,
            nullable: nullable,
            argumentsPropagateNullability: argumentsPropagateNullability, type: returnType, typeMapping: typeMapping);
    }

    #endregion Expression factory methods

    /// <inheritdoc />
    [return: NotNullIfNotNull("sqlExpression")]
    public override SqlExpression? ApplyTypeMapping(SqlExpression? sqlExpression, RelationalTypeMapping? typeMapping)
    {
        if (sqlExpression is not null && sqlExpression.TypeMapping is null)
        {
            sqlExpression = sqlExpression switch
            {
                SqlBinaryExpression e => ApplyTypeMappingOnSqlBinary(e, typeMapping),

                // PostgreSQL-specific expression types
                PgAnyExpression e => ApplyTypeMappingOnAny(e),
                PgAllExpression e => ApplyTypeMappingOnAll(e),
                PgArrayIndexExpression e => ApplyTypeMappingOnArrayIndex(e, typeMapping),
                PgArraySliceExpression e => ApplyTypeMappingOnArraySlice(e, typeMapping),
                PgBinaryExpression e => ApplyTypeMappingOnPostgresBinary(e, typeMapping),
                PgFunctionExpression e => e.ApplyTypeMapping(typeMapping),
                PgILikeExpression e => ApplyTypeMappingOnILike(e),
                PgNewArrayExpression e => ApplyTypeMappingOnNewArray(e, typeMapping),
                PgRegexMatchExpression e => ApplyTypeMappingOnRegexMatch(e),
                PgRowValueExpression e => ApplyTypeMappingOnRowValue(e, typeMapping),

                _ => base.ApplyTypeMapping(sqlExpression, typeMapping)
            };
        }

        if (!NpgsqlTypeMappingSource.LegacyTimestampBehavior
            && (typeMapping is NpgsqlTimestampTypeMapping && sqlExpression?.TypeMapping is NpgsqlTimestampTzTypeMapping
                || typeMapping is NpgsqlTimestampTzTypeMapping && sqlExpression?.TypeMapping is NpgsqlTimestampTypeMapping))
        {
            throw new NotSupportedException(
                "Cannot apply binary operation on types 'timestamp with time zone' and 'timestamp without time zone', convert one of the operands first.");
        }

        return sqlExpression;
    }

    private SqlBinaryExpression ApplyTypeMappingOnSqlBinary(SqlBinaryExpression binary, RelationalTypeMapping? typeMapping)
    {
        var (left, right) = (binary.Left, binary.Right);

        // The default SqlExpressionFactory behavior is to assume that the two added operands have the same type,
        // and so to infer one side's mapping from the other if needed. Here we take care of some heterogeneous
        // operand cases where this doesn't work:
        // * Period + Period (???)

        switch (binary.OperatorType)
        {
            // DateTime + TimeSpan => DateTime
            // DateTimeOffset + TimeSpan => DateTimeOffset
            // TimeOnly + TimeSpan => TimeOnly
            case ExpressionType.Add or ExpressionType.Subtract
                when right.Type == typeof(TimeSpan)
                && (left.Type == typeof(DateTime) || left.Type == typeof(DateTimeOffset) || left.Type == typeof(TimeOnly))
                || right.Type == typeof(int) && left.Type == typeof(DateOnly)
                || right.Type.FullName == "NodaTime.Period"
                && left.Type.FullName is "NodaTime.LocalDateTime" or "NodaTime.LocalDate" or "NodaTime.LocalTime"
                || right.Type.FullName == "NodaTime.Duration"
                && left.Type.FullName is "NodaTime.Instant" or "NodaTime.ZonedDateTime":
            {
                var newLeft = ApplyTypeMapping(left, typeMapping);
                var newRight = ApplyDefaultTypeMapping(right);
                return new SqlBinaryExpression(binary.OperatorType, newLeft, newRight, binary.Type, newLeft.TypeMapping);
            }

            // DateTime - DateTime => TimeSpan
            // DateTimeOffset - DateTimeOffset => TimeSpan
            // DateOnly - DateOnly => TimeSpan
            // TimeOnly - TimeOnly => TimeSpan
            // Instant - Instant => Duration
            // LocalDateTime - LocalDateTime => int (days)
            case ExpressionType.Subtract
                when left.Type == typeof(DateTime) && right.Type == typeof(DateTime)
                || left.Type == typeof(DateTimeOffset) && right.Type == typeof(DateTimeOffset)
                || left.Type == typeof(DateOnly) && right.Type == typeof(DateOnly)
                || left.Type == typeof(TimeOnly) && right.Type == typeof(TimeOnly)
                || left.Type.FullName == "NodaTime.Instant" && right.Type.FullName == "NodaTime.Instant"
                || left.Type.FullName == "NodaTime.LocalDateTime" && right.Type.FullName == "NodaTime.LocalDateTime"
                || left.Type.FullName == "NodaTime.ZonedDateTime" && right.Type.FullName == "NodaTime.ZonedDateTime"
                || left.Type.FullName == "NodaTime.LocalDate" && right.Type.FullName == "NodaTime.LocalDate"
                || left.Type.FullName == "NodaTime.LocalTime" && right.Type.FullName == "NodaTime.LocalTime":
            {
                var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(left, right);

                return new SqlBinaryExpression(
                    ExpressionType.Subtract,
                    ApplyTypeMapping(left, inferredTypeMapping),
                    ApplyTypeMapping(right, inferredTypeMapping),
                    binary.Type,
                    typeMapping ?? _typeMappingSource.FindMapping(binary.Type, "interval"));
            }

            // TODO: This is a hack until https://github.com/dotnet/efcore/pull/34995 is done; the translation of DateOnly.DayNumber
            // generates a substraction with a fragment, but for now we can't assign a type/type mapping to a fragment.
            case ExpressionType.Subtract when left.Type == typeof(DateOnly) && right is SqlFragmentExpression:
            {
                return new SqlBinaryExpression(
                    ExpressionType.Subtract,
                    ApplyDefaultTypeMapping(left),
                    right,
                    typeof(int),
                    _typeMappingSource.FindMapping(typeof(int)));
            }
        }

        // If this is a row value comparison (e.g. (a, b) > (5, 6)), doing type mapping inference on each corresponding pair.
        if (IsComparison(binary.OperatorType)
            && TryGetRowValueValues(binary.Left, out var leftValues)
            && TryGetRowValueValues(binary.Right, out var rightValues))
        {
            if (leftValues.Count != rightValues.Count)
            {
                throw new ArgumentException(NpgsqlStrings.RowValueComparisonRequiresTuplesOfSameLength);
            }

            var count = leftValues.Count;
            var updatedLeftValues = new SqlExpression[count];
            var updatedRightValues = new SqlExpression[count];

            for (var i = 0; i < count; i++)
            {
                var updatedElementBinaryExpression = MakeBinary(binary.OperatorType, leftValues[i], rightValues[i], typeMapping: null)!;

                if (updatedElementBinaryExpression is not SqlBinaryExpression
                    {
                        Left: var updatedLeft,
                        Right: var updatedRight,
                        OperatorType: var updatedOperatorType
                    }
                    || updatedOperatorType != binary.OperatorType)
                {
                    throw new UnreachableException("MakeBinary modified binary expression type/operator when doing row value comparison");
                }

                updatedLeftValues[i] = updatedLeft;
                updatedRightValues[i] = updatedRight;
            }

            // Note that we always return non-constant PostgresRowValueExpression operands, even if the original input was a
            // SqlConstantExpression. This is because each value in the row value needs to have its type mapping.
            binary = new SqlBinaryExpression(
                binary.OperatorType,
                new PgRowValueExpression(updatedLeftValues, binary.Left.Type),
                new PgRowValueExpression(updatedRightValues, binary.Right.Type),
                binary.Type,
                binary.TypeMapping);
        }

        return (SqlBinaryExpression)base.ApplyTypeMapping(binary, typeMapping);

        static bool IsComparison(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThanOrEqual:
                    return true;
                default:
                    return false;
            }
        }

        bool TryGetRowValueValues(SqlExpression e, [NotNullWhen(true)] out IReadOnlyList<SqlExpression>? values)
        {
            switch (e)
            {
                case PgRowValueExpression rowValueExpression:
                    values = rowValueExpression.Values;
                    return true;

                case SqlConstantExpression { Value : ITuple constantTuple }:
                    var v = new SqlExpression[constantTuple.Length];

                    for (var i = 0; i < v.Length; i++)
                    {
                        v[i] = Constant(constantTuple[i], typeof(object));
                    }

                    values = v;
                    return true;

                default:
                    values = null;
                    return false;
            }
        }
    }

    private SqlExpression ApplyTypeMappingOnRegexMatch(PgRegexMatchExpression pgRegexMatchExpression)
    {
        var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(
                pgRegexMatchExpression.Match, pgRegexMatchExpression.Pattern)
            ?? _typeMappingSource.FindMapping(pgRegexMatchExpression.Match.Type, Dependencies.Model);

        return new PgRegexMatchExpression(
            ApplyTypeMapping(pgRegexMatchExpression.Match, inferredTypeMapping),
            ApplyTypeMapping(pgRegexMatchExpression.Pattern, inferredTypeMapping),
            pgRegexMatchExpression.Options,
            _boolTypeMapping);
    }

    private SqlExpression ApplyTypeMappingOnRowValue(
        PgRowValueExpression pgRowValueExpression,
        RelationalTypeMapping? typeMapping)
    {
        // If the row value is in a binary expression (e.g. a comparison, (a, b) > (5, 6)), we have special type inference code
        // to infer from the other row value in ApplyTypeMappingOnSqlBinary.
        // If we're here, that means that no such inference can happen, and we just use the default type mappings.
        var updatedValues = new SqlExpression[pgRowValueExpression.Values.Count];

        for (var i = 0; i < updatedValues.Length; i++)
        {
            updatedValues[i] = ApplyDefaultTypeMapping(pgRowValueExpression.Values[i]);
        }

        return new PgRowValueExpression(updatedValues, pgRowValueExpression.Type, typeMapping);
    }

    private SqlExpression ApplyTypeMappingOnAny(PgAnyExpression pgAnyExpression)
    {
        var (item, array) = ApplyTypeMappingsOnItemAndArray(pgAnyExpression.Item, pgAnyExpression.Array);
        return new PgAnyExpression(item, array, pgAnyExpression.OperatorType, _boolTypeMapping);
    }

    private SqlExpression ApplyTypeMappingOnAll(PgAllExpression pgAllExpression)
    {
        var (item, array) = ApplyTypeMappingsOnItemAndArray(pgAllExpression.Item, pgAllExpression.Array);
        return new PgAllExpression(item, array, pgAllExpression.OperatorType, _boolTypeMapping);
    }

    internal (SqlExpression, SqlExpression) ApplyTypeMappingsOnItemAndArray(SqlExpression itemExpression, SqlExpression arrayExpression)
    {
        // Attempt type inference either from the operand to the array or the other way around
        var arrayMapping = arrayExpression.TypeMapping;

        var itemMapping =
            itemExpression.TypeMapping
            // Unwrap convert-to-object nodes - these get added for object[].Contains(x)
            ?? (itemExpression is SqlUnaryExpression { OperatorType: ExpressionType.Convert } unary && unary.Type == typeof(object)
                ? unary.Operand.TypeMapping
                : null)
            // If we couldn't find a type mapping on the item, try inferring it from the array
            ?? (RelationalTypeMapping?)arrayMapping?.ElementTypeMapping
            ?? _typeMappingSource.FindMapping(itemExpression.Type, Dependencies.Model);

        if (itemMapping is null)
        {
            throw new InvalidOperationException("Couldn't find element type mapping when applying item/array mappings");
        }

        // If the array's type mapping isn't provided (parameter/constant), attempt to infer it from the item.
        if (arrayMapping is null)
        {
            if (itemMapping.Converter is not null)
            {
                // If the item mapping has a value converter, construct an array mapping directly over it - this will build the
                // corresponding array type converter.
                arrayMapping = _typeMappingSource.FindMapping(arrayExpression.Type, Dependencies.Model, itemMapping);
            }
            else
            {
                // No value converter on the item mapping - just try to look up an array mapping based on the item type.
                // Special-case arrays of objects, not taking the array CLR type into account in the lookup (it would never succeed).
                // Note that we provide both the array CLR type *and* an array store type constructed from the element's store type.
                // If we use only the array CLR type, byte[] will yield bytea which we don't want.
                arrayMapping = arrayExpression.Type.TryGetSequenceType() == typeof(object)
                    ? _typeMappingSource.FindMapping(itemMapping.StoreType + "[]")
                    : _typeMappingSource.FindMapping(arrayExpression.Type, itemMapping.StoreType + "[]");
            }

            if (arrayMapping is null)
            {
                throw new InvalidOperationException("Couldn't find array type mapping when applying item/array mappings");
            }
        }

        return (ApplyTypeMapping(itemExpression, itemMapping), ApplyTypeMapping(arrayExpression, arrayMapping));
    }

    private SqlExpression ApplyTypeMappingOnArrayIndex(
        PgArrayIndexExpression pgArrayIndexExpression,
        RelationalTypeMapping? typeMapping)
    {
        // If a (non-null) type mapping is being applied, it's to the element being indexed.
        // Infer the array's mapping from that.
        var (_, array) = typeMapping is not null
            ? ApplyTypeMappingsOnItemAndArray(Constant(null, typeMapping.ClrType, typeMapping), pgArrayIndexExpression.Array)
            : (null, ApplyDefaultTypeMapping(pgArrayIndexExpression.Array));

        return new PgArrayIndexExpression(
            array,
            ApplyDefaultTypeMapping(pgArrayIndexExpression.Index),
            pgArrayIndexExpression.IsNullable,
            pgArrayIndexExpression.Type,
            // If the array has a type mapping (i.e. column), prefer that just like we prefer column mappings in general
            pgArrayIndexExpression.Array.TypeMapping is NpgsqlArrayTypeMapping arrayMapping
                ? arrayMapping.ElementTypeMapping
                : typeMapping
                ?? _typeMappingSource.FindMapping(pgArrayIndexExpression.Type, Dependencies.Model));
    }

    private SqlExpression ApplyTypeMappingOnArraySlice(
        PgArraySliceExpression slice,
        RelationalTypeMapping? typeMapping)
    {
        // If the slice operand has a type mapping, that bubbles up (slice is just a view over that). Otherwise apply the external type
        // mapping down. The bounds are always ints and don't participate in any inference.

        // If a (non-null) type mapping is being applied, it's to the element being indexed.
        // Infer the array's mapping from that.
        var array = ApplyTypeMapping(slice.Array, typeMapping);

        return new PgArraySliceExpression(
            array,
            slice.LowerBound is null ? null : ApplyDefaultTypeMapping(slice.LowerBound),
            slice.UpperBound is null ? null : ApplyDefaultTypeMapping(slice.UpperBound),
            slice.IsNullable,
            slice.Type,
            array.TypeMapping);
    }

    private SqlExpression ApplyTypeMappingOnILike(PgILikeExpression ilikeExpression)
    {
        var inferredTypeMapping = (ilikeExpression.EscapeChar is null
                ? ExpressionExtensions.InferTypeMapping(
                    ilikeExpression.Match, ilikeExpression.Pattern)
                : ExpressionExtensions.InferTypeMapping(
                    ilikeExpression.Match, ilikeExpression.Pattern,
                    ilikeExpression.EscapeChar))
            ?? _typeMappingSource.FindMapping(ilikeExpression.Match.Type, Dependencies.Model);

        return new PgILikeExpression(
            ApplyTypeMapping(ilikeExpression.Match, inferredTypeMapping),
            ApplyTypeMapping(ilikeExpression.Pattern, inferredTypeMapping),
            ApplyTypeMapping(ilikeExpression.EscapeChar, inferredTypeMapping),
            _boolTypeMapping);
    }

    private SqlExpression ApplyTypeMappingOnPostgresBinary(
        PgBinaryExpression pgBinaryExpression,
        RelationalTypeMapping? typeMapping)
    {
        var (left, right) = (pgBinaryExpression.Left, pgBinaryExpression.Right);

        Type resultType;
        RelationalTypeMapping? resultTypeMapping = null;
        RelationalTypeMapping? inferredTypeMapping;
        var operatorType = pgBinaryExpression.OperatorType;
        switch (operatorType)
        {
            case PgExpressionType.Overlaps:
            case PgExpressionType.Contains:
            case PgExpressionType.ContainedBy:
            case PgExpressionType.RangeIsStrictlyLeftOf:
            case PgExpressionType.RangeIsStrictlyRightOf:
            case PgExpressionType.RangeDoesNotExtendRightOf:
            case PgExpressionType.RangeDoesNotExtendLeftOf:
            case PgExpressionType.RangeIsAdjacentTo:
            {
                resultType = typeof(bool);
                resultTypeMapping = _boolTypeMapping;

                // Simple case: we have the same CLR type on both sides, or we have an array on either side
                // (e.g. overlap/intersect between two arrays); note that different CLR types may be mapped to arrays on the two sides
                // (e.g. int[] and List<int>)
                if (left.Type == right.Type
                    || left.TypeMapping is NpgsqlArrayTypeMapping
                    || right.TypeMapping is NpgsqlArrayTypeMapping)
                {
                    inferredTypeMapping = ExpressionExtensions.InferTypeMapping(left, right);
                    break;
                }

                // Multirange and range, cidr and ip - cases of different types where one contains the other.
                // We need fancier type mapping inference.
                SqlExpression newLeft, newRight;

                if (operatorType == PgExpressionType.ContainedBy)
                {
                    (newRight, newLeft) = InferContainmentMappings(right, left);
                }
                else
                {
                    (newLeft, newRight) = InferContainmentMappings(left, right);
                }

                return new PgBinaryExpression(operatorType, newLeft, newRight, resultType, resultTypeMapping);
            }

            case PgExpressionType.NetworkContainedByOrEqual:
            case PgExpressionType.NetworkContainsOrEqual:
            case PgExpressionType.NetworkContainsOrContainedBy:
            case PgExpressionType.TextSearchMatch:
            case PgExpressionType.JsonExists:
            case PgExpressionType.JsonExistsAny:
            case PgExpressionType.JsonExistsAll:
            case PgExpressionType.DictionaryContainsAnyKey:
            case PgExpressionType.DictionaryContainsAllKeys:
            case PgExpressionType.DictionaryContainsKey:
            {
                // TODO: For networking, this probably needs to be cleaned up, i.e. we know where the CIDR and INET are
                // based on operator type?
                return new PgBinaryExpression(
                    operatorType,
                    ApplyDefaultTypeMapping(left),
                    ApplyDefaultTypeMapping(right),
                    typeof(bool),
                    _boolTypeMapping);
            }

            case PgExpressionType.RangeUnion:
            case PgExpressionType.RangeIntersect:
            case PgExpressionType.RangeExcept:
            case PgExpressionType.TextSearchAnd:
            case PgExpressionType.TextSearchOr:
            {
                inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(left, right);
                resultType = inferredTypeMapping?.ClrType ?? left.Type;
                resultTypeMapping = inferredTypeMapping;
                break;
            }

            case PgExpressionType.Distance:
            {
                inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(left, right);

                resultType = inferredTypeMapping?.StoreTypeNameBase switch
                {
                    "geometry" or "geography" => typeof(double),

                    "date" => typeof(int),

                    "interval" when left.Type.FullName is "NodaTime.Period" or "NodaTime.Duration"
                        => _nodaTimePeriodType ??= left.Type.Assembly.GetType("NodaTime.Period")!,
                    "interval" => typeof(TimeSpan),

                    "timestamp" or "timestamptz" or "timestamp with time zone" or "timestamp without time zone"
                        when left.Type.FullName is "NodaTime.Instant" or "NodaTime.LocalDateTime" or "NodaTime.ZonedDateTime"
                        => _nodaTimePeriodType ??= left.Type.Assembly.GetType("NodaTime.Period")!,
                    "timestamp" or "timestamptz" or "timestamp with time zone" or "timestamp without time zone"
                        => typeof(TimeSpan),

                    null => throw new InvalidOperationException("No inferred type mapping for distance operator"),
                    _ => throw new InvalidOperationException(
                        $"PostgreSQL type '{inferredTypeMapping.StoreTypeNameBase}' isn't supported with the distance operator")
                };
                break;
            }

            case PgExpressionType.DictionaryValueForKey:
            case PgExpressionType.DictionarySubtract:
            case PgExpressionType.JsonValueForKeyAsText:
            case PgExpressionType.DictionaryConcat:
            {
                return new PgBinaryExpression(
                    operatorType,
                    ApplyDefaultTypeMapping(left),
                    ApplyDefaultTypeMapping(right),
                    typeMapping!.ClrType,
                    typeMapping);
            }
            default:
                throw new InvalidOperationException(
                    $"Incorrect {nameof(operatorType)} for {nameof(pgBinaryExpression)}: {operatorType}");
        }

        return new PgBinaryExpression(
            operatorType,
            ApplyTypeMapping(left, inferredTypeMapping),
            ApplyTypeMapping(right, inferredTypeMapping),
            resultType,
            resultTypeMapping ?? _typeMappingSource.FindMapping(resultType));

        (SqlExpression, SqlExpression) InferContainmentMappings(SqlExpression container, SqlExpression containee)
        {
            Debug.Assert(
                container.Type != containee.Type,
                "This method isn't meant for identical types, where type mapping inference is much simpler");

            // Attempt type inference either from the container or from the containee
            var containerMapping = container.TypeMapping;
            var containeeMapping = containee.TypeMapping;

            if (containeeMapping is null)
            {
                // If we couldn't find a type mapping on the containee, try inferring it from the container
                containeeMapping = containerMapping switch
                {
                    NpgsqlRangeTypeMapping rangeTypeMapping => rangeTypeMapping.SubtypeMapping,
                    NpgsqlMultirangeTypeMapping multirangeTypeMapping
                        => containee.Type.IsGenericType && containee.Type.GetGenericTypeDefinition() == typeof(NpgsqlRange<>)
                            ? multirangeTypeMapping.RangeMapping
                            : multirangeTypeMapping.SubtypeMapping,
                    _ => null
                };

                // Apply the inferred mapping to the containee, or fall back to the default type mapping
                if (containeeMapping is not null)
                {
                    containee = ApplyTypeMapping(containee, containeeMapping);
                }
                else
                {
                    containee = ApplyDefaultTypeMapping(containee);
                    containeeMapping = containee.TypeMapping;

                    if (containeeMapping is null)
                    {
                        throw new InvalidOperationException(
                            "Couldn't find containee type mapping when applying container/containee mappings");
                    }
                }
            }

            // If the container's type mapping isn't provided (parameter/constant), attempt to infer it from the item.
            if (containerMapping is null)
            {
                // TODO: FindContainerMapping currently works for range/multirange only, may want to extend it to other types
                // (e.g. IP address containment)
                containerMapping = _typeMappingSource.FindContainerMapping(container.Type, containeeMapping, Dependencies.Model);

                // Apply the inferred mapping to the container, or fall back to the default type mapping
                if (containerMapping is not null)
                {
                    container = ApplyTypeMapping(container, containerMapping);
                }
                else
                {
                    container = ApplyDefaultTypeMapping(container);

                    if (container.TypeMapping is null)
                    {
                        throw new InvalidOperationException(
                            "Couldn't find container type mapping when applying container/containee mappings");
                    }
                }
            }

            return (ApplyTypeMapping(container, containerMapping), ApplyTypeMapping(containee, containeeMapping));
        }
    }

    private SqlExpression ApplyTypeMappingOnNewArray(
        PgNewArrayExpression pgNewArrayExpression,
        RelationalTypeMapping? typeMapping)
    {
        var arrayTypeMapping = typeMapping as NpgsqlArrayTypeMapping;
        if (arrayTypeMapping is null && typeMapping is not null)
        {
            throw new ArgumentException($"Type mapping {typeMapping.GetType().Name} isn't an {nameof(NpgsqlArrayTypeMapping)}");
        }

        RelationalTypeMapping? elementTypeMapping = null;

        // First, loop over the expressions to infer the array's type mapping (if not provided), and to make
        // sure we don't have heterogeneous store types.
        foreach (var expression in pgNewArrayExpression.Expressions)
        {
            if (expression.TypeMapping is not { } expressionTypeMapping)
            {
                continue;
            }

            if (elementTypeMapping is null)
            {
                elementTypeMapping = expressionTypeMapping;
            }
            else if (expressionTypeMapping.StoreType != elementTypeMapping.StoreType)
            {
                // We have two heterogeneous store types in the array.
                // We allow this when they have the same base type but differing facets (e.g. varchar(10) and varchar(15)), in which case
                // we cast up. We also manually take care of some special cases (e.g. text and varchar(10) -> text).
                // This is a hacky solution until a full type compatibility chart is implemented
                // (https://github.com/dotnet/efcore/issues/15586)
                if (expressionTypeMapping.StoreTypeNameBase == elementTypeMapping.StoreTypeNameBase)
                {
                    if (expressionTypeMapping.Size is not null && elementTypeMapping.Size is not null)
                    {
                        var size = Math.Max(expressionTypeMapping.Size.Value, elementTypeMapping.Size.Value);

                        elementTypeMapping = _typeMappingSource.FindMapping($"{expressionTypeMapping.StoreTypeNameBase}({size})");
                    }
                    else if (expressionTypeMapping.Precision is not null
                             && elementTypeMapping.Precision is not null
                             && expressionTypeMapping.Scale is not null
                             && elementTypeMapping.Scale is not null)
                    {
                        var precision = Math.Max(expressionTypeMapping.Precision.Value, elementTypeMapping.Precision.Value);
                        var scale = Math.Max(expressionTypeMapping.Scale.Value, elementTypeMapping.Scale.Value);

                        elementTypeMapping =
                            _typeMappingSource.FindMapping($"{expressionTypeMapping.StoreTypeNameBase}({precision},{scale})");
                    }
                    else if (expressionTypeMapping.Precision is not null && elementTypeMapping.Precision is not null)
                    {
                        var precision = Math.Max(expressionTypeMapping.Precision.Value, elementTypeMapping.Precision.Value);

                        elementTypeMapping = _typeMappingSource.FindMapping($"{expressionTypeMapping.StoreTypeNameBase}({precision})");
                    }
                }
                else if (expressionTypeMapping.StoreType == "text" && IsTextualTypeMapping(elementTypeMapping))
                {
                    elementTypeMapping = expressionTypeMapping;
                }
                else if (elementTypeMapping.StoreType == "text" && IsTextualTypeMapping(expressionTypeMapping))
                {
                    // elementTypeMapping is already "text"
                }
                else
                {
                    throw new InvalidOperationException(
                        NpgsqlStrings.HeterogeneousTypesInNewArray(
                            elementTypeMapping.StoreType, expressionTypeMapping.StoreType));
                }

                static bool IsTextualTypeMapping(RelationalTypeMapping mapping)
                    => mapping.StoreTypeNameBase is "varchar" or "char" or "character varying" or "character" or "text";
            }
        }

        // None of the array's expressions had a type mapping (i.e. no columns, only parameters/constants)
        // Use the type mapping given externally
        if (elementTypeMapping is null)
        {
            // No type mapping could be inferred from the expressions, nor was one given from the outside -
            // we have no type mapping... Just return the original expression, which has no type mapping and will fail translation.
            if (arrayTypeMapping is null)
            {
                return pgNewArrayExpression;
            }

            elementTypeMapping = arrayTypeMapping.ElementTypeMapping;
        }
        else
        {
            // An element type mapping was successfully inferred from one of the expressions (there was a column).
            // Infer the array's type mapping from it.
            arrayTypeMapping = (NpgsqlArrayTypeMapping?)_typeMappingSource.FindMapping(
                pgNewArrayExpression.Type,
                elementTypeMapping.StoreType + "[]");

            // If the array's CLR type doesn't match the type mapping inferred from the element (e.g. CLR object[] with up-casted
            // elements). Just return the original expression, which has no type mapping and will fail translation.
            if (arrayTypeMapping is null)
            {
                return pgNewArrayExpression;
            }
        }

        // Now go over all expressions and apply the inferred element type mapping
        List<SqlExpression>? newExpressions = null;
        for (var i = 0; i < pgNewArrayExpression.Expressions.Count; i++)
        {
            var expression = pgNewArrayExpression.Expressions[i];
            var newExpression = ApplyTypeMapping(expression, elementTypeMapping);
            if (newExpression != expression && newExpressions is null)
            {
                newExpressions = [];
                for (var j = 0; j < i; j++)
                {
                    newExpressions.Add(pgNewArrayExpression.Expressions[j]);
                }
            }

            newExpressions?.Add(newExpression);
        }

        return new PgNewArrayExpression(
            newExpressions ?? pgNewArrayExpression.Expressions,
            pgNewArrayExpression.Type, arrayTypeMapping);
    }

    /// <summary>
    ///     PostgreSQL array indexing is 1-based. If the index happens to be a constant,
    ///     just increment it. Otherwise, append a +1 in the SQL.
    /// </summary>
    public virtual SqlExpression GenerateOneBasedIndexExpression(SqlExpression expression)
        => expression is SqlConstantExpression constant
            ? Constant(System.Convert.ToInt32(constant.Value) + 1, constant.TypeMapping)
            : Add(expression, Constant(1));
}
