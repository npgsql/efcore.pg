using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class NpgsqlSequenceHiLoValueGenerator<TValue> : HiLoValueGenerator<TValue>
    {
        readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
        readonly IUpdateSqlGenerator _sqlGenerator;
        readonly INpgsqlRelationalConnection _connection;
        readonly ISequence _sequence;
        readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandLogger;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NpgsqlSequenceHiLoValueGenerator(
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder,
            [NotNull] IUpdateSqlGenerator sqlGenerator,
            [NotNull] NpgsqlSequenceValueGeneratorState generatorState,
            [NotNull] INpgsqlRelationalConnection connection,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
            : base(generatorState)
        {
            _sequence = generatorState.Sequence;
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
            _sqlGenerator = sqlGenerator;
            _connection = connection;
            _commandLogger = commandLogger;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override long GetNewLowValue()
            => (long)Convert.ChangeType(
                _rawSqlCommandBuilder
                    .Build(_sqlGenerator.GenerateNextSequenceValueOperation(_sequence.Name, _sequence.Schema))
                    .ExecuteScalar(
                        new RelationalCommandParameterObject(
                            _connection,
                            parameterValues: null,
                            readerColumns: null,
                            context: null,
                            _commandLogger)),
                typeof(long),
                CultureInfo.InvariantCulture);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override async Task<long> GetNewLowValueAsync(CancellationToken cancellationToken = default(CancellationToken))
            => (long)Convert.ChangeType(
                await _rawSqlCommandBuilder
                    .Build(_sqlGenerator.GenerateNextSequenceValueOperation(_sequence.Name, _sequence.Schema))
                    .ExecuteScalarAsync(
                        new RelationalCommandParameterObject(
                            _connection,
                            parameterValues: null,
                            readerColumns: null,
                            context: null,
                            _commandLogger),
                        cancellationToken),
                typeof(long),
                CultureInfo.InvariantCulture);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool GeneratesTemporaryValues => false;
    }
}
