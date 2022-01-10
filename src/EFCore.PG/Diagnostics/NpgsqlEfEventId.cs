// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///   <para>
///     Event IDs for PostgreSQL/Npgsql events that correspond to messages logged to an <see cref="ILogger" />
///     and events sent to a <see cref="DiagnosticSource" />.
///   </para>
///   <para>
///     These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
///     behavior of warnings.
///   </para>
/// </summary>
public static class NpgsqlEfEventId
{
    // Warning: These values must not change between releases.
    // Only add new values to the end of sections, never in the middle.
    // Try to use {Noun}{Verb} naming and be consistent with existing names.
    private enum Id
    {
        // Model validation events
        // Scaffolding events
        ColumnFound = CoreEventId.ProviderDesignBaseId,
        //ColumnNotNamedWarning,
        //ColumnSkipped,
        //ForeignKeyColumnMissingWarning,
        //ForeignKeyColumnNotNamedWarning,
        //ForeignKeyColumnsNotMappedWarning,
        //ForeignKeyNotNamedWarning,
        ForeignKeyReferencesMissingPrincipalTableWarning,
        //IndexColumnFound,
        //IndexColumnNotNamedWarning,
        //IndexColumnSkipped,
        //IndexColumnsNotMappedWarning,
        //IndexNotNamedWarning,
        //IndexTableMissingWarning,
        MissingSchemaWarning,
        MissingTableWarning,
        SequenceFound,
        //SequenceNotNamedWarning,
        TableFound,
        //TableSkipped,
        //ForeignKeyTableMissingWarning,
        PrimaryKeyFound,
        UniqueConstraintFound,
        IndexFound,
        ForeignKeyFound,
        ForeignKeyPrincipalColumnMissingWarning,
        EnumColumnSkippedWarning,
        ExpressionIndexSkippedWarning,
        UnsupportedColumnIndexSkippedWarning,
        UnsupportedConstraintIndexSkippedWarning,
        CollationFound
    }

    private static readonly string ScaffoldingPrefix = DbLoggerCategory.Scaffolding.Name + ".";
    private static EventId MakeScaffoldingId(Id id) => new((int)id, ScaffoldingPrefix + id);

    /// <summary>
    ///   <para>
    ///     A column was found.
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId ColumnFound = MakeScaffoldingId(Id.ColumnFound);

    /// <summary>
    ///   <para>
    ///     The database is missing a schema.
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId MissingSchemaWarning = MakeScaffoldingId(Id.MissingSchemaWarning);

    /// <summary>
    ///   <para>
    ///     A collation was found.
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId CollationFound = MakeScaffoldingId(Id.CollationFound);

    /// <summary>
    ///   <para>
    ///     The database is missing a table.
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId MissingTableWarning = MakeScaffoldingId(Id.MissingTableWarning);

    /// <summary>
    ///   <para>
    ///     A foreign key references a missing table at the principal end.
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId ForeignKeyReferencesMissingPrincipalTableWarning = MakeScaffoldingId(Id.ForeignKeyReferencesMissingPrincipalTableWarning);

    /// <summary>
    ///   <para>
    ///     A table was found.
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId TableFound = MakeScaffoldingId(Id.TableFound);

    /// <summary>
    ///   <para>
    ///     A sequence was found.
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId SequenceFound = MakeScaffoldingId(Id.SequenceFound);

    /// <summary>
    ///   <para>
    ///     A primary key was found.
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId PrimaryKeyFound = MakeScaffoldingId(Id.PrimaryKeyFound);

    /// <summary>
    ///   <para>
    ///     A unique constraint was found.
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId UniqueConstraintFound = MakeScaffoldingId(Id.UniqueConstraintFound);

    /// <summary>
    ///   <para>
    ///     An index was found.
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId IndexFound = MakeScaffoldingId(Id.IndexFound);

    /// <summary>
    ///   <para>
    ///     A foreign key was found.
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId ForeignKeyFound = MakeScaffoldingId(Id.ForeignKeyFound);

    /// <summary>
    ///   <para>
    ///     A principal column referenced by a foreign key was not found.
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId ForeignKeyPrincipalColumnMissingWarning = MakeScaffoldingId(Id.ForeignKeyPrincipalColumnMissingWarning);

    /// <summary>
    ///   <para>
    ///     Enum column cannot be scaffolded, define a CLR enum type and add the property manually.
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId EnumColumnSkippedWarning = MakeScaffoldingId(Id.EnumColumnSkippedWarning);

    /// <summary>
    ///   <para>
    ///     Expression index cannot be scaffolded, expression indices aren't supported and must be added via raw SQL in migrations.
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId ExpressionIndexSkippedWarning = MakeScaffoldingId(Id.ExpressionIndexSkippedWarning);

    /// <summary>
    ///   <para>
    ///     Index '{name}' on table {tableName} cannot be scaffolded because it includes a column that cannot be scaffolded (e.g. enum).
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId UnsupportedColumnIndexSkippedWarning = MakeScaffoldingId(Id.UnsupportedColumnIndexSkippedWarning);

    /// <summary>
    ///   <para>
    ///     Constraint '{name}' on table {tableName} cannot be scaffolded because it includes a column that cannot be scaffolded (e.g. enum).
    ///   </para>
    ///   <para>
    ///     This event is in the <see cref="DbLoggerCategory.Scaffolding" /> category.
    ///   </para>
    /// </summary>
    public static readonly EventId UnsupportedColumnConstraintSkippedWarning = MakeScaffoldingId(Id.UnsupportedConstraintIndexSkippedWarning);
}