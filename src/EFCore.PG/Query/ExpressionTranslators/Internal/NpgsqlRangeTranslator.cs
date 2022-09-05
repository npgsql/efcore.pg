using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using static Npgsql.EntityFrameworkCore.PostgreSQL.Utilities.Statics;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlRangeTranslator : IMethodCallTranslator, IMemberTranslator
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;
    private readonly IModel _model;
    private readonly bool _supportsMultiranges;

    private static readonly MethodInfo EnumerableAnyWithoutPredicate =
        typeof(Enumerable).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(mi => mi.Name == nameof(Enumerable.Any) && mi.GetParameters().Length == 1);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlRangeTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        NpgsqlSqlExpressionFactory npgsqlSqlExpressionFactory,
        IModel model,
        INpgsqlSingletonOptions npgsqlSingletonOptions)
    {
        _typeMappingSource = typeMappingSource;
        _sqlExpressionFactory = npgsqlSqlExpressionFactory;
        _model = model;
        _supportsMultiranges = npgsqlSingletonOptions.PostgresVersionWithoutDefault is null
            || npgsqlSingletonOptions.PostgresVersionWithoutDefault.AtLeast(14);
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // Any() over multirange -> NOT isempty(). NpgsqlRange<T> has IsEmpty which is translated below.
        if (_supportsMultiranges
            && method.IsGenericMethod
            && method.GetGenericMethodDefinition() == EnumerableAnyWithoutPredicate
            && arguments[0].Type.TryGetMultirangeSubtype(out _))
        {
            return _sqlExpressionFactory.Not(
                _sqlExpressionFactory.Function(
                    "isempty",
                    new[] { arguments[0] },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(bool)));
        }

        if (method.DeclaringType != typeof(NpgsqlRangeDbFunctionsExtensions)
            && (method.DeclaringType != typeof(NpgsqlMultirangeDbFunctionsExtensions) || !_supportsMultiranges))
        {
            return null;
        }

        if (method.Name == nameof(NpgsqlRangeDbFunctionsExtensions.Merge))
        {
            if (method.DeclaringType == typeof(NpgsqlRangeDbFunctionsExtensions))
            {
                var inferredMapping = ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);

                return _sqlExpressionFactory.Function(
                    "range_merge",
                    new[] {
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[0], inferredMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[1], inferredMapping)
                    },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[2],
                    method.ReturnType,
                    inferredMapping);
            }

            if (method.DeclaringType == typeof(NpgsqlMultirangeDbFunctionsExtensions))
            {
                var returnTypeMapping = arguments[0].TypeMapping is NpgsqlMultirangeTypeMapping multirangeTypeMapping
                    ? multirangeTypeMapping.RangeMapping
                    : null;

                return _sqlExpressionFactory.Function(
                    "range_merge",
                    new[] { arguments[0] },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    method.ReturnType,
                    returnTypeMapping);
            }
        }

        return method.Name switch
        {
            nameof(NpgsqlRangeDbFunctionsExtensions.Contains)
                => _sqlExpressionFactory.Contains(arguments[0], arguments[1]),
            nameof(NpgsqlRangeDbFunctionsExtensions.ContainedBy)
                => _sqlExpressionFactory.ContainedBy(arguments[0], arguments[1]),
            nameof(NpgsqlRangeDbFunctionsExtensions.Overlaps)
                => _sqlExpressionFactory.Overlaps(arguments[0], arguments[1]),
            nameof(NpgsqlRangeDbFunctionsExtensions.IsStrictlyLeftOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIsStrictlyLeftOf, arguments[0], arguments[1]),
            nameof(NpgsqlRangeDbFunctionsExtensions.IsStrictlyRightOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIsStrictlyRightOf, arguments[0], arguments[1]),
            nameof(NpgsqlRangeDbFunctionsExtensions.DoesNotExtendRightOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeDoesNotExtendRightOf, arguments[0], arguments[1]),
            nameof(NpgsqlRangeDbFunctionsExtensions.DoesNotExtendLeftOf)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeDoesNotExtendLeftOf, arguments[0], arguments[1]),
            nameof(NpgsqlRangeDbFunctionsExtensions.IsAdjacentTo)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIsAdjacentTo, arguments[0], arguments[1]),
            nameof(NpgsqlRangeDbFunctionsExtensions.Union)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeUnion, arguments[0], arguments[1]),
            nameof(NpgsqlRangeDbFunctionsExtensions.Intersect)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeIntersect, arguments[0], arguments[1]),
            nameof(NpgsqlRangeDbFunctionsExtensions.Except)
                => _sqlExpressionFactory.MakePostgresBinary(PostgresExpressionType.RangeExcept, arguments[0], arguments[1]),

            _ => null
        };
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        var type = member.DeclaringType;
        if (type is null || !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(NpgsqlRange<>))
        {
            return null;
        }

        if (member.Name == nameof(NpgsqlRange<int>.LowerBound) || member.Name == nameof(NpgsqlRange<int>.UpperBound))
        {
            var typeMapping = instance!.TypeMapping is NpgsqlRangeTypeMapping rangeMapping
                ? rangeMapping.SubtypeMapping
                : _typeMappingSource.FindMapping(returnType, _model);

            return _sqlExpressionFactory.Function(
                member.Name == nameof(NpgsqlRange<int>.LowerBound) ? "lower" : "upper",
                new[] { instance },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                returnType,
                typeMapping);
        }

        return member.Name switch
        {
            nameof(NpgsqlRange<int>.IsEmpty)               => SingleArgBoolFunction("isempty", instance!),
            nameof(NpgsqlRange<int>.LowerBoundIsInclusive) => SingleArgBoolFunction("lower_inc", instance!),
            nameof(NpgsqlRange<int>.UpperBoundIsInclusive) => SingleArgBoolFunction("upper_inc", instance!),
            nameof(NpgsqlRange<int>.LowerBoundInfinite)    => SingleArgBoolFunction("lower_inf", instance!),
            nameof(NpgsqlRange<int>.UpperBoundInfinite)    => SingleArgBoolFunction("upper_inf", instance!),

            _ => null
        };

        SqlFunctionExpression SingleArgBoolFunction(string name, SqlExpression argument)
            => _sqlExpressionFactory.Function(
                name,
                new[] { argument },
                nullable: true,
                argumentsPropagateNullability: TrueArrays[1],
                typeof(bool));
    }
}
