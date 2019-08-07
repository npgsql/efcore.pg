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
            if (NpgsqlEntityTypeExtensions.GetComment(entityType) is string comment)
                yield return new Annotation(NpgsqlAnnotationNames.Comment, comment);
            if (entityType.GetIsUnlogged())
                yield return new Annotation(NpgsqlAnnotationNames.UnloggedTable, entityType.GetIsUnlogged());
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
            if (property.GetValueGenerationStrategy() is NpgsqlValueGenerationStrategy npgsqlValueGenerationStrategy &&
                npgsqlValueGenerationStrategy != NpgsqlValueGenerationStrategy.None)
            {
                yield return new Annotation(NpgsqlAnnotationNames.ValueGenerationStrategy,
                    npgsqlValueGenerationStrategy);
            }

            if (NpgsqlPropertyExtensions.GetComment(property) is string comment)
                yield return new Annotation(NpgsqlAnnotationNames.Comment, comment);
        }

        public override IEnumerable<IAnnotation> For(IIndex index)
        {
            if (index.GetMethod() is string method)
                yield return new Annotation(NpgsqlAnnotationNames.IndexMethod, method);
            if (index.GetOperators() is IReadOnlyList<string> operators)
                yield return new Annotation(NpgsqlAnnotationNames.IndexOperators, operators);
            if (index.GetCollation() is IReadOnlyList<string> collation)
                yield return new Annotation(NpgsqlAnnotationNames.IndexCollation, collation);
            if (index.GetSortOrder() is IReadOnlyList<SortOrder> sortOrder)
                yield return new Annotation(NpgsqlAnnotationNames.IndexSortOrder, sortOrder);
            if (index.GetNullSortOrder() is IReadOnlyList<SortOrder> nullSortOrder)
                yield return new Annotation(NpgsqlAnnotationNames.IndexNullSortOrder, nullSortOrder);
            if (index.GetIncludeProperties() is IReadOnlyList<string> includeProperties)
                yield return new Annotation(NpgsqlAnnotationNames.IndexInclude, includeProperties);

            var isCreatedConcurrently = index.IsCreatedConcurrently();
            if (isCreatedConcurrently.HasValue)
            {
                yield return new Annotation(
                    NpgsqlAnnotationNames.CreatedConcurrently,
                    isCreatedConcurrently.Value);
            }
        }

        public override IEnumerable<IAnnotation> For(IModel model)
            => model.GetAnnotations().Where(a =>
                a.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal) ||
                a.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal) ||
                a.Name.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal));
    }
}
