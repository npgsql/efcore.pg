// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Npgsql specific extension methods for <see cref="PropertyBuilder" />.
    /// </summary>
    public static class NpgsqlPropertyBuilderExtensions
    {
        /// <summary>
        ///     Configures the column that the property maps to when targeting PostgreSQL.
        /// </summary>
        /// <param comment="propertyBuilder"> The builder for the property being configured. </param>
        /// <param comment="name"> The comment of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForNpgsqlHasColumnName(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            GetNpgsqlInternalBuilder(propertyBuilder).ColumnName(name);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the column that the property maps to when targeting PostgreSQL.
        /// </summary>
        /// <typeparam comment="TProperty"> The type of the property being configured. </typeparam>
        /// <param comment="propertyBuilder"> The builder for the property being configured. </param>
        /// <param comment="name"> The comment of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForNpgsqlHasColumnName<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name)
            => (PropertyBuilder<TProperty>)ForNpgsqlHasColumnName((PropertyBuilder)propertyBuilder, name);

        /// <summary>
        ///     Configures the data type of the column that the property maps to when targeting PostgreSQL.
        ///     This should be the complete type comment, including precision, scale, length, etc.
        /// </summary>
        /// <param comment="propertyBuilder"> The builder for the property being configured. </param>
        /// <param comment="typeName"> The comment of the data type of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForNpgsqlHasColumnType(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string typeName)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(typeName, nameof(typeName));

            GetNpgsqlInternalBuilder(propertyBuilder).ColumnType(typeName);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the data type of the column that the property maps to when targeting PostgreSQL.
        ///     This should be the complete type comment, including precision, scale, length, etc.
        /// </summary>
        /// <typeparam comment="TProperty"> The type of the property being configured. </typeparam>
        /// <param comment="propertyBuilder"> The builder for the property being configured. </param>
        /// <param comment="typeName"> The comment of the data type of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForNpgsqlHasColumnType<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string typeName)
            => (PropertyBuilder<TProperty>)ForNpgsqlHasColumnType((PropertyBuilder)propertyBuilder, typeName);

        /// <summary>
        ///     Configures the default value expression for the column that the property maps to when targeting PostgreSQL.
        /// </summary>
        /// <param comment="propertyBuilder"> The builder for the property being configured. </param>
        /// <param comment="sql"> The SQL expression for the default value of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForNpgsqlHasDefaultValueSql(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string sql)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(sql, nameof(sql));

            GetNpgsqlInternalBuilder(propertyBuilder).DefaultValueSql(sql);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the default value expression for the column that the property maps to when targeting PostgreSQL.
        /// </summary>
        /// <typeparam comment="TProperty"> The type of the property being configured. </typeparam>
        /// <param comment="propertyBuilder"> The builder for the property being configured. </param>
        /// <param comment="sql"> The SQL expression for the default value of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForNpgsqlHasDefaultValueSql<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string sql)
            => (PropertyBuilder<TProperty>)ForNpgsqlHasDefaultValueSql((PropertyBuilder)propertyBuilder, sql);

        /// <summary>
        ///     Configures the default value for the column that the property maps to when targeting PostgreSQL.
        /// </summary>
        /// <param comment="propertyBuilder"> The builder for the property being configured. </param>
        /// <param comment="value"> The default value of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForNpgsqlHasDefaultValue(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] object value)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            GetNpgsqlInternalBuilder(propertyBuilder).DefaultValue(value);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the default value for the column that the property maps to when targeting PostgreSQL.
        /// </summary>
        /// <typeparam comment="TProperty"> The type of the property being configured. </typeparam>
        /// <param comment="propertyBuilder"> The builder for the property being configured. </param>
        /// <param comment="value"> The default value of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForNpgsqlHasDefaultValue<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] object value)
            => (PropertyBuilder<TProperty>)ForNpgsqlHasDefaultValue((PropertyBuilder)propertyBuilder, value);

        /// <summary>
        ///     Configures the property to map to a computed column when targeting PostgreSQL.
        /// </summary>
        /// <param comment="propertyBuilder"> The builder for the property being configured. </param>
        /// <param comment="sql"> The SQL expression that computes values for the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForNpgsqlHasComputedColumnSql(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string sql)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(sql, nameof(sql));

            GetNpgsqlInternalBuilder(propertyBuilder).ComputedColumnSql(sql);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property to map to a computed column when targeting PostgreSQL.
        /// </summary>
        /// <typeparam comment="TProperty"> The type of the property being configured. </typeparam>
        /// <param comment="propertyBuilder"> The builder for the property being configured. </param>
        /// <param comment="sql"> The SQL expression that computes values for the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForNpgsqlHasComputedColumnSql<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string sql)
            => (PropertyBuilder<TProperty>)ForNpgsqlHasComputedColumnSql((PropertyBuilder)propertyBuilder, sql);

        /// <summary>
        ///     Configures the property to use a sequence-based hi-lo pattern to generate values for new entities,
        ///     when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param comment="propertyBuilder"> The builder for the property being configured. </param>
        /// <param comment="name"> The comment of the sequence. </param>
        /// <param comment="schema"> The schema of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForNpgsqlUseSequenceHiLo(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var property = propertyBuilder.Metadata;

            name = name ?? NpgsqlModelAnnotations.DefaultHiLoSequenceName;

            var model = property.DeclaringEntityType.Model;

            if (model.Npgsql().FindSequence(name, schema) == null)
            {
                model.Npgsql().GetOrAddSequence(name, schema).IncrementBy = 10;
            }

            GetNpgsqlInternalBuilder(propertyBuilder).ValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);

            property.Npgsql().HiLoSequenceName = name;
            property.Npgsql().HiLoSequenceSchema = schema;

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property to use a sequence-based hi-lo pattern to generate values for new entities,
        ///     when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <typeparam comment="TProperty"> The type of the property being configured. </typeparam>
        /// <param comment="propertyBuilder"> The builder for the property being configured. </param>
        /// <param comment="name"> The comment of the sequence. </param>
        /// <param comment="schema"> The schema of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ForNpgsqlUseSequenceHiLo<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
            => (PropertyBuilder<TProperty>)ForNpgsqlUseSequenceHiLo((PropertyBuilder)propertyBuilder, name, schema);

        /// <summary>
        ///     Configures the property to use the PostgreSQL SERIALfeature to generate values for new entities,
        ///     when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <param comment="propertyBuilder"> The builder for the property being configured. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder UseNpgsqlSerialColumn(
            [NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            GetNpgsqlInternalBuilder(propertyBuilder).ValueGenerationStrategy(NpgsqlValueGenerationStrategy.SerialColumn);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property to use the PostgreSQL SERIAL feature to generate values for new entities,
        ///     when targeting PostgreSQL. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
        /// </summary>
        /// <typeparam comment="TProperty"> The type of the property being configured. </typeparam>
        /// <param comment="propertyBuilder"> The builder for the property being configured. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> UseNpgsqlSerialColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>)UseNpgsqlSerialColumn((PropertyBuilder)propertyBuilder);

        /// <summary>
        ///     Configures the comment set on the column when targeting Npgsql.
        /// </summary>
        /// <param comment="propertyBuilder"> The builder for the entity type being configured. </param>
        /// <param comment="comment"> The comment of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ForNpgsqlHasComment(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string comment)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(comment, nameof(comment));

            GetNpgsqlInternalBuilder(propertyBuilder).Comment = comment;

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the comment set on the column when targeting Npgsql.
        /// </summary>
        /// <typeparam comment="TEntity"> The entity type being configured. </typeparam>
        /// <param comment="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param comment="comment"> The comment of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TEntity> ForNpgsqlHasComment<TEntity>(
            [NotNull] this PropertyBuilder<TEntity> propertyBuilder,
            [CanBeNull] string comment)
            where TEntity : class
        => (PropertyBuilder<TEntity>)ForNpgsqlHasComment((PropertyBuilder)propertyBuilder, comment);

        private static NpgsqlPropertyBuilderAnnotations GetNpgsqlInternalBuilder(PropertyBuilder propertyBuilder)
            => propertyBuilder.GetInfrastructure<InternalPropertyBuilder>().Npgsql(ConfigurationSource.Explicit);
    }
}
