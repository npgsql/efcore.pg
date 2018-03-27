using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlTsRankingNormalizationTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlTsRankingNormalizationTypeMapping() : base(
            "integer",
            typeof(NpgsqlTsRankingNormalization),
            new EnumToNumberConverter<NpgsqlTsRankingNormalization, int>(),
            null,
            null,
            NpgsqlDbType.Integer) { }
    }
}
