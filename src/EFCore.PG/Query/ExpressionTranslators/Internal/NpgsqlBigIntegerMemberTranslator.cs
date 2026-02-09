using System.Numerics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlBigIntegerMemberTranslator(NpgsqlSqlExpressionFactory sqlExpressionFactory) : IMemberTranslator
{
    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (member.DeclaringType != typeof(BigInteger))
        {
            return null;
        }

        return member.Name switch
        {
            nameof(BigInteger.IsZero)
                => sqlExpressionFactory.Equal(instance!, sqlExpressionFactory.Constant(BigInteger.Zero)),
            nameof(BigInteger.IsOne)
                => sqlExpressionFactory.Equal(instance!, sqlExpressionFactory.Constant(BigInteger.One)),
            nameof(BigInteger.IsEven)
                => sqlExpressionFactory.Equal(
                    sqlExpressionFactory.Modulo(instance!, sqlExpressionFactory.Constant(new BigInteger(2))),
                    sqlExpressionFactory.Constant(BigInteger.Zero)),
            _ => null
        };
    }
}
