using NpgsqlTypes;
using System.Text;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

/// <summary>
/// The type mapping for the PostgreSQL pg_lsn type.
/// </summary>
/// <remarks>
/// See: https://www.postgresql.org/docs/current/datatype-pg-lsn.html
/// </remarks>
public class NpgsqlPgLsnTypeMapping : NpgsqlTypeMapping
{
    /// <summary>
    /// Initializes  a new instance of the <see cref="NpgsqlPgLsnTypeMapping"/> class.
    /// </summary>
    public NpgsqlPgLsnTypeMapping()
        : base("pg_lsn", typeof(NpgsqlLogSequenceNumber), NpgsqlDbType.PgLsn) {}

    protected NpgsqlPgLsnTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.PgLsn) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlPgLsnTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var lsn = (NpgsqlLogSequenceNumber)value;
        var builder = new StringBuilder("PG_LSN '")
                .Append(lsn.ToString())
                .Append('\'');
        return builder.ToString();
    }

    public override Expression GenerateCodeLiteral(object value)
    {
        var lsn = (NpgsqlLogSequenceNumber)value;
        return Expression.New(Constructor, Expression.Constant((ulong)lsn));
    }

    private static readonly ConstructorInfo Constructor =
        typeof(NpgsqlLogSequenceNumber).GetConstructor(new[] { typeof(ulong) })!;
}
