using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlTsRankingNormalizationTypeMapping : IntTypeMapping
    {
        public NpgsqlTsRankingNormalizationTypeMapping() : base(new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(typeof(NpgsqlTsRankingNormalization), new EnumToNumberConverter<NpgsqlTsRankingNormalization, int>()),
            "integer")) {}

        protected NpgsqlTsRankingNormalizationTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlTsRankingNormalizationTypeMapping(parameters);
    }
}
