using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
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
        public static string GetNpgsqlHiLoSequenceName([NotNull] this IProperty property)
            => (string)property[NpgsqlAnnotationNames.HiLoSequenceName];

        /// <summary>
        /// Sets the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="name">The sequence name to use.</param>
        public static void SetNpgsqlHiLoSequenceName([NotNull] this IMutableProperty property, [CanBeNull] string name)
            => property.SetOrRemoveAnnotation(
                NpgsqlAnnotationNames.HiLoSequenceName,
                Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        /// Sets the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="name">The sequence name to use.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        public static void SetNpgsqlHiLoSequenceName(
            [NotNull] this IConventionProperty property, [CanBeNull] string name, bool fromDataAnnotation = false)
            => property.SetOrRemoveAnnotation(
                NpgsqlAnnotationNames.HiLoSequenceName,
                Check.NullButNotEmpty(name, nameof(name)),
                fromDataAnnotation);

        /// <summary>
        /// Returns the <see cref="ConfigurationSource" /> for the hi-lo sequence name.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the hi-lo sequence name.</returns>
        public static ConfigurationSource? GetNpgsqlHiLoSequenceNameConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceName)?.GetConfigurationSource();

        /// <summary>
        /// Returns the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The schema to use for the hi-lo sequence.</returns>
        public static string GetNpgsqlHiLoSequenceSchema([NotNull] this IProperty property)
            => (string)property[NpgsqlAnnotationNames.HiLoSequenceSchema];

        /// <summary>
        /// Sets the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="schema">The schema to use.</param>
        public static void SetNpgsqlHiLoSequenceSchema([NotNull] this IMutableProperty property, [CanBeNull] string schema)
            => property.SetOrRemoveAnnotation(
                NpgsqlAnnotationNames.HiLoSequenceSchema,
                Check.NullButNotEmpty(schema, nameof(schema)));

        /// <summary>
        /// Sets the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="schema">The schema to use.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        public static void SetNpgsqlHiLoSequenceSchema(
            [NotNull] this IConventionProperty property, [CanBeNull] string schema, bool fromDataAnnotation = false)
            => property.SetOrRemoveAnnotation(
                NpgsqlAnnotationNames.HiLoSequenceSchema,
                Check.NullButNotEmpty(schema, nameof(schema)),
                fromDataAnnotation);

        /// <summary>
        /// Returns the <see cref="ConfigurationSource" /> for the hi-lo sequence schema.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the hi-lo sequence schema.</returns>
        public static ConfigurationSource? GetNpgsqlHiLoSequenceSchemaConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema)?.GetConfigurationSource();

        /// <summary>
        /// Finds the <see cref="ISequence" /> in the model to use for the hi-lo pattern.
        /// </summary>
        /// <returns>The sequence to use, or <c>null</c> if no sequence exists in the model.</returns>
        public static ISequence FindNpgsqlHiLoSequence([NotNull] this IProperty property)
        {
            var model = property.DeclaringEntityType.Model;

            if (property.GetNpgsqlValueGenerationStrategy() != NpgsqlValueGenerationStrategy.SequenceHiLo)
            {
                return null;
            }

            var sequenceName = property.GetNpgsqlHiLoSequenceName()
                               ?? model.GetNpgsqlHiLoSequenceName();

            var sequenceSchema = property.GetNpgsqlHiLoSequenceSchema()
                                 ?? model.GetNpgsqlHiLoSequenceSchema();

            return model.FindSequence(sequenceName, sequenceSchema);
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
        public static NpgsqlValueGenerationStrategy GetNpgsqlValueGenerationStrategy([NotNull] this IProperty property)
        {
            var annotation = property[NpgsqlAnnotationNames.ValueGenerationStrategy];
            if (annotation != null)
            {
                return (NpgsqlValueGenerationStrategy)annotation;
            }

            if (property.ValueGenerated != ValueGenerated.OnAdd
                || property.GetDefaultValue() != null
                || property.GetDefaultValueSql() != null
                || property.GetComputedColumnSql() != null
                || !IsCompatibleWithValueGeneration(property)
                || !property.ClrType.IsIntegerForValueGeneration())
            {
                return NpgsqlValueGenerationStrategy.None;
            }

            return property.DeclaringEntityType.Model.GetNpgsqlValueGenerationStrategy()
                ?? NpgsqlValueGenerationStrategy.None;
        }

        /// <summary>
        /// Sets the <see cref="NpgsqlValueGenerationStrategy" /> to use for the property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The strategy to use.</param>
        public static void SetNpgsqlValueGenerationStrategy(
            [NotNull] this IMutableProperty property, NpgsqlValueGenerationStrategy? value)
        {
            CheckNpgsqlValueGenerationStrategy(property, value);

            property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy, value);
        }

        /// <summary>
        /// Sets the <see cref="NpgsqlValueGenerationStrategy" /> to use for the property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The strategy to use.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        public static void SetNpgsqlValueGenerationStrategy(
            [NotNull] this IConventionProperty property, NpgsqlValueGenerationStrategy? value, bool fromDataAnnotation = false)
        {
            CheckNpgsqlValueGenerationStrategy(property, value);

            property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy, value, fromDataAnnotation);
        }

        static void CheckNpgsqlValueGenerationStrategy(IProperty property, NpgsqlValueGenerationStrategy? value)
        {
            if (value != null)
            {
                var propertyType = property.ClrType;

                if (value == NpgsqlValueGenerationStrategy.SerialColumn && !propertyType.IsIntegerForValueGeneration())
                {
                    throw new ArgumentException($"Serial value generation cannot be used for the property '{property.Name}' on entity type '{property.DeclaringEntityType.DisplayName()}' because the property type is '{propertyType.ShortDisplayName()}'. Serial columns can only be of type short, int or long.");
                }

                if ((value == NpgsqlValueGenerationStrategy.IdentityAlwaysColumn || value == NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    && !propertyType.IsIntegerForValueGeneration())
                {
                    throw new ArgumentException($"Identity value generation cannot be used for the property '{property.Name}' on entity type '{property.DeclaringEntityType.DisplayName()}' because the property type is '{propertyType.ShortDisplayName()}'. Identity columns can only be of type short, int or long.");
                }

                if (value == NpgsqlValueGenerationStrategy.SequenceHiLo && !propertyType.IsInteger())
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
        public static ConfigurationSource? GetNpgsqlValueGenerationStrategyConfigurationSource(
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

            return type.IsIntegerForValueGeneration()
                   && (property.FindMapping()?.Converter ?? property.GetValueConverter()) == null;
        }

        static bool IsIntegerForValueGeneration(this Type type)
        {
            type = type.UnwrapNullableType();
            return type == typeof(int) || type == typeof(long) || type == typeof(short);
        }

        #endregion Value generation

        #region Comment

        public static string GetNpgsqlComment([NotNull] this IProperty property)
            => (string)property[NpgsqlAnnotationNames.Comment];

        public static void SetNpgsqlComment([NotNull] this IMutableProperty property, [CanBeNull] string comment)
            => property.SetOrRemoveAnnotation(NpgsqlAnnotationNames.Comment, comment);

        #endregion Comment
    }
}
