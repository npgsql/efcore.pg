using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlTsRankingNormalizationTypeMapping : IntTypeMapping
    {
        public NpgsqlTsRankingNormalizationTypeMapping() : base(new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(typeof(NpgsqlTsRankingNormalization), new EnumToNumberConverter<NpgsqlTsRankingNormalization, int>()),
            "integer")) {}

        protected NpgsqlTsRankingNormalizationTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlTsRankingNormalizationTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlTsRankingNormalizationTypeMapping(Parameters.WithComposedConverter(converter));

    }
}
