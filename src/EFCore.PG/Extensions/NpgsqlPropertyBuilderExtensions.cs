using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Npgsql specific extension methods for <see cref="PropertyBuilder" />.
    /// </summary>
    public static class NpgsqlPropertyBuilderExtensions
    {
        #region HiLo

        /// <summary>
        /// Configures the property to use a sequence-based hi-lo pattern to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <param name="name"> The comment of the sequence.</param>
        /// <param name="schema"> The schema of the sequence.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder ForNpgsqlUseSequenceHiLo(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var property = propertyBuilder.Metadata;

            name ??= NpgsqlModelExtensions.DefaultHiLoSequenceName;

            var model = property.DeclaringEntityType.Model;

            if (model.FindSequence(name, schema) == null)
            {
                model.AddSequence(name, schema).IncrementBy = 10;
            }

            property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);
            property.SetNpgsqlHiLoSequenceName(name);
            property.SetNpgsqlHiLoSequenceSchema(schema);

            return propertyBuilder;
        }

        /// <summary>
        /// Configures the property to use a sequence-based hi-lo pattern to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <param name="name"> The comment of the sequence.</param>
        /// <param name="schema"> The schema of the sequence.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> ForNpgsqlUseSequenceHiLo<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
            => (PropertyBuilder<TProperty>)ForNpgsqlUseSequenceHiLo((PropertyBuilder)propertyBuilder, name, schema);

        /// <summary>
        /// Configures the database sequence used for the hi-lo pattern to generate values for the key property,
        /// when targeting SQL Server.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <param name="name"> The name of the sequence.</param>
        /// <param name="schema">The schema of the sequence.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the sequence.</returns>
        public static IConventionSequenceBuilder ForNpgsqlHasHiLoSequence(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.ForNpgsqlCanSetHiLoSequence(name, schema))
            {
                return null;
            }

            propertyBuilder.Metadata.SetNpgsqlHiLoSequenceName(name, fromDataAnnotation);
            propertyBuilder.Metadata.SetNpgsqlHiLoSequenceSchema(schema, fromDataAnnotation);

            return name == null
                ? null
                : propertyBuilder.Metadata.DeclaringEntityType.Model.Builder.HasSequence(name, schema, fromDataAnnotation);
        }

        /// <summary>
        /// Returns a value indicating whether the given name and schema can be set for the hi-lo sequence.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <param name="name"> The name of the sequence.</param>
        /// <param name="schema">The schema of the sequence.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><c>true</c> if the given name and schema can be set for the hi-lo sequence.</returns>
        public static bool ForNpgsqlCanSetHiLoSequence(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            return propertyBuilder.CanSetAnnotation(NpgsqlAnnotationNames.HiLoSequenceName, name, fromDataAnnotation)
                   && propertyBuilder.CanSetAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema, schema, fromDataAnnotation);
        }

        #endregion HiLo

        #region Serial

        /// <summary>
        /// Configures the property to use the PostgreSQL SERIAL feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder ForNpgsqlUseSerialColumn(
            [NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var property = propertyBuilder.Metadata;
            property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SerialColumn);
            property.SetNpgsqlHiLoSequenceName(null);
            property.SetNpgsqlHiLoSequenceSchema(null);

            return propertyBuilder;
        }

        /// <summary>
        /// Configures the property to use the PostgreSQL SERIAL feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured.</typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> ForNpgsqlUseSerialColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>)ForNpgsqlUseSerialColumn((PropertyBuilder)propertyBuilder);

        /// <summary>
        /// Configures the property to use the PostgreSQL SERIAL feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static IConventionPropertyBuilder ForNpgsqlUseSerialColumn(
            [NotNull] this IConventionPropertyBuilder propertyBuilder)
        {
            if (propertyBuilder.ForNpgsqlCanSetValueGenerationStrategy(NpgsqlValueGenerationStrategy.SerialColumn))
            {
                var property = propertyBuilder.Metadata;
                property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SerialColumn);
                property.SetNpgsqlHiLoSequenceName(null);
                property.SetNpgsqlHiLoSequenceSchema(null);

                return propertyBuilder;
            }

            return null;
        }

        #endregion Serial

        #region Identity always

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will always be generated as identity, and the application will not be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder ForNpgsqlUseIdentityAlwaysColumn([NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var property = propertyBuilder.Metadata;
            property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
            property.SetNpgsqlHiLoSequenceName(null);
            property.SetNpgsqlHiLoSequenceSchema(null);

            return propertyBuilder;
        }

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will always be generated as identity, and the application will not be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> ForNpgsqlUseIdentityAlwaysColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>)ForNpgsqlUseIdentityAlwaysColumn((PropertyBuilder)propertyBuilder);

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will always be generated as identity, and the application will not be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static IConventionPropertyBuilder ForNpgsqlUseIdentityAlwaysColumn([NotNull] this IConventionPropertyBuilder propertyBuilder)
        {
            if (propertyBuilder.ForNpgsqlCanSetValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityAlwaysColumn))
            {
                var property = propertyBuilder.Metadata;
                property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
                property.SetNpgsqlHiLoSequenceName(null);
                property.SetNpgsqlHiLoSequenceSchema(null);

                return propertyBuilder;
            }

            return null;
        }

        #endregion Identity always

        #region Identity by default

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder ForNpgsqlUseIdentityByDefaultColumn([NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var property = propertyBuilder.Metadata;
            property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            property.SetNpgsqlHiLoSequenceName(null);
            property.SetNpgsqlHiLoSequenceSchema(null);

            return propertyBuilder;
        }

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured.</typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> ForNpgsqlUseIdentityByDefaultColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>)ForNpgsqlUseIdentityByDefaultColumn((PropertyBuilder)propertyBuilder);

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static IConventionPropertyBuilder ForNpgsqlUseIdentityByDefaultColumn([NotNull] this IConventionPropertyBuilder propertyBuilder)
        {
            if (propertyBuilder.ForNpgsqlCanSetValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn))
            {
                var property = propertyBuilder.Metadata;
                property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
                property.SetNpgsqlHiLoSequenceName(null);
                property.SetNpgsqlHiLoSequenceSchema(null);

                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder ForNpgsqlUseIdentityColumn(
            [NotNull] this PropertyBuilder propertyBuilder)
            => propertyBuilder.ForNpgsqlUseIdentityByDefaultColumn();

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured.</typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> ForNpgsqlUseIdentityColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => propertyBuilder.ForNpgsqlUseIdentityByDefaultColumn();

        #endregion Identity by default

        #region General value generation strategy

        /// <summary>
        /// Configures the value generation strategy for the key property, when targeting PostgreSQL.
        /// </summary>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="valueGenerationStrategy">The value generation strategy.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        /// The same builder instance if the configuration was applied, <c>null</c> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder ForNpgsqlHasValueGenerationStrategy(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            NpgsqlValueGenerationStrategy? valueGenerationStrategy,
            bool fromDataAnnotation = false)
        {
            if (propertyBuilder.CanSetAnnotation(
                NpgsqlAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy, fromDataAnnotation))
            {
                propertyBuilder.Metadata.SetNpgsqlValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation);
                if (valueGenerationStrategy != NpgsqlValueGenerationStrategy.SequenceHiLo)
                {
                    propertyBuilder.ForNpgsqlHasHiLoSequence(null, null, fromDataAnnotation);
                }

                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether the given value can be set as the value generation strategy.
        /// </summary>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="valueGenerationStrategy">The value generation strategy.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><c>true</c> if the given value can be set as the default value generation strategy.</returns>
        public static bool ForNpgsqlCanSetValueGenerationStrategy(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            NpgsqlValueGenerationStrategy? valueGenerationStrategy,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            return (valueGenerationStrategy == null
                    || NpgsqlPropertyExtensions.IsCompatibleWithValueGeneration(propertyBuilder.Metadata))
                   && propertyBuilder.CanSetAnnotation(
                       NpgsqlAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy, fromDataAnnotation);
        }

        #endregion General value generation strategy

        #region Comment

        /// <summary>
        /// Configures the comment set on the column when targeting Npgsql.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <param name="comment"> The comment of the column.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder ForNpgsqlHasComment(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string comment)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(comment, nameof(comment));

            propertyBuilder.Metadata.SetNpgsqlComment(comment);

            return propertyBuilder;
        }

        /// <summary>
        /// Configures the comment set on the column when targeting Npgsql.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <param name="comment"> The comment of the column.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TEntity> ForNpgsqlHasComment<TEntity>(
            [NotNull] this PropertyBuilder<TEntity> propertyBuilder,
            [CanBeNull] string comment)
        => (PropertyBuilder<TEntity>)ForNpgsqlHasComment((PropertyBuilder)propertyBuilder, comment);

        #endregion Comment

        #region Obsolete

        /// <summary>
        /// Configures the property to use the PostgreSQL SERIAL feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured.</typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use ForNpgsqlUseSerialColumn instead")]
        public static PropertyBuilder UseNpgsqlSerialColumn([NotNull] this PropertyBuilder propertyBuilder)
            => propertyBuilder.ForNpgsqlUseSerialColumn();

        /// <summary>
        /// Configures the property to use the PostgreSQL SERIAL feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured.</typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use ForNpgsqlUseSerialColumn instead")]
        public static PropertyBuilder<TProperty> UseNpgsqlSerialColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => propertyBuilder.ForNpgsqlUseSerialColumn();

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will always be generated as identity, and the application will not be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use ForNpgsqlUseIdentityAlwaysColumn")]
        public static PropertyBuilder UseNpgsqlIdentityAlwaysColumn([NotNull] this PropertyBuilder propertyBuilder)
            => propertyBuilder.ForNpgsqlUseIdentityAlwaysColumn();

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will always be generated as identity, and the application will not be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use ForNpgsqlUseIdentityAlwaysColumn")]
        public static PropertyBuilder<TProperty> UseNpgsqlIdentityAlwaysColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => propertyBuilder.ForNpgsqlUseIdentityAlwaysColumn();

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use ForNpgsqlUseIdentityByDefaultColumn")]
        public static PropertyBuilder UseNpgsqlIdentityByDefaultColumn([NotNull] this PropertyBuilder propertyBuilder)
            => propertyBuilder.ForNpgsqlUseIdentityByDefaultColumn();

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured.</typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use ForNpgsqlUseIdentityByDefaultColumn")]
        public static PropertyBuilder<TProperty> UseNpgsqlIdentityByDefaultColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => propertyBuilder.ForNpgsqlUseIdentityByDefaultColumn();

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use ForNpgsqlUseIdentityColumn")]
        public static PropertyBuilder UseNpgsqlIdentityColumn(
            [NotNull] this PropertyBuilder propertyBuilder)
            => propertyBuilder.ForNpgsqlUseIdentityColumn();

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured.</typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use ForNpgsqlUseIdentityColumn")]
        public static PropertyBuilder<TProperty> UseNpgsqlIdentityColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => propertyBuilder.ForNpgsqlUseIdentityColumn();

        #endregion Obsolete
    }
}
