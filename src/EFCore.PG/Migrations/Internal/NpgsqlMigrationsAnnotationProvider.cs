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
            if (entityType.GetNpgsqlComment() is string comment)
                yield return new Annotation(NpgsqlAnnotationNames.Comment, comment);
            if (entityType.GetNpgsqlIsUnlogged())
                yield return new Annotation(NpgsqlAnnotationNames.UnloggedTable, entityType.GetNpgsqlIsUnlogged());
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
            if (property.GetNpgsqlValueGenerationStrategy() is NpgsqlValueGenerationStrategy npgsqlValueGenerationStrategy)
                yield return new Annotation(NpgsqlAnnotationNames.ValueGenerationStrategy, npgsqlValueGenerationStrategy);
            if (property.GetNpgsqlComment() is string comment)
                yield return new Annotation(NpgsqlAnnotationNames.Comment, comment);
        }

        public override IEnumerable<IAnnotation> For(IIndex index)
        {
            if (index.GetNpgsqlMethod() is string method)
                yield return new Annotation(NpgsqlAnnotationNames.IndexMethod, method);
            if (index.GetNpgsqlOperators() is IReadOnlyList<string> operators)
                yield return new Annotation(NpgsqlAnnotationNames.IndexOperators, operators);
            if (index.GetNpgsqlCollation() is IReadOnlyList<string> collation)
                yield return new Annotation(NpgsqlAnnotationNames.IndexCollation, collation);
            if (index.GetNpgsqlSortOrder() is IReadOnlyList<SortOrder> sortOrder)
                yield return new Annotation(NpgsqlAnnotationNames.IndexSortOrder, sortOrder);
            if (index.GetNpgsqlNullSortOrder() is IReadOnlyList<SortOrder> nullSortOrder)
                yield return new Annotation(NpgsqlAnnotationNames.IndexNullSortOrder, nullSortOrder);
            if (index.GetNpgsqlIncludeProperties() is IReadOnlyList<string> includeProperties)
                yield return new Annotation(NpgsqlAnnotationNames.IndexInclude, includeProperties);
        }

        public override IEnumerable<IAnnotation> For(IModel model)
            => model.GetAnnotations().Where(a =>
                a.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal) ||
                a.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal) ||
                a.Name.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal));
    }
}
