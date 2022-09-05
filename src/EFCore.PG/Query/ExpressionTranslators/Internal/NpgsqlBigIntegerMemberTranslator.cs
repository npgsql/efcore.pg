using System.Numerics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlBigIntegerMemberTranslator : IMemberTranslator
{
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

    private static readonly MemberInfo IsZero = typeof(BigInteger).GetProperty(nameof(BigInteger.IsZero))!;
    private static readonly MemberInfo IsOne = typeof(BigInteger).GetProperty(nameof(BigInteger.IsOne))!;
    private static readonly MemberInfo IsEven = typeof(BigInteger).GetProperty(nameof(BigInteger.IsEven))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlBigIntegerMemberTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    /// <inheritdoc />
    public virtual SqlExpression? Translate(SqlExpression? instance, MemberInfo member, Type returnType, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (member.DeclaringType == typeof(BigInteger))
        {
            if (member == IsZero)
            {
                return _sqlExpressionFactory.Equal(instance!, _sqlExpressionFactory.Constant(BigInteger.Zero));
            }

            if (member == IsOne)
            {
                return _sqlExpressionFactory.Equal(instance!, _sqlExpressionFactory.Constant(BigInteger.One));
            }

            if (member == IsEven)
            {
                return _sqlExpressionFactory.Equal(
                    _sqlExpressionFactory.Modulo(instance!, _sqlExpressionFactory.Constant(new BigInteger(2))),
                    _sqlExpressionFactory.Constant(BigInteger.Zero));
            }
        }

        return null;
    }
}
