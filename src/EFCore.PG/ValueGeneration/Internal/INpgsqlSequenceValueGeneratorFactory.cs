using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public interface INpgsqlSequenceValueGeneratorFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        ValueGenerator Create(
            [NotNull] IProperty property,
            [NotNull] NpgsqlSequenceValueGeneratorState generatorState,
            [NotNull] INpgsqlRelationalConnection connection,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger);
    }
}
