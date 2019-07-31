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
        public static PropertyBuilder UseHiLo(
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

            property.SetValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);
            property.SetHiLoSequenceName(name);
            property.SetHiLoSequenceSchema(schema);

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
        public static PropertyBuilder<TProperty> UseHiLo<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
            => (PropertyBuilder<TProperty>)UseHiLo((PropertyBuilder)propertyBuilder, name, schema);

        /// <summary>
        /// Configures the database sequence used for the hi-lo pattern to generate values for the key property,
        /// when targeting SQL Server.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <param name="name"> The name of the sequence.</param>
        /// <param name="schema">The schema of the sequence.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>A builder to further configure the sequence.</returns>
        public static IConventionSequenceBuilder HasHiLoSequence(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.CanSetHiLoSequence(name, schema))
            {
                return null;
            }

            propertyBuilder.Metadata.SetHiLoSequenceName(name, fromDataAnnotation);
            propertyBuilder.Metadata.SetHiLoSequenceSchema(schema, fromDataAnnotation);

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
        public static bool CanSetHiLoSequence(
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
        /// <para>
        /// This option should be considered deprecated starting with PostgreSQL 10, consider using <see cref="UseIdentityColumn"/> instead.
        /// </para>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder UseSerialColumn(
            [NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var property = propertyBuilder.Metadata;
            property.SetValueGenerationStrategy(NpgsqlValueGenerationStrategy.SerialColumn);
            property.SetHiLoSequenceName(null);
            property.SetHiLoSequenceSchema(null);

            return propertyBuilder;
        }

        /// <summary>
        /// Configures the property to use the PostgreSQL SERIAL feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <para>
        /// This option should be considered deprecated starting with PostgreSQL 10, consider using <see cref="UseIdentityColumn"/> instead.
        /// </para>
        /// <typeparam name="TProperty"> The type of the property being configured.</typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> UseSerialColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>)UseSerialColumn((PropertyBuilder)propertyBuilder);

        /// <summary>
        /// Configures the property to use the PostgreSQL SERIAL feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <para>
        /// This option should be considered deprecated starting with PostgreSQL 10, consider using <see cref="UseIdentityColumn"/> instead.
        /// </para>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static IConventionPropertyBuilder UseSerialColumn(
            [NotNull] this IConventionPropertyBuilder propertyBuilder)
        {
            if (propertyBuilder.CanSetValueGenerationStrategy(NpgsqlValueGenerationStrategy.SerialColumn))
            {
                var property = propertyBuilder.Metadata;
                property.SetValueGenerationStrategy(NpgsqlValueGenerationStrategy.SerialColumn);
                property.SetHiLoSequenceName(null);
                property.SetHiLoSequenceSchema(null);

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
        public static PropertyBuilder UseIdentityAlwaysColumn([NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var property = propertyBuilder.Metadata;
            property.SetValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
            property.SetHiLoSequenceName(null);
            property.SetHiLoSequenceSchema(null);

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
        public static PropertyBuilder<TProperty> UseIdentityAlwaysColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>)UseIdentityAlwaysColumn((PropertyBuilder)propertyBuilder);

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
        public static IConventionPropertyBuilder UseIdentityAlwaysColumn([NotNull] this IConventionPropertyBuilder propertyBuilder)
        {
            if (propertyBuilder.CanSetValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityAlwaysColumn))
            {
                var property = propertyBuilder.Metadata;
                property.SetValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
                property.SetHiLoSequenceName(null);
                property.SetHiLoSequenceSchema(null);

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
        /// <para>
        /// This is the default behavior when targeting PostgreSQL. Available only starting PostgreSQL 10.
        /// </para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder UseIdentityByDefaultColumn([NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var property = propertyBuilder.Metadata;
            property.SetValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            property.SetHiLoSequenceName(null);
            property.SetHiLoSequenceSchema(null);

            return propertyBuilder;
        }

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>
        /// This is the default behavior when targeting PostgreSQL. Available only starting PostgreSQL 10.
        /// </para>
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured.</typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> UseIdentityByDefaultColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>)UseIdentityByDefaultColumn((PropertyBuilder)propertyBuilder);

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>
        /// This is the default behavior when targeting PostgreSQL. Available only starting PostgreSQL 10.
        /// </para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static IConventionPropertyBuilder UseIdentityByDefaultColumn([NotNull] this IConventionPropertyBuilder propertyBuilder)
        {
            if (propertyBuilder.CanSetValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn))
            {
                var property = propertyBuilder.Metadata;
                property.SetValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
                property.SetHiLoSequenceName(null);
                property.SetHiLoSequenceSchema(null);

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
        /// <para>
        /// This internally calls <see cref="UseIdentityByDefaultColumn(Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder)"/>.
        /// This is the default behavior when targeting PostgreSQL. Available only starting PostgreSQL 10.
        /// </para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder UseIdentityColumn(
            [NotNull] this PropertyBuilder propertyBuilder)
            => propertyBuilder.UseIdentityByDefaultColumn();

        /// <summary>
        /// <para>
        /// Configures the property to use the PostgreSQL IDENTITY feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// Values for this property will be generated as identity by default, but the application will be able
        /// to override this behavior by providing a value.
        /// </para>
        /// <para>
        /// This internally calls <see cref="UseIdentityByDefaultColumn(Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder)"/>.
        /// This is the default behavior when targeting PostgreSQL. Available only starting PostgreSQL 10.
        /// </para>
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured.</typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> UseIdentityColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => propertyBuilder.UseIdentityByDefaultColumn();

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
        public static IConventionPropertyBuilder HasValueGenerationStrategy(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            NpgsqlValueGenerationStrategy? valueGenerationStrategy,
            bool fromDataAnnotation = false)
        {
            if (propertyBuilder.CanSetAnnotation(
                NpgsqlAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy, fromDataAnnotation))
            {
                propertyBuilder.Metadata.SetValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation);
                if (valueGenerationStrategy != NpgsqlValueGenerationStrategy.SequenceHiLo)
                {
                    propertyBuilder.HasHiLoSequence(null, null, fromDataAnnotation);
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
        public static bool CanSetValueGenerationStrategy(
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
        public static PropertyBuilder HasComment(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string comment)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(comment, nameof(comment));

            NpgsqlPropertyExtensions.SetComment(propertyBuilder.Metadata, comment);

            return propertyBuilder;
        }

        /// <summary>
        /// Configures the comment set on the column when targeting Npgsql.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <param name="comment"> The comment of the column.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TEntity> HasComment<TEntity>(
            [NotNull] this PropertyBuilder<TEntity> propertyBuilder,
            [CanBeNull] string comment)
        => (PropertyBuilder<TEntity>)HasComment((PropertyBuilder)propertyBuilder, comment);

        #endregion Comment

        #region Obsolete

        /// <summary>
        /// Configures the property to use a sequence-based hi-lo pattern to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <param name="name"> The comment of the sequence.</param>
        /// <param name="schema"> The schema of the sequence.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use UseHiLo")]
        public static PropertyBuilder ForNpgsqlUseSequenceHiLo([NotNull] this PropertyBuilder propertyBuilder, [CanBeNull] string name = null, [CanBeNull] string schema = null)
            => propertyBuilder.UseHiLo(name, schema);

        /// <summary>
        /// Configures the property to use a sequence-based hi-lo pattern to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <param name="name"> The comment of the sequence.</param>
        /// <param name="schema"> The schema of the sequence.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use UseHiLo")]
        public static PropertyBuilder<TProperty> ForNpgsqlUseSequenceHiLo<TProperty>([NotNull] this PropertyBuilder<TProperty> propertyBuilder, [CanBeNull] string name = null, [CanBeNull] string schema = null)
            => propertyBuilder.UseHiLo(name, schema);

        /// <summary>
        /// Configures the property to use the PostgreSQL SERIAL feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured.</typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use UseSerialColumn")]
        public static PropertyBuilder UseNpgsqlSerialColumn([NotNull] this PropertyBuilder propertyBuilder)
            => propertyBuilder.UseSerialColumn();

        /// <summary>
        /// Configures the property to use the PostgreSQL SERIAL feature to generate values for new entities,
        /// when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured.</typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use UseSerialColumn")]
        public static PropertyBuilder<TProperty> UseNpgsqlSerialColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => propertyBuilder.UseSerialColumn();

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
        [Obsolete("Use UseIdentityAlwaysColumn")]
        public static PropertyBuilder UseNpgsqlIdentityAlwaysColumn([NotNull] this PropertyBuilder propertyBuilder)
            => propertyBuilder.UseIdentityAlwaysColumn();

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
        [Obsolete("Use UseIdentityAlwaysColumn")]
        public static PropertyBuilder<TProperty> UseNpgsqlIdentityAlwaysColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => propertyBuilder.UseIdentityAlwaysColumn();

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
        [Obsolete("Use UseIdentityByDefaultColumn")]
        public static PropertyBuilder UseNpgsqlIdentityByDefaultColumn([NotNull] this PropertyBuilder propertyBuilder)
            => propertyBuilder.UseIdentityByDefaultColumn();

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
        [Obsolete("Use UseIdentityByDefaultColumn")]
        public static PropertyBuilder<TProperty> UseNpgsqlIdentityByDefaultColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => propertyBuilder.UseIdentityByDefaultColumn();

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
        [Obsolete("Use UseIdentityColumn")]
        public static PropertyBuilder UseNpgsqlIdentityColumn(
            [NotNull] this PropertyBuilder propertyBuilder)
            => propertyBuilder.UseIdentityColumn();

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
        [Obsolete("Use UseIdentityColumn")]
        public static PropertyBuilder<TProperty> UseNpgsqlIdentityColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => propertyBuilder.UseIdentityColumn();

        #endregion Obsolete
    }
}
