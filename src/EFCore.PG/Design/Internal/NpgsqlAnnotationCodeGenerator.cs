using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
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

            // Serial is the default value generation strategy.
            // So if ValueGenerated is OnAdd (which it must be if serial is set), make sure
            // ValueGenerationStrategy.Serial isn't code-generated because it's by-convention.
            if (annotation.Name == NpgsqlAnnotationNames.ValueGenerationStrategy
                && (NpgsqlValueGenerationStrategy)annotation.Value == NpgsqlValueGenerationStrategy.SerialColumn)
            {
                Debug.Assert(property.ValueGenerated == ValueGenerated.OnAdd);
                return true;
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

#pragma warning disable 618
            if (annotation.Name == NpgsqlAnnotationNames.Comment)
                return new MethodCallCodeFragment(nameof(NpgsqlEntityTypeBuilderExtensions.ForNpgsqlHasComment), annotation.Value);
#pragma warning restore 618

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
                switch ((NpgsqlValueGenerationStrategy)annotation.Value)
                {
                case NpgsqlValueGenerationStrategy.SerialColumn:
                    return new MethodCallCodeFragment(nameof(NpgsqlPropertyBuilderExtensions.UseSerialColumn));
                case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
                    return new MethodCallCodeFragment(nameof(NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn));
                case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                    return new MethodCallCodeFragment(nameof(NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn));
                case NpgsqlValueGenerationStrategy.SequenceHiLo:
                    throw new Exception($"Unexpected {NpgsqlValueGenerationStrategy.SequenceHiLo} value generation strategy when scaffolding");
                default:
                    throw new ArgumentOutOfRangeException();
                }

#pragma warning disable 618
            case NpgsqlAnnotationNames.Comment:
                return new MethodCallCodeFragment(nameof(NpgsqlPropertyBuilderExtensions.ForNpgsqlHasComment), annotation.Value);
#pragma warning restore 618
            }

            return null;
        }

        public override MethodCallCodeFragment GenerateFluentApi(IIndex index, IAnnotation annotation)
        {
            if (annotation.Name == NpgsqlAnnotationNames.IndexMethod)
                return new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.HasMethod), annotation.Value);
            if (annotation.Name == NpgsqlAnnotationNames.IndexOperators)
                return new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.HasOperators), annotation.Value);
            if (annotation.Name == NpgsqlAnnotationNames.IndexCollation)
                return new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.HasCollation), annotation.Value);
            if (annotation.Name == NpgsqlAnnotationNames.IndexSortOrder)
                return new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.HasSortOrder), annotation.Value);
            if (annotation.Name == NpgsqlAnnotationNames.IndexNullSortOrder)
                return new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.HasNullSortOrder), annotation.Value);
            if (annotation.Name == NpgsqlAnnotationNames.IndexInclude)
                return new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.IncludeProperties), annotation.Value);

            return null;
        }
    }
}
