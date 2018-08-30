#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using Npgsql.NameTranslation;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Npgsql specific extension methods for <see cref="ModelBuilder"/>.
    /// </summary>
    [PublicAPI]
    public static class NpgsqlModelBuilderExtensions
    {
        // TODO: update driver to expose this from INpgsqlTypeMapper.
        [NotNull] static readonly INpgsqlNameTranslator DefaultNameTranslator = new NpgsqlSnakeCaseNameTranslator();

        #region Sequences

        /// <summary>
        /// Configures the model to use a sequence-based hi-lo pattern to generate values for properties
        /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <param name="name">The name of the sequence.</param>
        /// <param name="schema">The schema of the sequence.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
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
        /// Configures the model to use the PostgreSQL SERIAL feature to generate values for properties
        /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL. This is the default
        /// behavior when targeting PostgreSQL.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
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

        #endregion

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

        #region Extensions

        public static ModelBuilder HasPostgresExtension(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));

            modelBuilder.Model.Npgsql().GetOrAddPostgresExtension(name);
            return modelBuilder;
        }

        #endregion

        #region Enums

        /// <summary>
        /// Registers a user-defined enum type in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder in which to create the enum type.</param>
        /// <param name="schema">The schema in which to create the enum type.</param>
        /// <param name="name">The name of the enum type to create.</param>
        /// <param name="labels">The enum label values.</param>
        /// <returns>
        /// The updated <see cref="ModelBuilder"/>.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/static/datatype-enum.html
        /// </remarks>
        /// <exception cref="ArgumentNullException">builder</exception>
        [NotNull]
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

        /// <summary>
        /// Registers a user-defined enum type in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder in which to create the enum type.</param>
        /// <param name="name">The name of the enum type to create.</param>
        /// <param name="labels">The enum label values.</param>
        /// <returns>
        /// The updated <see cref="ModelBuilder"/>.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/static/datatype-enum.html
        /// </remarks>
        /// <exception cref="ArgumentNullException">builder</exception>
        [NotNull]
        public static ModelBuilder ForNpgsqlHasEnum(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [NotNull] string[] labels)
            => modelBuilder.ForNpgsqlHasEnum(null, name, labels);

        /// <summary>
        /// Registers a user-defined enum type in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder in which to create the enum type.</param>
        /// <param name="schema">The schema in which to create the enum type.</param>
        /// <param name="name">The name of the enum type to create.</param>
        /// <param name="nameTranslator">
        /// The translator for name and label inference.
        /// Defaults to <see cref="NpgsqlSnakeCaseNameTranslator"/>.</param>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns>
        /// The updated <see cref="ModelBuilder"/>.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/static/datatype-enum.html
        /// </remarks>
        /// <exception cref="ArgumentNullException">builder</exception>
        [NotNull]
        public static ModelBuilder ForNpgsqlHasEnum<TEnum>(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string schema = null,
            [CanBeNull] string name = null,
            [CanBeNull] INpgsqlNameTranslator nameTranslator = null)
            where TEnum : struct, Enum
        {
            if (nameTranslator == null)
                nameTranslator = DefaultNameTranslator;

            return modelBuilder.ForNpgsqlHasEnum(
                schema,
                name ?? GetTypePgName<TEnum>(nameTranslator),
                GetMemberPgNames<TEnum>(nameTranslator));
        }

        #endregion

        #region Templates

        public static ModelBuilder HasDatabaseTemplate(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string templateDatabaseName)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(templateDatabaseName, nameof(templateDatabaseName));

            modelBuilder.Model.Npgsql().DatabaseTemplate = templateDatabaseName;
            return modelBuilder;
        }

        #endregion

        #region Ranges

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

        #endregion

        #region Tablespaces

        public static ModelBuilder ForNpgsqlUseTablespace(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string tablespace)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(tablespace, nameof(tablespace));

            modelBuilder.Model.Npgsql().Tablespace = tablespace;
            return modelBuilder;
        }

        #endregion

        #region Helpers

        // See: https://github.com/npgsql/npgsql/blob/dev/src/Npgsql/TypeMapping/TypeMapperBase.cs#L132-L138
        [NotNull]
        static string GetTypePgName<TEnum>([NotNull] INpgsqlNameTranslator nameTranslator) where TEnum : struct, Enum
            => typeof(TEnum).GetCustomAttribute<PgNameAttribute>()?.PgName ??
               nameTranslator.TranslateTypeName(typeof(TEnum).Name);

        // See: https://github.com/npgsql/npgsql/blob/dev/src/Npgsql/TypeHandlers/EnumHandler.cs#L118-L129
        [NotNull]
        [ItemNotNull]
        static string[] GetMemberPgNames<TEnum>([NotNull] INpgsqlNameTranslator nameTranslator) where TEnum : struct, Enum
            => typeof(TEnum)
               .GetFields(BindingFlags.Static | BindingFlags.Public)
               .Select(x => x.GetCustomAttribute<PgNameAttribute>()?.PgName ??
                            nameTranslator.TranslateMemberName(x.Name))
               .ToArray();

        #endregion
    }
}
