using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Property extension methods for Npgsql-specific metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>.
/// </remarks>
public static class NpgsqlPropertyExtensions
{
    #region Hi-lo

    /// <summary>
    /// Returns the name to use for the hi-lo sequence.
    /// </summary>
    /// <param name="property"> The property.</param>
    /// <returns>The name to use for the hi-lo sequence.</returns>
    public static string? GetHiLoSequenceName(this IReadOnlyProperty property)
        => (string?)property[NpgsqlAnnotationNames.HiLoSequenceName];

    /// <summary>
    ///     Returns the name to use for the hi-lo sequence.
    /// </summary>
    /// <param name="property"> The property. </param>
    /// <param name="storeObject"> The identifier of the store object. </param>
    /// <returns> The name to use for the hi-lo sequence. </returns>
    public static string? GetHiLoSequenceName(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
    {
        var annotation = property.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceName);
        if (annotation is not null)
        {
            return (string?)annotation.Value;
        }

        var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
        return sharedTableRootProperty is not null
            ? sharedTableRootProperty.GetHiLoSequenceName(storeObject)
            : null;
    }

    /// <summary>
    /// Sets the name to use for the hi-lo sequence.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="name">The sequence name to use.</param>
    public static void SetHiLoSequenceName(this IMutableProperty property, string? name)
        => property.SetOrRemoveAnnotation(
            NpgsqlAnnotationNames.HiLoSequenceName,
            Check.NullButNotEmpty(name, nameof(name)));

    /// <summary>
    /// Sets the name to use for the hi-lo sequence.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="name">The sequence name to use.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static string? SetHiLoSequenceName(
        this IConventionProperty property,
        string? name,
        bool fromDataAnnotation = false)
    {
        property.SetOrRemoveAnnotation(
            NpgsqlAnnotationNames.HiLoSequenceName,
            Check.NullButNotEmpty(name, nameof(name)),
            fromDataAnnotation);

        return name;
    }

    /// <summary>
    /// Returns the <see cref="ConfigurationSource" /> for the hi-lo sequence name.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the hi-lo sequence name.</returns>
    public static ConfigurationSource? GetHiLoSequenceNameConfigurationSource(this IConventionProperty property)
        => property.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceName)?.GetConfigurationSource();

    /// <summary>
    /// Returns the schema to use for the hi-lo sequence.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The schema to use for the hi-lo sequence.</returns>
    public static string? GetHiLoSequenceSchema(this IReadOnlyProperty property)
        => (string?)property[NpgsqlAnnotationNames.HiLoSequenceSchema];

    /// <summary>
    ///     Returns the schema to use for the hi-lo sequence.
    /// </summary>
    /// <param name="property"> The property. </param>
    /// <param name="storeObject"> The identifier of the store object. </param>
    /// <returns> The schema to use for the hi-lo sequence. </returns>
    public static string? GetHiLoSequenceSchema(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
    {
        var annotation = property.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema);
        if (annotation is not null)
        {
            return (string?)annotation.Value;
        }

        var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
        return sharedTableRootProperty is not null
            ? sharedTableRootProperty.GetHiLoSequenceSchema(storeObject)
            : null;
    }

    /// <summary>
    /// Sets the schema to use for the hi-lo sequence.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="schema">The schema to use.</param>
    public static void SetHiLoSequenceSchema(this IMutableProperty property, string? schema)
        => property.SetOrRemoveAnnotation(
            NpgsqlAnnotationNames.HiLoSequenceSchema,
            Check.NullButNotEmpty(schema, nameof(schema)));

    /// <summary>
    /// Sets the schema to use for the hi-lo sequence.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="schema">The schema to use.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static string? SetHiLoSequenceSchema(
        this IConventionProperty property, string? schema, bool fromDataAnnotation = false)
    {
        property.SetOrRemoveAnnotation(
            NpgsqlAnnotationNames.HiLoSequenceSchema,
            Check.NullButNotEmpty(schema, nameof(schema)),
            fromDataAnnotation);

        return schema;
    }

    /// <summary>
    /// Returns the <see cref="ConfigurationSource" /> for the hi-lo sequence schema.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the hi-lo sequence schema.</returns>
    public static ConfigurationSource? GetHiLoSequenceSchemaConfigurationSource(this IConventionProperty property)
        => property.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema)?.GetConfigurationSource();

    /// <summary>
    /// Finds the <see cref="ISequence" /> in the model to use for the hi-lo pattern.
    /// </summary>
    /// <param name="property"> The property. </param>
    /// <returns> The sequence to use, or <see langword="null" /> if no sequence exists in the model. </returns>
    public static IReadOnlySequence? FindHiLoSequence(this IReadOnlyProperty property)
    {
        var model = property.DeclaringEntityType.Model;

        var sequenceName = property.GetHiLoSequenceName()
            ?? model.GetHiLoSequenceName();

        var sequenceSchema = property.GetHiLoSequenceSchema()
            ?? model.GetHiLoSequenceSchema();

        return model.FindSequence(sequenceName, sequenceSchema);
    }

    /// <summary>
    /// Finds the <see cref="ISequence" /> in the model to use for the hi-lo pattern.
    /// </summary>
    /// <param name="property"> The property. </param>
    /// <param name="storeObject"> The identifier of the store object. </param>
    /// <returns> The sequence to use, or <see langword="null" /> if no sequence exists in the model. </returns>
    public static IReadOnlySequence? FindHiLoSequence(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
    {
        var model = property.DeclaringEntityType.Model;

        var sequenceName = property.GetHiLoSequenceName(storeObject)
            ?? model.GetHiLoSequenceName();

        var sequenceSchema = property.GetHiLoSequenceSchema(storeObject)
            ?? model.GetHiLoSequenceSchema();

        return model.FindSequence(sequenceName, sequenceSchema);
    }

    /// <summary>
    ///     Finds the <see cref="ISequence" /> in the model to use for the hi-lo pattern.
    /// </summary>
    /// <param name="property"> The property. </param>
    /// <returns> The sequence to use, or <see langword="null" /> if no sequence exists in the model. </returns>
    public static ISequence? FindHiLoSequence(this IProperty property)
        => (ISequence?)((IReadOnlyProperty)property).FindHiLoSequence();

    /// <summary>
    ///     Finds the <see cref="ISequence" /> in the model to use for the hi-lo pattern.
    /// </summary>
    /// <param name="property"> The property. </param>
    /// <param name="storeObject"> The identifier of the store object. </param>
    /// <returns> The sequence to use, or <see langword="null" /> if no sequence exists in the model. </returns>
    public static ISequence? FindHiLoSequence(this IProperty property, in StoreObjectIdentifier storeObject)
        => (ISequence?)((IReadOnlyProperty)property).FindHiLoSequence(storeObject);

    /// <summary>
    /// Removes all identity sequence annotations from the property.
    /// </summary>
    public static void RemoveHiLoOptions(this IMutableProperty property)
    {
        property.SetHiLoSequenceName(null);
        property.SetHiLoSequenceSchema(null);
    }

    /// <summary>
    /// Removes all identity sequence annotations from the property.
    /// </summary>
    public static void RemoveHiLoOptions(this IConventionProperty property)
    {
        property.SetHiLoSequenceName(null);
        property.SetHiLoSequenceSchema(null);
    }

    #endregion Hi-lo

    #region Sequence

    /// <summary>
    ///     Returns the name to use for the key value generation sequence.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The name to use for the key value generation sequence.</returns>
    public static string? GetSequenceName(this IReadOnlyProperty property)
        => (string?)property[NpgsqlAnnotationNames.SequenceName];

    /// <summary>
    ///     Returns the name to use for the key value generation sequence.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The name to use for the key value generation sequence.</returns>
    public static string? GetSequenceName(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
    {
        var annotation = property.FindAnnotation(NpgsqlAnnotationNames.SequenceName);
        if (annotation != null)
        {
            return (string?)annotation.Value;
        }

        return property.FindSharedStoreObjectRootProperty(storeObject)?.GetSequenceName(storeObject);
    }

    /// <summary>
    ///     Sets the name to use for the key value generation sequence.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="name">The sequence name to use.</param>
    public static void SetSequenceName(this IMutableProperty property, string? name)
        => property.SetOrRemoveAnnotation(
            NpgsqlAnnotationNames.SequenceName,
            Check.NullButNotEmpty(name, nameof(name)));

    /// <summary>
    ///     Sets the name to use for the key value generation sequence.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="name">The sequence name to use.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetSequenceName(
        this IConventionProperty property,
        string? name,
        bool fromDataAnnotation = false)
    {
        property.SetOrRemoveAnnotation(
            NpgsqlAnnotationNames.SequenceName,
            Check.NullButNotEmpty(name, nameof(name)),
            fromDataAnnotation);

        return name;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the key value generation sequence name.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the key value generation sequence name.</returns>
    public static ConfigurationSource? GetSequenceNameConfigurationSource(this IConventionProperty property)
        => property.FindAnnotation(NpgsqlAnnotationNames.SequenceName)?.GetConfigurationSource();

    /// <summary>
    ///     Returns the schema to use for the key value generation sequence.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The schema to use for the key value generation sequence.</returns>
    public static string? GetSequenceSchema(this IReadOnlyProperty property)
        => (string?)property[NpgsqlAnnotationNames.SequenceSchema];

    /// <summary>
    ///     Returns the schema to use for the key value generation sequence.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The schema to use for the key value generation sequence.</returns>
    public static string? GetSequenceSchema(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
    {
        var annotation = property.FindAnnotation(NpgsqlAnnotationNames.SequenceSchema);
        if (annotation != null)
        {
            return (string?)annotation.Value;
        }

        return property.FindSharedStoreObjectRootProperty(storeObject)?.GetSequenceSchema(storeObject);
    }

    /// <summary>
    ///     Sets the schema to use for the key value generation sequence.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="schema">The schema to use.</param>
    public static void SetSequenceSchema(this IMutableProperty property, string? schema)
        => property.SetOrRemoveAnnotation(
            NpgsqlAnnotationNames.SequenceSchema,
            Check.NullButNotEmpty(schema, nameof(schema)));

    /// <summary>
    ///     Sets the schema to use for the key value generation sequence.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="schema">The schema to use.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetSequenceSchema(
        this IConventionProperty property,
        string? schema,
        bool fromDataAnnotation = false)
    {
        property.SetOrRemoveAnnotation(
            NpgsqlAnnotationNames.SequenceSchema,
            Check.NullButNotEmpty(schema, nameof(schema)),
            fromDataAnnotation);

        return schema;
    }

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the key value generation sequence schema.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the key value generation sequence schema.</returns>
    public static ConfigurationSource? GetSequenceSchemaConfigurationSource(this IConventionProperty property)
        => property.FindAnnotation(NpgsqlAnnotationNames.SequenceSchema)?.GetConfigurationSource();

    /// <summary>
    ///     Finds the <see cref="ISequence" /> in the model to use for the key value generation pattern.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The sequence to use, or <see langword="null" /> if no sequence exists in the model.</returns>
    public static IReadOnlySequence? FindSequence(this IReadOnlyProperty property)
    {
        var model = property.DeclaringEntityType.Model;

        var sequenceName = property.GetSequenceName()
            ?? model.GetSequenceNameSuffix();

        var sequenceSchema = property.GetSequenceSchema()
            ?? model.GetSequenceSchema();

        return model.FindSequence(sequenceName, sequenceSchema);
    }

    /// <summary>
    ///     Finds the <see cref="ISequence" /> in the model to use for the key value generation pattern.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The sequence to use, or <see langword="null" /> if no sequence exists in the model.</returns>
    public static IReadOnlySequence? FindSequence(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
    {
        var model = property.DeclaringEntityType.Model;

        var sequenceName = property.GetSequenceName(storeObject)
            ?? model.GetSequenceNameSuffix();

        var sequenceSchema = property.GetSequenceSchema(storeObject)
            ?? model.GetSequenceSchema();

        return model.FindSequence(sequenceName, sequenceSchema);
    }

    /// <summary>
    ///     Finds the <see cref="ISequence" /> in the model to use for the key value generation pattern.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The sequence to use, or <see langword="null" /> if no sequence exists in the model.</returns>
    public static ISequence? FindSequence(this IProperty property)
        => (ISequence?)((IReadOnlyProperty)property).FindSequence();

    /// <summary>
    ///     Finds the <see cref="ISequence" /> in the model to use for the key value generation pattern.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The sequence to use, or <see langword="null" /> if no sequence exists in the model.</returns>
    public static ISequence? FindSequence(this IProperty property, in StoreObjectIdentifier storeObject)
        => (ISequence?)((IReadOnlyProperty)property).FindSequence(storeObject);

    #endregion Sequence

    #region Value generation

    /// <summary>
    /// <para>Returns the <see cref="NpgsqlValueGenerationStrategy" /> to use for the property.</para>
    /// <para>
    /// If no strategy is set for the property, then the strategy to use will be taken from the <see cref="IModel" />.
    /// </para>
    /// </summary>
    /// <returns>The strategy, or <see cref="NpgsqlValueGenerationStrategy.None"/> if none was set.</returns>
    public static NpgsqlValueGenerationStrategy GetValueGenerationStrategy(this IReadOnlyProperty property)
    {
        var annotation = property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy);
        if (annotation != null)
        {
            return (NpgsqlValueGenerationStrategy?)annotation.Value ?? NpgsqlValueGenerationStrategy.None;
        }

        var defaultValueGenerationStrategy = GetDefaultValueGenerationStrategy(property);
        if (property.ValueGenerated != ValueGenerated.OnAdd
            || property.IsForeignKey()
            || property.TryGetDefaultValue(out _)
            || (defaultValueGenerationStrategy != NpgsqlValueGenerationStrategy.Sequence && property.GetDefaultValueSql() != null)
            || property.GetComputedColumnSql() is not null)
        {
            return NpgsqlValueGenerationStrategy.None;
        }

        return defaultValueGenerationStrategy;
    }

    /// <summary>
    /// <para>Returns the <see cref="NpgsqlValueGenerationStrategy" /> to use for the property.</para>
    /// <para>
    /// If no strategy is set for the property, then the strategy to use will be taken from the <see cref="IModel" />.
    /// </para>
    /// </summary>
    /// <param name="property"> The property. </param>
    /// <param name="storeObject"> The identifier of the store object. </param>
    /// <returns> The strategy, or <see cref="NpgsqlValueGenerationStrategy.None" /> if none was set. </returns>
    public static NpgsqlValueGenerationStrategy GetValueGenerationStrategy(
        this IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject)
        => GetValueGenerationStrategy(property, storeObject, null);

    internal static NpgsqlValueGenerationStrategy GetValueGenerationStrategy(
        this IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject,
        ITypeMappingSource? typeMappingSource)
    {
        var @override = property.FindOverrides(storeObject)?.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy);
        if (@override != null)
        {
            return (NpgsqlValueGenerationStrategy?)@override.Value ?? NpgsqlValueGenerationStrategy.None;
        }

        var annotation = property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy);
        if (annotation?.Value != null
            && StoreObjectIdentifier.Create(property.DeclaringEntityType, storeObject.StoreObjectType) == storeObject)
        {
            return (NpgsqlValueGenerationStrategy)annotation.Value;
        }

        var table = storeObject;
        var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
        if (sharedTableRootProperty != null)
        {
            return sharedTableRootProperty.GetValueGenerationStrategy(storeObject, typeMappingSource) is var npgsqlValueGenerationStrategy
                && npgsqlValueGenerationStrategy is
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    or NpgsqlValueGenerationStrategy.IdentityAlwaysColumn
                    or NpgsqlValueGenerationStrategy.SerialColumn
                && table.StoreObjectType == StoreObjectType.Table
                && !property.GetContainingForeignKeys().Any(
                    fk =>
                        !fk.IsBaseLinking()
                        || (StoreObjectIdentifier.Create(fk.PrincipalEntityType, StoreObjectType.Table)
                                is StoreObjectIdentifier principal
                            && fk.GetConstraintName(table, principal) != null))
                    ? npgsqlValueGenerationStrategy
                    : NpgsqlValueGenerationStrategy.None;
        }

        if (property.ValueGenerated != ValueGenerated.OnAdd
            || table.StoreObjectType != StoreObjectType.Table
            || property.TryGetDefaultValue(storeObject, out _)
            || property.GetDefaultValueSql(storeObject) != null
            || property.GetComputedColumnSql(storeObject) != null
            || property.GetContainingForeignKeys()
                .Any(fk =>
                    !fk.IsBaseLinking()
                    || (StoreObjectIdentifier.Create(fk.PrincipalEntityType, StoreObjectType.Table)
                        is StoreObjectIdentifier principal
                        && fk.GetConstraintName(table, principal) != null)))
        {
            return NpgsqlValueGenerationStrategy.None;
        }

        var defaultStrategy = GetDefaultValueGenerationStrategy(property, storeObject, typeMappingSource);
        if (defaultStrategy != NpgsqlValueGenerationStrategy.None)
        {
            if (annotation != null)
            {
                return (NpgsqlValueGenerationStrategy?)annotation.Value ?? NpgsqlValueGenerationStrategy.None;
            }
        }

        return defaultStrategy;
    }

    /// <summary>
    ///     Returns the <see cref="NpgsqlValueGenerationStrategy" /> to use for the property.
    /// </summary>
    /// <remarks>
    ///     If no strategy is set for the property, then the strategy to use will be taken from the <see cref="IModel" />.
    /// </remarks>
    /// <param name="overrides">The property overrides.</param>
    /// <returns>The strategy, or <see cref="NpgsqlValueGenerationStrategy.None" /> if none was set.</returns>
    public static NpgsqlValueGenerationStrategy? GetValueGenerationStrategy(
        this IReadOnlyRelationalPropertyOverrides overrides)
        => (NpgsqlValueGenerationStrategy?)overrides.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy)
            ?.Value;

    private static NpgsqlValueGenerationStrategy GetDefaultValueGenerationStrategy(IReadOnlyProperty property)
    {
        var modelStrategy = property.DeclaringEntityType.Model.GetValueGenerationStrategy();

        switch (modelStrategy)
        {
            case NpgsqlValueGenerationStrategy.SequenceHiLo:
            case NpgsqlValueGenerationStrategy.SerialColumn:
            case NpgsqlValueGenerationStrategy.Sequence:
            case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
            case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                return IsCompatibleWithValueGeneration(property)
                    ? modelStrategy.Value
                    : NpgsqlValueGenerationStrategy.None;
            case NpgsqlValueGenerationStrategy.None:
            case null:
                return NpgsqlValueGenerationStrategy.None;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static NpgsqlValueGenerationStrategy GetDefaultValueGenerationStrategy(
        IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject,
        ITypeMappingSource? typeMappingSource)
    {
        var modelStrategy = property.DeclaringEntityType.Model.GetValueGenerationStrategy();

        switch (modelStrategy)
        {
            case NpgsqlValueGenerationStrategy.SequenceHiLo:
                return IsCompatibleWithValueGeneration(property, storeObject, typeMappingSource)
                    ? modelStrategy.Value
                    : NpgsqlValueGenerationStrategy.None;

            case NpgsqlValueGenerationStrategy.SerialColumn:
            case NpgsqlValueGenerationStrategy.Sequence:
            case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
            case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                return !IsCompatibleWithValueGeneration(property, storeObject, typeMappingSource)
                    ? NpgsqlValueGenerationStrategy.None
                    : property.DeclaringEntityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy
                        ? NpgsqlValueGenerationStrategy.Sequence
                        : modelStrategy.Value;

            case NpgsqlValueGenerationStrategy.None:
            case null:
                return NpgsqlValueGenerationStrategy.None;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Sets the <see cref="NpgsqlValueGenerationStrategy" /> to use for the property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="value">The strategy to use.</param>
    public static void SetValueGenerationStrategy(
        this IMutableProperty property, NpgsqlValueGenerationStrategy? value)
    {
        CheckValueGenerationStrategy(property, value);

        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy, value);
    }

    /// <summary>
    /// Sets the <see cref="NpgsqlValueGenerationStrategy" /> to use for the property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="value">The strategy to use.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static NpgsqlValueGenerationStrategy? SetValueGenerationStrategy(
        this IConventionProperty property,
        NpgsqlValueGenerationStrategy? value,
        bool fromDataAnnotation = false)
    {
        CheckValueGenerationStrategy(property, value);

        return (NpgsqlValueGenerationStrategy?)property.SetOrRemoveAnnotation(
                NpgsqlAnnotationNames.ValueGenerationStrategy, value, fromDataAnnotation)
            ?.Value;
    }

    /// <summary>
    ///     Sets the <see cref="NpgsqlValueGenerationStrategy" /> to use for the property for a particular table.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="value">The strategy to use.</param>
    /// <param name="storeObject">The identifier of the table containing the column.</param>
    public static void SetValueGenerationStrategy(
        this IMutableProperty property,
        NpgsqlValueGenerationStrategy? value,
        in StoreObjectIdentifier storeObject)
        => property.GetOrCreateOverrides(storeObject)
            .SetValueGenerationStrategy(value);

        /// <summary>
    ///     Sets the <see cref="NpgsqlValueGenerationStrategy" /> to use for the property for a particular table.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="value">The strategy to use.</param>
    /// <param name="storeObject">The identifier of the table containing the column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static NpgsqlValueGenerationStrategy? SetValueGenerationStrategy(
        this IConventionProperty property,
        NpgsqlValueGenerationStrategy? value,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
        => property.GetOrCreateOverrides(storeObject, fromDataAnnotation)
            .SetValueGenerationStrategy(value, fromDataAnnotation);

    /// <summary>
    ///     Sets the <see cref="NpgsqlValueGenerationStrategy" /> to use for the property for a particular table.
    /// </summary>
    /// <param name="overrides">The property overrides.</param>
    /// <param name="value">The strategy to use.</param>
    public static void SetValueGenerationStrategy(
        this IMutableRelationalPropertyOverrides overrides,
        NpgsqlValueGenerationStrategy? value)
    {
        CheckValueGenerationStrategy(overrides.Property, value);

        overrides.SetOrRemoveAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy, value);
    }

    /// <summary>
    ///     Sets the <see cref="NpgsqlValueGenerationStrategy" /> to use for the property for a particular table.
    /// </summary>
    /// <param name="overrides">The property overrides.</param>
    /// <param name="value">The strategy to use.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static NpgsqlValueGenerationStrategy? SetValueGenerationStrategy(
        this IConventionRelationalPropertyOverrides overrides,
        NpgsqlValueGenerationStrategy? value,
        bool fromDataAnnotation = false)
    {
        CheckValueGenerationStrategy(overrides.Property, value);

        return (NpgsqlValueGenerationStrategy?)overrides.SetOrRemoveAnnotation(
                NpgsqlAnnotationNames.ValueGenerationStrategy, value, fromDataAnnotation)
            ?.Value;
    }

    private static void CheckValueGenerationStrategy(IReadOnlyProperty property, NpgsqlValueGenerationStrategy? value)
    {
        if (value is not null)
        {
            var propertyType = property.ClrType;

            if ((value is NpgsqlValueGenerationStrategy.IdentityAlwaysColumn or NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                && !IsCompatibleWithValueGeneration(property))
            {
                throw new ArgumentException(
                    NpgsqlStrings.IdentityBadType(
                        property.Name, property.DeclaringEntityType.DisplayName(), propertyType.ShortDisplayName()));
            }
            
            if (value is NpgsqlValueGenerationStrategy.SerialColumn or NpgsqlValueGenerationStrategy.SequenceHiLo
                && !IsCompatibleWithValueGeneration(property))
            {
                throw new ArgumentException(
                    NpgsqlStrings.SequenceBadType(
                        property.Name, property.DeclaringEntityType.DisplayName(), propertyType.ShortDisplayName()));
            }
        }
    }

    /// <summary>
    /// Returns the <see cref="ConfigurationSource" /> for the <see cref="NpgsqlValueGenerationStrategy" />.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the <see cref="NpgsqlValueGenerationStrategy" />.</returns>
    public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(
        this IConventionProperty property)
        => property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the <see cref="NpgsqlValueGenerationStrategy" /> for a particular table.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="storeObject">The identifier of the table containing the column.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the <see cref="NpgsqlValueGenerationStrategy" />.</returns>
    public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(
        this IConventionProperty property,
        in StoreObjectIdentifier storeObject)
        => property.FindOverrides(storeObject)?.GetValueGenerationStrategyConfigurationSource();

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the <see cref="NpgsqlValueGenerationStrategy" /> for a particular table.
    /// </summary>
    /// <param name="overrides">The property overrides.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the <see cref="NpgsqlValueGenerationStrategy" />.</returns>
    public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(
        this IConventionRelationalPropertyOverrides overrides)
        => overrides.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();

    /// <summary>
    /// Returns a value indicating whether the property is compatible with any <see cref="NpgsqlValueGenerationStrategy" />.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns><see langword="true" /> if compatible.</returns>
    public static bool IsCompatibleWithValueGeneration(IReadOnlyProperty property)
    {
        var valueConverter = property.GetValueConverter()
            ?? property.FindTypeMapping()?.Converter;

        var type = (valueConverter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();

        return type.IsInteger() || type.IsEnum;
    }

    private static bool IsCompatibleWithValueGeneration(
        IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject,
        ITypeMappingSource? typeMappingSource)
    {
        var valueConverter = property.GetValueConverter()
            ?? (property.FindRelationalTypeMapping(storeObject)
                ?? typeMappingSource?.FindMapping((IProperty)property))?.Converter;

        var type = (valueConverter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();

        return type.IsInteger() || type.IsEnum;
    }

    #endregion Value generation

    #region Identity sequence options

    /// <summary>
    /// Returns the identity start value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The identity start value.</returns>
    public static long? GetIdentityStartValue(this IReadOnlyProperty property)
        => IdentitySequenceOptionsData.Get(property).StartValue;

    /// <summary>
    /// Sets the identity start value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="startValue">The value to set.</param>
    public static void SetIdentityStartValue(this IMutableProperty property, long? startValue)
    {
        var options = IdentitySequenceOptionsData.Get(property);
        options.StartValue = startValue;
        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IdentityOptions, options.Serialize());
    }

    /// <summary>
    /// Sets the identity start value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="startValue">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static long? SetIdentityStartValue(
        this IConventionProperty property, long? startValue, bool fromDataAnnotation = false)
    {
        var options = IdentitySequenceOptionsData.Get(property);
        options.StartValue = startValue;
        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IdentityOptions, options.Serialize(), fromDataAnnotation);
        return startValue;
    }

    /// <summary>
    /// Returns the <see cref="ConfigurationSource" /> for the identity start value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the identity start value.</returns>
    public static ConfigurationSource? GetIdentityStartValueConfigurationSource(
        this IConventionProperty property)
        => property.FindAnnotation(NpgsqlAnnotationNames.IdentityOptions)?.GetConfigurationSource();

    /// <summary>
    /// Returns the identity increment value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The identity increment value.</returns>
    public static long? GetIdentityIncrementBy(this IReadOnlyProperty property)
        => IdentitySequenceOptionsData.Get(property).IncrementBy;

    /// <summary>
    /// Sets the identity increment value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="incrementBy">The value to set.</param>
    public static void SetIdentityIncrementBy(this IMutableProperty property, long? incrementBy)
    {
        var options = IdentitySequenceOptionsData.Get(property);
        options.IncrementBy = incrementBy ?? 1;
        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IdentityOptions, options.Serialize());
    }

    /// <summary>
    /// Sets the identity increment value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="incrementBy">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static long? SetIdentityIncrementBy(
        this IConventionProperty property, long? incrementBy, bool fromDataAnnotation = false)
    {
        var options = IdentitySequenceOptionsData.Get(property);
        options.IncrementBy = incrementBy ?? 1;
        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IdentityOptions, options.Serialize(), fromDataAnnotation);
        return incrementBy;
    }

    /// <summary>
    /// Returns the <see cref="ConfigurationSource" /> for the identity increment value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the identity increment value.</returns>
    public static ConfigurationSource? GetIdentityIncrementByConfigurationSource(
        this IConventionProperty property)
        => property.FindAnnotation(NpgsqlAnnotationNames.IdentityOptions)?.GetConfigurationSource();

    /// <summary>
    /// Returns the identity minimum value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The identity minimum value.</returns>
    public static long? GetIdentityMinValue(this IReadOnlyProperty property)
        => IdentitySequenceOptionsData.Get(property).MinValue;

    /// <summary>
    /// Sets the identity minimum value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="minValue">The value to set.</param>
    public static void SetIdentityMinValue(this IMutableProperty property, long? minValue)
    {
        var options = IdentitySequenceOptionsData.Get(property);
        options.MinValue = minValue;
        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IdentityOptions, options.Serialize());
    }

    /// <summary>
    /// Sets the identity minimum value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="minValue">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static long? SetIdentityMinValue(
        this IConventionProperty property, long? minValue, bool fromDataAnnotation = false)
    {
        var options = IdentitySequenceOptionsData.Get(property);
        options.MinValue = minValue;
        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IdentityOptions, options.Serialize(), fromDataAnnotation);
        return minValue;
    }

    /// <summary>
    /// Returns the <see cref="ConfigurationSource" /> for the identity minimum value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the identity minimum value.</returns>
    public static ConfigurationSource? GetIdentityMinValueConfigurationSource(
        this IConventionProperty property)
        => property.FindAnnotation(NpgsqlAnnotationNames.IdentityOptions)?.GetConfigurationSource();

    /// <summary>
    /// Returns the identity maximum value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The identity maximum value.</returns>
    public static long? GetIdentityMaxValue(this IReadOnlyProperty property)
        => IdentitySequenceOptionsData.Get(property).MaxValue;

    /// <summary>
    /// Sets the identity maximum value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="maxValue">The value to set.</param>
    public static void SetIdentityMaxValue(this IMutableProperty property, long? maxValue)
    {
        var options = IdentitySequenceOptionsData.Get(property);
        options.MaxValue = maxValue;
        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IdentityOptions, options.Serialize());
    }

    /// <summary>
    /// Sets the identity maximum value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="maxValue">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static long? SetIdentityMaxValue(
        this IConventionProperty property, long? maxValue, bool fromDataAnnotation = false)
    {
        var options = IdentitySequenceOptionsData.Get(property);
        options.MaxValue = maxValue;
        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IdentityOptions, options.Serialize(), fromDataAnnotation);
        return maxValue;
    }

    /// <summary>
    /// Returns the <see cref="ConfigurationSource" /> for the identity maximum value.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the identity maximum value.</returns>
    public static ConfigurationSource? GetIdentityMaxValueConfigurationSource(
        this IConventionProperty property)
        => property.FindAnnotation(NpgsqlAnnotationNames.IdentityOptions)?.GetConfigurationSource();

    /// <summary>
    /// Returns whether the identity's sequence is cyclic.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>Whether the identity's sequence is cyclic.</returns>
    public static bool? GetIdentityIsCyclic(this IReadOnlyProperty property)
        => IdentitySequenceOptionsData.Get(property).IsCyclic;

    /// <summary>
    /// Sets whether the identity's sequence is cyclic.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="cyclic">The value to set.</param>
    public static void SetIdentityIsCyclic(this IMutableProperty property, bool? cyclic)
    {
        var options = IdentitySequenceOptionsData.Get(property);
        options.IsCyclic = cyclic ?? false;
        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IdentityOptions, options.Serialize());
    }

    /// <summary>
    /// Sets whether the identity's sequence is cyclic.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="cyclic">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static bool? SetIdentityIsCyclic(
        this IConventionProperty property, bool? cyclic, bool fromDataAnnotation = false)
    {
        var options = IdentitySequenceOptionsData.Get(property);
        options.IsCyclic = cyclic ?? false;
        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IdentityOptions, options.Serialize(), fromDataAnnotation);
        return cyclic;
    }

    /// <summary>
    /// Returns the <see cref="ConfigurationSource" /> for whether the identity's sequence is cyclic.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for whether the identity's sequence is cyclic.</returns>
    public static ConfigurationSource? GetIdentityIsCyclicConfigurationSource(
        this IConventionProperty property)
        => property.FindAnnotation(NpgsqlAnnotationNames.IdentityOptions)?.GetConfigurationSource();

    /// <summary>
    /// Returns the number of sequence numbers to be preallocated and stored in memory for faster access.
    /// Defaults to 1 (no cache).
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The number of sequence numbers to be cached.</returns>
    public static long? GetIdentityNumbersToCache(this IReadOnlyProperty property)
        => IdentitySequenceOptionsData.Get(property).NumbersToCache;

    /// <summary>
    /// Sets the number of sequence numbers to be preallocated and stored in memory for faster access.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="numbersToCache">The value to set.</param>
    public static void SetIdentityNumbersToCache(this IMutableProperty property, long? numbersToCache)
    {
        var options = IdentitySequenceOptionsData.Get(property);
        options.NumbersToCache = numbersToCache ?? 1;
        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IdentityOptions, options.Serialize());
    }

    /// <summary>
    /// Sets the number of sequence numbers to be preallocated and stored in memory for faster access.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="numbersToCache">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    public static long? SetIdentityNumbersToCache(
        this IConventionProperty property, long? numbersToCache, bool fromDataAnnotation = false)
    {
        var options = IdentitySequenceOptionsData.Get(property);
        options.NumbersToCache = numbersToCache ?? 1;
        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.IdentityOptions, options.Serialize(), fromDataAnnotation);
        return numbersToCache;
    }

    /// <summary>
    /// Returns the <see cref="ConfigurationSource" /> for the number of sequence numbers to be preallocated
    /// and stored in memory for faster access.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>
    /// The <see cref="ConfigurationSource" /> for the number of sequence numbers to be preallocated
    /// and stored in memory for faster access.
    /// </returns>
    public static ConfigurationSource? GetIdentityNumbersToCacheConfigurationSource(
        this IConventionProperty property)
        => property.FindAnnotation(NpgsqlAnnotationNames.IdentityOptions)?.GetConfigurationSource();

    /// <summary>
    /// Removes identity sequence options from the property.
    /// </summary>
    public static void RemoveIdentityOptions(this IMutableProperty property)
        => property.RemoveAnnotation(NpgsqlAnnotationNames.IdentityOptions);

    /// <summary>
    /// Removes identity sequence options from the property.
    /// </summary>
    public static void RemoveIdentityOptions(this IConventionProperty property)
        => property.RemoveAnnotation(NpgsqlAnnotationNames.IdentityOptions);

    #endregion Identity sequence options

    #region Generated tsvector column

    /// <summary>
    /// Returns the text search configuration for this generated tsvector property, or <c>null</c> if this is not a
    /// generated tsvector property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>
    /// <para>
    /// The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
    /// generated tsvector property.
    /// </para>
    /// <para>
    /// See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
    /// </para>
    /// </returns>
    public static string? GetTsVectorConfig(this IReadOnlyProperty property)
        => (string?)property[NpgsqlAnnotationNames.TsVectorConfig];

    /// <summary>
    /// Sets the text search configuration for this generated tsvector property, or <c>null</c> if this is not a
    /// generated tsvector property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="config">
    /// <para>
    /// The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
    /// generated tsvector property.
    /// </para>
    /// <para>
    /// See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
    /// </para>
    /// </param>
    public static void SetTsVectorConfig(this IMutableProperty property, string? config)
    {
        Check.NullButNotEmpty(config, nameof(config));

        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.TsVectorConfig, config);
    }

    /// <summary>
    /// Returns the text search configuration for this generated tsvector property, or <c>null</c> if this is not a
    /// generated tsvector property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <param name="config">
    /// <para>
    /// The text search configuration for this generated tsvector property, or <c>null</c> if this is not a
    /// generated tsvector property.
    /// </para>
    /// <para>
    /// See https://www.postgresql.org/docs/current/textsearch-controls.html for more information.
    /// </para>
    /// </param>
    public static string SetTsVectorConfig(
        this IConventionProperty property, string config, bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(config, nameof(config));

        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.TsVectorConfig, config, fromDataAnnotation);

        return config;
    }

    /// <summary>
    /// Returns the <see cref="ConfigurationSource" /> for the text search configuration for the generated tsvector
    /// property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The configuration source for the text search configuration for the generated tsvector property.</returns>
    public static ConfigurationSource? GetTsVectorConfigConfigurationSource(this IConventionProperty property)
        => property.FindAnnotation(NpgsqlAnnotationNames.TsVectorConfig)?.GetConfigurationSource();

    /// <summary>
    /// Returns the properties included in this generated tsvector property, or <c>null</c> if this is not a
    /// generated tsvector property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The included property names, or <c>null</c> if this is not a Generated tsvector column.</returns>
    public static IReadOnlyList<string>? GetTsVectorProperties(this IReadOnlyProperty property)
        => (string[]?)property[NpgsqlAnnotationNames.TsVectorProperties];

    /// <summary>
    /// Sets the properties included in this generated tsvector property, or <c>null</c> to make this a regular,
    /// non-generated property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="properties">The included property names.</param>
    public static void SetTsVectorProperties(
        this IMutableProperty property,
        IReadOnlyList<string>? properties)
    {
        Check.NullButNotEmpty(properties, nameof(properties));

        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.TsVectorProperties, properties);
    }

    /// <summary>
    /// Sets properties included in this generated tsvector property, or <c>null</c> to make this a regular,
    /// non-generated property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <param name="properties">The included property names.</param>
    public static IReadOnlyList<string>? SetTsVectorProperties(
        this IConventionProperty property,
        IReadOnlyList<string>? properties,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(properties, nameof(properties));

        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.TsVectorProperties, properties, fromDataAnnotation);

        return properties;
    }

    /// <summary>
    /// Returns the <see cref="ConfigurationSource" /> for the properties included in the generated tsvector property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The configuration source for the properties included in the generated tsvector property.</returns>
    public static ConfigurationSource? GetTsVectorPropertiesConfigurationSource(this IConventionProperty property)
        => property.FindAnnotation(NpgsqlAnnotationNames.TsVectorConfig)?.GetConfigurationSource();

    #endregion Generated tsvector column

    #region Collation

    /// <summary>
    /// Returns the collation to be used for the column - including the PostgreSQL-specific default column
    /// collation defined at the model level (see
    /// <see cref="NpgsqlModelExtensions.SetDefaultColumnCollation(Microsoft.EntityFrameworkCore.Metadata.IMutableModel,string)"/>).
    /// </summary>
    /// <param name="property"> The property. </param>
    /// <returns> The collation for the column this property is mapped to. </returns>
    [Obsolete("Use EF Core's standard model bulk configuration API")]
    public static string? GetDefaultCollation(this IReadOnlyProperty property)
        => property.FindTypeMapping() is StringTypeMapping
            ? property.DeclaringEntityType.Model.GetDefaultColumnCollation()
            : null;

    #endregion Collation

    #region Compression method

    /// <summary>
    /// Returns the compression method to be used, or <c>null</c> if it hasn't been specified.
    /// </summary>
    /// <remarks>This feature was introduced in PostgreSQL 14.</remarks>
    public static string? GetCompressionMethod(this IReadOnlyProperty property)
        => (property is RuntimeProperty)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (string?)property[NpgsqlAnnotationNames.CompressionMethod];

    /// <summary>
    /// Returns the compression method to be used, or <c>null</c> if it hasn't been specified.
    /// </summary>
    /// <remarks>This feature was introduced in PostgreSQL 14.</remarks>
    public static string? GetCompressionMethod(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        => property is RuntimeProperty
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : property.FindAnnotation(NpgsqlAnnotationNames.CompressionMethod) is { } annotation
                ? (string?)annotation.Value
                : property.FindSharedStoreObjectRootProperty(storeObject)?.GetCompressionMethod(storeObject);

    /// <summary>
    /// Sets the compression method to be used, or <c>null</c> if it hasn't been specified.
    /// </summary>
    /// <remarks>This feature was introduced in PostgreSQL 14.</remarks>
    public static void SetCompressionMethod(this IMutableProperty property, string? compressionMethod)
        => property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.CompressionMethod, compressionMethod);

    /// <summary>
    /// Sets the compression method to be used, or <c>null</c> if it hasn't been specified.
    /// </summary>
    /// <remarks>This feature was introduced in PostgreSQL 14.</remarks>
    public static string? SetCompressionMethod(
        this IConventionProperty property,
        string? compressionMethod,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(compressionMethod, nameof(compressionMethod));

        property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.CompressionMethod, compressionMethod, fromDataAnnotation);

        return compressionMethod;
    }

    /// <summary>
    /// Returns the <see cref="ConfigurationSource" /> for the compression method.
    /// </summary>
    /// <param name="index">The property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the compression method.</returns>
    public static ConfigurationSource? GetCompressionMethodConfigurationSource(this IConventionProperty index)
        => index.FindAnnotation(NpgsqlAnnotationNames.IndexMethod)?.GetConfigurationSource();

    #endregion Compression method
}
