using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal
{
    public class NpgsqlAnnotationCodeGenerator : AnnotationCodeGenerator
    {
        public NpgsqlAnnotationCodeGenerator([NotNull] AnnotationCodeGeneratorDependencies dependencies)
            : base(dependencies) {}

        public override bool IsHandledByConvention(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == RelationalAnnotationNames.DefaultSchema
                && string.Equals("public", (string)annotation.Value))
            {
                return true;
            }

            return false;
        }

        public override bool IsHandledByConvention(IIndex index, IAnnotation annotation)
        {
            Check.NotNull(index, nameof(index));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == NpgsqlAnnotationNames.IndexMethod
                && string.Equals("btree", (string)annotation.Value))
            {
                return true;
            }

            return false;
        }

        public override bool IsHandledByConvention(IProperty property, IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            // The default by-convention value generation strategy is serial in pre-10 PostgreSQL,
            // and IdentityByDefault otherwise.
            if (annotation.Name == NpgsqlAnnotationNames.ValueGenerationStrategy)
            {
                Debug.Assert(property.ValueGenerated == ValueGenerated.OnAdd);

                // Note: both serial and identity-by-default columns are considered by-convention - we don't want
                // to assume that the PostgreSQL version of the scaffolded database necessarily determines the
                // version of the database that the scaffolded model will target. This makes life difficult for
                // models with mixed strategies but that's an edge case.
                return (NpgsqlValueGenerationStrategy)annotation.Value switch
                {
                    NpgsqlValueGenerationStrategy.SerialColumn => true,
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn => true,
                    _ => false
                };
            }

            return false;
        }

        public override MethodCallCodeFragment GenerateFluentApi(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal))
            {
                var extension = new PostgresExtension(model, annotation.Name);

                return new MethodCallCodeFragment(nameof(NpgsqlModelBuilderExtensions.HasPostgresExtension),
                    extension.Name);
            }

            if (annotation.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal))
            {
                var enumTypeDef = new PostgresEnum(model, annotation.Name);

                return enumTypeDef.Schema == "public"
                    ? new MethodCallCodeFragment(nameof(NpgsqlModelBuilderExtensions.HasPostgresEnum),
                        enumTypeDef.Name, enumTypeDef.Labels)
                    : new MethodCallCodeFragment(nameof(NpgsqlModelBuilderExtensions.HasPostgresEnum),
                        enumTypeDef.Schema, enumTypeDef.Name, enumTypeDef.Labels);
            }

            if (annotation.Name.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal))
            {
                var rangeTypeDef = new PostgresRange(model, annotation.Name);

                if (rangeTypeDef.CanonicalFunction == null &&
                    rangeTypeDef.SubtypeOpClass == null &&
                    rangeTypeDef.Collation == null &&
                    rangeTypeDef.SubtypeDiff == null)
                {
                    return new MethodCallCodeFragment(nameof(NpgsqlModelBuilderExtensions.HasPostgresRange),
                        rangeTypeDef.Schema == "public" ? null : rangeTypeDef.Schema,
                        rangeTypeDef.Name,
                        rangeTypeDef.Subtype);
                }

                return new MethodCallCodeFragment(nameof(NpgsqlModelBuilderExtensions.HasPostgresRange),
                    rangeTypeDef.Schema == "public" ? null : rangeTypeDef.Schema,
                    rangeTypeDef.Name,
                    rangeTypeDef.Subtype,
                    rangeTypeDef.CanonicalFunction,
                    rangeTypeDef.SubtypeOpClass,
                    rangeTypeDef.Collation,
                    rangeTypeDef.SubtypeDiff);
            }

            return null;
        }

        public override MethodCallCodeFragment GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == NpgsqlAnnotationNames.Comment)
                return new MethodCallCodeFragment(nameof(NpgsqlEntityTypeBuilderExtensions.HasComment), annotation.Value);

            if (annotation.Name == NpgsqlAnnotationNames.UnloggedTable)
                return new MethodCallCodeFragment(nameof(NpgsqlEntityTypeBuilderExtensions.IsUnlogged), annotation.Value);

            return null;
        }

        public override MethodCallCodeFragment GenerateFluentApi(IProperty property, IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            switch (annotation.Name)
            {
            case NpgsqlAnnotationNames.ValueGenerationStrategy:
                return new MethodCallCodeFragment(annotation.Value switch
                {
                    NpgsqlValueGenerationStrategy.SerialColumn => nameof(NpgsqlPropertyBuilderExtensions.UseSerialColumn),
                    NpgsqlValueGenerationStrategy.IdentityAlwaysColumn => nameof(NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn),
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn => nameof(NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn),
                    _ => throw new ArgumentOutOfRangeException()
                });

            case NpgsqlAnnotationNames.IdentityOptions:
                var identityOptions = IdentitySequenceOptionsData.Deserialize((string)annotation.Value);
                return new MethodCallCodeFragment(
                    nameof(NpgsqlPropertyBuilderExtensions.HasIdentityOptions),
                    identityOptions.StartValue,
                    identityOptions.IncrementBy == 1 ? null : (long?)identityOptions.IncrementBy,
                    identityOptions.MinValue,
                    identityOptions.MaxValue,
                    identityOptions.IsCyclic ? true : (bool?)null,
                    identityOptions.NumbersToCache == 1 ? null : (long?)identityOptions.NumbersToCache);

            case NpgsqlAnnotationNames.Comment:
                return new MethodCallCodeFragment(nameof(NpgsqlPropertyBuilderExtensions.HasComment), annotation.Value);
            }

            return null;
        }

        public override MethodCallCodeFragment GenerateFluentApi(IIndex index, IAnnotation annotation)
            => annotation.Name switch
            {
                NpgsqlAnnotationNames.IndexMethod
                    => new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.HasMethod), annotation.Value),
                NpgsqlAnnotationNames.IndexOperators
                    => new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.HasOperators), annotation.Value),
                NpgsqlAnnotationNames.IndexCollation
                    => new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.HasCollation), annotation.Value),
                NpgsqlAnnotationNames.IndexSortOrder
                    => new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.HasSortOrder), annotation.Value),
                NpgsqlAnnotationNames.IndexNullSortOrder
                    => new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.HasNullSortOrder), annotation.Value),
                NpgsqlAnnotationNames.IndexInclude
                    => new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.IncludeProperties), annotation.Value),
                _ => null
            };
    }
}
