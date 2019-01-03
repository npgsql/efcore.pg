using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public abstract class NpgsqlTypeMapping : RelationalTypeMapping
    {
        public NpgsqlDbType NpgsqlDbType { get; }

        // ReSharper disable once PublicConstructorInAbstractClass
        public NpgsqlTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            NpgsqlDbType npgsqlDbType)
            : base(storeType, clrType)
            => NpgsqlDbType = npgsqlDbType;

        protected NpgsqlTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters)
            => NpgsqlDbType = npgsqlDbType;

        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);

            ((NpgsqlParameter)parameter).NpgsqlDbType = NpgsqlDbType;
        }
    }
}
