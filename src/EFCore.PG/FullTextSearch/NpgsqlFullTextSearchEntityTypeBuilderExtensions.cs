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
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlFullTextSearchEntityTypeBuilderExtensions
    {
        public static EntityTypeBuilder<TEntity> ForNpgsqlHasTextSearchVector<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> builder,
            [NotNull] Expression<Func<TEntity, NpgsqlTsVector>> expression,
            Expression<Func<TEntity, string>> textSearchConfigExpression,
            string indexMethod,
            [NotNull] Action<SearchVectorAnnotation<TEntity>> configureSearchVector) where TEntity : class =>
            builder.ForNpgsqlHasTextSearchVector(
                expression,
                TextSearchRegconfig.FromProperty(textSearchConfigExpression.GetPropertyAccess().Name),
                indexMethod,
                configureSearchVector);

        public static EntityTypeBuilder<TEntity> ForNpgsqlHasTextSearchVector<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> builder,
            [NotNull] Expression<Func<TEntity, NpgsqlTsVector>> expression,
            TextSearchRegconfig textSearchConfig,
            string indexMethod,
            [NotNull] Action<SearchVectorAnnotation<TEntity>> configureSearchVector) where TEntity : class =>
            builder.ForNpgsqlHasTextSearchVector(
                expression.GetPropertyAccess().Name,
                textSearchConfig,
                indexMethod,
                configureSearchVector) as EntityTypeBuilder<TEntity>;

        public static EntityTypeBuilder ForNpgsqlHasTextSearchVector(
            [NotNull] this EntityTypeBuilder builder,
            [NotNull] string propertyName,
            TextSearchRegconfig textSearchConfig,
            string indexMethod,
            [NotNull] Action<SearchVectorAnnotation> configureSearchVector) =>
            builder.ForNpgsqlHasTextSearchVector<SearchVectorAnnotation>(
                propertyName,
                textSearchConfig,
                indexMethod,
                configureSearchVector);

        static EntityTypeBuilder ForNpgsqlHasTextSearchVector<TSearchVectorAnnotation>(
            [NotNull] this EntityTypeBuilder builder,
            [NotNull] string propertyName,
            TextSearchRegconfig textSearchConfig,
            string indexMethod,
            [NotNull] Action<TSearchVectorAnnotation> configureSearchVector)
            where TSearchVectorAnnotation : SearchVectorAnnotation, new()
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureSearchVector == null) throw new ArgumentNullException(nameof(configureSearchVector));

            var annotation = new TSearchVectorAnnotation
            {
                Name = propertyName,
                Config = textSearchConfig
            };
            configureSearchVector(annotation);

            builder.Property(propertyName);
            builder.Metadata.Npgsql().SearchVectors[propertyName] = annotation;
            builder.HasIndex(propertyName).ForNpgsqlHasMethod(indexMethod);

            return builder;
        }
    }
}
