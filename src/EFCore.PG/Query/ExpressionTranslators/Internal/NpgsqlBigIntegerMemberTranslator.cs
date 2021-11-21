using System.Numerics;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

public class NpgsqlBigIntegerMemberTranslator : IMemberTranslator
{
    private readonly NpgsqlSqlExpressionFactory _sqlExpressionFactory;

    private static readonly MemberInfo IsZero = typeof(BigInteger).GetProperty(nameof(BigInteger.IsZero))!;
    private static readonly MemberInfo IsOne = typeof(BigInteger).GetProperty(nameof(BigInteger.IsOne))!;
    private static readonly MemberInfo IsEven = typeof(BigInteger).GetProperty(nameof(BigInteger.IsEven))!;

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