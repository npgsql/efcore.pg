using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Npgsql specific extension methods for <see cref="ModelBuilder" />.
    /// </summary>
    public static class NpgsqlModelBuilderExtensions
    {
        /// <summary>
        ///     Configures the model to use a sequence-based hi-lo pattern to generate values for properties
        ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder ForNpgsqlUseSequenceHiLo(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var model = modelBuilder.Model;

            name = name ?? NpgsqlModelAnnotations.DefaultHiLoSequenceName;

            if (model.Npgsql().FindSequence(name, schema) == null)
            {
                modelBuilder.HasSequence(name, schema).IncrementsBy(10);
            }

            model.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SequenceHiLo;
            model.Npgsql().HiLoSequenceName = name;
            model.Npgsql().HiLoSequenceSchema = schema;

            return modelBuilder;
        }

        /// <summary>
        ///     Configures the model to use the PostgreSQL SERIAL feature to generate values for properties
        ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL. This is the default
        ///     behavior when targeting PostgreSQL.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder ForNpgsqlUseSerialColumns(
            [NotNull] this ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            var property = modelBuilder.Model;

            property.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.SerialColumn;
            property.Npgsql().HiLoSequenceName = null;
            property.Npgsql().HiLoSequenceSchema = null;

            return modelBuilder;
        }

        #region Identity

        /// <summary>
        /// <para>
        /// Configures the model to use the PostgreSQL IDENTITY feature to generate values for properties
        /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL. Values for these
        /// columns will always be generated as identity, and the application will not be able to override
        /// this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static ModelBuilder ForNpgsqlUseIdentityAlwaysColumns(
            [NotNull] this ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            var property = modelBuilder.Model;

            property.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.IdentityAlwaysColumn;
            property.Npgsql().HiLoSequenceName = null;
            property.Npgsql().HiLoSequenceSchema = null;

            return modelBuilder;
        }

        /// <summary>
        /// <para>
        /// Configures the model to use the PostgreSQL IDENTITY feature to generate values for properties
        /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL. Values for these
        /// columns will be generated as identity by default, but the application will be able to override
        /// this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static ModelBuilder ForNpgsqlUseIdentityByDefaultColumns(
            [NotNull] this ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            var property = modelBuilder.Model;

            property.Npgsql().ValueGenerationStrategy = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn;
            property.Npgsql().HiLoSequenceName = null;
            property.Npgsql().HiLoSequenceSchema = null;

            return modelBuilder;
        }

        /// <summary>
        /// <para>
        /// Configures the model to use the PostgreSQL IDENTITY feature to generate values for properties
        /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL. Values for these
        /// columns will be generated as identity by default, but the application will be able to override
        /// this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static ModelBuilder ForNpgsqlUseIdentityColumns(
            [NotNull] this ModelBuilder modelBuilder)
            => modelBuilder.ForNpgsqlUseIdentityByDefaultColumns();

        #endregion Identity

        public static ModelBuilder HasPostgresExtension(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));

            modelBuilder.Model.Npgsql().GetOrAddPostgresExtension(name);
            return modelBuilder;
        }

        public static ModelBuilder ForNpgsqlHasEnum(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string[] labels)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(labels, nameof(labels));

            modelBuilder.Model.Npgsql().GetOrAddPostgresEnum(schema, name, labels);
            return modelBuilder;
        }

        public static ModelBuilder ForNpgsqlHasEnum(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [NotNull] string[] labels)
            => modelBuilder.ForNpgsqlHasEnum(null, name, labels);

        public static ModelBuilder HasDatabaseTemplate(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string templateDatabaseName)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(templateDatabaseName, nameof(templateDatabaseName));

            modelBuilder.Model.Npgsql().DatabaseTemplate = templateDatabaseName;
            return modelBuilder;
        }

        /// <summary>
        /// Registers a user-defined range type in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder on which to create the range type.</param>
        /// <param name="schema">The schema in which to create the range type.</param>
        /// <param name="name">The name of the range type to be created.</param>
        /// <param name="subtype">The subtype (or element type) of the range</param>
        /// <param name="canonicalFunction">
        /// An optional PostgreSQL function which converts range values to a canonical form.
        /// </param>
        /// <param name="subtypeOpClass">Used to specify a non-default operator class.</param>
        /// <param name="collation">Used to specify a non-default collation in the range's order.</param>
        /// <param name="subtypeDiff">
        /// An optional PostgreSQL function taking two values of the subtype type as argument, and return a double
        /// precision value representing the difference between the two given values.
        /// </param>
        /// <remarks>
        /// See https://www.postgresql.org/docs/current/static/rangetypes.html,
        /// https://www.postgresql.org/docs/current/static/sql-createtype.html,
        /// </remarks>
        [NotNull]
        public static ModelBuilder ForNpgsqlHasRange(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string subtype,
            string canonicalFunction = null,
            string subtypeOpClass = null,
            string collation = null,
            string subtypeDiff = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(subtype, nameof(subtype));

            modelBuilder.Model.Npgsql().GetOrAddPostgresRange(
                schema,
                name,
                subtype,
                canonicalFunction,
                subtypeOpClass,
                collation,
                subtypeDiff);
            return modelBuilder;
        }

        /// <summary>
        /// Registers a user-defined range type in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder on which to create the range type.</param>
        /// <param name="name">The name of the range type to be created.</param>
        /// <param name="subtype">The subtype (or element type) of the range</param>
        /// <remarks>
        /// See https://www.postgresql.org/docs/current/static/rangetypes.html,
        /// https://www.postgresql.org/docs/current/static/sql-createtype.html,
        /// </remarks>
        public static ModelBuilder ForNpgsqlHasRange(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [NotNull] string subtype)
            => ForNpgsqlHasRange(modelBuilder, null, name, subtype);

        public static ModelBuilder ForNpgsqlUseTablespace(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string tablespace)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(tablespace, nameof(tablespace));

            modelBuilder.Model.Npgsql().Tablespace = tablespace;
            return modelBuilder;
        }
    }
}
