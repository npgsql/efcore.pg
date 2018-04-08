using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    // TODO: See #357. We should be able to simply use StringTypeMapping but DbParameter.Size isn't managed properly.
    public class NpgsqlStringTypeMapping : StringTypeMapping
    {
        public NpgsqlStringTypeMapping([NotNull] string storeType, bool unicode = false, int? size = null)
            : base(storeType, System.Data.DbType.String, unicode, size) {}

        protected NpgsqlStringTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters) {}

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new NpgsqlStringTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new NpgsqlStringTypeMapping(Parameters.WithComposedConverter(converter));

        protected override void ConfigureParameter(DbParameter parameter)
        {
            // See #357
            if (Size.HasValue)
            {
                var value = parameter.Value;
                var length = (value as string)?.Length;
                var size = Size.Value;

                parameter.Size = value == null || value == DBNull.Value || length != null && length <= size
                    ? size
                    : -1;
            }
        }
    }
}
