using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlModelExtensions
    {
        public const string DefaultHiLoSequenceName = "EntityFrameworkHiLoSequence";

        #region HiLo

        /// <summary>
        ///     Returns the name to use for the default hi-lo sequence.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The name to use for the default hi-lo sequence. </returns>
        public static string GetHiLoSequenceName([NotNull] this IModel model)
            => (string)model[NpgsqlAnnotationNames.HiLoSequenceName]
               ?? DefaultHiLoSequenceName;

        /// <summary>
        ///     Sets the name to use for the default hi-lo sequence.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="name"> The value to set. </param>
        public static void SetHiLoSequenceName([NotNull] this IMutableModel model, [CanBeNull] string name)
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
        public static string SetHiLoSequenceName(
            [NotNull] this IConventionModel model, [CanBeNull] string name, bool fromDataAnnotation = false)
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
        public static ConfigurationSource? GetHiLoSequenceNameConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceName)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the schema to use for the default hi-lo sequence.
        ///     <see cref="NpgsqlPropertyBuilderExtensions.UseHiLo" />
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The schema to use for the default hi-lo sequence. </returns>
        public static string GetHiLoSequenceSchema([NotNull] this IModel model)
            => (string)model[NpgsqlAnnotationNames.HiLoSequenceSchema];

        /// <summary>
        ///     Sets the schema to use for the default hi-lo sequence.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetHiLoSequenceSchema([NotNull] this IMutableModel model, [CanBeNull] string value)
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
        public static string SetHiLoSequenceSchema(
            [NotNull] this IConventionModel model, [CanBeNull] string value, bool fromDataAnnotation = false)
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
        public static ConfigurationSource? GetHiLoSequenceSchemaConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema)?.GetConfigurationSource();

        #endregion

        #region Value Generation Strategy

        /// <summary>
        ///     Returns the <see cref="NpgsqlValueGenerationStrategy" /> to use for properties
        ///     of keys in the model, unless the property has a strategy explicitly set.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The default <see cref="NpgsqlValueGenerationStrategy" />. </returns>
        public static NpgsqlValueGenerationStrategy? GetValueGenerationStrategy([NotNull] this IModel model)
            => (NpgsqlValueGenerationStrategy?)model[NpgsqlAnnotationNames.ValueGenerationStrategy];

        /// <summary>
        ///     Attempts to set the <see cref="NpgsqlValueGenerationStrategy" /> to use for properties
        ///     of keys in the model that don't have a strategy explicitly set.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetValueGenerationStrategy([NotNull] this IMutableModel model, NpgsqlValueGenerationStrategy? value)
            => model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy, value);

        /// <summary>
        ///     Attempts to set the <see cref="NpgsqlValueGenerationStrategy" /> to use for properties
        ///     of keys in the model that don't have a strategy explicitly set.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static NpgsqlValueGenerationStrategy? SetValueGenerationStrategy(
            [NotNull] this IConventionModel model,
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
        public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();

        #endregion

        #region PostgreSQL Extensions

        [NotNull]
        public static PostgresExtension GetOrAddPostgresExtension(
            [NotNull] this IMutableModel model,
            [CanBeNull] string schema,
            [NotNull] string name,
            [CanBeNull] string version)
            => PostgresExtension.GetOrAddPostgresExtension(model, schema, name, version);

        public static IReadOnlyList<PostgresExtension> GetPostgresExtensions([NotNull] this IModel model)
            => PostgresExtension.GetPostgresExtensions(model).ToArray();

        #endregion

        #region Enum types

        public static PostgresEnum GetOrAddPostgresEnum(
            [NotNull] this IMutableModel model,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string[] labels)
            => PostgresEnum.GetOrAddPostgresEnum(model, schema, name, labels);

        public static IReadOnlyList<PostgresEnum> GetPostgresEnums([NotNull] this IModel model)
            => PostgresEnum.GetPostgresEnums(model).ToArray();

        #endregion Enum types

        #region Range types

        public static PostgresRange GetOrAddPostgresRange(
            [NotNull] this IMutableModel model,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string subtype,
            [CanBeNull] string canonicalFunction = null,
            [CanBeNull] string subtypeOpClass = null,
            [CanBeNull] string collation = null,
            [CanBeNull] string subtypeDiff = null)
            => PostgresRange.GetOrAddPostgresRange(
                model,
                schema,
                name,
                subtype,
                canonicalFunction,
                subtypeOpClass,
                collation,
                subtypeDiff);

        public static IReadOnlyList<PostgresRange> PostgresRanges([NotNull] this IModel model)
            => PostgresRange.GetPostgresRanges(model).ToArray();

        #endregion Range types

        #region Database Template

        public static string GetDatabaseTemplate([NotNull] this IModel model)
            => (string)model[NpgsqlAnnotationNames.DatabaseTemplate];

        public static void SetDatabaseTemplate([NotNull] this IMutableModel model, [CanBeNull] string template)
            => model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.DatabaseTemplate, template);

        public static string SetDatabaseTemplate(
            [NotNull] this IConventionModel model,
            [CanBeNull] string template,
            bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(template, nameof(template));

            model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.DatabaseTemplate, template, fromDataAnnotation);

            return template;
        }

        public static ConfigurationSource? GetDatabaseTemplateConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(NpgsqlAnnotationNames.DatabaseTemplate)?.GetConfigurationSource();

        #endregion

        #region Tablespace

        public static string GetTablespace([NotNull] this IModel model)
            => (string)model[NpgsqlAnnotationNames.Tablespace];

        public static void SetTablespace([NotNull] this IMutableModel model, [CanBeNull] string tablespace)
            => model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.Tablespace, tablespace);

        public static string SetTablespace(
            [NotNull] this IConventionModel model,
            [CanBeNull] string tablespace,
            bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(tablespace, nameof(tablespace));

            model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.Tablespace, tablespace, fromDataAnnotation);

            return tablespace;
        }

        public static ConfigurationSource? GetTablespaceConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(NpgsqlAnnotationNames.Tablespace)?.GetConfigurationSource();

        #endregion

        #region Collation management

        public static PostgresCollation GetOrAddCollation(
            [NotNull] this IMutableModel model,
            [CanBeNull] string schema,
            [NotNull] string name,
            [CanBeNull] string lcCollate = null,
            [CanBeNull] string lcCtype = null,
            [CanBeNull] string provider = null,
            bool? deterministic = null)
            => PostgresCollation.GetOrAddCollation(
                model,
                schema,
                name,
                lcCollate,
                lcCtype,
                provider,
                deterministic);

        public static IReadOnlyList<PostgresCollation> GetCollations([NotNull] this IModel model)
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
        public static string GetDefaultColumnCollation([NotNull] this IModel model)
            => (string)model[NpgsqlAnnotationNames.DefaultColumnCollation];

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
        public static void SetDefaultColumnCollation([NotNull] this IMutableModel model, [CanBeNull] string collation)
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
        public static string SetDefaultColumnCollation([NotNull] this IConventionModel model, [CanBeNull] string collation, bool fromDataAnnotation = false)
        {
            model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.DefaultColumnCollation, collation, fromDataAnnotation);
            return collation;
        }

        /// <summary>
        /// Returns the <see cref="ConfigurationSource" /> for the default column collation.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the default column collation.</returns>
        public static ConfigurationSource? GetDefaultColumnCollationConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(NpgsqlAnnotationNames.DefaultColumnCollation)?.GetConfigurationSource();

        #endregion Default column collation
    }
}
