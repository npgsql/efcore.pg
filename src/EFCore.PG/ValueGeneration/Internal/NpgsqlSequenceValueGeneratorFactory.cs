using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class NpgsqlSequenceValueGeneratorFactory : INpgsqlSequenceValueGeneratorFactory
    {
        readonly IUpdateSqlGenerator _sqlGenerator;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NpgsqlSequenceValueGeneratorFactory(
            [NotNull] IUpdateSqlGenerator sqlGenerator)
        {
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            _sqlGenerator = sqlGenerator;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ValueGenerator Create(
            IProperty property,
            NpgsqlSequenceValueGeneratorState generatorState,
            INpgsqlRelationalConnection connection,
            IRawSqlCommandBuilder rawSqlCommandBuilder,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
        {
            var type = property.ClrType.UnwrapNullableType().UnwrapEnumType();

            if (type == typeof(long))
            {
                return new NpgsqlSequenceHiLoValueGenerator<long>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
            }

            if (type == typeof(int))
            {
                return new NpgsqlSequenceHiLoValueGenerator<int>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
            }

            if (type == typeof(short))
            {
                return new NpgsqlSequenceHiLoValueGenerator<short>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
            }

            if (type == typeof(byte))
            {
                return new NpgsqlSequenceHiLoValueGenerator<byte>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
            }

            if (type == typeof(char))
            {
                return new NpgsqlSequenceHiLoValueGenerator<char>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
            }

            if (type == typeof(ulong))
            {
                return new NpgsqlSequenceHiLoValueGenerator<ulong>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
            }

            if (type == typeof(uint))
            {
                return new NpgsqlSequenceHiLoValueGenerator<uint>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
            }

            if (type == typeof(ushort))
            {
                return new NpgsqlSequenceHiLoValueGenerator<ushort>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
            }

            if (type == typeof(sbyte))
            {
                return new NpgsqlSequenceHiLoValueGenerator<sbyte>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
            }

            throw new ArgumentException(CoreStrings.InvalidValueGeneratorFactoryProperty(
                nameof(NpgsqlSequenceValueGeneratorFactory), property.Name, property.DeclaringEntityType.DisplayName()));
        }
    }
}
