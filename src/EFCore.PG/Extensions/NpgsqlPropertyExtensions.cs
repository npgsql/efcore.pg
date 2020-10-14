using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlPropertyExtensions
    {
        #region Hi-lo

        /// <summary>
        /// Returns the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property"> The property.</param>
        /// <returns>The name to use for the hi-lo sequence.</returns>
        public static string GetHiLoSequenceName([NotNull] this IProperty property)
            => (string)property[NpgsqlAnnotationNames.HiLoSequenceName];

        /// <summary>
        /// Sets the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="name">The sequence name to use.</param>
        public static void SetHiLoSequenceName([NotNull] this IMutableProperty property, [CanBeNull] string name)
            => property.SetOrRemoveAnnotation(
                NpgsqlAnnotationNames.HiLoSequenceName,
                Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        /// Sets the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="name">The sequence name to use.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        public static string SetHiLoSequenceName(
            [NotNull] this IConventionProperty property, [CanBeNull] string name, bool fromDataAnnotation = false)
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
        public static ConfigurationSource? GetHiLoSequenceNameConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceName)?.GetConfigurationSource();

        /// <summary>
        /// Returns the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The schema to use for the hi-lo sequence.</returns>
        public static string GetHiLoSequenceSchema([NotNull] this IProperty property)
            => (string)property[NpgsqlAnnotationNames.HiLoSequenceSchema];

        /// <summary>
        /// Sets the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="schema">The schema to use.</param>
        public static void SetHiLoSequenceSchema([NotNull] this IMutableProperty property, [CanBeNull] string schema)
            => property.SetOrRemoveAnnotation(
                NpgsqlAnnotationNames.HiLoSequenceSchema,
                Check.NullButNotEmpty(schema, nameof(schema)));

        /// <summary>
        /// Sets the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="schema">The schema to use.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        public static string SetHiLoSequenceSchema(
            [NotNull] this IConventionProperty property, [CanBeNull] string schema, bool fromDataAnnotation = false)
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
        public static ConfigurationSource? GetHiLoSequenceSchemaConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema)?.GetConfigurationSource();

        /// <summary>
        /// Finds the <see cref="ISequence" /> in the model to use for the hi-lo pattern.
        /// </summary>
        /// <returns>The sequence to use, or <c>null</c> if no sequence exists in the model.</returns>
        public static ISequence FindHiLoSequence([NotNull] this IProperty property)
        {
            var model = property.DeclaringEntityType.Model;

            if (property.GetValueGenerationStrategy() != NpgsqlValueGenerationStrategy.SequenceHiLo)
            {
                return null;
            }

            var sequenceName = property.GetHiLoSequenceName()
                               ?? model.GetHiLoSequenceName();

            var sequenceSchema = property.GetHiLoSequenceSchema()
                                 ?? model.GetHiLoSequenceSchema();

            return model.FindSequence(sequenceName, sequenceSchema);
        }

        /// <summary>
        /// Removes all identity sequence annotations from the property.
        /// </summary>
        public static void RemoveHiLoOptions([NotNull] this IMutableProperty property)
        {
            property.SetHiLoSequenceName(null);
            property.SetHiLoSequenceSchema(null);
        }

        /// <summary>
        /// Removes all identity sequence annotations from the property.
        /// </summary>
        public static void RemoveHiLoOptions([NotNull] this IConventionProperty property)
        {
            property.SetHiLoSequenceName(null);
            property.SetHiLoSequenceSchema(null);
        }

        #endregion Hi-lo

        #region Value generation

        /// <summary>
        /// <para>Returns the <see cref="NpgsqlValueGenerationStrategy" /> to use for the property.</para>
        /// <para>
        /// If no strategy is set for the property, then the strategy to use will be taken from the <see cref="IModel" />.
        /// </para>
        /// </summary>
        /// <returns>The strategy, or <see cref="NpgsqlValueGenerationStrategy.None"/> if none was set.</returns>
        public static NpgsqlValueGenerationStrategy GetValueGenerationStrategy([NotNull] this IProperty property)
        {
            if (property[NpgsqlAnnotationNames.ValueGenerationStrategy] is object annotation)
                return (NpgsqlValueGenerationStrategy)annotation;

            if (property.ValueGenerated != ValueGenerated.OnAdd
                || property.IsForeignKey()
                || property.GetDefaultValue() != null
                || property.GetDefaultValueSql() != null
                || property.GetComputedColumnSql() != null)
            {
                return NpgsqlValueGenerationStrategy.None;
            }

            return GetDefaultValueGenerationStrategy(property);
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
            [NotNull] this IProperty property,
            StoreObjectIdentifier storeObject)
        {
            if (property[NpgsqlAnnotationNames.ValueGenerationStrategy] is object annotation)
                return (NpgsqlValueGenerationStrategy)annotation;

            if (property.FindSharedStoreObjectRootProperty(storeObject) is IProperty sharedTableRootProperty)
            {
                var principalStrategy = sharedTableRootProperty.GetValueGenerationStrategy(storeObject);
                return principalStrategy switch
                {
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn => principalStrategy,
                    NpgsqlValueGenerationStrategy.IdentityAlwaysColumn    => principalStrategy,
                    NpgsqlValueGenerationStrategy.SerialColumn            => principalStrategy,
                    _                                                     => NpgsqlValueGenerationStrategy.None
                };
            }

            if (property.ValueGenerated != ValueGenerated.OnAdd
                || property.GetContainingForeignKeys().Any(fk => !fk.IsBaseLinking())
                || property.GetDefaultValue() != null
                || property.GetDefaultValueSql() != null
                || property.GetComputedColumnSql() != null)
            {
                return NpgsqlValueGenerationStrategy.None;
            }

            return GetDefaultValueGenerationStrategy(property);
        }

        static NpgsqlValueGenerationStrategy GetDefaultValueGenerationStrategy(IProperty property)
        {
            var modelStrategy = property.DeclaringEntityType.Model.GetValueGenerationStrategy();

            switch (modelStrategy)
            {
            case NpgsqlValueGenerationStrategy.SequenceHiLo:
            case NpgsqlValueGenerationStrategy.SerialColumn:
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

        /// <summary>
        /// Sets the <see cref="NpgsqlValueGenerationStrategy" /> to use for the property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The strategy to use.</param>
        public static void SetValueGenerationStrategy(
            [NotNull] this IMutableProperty property, NpgsqlValueGenerationStrategy? value)
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
            [NotNull] this IConventionProperty property, NpgsqlValueGenerationStrategy? value, bool fromDataAnnotation = false)
        {
            CheckValueGenerationStrategy(property, value);

            property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy, value, fromDataAnnotation);

            return value;
        }

        static void CheckValueGenerationStrategy(IProperty property, NpgsqlValueGenerationStrategy? value)
        {
            if (value != null)
            {
                var propertyType = property.ClrType;

                if (value == NpgsqlValueGenerationStrategy.SerialColumn && !IsCompatibleWithValueGeneration(property))
                {
                    throw new ArgumentException($"Serial value generation cannot be used for the property '{property.Name}' on entity type '{property.DeclaringEntityType.DisplayName()}' because the property type is '{propertyType.ShortDisplayName()}'. Serial columns can only be of type short, int or long.");
                }

                if ((value == NpgsqlValueGenerationStrategy.IdentityAlwaysColumn || value == NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    && !IsCompatibleWithValueGeneration(property))
                {
                    throw new ArgumentException($"Identity value generation cannot be used for the property '{property.Name}' on entity type '{property.DeclaringEntityType.DisplayName()}' because the property type is '{propertyType.ShortDisplayName()}'. Identity columns can only be of type short, int or long.");
                }

                if (value == NpgsqlValueGenerationStrategy.SequenceHiLo && !IsCompatibleWithValueGeneration(property))
                {
                    throw new ArgumentException($"PostgreSQL sequences cannot be used to generate values for the property '{property.Name}' on entity type '{property.DeclaringEntityType.DisplayName()}' because the property type is '{propertyType.ShortDisplayName()}'. Sequences can only be used with integer properties.");
                }
            }
        }

        /// <summary>
        /// Returns the <see cref="ConfigurationSource" /> for the <see cref="NpgsqlValueGenerationStrategy" />.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the <see cref="NpgsqlValueGenerationStrategy" />.</returns>
        public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(
            [NotNull] this IConventionProperty property)
            => property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();

        /// <summary>
        /// Returns a value indicating whether the property is compatible with any <see cref="NpgsqlValueGenerationStrategy"/>.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns><c>true</c> if compatible.</returns>
        public static bool IsCompatibleWithValueGeneration([NotNull] IProperty property)
        {
            var type = property.ClrType;

            return type.IsInteger() &&
                   (property.GetValueConverter() ?? property.FindTypeMapping()?.Converter) == null;
        }

        #endregion Value generation

        #region Identity sequence options

        /// <summary>
        /// Returns the identity start value.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The identity start value.</returns>
        public static long? GetIdentityStartValue([NotNull] this IProperty property)
            => IdentitySequenceOptionsData.Get(property).StartValue;

        /// <summary>
        /// Sets the identity start value.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="startValue">The value to set.</param>
        public static void SetIdentityStartValue([NotNull] this IMutableProperty property, long? startValue)
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
            [NotNull] this IConventionProperty property, long? startValue, bool fromDataAnnotation = false)
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
            [NotNull] this IConventionProperty property)
            => property.FindAnnotation(NpgsqlAnnotationNames.IdentityOptions)?.GetConfigurationSource();

        /// <summary>
        /// Returns the identity increment value.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The identity increment value.</returns>
        public static long? GetIdentityIncrementBy([NotNull] this IProperty property)
            => IdentitySequenceOptionsData.Get(property).IncrementBy;

        /// <summary>
        /// Sets the identity increment value.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="incrementBy">The value to set.</param>
        public static void SetIdentityIncrementBy([NotNull] this IMutableProperty property, long? incrementBy)
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
            [NotNull] this IConventionProperty property, long? incrementBy, bool fromDataAnnotation = false)
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
            [NotNull] this IConventionProperty property)
            => property.FindAnnotation(NpgsqlAnnotationNames.IdentityOptions)?.GetConfigurationSource();

        /// <summary>
        /// Returns the identity minimum value.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The identity minimum value.</returns>
        public static long? GetIdentityMinValue([NotNull] this IProperty property)
            => IdentitySequenceOptionsData.Get(property).MinValue;

        /// <summary>
        /// Sets the identity minimum value.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="minValue">The value to set.</param>
        public static void SetIdentityMinValue([NotNull] this IMutableProperty property, long? minValue)
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
            [NotNull] this IConventionProperty property, long? minValue, bool fromDataAnnotation = false)
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
            [NotNull] this IConventionProperty property)
            => property.FindAnnotation(NpgsqlAnnotationNames.IdentityOptions)?.GetConfigurationSource();

        /// <summary>
        /// Returns the identity maximum value.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The identity maximum value.</returns>
        public static long? GetIdentityMaxValue([NotNull] this IProperty property)
            => IdentitySequenceOptionsData.Get(property).MaxValue;

        /// <summary>
        /// Sets the identity maximum value.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="maxValue">The value to set.</param>
        public static void SetIdentityMaxValue([NotNull] this IMutableProperty property, long? maxValue)
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
            [NotNull] this IConventionProperty property, long? maxValue, bool fromDataAnnotation = false)
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
            [NotNull] this IConventionProperty property)
            => property.FindAnnotation(NpgsqlAnnotationNames.IdentityOptions)?.GetConfigurationSource();

        /// <summary>
        /// Returns whether the identity's sequence is cyclic.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>Whether the identity's sequence is cyclic.</returns>
        public static bool? GetIdentityIsCyclic([NotNull] this IProperty property)
            => IdentitySequenceOptionsData.Get(property).IsCyclic;

        /// <summary>
        /// Sets whether the identity's sequence is cyclic.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="cyclic">The value to set.</param>
        public static void SetIdentityIsCyclic([NotNull] this IMutableProperty property, bool? cyclic)
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
            [NotNull] this IConventionProperty property, bool? cyclic, bool fromDataAnnotation = false)
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
            [NotNull] this IConventionProperty property)
            => property.FindAnnotation(NpgsqlAnnotationNames.IdentityOptions)?.GetConfigurationSource();

        /// <summary>
        /// Returns the number of sequence numbers to be preallocated and stored in memory for faster access.
        /// Defaults to 1 (no cache).
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The number of sequence numbers to be cached.</returns>
        public static long? GetIdentityNumbersToCache([NotNull] this IProperty property)
            => IdentitySequenceOptionsData.Get(property).NumbersToCache;

        /// <summary>
        /// Sets the number of sequence numbers to be preallocated and stored in memory for faster access.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="numbersToCache">The value to set.</param>
        public static void SetIdentityNumbersToCache([NotNull] this IMutableProperty property, long? numbersToCache)
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
            [NotNull] this IConventionProperty property, long? numbersToCache, bool fromDataAnnotation = false)
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
            [NotNull] this IConventionProperty property)
            => property.FindAnnotation(NpgsqlAnnotationNames.IdentityOptions)?.GetConfigurationSource();

        /// <summary>
        /// Removes identity sequence options from the property.
        /// </summary>
        public static void RemoveIdentityOptions([NotNull] this IMutableProperty property)
            => property.RemoveAnnotation(NpgsqlAnnotationNames.IdentityOptions);

        /// <summary>
        /// Removes identity sequence options from the property.
        /// </summary>
        public static void RemoveIdentityOptions([NotNull] this IConventionProperty property)
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
        public static string GetTsVectorConfig([NotNull] this IProperty property)
            => (string)property[NpgsqlAnnotationNames.TsVectorConfig];

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
        public static void SetTsVectorConfig([NotNull] this IMutableProperty property, [CanBeNull] string config)
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
            [NotNull] this IConventionProperty property, [NotNull] string config, bool fromDataAnnotation = false)
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
        public static ConfigurationSource? GetTsVectorConfigConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(NpgsqlAnnotationNames.TsVectorConfig)?.GetConfigurationSource();

        /// <summary>
        /// Returns the properties included in this generated tsvector property, or <c>null</c> if this is not a
        /// generated tsvector property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The included property names, or <c>null</c> if this is not a Generated tsvector column.</returns>
        public static IReadOnlyList<string> GetTsVectorProperties([NotNull] this IProperty property)
            => (string[])property[NpgsqlAnnotationNames.TsVectorProperties];

        /// <summary>
        /// Sets the properties included in this generated tsvector property, or <c>null</c> to make this a regular,
        /// non-generated property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="properties">The included property names.</param>
        public static void SetTsVectorProperties(
            [NotNull] this IMutableProperty property,
            [CanBeNull] IReadOnlyList<string> properties)
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
        public static IReadOnlyList<string> SetTsVectorProperties(
            [NotNull] this IConventionProperty property,
            [CanBeNull] IReadOnlyList<string> properties,
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
        public static ConfigurationSource? GetTsVectorPropertiesConfigurationSource([NotNull] this IConventionProperty property)
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
        public static string GetDefaultCollation([NotNull] this IProperty property)
            => property.FindTypeMapping() is StringTypeMapping
                ? property.DeclaringEntityType.Model.GetDefaultColumnCollation()
                : null;

        #endregion Collation
    }
}
