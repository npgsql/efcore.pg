using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// The base class for mapping Npgsql-specific types. It configures parameters with the
    /// <see cref="NpgsqlDbType"/> provider-specific type enum.
    /// </summary>
    public abstract class NpgsqlTypeMapping : RelationalTypeMapping
    {
        /// <summary>
        /// The database type used by Npgsql.
        /// </summary>
        public virtual NpgsqlDbType NpgsqlDbType { get; }

        // ReSharper disable once PublicConstructorInAbstractClass
        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlTypeMapping"/> class.
        /// </summary>
        /// <param name="storeType">The database type to map.</param>
        /// <param name="clrType">The CLR type to map.</param>
        /// <param name="npgsqlDbType">The database type used by Npgsql.</param>
        public NpgsqlTypeMapping([NotNull] string storeType, [NotNull] Type clrType, NpgsqlDbType npgsqlDbType)
            : base(storeType, clrType)
            => NpgsqlDbType = npgsqlDbType;

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlTypeMapping"/> class.
        /// </summary>
        /// <param name="parameters">The parameters for this mapping.</param>
        /// <param name="npgsqlDbType">The database type of the range subtype.</param>
        protected NpgsqlTypeMapping(RelationalTypeMappingParameters parameters, NpgsqlDbType npgsqlDbType)
            : base(parameters)
            => NpgsqlDbType = npgsqlDbType;

        protected override void ConfigureParameter(DbParameter parameter)
        {
            var npgsqlParameter = parameter as NpgsqlParameter;
            if (npgsqlParameter == null)
                throw new ArgumentException($"Npgsql-specific type mapping {GetType().Name} being used with non-Npgsql parameter type {parameter.GetType().Name}");

            base.ConfigureParameter(parameter);

            npgsqlParameter.NpgsqlDbType = NpgsqlDbType;
        }
    }
}
