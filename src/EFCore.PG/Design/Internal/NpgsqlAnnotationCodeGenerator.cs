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
            : base(dependencies)
        {
        }

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

            if (annotation.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix))
            {
                var extension = new PostgresExtension(model, annotation.Name);

                return new MethodCallCodeFragment(nameof(NpgsqlModelBuilderExtensions.HasPostgresExtension),
                    extension.Name);
            }

            return null;
        }

        public override MethodCallCodeFragment GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == NpgsqlAnnotationNames.Comment)
                return new MethodCallCodeFragment(nameof(NpgsqlEntityTypeBuilderExtensions.ForNpgsqlHasComment), annotation.Value);

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
                    return new MethodCallCodeFragment(nameof(NpgsqlPropertyBuilderExtensions.UseNpgsqlSerialColumn));
                case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
                    return new MethodCallCodeFragment(nameof(NpgsqlPropertyBuilderExtensions.UseNpgsqlIdentityAlwaysColumn));
                case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                    return new MethodCallCodeFragment(nameof(NpgsqlPropertyBuilderExtensions.UseNpgsqlIdentityByDefaultColumn));
                case NpgsqlValueGenerationStrategy.SequenceHiLo:
                    throw new Exception($"Unexpected {NpgsqlValueGenerationStrategy.SequenceHiLo} value generation strategy when scaffolding");
                default:
                    throw new ArgumentOutOfRangeException();
                }

            case NpgsqlAnnotationNames.Comment:
                return new MethodCallCodeFragment(nameof(NpgsqlPropertyBuilderExtensions.ForNpgsqlHasComment), annotation.Value);
            }

            return null;
        }

        public override MethodCallCodeFragment GenerateFluentApi(IIndex index, IAnnotation annotation)
        {
            if (annotation.Name == NpgsqlAnnotationNames.IndexMethod)
                return new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.ForNpgsqlHasMethod), annotation.Value);

            return null;
        }
    }
}
