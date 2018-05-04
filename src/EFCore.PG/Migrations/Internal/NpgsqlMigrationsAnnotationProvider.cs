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
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal
{
    public class NpgsqlMigrationsAnnotationProvider : MigrationsAnnotationProvider
    {
        public NpgsqlMigrationsAnnotationProvider([NotNull] MigrationsAnnotationProviderDependencies dependencies)
            : base(dependencies)
        {
        }

        public override IEnumerable<IAnnotation> For(IEntityType entityType)
        {
            if (entityType.Npgsql().Comment != null)
                yield return new Annotation(NpgsqlAnnotationNames.Comment, entityType.Npgsql().Comment);

            if (entityType[CockroachDbAnnotationNames.InterleaveInParent] != null)
                yield return new Annotation(
                    CockroachDbAnnotationNames.InterleaveInParent,
                    entityType[CockroachDbAnnotationNames.InterleaveInParent]);

            foreach (var storageParamAnnotation in entityType.GetAnnotations()
                .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix)))
            {
                yield return storageParamAnnotation;
            }

            foreach (var annotation in GetSearchVectorAnnotations(entityType))
            {
                yield return annotation;
            }
        }

        static IEnumerable<IAnnotation> GetSearchVectorAnnotations(IEntityType entityType)
        {
            var searchVectors = entityType.Npgsql().SearchVectors;
            foreach (var searchVector in searchVectors)
            {
                var translatedSearchVector = new SearchVectorAnnotation
                {
                    Name = FindColumnNameOrThrow(entityType, searchVector.Key),
                    Config = searchVector.Value.Config.IsPropertyOrColumnName
                        ? new TextSearchRegconfig(
                            FindColumnNameOrThrow(entityType, searchVector.Value.Config.Name),
                            true)
                        : searchVector.Value.Config
                };

                foreach (var group in searchVector.Value.ComponentGroupsByLabel.Select(
                    x => new SearchVectorComponentGroup(x.Label)
                    {
                        Components = x.Components.Select(
                            p => new SearchVectorComponent(
                                FindColumnNameOrThrow(entityType, p.Name),
                                FindDefaultSqlValueOrThrow(entityType, p.Name))).ToList()
                    }))
                {
                    translatedSearchVector.ComponentGroupsByLabel.Add(group);
                }

                yield return new Annotation(
                    NpgsqlAnnotationNames.SearchVectorPrefix + searchVector.Key,
                    translatedSearchVector.Serialize());
            }
        }

        static string FindColumnNameOrThrow(IEntityType entityType, string propertyName) =>
            FindPropertyOrThrow(entityType, propertyName).Relational().ColumnName;

        static string FindDefaultSqlValueOrThrow(IEntityType entityType, string propertyName)
        {
            var property = FindPropertyOrThrow(entityType, propertyName).Relational();
            if (property.ColumnType.Equals("json", StringComparison.OrdinalIgnoreCase)
                || property.ColumnType.Equals("jsonb", StringComparison.OrdinalIgnoreCase))
            {
                return "{}";
            }

            return "";
        }

        static IProperty FindPropertyOrThrow(IEntityType entityType, string propertyName) =>
            entityType.FindProperty(propertyName)
            ?? throw new InvalidOperationException(
                $"Failed to find property '{propertyName}' in entity type '{entityType.Name}'");

        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            if (property.Npgsql().ValueGenerationStrategy == NpgsqlValueGenerationStrategy.SerialColumn)
                yield return new Annotation(NpgsqlAnnotationNames.ValueGenerationStrategy, NpgsqlValueGenerationStrategy.SerialColumn);
            if (property.Npgsql().ValueGenerationStrategy == NpgsqlValueGenerationStrategy.IdentityAlwaysColumn)
                yield return new Annotation(NpgsqlAnnotationNames.ValueGenerationStrategy, NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
            if (property.Npgsql().ValueGenerationStrategy == NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                yield return new Annotation(NpgsqlAnnotationNames.ValueGenerationStrategy, NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            if (property.Npgsql().Comment != null)
                yield return new Annotation(NpgsqlAnnotationNames.Comment, property.Npgsql().Comment);
        }

        public override IEnumerable<IAnnotation> For(IIndex index)
        {
            if (index.Npgsql().Method != null)
                yield return new Annotation(NpgsqlAnnotationNames.IndexMethod, index.Npgsql().Method);
        }

        public override IEnumerable<IAnnotation> For(IModel model)
            => model.GetAnnotations().Where(a =>
                a.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal) ||
                a.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal));
    }
}
