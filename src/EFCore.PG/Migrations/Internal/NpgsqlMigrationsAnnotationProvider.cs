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
            : base(dependencies) {}

        public override IEnumerable<IAnnotation> For(IEntityType entityType)
        {
            if (entityType.Npgsql().Comment != null)
                yield return new Annotation(NpgsqlAnnotationNames.Comment, entityType.Npgsql().Comment);
            if (entityType.Npgsql().IsUnlogged)
                yield return new Annotation(NpgsqlAnnotationNames.UnloggedTable, entityType.Npgsql().IsUnlogged);
            if (entityType[CockroachDbAnnotationNames.InterleaveInParent] != null)
                yield return new Annotation(CockroachDbAnnotationNames.InterleaveInParent, entityType[CockroachDbAnnotationNames.InterleaveInParent]);
            foreach (var storageParamAnnotation in entityType.GetAnnotations()
                .Where(a => a.Name.StartsWith(NpgsqlAnnotationNames.StorageParameterPrefix)))
            {
                yield return storageParamAnnotation;
            }
        }

        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            if (property.Npgsql().ValueGenerationStrategy is NpgsqlValueGenerationStrategy npgsqlValueGenerationStrategy)
                yield return new Annotation(NpgsqlAnnotationNames.ValueGenerationStrategy, npgsqlValueGenerationStrategy);
            if (property.Npgsql().Comment is string comment)
                yield return new Annotation(NpgsqlAnnotationNames.Comment, comment);
        }

        public override IEnumerable<IAnnotation> For(IIndex index)
        {
            if (index.Npgsql().Method != null)
                yield return new Annotation(NpgsqlAnnotationNames.IndexMethod, index.Npgsql().Method);
            if (index.Npgsql().Operators != null)
                yield return new Annotation(NpgsqlAnnotationNames.IndexOperators, index.Npgsql().Operators);
            if (index.Npgsql().Collation != null)
                yield return new Annotation(NpgsqlAnnotationNames.IndexCollation, index.Npgsql().Collation);
            if (index.Npgsql().SortOrder != null)
                yield return new Annotation(NpgsqlAnnotationNames.IndexSortOrder, index.Npgsql().SortOrder);
            if (index.Npgsql().NullSortOrder != null)
                yield return new Annotation(NpgsqlAnnotationNames.IndexNullSortOrder, index.Npgsql().NullSortOrder);
            if (index.Npgsql().IncludeProperties != null)
                yield return new Annotation(NpgsqlAnnotationNames.IndexInclude, index.Npgsql().IncludeProperties);
        }

        public override IEnumerable<IAnnotation> For(IModel model)
            => model.GetAnnotations().Where(a =>
                a.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal) ||
                a.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal) ||
                a.Name.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal));
    }
}
