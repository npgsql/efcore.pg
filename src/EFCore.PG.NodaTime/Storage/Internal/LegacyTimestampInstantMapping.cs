using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using NodaTime.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal
{
    // Should only be used only with EnableLegacyTimestampBehavior.
    // However, when upgrading to 6.0 with existing migrations, model snapshots still contain old mappings (Instant mapped to timestamp),
    // and EF Core's model differ expects type mappings to be found for these. See https://github.com/dotnet/efcore/issues/26168.
    public class LegacyTimestampInstantMapping : NpgsqlTypeMapping
    {
        public LegacyTimestampInstantMapping()
            : base("timestamp without time zone", typeof(Instant), NpgsqlDbType.Timestamp)
        {
        }

        protected LegacyTimestampInstantMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Timestamp) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new LegacyTimestampInstantMapping(parameters);

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new LegacyTimestampInstantMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter? converter)
            => new LegacyTimestampInstantMapping(Parameters.WithComposedConverter(converter));

        protected override string GenerateNonNullSqlLiteral(object value)
            => $"TIMESTAMP '{GenerateLiteralCore(value)}'";

        protected override string GenerateEmbeddedNonNullSqlLiteral(object value)
            => $@"""{GenerateLiteralCore(value)}""";

        private string GenerateLiteralCore(object value)
            => InstantPattern.ExtendedIso.Format((Instant)value);

        public override Expression GenerateCodeLiteral(object value)
            => TimestampTzInstantMapping.GenerateCodeLiteral((Instant)value);
    }
}
