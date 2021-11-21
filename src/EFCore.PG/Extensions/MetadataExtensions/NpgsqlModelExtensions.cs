using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public static class NpgsqlModelExtensions
{
    public const string DefaultHiLoSequenceName = "EntityFrameworkHiLoSequence";

    #region HiLo

    /// <summary>
    ///     Returns the name to use for the default hi-lo sequence.
    /// </summary>
    /// <param name="model"> The model. </param>
    /// <returns> The name to use for the default hi-lo sequence. </returns>
    public static string GetHiLoSequenceName(this IReadOnlyModel model)
        => (string?)model[NpgsqlAnnotationNames.HiLoSequenceName]
            ?? DefaultHiLoSequenceName;

    /// <summary>
    ///     Sets the name to use for the default hi-lo sequence.
    /// </summary>
    /// <param name="model"> The model. </param>
    /// <param name="name"> The value to set. </param>
    public static void SetHiLoSequenceName(this IMutableModel model, string? name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.HiLoSequenceName, name);
    }

    /// <summary>
    ///     Sets the name to use for the default hi-lo sequence.
    /// </summary>
    /// <param name="model"> The model. </param>
    /// <param name="name"> The value to set. </param>
    /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
    public static string? SetHiLoSequenceName(
        this IConventionModel model, string? name, bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(name, nameof(name));

        model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.HiLoSequenceName, name, fromDataAnnotation);

        return name;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the default hi-lo sequence name.
    /// </summary>
    /// <param name="model"> The model. </param>
    /// <returns> The <see cref="ConfigurationSource" /> for the default hi-lo sequence name. </returns>
    public static ConfigurationSource? GetHiLoSequenceNameConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceName)?.GetConfigurationSource();

    /// <summary>
    ///     Returns the schema to use for the default hi-lo sequence.
    ///     <see cref="NpgsqlPropertyBuilderExtensions.UseHiLo" />
    /// </summary>
    /// <param name="model"> The model. </param>
    /// <returns> The schema to use for the default hi-lo sequence. </returns>
    public static string? GetHiLoSequenceSchema(this IReadOnlyModel model)
        => (string?)model[NpgsqlAnnotationNames.HiLoSequenceSchema];

    /// <summary>
    ///     Sets the schema to use for the default hi-lo sequence.
    /// </summary>
    /// <param name="model"> The model. </param>
    /// <param name="value"> The value to set. </param>
    public static void SetHiLoSequenceSchema(this IMutableModel model, string? value)
    {
        Check.NullButNotEmpty(value, nameof(value));

        model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema, value);
    }

    /// <summary>
    ///     Sets the schema to use for the default hi-lo sequence.
    /// </summary>
    /// <param name="model"> The model. </param>
    /// <param name="value"> The value to set. </param>
    /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
    public static string? SetHiLoSequenceSchema(
        this IConventionModel model, string? value, bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(value, nameof(value));

        model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema, value, fromDataAnnotation);

        return value;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the default hi-lo sequence schema.
    /// </summary>
    /// <param name="model"> The model. </param>
    /// <returns> The <see cref="ConfigurationSource" /> for the default hi-lo sequence schema. </returns>
    public static ConfigurationSource? GetHiLoSequenceSchemaConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema)?.GetConfigurationSource();

    #endregion

    #region Value Generation Strategy

    /// <summary>
    ///     Returns the <see cref="NpgsqlValueGenerationStrategy" /> to use for properties
    ///     of keys in the model, unless the property has a strategy explicitly set.
    /// </summary>
    /// <param name="model"> The model. </param>
    /// <returns> The default <see cref="NpgsqlValueGenerationStrategy" />. </returns>
    public static NpgsqlValueGenerationStrategy? GetValueGenerationStrategy(this IReadOnlyModel model)
        => (NpgsqlValueGenerationStrategy?)model[NpgsqlAnnotationNames.ValueGenerationStrategy];

    /// <summary>
    ///     Attempts to set the <see cref="NpgsqlValueGenerationStrategy" /> to use for properties
    ///     of keys in the model that don't have a strategy explicitly set.
    /// </summary>
    /// <param name="model"> The model. </param>
    /// <param name="value"> The value to set. </param>
    public static void SetValueGenerationStrategy(this IMutableModel model, NpgsqlValueGenerationStrategy? value)
        => model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy, value);

    /// <summary>
    ///     Attempts to set the <see cref="NpgsqlValueGenerationStrategy" /> to use for properties
    ///     of keys in the model that don't have a strategy explicitly set.
    /// </summary>
    /// <param name="model"> The model. </param>
    /// <param name="value"> The value to set. </param>
    /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
    public static NpgsqlValueGenerationStrategy? SetValueGenerationStrategy(
        this IConventionModel model,
        NpgsqlValueGenerationStrategy? value,
        bool fromDataAnnotation = false)
    {
        model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy, value, fromDataAnnotation);

        return value;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the default <see cref="NpgsqlValueGenerationStrategy" />.
    /// </summary>
    /// <param name="model"> The model. </param>
    /// <returns> The <see cref="ConfigurationSource" /> for the default <see cref="NpgsqlValueGenerationStrategy" />. </returns>
    public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();

    #endregion

    #region PostgreSQL Extensions

    public static PostgresExtension GetOrAddPostgresExtension(
        this IMutableModel model,
        string? schema,
        string name,
        string? version)
        => PostgresExtension.GetOrAddPostgresExtension(model, schema, name, version);

    public static IReadOnlyList<PostgresExtension> GetPostgresExtensions(this IReadOnlyModel model)
        => PostgresExtension.GetPostgresExtensions(model).ToArray();

    #endregion

    #region Enum types

    public static PostgresEnum GetOrAddPostgresEnum(
        this IMutableModel model,
        string? schema,
        string name,
        string[] labels)
        => PostgresEnum.GetOrAddPostgresEnum(model, schema, name, labels);

    public static IReadOnlyList<PostgresEnum> GetPostgresEnums(this IReadOnlyModel model)
        => PostgresEnum.GetPostgresEnums(model).ToArray();

    #endregion Enum types

    #region Range types

    public static PostgresRange GetOrAddPostgresRange(
        this IMutableModel model,
        string? schema,
        string name,
        string subtype,
        string? canonicalFunction = null,
        string? subtypeOpClass = null,
        string? collation = null,
        string? subtypeDiff = null)
        => PostgresRange.GetOrAddPostgresRange(
            model,
            schema,
            name,
            subtype,
            canonicalFunction,
            subtypeOpClass,
            collation,
            subtypeDiff);

    public static IReadOnlyList<PostgresRange> PostgresRanges(this IReadOnlyModel model)
        => PostgresRange.GetPostgresRanges(model).ToArray();

    #endregion Range types

    #region Database Template

    public static string? GetDatabaseTemplate(this IReadOnlyModel model)
        => (string?)model[NpgsqlAnnotationNames.DatabaseTemplate];

    public static void SetDatabaseTemplate(this IMutableModel model, string? template)
        => model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.DatabaseTemplate, template);

    public static string? SetDatabaseTemplate(
        this IConventionModel model,
        string? template,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(template, nameof(template));

        model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.DatabaseTemplate, template, fromDataAnnotation);

        return template;
    }

    public static ConfigurationSource? GetDatabaseTemplateConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(NpgsqlAnnotationNames.DatabaseTemplate)?.GetConfigurationSource();

    #endregion

    #region Tablespace

    public static string? GetTablespace(this IReadOnlyModel model)
        => (string?)model[NpgsqlAnnotationNames.Tablespace];

    public static void SetTablespace(this IMutableModel model, string? tablespace)
        => model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.Tablespace, tablespace);

    public static string? SetTablespace(
        this IConventionModel model,
        string? tablespace,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(tablespace, nameof(tablespace));

        model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.Tablespace, tablespace, fromDataAnnotation);

        return tablespace;
    }

    public static ConfigurationSource? GetTablespaceConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(NpgsqlAnnotationNames.Tablespace)?.GetConfigurationSource();

    #endregion

    #region Collation management

    public static PostgresCollation GetOrAddCollation(
        this IMutableModel model,
        string? schema,
        string name,
        string lcCollate,
        string lcCtype,
        string? provider = null,
        bool? deterministic = null)
        => PostgresCollation.GetOrAddCollation(
            model,
            schema,
            name,
            lcCollate,
            lcCtype,
            provider,
            deterministic);

    public static IReadOnlyList<PostgresCollation> GetCollations(this IReadOnlyModel model)
        => PostgresCollation.GetCollations(model).ToArray();

    #endregion Collation management

    #region Default column collation

    /// <summary>
    /// Gets the default collation for all columns in the database, or <see langword="null" /> if none is defined.
    /// This causes EF Core to specify an explicit collation when creating all column, unless one is overridden
    /// on a column.
    /// </summary>
    /// <remarks>
    /// <p>
    /// See <see cref="RelationalModelExtensions.GetCollation" /> for another approach to defining a
    /// database-wide collation.
    /// </p>
    /// <p>
    /// For more information, see https://www.postgresql.org/docs/current/collation.html.
    /// </p>
    /// </remarks>
    public static string? GetDefaultColumnCollation(this IReadOnlyModel model)
        => (string?)model[NpgsqlAnnotationNames.DefaultColumnCollation];

    /// <summary>
    /// Sets the default collation for all columns in the database, or <c>null</c> if none is defined.
    /// This causes EF Core to specify an explicit collation when creating all column, unless one is overridden
    /// on a column.
    /// </summary>
    /// <remarks>
    /// <p>
    /// See <see cref="RelationalModelExtensions.GetCollation" /> for another approach to defining a
    /// database-wide collation.
    /// </p>
    /// <p>
    /// For more information, see https://www.postgresql.org/docs/current/collation.html.
    /// </p>
    /// </remarks>
    public static void SetDefaultColumnCollation(this IMutableModel model, string? collation)
        => model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.DefaultColumnCollation, collation);

    /// <summary>
    /// Sets the default collation for all columns in the database, or <c>null</c> if none is defined.
    /// This causes EF Core to specify an explicit collation when creating all column, unless one is overridden
    /// on a column.
    /// </summary>
    /// <remarks>
    /// <p>
    /// See <see cref="RelationalModelExtensions.SetCollation(Microsoft.EntityFrameworkCore.Metadata.IMutableModel,string)" />
    /// for another approach to defining a database-wide collation.
    /// </p>
    /// <p>
    /// For more information, see https://www.postgresql.org/docs/current/collation.html.
    /// </p>
    /// </remarks>
    public static string? SetDefaultColumnCollation(this IConventionModel model, string? collation, bool fromDataAnnotation = false)
    {
        model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.DefaultColumnCollation, collation, fromDataAnnotation);
        return collation;
    }

    /// <summary>
    /// Returns the <see cref="ConfigurationSource" /> for the default column collation.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the default column collation.</returns>
    public static ConfigurationSource? GetDefaultColumnCollationConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(NpgsqlAnnotationNames.DefaultColumnCollation)?.GetConfigurationSource();

    #endregion Default column collation
}