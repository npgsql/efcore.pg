namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

public class NpgsqlRegdictionaryTypeMapping : NpgsqlTypeMapping
{
    public NpgsqlRegdictionaryTypeMapping() : base("regdictionary", typeof(uint), NpgsqlDbType.Oid) { }

    protected NpgsqlRegdictionaryTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, NpgsqlDbType.Oid) { }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new NpgsqlRegdictionaryTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{EscapeSqlLiteral((string)value)}'";

    private string EscapeSqlLiteral(string literal)
        => Check.NotNull(literal, nameof(literal)).Replace("'", "''");
}