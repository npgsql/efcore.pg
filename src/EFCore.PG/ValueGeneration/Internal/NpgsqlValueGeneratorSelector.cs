using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NpgsqlValueGeneratorSelector : RelationalValueGeneratorSelector
{
    private readonly INpgsqlSequenceValueGeneratorFactory _sequenceFactory;
    private readonly INpgsqlRelationalConnection _connection;
    private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
    private readonly IRelationalCommandDiagnosticsLogger _commandLogger;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NpgsqlValueGeneratorSelector(
        ValueGeneratorSelectorDependencies dependencies,
        INpgsqlSequenceValueGeneratorFactory sequenceFactory,
        INpgsqlRelationalConnection connection,
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        IRelationalCommandDiagnosticsLogger commandLogger)
        : base(dependencies)
    {
        _sequenceFactory = sequenceFactory;
        _connection = connection;
        _rawSqlCommandBuilder = rawSqlCommandBuilder;
        _commandLogger = commandLogger;
    }

    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public new virtual INpgsqlValueGeneratorCache Cache
        => (INpgsqlValueGeneratorCache)base.Cache;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ValueGenerator Select(IProperty property, ITypeBase typeBase)
        => property.GetValueGeneratorFactory() is null
            && property.GetValueGenerationStrategy() == NpgsqlValueGenerationStrategy.SequenceHiLo
                ? _sequenceFactory.Create(
                    property,
                    Cache.GetOrAddSequenceState(property, _connection),
                    _connection,
                    _rawSqlCommandBuilder,
                    _commandLogger)
                : base.Select(property, typeBase);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ValueGenerator? FindForType(IProperty property, ITypeBase typeBase, Type clrType)
        => property.ClrType.UnwrapNullableType() == typeof(Guid)
            ? property.ValueGenerated == ValueGenerated.Never || property.GetDefaultValueSql() is not null
                ? new TemporaryGuidValueGenerator()
                : new GuidValueGenerator()
            : base.FindForType(property, typeBase, clrType);
}
